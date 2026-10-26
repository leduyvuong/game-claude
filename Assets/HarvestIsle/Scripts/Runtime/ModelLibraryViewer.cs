using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HarvestIsle
{
    public sealed partial class ModelLibraryViewer : MonoBehaviour
    {
        private ModelLibraryCatalog catalog;
        private readonly List<string> groups=new List<string>();
        private readonly List<int> filtered=new List<int>();
        private int group,selected,variant,page;
        private string search="";
        private GameObject stage,model;
        private Camera cameraComponent;
        private Vector3 cameraRest;
        private Quaternion rotationRest;
        private float sizeRest;
        private float farClipRest;
        private int maskRest;
        private Color backgroundRest;
        private Rect cameraRectRest;
        public bool Visible {get;private set;}
        public int ModelCount {get{return catalog==null?0:catalog.models.Length;}}
        public ModelLibraryCatalog Catalog {get{return catalog;}}
        private void Awake()
        {
            var json=Resources.Load<TextAsset>("FullModelCatalog");if(json==null)return;catalog=JsonUtility.FromJson<ModelLibraryCatalog>(json.text);
            groups.Add("Tất cả");foreach(ModelLibraryEntry entry in catalog.models)if(!groups.Contains(entry.group))groups.Add(entry.group);Filter();
        }
        public bool Show()
        {
            if(catalog==null||Camera.main==null)return false;
            if(!Visible){cameraComponent=Camera.main;cameraRest=cameraComponent.transform.position;rotationRest=cameraComponent.transform.rotation;sizeRest=cameraComponent.orthographicSize;farClipRest=cameraComponent.farClipPlane;maskRest=cameraComponent.cullingMask;backgroundRest=cameraComponent.backgroundColor;cameraRectRest=cameraComponent.rect;}
            Visible=true;SetLibraryViewport();cameraComponent.cullingMask=1<<31;cameraComponent.backgroundColor=FarmWorld.Hex("F2EDE0");Select(selected,variant);return true;
        }
        public void Hide()
        {
            if(!Visible)return;Visible=false;if(stage!=null)stage.SetActive(false);if(cameraComponent!=null){cameraComponent.transform.SetPositionAndRotation(cameraRest,rotationRest);cameraComponent.orthographicSize=sizeRest;cameraComponent.farClipPlane=farClipRest;cameraComponent.cullingMask=maskRest;cameraComponent.backgroundColor=backgroundRest;cameraComponent.rect=cameraRectRest;}
        }
        public bool Select(int index,int stageIndex)
        {
            if(catalog==null||index<0||index>=catalog.models.Length)return false;var entry=catalog.models[index];if(stageIndex<0||stageIndex>=entry.variants.Length)return false;
            var prefab=Resources.Load<GameObject>(entry.variants[stageIndex].resource);if(prefab==null){Debug.LogError("Missing library prefab "+entry.variants[stageIndex].resource);return false;}
            selected=index;variant=stageIndex;if(!Visible)return true;
            if(stage==null){stage=new GameObject("Library preview stage");stage.transform.position=new Vector3(-50,0,0);}
            stage.SetActive(true);if(model!=null){model.SetActive(false);Destroy(model);}model=Instantiate(prefab,stage.transform);model.name=entry.label+" · "+entry.variants[stageIndex].label;
            foreach(Transform t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            var rs=model.GetComponentsInChildren<Renderer>();Bounds bounds=rs[0].bounds;foreach(Renderer r in rs)bounds.Encapsulate(r.bounds);
            cameraComponent.transform.position=bounds.center+new Vector3(4,3.4f,5.5f)*Mathf.Max(.5f,bounds.size.magnitude);cameraComponent.transform.LookAt(bounds.center);
            float frame=.11f;Quaternion inv=Quaternion.Inverse(cameraComponent.transform.rotation);for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){Vector3 p=inv*Vector3.Scale(bounds.extents,new Vector3(x,y,z));frame=Mathf.Max(frame,Mathf.Abs(p.y),Mathf.Abs(p.x)/Mathf.Max(.4f,cameraComponent.aspect));}
            cameraComponent.orthographicSize=frame*1.22f;cameraComponent.farClipPlane=1500;return true;
        }
        private void Filter()
        {filtered.Clear();if(catalog==null)return;for(int i=0;i<catalog.models.Length;i++){var m=catalog.models[i];if((group==0||m.group==groups[group])&&(m.label.IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0||m.id.IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0))filtered.Add(i);}page=0;}
        private void Update()
        {
            if(!Visible)return;SetLibraryViewport();
            if(Mouse.current==null||!cameraComponent.pixelRect.Contains(Mouse.current.position.ReadValue()))return;
            float scroll=Mouse.current.scroll.ReadValue().y;if(Mathf.Abs(scroll)>.01f)cameraComponent.orthographicSize=Mathf.Clamp(cameraComponent.orthographicSize*(1-scroll*.001f),.07f,120);
        }
        private void OnDestroy(){Hide();if(stage!=null)Destroy(stage);if(libraryButtonSurface!=null)Destroy(libraryButtonSurface);if(libraryHoverSurface!=null)Destroy(libraryHoverSurface);}
    }
}
