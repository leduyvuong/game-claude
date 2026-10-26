using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    public static partial class PlaygroundSetup
    {
        [MenuItem("Harvest Isle/Production/Add production island to playground")]
        public static void AddProduction()
        {
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var previous=GameObject.Find("Production model showcase");if(previous!=null)Object.DestroyImmediate(previous);
            var root=new GameObject("Production model showcase");root.transform.position=new Vector3(23,0,0);
            var island=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentAssets.PrefabPath("IslandBase")));
            island.transform.SetParent(root.transform,false);island.transform.localScale=new Vector3(1.18f,1,1.25f);
            for(int i=0;i<ProductionDisplay.Types.Length;i++)
            {
                string type=ProductionDisplay.Types[i];var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ProductionAssets.PrefabPath(type));
                if(prefab==null)throw new System.InvalidOperationException("Missing production prefab: "+type);
                var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab);model.name=ProductionDisplay.Labels[i]+" - "+type;model.transform.SetParent(root.transform,false);
                model.transform.localPosition=new Vector3((i%4-1.5f)*3.25f,.15f,(1.5f-i/4)*2.85f);
                model.transform.localScale=Vector3.one*(type=="BeeHive"?.90f:type=="Lighthouse"?.65f:.78f);model.transform.localRotation=Quaternion.Euler(0,180,0);
                model.AddComponent<ProductionDisplay>().Configure(ProductionDisplay.Labels[i]);
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
            Debug.Log("PRODUCTION_SHOWCASE_ADDED: 16 models on the second island, 5 animated machines.");
        }
    }
}
