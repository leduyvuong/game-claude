using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace HarvestIsle.Editor
{
    public static class HumanRefresh
    {
        public static void Bake()
        {
            EditorSettings.asyncShaderCompilation = false;
            Shader.WarmupAllShaders();
            FarmerAssets.RebuildAndPreview(); FarmerFemaleAssets.RebuildAndPreview();
            Sync();
            CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),false,new[]{"Farmer","FarmerFemale","ElderTu","MerchantHaiAu"});
            HumanHairReview.After();
            File.WriteAllText(".tools/model-work/humans-result.json","{\"status\":\"complete\",\"prefabs\":5}");
        }
        public static void Sync()
        {
            if(AssetDatabase.LoadAssetAtPath<GameObject>(FarmerAssets.PrefabPath)==null||AssetDatabase.LoadAssetAtPath<GameObject>(FarmerFemaleAssets.PrefabPath)==null)return;
            const string geometry="Assets/HarvestIsle/Art/Farmer/VillageHumanDetails.asset";
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(geometry);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,geometry);}
            var oldMeshes=data.meshes;var oldMaterials=data.materials;
            var detailMeshes=new List<Mesh>();var detailMaterials=StructureModelBuilder.Materials();
            foreach(var material in detailMaterials)AssetDatabase.AddObjectToAsset(material,data);
            foreach(string id in new[]{"ElderTu","MerchantHaiAu","FarmerFemale"})
            {
                string source=id=="FarmerFemale"?FarmerFemaleAssets.PrefabPath:FarmerAssets.PrefabPath;
                var model=id=="FarmerFemale"?UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source)):StructureModelBuilder.CreateExtra(id,detailMaterials,detailMeshes);
                foreach(var mesh in detailMeshes)if(!AssetDatabase.Contains(mesh))AssetDatabase.AddObjectToAsset(mesh,data);
                model.name=id;
                string destination="Assets/HarvestIsle/Resources/Collection/"+id+".prefab";
                // Share the baked articulated geometry; only the villagers' outfit materials differ.
                var palette=new Dictionary<Material,Material>();
                foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    var slots=renderer.sharedMaterials;
                    for(int i=0;i<slots.Length;i++)
                    {
                        var mat=slots[i];Color c=mat.color;
                        bool shirt=c.g>c.r*1.12f&&c.g>c.b*1.05f;
                        bool hair=id=="ElderTu"&&c.r<.28f&&c.g<.23f&&c.b<.19f;
                        if(id=="FarmerFemale"||!shirt&&!hair)continue;
                        if(!palette.TryGetValue(mat,out var replacement))
                        {
                            string path="Assets/HarvestIsle/Art/Farmer/"+id+"-"+ColorUtility.ToHtmlStringRGB(c)+".mat";
                            replacement=AssetDatabase.LoadAssetAtPath<Material>(path);
                            if(replacement==null){replacement=new Material(mat);AssetDatabase.CreateAsset(replacement,path);}
                            replacement.color=FarmWorld.Hex(hair?"B7B4A5":id=="ElderTu"?"A18761":"6F9EB5");
                            EditorUtility.SetDirty(replacement);palette[mat]=replacement;
                        }
                        slots[i]=replacement;
                    }
                    renderer.sharedMaterials=slots;
                }
                PrefabUtility.SaveAsPrefabAsset(model,destination);
                UnityEngine.Object.DestroyImmediate(model);
            }
            data.meshes=detailMeshes.ToArray();data.materials=detailMaterials;data.version=1;EditorUtility.SetDirty(data);
            if(oldMeshes!=null)foreach(var mesh in oldMeshes)if(mesh!=null)UnityEngine.Object.DestroyImmediate(mesh,true);
            if(oldMaterials!=null)foreach(var material in oldMaterials)if(material!=null)UnityEngine.Object.DestroyImmediate(material,true);
            AssetDatabase.SaveAssets();
        }
    }
}
