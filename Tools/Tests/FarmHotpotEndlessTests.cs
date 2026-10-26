using System;
using System.Collections.Generic;
using HarvestIsle.Core;

internal static partial class FarmModelTests
{
    private static FarmModel EndlessFarm()
    {
        var f=KitchenFarm();f.TestHotpotRecipe(Recipe());f.SetHotpotMenu(1,true);
        foreach(var stock in f.State.hotpot.stock)stock.count=1000;
        f.State.coins=100000;return f;
    }
    private static void CookReady(HotpotServiceSession session,int key)
    {
        var order=session.Find(key);if(!order.answered)Assert(session.Answer(key,true),"Custom request failed");
        Assert(session.Cook(key),"Cook failed");for(int i=0;i<21;i++)session.Tick(1);
    }
    private static void HotpotEndlessTests()
    {
        Test("Endless migration preserves old money furniture fame and kitchen appearance",delegate {
            var state=FarmModel.NewState(Start);state.hotpotServed=40;state.coins=333;state.hotpot=new HotpotState();
            var f=new FarmModel(state);Equal(3,f.HotpotRank);Equal(3,f.KitchenLevel);Equal(333,state.coins);Equal(40,state.hotpot.progress.branchServed);
            string reason;Assert(FarmModel.Validate(state,out reason),reason);
        });
        Test("All 30 crops honey and edible processed goods transfer from actual farm stock",delegate {
            var f=Farm();foreach(var crop in CropCatalog.Original)
            {Assert(HotpotRules.Find(crop.id)!=null,"Missing crop "+crop.id);f.State.inventory.Find(x=>x.cropId==crop.id).count=1;Assert(f.ImportKitchen(crop.id,1,1).success,"Crop transfer");Equal(0,f.Count(crop.id));}
            f.State.honey=2;Assert(f.ImportKitchen("honey",1,1).success,"Honey transfer");Equal(1,f.State.honey);
            f.State.development.products.Add(new ProductStock{id="flour",count=2});Assert(f.ImportKitchen("flour",1,2).success,"Flour transfer");Equal(0,f.ItemCount("flour"));
            Assert(HotpotRules.Find("compost")==null&&HotpotRules.Find("herbal")==null,"Nonfood became edible");
            Assert(!f.BuyKitchen("mango",1).success&&!f.BuyKitchen("flour",1).success,"Farm bypass");
        });
        Test("Supplier unlocks and weekly quotes charge the displayed amount exactly",delegate {
            var f=EndlessFarm();int coins=f.State.coins;
            Assert(!f.BuyKitchen("lobster",1).success,"Locked seafood bought");Equal(coins,f.State.coins);
            int quote=f.KitchenUnitPrice("pork");Assert(f.BuyKitchen("pork",3).success,"New supplier item");Equal(coins-3*quote,f.State.coins);Equal(3,f.KitchenCount("pork"));
            Equal(quote,f.KitchenUnitPrice("pork"));
        });
        Test("Eight kitchen levels enforce funds rank parallel slots and faster cooking",delegate {
            var f=EndlessFarm();Assert(f.UpgradeHotpotKitchen().success,"Kitchen 2");Assert(!f.UpgradeHotpotKitchen().success,"Rank gate bypass");
            f.State.hotpot.progress.rank=8;f.State.coins=100000;int[] slots={1,1,2,4,4,4,4,6};
            for(int level=2;level<=8;level++){Equal(level,f.KitchenLevel);int effective=f.EffectiveKitchenLevel;Equal(slots[effective-1],f.HotpotBurners);if(level<8)Assert(f.UpgradeHotpotKitchen().success,"Upgrade "+level);}
            Assert(!f.UpgradeHotpotKitchen().success,"Level 9 created");Assert(f.HotpotCookSeconds(Recipe())<20,"No speed benefit");
        });
        Test("Guests and seasonal modifiers are deterministic unique and bounded",delegate {
            for(int season=0;season<400;season++)
            {
                int[] mods=HotpotRules.Modifiers(season);Equal(mods.Length,new HashSet<int>(mods).Count);Assert(mods.Length>=2&&mods.Length<=4,"Modifier count");
                var a=HotpotRules.Guest(17,season,10,season,1);var b=HotpotRules.Guest(17,season,10,season,1);Equal(a.archetype,b.archetype);Equal(a.request,b.request);Assert(a.threshold<=95&&a.budget>0,"Guest bounds");
            }
            Assert(HotpotRules.RankPots(30)>HotpotRules.RankPots(20),"Authored rank ceiling");Equal(95,HotpotRules.RankTaste(1000000000));Equal(1000000000,HotpotRules.RankPots(1000000000));
        });
        Test("Taste preferences and vegetarian penalty affect the real bill",delegate {
            var f=EndlessFarm();var r=Recipe();r.portions[0]=new HotpotPortion("beef",1,1);
            var guest=new HotpotGuest{archetype=3,sweet=1,salt=1,heat=0,sour=0,threshold=95,budget=10000};
            int normal=HotpotRules.Satisfaction(r,guest,20);guest.vegetarian=true;Equal(Math.Max(0,normal-15),HotpotRules.Satisfaction(r,guest,20));
            guest.salt=3;guest.heat=3;Assert(HotpotRules.Satisfaction(r,guest,20)<normal,"Preference ignored");
        });
        Test("Order reservation blocks overbooking and unavailable tables without charging",delegate {
            var f=EndlessFarm();f.State.hotpot.stock.Find(s=>s.id=="carrot").count=1;
            var session=new HotpotServiceSession(f,4);int coins=f.State.coins;
            Assert(session.Seat(0),"First table");Assert(!session.Seat(1),"Double-reserved last carrot");Assert(!session.Seat(26),"Locked floor seated");Equal(coins,f.State.coins);Equal(1,f.KitchenCount("carrot"));
        });
        Test("Cooking capacity timer payment and duplicate service are authoritative",delegate {
            var f=EndlessFarm();var s=new HotpotServiceSession(f,4);Assert(s.Seat(0)&&s.Seat(1),"Seat");
            if(!s.Find(0).answered)s.Answer(0,true);if(!s.Find(1).answered)s.Answer(1,true);
            Assert(!s.Serve(0),"Uncooked service");Assert(s.Cook(0)&&!s.Cook(1),"Burner capacity ignored");
            int stock=f.KitchenCount("carrot");for(int i=0;i<21;i++)s.Tick(1);
            Assert(s.Serve(0),"Ready dish not served");Equal(stock-1,f.KitchenCount("carrot"));int coins=f.State.coins;
            Assert(!s.Serve(0),"Double service");Equal(coins,f.State.coins);Assert(s.Cook(1),"Burner not released");Equal(1,f.State.hotpot.recipes[0].timesServed);
        });
        Test("Rejected custom orders release reservations without losing stock",delegate {
            var f=EndlessFarm();var s=new HotpotServiceSession(f,5);s.Seat(0);var o=s.Find(0);o.answered=false;o.guest.request=3;
            int stock=f.KitchenCount("carrot");Assert(s.Answer(0,false),"Decline failed");Equal(null,s.Find(0));Equal(stock,f.KitchenCount("carrot"));
            var r=Recipe();r.portions[0].id="onion";Assert(!HotpotRules.ValidRecipe(HotpotRules.Customize(r,3)),"One ingredient recipe accepted");Equal(2,r.portions.Count);
        });
        Test("Full wallets fail atomically and keep the prepared order available",delegate {
            var f=EndlessFarm();var s=new HotpotServiceSession(f,4);s.Seat(0);CookReady(s,0);f.State.coins=HotpotRules.Limit;int stock=f.KitchenCount("carrot");
            Assert(!s.Serve(0),"Overflow service");Equal(stock,f.KitchenCount("carrot"));Assert(!s.Find(0).served,"Order consumed on failure");
        });
        Test("Specialty discovery and mastery persist but copied recipes do not inherit XP",delegate {
            var f=EndlessFarm();f.State.hotpot.stock.Add(new KitchenStock{id="tomato",count=10});f.State.hotpot.stock.Add(new KitchenStock{id="eggs",count=10});
            var recipe=new HotpotRecipe{style=1,broth=1,heat=2,timesServed=200};recipe.portions.Add(new HotpotPortion("tomato",1,1));recipe.portions.Add(new HotpotPortion("eggs",1,1));
            Assert(f.TestHotpotRecipe(recipe).success,"Specialty cook");Equal(2,HotpotRules.Specialty(recipe));Assert(f.State.hotpot.progress.discoveries.Contains("special:2"),"Missing discovery");Equal(0,f.State.hotpot.recipes[1].timesServed);
            int normal=f.RecipePrice(recipe);recipe.timesServed=0;Assert(f.RecipePrice(recipe)<normal,"Mastery price missing");
        });
        Test("Repeated dishes lose price for that customer type and recover with time",delegate {
            var f=EndlessFarm();var r=f.State.hotpot.recipes[0];var g=HotpotRules.Guest(1,0,1,0,0);
            f.State.hotpot.progress.fatigue.Add(new HotpotFatigue{archetype=g.archetype,recipeId=r.id,streak=4,lastServed=Start});
            Assert(f.HotpotFatigueMultiplier(r,g,Start)<.6,"No fatigue");Equal(1d,f.HotpotFatigueMultiplier(r,g,Start+600));
            g.archetype=(g.archetype+1)%11;Equal(1d,f.HotpotFatigueMultiplier(r,g,Start));
        });
        Test("Promotion requires volume recent quality decor and a passed inspection",delegate {
            var f=EndlessFarm();Assert(f.HotpotPromotionMissing()!="","New shop can promote");var p=f.State.hotpot.progress;
            p.branchServed=100;for(int i=0;i<5;i++)p.recentScores.Add(100);f.PlaceHotpotFurniture(0,6,32,0);
            Equal("",f.HotpotPromotionMissing());Assert(!f.CompleteHotpotInspection(30),"Failed inspector promoted");Assert(f.CompleteHotpotInspection(100),"Inspector could not promote");Equal(2,f.HotpotRank);
        });
        Test("Season reward cannot be claimed twice and old timestamps cannot rewind seasons",delegate {
            var f=EndlessFarm();var p=f.State.hotpot.progress;p.seasonRevenue=HotpotRules.SeasonTarget(0);p.seasonStars=10;
            Assert(f.ClaimHotpotSeason().success,"Season reward");int coins=f.State.coins;Assert(!f.ClaimHotpotSeason().success,"Double season claim");Equal(coins,f.State.coins);
            int season=p.season;f.AdvanceHotpotSeason(Start-100);Equal(season,p.season);f.AdvanceHotpotSeason(Start+1209601);Equal(season+1,p.season);
        });
        Test("Operating fees cannot overdraw and debt blocks serving until paid",delegate {
            var f=EndlessFarm();f.State.coins=0;f.TickHotpotOperating(300);Equal(0,f.State.coins);Assert(f.State.hotpot.progress.operatingDebt>0,"Free operation");
            var s=new HotpotServiceSession(f,1);Assert(!s.Seat(0),"Debt ignored");f.State.coins=1000;Assert(f.PayHotpotDebt().success,"Pay debt");Equal(0,f.State.hotpot.progress.operatingDebt);
        });
        Test("Offline staff consume real ingredients once with an eight-hour cap",delegate {
            var f=EndlessFarm();var p=f.State.hotpot.progress;p.rank=5;p.waiter=1;p.assistant=1;p.cashier=1;
            Assert(f.SetHotpotOffline(true,Start).success,"Enable offline");int stock=f.KitchenCount("carrot");Assert(f.SettleHotpotOffline(Start+999999).success,"Offline settlement");
            int spent=stock-f.KitchenCount("carrot");Assert(spent>0&&spent<=48,"Offline cap");int coins=f.State.coins;f.SettleHotpotOffline(Start+999999);Equal(coins,f.State.coins);Equal(stock-spent,f.KitchenCount("carrot"));
        });
        Test("Prestige preserves recipes mastery stock but resets branch layout and rank",delegate {
            var f=EndlessFarm();var p=f.State.hotpot.progress;p.rank=8;p.seasonClaims=1;p.branchServed=1000;f.State.hotpot.recipes[0].timesServed=50;
            f.State.hotpot.floors=4;f.PlaceHotpotFurniture(1,0,8,0);int stock=f.KitchenCount("carrot");
            Assert(f.OpenHotpotBranch().success,"Prestige failed");Equal(1,p.branch);Equal(1,p.rank);Equal(1,f.State.hotpot.floors);Equal(0,f.State.hotpot.furniture.Count);Equal(50,f.State.hotpot.recipes[0].timesServed);Equal(stock,f.KitchenCount("carrot"));Assert(!f.ExpandHotpot().success,"Old lifetime fame opens branch floors");
        });
        Test("Rush generates successive waves and records only completed waves",delegate {
            var f=EndlessFarm();f.State.hotpot.progress.kitchenLevel=8;var s=new HotpotServiceSession(f,6);Assert(s.StartRush(),"Rush start");
            for(int second=0;second<181;second++)
            {
                for(int slot=0;slot<4;slot++){if(s.Find(slot)==null)s.Seat(slot);var o=s.Find(slot);if(o==null)continue;if(!o.answered)s.Answer(slot,true);if(!o.cooking&&!o.served)s.Cook(slot);if(o.ready&&!o.served)s.Serve(slot);}
                s.Tick(1);
            }
            Assert(f.State.hotpot.progress.bestWave>=1,"No completed wave recorded");s.EndRush();Assert(f.State.hotpot.progress.bestRushScore>0,"Rush score missing");
        });
        Test("Progress validation rejects bad state without accepting numeric overflow",delegate {
            var f=EndlessFarm();string error;Assert(FarmModel.Validate(f.State,out error),error);
            f.State.hotpot.progress.kitchenLevel=9;Assert(!FarmModel.Validate(f.State,out error),"Bad kitchen accepted");f.State.hotpot.progress.kitchenLevel=1;
            f.State.hotpot.progress.activeSeconds=double.NaN;Assert(!FarmModel.Validate(f.State,out error),"NaN accepted");f.State.hotpot.progress.activeSeconds=0;
            f.State.hotpot.progress.discoveries.Add("special:99");Assert(!FarmModel.Validate(f.State,out error),"Unknown discovery accepted");
        });
        Test("New farm processing chains feed the hotpot and never use supplier shortcuts",delegate {
            var f=EndlessFarm();f.State.development.buildings.Add(new BuildingState{id="kitchen",level=3});f.State.development.products.Add(new ProductStock{id="flour",count=1});f.State.fishStock=2;
            Assert(f.StartProduction("meatballs",Start).success,"Fish ball queue");Equal(0,f.State.fishStock);Equal(0,f.ItemCount("flour"));
            Assert(f.CollectProduction(f.State.development.jobs[0].id,Start+100).success,"Fish balls collect");
            Assert(f.ImportKitchen("meatballs",1,1).success,"Fish balls import");Equal(0,f.ItemCount("meatballs"));Assert(!f.BuyKitchen("meatballs",1).success,"Supplier bypasses farm factory");
            Assert(HotpotRules.Find("tofu").farm,"Tofu must be processed on farm");Equal(122,HotpotRules.Ingredients.Length);
        });
        Test("Zero-filled legacy progress migrates but partly corrupt progress does not",delegate {
            var state=FarmModel.NewState(Start);state.hotpot=new HotpotState{progress=new HotpotProgress{rank=0,kitchenLevel=0,reputation=0,recentScores=null,discoveries=null,fatigue=null}};
            var f=new FarmModel(state);Equal(1,f.HotpotRank);Equal(1,f.KitchenLevel);
            f.State.hotpot.progress.rank=0;string reason;Assert(!FarmModel.Validate(state,out reason),"Partial corruption silently reset");
        });
        Test("Restaurant staff use worker housing unlocks and never hire for free",delegate {
            var f=EndlessFarm();f.State.hotpot.progress.rank=5;int coins=f.State.coins;
            Assert(!f.UpgradeHotpotStaff(0).success,"No worker house required");Equal(coins,f.State.coins);
            f.State.workerHouse=1;Assert(f.UpgradeHotpotStaff(0).success,"Waiter hire");Assert(!f.UpgradeHotpotStaff(1).success,"Restaurant housing capacity ignored");
            f.State.workerHouse=3;Assert(f.UpgradeHotpotStaff(1).success&&f.UpgradeHotpotStaff(2).success,"Three roles");
            f.State.coins=0;Assert(!f.UpgradeHotpotStaff(0).success,"Unpaid staff upgrade");Equal(1,f.State.hotpot.progress.waiter);
        });
        Test("Assistant cooks and waiter serves real orders with the same stock transaction",delegate {
            var f=EndlessFarm();f.State.hotpot.progress.waiter=3;f.State.hotpot.progress.assistant=3;
            var s=new HotpotServiceSession(f,4);Assert(s.Seat(0),"Seat");s.Find(0).guest.request=0;s.Find(0).answered=true;s.Find(0).patience=120;
            int stock=f.KitchenCount("carrot");for(int i=0;i<35;i++)s.Tick(1);
            Equal(1,f.State.hotpotServed);Equal(stock-1,f.KitchenCount("carrot"));Equal(1,f.State.hotpot.recipes[0].timesServed);
        });
        Test("A wider menu attracts orders away from a fatigued recipe",delegate {
            var f=EndlessFarm();Assert(f.TestHotpotRecipe(Recipe()).success,"Second recipe");f.SetHotpotMenu(2,true);
            for(int type=0;type<12;type++)f.State.hotpot.progress.fatigue.Add(new HotpotFatigue{archetype=type,recipeId=1,streak=10,lastServed=Start});
            var s=new HotpotServiceSession(f,4);Assert(s.Seat(0),"Seat with varied menu");Equal(2,s.Find(0).recipeId);
        });
        Test("Party quote equals the actual charge and uses its least satisfied member",delegate {
            var f=EndlessFarm();var s=new HotpotServiceSession(f,4);s.Seat(0);f.State.hotpot.progress.rank=20;CookReady(s,0);var order=s.Find(0);
            order.party.Add(new HotpotGuest{archetype=order.guest.archetype,sweet=3,salt=3,sour=3,heat=3,budget=40,threshold=95});
            int quote=s.Quote(order),coins=f.State.coins;Assert(quote<=40,"Party budget ignored");Assert(s.Serve(0),"Party serve");Equal(coins+quote,f.State.coins);
        });
    }
}

