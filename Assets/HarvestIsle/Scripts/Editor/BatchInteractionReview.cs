using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;
namespace HarvestIsle.Editor
{
    internal static class BatchInteractionReview
    {
        internal static void Run()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            var test=new GameObject("Batch interaction review").AddComponent<FarmGame>();
            test.StartCoroutine(Review(live,test));
        }
        private static void Check(bool ok,string reason){if(!ok)throw new InvalidOperationException(reason);}
        private static void Call(object o,string name,params object[] args){o.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,args);}
        private static IEnumerator Review(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);bool success=false;
            var sun=RenderSettings.sun;var ambient=RenderSettings.ambientLight;
            Directory.CreateDirectory("Art/Previews/Farm/Batch");
            File.WriteAllText(".tools/model-work/batch-result.json","{\"status\":\"running\"}");
            live.gameObject.SetActive(false);
            try
            {
                double now=FarmGame.UtcNow();var state=FarmModelPark.CreateState(now);state.workers.Clear();state.coins=10000;
                foreach(var p in state.plots){p.cropId="";p.disease=0;p.pest=0;p.soilWater=.2;p.watered=false;}
                game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/batch-farm")),()=>now,state);
                game.CameraController.GoHome();yield return null;yield return null;
                game.SelectCrop("carrot");game.Batch.SetMode(1);
                string clean=JsonUtility.ToJson(game.Model.State);
                game.Batch.Begin();game.Batch.Add(0);game.Batch.Add(0);game.Batch.Add(1);
                Check(game.Batch.Count==2,"Repeated crossing duplicated selection");
                game.Batch.CancelSelection();Check(JsonUtility.ToJson(game.Model.State)==clean,"Canceled sow changed farm");
                game.Batch.Begin();for(int i=0;i<6;i++)game.Batch.Add(i);
                yield return new WaitForSecondsRealtime(.5f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Batch/01-Select.png"));yield return null;
                int coins=game.Model.State.coins;game.Batch.Release();
                for(int i=0;i<6;i++)Check(game.Model.State.plots[i].cropId=="carrot","Batch missed plot");
                Check(game.Model.State.coins==coins-6*CropCatalog.Find("carrot").seedCost,"Wrong seed charge");
                int after=game.Model.State.coins;game.Batch.Release();Check(game.Model.State.coins==after,"Double release charged twice");
                // Real camera path must paint multiple plots without moving its focus.
                var camera=game.World.GameCamera;var start=camera.WorldToScreenPoint(FarmWorld.PlotPosition(12));var end=camera.WorldToScreenPoint(FarmWorld.PlotPosition(17));
                Vector3 cameraBefore=camera.transform.position;
                Call(game.CameraController,"Begin",(Vector2)start);Call(game.CameraController,"Move",(Vector2)end);
                Check(game.Batch.Count>=2&&camera.transform.position==cameraBefore,"Drag did not paint or moved camera");
                Call(game.CameraController,"BeginPinch",(Vector2)start,(Vector2)end);
                Check(game.Batch.Count==0,"Pinch did not cancel preview");
                game.Batch.SetMode(2);now+=300;yield return null;
                game.Batch.Begin();for(int i=0;i<6;i++)game.Batch.Add(i);game.Batch.Release();
                Check(game.IsHarvesting,"Batch challenge did not start");
                string pending=JsonUtility.ToJson(game.Model.State);game.Batch.Cancel();
                Check(JsonUtility.ToJson(game.Model.State)==pending,"Cancel consumed crops");
                game.Batch.SetMode(2);game.Batch.Begin();for(int i=0;i<6;i++)game.Batch.Add(i);game.Batch.Release();
                yield return new WaitForSecondsRealtime(.4f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Batch/02-Rhythm.png"));yield return null;
                int harvested=game.Model.State.harvested;
                int rounds=game.Batch.Rounds;
                var time=typeof(FarmBatchActions).GetField("started",BindingFlags.Instance|BindingFlags.NonPublic);
                for(int i=0;i<rounds;i++)
                {
                    time.SetValue(game.Batch,Time.unscaledTime-game.Batch.RoundSeconds*.72f);
                    game.Batch.Strike(game.Batch.Target);game.Batch.Strike(game.Batch.Target);
                    time.SetValue(game.Batch,Time.unscaledTime-game.Batch.RoundSeconds*1.1f);Call(game.Batch,"Update");
                }
                Check(!game.IsHarvesting&&game.Model.State.harvested==harvested+6,"Challenge failed to harvest exactly once");
                Call(game.Batch,"Complete");Check(game.Model.State.harvested==harvested+6,"Duplicate completion awarded again");
                game.Batch.SetMode(1);game.Model.State.coins=CropCatalog.Find("carrot").seedCost;
                game.Batch.Begin();game.Batch.Add(0);game.Batch.Add(1);game.Batch.Release();
                Check(game.Model.State.coins==0&&game.Model.State.plots[0].cropId=="carrot"&&string.IsNullOrEmpty(game.Model.State.plots[1].cropId),"Insufficient funds charged incorrectly");
                now+=300;yield return null;
                game.Batch.SetMode(2);game.Batch.Begin();game.Batch.Add(0);game.Batch.Release();
                Check(game.Batch.Active,"Second challenge did not start");
                game.Batch.Strike((game.Batch.Target+1)%3);game.Batch.Strike(game.Batch.Target);
                Check((int)typeof(FarmBatchActions).GetField("mistakes",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game.Batch)==1,"Wrong/repeated input was not counted once");
                int missedRounds=game.Batch.Rounds;
                for(int i=0;i<missedRounds;i++){time.SetValue(game.Batch,Time.unscaledTime-game.Batch.RoundSeconds*1.1f);Call(game.Batch,"Update");}
                Check(!game.Batch.Active&&game.Model.State.harvested==harvested+7,"Timeout did not finish with low-score harvest");
                foreach(string path in new[]{"Characters/Farmer","Characters/FarmerFemale","Collection/ElderTu","Collection/MerchantHaiAu","Collection/FarmerFemale"})
                {
                    var npc=Object.Instantiate(Resources.Load<GameObject>(path));var avatar=npc.GetComponent<FarmerAvatar>();
                    Check(avatar!=null,"Missing avatar "+path);
                    Transform knee=null;foreach(var t in npc.GetComponentsInChildren<Transform>())if(t.name=="LeftShin"||t.name=="Knee")knee=t;
                    Check(knee!=null,"Missing articulated knee "+path);avatar.SetPose(0,1,0,FarmTool.Plant);
                    var pose=knee.localRotation;avatar.SetPose(.35f,1,0,FarmTool.Plant);
                    Check(Quaternion.Angle(pose,knee.localRotation)>5,"Knee does not animate");Object.Destroy(npc);
                }
                Check(JsonUtility.ToJson(live.Model.State)==before,"Live farm changed");success=true;
            }
            finally
            {
                game.gameObject.SetActive(false);Object.Destroy(game.gameObject);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;live.gameObject.SetActive(true);
                File.WriteAllText(".tools/model-work/batch-result.json",success?"{\"status\":\"complete\",\"duplicateAndCancel\":true,\"dragAndPinch\":true,\"seedCosts\":true,\"rhythmHarvest\":true,\"fiveArticulatedHumans\":true,\"liveFarmUnchanged\":true}":"{\"status\":\"failed\"}");
            }
        }
    }
}
