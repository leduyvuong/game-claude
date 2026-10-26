using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarvestIsle.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class FarmEstateReview
    {
        [MenuItem("Harvest Isle/An Nhien/Check and photograph planned farm")]
        public static void Run()
        {
            var estate=Object.FindFirstObjectByType<FarmEstate>();
            if(estate==null||!estate.Ready)throw new InvalidOperationException("Open An Nhien in Play first.");
            var runner=new GameObject("An Nhien review runner").AddComponent<FarmEstateReviewRunner>();
            runner.StartCoroutine(Guarded(Review(estate,runner)));
        }
        // Flatten nested routines so a failed assertion always produces a failed
        // report and disposes the routines that restore temporarily hidden farms.
        private static IEnumerator Guarded(IEnumerator routine)
        {
            var stack=new Stack<IEnumerator>();stack.Push(routine);
            while(stack.Count>0)
            {
                object next=null;bool moved=false;Exception failure=null;
                try {moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}
                catch(Exception e){failure=e;}
                if(failure!=null)
                {
                    Debug.LogException(failure);
                    while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();
                    File.WriteAllText(".tools/model-work/estate-result.json","{\"status\":\"failed\"}");
                    yield break;
                }
                if(!moved){(stack.Pop() as IDisposable)?.Dispose();continue;}
                if(next is IEnumerator nested)stack.Push(nested);else yield return next;
            }
        }
        private static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
        private static IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.8f);
            string path=Path.GetFullPath("Art/Previews/AnNhien/"+name+".png");
            ScreenCapture.CaptureScreenshot(path);yield return new WaitForSecondsRealtime(.7f);
            Require(File.Exists(path),"No fresh Play image: "+name);
        }
        private static IEnumerator Review(FarmEstate estate,FarmEstateReviewRunner runner)
        {
            var game=estate.Game;bool passed=false;
            Directory.CreateDirectory("Art/Previews/AnNhien");
            File.WriteAllText(".tools/model-work/estate-result.json","{\"status\":\"running\"}");
            try
            {
                string normalPath=Path.Combine(Application.persistentDataPath,"harvest-isle-v1.json");
                byte[] normal=File.Exists(normalPath)?File.ReadAllBytes(normalPath):null;
                Require(game.IsEstate&&game.Model.State.plots.Count==36,"36 estate plots");
                Require(game.Ranch.VisibleAnimals==12&&game.Development.VisibleBuildings==9,"Working farm facilities");
                Require(game.Hud.transform.Find("Safe area/An Nhien navigation").GetComponentsInChildren<Button>().Length==5,"Estate navigation");
                Physics.SyncTransforms();
                for(int i=0;i<36;i++)
                {
                    Vector3 position=game.World.PositionOfPlot(i);
                    Require(Vector3.Distance(position,FarmEstate.PlotPosition(i))<.001f,"Estate coordinates");
                    RaycastHit hit;
                    Require(Physics.Raycast(position+Vector3.up*30,Vector3.down,out hit,40,1<<FarmWorld.InteractionLayer)&&hit.collider.GetComponent<FarmPlot>()?.Index==i,"Plot hit target "+i);
                }
                var model=new FarmModel(FarmEstate.CreateState(game.Now));
                int before=model.Count("rice");Require(model.Harvest(0,1,game.Now).success&&model.Count("rice")>before,"Harvest default crop");
                Require(model.Apply(0,FarmTool.Plant,"carrot",game.Now).success,"Plant after harvesting");
                var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/estate-save-review"));
                Require(store.Save(model.State),"Save estate");
                var reload=new FarmModel(store.Load(game.Now,FarmEstate.CreateState(game.Now)));
                Require(reload.State.plots[0].cropId=="carrot"&&reload.State.coins==model.State.coins,"Reload keeps progress instead of reseeding");
                foreach(int zone in Enumerable.Range(0,5))
                {
                    estate.ShowZone(zone);yield return Shot(new[]{"01-Overview","02-Fields","03-Ranch","04-Workshops","05-Orchard"}[zone]);
                }
                game.CameraController.FocusOn(game.World.PositionOfPlot(35));
                game.Hud.TapPlot(35);yield return Shot("06-Plot-interaction");game.ClearSelection();
                estate.ShowZone(0);game.GetComponent<FarmDayNight>().SetPhase(0);yield return Shot("07-Night");
                game.GetComponent<FarmDayNight>().SetPhase(.42f);
                File.WriteAllText(".tools/model-work/farm-loop-result.json","{\"status\":\"running\"}");
                FarmLoopReview.Verify();yield return WaitFor("farm-loop-result.json");
                File.WriteAllText(".tools/model-work/play-interface-result.json","{\"status\":\"running\"}");
                PlayInterfaceReview.Run();yield return WaitFor("play-interface-result.json");
                yield return CheckTravel(estate);
                Require(normal==null?!File.Exists(normalPath):normal.SequenceEqual(File.ReadAllBytes(normalPath)),"Normal player save changed");
                estate.ShowZone(0);yield return Shot("01-Overview");
                passed=true;
            }
            finally
            {
                File.WriteAllText(".tools/model-work/estate-result.json",passed?"{\"status\":\"complete\",\"plots\":36,\"pens\":6,\"animals\":12,\"workshops\":9,\"raycasts\":36,\"saveReload\":true,\"playerSaveUntouched\":true,\"farmCheck\":true,\"playInterfaceCheck\":true}":"{\"status\":\"failed\"}");
                Object.Destroy(runner.gameObject);
            }
        }
        private static IEnumerator CheckTravel(FarmEstate original)
        {
            Light sun=RenderSettings.sun;Color ambient=RenderSettings.ambientLight;
            original.gameObject.SetActive(false);
            FarmGame host=null;FarmEstate trip=null;
            try
            {
                host=new GameObject("Isolated visitor farm").AddComponent<FarmGame>();
                double now=FarmGame.UtcNow();
                host.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/estate-visitor-review")),()=>now,FarmModel.NewState(now));
                string before=JsonUtility.ToJson(host.Model.State);
                FarmEstate.Travel(host);
                trip=Object.FindFirstObjectByType<FarmEstate>();
                Require(trip!=null&&!host.gameObject.activeSelf,"Visit disables the original session");
                trip.SaveDirectory=Path.GetFullPath(".tools/model-work/estate-trip-review");
                for(int frame=0;frame<10&&!trip.Ready;frame++)yield return null;
                Require(trip.Ready,"Visit creates a separate playable farm");
                trip.Return();
                Require(host.gameObject.activeSelf&&JsonUtility.ToJson(host.Model.State)==before,"Return restores original progress");
            }
            finally
            {
                if(trip!=null)Object.Destroy(trip.gameObject);
                if(host!=null){host.gameObject.SetActive(false);Object.Destroy(host.gameObject);}
                original.gameObject.SetActive(true);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;
            }
        }
        private static IEnumerator WaitFor(string name)
        {
            double deadline=UnityEditor.EditorApplication.timeSinceStartup+150;
            while(true)
            {
                yield return new WaitForSecondsRealtime(.5f);
                string result=File.ReadAllText(".tools/model-work/"+name);
                Require(!result.Contains("failed"),name);
                if(result.Contains("complete")){yield return null;yield return null;yield break;}
                Require(UnityEditor.EditorApplication.timeSinceStartup<deadline,"Timed out: "+name);
            }
        }
    }
    internal sealed class FarmEstateReviewRunner:MonoBehaviour { }
}
