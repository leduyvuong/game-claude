using System;
using System.Collections;
using System.IO;
using System.Linq;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace HarvestIsle.Editor
{
    internal static class PlayInterfaceReview
    {
        internal static void Run()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            if(live==null||live.Model==null)throw new InvalidOperationException("Farm must be playing");
            var test=new GameObject("Play interface review").AddComponent<FarmGame>();test.StartCoroutine(Review(live,test));
        }
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private static Button Find(FarmGame game,string name)
        {return game.Hud.GetComponentsInChildren<Button>().First(b=>b.name==name);}
        private static IEnumerator Shot(string name)
        {yield return new WaitForSecondsRealtime(.6f);ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Play-redesign/"+name+".png"));yield return new WaitForSecondsRealtime(.3f);}
        private static IEnumerator Review(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);Light sun=RenderSettings.sun;Color ambient=RenderSettings.ambientLight;bool success=false;
            Directory.CreateDirectory("Art/Previews/Farm/Play-redesign");
            File.WriteAllText(".tools/model-work/play-interface-result.json","{\"status\":\"running\"}");
            live.gameObject.SetActive(false);
            try
            {
                double now=FarmGame.UtcNow();var state=FarmModelPark.CreateState(now);state.coins=1200;state.workers.Clear();state.harvested=10;
                foreach(var p in state.plots){p.disease=0;p.soilWater=.2;p.pest=0;}
                state.plots[0].cropId="";state.plots[0].watered=false;
                game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/play-interface-farm")),()=>now,state);
                var day=game.gameObject.AddComponent<FarmDayNight>();day.Configure(game);day.SetPhase(.5f);
                game.CameraController.GoHome();yield return null;yield return null;
                yield return Shot("01-Farm");
                game.Hud.TapPlot(0);yield return null;
                Require(game.Hud.IsReading,"Empty plot should open seed tray");
                var scroll=game.Hud.GetComponentInChildren<ScrollRect>();Require(scroll!=null,"Seed strip should scroll");
                Require(scroll.content.GetComponentsInChildren<Button>().Length==CropCatalog.All.Length,"All seeds should be in strip");
                yield return Shot("02-Seeds");
                scroll.horizontalNormalizedPosition=1;yield return null;yield return Shot("03-Seeds-last");scroll.horizontalNormalizedPosition=0;
                Find(game,"Cà rốt").onClick.Invoke();Require(game.Model.State.plots[0].cropId=="carrot","Choosing seed should plant selected empty plot");
                game.SelectTool(FarmTool.Inspect);game.ActOnPlot(0);yield return null;
                game.Hud.ActOnSelected();Require(game.Model.State.plots[0].watered,"Context action should water");
                now+=120;game.SelectTool(FarmTool.Inspect);game.ActOnPlot(0);yield return null;
                yield return Shot("04-Context");
                int count=game.Model.Count("carrot");game.Hud.ActOnSelected();
                Require(!game.IsHarvesting&&game.Model.Count("carrot")>count,"Quick harvest should return stock without minigame");
                yield return new WaitForSecondsRealtime(.2f);yield return Shot("05-Harvest");
                game.QuickHarvest(0);Require(string.IsNullOrEmpty(game.Model.State.plots[0].cropId),"Repeat harvest must not create crop");
                game.Hud.OpenTrading();yield return null;
                double climate=game.Model.State.climateTime;yield return new WaitForSecondsRealtime(1.2f);
                Require(game.Model.State.climateTime==climate,"Reading inventory must pause adverse ecology");
                yield return Shot("06-Trading");
                game.Hud.OpenOrders();yield return Shot("07-Orders");
                game.Hud.ClosePanels();game.ClearSelection();
                game.Development.OpenBuilding("mill");yield return Shot("08-Factory");game.Development.Close();
                game.Ranch.OpenPen(0);yield return Shot("09-Ranch");game.Ranch.Close();
                game.CameraController.GoHome();day.SetPhase(0);yield return Shot("10-Night");
                Require(JsonUtility.ToJson(live.Model.State)==before,"Live save changed");success=true;
            }
            finally
            {
                game.gameObject.SetActive(false);Object.Destroy(game.gameObject);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;
                live.gameObject.SetActive(true);live.Development.Close();live.Hud.ClosePanels();live.ClearSelection();live.CameraController.GoHome();
                File.WriteAllText(".tools/model-work/play-interface-result.json",success?"{\"status\":\"complete\",\"seeds\":"+CropCatalog.All.Length+",\"contextPlantWaterHarvest\":true,\"readingPausesEcology\":true,\"liveFarmUnchanged\":true}":"{\"status\":\"failed\"}");
            }
        }
    }
}
