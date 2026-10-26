using System;
using System.Collections;
using System.IO;
using System.Linq;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class FarmDevelopmentReview
    {
        internal static void Verify()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            if(live==null)throw new InvalidOperationException("Farm must be playing");
            var game=new GameObject("Isolated development review").AddComponent<FarmGame>();
            game.StartCoroutine(Run(live,game));
        }
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private static IEnumerator Run(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);
            var previousSun=RenderSettings.sun;bool complete=false;
            File.WriteAllText(".tools/model-work/development-result.json","{\"status\":\"running\"}");
            live.gameObject.SetActive(false);
            try
            {
                double now=FarmGame.UtcNow();var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/development-"+DateTime.UtcNow.Ticks));
                game.InitializeSession(store,()=>now);
                game.Model.State.coins=10000;game.Model.GainExperience(400);game.Model.Island.chapter=3;
                game.Model.State.development.wood=500;game.Model.State.development.ore=500;
                yield return null;yield return null;
                var plots=game.World.GetComponentsInChildren<FarmPlot>().OrderBy(p=>p.Index).ToArray();
                foreach(var crop in CropCatalog.All)
                {
                    game.SelectCrop(crop.id);game.ActOnPlot(0);Require(plots[0].RenderedStage==1,"Seed model "+crop.id);
                    double planted=now;
                    for(int stage=2;stage<=5;stage++)
                    {
                        now=planted+crop.growSeconds*(stage==5?1.01:(stage-1)*.25+.03);
                        game.SelectTool(FarmTool.Inspect);game.ActOnPlot(0);
                        Require(plots[0].RenderedStage==stage,"Five-stage progression "+crop.id+" / "+stage);
                        Require(plots[0].GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith(crop.modelName+"_"+stage)),"New native stage "+crop.id+" / "+stage);
                        yield return null;
                    }
                    Require(plots[0].RenderedStage==5,"Mature model "+crop.id);
                    Require(plots[0].GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith(crop.modelName+"_5")),"Native crop prefab "+crop.modelName);
                    game.FinishHarvest(0,1);Require(game.Model.Count(crop.id)>=crop.yield,"Harvest inventory "+crop.id);
                    if(IslandCropCatalog.Leaf(crop.id))for(int harvest=0;harvest<2;harvest++){now+=crop.growSeconds+1;game.FinishHarvest(0,1);}
                    if(FarmModel.IsPerennial(crop.id))
                    {
                        Require(game.Model.State.plots[0].cropId==crop.id&&game.Model.State.plots[0].harvests==1,"Perennial stays planted "+crop.id);
                        now+=crop.growSeconds; // established trees refruit in 60% time
                        Require(game.Model.Progress(0,now)>=1,"Perennial refruits "+crop.id);
                        game.FinishHarvest(0,1);
                        Require(game.Model.ChopPerennial(0).success&&game.Model.State.plots[0].cropId=="","Chop perennial "+crop.id);
                    }
                }
                foreach(var b in DevelopmentCatalog.Buildings)
                    for(int level=1;level<=3;level++){game.Development.Perform(()=>game.Model.UpgradeBuilding(b.id));Require(game.Model.BuildingLevel(b.id)==level,"Upgrade "+b.id);}
                Require(game.Development.VisibleBuildings==9,"Nine building models");
                game.Model.State.inventory.Find(e=>e.cropId=="wheat").count=9;
                for(int i=0;i<3;i++)Require(game.Model.StartProduction("flour",now).success,"Queue flour");
                Require(!game.Model.StartProduction("flour",now).success,"Queue capacity");
                now+=100;game.SelectTool(FarmTool.Inspect);game.ActOnPlot(8);
                foreach(var job in game.Model.State.development.jobs.ToArray())Require(game.Model.CollectProduction(job.id,now).success,"Collect flour");
                Require(game.Model.StartProduction("bread",now).success,"Bread chain");now+=100;
                foreach(var job in game.Model.State.development.jobs.ToArray())Require(game.Model.CollectProduction(job.id,now).success,"Collect bread");
                game.Model.State.development.ordersCompleted=5;Require(game.Model.FulfillOrder().success,"Bread order");
                Require(game.Model.StartNursery("mango",now).success,"Nursery");
                Require(game.Model.ToggleGreenhouse(8).success,"Greenhouse allocation");
                game.Model.State.development.products.Add(new ProductStock{id="compost",count=1});
                Require(game.Model.UseCareProduct("compost",8).success,"Compost use");
                game.Save();var loaded=new FarmModel(store.Load(now));
                Require(loaded.State.inventory.Count==CropCatalog.All.Length&&loaded.BuildingLevel("bakery")==3&&loaded.State.development.seedlings.Count==1&&loaded.State.plots[8].greenhouse&&loaded.State.plots[8].soilQuality==1,"Development JSON roundtrip");
                // Land reclamation: two parcels extend the field to 36 tilled plots in the world.
                game.Model.State.hasSold=false;
                int beforeLandCoins=game.Model.State.coins;
                Require(!game.Model.UnlockParcel().success&&game.Model.State.coins==beforeLandCoins&&game.Model.State.plots.Count==24,"Reclamation mission gate preserves state");
                game.Development.Open(1);yield return null;
                Directory.CreateDirectory("Art/Previews/Farm");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Land-unlock-requirements.png"));
                yield return new WaitForSecondsRealtime(.4f);game.Development.Close();
                Require(game.Model.SellAll().success,"Reclamation sale mission");
                Require(game.Model.UnlockParcel().success&&game.Model.UnlockParcel().success,"Unlock parcels");
                game.World.SyncPlots(game.Model);yield return null;
                Require(game.Model.State.plots.Count==36,"36 plots in model");
                Require(game.World.GetComponentsInChildren<FarmPlot>().Length==36,"36 plot tiles in world");
                game.SelectCrop("carrot");game.ActOnPlot(35);
                Require(game.Model.State.plots[35].cropId=="carrot","Plant on reclaimed land");
                // Hotpot economy: serving consumes stock and pays the margin.
                game.Model.State.inventory.Find(e=>e.cropId=="greens").count=2;
                game.Model.State.inventory.Find(e=>e.cropId=="corn").count=2;
                Require(game.Model.ImportKitchen("greens",1,2).success&&game.Model.ImportKitchen("corn",1,2).success,"Import kitchen");
                Require(game.Model.BuyKitchen("base",2).success&&game.Model.BuyKitchen("seasoning",4).success,"Kitchen supplier");
                var recipe=new HotpotRecipe();recipe.portions.Add(new HotpotPortion("greens",1,1));recipe.portions.Add(new HotpotPortion("corn",1,1));
                Require(game.Model.TestHotpotRecipe(recipe).success&&game.Model.SetHotpotMenu(1,true).success,"Test recipe");
                int hotpotCoins=game.Model.State.coins;
                Require(game.Model.ServeHotpotRecipe(1).success,"Serve hotpot");
                Require(game.Model.State.coins>hotpotCoins&&game.Model.Count("greens")==0,"Hotpot margin");
                // Workers: house gates hiring; a grower waters and harvests on its own.
                Require(game.Model.UpgradeWorkerHouse().success&&game.Model.UpgradeWorkerHouse().success,"Worker house");
                Require(game.Model.HireWorker(1).success,"Hire grower");
                Require(game.Model.State.workers[0].level==2,"Hire level follows house");
                Require(game.Model.RunWorkers(now)>=1&&game.Model.State.plots[35].watered,"Worker watered");
                now+=200;Require(game.Model.RunWorkers(now)>=1&&game.Model.State.plots[35].cropId=="","Worker harvested");
                // Star-quality store: a pampered perfect harvest grades ★★★.
                game.Model.State.plots[35].soilQuality=2;
                game.SelectCrop("carrot");game.ActOnPlot(35);now+=50;
                Require(game.Model.Harvest(35,3,now).success&&game.Model.CountPrime("carrot")>0,"Prime-star store");
                // Ducks and pigs: pig compost feeds the soil loop.
                Require(game.Model.BuyAnimal("duck").success&&game.Model.BuyAnimal("pig").success,"Buy duck/pig");
                Require(game.Model.FeedAnimal("duck",false,now).success&&game.Model.FeedAnimal("pig",false,now).success,"Feed duck/pig");
                now+=151;int compostBefore=game.Model.ItemCount("compost");
                Require(game.Model.CollectAnimal("duck",now).success&&game.Model.CollectAnimal("pig",now).success,"Collect duck/pig");
                Require(game.Model.ItemCount("compost")==compostBefore+2,"Pig compost");
                yield return null;
                Require(game.Ranch.VisibleAnimals>=2,"Duck and pig models in pens");
                // Achievements: harvest milestone is already met by the crop loop.
                Require(game.Model.ClaimAchievement(0).success&&!game.Model.ClaimAchievement(0).success,"Achievement single claim");
                // Bees need blooms, then forage honey; pollination raises fruiting yield.
                Require(game.Model.BuyAnimal("bee").success,"Buy hive");
                Require(!game.Model.FeedAnimal("bee",false,now).success,"Bees foraged without flowers");
                Require(game.Model.Apply(30,FarmTool.Plant,"marigold",now).success&&game.Model.Apply(31,FarmTool.Plant,"beans",now).success,"Plant blooms");
                Require(game.Model.FeedAnimal("bee",false,now).success,"Release bees");
                now+=121;Require(game.Model.CollectAnimal("bee",now).success&&game.Model.State.honey>=2,"Honey store");
                // Fish pond cycle.
                Require(game.Model.BuyAnimal("fish").success&&game.Model.FeedAnimal("fish",false,now).success,"Fish feed");
                now+=141;Require(game.Model.CollectAnimal("fish",now).success&&game.Model.State.fishStock>=2,"Fish stock");
                // Weekly quests: three generated, pay exactly once.
                Require(game.Model.State.quests.Count==3,"Weekly quests generated");
                var quest=game.Model.State.quests[0];quest.kind=0;quest.target=1;quest.baseline=0;quest.claimed=false;
                Require(game.Model.ClaimQuest(0).success&&!game.Model.ClaimQuest(0).success,"Quest single claim");
                yield return null;
                game.SelectTool(FarmTool.Inspect);game.ActOnPlot(8);
                int coins=game.Model.State.coins;
                game.Development.Open(5);game.ActOnPlot(9);Require(game.Model.State.coins==coins&&game.SelectedPlot==8,"Management blocks world actions");
                yield return null;
                Directory.CreateDirectory("Art/Previews/Farm");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Development-seeds.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Development.Open(1);yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Development-buildings.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Development.Open(2);yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Development-recipes.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Development.Close();game.ClearSelection();game.CameraController.FocusOn(new Vector3(0,.2f,-7));
                game.World.GameCamera.orthographicSize=8;yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Development-island.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Hotpot.Open();Require(game.Hotpot.IsOpen,"Hotpot interior opens");
                yield return new WaitForSecondsRealtime(2.4f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-interior.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Hotpot.Close();
                Require(JsonUtility.ToJson(live.Model.State)==before,"Player farm preserved");
                complete=true;
            }
            finally
            {
                Object.Destroy(game.gameObject);RenderSettings.sun=previousSun;live.gameObject.SetActive(true);
                File.WriteAllText(".tools/model-work/development-result.json",complete?"{\"status\":\"complete\",\"crops\":"+CropCatalog.All.Length+",\"buildingUpgrades\":27,\"productionChain\":true,\"saveReload\":true,\"landParcels\":2,\"hotpot\":true,\"workers\":true,\"bees\":true,\"fishPond\":true,\"weeklyQuests\":3,\"playerFarmUntouched\":true}":"{\"status\":\"failed\",\"seeEditorLog\":true}");
            }
        }
    }
}
