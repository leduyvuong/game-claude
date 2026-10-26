using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class HotpotKitchenReview
    {
        internal static void Verify()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();if(live==null)throw new InvalidOperationException("Farm must be playing");
            var isolated=new GameObject("Isolated kitchen component review").AddComponent<FarmGame>();isolated.StartCoroutine(Run(live,isolated));
        }
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        static IEnumerator Run(FarmGame live,FarmGame game)
        {
            string before=JsonUtility.ToJson(live.Model.State);var sun=RenderSettings.sun;Color ambient=RenderSettings.ambientLight;bool wasOpen=live.Hotpot.IsOpen,complete=false;
            File.WriteAllText(".tools/model-work/hotpot-kitchen-result.json","{\"status\":\"running\"}");
            if(wasOpen)live.Hotpot.Close();ambient=RenderSettings.ambientLight;live.gameObject.SetActive(false);
            try
            {
                double now=FarmGame.UtcNow();game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/kitchen-"+DateTime.UtcNow.Ticks)),()=>now);
                game.enabled=false; // Freeze ecology while checking that visual APIs never mutate the model.
                for(int i=0;i<56;i++)
                {
                    var spec=HotpotKitchenCatalog.Items[i];var prefab=Resources.Load<GameObject>(spec.Resource);Require(prefab!=null,"Missing resource "+spec.id);
                    var item=Object.Instantiate(prefab,game.transform,false);item.SetActive(false);
                    var view=item.GetComponent<HotpotKitchenItem>();Require(view.catalogId==spec.id&&view.role==spec.role,"Wrong component");
                    if(view.contents!=null){view.ShowContents(false);Require(!view.contents.gameObject.activeSelf,"Hide contents");view.ShowContents(true);Require(view.contents.gameObject.activeSelf,"Show contents");}
                    Require(Resources.Load<Texture2D>("HotpotKitchenThumbnails/"+spec.id)!=null,"Missing thumbnail "+spec.id);Object.Destroy(item);
                }
                game.Hotpot.Open();var kitchen=game.Hotpot.GetComponentInChildren<HotpotKitchenLevelView>(true);Require(kitchen!=null,"Kitchen integrated into room");
                Require(!kitchen.SetLevel(0)&&!kitchen.SetLevel(9)&&kitchen.Level==1,"Invalid level must not replace visual");
                string economy=JsonUtility.ToJson(game.Model.State);
                for(int level=1;level<=8;level++)
                {
                    Require(kitchen.SetLevel(level)&&kitchen.Item.level==level,"Level switch "+level);var current=kitchen.Item;
                    Require(kitchen.SetLevel(level)&&kitchen.Item==current,"Repeated level rebuilt unnecessarily");
                    Require(current.sockets.Length==(level<3?1:level==3?2:level==8?6:4),"Burner sockets "+level);
                    yield return null;
                    foreach(var r in current.GetComponentsInChildren<MeshRenderer>())Require(r.enabled,"Static room batching hid replaceable kitchen");
                    if(level==1||level==8){ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/HotpotKitchen/Room-level-"+level+".png"));yield return new WaitForSecondsRealtime(.4f);}
                }
                var dishNode=new GameObject("Dish display API check");dishNode.transform.SetParent(game.transform,false);dishNode.transform.localPosition=new Vector3(0,0,300);
                var display=dishNode.AddComponent<HotpotDishDisplay>();Require(!display.SetDish(-1)&&!display.SetDish(12),"Invalid dish accepted");
                for(int i=0;i<12;i++){Require(display.SetDish(i)&&display.Dish==i,"Dish switch "+i);yield return null;Require(dishNode.transform.childCount==1,"Old dish leaked");}
                Require(JsonUtility.ToJson(game.Model.State)==economy,"Visual component changed economy");game.Hotpot.Close();complete=true;
            }
            finally
            {
                Object.Destroy(game.gameObject);live.gameObject.SetActive(true);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;if(wasOpen)live.Hotpot.Open();
                Require(JsonUtility.ToJson(live.Model.State)==before,"Live farm changed");
                File.WriteAllText(".tools/model-work/hotpot-kitchen-result.json",complete?"{\"status\":\"complete\",\"resources\":56,\"levelSwitches\":8,\"dishSwitches\":12,\"contentToggle\":true,\"economyUnchanged\":true,\"liveFarmUntouched\":true}":"{\"status\":\"failed\",\"seeEditorLog\":true}");
            }
        }
    }
}
