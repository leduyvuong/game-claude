using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarvestIsle.Core;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class FarmRecruitmentReview
    {
        internal static void Run()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            if(live==null)throw new InvalidOperationException("Open farm in Play first");
            var runner=new GameObject("Recruitment review runner").AddComponent<FarmEstateReviewRunner>();
            runner.StartCoroutine(Review(live,runner));
        }
        private static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        private static IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.6f);
            double deadline=EditorApplication.timeSinceStartup+45;
            while(ShaderUtil.anythingCompiling&&EditorApplication.timeSinceStartup<deadline)yield return null;
            yield return new WaitForSecondsRealtime(.3f);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Workers/"+name+".png"));
            yield return new WaitForSecondsRealtime(.4f);
        }
        private static void Search(FarmDevelopment view,string value)
        {typeof(FarmDevelopment).GetField("crewSearch",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,value);}
        private static IEnumerator Review(FarmGame live,FarmEstateReviewRunner runner)
        {
            string before=JsonUtility.ToJson(live.Model.State);Light sun=RenderSettings.sun;Color ambient=RenderSettings.ambientLight;bool success=false;
            Directory.CreateDirectory("Art/Previews/Farm/Workers");File.WriteAllText(".tools/model-work/recruitment-result.json","{\"status\":\"running\"}");
            live.gameObject.SetActive(false);
            var game=new GameObject("Isolated recruitment farm").AddComponent<FarmGame>();
            try
            {
                double now=FarmGame.UtcNow();var state=FarmEstate.CreateState(now);state.coins=30000;state.workerHouse=3;
                var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/recruitment-"+DateTime.UtcNow.Ticks));
                game.InitializeSession(store,()=>now,state,estate:true);game.enabled=false;
                var m=game.Model;
                var legacy=FarmModel.NewState(now);legacy.recruitment=null;legacy.workerHouse=1;legacy.workers.Add(new WorkerState{kind=1,level=1});
                var migrated=new FarmModel(JsonUtility.FromJson<FarmState>(JsonUtility.ToJson(legacy)));
                Check(migrated.State.coins==120&&migrated.State.workers.Count==1&&migrated.State.recruitment.roster.Count==0,"Unity legacy migration");
                foreach(string id in new[]{"tomato_senior","wheat_senior","soil_4"})
                {Check(m.HireSpecialist(id).success&&m.BuyWorkerShift(id).success&&m.AssignSpecialist(id,true).success,"Hire/shift/assign");}
                game.Development.Open(7);game.Development.ShowWorkerSection(0);yield return Shot("01-Crew");
                string blocked=JsonUtility.ToJson(m.State);game.ActOnPlot(0);Check(blocked==JsonUtility.ToJson(m.State),"Crew window leaks to farm");
                game.Development.ShowWorkerSection(1);Search(game.Development,"cà chua");yield return Shot("02-Tomato-specialists");
                Search(game.Development,"lúa mì");yield return Shot("03-Wheat-specialists");
                Search(game.Development,"");yield return null;
                for(int i=1;i<24;i++)game.Development.ChangeContentPage(1);
                yield return Shot("04-Catalog-last");Check(game.Development.ContentPage==23,"72 cards reachable");
                game.Development.ShowWorkerSection(2);yield return Shot("05-Recruitment");
                int coins=m.State.coins;game.Development.DrawWorkerPack(5);Check(m.State.coins==coins-150&&m.HasUnrevealedWorkers,"UI draw pays once");
                string pending=JsonUtility.ToJson(m.State.recruitment);yield return Shot("06-Five-backs");
                Check(store.Save(m.State),"Save pending pack");var restored=new FarmModel(store.Load(now));
                Check(JsonUtility.ToJson(restored.State.recruitment)==pending,"Save/reload preserves all pending cards and pity");
                game.Development.Close();game.Development.Open(7);yield return null;Check(pending==JsonUtility.ToJson(m.State.recruitment),"Reopening rerolled pack");
                game.Development.FlipWorkerCard(0);yield return Shot("07-First-flip");
                for(int i=0;i<5;i++)game.Development.FlipWorkerCard(i);
                Check(m.State.coins==coins-150&&!m.HasUnrevealedWorkers,"Flip rewards twice");yield return Shot("08-Revealed");
                Check(store.Save(m.State),"Save revealed pack");restored=new FarmModel(store.Load(now));Check(restored.State.recruitment.reveal.TrueForAll(c=>c.revealed),"Reveal persistence");
                int notes=m.State.recruitment.notes;
                Check(m.RecruitWorkers(5,m.State.recruitment.serial,n=>0).success,"Duplicate pack");
                for(int i=0;i<5;i++)game.Development.FlipWorkerCard(i);
                Check(m.State.recruitment.notes>=notes+3,"Duplicate notes awarded");yield return Shot("10-Duplicates");
                game.Development.Close();
                foreach(var p in m.State.plots){p.cropId="";p.plantedAt=0;p.pest=0;p.disease=0;}
                for(int i=0;i<2;i++){var p=m.State.plots[i];p.cropId=i==0?"tomato":"wheat";p.plantedAt=now-500;p.soilQuality=2;p.watered=true;p.soilWater=.25;}
                int actions=m.RunWorkers(now);Check(actions==2&&m.CountPrime("tomato")>0&&m.CountPrime("wheat")>0,"Two specialists produce real quality harvests");
                game.Workforce.ShowActions(m.LastWorkerActions);game.Refresh();game.CameraController.SetView(new Vector3(-2,0,7),12);
                yield return new WaitForSecondsRealtime(7);yield return Shot("09-Crew-on-farm");
                Check(game.Workforce.VisibleSpecialists==3,"Assigned worker avatars");
                Physics.SyncTransforms();Check(Physics.Raycast(game.Workforce.HousePoint+Vector3.up*15,Vector3.down,out var hit,20,1<<FarmWorld.InteractionLayer)&&hit.collider.GetComponent<FarmWorkerTarget>()!=null,"Worker house click target");
                Check(m.OwnedWorker("tomato_senior").jobs==9&&m.OwnedWorker("wheat_senior").jobs==9,"Jobs only spent on success");
                Check(JsonUtility.ToJson(live.Model.State)==before,"Live farm changed");success=true;
            }
            finally
            {
                game.gameObject.SetActive(false);Object.Destroy(game.gameObject);live.gameObject.SetActive(true);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;
                live.Development.Open(7);live.Development.ShowWorkerSection(1);
                File.WriteAllText(".tools/model-work/recruitment-result.json",success?"{\"status\":\"complete\",\"workers\":72,\"crops\":30,\"uiDraw\":true,\"pendingSaveReload\":true,\"legacyMigration\":true,\"specialistHarvests\":2,\"avatars\":3,\"worldInputBlocked\":true,\"liveFarmUntouched\":true}":"{\"status\":\"failed\"}");Object.Destroy(runner.gameObject);
            }
        }
    }
}
