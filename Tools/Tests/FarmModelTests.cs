using System;
using HarvestIsle.Core;

internal static partial class FarmModelTests
{
    private static int passed;
    private static int failed;
    private const double Start = 1700000000;

    private static FarmModel Farm() { return new FarmModel(FarmModel.NewState(Start)); }
    private static HotpotRecipe Recipe()
    {
        var r=new HotpotRecipe();r.portions.Add(new HotpotPortion("carrot",1,1));r.portions.Add(new HotpotPortion("corn",1,1));return r;
    }
    private static FarmModel KitchenFarm()
    {
        var f=Farm();f.State.coins=10000;
        f.State.inventory.Find(x=>x.cropId=="carrot").count=20;
        f.State.inventory.Find(x=>x.cropId=="corn").count=20;
        f.ImportKitchen("carrot",1,10);f.ImportKitchen("corn",1,10);
        f.BuyKitchen("base",10);f.BuyKitchen("seasoning",50);return f;
    }
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    private static void Equal<T>(T expected, T actual)
    {
        Assert(Object.Equals(expected, actual), "Expected " + expected + ", got " + actual);
    }
    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }

    public static int Main()
    {
        TradingTests();
        IslandExpansionTests();
        RecruitmentTests();
        JourneyTests();
        HotpotEndlessTests();
        Test("Furniture catalog preserves old IDs and covers all 52 reference models", delegate {
            Equal(60,HotpotFurnitureCatalog.Items.Length);Equal(25,HotpotRules.FurniturePrices[0]);Equal(6000,HotpotRules.FurniturePrices[5]);
            int tables=0;var ids=new System.Collections.Generic.HashSet<string>();
            for(int i=8;i<60;i++){Assert(ids.Add(HotpotFurnitureCatalog.Items[i].id),"Duplicate furniture ID");if(HotpotFurnitureCatalog.IsTable(i))tables++;Assert(HotpotRules.FurniturePrices[i]>0,"Free furniture");}
            Equal(16,tables);Assert(!HotpotFurnitureCatalog.IsTable(6)&&HotpotFurnitureCatalog.IsTable(23),"Legacy threshold leaked");
        });
        Test("Every new decoration purchases persists and rotates without charging twice", delegate {
            for(int i=8;i<60;i++)
            {
                var f=Farm();f.State.coins=10000;
                int slot=HotpotFurnitureCatalog.Items[i].mount==HotpotMount.Floor?6:HotpotFurnitureCatalog.Items[i].mount==HotpotMount.Wall?12:20;
                Assert(f.PlaceHotpotFurniture(0,slot,i,0).success,"Cannot place "+i);int after=f.State.coins;
                Equal(10000-HotpotRules.FurniturePrices[i],after);Assert(!f.PlaceHotpotFurniture(0,slot,i,0).success,"Duplicate buy");
                Assert(f.MoveHotpotFurniture(0,slot,slot+1,1).success,"Move failed");Equal(after,f.State.coins);
                string reason;Assert(FarmModel.Validate(f.State,out reason),reason);var restored=new FarmModel(f.State);Equal(i,restored.State.hotpot.furniture[0].kind);
            }
        });
        Test("Wall ceiling and floor furniture cannot cross mounting zones", delegate {
            var f=Farm();f.State.coins=10000;
            Assert(!f.PlaceHotpotFurniture(0,6,36,0).success,"Clock on floor");
            Assert(!f.PlaceHotpotFurniture(0,12,8,0).success,"Dining table on wall");
            Assert(f.PlaceHotpotFurniture(0,12,36,0).success,"Wall clock placement");
            Assert(!f.MoveHotpotFurniture(0,12,20,0).success,"Clock moved to ceiling");
            Equal(9940,f.State.coins);f.State.hotpot.furniture[0].slot=26;string reason;Assert(!FarmModel.Validate(f.State,out reason),"Out of range slot load");
        });
        Test("A new farm has 24 empty plots and 120 coins", delegate {
            FarmModel farm = Farm(); Equal(24, farm.State.plots.Count); Equal(120, farm.State.coins);
            Equal(0, farm.SellValue());
        });
        Test("Money formats Xu Mua Vang without changing stored units", delegate {
            Equal("0 xu", FarmModel.Money(0));
            Equal("3 xu", FarmModel.Money(FarmModel.TreatCost));
            Equal("12 xu", FarmModel.Money(FarmModel.DrainageCost));
            Equal("120 xu", FarmModel.Money(120));
            Equal("1.200 xu", FarmModel.Money(1200));
            Equal("1.000.000.000 xu", FarmModel.Money(1000000000));
        });
        Test("Seeds are charged exactly once on occupied plots", delegate {
            FarmModel farm = Farm(); Assert(farm.Apply(0, FarmTool.Plant, "carrot", Start).success, "Plant failed");
            Assert(!farm.Apply(0, FarmTool.Plant, "corn", Start).success, "Double plant allowed");
            Equal(116, farm.State.coins); Equal("carrot", farm.State.plots[0].cropId);
        });
        Test("An unaffordable seed does not alter the plot or wallet", delegate {
            FarmModel farm = Farm(); farm.State.coins = 3;
            Assert(!farm.Apply(0, FarmTool.Plant, "carrot", Start).success, "Debt allowed");
            Equal(3, farm.State.coins); Equal("", farm.State.plots[0].cropId);
        });
        Test("Immature crops cannot be harvested", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "carrot", Start);
            Assert(!farm.Apply(0, FarmTool.Harvest, "carrot", Start + 44).success, "Early harvest allowed");
            Equal(0, farm.Count("carrot")); Equal("carrot", farm.State.plots[0].cropId);
        });
        Test("Watering accelerates growth and cannot be repeated", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "carrot", Start);
            Assert(farm.Apply(0, FarmTool.Water, "carrot", Start + 5).success, "Water failed");
            Assert(!farm.Apply(0, FarmTool.Water, "carrot", Start + 5).success, "Repeat watering allowed");
            Assert(farm.Apply(0, FarmTool.Harvest, "carrot", Start + 30).success, "Water did not accelerate");
            Equal(3, farm.Count("carrot"));
        });
        Test("Harvest succeeds once and leaves crop rotation history", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "corn", Start);
            Assert(farm.Apply(0, FarmTool.Harvest, "corn", Start + 75).success, "Harvest failed");
            Assert(!farm.Apply(0, FarmTool.Harvest, "corn", Start + 75).success, "Duplicate harvest allowed");
            Equal(3, farm.Count("corn")); Equal("corn", farm.State.plots[0].previousCropId); Equal(1, farm.State.harvested);
        });
        Test("Changing crops on a plot grants one additional item", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "carrot", Start);
            farm.Apply(0, FarmTool.Harvest, "carrot", Start + 45);
            farm.Apply(0, FarmTool.Plant, "pumpkin", Start + 45);
            farm.Apply(0, FarmTool.Harvest, "pumpkin", Start + 165);
            Equal(3, farm.Count("pumpkin"));
        });
        Test("Repeating the same crop does not grant a rotation bonus", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "carrot", Start);
            farm.Apply(0, FarmTool.Harvest, "carrot", Start + 45);
            farm.Apply(0, FarmTool.Plant, "carrot", Start + 45);
            farm.Apply(0, FarmTool.Harvest, "carrot", Start + 90);
            Equal(6, farm.Count("carrot"));
        });
        Test("Sales credit inventory value exactly once", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "carrot", Start);
            farm.Apply(0, FarmTool.Harvest, "carrot", Start + 45);
            Equal(15, farm.SellValue()); Assert(farm.SellAll().success, "Sale failed");
            Equal(131, farm.State.coins); Equal(0, farm.Count("carrot"));
            Assert(!farm.SellAll().success, "Duplicate sale allowed"); Equal(131, farm.State.coins);
        });
        Test("Reopening a persisted farm preserves offline growth", delegate {
            FarmModel farm = Farm(); farm.Apply(2, FarmTool.Plant, "pumpkin", Start);
            FarmModel reopened = new FarmModel(farm.State);
            Assert(reopened.Apply(2, FarmTool.Harvest, "pumpkin", Start + 86400).success, "Offline growth lost");
            Equal(2, reopened.Count("pumpkin"));
        });
        Test("Moving the clock backwards cannot regress progress", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "corn", Start);
            farm.ObserveTime(Start + 40); double before = farm.Progress(0, Start + 40);
            farm.ObserveTime(Start - 1000); Equal(before, farm.Progress(0, Start - 1000));
        });
        Test("Invalid indices, crops and tools do not change currency", delegate {
            FarmModel farm = Farm();
            Assert(!farm.Apply(-1, FarmTool.Plant, "carrot", Start).success, "Negative index allowed");
            Assert(!farm.Apply(24, FarmTool.Plant, "carrot", Start).success, "Outside index allowed");
            Assert(!farm.Apply(0, FarmTool.Plant, "gold", Start).success, "Unknown crop allowed");
            farm.Apply(0, FarmTool.Plant, "carrot", Start);
            Assert(!farm.Apply(0, (FarmTool)99, "carrot", Start).success, "Unknown tool allowed"); Equal(116, farm.State.coins);
        });
        Test("Unknown crops and malformed inventory are rejected on load", delegate {
            string reason; FarmState state = FarmModel.NewState(Start);
            state.plots[0].cropId = "unknown"; Assert(!FarmModel.Validate(state, out reason), "Unknown crop accepted");
            state = FarmModel.NewState(Start); state.inventory[1].cropId = "carrot";
            Assert(!FarmModel.Validate(state, out reason), "Duplicate inventory accepted");
        });
        Test("Corrupt versions, counts and timestamps are rejected", delegate {
            string reason; FarmState state = FarmModel.NewState(Start);
            state.schemaVersion = 2; Assert(!FarmModel.Validate(state, out reason), "Unknown version accepted");
            state = FarmModel.NewState(Start); state.coins = -1; Assert(!FarmModel.Validate(state, out reason), "Negative coins accepted");
            state = FarmModel.NewState(Start); state.plots.RemoveAt(0); Assert(!FarmModel.Validate(state, out reason), "Wrong grid accepted");
            state = FarmModel.NewState(Start); state.lastSeenUtc = Double.NaN; Assert(!FarmModel.Validate(state, out reason), "NaN accepted");
            state = FarmModel.NewState(Start); state.plots[0].cropId = "corn"; state.plots[0].plantedAt = Start + 1;
            Assert(!FarmModel.Validate(state, out reason), "Future planting accepted");
        });
        Test("Non-finite clock input never produces non-finite progress", delegate {
            FarmModel farm = Farm(); farm.Apply(0, FarmTool.Plant, "carrot", Start);
            farm.ObserveTime(Double.NaN); Equal(0.0, farm.Progress(0, Double.NaN));
            farm.ObserveTime(Double.PositiveInfinity); Equal(Start, farm.State.lastSeenUtc);
        });
        Test("A full inventory rejects harvest without destroying the crop", delegate {
            FarmModel farm = Farm(); farm.State.inventory[0].count = 10000000;
            farm.Apply(0, FarmTool.Plant, "carrot", Start);
            Assert(!farm.Apply(0, FarmTool.Harvest, "carrot", Start + 45).success, "Overflow accepted");
            Equal("carrot", farm.State.plots[0].cropId); Equal(10000000, farm.Count("carrot"));
        });
        Test("First season milestones survive harvest, sale and reopening", delegate {
            FarmModel farm=Farm();
            Assert(!farm.SellAll().success && !farm.State.hasSold,"Empty sale completed milestone");
            farm.Apply(0,FarmTool.Plant,"carrot",Start);
            farm.Apply(0,FarmTool.Water,"carrot",Start);
            farm.Apply(0,FarmTool.Harvest,"carrot",Start+30);
            Assert(farm.State.hasWatered && !farm.State.plots[0].watered,"Water milestone lost after harvest");
            Assert(!farm.State.hasSold,"Harvest incorrectly marked sale");
            farm.SellAll();
            FarmModel reopened=new FarmModel(farm.State);
            Assert(reopened.State.hasSold && reopened.State.hasWatered,"Milestones lost on reopening");
            Equal(131,reopened.State.coins);
        });
        Test("Rain collects faster in low plots and clear weather dries soil",delegate {
            var farm=Farm();farm.State.climateTime=90;farm.AdvanceEcology(40);
            Assert(farm.State.plots[0].soilWater>farm.State.plots[8].soilWater,"Low ground not wetter");
            double wet=farm.State.plots[0].soilWater;farm.State.climateTime=230;farm.AdvanceEcology(20);
            Assert(farm.State.plots[0].soilWater<wet,"No evaporation");
        });
        Test("Long rain infects crops and sick neighbours spread infection",delegate {
            var farm=Farm();farm.Apply(0,FarmTool.Plant,"carrot",Start);farm.State.climateTime=90;farm.AdvanceEcology(100);
            Assert(farm.State.plots[0].watered&&farm.State.plots[0].disease>=35,"No rain disease");
            farm.Apply(8,FarmTool.Plant,"corn",Start);farm.Apply(9,FarmTool.Plant,"corn",Start);
            farm.State.plots[8].disease=50;farm.State.plots[9].soilWater=.35;farm.State.climateTime=90;farm.AdvanceEcology(1);
            Assert(farm.State.plots[9].disease>0,"No adjacency spread");
        });
        Test("Drainage allows planting and treatment charges once for valid care",delegate {
            var farm=Farm();farm.State.plots[0].soilWater=.9;
            Assert(!farm.Apply(0,FarmTool.Plant,"carrot",Start).success,"Flooded planting allowed");
            Assert(farm.Apply(0,FarmTool.Drain,"",Start).success,"Drain failed");Equal(120,farm.State.coins);
            farm.Apply(0,FarmTool.Plant,"carrot",Start);farm.State.plots[0].disease=45;
            Assert(farm.Apply(0,FarmTool.Treat,"",Start).success,"Treat failed");Equal(113,farm.State.coins);Equal(0.0,farm.State.plots[0].disease);
            Assert(!farm.Apply(0,FarmTool.Treat,"",Start).success,"Unneeded treatment charged");Equal(113,farm.State.coins);
        });
        Test("Crop rotation reduces infection and wet disease reduces yield",delegate {
            var farm=Farm();farm.Apply(0,FarmTool.Plant,"carrot",Start);farm.Apply(1,FarmTool.Plant,"carrot",Start);
            farm.State.plots[1].previousCropId="corn";farm.State.plots[0].soilWater=1;farm.State.plots[1].soilWater=1;
            farm.State.climateTime=100;farm.AdvanceEcology(20);
            Assert(farm.State.plots[1].disease<farm.State.plots[0].disease,"Rotation did not protect");
            farm.State.plots[0].disease=90;Equal(1,farm.HarvestYield(0,0));
            Assert(farm.Harvest(0,0,Start+100).success,"Wilted crop cannot be salvaged");Equal(1,farm.Count("carrot"));
        });
        Test("Ecology survives reload and does not advance just from offline UTC",delegate {
            var farm=Farm();farm.State.climateTime=170;farm.State.plots[0].soilWater=.8;farm.State.plots[0].disease=45;
            var reopened=new FarmModel(farm.State);reopened.ObserveTime(Start+86400);
            Equal(170.0,reopened.State.climateTime);Equal(.8,reopened.State.plots[0].soilWater);Equal(45.0,reopened.State.plots[0].disease);
            string reason;reopened.State.plots[0].soilWater=double.NaN;Assert(!FarmModel.Validate(reopened.State,out reason),"Corrupt ecology accepted");
        });
        Test("All three harvest challenges stop after three rounds and scale difficulty",delegate {
            foreach(string crop in new[]{"carrot","corn","pumpkin"}){
                var challenge=new HarvestChallenge(crop,0,3);
                for(int i=0;i<3;i++){if(crop=="corn")challenge.Choose(challenge.Sequence[i]);else challenge.Timing(.5);}
                Assert(challenge.Done&&challenge.Score==3,"Perfect harvest not scored");challenge.Timing(.5);challenge.Choose(0);Equal(3,challenge.Score);
                Assert(new HarvestChallenge(crop,40,3).Window<challenge.Window,"No difficulty progression");
            }
        });
        Test("Harvest quality changes reward without allowing duplicates",delegate {
            var farm=Farm();farm.Apply(0,FarmTool.Plant,"carrot",Start);
            Assert(!farm.Harvest(0,4,Start+100).success,"Invalid grade accepted");
            Assert(farm.Harvest(0,3,Start+100).success,"Perfect harvest failed");
            Equal(5,farm.Count("carrot")+farm.CountFine("carrot")+farm.CountPrime("carrot")); // skilled output now lands in star stores
            Assert(!farm.Harvest(0,3,Start+100).success,"Duplicate reward");
        });
        Test("Legacy farms gain empty pens without changing money or crops",delegate {
            var state=FarmModel.NewState(Start);state.chicken=null;state.cow=null;state.coins=64;
            var farm=new FarmModel(state);Equal(0,farm.State.chicken.count);Equal(0,farm.State.cow.count);Equal(64,farm.State.coins);
        });
        Test("Animal purchases respect funds, pen capacity and active production",delegate {
            var farm=Farm();Assert(!farm.BuyAnimal("dragon").success,"Unknown animal allowed");
            for(int i=0;i<3;i++)Assert(farm.BuyAnimal("chicken").success,"Chicken purchase failed");
            Equal(30,farm.State.coins);Assert(!farm.BuyAnimal("chicken").success,"Pen overflow");Assert(!farm.BuyAnimal("cow").success,"Debt allowed");
            var second=Farm();second.BuyAnimal("cow");second.FeedAnimal("cow",false,Start);
            Assert(!second.BuyAnimal("cow").success,"Batch altered by new purchase");Equal(1,second.State.cow.count);
        });
        Test("Feeding spends once and collecting cannot duplicate product",delegate {
            var farm=Farm();farm.BuyAnimal("chicken");Assert(farm.FeedAnimal("chicken",false,Start).success,"Feed failed");Equal(87,farm.State.coins);
            Assert(!farm.FeedAnimal("chicken",false,Start).success,"Double feed");Equal(87,farm.State.coins);
            Assert(!farm.CollectAnimal("chicken",Start+59).success,"Early collection");
            Assert(farm.CollectAnimal("chicken",Start+60).success,"Collection failed");Equal(2,farm.State.eggs);
            Assert(!farm.CollectAnimal("chicken",Start+61).success,"Duplicate collection");
            farm.SellAll();Equal(97,farm.State.coins);Equal(0,farm.State.eggs);
        });
        Test("Corn feeds the whole herd and offline time completes only one batch",delegate {
            var farm=Farm();farm.BuyAnimal("chicken");farm.BuyAnimal("chicken");farm.State.inventory[1].count=1;
            Assert(farm.FeedAnimal("chicken",true,Start).success,"Corn feed failed");Equal(60,farm.State.coins);Equal(0,farm.Count("corn"));
            var reopened=new FarmModel(farm.State);Assert(reopened.CollectAnimal("chicken",Start+86400).success,"Offline production failed");Equal(4,reopened.State.eggs);
            Assert(!reopened.CollectAnimal("chicken",Start+172800).success,"Unfed offline batch created");
        });
        Test("Full product storage and corrupt livestock are rejected safely",delegate {
            var farm=Farm();farm.BuyAnimal("cow");farm.FeedAnimal("cow",false,Start);farm.State.milk=1000000;
            Assert(!farm.CollectAnimal("cow",Start+100).success&&farm.State.cow.producing,"Full inventory lost batch");
            string reason;farm.State.cow.count=4;Assert(!FarmModel.Validate(farm.State,out reason),"Invalid herd size accepted");farm.State.cow.count=1;
            farm.State.cow.fedAt=double.NaN;Assert(!FarmModel.Validate(farm.State,out reason),"Invalid animal time accepted");
        });
        Test("Flood penalties start at visible pooling and stack with disease",delegate {
            var farm=Farm();farm.Apply(0,FarmTool.Plant,"carrot",Start);
            var p=farm.State.plots[0];p.soilWater=.5499;Equal(3,farm.HarvestYield(0,1));
            p.soilWater=.55;Equal(2,farm.HarvestYield(0,1));
            p.soilWater=.8499;Equal(2,farm.HarvestYield(0,1));
            p.soilWater=.85;Equal(1,farm.HarvestYield(0,1));
            p.disease=35;Equal(2,farm.HarvestYield(0,3));
            p.disease=100;Equal(1,farm.HarvestYield(0,0));
            farm.Apply(0,FarmTool.Drain,"",Start);p.disease=0;Equal(3,farm.HarvestYield(0,1));
            Assert(farm.Harvest(0,1,Start+100).success,"Harvest after rescue failed");Equal(3,farm.Count("carrot"));
        });
        Test("Permanent drainage costs once and cannot overdraft",delegate {
            var farm=Farm();farm.State.coins=11;
            Assert(!farm.Apply(0,FarmTool.BuildDrain,"",Start).success&&!farm.State.plots[0].drainageBuilt,"Unaffordable drain built");Equal(11,farm.State.coins);
            farm.State.coins=12;Assert(farm.Apply(0,FarmTool.BuildDrain,"",Start).success,"Drain construction failed");Equal(0,farm.State.coins);
            Assert(!farm.Apply(0,FarmTool.BuildDrain,"",Start).success,"Duplicate drain charged");Equal(0,farm.State.coins);
            farm.State.plots[0].soilWater=1;Assert(farm.Apply(0,FarmTool.Drain,"",Start).success,"Zero-coin emergency drain blocked");
        });
        Test("Permanent drain protects low ground through whole rain cycle",delegate {
            var farm=Farm();farm.Apply(0,FarmTool.BuildDrain,"",Start);farm.State.climateTime=90;farm.AdvanceEcology(139);
            Assert(farm.State.plots[0].soilWater<.55,"Protected low plot flooded");
            Assert(farm.State.plots[1].soilWater>=.85,"Unprotected control did not flood");
            farm.State.plots[0].soilWater=.8;farm.State.climateTime=190;farm.AdvanceEcology(20);
            Assert(farm.State.plots[0].soilWater<.8,"Drain failed to remove standing water in heavy rain");
            Assert(farm.State.plots[0].drainProtection==0,"Permanent protection depends on temporary timer");
        });
        Test("Drain survives harvest and old plots default to unprotected",delegate {
            var farm=Farm();Assert(!farm.State.plots[0].drainageBuilt,"Legacy default changed");
            farm.Apply(0,FarmTool.BuildDrain,"",Start);farm.Apply(0,FarmTool.Plant,"carrot",Start);
            farm.Harvest(0,1,Start+100);Assert(farm.State.plots[0].drainageBuilt,"Harvest removed drain");
            var reopened=new FarmModel(farm.State);Assert(reopened.State.plots[0].drainageBuilt,"Reload removed drain");
            Assert(reopened.Apply(0,FarmTool.Plant,"corn",Start+101).success,"Cannot rotate crop in protected plot");
        });
        Test("Legacy three-crop inventory migrates without losing balances",delegate {
            var state=FarmModel.NewState(Start);state.inventory.RemoveRange(3,state.inventory.Count-3);state.inventory[0].count=7;
            var farm=new FarmModel(state);Equal(60,farm.State.inventory.Count);Equal(7,farm.Count("carrot"));Equal(120,farm.State.coins);Equal(1,farm.Level);
            Assert(!farm.Apply(0,FarmTool.Plant,"mango",Start).success,"Locked crop planted");
        });
        Test("All 60 crops can be planted grown and harvested with unlocked level and chapter",delegate {
            Equal(60,CropCatalog.All.Length);
            foreach(var crop in CropCatalog.All){var farm=Farm();farm.GainExperience(400);farm.Island.chapter=3;Assert(farm.Apply(0,FarmTool.Plant,crop.id,Start).success,"Plant "+crop.id);Assert(farm.Harvest(0,1,Start+crop.growSeconds+1).success,"Harvest "+crop.id);Equal(crop.yield,farm.Count(crop.id));}
        });
        Test("Production consumes inputs once and progresses offline through a saved queue",delegate {
            var farm=Farm();farm.GainExperience(80);farm.State.coins=1000;farm.State.development.wood=100;farm.State.development.ore=100;
            Assert(farm.UpgradeBuilding("mill").success,"Build mill");farm.State.inventory.Find(e=>e.cropId=="wheat").count=9;
            Assert(farm.StartProduction("flour",Start).success,"Queue flour");Equal(6,farm.Count("wheat"));
            Assert(!farm.StartProduction("flour",Start).success,"Level one queue exceeded");Equal(6,farm.Count("wheat"));
            Assert(!farm.CollectProduction(1,Start+1).success,"Early collect");
            var reopened=new FarmModel(farm.State);Assert(reopened.CollectProduction(1,Start+86400).success,"Offline collect");Equal(1,reopened.ItemCount("flour"));
            Assert(!reopened.CollectProduction(1,Start+86400).success,"Duplicate collect");
            Assert(reopened.SellProduct("flour").success,"Product sale");Equal(990,reopened.State.coins);
        });
        Test("Upgraded serial queues obey capacity and missing inputs do not debit",delegate {
            var farm=Farm();farm.GainExperience(100);farm.State.coins=1000;farm.State.development.wood=100;farm.State.development.ore=100;
            farm.UpgradeBuilding("mill");farm.UpgradeBuilding("mill");farm.UpgradeBuilding("mill");Equal(3,farm.BuildingLevel("mill"));
            int balance=farm.State.coins;Assert(!farm.UpgradeBuilding("mill").success,"Over-upgrade");Equal(balance,farm.State.coins);
            Assert(!farm.StartProduction("flour",Start).success,"Missing input accepted");Equal(0,farm.State.development.jobs.Count);
            farm.State.inventory.Find(e=>e.cropId=="wheat").count=12;
            for(int i=0;i<3;i++)Assert(farm.StartProduction("flour",Start).success,"Queue slot");
            Assert(farm.State.development.jobs[2].readyAt>farm.State.development.jobs[0].readyAt,"Queue not serial");
            Assert(!farm.StartProduction("flour",Start).success,"Queue overfilled");Equal(3,farm.Count("wheat"));
            string reason;Assert(FarmModel.Validate(farm.State,out reason),"Valid queue rejected");
            farm.State.development.jobs[1].id=1;Assert(!FarmModel.Validate(farm.State,out reason),"Duplicate job accepted");
        });
        Test("Orders login gifts and journal cannot pay duplicate rewards",delegate {
            var farm=Farm();Assert(!farm.FulfillOrder().success,"Order without goods");farm.State.inventory[0].count=3;
            Assert(farm.FulfillOrder().success,"Order failed");Equal(0,farm.Count("carrot"));Equal(147,farm.State.coins);Equal(1,farm.State.development.ordersCompleted);
            Assert(!farm.FulfillOrder().success,"Duplicate order paid");
            Assert(farm.ClaimDaily(Start).success,"Daily gift failed");int coins=farm.State.coins;
            Assert(!farm.ClaimDaily(Start-86400).success&&!farm.ClaimDaily(Start).success,"Daily gift replayed");Equal(coins,farm.State.coins);
            Assert(!farm.ClaimChapter().success,"Unfinished chapter paid");farm.Apply(0,FarmTool.Plant,"carrot",Start);
            Assert(farm.ClaimChapter().success,"Chapter failed");Assert(!farm.ClaimChapter().success,"Unfinished next chapter paid");
        });
        Test("Nursery reserves seed cost and transplants once with a head start",delegate {
            var farm=Farm();farm.GainExperience(400);farm.State.coins=1000;farm.State.development.wood=100;
            farm.UpgradeBuilding("nursery");int coins=farm.State.coins;
            Assert(farm.StartNursery("carrot",Start).success,"Nursery failed");Equal(coins-4,farm.State.coins);
            Assert(farm.StartNursery("corn",Start).success,"Second nursery slot");Assert(!farm.StartNursery("corn",Start).success,"Nursery over capacity");
            coins=farm.State.coins;Assert(farm.Apply(0,FarmTool.Plant,"carrot",Start+20).success,"Transplant failed");Equal(coins,farm.State.coins);Equal(.25,farm.Progress(0,Start+20));Equal(1,farm.State.development.seedlings.Count);
            Assert(farm.Apply(1,FarmTool.Plant,"carrot",Start+20).success,"Normal planting failed");Equal(coins-4,farm.State.coins);
        });
        Test("Greenhouse capacity limits shelter and keeps rain out",delegate {
            var farm=Farm();farm.GainExperience(400);farm.State.coins=1000;farm.State.development.wood=100;
            Assert(!farm.ToggleGreenhouse(0).success,"Unbuilt shelter used");farm.UpgradeBuilding("greenhouse");
            for(int i=0;i<4;i++)Assert(farm.ToggleGreenhouse(i).success,"Shelter allocation");Assert(!farm.ToggleGreenhouse(4).success,"Shelter over capacity");
            farm.State.climateTime=190;farm.AdvanceEcology(35);Equal(0.0,farm.State.plots[0].soilWater);Assert(farm.State.plots[4].soilWater>.55,"Control not flooded");
            farm.ToggleGreenhouse(0);Assert(farm.ToggleGreenhouse(4).success,"Shelter slot not released");
        });
        Test("Water crops tolerate flooded soil and crafted care consumes one item",delegate {
            var farm=Farm();farm.GainExperience(400);farm.State.plots[0].soilWater=.95;
            Assert(farm.Apply(0,FarmTool.Plant,"lotus",Start).success,"Lotus rejected flooded plot");Equal(3,farm.HarvestYield(0,1));
            farm.State.development.products.Add(new ProductStock{id="compost",count=3});
            Assert(farm.UseCareProduct("compost",0).success,"Compost failed");Equal(4,farm.HarvestYield(0,1));farm.UseCareProduct("compost",0);
            Assert(!farm.UseCareProduct("compost",0).success,"Overfertilized");Equal(1,farm.ItemCount("compost"));
            farm.State.development.products.Add(new ProductStock{id="herbal",count=2});farm.State.plots[0].disease=40;
            Assert(farm.UseCareProduct("herbal",0).success,"Herbal treatment failed");Equal(5.0,farm.State.plots[0].disease);Equal(1,farm.ItemCount("herbal"));
        });
        Test("Rotation remembers groups and beans restore soil for the following crop",delegate {
            var farm=Farm();farm.GainExperience(400);var p=farm.State.plots[0];
            farm.Apply(0,FarmTool.Plant,"beans",Start);farm.Harvest(0,1,Start+200);Equal(1,p.soilQuality);
            farm.Apply(0,FarmTool.Plant,"carrot",Start+201);Equal(1,farm.RotationYieldBonus(0));farm.Harvest(0,1,Start+300);
            Equal("beans",p.secondPreviousCropId);Equal("carrot",p.previousCropId);
            farm.Apply(0,FarmTool.Plant,"radish",Start+301);Equal(0,farm.RotationYieldBonus(0));farm.Harvest(0,1,Start+500);
            farm.Apply(0,FarmTool.Plant,"onion",Start+501);Assert(farm.IsMonoculture(0),"Three root crops not identified as monoculture");farm.Harvest(0,1,Start+700);Equal(0,p.soilQuality);
        });
        Test("Companion protection and three sisters work only within their neighbourhood",delegate {
            var farm=Farm();farm.GainExperience(400);
            farm.Apply(8,FarmTool.Plant,"carrot",Start);farm.Apply(9,FarmTool.Plant,"onion",Start);Equal(.5,farm.CompanionProtection(8));
            farm.Apply(2,FarmTool.Plant,"marigold",Start);Assert(farm.CompanionProtection(8)<.5,"Marigold protection missing");
            farm.Apply(20,FarmTool.Plant,"corn",Start);farm.Apply(21,FarmTool.Plant,"beans",Start);farm.Apply(14,FarmTool.Plant,"pumpkin",Start);
            Equal(1,farm.CompanionYieldBonus(20));Equal(1,farm.CompanionYieldBonus(21));Equal(0,farm.CompanionYieldBonus(8));
        });
        Test("Large expanded inventory cannot overflow currency during sale",delegate {
            var farm=Farm();foreach(var item in farm.State.inventory)item.count=10000000;
            Assert(farm.SellValue()>1000000000,"Sale value overflowed");Assert(!farm.SellAll().success,"Oversized sale accepted");Equal(120,farm.State.coins);Equal(10000000,farm.Count("carrot"));
        });
        Test("Land parcels unlock six plots each and cap at 36", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000;
            farm.State.development.experience=160;farm.State.harvested=30;farm.State.hasSold=true;farm.State.development.ordersCompleted=5;
            Assert(farm.UnlockParcel().success, "First parcel refused");
            Equal(30, farm.State.plots.Count); Equal(900, farm.State.coins);
            Assert(farm.UnlockParcel().success, "Second parcel refused");
            Equal(36, farm.State.plots.Count); Equal(620, farm.State.coins);
            Assert(!farm.UnlockParcel().success, "Unlocked beyond the cap");
            Assert(farm.Apply(35, FarmTool.Plant, "carrot", Start).success, "Cannot plant on new land");
            farm.State.coins = 0; // broke farms cannot reclaim
            Assert(!new FarmModel(farm.State).UnlockParcel().success, "Reopened fully unlocked farm");
        });
        Test("Expanded farms validate and odd plot counts are rejected", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000;
            farm.State.development.experience=80;farm.State.harvested=10;farm.State.hasSold=true;
            Assert(farm.UnlockParcel().success,"First parcel refused");
            string reason;
            Assert(FarmModel.Validate(farm.State, out reason), "30 plots rejected: " + reason);
            farm.State.plots.Add(new PlotState());
            Assert(!FarmModel.Validate(farm.State, out reason), "31 plots accepted");
        });
        Test("Reclamation gates level missions and money independently without side effects", delegate {
            for(int parcel=0;parcel<2;parcel++)
            for(int missing=0;missing<4;missing++)
            {
                var f=Farm();for(int i=0;i<parcel*6;i++)f.State.plots.Add(new PlotState());
                f.State.development.experience=(FarmModel.ParcelLevel(parcel)-1)*40;
                f.State.harvested=FarmModel.ParcelHarvestGoal(parcel);f.State.hasSold=true;f.State.development.ordersCompleted=5;
                f.State.coins=FarmModel.ParcelCost(parcel);
                if(missing==0)f.State.development.experience--;
                if(missing==1)f.State.harvested--;
                if(missing==2){if(parcel==0)f.State.hasSold=false;else f.State.development.ordersCompleted--;}
                if(missing==3)f.State.coins--;
                int coins=f.State.coins,xp=f.State.development.experience,count=f.State.plots.Count;
                Assert(!f.CanUnlockParcel(parcel),"UI gate bypass");Assert(!f.UnlockParcel().success,"Core gate bypass");
                Equal(coins,f.State.coins);Equal(xp,f.State.development.experience);Equal(count,f.State.plots.Count);
            }
        });
        Test("Exact reclamation thresholds charge once preserve crops and advance sequentially", delegate {
            var f=Farm();f.Apply(0,FarmTool.Plant,"carrot",Start);
            f.State.coins=100;f.State.harvested=10;f.State.hasSold=true;f.State.development.experience=80;
            Assert(!f.CanUnlockParcel(1)&&!f.CanUnlockParcel(-1)&&!f.CanUnlockParcel(2),"Invalid or future parcel allowed");
            Assert(f.UnlockParcel().success,"Exact first threshold");Equal(0,f.State.coins);Equal(90,f.State.development.experience);
            Equal("carrot",f.State.plots[0].cropId);Equal(10,f.State.harvested);Assert(!f.UnlockParcel().success,"Repeated click unlocked second");
            f.State.coins=280;f.State.harvested=30;f.State.development.ordersCompleted=5;f.State.development.experience=160;
            Assert(f.UnlockParcel().success,"Exact second threshold");Equal(0,f.State.coins);Equal(170,f.State.development.experience);
            Equal(36,f.State.plots.Count);Equal(5,f.State.development.ordersCompleted);
        });
        Test("Legacy expanded saves keep their land despite unmet new requirements", delegate {
            foreach(int count in new[]{24,30,36})
            {
                var state=FarmModel.NewState(Start);while(state.plots.Count<count)state.plots.Add(new PlotState());
                string reason;Assert(FarmModel.Validate(state,out reason),reason);var loaded=new FarmModel(state);
                Equal(count,loaded.State.plots.Count);Equal((count-24)/6,loaded.ParcelsUnlocked);
                Assert(!loaded.UnlockParcel().success,"Legacy farm bypassed requirements");Equal(count,loaded.State.plots.Count);
            }
        });
        Test("Hotpot requires tested recipes and consumes only imported kitchen stock", delegate {
            var farm=KitchenFarm();var r=Recipe();int coins=farm.State.coins;
            Assert(!farm.ServeHotpot(new[]{"carrot","corn"}).success,"Legacy bypass allowed");
            Assert(!farm.ServeHotpotRecipe(1).success,"Untested recipe served");
            Assert(farm.TestHotpotRecipe(r).success,"Test cook failed");
            Equal(9,farm.KitchenCount("carrot"));Equal(coins,farm.State.coins);
            Assert(!farm.ServeHotpotRecipe(1).success,"Off-menu recipe served");
            farm.SetHotpotMenu(1,true);int price=farm.RecipePrice(r);
            Assert(farm.ServeHotpotRecipe(1).success,"Serve failed");
            Equal(8,farm.KitchenCount("carrot"));Equal(10,farm.Count("carrot"));Equal(coins+price,farm.State.coins);
        });
        Test("Worker house gates hiring, capacity and hire level", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000;
            farm.State.development.wood = 60; farm.State.development.ore = 20;
            Assert(!farm.HireWorker(0).success, "Hired without a house");
            Assert(farm.UpgradeWorkerHouse().success, "House level 1 refused");
            Assert(farm.HireWorker(1).success, "Hire refused");
            Equal(1, farm.State.workers[0].level);
            Assert(!farm.HireWorker(0).success, "Hired past capacity");
            Assert(farm.UpgradeWorkerHouse().success, "House level 2 refused");
            Assert(farm.HireWorker(2).success, "Second hire refused");
            Equal(2, farm.State.workers[1].level);
            string reason; Assert(FarmModel.Validate(farm.State, out reason), "Worker save rejected: " + reason);
        });
        Test("Workers water, harvest and tend animals by speciality and level", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000;
            farm.State.development.wood = 60; farm.State.development.ore = 20;
            farm.UpgradeWorkerHouse(); farm.UpgradeWorkerHouse();
            farm.HireWorker(1);
            farm.Apply(0, FarmTool.Plant, "carrot", Start);
            Equal(1, farm.RunWorkers(Start + 1));
            Assert(farm.State.plots[0].watered, "Worker did not water");
            Equal(1, farm.RunWorkers(Start + 1000));
            Equal(3, farm.Count("carrot"));
            farm.HireWorker(2);
            farm.State.coins = 1000; farm.BuyAnimal("chicken");
            farm.State.inventory.Find(e => e.cropId == "corn").count = 1;
            Equal(1, farm.RunWorkers(Start + 1001));
            Assert(farm.Animal("chicken").producing, "Rancher did not feed");
            Equal(1, farm.RunWorkers(Start + 1101));
            Equal(2, farm.State.eggs);
        });
        Test("Perennial trees refruit faster and only clear when chopped", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000; farm.GainExperience(400);
            Assert(farm.Apply(0, FarmTool.Plant, "mango", Start).success, "Plant mango");
            Assert(farm.Harvest(0, 1, Start + 241).success, "First mango harvest");
            Equal("mango", farm.State.plots[0].cropId); Equal(1, farm.State.plots[0].harvests);
            Assert(farm.Progress(0, Start + 241 + 145) >= 1, "Refruit not faster"); // 240 * 0.6 = 144s
            Assert(farm.Progress(0, Start + 241 + 120) < 1, "Refruit too fast");
            Assert(farm.Harvest(0, 1, Start + 241 + 145).success, "Second mango harvest");
            Equal(2, farm.State.plots[0].harvests);
            Assert(!farm.ChopPerennial(1).success, "Chopped an empty plot");
            Assert(farm.ChopPerennial(0).success, "Chop refused");
            Equal("", farm.State.plots[0].cropId); Equal("mango", farm.State.plots[0].previousCropId);
            string reason; Assert(FarmModel.Validate(farm.State, out reason), "Perennial save rejected: " + reason);
        });
        Test("Hotpot fame raises prices and opens tables", delegate {
            FarmModel farm = KitchenFarm();
            Equal(0, farm.HotpotTier); Equal(4, farm.HotpotTables);
            farm.TestHotpotRecipe(Recipe());farm.SetHotpotMenu(1,true);
            int basePrice = farm.RecipePrice(Recipe());
            farm.State.hotpotServed = 15;
            Equal(1, farm.HotpotTier); Equal(5, farm.HotpotTables);
            Assert(farm.RecipePrice(Recipe()) > basePrice, "Tier price flat");
            Assert(farm.ServeHotpotRecipe(1).success, "Serve at tier 1");
            Equal(16, farm.State.hotpotServed);
            farm.State.hotpotServed = 40; Equal(2, farm.HotpotTier); Equal(6, farm.HotpotTables);
        });
        Test("Harvest quality grades into starred stores with premium prices", delegate {
            FarmModel farm = Farm();
            farm.State.plots[0].soilQuality = 2;
            farm.Apply(0, FarmTool.Plant, "carrot", Start);
            Assert(farm.Harvest(0, 3, Start + 100).success, "Perfect harvest failed");
            Equal(0, farm.Count("carrot")); Equal(7, farm.CountPrime("carrot")); // 3+2 soil+3 score-1
            farm.Apply(1, FarmTool.Plant, "carrot", Start + 101);
            Assert(farm.Harvest(1, 2, Start + 200).success, "Skilled harvest failed");
            Equal(4, farm.CountFine("carrot"));
            farm.Apply(2, FarmTool.Plant, "carrot", Start + 201);
            Assert(farm.Harvest(2, 1, Start + 300).success, "Plain harvest failed");
            Equal(3, farm.Count("carrot"));
            farm.ObserveTime(Start + 400); // step outside the festival price window
            Equal(3 * 5 + 4 * 7 + 7 * 9, farm.SellValue()); // base 5, ★★ ceil(6.75)=7, ★★★ 9
            Assert(farm.SellAll().success, "Sale failed");
            Equal(0, farm.CountFine("carrot")); Equal(0, farm.CountPrime("carrot"));
            string reason; Assert(FarmModel.Validate(farm.State, out reason), "Star save rejected: " + reason);
        });
        Test("Pests appear on warm dry plots, cut yield and are caught by skill", delegate {
            FarmModel farm = Farm();
            var plot = farm.State.plots[0];
            plot.cropId = "carrot"; plot.plantedAt = 0; plot.soilWater = .1;
            farm.State.climateTime = 295;
            farm.AdvanceEcology(1);
            Equal(1, plot.pest);
            Equal(2, farm.HarvestYield(0, 1)); // 3 - 1 pest
            Assert(!farm.CatchPests(0, 1).success, "Weak catch cleared pest");
            Equal(1, plot.pest);
            Assert(farm.CatchPests(0, 3).success, "Full catch failed");
            Equal(0, plot.pest); Assert(plot.treatmentProtection >= 30, "No protection reward");
            Assert(!farm.CatchPests(0, 3).success, "Caught on a clean plot");
        });
        Test("Transplant minigame deepens the nursery head start", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000; farm.GainExperience(400);
            farm.State.development.wood = 60; farm.State.development.ore = 20;
            Assert(farm.UpgradeBuilding("nursery").success, "Nursery build failed");
            Assert(farm.StartNursery("carrot", Start).success, "Nursery start failed");
            double ready = Start + 45 * .25 + 1;
            Assert(farm.Apply(0, FarmTool.Plant, "carrot", ready, 1).success, "Transplant failed");
            Assert(farm.Progress(0, ready) >= .349 && farm.Progress(0, ready) < .40, "Head start not 35%");
        });
        Test("Ducks lay eggs and pigs compost scraps into the fertility loop", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000;
            Assert(farm.BuyAnimal("duck").success && farm.BuyAnimal("pig").success, "Buy failed");
            Assert(farm.FeedAnimal("duck", false, Start).success && farm.FeedAnimal("pig", false, Start).success, "Feed failed");
            Assert(farm.CollectAnimal("duck", Start + 81).success, "Duck collect failed");
            Equal(2, farm.State.eggs);
            Assert(farm.CollectAnimal("pig", Start + 151).success, "Pig collect failed");
            Equal(2, farm.ItemCount("compost"));
            Assert(farm.UseCareProduct("compost", 0).success, "Pig compost unusable");
            Equal(1, farm.State.plots[0].soilQuality);
            string reason; Assert(FarmModel.Validate(farm.State, out reason), "Pen save rejected: " + reason);
        });
        Test("Achievements pay once when their milestone is reached", delegate {
            FarmModel farm = Farm();
            Assert(!farm.ClaimAchievement(0).success, "Claimed before milestone");
            farm.State.harvested = 10;
            Assert(farm.ClaimAchievement(0).success, "Claim failed");
            Equal(170, farm.State.coins);
            Assert(farm.AchievementClaimed(0), "Bit not set");
            Assert(!farm.ClaimAchievement(0).success, "Double claim allowed");
            farm.State.hasSold = true;
            Assert(farm.ClaimAchievement(2).success, "Sale achievement failed");
            string reason; Assert(FarmModel.Validate(farm.State, out reason), "Achievement save rejected: " + reason);
        });
        Test("Bees need blooms, make honey and pollinate fruiting plots", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000; farm.GainExperience(400);
            Assert(farm.BuyAnimal("bee").success, "Buy hive failed");
            Assert(!farm.FeedAnimal("bee", false, Start).success, "Bees foraged without flowers");
            farm.Apply(0, FarmTool.Plant, "marigold", Start);
            farm.Apply(1, FarmTool.Plant, "beans", Start);
            int coins = farm.State.coins;
            Assert(farm.FeedAnimal("bee", false, Start + 1).success, "Release failed with blooms");
            Equal(coins, farm.State.coins); // releasing bees is free
            Assert(farm.CollectAnimal("bee", Start + 122).success, "Honey collect failed");
            Equal(2, farm.State.honey);
            farm.Apply(2, FarmTool.Plant, "tomato", Start + 130);
            Assert(farm.Harvest(2, 1, Start + 231).success, "Tomato harvest failed");
            Equal(4, farm.Count("tomato")); // 3 base + 1 pollination
            string reason; Assert(FarmModel.Validate(farm.State, out reason), "Bee save rejected: " + reason);
        });
        Test("The fish pond raises stock sold with the harvest boat", delegate {
            FarmModel farm = Farm(); farm.State.coins = 1000;
            Assert(farm.BuyAnimal("fish").success, "Buy fingerlings failed");
            Assert(farm.FeedAnimal("fish", false, Start).success, "Feed fish failed");
            Assert(!farm.CollectAnimal("fish", Start + 100).success, "Collected early");
            Assert(farm.CollectAnimal("fish", Start + 141).success, "Fish collect failed");
            Equal(2, farm.State.fishStock);
            int value = farm.SellValue();
            Assert(value >= 16, "Fish not priced");
            Assert(farm.SellAll().success, "Sale failed");
            Equal(0, farm.State.fishStock);
        });
        Test("Weekly quests rotate, pay once and reset next week", delegate {
            FarmModel farm = Farm();
            farm.ObserveTime(Start);
            Equal(3, farm.State.quests.Count);
            var quest = farm.State.quests[0];
            quest.kind = 0; quest.target = 2; quest.baseline = farm.State.harvested;
            Assert(!farm.ClaimQuest(0).success, "Claimed unfinished quest");
            farm.Apply(0, FarmTool.Plant, "carrot", Start); farm.Harvest(0, 1, Start + 100);
            farm.Apply(0, FarmTool.Plant, "carrot", Start + 101); farm.Harvest(0, 1, Start + 200);
            Assert(farm.ClaimQuest(0).success, "Claim failed");
            Assert(!farm.ClaimQuest(0).success, "Double claim allowed");
            int week = farm.State.questWeek;
            farm.ObserveTime(Start + 604800);
            Equal(week + 1, farm.State.questWeek);
            Equal(3, farm.State.quests.Count);
            Assert(!farm.State.quests[0].claimed, "New week kept claimed flag");
            string reason; Assert(FarmModel.Validate(farm.State, out reason), "Quest save rejected: " + reason);
        });
        Test("The Firefly Festival boosts sale prices by twenty percent", delegate {
            FarmModel farm = Farm();
            farm.State.inventory.Find(e => e.cropId == "carrot").count = 10;
            farm.ObserveTime(Start);
            Assert(!FarmModel.FestivalActive(farm.State.lastSeenUtc), "Festival unexpectedly active");
            Equal(50, farm.SellValue());
            farm.ObserveTime(Start + 360);
            Assert(FarmModel.FestivalActive(farm.State.lastSeenUtc), "Festival window missed");
            Equal(60, farm.SellValue());
            Assert(farm.SellAll().success, "Festival sale failed");
            Equal(180, farm.State.coins);
        });
        Test("Kitchen migrates legacy saves and rejects corrupt nested state", delegate {
            var state=FarmModel.NewState(Start);Equal(null,state.hotpot);var f=new FarmModel(state);Equal(0,f.State.hotpot.stock.Count);
            string reason;Assert(FarmModel.Validate(state,out reason),reason);
            state.hotpot.stock.Add(new KitchenStock{id="carrot",count=1});
            state.hotpot.stock.Add(new KitchenStock{id="carrot",count=1});Assert(!FarmModel.Validate(state,out reason),"Duplicate stock accepted");
            state.hotpot.stock.Clear();state.hotpot.floors=0;Assert(!FarmModel.Validate(state,out reason),"Zero floors accepted");
            state.hotpot.floors=1;state.hotpot.recipes.Add(Recipe());Assert(!FarmModel.Validate(state,out reason),"Unsaved recipe id accepted");
        });
        Test("Supplier cannot sell farm produce and all transfers are atomic", delegate {
            var f=KitchenFarm();int coins=f.State.coins,stock=f.KitchenCount("carrot"),available=f.Count("carrot");
            Assert(!f.BuyKitchen("carrot",1).success,"Supplier sells crops");
            Assert(!f.ImportKitchen("carrot",1,1000).success,"Overdraw imported");
            Assert(!f.ImportKitchen("base",1,1).success,"Supplier stock imported");
            Assert(!f.BuyKitchen("base",-1).success,"Negative purchase");
            Equal(coins,f.State.coins);Equal(stock,f.KitchenCount("carrot"));Equal(available,f.Count("carrot"));
            f.State.coins=0;Assert(!f.BuyKitchen("base",1).success,"Unpaid purchase");
            f.State.fishStock=2;Assert(f.ImportKitchen("fish",1,2).success,"Fish transfer");Equal(0,f.State.fishStock);
            Assert(!f.ImportKitchen("fish",2,1).success,"Invented fish grade");
        });
        Test("Star transfers preserve grade and farm sales do not touch the kitchen", delegate {
            var f=KitchenFarm();f.State.primeGoods.Add(new InventoryEntry("carrot",5));
            Assert(f.ImportKitchen("carrot",3,3).success,"Prime import");Equal(2,f.CountPrime("carrot"));Equal(3,f.KitchenCount("carrot",3));
            Assert(f.SellAll().success,"Farm sale failed");Equal(3,f.KitchenCount("carrot",3));Equal(10,f.KitchenCount("corn"));
        });
        Test("Taste rewards matching methods and quality, penalizes salty and clashing recipes", delegate {
            var r=Recipe();int balanced=HotpotRules.Taste(r).Score;r.salt=3;Assert(HotpotRules.Taste(r).Score<balanced,"Salt penalty missing");
            r=Recipe();r.portions[0].stars=3;r.portions[1].stars=3;Assert(HotpotRules.Taste(r).Score>balanced,"Quality bonus missing");
            r=Recipe();r.technique=1;Assert(HotpotRules.Taste(r).Score<balanced,"Technique penalty missing");
            r=Recipe();r.style=1;r.broth=1;r.heat=2;int spicy=HotpotRules.Taste(r).Score;r.heat=0;Assert(HotpotRules.Taste(r).Score<spicy,"Spicy style not respected");
            r.portions.Clear();r.portions.Add(new HotpotPortion("milk",1,1));r.portions.Add(new HotpotPortion("tomato",1,1));
            Assert(HotpotRules.Taste(r).notes.Exists(x=>x.Contains("Sữa")),"Clash feedback missing");
        });
        Test("Recipe tests consume once, save a deep copy and never mutate on failure", delegate {
            var f=KitchenFarm();var r=Recipe();Assert(f.TestHotpotRecipe(r).success,"Test refused");
            r.portions[0].amount=3;Equal(1,f.State.hotpot.recipes[0].portions[0].amount);
            r.portions.Add(new HotpotPortion("carrot",1,1));int stock=f.KitchenCount("carrot");
            Assert(!f.TestHotpotRecipe(r).success,"Duplicate ingredient accepted");Equal(stock,f.KitchenCount("carrot"));Equal(1,f.State.hotpot.recipes.Count);
            r=Recipe();r.portions[0].stars=3;Assert(!f.TestHotpotRecipe(r).success,"Missing grade accepted");Equal(stock,f.KitchenCount("carrot"));
            r=Recipe();r.portions.Clear();r.portions.Add(new HotpotPortion("beef",1,1));r.portions.Add(new HotpotPortion("shrimp",1,1));Assert(!HotpotRules.ValidRecipe(r),"Farm-free recipe accepted");
        });
        Test("Serving insufficient stock or a full wallet never consumes ingredients", delegate {
            var f=KitchenFarm();f.TestHotpotRecipe(Recipe());f.SetHotpotMenu(1,true);
            int stock=f.KitchenCount("carrot");f.State.coins=1000000000;
            Assert(!f.ServeHotpotRecipe(1).success,"Wallet overflow");Equal(stock,f.KitchenCount("carrot"));Equal(0,f.State.hotpot.reviews);
            f.State.coins=100;f.State.hotpot.stock.Find(x=>x.id=="base").count=0;
            Assert(!f.ServeHotpotRecipe(1).success,"Missing base served");Equal(stock,f.KitchenCount("carrot"));Equal(100,f.State.coins);
        });
        Test("Floor expansion and furniture validate costs capacity and moves", delegate {
            var f=KitchenFarm();Assert(!f.ExpandHotpot().success,"Expanded without fame");f.State.hotpotServed=100;
            Assert(f.ExpandHotpot().success&&f.ExpandHotpot().success&&f.ExpandHotpot().success,"Expansion refused");
            Assert(!f.ExpandHotpot().success,"Fifth floor created");
            Assert(!f.PlaceHotpotFurniture(0,4,0,0).success,"Reserved legacy slot occupied");
            Assert(f.PlaceHotpotFurniture(1,0,1,0).success,"Place table");int coins=f.State.coins;
            Assert(!f.PlaceHotpotFurniture(1,0,1,0).success,"Overlap accepted");Equal(coins,f.State.coins);
            Assert(f.MoveHotpotFurniture(1,0,1,2).success,"Move refused");Equal(coins,f.State.coins);
            Equal(1,f.State.hotpot.furniture[0].slot);string reason;Assert(FarmModel.Validate(f.State,out reason),reason);
        });
        Test("Deleting stopped recipes frees notebook capacity without refunds or id reuse", delegate {
            var f=KitchenFarm();f.TestHotpotRecipe(Recipe());f.SetHotpotMenu(1,true);
            Assert(!f.RemoveHotpotRecipe(1).success,"Deleted active menu item");
            f.SetHotpotMenu(1,false);int stock=f.KitchenCount("corn"),coins=f.State.coins;
            Assert(f.RemoveHotpotRecipe(1).success,"Could not delete stopped recipe");
            Equal(stock,f.KitchenCount("corn"));Equal(coins,f.State.coins);Equal(0,f.State.hotpot.recipes.Count);
            f.TestHotpotRecipe(Recipe());Equal(2,f.State.hotpot.recipes[0].id);
        });
        Console.WriteLine("\n" + passed + " passed, " + failed + " failed");
        return failed == 0 ? 0 : 1;
    }
}
