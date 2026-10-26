using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HarvestIsle
{
    // A separate playable exhibition. No reads or writes to the player's normal farm.
    public sealed partial class FarmModelPark : MonoBehaviour
    {
        public sealed class Exhibit
        {
            public string name, resource, group;
            public string type;
            public float displayScale;
            public GameObject root, model;
            public TextMesh label;
            public Vector3 center;
        }
        public sealed class District
        {
            public string name;
            public Rect area;
            public int first, count;
        }
        public FarmGame Game {get;private set;}
        public bool Ready {get;private set;}
        public string Error {get;private set;}
        public readonly List<Exhibit> Exhibits = new List<Exhibit>();
        public readonly List<District> Districts = new List<District>();
        public const int TestCoins = 999000000;
        public int ExpectedCount {get;private set;}
        private Transform park;
        private float northEdge;
        private int selected = -1;
        private bool farmView, menu;
        private Vector2 menuScroll;
        private float nextLabels;
        private Font font;
        private RenderPipelineAsset previousPipeline;
        private UniversalRenderPipelineAsset parkPipeline;

        private IEnumerator Start()
        {
            var json=Resources.Load<TextAsset>("FullModelCatalog");
            if(json==null){Error="Không tìm thấy danh mục model.";yield break;}
            var catalog=JsonUtility.FromJson<ModelLibraryCatalog>(json.text);
            foreach(var entry in catalog.models)ExpectedCount+=entry.variants.Length;
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Game=gameObject.AddComponent<FarmGame>();
            var state=CreateState(FarmGame.UtcNow());
            string reason;if(!FarmModel.Validate(state,out reason)){Error=reason;yield break;}
            Game.InitializeSession(new FarmSaveStore(Path.Combine(Application.persistentDataPath,"ModelPark")),FarmGame.UtcNow,state);
            Game.enabled=false;
            Game.Hud.gameObject.SetActive(false);
            Game.CameraController.enabled=false;
            Game.World.GameCamera.farClipPlane=700;
            Game.World.GameCamera.transform.rotation=Quaternion.Euler(56,0,0);
            // Calm daylight in the exhibition; the playable farm retains its normal rules.
            RenderSettings.fog=false;
            RenderSettings.ambientLight=new Color(.55f,.65f,.60f);
            RenderSettings.sun.intensity=.85f;
            previousPipeline=QualitySettings.renderPipeline;
            var source=(previousPipeline??GraphicsSettings.defaultRenderPipeline) as UniversalRenderPipelineAsset;
            if(source!=null)
            {
                parkPipeline=Instantiate(source);parkPipeline.name="Model park lighting (runtime)";
                parkPipeline.shadowDistance=350;parkPipeline.mainLightShadowmapResolution=4096;
                parkPipeline.shadowCascadeCount=4;QualitySettings.renderPipeline=parkPipeline;
            }
            var sea=Game.World.transform.Find("SeaSurface(Clone)");if(sea!=null)sea.gameObject.SetActive(false);
            Game.World.GameCamera.backgroundColor=FarmWorld.Hex("6CBFC2");
            Game.World.GameCamera.allowHDR=false;
            var season=GetComponent<FarmSeasons>();if(season!=null)season.enabled=false;
            GetComponent<FarmClimate>().enabled=false;
            park=new GameObject("Mùa Vàng · Công viên 600 model").transform;park.SetParent(Game.World.transform,false);
            DressFarm();
            var grouped=new Dictionary<string,List<ModelLibraryEntry>>();
            foreach(var entry in catalog.models)
            {if(!grouped.ContainsKey(entry.group))grouped[entry.group]=new List<ModelLibraryEntry>();grouped[entry.group].Add(entry);}
            float x=-125,z=65,rowDepth=0;int groupIndex=0;
            foreach(var pair in grouped)
            {
                int count=0;foreach(var entry in pair.Value)count+=entry.variants.Length;
                int columns=Mathf.Clamp(Mathf.CeilToInt(Mathf.Sqrt(count*1.3f)),3,14);
                int rows=Mathf.CeilToInt((float)count/columns);
                float width=columns*3.1f+3,depth=rows*3.6f+6;
                if(x+width>125){x=-125;z+=rowDepth+5;rowDepth=0;}
                var district=new District{name=pair.Key,area=new Rect(x,z,width,depth),first=Exhibits.Count,count=count};
                Districts.Add(district);BuildDistrict(district,groupIndex++);
                int cell=0;
                foreach(var entry in pair.Value)
                {
                    int first=Exhibits.Count;
                    foreach(var variant in entry.variants)
                    {
                        Vector3 p=new Vector3(x+3+cell%columns*3.1f,.42f,z+3.5f+cell/columns*3.6f);
                        if(!BuildExhibit(entry,variant,p,groupIndex)){yield break;}
                        cell++;if(cell%14==0)yield return null;
                    }
                    // Growth stages and building levels share one scale, based on the largest variant.
                    float commonScale=float.MaxValue;
                    for(int i=first;i<Exhibits.Count;i++)commonScale=Mathf.Min(commonScale,Exhibits[i].displayScale);
                    for(int i=first;i<Exhibits.Count;i++)
                    {
                        var exhibit=Exhibits[i];exhibit.model.transform.localScale*=commonScale/exhibit.displayScale;
                        var renderers=exhibit.model.GetComponentsInChildren<Renderer>();Bounds b=renderers[0].bounds;
                        foreach(var renderer in renderers)b.Encapsulate(renderer.bounds);
                        exhibit.model.transform.position+=new Vector3(exhibit.center.x-b.center.x,exhibit.center.y-b.min.y,exhibit.center.z-b.center.z);
                        exhibit.displayScale=commonScale;
                    }
                }
                x+=width+5;rowDepth=Mathf.Max(rowDepth,depth);northEdge=Mathf.Max(northEdge,z+depth);
            }
            BuildPromenade();
            Game.CameraController.ConfigureMap(new Vector2(-142,-55),new Vector2(142,northEdge+12),Mathf.Max(110,northEdge));
            Game.CameraController.PointerBlocked=BlocksPointer;
            Game.CameraController.InspectRay=SelectRay;
            Game.CameraController.enabled=true;
            Ready=true;ShowOverview();Game.Save();
            Game.Notify("Công viên Mùa Vàng · đủ "+Exhibits.Count+" mẫu. Chọn một khu hoặc chạm model để xem gần.");
        }

        public static FarmState CreateState(double now)
        {
            var model=new FarmModel(FarmModel.NewState(now));var s=model.State;
            s.coins=TestCoins;s.harvested=100;s.hasSold=true;s.hasWatered=true;
            s.development.experience=400;s.development.ordersCompleted=10;s.development.crafted=20;
            s.development.wood=10000;s.development.ore=10000;
            model.UnlockParcel();model.UnlockParcel();
            foreach(var b in DevelopmentCatalog.Buildings)for(int i=0;i<3;i++)model.UpgradeBuilding(b.id);
            for(int i=0;i<s.plots.Count;i++)
            {
                var crop=CropCatalog.All[i%CropCatalog.All.Length];
                model.Apply(i,FarmTool.Plant,crop.id,now);
                s.plots[i].plantedAt=now-crop.growSeconds-2;s.plots[i].watered=true;
                s.plots[i].soilQuality=2;s.plots[i].soilWater=.25;s.plots[i].drainageBuilt=true;
            }
            foreach(var spec in LivestockCatalog.All)for(int i=0;i<3;i++)model.BuyAnimal(spec.id);
            s.workerHouse=3; // Leave hiring to the tester so the displayed ripe crops remain visible.
            s.hotpotServed=100;s.hotpot.floors=4;
            foreach(var spec in HotpotRules.Ingredients)s.hotpot.stock.Add(new KitchenStock{id=spec.id,stars=1,count=500});
            var recipe=new HotpotRecipe{name="Lẩu vườn Mùa Vàng",broth=0,style=0};
            recipe.portions.Add(new HotpotPortion("carrot",2,1));recipe.portions.Add(new HotpotPortion("corn",1,1));
            if(model.TestHotpotRecipe(recipe).success)model.SetHotpotMenu(s.hotpot.recipes[0].id,true);
            foreach(var item in s.inventory)item.count=500;
            foreach(var recipeSpec in DevelopmentCatalog.Recipes)s.development.products.Add(new ProductStock{id=recipeSpec.id,count=100});
            s.eggs=s.milk=s.honey=s.fishStock=500;s.coins=TestCoins;
            return s;
        }

        public void Refill()
        {
            Game.Model.State.coins=TestCoins;
            Game.Model.State.development.wood=Game.Model.State.development.ore=10000;
            foreach(var item in Game.Model.State.inventory)item.count=500;
            foreach(var item in Game.Model.State.hotpot.stock)item.count=500;
            Game.Save();Game.Hud.Refresh();Game.Notify("Đã nạp tiền, gỗ, quặng và nguyên liệu cho màn chơi thử.");
        }

        private bool BuildExhibit(ModelLibraryEntry entry,ModelLibraryVariant variant,Vector3 position,int groupIndex)
        {
            var prefab=Resources.Load<GameObject>(variant.resource);
            if(prefab==null){Error="Thiếu model: "+variant.resource;return false;}
            var root=new GameObject("Exhibit "+Exhibits.Count+" · "+entry.label+" · "+variant.label);
            root.transform.SetParent(park,false);root.transform.position=position;
            var model=Instantiate(prefab,root.transform,false);
            foreach(var collider in model.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            var renderers=model.GetComponentsInChildren<Renderer>();
            if(renderers.Length==0){Error="Model không có hình: "+variant.resource;return false;}
            Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            float scale=2.05f/Mathf.Max(.01f,bounds.size.x,bounds.size.z,bounds.size.y*.8f);
            model.transform.localScale*=scale;
            bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            model.transform.position+=new Vector3(position.x-bounds.center.x,position.y-bounds.min.y,position.z-bounds.center.z);
            var plate=Block("Bệ trưng bày",new Vector3(position.x,.28f,position.z),new Vector3(2.7f,.16f,2.85f),groupIndex%2==0?"F0E5CC":"F8EED8");
            plate.layer=ExhibitLayer;var hit=plate.AddComponent<BoxCollider>();
            hit.center=new Vector3(0,.7f,0);hit.size=new Vector3(1,10,1);
            plate.AddComponent<ModelParkTarget>().Index=Exhibits.Count;
            string name=entry.label.Length>26?entry.label.Substring(0,24)+"…":entry.label;
            var label=Sign(name+"\n"+variant.label,new Vector3(position.x,.43f,position.z-1.24f),.033f,FarmWorld.Hex("36594C"));
            Exhibits.Add(new Exhibit{name=entry.label+" · "+variant.label,resource=variant.resource,group=entry.group,type=entry.id,displayScale=scale,root=root,model=model,label=label,center=position});
            return true;
        }

        public const int ExhibitLayer=29;
        public bool SelectExhibit(int index)
        {
            if(!Ready||index<0||index>=Exhibits.Count)return false;
            CloseFarmPanels();selected=index;menu=false;farmView=false;Game.Hud.gameObject.SetActive(false);
            Game.enabled=false;
            Game.CameraController.SetView(Exhibits[index].center,5.5f);return true;
        }
        private bool SelectRay(Ray ray)
        {if(Physics.Raycast(ray,out var hit,650,1<<ExhibitLayer)){var target=hit.collider.GetComponent<ModelParkTarget>();return target!=null&&SelectExhibit(target.Index);}return false;}
        public void ShowDistrict(int index)
        {
            if(!Ready||index<0||index>=Districts.Count)return;
            CloseFarmPanels();selected=-1;menu=false;farmView=false;Game.Hud.gameObject.SetActive(false);
            Game.enabled=false;
            Rect area=Districts[index].area;
            float size=Mathf.Max(8,area.height*.6f,area.width/Game.World.GameCamera.aspect*.65f);
            Game.CameraController.SetView(new Vector3(area.center.x,0,area.center.y+size*.3f),size);
        }
        public void ShowOverview()
        {
            if(!Ready)return;CloseFarmPanels();selected=-1;menu=false;farmView=false;Game.Hud.gameObject.SetActive(false);
            Game.enabled=false;
            float size=Mathf.Max((northEdge+30)*.56f,145/Game.World.GameCamera.aspect);
            Game.CameraController.SetView(new Vector3(0,0,(northEdge-14)*.5f+size*.3f),size);
        }
        public void ShowFarm()
        {
            if(!Ready)return;CloseFarmPanels();selected=-1;menu=false;farmView=true;Game.Hud.gameObject.SetActive(true);
            Game.enabled=true;
            Game.CameraController.SetView(new Vector3(0,0,-1),14);
            foreach(var text in Game.Hud.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                if(text.text==FarmModel.Money(Game.Model.State.coins)){text.resizeTextForBestFit=true;text.resizeTextMinSize=10;}
        }
        private void CloseFarmPanels()
        {
            if(Game.Development.IsOpen)Game.Development.Close();
            if(Game.Ranch.IsOpen)Game.Ranch.Close();
            if(Game.Hotpot.IsOpen)Game.Hotpot.Close();
        }
        private void Update()
        {
            if(!Ready||Time.unscaledTime<nextLabels)return;nextLabels=Time.unscaledTime+.4f;
            var camera=Game.World.GameCamera;bool near=camera.orthographicSize<16;
            foreach(var exhibit in Exhibits)
            {
                Vector3 p=camera.WorldToViewportPoint(exhibit.center);
                exhibit.label.gameObject.SetActive(near&&p.z>0&&p.x>-.1&&p.x<1.1&&p.y>-.1&&p.y<1.1);
            }
        }
        private void OnDestroy()
        {
            if(Game!=null&&Game.CameraController!=null){Game.CameraController.PointerBlocked=null;Game.CameraController.InspectRay=null;}
            if(parkPipeline!=null){QualitySettings.renderPipeline=previousPipeline;Destroy(parkPipeline);}
        }
    }

    public sealed class ModelParkTarget : MonoBehaviour { public int Index; }
}
