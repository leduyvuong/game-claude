using System;
using HarvestIsle.Core;

internal static partial class FarmModelTests
{
    private static FarmModel JourneyFarm()
    {var f=Farm();f.State.coins=1000000;f.State.development.experience=400;return f;}
    private static void FreshJourneyHarvest(FarmModel f,string id,int stars)
    {
        if(!String.IsNullOrEmpty(f.State.plots[0].cropId))Assert(f.ChopPerennial(0).success,"Clear perennial");
        double now=f.State.lastSeenUtc+1;var p=f.State.plots[0];p.soilWater=.25;p.soilQuality=2;p.disease=0;p.pest=0;
        Assert(f.Apply(0,FarmTool.Plant,id,now).success,"Plant fresh crop");
        Assert(f.Harvest(0,stars,now+1000).success,"Harvest fresh crop");
    }
    private static void JourneyTests()
    {
        Test("Journey migration preserves existing farm and starts honest per-crop records",delegate {
            var s=FarmModel.NewState(Start);s.harvested=1000;s.coins=876;s.inventory[0].count=45;
            var f=new FarmModel(s);Equal(876,f.State.coins);Equal(45,f.Count("carrot"));Equal(0,f.CropHarvests("carrot"));Equal(1,f.JourneyYear);
            Assert(!f.AcceptJourneyContract(0).success,"Locked before level 3");
        });
        Test("Unity zero-filled optional classes migrate only when every field is empty",delegate {
            var s=FarmModel.NewState(Start);s.journey=new JourneyState{nextSerial=0,active=new JourneyContract()};
            var f=new FarmModel(s);Equal(1,f.State.journey.nextSerial);Assert(f.State.journey.active==null,"Empty contract retained");
            f.State.journey.active=new JourneyContract();f=new FarmModel(f.State);Assert(f.State.journey.active==null,"Inline null not normalized");
            f.State.journey.active=new JourneyContract{reward=10};string reason;Assert(!FarmModel.Validate(f.State,out reason),"Corrupt partial contract accepted as empty");
        });
        Test("Journey offers remain reachable and bounded across 200 years",delegate {
            var f=JourneyFarm();
            for(int season=0;season<800;season++)
            {
                f.State.journey.seasons=season;f.State.journey.completed=season*12;f.State.journey.nextSerial=season*12+1;
                for(int slot=0;slot<3;slot++)
                {var c=f.JourneyOffer(slot);var again=f.JourneyOffer(slot);Equal(c.item,again.item);Equal(c.reward,again.reward);Assert(c.target<=8&&c.quantity<=20,"Unbounded grind");Assert(CropCatalog.Find(c.item).unlockLevel<=f.Level,"Locked crop");}
            }
            f.State.development.experience=80;
            for(int i=0;i<3;i++)Assert(CropCatalog.Find(f.JourneyOffer(i).item).unlockLevel<=3,"Early farm offer locked");
        });
        Test("Old inventory and old harvests cannot instantly complete a journey contract",delegate {
            var f=JourneyFarm();var c=f.JourneyOffer(0);
            for(int i=0;i<8;i++)FreshJourneyHarvest(f,c.item,1);
            Assert(f.AcceptJourneyContract(0).success,"Accept");c=f.State.journey.active;
            int coins=f.State.coins;Assert(!f.CompleteJourneyContract(c.serial).success,"Old activity credited");Equal(coins,f.State.coins);Equal(0,f.JourneyFreshProgress(c));
            for(int i=0;i<c.target;i++)FreshJourneyHarvest(f,c.item,1);
            Assert(f.CompleteJourneyContract(c.serial).success,"Fresh work rejected");Equal(1,f.State.journey.completed);
            coins=f.State.coins;Assert(!f.CompleteJourneyContract(c.serial).success,"Duplicate reward");Equal(coins,f.State.coins);
        });
        Test("Normal journey delivery spends lower quality first and remains atomic at wallet cap",delegate {
            var f=JourneyFarm();f.AcceptJourneyContract(0);var c=f.State.journey.active;
            for(int i=0;i<c.target;i++)FreshJourneyHarvest(f,c.item,1);
            f.State.inventory.Find(x=>x.cropId==c.item).count=1;
            f.State.fineGoods.Add(new InventoryEntry(c.item,2));f.State.primeGoods.Add(new InventoryEntry(c.item,c.quantity+3));
            f.State.coins=1000000000;Assert(!f.CompleteJourneyContract(c.serial).success,"Overflow paid");Equal(1,f.Count(c.item));Equal(2,f.CountFine(c.item));Equal(0,f.State.journey.completed);
            f.State.coins=1000;Assert(f.CompleteJourneyContract(c.serial).success,"Complete");Equal(0,f.Count(c.item));Equal(0,f.CountFine(c.item));Equal(6,f.CountPrime(c.item));
        });
        Test("Quality contracts require fresh three-star harvests and consume only three-star goods",delegate {
            var f=JourneyFarm();Assert(f.AcceptJourneyContract(2).success,"Accept");var c=f.State.journey.active;Equal(2,c.kind);
            FreshJourneyHarvest(f,c.item,1);Equal(0,f.JourneyFreshProgress(c));int normal=f.Count(c.item);
            for(int i=0;i<c.target;i++)FreshJourneyHarvest(f,c.item,3);
            int prime=f.CountPrime(c.item);Assert(f.CompleteJourneyContract(c.serial).success,"Complete prime");Equal(normal,f.Count(c.item));Equal(prime-c.quantity,f.CountPrime(c.item));
        });
        Test("Factory contracts count collected products after acceptance",delegate {
            var f=JourneyFarm();f.State.development.wood=1000;f.UpgradeBuilding("mill");
            foreach(var e in f.State.inventory)e.count=100;
            var offer=f.JourneyOffer(1);Equal(1,offer.kind);
            Assert(f.StartProduction(offer.item,Start).success,"Old job");Assert(f.CollectProduction(1,Start+1000).success,"Old collect");
            f.AcceptJourneyContract(1);var c=f.State.journey.active;Equal(0,f.JourneyFreshProgress(c));
            for(int i=0;i<c.target;i++){int job=f.State.development.nextJobId;Assert(f.StartProduction(c.item,f.State.lastSeenUtc).success,"New job");Assert(f.CollectProduction(job,f.State.lastSeenUtc+1000).success,"New collect");}
            Assert(f.CompleteJourneyContract(c.serial).success,"Factory delivery");Equal(1,f.ItemCount(c.item));
        });
        Test("Factory offers require unlocked ingredient chains and owned dairy animals",delegate {
            var f=JourneyFarm();f.State.development.wood=1000;f.State.development.ore=1000;
            f.UpgradeBuilding("mill");f.UpgradeBuilding("bakery");f.UpgradeBuilding("dairy");f.State.development.experience=80;
            for(int i=0;i<12;i++){f.State.journey.completed=i;f.State.journey.nextSerial=i+1;string id=f.JourneyOffer(1).item;Assert(id=="flour"||id=="bread"||id=="riceBag","Unreachable ingredients: "+id);}
        });
        Test("Restaurant journey contracts count real servings and do not deduct meals twice",delegate {
            var f=KitchenFarm();f.State.development.experience=400;
            Assert(f.TestHotpotRecipe(Recipe()).success,"Create dish");int recipe=f.State.hotpot.recipes[0].id;f.SetHotpotMenu(recipe,true);
            f.State.journey.completed=1;f.State.journey.nextSerial=2;
            Assert(f.AcceptJourneyContract(2).success,"Accept feast");var c=f.State.journey.active;Equal(3,c.kind);
            for(int i=0;i<c.target;i++)Assert(f.ServeHotpotRecipe(recipe).success,"Serve meal");
            string stock=String.Join(",",f.State.hotpot.stock.ConvertAll(x=>x.count.ToString()).ToArray());
            Assert(f.CompleteJourneyContract(c.serial).success,"Complete feast");Equal(stock,String.Join(",",f.State.hotpot.stock.ConvertAll(x=>x.count.ToString()).ToArray()));
        });
        Test("Cancelling keeps inventory and rejects stale serials after accepting again",delegate {
            var f=JourneyFarm();f.AcceptJourneyContract(0);var c=f.State.journey.active;FreshJourneyHarvest(f,c.item,1);
            int coins=f.State.coins,count=f.Count(c.item);Assert(f.CancelJourneyContract(c.serial).success,"Cancel");Equal(coins,f.State.coins);Equal(count,f.Count(c.item));
            f.AcceptJourneyContract(0);Assert(f.State.journey.active.serial!=c.serial,"Serial reused");Equal(0,f.JourneyFreshProgress(f.State.journey.active));
            Assert(!f.CompleteJourneyContract(c.serial).success&&!f.CancelJourneyContract(c.serial).success,"Stale request accepted");
        });
        Test("Ten years of actual harvest contracts progress without resetting the farm",delegate {
            var f=JourneyFarm();
            for(int season=0;season<40;season++)
            {
                for(int i=0;i<12;i++)
                {
                    Assert(f.AcceptJourneyContract(0).success,"Accept next");var c=f.State.journey.active;
                    for(int n=0;n<c.target;n++)FreshJourneyHarvest(f,c.item,1);
                    Assert(f.CompleteJourneyContract(c.serial).success,"Complete next");
                }
                Equal(12,f.JourneySeasonProgress);Assert(!f.AcceptJourneyContract(0).success,"Skipped summary");Assert(f.AdvanceJourneySeason().success,"Advance");
                Assert(!f.AdvanceJourneySeason().success,"Duplicate season medal");string reason;Assert(FarmModel.Validate(f.State,out reason),reason);
            }
            Equal(11,f.JourneyYear);Equal(480,f.State.journey.completed);Equal(24,f.State.plots.Count);Assert(f.CropHarvests("carrot")>0,"Mastery reset");
        });
        Test("Projects require medals and only accelerate newly queued work",delegate {
            var f=JourneyFarm();f.State.development.wood=1000;f.State.development.ore=1000;f.UpgradeBuilding("mill");f.UpgradeBuilding("nursery");
            foreach(var e in f.State.inventory)e.count=100;
            Assert(!f.UpgradeJourneyProject(0).success,"Money bypassed medals");
            f.StartProduction("flour",Start);double old=f.State.development.jobs[0].readyAt;f.StartNursery("carrot",Start);double seed=f.State.development.seedlings[0].readyAt;
            f.State.journey.medals=1000;
            for(int kind=0;kind<3;kind++)for(int i=0;i<5;i++)Assert(f.UpgradeJourneyProject(kind).success,"Project upgrade");
            Equal(old,f.State.development.jobs[0].readyAt);Equal(seed,f.State.development.seedlings[0].readyAt);
            Assert(Math.Abs(f.ProductionDuration(DevelopmentCatalog.Recipe("flour"),1)-36)<.001,"Factory bonus");
            Assert(Math.Abs(f.NurseryDuration(CropCatalog.Find("carrot"))-9)<.001,"Nursery bonus");
            Assert(!f.UpgradeJourneyProject(0).success&&!f.UpgradeJourneyProject(-1).success,"Invalid upgrade");
        });
        Test("Mastery counts real harvest cycles including perennials and has bounded reward",delegate {
            var f=JourneyFarm();for(int i=0;i<10;i++)FreshJourneyHarvest(f,"carrot",1);Equal(1,f.CropMastery("carrot"));
            var r=f.State.journey.crops.Find(x=>x.id=="carrot");r.count=1000;Equal(5,f.CropMastery("carrot"));r.count=1000000;Equal(5,f.CropMastery("carrot"));
            f.Apply(1,FarmTool.Plant,"mango",f.State.lastSeenUtc);f.Harvest(1,1,f.State.lastSeenUtc+1000);f.Harvest(1,1,f.State.lastSeenUtc+1000);Equal(2,f.CropHarvests("mango"));
        });
        Test("Journey validation rejects corrupt counters duplicate records and impossible contracts",delegate {
            var f=JourneyFarm();string reason;f.AcceptJourneyContract(0);Assert(FarmModel.Validate(f.State,out reason),reason);
            f.State.journey.active.baseline=999;Assert(!FarmModel.Validate(f.State,out reason),"Future baseline");f.State.journey.active.baseline=0;
            f.State.journey.active.serial++;Assert(!FarmModel.Validate(f.State,out reason),"Wrong serial");f.State.journey.active.serial--;
            f.State.journey.crops.Add(new JourneyRecord{id="carrot"});f.State.journey.crops.Add(new JourneyRecord{id="carrot"});Assert(!FarmModel.Validate(f.State,out reason),"Duplicate mastery");
            f.State.journey.crops.Clear();f.State.journey.seasons=1;Assert(!FarmModel.Validate(f.State,out reason),"Skipped season");
        });
        Test("Years away and a backward clock neither expire contracts nor advance seasons",delegate {
            var f=JourneyFarm();f.AcceptJourneyContract(0);int serial=f.State.journey.active.serial;
            f.ObserveTime(Start+10*365*86400.0);f.ObserveTime(Start);Equal(serial,f.State.journey.active.serial);Equal(1,f.JourneyYear);Equal(0,f.JourneyFreshProgress(f.State.journey.active));
        });
        Test("Legacy provides an optional medal sink after the first year with bounded prices",delegate {
            var f=JourneyFarm();f.State.journey.medals=1000;Assert(!f.InvestJourneyLegacy().success,"Early legacy");
            f.State.journey.completed=48;f.State.journey.nextSerial=49;f.State.journey.seasons=4;
            int coins=f.State.coins;Assert(f.InvestJourneyLegacy().success,"Legacy rank");Equal(1,f.State.journey.legacyRank);Equal(coins-1000,f.State.coins);Equal(976,f.State.journey.medals);
            f.State.journey.legacyRank=100000;Equal(26000,f.JourneyLegacyCoins);Equal(64,f.JourneyLegacyMedals);
            double speed=f.ProductionDuration(DevelopmentCatalog.Recipe("flour"),1);Equal(45.0,speed);
        });
    }
}
