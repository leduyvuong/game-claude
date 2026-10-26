using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class HotpotKitchenAssets
    {
        const string Art="Assets/HarvestIsle/Art/HotpotKitchen",Preview="Art/Previews/HotpotKitchen";
        public static string PrefabPath(int index){return "Assets/HarvestIsle/Resources/"+HotpotKitchenCatalog.Items[index].Resource+".prefab";}
        [MenuItem("Harvest Isle/Hotpot/Bake ingredients dishes and eight kitchens")]
        public static void RebuildAndPreview()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Bake in Edit Mode");
            Directory.CreateDirectory(Art);Directory.CreateDirectory("Assets/HarvestIsle/Resources/HotpotKitchen");Directory.CreateDirectory(Preview);AssetDatabase.Refresh();
            var materials=HotpotFurnitureAssets.Materials();
            foreach(string name in new[]{"Steel","Gold"})
            {
                string path=Art+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(materials[name]){name=name};AssetDatabase.CreateAsset(mat,path);}
                mat.SetFloat("_Smoothness",.32f);mat.SetFloat("_Metallic",.45f);mat.SetFloat("_EnvironmentReflections",0);EditorUtility.SetDirty(mat);materials[name]=mat;
            }
            string[] colors={"RawChicken|ECA08B","FishFlesh|EDD8B7","Pork|EFAC9C","Shrimp|EB7935","ShrimpLight|F6C09A","Corn|F4B938","CornCore|F4DA96","Tofu|F6DFAC","Mushroom|795035","Clay|965C32","Soup|EBD09B","Chili|842314","Noodle|E5AD3C","Cooktop|171D22","Fat|F9D4BF","Squid|E5BCAE","CabbageStem|DFE7B4","CabbageLeaf|CCD26F","TomatoFlesh|DD5930","Shell|294A40","Lime|A8BE45","ThaiSoup|D48A3D","CookedChicken|D8B378","Goat|987552","Taro|B59A97","Crab|C88C49","Sage|6C7860","Burgundy|793E32","Flame|4DADFF"};
            foreach(var spec in colors)
            {
                var f=spec.Split('|');string path=Art+"/"+f[0]+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=f[0]};AssetDatabase.CreateAsset(mat,path);}
                mat.color=FarmWorld.Hex(f[1]);mat.SetFloat("_Smoothness",f[0]=="Cooktop"?.7f:.28f);mat.enableInstancing=true;
                if(f[0]=="Cooktop"){mat.SetFloat("_Smoothness",.15f);mat.SetFloat("_SpecularHighlights",0);mat.SetFloat("_EnvironmentReflections",0);}
                if(f[0]=="Flame"){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",mat.color*.5f);}
                EditorUtility.SetDirty(mat);materials.Add(f[0],mat);
            }
            var builder=new HotpotFurnitureBuilder(materials);
            try
            {
                for(int i=0;i<HotpotKitchenCatalog.Items.Length;i++)
                {
                    var model=builder.CreateKitchen(i);
                    try
                    {
                        var view=model.GetComponent<HotpotKitchenItem>();HotpotFurnitureAssets.Merge(model.transform.Find("Structure"),model.name+"_Body",Art);
                        if(view.contents!=null)HotpotFurnitureAssets.Merge(view.contents,model.name+"_Contents",Art);
                        var renderers=model.GetComponentsInChildren<MeshRenderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                        var collider=model.AddComponent<BoxCollider>();collider.center=bounds.center;collider.size=bounds.size;
                        if(PrefabUtility.SaveAsPrefabAsset(model,PrefabPath(i))==null)throw new InvalidOperationException("Save failed "+model.name);
                    }
                    finally{Object.DestroyImmediate(model);}
                }
                AssetDatabase.SaveAssets();
            }
            finally{builder.Dispose();}
            var changed=HotpotKitchenCatalog.Items.Take(48).Select(x=>x.id).Concat(new[]{"HP_KC_Kitchen"}).ToArray();
            CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),false,changed);
            var all=JsonUtility.FromJson<ModelLibraryCatalog>(File.ReadAllText("Art/Previews/Library/models.json",Encoding.UTF8));
            var catalog=new ModelLibraryCatalog{version=1,prefabCount=all.prefabCount,models=all.models.Where(x=>changed.Contains(x.id)).ToArray()};
            Directory.CreateDirectory("Assets/HarvestIsle/Resources/HotpotKitchenThumbnails");
            var variants=catalog.models.SelectMany(x=>x.variants).ToDictionary(v=>v.resource);
            for(int i=0;i<56;i++)
            {
                var spec=HotpotKitchenCatalog.Items[i];string path="Assets/HarvestIsle/Resources/HotpotKitchenThumbnails/"+spec.id+".png";
                File.Copy("Art/Previews/Library/"+variants[spec.Resource].preview,path,true);AssetDatabase.ImportAsset(path);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=128;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            }
            Overview(0,36,6,240,"Ingredients-tools",variants);Overview(36,12,4,320,"Dishes",variants);Overview(48,8,4,400,"Kitchen-levels",variants);
            foreach(var entry in catalog.models)foreach(var v in entry.variants){v.preview="../Library/"+v.preview;v.views="../Library/"+v.views;}
            string json=JsonUtility.ToJson(catalog,true);File.WriteAllText(Preview+"/models.json",json,new UTF8Encoding(false));
            File.WriteAllText(Preview+"/index.html",File.ReadAllText("Tools/Templates/full-model-gallery.html",Encoding.UTF8).Replace("__MODEL_DATA__",json),new UTF8Encoding(false));
            VerifyAssets();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        static void Overview(int start,int count,int columns,int tile,string name,Dictionary<string,ModelLibraryVariant> variants)
        {
            int rows=(count+columns-1)/columns;var sheet=new Texture2D(columns*tile,rows*tile,TextureFormat.RGB24,false);var source=new Texture2D(2,2);
            try
            {
                for(int i=0;i<count;i++)
                {
                    source.LoadImage(File.ReadAllBytes("Art/Previews/Library/"+variants[HotpotKitchenCatalog.Items[start+i].Resource].preview));
                    var pixels=new Color[tile*tile];for(int y=0;y<tile;y++)for(int x=0;x<tile;x++)pixels[y*tile+x]=source.GetPixelBilinear((x+.5f)/tile,(y+.5f)/tile);
                    sheet.SetPixels(i%columns*tile,(rows-1-i/columns)*tile,tile,tile,pixels);
                }
                sheet.Apply();File.WriteAllBytes(Preview+"/"+name+".png",sheet.EncodeToPNG());
            }
            finally{Object.DestroyImmediate(sheet);Object.DestroyImmediate(source);}
        }
        internal static void VerifyAssets()
        {
            int max=0;var guids=new HashSet<string>();
            for(int i=0;i<56;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(i));if(prefab==null)throw new InvalidOperationException("Missing kitchen prefab "+i);
                var item=prefab.GetComponent<HotpotKitchenItem>();var spec=HotpotKitchenCatalog.Items[i];
                if(item==null||item.catalogId!=spec.id||item.role!=spec.role||item.level!=spec.level||item.size.sqrMagnitude==0)throw new InvalidOperationException("Invalid component "+i);
                if(!guids.Add(AssetDatabase.AssetPathToGUID(PrefabPath(i))))throw new InvalidOperationException("Duplicate prefab GUID");
                if(i>=36&&(item.contents==null||item.sockets.Length==0||item.sockets.Any(x=>x==null)))throw new InvalidOperationException("Missing contents/socket "+i);
                int triangles=0;foreach(var f in prefab.GetComponentsInChildren<MeshFilter>()){if(f.sharedMesh==null)throw new InvalidOperationException("Missing mesh "+i);triangles+=f.sharedMesh.triangles.Length/3;}
                foreach(var r in prefab.GetComponentsInChildren<MeshRenderer>())if(r.sharedMaterials.Any(m=>m==null||m.shader==null||m.shader.name.Contains("Error")))throw new InvalidOperationException("Invalid material "+i);
                if(triangles==0)throw new InvalidOperationException("Empty model "+i);max=Math.Max(max,triangles);
            }
            File.WriteAllText(".tools/model-work/hotpot-kitchen-assets-result.json","{\"status\":\"complete\",\"prefabs\":56,\"utensils\":12,\"ingredients\":24,\"dishes\":12,\"levels\":8,\"maxTriangles\":"+max+"}");
        }
    }
}
