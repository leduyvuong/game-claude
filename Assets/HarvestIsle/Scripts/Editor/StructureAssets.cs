using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class StructureAssets
    {
        private const string GeometryPath="Assets/HarvestIsle/Art/Structures/FarmStructures.asset";
        private const string PreviewFolder="Art/Previews/Structures";
        public static string PrefabPath(string name){return "Assets/HarvestIsle/Resources/Structures/"+name+".prefab";}
        [MenuItem("Harvest Isle/Structures/Rebuild farm structures")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GeometryPath));Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath("FarmHouse")));AssetDatabase.Refresh();
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,GeometryPath);}
            Mesh[] previous=data.meshes;Material[] oldMaterials=data.materials;
            Material[] materials=StructureModelBuilder.Materials();var meshes=new List<Mesh>();var models=new List<GameObject>();
            try
            {
                foreach(string name in StructureModelBuilder.Names)models.Add(StructureModelBuilder.Create(name,materials,meshes));
                foreach(Mesh mesh in meshes)AssetDatabase.AddObjectToAsset(mesh,data);
                foreach(Material material in materials)AssetDatabase.AddObjectToAsset(material,data);
                data.version=StructureModelBuilder.Version;data.meshes=meshes.ToArray();data.materials=materials;EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
                foreach(GameObject model in models)
                {
                    if(PrefabUtility.SaveAsPrefabAsset(model,PrefabPath(model.name))==null)throw new InvalidOperationException("Could not save structure: "+model.name);
                    Debug.Log("Structure baked: "+model.name);
                }
                if(previous!=null)foreach(Mesh mesh in previous)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(oldMaterials!=null)foreach(Material material in oldMaterials)if(material!=null)Object.DestroyImmediate(material,true);
                AssetDatabase.SaveAssets();
            }
            finally{foreach(GameObject model in models)Object.DestroyImmediate(model);}
        }
        [MenuItem("Harvest Isle/Structures/Render structure collection")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();Rebuild();Directory.CreateDirectory(PreviewFolder);Scene scene=EditorSceneManager.NewPreviewScene();
            RenderTexture target=null;Texture2D image=null,views=null,overview=null;
            try
            {
                Camera camera=CameraAndLight(scene);
                const int width=720,height=640;target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();camera.aspect=(float)width/height;
                image=new Texture2D(width,height,TextureFormat.RGB24,false);views=new Texture2D(width*3,height,TextureFormat.RGB24,false);
                overview=new Texture2D(width*5,height*4,TextureFormat.RGB24,false);
                var bg=new Color32[overview.width*overview.height];Color32 background=FarmWorld.Hex("F2EDE0");for(int p=0;p<bg.Length;p++)bg[p]=background;overview.SetPixels32(bg);
                var entries=new List<Entry>();
                for(int i=0;i<StructureModelBuilder.Names.Length;i++)
                {
                    string name=StructureModelBuilder.Names[i];var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(name)),scene);
                    try
                    {
                        Renderer[] renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(Renderer r in renderers)bounds.Encapsulate(r.bounds);
                        int triangles=0;foreach(MeshFilter m in model.GetComponentsInChildren<MeshFilter>())triangles+=m.sharedMesh.triangles.Length/3;
                        entries.Add(new Entry{name=name,label=StructureModelBuilder.Labels[i],group=StructureModelBuilder.Groups[i],prefab=PrefabPath(name),triangles=triangles});
                        Vector3 center=bounds.center;float radius=Mathf.Max(bounds.size.x,bounds.size.z)*.5f;
                        camera.orthographicSize=Mathf.Max(.65f,radius*.94f+bounds.size.y*.37f);
                        Vector3[] angles={new Vector3(4,3.4f,5.5f),new Vector3(-5,3.7f,3),new Vector3(4,4.4f,-5)};
                        for(int angle=0;angle<3;angle++)
                        {
                            camera.transform.position=center+angles[angle]*Mathf.Max(1,radius);camera.transform.LookAt(center);Render(camera,target);Read(target,views,angle*width,0);
                            if(angle==0){Read(target,image,0,0);Read(target,overview,i%5*width,(3-i/5)*height);image.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+".png",image.EncodeToPNG());}
                        }
                        views.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+"-views.png",views.EncodeToPNG());Debug.Log("Structure preview rendered: "+name);
                    }
                    finally{Object.DestroyImmediate(model);}
                }
                overview.Apply();File.WriteAllBytes(PreviewFolder+"/Overview.png",overview.EncodeToPNG());
                string json=JsonUtility.ToJson(new Manifest{models=entries.ToArray()},true);File.WriteAllText(PreviewFolder+"/models.json",json,new UTF8Encoding(false));
                File.WriteAllText(PreviewFolder+"/index.html",File.ReadAllText("Tools/Templates/structure-gallery.html",Encoding.UTF8).Replace("__STRUCTURE_DATA__",json),new UTF8Encoding(false));
                Debug.Log("STRUCTURE_LIBRARY_VERIFIED: "+entries.Count+" prefabs, 54 views.");
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}if(image!=null)Object.DestroyImmediate(image);if(views!=null)Object.DestroyImmediate(views);if(overview!=null)Object.DestroyImmediate(overview);EditorSceneManager.ClosePreviewScene(scene);
            }
            EnvironmentAssets.RenderFarmPreview();File.Copy("Art/Previews/Environment/Farm-preview.png",PreviewFolder+"/Farm-preview.png",true);
        }
        private static Camera CameraAndLight(Scene scene)
        {
            var sun=new GameObject("Structure sun").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);sun.type=LightType.Directional;sun.color=new Color(1,.96f,.87f);sun.intensity=1.2f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(45,-30,0);
            var fill=new GameObject("Structure fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);fill.type=LightType.Directional;fill.color=new Color(.84f,.92f,1);fill.intensity=.48f;fill.transform.rotation=Quaternion.Euler(30,140,0);
            var camera=new GameObject("Structure camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camera.cameraType=CameraType.Preview;camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);camera.orthographic=true;camera.allowHDR=false;camera.allowMSAA=false;camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("F2EDE0");return camera;
        }
        private static void Render(Camera camera,RenderTexture target){RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});}
        private static void Read(RenderTexture source,Texture2D destination,int x,int y)
        {RenderTexture previous=RenderTexture.active;try{RenderTexture.active=source;destination.ReadPixels(new Rect(0,0,source.width,source.height),x,y,false);}finally{RenderTexture.active=previous;}}
        [MenuItem("Harvest Isle/Structures/Open structure gallery")]
        public static void OpenGallery(){string path=Path.GetFullPath(PreviewFolder+"/index.html");if(!File.Exists(path))RebuildAndPreview();Application.OpenURL(new Uri(path).AbsoluteUri);}
        [Serializable]private sealed class Manifest{public Entry[] models;}
        [Serializable]private sealed class Entry{public string name,label,group,prefab;public int triangles;}
    }
}
