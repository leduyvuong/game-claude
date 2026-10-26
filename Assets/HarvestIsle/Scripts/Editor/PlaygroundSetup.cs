using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    public static partial class PlaygroundSetup
    {
        public const string ScenePath="Assets/HarvestIsle/Scenes/AnimalPlayground.unity";
        [MenuItem("Harvest Isle/Animals/Create roaming playground")]
        public static void Create()
        {
            ProjectSetup.Configure();
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var world=new GameObject("Island environment");
            PlaceEnvironment("SeaSurface",world.transform,Vector3.zero,1);
            PlaceEnvironment("IslandBase",world.transform,Vector3.zero,1);
            PlaceEnvironment("Pond",world.transform,new Vector3(-6,0.15f,-0.8f),1);
            PlaceEnvironment("Lake",world.transform,new Vector3(6,0.15f,-0.4f),0.65f);
            PlaceEnvironment("MountainRange",world.transform,new Vector3(0,0.15f,6.4f),0.80f);
            PlaceEnvironment("Hill",world.transform,new Vector3(-6.0f,0.15f,4.1f),0.65f);
            PlaceEnvironment("RockCluster",world.transform,new Vector3(-6.9f,0.15f,-4.4f),0.6f);
            PlaceEnvironment("RockCluster",world.transform,new Vector3(6.7f,0.15f,4.0f),0.6f);
            var palm=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HarvestIsle/Resources/Nature/CoconutPalm.prefab");
            foreach(Vector3 p in new[]{new Vector3(-7,0.15f,2.0f),new Vector3(6.2f,0.15f,-4.5f),new Vector3(-3.8f,0.15f,5.6f)})
            {var instance=(GameObject)PrefabUtility.InstantiatePrefab(palm);instance.transform.SetParent(world.transform);instance.transform.position=p;}
            var residents=new GameObject("All characters - 1 farmer and 13 species");
            int groundIndex=0;
            for(int i=0;i<AnimalModelBuilder.Names.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AnimalAssets.PrefabPath(AnimalModelBuilder.Names[i]));
                if(prefab==null)throw new InvalidOperationException("Missing animal prefab: "+AnimalModelBuilder.Names[i]);
                var actor=(GameObject)PrefabUtility.InstantiatePrefab(prefab);actor.name=AnimalModelBuilder.Labels[i]+" - "+AnimalModelBuilder.Names[i];
                actor.transform.SetParent(residents.transform);
                if(actor.GetComponent<FarmAnimalAvatar>().IsAquatic)
                {
                    bool koi=i==9;actor.transform.localScale=Vector3.one*(koi?0.40f:0.35f);
                    actor.transform.position=koi?new Vector3(-6,0.188f,-0.8f):new Vector3(6,0.162f,-0.4f);
                }
                else
                {actor.transform.position=new Vector3((groundIndex%5-2)*1.5f,0.15f,(groundIndex/5)*2.2f-1.2f);groundIndex++;}
                actor.transform.rotation=Quaternion.Euler(0,140+i*19,0);
            }
            var farmer=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(FarmerAssets.PrefabPath));
            farmer.name="Nông dân";farmer.transform.SetParent(residents.transform);farmer.transform.position=new Vector3(0,0.15f,2.5f);farmer.transform.rotation=Quaternion.Euler(0,145,0);
            var manager=new GameObject("Playground controller").AddComponent<PlaygroundDirector>();
            var data=new SerializedObject(manager);data.FindProperty("residents").objectReferenceValue=residents.transform;data.ApplyModifiedPropertiesWithoutUndo();
            var sun=new GameObject("Warm afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;sun.color=FarmWorld.Hex("FFF0CB");sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(45,-32,0);
            var fill=new GameObject("Soft sky fill").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=0.40f;fill.color=new Color(0.84f,0.92f,1);fill.transform.rotation=Quaternion.Euler(30,140,0);
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(0.55f,0.60f,0.55f);RenderSettings.sun=sun;RenderSettings.fog=false;
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(UniversalAdditionalCameraData),typeof(PlaygroundCamera)).GetComponent<Camera>();
            camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=9.6f;camera.nearClipPlane=0.1f;camera.farClipPlane=150;camera.allowHDR=false;camera.backgroundColor=FarmWorld.Hex("62B7B6");camera.clearFlags=CameraClearFlags.SolidColor;
            camera.transform.position=new Vector3(15,18,-20);camera.transform.LookAt(new Vector3(0,0.3f,0));
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            Debug.Log("PLAYGROUND_SCENE_CREATED: "+ScenePath+" contains 14 characters and all 13 animal species.");
        }
        private static void PlaceEnvironment(string name,Transform parent,Vector3 position,float scale)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentAssets.PrefabPath(name));
            if(prefab==null)throw new InvalidOperationException("Missing environment: "+name);
            var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab);model.transform.SetParent(parent);model.transform.position=position;model.transform.localScale=Vector3.one*scale;
        }
        [MenuItem("Harvest Isle/Structures/Add structures to playground")]
        public static void AddStructures()
        {
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var previous=GameObject.Find("Generated farm structures");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous);
            var root=new GameObject("Generated farm structures");
            string[] names={"FarmHouse","RiceBarn","ChickenCoop","DuckShelter","CowShed","PigPen","Greenhouse","Nursery","VillageWell","LanternPost","Scarecrow"};
            Vector3[] positions={new Vector3(-4.8f,.15f,4.5f),new Vector3(4.7f,.15f,4.6f),new Vector3(-4.7f,.15f,-3.9f),new Vector3(-7.2f,.15f,-2.4f),
                new Vector3(-.8f,.15f,-5.2f),new Vector3(2.3f,.15f,-5.1f),new Vector3(6.4f,.15f,2.6f),new Vector3(-6.1f,.15f,2.6f),new Vector3(0,.15f,4.7f),new Vector3(-3.5f,.15f,3.6f),new Vector3(3.4f,.15f,3.7f)};
            for(int i=0;i<names.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(StructureAssets.PrefabPath(names[i]));if(prefab==null)continue;
                var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab);model.transform.SetParent(root.transform);
                model.transform.position=positions[i];model.transform.localScale=Vector3.one*(i==8?.65f:i>=9?.75f:.60f);
                model.transform.rotation=Quaternion.Euler(0,positions[i].z>0?180:0,0);
                if(Array.IndexOf(BuildingLevelView.Types,names[i])>=0)
                {
                    var wrapper=new GameObject(names[i]+" - Levels");wrapper.transform.SetParent(root.transform);
                    wrapper.transform.position=model.transform.position;wrapper.transform.rotation=model.transform.rotation;wrapper.transform.localScale=model.transform.localScale;
                    model.transform.SetParent(wrapper.transform,false);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;
                    wrapper.AddComponent<BuildingLevelView>().Configure(names[i],2,model);
                }
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
            Debug.Log("PLAYGROUND_STRUCTURES_ADDED: 11 display buildings/props outside the roaming area.");
        }
        [MenuItem("Harvest Isle/Animals/Open roaming playground and Play")]
        public static void OpenAndPlay()
        {
            if(!File.Exists(ScenePath))Create();
            else EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            EditorApplication.delayCall+=StartPlay;
        }
        private static void StartPlay()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=StartPlay;return;}
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying=true;
            Debug.Log("PLAYGROUND_PLAY_REQUESTED");
        }
    }
}
