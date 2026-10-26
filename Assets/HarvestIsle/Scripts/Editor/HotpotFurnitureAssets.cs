using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarvestIsle.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class HotpotFurnitureAssets
    {
        const string Art="Assets/HarvestIsle/Art/HotpotFurniture";
        public static string PrefabPath(int kind){return "Assets/HarvestIsle/Resources/"+HotpotFurnitureCatalog.Resource(kind)+".prefab";}
        [MenuItem("Harvest Isle/Hotpot/Bake furniture and render gallery")]
        public static void RebuildAndPreview()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Bake furniture in Edit Mode.");
            Directory.CreateDirectory(Art);Directory.CreateDirectory("Assets/HarvestIsle/Resources/HotpotFurniture");AssetDatabase.Refresh();
            var materials=Materials();var builder=new HotpotFurnitureBuilder(materials);var ids=new List<string>();
            try
            {
                for(int kind=HotpotFurnitureCatalog.ReferenceStart;kind<HotpotFurnitureCatalog.Items.Length;kind++)
                {
                    var model=builder.Create(kind);
                    try
                    {
                        string id=model.name;ids.Add(id);
                        Merge(model.transform.Find("Structure"),id+"_Structure");
                        var serving=model.GetComponent<HotpotFurnitureView>().serving;if(serving!=null)Merge(serving,id+"_Serving");
                        var rs=model.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);
                        var hit=model.AddComponent<BoxCollider>();hit.center=bounds.center;hit.size=bounds.size;
                        if(PrefabUtility.SaveAsPrefabAsset(model,PrefabPath(kind))==null)throw new InvalidOperationException("Save failed "+id);
                    }
                    finally{Object.DestroyImmediate(model);}
                }
                AssetDatabase.SaveAssets();
            }
            finally{builder.Dispose();}
            CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),false,ids.ToArray());
            Directory.CreateDirectory("Assets/HarvestIsle/Resources/HotpotThumbnails");Directory.CreateDirectory("Art/Previews/HotpotFurniture");
            for(int kind=8;kind<HotpotFurnitureCatalog.Items.Length;kind++)
            {
                string id="HP_"+HotpotFurnitureCatalog.Items[kind].id,path="Assets/HarvestIsle/Resources/HotpotThumbnails/"+id+".png";
                File.Copy("Art/Previews/Library/"+id+"_1.png",path,true);AssetDatabase.ImportAsset(path);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=128;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            }
            // Gallery uses the same checked-in template as the complete library.
            var catalog=JsonUtility.FromJson<ModelLibraryCatalog>(File.ReadAllText("Art/Previews/Library/models.json",Encoding.UTF8));
            catalog.models=catalog.models.Where(x=>ids.Contains(x.id)).ToArray();
            foreach(var e in catalog.models)foreach(var v in e.variants){v.preview="../Library/"+v.preview;v.views="../Library/"+v.views;}
            string json=JsonUtility.ToJson(catalog,true);File.WriteAllText("Art/Previews/HotpotFurniture/models.json",json,new UTF8Encoding(false));
            File.WriteAllText("Art/Previews/HotpotFurniture/index.html",File.ReadAllText("Tools/Templates/full-model-gallery.html",Encoding.UTF8).Replace("__MODEL_DATA__",json),new UTF8Encoding(false));
            Overview(8,16,4,320,"Dining");Overview(24,36,6,240,"Decor");
            VerifyAssets();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        static void Overview(int start,int count,int columns,int tile,string name)
        {
            int rows=(count+columns-1)/columns;var sheet=new Texture2D(columns*tile,rows*tile,TextureFormat.RGB24,false);var source=new Texture2D(2,2);
            try
            {
                for(int i=0;i<count;i++)
                {
                    source.LoadImage(File.ReadAllBytes("Art/Previews/Library/HP_"+HotpotFurnitureCatalog.Items[start+i].id+"_1.png"));
                    var pixels=new Color[tile*tile];for(int y=0;y<tile;y++)for(int x=0;x<tile;x++)pixels[y*tile+x]=source.GetPixelBilinear((x+.5f)/tile,(y+.5f)/tile);
                    sheet.SetPixels(i%columns*tile,(rows-1-i/columns)*tile,tile,tile,pixels);
                }
                sheet.Apply();File.WriteAllBytes("Art/Previews/HotpotFurniture/"+name+".png",sheet.EncodeToPNG());
            }
            finally{Object.DestroyImmediate(sheet);Object.DestroyImmediate(source);}
        }
        internal static void Merge(Transform root,string name,string folder=Art)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();var filters=root.GetComponentsInChildren<MeshFilter>();
            foreach(var f in filters)
            {
                var r=f.GetComponent<MeshRenderer>();List<CombineInstance> group;
                if(!groups.TryGetValue(r.sharedMaterial,out group)){group=new List<CombineInstance>();groups.Add(r.sharedMaterial,group);}
                group.Add(new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});
            }
            foreach(var pair in groups)
            {
                string path=folder+"/"+name+"_"+pair.Key.name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(mesh==null){mesh=new Mesh{name=name+"_"+pair.Key.name,indexFormat=IndexFormat.UInt32};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
                mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
                var o=new GameObject(pair.Key.name);o.transform.SetParent(root,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            // Keep transforms/seat sockets; discard only temporary geometry and render components.
            foreach(var f in filters){Object.DestroyImmediate(f.GetComponent<MeshRenderer>());Object.DestroyImmediate(f);}
            Prune(root);
        }
        static void Prune(Transform root)
        {for(int i=root.childCount-1;i>=0;i--){var c=root.GetChild(i);Prune(c);if(c.childCount==0&&c.GetComponent<MeshFilter>()==null&&!c.name.StartsWith("Seat "))Object.DestroyImmediate(c.gameObject);}}
        internal static Dictionary<string,Material> Materials()
        {
            string[] specs={"Wood|985D2D","DarkWood|53341F","Bamboo|C69851","Steel|A3ADB0","Black|282F30","Ceramic|F3E8CE","Red|BD3126","RedShade|9F2923","Green|219854","Gold|BA8D3B","Leaf|437632","LightLeaf|85A744","Broth|CE7724","Meat|D75F5C","Soil|382A21","Flower|EC7477","Glow|FFD996","Marble|E5DECF","Stone|82857F","Upholstery|DDD0B7","Leather|695044","Seam|392C26","Teal|276F6A","Navy|344A62","Paper|E9D6AB","Water|549D9D","Sand|C6BC8F","Glass|C9E4DE","Amber|EEA032","Poster|D89245","Screen|597C74","Ice|C0DCDB"};
            var result=new Dictionary<string,Material>();
            foreach(string s in specs)
            {
                var p=s.Split('|');string path=Art+"/"+p[0]+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=p[0]};AssetDatabase.CreateAsset(mat,path);}
                mat.color=FarmWorld.Hex(p[1]);mat.enableInstancing=true;mat.SetFloat("_Smoothness",p[0]=="Steel"||p[0]=="Gold"?.65f:.32f);mat.SetFloat("_Metallic",p[0]=="Steel"||p[0]=="Gold"?.72f:0);
                if(p[0]=="Glow"){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",FarmWorld.Hex(p[1])*.55f);}
                if(p[0]=="Glass")
                {mat.shader=Shader.Find("Universal Render Pipeline/Unlit");mat.color=new Color(.75f,.91f,.87f,.12f);mat.SetFloat("_Surface",1);mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);mat.SetFloat("_ZWrite",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;}
                if(p[0]=="Wood"||p[0]=="DarkWood"||p[0]=="Bamboo"||p[0]=="Marble"||p[0]=="Stone")
                {
                    string tp=Art+"/"+p[0]+"Grain.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(tp);bool created=texture==null;if(created)texture=new Texture2D(128,128,TextureFormat.RGB24,true){name=p[0]+"Grain"};
                    var pixels=new Color[128*128];for(int y=0;y<128;y++)for(int x=0;x<128;x++)
                    {float n=Mathf.PerlinNoise(x*.09f,y*.19f);float v=p[0]=="Marble"?.90f+.10f*Mathf.Sin(y*.08f+n*4):p[0]=="Stone"?.83f+n*.17f:.92f+.035f*Mathf.Sin(y*.42f+Mathf.Sin(x*.06f)*1.9f)+n*.035f;pixels[y*128+x]=new Color(v,v,v);}
                    texture.SetPixels(pixels);texture.Apply();if(created)AssetDatabase.CreateAsset(texture,tp);else EditorUtility.SetDirty(texture);mat.SetTexture("_BaseMap",texture);
                }
                EditorUtility.SetDirty(mat);result.Add(p[0],mat);
            }
            return result;
        }
        public static void VerifyAssets()
        {
            int tables=0,total=0,maxTriangles=0;
            for(int i=8;i<HotpotFurnitureCatalog.Items.Length;i++)
            {
                var p=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(i));if(p==null)throw new InvalidOperationException("Missing furniture "+i);
                var v=p.GetComponent<HotpotFurnitureView>();if(v==null||v.kind!=i||v.footprint.x>2.66f||v.footprint.z>2.66f)throw new InvalidOperationException("Invalid footprint/component "+i);
                if(HotpotFurnitureCatalog.IsTable(i)){tables++;if(v.seats.Length<2||v.serving==null||v.seats.Any(x=>x==null))throw new InvalidOperationException("Missing dining anchors "+i);}
                int triangles=0;foreach(var f in p.GetComponentsInChildren<MeshFilter>()){if(f.sharedMesh==null)throw new InvalidOperationException("Missing mesh "+i);triangles+=f.sharedMesh.triangles.Length/3;}
                foreach(var r in p.GetComponentsInChildren<Renderer>())if(r.sharedMaterials.Any(m=>m==null||m.shader==null||m.shader.name.Contains("Error")))throw new InvalidOperationException("Invalid material "+i);
                if(triangles==0)throw new InvalidOperationException("Empty furniture "+i);maxTriangles=Math.Max(maxTriangles,triangles);total++;
            }
            File.WriteAllText(".tools/model-work/hotpot-assets-result.json","{\"status\":\"complete\",\"prefabs\":"+total+",\"diningSets\":"+tables+",\"maxTriangles\":"+maxTriangles+"}");
        }
    }
}
