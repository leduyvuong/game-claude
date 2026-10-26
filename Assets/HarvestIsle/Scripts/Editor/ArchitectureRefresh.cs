using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class ArchitectureRefresh
    {
        [MenuItem("Harvest Isle/Structures/Bake detailed architecture")]
        public static void Build()
        {
            File.WriteAllText(".tools/model-work/architecture-result.json","{\"status\":\"running\"}");
            const string path="Assets/HarvestIsle/Art/Structures/ArchitectureCollection.asset";
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(path);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,path);}
            var oldMeshes=data.meshes;var oldMaterials=data.materials;
            var meshes=new List<Mesh>();var mats=StructureModelBuilder.Materials();
            var models=new Dictionary<string,GameObject>();var guids=new Dictionary<string,string>();
            int before=Directory.GetFiles("Assets/HarvestIsle/Resources","*.prefab",SearchOption.AllDirectories).Length;
            try
            {
                foreach(string type in StructureModelBuilder.ArchitectureTypes)
                {
                    bool native=Array.IndexOf(BuildingLevelView.Types,type)>=0;
                    bool production=Array.IndexOf(ProductionDisplay.Types,type)>=0;
                    string basePath=native?StructureAssets.PrefabPath(type):production?ProductionAssets.PrefabPath(type):"Assets/HarvestIsle/Resources/Collection/"+type+".prefab";
                    models.Add(basePath,native?StructureModelBuilder.Create(type,mats,meshes):production?StructureModelBuilder.CreateProduction(type,mats,meshes):StructureModelBuilder.CreateExtra(type,mats,meshes));
                    for(int level=1;level<=3;level++)
                        models.Add(native?BuildingLevelAssets.PrefabPath(type,level):"Assets/HarvestIsle/Resources/Collection/Levels/"+type+"_L"+level+".prefab",native?StructureModelBuilder.CreateLevel(type,level,mats,meshes):StructureModelBuilder.CreateExpandedLevel(type,level,mats,meshes));
                }
                foreach(var pair in models)
                {
                    if(!File.Exists(pair.Key))throw new InvalidOperationException("Refusing new prefab: "+pair.Key);
                    guids.Add(pair.Key,AssetDatabase.AssetPathToGUID(pair.Key));
                    foreach(var r in pair.Value.GetComponentsInChildren<Renderer>())if(r.sharedMaterials.Any(m=>m==null))throw new InvalidOperationException("Missing material "+pair.Key);
                }
                foreach(var mesh in meshes)
                {
                    if(mesh.vertexCount==0||mesh.vertices.Any(p=>float.IsNaN(p.sqrMagnitude)||float.IsInfinity(p.sqrMagnitude)))throw new InvalidOperationException("Invalid geometry "+mesh.name);
                    AssetDatabase.AddObjectToAsset(mesh,data);
                }
                foreach(var mat in mats)AssetDatabase.AddObjectToAsset(mat,data);
                data.meshes=meshes.ToArray();data.materials=mats;data.version=1;EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
                foreach(var pair in models)
                {
                    if(PrefabUtility.SaveAsPrefabAsset(pair.Value,pair.Key)==null||AssetDatabase.AssetPathToGUID(pair.Key)!=guids[pair.Key])throw new InvalidOperationException("Prefab persistence: "+pair.Key);
                }
                if(oldMeshes!=null)foreach(var mesh in oldMeshes)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(oldMaterials!=null)foreach(var mat in oldMaterials)if(mat!=null)Object.DestroyImmediate(mat,true);
                AssetDatabase.SaveAssets();
                if(before!=Directory.GetFiles("Assets/HarvestIsle/Resources","*.prefab",SearchOption.AllDirectories).Length)throw new InvalidOperationException("Catalog count changed");
                BakeShopRoof();
                CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),false,StructureModelBuilder.ArchitectureTypes);
                DevelopmentUiPreview.BuildThumbnails();
                int max=models.Values.Max(go=>go.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3));
                File.WriteAllText(".tools/model-work/architecture-result.json","{\"status\":\"complete\",\"types\":19,\"prefabs\":"+models.Count+",\"guidsPreserved\":true,\"resources\":"+before+",\"maxTriangles\":"+max+"}");
                Debug.Log("ARCHITECTURE_BAKED: "+models.Count+" existing prefabs; max triangles "+max);
            }
            catch(Exception e){File.WriteAllText(".tools/model-work/architecture-result.json",JsonUtility.ToJson(new Failure{error=e.ToString()},true));throw;}
            finally{foreach(var go in models.Values)Object.DestroyImmediate(go);}
        }
        private static void BakeShopRoof()
        {
            const string folder="Assets/HarvestIsle/Resources/Architecture",path=folder+"/ShopRoof.asset";
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(path);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,path);}
            var oldMeshes=data.meshes;var oldMaterials=data.materials;
            var mesh=StructureModelBuilder.DetailedShopRoof();var mats=StructureModelBuilder.Materials();
            AssetDatabase.AddObjectToAsset(mesh,data);foreach(var mat in mats)AssetDatabase.AddObjectToAsset(mat,data);
            data.meshes=new[]{mesh};data.materials=mats;data.version=2;EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
            if(oldMeshes!=null)foreach(var old in oldMeshes)if(old!=null)Object.DestroyImmediate(old,true);
            if(oldMaterials!=null)foreach(var old in oldMaterials)if(old!=null)Object.DestroyImmediate(old,true);
            AssetDatabase.SaveAssets();
        }
        [Serializable]private sealed class Failure{public string status="failed",error;}
    }
}
