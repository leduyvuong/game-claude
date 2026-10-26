using System;
using System.IO;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class FarmRanchReview
    {
        internal static void Verify()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();if(live==null)throw new InvalidOperationException("Farm not open");
            var game=new GameObject("Isolated livestock review").AddComponent<FarmGame>();game.StartCoroutine(Run(live,game));
        }
        private static System.Collections.IEnumerator Run(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);Light sun=RenderSettings.sun;
            double now=FarmGame.UtcNow();var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/ranch-"+DateTime.UtcNow.Ticks));
            live.gameObject.SetActive(false);
            try
            {
                game.InitializeSession(store,()=>now);yield return null;
                game.Ranch.Open();game.Ranch.Act("chicken","buy");game.Ranch.Act("cow","buy");
                Check(game.Model.State.coins==15&&game.Ranch.VisibleAnimals==2,"Purchases spawn owned animals");
                game.ActOnPlot(0);Check(string.IsNullOrEmpty(game.Model.State.plots[0].cropId),"Ranch blocks world actions");
                game.Ranch.Act("chicken","feed");game.Ranch.Act("cow","feed");Check(game.Model.State.coins==6,"Feed cost");
                var saved=store.Load(now);Check(saved.chicken.producing&&saved.cow.producing&&saved.chicken.count==1&&saved.cow.count==1,"Livestock save");
                yield return null;
                Directory.CreateDirectory("Art/Previews/Farm");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Ranch-feeding.png"));
                yield return new WaitForSecondsRealtime(.3f);
                now+=101;yield return null;
                Check(game.Model.AnimalRemaining("cow",now)==0,"Offline timer");
                game.Ranch.Act("chicken","collect");game.Ranch.Act("cow","collect");
                Check(game.Model.State.eggs==2&&game.Model.State.milk==2,"Products collected");
                game.Ranch.Act("cow","collect");Check(game.Model.State.milk==2,"Duplicate reward blocked");
                game.Sell();Check(game.Model.State.coins==40&&game.Model.State.eggs==0&&game.Model.State.milk==0,"Sale clears animal products");
                game.Save();var restored=store.Load(now);Check(restored.coins==40&&restored.cow.count==1&&!restored.cow.producing,"Restored herd and economy");
                game.Ranch.Close();game.CameraController.FocusOn(new Vector3(-10.5f,.15f,-1.5f));
                yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Chicken-paddock.png"));yield return new WaitForSecondsRealtime(.3f);
                game.CameraController.FocusOn(new Vector3(9.5f,.15f,-.7f));
                yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Cow-paddock.png"));yield return new WaitForSecondsRealtime(.3f);
                Check(JsonUtility.ToJson(live.Model.State)==before,"Existing farm untouched");
                File.WriteAllText(".tools/model-work/ranch-result.json","{\"status\":\"complete\",\"ownedModels\":2,\"products\":2,\"finalCoins\":40,\"saveReload\":true,\"duplicateBlocked\":true,\"existingFarmUntouched\":true}");
            }
            finally{Object.Destroy(game.gameObject);RenderSettings.sun=sun;live.gameObject.SetActive(true);}
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("Ranch check failed: "+label);}
    }
}
