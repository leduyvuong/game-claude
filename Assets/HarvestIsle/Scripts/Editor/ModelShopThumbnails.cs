using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HarvestIsle.Editor
{
    public static class ModelShopThumbnails
    {
        public static void Review()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<FarmGame>();
            if(game==null||game.Hotpot.IsOpen)throw new InvalidOperationException("Open the farm before library review.");
            game.StartCoroutine(ReviewCards(game));
        }
        private static IEnumerator ReviewCards(FarmGame game)
        {
            string before=JsonUtility.ToJson(game.Model.State);bool hud=game.Hud.gameObject.activeSelf,controller=game.CameraController.enabled,simulation=game.enabled;
            var camera=game.World.GameCamera;var rect=camera.rect;int mask=camera.cullingMask;
            var root=new GameObject("Temporary model card review");var viewer=root.AddComponent<ModelLibraryViewer>();
            bool complete=false;
            File.WriteAllText(".tools/model-work/library-cards-result.json","{\"status\":\"running\"}");
            try
            {
                game.Hud.gameObject.SetActive(false);game.CameraController.enabled=false;game.enabled=false;
                foreach(var entry in viewer.Catalog.models)
                    if(Resources.Load<Texture2D>("ModelThumbnails/"+entry.id)==null)throw new InvalidOperationException("Missing card: "+entry.id);
                if(!viewer.Show()||!viewer.Select(0,4))throw new InvalidOperationException("Library cannot preview mature crop");
                yield return null;yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Library/Unity-library-cards.png"));
                yield return new WaitForSecondsRealtime(.4f);
                int kitchen=Array.FindIndex(viewer.Catalog.models,e=>e.variants.Length==8);
                if(kitchen<0||!viewer.Select(kitchen,7))throw new InvalidOperationException("Kitchen level preview failed");
                yield return null;
                viewer.Hide();
                if(camera.rect!=rect||camera.cullingMask!=mask)throw new InvalidOperationException("Library camera not restored");
                if(JsonUtility.ToJson(game.Model.State)!=before)throw new InvalidOperationException("Library modified the farm");
                complete=true;
            }
            finally
            {
                viewer.Hide();UnityEngine.Object.Destroy(root);game.Hud.gameObject.SetActive(hud);game.CameraController.enabled=controller;game.enabled=simulation;
                File.WriteAllText(".tools/model-work/library-cards-result.json",complete?"{\"status\":\"complete\",\"cards\":"+viewer.ModelCount+",\"cameraRestored\":true,\"farmUntouched\":true}":"{\"status\":\"failed\"}");
            }
        }
        // Small build-ready images. Source gallery renders stay outside Resources.
        [MenuItem("Harvest Isle/Library/Build model card thumbnails")]
        public static void Build()
        {
            var catalog=JsonUtility.FromJson<ModelLibraryCatalog>(File.ReadAllText("Assets/HarvestIsle/Resources/FullModelCatalog.json"));
            const string folder="Assets/HarvestIsle/Resources/ModelThumbnails";
            Directory.CreateDirectory(folder);
            var target=RenderTexture.GetTemporary(192,192,0);var previous=RenderTexture.active;
            var source=new Texture2D(2,2);var thumb=new Texture2D(192,192,TextureFormat.RGB24,false);
            try
            {
                foreach(var entry in catalog.models)
                {
                    // A mature plant communicates the species more clearly than a sprout.
                    int index=entry.group=="Cây trồng"?Mathf.Min(4,entry.variants.Length-1):0;
                    string path="Art/Previews/Library/"+entry.variants[index].preview;
                    if(!File.Exists(path))throw new InvalidOperationException("Missing thumbnail source: "+path);
                    source.LoadImage(File.ReadAllBytes(path));Graphics.Blit(source,target);RenderTexture.active=target;
                    thumb.ReadPixels(new Rect(0,0,192,192),0,0);thumb.Apply();
                    File.WriteAllBytes(folder+"/"+entry.id+".png",thumb.EncodeToPNG());
                }
            }
            finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(thumb);}
            AssetDatabase.Refresh();
            foreach(var entry in catalog.models)
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(folder+"/"+entry.id+".png");
                importer.maxTextureSize=256;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Compressed;
                importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            }
            File.WriteAllText(".tools/model-work/model-thumbnails-result.json","{\"status\":\"complete\",\"thumbnails\":"+catalog.models.Length+"}");
        }
    }
}
