using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static partial class AnimalAssets
    {
        internal static string[] AllAnimalNames(){return AnimalModelBuilder.Names.Concat(AnimalModelBuilder.AmbientNames).ToArray();}
        private static string ReviewPath(string id){return Array.IndexOf(AnimalModelBuilder.Names,id)>=0?PrefabPath(id):CompleteModelDefinitions.ExtraPath(id);}
        internal static void BakeAmbient()
        {
            const string path="Assets/HarvestIsle/Art/Animals/SmallCreatures.asset";
            var data=AssetDatabase.LoadAssetAtPath<NatureModelAsset>(path);
            if(data==null){data=ScriptableObject.CreateInstance<NatureModelAsset>();AssetDatabase.CreateAsset(data,path);}
            var oldMeshes=data.meshes;var oldMaterials=data.materials;
            var mats=AnimalModelBuilder.CreateMaterials();var meshes=new List<Mesh>();var models=new List<GameObject>();
            try
            {
                foreach(string name in AnimalModelBuilder.AmbientNames)models.Add(AnimalModelBuilder.CreateAmbient(name,mats,meshes));
                foreach(var mesh in meshes)AssetDatabase.AddObjectToAsset(mesh,data);foreach(var mat in mats)AssetDatabase.AddObjectToAsset(mat,data);
                data.meshes=meshes.ToArray();data.materials=mats;data.version=1;EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
                foreach(var model in models)if(PrefabUtility.SaveAsPrefabAsset(model,ReviewPath(model.name))==null)throw new InvalidOperationException("Cannot save creature "+model.name);
                if(oldMeshes!=null)foreach(var mesh in oldMeshes)if(mesh!=null)Object.DestroyImmediate(mesh,true);
                if(oldMaterials!=null)foreach(var mat in oldMaterials)if(mat!=null)Object.DestroyImmediate(mat,true);
                AssetDatabase.SaveAssets();
            }
            finally{foreach(var model in models)Object.DestroyImmediate(model);}
        }
        internal static void RenderAnimalReview()
        {
            var scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D board=null;
            try
            {
                var camera=PreviewCamera(scene);camera.aspect=1;var names=AllAnimalNames();const int cell=440,columns=5;
                int rows=Mathf.CeilToInt(names.Length/(float)columns);target=new RenderTexture(cell,cell,24);target.Create();board=new Texture2D(cell*columns,cell*rows,TextureFormat.RGB24,false);
                var pixels=new Color32[board.width*board.height];for(int i=0;i<pixels.Length;i++)pixels[i]=FarmWorld.Hex("F2EDE0");board.SetPixels32(pixels);
                for(int i=0;i<names.Length;i++)
                {
                    var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ReviewPath(names[i])),scene);
                    try
                    {
                        Bounds b=BoundsOf(model);camera.orthographicSize=Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z))*.77f;
                        camera.transform.position=b.center+new Vector3(3,1.9f,5);camera.transform.LookAt(b.center);
                        Render(camera,target);Read(target,board,(i%columns)*cell,(rows-1-i/columns)*cell);
                    }
                    finally{Object.DestroyImmediate(model);}
                }
                board.Apply();File.WriteAllBytes(PreviewFolder+"/All-animals-reviewed.png",board.EncodeToPNG());
            }
            finally{if(target!=null){target.Release();Object.DestroyImmediate(target);}if(board!=null)Object.DestroyImmediate(board);EditorSceneManager.ClosePreviewScene(scene);}
            var catalog=JsonUtility.FromJson<ModelLibraryCatalog>(File.ReadAllText("Art/Previews/Library/models.json"));
            var cards=new System.Text.StringBuilder();
            foreach(string name in AllAnimalNames())
            {var entry=Array.Find(catalog.models,m=>m.id==name);cards.Append("<a href='../Library/"+entry.variants[0].views+"'><img loading='lazy' src='../Library/"+entry.variants[0].preview+"' alt='"+entry.label+"'><strong>"+entry.label+"</strong></a>");}
            File.WriteAllText(PreviewFolder+"/review.html",File.ReadAllText("Tools/Templates/animal-review.html").Replace("__CARDS__",cards.ToString()),new System.Text.UTF8Encoding(false));
        }
        internal static void VerifyAllAnimalsLive()
        {
            int poses=0,ambientSamples=0;var instances=new List<GameObject>();
            try
            {
                foreach(string name in AllAnimalNames())
                {
                    string resource=ReviewPath(name).Replace("Assets/HarvestIsle/Resources/","").Replace(".prefab","");
                    var prefab=Resources.Load<GameObject>(resource);if(prefab==null)throw new InvalidOperationException("Missing animal: "+name);
                    var model=Object.Instantiate(prefab);instances.Add(model);model.transform.position=new Vector3(200,0,200);
                    var avatar=model.GetComponent<FarmAnimalAvatar>();
                    if(avatar!=null)for(int pose=0;pose<4;pose++)for(int frame=0;frame<16;frame++)
                    {avatar.PreviewPose((FarmAnimalPose)pose,frame*.13f);ValidateRig(model,avatar);poses++;}
                    else
                    {
                        var motion=model.GetComponent<AmbientCreatureMotion>();if(motion==null)throw new InvalidOperationException("Missing ambient animation: "+name);
                        for(int frame=0;frame<16;frame++)
                        {motion.PreviewMotion(frame*.13f);if(Mathf.Abs(model.transform.position.x-200)>.01f||Mathf.Abs(model.transform.position.z-200)>.01f)throw new InvalidOperationException("Creature position reset: "+name);foreach(Transform joint in model.GetComponentsInChildren<Transform>())if(float.IsNaN(joint.localRotation.x)||float.IsInfinity(joint.position.sqrMagnitude))throw new InvalidOperationException("Invalid creature motion: "+name);ambientSamples++;}
                    }
                    foreach(var renderer in model.GetComponentsInChildren<MeshRenderer>())foreach(var material in renderer.sharedMaterials)if(material==null||material.shader==null)throw new InvalidOperationException("Missing animal material: "+name);
                }
                File.WriteAllText(".tools/model-work/animal-review-result.json","{\"status\":\"complete\",\"models\":22,\"rigPoseSamples\":"+poses+",\"ambientMotionSamples\":"+ambientSamples+"}");
                Debug.Log("ALL_ANIMALS_VERIFIED: 22 models; "+poses+" rig poses; "+ambientSamples+" ambient motion samples.");
            }
            finally{foreach(var instance in instances)Object.DestroyImmediate(instance);}
            var viewer=Object.FindFirstObjectByType<ModelLibraryViewer>();if(viewer!=null)viewer.Hide();
            var weather=Object.FindFirstObjectByType<PlaygroundWeather>();if(weather!=null)weather.SetWeather(FarmWeather.Clear);
        }
    }
}
