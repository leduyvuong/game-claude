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
    public static class CompleteModelAssets
    {
        private const string AssetPath="Assets/HarvestIsle/Art/Structures/CompleteCollection.asset";
        private const string Folder="Art/Previews/Library";
        private const string CatalogPath="Assets/HarvestIsle/Resources/FullModelCatalog.json";
        private static readonly string[] Growth={"Mầm non","Cây non","Phát triển thân lá","Hoa / quả non / gần trưởng thành","Sẵn sàng thu hoạch"};
        [MenuItem("Harvest Isle/Library/Rebuild complete model collection")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();Bake();RenderCatalog(BuildCatalog());PlaygroundSetup.AddLibrary();
        }
        [MenuItem("Harvest Isle/Crops/Rebuild all five-stage reference crops")]
        public static void RebuildReferenceCrops()
        {
            // Health states must be rebuilt after BOTH healthy crop material assets.
            RiceAssets.RebuildAndPreview();
            CropCollectionAssets.RebuildAndPreview();
            Bake();
            RenderCatalog(BuildCatalog(), false, CompleteModelDefinitions.ExistingCrops.Concat(CompleteCropBuilder.Names).ToArray());
            CropGrowthShowcaseAssets.Build();
        }
        internal static void Bake()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));Directory.CreateDirectory(Path.GetDirectoryName(CompleteModelDefinitions.ExtraPath("Hoe")));Directory.CreateDirectory(Path.GetDirectoryName(CompleteModelDefinitions.LevelPath("Windmill",1)));
            foreach(string name in CompleteCropBuilder.Names)Directory.CreateDirectory(Path.GetDirectoryName(CropCollectionAssets.PrefabPath(name,1)));
            AssetDatabase.Refresh();var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(AssetPath);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,AssetPath);}
            var oldMeshes=AssetDatabase.LoadAllAssetsAtPath(AssetPath).OfType<Mesh>().ToArray();var oldMaterials=AssetDatabase.LoadAllAssetsAtPath(AssetPath).OfType<Material>().ToArray();var meshes=new List<Mesh>();var materials=new List<Material>();var models=new List<KeyValuePair<string,GameObject>>();
            var structureMats=StructureModelBuilder.Materials();var cropMats=CropCollectionBuilder.Materials();materials.AddRange(structureMats);materials.AddRange(cropMats);
            try
            {
                foreach(string type in CompleteCropBuilder.Names)for(int stage=1;stage<=5;stage++)
                {Mesh mesh;var model=CompleteCropBuilder.Create(type,stage,cropMats,out mesh);meshes.Add(mesh);models.Add(new KeyValuePair<string,GameObject>(CropCollectionAssets.PrefabPath(type,stage),model));}
                foreach(string specification in CompleteModelDefinitions.Props.Concat(CompleteModelDefinitions.Supplements))
                {
                    string type=specification.Split('|')[0];var model=StructureModelBuilder.CreateExtra(type,structureMats,meshes);
                    if(type=="ElderTu"||type=="MerchantHaiAu"||type=="FarmerFemale")RecolorHuman(model,type,materials);
                    models.Add(new KeyValuePair<string,GameObject>(CompleteModelDefinitions.ExtraPath(type),model));
                }
                var cropNames=CompleteModelDefinitions.ExistingCrops.Concat(CompleteCropBuilder.Names).ToArray();
                foreach(string crop in cropNames)models.Add(new KeyValuePair<string,GameObject>(CompleteModelDefinitions.ExtraPath("Product_"+crop),StructureModelBuilder.CreateExtra("Product_"+crop,structureMats,meshes)));
                foreach(string spec in CompleteModelDefinitions.Processed){string type="Product_"+spec.Split('|')[0];models.Add(new KeyValuePair<string,GameObject>(CompleteModelDefinitions.ExtraPath(type),StructureModelBuilder.CreateExtra(type,structureMats,meshes)));}
                foreach(string type in ProductionDisplay.Types.Concat(CompleteModelDefinitions.LevelledDefense))for(int level=1;level<=3;level++)
                    models.Add(new KeyValuePair<string,GameObject>(CompleteModelDefinitions.LevelPath(type,level),StructureModelBuilder.CreateExpandedLevel(type,level,structureMats,meshes)));
                // Save healthy plants first so state variants always derive from the current crop geometry.
                foreach(var model in models)foreach(Renderer renderer in model.Value.GetComponentsInChildren<Renderer>(true))foreach(Material mat in renderer.sharedMaterials)if(mat!=null&&!AssetDatabase.Contains(mat)&&!materials.Contains(mat))materials.Add(mat);
                StoreGeometry(data,meshes,materials);foreach(var model in models)Save(model.Key,model.Value);
                foreach(string crop in cropNames)foreach(string state in new[]{"Diseased","Wilted"})
                {
                    var model=CreateState(crop,state,meshes,materials,structureMats);models.Add(new KeyValuePair<string,GameObject>(CompleteModelDefinitions.StatePath(crop,state),model));
                }
                StoreGeometry(data,meshes,materials);foreach(var model in models.Skip(models.Count-60))Save(model.Key,model.Value);
                if(oldMeshes!=null)foreach(Mesh mesh in oldMeshes)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(oldMaterials!=null)foreach(Material mat in oldMaterials)if(mat!=null)Object.DestroyImmediate(mat,true);
                AssetDatabase.SaveAssets();Debug.Log("COMPLETE_MODELS_BAKED: "+models.Count+" new prefabs.");
            }
            finally{foreach(var model in models)if(model.Value!=null)Object.DestroyImmediate(model.Value);}
        }
        private static void StoreGeometry(NatureModelAsset data,List<Mesh> meshes,List<Material> materials)
        {
            foreach(Mesh mesh in meshes)if(!AssetDatabase.Contains(mesh))AssetDatabase.AddObjectToAsset(mesh,data);
            foreach(Material mat in materials)if(!AssetDatabase.Contains(mat))AssetDatabase.AddObjectToAsset(mat,data);
            data.version=1;data.meshes=meshes.ToArray();data.materials=materials.ToArray();EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
        }
        private static void Save(string path,GameObject model)
        {if(PrefabUtility.SaveAsPrefabAsset(model,path)==null)throw new InvalidOperationException("Failed to save model "+path);}
        private static void RecolorHuman(GameObject model,string type,List<Material> materials)
        {
            var lookup=new Dictionary<Material,Material>();foreach(Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                var slots=renderer.sharedMaterials;for(int i=0;i<slots.Length;i++)
                {
                    Material original=slots[i];if(original==null||!original.HasProperty("_BaseColor"))continue;Color c=original.GetColor("_BaseColor");
                    bool shirt=c.g>c.r*1.12f&&c.g>c.b*1.05f, hair=c.r<.28f&&c.g<.23f&&c.b<.19f;
                    if(!shirt&&!(type=="ElderTu"&&hair))continue;
                    if(!lookup.TryGetValue(original,out Material replacement))
                    {replacement=new Material(original){name=type+" "+original.name};replacement.SetColor("_BaseColor",FarmWorld.Hex(hair?"B7B4A5":type=="ElderTu"?"8A7557":type=="MerchantHaiAu"?"607E9A":"B16B77"));lookup.Add(original,replacement);materials.Add(replacement);}slots[i]=replacement;
                }renderer.sharedMaterials=slots;
            }
        }
        private static GameObject CreateState(string crop,string state,List<Mesh> meshes,List<Material> materials,Material[] structureMats)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(CropCollectionAssets.PrefabPath(crop,5));if(prefab==null)throw new InvalidOperationException("Missing mature crop: "+crop);
            var root=Object.Instantiate(prefab);root.name=crop+"_"+state;var colors=new Dictionary<Material,Material>();var spots=new EnvironmentMesh(16);int spotCount=0;
            foreach(MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var original=filter.sharedMesh;var copy=Object.Instantiate(original);copy.name=root.name+" "+original.name;
                var vertices=copy.vertices;if(state=="Wilted")for(int i=0;i<vertices.Length;i++){Vector3 p=vertices[i];p.y=p.y>0?p.y*.64f:p.y;p.x*=1.06f;p.z*=1.06f;vertices[i]=p;}copy.vertices=vertices;copy.RecalculateBounds();copy.RecalculateNormals();filter.sharedMesh=copy;meshes.Add(copy);
                var renderer=filter.GetComponent<Renderer>();var slots=renderer.sharedMaterials;
                for(int materialIndex=0;materialIndex<slots.Length;materialIndex++)
                {
                    Material mat=slots[materialIndex];if(mat==null||!mat.HasProperty("_BaseColor"))continue;Color c=mat.GetColor("_BaseColor");bool green=(c.g>c.r*1.08f&&c.g>c.b*1.08f)||(crop=="Mushroom"&&c.r>c.g*1.15f);
                    if(!green)continue;
                    if(!colors.TryGetValue(mat,out Material changed)){changed=new Material(mat){name=root.name+" foliage"};changed.SetColor("_BaseColor",FarmWorld.Hex(state=="Wilted"?"A59159":"86974D"));colors.Add(mat,changed);materials.Add(changed);}slots[materialIndex]=changed;
                    if(state=="Diseased"&&materialIndex<copy.subMeshCount)
                    {int[] indices=copy.GetTriangles(materialIndex);for(int i=0;i<indices.Length&&spotCount<22;i+=Mathf.Max(3,indices.Length/10/3*3))
                        {Vector3 p=filter.transform.TransformPoint(vertices[indices[i]])+Vector3.up*.003f;float radius=Mathf.Clamp(copy.bounds.size.magnitude*.006f,.006f,.015f);spots.Disc(p,radius,radius,3,8);spotCount++;}}
                }renderer.sharedMaterials=slots;foreach(MeshCollider collider in filter.GetComponents<MeshCollider>())collider.sharedMesh=copy;
            }
            if(spotCount>0){var mesh=spots.Finish(root.name+" leaf spots");meshes.Add(mesh);var detail=new GameObject("Disease spots",typeof(MeshFilter),typeof(MeshRenderer));detail.transform.SetParent(root.transform,false);detail.GetComponent<MeshFilter>().sharedMesh=mesh;detail.GetComponent<MeshRenderer>().sharedMaterials=structureMats.Take(16).ToArray();}
            return root;
        }
        internal static ModelLibraryCatalog BuildCatalog()
        {
            var entries=new List<ModelLibraryEntry>();string[] crops=CompleteModelDefinitions.ExistingCrops.Concat(CompleteCropBuilder.Names).ToArray(),labels=CompleteModelDefinitions.ExistingCropLabels.Concat(CompleteCropBuilder.Labels).ToArray();
            for(int i=0;i<crops.Length;i++)
            {var variants=new List<ModelLibraryVariant>();for(int stage=1;stage<=5;stage++)variants.Add(Variant(Growth[stage-1],CropCollectionAssets.PrefabPath(crops[i],stage)));variants.Add(Variant("Bị bệnh",CompleteModelDefinitions.StatePath(crops[i],"Diseased")));variants.Add(Variant("Héo",CompleteModelDefinitions.StatePath(crops[i],"Wilted")));entries.Add(Entry(crops[i],labels[i],i>=24?"Cây ăn quả":"Cây trồng",variants.ToArray()));}
            entries.Add(Entry("Farmer","Nông dân","Nhân vật",Variant("Mẫu gốc",FarmerAssets.PrefabPath)));
            entries.Add(Entry("CoconutPalm","Dừa trang trí","Thiên nhiên",Variant("Trưởng thành","Assets/HarvestIsle/Resources/Nature/CoconutPalm.prefab")));
            for(int i=0;i<AnimalModelBuilder.Names.Length;i++)entries.Add(Entry(AnimalModelBuilder.Names[i],AnimalModelBuilder.Labels[i],"Vật nuôi",Variant("Trưởng thành",AnimalAssets.PrefabPath(AnimalModelBuilder.Names[i]))));
            for(int i=0;i<EnvironmentModelBuilder.Names.Length;i++)entries.Add(Entry(EnvironmentModelBuilder.Names[i],EnvironmentModelBuilder.Labels[i],"Địa hình",Variant("Mẫu gốc",EnvironmentAssets.PrefabPath(EnvironmentModelBuilder.Names[i]))));
            for(int i=0;i<StructureModelBuilder.Names.Length;i++)
            {string type=StructureModelBuilder.Names[i];ModelLibraryVariant[] variants=Array.IndexOf(BuildingLevelView.Types,type)>=0?Enumerable.Range(1,3).Select(level=>Variant("Cấp "+level,BuildingLevelAssets.PrefabPath(type,level))).ToArray():new[]{Variant("Mẫu gốc",StructureAssets.PrefabPath(type))};entries.Add(Entry(type,StructureModelBuilder.Labels[i],"Công trình",variants));}
            for(int i=0;i<ProductionDisplay.Types.Length;i++){string type=ProductionDisplay.Types[i];entries.Add(Entry(type,ProductionDisplay.Labels[i],"Sản xuất và giao thương",Enumerable.Range(1,3).Select(level=>Variant("Cấp "+level,CompleteModelDefinitions.LevelPath(type,level))).ToArray()));}
            foreach(string specification in CompleteModelDefinitions.Props.Concat(CompleteModelDefinitions.Supplements))
            {string[] fields=specification.Split('|');var variants=Array.IndexOf(CompleteModelDefinitions.LevelledDefense,fields[0])>=0?Enumerable.Range(1,3).Select(level=>Variant("Cấp "+level,CompleteModelDefinitions.LevelPath(fields[0],level))).ToArray():new[]{Variant("Mẫu gốc",CompleteModelDefinitions.ExtraPath(fields[0]))};entries.Add(Entry(fields[0],fields[1],fields[2],variants));}
            for(int i=0;i<crops.Length;i++)entries.Add(Entry("Product_"+crops[i],labels[i]+" thu hoạch","Nông sản",Variant("Sản phẩm",CompleteModelDefinitions.ExtraPath("Product_"+crops[i]))));
            foreach(string specification in CompleteModelDefinitions.Processed){string[] f=specification.Split('|');entries.Add(Entry("Product_"+f[0],f[1],"Thành phẩm",Variant("Sản phẩm",CompleteModelDefinitions.ExtraPath("Product_"+f[0]))));}
            for(int i=8;i<HarvestIsle.Core.HotpotFurnitureCatalog.Items.Length;i++)
            {
                var spec=HarvestIsle.Core.HotpotFurnitureCatalog.Items[i];string path=HotpotFurnitureAssets.PrefabPath(i);
                if(File.Exists(path))entries.Add(Entry("HP_"+spec.id,spec.name,"Quán lẩu · "+HarvestIsle.Core.HotpotFurnitureCatalog.Categories[spec.category],Variant("Mẫu gốc",path)));
            }
            for(int i=0;i<48;i++)
            {var spec=HotpotKitchenCatalog.Items[i];string path=HotpotKitchenAssets.PrefabPath(i);if(File.Exists(path))entries.Add(Entry(spec.id,spec.label,HotpotKitchenCatalog.Groups[(int)spec.role],Variant("Mẫu gốc",path)));}
            if(File.Exists(HotpotKitchenAssets.PrefabPath(55)))entries.Add(Entry("HP_KC_Kitchen","Bếp lẩu tám cấp",HotpotKitchenCatalog.Groups[3],Enumerable.Range(1,8).Select(level=>Variant("Cấp "+level,HotpotKitchenAssets.PrefabPath(47+level))).ToArray()));
            entries.AddRange(IslandExpansionAssets.CatalogEntries());
            int count=Directory.GetFiles("Assets/HarvestIsle/Resources","*.prefab",SearchOption.AllDirectories).Length;
            return new ModelLibraryCatalog{version=1,prefabCount=count,models=entries.ToArray()};
        }
        private static ModelLibraryEntry Entry(string id,string label,string group,params ModelLibraryVariant[] variants){return new ModelLibraryEntry{id=id,label=label,group=group,variants=variants};}
        private static ModelLibraryVariant Variant(string label,string path){return new ModelLibraryVariant{label=label,prefab=path,resource=path.Substring("Assets/HarvestIsle/Resources/".Length).Replace(".prefab","")};}
        internal static void RenderCatalog(ModelLibraryCatalog catalog,bool environmentOnly=false,string[] changedIds=null)
        {
            var previous=(environmentOnly||changedIds!=null)&&File.Exists(CatalogPath)?JsonUtility.FromJson<ModelLibraryCatalog>(File.ReadAllText(CatalogPath)):null;
            Directory.CreateDirectory(Folder);Scene scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D image=null,views=null;
            try
            {
                var sun=new GameObject("Library sun").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);sun.type=LightType.Directional;sun.intensity=1.2f;sun.color=new Color(1,.96f,.87f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(45,-30,0);
                var fill=new GameObject("Library fill").AddComponent<Light>();SceneManager.MoveGameObjectToScene(fill.gameObject,scene);fill.type=LightType.Directional;fill.intensity=.48f;fill.color=new Color(.84f,.92f,1);fill.transform.rotation=Quaternion.Euler(30,140,0);
                var camera=new GameObject("Library camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(camera.gameObject,scene);camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.cameraType=CameraType.Preview;camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);camera.orthographic=true;camera.allowHDR=false;camera.allowMSAA=false;camera.nearClipPlane=.01f;camera.farClipPlane=1200;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FarmWorld.Hex("F2EDE0");
                const int width=640,height=640;target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();camera.aspect=1;
                image=new Texture2D(width,height,TextureFormat.RGB24,false);views=new Texture2D(width*3,height,TextureFormat.RGB24,false);int count=0;
                foreach(ModelLibraryEntry entry in catalog.models)
                {
                    var old=previous==null?null:Array.Find(previous.models,e=>e.id==entry.id);
                    if((environmentOnly&&Array.IndexOf(EnvironmentModelBuilder.Names,entry.id)<0||changedIds!=null&&Array.IndexOf(changedIds,entry.id)<0)&&old!=null&&old.variants.Length==entry.variants.Length)
                    {entry.variants=old.variants;count+=entry.variants.Length;continue;}
                    var models=new List<GameObject>();var bounds=new List<Bounds>();var snapshots=new List<HumanPoseSnapshot>();
                    try
                    {
                        foreach(ModelLibraryVariant variant in entry.variants)
                        {
                            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(variant.prefab);if(prefab==null)throw new InvalidOperationException("Catalog prefab missing "+variant.prefab);
                            var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);models.Add(model);if(model.GetComponent<FarmerAvatar>()!=null){model.GetComponent<FarmerAvatar>().SetPose(0,0,0,HarvestIsle.Core.FarmTool.Plant);snapshots.Add(new HumanPoseSnapshot(model));}var rs=model.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();if(rs.Length==0)throw new InvalidOperationException("Catalog model empty "+entry.id);Bounds b=rs[0].bounds;foreach(Renderer r in rs){b.Encapsulate(r.bounds);foreach(Material mat in r.sharedMaterials)if(mat==null)throw new InvalidOperationException("Missing material "+entry.id);}bounds.Add(b);
                            variant.triangles=0;foreach(MeshFilter f in model.GetComponentsInChildren<MeshFilter>()){if(f.sharedMesh==null)throw new InvalidOperationException("Missing mesh "+entry.id);variant.triangles+=f.sharedMesh.triangles.Length/3;foreach(Vector3 p in f.sharedMesh.vertices)if(!Finite(p.x)||!Finite(p.y)||!Finite(p.z))throw new InvalidOperationException("Nonfinite mesh "+entry.id);}
                            if(!Finite(b.size.sqrMagnitude)||b.size.sqrMagnitude==0)throw new InvalidOperationException("Invalid bounds "+entry.id);model.SetActive(false);
                        }
                        Vector3[] angles=entry.id.StartsWith("HP_")?new[]{new Vector3(4,3.4f,-5.5f),new Vector3(-5,3.7f,-3),new Vector3(4,4.4f,5)}:new[]{new Vector3(4,3.4f,5.5f),new Vector3(-5,3.7f,3),new Vector3(4,4.4f,-5)};float frame=.10f;
                        foreach(Bounds b in bounds)foreach(Vector3 angle in angles){Quaternion inv=Quaternion.Inverse(Quaternion.LookRotation(-angle));for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){Vector3 p=inv*Vector3.Scale(b.extents,new Vector3(x,y,z));frame=Mathf.Max(frame,Mathf.Abs(p.x),Mathf.Abs(p.y));}}
                        camera.orthographicSize=frame*1.12f;
                        for(int v=0;v<models.Count;v++)
                        {
                            GameObject model=models[v];model.SetActive(true);ModelLibraryVariant variant=entry.variants[v];variant.preview=entry.id+"_"+(v+1)+".png";variant.views=entry.id+"_"+(v+1)+"-views.png";
                            for(int a=0;a<3;a++)
                            {
                                camera.transform.position=bounds[v].center+angles[a]*Mathf.Max(.5f,frame);camera.transform.LookAt(bounds[v].center);
                                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});Read(target,views,a*width,0);
                                if(a==0){Read(target,image,0,0);image.Apply();File.WriteAllBytes(Folder+"/"+variant.preview,image.EncodeToPNG());}
                            }
                            views.Apply();File.WriteAllBytes(Folder+"/"+variant.views,views.EncodeToPNG());model.SetActive(false);count++;
                        }
                        Debug.Log("LIBRARY_PREVIEW: "+entry.id+" ("+entry.variants.Length+" variants)");
                    }
                    finally{foreach(var snapshot in snapshots)snapshot.Dispose();foreach(GameObject model in models)Object.DestroyImmediate(model);}
                }
                string json=JsonUtility.ToJson(catalog,true);File.WriteAllText(Folder+"/models.json",json,new UTF8Encoding(false));File.WriteAllText(CatalogPath,json,new UTF8Encoding(false));
                File.WriteAllText(Folder+"/index.html",File.ReadAllText("Tools/Templates/full-model-gallery.html",Encoding.UTF8).Replace("__MODEL_DATA__",json),new UTF8Encoding(false));AssetDatabase.Refresh();
                ModelShopThumbnails.Build();
                Debug.Log("FULL_LIBRARY_VERIFIED: "+catalog.models.Length+" model types, "+count+" variants, "+count*3+" views, "+catalog.prefabCount+" project prefabs.");
            }
            finally{if(target!=null){target.Release();Object.DestroyImmediate(target);}if(image!=null)Object.DestroyImmediate(image);if(views!=null)Object.DestroyImmediate(views);EditorSceneManager.ClosePreviewScene(scene);}
        }
        private static bool Finite(float x){return !float.IsNaN(x)&&!float.IsInfinity(x);}
        private static void Read(RenderTexture source,Texture2D dest,int x,int y){RenderTexture previous=RenderTexture.active;try{RenderTexture.active=source;dest.ReadPixels(new Rect(0,0,source.width,source.height),x,y,false);}finally{RenderTexture.active=previous;}}
        [MenuItem("Harvest Isle/Library/Open complete model gallery")]
        public static void OpenGallery(){string path=Path.GetFullPath(Folder+"/index.html");if(!File.Exists(path))RebuildAndPreview();Application.OpenURL(new Uri(path).AbsoluteUri);}
    }
}
