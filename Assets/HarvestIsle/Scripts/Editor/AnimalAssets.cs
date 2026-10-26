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
    public static partial class AnimalAssets
    {
        private const string GeometryPath="Assets/HarvestIsle/Art/Animals/FarmAnimals.asset";
        private const string PreviewFolder="Art/Previews/Animals";
        public static string PrefabPath(string name){return "Assets/HarvestIsle/Resources/Animals/"+name+".prefab";}
        static AnimalAssets(){EditorApplication.delayCall+=Ensure;}
        private static void Ensure()
        {
            if(Application.isBatchMode||EditorApplication.isPlayingOrWillChangePlaymode)return;
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            bool missing=data==null||data.version!=AnimalModelBuilder.Version;
            foreach(string name in AnimalModelBuilder.Names)missing|=!File.Exists(PrefabPath(name));
            if(missing)Rebuild();
        }
        [MenuItem("Harvest Isle/Animals/Rebuild farm animals")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GeometryPath));Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath("Cow")));
            AssetDatabase.Refresh();var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,GeometryPath);}
            Mesh[] previousMeshes=data.meshes;Material[] previousMaterials=data.materials;
            Material[] materials=AnimalModelBuilder.CreateMaterials();var meshes=new List<Mesh>();var models=new List<GameObject>();
            try
            {
                for(int i=0;i<AnimalModelBuilder.Names.Length;i++)models.Add(AnimalModelBuilder.Create(i,materials,meshes));
                foreach(Mesh mesh in meshes)AssetDatabase.AddObjectToAsset(mesh,data);
                foreach(Material material in materials)AssetDatabase.AddObjectToAsset(material,data);
                data.version=AnimalModelBuilder.Version;data.meshes=meshes.ToArray();data.materials=materials;
                EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
                foreach(GameObject model in models)
                {
                    if(PrefabUtility.SaveAsPrefabAsset(model,PrefabPath(model.name))==null)throw new InvalidOperationException("Could not save animal prefab: "+model.name);
                    int count=0;foreach(MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())count+=filter.sharedMesh.triangles.Length/3;
                    Debug.Log("Animal baked: "+model.name+", "+count+" triangles.");
                }
                if(previousMeshes!=null)foreach(Mesh mesh in previousMeshes)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(previousMaterials!=null)foreach(Material material in previousMaterials)if(material!=null)Object.DestroyImmediate(material,true);
                AssetDatabase.SaveAssets();
            }
            finally{foreach(GameObject model in models)Object.DestroyImmediate(model);}
        }

        [MenuItem("Harvest Isle/Animals/Render animal collection")]
        public static void RebuildAndPreview()
        {
            Rebuild();Directory.CreateDirectory(PreviewFolder);
            Scene scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D single=null,views=null,actions=null,overview=null,newAnimals=null;Material ground=null;
            try
            {
                Camera camera=PreviewCamera(scene);
                var floor=GameObject.CreatePrimitive(PrimitiveType.Cylinder);SceneManager.MoveGameObjectToScene(floor,scene);
                ground=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));ground.SetColor("_BaseColor",FarmWorld.Hex("D2CEB1"));
                ground.SetColor("_SpecColor",Color.black);floor.GetComponent<Renderer>().sharedMaterial=ground;
                floor.transform.position=new Vector3(0,-0.035f,0);floor.transform.localScale=new Vector3(1.65f,0.035f,1.65f);
                const int width=640,height=720;target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();camera.aspect=(float)width/height;
                single=new Texture2D(width,height,TextureFormat.RGB24,false);views=new Texture2D(width*3,height,TextureFormat.RGB24,false);
                actions=new Texture2D(width*4,height,TextureFormat.RGB24,false);
                int rows=(AnimalModelBuilder.Names.Length+4)/5;
                overview=new Texture2D(width*5,height*rows,TextureFormat.RGB24,false);
                var background=new Color32[overview.width*overview.height];Color32 bg=FarmWorld.Hex("F2EDE0");
                for(int p=0;p<background.Length;p++)background[p]=bg;overview.SetPixels32(background);
                newAnimals=new Texture2D(width*4,height*2,TextureFormat.RGB24,false);
                var entries=new List<ReviewEntry>();
                for(int i=0;i<AnimalModelBuilder.Names.Length;i++)
                {
                    string name=AnimalModelBuilder.Names[i];var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(name)),scene);
                    try
                    {
                        var avatar=model.GetComponent<FarmAnimalAvatar>();avatar.PreviewPose(FarmAnimalPose.Idle,0);
                        Bounds bounds=BoundsOf(model);Vector3 center=new Vector3(0,bounds.center.y,0);
                        camera.orthographicSize=Mathf.Max(i<5?0.72f:0.44f,Mathf.Max(bounds.size.z,bounds.size.x)*0.61f+bounds.size.y*0.25f);
                        floor.SetActive(!avatar.IsAquatic);
                        float stand=Mathf.Min(camera.orthographicSize*camera.aspect*1.80f,Mathf.Max(bounds.size.x,bounds.size.z)*1.18f+0.10f);
                        floor.transform.localScale=new Vector3(stand,0.035f,stand);
                        Vector3[] angles={new Vector3(3.0f,1.45f,5),new Vector3(-5,1.50f,1.4f),new Vector3(2.8f,1.60f,-5)};
                        int triangles=0;foreach(MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())triangles+=filter.sharedMesh.triangles.Length/3;
                        entries.Add(new ReviewEntry{name=name,label=AnimalModelBuilder.Labels[i],prefab=PrefabPath(name),triangles=triangles,
                            actions=avatar.IsAquatic?new[]{"Thả mình","Bơi","Ngoi đầu","Lượn"}:
                                new[]{"Đứng", "Đi", avatar.IsBird?"Mổ thức ăn":i==3?"Ngửi đất":"Cúi đầu",avatar.IsSongBird?"Bay và vỗ cánh":avatar.IsBird?"Vỗ cánh":"Nhìn quanh"}});
                        for(int view=0;view<3;view++)
                        {
                            camera.transform.position=center+angles[view];camera.transform.LookAt(center);
                            Render(camera,target);Read(target,views,view*width,0);
                            if(view==0)
                            {
                                Read(target,single,0,0);Read(target,overview,(i%5)*width,(rows-1-i/5)*height);
                                if(i>=5)Read(target,newAnimals,(i-5)%4*width,(1-(i-5)/4)*height);
                                single.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+".png",single.EncodeToPNG());
                            }
                        }
                        views.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+"-views.png",views.EncodeToPNG());
                        camera.transform.position=center+angles[0];camera.transform.LookAt(center);
                        for(int pose=0;pose<4;pose++)
                        {
                            float t=pose==1?Mathf.PI/(2*(avatar.IsAquatic?6:avatar.IsBird?10:7)):pose==3?0.60f:0;
                            avatar.PreviewPose((FarmAnimalPose)pose,t);ValidateRig(model,avatar);
                            Render(camera,target);Read(target,actions,pose*width,0);Read(target,single,0,0);
                            single.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+"-pose-"+pose+".png",single.EncodeToPNG());
                        }
                        actions.Apply();File.WriteAllBytes(PreviewFolder+"/"+name+"-actions.png",actions.EncodeToPNG());
                        Debug.Log("Animal views and poses verified: "+name);
                    }
                    finally{Object.DestroyImmediate(model);}
                }
                overview.Apply();File.WriteAllBytes(PreviewFolder+"/Overview.png",overview.EncodeToPNG());
                newAnimals.Apply();File.WriteAllBytes(PreviewFolder+"/New-animals.png",newAnimals.EncodeToPNG());
                string json=JsonUtility.ToJson(new ReviewManifest{animals=entries.ToArray()},true);
                File.WriteAllText(PreviewFolder+"/models.json",json,new UTF8Encoding(false));
                File.WriteAllText(PreviewFolder+"/index.html",File.ReadAllText("Tools/Templates/animal-gallery.html",Encoding.UTF8).Replace("__ANIMAL_DATA__",json),new UTF8Encoding(false));
                Debug.Log("Animal collection rendered: "+entries.Count+" prefabs, "+entries.Count*4+" poses. "+Path.GetFullPath(PreviewFolder+"/index.html"));
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}if(single!=null)Object.DestroyImmediate(single);
                if(views!=null)Object.DestroyImmediate(views);if(actions!=null)Object.DestroyImmediate(actions);if(overview!=null)Object.DestroyImmediate(overview);
                if(newAnimals!=null)Object.DestroyImmediate(newAnimals);
                if(ground!=null)Object.DestroyImmediate(ground);EditorSceneManager.ClosePreviewScene(scene);
            }
            EnvironmentAssets.RenderFarmPreview();
            File.Copy("Art/Previews/Environment/Farm-preview.png",PreviewFolder+"/Farm-preview.png",true);
            RenderFishPond();
        }
        private static void ValidateRig(GameObject model,FarmAnimalAvatar avatar)
        {
            foreach(Transform joint in model.GetComponentsInChildren<Transform>())
            {
                Vector3 p=joint.localPosition,s=joint.localScale;Quaternion r=joint.localRotation;
                if(float.IsNaN(p.x)||float.IsNaN(p.y)||float.IsNaN(p.z)||float.IsNaN(s.x)||float.IsNaN(s.y)||float.IsNaN(s.z)
                    ||float.IsNaN(r.x)||float.IsNaN(r.y)||float.IsNaN(r.z)||float.IsNaN(r.w))throw new InvalidOperationException("Invalid animal pose: "+avatar.Kind);
            }
            if(!avatar.GroundedPose)return;
            float lowest=float.PositiveInfinity;
            foreach(Transform joint in model.GetComponentsInChildren<Transform>())if(joint.name.EndsWith("contact"))lowest=Mathf.Min(lowest,model.transform.InverseTransformPoint(joint.position).y);
            if(Mathf.Abs(lowest)>0.005f)throw new InvalidOperationException("Animal stance is off the ground: "+avatar.Kind+" "+lowest);
        }
        private static Bounds BoundsOf(GameObject model)
        {var renderers=model.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(Renderer renderer in renderers)b.Encapsulate(renderer.bounds);return b;}
        [MenuItem("Harvest Isle/Animals/Render fish pond")]
        public static void RenderFishPond()
        {
            Scene scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D image=null;
            try
            {
                Camera camera=PreviewCamera(scene);
                var pond=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentAssets.PrefabPath("Pond")),scene);
                foreach(Renderer renderer in pond.GetComponentsInChildren<Renderer>())
                    foreach(Material material in renderer.sharedMaterials)if(material.shader.name=="Harvest Isle/Stylized Water")
                    {var block=new MaterialPropertyBlock();block.SetFloat("_PreviewTime",3.5f);renderer.SetPropertyBlock(block);break;}
                string[] species={"Koi","Carp","Perch"};
                Vector3[] positions={new Vector3(-0.53f,0.043f,-0.05f),new Vector3(0.36f,0.043f,-0.32f),new Vector3(0.39f,0.043f,0.40f)};
                for(int i=0;i<species.Length;i++)
                {
                    var fish=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(species[i])),scene);
                    fish.transform.position=positions[i];fish.transform.localScale=Vector3.one*0.40f;
                    fish.transform.rotation=Quaternion.Euler(0,20+i*95,0);fish.GetComponent<FarmAnimalAvatar>().PreviewPose(FarmAnimalPose.Walk,0.2f+i*0.16f);
                }
                camera.orthographicSize=1.30f;camera.aspect=1280f/900;camera.transform.position=new Vector3(2.2f,3.8f,3.3f);camera.transform.LookAt(new Vector3(0,0.14f,0));
                target=new RenderTexture(1280,900,24,RenderTextureFormat.ARGB32){antiAliasing=1};target.Create();
                image=new Texture2D(1280,900,TextureFormat.RGB24,false);Render(camera,target);Read(target,image,0,0);image.Apply();
                Directory.CreateDirectory(PreviewFolder);File.WriteAllBytes(PreviewFolder+"/Fish-pond.png",image.EncodeToPNG());
                Debug.Log("Three fish species rendered beneath clear pond water.");
            }
            finally
            {
                if(target!=null){target.Release();Object.DestroyImmediate(target);}if(image!=null)Object.DestroyImmediate(image);EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        private static Camera PreviewCamera(Scene scene)
        {
            var sun=new GameObject("Animal sun").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);
            sun.type=LightType.Directional;sun.color=new Color(1,0.96f,0.87f);sun.intensity=1.2f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(42,-30,0);
            var fill=new GameObject("Animal fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);
            fill.type=LightType.Directional;fill.color=new Color(0.84f,0.92f,1);fill.intensity=0.45f;fill.transform.rotation=Quaternion.Euler(30,140,0);
            var camera=new GameObject("Animal camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.cameraType=CameraType.Preview;camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);
            camera.orthographic=true;camera.nearClipPlane=0.1f;camera.farClipPlane=30;camera.allowHDR=false;camera.allowMSAA=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("F2EDE0");return camera;
        }
        private static void Render(Camera camera,RenderTexture target){RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});}
        private static void Read(RenderTexture source,Texture2D destination,int x,int y)
        {RenderTexture previous=RenderTexture.active;try{RenderTexture.active=source;destination.ReadPixels(new Rect(0,0,source.width,source.height),x,y,false);}finally{RenderTexture.active=previous;}}
        [MenuItem("Harvest Isle/Animals/Open animal gallery")]
        public static void OpenGallery(){string path=Path.GetFullPath(PreviewFolder+"/index.html");if(!File.Exists(path))RebuildAndPreview();Application.OpenURL(new Uri(path).AbsoluteUri);}
        [Serializable]private sealed class ReviewManifest{public ReviewEntry[] animals;}
        [Serializable]private sealed class ReviewEntry{public string name,label,prefab;public string[] actions;public int triangles;}
    }
}
