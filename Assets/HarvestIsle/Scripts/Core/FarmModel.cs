using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    public enum FarmTool { Plant, Water, Harvest, Inspect, Drain, Treat, BuildDrain }

    [Serializable]
    public sealed class CropSpec
    {
        public string id;
        public string displayName;
        public int seedCost;
        public int sellPrice;
        public int yield;
        public double growSeconds;
        public string modelName;
        public int unlockLevel=1;

        public CropSpec(string id, string name, int cost, int price, int yield, double seconds)
        {
            this.id = id; displayName = name; seedCost = cost;
            sellPrice = price; this.yield = yield; growSeconds = seconds;
            modelName=char.ToUpperInvariant(id[0])+id.Substring(1);
        }

        public CropSpec(string id,string name,string model,int cost,int price,int yield,double seconds,int level):this(id,name,cost,price,yield,seconds)
        {modelName=model;unlockLevel=level;}
    }

    public static class CropCatalog
    {
        public static readonly CropSpec[] Original = {
            new CropSpec("carrot", "Cà rốt", 4, 5, 3, 45),
            new CropSpec("corn", "Ngô", 6, 7, 3, 75),
            new CropSpec("pumpkin", "Bí đỏ", 9, 12, 2, 120),
            new CropSpec("rice","Lúa","Rice",5,6,4,120,2),
            new CropSpec("greens","Cải ngọt","Greens",3,4,3,55,2),
            new CropSpec("tomato","Cà chua","Tomato",7,9,3,100,2),
            new CropSpec("onion","Hành","Onion",5,7,3,90,3),
            new CropSpec("beans","Đậu leo","Beans",5,6,4,100,3),
            new CropSpec("watermelon","Dưa hấu","Watermelon",12,19,2,180,3),
            new CropSpec("sugarcane","Mía","Sugarcane",9,11,4,180,4),
            new CropSpec("peas","Đậu Hà Lan","Peas",6,8,3,100,4),
            new CropSpec("strawberry","Dâu tây","Strawberry",9,12,3,140,4),
            new CropSpec("marigold","Cúc vạn thọ","Marigold",5,8,3,110,5),
            new CropSpec("chili","Ớt","Chili",6,9,3,110,5),
            new CropSpec("lemongrass","Sả","Lemongrass",4,6,4,95,5),
            new CropSpec("waterspinach","Rau muống","WaterSpinach",4,6,4,85,6),
            new CropSpec("taro","Khoai môn","Taro",8,12,3,150,6),
            new CropSpec("lotus","Sen","Lotus",9,15,3,170,6),
            new CropSpec("mushroom","Nấm","Mushroom",7,10,3,100,7),
            new CropSpec("radish","Củ cải","Radish",5,7,3,85,7),
            new CropSpec("cabbage","Bắp cải","Cabbage",7,10,3,120,7),
            new CropSpec("wheat","Lúa mì","Wheat",5,6,4,110,2),
            new CropSpec("snowtea","Trà tuyết","SnowTea",14,21,3,220,8),
            new CropSpec("garlic","Tỏi","Garlic",6,9,3,100,5),
            new CropSpec("mango","Xoài","Mango",18,16,4,240,8),
            new CropSpec("coconut","Dừa","Coconut",20,18,4,270,8),
            new CropSpec("orange","Cam","Orange",19,17,4,250,9),
            new CropSpec("lemon","Chanh","Lemon",16,14,4,220,9),
            new CropSpec("banana","Chuối","Banana",15,12,5,200,9),
            new CropSpec("papaya","Đu đủ","Papaya",18,16,4,230,10)
        };

        public static readonly CropSpec[] All = IslandCropCatalog.Append(Original);
        public static CropSpec Find(string id)
        {
            foreach (CropSpec crop in All) if (crop.id == id) return crop;
            return null;
        }
    }

    [Serializable]
    public sealed class PlotState
    {
        public string cropId = "";
        public double plantedAt;
        public bool watered;
        public string previousCropId = "";
        public string secondPreviousCropId = "";
        public double soilWater,disease,drainProtection,treatmentProtection;
        public bool drainageBuilt;
        public bool greenhouse;
        public int soilQuality;
        // Schema 1 additions (10/2026): harvests since planting (perennials regrow
        // faster from the second crop) and a pest infestation flag (0/1).
        public int harvests;
        public int pest;
    }

    [Serializable]
    public sealed class InventoryEntry
    {
        public string cropId;
        public int count;
        public InventoryEntry() { }
        public InventoryEntry(string id, int count) { cropId = id; this.count = count; }
    }

    [Serializable]
    public sealed class QuestState
    {
        public int kind;      // 0 harvests, 1 hotpot pots, 2 crafted goods, 3 orders
        public int target;
        public int baseline;  // counter value when the quest week started
        public bool claimed;
    }

    [Serializable]
    public sealed class WorkerState
    {
        public int kind;   // 0 tạp vụ, 1 trồng trọt, 2 chăn nuôi
        public int level;  // 1-3, bằng cấp Nhà nhân công lúc thuê
    }

    [Serializable]
    public sealed class FarmState
    {
        public int schemaVersion = 1;
        public int coins = 120;
        public int harvested;
        public int helped;
        public bool hasWatered;
        public bool hasSold;
        public double climateTime,ecologyRemainder;
        public double lastSeenUtc;
        public LivestockState chicken=new LivestockState(),cow=new LivestockState();
        public int eggs,milk;
        public DevelopmentState development;
        public List<PlotState> plots = new List<PlotState>();
        public List<InventoryEntry> inventory = new List<InventoryEntry>();
        // Schema 1 additions (10/2026): worker housing and hotpot shop fame. Old saves default to 0/empty.
        public int workerHouse;
        public List<WorkerState> workers = new List<WorkerState>();
        public int hotpotServed;
        // Star-quality produce stores (★★ and ★★★) sell at a premium; crafting and
        // orders keep using the base inventory. Pig/duck pens and claimed-achievement
        // bits follow the same append-only schema-1 rules.
        public List<InventoryEntry> fineGoods = new List<InventoryEntry>();
        public List<InventoryEntry> primeGoods = new List<InventoryEntry>();
        public LivestockState pig = new LivestockState(), duck = new LivestockState();
        public int achievementsClaimed;
        // Bees forage flowers for honey (and pollinate fruit plots); the fish pond
        // raises fresh fish. Weekly quests rotate on the UTC week index.
        public LivestockState bee = new LivestockState(), fish = new LivestockState();
        public int honey, fishStock;
        public int questWeek;
        public List<QuestState> quests = new List<QuestState>();
        public HotpotState hotpot;
        public JourneyState journey;
        public RecruitmentState recruitment;
        public IslandExpansionState islandExpansion;
    }

    public struct ActionResult
    {
        public bool success;
        public string message;
        public ActionResult(bool success, string message) { this.success = success; this.message = message; }
    }

    // No Unity dependency: timestamps and commands can later be supplied by a server.
    public sealed partial class FarmModel
    {
        public const int Width = 6;
        public const int Height = 4;
        public const int PlotCount = Width * Height; // starting field; parcels extend to MaxPlots
        public const int MaxPlots = 36;
        public FarmState State { get; private set; }

        public FarmModel(FarmState state)
        {
            string reason;
            if (!Validate(state, out reason)) throw new ArgumentException(reason);
            State = state;
            InitializeIslandExpansion();
            foreach(var spec in CropCatalog.All)
                if(!State.inventory.Exists(e=>e.cropId==spec.id))State.inventory.Add(new InventoryEntry(spec.id,0));
            if(State.development==null)State.development=new DevelopmentState();
            if(State.chicken==null)State.chicken=new LivestockState();
            if(State.cow==null)State.cow=new LivestockState();
            if(State.workers==null)State.workers=new List<WorkerState>();
            if(State.recruitment==null)State.recruitment=new RecruitmentState();
            if(State.recruitment.roster==null)State.recruitment.roster=new List<SpecialistWorker>();
            if(State.recruitment.reveal==null)State.recruitment.reveal=new List<WorkerReveal>();
            if(State.fineGoods==null)State.fineGoods=new List<InventoryEntry>();
            if(State.primeGoods==null)State.primeGoods=new List<InventoryEntry>();
            if(State.pig==null)State.pig=new LivestockState();
            if(State.duck==null)State.duck=new LivestockState();
            if(State.bee==null)State.bee=new LivestockState();
            if(State.fish==null)State.fish=new LivestockState();
            if(State.quests==null)State.quests=new List<QuestState>();
            if(State.hotpot==null)State.hotpot=new HotpotState();
            if(HotpotRules.EmptyProgress(State.hotpot.progress))State.hotpot.progress=new HotpotProgress{rank=HotpotTier+1,kitchenLevel=HotpotTier+1,branchServed=State.hotpotServed};
            State.hotpot.progress.initialized=true;
            if(EmptyJourneyState(State.journey))State.journey=new JourneyState();
            if(EmptyJourneyContract(State.journey.active))State.journey.active=null;
        }

        public static FarmState NewState(double now)
        {
            FarmState state = new FarmState();
            state.lastSeenUtc = now;
            for (int i = 0; i < PlotCount; i++) state.plots.Add(new PlotState());
            foreach (CropSpec crop in CropCatalog.All) state.inventory.Add(new InventoryEntry(crop.id, 0));
            return state;
        }

        public double ObserveTime(double now)
        {
            if (!IsFinite(now)) return State.lastSeenUtc;
            State.lastSeenUtc = Math.Max(now, State.lastSeenUtc);
            EnsureQuests(State.lastSeenUtc);
            return State.lastSeenUtc;
        }

        public double Progress(int index, double now)
        {
            if (index < 0 || index >= State.plots.Count) return 0;
            PlotState plot = State.plots[index];
            CropSpec crop = CropCatalog.Find(plot.cropId);
            if (crop == null) return 0;
            double duration = crop.growSeconds * (plot.watered ? 0.65 : 1.0);
            if (IsPerennial(plot.cropId) && plot.harvests > 0) duration *= 0.6; // established trees refruit faster
            double observed = Math.Max(IsFinite(now) ? now : State.lastSeenUtc, State.lastSeenUtc);
            return Math.Max(0, Math.Min(1, (observed - plot.plantedAt) / duration));
        }

        public double RemainingSeconds(int index, double now)
        {
            if (index < 0 || index >= State.plots.Count) return 0;
            PlotState plot = State.plots[index];
            CropSpec crop = CropCatalog.Find(plot.cropId);
            if (crop == null) return 0;
            return (1 - Progress(index, now)) * crop.growSeconds * (plot.watered ? 0.65 : 1.0);
        }

        public ActionResult Apply(int index, FarmTool tool, string cropId, double now, int transplantScore = 0)
        {
            if (index < 0 || index >= State.plots.Count) return Fail("Ô đất không hợp lệ.");
            now = ObserveTime(now);
            PlotState plot = State.plots[index];
            if(tool==FarmTool.Drain||tool==FarmTool.Treat||tool==FarmTool.BuildDrain)return Care(index,tool);
            if (tool == FarmTool.Plant)
            {
                CropSpec crop = CropCatalog.Find(cropId);
                if (crop == null) return Fail("Giống cây không hợp lệ.");
                if(!IslandCropUnlocked(cropId))return Fail("Mở chương M"+IslandCropCatalog.Chapter(cropId)+" trong Đảo trăm nghề để gieo giống này.");
                if(Level<crop.unlockLevel)return Fail("Giống này mở ở cấp "+crop.unlockLevel+". Làm đơn hàng và thu hoạch để tăng cấp.");
                if (!String.IsNullOrEmpty(plot.cropId)) return Fail("Ô này đã có cây. Hãy chọn ô trống.");
                if(plot.soilWater>=.65&&!IsWaterCrop(cropId))return Fail("Đất đang úng. Đào rãnh thoát nước trước khi gieo.");
                var seedling=State.development.seedlings.Find(s=>s.cropId==cropId&&s.readyAt<=now);
                if (seedling==null&&State.coins < crop.seedCost) return Fail("Chưa đủ tiền mua giống. Thu hoạch rồi bán ở thuyền nhé!");
                if(seedling==null)State.coins -= crop.seedCost;else State.development.seedlings.Remove(seedling);
                plot.cropId = crop.id;
                // Transplant minigame success deepens the nursery head start (25% → 35%).
                double headStart = seedling == null ? 0 : transplantScore > 0 ? .35 : .25;
                plot.plantedAt = seedling==null?now:Math.Max(0,now-crop.growSeconds*headStart);
                plot.watered = false;
                plot.harvests = 0;
                GainExperience(2);
                return Ok(seedling!=null&&transplantScore>0
                    ? "Cấy mạ khéo tay! " + crop.displayName + " bắt đầu ở 35% tiến độ."
                    : "Đã gieo " + crop.displayName + ". Tưới để cây lớn nhanh hơn!");
            }
            if (String.IsNullOrEmpty(plot.cropId)) return Fail("Ô đất đang trống. Chọn Gieo để trồng cây.");
            if (tool == FarmTool.Water)
            {
                if (Progress(index, now) >= 1) return Fail("Cây đã chín. Chọn Thu hoạch nhé!");
                if (plot.watered) return Fail("Cây đã được tưới rồi.");
                plot.watered = true;
                plot.soilWater=Math.Max(plot.soilWater,.25);
                State.hasWatered = true;
                return Ok("Đã tưới! Thời gian lớn giảm 35%.");
            }
            if (tool != FarmTool.Harvest) return Fail("Công cụ không hợp lệ.");
            return Harvest(index,1,now);
        }

        public ActionResult Harvest(int index,int score,double now)
        {
            if(index<0||index>=State.plots.Count||score<0||score>3)return Fail("Kết quả thu hoạch không hợp lệ.");
            now=ObserveTime(now);PlotState plot=State.plots[index];
            if(string.IsNullOrEmpty(plot.cropId))return Fail("Ô đất đang trống.");
            if (Progress(index, now) < 1) return Fail("Cây chưa chín: còn " + Math.Ceiling(RemainingSeconds(index, now)) + " giây.");
            CropSpec mature = CropCatalog.Find(plot.cropId);
            int bonus = RotationYieldBonus(index);
            int amount=HarvestYield(index,score);
            // Star grading: pampered perfect harvests reach ★★★, clean skilled ones ★★.
            int floodLoss = IsWaterCrop(plot.cropId) ? 0 : FloodYieldLoss(plot.soilWater);
            int stars = score == 3 && plot.disease < 1 && floodLoss == 0 && plot.pest == 0 && plot.soilQuality == 2 ? 3
                : score >= 2 && plot.disease < 35 && floodLoss == 0 && plot.pest == 0 ? 2 : 1;
            InventoryEntry entry = stars == 3 ? Bucket(State.primeGoods, mature.id)
                : stars == 2 ? Bucket(State.fineGoods, mature.id) : Entry(mature.id);
            if (entry.count > 10000000 - amount || State.harvested >= 1000000000)
                return Fail("Kho đã đầy. Bán nông sản trước khi thu hoạch.");
            entry.count += amount;
            Island.pruning.RemoveAll(p=>p.plot==index);
            plot.pest = 0;
            State.harvested++;
            RecordJourneyCrop(mature.id,stars);
            GainExperience(8);
            if (IsPerennial(plot.cropId)||IslandCropCatalog.Leaf(plot.cropId)&&plot.harvests<2)
            {
                // Perennial trees stay planted, sit outside crop rotation and refruit.
                plot.harvests = Math.Min(1000000000, plot.harvests + 1);
                plot.plantedAt = now;
                plot.watered = false;
                plot.disease *= .4;
                return Ok("+" + amount + " " + mature.displayName + Stars(stars) + " · kỹ năng " + score + "/3 · "+(IslandCropCatalog.Leaf(plot.cropId)?"còn "+(3-plot.harvests)+" lượt thu lá.":"cây tiếp tục ra quả."));
            }
            if(CropGroup(plot.cropId)=="Đậu")plot.soilQuality=Math.Min(2,plot.soilQuality+1);
            else if(IsMonoculture(index))plot.soilQuality=Math.Max(0,plot.soilQuality-1);
            plot.secondPreviousCropId=plot.previousCropId;
            plot.previousCropId = plot.cropId;
            plot.cropId = "";
            plot.plantedAt = 0;
            plot.watered = false;
            plot.harvests = 0;
            plot.disease*=.4;
            return Ok("+" + amount + " " + mature.displayName + Stars(stars) + " · kỹ năng " + score + "/3" + (bonus > 0 ? " · thưởng luân canh" : ""));
        }

        public int Count(string id) { return Entry(id).count; }
        public int CountFine(string id) { var e = State.fineGoods.Find(x => x.cropId == id); return e == null ? 0 : e.count; }
        public int CountPrime(string id) { var e = State.primeGoods.Find(x => x.cropId == id); return e == null ? 0 : e.count; }
        private static string Stars(int stars) { return stars == 3 ? " ★★★ vào kho" : stars == 2 ? " ★★ vào kho" : " vào kho"; }
        private static InventoryEntry Bucket(List<InventoryEntry> list, string id)
        {
            var entry = list.Find(x => x.cropId == id);
            if (entry == null) { entry = new InventoryEntry(id, 0); list.Add(entry); }
            return entry;
        }
        public static int StarUnitPrice(int basePrice, int stars)
        { return stars == 3 ? (int)Math.Ceiling(basePrice * 1.8) : stars == 2 ? (int)Math.Ceiling(basePrice * 1.35) : basePrice; }

        public int SellValue()
        {
            long total = 0;
            foreach (InventoryEntry entry in State.inventory) total += entry.count * CropCatalog.Find(entry.cropId).sellPrice;
            foreach (InventoryEntry entry in State.fineGoods) total += (long)entry.count * StarUnitPrice(CropCatalog.Find(entry.cropId).sellPrice, 2);
            foreach (InventoryEntry entry in State.primeGoods) total += (long)entry.count * StarUnitPrice(CropCatalog.Find(entry.cropId).sellPrice, 3);
            total+=State.eggs*5+State.milk*12+State.honey*18+State.fishStock*8;
            if (FestivalActive(State.lastSeenUtc)) total = (long)(total * 1.2);
            return (int)Math.Min(int.MaxValue,total);
        }

        public ActionResult SellAll()
        {
            int value = SellValue();
            if (value == 0) return Fail("Kho đang trống. Thu hoạch trước khi bán nhé!");
            // Keep currency bounded even when reading an externally edited local save.
            if ((long)State.coins + value > 1000000000) return Fail("Kho tiền đã đầy.");
            State.coins += value;
            State.hasSold = true;
            foreach (InventoryEntry entry in State.inventory) entry.count = 0;
            State.fineGoods.Clear(); State.primeGoods.Clear();
            State.eggs=0;State.milk=0;State.honey=0;State.fishStock=0;
            return Ok((FestivalActive(State.lastSeenUtc)?"Lễ hội Đom Đóm +20% giá! ":"")+"Thương nhân Hải Âu đã mua hàng: +" + Money(value) + "!");
        }

        public static bool Validate(FarmState state, out string reason)
        {
            reason = "Dữ liệu lưu không hợp lệ.";
            if (state == null || state.schemaVersion != 1 || state.coins < 0 || state.coins > 1000000000 ||
                state.harvested < 0 || state.harvested > 1000000000 || !IsFinite(state.lastSeenUtc) || state.lastSeenUtc < 0 ||
                state.plots == null || state.plots.Count < PlotCount || state.plots.Count > MaxPlots ||
                (state.plots.Count - PlotCount) % Width != 0 || state.inventory == null || state.inventory.Count<3||state.inventory.Count>CropCatalog.All.Length) return false;
            if(state.workerHouse<0||state.workerHouse>3||state.hotpotServed<0||state.hotpotServed>1000000000)return false;
            if(!HotpotRules.Validate(state.hotpot))return false;
            if(!ValidIslandExpansion(state.islandExpansion,state))return false;
            if(!ValidJourney(state.journey,state))return false;
            if(state.achievementsClaimed<0||state.achievementsClaimed>=1<<Achievements.Length)return false;
            if(!ValidLivestock(state.pig,state.lastSeenUtc)||!ValidLivestock(state.duck,state.lastSeenUtc))return false;
            if(!ValidLivestock(state.bee,state.lastSeenUtc)||!ValidLivestock(state.fish,state.lastSeenUtc))return false;
            if(!ValidBucket(state.fineGoods)||!ValidBucket(state.primeGoods))return false;
            if(state.honey<0||state.honey>1000000||state.fishStock<0||state.fishStock>1000000||state.questWeek<0)return false;
            if(state.quests!=null)
            {
                if(state.quests.Count>3)return false;
                foreach(QuestState quest in state.quests)
                    if(quest==null||quest.kind<0||quest.kind>3||quest.target<1||quest.target>1000000||quest.baseline<0)return false;
            }
            if(state.workers!=null)
            {
                if(state.workers.Count>state.workerHouse)return false;
                foreach(WorkerState worker in state.workers)
                    if(worker==null||worker.kind<0||worker.kind>2||worker.level<1||worker.level>3)return false;
            }
            if(!ValidRecruitment(state.recruitment,state.workerHouse,state.workers==null?0:state.workers.Count,state.lastSeenUtc))return false;
            HashSet<string> inventoryIds = new HashSet<string>();
            if(state.eggs<0||state.eggs>1000000||state.milk<0||state.milk>1000000||!ValidLivestock(state.chicken,state.lastSeenUtc)||!ValidLivestock(state.cow,state.lastSeenUtc))return false;
            if(!IsFinite(state.climateTime)||state.climateTime<0||state.climateTime>=300||!IsFinite(state.ecologyRemainder)||state.ecologyRemainder<0||state.ecologyRemainder>=1)return false;
            foreach (InventoryEntry entry in state.inventory)
            {
                if (entry == null || CropCatalog.Find(entry.cropId) == null || !inventoryIds.Add(entry.cropId) || entry.count < 0 || entry.count > 10000000) return false;
            }
            foreach (PlotState plot in state.plots)
            {
                if (plot == null || !IsFinite(plot.plantedAt) || plot.plantedAt < 0) return false;
                if(!ValidEcology(plot)||plot.soilQuality<0||plot.soilQuality>2||plot.harvests<0||plot.harvests>1000000000||plot.pest<0||plot.pest>1)return false;
                if (!String.IsNullOrEmpty(plot.cropId) && CropCatalog.Find(plot.cropId) == null) return false;
                if (!String.IsNullOrEmpty(plot.previousCropId) && CropCatalog.Find(plot.previousCropId) == null) return false;
                if (!String.IsNullOrEmpty(plot.secondPreviousCropId) && CropCatalog.Find(plot.secondPreviousCropId) == null) return false;
                if (!String.IsNullOrEmpty(plot.cropId) && plot.plantedAt > state.lastSeenUtc) return false;
            }
            if(!inventoryIds.Contains("carrot")||!inventoryIds.Contains("corn")||!inventoryIds.Contains("pumpkin")||!ValidDevelopment(state.development,state.lastSeenUtc))return false;
            int covered=state.plots.FindAll(p=>p.greenhouse).Count;
            var greenhouse=state.development==null?null:state.development.buildings.Find(b=>b.id=="greenhouse");
            if(covered>(greenhouse==null?0:greenhouse.level*4))return false;
            reason = "";
            return true;
        }

        private static bool ValidBucket(List<InventoryEntry> bucket)
        {
            if (bucket == null) return true; // older schema-1 saves migrate to empty stores
            HashSet<string> ids = new HashSet<string>();
            foreach (InventoryEntry entry in bucket)
                if (entry == null || CropCatalog.Find(entry.cropId) == null || !ids.Add(entry.cropId) || entry.count < 0 || entry.count > 10000000) return false;
            return true;
        }

        private InventoryEntry Entry(string id)
        {
            foreach (InventoryEntry entry in State.inventory) if (entry.cropId == id) return entry;
            throw new ArgumentException("Unknown inventory item: " + id);
        }
        private static bool IsFinite(double value) { return !Double.IsNaN(value) && !Double.IsInfinity(value); }
        private static ActionResult Ok(string message) { return new ActionResult(true, message); }
        private static ActionResult Fail(string message) { return new ActionResult(false, message); }
    }
}
