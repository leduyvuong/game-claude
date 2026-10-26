using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed class FarmRanch:MonoBehaviour
    {
        private FarmGame game;
        private readonly List<GameObject>[] residents={new List<GameObject>(),new List<GameObject>(),new List<GameObject>(),new List<GameObject>(),new List<GameObject>(),new List<GameObject>()};
        private readonly Vector3[] centers={new Vector3(-10.5f,.15f,-1.5f),new Vector3(9.5f,.15f,-.7f),new Vector3(-10.5f,.15f,1.6f),new Vector3(9.5f,.15f,-3.4f),new Vector3(-10.5f,.15f,-4.4f),new Vector3(9.5f,.15f,2.1f)};
        private static readonly string[] Coops={"ChickenCoop","CowShed","ChickenCoop","CowShed","",""};
        private static readonly string[] EstateCoops={"ChickenCoop","CowShed","DuckShelter","PigPen"};
        private static readonly string[] AnimalModels={"Chicken","Cow","Duck","Pig","",""};
        private static readonly float[] AnimalScales={.95f,.75f,.85f,.8f,1,1};
        public static bool InClearing(Vector3 p)
        {return Mathf.Abs(p.x+10.5f)<3.5f&&Mathf.Abs(p.z+1.5f)<3.5f||Mathf.Abs(p.x-9.5f)<3.5f&&Mathf.Abs(p.z+.7f)<3.5f;}
        private Transform[] pens=new Transform[6];
        private GameObject[] feedBowls=new GameObject[6];
        public bool IsOpen {get;private set;}
        private int selectedPen;
        public void OpenPen(int index){selectedPen=Mathf.Clamp(index,0,5);game.CameraController.FocusOn(centers[selectedPen]);Open();}
        public int VisibleAnimals => residents[0].Count+residents[1].Count+residents[2].Count+residents[3].Count+residents[4].Count+residents[5].Count;
        public void Configure(FarmGame owner)
        {
            game=owner;
            if(game.IsEstate)for(int i=0;i<centers.Length;i++)centers[i]=FarmEstate.PenPosition(i);
            for(int i=0;i<6;i++)
            {
                var root=new GameObject(LivestockCatalog.All[i].name+" paddock");root.transform.SetParent(game.World.transform,false);root.transform.localPosition=centers[i];pens[i]=root.transform;
                root.layer=FarmWorld.InteractionLayer;root.AddComponent<FarmRanchTarget>().Index=i;
                var hit=root.AddComponent<BoxCollider>();hit.center=new Vector3(0,.5f,.2f);hit.size=new Vector3(4,1.5f,4.3f);
                if(i==4&&game.IsEstate)
                {
                    for(int hive=0;hive<2;hive++)
                    {
                        var prefab=Resources.Load<GameObject>("Production/BeeHive");
                        var house=Instantiate(prefab,pens[i],false);house.transform.localPosition=new Vector3(hive==0?-.7f:.7f,0,1.1f);house.transform.localScale=Vector3.one*.85f;house.transform.localRotation=Quaternion.Euler(0,180,0);
                        foreach(var c in house.GetComponentsInChildren<Collider>())c.enabled=false;
                    }
                }
                else if(i==4)
                {
                    // Beehive: stacked boxes with a little roof.
                    game.World.Shape(PrimitiveType.Cube,"Hive base",pens[i],new Vector3(0,.3f,1.2f),new Vector3(.75f,.55f,.75f),FarmWorld.Hex("E8B84B"));
                    game.World.Shape(PrimitiveType.Cube,"Hive top",pens[i],new Vector3(0,.72f,1.2f),new Vector3(.6f,.3f,.6f),FarmWorld.Hex("F3CC6B"));
                    game.World.Shape(PrimitiveType.Cube,"Hive roof",pens[i],new Vector3(0,.95f,1.2f),new Vector3(.85f,.12f,.85f),FarmWorld.Hex("A6683F"));
                }
                else if(i==5)
                {
                    var pond=Resources.Load<GameObject>("Environment/Pond");
                    if(pond!=null){var water=Instantiate(pond,pens[i],false);water.transform.localPosition=new Vector3(0,0,.2f);water.transform.localScale=Vector3.one*.55f;foreach(var c in water.GetComponentsInChildren<Collider>())c.enabled=false;}
                }
                else
                {
                    var prefab=Resources.Load<GameObject>("Structures/Levels/"+(game.IsEstate?EstateCoops[i]+"_L2":Coops[i]+"_L1"));
                    if(prefab!=null){var house=Instantiate(prefab,pens[i],false);house.transform.localPosition=new Vector3(0,0,1.4f);house.transform.localScale=Vector3.one*(game.IsEstate?.9f:.65f);if(game.IsEstate)house.transform.localRotation=Quaternion.Euler(0,180,0);foreach(var c in house.GetComponentsInChildren<Collider>())c.enabled=false;}
                }
                for(int side=0;side<2;side++)for(int p=0;p<5;p++)
                {
                    Vector3 pos=new Vector3(side==0?-1.9f:1.9f,.32f,-1.8f+p*.85f);
                    game.World.Shape(PrimitiveType.Cube,"Pen post",pens[i],pos,new Vector3(.1f,.65f,.1f),FarmWorld.Hex("B88C57"));
                    if(p<4)game.World.Shape(PrimitiveType.Cube,"Pen rail",pens[i],pos+new Vector3(0,.05f,.42f),new Vector3(.08f,.1f,.85f),FarmWorld.Hex("DBB87A"));
                }
                game.World.Shape(PrimitiveType.Cube,"Feed trough",pens[i],new Vector3(0,.1f,-1.65f),new Vector3(1.1f,.2f,.35f),FarmWorld.Hex("9E7854"));
                feedBowls[i]=game.World.Shape(PrimitiveType.Sphere,"Feed",pens[i],new Vector3(0,.21f,-1.65f),new Vector3(.9f,.08f,.26f),FarmWorld.Hex("D9C16E"));
            }
            SyncModels();
        }
        public Vector3 PositionOfPen(int index){return pens[Mathf.Clamp(index,0,pens.Length-1)].position;}
        public void Open(){if(game.IsHarvesting||(game.Hotpot!=null&&game.Hotpot.IsOpen))return;IsOpen=true;game.Hud.gameObject.SetActive(false);}
        public void Close(){IsOpen=false;game.Hud.gameObject.SetActive(true);}
        public void Act(string id,string action)
        {
            if(game.IsHarvesting)return;
            if(game.Online!=null)
            {
                if(id=="duck"||id=="pig"||id=="bee"||id=="fish"){game.Notify("Chuồng mới chưa hỗ trợ chế độ online.",true);return;}
                string kind=action=="buy"?"buyAnimal":action=="feed"||action=="corn"?"feedAnimal":action=="collect"?"collectAnimal":null;
                if(kind!=null)game.Online.Command(kind,id,useCorn:action=="corn");
                return;
            }
            ActionResult result=action=="buy"?game.Model.BuyAnimal(id):action=="feed"?game.Model.FeedAnimal(id,false,game.Now):action=="corn"?game.Model.FeedAnimal(id,true,game.Now):action=="collect"?game.Model.CollectAnimal(id,game.Now):new ActionResult(false,"Thao tác không hợp lệ.");
            game.Notify(result.message,!result.success);if(result.success)game.Save();SyncModels();
            if(result.success&&(action=="feed"||action=="corn"))foreach(var item in residents[PenIndex(id)]){var avatar=item.GetComponent<FarmAnimalAvatar>();if(avatar!=null)avatar.Feed();}
        }
        private static int PenIndex(string id){return id=="chicken"?0:id=="cow"?1:id=="duck"?2:id=="pig"?3:id=="bee"?4:5;}
        private void SyncModels()
        {
            for(int i=0;i<6;i++)
            {
                string id=LivestockCatalog.All[i].id;
                var animal=game.Model.Animal(id);
                while(residents[i].Count<animal.count)
                {
                    int n=residents[i].Count;GameObject model;
                    if(i==4)
                    {
                        model=game.World.Imported("Collection/Bee",pens[i],new Vector3((n-1)*.5f,.6f,-.3f),.16f,.3f);
                        if(model==null)break;
                        model.AddComponent<FarmFlutter>().Initialize(new Vector3((n-1)*.5f,.65f,-.35f),.45f,1.8f+n*.4f);
                    }
                    else if(i==5)
                    {
                        model=Instantiate(Resources.Load<GameObject>("Animals/Koi"),pens[i],false);
                        model.transform.localScale=Vector3.one*.3f;
                        model.AddComponent<FarmFishSwim>().Configure(new Vector3(0,.07f,.2f),.5f,.38f,.3f,n+3);
                    }
                    else
                    {
                        model=Instantiate(Resources.Load<GameObject>("Animals/"+AnimalModels[i]),pens[i],false);
                        Vector3 center=new Vector3((n-1)*.9f,0,-.65f);model.transform.localPosition=center;model.transform.localScale=Vector3.one*AnimalScales[i];
                        model.AddComponent<FarmAnimalWander>().Configure(center,.23f,.18f,i*31+n+5);
                    }
                    foreach(var c in model.GetComponentsInChildren<Collider>())c.enabled=false;
                    residents[i].Add(model);
                }
                feedBowls[i].SetActive(animal.producing&&game.Model.AnimalRemaining(id,game.Now)>0);
            }
        }
        private void Update(){if(game!=null&&game.Model!=null)SyncModels();}
        private void OnGUI()
        {
            if(!IsOpen)return;
            Rect frame=FarmWindow.Frame(900,600);
            if(FarmWindow.DismissRequested(frame)){Close();return;}
            var old=GUI.matrix;var oldColor=GUI.color;var oldBackground=GUI.backgroundColor;var oldContent=GUI.contentColor;bool oldEnabled=GUI.enabled;
            GUI.color=GUI.contentColor=GUI.backgroundColor=Color.white;GUI.enabled=true;
            FarmWindow.Draw(frame);GUI.matrix=FarmWindow.Matrix(frame,900);
            var text=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};text.normal.textColor=FarmWorld.Hex("5A4327");
            text.hover.textColor=text.active.textColor=text.focused.textColor=text.normal.textColor;
            var title=new GUIStyle(text){fontSize=25,fontStyle=FontStyle.Bold};
            var button=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true};button.normal.textColor=button.hover.textColor=button.active.textColor=FarmWorld.Hex("5A4327");
            button.normal.background=button.hover.background=button.active.background=button.focused.background=Texture2D.whiteTexture;button.border=new RectOffset(0,0,0,0);
            GUI.backgroundColor=FarmWorld.Hex("FFF6DF");
            GUI.Label(new Rect(24,18,420,38),"CHĂN NUÔI",title);
            GUI.Label(new Rect(540,23,265,35),FarmModel.Money(game.Model.State.coins),text);
            if(GUI.Button(new Rect(827,14,50,46),"×",button))Close();
            string[] images={"Chicken","Cow","Duck","Pig","Basket","Product_FishBasket"};
            for(int i=0;i<6;i++)
            {
                Rect tab=new Rect(20+i*144,72,136,76);GUI.backgroundColor=FarmWorld.Hex(i==selectedPen?"EDD59F":"FFF6DF");
                if(GUI.Button(tab,GUIContent.none,button))OpenPen(i);
                var image=Resources.Load<Texture2D>("ModelThumbnails/"+images[i]);if(image!=null)GUI.DrawTexture(new Rect(tab.x+48,tab.y+3,40,40),image,ScaleMode.ScaleToFit);
                var tabText=new GUIStyle(text){alignment=TextAnchor.MiddleCenter,fontSize=17};GUI.Label(new Rect(tab.x+3,tab.y+44,130,27),LivestockCatalog.All[i].name,tabText);
            }
            GUI.backgroundColor=FarmWorld.Hex("FFF6DF");
            GUI.color=FarmWorld.Hex("FFF8E8");GUI.DrawTexture(new Rect(20,164,860,346),Texture2D.whiteTexture);GUI.color=Color.white;
            var spec=LivestockCatalog.All[selectedPen];var state=game.Model.Animal(spec.id);double left=game.Model.AnimalRemaining(spec.id,game.Now);bool bee=spec.id=="bee";
            var portrait=Resources.Load<Texture2D>("ModelThumbnails/"+images[selectedPen]);
            if(portrait!=null)GUI.DrawTexture(new Rect(42,186,254,242),portrait,ScaleMode.ScaleToFit);
            GUI.Label(new Rect(48,444,245,40),state.count+" / 3 đang nuôi",title);
            GUI.Label(new Rect(326,179,526,38),spec.name,title);
            GUI.Label(new Rect(326,223,526,65),state.count==0?"Chuồng đang trống. Đón một người bạn mới!":!state.producing?"Đang chờ bạn cho ăn":left>0?"Đang lớn · còn "+Math.Ceiling(left)+" giây":"Đã sẵn sàng! Thu "+spec.product,text);
            GUI.enabled=state.count<3&&!state.producing&&game.Model.State.coins>=spec.price;
            if(GUI.Button(new Rect(326,301,252,56),"Mua · "+FarmModel.Money(spec.price),button))Act(spec.id,"buy");
            GUI.enabled=state.count>0&&!state.producing&&(bee||game.Model.State.coins>=spec.feedCost*state.count);
            if(GUI.Button(new Rect(594,301,262,56),bee?"Thả ong lấy mật":"Cho ăn · "+FarmModel.Money(spec.feedCost*state.count),button))Act(spec.id,"feed");
            GUI.enabled=!bee&&state.count>0&&!state.producing&&game.Model.Count("corn")>0;
            if(GUI.Button(new Rect(326,373,252,56),"Cho cả đàn ăn 1 ngô",button))Act(spec.id,"corn");
            GUI.enabled=state.producing&&left<=0;
            if(GUI.Button(new Rect(594,373,262,56),"Thu "+spec.product,button))Act(spec.id,"collect");
            GUI.enabled=true;
            GUI.Label(new Rect(326,448,526,47),"Mỗi con cho 2 "+spec.product+(bee?" · cần 2 ô hoa/đậu":""),text);
            if(GUI.Button(new Rect(20,530,252,50),"Giỏ hàng · chọn bán",button)){Close();game.Hud.OpenTrading();}
            if(GUI.Button(new Rect(700,530,180,50),"Đóng  ×",button))Close();
            var note=new GUIStyle(text){fontSize=16};GUI.Label(new Rect(292,526,390,59),game.HasNotification?game.Notification:"Chọn chuồng ở trên để đổi vật nuôi",note);
            GUI.matrix=old;GUI.color=oldColor;GUI.backgroundColor=oldBackground;GUI.contentColor=oldContent;GUI.enabled=oldEnabled;
        }

    }

    public sealed class FarmRanchTarget:MonoBehaviour{public int Index;}
}
