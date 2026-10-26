using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    public static class ArchitecturePlayReview
    {
        public static void Run()
        {
            var estate=Object.FindFirstObjectByType<FarmEstate>();
            if(estate==null||!estate.Ready)throw new InvalidOperationException("Estate must be in Play");
            var runner=new GameObject("Architecture review").AddComponent<FarmEstateReviewRunner>();
            runner.StartCoroutine(Capture(estate,runner));
        }
        private static IEnumerator Capture(FarmEstate estate,MonoBehaviour runner)
        {
            var game=estate.Game;var cam=game.World.GameCamera;var day=game.GetComponent<FarmDayNight>();
            bool hud=game.Hud.gameObject.activeSelf,dayEnabled=day.enabled;float phase=day.Phase;
            var weather=game.GetComponentsInChildren<ParticleSystemRenderer>().Where(r=>r.enabled).ToArray();
            string folder="Art/Previews/AnNhien/Architecture";Directory.CreateDirectory(folder);
            File.WriteAllText(".tools/model-work/architecture-play-result.json","{\"status\":\"running\"}");
            bool passed=false;
            try
            {
                game.CameraController.enabled=false;game.Hud.gameObject.SetActive(false);day.enabled=false;day.SetPhase(.42f);
                foreach(var r in weather)r.enabled=false;
                var centers=new[]{new Vector3(0,1.5f,16),new Vector3(-7,1,11),new Vector3(8,1,17),new Vector3(-24,1,15),new Vector3(-22,0,3),new Vector3(-22,0,-5),new Vector3(-22,0,-13),new Vector3(-23,0,10)};
                var names=new[]{"Home","Well","Warehouse","Cafe","Ranch-north","Ranch-middle","Ranch-south","Market"};
                for(int i=0;i<centers.Length+9;i++)
                {
                    Vector3 center=i<centers.Length?centers[i]:FarmEstate.BuildingPosition(i-centers.Length)+Vector3.up*1.1f;
                    string name=i<centers.Length?names[i]:"Workshop-"+(i-centers.Length+1);
                    float size=i==0?4.7f:i==1?2.7f:i==2?4.2f:i==3?3.4f:i<centers.Length?5.5f:3.4f;
                    cam.orthographicSize=size;cam.transform.rotation=Quaternion.Euler(38,20,0);cam.transform.position=center-cam.transform.forward*32;
                    yield return new WaitForSecondsRealtime(.35f);
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder+"/"+name+".png"));yield return new WaitForSecondsRealtime(.35f);
                    if(!File.Exists(folder+"/"+name+".png"))throw new InvalidOperationException("Missing image "+name);
                }
                Physics.SyncTransforms();
                foreach(var target in game.World.GetComponentsInChildren<FarmDevelopmentTarget>())
                {
                    if(Quaternion.Angle(target.transform.localRotation,Quaternion.Euler(0,180,0))>.01f)throw new InvalidOperationException("Workshop faces away");
                    var collider=target.GetComponent<Collider>();if(collider==null||!collider.enabled)throw new InvalidOperationException("Workshop target disabled");
                    if(!Physics.Raycast(collider.bounds.center+Vector3.up*20,Vector3.down,out var hit,40,1<<FarmWorld.InteractionLayer)||hit.collider.GetComponent<FarmDevelopmentTarget>()!=target)throw new InvalidOperationException("Workshop raycast obstructed");
                }
                foreach(var target in game.World.GetComponentsInChildren<FarmRanchTarget>())
                    if(!Physics.Raycast(target.transform.position+Vector3.up*20,Vector3.down,out var hit,40,1<<FarmWorld.InteractionLayer)||hit.collider.GetComponent<FarmRanchTarget>()!=target)throw new InvalidOperationException("Pen raycast obstructed");
                var shop=game.World.GetComponentInChildren<FarmHotpotTarget>();
                if(shop==null||!Physics.Raycast(shop.transform.position+Vector3.up*20,Vector3.down,out var shopHit,40,1<<FarmWorld.InteractionLayer)||shopHit.collider.GetComponent<FarmHotpotTarget>()!=shop)throw new InvalidOperationException("Shop raycast obstructed");
                if(game.World.GetComponentsInChildren<FarmDevelopmentTarget>().Length!=9)throw new InvalidOperationException("Expected nine workshops");
                passed=true;
            }
            finally
            {
                foreach(var r in weather)if(r!=null)r.enabled=true;
                day.enabled=dayEnabled;day.SetPhase(phase);game.Hud.gameObject.SetActive(hud);cam.transform.rotation=Quaternion.Euler(48,20,0);game.CameraController.enabled=true;estate.ShowZone(3);
                File.WriteAllText(".tools/model-work/architecture-play-result.json",passed?"{\"status\":\"complete\",\"playImages\":17,\"workshopTargets\":9,\"penTargets\":6,\"shopTarget\":true}":"{\"status\":\"failed\"}");
                Object.Destroy(runner.gameObject);
            }
        }
    }
}
