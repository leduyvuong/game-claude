using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace HarvestIsle.Editor
{
    internal static class IslandExpansionIntegration
    {
        public static void MergeCatalog()
        {
            const string path="Assets/HarvestIsle/Resources/FullModelCatalog.json",folder="Art/Previews/Library";
            var old=JsonUtility.FromJson<ModelLibraryCatalog>(File.ReadAllText(path));var extra=IslandExpansionAssets.CatalogEntries();
            if(extra.Length!=150)throw new InvalidOperationException("Bake the whole expansion first");
            foreach(var entry in extra)foreach(var v in entry.variants)if(!File.Exists(Path.Combine(folder,v.preview))||!File.Exists(Path.Combine(folder,v.views)))throw new InvalidOperationException("Render missing "+entry.id);
            var catalog=new ModelLibraryCatalog{version=1,prefabCount=Directory.GetFiles("Assets/HarvestIsle/Resources","*.prefab",SearchOption.AllDirectories).Length,models=old.models.Where(m=>!m.id.StartsWith("IX_")).Concat(extra).ToArray()};
            string json=JsonUtility.ToJson(catalog,true);File.WriteAllText(path,json,new UTF8Encoding(false));File.WriteAllText(folder+"/models.json",json,new UTF8Encoding(false));
            File.WriteAllText(folder+"/index.html",File.ReadAllText("Tools/Templates/full-model-gallery.html",Encoding.UTF8).Replace("__MODEL_DATA__",json),new UTF8Encoding(false));
            AssetDatabase.Refresh();File.WriteAllText(".tools/model-work/island-catalog-result.json","{\"status\":\"complete\",\"types\":"+catalog.models.Length+",\"variants\":"+catalog.models.Sum(m=>m.variants.Length)+",\"prefabs\":"+catalog.prefabCount+"}");
        }
    }
}
