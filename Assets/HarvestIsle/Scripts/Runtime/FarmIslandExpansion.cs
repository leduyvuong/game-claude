using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.InputSystem;
namespace HarvestIsle
{
    public sealed partial class FarmIslandExpansion : MonoBehaviour
    {
        public static readonly Vector3 Origin=new Vector3(135,.24f,0);
        private FarmGame game;private Transform district,placedRoot;
        public bool IsOpen {get;private set;}
        public bool IsPlacing {get{return placing!=0;}}
        public bool BlocksWorld {get{return IsOpen||IsPlacing;}}
        private int tab,page,placing,turn,host,selectedPlot,quality=1,quantity=1;
        private string search="",buildingFilter="",selectedCrop="sweetpotato",feedback="";
        private Vector2 scroll;
        private Vector3 candidate;
        private GameObject ghost,ghostSupport;private LineRenderer outline;
        private int mountSlot;
        private readonly Dictionary<string,Texture2D> thumbnails=new Dictionary<string,Texture2D>();
        private GUIStyle title,body,button,small;
        private GUISkin skin;private readonly List<Texture2D> styleTextures=new List<Texture2D>();
        private string visualSignature="";
        public void Configure(FarmGame owner)
        {
            game=owner;
            game.CameraController.InspectRay+=Inspect;
            if(game.Online==null&&game.Model.Island.owned.Exists(p=>p.placed))RefreshModels();
        }
        public void Open(int section=0)
        {
            if(game.Online!=null){game.Notify("Đảo trăm nghề hiện dành cho nông trại solo.");return;}
            game.Hud.ClosePanels();game.Development.Close();game.Batch.SetMode(0);
            IsOpen=true;tab=section;page=0;scroll=Vector2.zero;buildingFilter="";feedback="";
            if(section==6)CycleService(1);
            selectedPlot=Mathf.Max(0,game.SelectedPlot);EnsureDistrict();
        }
        public void Close(){IsOpen=false;CancelPlacement();}
        public void ShowDistrict(){EnsureDistrict();game.CameraController.ConfigureMap(new Vector2(-60,-51),new Vector2(180,51),76);game.CameraController.SetView(Origin,36);}
        public void FocusBuilding(string id)
        {var p=game.Model.IslandBuilding(id);if(p!=null){ShowDistrict();game.CameraController.SetView(Origin+new Vector3(p.x,0,p.z),8);}}
        public void ShowSection(int section){tab=section;page=0;scroll=Vector2.zero;if(section==6&&buildingFilter=="")CycleService(1);}
        public void OpenService(string id){Open(6);if(game.Model.HasIslandBuilding(id))buildingFilter=id;}
        private void Result(ActionResult r,bool rebuild=false)
        {feedback=r.message;game.Notify(r.message,!r.success);if(r.success){game.Save();game.Refresh();if(rebuild)RefreshModels();}}
        private bool Inspect(Ray ray)
        {
            if(game.Online!=null||BlocksWorld)return false;
            if(Physics.Raycast(ray,out var hit,400,1<<FarmWorld.InteractionLayer))
            {
                var t=hit.collider.GetComponent<IslandExpansionTarget>();if(t!=null&&t.owner==this){Open(t.id.StartsWith("B")?(int.Parse(t.id.Substring(1))>=16?6:2):4);buildingFilter=t.id;return true;}
            }
            return false;
        }
        private void EnsureDistrict()
        {
            if(district!=null)return;
            district=new GameObject("Đảo trăm nghề").transform;district.SetParent(game.World.transform,false);district.position=Origin;
            var w=game.World;
            w.Shape(PrimitiveType.Cylinder,"Bờ cát đảo nghề",district,new Vector3(0,-.8f,0),new Vector3(94,.8f,72),FarmWorld.Hex("DCC995"));
            w.Shape(PrimitiveType.Cube,"Đất vườn nghề",district,new Vector3(0,-.22f,0),new Vector3(84,.45f,61),FarmWorld.Hex("8FA660"));
            foreach(float x in new[]{-28f,0,28})foreach(float z in new[]{-15f,15f})
                w.Shape(PrimitiveType.Cube,"Sân cụm nghề",district,new Vector3(x,.015f,z),new Vector3(24,.035f,26),FarmWorld.Hex(x<0?"D0BA8B":x==0?"D8C7A3":"C9BC92"));
            w.Shape(PrimitiveType.Cube,"Đường trung tâm",district,new Vector3(0,.055f,0),new Vector3(84,.05f,4),FarmWorld.Hex("E5D6B0"));
            foreach(float x in new[]{-14f,14f})w.Shape(PrimitiveType.Cube,"Đường nối khu",district,new Vector3(x,.055f,0),new Vector3(3,.05f,59),FarmWorld.Hex("E5D6B0"));
            w.Shape(PrimitiveType.Cube,"Hồ cảnh",district,new Vector3(-35,.07f,23),new Vector3(10,.05f,10),FarmWorld.Hex("5EACA3"));
            string[] names={"BẾP VƯỜN","XÓM THỢ","SÂN LÀNG","BỜ NƯỚC","VƯỜN HƯƠNG","VƯỜN HỌC NGHỀ"};int n=0;
            foreach(float z in new[]{-29f,29f})foreach(float x in new[]{-28f,0,28})
            {
                var sign=new GameObject(names[n++]);sign.transform.SetParent(district,false);sign.transform.localPosition=new Vector3(x,.12f,z);
                sign.transform.localRotation=Quaternion.Euler(70,0,0);var text=sign.AddComponent<TextMesh>();text.text=sign.name;text.fontSize=48;text.characterSize=.13f;text.anchor=TextAnchor.MiddleCenter;text.color=FarmWorld.Hex("3D5136");
                var palm=Resources.Load<GameObject>("Nature/CoconutPalm");if(palm!=null){var tree=Instantiate(palm,district,false);tree.transform.localPosition=new Vector3(x+11,0,z);tree.transform.localScale=Vector3.one*1.5f;}
            }
            placedRoot=new GameObject("Công trình và đồ sở hữu").transform;placedRoot.SetParent(district,false);
        }
        public void RefreshModels()
        {
            if(game.Online!=null)return;EnsureDistrict();string signature=JsonUtility.ToJson(game.Model.Island);
            if(signature==visualSignature)return;visualSignature=signature;
            foreach(Transform child in placedRoot){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            foreach(var p in game.Model.Island.owned)
            {
                if(!p.placed)continue;var s=IslandExpansionCatalog.Thing(p.id);var prefab=Resources.Load<GameObject>(s.Resource);if(prefab==null)continue;
                var model=Instantiate(prefab,placedRoot,false);model.name=s.name+" #"+p.serial;
                model.transform.localPosition=Position(p);model.transform.localRotation=Quaternion.Euler(0,p.turn*90,0);
                if(p.host>0)CreateSupport(p,placedRoot);
                foreach(var c in model.GetComponentsInChildren<Collider>())c.enabled=false;
                model.layer=FarmWorld.InteractionLayer;var box=model.AddComponent<BoxCollider>();box.center=Vector3.up*Mathf.Max(.4f,s.height/2);box.size=new Vector3(s.width,Mathf.Max(.8f,s.height),s.depth);
                var target=model.AddComponent<IslandExpansionTarget>();target.owner=this;target.id=p.id;target.serial=p.serial;
            }
        }
        private Vector3 Position(IslandPlacement p)
        {
            var s=IslandExpansionCatalog.Thing(p.id);float y=s.kind=="water"?.11f:.08f;
            if(p.host>0)
            {
                var prefab=Resources.Load<GameObject>(s.Resource);var mesh=prefab==null?null:prefab.GetComponent<MeshFilter>();var bounds=mesh==null?new Bounds(Vector3.up*s.height/2,new Vector3(s.width,s.height,s.depth)):mesh.sharedMesh.bounds;
                y=s.kind=="wall"?1.85f-bounds.max.y:.94f-bounds.min.y;
            }
            return new Vector3(p.x,y,p.z);
        }
        private GameObject CreateSupport(IslandPlacement p,Transform parent)
        {
            var root=new GameObject("Giá đỡ sân trước");root.transform.SetParent(parent,false);root.transform.localPosition=new Vector3(p.x,.08f,p.z);root.transform.localRotation=Quaternion.Euler(0,p.turn*90,0);
            var s=IslandExpansionCatalog.Thing(p.id);Color wood=FarmWorld.Hex("977147");
            if(s.kind=="wall")
            {
                foreach(float x in new[]{-.62f,.62f})game.World.Shape(PrimitiveType.Cube,"Cột giá treo",root.transform,new Vector3(x,.9f,-.12f),new Vector3(.08f,1.8f,.1f),wood);
                game.World.Shape(PrimitiveType.Cube,"Thanh treo",root.transform,new Vector3(0,1.8f,-.05f),new Vector3(1.35f,.1f,.16f),wood);
            }
            else
            {
                foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.34f,.34f})game.World.Shape(PrimitiveType.Cube,"Chân kệ",root.transform,new Vector3(x,.4f,z),new Vector3(.075f,.8f,.075f),wood);
                game.World.Shape(PrimitiveType.Cube,"Mặt kệ",root.transform,new Vector3(0,.82f,0),new Vector3(1.2f,.08f,.85f),wood);
            }
            return root;
        }
        public void BeginPlacement(int serial)
        {
            var own=game.Model.Island.owned.Find(p=>p.serial==serial);if(own==null)return;
            var spec=IslandExpansionCatalog.Thing(own.id);var prefab=Resources.Load<GameObject>(spec.Resource);
            if(prefab==null){feedback="Chưa tải được model của mẫu này.";return;}
            CancelPlacement();placing=serial;turn=own.turn;host=own.host;candidate=new Vector3(own.placed?own.x:-28,0,own.placed?own.z:-16);
            if((spec.kind=="wall"||spec.kind=="table")&&!own.placed)NextMount();
            IsOpen=false;ShowDistrict();ghost=Instantiate(prefab,district,false);ghost.name="Bóng đặt đồ";
            foreach(var c in ghost.GetComponentsInChildren<Collider>())c.enabled=false;
            outline=game.World.Ring(district,Vector3.zero,1,FarmWorld.Hex("F3CB68"),.09f,4);outline.name="Khung xem trước";
            UpdateGhost();
        }
        private void NextMount()
        {
            var hosts=game.Model.Island.owned.FindAll(v=>v.placed&&v.id.StartsWith("B"));if(hosts.Count==0)return;
            int idx=hosts.FindIndex(v=>v.serial==host);if(idx<0){idx=0;mountSlot=0;}else if(mountSlot==0)mountSlot=1;else{idx=(idx+1)%hosts.Count;mountSlot=0;}
            var next=hosts[idx];host=next.serial;turn=next.turn;float x,z;FarmModel.IslandMountPosition(next,mountSlot,out x,out z);candidate=new Vector3(x,0,z);
            if(ghostSupport!=null)Destroy(ghostSupport);ghostSupport=null;
        }
        private Rect PlacementBar(){var a=Screen.safeArea;return new Rect(a.x+8,Screen.height-a.yMin-124,a.width-16,116);}
        private void Update()
        {
            if(!IsPlacing)return;
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){CancelPlacement();return;}
            Vector2 pointer;bool active=false;
            if(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count>1)return;
            if(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.isPressed){pointer=Touchscreen.current.primaryTouch.position.ReadValue();active=true;}
            else {pointer=Mouse.current==null?Vector2.zero:Mouse.current.position.ReadValue();active=Mouse.current!=null&&Mouse.current.leftButton.isPressed;}
            if(active&&!PlacementBar().Contains(new Vector2(pointer.x,Screen.height-pointer.y)))
            {
                var ray=game.World.GameCamera.ScreenPointToRay(pointer);float distance;
                if(new Plane(Vector3.up,Origin).Raycast(ray,out distance)){var p=ray.GetPoint(distance)-Origin;candidate=new Vector3(Mathf.Round(p.x*2)/2,0,Mathf.Round(p.z*2)/2);}
            }
            UpdateGhost();
        }
        private void UpdateGhost()
        {
            if(ghost==null)return;var p=game.Model.Island.owned.Find(x=>x.serial==placing);var spec=IslandExpansionCatalog.Thing(p.id);
            ghost.transform.localPosition=Position(new IslandPlacement{id=p.id,x=candidate.x,z=candidate.z,host=host});ghost.transform.localRotation=Quaternion.Euler(0,turn*90,0);
            if(host>0){if(ghostSupport==null)ghostSupport=CreateSupport(new IslandPlacement{id=p.id,x=candidate.x,z=candidate.z,host=host,turn=turn},district);ghostSupport.transform.localPosition=new Vector3(candidate.x,.08f,candidate.z);ghostSupport.transform.localRotation=Quaternion.Euler(0,turn*90,0);}
            string reason=game.Model.IslandPlacementBlocker(placing,candidate.x,candidate.z,turn,host);
            float w,d;FarmModel.IslandSize(spec,turn,out w,out d);Vector3 center=candidate+Vector3.up*.14f;
            for(int n=0;n<4;n++)outline.SetPosition(n,center+new Vector3(n==0||n==3?-w/2:w/2,0,n<2?-d/2:d/2));
            outline.sharedMaterial=game.World.Material(FarmWorld.Hex(reason==""?"68AC6E":"DC7154"));
        }
        public void CancelPlacement()
        {placing=0;if(ghost!=null)Destroy(ghost);if(ghostSupport!=null)Destroy(ghostSupport);if(outline!=null)Destroy(outline.gameObject);ghost=null;ghostSupport=null;outline=null;}
        private void OnApplicationFocus(bool focus){if(!focus)CancelPlacement();}
        private void OnDisable(){IsOpen=false;CancelPlacement();}
        private void OnGUI()
        {
            if(game==null||game.Model==null||!BlocksWorld)return;
            if(title==null)CreateSkin();
            var oldSkin=GUI.skin;GUI.skin=skin;
            if(IsPlacing){DrawPlacement();GUI.skin=oldSkin;return;}
            Rect frame=FarmWindow.Frame(1120,700);if(FarmWindow.DismissRequested(frame)){Close();GUI.skin=oldSkin;return;}
            FarmWindow.Draw(frame);var previous=GUI.matrix;GUI.matrix=FarmWindow.Matrix(frame,1120);
            GUI.contentColor=Color.white;GUI.Label(new Rect(25,15,730,44),"ĐẢO TRĂM NGHỀ  ·  "+FarmModel.Money(game.Model.State.coins),title);
            if(GUI.Button(new Rect(894,17,150,38),"Xem khu nghề",button)){IsOpen=false;ShowDistrict();}
            if(GUI.Button(new Rect(1054,17,42,38),"×",button))Close();
            string[] tabs={"Nhật ký nghề","Công trình","Làm nghề","Trang trí","Kho sở hữu","Vật tư & hàng","Dịch vụ"};
            for(int i=0;i<tabs.Length;i++){GUI.backgroundColor=tab==i?FarmWorld.Hex("E9C675"):Color.white;if(GUI.Button(new Rect(20+i*155,67,150,40),tabs[i],button)){tab=i;page=0;scroll=Vector2.zero;buildingFilter="";}}
            GUI.backgroundColor=Color.white;
            GUI.Label(new Rect(24,115,110,28),"Tìm tên:",body);string q=GUI.TextField(new Rect(120,115,380,30),search,80);if(q!=search){search=q;page=0;}
            GUI.Label(new Rect(520,115,575,30),"M"+game.Model.Island.chapter+" · "+IslandExpansionCatalog.Chapters[game.Model.Island.chapter]+" · "+game.Model.Island.completed+" lượt hoàn thành",body);
            GUILayout.BeginArea(new Rect(24,158,1072,451));scroll=GUILayout.BeginScrollView(scroll);
            if(tab==0)DrawJournal();else if(tab==1||tab==3)DrawShop(tab==1);else if(tab==2)DrawRecipes();else if(tab==4)DrawOwned();else if(tab==5)DrawStock();else DrawServices();
            GUILayout.EndScrollView();GUILayout.EndArea();
            GUI.Label(new Rect(24,620,1064,64),feedback,body);GUI.matrix=previous;GUI.contentColor=Color.white;GUI.skin=oldSkin;
        }
        private void DrawPlacement()
        {
            Rect bar=PlacementBar();GUI.Box(bar,"");GUILayout.BeginArea(new Rect(bar.x+10,bar.y+5,bar.width-20,bar.height-10));
            var p=game.Model.Island.owned.Find(x=>x.serial==placing);var s=IslandExpansionCatalog.Thing(p.id);string reason=game.Model.IslandPlacementBlocker(placing,candidate.x,candidate.z,turn,host);
            GUILayout.Label(s.name+" · kéo/chạm nền để đặt · "+(reason==""?"✓ Hợp lệ":reason),body);
            GUILayout.BeginHorizontal();if(s.kind!="wall"&&s.kind!="table"&&GUILayout.Button("Xoay 90°",button,GUILayout.Height(42)))turn=(turn+1)%4;
            if(s.kind=="wall"||s.kind=="table")if(GUILayout.Button("Chọn giá đỡ",button,GUILayout.Height(42)))
            NextMount();
            GUI.enabled=reason=="";if(GUILayout.Button("Xác nhận",button,GUILayout.Height(42))){Result(game.Model.PlaceIslandThing(placing,candidate.x,candidate.z,turn,host),true);CancelPlacement();IsOpen=true;tab=4;}GUI.enabled=true;
            if(GUILayout.Button("Hủy",button,GUILayout.Height(42))){CancelPlacement();IsOpen=true;tab=4;}
            GUILayout.EndHorizontal();GUILayout.EndArea();
        }
        private bool Match(string name){return string.IsNullOrWhiteSpace(search)||name.IndexOf(search,StringComparison.CurrentCultureIgnoreCase)>=0;}
        private Texture2D Solid(string hex){var t=new Texture2D(1,1);t.SetPixel(0,0,FarmWorld.Hex(hex));t.Apply();styleTextures.Add(t);return t;}
        private void CreateSkin()
        {
            skin=Instantiate(GUI.skin);Color ink=FarmWorld.Hex("3F3829");
            body=new GUIStyle(skin.label){fontSize=17,wordWrap=true};
            foreach(var state in new[]{body.normal,body.hover,body.active,body.focused,body.onNormal,body.onHover,body.onActive,body.onFocused})state.textColor=ink;
            title=new GUIStyle(body){fontSize=25,fontStyle=FontStyle.Bold};small=new GUIStyle(body){fontSize=14};
            button=new GUIStyle(skin.button){fontSize=16,wordWrap=true,padding=new RectOffset(8,8,7,7),border=new RectOffset(0,0,0,0)};
            button.normal.background=Solid("E8D7B0");button.hover.background=Solid("E8C777");button.active.background=Solid("CEAA5E");button.focused.background=button.hover.background;
            button.normal.textColor=ink;button.hover.textColor=ink;button.active.textColor=ink;button.focused.textColor=ink;
            foreach(var state in new[]{button.onNormal,button.onHover,button.onActive,button.onFocused}){state.background=button.hover.background;state.textColor=ink;}
            skin.button=button;skin.label=body;skin.box=new GUIStyle(skin.box){padding=new RectOffset(8,8,8,8),margin=new RectOffset(2,2,4,4)};skin.box.normal.background=Solid("FFFAEC");
            skin.textField=new GUIStyle(skin.textField){fontSize=17,padding=new RectOffset(8,8,5,5)};skin.textField.normal.background=Solid("FFFAEC");skin.textField.focused.background=skin.textField.normal.background;skin.textField.normal.textColor=ink;skin.textField.focused.textColor=ink;
        }
        private void OnDestroy(){foreach(var t in styleTextures)if(t!=null)Destroy(t);if(skin!=null)Destroy(skin);}
        private void Icon(string id)
        {if(!thumbnails.TryGetValue(id,out var t)){t=Resources.Load<Texture2D>("IslandExpansion/Thumbnails/"+id);thumbnails[id]=t;}GUILayout.Label(t,GUILayout.Width(72),GUILayout.Height(65));}
        private bool Button(string text,int width=155){return GUILayout.Button(text,button,GUILayout.Width(width),GUILayout.MinHeight(38));}
        private void Pager(int count)
        {int pages=Mathf.Max(1,(count+5)/6);page=Mathf.Clamp(page,0,pages-1);GUILayout.BeginHorizontal();if(Button("←",60)){page=Mathf.Max(0,page-1);scroll=Vector2.zero;}GUILayout.Label((page+1)+" / "+pages+" · "+count+" mục",body);if(Button("→",60)){page=Mathf.Min(pages-1,page+1);scroll=Vector2.zero;}GUILayout.EndHorizontal();}
    }
    public sealed class IslandExpansionTarget : MonoBehaviour {public FarmIslandExpansion owner;public string id;public int serial;}
}
