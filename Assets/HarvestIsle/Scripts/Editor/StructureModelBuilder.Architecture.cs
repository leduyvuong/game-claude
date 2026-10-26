using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        // Large, readable joinery and functional workshop silhouettes. One baked
        // static mesh per model, no runtime construction or per-detail objects.
        internal static readonly string[] ArchitectureTypes={"FarmHouse","RiceBarn","VillageWell","ChickenCoop","DuckShelter","CowShed","PigPen","Greenhouse","Nursery","Windmill","BakeryOven","DairyChurn","JuicePress","SoyVat","HerbalBench","CompostBin","BeeHive","MarketStall","VillageWarehouse"};
        private static void Architecture(GameObject root,string type,Material[] mats,List<Mesh> meshes)
        {
            if(System.Array.IndexOf(ArchitectureTypes,type)<0)return;
            var m=new EnvironmentMesh(Colors.Length);
            switch(type)
            {
                case "FarmHouse": HomeJoinery(m);break;
                case "RiceBarn": case "VillageWarehouse": StoreJoinery(m,type=="VillageWarehouse");break;
                case "VillageWell": WellJoinery(m);break;
                case "ChickenCoop": case "DuckShelter": CoopJoinery(m,type=="DuckShelter");break;
                case "CowShed": case "PigPen": StableJoinery(m,type=="PigPen");break;
                case "Greenhouse": GlasshouseJoinery(m);break;
                case "Nursery": NurseryJoinery(m);break;
                case "Windmill": MillJoinery(m);break;
                case "BakeryOven": BakeryJoinery(m);break;
                case "DairyChurn":
                    WorkshopRear(m,2.65f,1.85f,Water,true);
                    for(int i=0;i<5;i++)Box(m,new Vector3(-1.02f+i*.51f,.67f,-1.31f),new Vector3(.48f,.34f,.05f),Plaster);
                    MilkCan(m,new Vector3(-1.0f,.08f,.65f));MilkCan(m,new Vector3(-.96f,.08f,-.03f));
                    Beam(m,new Vector3(.15f,.39f,.22f),new Vector3(.32f,.39f,.48f),.045f,StoneDark);
                    break;
                case "JuicePress":
                    WorkshopRear(m,2.75f,2.0f,Tile,false);
                    for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.53f,1.1f,.05f),new Vector3(side*.28f,1.59f,.05f),.045f,Wood);
                    Beam(m,new Vector3(0,.23f,.38f),new Vector3(.38f,.23f,.65f),.045f,StoneDark);
                    Jar(m,new Vector3(.48f,.05f,.83f),.62f,Water);
                    FruitTray(m,new Vector3(-1.0f,.12f,.64f));break;
                case "SoyVat":
                    WorkshopRear(m,2.8f,1.9f,Straw,false);
                    Box(m,new Vector3(0,.85f,-1.02f),new Vector3(2.45f,.10f,.38f),WoodDark);
                    for(int i=0;i<4;i++)Jar(m,new Vector3(-.9f+i*.58f,.9f,-1.02f),.58f,i%2==0?TileDark:Water);
                    Beam(m,new Vector3(1.12f,.12f,.15f),new Vector3(.97f,1.43f,.18f),.028f,Bamboo);break;
                case "HerbalBench":
                    WorkshopRear(m,2.65f,1.95f,Cloth,false);
                    Beam(m,new Vector3(-1.05f,1.56f,-.28f),new Vector3(1.05f,1.56f,-.28f),.035f,Bamboo);
                    for(int i=0;i<5;i++)
                    {
                        Vector3 p=new Vector3(-.88f+i*.44f,1.49f,-.28f);
                        Beam(m,p+Vector3.up*.07f,p-Vector3.up*.11f,.014f,Straw);
                        for(int leaf=0;leaf<3;leaf++)Ellipsoid(m,p+new Vector3((leaf-1)*.07f,-.21f,0),new Vector3(.055f,.18f,.065f),i%2==0?Cloth:StoneDark);
                    }
                    for(int i=0;i<3;i++)Box(m,new Vector3(-.6f+i*.6f,.30f,.51f),new Vector3(.47f,.22f,.07f),WoodDark);
                    break;
                case "CompostBin":
                    for(int i=0;i<5;i++)Box(m,new Vector3(1.12f,.14f+i*.14f,-.18f),new Vector3(.60f,.10f,.07f),Wood);
                    for(int i=0;i<3;i++)Beam(m,new Vector3(1.55f+(i-1)*.07f,.08f,.35f),new Vector3(1.43f+(i-1)*.07f,.38f,.35f),.014f,StoneDark);
                    Beam(m,new Vector3(1.43f,.38f,.35f),new Vector3(1.15f,1.3f,.35f),.023f,Wood);
                    Box(m,new Vector3(0,.32f,.615f),new Vector3(.65f,.08f,.06f),Dark);break;
                case "BeeHive":
                    for(int i=0;i<3;i++)Box(m,new Vector3(-.36f,.48f+i*.24f,0),new Vector3(.025f,.05f,.42f),WoodDark);
                    for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.28f,.08f,-.28f),new Vector3(-side*.28f,.30f,-.28f),.024f);
                    Hoop(m,new Vector3(0,.62f,.355f),Vector3.forward,.10f,.016f,Gold,12);break;
                case "MarketStall":
                    for(int side=-1;side<=1;side+=2)
                    {
                        Beam(m,new Vector3(side*1.03f,1.25f,-.46f),new Vector3(side*.65f,1.74f,-.46f),.035f);
                        Beam(m,new Vector3(side*1.04f,.12f,.76f),new Vector3(side*1.04f,1.57f,.76f),.035f);
                    }
                    for(int i=0;i<10;i++)Box(m,new Vector3(-.90f+i*.20f,.50f,.60f),new Vector3(.17f,.43f,.035f),i%3==0?WoodDark:Wood);
                    Table(m,new Vector3(0,.12f,-.53f),1.72f,.33f,1.06f);
                    for(int i=0;i<5;i++)Jar(m,new Vector3(-.65f+i*.32f,1.23f,-.53f),.42f,i%2==0?Tile:Gold);
                    HangingLantern(m,new Vector3(-.90f,1.52f,.69f));break;
            }
            Craftsmanship(m,type);
            TradeSign(m,type);
            var detail=m.Finish(type+" architectural details");
            // Do not retain empty material slots in the additional renderer.
            var indices=new List<int[]>();var palette=new List<Material>();
            for(int i=0;i<detail.subMeshCount;i++)if(detail.GetIndexCount(i)>0){indices.Add(detail.GetTriangles(i));palette.Add(mats[i]);}
            detail.subMeshCount=indices.Count;for(int i=0;i<indices.Count;i++)detail.SetTriangles(indices[i],i);
            Add(root,"Architecture and joinery",detail,palette.ToArray(),meshes,false);
            root.transform.Find("Architecture and joinery").GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.On;
        }
        private static void WorkshopRear(EnvironmentMesh m,float width,float height,int roof,bool clean)
        {
            Box(m,new Vector3(0,.035f,-.15f),new Vector3(width+.12f,.07f,2.65f),clean?Stone:WoodDark);
            for(int i=0;i<9;i++)Box(m,new Vector3(-width*.44f+i*width*.11f,.079f,-.15f),new Vector3(width*.10f,.018f,2.50f),clean?Plaster:Wood);
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 p=new Vector3(side*(width*.5f-.12f),.09f,-1.23f);
                Box(m,p+Vector3.up*.08f,new Vector3(.22f,.16f,.22f),Stone);
                float rearTop=height+.24f*((1.23f-.38f)/1.12f)-.02f;
                Beam(m,p,new Vector3(p.x,rearTop,-1.23f),.065f);
                Beam(m,new Vector3(p.x,rearTop-.43f,-1.23f),new Vector3(p.x-side*.4f,rearTop-.03f,-1.23f),.042f);
                Beam(m,new Vector3(p.x,height-.45f,-1.23f),new Vector3(p.x,height-.028f,-.46f),.04f);
            }
            Beam(m,new Vector3(-width*.5f,height+.137f,-1.23f),new Vector3(width*.5f,height+.137f,-1.23f),.045f);
            for(int i=0;i<8;i++)Box(m,new Vector3(-width*.42f+i*width*.12f,.67f,-1.31f),new Vector3(width*.10f,1.14f,.07f),clean?Plaster:i%3==0?WoodDark:Wood);
            Box(m,new Vector3(0,1.21f,-1.31f),new Vector3(width,.08f,.10f),WoodDark);
            LeanRoof(m,new Vector3(0,height,-.94f),width+.23f,1.12f,.24f,roof);
        }
        private static void LeanRoof(EnvironmentMesh m,Vector3 p,float width,float depth,float rise,int color)
        {
            Face(m,p+new Vector3(-width/2,rise,-depth/2),p+new Vector3(width/2,rise,-depth/2),p+new Vector3(width/2,0,depth/2),p+new Vector3(-width/2,0,depth/2),color);
            if(width>.8f)RoofSkin(m,p+new Vector3(0,rise,-depth/2),new Vector3(0,-rise,depth),width,color);
            for(int i=0;i<=6;i++)
            {
                float x=-width/2+width*i/6;
                float under=width>.8f?-.055f:0;
                Beam(m,p+new Vector3(x,rise+under,-depth/2),p+new Vector3(x,under,depth/2),.025f,color==Tile?TileDark:WoodDark);
            }
            for(int side=-1;side<=1;side+=2)Beam(m,p+new Vector3(-width/2,side<0?rise:0,side*depth/2),p+new Vector3(width/2,side<0?rise:0,side*depth/2),.045f,WoodDark);
        }
        private static void Shutters(EnvironmentMesh m,Vector3 p,float width=.55f,float height=.64f)
        {
            Box(m,p,new Vector3(width,height,.045f),Dark);
            for(int side=-1;side<=1;side+=2)
            {
                Box(m,p+new Vector3(side*(width*.5f+.13f),0,.10f),new Vector3(.24f,height+.08f,.075f),Cloth);
                for(int i=0;i<5;i++)Box(m,p+new Vector3(side*(width*.5f+.13f),-height*.36f+i*height*.18f,.15f),new Vector3(.25f,.035f,.035f),WoodDark);
            }
            Box(m,p+Vector3.up*(height*.5f+.045f),new Vector3(width+.60f,.09f,.14f),WoodDark);
            Box(m,p-Vector3.up*(height*.5f+.03f),new Vector3(width+.24f,.10f,.19f),Stone);
        }
        private static void HomeJoinery(EnvironmentMesh m)
        {
            for(int i=0;i<13;i++)Box(m,new Vector3(-1.32f+i*.22f,.22f,.88f),new Vector3(.20f,.035f,.52f),i%3==0?WoodDark:Wood);
            for(int side=-1;side<=1;side+=2)
            {
                Box(m,new Vector3(side*1.31f,.28f,1.13f),new Vector3(.20f,.18f,.20f),Stone);
                Beam(m,new Vector3(side*1.31f,1.41f,1.13f),new Vector3(side*.93f,1.78f,1.13f),.035f);
                Shutters(m,new Vector3(side*.88f,1.19f,.655f));
                for(int row=0;row<2;row++)for(int brick=0;brick<7;brick++)Box(m,new Vector3(-1.22f+brick*.40f,.30f+row*.12f,side>0?.615f:-.965f),new Vector3(.37f,.105f,.04f),brick%3==0?StoneDark:Stone);
            }
            // Rear and side elevations remain authored when the camera rotates.
            Box(m,new Vector3(0,1.16f,-.97f),new Vector3(.8f,.65f,.06f),Dark);
            for(int i=0;i<5;i++)Box(m,new Vector3(-.36f+i*.18f,1.16f,-1.01f),new Vector3(.13f,.61f,.04f),Cloth);
            Box(m,new Vector3(1.395f,1.0f,-.23f),new Vector3(.05f,1.4f,.58f),WoodDark);
            for(int i=0;i<4;i++)Box(m,new Vector3(1.43f,1.0f,-.44f+i*.14f),new Vector3(.025f,1.32f,.11f),Wood);
            Box(m,new Vector3(-1.40f,1.2f,-.20f),new Vector3(.06f,.68f,.70f),Dark);
            for(int i=0;i<5;i++)Box(m,new Vector3(-1.445f,1.2f,-.48f+i*.14f),new Vector3(.04f,.62f,.10f),Cloth);
            Beam(m,new Vector3(-1.58f,2.0f,-1.30f),new Vector3(1.58f,2.0f,-1.30f),.04f,StoneDark);
            Beam(m,new Vector3(1.48f,2.0f,-1.30f),new Vector3(1.48f,.52f,-1.30f),.035f,StoneDark);
            Jar(m,new Vector3(1.47f,0,-1.27f),1.0f,Water);
            HangingLantern(m,new Vector3(-1.12f,1.76f,1.12f));
            FlowerPot(m,new Vector3(-1.48f,.12f,1.12f));FlowerPot(m,new Vector3(1.48f,.12f,1.12f));
        }
        private static void StoreJoinery(EnvironmentMesh m,bool village)
        {
            float width=village?3.10f:2.35f,depth=village?2.14f:1.68f,y=village?.17f:.16f;
            for(int side=-1;side<=1;side+=2)for(int row=0;row<7;row++)
                Box(m,new Vector3(side*(width/2+.01f),y+.18f+row*.23f,-.06f),new Vector3(.045f,.18f,depth-.12f),row%3==0?WoodDark:Wood);
            for(int i=0;i<9;i++)Box(m,new Vector3(-width*.44f+i*width*.11f,y+.87f,-depth/2-.01f),new Vector3(width*.09f,1.54f,.045f),WoodDark);
            Box(m,new Vector3(0,y+1.48f,depth/2+.025f),new Vector3(1.10f,.09f,.12f),StoneDark);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.34f,y+.15f,depth/2+.06f),new Vector3(-side*.34f,y+1.22f,depth/2+.06f),.035f,Wood);
            for(int i=0;i<4;i++)Box(m,new Vector3(0,.04f+i*.025f,depth/2+.54f-i*.12f),new Vector3(1.12f,.07f,.12f),Wood);
            float eave=village?1.87f:1.98f,rise=village?.40f:.48f;
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*width/2;
                Face(m,new Vector3(x,eave,-depth/2),new Vector3(x,eave+rise,0),new Vector3(x,eave,depth/2),new Vector3(x,eave,depth/2),Wood);
                for(int i=0;i<5;i++)Box(m,new Vector3(x+side*.025f,eave+.12f,-.30f+i*.15f),new Vector3(.04f,.14f,.09f),Dark);
            }
        }
        private static void WellJoinery(EnvironmentMesh m)
        {
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.62f,1.14f,0),new Vector3(side*.31f,1.48f,0),.04f);
            for(int i=0;i<8;i++)Hoop(m,new Vector3(-.16f+i*.045f,1.18f,0),Vector3.right,.075f,.012f,Straw,12);
            Beam(m,new Vector3(.70f,1.18f,0),new Vector3(.70f,1.18f,.24f),.025f,WoodDark);
            Beam(m,new Vector3(.70f,1.18f,.24f),new Vector3(.87f,1.18f,.24f),.027f,Wood);
            Cylinder(m,new Vector3(-.82f,.02f,.53f),.26f,.12f,.16f,Wood,12);
            Hoop(m,new Vector3(-.82f,.23f,.53f),Vector3.up,.15f,.016f,StoneDark,12);
            Hoop(m,new Vector3(-.82f,.29f,.53f),Vector3.forward,.13f,.014f,StoneDark,12);
        }
        private static void CoopJoinery(EnvironmentMesh m,bool duck)
        {
            float h=duck?.85f:1.2f;
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)Box(m,new Vector3(side*(duck?.79f:.66f),.20f+i*.16f,-.13f),new Vector3(.04f,.13f,.94f),i%2==0?Wood:WoodDark);
            if(!duck)
            {
                Box(m,new Vector3(.86f,.52f,-.15f),new Vector3(.43f,.37f,.65f),WoodDark);
                LeanRoof(m,new Vector3(.87f,.74f,-.15f),.55f,.78f,.10f,Tile);
                for(int i=0;i<5;i++)Box(m,new Vector3(.10f,.26f-i*.045f,.69f+i*.12f),new Vector3(.48f,.05f,.07f),Wood);
                Ellipsoid(m,new Vector3(.88f,.73f,.04f),new Vector3(.055f,.07f,.055f),Plaster);
            }
            else Trough(m,new Vector3(.70f,0,.93f),true);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.56f,h-.27f,-.56f),new Vector3(side*.30f,h,-.56f),.03f);
        }
        private static void StableJoinery(EnvironmentMesh m,bool pig)
        {
            float w=pig?1.22f:1.35f,h=pig?1.25f:1.87f;
            for(int side=-1;side<=1;side+=2)
            {
                Beam(m,new Vector3(side*w,.10f,-.67f),new Vector3(side*w,h,-.67f),.06f);
                Beam(m,new Vector3(side*w,h-.45f,-.67f),new Vector3(side*(w-.42f),h-.03f,-.67f),.04f);
                for(int row=0;row<4;row++)Box(m,new Vector3(side*w,.25f+row*.15f,-.25f),new Vector3(.065f,.11f,1.0f),Wood);
            }
            if(pig){Box(m,new Vector3(0,.03f,.35f),new Vector3(1.9f,.05f,.8f),WoodDark);Beam(m,new Vector3(1.23f,.45f,0),new Vector3(.97f,.45f,0),.03f,StoneDark);}
            else {Bale(m,new Vector3(-1.03f,.13f,-.42f),.55f);for(int i=0;i<6;i++)Beam(m,new Vector3(-.90f+i*.36f,.40f,.73f),new Vector3(-.90f+i*.36f,.83f,.60f),.023f,Wood);}
        }
        private static void GlasshouseJoinery(EnvironmentMesh m)
        {
            for(int side=-1;side<=1;side+=2)
            {
                for(int i=0;i<8;i++)Box(m,new Vector3(side*1.18f,.18f,-.84f+i*.24f),new Vector3(.12f,.21f,.215f),i%3==0?TileDark:Tile);
                Beam(m,new Vector3(-1.22f,1.69f,side*.98f),new Vector3(1.22f,1.69f,side*.98f),.04f,StoneDark);
            }
            LeanRoof(m,new Vector3(-.38f,2.13f,-.28f),.68f,.61f,.16f,Water);
            Beam(m,new Vector3(-.68f,1.98f,-.28f),new Vector3(-.68f,2.22f,-.28f),.02f,StoneDark);
            Beam(m,new Vector3(1.22f,1.69f,-.90f),new Vector3(1.22f,.50f,-.90f),.03f,StoneDark);
            Jar(m,new Vector3(1.35f,.05f,-.90f),.9f,Water);
            Box(m,new Vector3(.22f,.85f,1.02f),new Vector3(.10f,.04f,.08f),Gold);
        }
        private static void NurseryJoinery(EnvironmentMesh m)
        {
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*1.03f,1.04f,.72f),new Vector3(side*.65f,1.43f,.72f),.035f,Bamboo);
            for(int i=0;i<8;i++)Beam(m,new Vector3(-1.14f+i*.32f,1.42f,-.9f),new Vector3(-1.14f+i*.32f,1.42f,.9f),.018f,Bamboo);
            Box(m,new Vector3(0,.76f,-.83f),new Vector3(1.90f,.07f,.33f),Wood);
            for(int i=0;i<7;i++){Cylinder(m,new Vector3(-.76f+i*.25f,.80f,-.83f),.18f,.07f,.10f,Tile,8);m.Leaf(new Vector3(-.76f+i*.25f,.98f,-.83f),Vector3.forward,.16f,.04f,Cloth);}
            Jar(m,new Vector3(1.27f,.04f,.38f),.70f,Water);
        }
        private static void MillJoinery(EnvironmentMesh m)
        {
            for(int row=0;row<3;row++)for(int i=0;i<16;i++)
            {
                float a=(i+row*.5f)*Mathf.PI/8;
                float y=.16f+row*.13f,radius=.87f-(y-.13f)*(.25f/2.35f)-.025f;
                Vector3 p=new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius),tangent=new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a))*.14f;
                Beam(m,p-tangent,p+tangent,.045f,i%3==0?StoneDark:Stone);
            }
            Box(m,new Vector3(0,.60f,.96f),new Vector3(.44f,1.06f,.06f),Wood);
            for(int i=0;i<5;i++)Box(m,new Vector3(-.18f+i*.09f,.60f,1.0f),new Vector3(.025f,1.0f,.025f),WoodDark);
            Box(m,new Vector3(0,.08f,1.13f),new Vector3(.75f,.15f,.4f),Stone);
            LeanRoof(m,new Vector3(-1.16f,1.17f,-.08f),.70f,1.45f,.15f,Tile);
            for(int z=-1;z<=1;z+=2)Beam(m,new Vector3(-1.45f,.1f,z*.58f),new Vector3(-1.45f,1.17f+(.645f-z*.58f)/1.45f*.15f,z*.58f),.035f);
        }
        private static void BakeryJoinery(EnvironmentMesh m)
        {
            LeanRoof(m,new Vector3(1.12f,1.72f,-.25f),1.28f,1.15f,.20f,Tile);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(1.12f+side*.51f,.04f,-.70f),new Vector3(1.12f+side*.51f,1.9f,-.70f),.045f);
            for(int i=0;i<5;i++)Beam(m,new Vector3(.83f+i*.13f,.26f,-.01f),new Vector3(.83f+i*.13f,.26f,.56f),.055f,WoodDark);
            for(int row=0;row<3;row++)for(int i=0;i<5;i++)Box(m,new Vector3(-.90f+i*.31f,.19f+row*.15f,.70f),new Vector3(.28f,.12f,.035f),i%2==0?Tile:TileLight);
        }
        private static void MilkCan(EnvironmentMesh m,Vector3 p)
        {
            Cylinder(m,p,.43f,.16f,.16f,Plaster,12);Cylinder(m,p+Vector3.up*.43f,.13f,.16f,.09f,Stone,12);
            Cylinder(m,p+Vector3.up*.56f,.07f,.11f,.11f,StoneDark,12);
            for(int side=-1;side<=1;side+=2)Hoop(m,p+new Vector3(side*.15f,.42f,0),Vector3.forward,.07f,.015f,StoneDark,10);
        }
        private static void FruitTray(EnvironmentMesh m,Vector3 p)
        {
            Box(m,p,new Vector3(.52f,.10f,.61f),WoodDark);
            for(int i=0;i<6;i++)Ellipsoid(m,p+new Vector3((i%2-.5f)*.20f,.13f,(i/2-1)*.18f),Vector3.one*.10f,i%2==0?Gold:Red);
        }
        private static void FlowerPot(EnvironmentMesh m,Vector3 p)
        {
            Cylinder(m,p,.28f,.14f,.20f,Tile,12);Hoop(m,p+Vector3.up*.28f,Vector3.up,.20f,.025f,TileDark,12);
            for(int i=0;i<5;i++){float a=i*2.4f;Vector3 q=p+new Vector3(Mathf.Cos(a)*.11f,.43f,Mathf.Sin(a)*.11f);Beam(m,p+Vector3.up*.25f,q,.012f,Cloth);Ellipsoid(m,q,new Vector3(.06f,.035f,.06f),i%2==0?Gold:Red);}
        }
    }
}
