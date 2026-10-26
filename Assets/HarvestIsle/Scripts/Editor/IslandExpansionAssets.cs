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
    public static class IslandExpansionAssets
    {
        const string Root="Assets/HarvestIsle/Resources/IslandExpansion",Art="Assets/HarvestIsle/Art/IslandExpansion",Preview="Art/Previews/IslandExpansion";
        static readonly string[] Samples={"B01","B02","B05","C04","C06","C10","D06","D11","D13","D17"};
        public static string Resource(string id,string variant="")
        {return "IslandExpansion/"+(id[0]=='B'?"Buildings/":id[0]=='D'?"Decor/":variant=="Product"?"Products/":"Crops/")+id+(id[0]=='C'&&variant!="Product"?"_"+variant:"");}
        public static string PrefabPath(string id,string variant=""){return "Assets/HarvestIsle/Resources/"+Resource(id,variant)+".prefab";}
        [MenuItem("Harvest Isle/Expansion/1 Bake designer sample")]
        public static void BuildSample(){Build(Samples,"sample");}
        [MenuItem("Harvest Isle/Expansion/3 Bake all 30-30-60 models")]
        public static void BuildAll(){Build(AllIds(),"all");}
        [MenuItem("Harvest Isle/Expansion/2 Render designer sample")]
        public static void RenderSample(){Render(CatalogEntries().Where(e=>Samples.Any(id=>e.id=="IX_"+id||e.id=="IX_Product_"+id)).ToArray(),"Sample");}
        [MenuItem("Harvest Isle/Expansion/4 Render all 30-30-60 models")]
        public static void RenderAll(){Render(CatalogEntries(),"All");}
        static string[] AllIds(){return Enumerable.Range(1,30).Select(n=>"B"+n.ToString("00")).Concat(Enumerable.Range(1,30).Select(n=>"C"+n.ToString("00"))).Concat(Enumerable.Range(1,60).Select(n=>"D"+n.ToString("00"))).ToArray();}
        static Material[] Materials()
        {
            Directory.CreateDirectory(Art+"/Materials");AssetDatabase.Refresh();var result=new Material[IslandExpansionModels.Colors.Length];
            for(int i=0;i<result.Length;i++)
            {string hex=IslandExpansionModels.Colors[i],path=Art+"/Materials/Color_"+i.ToString("00")+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Simple Lit")){name="Island expansion "+hex};AssetDatabase.CreateAsset(mat,path);}mat.SetColor("_BaseColor",FarmWorld.Hex(hex));mat.SetColor("_SpecColor",Color.black);mat.SetFloat("_Smoothness",.08f);mat.enableInstancing=true;EditorUtility.SetDirty(mat);result[i]=mat;}
            return result;
        }
        static void Build(string[] ids,string mode)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Expansion asset bake requires Edit Mode");
            foreach(string folder in new[]{Root+"/Crops",Root+"/Products",Root+"/Buildings",Root+"/Decor",Root+"/Thumbnails",Art+"/Meshes",Preview,".tools/model-work"})Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();File.WriteAllText(".tools/model-work/expansion-assets-"+mode+".json","{\"status\":\"running\"}");
            Material[] mats=Materials();
            if(mode=="all")ValidateGeometry(mats);
            int count=0,max=0;
            foreach(string id in ids)
            {
                string[] variants=id[0]=='C'?new[]{"1","2","3","4","5","Diseased","Wilted","Product"}:new[]{""};
                foreach(string variant in variants)
                {
                    GameObject model=null;
                    try
                    {
                        model=IslandExpansionModels.Create(id,variant,mats);var filter=model.GetComponent<MeshFilter>();Mesh fresh=filter.sharedMesh;
                        string meshPath=Art+"/Meshes/"+id+(variant==""?"":"_"+variant)+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                        if(mesh==null){mesh=fresh;AssetDatabase.CreateAsset(mesh,meshPath);}
                        else
                        {
                            // Mesh native buffers must be replaced explicitly. CopySerialized can retain
                            // the previous vertex/index buffers while updating submesh metadata in Editor.
                            mesh.Clear();mesh.indexFormat=fresh.indexFormat;mesh.vertices=fresh.vertices;
                            mesh.normals=fresh.normals;mesh.colors=fresh.colors;mesh.subMeshCount=fresh.subMeshCount;
                            for(int sub=0;sub<fresh.subMeshCount;sub++)mesh.SetTriangles(fresh.GetTriangles(sub),sub);
                            mesh.bounds=fresh.bounds;mesh.name=fresh.name;mesh.UploadMeshData(false);
                            Object.DestroyImmediate(fresh);EditorUtility.SetDirty(mesh);
                        }
                        // Prefab import may reload its dependencies: persist the new buffers before it runs.
                        AssetDatabase.SaveAssetIfDirty(mesh);filter.sharedMesh=mesh;
                        if(mesh.vertexCount==0||mesh.vertices.Any(v=>float.IsNaN(v.sqrMagnitude)||float.IsInfinity(v.sqrMagnitude)))throw new InvalidOperationException("Invalid expansion mesh "+id);
                        var bounds=mesh.bounds;if(bounds.min.y<-.12f)throw new InvalidOperationException("Expansion model below floor: "+id+" "+bounds.min.y);
                        string path=PrefabPath(id,variant),guid=AssetDatabase.AssetPathToGUID(path);
                        if(PrefabUtility.SaveAsPrefabAsset(model,path)==null)throw new InvalidOperationException("Failed to save "+path);
                        if(!string.IsNullOrEmpty(guid)&&guid!=AssetDatabase.AssetPathToGUID(path))throw new InvalidOperationException("GUID changed "+path);
                        max=Mathf.Max(max,mesh.triangles.Length/3);count++;
                    }
                    finally{if(model!=null)Object.DestroyImmediate(model);}
                }
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();WriteManifest();
            File.WriteAllText(".tools/model-work/expansion-assets-"+mode+".json","{\"status\":\"complete\",\"types\":"+ids.Length+",\"prefabs\":"+count+",\"maxTriangles\":"+max+"}");
            Debug.Log("ISLAND_EXPANSION_BAKED "+mode+": "+count+" prefabs");
        }
        public static ModelLibraryEntry[] CatalogEntries(bool existingOnly=true)
        {
            var entries=new List<ModelLibraryEntry>();
            foreach(string id in AllIds())
            {
                int n=int.Parse(id.Substring(1))-1;string label=id[0]=='B'?IslandExpansionModels.Buildings[n]:id[0]=='D'?IslandExpansionModels.Decorations[n]:IslandExpansionModels.Crops[n];
                var variants=new List<ModelLibraryVariant>();string[] states=id[0]=='C'?new[]{"1","2","3","4","5","Diseased","Wilted"}:new[]{""};
                string[] stateLabels={"Mầm non","Cây non","Phát triển thân lá","Gần thu hoạch","Sẵn sàng thu hoạch","Bị bệnh","Héo"};
                for(int i=0;i<states.Length;i++)
                {string path=PrefabPath(id,states[i]);if(existingOnly&&!File.Exists(path))continue;variants.Add(Variant(id,states[i],id[0]=='C'?stateLabels[i]:label));}
                if(variants.Count>0)entries.Add(new ModelLibraryEntry{id="IX_"+id,label=label,group=id[0]=='B'?"Mở rộng · Công trình":id[0]=='D'?"Mở rộng · Trang trí":"Mở rộng · Cây trồng",variants=variants.ToArray()});
                if(id[0]=='C'&&(!existingOnly||File.Exists(PrefabPath(id,"Product"))))entries.Add(new ModelLibraryEntry{id="IX_Product_"+id,label=label+" thu hoạch",group="Mở rộng · Nông sản",variants=new[]{Variant(id,"Product","Nông sản")}});
            }
            return entries.ToArray();
        }
        static ModelLibraryVariant Variant(string id,string state,string label)
        {
            string name=id+(state==""?"":"_"+state);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(id,state));
            return new ModelLibraryVariant{label=label,prefab=PrefabPath(id,state),resource=Resource(id,state),preview="../IslandExpansion/Models/"+name+".png",views="../IslandExpansion/Models/"+name+"-views.png",triangles=prefab==null?0:prefab.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh==null?0:f.sharedMesh.triangles.Length/3)};
        }
        static void WriteManifest()
        {var catalog=new ModelLibraryCatalog{version=1,prefabCount=Directory.GetFiles(Root,"*.prefab",SearchOption.AllDirectories).Length,models=CatalogEntries()};File.WriteAllText(Preview+"/models.json",JsonUtility.ToJson(catalog,true),new UTF8Encoding(false));}
        public static void ValidateGeometry(){ValidateGeometry(Materials());}
        static void ValidateGeometry(Material[] materials)
        {
            var failures=new List<string>();int count=0,max=0;float min=0;
            foreach(string id in AllIds())foreach(string state in id[0]=='C'?new[]{"1","2","3","4","5","Diseased","Wilted","Product"}:new[]{""})
            {
                GameObject model=null;Mesh mesh=null;
                try
                {
                    model=IslandExpansionModels.Create(id,state,materials);mesh=model.GetComponent<MeshFilter>().sharedMesh;count++;
                    if(mesh.vertices.Any(v=>float.IsNaN(v.sqrMagnitude)||float.IsInfinity(v.sqrMagnitude)))failures.Add(id+"_"+state+" nonfinite");
                    if(mesh.bounds.min.y<-.12f)failures.Add(id+"_"+state+" floor "+mesh.bounds.min.y);
                    if(model.GetComponent<Renderer>().sharedMaterials.Length!=mesh.subMeshCount)failures.Add(id+"_"+state+" material slots");
                    min=Mathf.Min(min,mesh.bounds.min.y);max=Mathf.Max(max,mesh.triangles.Length/3);
                }
                catch(Exception ex){failures.Add(id+"_"+state+" "+ex.Message);}
                finally{if(model!=null)Object.DestroyImmediate(model);if(mesh!=null)Object.DestroyImmediate(mesh);}
            }
            Directory.CreateDirectory(".tools/model-work");File.WriteAllText(".tools/model-work/expansion-geometry-result.json",JsonUtility.ToJson(new GeometryResult{status=failures.Count==0?"complete":"failed",models=count,maxTriangles=max,minY=min,failures=failures.ToArray()},true));
            if(failures.Count>0)throw new InvalidOperationException("Expansion geometry failed: "+string.Join("; ",failures));
        }
        [Serializable]sealed class GeometryResult{public string status;public int models,maxTriangles;public float minY;public string[] failures;}
        static void Render(ModelLibraryEntry[] entries,string mode)
        {
            if(ShaderUtil.anythingCompiling)throw new InvalidOperationException("Wait for shader compilation before expansion previews");
            Directory.CreateDirectory(Preview+"/Models");var scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D image=null,views=null;
            try
            {
                var sun=new GameObject("Expansion sun").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);sun.type=LightType.Directional;sun.intensity=1.15f;sun.color=new Color(1,.96f,.88f);sun.transform.rotation=Quaternion.Euler(48,-35,0);
                var fill=new GameObject("Expansion fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);fill.type=LightType.Directional;fill.intensity=.5f;fill.color=new Color(.86f,.94f,1);fill.transform.rotation=Quaternion.Euler(30,145,0);
                var camera=new GameObject("Expansion camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.cameraType=CameraType.Preview;camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);camera.orthographic=true;camera.allowHDR=false;camera.allowMSAA=false;camera.nearClipPlane=.01f;camera.farClipPlane=500;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("EEE6D5");camera.aspect=1;
                const int size=480;target=new RenderTexture(size,size,24,RenderTextureFormat.ARGB32);target.Create();image=new Texture2D(size,size,TextureFormat.RGB24,false);views=new Texture2D(size*3,size,TextureFormat.RGB24,false);
                Vector3[] angles={V(4,3.2f,5.5f),V(-5,3.5f,3),V(4,3.8f,-5)};
                foreach(var entry in entries)
                {
                    var prefabs=entry.variants.Select(v=>AssetDatabase.LoadAssetAtPath<GameObject>(v.prefab)).ToArray();float frame=.12f;
                    foreach(var prefab in prefabs){if(prefab==null)throw new InvalidOperationException("Missing preview prefab "+entry.id);var b=prefab.GetComponent<MeshFilter>().sharedMesh.bounds;foreach(var a in angles){var inv=Quaternion.Inverse(Quaternion.LookRotation(-a));for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){var p=inv*Vector3.Scale(b.extents,V(x,y,z));frame=Mathf.Max(frame,Mathf.Abs(p.x),Mathf.Abs(p.y));}}}
                    camera.orthographicSize=frame*1.14f;
                    for(int i=0;i<prefabs.Length;i++)
                    {
                        var model=(GameObject)PrefabUtility.InstantiatePrefab(prefabs[i],scene);
                        try
                        {
                            Bounds bounds=model.GetComponent<Renderer>().bounds;string name=Path.GetFileNameWithoutExtension(entry.variants[i].prefab)+(entry.id.StartsWith("IX_Product_")?"_Product":"");
                            for(int a=0;a<3;a++){camera.transform.position=bounds.center+angles[a]*Mathf.Max(.5f,frame);camera.transform.LookAt(bounds.center);RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});Read(target,views,a*size,0);if(a==0){Read(target,image,0,0);image.Apply();File.WriteAllBytes(Preview+"/Models/"+name+".png",image.EncodeToPNG());}}
                            views.Apply();File.WriteAllBytes(Preview+"/Models/"+name+"-views.png",views.EncodeToPNG());
                            bool icon=entry.id.StartsWith("IX_B")||entry.id.StartsWith("IX_D")||entry.id.StartsWith("IX_Product_")||i==4;
                            if(icon){string iconName=entry.id.StartsWith("IX_Product_")?"Product_"+entry.id.Substring(11):entry.id.Substring(3);string path=Root+"/Thumbnails/"+iconName+".png";File.Copy(Preview+"/Models/"+name+".png",path,true);AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=128;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();}
                        }
                        finally{Object.DestroyImmediate(model);}
                    }
                }
                WriteManifest();var catalog=new ModelLibraryCatalog{version=1,prefabCount=Directory.GetFiles(Root,"*.prefab",SearchOption.AllDirectories).Length,models=CatalogEntries()};foreach(var entry in catalog.models)foreach(var variant in entry.variants){variant.preview=variant.preview.Replace("../IslandExpansion/","");variant.views=variant.views.Replace("../IslandExpansion/","");}
                File.WriteAllText(Preview+"/index.html",File.ReadAllText("Tools/Templates/full-model-gallery.html",Encoding.UTF8).Replace("__MODEL_DATA__",JsonUtility.ToJson(catalog)),new UTF8Encoding(false));
                if(mode=="All"){Overview("B",30,5,"");Overview("C",30,5,"_5");Overview("D",60,6,"");Overview("C",30,5,"_Product");}
                File.WriteAllText(".tools/model-work/expansion-preview-"+mode.ToLowerInvariant()+".json","{\"status\":\"complete\",\"types\":"+entries.Length+",\"variants\":"+entries.Sum(e=>e.variants.Length)+",\"angles\":3}");
            }
            finally{if(target!=null){target.Release();Object.DestroyImmediate(target);}if(image!=null)Object.DestroyImmediate(image);if(views!=null)Object.DestroyImmediate(views);EditorSceneManager.ClosePreviewScene(scene);}
        }
        static Vector3 V(float x,float y,float z){return new Vector3(x,y,z);}
        static void Overview(string prefix,int count,int columns,string suffix)
        {
            const int tile=240;int rows=(count+columns-1)/columns;var sheet=new Texture2D(columns*tile,rows*tile,TextureFormat.RGB24,false);var source=new Texture2D(2,2);
            try
            {
                for(int i=0;i<count;i++)
                {
                    string path=Preview+"/Models/"+prefix+(i+1).ToString("00")+suffix+".png";if(!File.Exists(path))continue;
                    source.LoadImage(File.ReadAllBytes(path));var pixels=new Color[tile*tile];for(int y=0;y<tile;y++)for(int x=0;x<tile;x++)pixels[y*tile+x]=source.GetPixelBilinear((x+.5f)/tile,(y+.5f)/tile);
                    sheet.SetPixels(i%columns*tile,(rows-1-i/columns)*tile,tile,tile,pixels);
                }
                sheet.Apply();File.WriteAllBytes(Preview+"/Overview-"+prefix+suffix+".png",sheet.EncodeToPNG());
            }
            finally{Object.DestroyImmediate(sheet);Object.DestroyImmediate(source);}
        }
        static void Read(RenderTexture source,Texture2D dest,int x,int y){var previous=RenderTexture.active;try{RenderTexture.active=source;dest.ReadPixels(new Rect(0,0,source.width,source.height),x,y,false);}finally{RenderTexture.active=previous;}}
    }
}
