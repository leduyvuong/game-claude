using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static class CropGrowthShowcaseAssets
    {
        public const string ScenePath="Assets/HarvestIsle/Scenes/CropGrowthShowcase.unity";
        private const string Folder="Art/Previews/CropGrowth";
        [MenuItem("Harvest Isle/Crops/Show all 30 crops and growth stages")]
        public static void Build()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            string[] names=CompleteModelDefinitions.ExistingCrops.Concat(CompleteCropBuilder.Names).ToArray();
            string[] labels=CompleteModelDefinitions.ExistingCropLabels.Concat(CompleteCropBuilder.Labels).ToArray();
            var garden=new GameObject("Growth garden - 30 rows, 150 plants");var rows=new List<CropGrowthShowcase.Row>();
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            for(int i=0;i<names.Length;i++)
            {
                int block=i/10,row=i%10;var group=new GameObject((i+1)+" - "+names[i]);group.transform.SetParent(garden.transform);
                group.transform.position=new Vector3((block-1)*12.5f-4.2f,0,10.7f-row*2.35f);
                var stages=new Transform[5];
                for(int stage=1;stage<=5;stage++)
                {
                    Vector3 p=group.transform.position+new Vector3((stage-1)*2.1f,0,0);
                    var bed=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentAssets.PrefabPath(names[i]=="Rice"||names[i]=="Lotus"||names[i]=="WaterSpinach"?"WetDirtTile":"DirtTile")));
                    bed.transform.SetParent(group.transform,true);bed.transform.position=p;bed.transform.localScale=new Vector3(.66f,.55f,.66f);
                    string path=CropCollectionAssets.PrefabPath(names[i],stage);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null)throw new InvalidOperationException("Missing crop stage: "+path);
                    var crop=(GameObject)PrefabUtility.InstantiatePrefab(prefab);crop.name=names[i]+"_"+stage;crop.transform.SetParent(group.transform,true);crop.transform.position=p+Vector3.up*.012f;stages[stage-1]=crop.transform;
                    Label(stage.ToString(),p+new Vector3(0,.03f,-.83f),.065f,font,group.transform);
                }
                Label((i+1)+". "+labels[i],group.transform.position+new Vector3(4.2f,.03f,-1.38f),.050f,font,group.transform);
                rows.Add(new CropGrowthShowcase.Row{id=names[i],label=labels[i],root=group.transform,stages=stages});
            }
            var ground=GameObject.CreatePrimitive(PrimitiveType.Cube);ground.name="Garden lawn";ground.transform.position=new Vector3(0,-.21f,0);ground.transform.localScale=new Vector3(38,.2f,26);
            const string matPath="Assets/HarvestIsle/Art/Environment/CropGardenGround.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));AssetDatabase.CreateAsset(mat,matPath);}mat.SetColor("_BaseColor",FarmWorld.Hex("ACBD89"));mat.SetColor("_SpecColor",Color.black);ground.GetComponent<Renderer>().sharedMaterial=mat;
            var sun=new GameObject("Garden sunlight").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.1f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(53,-25,0);sun.color=FarmWorld.Hex("FFF4DE");
            var fill=new GameObject("Soft fill").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.4f;fill.transform.rotation=Quaternion.Euler(35,140,0);
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.57f,.60f,.56f);RenderSettings.sun=sun;RenderSettings.fog=false;
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(UniversalAdditionalCameraData)).GetComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.nearClipPlane=.05f;camera.farClipPlane=150;camera.allowHDR=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("ECE6D6");
            var viewer=new GameObject("Growth garden viewer").AddComponent<CropGrowthShowcase>();viewer.Configure(rows.ToArray(),camera);viewer.Overview();
            Directory.CreateDirectory(Folder);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
            camera.aspect=1.5f;viewer.Overview();Render(camera,Folder+"/All-30-crops.png",2400,1600);
            for(int i=0;i<rows.Count;i++){camera.aspect=2.5f;viewer.Select(i);Render(camera,Folder+"/"+names[i]+".png",2000,800);}
            viewer.Overview();EditorSceneManager.SaveScene(scene,ScenePath);
            var cards=new StringBuilder();for(int i=0;i<names.Length;i++)cards.Append("<article><h2>"+(i+1)+". "+labels[i]+"</h2><a href='"+names[i]+".png'><img loading='lazy' src='"+names[i]+".png' alt='Năm giai đoạn của "+labels[i]+"'></a><p>Mầm non → Cây non → Phát triển thân lá → Hoa / quả non → Thu hoạch</p></article>");
            File.WriteAllText(Folder+"/index.html",File.ReadAllText("Tools/Templates/crop-growth-gallery.html",Encoding.UTF8).Replace("__CARDS__",cards.ToString()),new UTF8Encoding(false));
            Debug.Log("CROP_GARDEN_CREATED: 30 species, 150 stage prefabs, unchanged scale between stages.");
        }
        private static void Label(string text,Vector3 p,float size,Font font,Transform parent)
        {
            var label=new GameObject("Label "+text).AddComponent<TextMesh>();label.transform.SetParent(parent);label.transform.position=p;label.transform.rotation=Quaternion.Euler(70,0,0);label.text=text;label.font=font;label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=FarmWorld.Hex("35482D");label.GetComponent<MeshRenderer>().sharedMaterial=font.material;
        }
        private static void Render(Camera camera,string path,int width,int height)
        {
            var target=new RenderTexture(width,height,24);var image=new Texture2D(width,height,TextureFormat.RGB24,false);var old=RenderTexture.active;float aspect=camera.aspect;
            try{target.Create();camera.aspect=(float)width/height;RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{camera.aspect=aspect;RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(image);}
        }
        internal static void VerifyLive()
        {
            var viewer=Object.FindFirstObjectByType<CropGrowthShowcase>();if(viewer==null||viewer.Rows.Length!=30)throw new InvalidOperationException("Missing crop garden rows");int count=0;
            int healthCount=0, maxTriangles=0;
            foreach(var row in viewer.Rows)
            {
                if(row.stages.Length!=5)throw new InvalidOperationException("Incomplete row "+row.id);
                var shapes=new HashSet<string>();
                for(int i=0;i<5;i++)
                {
                    var crop=row.stages[i];
                    if(crop==null||crop.name!=row.id+"_"+(i+1)||crop.localScale!=Vector3.one)throw new InvalidOperationException("Invalid stage/scale "+row.id);
                    ValidatePlant(crop.gameObject);
                    var mesh=crop.GetComponent<MeshFilter>().sharedMesh;
                    maxTriangles=Math.Max(maxTriangles,mesh.triangles.Length/3);
                    using(var stream=new MemoryStream())
                    {
                        using(var writer=new BinaryWriter(stream,Encoding.UTF8,true))foreach(var p in mesh.vertices){writer.Write(p.x);writer.Write(p.y);writer.Write(p.z);}
                        using(var hash=System.Security.Cryptography.SHA256.Create())
                            if(!shapes.Add(Convert.ToBase64String(hash.ComputeHash(stream.ToArray()))))throw new InvalidOperationException("Repeated growth geometry "+row.id);
                    }
                    count++;
                }
                foreach(string state in new[]{"Diseased","Wilted"})
                {
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(CompleteModelDefinitions.StatePath(row.id,state));
                    ValidatePlant(prefab);healthCount++;
                }
            }
            for(int i=0;i<30;i++)if(!viewer.Select(i))throw new InvalidOperationException("Cannot select crop "+i);
            viewer.Overview();ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/Unity-garden.png"));File.WriteAllText(".tools/model-work/crop-garden-result.json","{\"status\":\"complete\",\"species\":30,\"stages\":"+count+",\"healthPrefabs\":"+healthCount+",\"distinctGrowthGeometry\":true,\"maxPlantTriangles\":"+maxTriangles+",\"cameraSelections\":30}");Debug.Log("CROP_GARDEN_VERIFIED: all 150 plants and 30 row selections in Play.");
        }
        private static void ValidatePlant(GameObject plant)
        {
            if(plant==null)throw new InvalidOperationException("Missing plant prefab");
            foreach(var filter in plant.GetComponentsInChildren<MeshFilter>(true))
            {
                if(filter.sharedMesh==null||filter.sharedMesh.vertexCount==0)throw new InvalidOperationException("Missing crop mesh "+plant.name);
                foreach(var p in filter.sharedMesh.vertices)if(!float.IsFinite(p.x)||!float.IsFinite(p.y)||!float.IsFinite(p.z))throw new InvalidOperationException("Invalid crop vertex "+plant.name);
            }
            foreach(var renderer in plant.GetComponentsInChildren<Renderer>(true))
                foreach(var material in renderer.sharedMaterials)
                    if(material==null||material.shader==null||!material.shader.isSupported||material.shader.name.Contains("InternalError"))throw new InvalidOperationException("Missing/invalid crop material "+plant.name);
        }
    }
}
