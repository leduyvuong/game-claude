using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class HumanHairReview
    {
        public static void Before(){Capture("before");}
        public static void After(){Capture("after");}
        private static void Capture(string suffix)
        {
            var scene=EditorSceneManager.NewPreviewScene();
            var ambient=RenderSettings.ambientLight;var mode=RenderSettings.ambientMode;
            var target=new RenderTexture(600,600,24);target.Create();
            var sheet=new Texture2D(2400,1200,TextureFormat.RGB24,false);
            bool asyncCompilation=EditorSettings.asyncShaderCompilation;EditorSettings.asyncShaderCompilation=false;
            try
            {
                RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.5f,.5f,.5f);
                var camera=new GameObject("Hair review camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.cameraType=CameraType.Preview;
                camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);camera.orthographic=true;
                camera.orthographicSize=.235f;camera.aspect=1;camera.nearClipPlane=.1f;camera.farClipPlane=10;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("DDD9D2");
                var light=new GameObject("Hair review key").AddComponent<Light>();SceneManager.MoveGameObjectToScene(light.gameObject,scene);
                light.type=LightType.Directional;light.intensity=1.05f;light.transform.rotation=Quaternion.Euler(30,-35,0);
                var fill=new GameObject("Hair review fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);
                fill.type=LightType.Directional;fill.intensity=.45f;fill.transform.rotation=Quaternion.Euler(25,140,0);
                foreach(bool female in new[]{false,true})
                {
                    var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(female?FarmerFemaleAssets.PrefabPath:FarmerAssets.PrefabPath),scene);
                    root.GetComponent<FarmerAvatar>().SetPose(0,0,0,HarvestIsle.Core.FarmTool.Plant);
                    using(var snapshot=new HumanPoseSnapshot(root))
                    {
                        Vector3 focus=new Vector3(0,1.445f,.05f);
                        var angles=new[]{new Vector3(0,.08f,3),new Vector3(2,.15f,3),new Vector3(3,.08f,0),new Vector3(0,.08f,-3)};
                        for(int row=0;row<2;row++)
                        {
                            foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name=="Non la"||t.name=="Soft hat ties")t.gameObject.SetActive(row==1);
                            for(int column=0;column<4;column++)
                            {
                                camera.transform.position=focus+angles[column];camera.transform.LookAt(focus);
                                // Warm each camera/light/material variant before reading pixels.
                                for(int warm=0;warm<3;warm++)RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                                var previous=RenderTexture.active;RenderTexture.active=target;
                                sheet.ReadPixels(new Rect(0,0,600,600),column*600,(1-row)*600,false);RenderTexture.active=previous;
                            }
                        }
                        sheet.Apply();Directory.CreateDirectory("Art/Previews/HumanHair");
                        File.WriteAllBytes("Art/Previews/HumanHair/"+(female?"Female":"Male")+"-"+suffix+".png",sheet.EncodeToPNG());
                    }
                    Object.DestroyImmediate(root);
                }
                File.WriteAllText(".tools/model-work/hair-"+suffix+".json","{\"status\":\"complete\",\"views\":16}");
            }
            finally
            {
                EditorSettings.asyncShaderCompilation=asyncCompilation;RenderSettings.ambientLight=ambient;RenderSettings.ambientMode=mode;
                target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(sheet);EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
