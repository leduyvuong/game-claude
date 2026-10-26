using System;
using System.Collections.Generic;
using HarvestIsle.Core;

internal static partial class FarmModelTests
{
    private static FarmModel CrewFarm(){var f=Farm();f.State.workerHouse=3;f.State.coins=1000000;return f;}
    private static void RevealAll(FarmModel f){for(int i=0;i<f.State.recruitment.reveal.Count;i++)f.RevealWorker(f.State.recruitment.serial,i);}
    private static SpecialistWorker Crew(FarmModel f,string id)
    {Assert(f.HireSpecialist(id).success,"Hire");Assert(f.BuyWorkerShift(id).success,"Shift");Assert(f.AssignSpecialist(id,true).success,"Assign");return f.OwnedWorker(id);}
    private static void ReadyCrop(FarmModel f,int index,string id)
    {var p=f.State.plots[index];p.cropId=id;p.plantedAt=Start-500;p.watered=true;p.soilWater=.25;p.soilQuality=2;p.disease=0;p.pest=0;}
    private static void RecruitmentTests()
    {
        Test("72 unique workers cover all 30 crops with meaningful price and skill tiers",delegate{
            Equal(72,WorkerCatalog.All.Length);var ids=new HashSet<string>();int[] rarity=new int[4];
            foreach(var s in WorkerCatalog.All){Assert(ids.Add(s.id),"Duplicate ID");rarity[s.rarity-1]++;Assert(s.HirePrice>0&&s.ShiftPrice>0,"Price");}
            foreach(var c in CropCatalog.Original){var a=WorkerCatalog.Find(c.id+"_junior");var b=WorkerCatalog.Find(c.id+"_senior");Assert(a!=null&&b!=null&&b.HirePrice>a.HirePrice&&b.Skill(0)>a.Skill(0)&&b.Interval(0)<a.Interval(0),"Specialty progression");}
            Equal(30,rarity[0]);Equal(19,rarity[1]);Equal(14,rarity[2]);Equal(9,rarity[3]);
        });
        Test("Legacy farm and existing workers migrate without deductions or reassignment",delegate{
            var s=FarmModel.NewState(Start);s.workerHouse=2;s.workers.Add(new WorkerState{kind=1,level=2});s.recruitment=null;var f=new FarmModel(s);
            Equal(120,f.State.coins);Equal(1,f.State.workers.Count);Equal(0,f.State.recruitment.roster.Count);Equal(1,f.AssignedCrew);
            s.recruitment=new RecruitmentState{roster=null,reveal=null};f=new FarmModel(s);Equal(0,f.State.recruitment.serial);
        });
        Test("Direct hiring shifts training and assignment validate before charging",delegate{
            var f=Farm();int before=f.State.coins;Assert(!f.HireSpecialist("tomato_junior").success,"House gate");Equal(before,f.State.coins);
            f.State.workerHouse=1;Assert(f.HireSpecialist("tomato_junior").success,"Hire exact");Equal(0,f.State.coins);
            Assert(!f.HireSpecialist("tomato_junior").success&&!f.BuyWorkerShift("tomato_junior").success&&!f.AssignSpecialist("tomato_junior",true).success,"Missing funds or shift");
            f.State.coins=200;Assert(f.BuyWorkerShift("tomato_junior").success,"Prepay");Equal(196,f.State.coins);Equal(10,f.OwnedWorker("tomato_junior").jobs);
            Assert(!f.TrainSpecialist("tomato_junior").success,"Training without notes");Equal(196,f.State.coins);
            f.State.recruitment.notes=28;Assert(f.TrainSpecialist("tomato_junior").success,"Train1");Assert(!f.TrainSpecialist("tomato_junior").success,"Train2 funds");
            Equal(1,f.OwnedWorker("tomato_junior").training);Equal(20,f.State.recruitment.notes);
        });
        Test("Assignment shares six slots with legacy crew and retirement releases capacity",delegate{
            var f=CrewFarm();f.State.workers.Add(new WorkerState{kind=0,level=1});
            for(int i=0;i<5;i++)Crew(f,WorkerCatalog.All[i].id);
            string id=WorkerCatalog.All[5].id;f.HireSpecialist(id);f.BuyWorkerShift(id);Assert(!f.AssignSpecialist(id,true).success,"Capacity bypass");
            Assert(f.RetireLegacyWorker(0).success&&f.AssignSpecialist(id,true).success,"Retirement");Equal(6,f.AssignedCrew);
            Assert(!f.HireWorker(0).success,"Legacy hire bypass");
        });
        Test("Tomato specialist prefers tomatoes and cannot harvest wheat in strict mode",delegate{
            var f=CrewFarm();var w=Crew(f,"tomato_senior");ReadyCrop(f,0,"wheat");ReadyCrop(f,1,"tomato");
            Equal(1,f.RunWorkers(Start));Equal("wheat",f.State.plots[0].cropId);Equal("",f.State.plots[1].cropId);Assert(f.CountPrime("tomato")>0,"Expert quality");Equal(9,w.jobs);
            Equal(0,f.RunWorkers(Start+20));Equal(9,w.jobs);f.SetWorkerAnyCrop(w.id,true);Equal(1,f.RunWorkers(Start+24));Assert(f.Count("wheat")>0&&f.CountPrime("wheat")==0,"Off-specialty quality");
        });
        Test("Wheat expert makes more and better produce than apprentice on identical healthy plots",delegate{
            var a=CrewFarm();var b=CrewFarm();Crew(a,"wheat_junior");Crew(b,"wheat_senior");ReadyCrop(a,0,"wheat");ReadyCrop(b,0,"wheat");
            a.RunWorkers(Start);b.RunWorkers(Start);Assert(b.CountPrime("wheat")>a.Count("wheat"),"Skill reward missing");
        });
        Test("Cooldown survives stop start and training; idle or exhausted workers spend nothing",delegate{
            var f=CrewFarm();var w=Crew(f,"carrot_junior");ReadyCrop(f,0,"carrot");ReadyCrop(f,1,"carrot");f.RunWorkers(Start);
            f.AssignSpecialist(w.id,false);f.AssignSpecialist(w.id,true);Equal(0,f.RunWorkers(Start));Equal(9,w.jobs);Equal(1,f.RunWorkers(Start+24));
            int coins=f.State.coins;Equal(0,f.RunWorkers(Start+100));Equal(8,w.jobs);Equal(coins,f.State.coins);
            w.jobs=0;ReadyCrop(f,0,"carrot");Equal(0,f.RunWorkers(Start+1000));Equal("carrot",f.State.plots[0].cropId);
        });
        Test("No offline catch-up and two workers cannot double-harvest one plot",delegate{
            var f=CrewFarm();var a=Crew(f,"mango_junior");var b=Crew(f,"mango_senior");ReadyCrop(f,0,"mango");
            Equal(1,f.RunWorkers(Start+86400));Equal(1,f.State.harvested);Equal(19,a.jobs+b.jobs);Equal(0,f.RunWorkers(Start+86400));Equal("mango",f.State.plots[0].cropId);
        });
        Test("Full stock rejects harvest without consuming a prepaid action",delegate{
            var f=CrewFarm();var w=Crew(f,"carrot_junior");ReadyCrop(f,0,"carrot");f.State.inventory.Find(e=>e.cropId=="carrot").count=10000000;
            Equal(0,f.RunWorkers(Start));Equal(10,w.jobs);Equal("carrot",f.State.plots[0].cropId);
        });
        Test("Irrigation soil and ranch specialists execute their actual duties",delegate{
            var f=CrewFarm();Crew(f,"irrigation_3");ReadyCrop(f,0,"tomato");f.State.plots[0].soilWater=.8;Equal(1,f.RunWorkers(Start));Equal(.05,f.State.plots[0].soilWater);
            f.AssignSpecialist("irrigation_3",false);Crew(f,"soil_4");f.State.plots[0].pest=1;f.State.plots[0].disease=60;
            Equal(1,f.RunWorkers(Start+30));Equal(0,f.State.plots[0].pest);Equal(1,f.RunWorkers(Start+40));Equal(25d,f.State.plots[0].disease);
            f.AssignSpecialist("soil_4",false);Crew(f,"ranch_3");f.BuyAnimal("cow");f.State.inventory.Find(e=>e.cropId=="corn").count=1;
            Equal(1,f.RunWorkers(Start+50));Equal(0,f.Count("corn"));Equal(1,f.RunWorkers(Start+500));Assert(f.State.milk>0,"Collection");
        });
        Test("Recruitment validates price serial pending and invalid random rolls atomically",delegate{
            var f=CrewFarm();int coins=f.State.coins;
            Assert(!f.RecruitWorkers(2,0,n=>0).success&&!f.RecruitWorkers(5,1,n=>0).success,"Bad request");Equal(coins,f.State.coins);
            int calls=0;Assert(!f.RecruitWorkers(5,0,n=>++calls==4?n:0).success,"Invalid rng");Equal(0,f.State.recruitment.roster.Count);Equal(coins,f.State.coins);
            Assert(f.RecruitWorkers(5,0,n=>0).success,"Draw");Equal(coins-150,f.State.coins);Equal(1,f.State.recruitment.roster.Count);Equal(4,f.State.recruitment.notes);
            Assert(!f.RecruitWorkers(1,0,n=>0).success&&!f.RecruitWorkers(1,1,n=>0).success,"Replay or pending accepted");Equal(coins-150,f.State.coins);
        });
        Test("Revealing grants nothing twice and survives constructing a new model",delegate{
            var f=CrewFarm();f.RecruitWorkers(5,0,n=>0);int notes=f.State.recruitment.notes;int coins=f.State.coins;
            var restored=new FarmModel(f.State);Assert(restored.RevealWorker(1,0).success,"Reveal");Assert(!restored.RevealWorker(1,0).success&&!restored.RevealWorker(0,1).success,"Stale reveal");
            Equal(notes,restored.State.recruitment.notes);Equal(coins,restored.State.coins);RevealAll(restored);Assert(restored.RecruitWorkers(1,1,n=>0).success,"Next batch");
        });
        Test("Ten-draw expert and fifty-draw master guarantees survive batch boundaries",delegate{
            var f=CrewFarm();
            for(int i=1;i<=50;i++)
            {
                Assert(f.RecruitWorkers(1,f.State.recruitment.serial,n=>0).success,"Draw "+i);var spec=WorkerCatalog.Find(f.State.recruitment.reveal[0].id);
                if(i%10==0)Assert(spec.rarity>=3,"Expert pity");if(i==50)Equal(4,spec.rarity);RevealAll(f);
            }
            Equal(0,f.State.recruitment.sinceExpert);Equal(0,f.State.recruitment.sinceMaster);
        });
        Test("Natural master resets both guarantees and probability boundaries choose correct tiers",delegate{
            int[] rolls={0,5999,6000,8799,8800,9799,9800,9999};int[] tiers={1,1,2,2,3,3,4,4};
            for(int i=0;i<rolls.Length;i++){var f=CrewFarm();int r=rolls[i];Assert(f.RecruitWorkers(1,0,n=>n==10000?r:0).success,"Draw");Equal(tiers[i],WorkerCatalog.Find(f.State.recruitment.reveal[0].id).rarity);}
        });
        Test("Recruitment validation rejects corrupt roster pity timestamps and assignment",delegate{
            var f=CrewFarm();Crew(f,"tomato_junior");string reason;Assert(FarmModel.Validate(f.State,out reason),"Healthy");
            f.State.recruitment.roster.Add(f.OwnedWorker("tomato_junior"));Assert(!FarmModel.Validate(f.State,out reason),"Duplicate");f.State.recruitment.roster.RemoveAt(1);
            f.OwnedWorker("tomato_junior").nextActionAt=double.NaN;Assert(!FarmModel.Validate(f.State,out reason),"NaN");f.OwnedWorker("tomato_junior").nextActionAt=Start;
            f.State.recruitment.sinceMaster=50;Assert(!FarmModel.Validate(f.State,out reason),"Pity overflow");
        });
    }
}
