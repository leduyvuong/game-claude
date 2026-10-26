using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace HarvestIsle
{
    public sealed class FarmCamera : MonoBehaviour
    {
        private FarmGame game;
        private Camera view;
        private Vector3 focus = new Vector3(0, 0, 0);
        private Vector2 pressedAt;
        private Vector3 worldAtPress;
        private bool tracking;
        private bool dragged;
        private bool batchStroke;
        private Vector2 batchLast;
        private bool pinching;
        private float previousPinchDistance;
        private float lastTapTime = -10;
        private int fingerId=-1;
        private float suppressMouseUntil;
        private bool pinchAllowed;
        private float DragThreshold => Mathf.Clamp(Screen.dpi>0?Screen.dpi*.07f:12,10,32);
        private readonly Plane ground = new Plane(Vector3.up, Vector3.zero);
        private Vector2 panMinimum = new Vector2(-60, -51), panMaximum = new Vector2(60, 51);
        private float maximumZoom = 76;
        public System.Func<Vector2, bool> PointerBlocked;
        public System.Func<Ray, bool> InspectRay;

        public void ConfigureMap(Vector2 minimum, Vector2 maximum, float zoom)
        { panMinimum=minimum;panMaximum=maximum;maximumZoom=Mathf.Max(28,zoom); }
        public void SetView(Vector3 point, float size)
        { CancelGesture();focus=point;Zoom(size); }

        public void Initialize(FarmGame game)
        {
            this.game = game;
            view = GetComponent<Camera>();
            transform.rotation = Quaternion.Euler(48, 20, 0);
            view.orthographic = true;
            view.orthographicSize = 18;
            PositionCamera();
        }

        private void OnEnable() { EnhancedTouchSupport.Enable(); }
        private void OnDisable() { CancelGesture();EnhancedTouchSupport.Disable(); }
        private void OnApplicationFocus(bool focused){if(!focused)CancelGesture();}
        private void OnApplicationPause(bool paused){if(paused)CancelGesture();}
        private void CancelGesture(){if(game!=null&&game.Batch!=null)game.Batch.CancelSelection();batchStroke=false;tracking=false;dragged=false;pinching=false;fingerId=-1;lastTapTime=-10;}

        private void Update()
        {
            if (game == null || view == null) return;
            if(game.IsHarvesting||game.IsManaging||(game.Ranch!=null&&game.Ranch.IsOpen)||(game.Hotpot!=null&&game.Hotpot.IsOpen)){CancelGesture();return;}
            var touches = Touch.activeTouches;
            if(touches.Count>0)suppressMouseUntil=Time.unscaledTime+.3f;
            if (touches.Count >= 2)
            {
                if (!pinching)
                {
                    BeginPinch(touches[0].screenPosition,touches[1].screenPosition);
                }
                ContinuePinch(touches[0].screenPosition,touches[1].screenPosition);
                return;
            }
            if (pinching)
            {
                if (touches.Count == 0) pinching = false;
                return; // Never turn the tail of a pinch into a planting tap.
            }
            if (touches.Count == 1)
            {
                Touch touch = touches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began){fingerId=touch.touchId;Begin(touch.screenPosition);}
                else if(touch.touchId!=fingerId){tracking=false;return;}
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended) End(touch.screenPosition);
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) CancelGesture();
                else Move(touch.screenPosition);
                return;
            }
            if (Mouse.current == null||Time.unscaledTime<suppressMouseUntil) return;
            Vector2 pointer = Mouse.current.position.ReadValue();
            if (Mouse.current.leftButton.wasPressedThisFrame) Begin(pointer);
            if (Mouse.current.leftButton.isPressed) Move(pointer);
            if (Mouse.current.leftButton.wasReleasedThisFrame) End(pointer);
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (!IsOverUi(pointer) && Mathf.Abs(scroll) > 0.01f) Zoom(view.orthographicSize - scroll * 0.008f);
            if (Keyboard.current != null && Keyboard.current.homeKey.wasPressedThisFrame) GoHome();
        }

        private void Begin(Vector2 pointer)
        {
            tracking = !IsOverUi(pointer);
            dragged = false;
            pressedAt = pointer;
            worldAtPress = OnGround(pointer);
            batchStroke=tracking&&game.Batch.Mode!=0;batchLast=pointer;
            if(batchStroke){game.Batch.Begin();PaintBatch(pointer);}
        }

        private void Move(Vector2 pointer)
        {
            if (!tracking) return;
            if(batchStroke){PaintBatch(pointer);return;}
            if ((pointer - pressedAt).sqrMagnitude > DragThreshold*DragThreshold) dragged = true;
            if (!dragged) return;
            Vector3 movement = worldAtPress - OnGround(pointer);
            focus += movement;
            focus.x = Mathf.Clamp(focus.x, panMinimum.x, panMaximum.x);
            focus.z = Mathf.Clamp(focus.z, panMinimum.y, panMaximum.y);
            PositionCamera();
        }

        private void End(Vector2 pointer)
        {
            if (!tracking) return;
            tracking = false;
            if(batchStroke){PaintBatch(pointer);batchStroke=false;if(IsOverUi(pointer))game.Batch.CancelSelection();else game.Batch.Release();return;}
            if (dragged || (pointer-pressedAt).sqrMagnitude>DragThreshold*DragThreshold || IsOverUi(pointer)) return;
            if (InspectRay != null && InspectRay(view.ScreenPointToRay(pointer))) return;
            RaycastHit hit;
            if (Physics.Raycast(view.ScreenPointToRay(pointer), out hit, view.farClipPlane, 1 << FarmWorld.InteractionLayer))
            {
                FarmPlot plot = hit.collider.GetComponent<FarmPlot>();
                if (plot != null) { game.Hud.TapPlot(plot.Index); return; }
                var pen=hit.collider.GetComponent<FarmRanchTarget>();if(pen!=null){game.Ranch.OpenPen(pen.Index);return;}
                var hotpot=hit.collider.GetComponent<FarmHotpotTarget>();if(hotpot!=null){game.Hotpot.Open();return;}
                var building=hit.collider.GetComponent<FarmDevelopmentTarget>();if(building!=null){game.Development.OpenBuilding(building.BuildingId);return;}
                if(hit.collider.GetComponent<FarmWorkerTarget>()!=null){game.Development.Open(7);return;}
                if (hit.collider.GetComponent<FarmMarket>() != null) { game.Hud.OpenTrading(); return; }
            }
            if (Time.unscaledTime - lastTapTime < 0.32f) { GoHome(); lastTapTime = -10; }
            else lastTapTime = Time.unscaledTime;
        }

        private void PaintBatch(Vector2 pointer)
        {
            int steps=Mathf.Clamp(Mathf.CeilToInt(Vector2.Distance(batchLast,pointer)/8),1,256);
            for(int i=1;i<=steps;i++){var point=Vector2.Lerp(batchLast,pointer,i/(float)steps);if(IsOverUi(point))continue;
                if(Physics.Raycast(view.ScreenPointToRay(point),out var hit,view.farClipPlane,1<<FarmWorld.InteractionLayer)){var plot=hit.collider.GetComponent<FarmPlot>();if(plot!=null)game.Batch.Add(plot.Index);}}
            batchLast=pointer;
        }

        private Vector3 OnGround(Vector2 pointer)
        {
            Ray ray = view.ScreenPointToRay(pointer);
            float distance;
            return ground.Raycast(ray, out distance) ? ray.GetPoint(distance) : focus;
        }

        private void BeginPinch(Vector2 a,Vector2 b)
        {
            game.Batch.CancelSelection();batchStroke=false;tracking=false;pinching=true;
            pinchAllowed=!IsOverUi(a)&&!IsOverUi(b);
            previousPinchDistance=Vector2.Distance(a,b);
        }
        private void ContinuePinch(Vector2 a,Vector2 b)
        {
            float distance=Vector2.Distance(a,b);
            if(pinchAllowed&&distance>1&&previousPinchDistance>1)Zoom(view.orthographicSize*previousPinchDistance/distance);
            previousPinchDistance=distance;
        }
        public void ZoomBy(float amount){CancelGesture();Zoom(view.orthographicSize+amount);}

        private bool IsOverUi(Vector2 pointer)
        { return game.Hud.BlocksWorld||PointerBlocked!=null&&PointerBlocked(pointer)||OverUi(pointer); }

        private static bool OverUi(Vector2 pointer)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = pointer };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            return results.Count > 0;
        }

        private void Zoom(float size) { view.orthographicSize = Mathf.Clamp(size, 5.5f, maximumZoom); PositionCamera(); }
        private void PositionCamera() { transform.position = focus - transform.forward * (maximumZoom>76?Mathf.Max(55,view.orthographicSize*2.5f):Mathf.Max(22,view.orthographicSize*1.8f)); }
        public void ShowIsland(){CancelGesture();focus=Vector3.zero;view.orthographicSize=Mathf.Min(maximumZoom,Mathf.Max(55,75/view.aspect));PositionCamera();}
        public void ShowTown(){SetView(game.IsEstate?new Vector3(-22,0,15):FarmWorld.TownCenter,16);}
        public void FocusOn(Vector3 point){CancelGesture();focus=point;view.orthographicSize=7;PositionCamera();}
        public void GoHome() { CancelGesture();focus = new Vector3(0,0,game.IsEstate?-5.5f:-.3f); view.orthographicSize = game.IsEstate?15:5.8f; PositionCamera(); }
    }
}
