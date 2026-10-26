using System;
using HarvestIsle.Core;
using UnityEngine;
namespace HarvestIsle
{
    public sealed partial class FarmIslandExpansion
    {
        private int serviceChoice;
        private void DrawJournal()
        {
            GUILayout.Label("Nghề mới, vườn mới, nếp sống mới",title);
            GUILayout.Label("Trồng 30 giống mới, mở 30 công trình và phối 60 đồ trang trí. Nội dung mở dần theo chương; đồ mua và đồ tự làm đều được cất lại để đặt miễn phí.",body);
            GUILayout.Space(12);
            GUILayout.Label("M1 · Bếp vườn: cấp 3.\nM2 · Nghề đảo: cấp 7 và 3 mẻ hoàn thành.\nM3 · Làng hồi sinh: cấp 10 và 8 mẻ hoàn thành.",body);
            if(Button("Bắt đầu chương kế",235))Result(game.Model.BeginIslandChapter());
            GUILayout.Space(12);GUILayout.Label("Bắt đầu: mua Xưởng bún gạo → đặt ở khu nghề → mang 3 lúa thường ★ → làm bún → thu thành phẩm. Gieo lạc và mè để mở các nhánh dầu/kẹo/bánh tráng.",body);
            GUILayout.Label("Kéo khay giống ở ruộng để xem đủ 60 giống. Mỗi mẻ chỉ lấy hàng thường ★; hàng ★★/★★★ và các lô để riêng được giữ lại. Hàng đã xong sẽ chờ bạn quay lại.",body);
            GUILayout.Space(10);GUILayout.Label("HỒ SƠ ĐÃ MỞ · "+game.Model.Island.journal.Count,title);
            foreach(string key in game.Model.Island.journal)GUILayout.Label("• "+StoryName(key),body);
        }
        private void DrawShop(bool buildings)
        {
            var entries=Array.FindAll(IslandExpansionCatalog.Things,s=>s.Building==buildings&&Match(s.name));Pager(entries.Length);
            for(int i=page*6;i<Math.Min(entries.Length,(page+1)*6);i++)
            {
                var s=entries[i];GUILayout.BeginHorizontal(GUI.skin.box);Icon(s.id);GUILayout.BeginVertical();
                GUILayout.Label(s.name+" · M"+s.chapter+" · "+s.width+"×"+s.depth+" m",body);GUILayout.Label(s.description,small);GUILayout.EndVertical();
                bool have=buildings&&game.Model.Island.owned.Exists(p=>p.id==s.id);GUI.enabled=!have&&game.Model.Island.chapter>=s.chapter&&game.Model.State.coins>=s.price;
                if(Button(have?"Đã sở hữu":FarmModel.Money(s.price)))Result(game.Model.BuyIslandThing(s.id));GUI.enabled=true;GUILayout.EndHorizontal();
            }
        }
        private void DrawRecipes()
        {
            GUILayout.Label("Hàng chờ · nhận hàng trước khi cất công trình",body);
            foreach(var j in game.Model.Island.jobs.ToArray())
            {
                var r=IslandExpansionCatalog.Recipe(j.recipe);GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(r==null?StoryName(j.recipe):r.name,body);double seconds=Math.Max(0,j.readyAt-game.Now);
                GUI.enabled=seconds==0;if(Button(seconds==0?"Nhận thành phẩm":Math.Ceiling(seconds)+" giây"))Result(game.Model.CollectIslandJob(j.serial,game.Now));GUI.enabled=true;GUILayout.EndHorizontal();
            }
            var entries=Array.FindAll(IslandExpansionCatalog.Recipes,r=>Match(r.name)&&(buildingFilter==""||r.building==buildingFilter));
            GUILayout.BeginHorizontal();GUILayout.Label(buildingFilter==""?"Tất cả công thức":"Tại "+IslandExpansionCatalog.Name(buildingFilter),body);if(buildingFilter!=""&&Button("Xem tất cả")){buildingFilter="";page=0;}GUILayout.EndHorizontal();Pager(entries.Length);
            for(int i=page*6;i<Math.Min(entries.Length,(page+1)*6);i++)
            {
                var r=entries[i];string input="";for(int n=0;n<r.inputs.Length;n++)input+=(n>0?" + ":"")+r.amounts[n]+" "+IslandExpansionCatalog.Name(r.inputs[n])+" (có "+game.Model.IslandCount(r.inputs[n])+")";
                string blocker=game.Model.IslandRecipeBlocker(r.id);GUILayout.BeginHorizontal(GUI.skin.box);GUILayout.BeginVertical();
                GUILayout.Label(r.name+" ×"+r.count+" · "+r.seconds+" giây · "+IslandExpansionCatalog.Name(r.building),body);GUILayout.Label(input,small);if(blocker!="")GUILayout.Label(blocker,small);GUILayout.EndVertical();
                GUI.enabled=blocker=="";if(Button("Bắt đầu mẻ"))Result(game.Model.StartIslandRecipe(r.id,game.Now));GUI.enabled=true;GUILayout.EndHorizontal();
            }
        }
        private void DrawOwned()
        {
            var entries=game.Model.Island.owned.FindAll(p=>Match(IslandExpansionCatalog.Name(p.id)));Pager(entries.Count);
            for(int i=page*6;i<Math.Min(entries.Count,(page+1)*6);i++)
            {
                var p=entries[i];var s=IslandExpansionCatalog.Thing(p.id);GUILayout.BeginHorizontal(GUI.skin.box);Icon(s.id);GUILayout.BeginVertical();
                GUILayout.Label(s.name+(s.Building?" · cấp "+p.level:""),body);GUILayout.Label(p.placed?"Trên đảo · "+p.x+", "+p.z:"Trong kho sở hữu",small);GUILayout.EndVertical();
                if(Button(p.placed?"Di chuyển":"Đặt",120))BeginPlacement(p.serial);
                if(p.placed&&Button("Cất",70))Result(game.Model.StoreIslandThing(p.serial),true);
                if(s.Building&&p.placed)
                {
                    if(Button("Mở nghề",95)){buildingFilter=s.id;tab=int.Parse(s.id.Substring(1))>=16?6:2;page=0;}
                    if(p.level<3&&Button("Nâng "+FarmModel.Money(s.price*(p.level+1)),140))Result(game.Model.UpgradeIslandBuilding(s.id),true);
                }
                GUILayout.EndHorizontal();
            }
        }
        private void DrawStock()
        {
            GUILayout.Label("Nhà cung cấp bán vật tư; thành phẩm có thể bán hoặc nhập bếp nếu phù hợp.",body);
            GUILayout.BeginHorizontal();GUILayout.Label("Số lượng",body);if(Button("−",45))quantity=Mathf.Max(1,quantity-1);GUILayout.Label(quantity.ToString(),body,GUILayout.Width(40));if(Button("+",45))quantity=Mathf.Min(100,quantity+1);if(Button("10",50))quantity=10;GUILayout.EndHorizontal();
            var entries=Array.FindAll(IslandExpansionCatalog.Items,s=>Match(s.name));Pager(entries.Length);
            for(int i=page*6;i<Math.Min(entries.Length,(page+1)*6);i++)
            {
                var s=entries[i];GUILayout.BeginHorizontal(GUI.skin.box);GUILayout.Label(s.name+" · có "+game.Model.IslandCount(s.id),body);
                if(s.buy>0&&Button("Mua "+quantity+" · "+FarmModel.Money(s.buy*quantity),210))Result(game.Model.BuyIslandSupply(s.id,quantity));
                if(s.sell>0&&Button("Bán "+quantity+" · "+FarmModel.Money(s.sell*quantity),210))Result(game.Model.SellIslandGoods(s.id,quantity));
                if(s.edible&&s.id!="ix_salt"&&Button("Nhập bếp",120))Result(game.Model.ImportIslandKitchen(s.id,quantity));
                GUILayout.EndHorizontal();
            }
        }
        private void DrawServices()
        {
            GUILayout.BeginHorizontal();GUILayout.Label("Chọn công trình dịch vụ đã đặt:",body);
            if(Button("←",55))CycleService(-1);GUILayout.Label(IslandExpansionCatalog.Name(buildingFilter),body);if(Button("→",55))CycleService(1);GUILayout.EndHorizontal();
            if(buildingFilter==""||!game.Model.HasIslandBuilding(buildingFilter)){GUILayout.Label("Mua và đặt một công trình từ thư viện hạt giống đến trạm xe đạp. Mỗi nơi có hoạt động riêng.",body);return;}
            var spec=IslandExpansionCatalog.Thing(buildingFilter);GUILayout.Label(spec.name,title);GUILayout.Label(spec.description,body);
            if(Button("Nhìn công trình",200)){IsOpen=false;FocusBuilding(buildingFilter);}GUILayout.Space(8);
            if(buildingFilter=="B16")
            {CropSelector();if(Button("Nộp một mẫu thường ★",250))Result(game.Model.RegisterIslandSeed(selectedCrop));}
            else if(buildingFilter=="B17")
            {PlotSelector();GUILayout.Label(game.Model.IslandSoilReport(selectedPlot),body);}
            else if(buildingFilter=="B18")
            {GUILayout.Label("Còn "+game.Model.Island.waterJobs+" lượt. Cứ 20 giây khi chơi, trạm tưới một ô chưa tưới. Không tưới cây chín, không làm bù khi vắng mặt.",body);if(Button("5 lượt · 5 xu",220))Result(game.Model.ScheduleIslandWater(true,game.Now));if(Button("Dừng lịch",220))Result(game.Model.ScheduleIslandWater(false,game.Now));}
            else if(buildingFilter=="B19")
            {PlotSelector();GUILayout.Label("Có "+game.Model.IslandCount("ix_helpers")+" đợt thiên địch. Làm thêm ở tab Làm nghề.",body);if(Button("Thả vào ô đã chọn",220))Result(game.Model.ReleaseIslandHelpers(selectedPlot));}
            else if(buildingFilter=="B20")
            {PlotSelector();GUILayout.Label("Tỉa khi cây lâu năm đang tái quả: đợt hiện tại bắt đầu lại, bù +1 sản phẩm lần thu tới. Chỉ tỉa một lần/đợt.",body);if(Button("Tỉa tán cây",220))Result(game.Model.PruneIslandTree(selectedPlot,game.Now));}
            else if(buildingFilter=="B22")
            {GUILayout.Label("Đang trưng bày: "+StoryName(game.Model.Island.exhibition),body);foreach(string key in game.Model.Island.journal.ToArray())if(Button(StoryName(key),680))Result(game.Model.SetIslandExhibition(key));}
            else if(buildingFilter=="B23")DrawLessons();
            else if(buildingFilter=="B25")
            {GUILayout.Label("Chải/làm sạch miễn phí; vật nuôi nghỉ 120 giây giữa các lượt. Chăm sóc không thay thế thức ăn hoặc thuốc.",body);foreach(string animal in new[]{"chicken","cow","pig","duck"})if(Button(animal=="chicken"?"Chăm gà":animal=="cow"?"Chăm bò":animal=="pig"?"Chăm heo":"Chăm vịt",200))Result(game.Model.BrushIslandAnimal(animal,game.Now));}
            else if(buildingFilter=="B28")
            {
                CropSelector();GUILayout.BeginHorizontal();GUILayout.Label("Phẩm chất "+quality+" sao · lượng "+quantity,body);if(Button("Đổi sao",120))quality=quality%3+1;if(Button("−",45))quantity=Mathf.Max(1,quantity-1);if(Button("+",45))quantity=Mathf.Min(100,quantity+1);GUILayout.EndHorizontal();
                if(Button("Để riêng lô nông sản",260))Result(game.Model.ReserveIslandCrop(selectedCrop,quality,quantity));
                foreach(var r in game.Model.Island.reserved.ToArray()){GUILayout.BeginHorizontal();GUILayout.Label(r.count+" "+CropCatalog.Find(r.crop).displayName+" · "+r.stars+" sao",body);if(Button("Trả về kho",140))Result(game.Model.UnreserveIslandCrop(r.serial));GUILayout.EndHorizontal();}
            }
            else
            {
                string[] choices=buildingFilter=="B21"?new[]{"Sân chung","Góc nghỉ","Ngày góp hàng"}:buildingFilter=="B24"?new[]{"Chiều đờn ca","Đêm mùa vàng","Hát giữa vườn"}:buildingFilter=="B26"?new[]{"Sớm cà phê","Chiều ca cao","Một ngày trong vườn"}:buildingFilter=="B27"?new[]{"Chim non lạc tổ","Chim mỏi cánh","Ngày trở về"}:buildingFilter=="B29"?new[]{"Thư người bạn xa","Gửi quà cô giáo","Chúc mùa vàng"}:new[]{"Vòng làng nghề","Đường hoa","Bờ nước"};
                serviceChoice=GUILayout.SelectionGrid(serviceChoice,choices,3,button,GUILayout.Height(46));
                GUILayout.Label(buildingFilter=="B21"?"Chuẩn bị 6 lúa + 1 chiếu cói.":buildingFilter=="B24"?"Chuẩn bị 2 bún gạo + 2 kẹo lạc.":buildingFilter=="B26"?"Chuẩn bị 1 "+(serviceChoice==0?"cà phê rang":"thanh ca cao")+" + 1 quà kẹo; trang trí ít nhất 3 món.":buildingFilter=="B27"?"Chuẩn bị 1 bộ chăm sóc từ nhà cung cấp. Không bắt hoặc bán chim.":buildingFilter=="B29"?"Gửi 1 "+(serviceChoice==0?"quà cà phê và mít":"quà kẹo vườn")+" cho NPC.":"Nối ít nhất 3 công trình đã đặt. Chuyến đi không thu phí, không trả xu.",body);
                GUILayout.Label("90 giây · hoàn thành ở tab Làm nghề. Mỗi câu chuyện chỉ tặng đồ lần đầu, các lượt sau lưu số lần trải nghiệm.",body);
                if(Button("Bắt đầu trải nghiệm",270))Result(game.Model.StartIslandService(buildingFilter,serviceChoice,game.Model.Island.serviceSerial,game.Now));
            }
        }
        private void CycleService(int direction)
        {var list=game.Model.Island.owned.FindAll(p=>p.placed&&p.id.StartsWith("B")&&int.Parse(p.id.Substring(1))>=16);if(list.Count==0)return;int index=list.FindIndex(p=>p.id==buildingFilter);buildingFilter=list[(index+direction+list.Count)%list.Count].id;serviceChoice=0;}
        private void CropSelector()
        {GUILayout.BeginHorizontal();if(Button("Giống trước",140)){int i=Array.FindIndex(CropCatalog.All,c=>c.id==selectedCrop);selectedCrop=CropCatalog.All[(i+CropCatalog.All.Length-1)%CropCatalog.All.Length].id;}GUILayout.Label(CropCatalog.Find(selectedCrop).displayName+" · thường ★: "+game.Model.Count(selectedCrop),body);if(Button("Giống sau",140)){int i=Array.FindIndex(CropCatalog.All,c=>c.id==selectedCrop);selectedCrop=CropCatalog.All[(i+1)%CropCatalog.All.Length].id;}GUILayout.EndHorizontal();}
        private void PlotSelector()
        {GUILayout.BeginHorizontal();if(Button("Ô trước",120))selectedPlot=(selectedPlot+game.Model.State.plots.Count-1)%game.Model.State.plots.Count;GUILayout.Label("Ô "+(selectedPlot+1)+" · "+(CropCatalog.Find(game.Model.State.plots[selectedPlot].cropId)?.displayName??"đất trống"),body);if(Button("Ô sau",120))selectedPlot=(selectedPlot+1)%game.Model.State.plots.Count;GUILayout.EndHorizontal();}
        private void DrawLessons()
        {
            int n=game.Model.Island.lesson;if(n>=3){GUILayout.Label("Đã hoàn thành ba bài: luân canh, thoát nước, giữ hàng. Các bài mô phỏng không trừ xu/cây trên farm.",body);return;}
            string[] questions={"Ô vừa thu khoai. Vụ kế tiếp nào phù hợp luân canh?","Đất đang úng, bạn muốn gieo khoai. Làm gì trước?","Sắp giao đơn cần lạc. Làm sao giữ hàng khi bán kho?"};
            string[][] answers={new[]{"Trồng khoai tiếp","Chuyển sang đậu","Chọn cùng nhóm củ"},new[]{"Gieo ngay","Tưới thêm","Thoát nước trước"},new[]{"Để riêng đúng lô và sao","Bán toàn bộ","Chuyển hàng lên ba sao"}};
            GUILayout.Label(questions[n],body);for(int a=0;a<3;a++)if(Button(answers[n][a],600))Result(game.Model.AnswerIslandLesson(n,a));
        }
        private string StoryName(string key)
        {
            if(string.IsNullOrEmpty(key))return "Chưa chọn";var parts=key.Split(':');
            if(parts[0]=="seed")return "Hồ sơ "+CropCatalog.Find(parts[1]).displayName;
            if(parts[0]=="lesson")return "Bài học nhà nông "+(int.Parse(parts[1])+1);
            if(parts[0]=="care")return "Nhật ký chăm "+(parts[1]=="cow"?"bò":parts[1]=="chicken"?"gà":parts[1]=="pig"?"heo":"vịt");
            return IslandExpansionCatalog.Name(parts[0])+" · câu chuyện "+(parts.Length>1?(int.Parse(parts[1])+1).ToString():"");
        }
    }
}
