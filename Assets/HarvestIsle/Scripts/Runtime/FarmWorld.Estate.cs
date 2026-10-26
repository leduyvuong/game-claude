using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmWorld
    {
        private bool estateLayout;
        public Vector3 PositionOfPlot(int index) { return estateLayout?FarmEstate.PlotPosition(index):PlotPosition(index); }

        private void BuildEstate(FarmGame game)
        {
            // A 70 x 60 m planned clearing; all dressing is deterministic.
            EstateBlock("An Nhien meadow",0,0,70,60,"A6BF73",.14f,.04f);
            EstateBlock("Field lawn",0,-5.5f,23,24,"91AD59",.17f,.025f);
            EstateBlock("West pasture",-23,-5,20,28,"B3C975",.17f,.025f);
            EstateBlock("Workshop court",23,-13,21,25,"C5B58A",.17f,.025f);
            EstateBlock("Orchard lawn",23,15,20,18,"9BB66A",.17f,.025f);

            EstatePath(0,-25,68,2.4f); EstatePath(0,6.4f,68,2.4f);
            EstatePath(-12.5f,-5,2.3f,39); EstatePath(12.5f,-5,2.3f,39);
            EstatePath(0,24,67,2); EstatePath(-33,0,2,48); EstatePath(33,0,2,48);
            EstatePath(-3.6f,-5.5f,1.4f,21); EstatePath(3.6f,-5.5f,1.4f,21);
            EstatePath(0,-1.9f,21,1.4f); EstatePath(0,-9.1f,21,1.4f);
            EstatePath(0,-18,24,2); EstatePath(0,-27,3,5);
            EstateBlock("Homestead plaza",0,12,24,9,"E5D5AE",.18f,.035f);
            EstatePath(-23,-1,20,1.3f); EstatePath(-23,-9,20,1.3f);
            EstatePath(23,-9,20,1.3f); EstatePath(23,-17,20,1.3f);
            EstatePath(23,15,1.5f,18);
            for(int i=0;i<9;i++)
            {
                Vector3 p=FarmEstate.BuildingPosition(i);
                EstateBlock("Workshop foundation",p.x,p.z,5,5.8f,"DAC79B",.19f,.025f);
                EstateProp("Collection/WoodBundle",p+new Vector3(-1.8f,0,1.8f),.8f);
                EstateProp("Collection/Basket",p+new Vector3(1.7f,0,1.7f),.8f);
                EstateProp("Collection/SeedBag",p+new Vector3(1.6f,0,-1.8f),.8f);
            }

            plots=new FarmPlot[FarmModel.MaxPlots];
            for(int i=0;i<game.Model.State.plots.Count;i++)CreatePlot(i);
            SyncParcelMarkers(game.Model);
            StructureModel("FarmHouse",new Vector3(0,.15f,16),1.8f,180);
            StructureModel("VillageWell",new Vector3(-7,.15f,11),1.15f,0);
            EstateProp("Collection/VillageWarehouse",new Vector3(8,.15f,17),1.15f,180);
            EstateProp("Production/HandCart",new Vector3(9,.18f,10),1.1f,120);
            EstateProp("Collection/OrderBoard",new Vector3(8,.18f,8),1.1f,180);
            EstateLabel("AN NHIÊN",new Vector3(0,.3f,9),.44f);
            EstateLabel("RUỘNG MÙA VÀNG",new Vector3(0,.28f,-20),.32f);
            EstateLabel("ĐỒNG CỎ",new Vector3(-23,.28f,8),.32f);
            EstateLabel("XÓM THỢ",new Vector3(23,.28f,1.5f),.32f);
            EstateLabel("VƯỜN BỐN MÙA",new Vector3(23,.28f,25.5f),.32f);

            // A grove for shade and the northern lakeside garden; harvestable fruit
            // trees belong to the nine real crop beds and share their usual rules.
            string[] grove={"Mango","Orange","Lemon"};
            for(int row=0;row<3;row++)for(int col=0;col<4;col++)
            {
                Vector3 p=new Vector3(16.5f+col*4.3f,.18f,10+row*4.3f);
                Shape(PrimitiveType.Cylinder,"Orchard mulch",transform,p,new Vector3(2,.015f,2),Hex("AD9160"));
                EstateProp("Crops/"+grove[(row+col)%3]+"/"+grove[(row+col)%3]+"_5",p,2.5f,18*(row+col));
            }
            EnvironmentModel("Lake",new Vector3(-14,.16f,20),1.55f);
            EnvironmentModel("WillowTree",new Vector3(-18,.15f,23),1.4f);
            EnvironmentModel("RainTree",new Vector3(-8,.15f,23),1.2f);
            EstateProp("Collection/Bench",new Vector3(-11,.18f,15),1.3f,0);
            EstateProp("Collection/Bench",new Vector3(-17,.18f,14),1.3f,25);
            Fish("Koi",new Vector3(-14,.20f,20),1.4f,1,.6f,21);
            for(int i=0;i<5;i++)
            {
                var stall=EstateProp("Production/MarketStall",new Vector3(-28+i*2.4f,.18f,10),.7f,180);
                stall.layer=InteractionLayer;stall.AddComponent<FarmMarket>();
                var hit=stall.AddComponent<BoxCollider>();hit.center=Vector3.up;hit.size=new Vector3(2.5f,2,2.5f);
                Flowers(new Vector3(-8+i*4,.19f,8));
            }
            // Perimeter trees and an open southern gate give the estate a clear boundary.
            for(int i=0;i<18;i++)
            {
                float x=-34+i*4;
                EnvironmentModel(i%3==0?"ArecaPalm":"YoungForestTree",new Vector3(x,.15f,29),1.15f);
                if(Mathf.Abs(x)>4)EnvironmentModel(i%3==0?"ArecaPalm":"YoungForestTree",new Vector3(x,.15f,-29),1.05f);
                if(Mathf.Abs(x)>4)EstateRail(new Vector3(x,.17f,-27.5f),false,3.8f);
                EstateRail(new Vector3(x,.17f,27),false,3.8f);
            }
            for(int i=0;i<13;i++)
            {
                float z=-24+i*4;
                for(int side=-1;side<=1;side+=2)
                {
                    EnvironmentModel(i%3==0?"FlameTree":"YoungForestTree",new Vector3(side*36,.15f,z),1.15f);
                    EstateRail(new Vector3(side*34.5f,.17f,z),true,3.8f);
                }
            }
            EstateBlock("Gate post west",-2.2f,-27.5f,.45f,.45f,"896343",1.15f,2);
            EstateBlock("Gate post east",2.2f,-27.5f,.45f,.45f,"896343",1.15f,2);
            EstateBlock("Gate lintel",0,-27.5f,5,.38f,"896343",2.15f,.3f);
            TownRoot=new GameObject("An Nhien lighting").transform; TownRoot.SetParent(transform,false);
            foreach(float x in new[]{-12.5f,12.5f})foreach(float z in new[]{-23f,-7f,8f,23f})LampPost(new Vector3(x,.18f,z));
            farmer=Farmer(new Vector3(-3,.21f,6.2f)); farmerTarget=farmer.position;
            Animal("Cat",new Vector3(3,.21f,11),1,.18f,55);
            Animal("Buffalo",new Vector3(-23,.15f,-20),1.6f,.2f,77);
            Animal("Sparrow",new Vector3(-5,.15f,12),.8f,.18f,63);
            BuildScenery();
        }
        private void EstatePath(float x,float z,float width,float depth)
        { EstateBlock("Estate path",x,z,width,depth,"DDCA9E",.19f,.025f); }
        private void EstateBlock(string name,float x,float z,float width,float depth,string color,float y,float height)
        { Shape(PrimitiveType.Cube,name,transform,new Vector3(x,y,z),new Vector3(width,height,depth),Hex(color)); }
        private GameObject EstateProp(string resource,Vector3 position,float scale,float yaw=0)
        {
            var prefab=Resources.Load<GameObject>(resource);
            if(prefab==null){Debug.LogWarning("Estate missing scenery: "+resource);return null;}
            var model=Instantiate(prefab,transform,false); model.transform.localPosition=position;
            model.transform.localScale=Vector3.one*scale;model.transform.localRotation=Quaternion.Euler(0,yaw,0);
            foreach(var collider in model.GetComponentsInChildren<Collider>())collider.enabled=false;
            return model;
        }
        private void EstateRail(Vector3 p,bool alongZ,float length)
        {
            Vector3 direction=alongZ?Vector3.forward:Vector3.right;
            for(int i=-1;i<=1;i+=2)Shape(PrimitiveType.Cube,"Estate fence post",transform,p+direction*i*length/2+Vector3.up*.4f,new Vector3(.12f,.8f,.12f),Hex("B29569"));
            for(int i=0;i<2;i++)Shape(PrimitiveType.Cube,"Estate fence rail",transform,p+Vector3.up*(.3f+i*.3f),alongZ?new Vector3(.08f,.10f,length):new Vector3(length,.10f,.08f),Hex("E5C898"));
        }
        private void EstateLabel(string text,Vector3 position,float size)
        {
            var label=new GameObject(text).AddComponent<TextMesh>();label.transform.SetParent(transform,false);
            float width=Mathf.Clamp(text.Length*.34f,3.5f,7);
            EstateBlock("Garden sign",position.x,position.z,width,.12f,"D7BA85",1.3f,.65f);
            EstateBlock("Garden sign post",position.x-width*.36f,position.z,.1f,.1f,"896343",.7f,1.4f);
            EstateBlock("Garden sign post",position.x+width*.36f,position.z,.1f,.1f,"896343",.7f,1.4f);
            label.transform.localPosition=new Vector3(position.x,1.3f,position.z-.07f);
            label.text=text;label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.GetComponent<MeshRenderer>().sharedMaterial=label.font.material;label.fontSize=64;label.characterSize=size*.22f;
            label.anchor=TextAnchor.MiddleCenter;label.color=Hex("465536");
        }
    }
}
