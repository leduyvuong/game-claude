using System;
using System.Collections;
using System.IO;
using System.Linq;
using HarvestIsle.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class FarmModelParkAssets
    {
        public const string ScenePath="Assets/HarvestIsle/Scenes/FarmModelPark.unity";
        [MenuItem("Harvest Isle/Showcase/Open all-model farm park")]
        public static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Open the park in Edit Mode.");
            if(!File.Exists(ScenePath))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
                var root=new GameObject("Mùa Vàng · Công viên model");SceneManager.MoveGameObjectToScene(root,scene);
                root.AddComponent<FarmModelPark>();EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.CloseScene(scene,true);
            }
            EditorSceneManager.OpenScene(ScenePath);
            File.WriteAllText(".tools/model-work/model-park-result.json","{\"status\":\"scene-ready\"}");
        }
        public static void Review()
        {
            var park=Object.FindFirstObjectByType<FarmModelPark>();
            if(park==null)throw new InvalidOperationException("The all-model farm park must be playing.");
            park.StartCoroutine(Check(park));
        }
        private static void Require(bool condition,string reason){if(!condition)throw new InvalidOperationException(reason);}
        private static string MainSave(string suffix)
        {string p=Path.Combine(Application.persistentDataPath,"harvest-isle-v1.json"+suffix);return File.Exists(p)?File.ReadAllText(p):null;}
        private static IEnumerator Check(FarmModelPark park)
        {
            const string result=".tools/model-work/model-park-result.json";
            File.WriteAllText(result,"{\"status\":\"checking\"}");
            string original=MainSave(""),backup=MainSave(".bak");bool complete=false;
            try
            {
                float timeout=Time.realtimeSinceStartup+180;
                while(!park.Ready&&park.Error==null&&Time.realtimeSinceStartup<timeout)yield return null;
                Require(park.Ready,park.Error??"Park load timed out");
                Require(park.Exhibits.Count==park.ExpectedCount&&park.ExpectedCount==600,"All 600 variants must be loaded simultaneously");
                Require(park.Exhibits.Select(e=>e.resource).Distinct().Count()==600,"Duplicated or missing catalog variants");
                foreach(var exhibit in park.Exhibits)
                {
                    Require(exhibit.root.activeInHierarchy&&exhibit.model.activeInHierarchy,"Hidden exhibit: "+exhibit.resource);
                    var renderers=exhibit.model.GetComponentsInChildren<Renderer>();Require(renderers.Length>0,"No visible mesh");
                    foreach(var renderer in renderers)foreach(var material in renderer.sharedMaterials)
                        Require(material!=null&&material.shader!=null&&material.shader.name!="Hidden/InternalErrorShader","Invalid material: "+exhibit.resource);
                    Require(Physics.Raycast(new Ray(exhibit.center+Vector3.up*20,Vector3.down),out var hit,40,1<<FarmModelPark.ExhibitLayer)
                        &&hit.collider.GetComponent<ModelParkTarget>().Index==park.Exhibits.IndexOf(exhibit),"Exhibit picking: "+exhibit.resource);
                }
                Require(park.Districts.Count==25,"25 themed gardens");
                foreach(var type in park.Exhibits.GroupBy(e=>e.type))Require(type.All(e=>Mathf.Approximately(e.displayScale,type.First().displayScale)),"Variant scales: "+type.Key);
                var game=park.Game;string reason;
                Require(FarmModel.Validate(game.Model.State,out reason),reason);
                Require(game.Model.Level==10&&game.Model.State.plots.Count==36&&game.Development.VisibleBuildings==9,"Fully developed playable farm");
                Require(game.Ranch.VisibleAnimals==18&&game.Model.State.hotpot.floors==4,"Full ranch and restaurant");
                park.Refill();Require(game.Model.State.coins==FarmModelPark.TestCoins,"Sandbox wallet refill");
                Require(!park.SelectExhibit(-1)&&!park.SelectExhibit(600),"Invalid model indices accepted");
                for(int i=0;i<park.Districts.Count;i++)park.ShowDistrict(i);
                Require(park.SelectExhibit(0)&&park.SelectExhibit(599),"Exhibit navigation");
                Rect safe=Screen.safeArea;float uiScale=Mathf.Min(safe.width/1240f,safe.height/760f);
                float top=Screen.height-safe.yMax+(safe.height-760*uiScale)*.5f;
                Require(park.BlocksPointer(new Vector2(safe.center.x,Screen.height-top-118*uiScale)),"Toolbar input passthrough");
                var snapshot=JsonUtility.ToJson(game.Model.State);
                var store=new FarmSaveStore(Path.GetFullPath(".tools/model-work/park-save-check"));
                Require(store.Save(game.Model.State),store.LastError);
                // Match FarmGame's actual load path: Core normalizes Unity's inline
                // empty-object representation of a null Journey contract.
                var loaded=new FarmModel(store.Load(game.Now));
                Require(JsonUtility.ToJson(loaded.State)==snapshot,"Sandbox JSON roundtrip");
                Directory.CreateDirectory("Art/Previews/ModelPark");
                park.ShowOverview();yield return new WaitForSecondsRealtime(.7f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/ModelPark/Overview.png"));
                yield return new WaitForSecondsRealtime(.5f);
                park.ShowDistrict(0);yield return new WaitForSecondsRealtime(.5f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/ModelPark/Crop-garden.png"));
                yield return new WaitForSecondsRealtime(.5f);
                park.ShowDistrict(park.Districts.FindIndex(d=>d.name=="Công trình"));yield return new WaitForSecondsRealtime(.5f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/ModelPark/Buildings.png"));
                yield return new WaitForSecondsRealtime(.5f);
                park.ShowFarm();yield return new WaitForSecondsRealtime(.5f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/ModelPark/Playable-farm.png"));
                yield return new WaitForSecondsRealtime(.5f);
                park.ShowOverview();
                Require(MainSave("")==original&&MainSave(".bak")==backup,"Normal farm save changed");
                complete=true;
            }
            finally
            {
                File.WriteAllText(result,complete?"{\"status\":\"complete\",\"variants\":600,\"groups\":25,\"allActive\":true,\"plots\":36,\"buildings\":9,\"animals\":18,\"saveRoundtrip\":true,\"normalSaveUntouched\":true}":"{\"status\":\"failed\",\"seeEditorLog\":true}");
            }
        }
    }
}
