using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        public bool OpenNextBranch()
        {
            if(!IsOpen||tab!=5||game.Online!=null)return false;
            if(HasWaitingOrders){message="Phục vụ hoặc chờ hết khách trước khi mở chi nhánh.";return false;}
            var result=game.Model.OpenHotpotBranch();Perform(result);if(!result.success)return false;
            Service.EndRush();Service=new HarvestIsle.Core.HotpotServiceSession(game.Model,HarvestIsle.Core.HotpotRules.Hash((int)(game.Now/1800),game.Model.State.hotpot.progress.branch,17));
            foreach(var table in tables){table.occupied=false;table.served=false;}
            nextArrival=1.5f;BuildRoom();return true;
        }
        private void BranchProp(Transform parent,string resource,Vector3 position,float scale)
        {
            var prefab=Resources.Load<GameObject>(resource);if(prefab==null)return;
            var model=Instantiate(prefab,parent,false);model.transform.localPosition=position;model.transform.localScale*=scale;
            foreach(var collider in model.GetComponentsInChildren<Collider>(true))collider.enabled=false;
        }
        private void BuildBranchSurroundings(Transform parent)
        {
            int branch=game.Model.State.hotpot.progress.branch%4;
            if(branch==1)
            {
                Box("Coastal water",parent,new Vector3(0,-.11f,13),new Vector3(36,.05f,8),"5EAFBB");
                Box("Coastal sand",parent,new Vector3(0,-.10f,9.5f),new Vector3(36,.04f,2),"DFCF9B");
                for(int side=-1;side<=1;side+=2)
                {
                    BranchProp(parent,"Nature/CoconutPalm",new Vector3(side*10,0,-5),1.4f);
                    BranchProp(parent,"Environment/ArecaPalm",new Vector3(side*11,0,7),1.2f);
                }
                BranchProp(parent,"Production/CargoBoat",new Vector3(7,-.05f,13),.65f);
            }
            else if(branch==2)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    BranchProp(parent,"Environment/RockLarge",new Vector3(side*12,0,9),2);
                    BranchProp(parent,"Environment/PineTree",new Vector3(side*10.5f,0,6),1.5f);
                    BranchProp(parent,"Environment/PineTree",new Vector3(side*11,0,-4),1.25f);
                    BranchProp(parent,"Collection/BambooClump",new Vector3(side*10,0,-9),1);
                }
            }
            else if(branch==3)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    BranchProp(parent,"Production/MarketStall",new Vector3(side*11,0,8),1);
                    for(int z=-6;z<=4;z+=5)
                    {
                        BranchProp(parent,"HotpotFurniture/HP_LanternPost",new Vector3(side*10,0,z),1);
                        var light=Group("Night market warm light",parent,new Vector3(side*9,2,z)).AddComponent<Light>();
                        light.type=LightType.Point;light.color=FarmWorld.Hex("FFBB75");light.intensity=1.4f;light.range=5;light.shadows=LightShadows.None;
                    }
                }
            }
        }
    }
}
