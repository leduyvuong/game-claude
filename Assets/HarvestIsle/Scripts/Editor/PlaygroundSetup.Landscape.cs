using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    public static partial class PlaygroundSetup
    {
        [MenuItem("Harvest Isle/Environment/Update island landscape")]
        public static void UpdateLandscape()
        {
            EnvironmentAssets.RebuildAndPreview();
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var world=GameObject.Find("Island environment");
            if(world==null)throw new InvalidOperationException("Missing playground island root");
            for(int i=world.transform.childCount-1;i>=0;i--)
            {
                var child=world.transform.GetChild(i);
                if(child.name=="MountainRange"||child.name=="Landscape vegetation")UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            var foliage=new GameObject("Landscape vegetation");foliage.transform.SetParent(world.transform,false);
            EnvironmentAssets.PlaceLandscapeTrees(SceneManager.GetActiveScene(),foliage.transform);
            // Small ground-cover pockets are embedded in the earth instead of floating as display slabs.
            string[] tiles={"MeadowTile","FlowerMeadowTile","ForestFloorTile","RedEarthTile","AlluvialTile","DryGrassTile","GravelTile","HumusTile"};
            Vector3[] patches={new Vector3(-7.4f,0,3.3f),new Vector3(7.2f,0,-4.6f),new Vector3(-5.9f,0,5.4f),new Vector3(7.1f,0,4.1f),new Vector3(-3.5f,0,-5.9f),new Vector3(4.4f,0,-5.8f),new Vector3(5.9f,0,5.4f),new Vector3(-7.1f,0,4.4f)};
            for(int i=0;i<tiles.Length;i++)
            {Vector3 p=patches[i];p.y=IslandTerrainProfile.Height(p.x,p.z)+.005f;PlaceEnvironment(tiles[i],foliage.transform,p,.53f);}
            Physics.SyncTransforms();
            // Place scenery on the actual baked collider (not an approximation of the sampled mesh).
            foreach(Transform tree in foliage.transform)
            {
                if(tree.name.EndsWith("Tile"))continue;
                Vector3 p=tree.position;
                var island=world.transform.Find("IslandBase");
                var collider=island==null?null:island.GetComponentInChildren<MeshCollider>();
                if(collider!=null&&collider.Raycast(new Ray(p+Vector3.up*20,Vector3.down),out RaycastHit hit,40))tree.position=hit.point;
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
            CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(),true);
            Debug.Log("LANDSCAPE_BAKED: continuous coastal plain and mountain slope, 8 new soils, 7 new tree models.");
        }

        internal static void VerifyLandscapeLive()
        {
            var viewer=UnityEngine.Object.FindFirstObjectByType<ModelLibraryViewer>();if(viewer!=null)viewer.Hide();
            var director=UnityEngine.Object.FindFirstObjectByType<PlaygroundDirector>();if(director!=null)director.ShowProduction(false);
            var weather=UnityEngine.Object.FindFirstObjectByType<PlaygroundWeather>();if(weather!=null)weather.SetWeather(FarmWeather.Clear);
            var world=GameObject.Find("Island environment");var island=world.transform.Find("IslandBase").GetComponentInChildren<MeshCollider>();
            float previous=.14f;int samples=0;
            foreach(float z in new[]{0f,4.8f,5.1f,5.4f,5.7f,6.0f,6.3f,6.6f})
            {
                if(!island.Raycast(new Ray(new Vector3(0,20,z),Vector3.down),out RaycastHit hit,40)||hit.point.y<previous-.03f)throw new InvalidOperationException("Mountain slope is not continuous and ascending at "+z);
                previous=hit.point.y;samples++;
            }
            if(previous<3)throw new InvalidOperationException("Mountain is too low");
            foreach(var filter in world.GetComponentsInChildren<MeshFilter>())if(filter.sharedMesh==null)throw new InvalidOperationException("Missing landscape mesh");
            Directory.CreateDirectory("Art/Previews/Environment");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Environment/Unity-landscape.png"));
            File.WriteAllText(".tools/model-work/landscape-result.json","{\"status\":\"complete\",\"slopeSamples\":"+samples+",\"newSoils\":8,\"newTrees\":7}");
            Debug.Log("LANDSCAPE_LIVE_VERIFIED: ascending slope sampled against baked collision mesh.");
        }
    }
}
