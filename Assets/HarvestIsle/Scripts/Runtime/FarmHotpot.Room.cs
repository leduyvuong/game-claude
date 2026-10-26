using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private GameObject room;
        private Camera interiorCamera;
        private readonly List<GameObject> floorRoots=new List<GameObject>();
        private readonly Dictionary<string,Material> roomMaterials=new Dictionary<string,Material>();
        private readonly List<Mesh> roomMeshes=new List<Mesh>();
        private string roomLayout;
        private Texture2D woodGrain, steamMap;
        private Material steamMaterial;
        private int currentFloor, selectedTable=-1;
        private float viewAngle, viewSize=9.5f;
        private GameObject Part(PrimitiveType type,string name,Transform parent,Vector3 p,Vector3 scale,string hex)
        {
            var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=p;o.transform.localScale=scale;
            var collider=o.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            bool wood=name.Contains("Wood")||name.Contains("Chair")||name.Contains("Plank");
            string key=hex+(wood?"wood":"")+(name.Contains("Steel")?"steel":"");Material mat;
            if(!roomMaterials.TryGetValue(key,out mat))
            {
                mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=FarmWorld.Hex(hex),enableInstancing=true};
                mat.SetFloat("_Smoothness",name.Contains("Steel")?.65f:.22f);
                if(name.Contains("Steel"))mat.SetFloat("_Metallic",.65f);
                if(name=="Warm bulb"||name=="Sconce warm glass")
                {mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",FarmWorld.Hex(hex)*1.4f);}
                if(wood)
                {
                    if(woodGrain==null)
                    {
                        woodGrain=new Texture2D(128,128,TextureFormat.RGB24,false);woodGrain.name="Hotpot wood grain";
                        var pixels=new Color[128*128];
                        for(int y=0;y<128;y++)for(int x=0;x<128;x++)
                        {float v=.84f+.10f*Mathf.Sin(y*1.5f+Mathf.Sin(x*.08f)*1.4f)+.04f*Mathf.Sin(y*4.1f+x*.15f);pixels[y*128+x]=new Color(v,v,v);}
                        woodGrain.SetPixels(pixels);woodGrain.Apply(false,true);
                    }
                    mat.SetTexture("_BaseMap",woodGrain);
                }
                roomMaterials.Add(key,mat);
            }
            o.GetComponent<Renderer>().sharedMaterial=mat;return o;
        }
        private void BuildRoom()
        {
            string layout=game.Model.State.hotpot.floors+":"+game.Model.HotpotTables+":"+game.Model.KitchenLevel+":"+game.Model.State.hotpot.progress.branch;
            foreach(var f in game.Model.State.hotpot.furniture)layout+=";"+f.floor+","+f.slot+","+f.kind+","+f.rotation;
            if(room!=null&&roomLayout==layout){room.SetActive(true);interiorCamera.enabled=true;ShowFloor(currentFloor);return;}
            if(room!=null){room.SetActive(false);Destroy(room);}
            foreach(var mesh in roomMeshes)Destroy(mesh);roomMeshes.Clear();roomLayout=layout;
            var old=new List<Table>(tables);tables.Clear();floorRoots.Clear();
            RenderSettings.ambientLight=FarmWorld.Hex(game.Model.State.hotpot.progress.branch%4==3?"7B7895":"8D7B64");
            room=new GameObject("Hotpot 3D interior");room.transform.SetParent(transform,false);room.transform.localPosition=new Vector3(0,0,180);
            interiorCamera=new GameObject("Hotpot camera").AddComponent<Camera>();interiorCamera.transform.SetParent(room.transform,false);
            interiorCamera.orthographic=true;interiorCamera.backgroundColor=FarmWorld.Hex("998E7B");interiorCamera.clearFlags=CameraClearFlags.SolidColor;
            interiorCamera.nearClipPlane=.1f;interiorCamera.farClipPlane=70;interiorCamera.depth=2;
            for(int f=0;f<game.Model.State.hotpot.floors;f++)
            {
                var root=new GameObject("Dining floor "+f);root.transform.SetParent(room.transform,false);floorRoots.Add(root);
                BuildRestaurantShell(root.transform,f);
                BuildBranchSurroundings(root.transform);
                CombineStatic(root.transform);
                for(int s=0;s<HotpotFurnitureCatalog.SlotCount;s++)
                {
                    var furniture=game.Model.State.hotpot.furniture.Find(v=>v.floor==f&&v.slot==s);
                    int kind=f==0&&s<game.Model.HotpotTables?0:furniture==null?-1:furniture.kind;
                    if(kind<0)continue;
                    var node=new GameObject("Furniture "+f+" "+s);node.transform.SetParent(root.transform,false);node.transform.localPosition=SlotPosition(s);
                    node.transform.localScale=Vector3.one*(kind<8?1.12f:1);
                    node.transform.localRotation=Quaternion.Euler(0,furniture==null?0:furniture.rotation*90,0);
                    if(kind>=HotpotFurnitureCatalog.ReferenceStart)
                    {
                        if(HotpotFurnitureCatalog.SlotMount(s)==HotpotMount.Wall)
                        {
                            bool side=s>=16;node.transform.SetParent(root.transform.Find(side?"Left cutaway":"Back cutaway"),true);
                            node.transform.localRotation=Quaternion.Euler(0,side?-90:0,0)*Quaternion.AngleAxis(furniture.rotation*90,Vector3.forward);
                        }
                        var prefab=Resources.Load<GameObject>(HotpotFurnitureCatalog.Resource(kind));
                        if(prefab==null){Debug.LogError("Missing baked hotpot furniture: "+kind);continue;}
                        var instance=Instantiate(prefab,node.transform,false);var view=instance.GetComponent<HotpotFurnitureView>();
                        foreach(var c in instance.GetComponentsInChildren<Collider>())c.enabled=false;
                        if(!HotpotFurnitureCatalog.IsTable(kind))continue;
                        var diners=new GameObject("Dining guests");diners.transform.SetParent(node.transform,false);
                        // Seats are authored in the prefab, including booth/counter/floor seating heights.
                        for(int seat=0;seat<Mathf.Min(2,view.seats.Length);seat++)
                        {
                            var anchor=view.seats[seat];var socket=new GameObject("Diner socket").transform;socket.SetParent(diners.transform,false);
                            socket.position=anchor.position;socket.rotation=anchor.rotation;socket.localScale=instance.transform.Find("Structure").localScale;
                            Guest(socket,Vector3.zero,0,seat==0);
                        }
                        var session=old.Find(t=>t.floor==f&&t.slot==s)??new Table{floor=f,slot=s};session.pot=view.serving;session.guest=diners.transform;tables.Add(session);SyncTable(session);continue;
                    }
                    if(kind>=6){if(kind==6)Planter(node.transform,Vector3.zero,1.4f);else Pendant(node.transform,new Vector3(0,2.6f,0));continue;}
                    BuildDiningTable(node.transform,kind,s);
                    CombineStatic(node.transform);
                    var pot=new GameObject("Served hotpot and dishes");pot.transform.SetParent(node.transform,false);
                    Cookpot(pot.transform,new Vector3(0,1.20f,0),.65f,true);
                    FoodTray(pot.transform,new Vector3(-.55f,1.17f,.43f),0);
                    FoodTray(pot.transform,new Vector3(.5f,1.17f,-.42f),1);
                    CombineStatic(pot.transform);
                    var guest=new GameObject("Dining guests");guest.transform.SetParent(node.transform,false);
                    Guest(guest.transform,new Vector3(-1.23f,.08f,0),90,s%2==0);
                    Guest(guest.transform,new Vector3(1.23f,.08f,0),-90,s%2!=0);
                    var table=old.Find(t=>t.floor==f&&t.slot==s)??new Table{floor=f,slot=s};table.pot=pot.transform;table.guest=guest.transform;tables.Add(table);SyncTable(table);
                }
            }
            ShowFloor(Mathf.Min(currentFloor,floorRoots.Count-1));
        }
        private static Vector3 SlotPosition(int slot)
        {
            if(slot>=20)return new Vector3(-4.7f+(slot-20)%3*4.7f,3.7f,2.3f-(slot-20)/3*6.3f);
            if(slot>=16)return new Vector3(-7.76f,2.45f,5.5f-(slot-16)*3.5f);
            if(slot>=12)return new Vector3(-5.5f+(slot-12)*3.6f,2.70f,8.20f);
            int[] displayOrder={0,2,4,6,8,10,1,3,5,7,9,11};int cell=displayOrder[slot];
            return new Vector3(-4.7f+(cell%3)*4.7f,0,3.4f-(cell/3)*3.15f);
        }
        private void SyncTable(Table t)
        {if(t.guest!=null)t.guest.gameObject.SetActive(t.occupied);if(t.pot!=null)t.pot.gameObject.SetActive(t.served);SyncSpecialtyVisual(t);}
        public void ShowFloor(int floor)
        {
            if(floor<0||floor>=floorRoots.Count)return;
            currentFloor=floor;selectedTable=-1;moveFrom=-1;for(int i=0;i<floorRoots.Count;i++)floorRoots[i].SetActive(i==floor);PositionCamera();
        }
        private void PositionCamera()
        {
            if(interiorCamera==null)return;
            interiorCamera.rect=FarmWindow.Viewport(1200,720);
            var target=room.transform.position+new Vector3(0,.6f,.1f);
            // Orthographic framing stays identical while casters remain within the 35m URP shadow range.
            interiorCamera.transform.position=target+Quaternion.Euler(0,viewAngle,0)*new Vector3(7.8f,12.35f,-14.95f);
            interiorCamera.transform.LookAt(target);interiorCamera.orthographicSize=viewSize;
            if(decorPlacing&&HotpotFurnitureCatalog.Items[furnitureKind].mount==HotpotMount.Floor)
                interiorCamera.transform.position-=interiorCamera.transform.up*2.1f;
            Vector3 direction=interiorCamera.transform.position-target;
            foreach(var floor in floorRoots)
            {
                var back=floor.transform.Find("Back cutaway");if(back!=null)back.gameObject.SetActive(direction.z<0);
                var left=floor.transform.Find("Left cutaway");if(left!=null)left.gameObject.SetActive(direction.x>0);
            }
        }
        private void OnDestroy()
        {
            DisposeDecorPreview();
            foreach(var mesh in roomMeshes)if(mesh!=null)Destroy(mesh);
            foreach(var material in roomMaterials.Values)if(material!=null)Destroy(material);
            if(woodGrain!=null)Destroy(woodGrain);if(steamMaterial!=null)Destroy(steamMaterial);if(steamMap!=null)Destroy(steamMap);
            DisposeHotpotSkin();
        }
    }
}
