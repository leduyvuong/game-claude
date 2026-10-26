using System;
using System.Collections.Generic;
namespace HarvestIsle.Core
{
    [Serializable] public sealed class IslandPlacement
    {
        public int serial,turn,host,level=1;public string id;public bool placed;public float x,z;
    }
    [Serializable] public sealed class IslandStock {public string id;public int count;}
    [Serializable] public sealed class IslandJob {public int serial;public string recipe;public double readyAt;}
    [Serializable] public sealed class IslandReservation {public int serial,stars,count;public string crop;}
    [Serializable] public sealed class IslandPruning {public int plot;public string crop;public double plantedAt;}
    [Serializable] public sealed class IslandExpansionState
    {
        public int chapter,completed,nextSerial=1,lesson,waterJobs,waterCursor,serviceSerial;
        public double nextWaterAt,careAt;
        public string exhibition="";
        public List<IslandPlacement> owned=new List<IslandPlacement>();
        public List<IslandStock> goods=new List<IslandStock>();
        public List<IslandJob> jobs=new List<IslandJob>();
        public List<string> journal=new List<string>();
        public List<IslandReservation> reserved=new List<IslandReservation>();
        public List<IslandPruning> pruning=new List<IslandPruning>();
    }
    public sealed partial class FarmModel
    {
        public IslandExpansionState Island {get{return State.islandExpansion;}}
        private void InitializeIslandExpansion()
        {
            if(State.islandExpansion==null)State.islandExpansion=new IslandExpansionState();var e=Island;
            if(e.nextSerial==0)e.nextSerial=1;
            if(e.owned==null)e.owned=new List<IslandPlacement>();if(e.goods==null)e.goods=new List<IslandStock>();
            if(e.jobs==null)e.jobs=new List<IslandJob>();if(e.journal==null)e.journal=new List<string>();
            if(e.reserved==null)e.reserved=new List<IslandReservation>();if(e.pruning==null)e.pruning=new List<IslandPruning>();
        }
        public bool IslandCropUnlocked(string id){return IslandCropCatalog.Chapter(id)<=Island.chapter;}
        public ActionResult BeginIslandChapter()
        {
            int next=Island.chapter+1;if(next>3)return Fail("Đã mở cả ba chương.");
            int level=next==1?3:next==2?7:10,goal=next==1?0:next==2?3:8;
            if(Level<level||Island.completed<goal)return Fail("Cần cấp "+level+" và hoàn thành "+goal+" mẻ nghề/dịch vụ.");
            Island.chapter=next;return Ok("Mở M"+next+" · "+IslandExpansionCatalog.Chapters[next]+". Giống và công thức mới đã sẵn sàng.");
        }
        public int IslandCount(string id)
        {var item=IslandExpansionCatalog.Item(id);if(item==null)return ItemCount(id);var s=Island.goods.Find(x=>x.id==id);return s==null?0:s.count;}
        private void ChangeIslandItem(string id,int delta)
        {
            if(IslandExpansionCatalog.Item(id)==null){ChangeItem(id,delta);return;}
            var s=Island.goods.Find(x=>x.id==id);if(s==null){s=new IslandStock{id=id};Island.goods.Add(s);}s.count+=delta;
        }
        public IslandPlacement IslandBuilding(string id){return Island.owned.Find(x=>x.id==id&&x.placed);}
        public bool HasIslandBuilding(string id){return IslandBuilding(id)!=null;}
        public ActionResult BuyIslandSupply(string id,int count)
        {
            var s=IslandExpansionCatalog.Item(id);if(Island.chapter<1||s==null||s.buy<=0||count<1||count>100)return Fail("Vật tư hoặc số lượng không hợp lệ.");
            long cost=(long)count*s.buy;if(State.coins<cost||IslandCount(id)>1000000-count)return Fail("Thiếu xu hoặc kho đầy.");
            State.coins-=(int)cost;ChangeIslandItem(id,count);return Ok("Đã nhận "+count+" "+s.name+".");
        }
        public ActionResult SellIslandGoods(string id,int count)
        {
            var s=IslandExpansionCatalog.Item(id);if(s==null||s.sell<=0||count<1||count>IslandCount(id))return Fail("Không có hàng để bán.");
            long total=(long)count*s.sell;if(total+State.coins>1000000000)return Fail("Ví đầy.");
            ChangeIslandItem(id,-count);State.coins+=(int)total;return Ok("Đã bán: +"+Money((int)total));
        }
        public ActionResult ImportIslandKitchen(string id,int count)
        {
            var item=IslandExpansionCatalog.Item(id);string target=id=="ix_noodles"?"riceNoodles":id=="ix_soytofu"?"tofu":id;
            if(item==null||!item.edible||HotpotRules.Find(target)==null||count<1||count>1000||IslandCount(id)<count)return Fail("Chọn thực phẩm có đủ trong kho nghề.");
            if(KitchenCount(target)>1000000-count)return Fail("Kho bếp đầy.");
            ChangeIslandItem(id,-count);KitchenAdd(target,1,count);return Ok("Đã nhập "+count+" "+item.name+" vào bếp; giữ riêng tiền và kho còn lại.");
        }
        public ActionResult BuyIslandThing(string id)
        {
            var s=IslandExpansionCatalog.Thing(id);if(s==null||Island.chapter<s.chapter)return Fail("Chưa mở chương của mẫu này.");
            if(s.Building&&Island.owned.Exists(x=>x.id==id))return Fail("Công trình đã có trong kho hoặc trên đảo.");
            if(Island.owned.Count>=300||Island.nextSerial>=1000000000||State.coins<s.price)return Fail("Thiếu xu hoặc đã đủ 300 món.");
            State.coins-=s.price;Island.owned.Add(new IslandPlacement{serial=Island.nextSerial++,id=id});return Ok("Đã mua "+s.name+" · chọn Đặt từ kho sở hữu.");
        }
        public ActionResult UpgradeIslandBuilding(string id)
        {
            var b=IslandBuilding(id);var s=IslandExpansionCatalog.Thing(id);
            if(b==null||!s.Building||b.level>=3)return Fail("Đặt công trình trước, tối đa cấp 3.");
            int cost=s.price*(b.level+1);if(State.coins<cost)return Fail("Cần "+Money(cost));
            State.coins-=cost;b.level++;return Ok("Cấp "+b.level+" · thêm chỗ hàng chờ, mỗi cấp nhanh hơn 10%.");
        }
        public static bool IslandWater(float x,float z){return x>=-40&&x<=-30&&z>=18&&z<=28;}
        public static bool IslandRoad(float x,float z,float w,float d)
        {return Math.Abs(z)<d/2+2||Math.Abs(x-14)<w/2+1.5||Math.Abs(x+14)<w/2+1.5;}
        public static void IslandSize(IslandThingSpec s,int turn,out float w,out float d)
        {w=turn%2==0?s.width:s.depth;d=turn%2==0?s.depth:s.width;}
        public static void IslandMountPosition(IslandPlacement support,int slot,out float x,out float z)
        {
            var s=IslandExpansionCatalog.Thing(support.id);float a=(slot==0?-1:1)*Math.Min(s.width*.28f,1.2f),b=s.depth/2-.4f;
            float dx=support.turn==0?a:support.turn==1?b:support.turn==2?-a:-b;
            float dz=support.turn==0?b:support.turn==1?-a:support.turn==2?-b:a;
            x=(float)Math.Round((support.x+dx)*2)/2;z=(float)Math.Round((support.z+dz)*2)/2;
        }
        public string IslandPlacementBlocker(int serial,float x,float z,int turn,int host=0)
        {return IslandPlacementBlocker(Island,serial,x,z,turn,host);}
        private static string IslandPlacementBlocker(IslandExpansionState Island,int serial,float x,float z,int turn,int host=0)
        {
            var own=Island.owned.Find(p=>p.serial==serial);if(own==null||!IsFinite(x)||!IsFinite(z)||turn<0||turn>3)return "Vị trí không hợp lệ.";
            var s=IslandExpansionCatalog.Thing(own.id);float w,d;IslandSize(s,turn,out w,out d);
            if(Math.Abs(x*2-Math.Round(x*2))>.001||Math.Abs(z*2-Math.Round(z*2))>.001)return "Lưới đặt cách nhau 0,5 m.";
            if(Math.Abs(x)+w/2>40||Math.Abs(z)+d/2>28)return "Ngoài vùng xây của Đảo trăm nghề.";
            if(s.kind=="wall"||s.kind=="table")
            {
                var support=Island.owned.Find(p=>p.serial==host&&p.placed&&IslandExpansionCatalog.Thing(p.id).Building);
                if(support==null)return "Chọn công trình đỡ đồ treo/đồ trên bàn.";
                float sw,sd;IslandSize(IslandExpansionCatalog.Thing(support.id),support.turn,out sw,out sd);
                if(Math.Abs(x-support.x)>sw/2||Math.Abs(z-support.z)>sd/2)return "Đặt trong phạm vi công trình đỡ.";
                float x0,z0,x1,z1;IslandMountPosition(support,0,out x0,out z0);IslandMountPosition(support,1,out x1,out z1);
                if((Math.Abs(x-x0)>.01||Math.Abs(z-z0)>.01)&&(Math.Abs(x-x1)>.01||Math.Abs(z-z1)>.01))return "Dùng Chọn giá đỡ để chọn một trong hai vị trí ở sân trước.";
                if(turn!=support.turn)return "Đồ trên giá xoay cùng công trình đỡ.";
                foreach(var p in Island.owned)if(p.placed&&p.serial!=serial&&p.host==host&&Math.Abs(x-p.x)<.6&&Math.Abs(z-p.z)<.6)return "Ổ gắn đang có đồ.";
                return "";
            }
            if(host!=0)return "Loại này đặt trên nền.";
            bool water=IslandWater(x,z);
            if(s.kind=="water")return water&&x-w/2>=-40&&x+w/2<=-30&&z-d/2>=18&&z+d/2<=28?IslandOverlap(Island,serial,x,z,w,d):"Chỉ đặt trên hồ cảnh phía tây bắc.";
            if(water||x-w/2<-30&&z+d/2>18)return "Hồ cảnh dành cho đồ nổi.";
            if(s.kind=="shore"&&!(x>=-29&&x<=-26&&z>=18||z>=14&&z<=17&&x<=-30))return "Chọn bờ nam hoặc đông của hồ cảnh.";
            if(IslandRoad(x,z,w,d))return "Giữ trống đường đi chính.";
            return IslandOverlap(Island,serial,x,z,w,d);
        }
        private static string IslandOverlap(IslandExpansionState Island,int serial,float x,float z,float w,float d)
        {
            var candidate=IslandExpansionCatalog.Thing(Island.owned.Find(p=>p.serial==serial).id);
            foreach(var p in Island.owned)
            {
                if(!p.placed||p.serial==serial||p.host!=0)continue;var other=IslandExpansionCatalog.Thing(p.id);float pw,pd;IslandSize(other,p.turn,out pw,out pd);
                float space=candidate.Building||other.Building?1.0f:.15f;
                if(Math.Abs(x-p.x)<(w+pw)/2+space&&Math.Abs(z-p.z)<(d+pd)/2+space)return "Chồng đồ hoặc chắn khoảng trống trước cửa.";
            }
            return "";
        }
        public ActionResult PlaceIslandThing(int serial,float x,float z,int turn,int host=0)
        {
            string reason=IslandPlacementBlocker(serial,x,z,turn,host);if(reason!="")return Fail(reason);
            var p=Island.owned.Find(v=>v.serial==serial);
            if(Island.owned.Exists(v=>v.placed&&v.host==serial))return Fail("Cất các đồ đang gắn lên công trình trước khi di chuyển.");
            p.x=x;p.z=z;p.turn=turn;p.host=host;p.placed=true;return Ok("Đã đặt "+IslandExpansionCatalog.Thing(p.id).name+".");
        }
        public ActionResult StoreIslandThing(int serial)
        {
            var p=Island.owned.Find(v=>v.serial==serial);if(p==null||!p.placed)return Fail("Món này đã ở trong kho.");
            if(Island.owned.Exists(v=>v.placed&&v.host==serial))return Fail("Cất đồ treo và đồ trên bàn trước.");
            if(Island.jobs.Exists(j=>JobBuilding(j)==p.id)||p.id=="B18"&&Island.waterJobs>0)return Fail("Thu hết hàng/dịch vụ hoặc dừng lịch tưới trước khi cất.");
            p.placed=false;p.host=0;return Ok("Đã cất, đặt lại không mất xu.");
        }
        public string IslandRecipeBlocker(string id)
        {
            var r=IslandExpansionCatalog.Recipe(id);if(r==null||Island.chapter<r.chapter)return "Chưa mở công thức.";
            int level=r.building.StartsWith("B")?(IslandBuilding(r.building)==null?0:IslandBuilding(r.building).level):BuildingLevel(r.building);
            if(level==0)return "Cần xây và đặt "+IslandExpansionCatalog.Name(r.building)+".";
            if(Island.jobs.FindAll(j=>JobBuilding(j)==r.building).Count>=level)return "Hàng chờ đầy.";
            if(Island.nextSerial>=1000000000)return "Đã đạt giới hạn lượt.";
            for(int i=0;i<r.inputs.Length;i++)if(IslandCount(r.inputs[i])<r.amounts[i])return "Thiếu "+IslandExpansionCatalog.Name(r.inputs[i])+" (chỉ dùng hàng thường ★).";
            return "";
        }
        public ActionResult StartIslandRecipe(string id,double now)
        {
            if(!IsFinite(now)||now<0)return Fail("Thời gian không hợp lệ.");string reason=IslandRecipeBlocker(id);if(reason!="")return Fail(reason);
            var r=IslandExpansionCatalog.Recipe(id);int level=r.building.StartsWith("B")?IslandBuilding(r.building).level:BuildingLevel(r.building);
            for(int i=0;i<r.inputs.Length;i++)ChangeIslandItem(r.inputs[i],-r.amounts[i]);
            Island.jobs.Add(new IslandJob{serial=Island.nextSerial++,recipe=id,readyAt=ObserveTime(now)+r.seconds*(1-.1*(level-1))});
            return Ok("Đang làm "+r.name+". Nguyên liệu đã trừ một lần.");
        }
        private static string JobBuilding(IslandJob job)
        {var r=IslandExpansionCatalog.Recipe(job.recipe);return r==null?job.recipe.Split(':')[0]:r.building;}
        public ActionResult CollectIslandJob(int serial,double now)
        {
            var j=Island.jobs.Find(x=>x.serial==serial);if(j==null)return Fail("Lượt này đã được nhận.");
            if(!IsFinite(now)||now<0||ObserveTime(now)<j.readyAt)return Fail("Chưa hoàn thành.");
            var r=IslandExpansionCatalog.Recipe(j.recipe);
            if(r!=null)
            {
                var thing=IslandExpansionCatalog.Thing(r.output);
                if(thing!=null){if(Island.owned.Count>=300||Island.nextSerial>=1000000000)return Fail("Kho đồ đầy.");Island.owned.Add(new IslandPlacement{serial=Island.nextSerial++,id=r.output});}
                else {if(IslandCount(r.output)>1000000-r.count)return Fail("Kho hàng đầy.");ChangeIslandItem(r.output,r.count);}
            }
            else
            {
                string building=JobBuilding(j),key=j.recipe;
                string reward=building=="B21"?"D54":building=="B24"?"D53":building=="B26"?"D32":building=="B27"?"D34":building=="B29"?"D58":"D59";
                if(!Island.journal.Contains(key))
                {
                    if(Island.owned.Count>=300||Island.nextSerial>=1000000000)return Fail("Cất bớt lượt chế tác, kho đủ 300 món.");
                    Island.journal.Add(key);Island.owned.Add(new IslandPlacement{serial=Island.nextSerial++,id=reward});
                }
            }
            Island.jobs.Remove(j);Island.completed=Math.Min(1000000000,Island.completed+1);GainExperience(8);
            return Ok(r!=null?"Nhận "+r.count+" "+IslandExpansionCatalog.Name(r.output):"Đã lưu câu chuyện · quà lần đầu vào Kho sở hữu.");
        }
        public ActionResult RegisterIslandSeed(string id)
        {
            if(!HasIslandBuilding("B16")||IslandCropCatalog.Index(id)<0||!IslandCropUnlocked(id))return Fail("Đặt thư viện và mở giống trước.");
            string key="seed:"+id;if(Island.journal.Contains(key))return Fail("Đã có hồ sơ giống này.");if(Count(id)<1)return Fail("Cần một nông sản thường ★ tự trồng.");
            ChangeItem(id,-1);Island.journal.Add(key);GainExperience(5);return Ok("Đã ghi hồ sơ "+CropCatalog.Find(id).displayName+".");
        }
        public string IslandSoilReport(int index)
        {
            if(!HasIslandBuilding("B17")||index<0||index>=State.plots.Count)return "Đặt trạm kiểm đất và chọn một ô ruộng.";
            var p=State.plots[index];return "Ô "+(index+1)+" · độ phì "+p.soilQuality+"/2 · nước "+(int)(p.soilWater*100)+"%\nHai vụ trước: "+CropGroup(p.previousCropId)+" / "+CropGroup(p.secondPreviousCropId)+".\nChọn nhóm khác vụ trước để luân canh; phân ủ cải thiện đất. Trạm chỉ phân tích, không tự tăng độ phì.";
        }
        public ActionResult ScheduleIslandWater(bool start,double now)
        {
            if(!HasIslandBuilding("B18")||!IsFinite(now)||now<0)return Fail("Đặt trạm phối nước tưới trước.");
            if(!start){Island.waterJobs=0;return Ok("Đã dừng lịch tưới. Không tự gia hạn.");}
            if(Island.waterJobs>0)return Fail("Lịch hiện tại chưa hết.");if(State.coins<5)return Fail("Cần 5 xu cho 5 lượt tưới.");
            State.coins-=5;Island.waterJobs=5;Island.nextWaterAt=ObserveTime(now);return Ok("5 lượt tưới · chỉ chạy khi đang chơi, không làm bù offline.");
        }
        public bool TickIslandWater(double now)
        {
            if(!IsFinite(now)||now<Island.nextWaterAt||Island.waterJobs==0||!HasIslandBuilding("B18"))return false;
            for(int n=0;n<State.plots.Count;n++)
            {int i=(Island.waterCursor+n)%State.plots.Count;var p=State.plots[i];if(string.IsNullOrEmpty(p.cropId)||p.watered||Progress(i,now)>=1)continue;
                if(Apply(i,FarmTool.Water,p.cropId,now).success){Island.waterJobs--;Island.waterCursor=(i+1)%State.plots.Count;Island.nextWaterAt=now+20;return true;}}
            Island.nextWaterAt=now+20;return false;
        }
        public ActionResult ReleaseIslandHelpers(int index)
        {
            if(!HasIslandBuilding("B19")||index<0||index>=State.plots.Count||string.IsNullOrEmpty(State.plots[index].cropId)||IslandCount("ix_helpers")<1)return Fail("Cần một đợt thiên địch và chọn ô có cây.");
            var p=State.plots[index];if(p.treatmentProtection>=45)return Fail("Ô đang được bảo vệ.");
            ChangeIslandItem("ix_helpers",-1);p.treatmentProtection=45;return Ok("Bảo vệ ô trong 45 giây sinh thái; không chữa bệnh đã có.");
        }
        public ActionResult PruneIslandTree(int index,double now)
        {
            if(!HasIslandBuilding("B20")||index<0||index>=State.plots.Count||!IsFinite(now)||now<0)return Fail("Đặt trạm chăm tán và chọn cây.");
            var p=State.plots[index];if(!IsPerennial(p.cropId)||p.harvests<1||Progress(index,now)>=1||Island.pruning.Exists(v=>v.plot==index))return Fail("Chọn cây lâu năm đang tái quả, chưa tỉa đợt này.");
            p.plantedAt=ObserveTime(now);Island.pruning.Add(new IslandPruning{plot=index,crop=p.cropId,plantedAt=p.plantedAt});
            return Ok("Tỉa tán: bắt đầu lại đợt quả hiện tại, bù +1 sản phẩm lần thu tới. Không cộng dồn.");
        }
        private int IslandPruneBonus(int index)
        {var p=State.plots[index];return Island.pruning.Exists(v=>v.plot==index&&v.crop==p.cropId&&v.plantedAt==p.plantedAt)?1:0;}
        public ActionResult SetIslandExhibition(string key)
        {
            if(!HasIslandBuilding("B22")||!Island.journal.Contains(key))return Fail("Chọn một trang đã có trong nhật ký.");
            Island.exhibition=key;return Ok("Đã trưng bày bản sao câu chuyện. Bản gốc vẫn giữ trong nhật ký.");
        }
        public ActionResult AnswerIslandLesson(int serial,int answer)
        {
            if(!HasIslandBuilding("B23")||serial!=Island.lesson||serial>=3)return Fail("Bài học đã đổi hoặc đã hoàn thành.");
            int[] correct={1,2,0};if(answer!=correct[serial])return Fail("Thử lại: luân canh đổi nhóm, thoát úng trước gieo, giữ lại hàng cho đơn.");
            Island.lesson++;Island.journal.Add("lesson:"+serial);GainExperience(10);return Ok("Đúng! Đã ghi bài học, không chạm cây trên ruộng thật.");
        }
        public ActionResult BrushIslandAnimal(string id,double now)
        {
            if(!HasIslandBuilding("B25")||!IsFinite(now)||now<0)return Fail("Đặt trạm chăm thú trước.");
            var a=id=="chicken"?State.chicken:id=="cow"?State.cow:id=="pig"?State.pig:id=="duck"?State.duck:null;
            if(a==null||a.count==0)return Fail("Chọn vật nuôi đang có.");if(now<Island.careAt)return Fail("Bạn nhỏ đang nghỉ, quay lại sau.");
            Island.careAt=ObserveTime(now)+120;string key="care:"+id;if(!Island.journal.Contains(key))Island.journal.Add(key);return Ok("Đã chải và làm sạch · vẫn cần cho ăn theo lịch chuồng.");
        }
        public ActionResult ReserveIslandCrop(string id,int stars,int count)
        {
            if(!HasIslandBuilding("B28")||CropCatalog.Find(id)==null||stars<1||stars>3||count<1||count>1000||StockCount(id,stars)<count||Island.reserved.Count>=90||Island.nextSerial>=1000000000)return Fail("Chọn loại, sao và lượng thực có trong kho.");
            if(stars==1)ChangeItem(id,-count);else Bucket(stars==2?State.fineGoods:State.primeGoods,id).count-=count;
            Island.reserved.Add(new IslandReservation{serial=Island.nextSerial++,crop=id,stars=stars,count=count});return Ok("Đã để riêng, giữ nguyên số lượng và sao; không bị Bán tất cả lấy đi.");
        }
        public ActionResult UnreserveIslandCrop(int serial)
        {
            var r=Island.reserved.Find(x=>x.serial==serial);if(r==null)return Fail("Lô đã được trả.");if(StockCount(r.crop,r.stars)>10000000-r.count)return Fail("Kho đầy.");
            if(r.stars==1)ChangeItem(r.crop,r.count);else Bucket(r.stars==2?State.fineGoods:State.primeGoods,r.crop).count+=r.count;
            Island.reserved.Remove(r);return Ok("Đã trả lô về đúng kho phẩm chất.");
        }
        public ActionResult StartIslandService(string building,int choice,int serial,double now)
        {
            if(!HasIslandBuilding(building)||choice<0||choice>2||serial!=Island.serviceSerial||Island.serviceSerial>=1000000000||Island.nextSerial>=1000000000||!IsFinite(now)||now<0)return Fail("Lựa chọn dịch vụ không hợp lệ hoặc đã gửi.");
            if(building!="B21"&&building!="B24"&&building!="B26"&&building!="B27"&&building!="B29"&&building!="B30")return Fail("Công trình này có thao tác riêng.");
            if(Island.jobs.Exists(j=>JobBuilding(j)==building))return Fail("Hoàn thành lượt đang chờ trước.");
            string[] inputs;int[] amounts;
            if(building=="B21"){inputs=new[]{"rice","ix_mat"};amounts=new[]{6,1};}
            else if(building=="B24"){inputs=new[]{"ix_noodles","ix_peanutcandy"};amounts=new[]{2,2};}
            else if(building=="B26"){inputs=new[]{choice==0?"ix_coffee":"ix_chocolate","ix_giftcandy"};amounts=new[]{1,1};if(Island.owned.FindAll(p=>p.placed&&!p.id.StartsWith("B")).Count<3)return Fail("Trang trí ít nhất ba món để đón khách.");}
            else if(building=="B27"){inputs=new[]{"ix_carekit"};amounts=new[]{1};}
            else if(building=="B29"){inputs=new[]{choice==0?"ix_giftcoffee":"ix_giftcandy"};amounts=new[]{1};}
            else {inputs=new string[0];amounts=new int[0];if(Island.owned.FindAll(p=>p.placed&&p.id.StartsWith("B")).Count<3)return Fail("Đặt ít nhất ba điểm công trình để nối tuyến xe đạp.");}
            for(int i=0;i<inputs.Length;i++)if(IslandCount(inputs[i])<amounts[i])return Fail("Cần "+amounts[i]+" "+IslandExpansionCatalog.Name(inputs[i])+".");
            for(int i=0;i<inputs.Length;i++)ChangeIslandItem(inputs[i],-amounts[i]);
            Island.serviceSerial++;Island.jobs.Add(new IslandJob{serial=Island.nextSerial++,recipe=building+":"+choice,readyAt=ObserveTime(now)+90});return Ok("Đã chuẩn bị lượt trải nghiệm · nhận kết quả sau 90 giây, không mất khi vắng mặt.");
        }
        private static bool ValidIslandExpansion(IslandExpansionState e,FarmState state)
        {
            if(e==null)return true;
            double now=state.lastSeenUtc;
            if(e.chapter<0||e.chapter>3||e.completed<0||e.completed>1000000000||e.nextSerial<0||e.nextSerial>1000000000||e.lesson<0||e.lesson>3||e.waterJobs<0||e.waterJobs>5||e.waterCursor<0||e.waterCursor>=MaxPlots||e.serviceSerial<0||e.serviceSerial>1000000000||!IsFinite(e.nextWaterAt)||e.nextWaterAt<0||e.nextWaterAt>now+86400||!IsFinite(e.careAt)||e.careAt<0||e.careAt>now+86400)return false;
            var ids=new HashSet<int>();var buildings=new HashSet<string>();
            if(e.owned!=null){if(e.owned.Count>300)return false;foreach(var p in e.owned){var s=p==null?null:IslandExpansionCatalog.Thing(p.id);if(s==null||p.serial<1||p.serial>=e.nextSerial||!ids.Add(p.serial)||p.level<1||p.level>3||!s.Building&&p.level!=1||p.turn<0||p.turn>3||p.host<0||p.host==p.serial||!IsFinite(p.x)||!IsFinite(p.z)||Math.Abs(p.x)>40||Math.Abs(p.z)>28||s.Building&&!buildings.Add(s.id))return false;}}
            if(e.owned!=null)foreach(var p in e.owned)if(p.placed&&IslandPlacementBlocker(e,p.serial,p.x,p.z,p.turn,p.host)!="")return false;
            if(e.waterJobs>0&&(e.owned==null||!e.owned.Exists(p=>p.id=="B18"&&p.placed)))return false;
            var stock=new HashSet<string>();if(e.goods!=null){if(e.goods.Count>IslandExpansionCatalog.Items.Length)return false;foreach(var s in e.goods)if(s==null||IslandExpansionCatalog.Item(s.id)==null||s.count<0||s.count>1000000||!stock.Add(s.id))return false;}
            var counts=new Dictionary<string,int>();
            if(e.jobs!=null)
            {
                if(e.jobs.Count>120)return false;
                foreach(var j in e.jobs)
                {
                    if(j==null||j.serial<1||j.serial>=e.nextSerial||!ids.Add(j.serial)||!IsFinite(j.readyAt)||j.readyAt<0||j.readyAt>now+86400||string.IsNullOrEmpty(j.recipe))return false;
                    var r=IslandExpansionCatalog.Recipe(j.recipe);string b=JobBuilding(j);
                    if(r==null&&(!b.StartsWith("B")||!ValidIslandStory(j.recipe)))return false;
                    var placed=e.owned==null?null:e.owned.Find(p=>p.id==b&&p.placed);
                    var old=state.development==null||state.development.buildings==null?null:state.development.buildings.Find(p=>p.id==b);
                    int capacity=b.StartsWith("B")?(placed==null?0:placed.level):(old==null?0:old.level);
                    if(r==null)capacity=Math.Min(capacity,1);
                    if(!counts.ContainsKey(b))counts[b]=0;if(++counts[b]>capacity)return false;
                }
            }
            stock.Clear();if(e.journal!=null){if(e.journal.Count>100)return false;foreach(var s in e.journal)if(!ValidIslandStory(s)||!stock.Add(s))return false;}
            if(!string.IsNullOrEmpty(e.exhibition)&&(e.journal==null||!e.journal.Contains(e.exhibition)))return false;
            if(e.reserved!=null){if(e.reserved.Count>90)return false;foreach(var r in e.reserved)if(r==null||r.serial<1||r.serial>=e.nextSerial||!ids.Add(r.serial)||CropCatalog.Find(r.crop)==null||r.stars<1||r.stars>3||r.count<1||r.count>1000)return false;}
            var plots=new HashSet<int>();if(e.pruning!=null){if(e.pruning.Count>MaxPlots)return false;foreach(var p in e.pruning)if(p==null||p.plot<0||p.plot>=state.plots.Count||!plots.Add(p.plot)||!IsPerennial(p.crop)||!IsFinite(p.plantedAt)||p.plantedAt<0||p.plantedAt>now||state.plots[p.plot].cropId!=p.crop||state.plots[p.plot].plantedAt!=p.plantedAt)return false;}
            return true;
        }
        private static bool ValidIslandStory(string key)
        {
            if(string.IsNullOrEmpty(key)||key.Length>80)return false;var parts=key.Split(':');if(parts.Length!=2)return false;
            if(parts[0]=="seed")return IslandCropCatalog.Index(parts[1])>=0;
            if(parts[0]=="care")return parts[1]=="chicken"||parts[1]=="cow"||parts[1]=="pig"||parts[1]=="duck";
            if(parts[1]!="0"&&parts[1]!="1"&&parts[1]!="2")return false;
            return parts[0]=="lesson"||parts[0]=="B21"||parts[0]=="B24"||parts[0]=="B26"||parts[0]=="B27"||parts[0]=="B29"||parts[0]=="B30";
        }
    }
}
