using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using HarvestIsle.Core;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    [InitializeOnLoad]
    public static class GameHumanReview
    {
        private const string Key="HarvestIsle.GameHumanReview";
        private const string PreviewKey="HarvestIsle.GameHumanPreview";
        private static int stage=-1;
        private static double deadline;
        private static FarmGame game;
        private static string pending;
        static GameHumanReview(){EditorApplication.update+=Tick;}
        public static void Begin()
        {
            SessionState.SetBool(PreviewKey,false);
            SessionState.SetBool(Key,true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            EditorApplication.isPlaying=true;
        }
        [MenuItem("Harvest Isle/Characters/View fitted humans in farm")]
        public static void OpenPreview()
        {
            Begin();
            SessionState.SetBool(PreviewKey,true);
            EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        }
        private static void Tick()
        {
            if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            try
            {
                if(stage<0)
                {
                    bool preview=SessionState.GetBool(PreviewKey,false);
                    if(!preview)ValidateModels();
                    game=new GameObject("Human integration isolated farm").AddComponent<FarmGame>();
                    game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/human-integration-farm")),FarmGame.UtcNow);
                    stage=preview?4:0;deadline=EditorApplication.timeSinceStartup+5;return;
                }
                if(pending!=null)
                {
                    string result=File.ReadAllText(pending);
                    if(result.Contains("failed"))throw new InvalidOperationException(pending+": "+result);
                    if(!result.Contains("complete"))
                    {if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException(pending);return;}
                    pending=null;stage++;deadline=EditorApplication.timeSinceStartup+1;return;
                }
                if(EditorApplication.timeSinceStartup<deadline)return;
                switch(stage)
                {
                    case 0: Start("batch-result.json",BatchInteractionReview.Run);break;
                    case 1: Start("farm-loop-result.json",FarmLoopReview.Verify);break;
                    case 2: Start("town-result.json",FarmTownReview.Verify);break;
                    case 3: Start("play-interface-result.json",PlayInterfaceReview.Run);break;
                    case 4: PrepareCapture();stage++;deadline=EditorApplication.timeSinceStartup+2;break;
                    case 5: Capture();stage++;break;
                    default: Finish(true,"Five reference rigs, preserved outfits, animated skins, batch/farm/town/interface checks passed on isolated saves.");break;
                }
            }
            catch(Exception e){Finish(false,e.ToString());}
        }
        private static void Start(string name,Action run)
        {pending=".tools/model-work/"+name;File.WriteAllText(pending,"{\"status\":\"running\"}");deadline=EditorApplication.timeSinceStartup+150;run();}
        private static void ValidateModels()
        {
            var baked=new Mesh();
            foreach(string path in new[]{"Characters/Farmer","Characters/FarmerFemale","Collection/ElderTu","Collection/MerchantHaiAu","Collection/FarmerFemale"})
            {
                var npc=Object.Instantiate(Resources.Load<GameObject>(path));var avatar=npc.GetComponent<FarmerAvatar>();
                var motion=npc.GetComponentInChildren<ReferenceMaleMotion>();
                Require(avatar!=null&&motion!=null&&motion.bones.Length==49,"Reference anatomy/rig missing "+path);
                Require(!motion.enabled&&!motion.playing,"Competing animation driver "+path);
                Require(motion.workPlant==null&&motion.soilPatch==null,"Studio ground props leaked into game");
                var skins=npc.GetComponentsInChildren<SkinnedMeshRenderer>();Require(skins.Length>10,"Outfit skins missing");
                Vector3 previous=Vector3.zero;float movement=0;
                for(int frame=0;frame<32;frame++)
                {
                    avatar.SetPose(frame*.045f,1,0,FarmTool.Plant);
                    var body=skins[0];body.BakeMesh(baked,true);baked.RecalculateBounds();
                    Require(baked.bounds.size.y>1.4f&&baked.bounds.size.y<2.3f,"Invalid anatomy bounds "+path);
                    Vector3 current=baked.vertices[0];if(frame>0)movement+=Vector3.Distance(previous,current);previous=current;
                    foreach(var skin in skins)
                    {skin.BakeMesh(baked,true);baked.RecalculateBounds();Require(!float.IsNaN(baked.bounds.size.sqrMagnitude)&&baked.bounds.size.sqrMagnitude<20,"Invalid outfit deformation");}
                }
                Require(movement>.01f,"Skin did not animate "+path);
                foreach(var tool in new[]{FarmTool.Water,FarmTool.Plant,FarmTool.Harvest})
                {avatar.SetPose(0,0,1,tool);Require(motion.wateringCan.activeSelf==(tool==FarmTool.Water),"Wrong action prop");}
                Object.DestroyImmediate(npc);
            }
            Object.DestroyImmediate(baked);
        }
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private static void PrepareCapture()
        {
            game.Hud.gameObject.SetActive(false);game.CameraController.enabled=false;
            var center=new Vector3(-3, .16f, -5);
            for(int i=0;i<2;i++)
            {
                var model=Object.Instantiate(Resources.Load<GameObject>("Characters/"+(i==0?"Farmer":"FarmerFemale")),game.transform);
                model.transform.position=center+new Vector3(i*1.05f-.525f,0,0);
                model.transform.rotation=Quaternion.Euler(0,15,0);
                model.GetComponent<FarmerAvatar>().SetPose(0,0,0,FarmTool.Plant);
                model.GetComponent<FarmerAvatar>().enabled=false;
            }
            var camera=game.World.GameCamera;camera.orthographicSize=1.55f;
            camera.transform.position=center+new Vector3(2.2f,2.1f,5);
            camera.transform.LookAt(center+Vector3.up*.85f);
        }
        private static void Capture()
        {
            var target=new RenderTexture(1400,1000,24);target.Create();var image=new Texture2D(1400,1000,TextureFormat.RGB24,false);
            var camera=game.World.GameCamera;camera.aspect=1.4f;
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            var previous=RenderTexture.active;RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1400,1000),0,0);image.Apply();RenderTexture.active=previous;
            File.WriteAllBytes("Art/Previews/Farm/Humans-new-body.png",image.EncodeToPNG());
            target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(image);
        }
        private static void Finish(bool success,string message)
        {
            SessionState.SetBool(Key,false);
            bool preview=SessionState.GetBool(PreviewKey,false);
            if(preview&&success)message="Fitted male and female preview open in Play Mode.";
            File.WriteAllText(".tools/model-work/"+(preview?"human-preview-result.json":"human-integration-result.json"),JsonUtility.ToJson(new Result{status=success?"complete":"failed",message=message},true));
            if(success)Debug.Log(message);else Debug.LogError(message);
            if(Application.isBatchMode)EditorApplication.Exit(success?0:1);
        }
        [Serializable]private class Result{public string status,message;}
    }
}
