using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class HotpotEndlessReview
    {
        public static void Verify()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();if(live==null)throw new InvalidOperationException("Play the farm first");
            var review=new GameObject("Isolated endless hotpot review").AddComponent<FarmGame>();review.StartCoroutine(Run(live,review));
        }
        private static void Require(bool condition,string text){if(!condition)throw new InvalidOperationException(text);}
        private static void Set(FarmHotpot shop,string field,object value){typeof(FarmHotpot).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(shop,value);}
        private static void Sync(FarmHotpot shop){typeof(FarmHotpot).GetMethod("SyncServiceTables",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(shop,null);}
        private static IEnumerator Run(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);bool open=live.Hotpot.IsOpen;var sun=RenderSettings.sun;var ambient=RenderSettings.ambientLight;
            bool complete=false;live.Hotpot.SuspendForReview(true);live.gameObject.SetActive(false);
            File.WriteAllText(".tools/model-work/hotpot-endless-result.json","{\"status\":\"running\"}");
            try
            {
                double now=FarmGame.UtcNow();var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/hotpot-endless-"+DateTime.UtcNow.Ticks));
                var legacy=FarmModel.NewState(now);legacy.coins=789;legacy.hotpotServed=40;
                legacy.hotpot=JsonUtility.FromJson<HotpotState>("{\"stock\":[],\"recipes\":[],\"furniture\":[],\"nextRecipeId\":1,\"floors\":1,\"reviews\":0,\"tasteTotal\":0}");
                var migrated=new FarmModel(legacy);Require(migrated.HotpotRank==3&&migrated.KitchenLevel==3&&legacy.coins==789,"Unity legacy migration");
                game.InitializeSession(store,()=>now);var model=game.Model;model.State.coins=100000;model.State.hotpotServed=100;model.State.workerHouse=3;
                var p=model.State.hotpot.progress;p.rank=8;p.branchServed=1000;
                foreach(var ingredient in HotpotRules.Ingredients)model.State.hotpot.stock.Add(new KitchenStock{id=ingredient.id,stars=CropCatalog.Find(ingredient.id)!=null?3:1,count=100});
                string[][] ingredients={new[]{"shrimp","lemongrass"},new[]{"beef","chili"},new[]{"tomato","eggs"},new[]{"mushroom","greens"},new[]{"shrimp","fish","corn"},new[]{"beef","onion"},new[]{"chickenMeat","herbs","mushroom"},new[]{"fish","tomato"},new[]{"goat","taro"},new[]{"crab","tomato"},new[]{"beef","tofu","mushroom"},new[]{"beef","cabbage"}};
                int[] styles={1,1,1,0,0,2,0,1,2,1,2,0};
                for(int dish=0;dish<12;dish++)
                {
                    var r=new HotpotRecipe{style=styles[dish],broth=styles[dish],heat=styles[dish]==1?2:0,salt=styles[dish]==2?2:1};
                    foreach(string id in ingredients[dish])r.portions.Add(new HotpotPortion(id,1,CropCatalog.Find(id)!=null?3:1));
                    Require(HotpotRules.Specialty(r)==dish,"Pattern "+dish);Require(model.TestHotpotRecipe(r).success,"Test specialty "+dish);Require(model.SetHotpotMenu(dish+1,true).success,"Menu specialty");
                }
                for(int i=1;i<8;i++)Require(model.UpgradeHotpotKitchen().success,"Upgrade kitchen "+i);
                Require(model.KitchenLevel==8&&p.discoveries.Count==12,"All kitchen levels and discoveries");
                int[] furniture={32,33,28,42,18,16};for(int i=0;i<6;i++)Require(model.PlaceHotpotFurniture(0,i+6,furniture[i],0).success,"Space decor");
                p.waiter=1;p.assistant=1;p.cashier=1;
                game.Hotpot.Open();Require(game.Hotpot.GetComponentsInChildren<HotpotKitchenLevelView>(true).Length>0,"Kitchen visual exists");
                var shop=game.Hotpot;Require(shop.Service.Seat(0),"Guest seat");var order=shop.Service.Find(0);
                order.recipe=HotpotRules.Copy(model.State.hotpot.recipes[0]);order.recipeId=1;order.guest.request=0;order.answered=true;order.patience=120;
                Require(shop.Service.Cook(0),"Start actual cook");Require(!shop.ServeTable(0,0),"Cannot serve raw meal");
                // Staff are temporarily paused to verify the manual button's transaction.
                p.waiter=0;p.assistant=0;for(int i=0;i<22;i++)shop.Service.Tick(1);Sync(shop);
                Require(shop.ServeTable(0,0),"Manual serving");Require(!shop.ServeTable(0,0),"Duplicate serving");
                Require(shop.GetComponentsInChildren<HotpotKitchenItem>(true).Length>1,"Specialty prefab appears");
                Set(shop,"selectedTable",0);yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-Endless-dining.png"));
                yield return new WaitForSecondsRealtime(.4f);
                p.waiter=1;p.assistant=1;p.cashier=1;Require(model.SetHotpotOffline(true,now).success,"Offline staff toggle");
                Set(shop,"tab",5);yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-Endless-business.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Set(shop,"tab",6);yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-Endless-discoveries.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Set(shop,"tab",7);yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-Endless-rush.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Save();var loaded=new FarmModel(store.Load(now));Require(loaded.KitchenLevel==8&&loaded.State.hotpot.progress.discoveries.Count==12&&loaded.State.hotpot.recipes[0].timesServed==1,"Progress JSON persists");
                Require(store.LastError==null,"Save did not fall back");
                for(int branch=1;branch<=3;branch++)
                {
                    Set(shop,"tab",5);p.rank=8;p.seasonClaims=1;Require(shop.OpenNextBranch(),"Open branch "+branch);
                    Require(model.State.hotpot.recipes.Count==12&&model.State.hotpot.recipes[0].timesServed==1&&model.State.hotpot.furniture.Count==0,"Prestige retains cookbook only");
                    Set(shop,"tab",0);yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-branch-"+branch+".png"));
                    yield return new WaitForSecondsRealtime(.4f);
                }
                shop.Close();complete=true;
            }
            finally
            {
                if(game.Hotpot!=null&&game.Hotpot.IsOpen)game.Hotpot.Close();Object.Destroy(game.gameObject);live.gameObject.SetActive(true);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;
                live.Hotpot.SuspendForReview(false);
                Require(before==JsonUtility.ToJson(live.Model.State),"Live farm preserved");
                File.WriteAllText(".tools/model-work/hotpot-endless-result.json",complete?"{\"status\":\"complete\",\"legacyJson\":true,\"specialties\":12,\"kitchenLevels\":8,\"cookServe\":true,\"saveReload\":true,\"playerFarmUntouched\":true}":"{\"status\":\"failed\"}");
            }
        }
    }
}
