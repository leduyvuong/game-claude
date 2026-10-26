using System;
using System.Collections;
using System.IO;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle.Editor
{
    public static class FarmOnlineReview
    {
        public static void Verify()
        {
            var live=UnityEngine.Object.FindFirstObjectByType<FarmGame>();
            if(live==null||live.Online!=null)throw new InvalidOperationException("Open solo farm before online review");
            var hub=FarmOnlineHub.Open(live);hub.StartCoroutine(Run(live,hub));
        }
        private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        private static IEnumerator Run(FarmGame live,FarmOnlineHub hub)
        {
            string path=".tools/model-work/online-result.json";
            File.WriteAllText(path,"{\"status\":\"running\"}");
            bool complete=false;
            try
            {
                string name="review_"+Guid.NewGuid().ToString("N").Substring(0,12),password=Guid.NewGuid().ToString("N");
                hub.Authenticate(name,password,true);
                while(hub.Busy)yield return null;
                Require(hub.OnlineGame!=null,"Unity HTTP registration and server farm");
                string solo=JsonUtility.ToJson(live.Model.State);
                var online=hub.OnlineGame;Require(online.Model.State.coins==120,"Separate starting wallet");
                hub.ClosePanel();online.SelectCrop("carrot");online.ActOnPlot(0);
                while(hub.Busy)yield return null;
                Require(hub.LastReply.success&&online.Model.State.plots[0].cropId=="carrot","Plant roundtrip");
                int plantedCoins=online.Model.State.coins;
                online.SelectTool(FarmTool.Water);online.ActOnPlot(0);
                while(hub.Busy)yield return null;
                Require(hub.LastReply.success&&online.Model.State.plots[0].watered,"Water roundtrip");
                online.Ranch.Act("chicken","buy");while(hub.Busy)yield return null;
                Require(online.Model.Animal("chicken").count==1&&online.Model.State.coins==plantedCoins-30,"Ranch routed to server");
                hub.Command("daily");while(hub.Busy)yield return null;
                Require(hub.LastReply.success,"Daily server reward");
                int rewarded=online.Model.State.coins;
                hub.Command("daily");while(hub.Busy)yield return null;
                Require(!hub.LastReply.success&&online.Model.State.coins==rewarded,"No duplicate reward");
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Online-farm.png"));
                yield return new WaitForSecondsRealtime(.5f);
                Require(JsonUtility.ToJson(live.Model.State)==solo,"Solo farm unchanged during online session");
                hub.ReturnToSolo();while(hub.Busy)yield return null;
                Require(hub.OnlineGame==null&&live.gameObject.activeSelf,"Return to solo");
                FarmOnlineHub.Open(live);hub.Authenticate(name,password,false);
                while(hub.Busy)yield return null;
                Require(hub.OnlineGame!=null&&hub.OnlineGame.Model.State.coins==rewarded&&hub.OnlineGame.Model.Animal("chicken").count==1,"Re-login persistence");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Online-hub.png"));
                yield return new WaitForSecondsRealtime(.5f);
                hub.ReturnToSolo();while(hub.Busy)yield return null;
                complete=true;
                File.WriteAllText(path,"{\"status\":\"complete\",\"checks\":\"register, separate farm, plant, water, ranch, daily, duplicate rejection, solo preservation, logout, re-login persistence\"}");
            }
            finally
            {
                if(!complete){File.WriteAllText(path,"{\"status\":\"failed\",\"detail\":\"See live-editor.log\"}");if(!hub.Busy)hub.ReturnToSolo();}
            }
        }
    }
}
