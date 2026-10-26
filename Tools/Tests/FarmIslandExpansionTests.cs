using System;
using System.Collections.Generic;
using HarvestIsle.Core;
internal static partial class FarmModelTests
{
    private static FarmModel IslandFarm(){var f=Farm();f.GainExperience(400);f.Island.chapter=3;f.State.coins=100000;return f;}
    private static int PlaceIsland(FarmModel f,string id,float x=-28,float z=-16)
    {Assert(f.BuyIslandThing(id).success,"Buy "+id);int serial=f.Island.owned[f.Island.owned.Count-1].serial;Assert(f.PlaceIslandThing(serial,x,z,0).success,"Place "+id);return serial;}
    private static void IslandInput(FarmModel f,string id,int amount)
    {
        if(IslandExpansionCatalog.Item(id)!=null){var s=f.Island.goods.Find(x=>x.id==id);if(s==null){s=new IslandStock{id=id};f.Island.goods.Add(s);}s.count+=amount;}
        else if(CropCatalog.Find(id)!=null)f.State.inventory.Find(x=>x.cropId==id).count+=amount;
        else if(id=="milk")f.State.milk+=amount;
        else {var s=f.State.development.products.Find(x=>x.id==id);if(s==null){s=new ProductStock{id=id};f.State.development.products.Add(s);}s.count+=amount;}
    }
    private static void IslandExpansionTests()
    {
        Test("Expansion has exact 30 crops 30 buildings 60 decor and valid recipe graph",delegate{
            Equal(30,IslandCropCatalog.Ids.Length);Equal(90,IslandExpansionCatalog.Things.Length);Equal(30,Array.FindAll(IslandExpansionCatalog.Things,x=>x.Building).Length);Equal(60,CropCatalog.All.Length);
            var ids=new HashSet<string>();foreach(var s in IslandExpansionCatalog.Things)Assert(ids.Add(s.id)&&s.price>0&&s.width>0&&s.depth>0,"Duplicate or empty thing");
            ids.Clear();foreach(var r in IslandExpansionCatalog.Recipes){Assert(ids.Add(r.id)&&r.inputs.Length==r.amounts.Length&&r.inputs.Length>0,"Recipe identity");Assert(IslandExpansionCatalog.Thing(r.output)!=null||IslandExpansionCatalog.Item(r.output)!=null,"Output missing");for(int i=0;i<r.inputs.Length;i++){string id=r.inputs[i];Assert(r.amounts[i]>0&&(CropCatalog.Find(id)!=null||IslandExpansionCatalog.Item(id)!=null||DevelopmentCatalog.Recipe(id)!=null||id=="milk"),"Input missing");Assert(IslandCropCatalog.Chapter(id)<=r.chapter,"Recipe opens before crop");}}
        });
        Test("Expansion old null and zero-filled saves preserve money roster crops and inventory",delegate{
            var f=Farm();f.State.coins=287;f.Apply(0,FarmTool.Plant,"carrot",Start);int coins=f.State.coins;f.State.islandExpansion=null;var migrated=new FarmModel(f.State);Equal(coins,migrated.State.coins);Equal("carrot",migrated.State.plots[0].cropId);Equal(0,migrated.Island.chapter);Equal(72,WorkerCatalog.All.Length);
            f.State.islandExpansion=new IslandExpansionState{nextSerial=0,owned=null,goods=null,jobs=null,journal=null,reserved=null,pruning=null};migrated=new FarmModel(f.State);Equal(1,migrated.Island.nextSerial);Equal(coins,migrated.State.coins);
        });
        Test("Expansion chapter gates and orders never offer inaccessible crops",delegate{
            var f=Farm();Assert(!f.BeginIslandChapter().success,"Level bypass");f.GainExperience(400);Assert(!f.Apply(0,FarmTool.Plant,"coffee",Start).success,"Chapter bypass");
            for(int i=0;i<90;i++){f.State.development.ordersCompleted=i;Assert(IslandCropCatalog.Index(f.ChoiceOrderItem(1))<0,"Locked order");}
            Assert(f.BeginIslandChapter().success,"M1");Assert(f.Apply(0,FarmTool.Plant,"peanut",Start).success,"M1 seed");Assert(!f.BeginIslandChapter().success,"Progress bypass");f.Island.completed=3;Assert(f.BeginIslandChapter().success,"M2");f.Island.completed=8;Assert(f.BeginIslandChapter().success,"M3");Assert(!f.BeginIslandChapter().success,"Repeat chapter");Equal(10,f.Level);
        });
        Test("Expansion leaf crops last exactly three harvests and trees keep their trunks",delegate{
            foreach(string id in new[]{"pandan","basil","perilla","mint"}){var f=IslandFarm();Assert(f.Apply(0,FarmTool.Plant,id,Start).success,"Sow leaf");for(int n=1;n<=3;n++){Assert(f.Harvest(0,1,Start+n*500).success,"Leaf harvest");Equal(n<3?id:"",f.State.plots[0].cropId);}Assert(!f.Harvest(0,1,Start+2000).success,"Fourth harvest");}
            var tree=IslandFarm();tree.Apply(0,FarmTool.Plant,"cacao",Start);tree.Harvest(0,1,Start+1000);Equal("cacao",tree.State.plots[0].cropId);Assert(tree.ChopPerennial(0).success,"Clear tree");
        });
        Test("Expansion purchase placement overlap roads rotation mounts and storage are atomic",delegate{
            var f=IslandFarm();int b=PlaceIsland(f,"B01"),coins=f.State.coins;Assert(!f.BuyIslandThing("B01").success,"Duplicate building");Equal(coins,f.State.coins);
            int d=PlaceIsland(f,"D01",-20,-16);Assert(!f.PlaceIslandThing(d,-28,-16,0).success,"Overlap");Assert(!f.PlaceIslandThing(d,0,0,0).success,"Road");Assert(!f.PlaceIslandThing(d,100,0,0).success,"Bounds");Assert(!f.PlaceIslandThing(d,float.NaN,0,0).success,"NaN");
            Assert(f.StoreIslandThing(d).success&&f.PlaceIslandThing(d,-20,-16,3).success,"Store/reuse");int before=f.State.coins;Assert(f.BuyIslandThing("D16").success,"Hanging decor");int mounted=f.Island.owned[f.Island.owned.Count-1].serial;Assert(!f.PlaceIslandThing(mounted,-28,-16,0).success,"Floating wall object");float mx,mz;FarmModel.IslandMountPosition(f.IslandBuilding("B01"),0,out mx,out mz);Assert(f.PlaceIslandThing(mounted,mx,mz,0,b).success,"Mount");Assert(!f.StoreIslandThing(b).success,"Removed support");Assert(f.StoreIslandThing(mounted).success&&f.StoreIslandThing(b).success,"Unhook then store");Equal(before-IslandExpansionCatalog.Thing("D16").price,f.State.coins);
        });
        Test("Expansion water decoration and shore require the designated pond",delegate{
            var f=IslandFarm();f.BuyIslandThing("D26");int serial=f.Island.owned[0].serial;Assert(!f.PlaceIslandThing(serial,0,12,0).success,"Water on land");Assert(f.PlaceIslandThing(serial,-35,23,0).success,"Pond placement");f.BuyIslandThing("D21");serial=f.Island.owned[1].serial;Assert(!f.PlaceIslandThing(serial,10,12,0).success,"Shore on land");Assert(f.PlaceIslandThing(serial,-28,23,0).success,"Shore placement");
        });
        Test("Every expansion recipe consumes exact ingredients and collects once after reload",delegate{
            foreach(var r in IslandExpansionCatalog.Recipes)
            {
                var f=IslandFarm();if(r.building.StartsWith("B"))PlaceIsland(f,r.building);else f.State.development.buildings.Add(new BuildingState{id=r.building,level=1});
                for(int i=0;i<r.inputs.Length;i++)IslandInput(f,r.inputs[i],r.amounts[i]);
                Assert(f.StartIslandRecipe(r.id,Start).success,"Start "+r.id);for(int i=0;i<r.inputs.Length;i++)Equal(0,f.IslandCount(r.inputs[i]));
                int serial=f.Island.jobs[0].serial;Assert(!f.CollectIslandJob(serial,Start).success,"Early collect");f=new FarmModel(f.State);Assert(f.CollectIslandJob(serial,Start+1000).success,"Collect "+r.id);Assert(!f.CollectIslandJob(serial,Start+1000).success,"Double collect");
                if(IslandExpansionCatalog.Thing(r.output)==null)Equal(r.count,f.IslandCount(r.output));else Assert(f.Island.owned.Exists(p=>p.id==r.output&&!p.placed),"Crafted ownership");string reason;Assert(FarmModel.Validate(f.State,out reason),"Recipe save "+r.id+" "+reason);
            }
        });
        Test("Expansion insufficient ingredients full queue and full output do not lose stock",delegate{
            var f=IslandFarm();int b=PlaceIsland(f,"B01");IslandInput(f,"rice",2);int before=f.State.coins;Assert(!f.StartIslandRecipe("ix_noodles",Start).success,"Short input");Equal(2,f.Count("rice"));Equal(before,f.State.coins);
            IslandInput(f,"rice",4);Assert(f.StartIslandRecipe("ix_noodles",Start).success,"First");Assert(!f.StartIslandRecipe("ix_noodles",Start).success,"Queue capacity");Equal(3,f.Count("rice"));Assert(!f.StoreIslandThing(b).success,"Store while running");
            IslandInput(f,"ix_noodles",1000000);int serial=f.Island.jobs[0].serial;Assert(!f.CollectIslandJob(serial,Start+1000).success,"Full stock");Equal(1,f.Island.jobs.Count);
        });
        Test("Expansion three-input recipe keeps high-star stock untouched",delegate{
            var f=IslandFarm();PlaceIsland(f,"B08");foreach(string id in new[]{"ginger","turmeric","galangal"}){IslandInput(f,id,1);f.State.primeGoods.Add(new InventoryEntry(id,3));}Assert(f.StartIslandRecipe("ix_spice",Start).success,"Multi input");foreach(string id in new[]{"ginger","turmeric","galangal"})Equal(3,f.CountPrime(id));
        });
        Test("Expansion reserved produce preserves quantity grade and excludes bulk sales",delegate{
            var f=IslandFarm();PlaceIsland(f,"B28");f.State.primeGoods.Add(new InventoryEntry("coffee",7));Assert(f.ReserveIslandCrop("coffee",3,4).success,"Reserve");Equal(3,f.CountPrime("coffee"));int serial=f.Island.reserved[0].serial;f.SellAll();Equal(4,f.Island.reserved[0].count);f=new FarmModel(f.State);Assert(f.UnreserveIslandCrop(serial).success,"Unreserve");Equal(4,f.CountPrime("coffee"));Assert(!f.UnreserveIslandCrop(serial).success,"Duplicate return");
        });
        Test("Expansion kitchen mapping excludes raw cassava crafts and preserves old noodle IDs",delegate{
            var f=IslandFarm();IslandInput(f,"ix_noodles",4);Assert(f.ImportIslandKitchen("ix_noodles",3).success,"Noodle mapping");Equal(3,f.KitchenCount("riceNoodles"));Equal(1,f.IslandCount("ix_noodles"));Assert(HotpotRules.Find("cassava")==null,"Raw cassava");Assert(!f.ImportIslandKitchen("ix_paper",1).success&&!f.ImportIslandKitchen("ix_helpers",1).success,"Non-food");
            foreach(string id in IslandCropCatalog.Ids)if(id!="cassava"){IslandInput(f,id,1);Assert(f.ImportKitchen(id,1,1).success,"New crop kitchen "+id);}
        });
        Test("Expansion support actions execute once and survive a later clock",delegate{
            var f=IslandFarm();PlaceIsland(f,"B18");f.Apply(0,FarmTool.Plant,"basil",Start);Assert(f.ScheduleIslandWater(true,Start).success,"Schedule");Assert(f.TickIslandWater(Start),"Actual water");Equal(4,f.Island.waterJobs);Assert(!f.TickIslandWater(Start+100000),"No offline empty action");Equal(4,f.Island.waterJobs);
            Assert(f.ScheduleIslandWater(false,Start+100000).success,"Stop");Equal(0,f.Island.waterJobs);
        });
        Test("Expansion pruning exchanges current progress for one bounded next-harvest bonus",delegate{
            var f=IslandFarm();PlaceIsland(f,"B20");f.Apply(0,FarmTool.Plant,"coffee",Start);f.Harvest(0,1,Start+1000);int before=f.Count("coffee");Assert(f.PruneIslandTree(0,Start+1010).success,"Prune");Assert(!f.PruneIslandTree(0,Start+1011).success,"Stack prune");Assert(f.Harvest(0,1,Start+2000).success,"Harvest pruned");Equal(before+CropCatalog.Find("coffee").yield+1,f.Count("coffee"));Equal(0,f.Island.pruning.Count);
        });
        Test("Expansion service consumes once rewards first story once and validates stale serial",delegate{
            var f=IslandFarm();PlaceIsland(f,"B27");IslandInput(f,"ix_carekit",3);Assert(f.StartIslandService("B27",0,0,Start).success,"Rescue");Assert(!f.StartIslandService("B27",0,0,Start).success,"Replay");Equal(2,f.IslandCount("ix_carekit"));int serial=f.Island.jobs[0].serial;Assert(f.CollectIslandJob(serial,Start+100).success,"Finish");int owned=f.Island.owned.Count;Assert(f.StartIslandService("B27",0,1,Start+100).success,"Replay story");serial=f.Island.jobs[0].serial;f.CollectIslandJob(serial,Start+200);Equal(owned,f.Island.owned.Count);Equal(1,f.Island.journal.Count);
        });
        Test("Expansion lessons register outcomes once and invalid answers do not mutate",delegate{
            var f=IslandFarm();PlaceIsland(f,"B23");Assert(!f.AnswerIslandLesson(0,0).success,"Wrong answer");Equal(0,f.Island.lesson);Assert(f.AnswerIslandLesson(0,1).success,"Correct");Assert(!f.AnswerIslandLesson(0,1).success,"Stale lesson");Equal(1,f.Island.lesson);
        });
        Test("Expansion validation rejects duplicate serial invalid jobs and unknown stock",delegate{
            var f=IslandFarm();PlaceIsland(f,"B01");string why;Assert(FarmModel.Validate(f.State,out why),why);f.Island.owned.Add(f.Island.owned[0]);Assert(!FarmModel.Validate(f.State,out why),"Duplicate serial");f.Island.owned.RemoveAt(1);f.Island.goods.Add(new IslandStock{id="made-up",count=1});Assert(!FarmModel.Validate(f.State,out why),"Unknown goods");
        });
        Test("Expansion helpers protect an actual crop and remain valid across saves",delegate{
            var f=IslandFarm();PlaceIsland(f,"B19");f.Apply(0,FarmTool.Plant,"peanut",Start);IslandInput(f,"ix_helpers",2);
            Assert(f.ReleaseIslandHelpers(0).success,"Release helpers");Equal(1,f.IslandCount("ix_helpers"));Assert(!f.ReleaseIslandHelpers(0).success,"Duplicate protection");
            string why;Assert(FarmModel.Validate(f.State,out why),"Helper save: "+why);f=new FarmModel(f.State);Equal(45,(int)f.State.plots[0].treatmentProtection);
        });
        Test("Expansion seed soil exhibition and animal care have persistent distinct effects",delegate{
            var f=IslandFarm();PlaceIsland(f,"B16");PlaceIsland(f,"B17",-20,-16);PlaceIsland(f,"B22",4,-16);PlaceIsland(f,"B25",24,-16);
            IslandInput(f,"peanut",2);Assert(f.RegisterIslandSeed("peanut").success,"Register seed");Assert(!f.RegisterIslandSeed("peanut").success,"Duplicate dossier");Equal(1,f.Count("peanut"));
            int soil=f.State.plots[0].soilQuality;Assert(f.IslandSoilReport(0).Contains("Ô 1"),"Soil report");Equal(soil,f.State.plots[0].soilQuality);
            Assert(f.SetIslandExhibition("seed:peanut").success,"Exhibition");Assert(!f.SetIslandExhibition("seed:coffee").success,"Unowned exhibit");
            f.State.chicken.count=1;Assert(f.BrushIslandAnimal("chicken",Start).success,"Animal care");Assert(!f.BrushIslandAnimal("chicken",Start+1).success,"Rest cooldown");
            f=new FarmModel(f.State);Equal("seed:peanut",f.Island.exhibition);Assert(f.Island.journal.Contains("care:chicken"),"Care journal");string why;Assert(FarmModel.Validate(f.State,out why),why);
        });
        Test("Expansion all six timed services consume requirements and award each story once",delegate{
            foreach(string id in new[]{"B21","B24","B26","B27","B29","B30"})for(int choice=0;choice<3;choice++)
            {
                var f=IslandFarm();PlaceIsland(f,id);PlaceIsland(f,"B01",-20,-16);PlaceIsland(f,"B02",4,-16);
                PlaceIsland(f,"D01",24,-16);PlaceIsland(f,"D02",28,-16);PlaceIsland(f,"D03",32,-16);
                foreach(string input in new[]{"rice","ix_mat","ix_noodles","ix_peanutcandy","ix_coffee","ix_chocolate","ix_giftcandy","ix_carekit","ix_giftcoffee"})IslandInput(f,input,20);
                Assert(f.StartIslandService(id,choice,0,Start).success,"Service "+id);int serial=f.Island.jobs[0].serial;Assert(!f.CollectIslandJob(serial,Start+89).success,"Service time");
                Assert(f.CollectIslandJob(serial,Start+90).success,"Collect story");Assert(f.Island.journal.Contains(id+":"+choice),"Unique story");Equal(7,f.Island.owned.Count);string why;Assert(FarmModel.Validate(f.State,out why),id+" "+why);
            }
        });
        Test("Expansion mounted decorations follow each host rotation and reject occupied sockets",delegate{
            for(int turn=0;turn<4;turn++)
            {
                var f=IslandFarm();int host=PlaceIsland(f,"B02");f.PlaceIslandThing(host,-28,-16,turn);f.BuyIslandThing("D16");int serial=f.Island.owned[1].serial;float x,z;FarmModel.IslandMountPosition(f.IslandBuilding("B02"),0,out x,out z);
                Assert(f.PlaceIslandThing(serial,x,z,turn,host).success,"Rotated mount");f.BuyIslandThing("D07");Assert(!f.PlaceIslandThing(f.Island.owned[2].serial,x,z,turn,host).success,"Occupied socket");string why;Assert(FarmModel.Validate(f.State,out why),why);
            }
        });
        Test("Expansion sixty harvested species persist together in the journey notebook",delegate{
            var f=IslandFarm();double now=Start;
            foreach(var crop in CropCatalog.All)
            {
                f.State.plots[0].cropId="";f.State.plots[0].harvests=0;f.State.plots[0].watered=false;
                Assert(f.Apply(0,FarmTool.Plant,crop.id,now).success,"Sow collection");now+=1000;Assert(f.Harvest(0,1,now).success,"Harvest collection");
            }
            Equal(60,f.State.journey.crops.Count);string why;Assert(FarmModel.Validate(f.State,out why),"Sixty-crop save "+why);Equal(60,new FarmModel(f.State).State.journey.crops.Count);
        });
        Test("Expansion validation rejects malformed story keys placement and oversized queues",delegate{
            var f=IslandFarm();PlaceIsland(f,"B27");IslandInput(f,"ix_carekit",1);f.StartIslandService("B27",0,0,Start);string why;Assert(FarmModel.Validate(f.State,out why),why);
            f.Island.jobs[0].recipe="B27:extra:0";Assert(!FarmModel.Validate(f.State,out why),"Malformed service");f.Island.jobs[0].recipe="B27:0";
            f.Island.jobs.Add(new IslandJob{serial=f.Island.nextSerial++,recipe="B27:1",readyAt=Start+90});Assert(!FarmModel.Validate(f.State,out why),"Service capacity");f.Island.jobs.RemoveAt(1);
            f.Island.owned[0].x=0;f.Island.owned[0].z=0;Assert(!FarmModel.Validate(f.State,out why),"Saved building on road");f.Island.owned[0].x=-28;f.Island.owned[0].z=-16;
            f.Island.journal.Add("lesson:-1");Assert(!FarmModel.Validate(f.State,out why),"Malformed journal");
        });
    }
}
