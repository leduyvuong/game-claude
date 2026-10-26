using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HarvestIsle.Editor
{
    /// <summary>Local, explicitly queued model work inside an already open Editor.</summary>
    [InitializeOnLoad]
    public static class EditorWorkBridge
    {
        private const string Folder=".tools/model-work";
        private static bool busy;
        static EditorWorkBridge(){EditorApplication.update+=Poll;}
        private static void Poll()
        {
            string request=Folder+"/request.json";
            if(Application.isBatchMode||busy||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(request))return;
            var queued=JsonUtility.FromJson<Work>(File.ReadAllText(request));
            if((queued.command=="island-sample-check"||queued.command=="island-all-check")&&EditorApplication.isPlaying)
            {File.Delete(request);IslandExpansionReview.Run(queued.command=="island-all-check");return;}
            if(queued.command=="island-open"&&EditorApplication.isPlaying)
            {File.Delete(request);UnityEngine.Object.FindFirstObjectByType<FarmGame>().IslandExpansion.Open();return;}
            if(queued.command=="island-catalog"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {if(ShaderUtil.anythingCompiling)return;File.Delete(request);IslandExpansionIntegration.MergeCatalog();return;}
            if(queued.command.StartsWith("island-assets-")&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if(ShaderUtil.anythingCompiling)return;
                File.Delete(request);
                try{
                    if(queued.command=="island-assets-sample")IslandExpansionAssets.BuildSample();
                    else if(queued.command=="island-assets-render-sample")IslandExpansionAssets.RenderSample();
                    else if(queued.command=="island-assets-all")IslandExpansionAssets.BuildAll();
                    else if(queued.command=="island-assets-render-all")IslandExpansionAssets.RenderAll();
                }catch(Exception e){File.WriteAllText(Folder+"/island-assets-error.txt",e.ToString());string mode=queued.command.EndsWith("all")?"all":"sample";string report=queued.command.Contains("render")?"expansion-preview-":"expansion-assets-";File.WriteAllText(Folder+"/"+report+mode+".json","{\"status\":\"failed\",\"detail\":\"See island-assets-error.txt\"}");Debug.LogException(e);}
                return;
            }
            if(queued.command=="workers-open"&&EditorApplication.isPlaying)
            {File.Delete(request);var game=UnityEngine.Object.FindFirstObjectByType<FarmGame>();game.Development.Open(7);game.Development.ShowWorkerSection(1);return;}
            if(queued.command=="recruitment-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmRecruitmentReview.Run();return;}
            if(queued.command=="windows-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmWindowReview.Run();return;}
            if(queued.command=="architecture-play"&&EditorApplication.isPlaying)
            {File.Delete(request);ArchitecturePlayReview.Run();return;}
            if(queued.command=="architecture-build"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ArchitectureRefresh.Build();return;}
            if(queued.command=="estate-open"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);FarmEstateAssets.Open();return;}
            if(queued.command=="estate-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmEstateReview.Run();return;}
            if(queued.command=="currency-build"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);HarvestCoinAssets.Build();return;}
            if(queued.command=="currency-check"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);HarvestCoinReview.Begin();return;}
            if(queued.command=="humans-hair-before"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);HumanHairReview.Before();return;}
            if(queued.command=="humans-hair-after"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);HumanHairReview.After();return;}
            if(queued.command=="humans-preview"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);GameHumanReview.OpenPreview();return;}
            if(queued.command=="humans-check"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);GameHumanReview.Begin();return;}
            if(queued.command=="reference-female-motion"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ReferenceFemaleStudy.BuildMotions();return;}
            if(queued.command=="reference-female-motion-open"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ReferenceFemaleStudy.Open();return;}
            if(queued.command=="reference-female"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ReferenceFemaleStudy.Build();return;}
            if(queued.command=="reference-male-motion"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ReferenceMaleMotionStudy.Build();return;}
            if(queued.command=="reference-male-motion-open"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ReferenceMaleMotionStudy.Open();return;}
            if(queued.command=="reference-male"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ReferenceMaleStudy.Build();return;}
            if(queued.command=="humans-rebuild"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);HumanRefresh.Bake();return;}
            if(queued.command=="batch-check"&&EditorApplication.isPlaying)
            {File.Delete(request);BatchInteractionReview.Run();return;}
            if(queued.command=="refresh")
            {File.Delete(request);AssetDatabase.Refresh();return;}
            if(queued.command=="model-park-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmModelParkAssets.Review();return;}
            if(queued.command=="model-thumbnails"&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {File.Delete(request);ModelShopThumbnails.Build();return;}
            if(queued.command=="library-cards-check"&&EditorApplication.isPlaying)
            {File.Delete(request);ModelShopThumbnails.Review();return;}
            if(queued.command=="stop")
            {
                File.Delete(request);if(EditorApplication.isPlayingOrWillChangePlaymode)EditorApplication.isPlaying=false;
                File.WriteAllText(Folder+"/play-status.json","{\"isPlaying\":false,\"willPlay\":false}");
                Debug.Log("LOCAL_STOP_REQUESTED");return;
            }
            if(queued.command=="play")
            {
                File.Delete(request);if(!EditorApplication.isPlayingOrWillChangePlaymode)EditorApplication.isPlaying=true;
                File.WriteAllText(Folder+"/play-status.json","{\"isPlaying\":"+EditorApplication.isPlaying.ToString().ToLowerInvariant()+",\"willPlay\":"+EditorApplication.isPlayingOrWillChangePlaymode.ToString().ToLowerInvariant()+"}");
                Debug.Log("LOCAL_PLAY_REQUESTED: "+EditorApplication.isPlayingOrWillChangePlaymode);return;
            }
            if(queued.command=="weather-check"&&EditorApplication.isPlaying)
            {File.Delete(request);WeatherAssets.VerifyLive();return;}
            if(queued.command=="farm-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmLoopReview.Verify();return;}
            if(queued.command=="play-interface-check"&&EditorApplication.isPlaying)
            {File.Delete(request);PlayInterfaceReview.Run();return;}
            if(queued.command=="farm-ui-preview"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmLoopReview.PreviewHud();return;}
            if(queued.command=="farm-drainage-preview"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmLoopReview.PreviewDrainage();return;}
            if(queued.command=="farm-density-preview"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmLoopReview.PreviewDensity();return;}
            if(queued.command=="development-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmDevelopmentReview.Verify();return;}
            if(queued.command=="journey-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmJourneyReview.Run();return;}
            if(queued.command=="hotpot-kitchen-check"&&EditorApplication.isPlaying)
            {File.Delete(request);HotpotKitchenReview.Verify();return;}
            if(queued.command=="hotpot-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmHotpotReview.Verify();return;}
            if(queued.command=="hotpot-endless-check"&&EditorApplication.isPlaying)
            {File.Delete(request);HotpotEndlessReview.Verify();return;}
            if(queued.command=="hotpot-open"&&EditorApplication.isPlaying)
            {File.Delete(request);UnityEngine.Object.FindFirstObjectByType<FarmGame>().Hotpot.Open();return;}
            if(queued.command=="online-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmOnlineReview.Verify();return;}
            if(queued.command=="online-open"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmOnlineHub.Open(UnityEngine.Object.FindFirstObjectByType<FarmGame>());return;}
            if(queued.command=="development-open"&&EditorApplication.isPlaying)
            {File.Delete(request);UnityEngine.Object.FindFirstObjectByType<FarmGame>().Development.Open(0);return;}
            if(queued.command=="development-ui-preview"&&EditorApplication.isPlaying)
            {File.Delete(request);DevelopmentUiPreview.Run();return;}
            if(queued.command=="farm-games-preview"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmLoopReview.PreviewGames();return;}
            if(queued.command=="ranch-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmRanchReview.Verify();return;}
            if(queued.command=="town-check"&&EditorApplication.isPlaying)
            {File.Delete(request);FarmTownReview.Verify();return;}
            if(queued.command=="town-open"&&EditorApplication.isPlaying)
            {File.Delete(request);UnityEngine.Object.FindFirstObjectByType<FarmGame>().CameraController.ShowTown();return;}
            if(queued.command=="ranch-open"&&EditorApplication.isPlaying)
            {File.Delete(request);UnityEngine.Object.FindFirstObjectByType<FarmGame>().Ranch.Open();return;}
            if(queued.command=="farm-selection-preview"&&EditorApplication.isPlaying)
            {
                File.Delete(request);var game=UnityEngine.Object.FindFirstObjectByType<FarmGame>();
                game.CameraController.GoHome();game.SelectTool(HarvestIsle.Core.FarmTool.Inspect);game.ActOnPlot(8);
                Directory.CreateDirectory("Art/Previews/Farm");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Selected-plot.png"));return;
            }
            if(queued.command=="crop-garden-check"&&EditorApplication.isPlaying)
            {File.Delete(request);CropGrowthShowcaseAssets.VerifyLive();return;}
            if(queued.command=="livestock-check"&&EditorApplication.isPlaying)
            {File.Delete(request);AnimalAssets.VerifyLivestockLive();return;}
            if(queued.command=="animals-check"&&EditorApplication.isPlaying)
            {File.Delete(request);AnimalAssets.VerifyAllAnimalsLive();return;}
            if(queued.command=="landscape-check"&&EditorApplication.isPlaying)
            {File.Delete(request);PlaygroundSetup.VerifyLandscapeLive();return;}
            if(queued.command=="library-check"&&EditorApplication.isPlaying)
            {
                File.Delete(request);busy=true;var viewer=UnityEngine.Object.FindFirstObjectByType<ModelLibraryViewer>();int index=0,variant=0,checkedCount=0;
                EditorApplication.CallbackFunction tick=null;tick=()=>
                {
                    try
                    {
                        if(viewer==null||!viewer.Show())throw new InvalidOperationException("Library viewer unavailable");
                        if(index<viewer.Catalog.models.Length)
                        {
                            if(!viewer.Select(index,variant)||Camera.main.cullingMask!=(1<<31))throw new InvalidOperationException("Library selection failed: "+index+" / "+variant);
                            checkedCount++;variant++;if(variant>=viewer.Catalog.models[index].variants.Length){index++;variant=0;}return;
                        }
                        viewer.Hide();if(Camera.main.cullingMask==(1<<31))throw new InvalidOperationException("Library failed to restore the farm camera");viewer.Show();
                        int elder=Array.FindIndex(viewer.Catalog.models,m=>m.id=="ElderTu");viewer.Select(elder,0);ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Library/Unity-library.png"));
                        File.WriteAllText(Folder+"/library-result.json","{\"status\":\"complete\",\"types\":"+viewer.ModelCount+",\"variantsChecked\":"+checkedCount+"}");
                        Debug.Log("LIVE_LIBRARY_VERIFIED: "+viewer.ModelCount+" types and "+checkedCount+" variants loaded in Play Mode; farm camera restored.");
                    }
                    catch(Exception error){File.WriteAllText(Folder+"/library-result.json",JsonUtility.ToJson(new Result{status="failed",error=error.ToString()},true));Debug.LogException(error);}
                    EditorApplication.update-=tick;busy=false;
                };EditorApplication.update+=tick;return;
            }
            if(queued.command=="production-view"&&EditorApplication.isPlaying)
            {
                File.Delete(request);
                try
                {
                    var director=UnityEngine.Object.FindFirstObjectByType<PlaygroundDirector>();
                    if(director==null||!director.ShowProduction(true)||Mathf.Abs(Camera.main.transform.position.x-38)>.1f)throw new InvalidOperationException("Production camera switch failed");
                    if(!director.ShowProduction(false)||Mathf.Abs(Camera.main.transform.position.x-15)>.1f)throw new InvalidOperationException("Return camera switch failed");
                    director.ShowProduction(true);ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Production/Showcase.png"));
                    File.WriteAllText(Folder+"/view-result.json","{\"status\":\"complete\",\"cameraSwitches\":3}");
                    Debug.Log("PRODUCTION_VIEW_VERIFIED: camera switched to the display island and back; screenshot requested.");
                }
                catch(Exception error){File.WriteAllText(Folder+"/view-result.json",JsonUtility.ToJson(new Result{status="failed",error=error.ToString()},true));Debug.LogException(error);}
                return;
            }
            if(EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.isPlaying=false;return;}
            busy=true;Directory.CreateDirectory(Folder);File.Move(request,Folder+"/active.json");
            try
            {
                var work=JsonUtility.FromJson<Work>(File.ReadAllText(Folder+"/active.json"));
                if(work.command=="development-thumbnails")
                {DevelopmentUiPreview.BuildThumbnails();return;}
                if(work.command=="model-park")
                {FarmModelParkAssets.Open();return;}
                if(work.command=="hotpot-kitchen-assets")
                {
                    HotpotKitchenAssets.RebuildAndPreview();
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"kitchenComponents\":56}");return;
                }
                if(work.command=="hotpot-assets")
                {
                    HotpotFurnitureAssets.RebuildAndPreview();
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"hotpotFurniture\":52}");return;
                }
                if(work.command=="farm")
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"scene\":\"HarvestIsle\"}");return;
                }
                if(work.command=="puddles")
                {
                    PuddleAssets.Rebuild();
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"puddleVariants\":4}");return;
                }
                if(work.command=="crop-soil")
                {
                    EnvironmentAssets.RebuildAndPreview();
                    CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),false,new[]{"DirtTile","WetDirtTile"});
                    CropGrowthShowcaseAssets.Build();
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"tilledSoil\":2}");return;
                }
                if(work.command=="crop-garden")
                {
                    CropGrowthShowcaseAssets.Build();File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"cropGarden\":150}");return;
                }
                if(work.command=="crops-five")
                {
                    CompleteModelAssets.RebuildReferenceCrops();
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"cropSpecies\":30,\"growthPrefabs\":150,\"healthPrefabs\":60}");return;
                }
                if(work.command=="animals-review")
                {
                    AnimalAssets.RebuildAndPreview();AnimalAssets.BakeAmbient();
                    CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),false,AnimalAssets.AllAnimalNames());
                    AnimalAssets.RenderAnimalReview();
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"animalReview\":22}");PlaygroundSetup.OpenAndPlay();return;
                }
                if(work.command=="livestock")
                {
                    AnimalAssets.RebuildAndPreview();AnimalAssets.RenderLivestockLineup();
                    CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),false,AnimalModelBuilder.Names);
                    File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"livestock\":3}");PlaygroundSetup.OpenAndPlay();return;
                }
                if(work.command=="landscape")
                {
                    PlaygroundSetup.UpdateLandscape();File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"landscape\":true}");PlaygroundSetup.OpenAndPlay();return;
                }
                if(work.command=="weather")
                {
                    WeatherAssets.AddToPlayground();File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"weatherModes\":5}");PlaygroundSetup.OpenAndPlay();return;
                }
                if(work.command=="levels")BuildingLevelAssets.RebuildAndPreview();
                else if(work.command=="structures")StructureAssets.RebuildAndPreview();
                else if(work.command=="production")ProductionAssets.RebuildAndPreview();
                else if(work.command=="complete")CompleteModelAssets.RebuildAndPreview();
                else throw new InvalidOperationException("Unsupported local model command");
                PlaygroundSetup.AddStructures();
                if(work.command=="production")PlaygroundSetup.AddProduction();
                File.WriteAllText(Folder+"/result.json",work.command=="complete"?"{\"status\":\"complete\",\"collection\":\"full\"}":work.command=="levels"?"{\"status\":\"complete\",\"buildings\":9,\"variants\":27}":work.command=="production"?"{\"status\":\"complete\",\"models\":16,\"animated\":5}":"{\"status\":\"complete\",\"models\":18}");
                Debug.Log(work.command=="complete"?"FULL_MODEL_WORK_COMPLETE: complete collection and playground library.":work.command=="levels"?"BUILDING_LEVEL_WORK_COMPLETE: 27 variants and previews.":work.command=="production"?"PRODUCTION_WORK_COMPLETE: 16 prefabs and previews.":"STRUCTURE_WORK_COMPLETE: 18 prefabs and previews.");
                PlaygroundSetup.OpenAndPlay();
            }
            catch(Exception error)
            {
                File.WriteAllText(Folder+"/result.json",JsonUtility.ToJson(new Result{status="failed",error=error.ToString()},true));Debug.LogException(error);
            }
            finally{File.Delete(Folder+"/active.json");busy=false;}
        }
        public static void FinishStructuresBatch()
        {
            StructureAssets.RebuildAndPreview();PlaygroundSetup.AddStructures();Directory.CreateDirectory(Folder);
            if(File.Exists(Folder+"/request.json"))File.Delete(Folder+"/request.json");
            File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"models\":18}");
            Debug.Log("STRUCTURE_WORK_COMPLETE: 18 prefabs and playground updated.");
        }
        public static void FinishLevelsBatch()
        {
            BuildingLevelAssets.RebuildAndPreview();PlaygroundSetup.AddStructures();Directory.CreateDirectory(Folder);
            if(File.Exists(Folder+"/request.json"))File.Delete(Folder+"/request.json");
            File.WriteAllText(Folder+"/result.json","{\"status\":\"complete\",\"buildings\":9,\"variants\":27}");
            Debug.Log("BUILDING_LEVEL_WORK_COMPLETE: 27 variants and playground updated.");
        }
        [Serializable]private sealed class Work{public string command;}
        [Serializable]private sealed class Result{public string status,error;}
    }
}
