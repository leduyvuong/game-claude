using System;
using System.Collections;
using System.IO;
using HarvestIsle.Core;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    internal static class DevelopmentUiPreview
    {
        internal static void BuildThumbnails()
        {
            var catalog=JsonUtility.FromJson<ModelLibraryCatalog>(File.ReadAllText("Assets/HarvestIsle/Resources/FullModelCatalog.json"));
            const string folder="Assets/HarvestIsle/Resources/DevelopmentThumbnails";Directory.CreateDirectory(folder);
            foreach(var spec in DevelopmentCatalog.Buildings)
            {
                var entry=Array.Find(catalog.models,e=>e.id==spec.model);
                if(entry==null)throw new InvalidOperationException("No catalog model "+spec.model);
                string file=folder+"/"+spec.model+".png";
                File.Copy("Art/Previews/Library/"+entry.variants[entry.variants.Length-1].preview,file,true);
                AssetDatabase.ImportAsset(file);
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);
                importer.maxTextureSize=256;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            }
        }
        internal static void Run()
        {
            var live=Object.FindFirstObjectByType<FarmGame>();
            if(live==null||live.Model==null)throw new InvalidOperationException("Open a farm in Play Mode.");
            var test=new GameObject("Development UI preview").AddComponent<FarmGame>();
            test.StartCoroutine(Capture(live,test));
        }
        private static IEnumerator Capture(FarmGame live,FarmGame test)
        {
            const string result=".tools/model-work/development-ui-result.json";
            string original=JsonUtility.ToJson(live.Model.State);bool success=false;
            Light sun=RenderSettings.sun;Color ambient=RenderSettings.ambientLight;bool fog=RenderSettings.fog;
            live.gameObject.SetActive(false);
            File.WriteAllText(result,"{\"status\":\"running\"}");
            try
            {
                double now=FarmGame.UtcNow();var fixture=new FarmModel(FarmModelPark.CreateState(now));
                fixture.StartProduction("flour",now);fixture.StartProduction("bread",now);
                fixture.StartNursery("carrot",now);fixture.StartNursery("corn",now);
                fixture.State.development.jobs[0].readyAt=now-1;
                fixture.State.development.seedlings[0].readyAt=now-1;
                for(int kind=0;kind<3;kind++)fixture.HireWorker(kind);
                test.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/development-ui-preview")),()=>now,fixture.State);
                test.enabled=false;
                string baseline=JsonUtility.ToJson(test.Model.State);
                foreach(var spec in DevelopmentCatalog.Buildings)
                    if(Resources.Load<Texture2D>("DevelopmentThumbnails/"+spec.model)==null)throw new InvalidOperationException("Missing thumbnail "+spec.model);
                const string folder="Art/Previews/Farm/Development-tabs/";Directory.CreateDirectory(folder);
                string[] names={"Orders","Building","Production","Journal","Inventory","Seeds","Agronomy","Workers"};
                for(int i=0;i<8;i++)
                {
                    test.Development.Open(i);yield return new WaitForSecondsRealtime(.25f);
                    test.ActOnPlot(0);
                    if(JsonUtility.ToJson(test.Model.State)!=baseline||test.SelectedPlot!=-1)throw new InvalidOperationException("Modal input leak");
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+(i+1)+"-"+names[i]+".png"));
                    yield return new WaitForSecondsRealtime(.25f);
                    int pages=test.Development.ContentPages;
                    for(int page=1;page<pages;page++){test.Development.ChangeContentPage(1);yield return null;}
                    if(test.Development.ContentPage!=pages-1)throw new InvalidOperationException("Unreachable last page "+i);
                    if(pages>1){ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+(i+1)+"-"+names[i]+"-last.png"));yield return new WaitForSecondsRealtime(.25f);}
                    if(i==2||i==4||i==6)
                    {
                        for(int section=1;section<=(i==4?2:1);section++)
                        {
                            test.Development.SelectSection(section);yield return new WaitForSecondsRealtime(.25f);
                            ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+(i+1)+"-"+names[i]+"-section"+section+".png"));
                            yield return new WaitForSecondsRealtime(.25f);
                        }
                    }
                }
                if(JsonUtility.ToJson(test.Model.State)!=baseline)throw new InvalidOperationException("Browsing changed test state");
                if(JsonUtility.ToJson(live.Model.State)!=original)throw new InvalidOperationException("Live farm changed");
                success=true;
            }
            finally
            {
                test.gameObject.SetActive(false);Object.Destroy(test.gameObject);
                RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;RenderSettings.fog=fog;
                live.gameObject.SetActive(true);
                var park=live.GetComponent<FarmModelPark>();if(park!=null)park.ShowFarm();
                live.Development.Open(1);
                File.WriteAllText(result,success?"{\"status\":\"complete\",\"buildingThumbnails\":9,\"tabs\":8,\"worldInputBlocked\":true,\"liveFarmUnchanged\":true}":"{\"status\":\"failed\"}");
                if(success)live.StartCoroutine(CaptureLive());
            }
        }
        private static IEnumerator CaptureLive()
        {
            yield return new WaitForSecondsRealtime(.5f);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Development-redesign.png"));
        }
    }
}
