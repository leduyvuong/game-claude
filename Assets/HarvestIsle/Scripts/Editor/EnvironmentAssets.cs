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
    [InitializeOnLoad]
    public static class EnvironmentAssets
    {
        private const string GeometryPath="Assets/HarvestIsle/Art/Environment/EnvironmentModels.asset";
        private const string PreviewFolder="Art/Previews/Environment";
        public static string PrefabPath(string name){return "Assets/HarvestIsle/Resources/Environment/"+name+".prefab";}
        static EnvironmentAssets(){EditorApplication.delayCall+=Ensure;}
        private static void Ensure()
        {
            if(Application.isBatchMode||EditorApplication.isPlayingOrWillChangePlaymode)return;
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            bool missing=data==null||data.version!=EnvironmentModelBuilder.Version;
            foreach(string name in EnvironmentModelBuilder.Names)missing|=!File.Exists(PrefabPath(name));
            if(missing)Rebuild();
        }

        [MenuItem("Harvest Isle/Environment/Rebuild environment models")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GeometryPath));
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath("GrassTile")));
            AssetDatabase.Refresh();
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,GeometryPath);}
            Mesh[] previousMeshes=data.meshes;Material[] previousMaterials=data.materials;
            Material[] materials=EnvironmentModelBuilder.CreateMaterials();
            var meshes=new List<Mesh>();var models=new List<GameObject>();
            try
            {
                foreach(Material material in materials)AssetDatabase.AddObjectToAsset(material,data);
                foreach(string name in EnvironmentModelBuilder.Names)
                    models.Add(EnvironmentModelBuilder.Create(name,materials,meshes));
                foreach(Mesh mesh in meshes)AssetDatabase.AddObjectToAsset(mesh,data);
                data.version=EnvironmentModelBuilder.Version;data.meshes=meshes.ToArray();data.materials=materials;
                EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
                foreach(GameObject model in models)
                {
                    if(PrefabUtility.SaveAsPrefabAsset(model,PrefabPath(model.name))==null)
                        throw new InvalidOperationException("Could not save environment prefab: "+model.name);
                    int triangles=0;foreach(MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())triangles+=filter.sharedMesh.triangles.Length/3;
                    Debug.Log("Environment baked: "+model.name+", "+triangles+" triangles.");
                }
                if(previousMeshes!=null)foreach(Mesh mesh in previousMeshes)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(previousMaterials!=null)foreach(Material material in previousMaterials)if(material!=null)Object.DestroyImmediate(material,true);
                AssetDatabase.SaveAssets();
            }
            finally{foreach(GameObject model in models)Object.DestroyImmediate(model);}
        }

        [MenuItem("Harvest Isle/Environment/Render environment previews")]
        public static void RebuildAndPreview()
        {
            Rebuild();Directory.CreateDirectory(PreviewFolder);
            Scene preview=EditorSceneManager.NewPreviewScene();
            RenderTexture target=null;Texture2D image=null,board=null,overview=null;
            try
            {
                Camera camera=CreateLightingAndCamera(preview);
                const int width=720,height=640;
                target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();
                camera.aspect=(float)width/height;
                image=new Texture2D(width,height,TextureFormat.RGB24,false);
                board=new Texture2D(width*2,height,TextureFormat.RGB24,false);
                int overviewRows=Mathf.CeilToInt(EnvironmentModelBuilder.Names.Length/4f);
                overview=new Texture2D(width*4,height*overviewRows,TextureFormat.RGB24,false);
                var entries=new List<ReviewEntry>();
                for(int i=0;i<EnvironmentModelBuilder.Names.Length;i++)
                {
                    string name=EnvironmentModelBuilder.Names[i];
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(name));
                    var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab,preview);
                    try
                    {
                        if(name=="SeaSurface")model.transform.localScale=new Vector3(0.05f,1,0.05f);
                        Bounds bounds=BoundsOf(model);int triangles=0;
                        foreach(MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())triangles+=filter.sharedMesh.triangles.Length/3;
                        entries.Add(new ReviewEntry{name=name,label=EnvironmentModelBuilder.Labels[i],group=EnvironmentModelBuilder.Groups[i],prefab=PrefabPath(name),triangles=triangles});
                        FreezeWater(model,3.5f);
                        Vector3 center=bounds.center;
                        float size=Mathf.Max(bounds.size.x,bounds.size.z)*0.5f;
                        camera.orthographicSize=Mathf.Max(0.58f,size*1.02f+bounds.size.y*0.36f);
                        Vector3[] views={new Vector3(4,3.2f,5.5f),new Vector3(-4,5.4f,4)};
                        for(int view=0;view<2;view++)
                        {
                            camera.transform.position=center+views[view]*Mathf.Max(size,1);
                            camera.transform.LookAt(center);
                            Render(camera,target);Read(target,image,0,0);Read(target,board,view*width,0);
                            if(view==0)
                            {
                                image.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+".png",image.EncodeToPNG());
                                Read(target,overview,(i%4)*width,(overviewRows-1-i/4)*height);
                            }
                        }
                        board.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+"-views.png",board.EncodeToPNG());
                        if(name=="WaterTile")
                        {
                            var motion=new Texture2D(width*3,height,TextureFormat.RGB24,false);
                            try
                            {
                                camera.transform.position=center+views[0]*Mathf.Max(size,1);camera.transform.LookAt(center);
                                for(int frame=0;frame<3;frame++)
                                {FreezeWater(model,frame*4);Render(camera,target);Read(target,motion,frame*width,0);Read(target,image,0,0);
                                    image.Apply();File.WriteAllBytes(PreviewFolder+"/Water-frame-"+frame+".png",image.EncodeToPNG());}
                                motion.Apply();File.WriteAllBytes(PreviewFolder+"/Water-motion.png",motion.EncodeToPNG());
                            }
                            finally{Object.DestroyImmediate(motion);}
                        }
                    }
                    finally{Object.DestroyImmediate(model);}
                    Debug.Log("Environment preview rendered: "+name);
                }
                overview.Apply();File.WriteAllBytes(PreviewFolder+"/Overview.png",overview.EncodeToPNG());
                string json=JsonUtility.ToJson(new ReviewManifest{models=entries.ToArray()},true);
                File.WriteAllText(PreviewFolder+"/models.json",json,new UTF8Encoding(false));
                string html=File.ReadAllText("Tools/Templates/environment-gallery.html",Encoding.UTF8).Replace("__MODEL_DATA__",json);
                File.WriteAllText(PreviewFolder+"/index.html",html,new UTF8Encoding(false));
                Debug.Log("Environment library verified: "+entries.Count+" prefabs. "+Path.GetFullPath(PreviewFolder+"/index.html"));
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}
                if(image!=null)Object.DestroyImmediate(image);if(board!=null)Object.DestroyImmediate(board);if(overview!=null)Object.DestroyImmediate(overview);
                EditorSceneManager.ClosePreviewScene(preview);
            }
            RenderShowcase();
            RenderFarmPreview();
        }

        private static Camera CreateLightingAndCamera(Scene scene)
        {
            var sun=new GameObject("Environment sun").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);
            sun.type=LightType.Directional;sun.color=new Color(1,0.96f,0.87f);sun.intensity=1.15f;sun.shadows=LightShadows.Soft;
            sun.transform.rotation=Quaternion.Euler(48,-32,0);
            var fill=new GameObject("Environment fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);
            fill.type=LightType.Directional;fill.color=new Color(0.84f,0.92f,1);fill.intensity=0.48f;fill.transform.rotation=Quaternion.Euler(30,140,0);
            var camera=new GameObject("Environment camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.cameraType=CameraType.Preview;
            camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);
            camera.orthographic=true;camera.nearClipPlane=0.1f;camera.farClipPlane=200;
            camera.allowHDR=false;camera.allowMSAA=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("F2EDE0");
            return camera;
        }
        private static Bounds BoundsOf(GameObject model)
        {
            var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);return bounds;
        }
        private static void FreezeWater(GameObject model,float time)
        {
            foreach(Renderer renderer in model.GetComponentsInChildren<Renderer>())
            {
                foreach(Material material in renderer.sharedMaterials)
                    if(material.shader.name=="Harvest Isle/Stylized Water")
                    {var block=new MaterialPropertyBlock();block.SetFloat("_PreviewTime",time);renderer.SetPropertyBlock(block);break;}
            }
        }
        private static void Render(Camera camera,RenderTexture target)
        {RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});}
        private static void Read(RenderTexture source,Texture2D destination,int x,int y)
        {
            RenderTexture previous=RenderTexture.active;
            try{RenderTexture.active=source;destination.ReadPixels(new Rect(0,0,source.width,source.height),x,y,false);}
            finally{RenderTexture.active=previous;}
        }

        [MenuItem("Harvest Isle/Environment/Render island showcase")]
        public static void RenderShowcase()
        {
            Directory.CreateDirectory(PreviewFolder);Scene scene=EditorSceneManager.NewPreviewScene();
            RenderTexture target=null;Texture2D image=null;
            try
            {
                Camera camera=CreateLightingAndCamera(scene);
                Place("SeaSurface",scene,Vector3.zero,1);
                Place("IslandBase",scene,Vector3.zero,1);
                PlaceLandscapeTrees(scene);
                Place("Hill",scene,new Vector3(-6.4f,0.15f,4.2f),0.80f);
                Place("Pond",scene,new Vector3(-5.8f,0.15f,-0.7f),1);
                Place("Lake",scene,new Vector3(5.8f,0.15f,-3.3f),0.56f);
                Place("RockCluster",scene,new Vector3(7.6f,0.15f,2.1f),0.65f);
                Place("RockCluster",scene,new Vector3(-6.9f,0.15f,-4.2f),0.55f);
                for(int i=0;i<6;i++)
                {float a=i*2.4f;Place("RockSmall",scene,new Vector3(Mathf.Cos(a)*8.5f,0.10f,Mathf.Sin(a)*6.9f),0.9f);}
                var palm=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HarvestIsle/Resources/Nature/CoconutPalm.prefab");
                if(palm!=null)
                {
                    Vector3[] positions={new Vector3(-7,0.15f,1.7f),new Vector3(5.9f,0.15f,1.4f),new Vector3(2.9f,0.15f,-6.3f)};
                    foreach(Vector3 p in positions){var model=(GameObject)PrefabUtility.InstantiatePrefab(palm,scene);model.transform.position=p;model.transform.localScale=Vector3.one*0.85f;}
                }
                for(int row=0;row<3;row++)for(int column=0;column<5;column++)
                {
                    Vector3 p=new Vector3((column-2)*1.18f,0.15f,(row-1)*1.18f-0.4f);
                    Place("RaisedBed",scene,p,1);
                    string[] crops={"Rice","Carrot","Corn","Pumpkin","Tomato"};
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(CropCollectionAssets.PrefabPath(crops[column],5));
                    if(prefab!=null){var crop=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);crop.transform.position=p+Vector3.up*0.21f;}
                }
                var farmer=AssetDatabase.LoadAssetAtPath<GameObject>(FarmerAssets.PrefabPath);
                if(farmer!=null){var model=(GameObject)PrefabUtility.InstantiatePrefab(farmer,scene);model.transform.position=new Vector3(-1.5f,0.15f,2.6f);model.transform.rotation=Quaternion.Euler(0,145,0);}
                camera.orthographicSize=10.4f;camera.aspect=1.5f;camera.transform.position=new Vector3(15,18,-20);camera.transform.LookAt(new Vector3(0,0.6f,0));
                target=new RenderTexture(1500,1000,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();
                image=new Texture2D(1500,1000,TextureFormat.RGB24,false);Render(camera,target);Read(target,image,0,0);image.Apply();
                File.WriteAllBytes(PreviewFolder+"/Island-showcase.png",image.EncodeToPNG());
                Debug.Log("Environment island showcase rendered.");
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}if(image!=null)Object.DestroyImmediate(image);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        private static GameObject Place(string name,Scene scene,Vector3 position,float scale)
        {
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(name)),scene);
            model.transform.position=position;model.transform.localScale=Vector3.one*scale;FreezeWater(model,3.5f);return model;
        }

        internal static void PlaceLandscapeTrees(Scene scene,Transform parent=null)
        {
            string[] trees={"RainTree","BanyanTree","PineTree","WillowTree","FlameTree","ArecaPalm","YoungForestTree","PineTree","RainTree","YoungForestTree"};
            Vector3[] positions={new Vector3(-5.8f,0,5.1f),new Vector3(6.1f,0,4.7f),new Vector3(-1.8f,0,6.4f),new Vector3(-7.5f,0,.7f),new Vector3(7.0f,0,-3.5f),new Vector3(-6.6f,0,-5.2f),new Vector3(3.0f,0,5.8f),new Vector3(1.1f,0,6.65f),new Vector3(4.8f,0,5.5f),new Vector3(-3.1f,0,5.7f)};
            for(int i=0;i<trees.Length;i++)
            {Vector3 p=positions[i];p.y=IslandTerrainProfile.Height(p.x,p.z);var tree=Place(trees[i],scene,p,i==3?.7f:i==2||i==7?.65f:.72f);tree.transform.rotation=Quaternion.Euler(0,i*137.5f,0);if(parent!=null)tree.transform.SetParent(parent,true);}
        }

        [MenuItem("Harvest Isle/Environment/Render farm preview")]
        public static void RenderFarmPreview()
        {
            Directory.CreateDirectory(PreviewFolder);Scene scene=EditorSceneManager.NewPreviewScene();
            RenderTexture target=null;Texture2D image=null;
            AmbientMode ambient=RenderSettings.ambientMode;Color ambientColor=RenderSettings.ambientLight;
            bool fog=RenderSettings.fog;Light previousSun=RenderSettings.sun;
            try
            {
                var root=new GameObject("Farm environment preview");SceneManager.MoveGameObjectToScene(root,scene);
                var world=root.AddComponent<FarmWorld>();world.Build(null);
                double now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();var state=HarvestIsle.Core.FarmModel.NewState(now-180);state.coins=1000;
                var model=new HarvestIsle.Core.FarmModel(state);
                for(int i=0;i<HarvestIsle.Core.FarmModel.PlotCount;i++)
                    model.Apply(i,HarvestIsle.Core.FarmTool.Plant,HarvestIsle.Core.CropCatalog.All[i%3].id,now-180);
                world.Refresh(model,now,-1);FreezeWater(root,3.5f);
                Camera camera=world.GameCamera;camera.cameraType=CameraType.Preview;
                camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);
                camera.allowMSAA=false;camera.orthographic=true;camera.orthographicSize=10.6f;camera.aspect=1.5f;
                camera.transform.position=new Vector3(15,18,-20);camera.transform.LookAt(new Vector3(0,0.6f,0));
                var fill=new GameObject("Farm preview fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);
                fill.type=LightType.Directional;fill.intensity=0.30f;fill.color=new Color(0.85f,0.92f,1);fill.transform.rotation=Quaternion.Euler(30,140,0);
                target=new RenderTexture(1500,1000,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();
                image=new Texture2D(1500,1000,TextureFormat.RGB24,false);Render(camera,target);Read(target,image,0,0);image.Apply();
                File.WriteAllBytes(PreviewFolder+"/Farm-preview.png",image.EncodeToPNG());
                Debug.Log("Farm environment preview rendered through FarmWorld with native assets.");
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}if(image!=null)Object.DestroyImmediate(image);
                RenderSettings.ambientMode=ambient;RenderSettings.ambientLight=ambientColor;RenderSettings.fog=fog;RenderSettings.sun=previousSun;
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [MenuItem("Harvest Isle/Environment/Open environment gallery")]
        public static void OpenGallery()
        {
            string path=Path.GetFullPath(PreviewFolder+"/index.html");if(!File.Exists(path))RebuildAndPreview();Application.OpenURL(new Uri(path).AbsoluteUri);
        }
        [Serializable]private sealed class ReviewManifest{public ReviewEntry[] models;}
        [Serializable]private sealed class ReviewEntry{public string name,label,group,prefab;public int triangles;}
    }
}
