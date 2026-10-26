using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static partial class AnimalAssets
    {
        internal static void RenderLivestockLineup()
        {
            Scene scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D image=null;
            try
            {
                Camera camera=PreviewCamera(scene);
                string[] names={"Buffalo","Cow","Pig"};
                for(int i=0;i<names.Length;i++)
                {
                    var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(names[i])),scene);
                    model.transform.position=new Vector3((i-1)*1.65f,0,0);model.transform.rotation=Quaternion.Euler(0,28,0);
                    model.GetComponent<FarmAnimalAvatar>().PreviewPose(FarmAnimalPose.Idle,0);
                }
                camera.aspect=2;camera.orthographicSize=1.5f;camera.transform.position=new Vector3(0,2.2f,7);camera.transform.LookAt(new Vector3(0,.48f,0));
                target=new RenderTexture(1600,800,24,RenderTextureFormat.ARGB32);target.Create();image=new Texture2D(1600,800,TextureFormat.RGB24,false);
                Render(camera,target);Read(target,image,0,0);image.Apply();File.WriteAllBytes(PreviewFolder+"/Livestock-redesign.png",image.EncodeToPNG());
            }
            finally{if(target!=null){target.Release();Object.DestroyImmediate(target);}if(image!=null)Object.DestroyImmediate(image);EditorSceneManager.ClosePreviewScene(scene);}
        }
        internal static void VerifyLivestockLive()
        {
            int checkedCount=0;
            foreach(var actor in Object.FindObjectsByType<FarmAnimalAvatar>(FindObjectsSortMode.None))
            {
                if(actor.Kind!=FarmAnimalKind.Cow&&actor.Kind!=FarmAnimalKind.Pig&&actor.Kind!=FarmAnimalKind.Buffalo)continue;
                for(int pose=0;pose<4;pose++)for(int step=0;step<16;step++)
                {actor.PreviewPose((FarmAnimalPose)pose,step*.13f);ValidateRig(actor.gameObject,actor);checkedCount++;}
                actor.PreviewPose(FarmAnimalPose.Idle,0);
            }
            if(checkedCount!=192)throw new InvalidOperationException("Expected three livestock actors in Play");
            var viewer=Object.FindFirstObjectByType<ModelLibraryViewer>();if(viewer!=null)viewer.Hide();
            var weather=Object.FindFirstObjectByType<PlaygroundWeather>();if(weather!=null)weather.SetWeather(FarmWeather.Clear);
            File.WriteAllText(".tools/model-work/livestock-result.json","{\"status\":\"complete\",\"actors\":3,\"poseSamples\":192}");
            Debug.Log("LIVESTOCK_LIVE_VERIFIED: 3 redesigned actors, 192 finite and grounded pose samples.");
        }
    }
}
