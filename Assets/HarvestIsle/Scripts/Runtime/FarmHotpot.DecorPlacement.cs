using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private bool decorPlacing;
        private int pendingSlot=-1,pendingFloor,pendingSource=-1,previewKind=-1;
        private GameObject decorStage,decorModel,placementGhost;
        private Camera decorCamera;
        private RenderTexture decorRender;
        private Material ghostMaterial;
        private Bounds previewBounds;
        private float placementViewSize;

        private GameObject CreateDecorModel(int kind,Transform parent)
        {
            GameObject node;
            if(kind>=8)
            {
                var prefab=Resources.Load<GameObject>(HotpotFurnitureCatalog.Resource(kind));
                if(prefab==null)return null;
                node=Instantiate(prefab,parent,false);
            }
            else
            {
                node=new GameObject("Basic furniture preview");node.transform.SetParent(parent,false);
                node.transform.localScale=Vector3.one*1.12f;
                if(kind<6)BuildDiningTable(node.transform,kind,0);
                else if(kind==6)Planter(node.transform,Vector3.zero,1.4f);
                else Pendant(node.transform,new Vector3(0,2.6f,0));
            }
            foreach(var collider in node.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var light in node.GetComponentsInChildren<Light>(true))light.enabled=false;
            return node;
        }
        private void EnsureDecorPreview()
        {
            if(decorStage==null)
            {
                decorStage=new GameObject("Furniture shop preview");decorStage.transform.SetParent(transform,false);
                decorStage.transform.localPosition=new Vector3(500,0,180);
                var cameraObject=new GameObject("Furniture preview camera");cameraObject.transform.SetParent(decorStage.transform,false);
                decorCamera=cameraObject.AddComponent<Camera>();decorCamera.orthographic=true;decorCamera.cullingMask=1<<30;
                decorCamera.clearFlags=CameraClearFlags.SolidColor;decorCamera.backgroundColor=FarmWorld.Hex("E8D8BC");
                decorCamera.nearClipPlane=.01f;decorCamera.farClipPlane=100;
                decorRender=new RenderTexture(512,340,16){name="Furniture shop preview"};decorRender.Create();decorCamera.targetTexture=decorRender;
            }
            decorStage.SetActive(true);
            if(previewKind==furnitureKind&&decorModel!=null)return;
            if(decorModel!=null){decorModel.SetActive(false);Destroy(decorModel);}
            previewKind=furnitureKind;decorModel=CreateDecorModel(furnitureKind,decorStage.transform);
            if(decorModel==null)return;
            foreach(var t in decorModel.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
            FrameDecorPreview();
        }
        private void FrameDecorPreview()
        {
            var renderers=decorModel.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return;
            previewBounds=renderers[0].bounds;foreach(var r in renderers)previewBounds.Encapsulate(r.bounds);
            float radius=Mathf.Max(.2f,previewBounds.extents.magnitude);
            decorCamera.transform.position=previewBounds.center+new Vector3(1,1.05f,-1.4f).normalized*(radius*3+2);
            decorCamera.transform.LookAt(previewBounds.center);decorCamera.orthographicSize=radius*1.1f;
        }
        private void RotateDecorPreview(float degrees)
        {if(decorModel==null)return;decorModel.transform.RotateAround(previewBounds.center,Vector3.up,degrees);FrameDecorPreview();}

        // UI and isolated Editor review share this transaction boundary. Preview never writes the save.
        public bool BeginDecorPlacement(int kind,int floor,int source=-1)
        {
            if(!IsOpen||tab!=4||game.Online!=null||kind<0||kind>=HotpotFurnitureCatalog.Items.Length||floor<0||floor>=game.Model.State.hotpot.floors)return false;
            var existing=game.Model.State.hotpot.furniture.Find(f=>f.floor==floor&&f.slot==source&&f.kind==kind);
            if(source>=0&&existing==null)return false;
            CancelDecorPlacement();ShowFloor(floor);
            placementViewSize=viewSize;viewSize=12.5f;
            furnitureKind=kind;pendingFloor=floor;pendingSource=source;pendingSlot=source;furnitureRotation=existing==null?0:existing.rotation;
            decorPlacing=true;moveFrom=source;PositionCamera();
            if(decorStage!=null)decorStage.SetActive(false);
            message=source<0?"Chọn vị trí xanh để đặt thử. Chỉ trừ tiền khi xác nhận mua.":"Chọn vị trí mới trên tầng này. Di chuyển và xoay miễn phí.";
            RefreshPlacementGhost();return true;
        }
        public void CancelDecorPlacement()
        {
            bool restore=decorPlacing;decorPlacing=false;
            if(restore){viewSize=placementViewSize;PositionCamera();}
            pendingSlot=-1;pendingSource=-1;moveFrom=-1;
            if(placementGhost!=null){placementGhost.SetActive(false);Destroy(placementGhost);placementGhost=null;}
        }
        private bool CanPlaceDecor(int slot)
        {
            if(!decorPlacing||pendingFloor!=currentFloor||!HotpotFurnitureCatalog.Fits(furnitureKind,slot)||(pendingFloor==0&&slot<6))return false;
            if(game.Model.State.hotpot.furniture.Exists(f=>f.floor==pendingFloor&&f.slot==slot&&f.slot!=pendingSource))return false;
            return pendingSource>=0?game.Model.State.hotpot.furniture.Exists(f=>f.floor==pendingFloor&&f.slot==pendingSource&&f.kind==furnitureKind):game.Model.State.coins>=HotpotFurnitureCatalog.Items[furnitureKind].price;
        }
        public bool SelectDecorPosition(int slot)
        {
            if(!IsOpen||tab!=4||!CanPlaceDecor(slot))return false;
            pendingSlot=slot;RefreshPlacementGhost();return true;
        }
        public bool ConfirmDecorPlacement()
        {
            if(!IsOpen||tab!=4||game.Online!=null||!CanPlaceDecor(pendingSlot))return false;
            int floor=pendingFloor,slot=pendingSlot,source=pendingSource,kind=furnitureKind,rotation=furnitureRotation;
            // Clear pending state first so two clicks cannot perform the same purchase twice.
            CancelDecorPlacement();bool success;
            if(source>=0)success=MoveFurniture(floor,source,slot,rotation);
            else{var result=game.Model.PlaceHotpotFurniture(floor,slot,kind,rotation);Perform(result);success=result.success;if(success)BuildRoom();}
            if(success){ownedFloor=floor;ownedSlot=slot;}
            return success;
        }
        private void RefreshPlacementGhost()
        {
            if(placementGhost!=null){placementGhost.SetActive(false);Destroy(placementGhost);placementGhost=null;}
            if(!decorPlacing||pendingSlot<0)return;
            placementGhost=CreateDecorModel(furnitureKind,room.transform);if(placementGhost==null)return;
            placementGhost.name="Furniture placement preview";
            placementGhost.transform.localPosition=SlotPosition(pendingSlot);
            placementGhost.transform.localRotation=HotpotFurnitureCatalog.Items[furnitureKind].mount==HotpotMount.Wall?
                Quaternion.Euler(0,pendingSlot>=16?-90:0,0)*Quaternion.AngleAxis(furnitureRotation*90,Vector3.forward):Quaternion.Euler(0,furnitureRotation*90,0);
            if(ghostMaterial==null)
            {
                ghostMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Furniture preview only"};
                ghostMaterial.SetFloat("_Surface",1);ghostMaterial.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
                ghostMaterial.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);ghostMaterial.SetFloat("_ZWrite",0);
                ghostMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");ghostMaterial.renderQueue=(int)RenderQueue.Transparent;
                ghostMaterial.SetColor("_BaseColor",new Color(.25f,.95f,.65f,.48f));
            }
            foreach(var renderer in placementGhost.GetComponentsInChildren<Renderer>())
            {var materials=new Material[renderer.sharedMaterials.Length];for(int i=0;i<materials.Length;i++)materials[i]=ghostMaterial;renderer.sharedMaterials=materials;renderer.shadowCastingMode=ShadowCastingMode.Off;}
        }
        private void DrawDecorPlacement()
        {
            Panel(new Rect(16,91,1168,61));
            for(int floor=0;floor<game.Model.State.hotpot.floors;floor++)
            {
                GUI.enabled=pendingSource<0||floor==pendingFloor;
                if(Button(28+floor*108,102,100,39,(floor==currentFloor?"● ":"")+"Tầng "+(floor+1)))BeginDecorPlacement(furnitureKind,floor,pendingSource);
            }
            GUI.enabled=true;
            GUI.enabled=game.Model.State.hotpot.floors<4;
            if(Button(473,102,352,39,game.Model.State.hotpot.floors>=4?"Đã mở đủ 4 tầng":"Mở tầng · "+FarmModel.Money(game.Model.NextHotpotFloorCost)))
            {CancelDecorPlacement();var result=game.Model.ExpandHotpot();Perform(result);if(result.success)BuildRoom();return;}
            GUI.enabled=true;
            if(Button(840,102,160,39,"Góc nhìn trái")){viewAngle-=90;PositionCamera();}
            if(Button(1010,102,160,39,"Góc nhìn phải")){viewAngle+=90;PositionCamera();}
            for(int slot=0;slot<HotpotFurnitureCatalog.SlotCount;slot++)
            {
                if(!HotpotFurnitureCatalog.Fits(furnitureKind,slot))continue;
                if(slot>=12&&slot<20)
                {var wall=floorRoots[currentFloor].transform.Find(slot>=16?"Left cutaway":"Back cutaway");if(wall!=null&&!wall.gameObject.activeSelf)continue;}
                Vector3 p=interiorCamera.WorldToScreenPoint(room.transform.position+SlotPosition(slot)+Vector3.up*.2f);
                float x=(p.x-guiArea.x)/guiScale,y=(Screen.height-p.y-guiArea.y)/guiScale;
                if(p.z<=0||y<174||y>493||x<28||x>1172)continue;
                bool valid=CanPlaceDecor(slot);
                Ink(new Rect(x-23,y-23,46,46),FarmWorld.Hex(slot==pendingSlot?"D9AC52":valid?"397B59":"803D36"),true);
                GUI.Label(new Rect(x-23,y-23,46,46),slot==pendingSlot?"✓":valid?"+":"×",smallStyle);
                if(GUI.Button(new Rect(x-25,y-25,50,50),GUIContent.none,GUIStyle.none))
                {if(!SelectDecorPosition(slot))message="Vị trí này đã có đồ hoặc dành cho bàn theo hạng quán.";}
            }
            Panel(new Rect(16,512,1168,77));
            Label(30,521,414,30,HotpotFurnitureCatalog.Items[furnitureKind].name,true);
            Label(30,554,414,27,pendingSource>=0?"Di chuyển miễn phí · "+MountLabel(furnitureKind):FarmModel.Money(HotpotFurnitureCatalog.Items[furnitureKind].price)+" · "+MountLabel(furnitureKind));
            if(Button(455,527,135,47,"Xoay 90°")){furnitureRotation=(furnitureRotation+1)%4;RefreshPlacementGhost();}
            if(Button(601,527,143,47,"Hủy")){CancelDecorPlacement();return;}
            GUI.enabled=CanPlaceDecor(pendingSlot);
            if(Button(757,527,410,47,pendingSource>=0?"Xác nhận vị trí":"Xác nhận mua & đặt"))ConfirmDecorPlacement();
            GUI.enabled=true;
        }
        private void DisposeDecorPreview()
        {
            CancelDecorPlacement();
            if(decorCamera!=null)decorCamera.targetTexture=null;
            if(decorStage!=null)Destroy(decorStage);
            if(decorRender!=null){decorRender.Release();Destroy(decorRender);}
            if(ghostMaterial!=null)Destroy(ghostMaterial);
            decorStage=null;decorRender=null;decorModel=null;decorCamera=null;ghostMaterial=null;previewKind=-1;
        }
    }
}
