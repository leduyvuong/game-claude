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
    public static class BuildingLevelAssets
    {
        private const string GeometryPath="Assets/HarvestIsle/Art/Structures/BuildingLevels.asset";
        private const string Folder="Art/Previews/BuildingLevels";
        public static string PrefabPath(string type,int level){return "Assets/HarvestIsle/Resources/Structures/Levels/"+type+"_L"+level+".prefab";}
        [MenuItem("Harvest Isle/Structures/Rebuild building levels")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GeometryPath));Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath("FarmHouse",1)));AssetDatabase.Refresh();
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,GeometryPath);}
            Mesh[] oldMeshes=data.meshes;Material[] oldMaterials=data.materials;
            var meshes=new List<Mesh>();var models=new List<GameObject>();var materials=StructureModelBuilder.Materials();
            try
            {
                foreach(string type in BuildingLevelView.Types)for(int level=1;level<=3;level++)models.Add(StructureModelBuilder.CreateLevel(type,level,materials,meshes));
                foreach(Mesh mesh in meshes)AssetDatabase.AddObjectToAsset(mesh,data);foreach(Material material in materials)AssetDatabase.AddObjectToAsset(material,data);
                data.version=1;data.meshes=meshes.ToArray();data.materials=materials;EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
                for(int i=0;i<models.Count;i++)
                {
                    if(PrefabUtility.SaveAsPrefabAsset(models[i],PrefabPath(BuildingLevelView.Types[i/3],i%3+1))==null)throw new InvalidOperationException("Could not save building level "+models[i].name);
                    Debug.Log("Building level baked: "+models[i].name);
                }
                if(oldMeshes!=null)foreach(Mesh mesh in oldMeshes)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(oldMaterials!=null)foreach(Material material in oldMaterials)if(material!=null)Object.DestroyImmediate(material,true);
                AssetDatabase.SaveAssets();
            }
            finally{foreach(GameObject model in models)Object.DestroyImmediate(model);}
        }
        [MenuItem("Harvest Isle/Structures/Render building levels")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();Rebuild();Directory.CreateDirectory(Folder);Scene scene=EditorSceneManager.NewPreviewScene();
            RenderTexture target=null;Texture2D single=null,board=null;
            try
            {
                var sun=new GameObject("Level sun").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);sun.type=LightType.Directional;sun.intensity=1.2f;sun.color=new Color(1,.96f,.87f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(45,-30,0);
                var fill=new GameObject("Level fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);fill.type=LightType.Directional;fill.intensity=.48f;fill.color=new Color(.84f,.92f,1);fill.transform.rotation=Quaternion.Euler(30,140,0);
                var camera=new GameObject("Level camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camera.cameraType=CameraType.Preview;camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);camera.orthographic=true;camera.allowHDR=false;camera.allowMSAA=false;camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("F2EDE0");
                const int width=640,height=720;target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();camera.aspect=(float)width/height;
                single=new Texture2D(width,height,TextureFormat.RGB24,false);board=new Texture2D(width*3,height,TextureFormat.RGB24,false);var entries=new List<Entry>();
                for(int typeIndex=0;typeIndex<BuildingLevelView.Types.Length;typeIndex++)
                {
                    string type=BuildingLevelView.Types[typeIndex];var models=new GameObject[3];Bounds[] bounds=new Bounds[3];
                    try
                    {
                        var entry=new Entry{type=type,label=BuildingLevelView.Labels[typeIndex],prefabs=new string[3],triangles=new int[3]};
                        for(int level=1;level<=3;level++)
                        {
                            string path=PrefabPath(type,level);models[level-1]=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),scene);
                            var rs=models[level-1].GetComponentsInChildren<Renderer>();bounds[level-1]=rs[0].bounds;foreach(Renderer r in rs)bounds[level-1].Encapsulate(r.bounds);
                            entry.prefabs[level-1]=path;foreach(MeshFilter f in models[level-1].GetComponentsInChildren<MeshFilter>())entry.triangles[level-1]+=f.sharedMesh.triangles.Length/3;
                            models[level-1].SetActive(false);
                        }
                        camera.transform.rotation=Quaternion.LookRotation(-new Vector3(4,3.3f,5.5f));
                        float frame=0;
                        foreach(Bounds b in bounds)for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                        {
                            Vector3 corner=Quaternion.Inverse(camera.transform.rotation)*Vector3.Scale(b.extents,new Vector3(x,y,z));
                            frame=Mathf.Max(frame,Mathf.Abs(corner.y),Mathf.Abs(corner.x)/camera.aspect);
                        }
                        camera.orthographicSize=Mathf.Max(.75f,frame*1.12f);
                        for(int level=1;level<=3;level++)
                        {
                            models[level-1].SetActive(true);Vector3 center=bounds[level-1].center;camera.transform.position=center+new Vector3(4,3.3f,5.5f)*Mathf.Max(1,frame);camera.transform.LookAt(center);
                            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture previous=RenderTexture.active;
                            try{RenderTexture.active=target;single.ReadPixels(new Rect(0,0,width,height),0,0,false);board.ReadPixels(new Rect(0,0,width,height),(level-1)*width,0,false);}finally{RenderTexture.active=previous;}
                            single.Apply();File.WriteAllBytes(Folder+"/"+type+"_L"+level+".png",single.EncodeToPNG());models[level-1].SetActive(false);
                        }
                        board.Apply();File.WriteAllBytes(Folder+"/"+type+"-levels.png",board.EncodeToPNG());entries.Add(entry);
                    }
                    finally{foreach(GameObject model in models)if(model!=null)Object.DestroyImmediate(model);}
                }
                string json=JsonUtility.ToJson(new Manifest{buildings=entries.ToArray()},true);File.WriteAllText(Folder+"/models.json",json,new UTF8Encoding(false));
                File.WriteAllText(Folder+"/index.html",File.ReadAllText("Tools/Templates/building-level-gallery.html",Encoding.UTF8).Replace("__LEVEL_DATA__",json),new UTF8Encoding(false));
                VerifySwitching(scene);
                Debug.Log("BUILDING_LEVELS_VERIFIED: 9 buildings, 27 variants and 45 level switches.");
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}if(single!=null)Object.DestroyImmediate(single);if(board!=null)Object.DestroyImmediate(board);EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        private static void VerifySwitching(Scene scene)
        {
            foreach(string type in BuildingLevelView.Types)
            {
                var root=new GameObject("Level switch check");SceneManager.MoveGameObjectToScene(root,scene);var view=root.AddComponent<BuildingLevelView>();view.Configure(type,2,null);
                try
                {
                    foreach(int level in new[]{1,2,3,1,2})
                    {
                        if(!view.SetLevel(level)||view.Level!=level)throw new InvalidOperationException("Level switch failed: "+type);
                        int active=0;foreach(Transform child in root.transform)if(child.gameObject.activeSelf)active++;
                        if(active!=1||view.WorldBounds.size.sqrMagnitude<=0)throw new InvalidOperationException("Level switch left invalid visuals: "+type);
                    }
                    if(view.SetLevel(0)||view.SetLevel(4))throw new InvalidOperationException("Out of range building level accepted");
                }
                finally{Object.DestroyImmediate(root);}
            }
        }
        [MenuItem("Harvest Isle/Structures/Open building level gallery")]
        public static void OpenGallery(){string path=Path.GetFullPath(Folder+"/index.html");if(!File.Exists(path))RebuildAndPreview();Application.OpenURL(new Uri(path).AbsoluteUri);}
        [Serializable]private sealed class Manifest{public Entry[] buildings;}
        [Serializable]private sealed class Entry{public string type,label;public string[] prefabs;public int[] triangles;}
    }
}
