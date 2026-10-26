using System;
using System.Collections;
using System.IO;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class FarmJourneyReview
    {
        internal static void Run()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            if(live==null||live.Model==null)throw new InvalidOperationException("Open farm in Play first.");
            var test=new GameObject("Journey isolated review").AddComponent<FarmGame>();test.StartCoroutine(Review(live,test));
        }
        private static void Check(bool pass,string message){if(!pass)throw new InvalidOperationException(message);}
        private static IEnumerator Review(FarmGame live,FarmGame test)
        {
            const string result=".tools/model-work/journey-result.json",folder="Art/Previews/Farm/Journey/";
            bool success=false;string original=JsonUtility.ToJson(live.Model.State);
            var sun=RenderSettings.sun;var ambient=RenderSettings.ambientLight;bool fog=RenderSettings.fog;
            live.gameObject.SetActive(false);File.WriteAllText(result,"{\"status\":\"running\"}");
            try
            {
                double now=FarmGame.UtcNow();var state=FarmModelPark.CreateState(now);state.journey.medals=36;
                var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/journey-review"));
                var legacy=FarmModel.NewState(now);legacy.coins=321;
                var inline=new FarmModel(JsonUtility.FromJson<FarmState>(JsonUtility.ToJson(legacy)));
                Check(inline.State.coins==321&&inline.State.journey.nextSerial==1&&inline.State.journey.active==null,"Inline null migration");
                string legacyJson=JsonUtility.ToJson(legacy);int field=legacyJson.LastIndexOf(",\"journey\":",StringComparison.Ordinal);
                Check(field>=0,"Locate last schema field");
                var oldFarm=new FarmModel(JsonUtility.FromJson<FarmState>(legacyJson.Substring(0,field)+"}"));
                Check(oldFarm.State.coins==321&&oldFarm.State.journey.nextSerial==1,"Missing field migration");
                test.InitializeSession(store,()=>now,state);test.enabled=false;var m=test.Model;
                Directory.CreateDirectory(folder);test.Development.Open(8);yield return new WaitForSecondsRealtime(.4f);
                int coins=m.State.coins;test.ActOnPlot(0);Check(m.State.coins==coins&&test.SelectedPlot==-1,"World input leak");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+"Contracts.png"));yield return new WaitForSecondsRealtime(.3f);
                test.Development.Perform(()=>m.AcceptJourneyContract(0));var contract=m.State.journey.active;
                Check(contract!=null&&!m.CompleteJourneyContract(contract.serial).success,"Old stock skipped fresh activity");
                for(int i=0;i<contract.target;i++)
                {
                    var p=m.State.plots[i];p.cropId=contract.item;p.plantedAt=now-1000;p.soilWater=.25;p.soilQuality=2;p.disease=0;p.pest=0;
                    Check(m.Harvest(i,1,now).success,"Fixture harvest failed");
                }
                test.Development.Open(8);yield return new WaitForSecondsRealtime(.3f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+"Active-contract.png"));yield return new WaitForSecondsRealtime(.3f);
                Check(store.Save(m.State),"Save active contract");var restored=new FarmModel(store.Load(now));
                Check(restored.State.journey.active.serial==contract.serial&&restored.JourneyFreshProgress(restored.State.journey.active)==contract.target,"JSON lost active progress");
                Check(restored.CompleteJourneyContract(contract.serial).success&&!restored.CompleteJourneyContract(contract.serial).success,"Replay guard after load");
                test.Development.Perform(()=>m.CompleteJourneyContract(contract.serial));
                test.Development.SelectSection(1);yield return new WaitForSecondsRealtime(.3f);
                Check(test.Development.ContentPages==5,"Missing mastery pages");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+"Mastery.png"));yield return new WaitForSecondsRealtime(.3f);
                test.Development.ChangeContentPage(4);yield return null;Check(test.Development.ContentPage==4,"Mastery last page");
                test.Development.Perform(()=>m.UpgradeJourneyProject(0));test.Development.SelectSection(2);yield return new WaitForSecondsRealtime(.3f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+"Projects.png"));yield return new WaitForSecondsRealtime(.3f);
                Check(store.Save(m.State),"Save project");restored=new FarmModel(store.Load(now));Check(store.LastError==null&&restored.State.journey.workshop==1&&restored.State.journey.active==null,"Lost project or empty-contract fallback");
                // Prepare the fourth season summary visual; real multi-year progression is tested in Core.
                m.State.journey.completed=48;m.State.journey.seasons=3;m.State.journey.nextSerial=49;
                test.Development.Open(8);yield return new WaitForSecondsRealtime(.3f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+"Season-summary.png"));yield return new WaitForSecondsRealtime(.3f);
                Check(m.AdvanceJourneySeason().success&&m.JourneyYear==2,"Year rollover");
                Check(JsonUtility.ToJson(live.Model.State)==original,"Live farm changed");success=true;
            }
            finally
            {
                test.gameObject.SetActive(false);Object.Destroy(test.gameObject);
                RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;RenderSettings.fog=fog;
                live.gameObject.SetActive(true);var park=live.GetComponent<FarmModelPark>();if(park!=null)park.ShowFarm();live.Development.Open(8);
                File.WriteAllText(result,success?"{\"status\":\"complete\",\"activeContractJson\":true,\"duplicateBlocked\":true,\"projectsSaved\":true,\"masteryPages\":5,\"yearRollover\":true,\"worldInputBlocked\":true,\"liveFarmUnchanged\":true}":"{\"status\":\"failed\"}");
            }
        }
    }
}
