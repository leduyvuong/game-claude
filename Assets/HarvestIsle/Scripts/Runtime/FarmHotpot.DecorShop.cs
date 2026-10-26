using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private int decorCategory=-1,decorPage,ownedFloor=-1,ownedSlot=-1;
        private bool decorOwned,decorAffordable,decorPriceSort;
        private string decorSearch="";
        private readonly Dictionary<int,Texture2D> decorIcons=new Dictionary<int,Texture2D>();
        private Texture2D DecorIcon(int kind)
        {
            Texture2D icon;
            if(!decorIcons.TryGetValue(kind,out icon))
            {
                icon=kind<8?null:Resources.Load<Texture2D>("ModelThumbnails/HP_"+HotpotFurnitureCatalog.Items[kind].id);
                if(icon==null&&kind>=8)icon=Resources.Load<Texture2D>("HotpotThumbnails/HP_"+HotpotFurnitureCatalog.Items[kind].id);
                decorIcons.Add(kind,icon);
            }
            return icon;
        }
        private static string MountLabel(int kind)
        {
            var mount=HotpotFurnitureCatalog.Items[kind].mount;
            return mount==HotpotMount.Floor?"Đặt sàn":mount==HotpotMount.Wall?"Treo tường":"Treo trần";
        }
        private void DrawFurnitureShop()
        {
            if(decorPlacing){DrawDecorPlacement();return;}
            Panel(new Rect(10,90,1180,505));
            if(Button(28,103,176,38,decorOwned?"Mua đồ mới":"● Mua đồ mới")){decorOwned=false;decorPage=0;}
            if(Button(214,103,180,38,decorOwned?"● Đồ đã có":"Đồ đã có")){decorOwned=true;decorPage=0;}
            Label(417,109,70,30,"Tìm đồ");
            string search=GUI.TextField(new Rect(489,105,290,34),decorSearch,buttonStyle);
            if(search!=decorSearch){decorSearch=search;decorPage=0;}
            if(Button(28,151,100,34,(decorCategory<0?"● ":"")+"Tất cả")){decorCategory=-1;decorPage=0;}
            for(int c=0;c<HotpotFurnitureCatalog.Categories.Length;c++)
                if(Button(137+c*131,151,124,34,(decorCategory==c?"● ":"")+HotpotFurnitureCatalog.Categories[c])){decorCategory=c;decorPage=0;}
            if(Button(28,192,178,32,(decorAffordable?"✓ ":"")+"Vừa túi tiền")){decorAffordable=!decorAffordable;decorPage=0;}
            if(Button(216,192,198,32,decorPriceSort?"Giá: thấp → cao":"Sắp xếp: bộ sưu tập")){decorPriceSort=!decorPriceSort;decorPage=0;}
            var kinds=new List<int>();var floors=new List<int>();var slots=new List<int>();
            if(decorOwned)
            {
                foreach(var item in game.Model.State.hotpot.furniture)
                    if(DecorMatches(item.kind)){kinds.Add(item.kind);floors.Add(item.floor);slots.Add(item.slot);}
            }
            else
            {
                for(int n=0;n<HotpotFurnitureCatalog.Items.Length;n++)
                {int kind=(n+8)%HotpotFurnitureCatalog.Items.Length;if(DecorMatches(kind))kinds.Add(kind);}
                if(decorPriceSort)kinds.Sort((a,b)=>HotpotFurnitureCatalog.Items[a].price.CompareTo(HotpotFurnitureCatalog.Items[b].price));
            }
            int pages=Mathf.Max(1,(kinds.Count+5)/6);decorPage=Mathf.Clamp(decorPage,0,pages-1);
            Label(439,196,347,26,kinds.Count+(decorOwned?" món đang đặt":" mẫu trang trí"));
            for(int n=0;n<6;n++)
            {
                int index=decorPage*6+n;if(index>=kinds.Count)break;
                int kind=kinds[index];var spec=HotpotFurnitureCatalog.Items[kind];float x=28+n%3*254,y=235+n/3*150;
                bool selected=kind==furnitureKind&&(!decorOwned||(ownedFloor==floors[index]&&ownedSlot==slots[index]));
                GUI.Box(new Rect(x,y,244,142),GUIContent.none,selected?activeNavStyle:navStyle);
                var icon=DecorIcon(kind);
                if(icon!=null)GUI.DrawTextureWithTexCoords(new Rect(x+7,y+9,124,124),icon,new Rect(.12f,.12f,.76f,.76f));
                else GUI.Label(new Rect(x+7,y+9,124,124),"BỘ CƠ BẢN",smallStyle);
                Label(x+140,y+15,98,77,spec.name);
                Label(x+140,y+97,98,39,decorOwned?"Tầng "+(floors[index]+1):FarmModel.Money(spec.price));
                if(GUI.Button(new Rect(x,y,244,142),GUIContent.none,GUIStyle.none))
                {furnitureKind=kind;ownedFloor=decorOwned?floors[index]:-1;ownedSlot=decorOwned?slots[index]:-1;}
            }
            if(kinds.Count==0)Label(70,300,660,85,decorOwned?"Chưa có đồ phù hợp. Đồ đã mua và đặt trong quán sẽ xuất hiện ở đây.":"Không có mẫu phù hợp. Thử đổi từ khóa hoặc tắt bộ lọc giá.");
            if(Button(28,546,72,34,"‹"))decorPage=(decorPage+pages-1)%pages;
            Label(118,550,560,28,"Trang "+(decorPage+1)+" / "+pages+" · Chọn thẻ để xem trước");
            if(Button(708,546,72,34,"›"))decorPage=(decorPage+1)%pages;
            DrawDecorDetails();
        }
        private bool DecorMatches(int kind)
        {
            var spec=HotpotFurnitureCatalog.Items[kind];
            return (decorCategory<0||decorCategory==spec.category)&&
                (decorOwned||!decorAffordable||spec.price<=game.Model.State.coins)&&
                (string.IsNullOrWhiteSpace(decorSearch)||spec.name.IndexOf(decorSearch.Trim(),StringComparison.OrdinalIgnoreCase)>=0);
        }
        private void DrawDecorDetails()
        {
            var spec=HotpotFurnitureCatalog.Items[furnitureKind];
            Panel(new Rect(803,103,372,478));EnsureDecorPreview();
            if(decorRender!=null)GUI.DrawTexture(new Rect(815,115,348,231),decorRender,ScaleMode.ScaleToFit);
            if(Button(821,349,77,32,"↶"))RotateDecorPreview(-30);
            Label(909,352,164,29,"Xem model 3D");
            if(Button(1078,349,77,32,"↷"))RotateDecorPreview(30);
            Label(821,391,337,35,spec.name,true);
            Label(821,430,337,28,MountLabel(furnitureKind)+" · "+(spec.table?"Bàn phục vụ khách":"Đồ trang trí"));
            bool owns=decorOwned&&game.Model.State.hotpot.furniture.Exists(f=>f.floor==ownedFloor&&f.slot==ownedSlot&&f.kind==furnitureKind);
            Label(821,464,337,30,decorOwned?(owns?"Đang đặt ở tầng "+(ownedFloor+1):"Chọn một món trong danh sách"):FarmModel.Money(spec.price),true);
            bool previous=GUI.enabled;GUI.enabled=decorOwned?owns:game.Model.State.coins>=spec.price;
            if(Button(821,506,337,49,decorOwned?"Di chuyển miễn phí":"Đặt thử trong quán"))BeginDecorPlacement(furnitureKind,decorOwned?ownedFloor:currentFloor,decorOwned?ownedSlot:-1);
            GUI.enabled=previous;
            if(!decorOwned&&spec.price>game.Model.State.coins)Label(821,555,337,25,"Chưa đủ tiền để mua món này.");
        }
    }
}
