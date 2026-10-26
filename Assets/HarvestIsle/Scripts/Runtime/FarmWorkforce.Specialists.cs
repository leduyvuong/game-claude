using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmWorkforce
    {
        private readonly Dictionary<string,FarmCrewAvatar> specialists=new Dictionary<string,FarmCrewAvatar>();
        public int VisibleSpecialists {get{return specialists.Count;}}
        private void SyncSpecialists()
        {
            foreach(var id in new List<string>(specialists.Keys))
                if(game.Model.OwnedWorker(id)==null||!game.Model.OwnedWorker(id).assigned){Destroy(specialists[id].gameObject);specialists.Remove(id);}
            foreach(var worker in game.Model.State.recruitment.roster)
            {
                if(!worker.assigned||specialists.ContainsKey(worker.id))continue;
                var spec=WorkerCatalog.Find(worker.id);
                string path=spec.portrait=="Farmer"?"Characters/Farmer":"Collection/"+spec.portrait;
                var model=game.World.Imported(path,game.World.transform,HousePoint+new Vector3(specialists.Count*.7f-1,0,-1.7f),game.IsEstate?1.5f:1.05f);
                if(model==null)continue;
                foreach(var collider in model.GetComponentsInChildren<Collider>())collider.enabled=false;
                model.name="Crew · "+spec.name;var actor=model.AddComponent<FarmCrewAvatar>();actor.Configure(game);specialists.Add(worker.id,actor);
            }
        }
        public void ShowActions(List<WorkerAction> actions)
        {
            SyncSpecialists();
            foreach(var action in actions)
            {
                if(!specialists.TryGetValue(action.id,out var actor))continue;
                Vector3 target=action.plot>=0?game.World.PositionOfPlot(action.plot):HousePoint;
                if(action.animalId!=null)
                {
                    int index=0;for(int i=0;i<LivestockCatalog.All.Length;i++)if(LivestockCatalog.All[i].id==action.animalId)index=i;
                    target=game.Ranch.PositionOfPen(index);
                }
                actor.ShowJob(target+new Vector3(.6f,0,-.7f),action.tool);
            }
        }
    }
    public sealed class FarmWorkerTarget:MonoBehaviour { }
    public sealed class FarmCrewAvatar:MonoBehaviour
    {
        private FarmGame game;private FarmerAvatar avatar;private Vector3 destination;private bool moving;private FarmTool tool;
        public void Configure(FarmGame owner){game=owner;avatar=GetComponent<FarmerAvatar>();destination=transform.position;}
        public void ShowJob(Vector3 point,FarmTool action){destination=point;destination.y=transform.position.y;tool=action;moving=true;}
        private void Update()
        {
            if(game.IsManaging||game.IsHarvesting||game.Ranch.IsOpen||game.Hotpot.IsOpen||game.Hud.IsReading){if(avatar!=null)avatar.SetWalking(false);return;}
            if(!moving)return;
            Vector3 delta=destination-transform.position;
            if(delta.sqrMagnitude<.04f){moving=false;if(avatar!=null){avatar.SetWalking(false);avatar.Perform(tool);}return;}
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*6);
            transform.position=Vector3.MoveTowards(transform.position,destination,Time.deltaTime*3.5f);if(avatar!=null)avatar.SetWalking(true);
        }
    }
}
