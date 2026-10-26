using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        private static void CraftBlock(EnvironmentMesh m,Vector3 p,Vector3 size,int color,int edge=-1)
        {
            Vector3 h=size*.5f;float cut=Mathf.Min(.035f,Mathf.Min(h.x,h.z)*.27f),bevel=Mathf.Min(.016f,h.y*.20f);
            var outline=new[]{new Vector2(-h.x+cut,-h.z),new Vector2(h.x-cut,-h.z),new Vector2(h.x,-h.z+cut),new Vector2(h.x,h.z-cut),new Vector2(h.x-cut,h.z),new Vector2(-h.x+cut,h.z),new Vector2(-h.x,h.z-cut),new Vector2(-h.x,-h.z+cut)};
            for(int i=0;i<8;i++)
            {
                Vector2 a=outline[i],b=outline[(i+1)%8];
                Vector3 lowA=p+new Vector3(a.x,-h.y,a.y),lowB=p+new Vector3(b.x,-h.y,b.y),highA=lowA+Vector3.up*(size.y-bevel),highB=lowB+Vector3.up*(size.y-bevel);
                Vector3 capA=p+new Vector3(a.x*(1-bevel/Mathf.Max(.05f,h.x)),h.y,a.y*(1-bevel/Mathf.Max(.05f,h.z))),capB=p+new Vector3(b.x*(1-bevel/Mathf.Max(.05f,h.x)),h.y,b.y*(1-bevel/Mathf.Max(.05f,h.z)));
                m.Quad(lowB,lowA,highA,highB,color);m.Quad(highB,highA,capA,capB,edge<0?color:edge);m.Tri(p+Vector3.up*h.y,capB,capA,color);
            }
        }
        private static void BoltZ(EnvironmentMesh m,Vector3 p,float radius=.024f)
        {m.Tube(new[]{p,p+Vector3.forward*.018f},radius,radius,StoneDark,6);}
        private static void StrapZ(EnvironmentMesh m,Vector3 p,float width)
        {
            CraftBlock(m,p,new Vector3(width,.075f,.035f),StoneDark);
            BoltZ(m,p+new Vector3(-width*.34f,0,.025f));BoltZ(m,p+new Vector3(width*.34f,0,.025f));
        }
        private static void GrainZ(EnvironmentMesh m,Vector3 p,float width,float height,int seed)
        {
            for(int line=0;line<2;line++)for(int segment=0;segment<4;segment++)
            {
                float y=-height*.4f+segment*height*.2f,x=(line==0?-.22f:.23f)*width;
                float a=Mathf.Sin((seed+segment)*1.7f)*width*.08f,b=Mathf.Sin((seed+segment+1)*1.7f)*width*.08f;
                Face(m,p+new Vector3(x+a-.003f,y,0),p+new Vector3(x+a+.003f,y,0),p+new Vector3(x+b+.003f,y+height*.19f,0),p+new Vector3(x+b-.003f,y+height*.19f,0),WoodDark);
            }
        }
        private static void DoorHardware(EnvironmentMesh m,Vector3 p,float width,float height)
        {
            for(int side=-1;side<=1;side+=2)
            {
                CraftBlock(m,p+new Vector3(side*(width*.5f+.045f),0,0),new Vector3(.095f,height+.16f,.105f),WoodDark);
                StrapZ(m,p+new Vector3(-width*.27f,side*height*.30f,.07f),width*.34f);
            }
            CraftBlock(m,p+new Vector3(0,height*.5f+.055f,.01f),new Vector3(width+.25f,.13f,.16f),Wood);
            Box(m,p+new Vector3(width*.27f,-.04f,.08f),new Vector3(.09f,.18f,.025f),StoneDark);
            Hoop(m,p+new Vector3(width*.27f,-.06f,.12f),Vector3.forward,.055f,.012f,Gold,12);
            for(int i=0;i<5;i++)GrainZ(m,p+new Vector3(-width*.38f+i*width*.19f,0,.065f),width*.18f,height*.9f,i+7);
        }
        private static void CornerMasonry(EnvironmentMesh m,float x,float z,float floor,int rows)
        {
            for(int row=0;row<rows;row++)for(int side=-1;side<=1;side+=2)for(int back=-1;back<=1;back+=2)
            {
                float y=floor+row*.25f;
                CraftBlock(m,new Vector3(side*x,y,z+back*.765f),new Vector3(row%2==0?.29f:.17f,.18f,row%2==0?.17f:.29f),row%3==0?Stone:Plaster,Plaster);
            }
        }
        private static void Craftsmanship(EnvironmentMesh m,string type)
        {
            switch(type)
            {
                case "FarmHouse":
                    CornerMasonry(m,1.36f,-.17f,.38f,6);
                    DoorHardware(m,new Vector3(0,.98f,.69f),.67f,1.55f);
                    for(int side=-1;side<=1;side+=2)
                    {
                        for(int row=0;row<3;row++)CraftBlock(m,new Vector3(side*1.31f,.25f+row*.11f,1.13f),new Vector3(.24f-row*.035f,.10f,.24f-row*.035f),Stone,Plaster);
                        StrapZ(m,new Vector3(side*1.31f,1.48f,1.185f),.16f);
                        for(int i=0;i<5;i++)CraftBlock(m,new Vector3(side*1.31f,.54f,.64f+i*.11f),new Vector3(.09f,.53f,.045f),Wood);
                        CraftBlock(m,new Vector3(side*1.31f,.83f,.86f),new Vector3(.14f,.09f,.63f),WoodDark);
                    }
                    for(int row=0;row<3;row++)for(int col=0;col<8;col++)CraftBlock(m,new Vector3(-1.18f+col*.34f,.244f,.70f+row*.17f),new Vector3(.32f,.025f,.15f),((col+row)%4==0)?TileDark:Tile);
                    // Carved ventilation panel is contained in the side gable.
                    for(int side=-1;side<=1;side+=2)
                    {
                        Box(m,new Vector3(side*1.393f,2.17f,-.17f),new Vector3(.04f,.27f,.60f),Dark);
                        for(int i=0;i<7;i++)Box(m,new Vector3(side*1.42f,2.17f,-.42f+i*.083f),new Vector3(.035f,.25f,.035f),Wood);
                        for(int i=0;i<3;i++)Box(m,new Vector3(side*1.42f,2.07f+i*.10f,-.17f),new Vector3(.035f,.026f,.61f),Wood);
                    }
                    break;
                case "RiceBarn":case "VillageWarehouse":
                    bool village=type=="VillageWarehouse";float width=village?3.1f:2.35f,depth=village?2.14f:1.68f;
                    DoorHardware(m,new Vector3(0,.85f,depth/2+.075f),.78f,1.26f);
                    for(int side=-1;side<=1;side+=2)
                    {
                        for(int row=0;row<3;row++)CraftBlock(m,new Vector3(side*(width/2-.08f),.24f+row*.13f,depth/2+.02f),new Vector3(.29f,.11f,.12f),Stone,Plaster);
                        for(int row=0;row<6;row++)StrapZ(m,new Vector3(side*(width/2-.10f),.30f+row*.25f,depth/2+.07f),.18f);
                    }
                    for(int i=0;i<9;i++)GrainZ(m,new Vector3(-width*.44f+i*width*.11f,.95f,-depth/2-.04f),width*.09f,1.45f,i);
                    break;
                case "Windmill": MillCraft(m);break;
                case "BakeryOven": OvenCraft(m);break;
                case "DairyChurn":case "JuicePress":case "SoyVat":case "HerbalBench": WorkshopCraft(m,type);break;
                case "VillageWell": WellCraft(m);break;
                case "ChickenCoop":case "DuckShelter":
                    float floor=type=="ChickenCoop"?.29f:.05f;
                    DoorHardware(m,new Vector3(.10f,floor+.25f,.46f),.41f,.46f);
                    for(int i=0;i<7;i++)GrainZ(m,new Vector3(-.58f+i*.18f,floor+.63f,.44f),.15f,.32f,i);
                    for(int side=-1;side<=1;side+=2)StrapZ(m,new Vector3(side*.55f,floor+.35f,.46f),.17f);
                    break;
                case "CowShed":case "PigPen":
                    for(int side=-1;side<=1;side+=2)
                    {
                        for(int row=0;row<3;row++)Hoop(m,new Vector3(side*1.21f,.44f+row*.05f,.92f),Vector3.up,.075f,.012f,Straw,10);
                        CraftBlock(m,new Vector3(side*1.20f,.12f,.90f),new Vector3(.23f,.16f,.23f),Stone);
                    }
                    for(int i=0;i<7;i++)Box(m,new Vector3(-.29f+i*.15f,.31f,.62f),new Vector3(.025f,.20f,.34f),WoodDark);
                    break;
                case "Nursery":case "Greenhouse": GardenCraft(m,type=="Greenhouse");break;
                case "CompostBin":
                    for(int side=-1;side<=1;side+=2)for(int row=0;row<5;row++)BoltZ(m,new Vector3(side*.67f,.19f+row*.15f,.615f));
                    for(int i=0;i<3;i++)CraftBlock(m,new Vector3(-.50f+i*.50f,.34f,.63f),new Vector3(.44f,.10f,.04f),WoodDark);
                    StrapZ(m,new Vector3(0,.61f,.65f),.32f);break;
                case "BeeHive":
                    for(int row=0;row<3;row++)for(int side=-1;side<=1;side+=2)
                    {
                        Box(m,new Vector3(side*.34f,.48f+row*.24f,.348f),new Vector3(.075f,.19f,.028f),Wood);
                        BoltZ(m,new Vector3(side*.34f,.48f+row*.24f,.37f),.015f);
                    }
                    for(int i=0;i<4;i++)GrainZ(m,new Vector3(-.25f+i*.16f,.74f,.356f),.13f,.52f,i);break;
                case "MarketStall":
                    for(int i=0;i<10;i++)GrainZ(m,new Vector3(-.90f+i*.20f,.5f,.62f),.17f,.39f,i);
                    for(int side=-1;side<=1;side+=2)StrapZ(m,new Vector3(side*1.04f,1.25f,.80f),.16f);
                    for(int row=0;row<3;row++)for(int col=0;col<3;col++)CraftBlock(m,new Vector3(-.65f+col*.65f,.88f,-.04f+row*.25f),new Vector3(.59f,.035f,.21f),Wood);
                    break;
            }
        }
        private static void WorkshopCraft(EnvironmentMesh m,string type)
        {
            float width=type=="JuicePress"?2.75f:type=="SoyVat"?2.8f:2.65f;
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*(width*.5f-.12f);
                CraftBlock(m,new Vector3(x,.105f,-1.23f),new Vector3(.30f,.12f,.30f),StoneDark,Stone);
                CraftBlock(m,new Vector3(x,.21f,-1.23f),new Vector3(.23f,.09f,.23f),Stone,Plaster);
                StrapZ(m,new Vector3(x,1.42f,-1.14f),.18f);
            }
            // Exposed floor joints, sunk fasteners and two front steps.
            for(int col=0;col<9;col++)
            {
                float x=-width*.44f+col*width*.11f;
                Box(m,new Vector3(x,.093f,(col%3-1)*.56f),new Vector3(width*.095f,.008f,.015f),WoodDark);
                for(int z=-1;z<=1;z+=2)Cylinder(m,new Vector3(x,.093f,z*1.03f),.009f,.018f,.018f,StoneDark,6);
            }
            for(int i=0;i<2;i++)CraftBlock(m,new Vector3(0,.025f+i*.024f,1.31f-i*.11f),new Vector3(.95f,.05f,.19f),Stone,Plaster);
            if(type=="DairyChurn"||type=="JuicePress")
            {
                float cx=type=="DairyChurn"?-.18f:0,h=type=="DairyChurn"?.94f:.53f;
                for(int ring=0;ring<2;ring++)for(int bolt=0;bolt<12;bolt++)
                {
                    float a=bolt*Mathf.PI/6,y=.27f+ring*h*.55f,r=type=="DairyChurn"?.40f-(y-.12f)/.94f*.06f+.004f:.382f;Vector3 p=new Vector3(cx+Mathf.Cos(a)*r,y,Mathf.Sin(a)*r);
                    m.Tube(new[]{p,p+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.035f},.022f,.022f,StoneDark,6);
                }
                if(type=="JuicePress")
                {
                    CraftBlock(m,new Vector3(0,1.73f,0),new Vector3(.33f,.09f,.27f),StoneDark,Stone);
                    for(int side=-1;side<=1;side+=2)StrapZ(m,new Vector3(side*.54f,1.48f,.075f),.15f);
                }
            }
            if(type=="HerbalBench")for(int i=0;i<3;i++)
            {StrapZ(m,new Vector3(-.6f+i*.6f,.3f,.565f),.16f);Hoop(m,new Vector3(-.6f+i*.6f,.28f,.60f),Vector3.forward,.045f,.01f,Gold,10);}
            if(type=="SoyVat")for(int i=0;i<2;i++)
            {Hoop(m,new Vector3(-.39f+i*.79f,.18f,-.05f),Vector3.up,.345f,.023f,StoneDark,20);}
        }
        private static void MillCraft(EnvironmentMesh m)
        {
            // Masonry patches follow the taper and remain below the sail sweep.
            for(int row=0;row<5;row++)for(int i=0;i<16;i++)
            {
                if((i*3+row*7)%11>3)continue;
                float y=.64f+row*.23f,r=.87f-(y-.13f)*(.25f/2.35f)+.007f,a=(i+row*.5f)*Mathf.PI/8,b=a+.27f;
                Face(m,new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r),new Vector3(Mathf.Cos(b)*r,y,Mathf.Sin(b)*r),new Vector3(Mathf.Cos(b)*(r-.015f),y+.12f,Mathf.Sin(b)*(r-.015f)),new Vector3(Mathf.Cos(a)*(r-.015f),y+.12f,Mathf.Sin(a)*(r-.015f)),row%2==0?Stone:Plaster);
            }
            DoorHardware(m,new Vector3(0,.60f,1.025f),.44f,1.06f);
            for(int ring=0;ring<4;ring++)
            {
                float y=2.52f+ring*.14f,r=.89f*(1-(y-2.47f)/.66f)+.01f;
                Hoop(m,new Vector3(0,y,0),Vector3.up,r,.018f,TileDark,24);
            }
        }
        private static void OvenCraft(EnvironmentMesh m)
        {
            for(int row=0;row<5;row++)
            {
                for(int i=0;i<6;i++)
                {
                    float x=-.99f+i*.285f+(row%2)*.045f;
                    if(x>-.74f&&x<.20f)continue;
                    CraftBlock(m,new Vector3(x,.20f+row*.085f,.76f),new Vector3(.255f,.065f,.045f),i%3==0?TileLight:Tile,TileDark);
                }
                for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)CraftBlock(m,new Vector3(-.27f+side*.87f,.20f+row*.085f,-.59f+i*.29f),new Vector3(.045f,.065f,.26f),i%4==0?TileDark:Tile);
            }
            for(int row=0;row<6;row++)for(int side=-1;side<=1;side+=2)CraftBlock(m,new Vector3(-.63f+side*.16f,1.24f+row*.18f,-.24f),new Vector3(.035f,.14f,.33f),row%3==0?TileDark:TileLight);
            CraftBlock(m,new Vector3(-.63f,2.36f,-.24f),new Vector3(.31f,.045f,.33f),Black);
            for(int i=0;i<4;i++)Beam(m,new Vector3(.81f+i*.17f,.97f,.70f),new Vector3(.81f+i*.17f,.97f,1.0f),.018f,StoneDark);
            Beam(m,new Vector3(.79f,.97f,.72f),new Vector3(1.34f,.97f,.72f),.025f,WoodDark);
        }
        private static void WellCraft(EnvironmentMesh m)
        {
            for(int row=0;row<3;row++)for(int i=0;i<18;i++)
            {
                float a=(i+row*.5f)*Mathf.PI/9,b=a+.30f,y=.09f+row*.18f;
                Face(m,new Vector3(Mathf.Cos(a)*.558f,y,Mathf.Sin(a)*.558f),new Vector3(Mathf.Cos(b)*.558f,y,Mathf.Sin(b)*.558f),new Vector3(Mathf.Cos(b)*.558f,y+.14f,Mathf.Sin(b)*.558f),new Vector3(Mathf.Cos(a)*.558f,y+.14f,Mathf.Sin(a)*.558f),i%5==0?StoneDark:Stone);
            }
            for(int i=0;i<6;i++)
            {
                float a=2.6f+i*.22f;
                Ellipsoid(m,new Vector3(Mathf.Cos(a)*.56f,.06f,Mathf.Sin(a)*.56f),new Vector3(.09f,.025f,.08f),Cloth);
            }
            for(int side=-1;side<=1;side+=2)StrapZ(m,new Vector3(side*.62f,.98f,.065f),.18f);
        }
        private static void GardenCraft(EnvironmentMesh m,bool glass)
        {
            for(int side=-1;side<=1;side+=2)
            {
                for(int row=0;row<4;row++)
                {
                    float z=-.49f+row*.32f;
                    Box(m,new Vector3(side*.65f,.71f,z+.08f),new Vector3(.14f,.10f,.018f),Plaster);
                    Beam(m,new Vector3(side*.65f,.49f,z+.08f),new Vector3(side*.65f,.68f,z+.08f),.012f,Wood);
                }
                if(glass)for(int z=-1;z<=1;z+=2)CraftBlock(m,new Vector3(side*1.18f,.12f,z*.94f),new Vector3(.23f,.15f,.23f),StoneDark);
            }
            if(glass)
            {
                for(int side=-1;side<=1;side+=2)for(int z=-1;z<=1;z+=2)Beam(m,new Vector3(side*1.18f,.16f,z*.94f),new Vector3(side*1.18f,.67f,z*.55f),.018f,StoneDark);
                Box(m,new Vector3(.38f,1.05f,.99f),new Vector3(.08f,.25f,.035f),Plaster);
                Box(m,new Vector3(.38f,1.04f,1.015f),new Vector3(.018f,.16f,.015f),Red);
            }
            else
            {
                for(int side=-1;side<=1;side+=2)StrapZ(m,new Vector3(side*1.03f,1.20f,.77f),.15f);
                for(int i=0;i<4;i++){Cylinder(m,new Vector3(-.30f+i*.20f,.08f,-.02f),.23f,.065f,.11f,Tile,10);Hoop(m,new Vector3(-.30f+i*.20f,.31f,-.02f),Vector3.up,.11f,.015f,TileLight,10);}
            }
        }
    }
}
