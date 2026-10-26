using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace HarvestIsle.Editor
{
    public static partial class PlaygroundSetup
    {
        public static void AddLibrary()
        {
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var root=GameObject.Find("Complete model library");if(root==null)root=new GameObject("Complete model library");
            if(root.GetComponent<ModelLibraryViewer>()==null)root.AddComponent<ModelLibraryViewer>();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
        }
    }
}
