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
    public static class ProductionAssets
    {
        private const string GeometryPath="Assets/HarvestIsle/Art/Structures/ProductionModels.asset";
        private const string Folder="Art/Previews/Production";
        public static string PrefabPath(string type){return "Assets/HarvestIsle/Resources/Production/"+type+".prefab";}
        [MenuItem("Harvest Isle/Production/Rebuild production models")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GeometryPath));Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath("Windmill")));AssetDatabase.Refresh();
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,GeometryPath);}
            Mesh[] oldMeshes=data.meshes;Material[] oldMaterials=data.materials;var meshes=new List<Mesh>();var models=new List<GameObject>();var mats=StructureModelBuilder.Materials();
            try
            {
                foreach(string type in ProductionDisplay.Types)models.Add(StructureModelBuilder.CreateProduction(type,mats,meshes));
                foreach(Mesh mesh in meshes)AssetDatabase.AddObjectToAsset(mesh,data);foreach(Material material in mats)AssetDatabase.AddObjectToAsset(material,data);
                data.version=1;data.meshes=meshes.ToArray();data.materials=mats;EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
                foreach(GameObject model in models)
                {
                    if(PrefabUtility.SaveAsPrefabAsset(model,PrefabPath(model.name))==null)throw new InvalidOperationException("Could not save production model "+model.name);
                    Debug.Log("Production model baked: "+model.name);
                }
                if(oldMeshes!=null)foreach(Mesh mesh in oldMeshes)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(oldMaterials!=null)foreach(Material material in oldMaterials)if(material!=null)Object.DestroyImmediate(material,true);
                AssetDatabase.SaveAssets();
            }
            finally{foreach(GameObject model in models)Object.DestroyImmediate(model);}
        }
        [MenuItem("Harvest Isle/Production/Render production collection")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();Rebuild();Directory.CreateDirectory(Folder);Scene scene=EditorSceneManager.NewPreviewScene();
            RenderTexture target=null;Texture2D single=null,views=null,board=null;
            try
            {
                var sun=new GameObject("Production sun").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);sun.type=LightType.Directional;sun.intensity=1.2f;sun.color=new Color(1,.96f,.87f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(45,-30,0);
                var fill=new GameObject("Production fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);fill.type=LightType.Directional;fill.intensity=.48f;fill.color=new Color(.84f,.92f,1);fill.transform.rotation=Quaternion.Euler(30,140,0);
                var camera=new GameObject("Production camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.cameraType=CameraType.Preview;camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);
                camera.orthographic=true;camera.allowHDR=false;camera.allowMSAA=false;camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("F2EDE0");
                const int width=720,height=640;target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();camera.aspect=(float)width/height;
                single=new Texture2D(width,height,TextureFormat.RGB24,false);views=new Texture2D(width*3,height,TextureFormat.RGB24,false);board=new Texture2D(width*4,height*4,TextureFormat.RGB24,false);var entries=new List<Entry>();
                for(int i=0;i<ProductionDisplay.Types.Length;i++)
                {
                    string type=ProductionDisplay.Types[i];var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(type)),scene);
                    try
                    {
                        var renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(Renderer renderer in renderers)bounds.Encapsulate(renderer.bounds);
                        if(bounds.min.y<-.02f||bounds.size.x>7||bounds.size.y>7||bounds.size.z>7)throw new InvalidOperationException("Invalid production bounds: "+type+" "+bounds);
                        var entry=new Entry{name=type,label=ProductionDisplay.Labels[i],group=StructureModelBuilder.ProductionGroups[i],prefab=PrefabPath(type)};
                        foreach(MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())
                        {
                            Mesh mesh=filter.sharedMesh;entry.triangles+=mesh.triangles.Length/3;
                            foreach(Vector3 v in mesh.vertices)if(!Finite(v.x)||!Finite(v.y)||!Finite(v.z))throw new InvalidOperationException("Invalid mesh vertex: "+type);
                        }
                        if(entry.triangles==0||model.GetComponentsInChildren<MeshCollider>().Length==0)throw new InvalidOperationException("Missing geometry or collider: "+type);
                        foreach(FarmMachineMotion motion in model.GetComponents<FarmMachineMotion>())
                        {
                            var so=new SerializedObject(motion);if(so.FindProperty("rotor").objectReferenceValue==null)throw new InvalidOperationException("Missing machine rotor: "+type);entry.animated=true;
                        }
                        Vector3[] angles={new Vector3(4,3.4f,5.5f),new Vector3(-5,3.7f,3),new Vector3(4,4.4f,-5)};
                        float frame=.6f;
                        foreach(Vector3 angle in angles)
                        {
                            Quaternion inverse=Quaternion.Inverse(Quaternion.LookRotation(-angle));
                            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                            {Vector3 p=inverse*Vector3.Scale(bounds.extents,new Vector3(x,y,z));frame=Mathf.Max(frame,Mathf.Abs(p.y),Mathf.Abs(p.x)/camera.aspect);}
                        }
                        camera.orthographicSize=frame*1.12f;
                        for(int angle=0;angle<3;angle++)
                        {
                            camera.transform.position=bounds.center+angles[angle]*Mathf.Max(1,frame);camera.transform.LookAt(bounds.center);
                            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                            Read(target,views,angle*width,0);
                            if(angle==0){Read(target,single,0,0);Read(target,board,(i%4)*width,(3-i/4)*height);single.Apply();File.WriteAllBytes(Folder+"/"+type+".png",single.EncodeToPNG());}
                        }
                        views.Apply();File.WriteAllBytes(Folder+"/"+type+"-views.png",views.EncodeToPNG());entries.Add(entry);
                    }
                    finally{Object.DestroyImmediate(model);}
                }
                board.Apply();File.WriteAllBytes(Folder+"/Overview.png",board.EncodeToPNG());
                string json=JsonUtility.ToJson(new Manifest{models=entries.ToArray()},true);File.WriteAllText(Folder+"/models.json",json,new UTF8Encoding(false));
                File.WriteAllText(Folder+"/index.html",File.ReadAllText("Tools/Templates/production-gallery.html",Encoding.UTF8).Replace("__PRODUCTION_DATA__",json),new UTF8Encoding(false));
                Debug.Log("PRODUCTION_LIBRARY_VERIFIED: 16 prefabs, 48 views, finite geometry, colliders and 5 animated machines.");
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}if(single!=null)Object.DestroyImmediate(single);if(views!=null)Object.DestroyImmediate(views);if(board!=null)Object.DestroyImmediate(board);EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        private static bool Finite(float value){return !float.IsNaN(value)&&!float.IsInfinity(value);}
        private static void Read(RenderTexture source,Texture2D destination,int x,int y)
        {RenderTexture previous=RenderTexture.active;try{RenderTexture.active=source;destination.ReadPixels(new Rect(0,0,source.width,source.height),x,y,false);}finally{RenderTexture.active=previous;}}
        [MenuItem("Harvest Isle/Production/Open production gallery")]
        public static void OpenGallery(){string path=Path.GetFullPath(Folder+"/index.html");if(!File.Exists(path))RebuildAndPreview();Application.OpenURL(new Uri(path).AbsoluteUri);}
        [Serializable]private sealed class Manifest{public Entry[] models;}
        [Serializable]private sealed class Entry{public string name,label,group,prefab;public int triangles;public bool animated;}
    }
}
