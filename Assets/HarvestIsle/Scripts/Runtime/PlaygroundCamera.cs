using UnityEngine;
using UnityEngine.InputSystem;

namespace HarvestIsle
{
    public sealed class PlaygroundCamera : MonoBehaviour
    {
        private Camera cameraComponent;
        private Vector3 dragStart;
        private Vector3 focus;
        private ModelLibraryViewer library;
        private bool dragging;
        private void Awake(){cameraComponent=GetComponent<Camera>();library=FindFirstObjectByType<ModelLibraryViewer>();}
        public void FocusOn(Vector3 center)
        {focus=center;dragging=false;transform.position=center+new Vector3(15,18,-20);transform.LookAt(center+Vector3.up*.3f);cameraComponent.orthographicSize=9.6f;}
        private bool Ground(Vector2 screen,out Vector3 point)
        {
            var plane=new Plane(Vector3.up,new Vector3(0,0.15f,0));Ray ray=cameraComponent.ScreenPointToRay(screen);
            if(plane.Raycast(ray,out float distance)){point=ray.GetPoint(distance);return true;}point=Vector3.zero;return false;
        }
        private void Update()
        {
            if(library!=null&&library.Visible){dragging=false;return;}
            var mouse=Mouse.current;if(mouse==null)return;Vector2 position=mouse.position.ReadValue();
            bool overControls=(position.x<480&&position.y>Screen.height-140)||(position.x<370&&position.y>Screen.height-375)||(position.x>Screen.width-340&&position.y>Screen.height-180);
            if(mouse.leftButton.wasPressedThisFrame&&!overControls)dragging=Ground(position,out dragStart);
            if(mouse.leftButton.wasReleasedThisFrame)dragging=false;
            if(dragging&&Ground(position,out Vector3 point))
            {
                Vector3 delta=dragStart-point;delta.y=0;
                Vector3 next=transform.position+delta;
                next.x=Mathf.Clamp(next.x,focus.x+9,focus.x+21);next.z=Mathf.Clamp(next.z,focus.z-25,focus.z-13);transform.position=next;
            }
            float scroll=mouse.scroll.ReadValue().y;
            if(Mathf.Abs(scroll)>0.01f)cameraComponent.orthographicSize=Mathf.Clamp(cameraComponent.orthographicSize-scroll*0.008f,4.5f,12);
        }
    }
}
