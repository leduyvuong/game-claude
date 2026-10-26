using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmDevelopment
    {
        private readonly Dictionary<string,Texture2D> buildingImages=new Dictionary<string,Texture2D>();
        private GUIStyle cardTitle,caption,lightTitle,tabStyle;
        private int buildingPage;
        private static void Paint(Rect rect,string color)
        {var old=GUI.color;GUI.color=FarmWorld.Hex(color);GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        private static void Ink(GUIStyle style,string color)
        {
            var ink=FarmWorld.Hex(color);
            style.normal.textColor=style.hover.textColor=style.active.textColor=style.focused.textColor=ink;
            style.onNormal.textColor=style.onHover.textColor=style.onActive.textColor=style.onFocused.textColor=ink;
        }
        private void PrepareStyles()
        {
            if(cardTitle!=null)return;
            heading=new GUIStyle(GUI.skin.label){fontSize=27,fontStyle=FontStyle.Bold};Ink(heading,"5A4327");
            body=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};Ink(body,"5A4327");
            small=new GUIStyle(body){fontSize=16};Ink(small,"76674D");
            cardTitle=new GUIStyle(heading){fontSize=20};
            caption=new GUIStyle(small){fontSize=15};
            lightTitle=new GUIStyle(heading){fontSize=23,alignment=TextAnchor.MiddleLeft};Ink(lightTitle,"F4F6ED");
            button=new GUIStyle(GUI.skin.button){fontSize=17,wordWrap=true,border=new RectOffset(0,0,0,0)};
            button.normal.background=button.hover.background=button.active.background=button.focused.background=Texture2D.whiteTexture;
            Ink(button,"5A4327");tabStyle=new GUIStyle(button);
        }
        private bool SoftButton(Rect rect,string text,bool active=false)
        {
            Color old=GUI.backgroundColor;GUI.backgroundColor=FarmWorld.Hex(active?"679447":"F3E6C8");
            Ink(tabStyle,active?"FFFFFF":"5A4327");bool clicked=GUI.Button(rect,text,tabStyle);GUI.backgroundColor=old;return clicked;
        }
        private void DrawHeader()
        {
            var m=game.Model;var d=m.State.development;
            GUI.Label(new Rect(28,18,450,38),"Phát triển đảo",heading);
            GUI.Label(new Rect(28,60,525,30),"Cấp "+m.Level+"  ·  "+d.experience+" XP  ·  "+d.ordersCompleted+" đơn đã giao",small);
            Paint(new Rect(557,20,325,47),"725537");
            GUI.Label(new Rect(573,21,299,45),FarmModel.Money(m.State.coins),lightTitle);
            GUI.Label(new Rect(557,72,325,25),d.wood.ToString("N0")+" gỗ   ·   "+d.ore.ToString("N0")+" quặng",caption);
            if(SoftButton(new Rect(920,24,172,48),"Đóng  ×"))Close();
            string[] tabs={"Đơn hàng","Xây dựng","Chế biến","Nhật ký","Kho đầy đủ","30 giống","Canh tác","Nhân công","Hành trình"};
            int group=tab<5?0:5;
            for(int n=0;n<5&&group+n<tabs.Length;n++)
            {int i=group+n;if(SoftButton(new Rect(28+n*176,110,166,48),tabs[i],i==tab)){tab=i;ResetPages();buildingFilter="";}}
            if(SoftButton(new Rect(914,110,178,48),group==0?"Chăm đảo →":"← Sản xuất")){tab=group==0?5:0;ResetPages();buildingFilter="";}
        }
        private void DrawFooter()
        {
            Paint(new Rect(28,651,1064,64),"EDE0BF");
            GUI.Label(new Rect(44,661,1030,48),game.HasNotification?game.Notification:TabHint(),small);
        }
        private void DrawBuildingPage()
        {
            var m=game.Model;var d=m.State.development;bool allLand=m.ParcelsUnlocked==2;
            float top=allLand?75:178;
            if(allLand)
            {
                Paint(new Rect(16,14,1032,48),"E6EEDC");
                GUI.Label(new Rect(30,25,570,30),"Đất canh tác  ·  36 / 36 ô",cardTitle);
                GUI.Label(new Rect(720,27,310,27),"Đã khai hoang cả hai khu đất",small);
            }
            else for(int parcel=0;parcel<2;parcel++)
            {
                int index=parcel;float x=16+parcel*524;bool owned=parcel<m.ParcelsUnlocked;
                Paint(new Rect(x,14,508,159),"F1E6C9");
                GUI.Label(new Rect(x+14,23,480,30),"Khu đất "+(parcel+1)+"  ·  +6 ô"+(owned?"  ·  Đã mở":"  ·  Cấp "+FarmModel.ParcelLevel(parcel)),cardTitle);
                GUI.Label(new Rect(x+14,57,480,42),owned?"Sẵn sàng cho những vụ mùa mới.":m.ParcelMissionProgress(parcel),small);
                string status=owned?"Đã khai hoang":m.ParcelUnlockBlocker(parcel);
                GUI.Label(new Rect(x+14,101,281,59),status.Length==0?"Đủ điều kiện mở đất":status,caption);
                ActionButton(new Rect(x+308,119,183,40),owned?"Đã mở":FarmModel.Money(FarmModel.ParcelCost(parcel)),game.Online==null&&m.CanUnlockParcel(index),
                    ()=>Local(()=>{var result=m.UnlockParcel();if(result.success)game.World.SyncPlots(m);return result;}));
            }
            GUI.Label(new Rect(16,top,750,30),"Công trình của bạn",cardTitle);
            GUI.Label(new Rect(735,top+5,310,25),"Ảnh minh họa khi đạt cấp 3",caption);
            int perPage=allLand?6:3,pages=(DevelopmentCatalog.Buildings.Length+perPage-1)/perPage;
            buildingPage=Mathf.Clamp(buildingPage,0,pages-1);float height=allLand?149:207;
            for(int n=0;n<perPage&&buildingPage*perPage+n<DevelopmentCatalog.Buildings.Length;n++)
            {
                var spec=DevelopmentCatalog.Buildings[buildingPage*perPage+n];int level=m.BuildingLevel(spec.id);
                float x=16+n%3*349,y=top+36+n/3*(height+12);
                Paint(new Rect(x,y,334,height),"D9E2D1");Paint(new Rect(x+1,y+1,332,height-2),"FFFFFF");
                Paint(new Rect(x+10,y+11,78,78),"EEF2E6");
                if(!buildingImages.TryGetValue(spec.model,out var image)){image=Resources.Load<Texture2D>("DevelopmentThumbnails/"+spec.model);buildingImages[spec.model]=image;}
                if(image!=null)GUI.DrawTexture(new Rect(x+12,y+13,74,74),image,ScaleMode.ScaleToFit);
                GUI.Label(new Rect(x+101,y+12,223,29),spec.name,cardTitle);
                GUI.Label(new Rect(x+101,y+45,223,25),level==0?"Chưa xây  ·  Mở cấp "+spec.level:"Cấp "+level+" / 3",small);
                for(int dot=0;dot<3;dot++)Paint(new Rect(x+103+dot*34,y+78,25,5),dot<level?"709565":"DFE7D8");
                string benefits=spec.id=="nursery"?Math.Max(1,level)*2+" khay ươm":spec.id=="greenhouse"?Math.Max(1,level)*4+" ô che mưa":Math.Max(1,level)+" chỗ chế biến";
                if(level==3)
                {
                    GUI.Label(new Rect(x+13,y+99,165,38),"Tối đa · "+benefits,caption);
                    ActionButton(new Rect(x+183,y+height-45,138,34),"Mở công trình",true,()=>Open(spec.id=="nursery"||spec.id=="greenhouse"?6:2));
                }
                else
                {
                    bool affordable=m.Level>=spec.level&&m.State.coins>=spec.price*(level+1)&&d.wood>=3*(level+1)&&d.ore>=level*2;
                    string price=FarmModel.Money(spec.price*(level+1))+" · "+3*(level+1)+" gỗ · "+level*2+" quặng";
                    GUI.Label(new Rect(x+13,y+96,height>180?308:164,36),price,caption);
                    if(height>180)GUI.Label(new Rect(x+13,y+129,308,28),m.Level<spec.level?"Cần đạt cấp "+spec.level:!affordable?"Cần thêm tiền hoặc vật liệu":"Sẵn sàng "+(level==0?"xây dựng":"nâng cấp"),caption);
                    ActionButton(new Rect(x+183,y+height-45,138,34),level==0?"Xây dựng":"Nâng cấp",affordable,()=>Execute("build",()=>m.UpgradeBuilding(spec.id),spec.id));
                }
            }
            if(SoftButton(new Rect(16,421,108,34),"← Trước"))buildingPage=(buildingPage+pages-1)%pages;
            GUI.Label(new Rect(137,429,470,25),"Trang "+(buildingPage+1)+" / "+pages+"  ·  "+DevelopmentCatalog.Buildings.Length+" công trình",caption);
            if(SoftButton(new Rect(940,421,108,34),"Tiếp →"))buildingPage=(buildingPage+1)%pages;
        }
    }
}
