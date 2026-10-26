using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarvestIsle.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class FarmWindowReview
    {
        internal static void Run()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            var runner=new GameObject("Window review").AddComponent<FarmWindowReviewRunner>();
            runner.StartCoroutine(Review(live,runner));
        }
        private static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        private static IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.5f);
            double deadline=EditorApplication.timeSinceStartup+40;
            while(ShaderUtil.anythingCompiling)
            {Check(EditorApplication.timeSinceStartup<deadline,"Shader compilation timed out before screenshot");yield return null;}
            yield return new WaitForSecondsRealtime(.3f);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Windows/"+name+".png"));
            yield return new WaitForSecondsRealtime(.5f);
        }
        // Route dismissal through the actual OnGUI handler without requiring OS
        // focus (the editor may be rendering Play in the background).
        private static void Send(MonoBehaviour target,Event e)
        {
            FarmWindowReviewRunner.Pending=()=>
            {
                var previous=Event.current;
                try{Event.current=e;target.GetType().GetMethod("OnGUI",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);}
                finally{Event.current=previous;}
            };
        }
        private static IEnumerator Review(FarmGame live,FarmWindowReviewRunner runner)
        {
            bool passed=false;var sun=RenderSettings.sun;var ambient=RenderSettings.ambientLight;
            string before=JsonUtility.ToJson(live.Model.State);
            Directory.CreateDirectory("Art/Previews/Farm/Windows");
            File.WriteAllText(".tools/model-work/windows-result.json","{\"status\":\"running\"}");
            live.Development.Close();live.Hud.ClosePanels();live.gameObject.SetActive(false);
            var game=new GameObject("Isolated window farm").AddComponent<FarmGame>();
            try
            {
                double now=FarmGame.UtcNow();game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/windows-"+DateTime.UtcNow.Ticks)),()=>now,FarmEstate.CreateState(now),estate:true);
                game.enabled=false;
                foreach(var safe in new[]{new Rect(0,0,1280,720),new Rect(48,24,2304,1032),new Rect(0,0,720,1280)})
                {
                    var frame=FarmWindow.Frame(safe,safe.yMax,1120,740);
                    Check(frame.width<=safe.width*.821f&&frame.height<=safe.height*.821f,"Window exceeds safe area margin");
                }
                game.CameraController.SetView(new Vector3(18,0,6),15);yield return null;
                game.Development.Open(1);yield return Shot("01-Buildings");
                var state=JsonUtility.ToJson(game.Model.State);game.ActOnPlot(0);
                Check(JsonUtility.ToJson(game.Model.State)==state,"Dialog allows world action");
                Send(game.Development,new Event{type=EventType.MouseDown,button=0,mousePosition=new Vector2(5,5)});yield return new WaitForSecondsRealtime(.2f);
                Check(!game.Development.IsOpen,"Outside click did not close development");
                game.Development.Open(2);yield return Shot("02-Workshop");
                Send(game.Development,new Event{type=EventType.KeyDown,keyCode=KeyCode.Escape});yield return new WaitForSecondsRealtime(.2f);
                Check(!game.Development.IsOpen,"Escape did not close development");
                game.Ranch.Open();yield return Shot("03-Ranch");
                Send(game.Ranch,new Event{type=EventType.MouseDown,button=0,mousePosition=new Vector2(5,5)});yield return new WaitForSecondsRealtime(.2f);
                Check(!game.Ranch.IsOpen,"Outside click did not close ranch");
                game.Hud.OpenTrading();yield return Shot("04-Market");
                Check(game.Hud.BlocksWorld,"Market does not block world input");
                var backdrop=game.Hud.transform.Find("Safe area/Window backdrop").GetComponent<Button>();
                backdrop.onClick.Invoke();yield return null;Check(!game.Hud.BlocksWorld,"Market backdrop did not dismiss");
                game.Hud.OpenOrders();yield return Shot("05-Orders");game.Hud.ClosePanels();
                game.Hotpot.Open();yield return Shot("06-Restaurant");
                var camera=(Camera)typeof(FarmHotpot).GetField("interiorCamera",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.Hotpot);
                var tablePoint=camera.WorldToViewportPoint(camera.transform.parent.position+new Vector3(-4.7f,1.2f,3.4f));
                Check(tablePoint.z>0&&tablePoint.x>0&&tablePoint.x<1&&tablePoint.y>0&&tablePoint.y<1,"Dining table is outside the restaurant viewport");
                Check(game.World.GameCamera.enabled&&camera.rect.width<=.83f&&camera.rect.height<=.83f,"Restaurant hides farm");
                typeof(FarmHotpot).GetField("tab",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game.Hotpot,1);
                yield return Shot("07-Restaurant-shop");
                Send(game.Hotpot,new Event{type=EventType.KeyDown,keyCode=KeyCode.Escape});yield return new WaitForSecondsRealtime(.2f);
                Check(!game.Hotpot.IsOpen&&game.World.GameCamera.enabled,"Restaurant escape did not restore farm");
                Check(JsonUtility.ToJson(live.Model.State)==before,"Live farm changed");passed=true;
            }
            finally
            {
                if(game.Hotpot!=null&&game.Hotpot.IsOpen)game.Hotpot.Close();
                game.gameObject.SetActive(false);Object.Destroy(game.gameObject);
                live.gameObject.SetActive(true);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;
                File.WriteAllText(".tools/model-work/windows-result.json",passed?"{\"status\":\"complete\",\"outsideClick\":true,\"escape\":true,\"farmVisible\":true,\"inputBlocked\":true,\"liveFarmUntouched\":true,\"screenshots\":7}":"{\"status\":\"failed\"}");
                Object.Destroy(runner.gameObject);
            }
        }
    }
    internal sealed class FarmWindowReviewRunner:MonoBehaviour
    {
        internal static Action Pending;
        private void OnGUI(){if(Pending==null)return;var action=Pending;Pending=null;action();}
    }
}
