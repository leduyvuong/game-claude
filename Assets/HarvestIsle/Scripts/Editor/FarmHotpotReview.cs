using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class FarmHotpotReview
    {
        internal static void Verify()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            if(live==null)throw new InvalidOperationException("Farm must be playing");
            var game=new GameObject("Isolated hotpot review").AddComponent<FarmGame>();
            game.StartCoroutine(Run(live,game));
        }
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private static FieldInfo Field(string name){return typeof(FarmHotpot).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);}
        private static IEnumerator Run(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);var sun=RenderSettings.sun;bool complete=false;
            File.WriteAllText(".tools/model-work/hotpot-result.json","{\"status\":\"running\"}");
            bool wasOpen=live.Hotpot.IsOpen;live.Hotpot.SuspendForReview(true);
            live.gameObject.SetActive(false);
            try
            {
                double now=FarmGame.UtcNow();var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/hotpot-"+DateTime.UtcNow.Ticks));
                game.InitializeSession(store,()=>now);var model=game.Model;
                model.State.coins=100000;model.State.hotpotServed=100;
                VerifyFurnitureResources();
                model.State.primeGoods.Add(new InventoryEntry("corn",30));
                model.State.primeGoods.Add(new InventoryEntry("mushroom",30));
                Require(model.ImportKitchen("corn",3,30).success&&model.ImportKitchen("mushroom",3,30).success,"Farm imports");
                Require(model.BuyKitchen("base",30).success&&model.BuyKitchen("seasoning",60).success,"Supplier orders");
                Require(!model.BuyKitchen("corn",1).success,"Supplier crop restriction");
                var recipe=new HotpotRecipe{name="Lẩu ngô nấm vườn nhà"};
                recipe.portions.Add(new HotpotPortion("corn",1,3));recipe.portions.Add(new HotpotPortion("mushroom",1,3));
                Require(model.TestHotpotRecipe(recipe).success&&model.SetHotpotMenu(1,true).success,"Recipe test and menu");
                Require(model.ExpandHotpot().success&&model.ExpandHotpot().success&&model.ExpandHotpot().success,"Four floors");
                Require(model.PlaceHotpotFurniture(1,0,2,1).success&&model.PlaceHotpotFurniture(1,1,6,0).success,"Furniture purchase");
                Require(model.MoveHotpotFurniture(1,0,4,2).success,"Furniture move");
                int[] display={8,13,16,18,19,21,23,32,33,42,46,53};
                for(int slot=0;slot<display.Length;slot++)Require(model.PlaceHotpotFurniture(2,slot,display[slot],0).success,"Reference placement "+display[slot]);
                Require(model.PlaceHotpotFurniture(2,12,36,0).success&&model.PlaceHotpotFurniture(2,13,37,0).success&&model.PlaceHotpotFurniture(2,17,38,0).success,"Wall decoration");
                Require(model.PlaceHotpotFurniture(2,20,35,0).success&&model.PlaceHotpotFurniture(2,23,39,0).success,"Ceiling decoration");
                game.Save();var loaded=new FarmModel(store.Load(now));
                Require(loaded.State.hotpot.floors==4&&loaded.State.hotpot.recipes[0].name==recipe.name&&loaded.KitchenCount("corn",3)==29&&loaded.State.hotpot.furniture[0].slot==4,"Kitchen JSON roundtrip");
                var legacy=FarmModel.NewState(now);Require(new FarmModel(JsonUtility.FromJson<FarmState>(JsonUtility.ToJson(legacy))).State.hotpot.recipes.Count==0,"Legacy JSON migration");
                Color ambientBefore=RenderSettings.ambientLight;
                game.Hotpot.Open();Require(game.Hotpot.IsOpen&&game.World.GameCamera.enabled,"Farm remains visible behind restaurant window");
                var interior=(Camera)typeof(FarmHotpot).GetField("interiorCamera",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.Hotpot);
                Require(interior.enabled&&interior.rect.width<=.83f&&interior.rect.height<=.83f,"Interior is confined to dialog viewport");
                int coins=model.State.coins;game.SelectCrop("carrot");game.ActOnPlot(0);Require(model.State.coins==coins&&model.State.plots[0].cropId=="","World input blocked");
                yield return new WaitForSecondsRealtime(2);
                PrepareService(game.Hotpot,0,0);
                Require(game.Hotpot.ServeTable(0,0),"Serve visible table");coins=model.State.coins;
                Require(!game.Hotpot.ServeTable(0,0)&&model.State.coins==coins,"Duplicate serve blocked");
                var canSeat=typeof(FarmHotpot).GetMethod("CanSeatRecipe",BindingFlags.Instance|BindingFlags.NonPublic);
                Require((bool)canSeat.Invoke(game.Hotpot,new object[]{model.State.hotpot.recipes[0]}),"Stock supports another order");
                // Populate visual-only dining states on this isolated farm for the reference preview.
                var previewTables=(IList)Field("tables").GetValue(game.Hotpot);
                var sync=typeof(FarmHotpot).GetMethod("SyncTable",BindingFlags.Instance|BindingFlags.NonPublic);
                bool newTableServed=false;
                foreach(var table in previewTables)
                {
                    var type=table.GetType();
                    if((int)type.GetField("floor").GetValue(table)!=2||(int)type.GetField("slot").GetValue(table)!=0)continue;
                    type.GetField("occupied").SetValue(table,true);type.GetField("recipeId").SetValue(table,1);type.GetField("remaining").SetValue(table,45f);
                    PrepareService(game.Hotpot,2,0);
                    Require(game.Hotpot.ServeTable(2,0),"Serve new prefab table");
                    Require(((Transform)type.GetField("pot").GetValue(table)).gameObject.activeSelf||type.GetField("specialtyVisual").GetValue(table)!=null,"Baked food becomes visible");
                    Require(!game.Hotpot.ServeTable(2,0),"New prefab duplicate service blocked");newTableServed=true;
                }
                Require(newTableServed,"Reference table was registered for service");
                foreach(var table in previewTables)
                {
                    var type=table.GetType();int slot=(int)type.GetField("slot").GetValue(table);
                    if((int)type.GetField("floor").GetValue(table)!=0||(slot!=2&&slot!=4))continue;
                    type.GetField("occupied").SetValue(table,true);type.GetField("served").SetValue(table,true);
                    type.GetField("recipeId").SetValue(table,1);type.GetField("remaining").SetValue(table,12f);sync.Invoke(game.Hotpot,new[]{table});
                }
                Directory.CreateDirectory("Art/Previews/Farm");
                yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-3D.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Field("tab").SetValue(game.Hotpot,3);yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-recipes.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Field("tab").SetValue(game.Hotpot,1);Field("stars").SetValue(game.Hotpot,3);yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-supply.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Field("supplier").SetValue(game.Hotpot,true);yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-supplier.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Field("tab").SetValue(game.Hotpot,2);Field("draft").SetValue(game.Hotpot,HotpotRules.Copy(recipe));yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-create-recipe.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Field("tab").SetValue(game.Hotpot,4);game.Hotpot.ShowFloor(1);yield return null;
                int shopCoins=model.State.coins,shopCount=model.State.hotpot.furniture.Count;
                Require(game.Hotpot.BeginDecorPlacement(8,1),"Start placement preview");
                Require(game.Hotpot.SelectDecorPosition(2),"Choose empty slot");
                Require(model.State.coins==shopCoins&&model.State.hotpot.furniture.Count==shopCount,"Preview has no economic effects");
                yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-placement.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Hotpot.CancelDecorPlacement();
                Require(!game.Hotpot.ConfirmDecorPlacement()&&model.State.coins==shopCoins,"Cancel cannot purchase");
                Require(game.Hotpot.BeginDecorPlacement(8,0)&&!game.Hotpot.SelectDecorPosition(0),"Reserved slots blocked");
                game.Hotpot.CancelDecorPlacement();
                Require(game.Hotpot.BeginDecorPlacement(8,1)&&!game.Hotpot.SelectDecorPosition(1),"Occupied slots blocked");
                Require(!game.Hotpot.SelectDecorPosition(12),"Mount mismatch blocked");
                Require(game.Hotpot.SelectDecorPosition(2)&&game.Hotpot.ConfirmDecorPlacement(),"Confirm purchase");
                Require(model.State.coins==shopCoins-HotpotFurnitureCatalog.Items[8].price&&model.State.hotpot.furniture.Count==shopCount+1,"Charge exactly once");
                Require(!game.Hotpot.ConfirmDecorPlacement(),"Duplicate confirmation blocked");
                shopCoins=model.State.coins;
                Require(game.Hotpot.BeginDecorPlacement(8,1,2)&&game.Hotpot.SelectDecorPosition(3)&&game.Hotpot.ConfirmDecorPlacement(),"Owned item move");
                Require(model.State.coins==shopCoins&&model.State.hotpot.furniture.Exists(f=>f.floor==1&&f.slot==3&&f.kind==8),"Move is free");
                Require(game.Hotpot.BeginDecorPlacement(36,1)&&game.Hotpot.SelectDecorPosition(12),"Wall preview");
                game.Hotpot.CancelDecorPlacement();
                Require(game.Hotpot.BeginDecorPlacement(35,1)&&game.Hotpot.SelectDecorPosition(20),"Ceiling preview");
                game.Hotpot.CancelDecorPlacement();
                yield return null;yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-shop.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Field("decorOwned").SetValue(game.Hotpot,true);Field("ownedFloor").SetValue(game.Hotpot,1);Field("ownedSlot").SetValue(game.Hotpot,3);Field("furnitureKind").SetValue(game.Hotpot,8);
                yield return null;yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-owned.png"));
                yield return new WaitForSecondsRealtime(.4f);Field("decorOwned").SetValue(game.Hotpot,false);
                var sessions=(IList)Field("tables").GetValue(game.Hotpot);
                object moved=null;
                foreach(var table in sessions)
                {
                    var type=table.GetType();
                    if((int)type.GetField("floor").GetValue(table)==1&&(int)type.GetField("slot").GetValue(table)==4)
                    {
                        moved=table;type.GetField("occupied").SetValue(table,true);type.GetField("recipeId").SetValue(table,1);type.GetField("remaining").SetValue(table,45f);break;
                    }
                }
                Require(moved!=null&&game.Hotpot.MoveFurniture(1,4,5,2),"Move occupied table");
                Require((int)moved.GetType().GetField("slot").GetValue(moved)==5&&(bool)moved.GetType().GetField("occupied").GetValue(moved),"Moving table preserves order");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-decoration.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Field("tab").SetValue(game.Hotpot,0);game.Hotpot.ShowFloor(2);
                foreach(var table in (IList)Field("tables").GetValue(game.Hotpot))
                {
                    var type=table.GetType();if((int)type.GetField("floor").GetValue(table)!=2)continue;
                    type.GetField("occupied").SetValue(table,true);type.GetField("served").SetValue(table,true);type.GetField("remaining").SetValue(table,60f);sync.Invoke(game.Hotpot,new[]{table});
                }
                yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Hotpot-furniture-room.png"));
                yield return new WaitForSecondsRealtime(.4f);game.Hotpot.ShowFloor(0);
                var cachedRoom=(GameObject)Field("room").GetValue(game.Hotpot);
                game.Hotpot.Close();Require(game.World.GameCamera.enabled&&game.Hud.gameObject.activeSelf,"Farm view restored");
                Require(RenderSettings.ambientLight==ambientBefore,"Farm lighting restored");
                game.Hotpot.Open();Require(game.Hotpot.IsOpen,"Reopen works");
                Require((GameObject)Field("room").GetValue(game.Hotpot)==cachedRoom,"Room reused on reopen");game.Hotpot.Close();
                game.Save();loaded=new FarmModel(store.Load(now));Require(loaded.State.hotpot.reviews==2&&loaded.State.hotpotServed==102,"Service persisted");
                complete=true;
            }
            finally
            {
                Object.Destroy(game.gameObject);live.gameObject.SetActive(true);RenderSettings.sun=sun;
                live.Hotpot.SuspendForReview(false);
                Require(JsonUtility.ToJson(live.Model.State)==before,"Live farm untouched");
                File.WriteAllText(".tools/model-work/hotpot-result.json",complete?"{\"status\":\"complete\",\"kitchenImports\":true,\"recipeScoring\":true,\"saveReload\":true,\"floors\":4,\"furniture\":true,\"referencePrefabs\":52,\"newTableService\":true,\"duplicateServeBlocked\":true,\"worldInputBlocked\":true,\"playerFarmUntouched\":true}":"{\"status\":\"failed\",\"seeEditorLog\":true}");
            }
        }
        private static void VerifyFurnitureResources()
        {
            for(int kind=8;kind<HotpotFurnitureCatalog.Items.Length;kind++)
            {
                var prefab=Resources.Load<GameObject>(HotpotFurnitureCatalog.Resource(kind));Require(prefab!=null,"Resource load "+kind);
                var instance=Object.Instantiate(prefab);instance.SetActive(false);
                try
                {
                    var view=instance.GetComponent<HotpotFurnitureView>();Require(view!=null&&view.kind==kind,"Component kind "+kind);
                    if(HotpotFurnitureCatalog.IsTable(kind))Require(view.seats.Length>=2&&view.serving!=null,"Dining sockets "+kind);
                    Require(Resources.Load<Texture2D>("HotpotThumbnails/HP_"+HotpotFurnitureCatalog.Items[kind].id)!=null,"Thumbnail "+kind);
                    var model=new FarmModel(FarmModel.NewState(FarmGame.UtcNow()));model.State.coins=10000;
                    int slot=HotpotFurnitureCatalog.Items[kind].mount==HotpotMount.Floor?6:HotpotFurnitureCatalog.Items[kind].mount==HotpotMount.Wall?12:20;
                    Require(model.PlaceHotpotFurniture(0,slot,kind,0).success,"New furniture buy "+kind);
                    var restored=JsonUtility.FromJson<FarmState>(JsonUtility.ToJson(model.State));string error;
                    Require(FarmModel.Validate(restored,out error)&&restored.hotpot.furniture[0].kind==kind,"Furniture JSON "+kind);
                }
                finally{Object.Destroy(instance);}
            }
        }
        private static void PrepareService(FarmHotpot shop,int floor,int slot)
        {
            int key=floor*26+slot;var session=shop.Service;
            if(session.Find(key)==null)Require(session.Seat(key),"Seat review table");
            var order=session.Find(key);order.patience=120;
            if(!order.answered)Require(session.Answer(key,true),"Accept review request");
            Require(session.Cook(key),"Start cooking before serving");
            for(int i=0;i<21;i++)session.Tick(1);
            typeof(FarmHotpot).GetMethod("SyncServiceTables",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(shop,null);
        }
    }
}
