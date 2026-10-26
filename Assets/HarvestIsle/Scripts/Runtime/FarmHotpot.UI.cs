using System;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private int tab, stars=1, batch=1, furnitureKind=8, furnitureRotation, moveFrom=-1;
        private bool supplier;
        private Vector2 scroll;
        private string message="Nhập nông sản → mua nước dùng/gia vị → tự phối và nấu thử → đưa món lên thực đơn.";
        private HotpotRecipe draft=new HotpotRecipe();
        private Rect guiArea;
        private float guiScale;
        private GUIStyle textStyle,headingStyle,buttonStyle;
        private void OnGUI()
        {
            if(!IsOpen)return;
            Rect frame=FarmWindow.Frame(1200,720);
            if(FarmWindow.DismissRequested(frame)){Close();return;}
            FarmWindow.Draw(frame,false);
            guiScale=frame.width/1200f;guiArea=new Rect(frame.x,frame.y,1200,720);
            Matrix4x4 previous=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3(guiArea.x,guiArea.y,0),Quaternion.identity,Vector3.one*guiScale);
            EnsureHotpotSkin();
            var h=game.Model.State.hotpot;
            Panel(new Rect(16,12,315,65));Label(31,19,295,28,"LẨU MÙA VÀNG",true);
            Label(31,49,290,23,"Hạng "+game.Model.HotpotRank+" · Danh tiếng "+h.progress.reputation+"/100");
            Panel(new Rect(725,12,270,47));Ink(new Rect(740,23,25,25),FarmWorld.Hex("DFB85B"),true);
            Label(778,21,206,32,FarmModel.Money(game.Model.State.coins),true);
            if(Button(1008,12,175,47,"Đóng  ×"))Close();
            DrawCompactNavigation();
            if(tab==0&&Time.unscaledTime<feedbackUntil){Panel(new Rect(340,12,365,65));Label(352,18,342,55,message);}
            if(tab!=0){Panel(new Rect(15,596,1170,32));Label(28,598,1140,28,message);}
            if(tab==0)DrawDining();
            else if(tab==4)DrawDecor();
            else
            {
                Panel(new Rect(10,90,1180,505));
                if(tab==1)DrawSupply();else if(tab==2)DrawRecipe();else if(tab==3)DrawMenu();else if(tab==5)DrawHotpotBusiness();else if(tab==6)DrawHotpotDiscoveries();else DrawHotpotRush();
            }
            GUI.matrix=previous;
        }
        private void Panel(Rect rect)
        {GUI.Box(rect,GUIContent.none,panelStyle);}
        private void Label(float x,float y,float w,float h,string value,bool title=false)
        {GUI.Label(new Rect(x,y,w,h),value,title?headingStyle:textStyle);}
        private bool Button(float x,float y,float w,float h,string value)
        {return GUI.Button(new Rect(x,y,w,h),value,buttonStyle);}
        private void DrawDining()
        {
            DrawFloorButtons(100);
            if(Button(16,529,58,40,"Trái")){viewAngle-=90;PositionCamera();}
            if(Button(80,529,58,40,"Phải")){viewAngle+=90;PositionCamera();}
            if(Button(16,575,58,40,"−")){viewSize=Mathf.Min(16,viewSize+1);PositionCamera();}
            if(Button(80,575,58,40,"+")){viewSize=Mathf.Max(7,viewSize-1);PositionCamera();}
            foreach(var t in tables)
            {
                if(t.floor!=currentFloor)continue;
                Vector3 p=interiorCamera.WorldToScreenPoint(room.transform.position+SlotPosition(t.slot)+new Vector3(0,1.2f,0));
                float x=(p.x-guiArea.x)/guiScale,y=(Screen.height-p.y-guiArea.y)/guiScale;
                if(y<95||y>576||x<166||x>1090)continue;
                if(GUI.Button(new Rect(x-47,y-30,94,72),GUIContent.none,GUIStyle.none))selectedTable=t.slot;
                if(t.occupied&&!t.served||selectedTable==t.slot)
                {
                    Ink(new Rect(x-13,y-37,26,26),t.occupied&&!t.served?FarmWorld.Hex("D9AC52"):FarmWorld.Hex("334A35"),true);
                    GUI.Label(new Rect(x-13,y-37,26,26),(t.slot+1).ToString(),smallStyle);
                }
            }
            var selected=tables.Find(t=>t.floor==currentFloor&&t.slot==selectedTable);
            if(selected!=null)DrawEndlessTable(selected);
            if(Service!=null){Panel(new Rect(154,92,491,39));Label(168,97,471,29,"Bếp "+game.Model.KitchenLevel+" · "+Service.CookingCount+"/"+game.Model.HotpotBurners+" nồi"+(Service.Rush?" · Sóng "+Service.Wave+" · "+Mathf.CeilToInt((float)Service.WaveRemaining)+"s":""));}
            if(game.Model.State.hotpot.recipes.Count==0&&selected==null)
            {
                Panel(new Rect(804,609,380,98));
                Label(822,616,344,82,"Món đầu tay\nNhập hàng → Nấu món → nấu thử\nrồi đưa lên thực đơn để đón khách.");
            }
        }
        private void DrawSelectedTable(Table t)
        {
            Panel(new Rect(804,552,380,155));NavIcon(new Rect(820,564,38,42),2);
            Label(875,562,290,30,"Bàn "+(t.slot+1)+(t.occupied?t.served?" · Đang ăn":" · Chờ món":" · Trống"),true);
            var recipe=game.Model.State.hotpot.recipes.Find(r=>r.id==t.recipeId);
            Label(820,603,345,27,t.occupied&&recipe!=null?recipe.name:"Sẵn sàng đón khách");
            if(t.occupied)
            {
                Ink(new Rect(820,635,345,8),FarmWorld.Hex("475149"));
                Ink(new Rect(820,635,345*Mathf.Clamp01(t.remaining/(t.served?12:60)),8),FarmWorld.Hex(t.served?"73AD68":"D9AC52"));
                if(!t.served&&Button(820,652,345,41,"Lên món · "+FarmModel.Money(recipe==null?0:game.Model.RecipePrice(recipe))))ServeTable(t.floor,t.slot);
                else Label(820,650,345,33,"Khách dùng bữa · "+Mathf.CeilToInt(t.remaining)+"s");
            }
        }
        private void DrawFloorButtons(float y)
        {
            for(int f=0;f<game.Model.State.hotpot.floors;f++)
            {int waiting=tables.FindAll(t=>t.floor==f&&t.occupied&&!t.served).Count;if(Button(16,y+f*47,122,39,(f==currentFloor?"• ":"")+(f==3?"Sân thượng":"Tầng "+(f+1))+(waiting>0?" · "+waiting:"")))ShowFloor(f);}
        }
        private void DrawSupply()
        {
            Label(30,102,600,32,"KHO BẾP · tách khỏi kho nông trại",true);
            if(Button(650,100,180,40,"Phẩm chất: "+new string('★',stars)))stars=stars%3+1;
            if(Button(845,100,155,40,"Số lượng: "+batch))batch=batch==1?5:batch==5?10:1;
            if(Button(30,150,245,42,(supplier?"":"● ")+"Từ nông trại")){supplier=false;scroll=Vector2.zero;}
            if(Button(290,150,245,42,(supplier?"● ":"")+"Nhà cung cấp")){supplier=true;scroll=Vector2.zero;}
            Label(555,155,610,38,supplier?"Mua nước nền, gia vị và thịt/hải sản.":"Giữ nguyên phẩm chất sao khi chuyển vào bếp.");
            int rows=Array.FindAll(HotpotRules.Ingredients,s=>s.farm!=supplier).Length;
            scroll=GUI.BeginScrollView(new Rect(25,202,1150,374),scroll,new Rect(0,0,1120,rows*58));
            int index=0;
            foreach(var s in HotpotRules.Ingredients)
            {
                if(s.farm==supplier)continue;
                int quality=s.farm&&CropCatalog.Find(s.id)!=null?stars:1;float y=index++*58;
                Label(10,y+8,220,40,s.name);
                int unit=game.Model.KitchenUnitPrice(s.id);
                Label(240,y+8,470,40,s.farm?"Farm: "+game.Model.FarmKitchenAvailable(s.id,quality)+" · Bếp: "+game.Model.KitchenCount(s.id,quality)+" "+new string('★',quality):FarmModel.Money(unit)+"/phần · Hạng "+FarmModel.SupplierUnlockRank(s.id)+" · Bếp: "+game.Model.KitchenCount(s.id));
                if(Button(755,y,340,48,s.farm?"Nhập "+batch+" từ nông trại":"Mua "+batch+" · "+FarmModel.Money(unit*batch)))Perform(s.farm?game.Model.ImportKitchen(s.id,quality,batch):game.Model.BuyKitchen(s.id,batch));
            }
            GUI.EndScrollView();
        }
        private void DrawRecipe()
        {
            Label(28,101,145,30,"Tên món:");draft.name=GUI.TextField(new Rect(165,101,380,38),draft.name,32,new GUIStyle(GUI.skin.textField){fontSize=20});
            if(Button(568,100,290,40,"Vị: "+HotpotRules.Styles[draft.style]))draft.style=(draft.style+1)%3;
            if(Button(880,100,285,40,"Nền: "+HotpotRules.Broths[draft.broth]))draft.broth=(draft.broth+1)%3;
            if(Button(28,150,160,40,"Mặn: "+draft.salt))draft.salt=(draft.salt+1)%4;
            if(Button(198,150,160,40,"Cay: "+draft.heat))draft.heat=(draft.heat+1)%4;
            if(Button(370,150,430,40,HotpotRules.Techniques[draft.technique]))draft.technique=(draft.technique+1)%3;
            if(Button(815,150,350,40,"Thêm phẩm chất "+new string('★',stars)))stars=stars%3+1;
            scroll=GUI.BeginScrollView(new Rect(25,200,570,320),scroll,new Rect(0,0,540,(HotpotRules.Ingredients.Length-2)*48));
            int i=0;
            foreach(var s in HotpotRules.Ingredients)
            {
                if(s.id=="base"||s.id=="seasoning")continue;
                int quality=CropCatalog.Find(s.id)!=null?stars:1;float y=i++*48;
                Label(5,y+4,390,38,s.name+" "+new string('★',quality)+" · bếp "+game.Model.KitchenCount(s.id,quality));
                if(Button(400,y,120,40,"+ Thêm"))
                {
                    var existing=draft.portions.Find(p=>p.id==s.id);
                    if(existing!=null){if(existing.stars!=quality){existing.stars=quality;existing.amount=1;}else existing.amount=Math.Min(3,existing.amount+1);}
                    else if(draft.portions.Count<6)draft.portions.Add(new HotpotPortion(s.id,1,quality));
                }
            }
            GUI.EndScrollView();
            Label(625,201,530,30,"Mỗi nồi: 2–6 nguyên liệu",true);
            for(int j=0;j<draft.portions.Count;j++)
            {
                var p=draft.portions[j];float y=240+j*42;
                Label(625,y,435,40,HotpotRules.Find(p.id).name+" "+new string('★',p.stars)+" × "+p.amount);
                if(Button(1070,y,80,36,"−")){p.amount--;if(p.amount==0)draft.portions.RemoveAt(j);break;}
            }
            Label(625,500,540,40,"Mỗi nồi dùng 1 nước nền + "+(1+draft.salt+draft.heat)+" gia vị.");
            if(Button(28,536,550,45,"Nấu thử, tiêu hao 1 nồi và lưu phiên bản"))
            {
                if(HasWaitingOrders)message="Phục vụ các bàn đang chờ trước khi dùng nguyên liệu nấu thử.";
                else {var result=game.Model.TestHotpotRecipe(draft);Perform(result);if(result.success){tab=3;scroll=Vector2.zero;}}
            }
            if(Button(625,545,530,36,"Xóa bản đang phối"))draft=new HotpotRecipe();
        }
        private void DrawMenu()
        {
            var recipes=game.Model.State.hotpot.recipes;
            Label(30,100,1140,42,"SỔ CÔNG THỨC · "+recipes.Count+"/24 · Muốn đổi định lượng, tạo phiên bản và nấu thử lại",true);
            scroll=GUI.BeginScrollView(new Rect(25,153,1150,425),scroll,new Rect(0,0,1120,Mathf.Max(400,recipes.Count*235)));
            if(recipes.Count==0)Label(20,20,1000,100,"Chưa có công thức. Nhập hàng rồi sang Sáng tạo món để nấu thử món đầu tiên.");
            for(int i=0;i<recipes.Count;i++)
            {
                var r=recipes[i];var taste=HotpotRules.Taste(r);float y=i*235;
                Label(15,y,730,36,r.name+" #"+r.id+" · "+taste.Score+"/100 · "+FarmModel.Money(game.Model.RecipePrice(r)),true);
                Label(15,y+38,1030,32,"Vị "+taste.balance+"/30 · Phối hợp "+taste.pairing+"/25 · Tỷ lệ "+taste.ratio+"/20 · Nấu "+taste.technique+"/15 · Nông sản "+taste.quality+"/10");
                Label(15,y+73,730,76,String.Join("\n",taste.notes.GetRange(0,Math.Min(3,taste.notes.Count)).ToArray()));
                string missing=game.Model.KitchenMissing(r);Label(15,y+155,720,66,"Tay nghề "+HotpotRules.Mastery(r)+" · "+r.timesServed+" nồi · "+(int)Math.Ceiling(game.Model.HotpotCookSeconds(r))+"s/nồi\n"+(missing==""?"Kho bếp đủ nguyên liệu.":missing));
                if(Button(785,y+75,305,50,r.onMenu?"Ngừng bán":"Đưa lên thực đơn"))
                {
                    if(tables.Exists(t=>t.occupied&&!t.served&&t.recipeId==r.id))message="Phục vụ xong các bàn đã gọi món trước khi ngừng bán.";
                    else Perform(game.Model.SetHotpotMenu(r.id,!r.onMenu));
                }
                if(Button(785,y+139,305,48,"Tạo biến thể")){draft=HotpotRules.Copy(r);draft.id=0;draft.onMenu=false;tab=2;scroll=Vector2.zero;break;}
                if(!r.onMenu&&Button(785,y+190,305,36,"Xóa khỏi sổ")){Perform(game.Model.RemoveHotpotRecipe(r.id));break;}
            }
            GUI.EndScrollView();
        }
        private void DrawDecor() { DrawFurnitureShop(); }
    }
}
