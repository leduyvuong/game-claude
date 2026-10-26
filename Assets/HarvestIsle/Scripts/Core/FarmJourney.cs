using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    [Serializable] public sealed class JourneyRecord { public string id; public int count,perfect; }
    [Serializable] public sealed class JourneyContract
    {
        public int serial,kind,quantity,target,baseline,reward,medals;
        public string item;
    }
    [Serializable] public sealed class JourneyState
    {
        public int completed,seasons,medals,nextSerial=1;
        public int workshop,nursery,harbor;
        public int legacyRank;
        public List<JourneyRecord> crops=new List<JourneyRecord>();
        public List<JourneyRecord> products=new List<JourneyRecord>();
        public JourneyContract active;
    }

    public sealed partial class FarmModel
    {
        public static readonly string[] JourneySeasons={"Mầm xanh","Nắng vàng","Mùa quả","Bếp ấm"};
        public static readonly string[] JourneyProjects={"Xưởng làng","Vườn giống","Bến giao thương"};
        public int JourneyYear {get{return State.journey.seasons/4+1;}}
        public int JourneySeasonProgress {get{return State.journey.completed-State.journey.seasons*12;}}
        public int JourneyRank {get{return 1+State.journey.completed/12;}}
        public int JourneyProjectLevel(int kind){return kind==0?State.journey.workshop:kind==1?State.journey.nursery:State.journey.harbor;}
        public int JourneyProjectCost(int kind){int next=JourneyProjectLevel(kind)+1;return 300*next*next;}
        public int JourneyProjectMedals(int kind){return 12*(JourneyProjectLevel(kind)+1);}
        public int JourneyLegacyCoins {get{return 1000+Math.Min(100,State.journey.legacyRank)*250;}}
        public int JourneyLegacyMedals {get{return 24+Math.Min(20,State.journey.legacyRank)*2;}}
        public double ProductionDuration(RecipeSpec recipe,int level){return recipe.seconds/(1+.25*Math.Max(0,level-1))*(1-.04*State.journey.workshop);}
        public double NurseryDuration(CropSpec crop){return crop.growSeconds*.25*(1-.04*State.journey.nursery);}
        public int CropHarvests(string id){var r=State.journey.crops.Find(x=>x.id==id);return r==null?0:r.count;}
        public int CropMastery(string id)
        {int n=CropHarvests(id);return n>=1000?5:n>=400?4:n>=150?3:n>=50?2:n>=10?1:0;}
        public static int MasteryGoal(int rank){int[] goals={10,50,150,400,1000};return goals[Math.Min(4,Math.Max(0,rank))];}
        private void RecordJourneyCrop(string id,int stars)
        {
            var r=State.journey.crops.Find(x=>x.id==id);
            if(r==null){r=new JourneyRecord{id=id};State.journey.crops.Add(r);}
            r.count=Math.Min(1000000000,r.count+1);if(stars==3)r.perfect=Math.Min(r.count,r.perfect+1);
        }
        private void RecordJourneyProduct(string id)
        {
            var r=State.journey.products.Find(x=>x.id==id);
            if(r==null){r=new JourneyRecord{id=id};State.journey.products.Add(r);}
            r.count=Math.Min(1000000000,r.count+1);
        }
        private int JourneyCounter(JourneyContract c)
        {
            if(c.kind==3)return State.hotpotServed;
            var r=(c.kind==1?State.journey.products:State.journey.crops).Find(x=>x.id==c.item);
            return r==null?0:c.kind==2?r.perfect:r.count;
        }
        private bool JourneyIngredientAvailable(string id,int depth)
        {
            if(id=="")return true;
            var crop=CropCatalog.Find(id);if(crop!=null)return crop.unlockLevel<=Level&&IslandCropUnlocked(id);
            if(id=="milk")return State.cow.count>0;
            if(id=="fish")return State.fish.count>0;
            if(id=="honey")return State.bee.count>0;
            if(id=="eggs")return State.chicken.count>0||State.duck.count>0;
            var recipe=DevelopmentCatalog.Recipe(id);
            return recipe!=null&&depth<4&&BuildingLevel(recipe.building)>0&&JourneyIngredientAvailable(recipe.first,depth+1)&&JourneyIngredientAvailable(recipe.second,depth+1);
        }
        // Offers are reproducible, unlocked, and only scale to a bounded workload.
        // Seasons advance through play, never through a wall-clock deadline.
        public JourneyContract JourneyOffer(int slot)
        {
            if(slot<0||slot>2||JourneySeasonProgress>=12)return null;
            int season=State.journey.seasons%4;
            var unlocked=new List<CropSpec>();var themed=new List<CropSpec>();
            foreach(var c in CropCatalog.All)
            {
                if(c.unlockLevel>Level||!IslandCropUnlocked(c.id))continue;unlocked.Add(c);
                string group=CropGroup(c.id);
                if(season==0&&(group=="Lá"||group=="Đậu")||season==1&&(c.id=="rice"||c.id=="wheat"||c.id=="corn"||c.id=="sugarcane")||season==2&&group=="Quả"||season==3&&(group=="Củ"||c.id=="chili"||c.id=="lemongrass"))themed.Add(c);
            }
            if(themed.Count==0)themed=unlocked;
            long seed=(long)State.journey.completed*17+State.journey.seasons*31L+slot*7;
            var crop=(slot==0?themed:unlocked)[(int)(seed%(slot==0?themed.Count:unlocked.Count))];
            int difficulty=Math.Min(5,State.journey.seasons/4);
            var offer=new JourneyContract{item=crop.id,kind=slot==2?2:0,target=3+difficulty,quantity=6+difficulty*2,medals=3+slot};
            int unit=crop.sellPrice;
            if(slot==1)
            {
                var recipes=new List<RecipeSpec>();foreach(var r in DevelopmentCatalog.Recipes)if(JourneyIngredientAvailable(r.id,0))recipes.Add(r);
                if(recipes.Count>0){var r=recipes[(int)(seed%recipes.Count)];offer.kind=1;offer.item=r.id;offer.target=offer.quantity=2+difficulty/2;unit=r.price;}
            }
            if(slot==2)
            {
                offer.target=2+difficulty/2;offer.quantity=3+difficulty;unit=StarUnitPrice(unit,3);
                if(State.hotpot.recipes.Exists(r=>r.onMenu)&&State.journey.completed%2==1)
                {offer.kind=3;offer.item="";offer.quantity=0;offer.target=3+difficulty;unit=0;}
            }
            int bonus=State.journey.harbor*4+(offer.kind==0||offer.kind==2?CropMastery(offer.item)*2:0);
            offer.reward=(int)Math.Ceiling((80+offer.quantity*unit*1.3+offer.target*12)*(1+bonus/100.0));
            return offer;
        }
        public ActionResult AcceptJourneyContract(int slot)
        {
            if(Level<3)return Fail("Hành trình mở ở cấp 3.");
            if(State.journey.active!=null)return Fail("Hãy hoàn thành hoặc hủy hợp đồng đang nhận.");
            if(State.journey.nextSerial>=1000000000)return Fail("Sổ hợp đồng đã đầy.");
            var offer=JourneyOffer(slot);if(offer==null)return Fail("Hãy tổng kết mùa trước khi nhận hợp đồng mới.");
            if(JourneyCounter(offer)>1000000000-offer.target)return Fail("Bộ đếm hoạt động đã đầy; hãy chọn hướng khác.");
            offer.serial=State.journey.nextSerial++;offer.baseline=JourneyCounter(offer);State.journey.active=offer;
            return Ok("Đã nhận hợp đồng. Chỉ tính hoạt động mới từ lúc nhận; không có hạn chót.");
        }
        public int JourneyFreshProgress(JourneyContract c){return c==null?0:Math.Max(0,Math.Min(c.target,JourneyCounter(c)-c.baseline));}
        public int JourneyStock(JourneyContract c)
        {return c.kind==3?0:c.kind==1?ItemCount(c.item):c.kind==2?CountPrime(c.item):Count(c.item)+CountFine(c.item)+CountPrime(c.item);}
        public string JourneyBlocker(JourneyContract c)
        {
            if(c==null)return "Chưa nhận hợp đồng.";
            if(JourneyFreshProgress(c)<c.target)return "Cần làm mới: "+JourneyFreshProgress(c)+" / "+c.target;
            if(JourneyStock(c)<c.quantity)return "Cần giữ đủ hàng để giao: "+JourneyStock(c)+" / "+c.quantity;
            if((long)State.coins+c.reward>1000000000||State.journey.medals>1000000000-c.medals)return "Kho thưởng đã đầy.";
            return "";
        }
        public ActionResult CompleteJourneyContract(int serial)
        {
            var c=State.journey.active;
            if(c==null||c.serial!=serial)return Fail("Hợp đồng đã đổi hoặc đã hoàn thành.");
            string blocker=JourneyBlocker(c);if(blocker.Length>0)return Fail(blocker);
            if(c.kind==1)ChangeItem(c.item,-c.quantity);
            else if(c.kind==2)Bucket(State.primeGoods,c.item).count-=c.quantity;
            else if(c.kind==0)
            {
                int left=c.quantity;var normal=Entry(c.item);int take=Math.Min(left,normal.count);normal.count-=take;left-=take;
                if(left>0){var fine=Bucket(State.fineGoods,c.item);take=Math.Min(left,fine.count);fine.count-=take;left-=take;}
                if(left>0)Bucket(State.primeGoods,c.item).count-=left;
            }
            State.coins+=c.reward;State.journey.medals+=c.medals;State.journey.completed++;State.journey.active=null;GainExperience(20);
            return Ok("Hợp đồng hoàn tất · +"+Money(c.reward)+" · +"+c.medals+" huy hiệu. Mục tiêu tiếp theo đã sẵn sàng.");
        }
        public ActionResult CancelJourneyContract(int serial)
        {
            if(State.journey.active==null||State.journey.active.serial!=serial)return Fail("Không có hợp đồng này.");
            State.journey.active=null;return Ok("Đã hủy. Giữ nguyên tiền và hàng; tiến độ làm mới của hợp đồng bị bỏ.");
        }
        public ActionResult AdvanceJourneySeason()
        {
            if(JourneySeasonProgress!=12||State.journey.active!=null)return Fail("Hoàn tất 12 hợp đồng để tổng kết mùa.");
            if(State.journey.medals>999999988)return Fail("Kho huy hiệu đã đầy.");
            State.journey.seasons++;State.journey.medals+=12;
            return Ok("Lưu kỷ niệm mùa vừa qua · +12 huy hiệu. Chào năm "+JourneyYear+" · "+JourneySeasons[State.journey.seasons%4]+"!");
        }
        public ActionResult UpgradeJourneyProject(int kind)
        {
            if(kind<0||kind>2)return Fail("Dự án không hợp lệ.");
            int level=JourneyProjectLevel(kind);if(level>=5)return Fail("Dự án đã đạt cấp 5.");
            int coins=JourneyProjectCost(kind),medals=JourneyProjectMedals(kind);
            if(State.coins<coins||State.journey.medals<medals)return Fail("Cần "+Money(coins)+" và "+medals+" huy hiệu.");
            State.coins-=coins;State.journey.medals-=medals;
            if(kind==0)State.journey.workshop++;else if(kind==1)State.journey.nursery++;else State.journey.harbor++;
            return Ok(JourneyProjects[kind]+" đạt cấp "+(level+1)+". Lợi ích áp dụng cho lượt mới.");
        }
        public ActionResult InvestJourneyLegacy()
        {
            if(State.journey.seasons<4)return Fail("Hoàn thành một năm hành trình để ghi danh di sản.");
            if(State.journey.legacyRank>=1000000000)return Fail("Sổ di sản đã đầy.");
            int coins=JourneyLegacyCoins,medals=JourneyLegacyMedals;
            if(State.coins<coins||State.journey.medals<medals)return Fail("Cần "+Money(coins)+" và "+medals+" huy hiệu.");
            State.coins-=coins;State.journey.medals-=medals;State.journey.legacyRank++;
            return Ok("Di sản đảo đạt bậc "+State.journey.legacyRank+" · ghi danh vĩnh viễn, không tăng sức mạnh.");
        }
        // Unity serializes an inline null class as its zero-filled object.
        // Only that exact empty shape is accepted and normalized after validation.
        private static bool EmptyJourneyContract(JourneyContract c)
        {return c==null||c.serial==0&&c.kind==0&&c.quantity==0&&c.target==0&&c.baseline==0&&c.reward==0&&c.medals==0&&String.IsNullOrEmpty(c.item);}
        private static bool EmptyJourneyState(JourneyState s)
        {return s==null||s.nextSerial==0&&s.completed==0&&s.seasons==0&&s.medals==0&&s.workshop==0&&s.nursery==0&&s.harbor==0&&s.legacyRank==0&&(s.crops==null||s.crops.Count==0)&&(s.products==null||s.products.Count==0)&&EmptyJourneyContract(s.active);}
        private static bool ValidJourney(JourneyState s,FarmState farm)
        {
            if(EmptyJourneyState(s))return true;
            if(s.completed<0||s.completed>999999999||s.seasons<0||s.seasons>s.completed/12||s.completed-(long)s.seasons*12>12||
                s.medals<0||s.medals>1000000000||s.nextSerial<1||s.nextSerial>1000000000||s.completed>=s.nextSerial||
                s.workshop<0||s.workshop>5||s.nursery<0||s.nursery>5||s.harbor<0||s.harbor>5||s.legacyRank<0||s.legacyRank>1000000000||s.legacyRank>0&&s.seasons<4||s.crops==null||s.products==null||s.crops.Count>CropCatalog.All.Length||s.products.Count>10)return false;
            var seen=new HashSet<string>();
            foreach(var r in s.crops)if(r==null||CropCatalog.Find(r.id)==null||!seen.Add(r.id)||r.count<0||r.count>1000000000||r.perfect<0||r.perfect>r.count)return false;
            seen.Clear();foreach(var r in s.products)if(r==null||DevelopmentCatalog.Recipe(r.id)==null||!seen.Add(r.id)||r.count<0||r.count>1000000000||r.perfect!=0)return false;
            var c=s.active;if(EmptyJourneyContract(c))return true;
            if(s.completed-(long)s.seasons*12>=12||c.serial<1||c.serial!=s.nextSerial-1||c.kind<0||c.kind>3||c.quantity<0||c.quantity>20||c.target<1||c.target>8||c.baseline<0||c.baseline>1000000000-c.target||c.reward<1||c.reward>100000||c.medals<3||c.medals>5)return false;
            if(c.kind==3)return c.quantity==0&&c.item==""&&c.baseline<=farm.hotpotServed;
            if(c.quantity<1||(c.kind==1?DevelopmentCatalog.Recipe(c.item)==null:CropCatalog.Find(c.item)==null))return false;
            var record=(c.kind==1?s.products:s.crops).Find(x=>x.id==c.item);
            return c.baseline<=(record==null?0:c.kind==2?record.perfect:record.count);
        }
    }
}
