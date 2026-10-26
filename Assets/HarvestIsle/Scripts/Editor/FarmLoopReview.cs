using System;
using System.IO;
using System.Linq;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class FarmLoopReview
    {
        internal static void Verify()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            if(live==null||live.Model==null)throw new InvalidOperationException("Open the farm in Play first");
            var root=new GameObject("Isolated farm loop verification");
            var game=root.AddComponent<FarmGame>();
            game.StartCoroutine(VerifyAfterLayout(live,game));
        }
        private static System.Collections.IEnumerator VerifyAfterLayout(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);
            string folder=Path.GetFullPath(".tools/model-work/farm-loop-"+DateTime.UtcNow.Ticks);
            // Keep the three normal-price cycles outside the periodic festival window.
            var store=new FarmSaveStore(folder);double now=Math.Floor(FarmGame.UtcNow()/600)*600+20;
            GameObject testRoot=game.gameObject;
            Light previousSun=RenderSettings.sun;
            live.gameObject.SetActive(false);
            game.InitializeSession(store,()=>now);
            // UGUI needs a frame to apply CanvasScaler and populate graphic raycast depths.
            yield return null;
            yield return null;
            try
            {
                var plots=game.World.GetComponentsInChildren<FarmPlot>().OrderBy(p=>p.Index).ToArray();
                Require(plots.Length==24,"24 plots");int stages=0;
                var land=game.World.GetComponentsInChildren<MeshCollider>().First(c=>c.sharedMesh!=null&&c.sharedMesh.name=="IslandBase geometry");
                Require(land.bounds.size.x>35&&land.bounds.size.z>28,"Expanded island bounds");
                foreach(var point in new[]{new Vector3(12,20,0),new Vector3(-12,20,0),new Vector3(0,20,-11),new Vector3(0,20,12)})
                    Require(land.Raycast(new Ray(point,Vector3.down),out RaycastHit ground,40)&&ground.point.y>=.1f,"New land has ground collision");
                VerifyInput(game,plots);
                VerifyHud(game);
                int expectedCoins=120;
                foreach(var crop in CropCatalog.All.Take(3))
                {
                    int index=Array.IndexOf(CropCatalog.All,crop);
                    game.SelectCrop(crop.id);game.ActOnPlot(index);
                    Require(game.Model.State.coins==expectedCoins-crop.seedCost,"Seed debit");
                    Require(plots[index].RenderedStage==1,"Seedling visual");stages++;
                    Click(game,"Tưới cây");Require(plots[index].IsWateredVisual,"Wet tilled soil");
                    double start=now;
                    for(int stage=2;stage<=5;stage++)
                    {
                        now=start+crop.growSeconds*.65*(stage==5?1.01:stage==2?.3:stage==3?.55:.8);
                        game.SelectTool(FarmTool.Water);
                        // Public Refresh paths use the observed session clock on commands.
                        game.ActOnPlot(index);
                        Require(plots[index].RenderedStage==stage,"Growth visual "+stage);stages++;
                    }
                    game.Save();var loaded=new FarmModel(store.Load(now+600));
                    Require(loaded.Progress(index,now+600)==1&&loaded.State.plots[index].watered,"Save/offline growth");
                    game.SelectTool(FarmTool.Harvest);game.ActOnPlot(index);
                    Require(game.IsHarvesting,"Harvest mini-game opens");
                    game.HarvestGame.Cancel();Require(game.Model.State.plots[index].cropId==crop.id,"Cancel keeps crop");game.ActOnPlot(index);
                    var challenge=game.HarvestGame.Challenge;
                    for(int round=0;round<3;round++)
                        if(crop.id=="corn")challenge.Choose(round==0?challenge.Sequence[round]:(challenge.Sequence[round]+1)%3);
                        else challenge.Timing(round==0?.5:0);
                    game.HarvestGame.Complete();Require(!game.IsHarvesting,"Harvest mini-game closes");
                    Require(game.Model.Count(crop.id)==crop.yield&&plots[index].RenderedStage==0,"Harvest and clear plot");
                    Click(game,"Kho");
                    var sale=game.Model.SellStock(crop.id,1,game.Model.Count(crop.id));Require(sale.success,"Selective sale succeeds");game.Save();
                    Click(game,"Kho");
                    expectedCoins+=crop.yield*crop.sellPrice-crop.seedCost;
                    Require(game.Model.State.coins==expectedCoins&&game.Model.Count(crop.id)==0,"Sale credits once");
                    game.Sell();Require(game.Model.State.coins==expectedCoins,"Reject duplicate sale");
                }
                game.Save();var reloaded=store.Load(now);
                Require(reloaded.coins==161&&reloaded.harvested==3&&reloaded.hasSold&&reloaded.hasWatered,"Reload economy and tutorial");
                string path=Path.Combine(folder,"harvest-isle-v1.json");
                File.WriteAllText(path,"{broken");var recovered=store.Load(now);
                Require(recovered.coins==161&&store.LastError!=null,"Backup recovery");
                VerifyEcology(game,plots,now,store);
                Require(JsonUtility.ToJson(live.Model.State)==before,"Existing farm untouched");
                File.WriteAllText(".tools/model-work/farm-loop-result.json","{\"status\":\"complete\",\"crops\":3,\"stages\":"+stages+",\"plots\":24,\"finalCoins\":161,\"saveReload\":true,\"backupRecovery\":true,\"existingFarmUntouched\":true}");
                Debug.Log("FARM_LOOP_VERIFIED: input guards, three full crop cycles, 15 growth visuals, soil, UI actions, economy and save recovery.");
            }
            catch(Exception e){File.WriteAllText(".tools/model-work/farm-loop-result.json",JsonUtility.ToJson(new Failure{error=e.ToString()},true));Debug.LogException(e);}
            finally
            {
                if(testRoot!=null)Object.Destroy(testRoot);
                RenderSettings.sun=previousSun;
                live.gameObject.SetActive(true);
                Directory.CreateDirectory("Art/Previews/Farm");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Playable-farm.png"));
            }
        }
        private static void Click(FarmGame game,string label)
        {
            var button=game.Hud.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.GetComponentInChildren<Text>().text==label||b.name==label);
            Require(button!=null&&button.interactable,"Enabled UI button: "+label);button.onClick.Invoke();
        }
        private static bool PanelOpen(FarmGame game,string name)
        {
            return game.Hud.GetComponentsInChildren<RectTransform>().Any(t=>t.name==name);
        }
        private static void VerifyHud(FarmGame game)
        {
            game.ClearSelection();
            Require(!PanelOpen(game,"Plot detail"),"Unselected plot panel hidden");
            Click(game,"Menu");Require(PanelOpen(game,"Menu panel"),"Menu opens");
            Click(game,"Hướng dẫn & tiến độ");Require(PanelOpen(game,"Help")&&!PanelOpen(game,"Menu panel"),"Help replaces menu");
            Click(game,"Kho");Require(PanelOpen(game,"Inventory")&&!PanelOpen(game,"Help"),"Inventory replaces help");
            Click(game,"Giống cây");Require(PanelOpen(game,"Seeds")&&!PanelOpen(game,"Inventory"),"Seed tray replaces inventory");
            var seed=game.Hud.GetComponentsInChildren<Button>().First(b=>b.name.StartsWith("Cà rốt"));
            seed.onClick.Invoke();Require(!PanelOpen(game,"Seeds")&&game.Tool==FarmTool.Plant,"Seed choice closes tray");
            game.SelectTool(FarmTool.Inspect);game.ActOnPlot(8);Require(PanelOpen(game,"Plot detail"),"Selection reveals plot card");
            game.ClearSelection();Require(game.SelectedPlot==-1&&!PanelOpen(game,"Plot detail"),"Close deselects plot");
        }

        internal static void PreviewHud()
        {
            var game=Object.FindFirstObjectByType<FarmGame>();
            Require(game!=null,"Farm open");game.StartCoroutine(HudPreviewRoutine(game));
        }
        private static System.Collections.IEnumerator HudPreviewRoutine(FarmGame game)
        {
            if(game.Ranch.IsOpen)game.Ranch.Close();
            game.Hud.ClosePanels();game.SelectTool(FarmTool.Inspect);game.ClearSelection();game.CameraController.GoHome();
            yield return new WaitForSecondsRealtime(6.5f);
            string folder=Path.GetFullPath("Art/Previews/Farm/");Directory.CreateDirectory(folder);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"UI-clean.png"));
            yield return new WaitForSecondsRealtime(.5f);
            game.ActOnPlot(8);
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"UI-plot.png"));
            yield return new WaitForSecondsRealtime(.5f);
            Click(game,"Kho");
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"UI-inventory.png"));
            yield return new WaitForSecondsRealtime(.5f);
            Click(game,"Menu");
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"UI-menu.png"));
            yield return new WaitForSecondsRealtime(.5f);
            Click(game,"Giống cây");
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"UI-seeds.png"));
            yield return new WaitForSecondsRealtime(.5f);
            game.Hud.ClosePanels();game.ClearSelection();
            File.WriteAllText(".tools/model-work/hud-result.json","{\"status\":\"complete\",\"screenshots\":5}");
        }
        internal static void PreviewGames()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();Require(live!=null,"Farm open");
            var game=new GameObject("Isolated ecology presentation").AddComponent<FarmGame>();
            game.StartCoroutine(PreviewRoutine(live,game));
        }
        private static System.Collections.IEnumerator PreviewRoutine(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);Light previousSun=RenderSettings.sun;
            double now=FarmGame.UtcNow();live.gameObject.SetActive(false);
            try
            {
                game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/ecology-preview-"+DateTime.UtcNow.Ticks)),()=>now);
                game.CameraController.GoHome();Directory.CreateDirectory("Art/Previews/Farm");
                yield return null;
                for(int i=0;i<3;i++)
                {
                    string crop=CropCatalog.All[i].id;game.SelectCrop(crop);game.ActOnPlot(i);now+=121;
                    game.SelectTool(FarmTool.Harvest);game.ActOnPlot(i);Require(game.IsHarvesting,"Mini-game preview opened");
                    yield return null;yield return new WaitForSecondsRealtime(.3f);
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Harvest-"+crop+".png"));
                    yield return new WaitForSecondsRealtime(.3f);
                    game.HarvestGame.Cancel();
                }
                game.Model.State.climateTime=90;game.Model.AdvanceEcology(110);game.SelectTool(FarmTool.Inspect);game.ActOnPlot(0);
                yield return new WaitForSecondsRealtime(1.2f);
                Require(game.GetComponentsInChildren<ParticleSystem>().Any(p=>p.particleCount>0),"Live rain particles");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Rain-disease-play.png"));
                yield return new WaitForSecondsRealtime(.3f);
                Require(JsonUtility.ToJson(live.Model.State)==before,"User save preserved during previews");
                File.WriteAllText(".tools/model-work/farm-games-preview-result.json","{\"status\":\"complete\",\"miniGameScreens\":3,\"liveRain\":true,\"userFarmUntouched\":true}");
            }
            finally{Object.Destroy(game.gameObject);RenderSettings.sun=previousSun;live.gameObject.SetActive(true);}
        }
        private static void VerifyEcology(FarmGame game,FarmPlot[] plots,double now,FarmSaveStore store)
        {
            game.SelectCrop("carrot");game.ActOnPlot(0);
            game.Model.State.climateTime=90;game.Model.AdvanceEcology(110);
            game.World.Refresh(game.Model,now+1000,0);
            Require(plots[0].HasPuddle&&plots[0].HealthVisual>0,"Rain puddle and sick crop prefab");
            var water=plots[0].GetComponentsInChildren<MeshRenderer>().First(r=>r.sharedMaterial.shader.name=="Harvest Isle/Farm Puddle");
            Require(water.GetComponent<MeshFilter>().sharedMesh.bounds.size.y<.001f,"Standing water is flat");
            Require(water.GetComponent<Collider>()==null,"Water does not block plot picking");
            Require(water.sharedMaterial.shader.isSupported,"Puddle shader supported");
            game.Save();
            var saved=store.Load(now);Require(saved.plots[0].disease==game.Model.State.plots[0].disease&&saved.plots[0].soilWater==game.Model.State.plots[0].soilWater,"Ecology save roundtrip");
            game.CameraController.GoHome();
            game.World.GameCamera.orthographicSize=5.5f;
            CaptureEcology(game.World.GameCamera);
            game.CameraController.FocusOn(plots[1].transform.position);
            game.World.GameCamera.orthographicSize=2.6f;
            CaptureEcology(game.World.GameCamera,"Puddles-detail.png");
            game.CameraController.GoHome();
            game.Model.AdvanceEcology(40);game.World.Refresh(game.Model,now+1000,0);
            Require(plots[0].HealthVisual==2,"Severe disease switches to wilted prefab");
            game.SelectTool(FarmTool.Drain);game.ActOnPlot(0);Require(!plots[0].HasPuddle,"Drain removes standing water");
            double illness=game.Model.State.plots[0].disease;
            game.SelectTool(FarmTool.Treat);game.ActOnPlot(0);Require(game.Model.State.plots[0].disease<illness,"Treatment heals");
            game.SelectTool(FarmTool.Harvest);game.ActOnPlot(0);
            if(game.IsHarvesting){int coins=game.Model.State.coins;game.HarvestGame.Cancel();Require(game.Model.State.coins==coins&&game.Model.State.plots[0].cropId=="carrot","Cancel preserves crop");}
            game.SelectTool(FarmTool.Inspect);game.ActOnPlot(0);
            int beforeBuild=game.Model.State.coins;
            game.Hud.SetCareExpanded(true);Click(game,"Xây rãnh · "+FarmModel.Money(FarmModel.DrainageCost));
            Require(game.Model.State.coins==beforeBuild-FarmModel.DrainageCost&&plots[0].HasDrainageVisual,"Drain construction UI and model");
            game.Save();Require(store.Load(now).plots[0].drainageBuilt,"Permanent drain saved to JSON");
            game.Model.State.plots[0].drainProtection=0;game.Model.State.plots[0].soilWater=.05;
            game.Model.State.climateTime=90;game.Model.AdvanceEcology(139);game.World.Refresh(game.Model,now,0);
            Require(!plots[0].HasPuddle&&plots[1].HasPuddle,"Permanent protection through rain without temporary drain");
            File.WriteAllText(".tools/model-work/farm-ecology-result.json","{\"status\":\"complete\",\"puddles\":true,\"diseaseVisual\":true,\"drainage\":true,\"treatment\":true,\"harvestGames\":3}");
        }
        private static void CaptureEcology(Camera camera,string filename="Waterlogging-disease.png")
        {
            Directory.CreateDirectory("Art/Previews/Farm");
            var target=new RenderTexture(1440,900,24);var pixels=new Texture2D(1440,900,TextureFormat.RGB24,false);var previous=RenderTexture.active;float aspect=camera.aspect;
            try
            {
                target.Create();camera.aspect=1.6f;
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1440,900),0,0);pixels.Apply();File.WriteAllBytes("Art/Previews/Farm/"+filename,pixels.EncodeToPNG());
            }
            finally{camera.aspect=aspect;RenderTexture.active=previous;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);}
        }
        internal static void PreviewDrainage()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();Require(live!=null,"Farm open");
            var game=new GameObject("Isolated drainage preview").AddComponent<FarmGame>();
            game.StartCoroutine(DrainagePreviewRoutine(live,game));
        }
        internal static void PreviewDensity()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();Require(live!=null,"Farm open");
            var game=new GameObject("Isolated crop density preview").AddComponent<FarmGame>();
            game.StartCoroutine(DensityPreviewRoutine(live,game));
        }
        private static System.Collections.IEnumerator DensityPreviewRoutine(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);Light previousSun=RenderSettings.sun;
            live.gameObject.SetActive(false);
            try
            {
                double now=FarmGame.UtcNow();
                game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/density-"+DateTime.UtcNow.Ticks)),()=>now);
                game.enabled=false;
                string[] crops={"carrot","corn","rice","greens","tomato","wheat"};
                for(int i=0;i<game.Model.State.plots.Count;i++)
                {
                    var plot=game.Model.State.plots[i];
                    plot.cropId=crops[i%6];plot.plantedAt=now-1000;plot.watered=true;
                }
                game.World.Refresh(game.Model,now,-1);game.CameraController.GoHome();
                game.World.GameCamera.orthographicSize=4.8f;
                yield return null;yield return null;
                CaptureEcology(game.World.GameCamera,"Dense-crop-field.png");
                var plots=game.World.GetComponentsInChildren<FarmPlot>().OrderBy(p=>p.Index).ToArray();
                game.CameraController.FocusOn((plots[8].transform.position+plots[9].transform.position)*.5f);
                game.World.GameCamera.orthographicSize=2.1f;
                yield return null;
                CaptureEcology(game.World.GameCamera,"Dense-crop-detail.png");
                Require(JsonUtility.ToJson(live.Model.State)==before,"Player farm preserved during density preview");
                File.WriteAllText(".tools/model-work/farm-density-result.json","{\"status\":\"complete\",\"previewPlots\":24,\"userFarmUntouched\":true}");
            }
            finally{Object.Destroy(game.gameObject);RenderSettings.sun=previousSun;live.gameObject.SetActive(true);}
        }
        private static System.Collections.IEnumerator DrainagePreviewRoutine(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);Light previousSun=RenderSettings.sun;
            live.gameObject.SetActive(false);
            try
            {
                double now=FarmGame.UtcNow();var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/drainage-"+DateTime.UtcNow.Ticks));
                game.InitializeSession(store,()=>now);
                game.SelectCrop("carrot");game.ActOnPlot(8);
                now+=60;
                foreach(var plot in game.Model.State.plots){plot.soilWater=.90;plot.watered=true;}
                game.Model.State.climateTime=190;
                game.SelectTool(FarmTool.Inspect);game.ActOnPlot(8);game.CameraController.GoHome();
                yield return null;yield return null;
                Directory.CreateDirectory("Art/Previews/Farm");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Drainage-before.png"));
                yield return new WaitForSecondsRealtime(.4f);
                game.Hud.SetCareExpanded(true);Click(game,"Xây rãnh · "+FarmModel.Money(FarmModel.DrainageCost));Click(game,"Thoát nước\nMiễn phí");
                yield return null;
                Require(game.Model.State.plots[8].drainageBuilt&&game.Model.HarvestYield(8,1)==3,"Protected yield recovered");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Drainage-after.png"));
                yield return new WaitForSecondsRealtime(.4f);
                Require(JsonUtility.ToJson(live.Model.State)==before,"Live farm preserved");
                File.WriteAllText(".tools/model-work/drainage-preview-result.json","{\"status\":\"complete\",\"screenshots\":2,\"liveFarmUntouched\":true}");
            }
            finally{Object.Destroy(game.gameObject);RenderSettings.sun=previousSun;live.gameObject.SetActive(true);}
        }
        private static void VerifyInput(FarmGame game,FarmPlot[] plots)
        {
            Canvas.ForceUpdateCanvases();Physics.SyncTransforms();
            var camera=game.World.GameCamera;var controller=game.CameraController;
            controller.GoHome();
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            Action<string,object[]> call=(name,args)=>typeof(FarmCamera).GetMethod(name,flags).Invoke(controller,args);
            Func<Vector2,bool> over=p=>(bool)typeof(FarmCamera).GetMethod("OverUi",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,new object[]{p});
            Vector2 p=camera.WorldToScreenPoint(plots[8].transform.position);Require(!over(p),"Plot is unobstructed by UI");
            call("Begin",new object[]{p});call("End",new object[]{p});
            Require(game.SelectedPlot==8&&game.Model.State.coins==120,"Inspect selects without seed debit");
            game.SelectCrop("carrot");
            call("Begin",new object[]{p});call("End",new object[]{p+new Vector2(90,0)});
            Require(game.Model.State.coins==120,"Fast swipe never plants");
            call("Begin",new object[]{p});call("Move",new object[]{p+new Vector2(90,0)});call("End",new object[]{p+new Vector2(90,0)});
            Require(game.Model.State.coins==120,"Drag never plants");controller.GoHome();
            call("Begin",new object[]{p});call("CancelGesture",new object[0]);call("End",new object[]{p});
            Require(game.Model.State.coins==120,"Cancelled gesture never plants");
            var uiRect=(RectTransform)game.Hud.GetComponentsInChildren<Button>().First(b=>b.name=="Kho").transform;
            Vector2 ui=RectTransformUtility.WorldToScreenPoint(null,uiRect.TransformPoint(uiRect.rect.center));
            Require(over(ui),"UI raycast blocks world");
            call("Begin",new object[]{ui});call("End",new object[]{p});
            Require(game.Model.State.coins==120,"UI gesture never reaches plot");
            float size=camera.orthographicSize;
            call("BeginPinch",new object[]{ui,p});call("ContinuePinch",new object[]{p-new Vector2(20,0),p+new Vector2(20,0)});
            Require(camera.orthographicSize==size,"Pinch beginning on UI stays blocked");
            call("End",new object[]{p});Require(game.Model.State.coins==120,"Blocked pinch tail never plants");
            call("BeginPinch",new object[]{p-new Vector2(30,0),p+new Vector2(30,0)});
            call("ContinuePinch",new object[]{p-new Vector2(60,0),p+new Vector2(60,0)});
            Require(camera.orthographicSize<size,"World pinch zooms");
            call("End",new object[]{p});Require(game.Model.State.coins==120,"Pinch tail never plants");
            controller.ZoomBy(100);Require(camera.orthographicSize==76,"Far zoom clamp for expanded island");
            controller.ZoomBy(-100);Require(camera.orthographicSize==5.5f,"Near zoom clamp");
            controller.GoHome();Require(camera.orthographicSize==5.8f,"Home resets close farm view");
            game.SelectTool(FarmTool.Inspect);
            File.WriteAllText(".tools/model-work/farm-input-result.json","{\"status\":\"complete\",\"inspect\":true,\"fastSwipe\":true,\"drag\":true,\"cancel\":true,\"uiBlocking\":true,\"pinch\":true,\"pinchTail\":true,\"zoomLimits\":true}");
        }
        private static void Require(bool condition,string check){if(!condition)throw new InvalidOperationException("Farm check failed: "+check);}
        [Serializable] private sealed class Failure{public string status="failed";public string error;}
    }
}
