using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmDevelopment
    {
        private int contentPage,section;
        private string itemSearch="";
        private GUIStyle searchStyle;
        private readonly Dictionary<string,Texture2D> itemImages=new Dictionary<string,Texture2D>();
        public int ContentPage => contentPage;
        public int ContentPages {get;private set;}=1;
        public void ChangeContentPage(int direction){contentPage=(contentPage+direction%ContentPages+ContentPages)%ContentPages;if(tab==7)crewPage=contentPage;}
        public void SelectSection(int value){section=Mathf.Clamp(value,0,tab==4||tab==8?2:1);contentPage=0;itemSearch="";}
        private void ResetPages(){confirmJourneyCancel=false;buildingPage=contentPage=section=0;ContentPages=1;itemSearch="";}
        private string TabHint()
        {
            string[] hints={"Giao đơn để nhận tiền, gỗ, quặng và XP. Nhiệm vụ tuần tích lũy khi bạn chơi.",
                "Dùng nút trên thẻ để xây, nâng cấp hoặc mở khu sản xuất.",
                "Nguyên liệu được trừ khi bắt đầu. Hàng chờ vẫn tiếp tục chế biến khi thoát game.",
                "Hoàn thành từng trang nhật ký và nhận thưởng thành tích khi đạt mục tiêu.",
                "Hàng hai sao bán giá ×1,35; ba sao ×1,8. Bán nông sản trong Kho ở nông trại.",
                "Chọn giống rồi chạm ô đất trống để gieo. Sáu cây ăn quả có thể thu nhiều vụ.",
                "Chọn ô đất trước khi chăm sóc. Khay ươm sẵn sàng giúp cấy cây mà không mua lại giống.",
                "Nhân công chuyên môn dùng ca trả trước, chỉ làm ở ruộng khi chơi solo. Bậc học 2 tăng kỹ năng; không có thưởng offline."};
            return tab==8?"Bốn mùa nối tiếp theo nhịp chơi. Giữ trang trại, tích lũy tay nghề và xây di sản cho hòn đảo.":hints[tab];
        }
        private void Card(Rect r){Paint(r,"D9E2D1");Paint(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),"FFFFFF");}
        private void Meter(Rect r,float progress)
        {Paint(r,"E5EBDE");if(progress>0)Paint(new Rect(r.x,r.y,r.width*Mathf.Clamp01(progress),r.height),"7FA16D");}
        private void Icon(Rect rect,string path)
        {
            Paint(rect,"F5EBD4");
            if(path.Contains("IX_"))path=path.Replace("ModelThumbnails/","IslandExpansion/Thumbnails/").Replace("IX_","");
            if(!itemImages.TryGetValue(path,out var image)){image=Resources.Load<Texture2D>(path);itemImages[path]=image;}
            if(image!=null)GUI.DrawTexture(rect,image,ScaleMode.ScaleToFit);
        }
        private static string ItemImage(string id)
        {
            var crop=CropCatalog.Find(id);if(crop!=null)return "ModelThumbnails/Product_"+crop.modelName;
            switch(id)
            {
                case "eggs":return "ModelThumbnails/Product_Egg";
                case "milk":return "ModelThumbnails/Product_Milk";
                case "honey":return "ModelThumbnails/Product_Honey";
                case "fish":return "ModelThumbnails/Product_FishBasket";
                case "flour":return "ModelThumbnails/Product_Flour";
                case "bread":return "ModelThumbnails/Product_Bread";
                case "sandwich":return "ModelThumbnails/Product_Sandwich";
                case "cheese":return "ModelThumbnails/Product_Cheese";
                case "herbal":return "ModelThumbnails/Product_HerbalPotion";
                case "compost":return "ModelThumbnails/Product_Compost";
                default:return "ModelThumbnails/"+FarmHud.ProductPicture(id);
            }
        }
        private int PageStart(int count,int perPage)
        {ContentPages=Mathf.Max(1,(count+perPage-1)/perPage);contentPage=Mathf.Clamp(contentPage,0,ContentPages-1);return contentPage*perPage;}
        private void Pager(int count,string unit)
        {
            ActionButton(new Rect(16,421,108,34),"← Trước",ContentPages>1,()=>ChangeContentPage(-1));
            GUI.Label(new Rect(140,429,750,25),"Trang "+(contentPage+1)+" / "+ContentPages+"  ·  "+count+" "+unit,caption);
            ActionButton(new Rect(940,421,108,34),"Tiếp →",ContentPages>1,()=>ChangeContentPage(1));
        }
        private void Sections(params string[] labels)
        {for(int i=0;i<labels.Length;i++)if(SoftButton(new Rect(16+i*221,14,211,38),labels[i],section==i))SelectSection(i);}
        private void Search(float x=696,float y=16,float width=350)
        {
            GUI.Label(new Rect(x,y+5,65,28),"Tìm tên",caption);
            if(searchStyle==null)
            {
                searchStyle=new GUIStyle(GUI.skin.textField){fontSize=18,padding=new RectOffset(8,8,6,4),border=new RectOffset(0,0,0,0)};
                searchStyle.normal.background=searchStyle.hover.background=searchStyle.active.background=searchStyle.focused.background=Texture2D.whiteTexture;
                Ink(searchStyle,"5A4327");
            }
            var old=GUI.backgroundColor;GUI.backgroundColor=FarmWorld.Hex("E5EBDE");
            string next=GUI.TextField(new Rect(x+66,y,width-66,34),itemSearch,searchStyle);GUI.backgroundColor=old;
            if(next!=itemSearch){itemSearch=next;contentPage=0;}
        }
        private bool Matches(string label){return label.IndexOf(itemSearch,StringComparison.OrdinalIgnoreCase)>=0;}
        private void Empty(string title,string description)
        {Paint(new Rect(16,87,1032,128),"F5EBD4");GUI.Label(new Rect(38,108,990,32),title,cardTitle);GUI.Label(new Rect(38,152,985,54),description,small);}
        private Rect GridCard(int n,float top=66,float height=166)
        {var r=new Rect(16+n%3*349,top+n/3*(height+12),334,height);Card(r);return r;}

        private void DrawOrdersPage()
        {
            var m=game.Model;var d=m.State.development;string id=m.OrderItem;
            Card(new Rect(16,14,668,191));Icon(new Rect(31,30,107,107),ItemImage(id));
            GUI.Label(new Rect(156,29,503,30),"Đơn hàng của Hải Âu",cardTitle);
            GUI.Label(new Rect(156,66,503,31),m.OrderQuantity+" "+DevelopmentCatalog.ItemName(id)+"  ·  Kho: "+m.ItemCount(id),body);
            GUI.Label(new Rect(156,105,500,46),"Thưởng "+FarmModel.Money(m.OrderReward)+"\n+3 gỗ · +1 quặng · +20 XP",small);
            Meter(new Rect(31,150,335,7),(float)m.ItemCount(id)/m.OrderQuantity);
            ActionButton(new Rect(413,154,254,38),"Giao đơn",m.ItemCount(id)>=m.OrderQuantity,()=>Execute("order",()=>m.FulfillOrder()));
            Card(new Rect(700,14,348,191));
            GUI.Label(new Rect(718,30,314,30),"Quà mỗi ngày",cardTitle);
            bool claimed=(int)(game.Now/86400)<=d.lastLoginDay;
            GUI.Label(new Rect(718,70,314,64),"Chuỗi đăng nhập: "+d.loginStreak+" ngày\nNhận tiền và 2 gỗ mỗi ngày.",small);
            ActionButton(new Rect(718,154,312,38),claimed?"Hôm nay đã nhận":"Nhận quà hôm nay",!claimed,()=>Execute("daily",()=>m.ClaimDaily(game.Now)));
            GUI.Label(new Rect(16,222,700,30),"Nhiệm vụ tuần",cardTitle);
            GUI.Label(new Rect(688,226,360,27),FarmModel.FestivalActive(game.Now)?"Lễ hội Đom Đóm · Giá bán +20%":"Tích lũy qua các hoạt động trên đảo",caption);
            for(int i=0;i<m.State.quests.Count;i++)
            {
                int index=i;var q=m.State.quests[i];float x=16+i*349;Card(new Rect(x,264,334,174));
                GUI.Label(new Rect(x+14,278,307,46),FarmModel.QuestName(q),body);
                int progress=m.QuestProgress(i);GUI.Label(new Rect(x+14,326,307,26),progress+" / "+q.target+"  ·  +20 XP",caption);
                Meter(new Rect(x+14,360,305,7),(float)progress/q.target);
                ActionButton(new Rect(x+14,384,305,39),q.claimed?"Đã nhận thưởng":"Nhận "+FarmModel.Money(FarmModel.QuestReward(q)),game.Online==null&&!q.claimed&&progress>=q.target,()=>Local(()=>m.ClaimQuest(index)));
            }
        }

        private void DrawProductionPage()
        {
            var m=game.Model;var d=m.State.development;
            Sections("Công thức", "Hàng chờ · "+d.jobs.Count);
            if(!string.IsNullOrEmpty(buildingFilter)&&SoftButton(new Rect(700,14,340,38),"Xem tất cả xưởng")){buildingFilter="";contentPage=0;}
            if(section==1)
            {
                var jobs=d.jobs.ToArray();int start=PageStart(jobs.Length,6);
                if(jobs.Length==0)Empty("Các xưởng đang nghỉ","Chọn Công thức để bắt đầu một lượt chế biến. Nguyên liệu sẽ được trừ khi xếp hàng.");
                for(int n=0;n<6&&start+n<jobs.Length;n++)
                {
                    var job=jobs[start+n];var spec=DevelopmentCatalog.Recipe(job.recipe);Rect r=GridCard(n);
                    Icon(new Rect(r.x+12,r.y+12,66,66),ItemImage(job.recipe));
                    GUI.Label(new Rect(r.x+91,r.y+14,230,29),spec.name,cardTitle);
                    double remaining=Math.Max(0,job.readyAt-game.Now);
                    GUI.Label(new Rect(r.x+91,r.y+51,230,27),remaining<=0?"Đã hoàn thành":"Còn "+Math.Ceiling(remaining)+" giây",small);
                    GUI.Label(new Rect(r.x+12,r.y+89,309,27),DevelopmentCatalog.Building(spec.building).name,caption);
                    ActionButton(new Rect(r.x+12,r.y+122,309,32),"Thu thành phẩm",remaining<=0,()=>Execute("collect",()=>m.CollectProduction(job.id,game.Now),job:job.id));
                }
                Pager(jobs.Length,"lượt chế biến");return;
            }
            var recipes=Array.FindAll(DevelopmentCatalog.Recipes,r=>string.IsNullOrEmpty(buildingFilter)||r.building==buildingFilter);
            int first=PageStart(recipes.Length,6);
            for(int n=0;n<6&&first+n<recipes.Length;n++)
            {
                var recipe=recipes[first+n];Rect r=GridCard(n);int level=m.BuildingLevel(recipe.building);
                Icon(new Rect(r.x+12,r.y+12,64,64),ItemImage(recipe.id));
                GUI.Label(new Rect(r.x+89,r.y+12,235,29),recipe.name,cardTitle);
                GUI.Label(new Rect(r.x+89,r.y+46,232,43),Math.Ceiling(m.ProductionDuration(recipe,level))+"s · Bán "+FarmModel.Money(recipe.price)+"\n"+DevelopmentCatalog.Building(recipe.building).name,caption);
                GUI.Label(new Rect(r.x+12,r.y+92,311,26),m.ItemCount(recipe.first)+"/"+recipe.firstCount+" "+DevelopmentCatalog.ItemName(recipe.first)+(recipe.secondCount>0?" · "+m.ItemCount(recipe.second)+"/"+recipe.secondCount+" "+DevelopmentCatalog.ItemName(recipe.second):""),caption);
                int queued=d.jobs.FindAll(j=>DevelopmentCatalog.Recipe(j.recipe).building==recipe.building).Count;
                bool enough=m.ItemCount(recipe.first)>=recipe.firstCount&&(recipe.secondCount==0||m.ItemCount(recipe.second)>=recipe.secondCount);
                string status=level==0?"Cần xây xưởng":queued>=level?"Hàng chờ đầy":!enough?"Thiếu nguyên liệu":"Chế biến";
                ActionButton(new Rect(r.x+12,r.y+122,309,32),status,level>0&&queued<level&&enough,()=>Execute("produce",()=>m.StartProduction(recipe.id,game.Now),recipe.id));
            }
            Pager(recipes.Length,"công thức");
        }

        private void DrawJournalPage()
        {
            var m=game.Model;var d=m.State.development;Card(new Rect(16,14,366,397));
            GUI.Label(new Rect(34,30,327,27),"NHẬT KÝ · TRANG "+Math.Min(5,d.chapter+1)+" / 5",small);
            GUI.Label(new Rect(34,69,328,58),m.ChapterTitle,new GUIStyle(cardTitle){wordWrap=true});
            string[] stories={"Ông bà để lại hòn đảo và một lời hẹn: hãy bắt đầu từ hạt giống đầu tiên.","Đất từng bạc màu sau Bão Xám. Mỗi vụ mùa là một bước hồi sinh.","Một bếp lửa sáng lên cũng là một mái nhà trở lại.","Hải Âu mang hàng hóa và tin vui đến những miền xa.","Đom Đóm Vàng xuất hiện khi con người biết chăm cả đất lẫn cây.","Năm trang đầu đã được khôi phục. Câu chuyện các vùng đảo còn tiếp diễn."};
            GUI.Label(new Rect(34,135,328,91),stories[Math.Min(5,d.chapter)],small);
            Paint(new Rect(30,242,338,75),"EDF1E5");GUI.Label(new Rect(42,251,314,57),m.ChapterTask,body);
            GUI.Label(new Rect(34,328,330,24),"Thưởng "+FarmModel.Money(25)+" · 5 gỗ · 20 XP",caption);
            bool complete=d.chapter==0?m.State.plots.Exists(p=>!string.IsNullOrEmpty(p.cropId))||m.State.harvested>0:d.chapter==1?m.State.harvested>=3:d.chapter==2?d.crafted>=3:d.chapter==3?d.ordersCompleted>=5:d.chapter==4&&m.Level>=6;
            ActionButton(new Rect(34,364,330,34),d.chapter>=5?"Đã hoàn tất nhật ký":"Hoàn thành trang",d.chapter<5&&complete,()=>Execute("chapter",()=>m.ClaimChapter()));
            GUI.Label(new Rect(402,19,620,33),"Thành tích của bạn",cardTitle);
            int start=PageStart(FarmModel.Achievements.Length,4);
            for(int n=0;n<4&&start+n<FarmModel.Achievements.Length;n++)
            {
                int index=start+n;var a=FarmModel.Achievements[index];float y=61+n*88;Card(new Rect(400,y,648,79));
                GUI.Label(new Rect(413,y+7,620,26),a.name,small);
                int progress=Math.Min(a.goal,m.AchievementProgress(index));bool claimed=m.AchievementClaimed(index);
                GUI.Label(new Rect(414,y+41,85,25),progress+" / "+a.goal,caption);Meter(new Rect(503,y+49,210,7),(float)progress/a.goal);
                ActionButton(new Rect(732,y+35,302,33),claimed?"Đã nhận":"Nhận "+FarmModel.Money(a.reward)+" +15 XP",game.Online==null&&!claimed&&progress>=a.goal,()=>Local(()=>m.ClaimAchievement(index)));
            }
            Pager(FarmModel.Achievements.Length,"thành tích");
        }

        private void DrawInventoryPage()
        {
            var m=game.Model;Sections("Nông sản", "Thành phẩm", "Chăn nuôi");Search();
            var ids=new List<string>();
            if(section==0){foreach(var c in CropCatalog.All)if(Matches(c.displayName))ids.Add(c.id);}
            else if(section==1){foreach(var r in DevelopmentCatalog.Recipes)if(Matches(r.name))ids.Add(r.id);}
            else foreach(var id in new[]{"eggs","milk","honey","fish"})if(Matches(StockName(id)))ids.Add(id);
            int start=PageStart(ids.Count,6);if(ids.Count==0)Empty("Chưa tìm thấy món phù hợp","Thử tìm bằng tên nông sản hoặc chuyển sang một nhóm khác.");
            for(int n=0;n<6&&start+n<ids.Count;n++)
            {
                string id=ids[start+n];Rect r=GridCard(n);Icon(new Rect(r.x+12,r.y+12,69,69),ItemImage(id));
                int count=id=="honey"?m.State.honey:id=="fish"?m.State.fishStock:m.ItemCount(id);
                GUI.Label(new Rect(r.x+95,r.y+12,225,29),StockName(id),cardTitle);
                GUI.Label(new Rect(r.x+95,r.y+50,225,30),"Trong kho: "+count,body);
                var crop=CropCatalog.Find(id);var recipe=DevelopmentCatalog.Recipe(id);
                if(crop!=null)
                {
                    GUI.Label(new Rect(r.x+12,r.y+93,309,26),"★★ "+m.CountFine(id)+"    ★★★ "+m.CountPrime(id),small);
                    GUI.Label(new Rect(r.x+12,r.y+129,309,26),"Giá thường: "+FarmModel.Money(crop.sellPrice)+" / món",caption);
                }
                else if(recipe!=null)
                {
                    GUI.Label(new Rect(r.x+12,r.y+92,309,26),"Giá: "+FarmModel.Money(recipe.price)+" / món",caption);
                    ActionButton(new Rect(r.x+12,r.y+122,309,32),"Chọn số lượng bán",count>0,()=>{Close();game.Hud.OpenTrading();});
                }
                else GUI.Label(new Rect(r.x+12,r.y+98,309,47),"Thu từ chuồng hoặc ao cá.\nBán cùng nông sản tại Kho nông trại.",small);
            }
            Pager(ids.Count,"mặt hàng");
        }
        private static string StockName(string id){return id=="honey"?"Mật ong":id=="fish"?"Cá tươi":DevelopmentCatalog.ItemName(id);}

        private void DrawSeedsPage()
        {
            var m=game.Model;GUI.Label(new Rect(16,19,665,32),"Vườn giống · "+CropCatalog.All.Length+" loại cây",cardTitle);Search();
            var crops=new List<CropSpec>();foreach(var c in CropCatalog.All)if(Matches(c.displayName))crops.Add(c);
            int start=PageStart(crops.Count,6);if(crops.Count==0)Empty("Không tìm thấy giống cây","Thử tìm bằng một tên ngắn hơn.");
            for(int n=0;n<6&&start+n<crops.Count;n++)
            {
                var crop=crops[start+n];Rect r=GridCard(n);Icon(new Rect(r.x+12,r.y+12,69,69),"ModelThumbnails/"+crop.modelName);
                GUI.Label(new Rect(r.x+94,r.y+12,228,29),crop.displayName,cardTitle);
                GUI.Label(new Rect(r.x+94,r.y+46,228,45),FarmModel.Money(crop.seedCost)+" · "+crop.growSeconds+"s\n"+(FarmModel.IsPerennial(crop.id)?"Cây lâu năm":FarmModel.CropGroup(crop.id)),caption);
                GUI.Label(new Rect(r.x+12,r.y+94,309,26),"Thu "+crop.yield+" mỗi vụ · Mở cấp "+crop.unlockLevel,small);
                bool unlocked=m.Level>=crop.unlockLevel&&m.IslandCropUnlocked(crop.id)&&(IslandCropCatalog.Index(crop.id)<0||game.Online==null);
                string status=m.Level<crop.unlockLevel?"Mở ở cấp "+crop.unlockLevel:!unlocked?"Cần M"+IslandCropCatalog.Chapter(crop.id)+" · solo":"Chọn giống";
                ActionButton(new Rect(r.x+12,r.y+124,309,32),status,unlocked,()=>{Close();game.SelectCrop(crop.id);});
            }
            Pager(crops.Count,"giống cây");
        }

        private void DrawAgronomyPage()
        {
            var m=game.Model;var d=m.State.development;int selected=game.SelectedPlot;bool hasPlot=selected>=0&&selected<m.State.plots.Count;
            var plot=hasPlot?m.State.plots[selected]:null;
            Paint(new Rect(16,14,1032,104),"E9EFDF");
            GUI.Label(new Rect(30,23,780,29),hasPlot?"Chăm ô "+(selected+1)+" · Độ phì "+(plot.soilQuality+1)+" / 3":"Chọn một ô đất để chăm sóc",cardTitle);
            if(SoftButton(new Rect(861,24,173,31),"Chọn ô trên ruộng"))Close();
            ActionButton(new Rect(30,69,240,35),"Bón phân · "+m.ItemCount("compost"),hasPlot&&plot.soilQuality<2&&m.ItemCount("compost")>0,()=>Execute("careProduct",()=>m.UseCareProduct("compost",selected),"compost"));
            ActionButton(new Rect(282,69,240,35),"Trị bệnh · "+m.ItemCount("herbal"),hasPlot&&plot.disease>=1&&m.ItemCount("herbal")>0,()=>Execute("careProduct",()=>m.UseCareProduct("herbal",selected),"herbal"));
            ActionButton(new Rect(534,69,240,35),hasPlot&&plot.greenhouse?"Bỏ mái che":"Che nhà kính",hasPlot,()=>Execute("greenhouse",()=>m.ToggleGreenhouse(selected)));
            ActionButton(new Rect(786,69,248,35),confirmChop==selected&&selected>=0?"Xác nhận chặt cây?":"Chặt cây lâu năm",hasPlot&&FarmModel.IsPerennial(plot.cropId)&&game.Online==null,()=>{if(confirmChop==selected){Local(()=>m.ChopPerennial(selected));confirmChop=-1;}else confirmChop=selected;});
            if(SoftButton(new Rect(16,130,215,37),"Ươm giống mới",section==0))SelectSection(0);
            if(SoftButton(new Rect(239,130,230,37),"Khay ươm · "+d.seedlings.Count+" / "+m.BuildingLevel("nursery")*2,section==1))SelectSection(1);
            Search(690,132,357);
            if(section==1)
            {
                var seedlings=d.seedlings.FindAll(s=>Matches(DevelopmentCatalog.ItemName(s.cropId))).ToArray();int start=PageStart(seedlings.Length,3);
                if(seedlings.Length==0){GUI.Label(new Rect(32,216,990,72),"Chưa có khay ươm phù hợp. Chọn Ươm giống mới để chuẩn bị cây con cho vụ sau.",body);}
                for(int n=0;n<3&&start+n<seedlings.Length;n++)
                {
                    var seed=seedlings[start+n];var crop=CropCatalog.Find(seed.cropId);Rect r=GridCard(n,182,222);
                    Icon(new Rect(r.x+12,r.y+12,70,70),"ModelThumbnails/"+crop.modelName);
                    GUI.Label(new Rect(r.x+95,r.y+16,225,31),crop.displayName,cardTitle);
                    bool ready=seed.readyAt<=game.Now;GUI.Label(new Rect(r.x+12,r.y+102,309,52),ready?"Đã sẵn sàng cấy ra ruộng.":"Còn "+Math.Ceiling(seed.readyAt-game.Now)+" giây.",body);
                    ActionButton(new Rect(r.x+12,r.y+174,309,35),"Chọn để cấy",ready,()=>{Close();game.SelectCrop(seed.cropId);});
                }
                Pager(seedlings.Length,"khay ươm");return;
            }
            var crops=new List<CropSpec>();foreach(var c in CropCatalog.All)if(Matches(c.displayName))crops.Add(c);
            int first=PageStart(crops.Count,3);
            for(int n=0;n<3&&first+n<crops.Count;n++)
            {
                var crop=crops[first+n];Rect r=GridCard(n,182,222);Icon(new Rect(r.x+12,r.y+12,70,70),"ModelThumbnails/"+crop.modelName);
                GUI.Label(new Rect(r.x+95,r.y+16,225,31),crop.displayName,cardTitle);
                GUI.Label(new Rect(r.x+95,r.y+51,225,30),"Mở cấp "+crop.unlockLevel,small);
                GUI.Label(new Rect(r.x+12,r.y+102,309,58),FarmModel.Money(crop.seedCost)+" · Ươm "+Math.Round(m.NurseryDuration(crop),1)+"s\nCấy ra ruộng không mua lại giống.",small);
                bool room=d.seedlings.Count<m.BuildingLevel("nursery")*2,unlocked=m.IslandCropUnlocked(crop.id)&&(IslandCropCatalog.Index(crop.id)<0||game.Online==null);
                string status=m.BuildingLevel("nursery")==0?"Cần xây vườn ươm":!room?"Khay ươm đầy":m.Level<crop.unlockLevel?"Chưa mở giống":!unlocked?"Cần M"+IslandCropCatalog.Chapter(crop.id)+" · solo":m.State.coins<crop.seedCost?"Chưa đủ tiền":"Ươm cây";
                ActionButton(new Rect(r.x+12,r.y+174,309,35),status,room&&unlocked&&m.Level>=crop.unlockLevel&&m.State.coins>=crop.seedCost,()=>Execute("nursery",()=>m.StartNursery(crop.id,game.Now),crop.id));
            }
            if(crops.Count==0)GUI.Label(new Rect(32,219,990,48),"Không tìm thấy giống cây phù hợp.",body);
            Pager(crops.Count,"giống cây");
        }

    }
}
