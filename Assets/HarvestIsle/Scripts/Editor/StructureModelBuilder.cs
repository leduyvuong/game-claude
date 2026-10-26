using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        internal const int Version=2;
        internal static readonly string[] Names={"FarmHouse","RiceBarn","ChickenCoop","DuckShelter","CowShed","PigPen","Greenhouse","Nursery",
            "BambooFence","BambooCorner","BambooGate","Scarecrow","FeedingTrough","WaterTrough","HayBale","SupplyCrate","LanternPost","VillageWell"};
        internal static readonly string[] Labels={"Nhà mái ngói","Kho lúa","Chuồng gà","Chuồng vịt","Chuồng bò","Chuồng heo","Nhà kính","Vườn ươm",
            "Hàng rào tre","Góc rào tre","Cổng tre","Bù nhìn","Máng ăn","Máng nước","Kiện rơm","Thùng nông sản","Cột đèn lồng","Giếng làng"};
        internal static readonly string[] Groups={"Nhà cửa","Nhà cửa","Chuồng trại","Chuồng trại","Chuồng trại","Chuồng trại","Trồng trọt","Trồng trọt",
            "Đồ dùng","Đồ dùng","Đồ dùng","Trồng trọt","Chuồng trại","Chuồng trại","Đồ dùng","Đồ dùng","Đồ dùng","Nhà cửa"};
        private static readonly string[] Colors={"E5D6B5","BD9C72","896343","5C4635","B86D4C","D08456","965236","D8BB73","89937C","5E705E","668C6C","C9BD90","82B6B2","B85242","F0CE79","3B4237"};
        private const int Plaster=0,Wood=1,WoodDark=2,Dark=3,Tile=4,TileLight=5,TileDark=6,Straw=7,Stone=8,StoneDark=9,Cloth=10,Bamboo=11,Water=12,Red=13,Gold=14,Black=15;

        internal static Material[] Materials()
        {
            var materials=new Material[Colors.Length+1];
            for(int i=0;i<Colors.Length;i++)
            {
                var m=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));m.name="Structures "+Colors[i];
                m.SetColor("_BaseColor",FarmWorld.Hex(Colors[i]));m.SetColor("_SpecColor",Color.black);m.SetFloat("_Smoothness",0.04f);materials[i]=m;
            }
            var glass=new Material(Shader.Find("Universal Render Pipeline/Simple Lit")){name="Greenhouse glass",renderQueue=3000};
            glass.SetColor("_BaseColor",new Color(0.45f,0.72f,0.73f,0.48f));glass.SetColor("_SpecColor",Color.black);glass.SetFloat("_BlendModePreserveSpecular",0);glass.SetFloat("_Smoothness",0.12f);
            glass.SetFloat("_Surface",1);glass.SetFloat("_SrcBlend",5);glass.SetFloat("_DstBlend",10);glass.SetFloat("_ZWrite",0);
            glass.SetOverrideTag("RenderType","Transparent");glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");materials[Colors.Length]=glass;
            return materials;
        }
        internal static GameObject Create(string name,Material[] materials,List<Mesh> meshes,bool upgradedRoof=false)
        {
            var root=new GameObject(name);var solid=new EnvironmentMesh(Colors.Length);EnvironmentMesh glass=null;
            switch(name)
            {
                case "FarmHouse": House(solid);break;
                case "RiceBarn": Barn(solid);break;
                case "ChickenCoop": Coop(solid,false);break;
                case "DuckShelter": Coop(solid,true);break;
                case "CowShed": Shed(solid);break;
                case "PigPen": Pen(solid,upgradedRoof);break;
                case "Greenhouse": glass=new EnvironmentMesh(1);Greenhouse(solid,glass);break;
                case "Nursery": Nursery(solid);break;
                case "BambooFence": Fence(solid,Vector3.zero,false,1.8f,0.82f);break;
                case "BambooCorner": Fence(solid,new Vector3(0,0,-0.9f),false,1.8f,0.82f);Fence(solid,new Vector3(-0.9f,0,0),true,1.8f,0.82f);break;
                case "BambooGate": Gate(solid);break;
                case "Scarecrow": Scarecrow(solid);break;
                case "FeedingTrough": Trough(solid,Vector3.zero,false);break;
                case "WaterTrough": Trough(solid,Vector3.zero,true);break;
                case "HayBale": Bale(solid,Vector3.zero,1);break;
                case "SupplyCrate": Crate(solid,Vector3.zero);break;
                case "LanternPost": Lantern(solid);break;
                case "VillageWell": Well(solid);break;
            }
            Add(root,"Structure",solid.Finish(name),Slice(materials),meshes,true);
            if(glass!=null)Add(root,"Glass panels",glass.Finish(name+" glass"),new[]{materials[Colors.Length]},meshes,false);
            Architecture(root,name,materials,meshes);
            return root;
        }
        private static Material[] Slice(Material[] source){var result=new Material[Colors.Length];System.Array.Copy(source,result,result.Length);return result;}
        private static void Add(GameObject root,string name,Mesh mesh,Material[] materials,List<Mesh> meshes,bool collider)
        {
            var part=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(root.transform,false);
            part.GetComponent<MeshFilter>().sharedMesh=mesh;var r=part.GetComponent<MeshRenderer>();r.sharedMaterials=materials;r.shadowCastingMode=collider?ShadowCastingMode.On:ShadowCastingMode.Off;
            if(collider)part.AddComponent<MeshCollider>().sharedMesh=mesh;meshes.Add(mesh);
        }
        private static void Box(EnvironmentMesh mesh,Vector3 center,Vector3 size,int color){mesh.Cube(center,size,color,color);}
        private static void Beam(EnvironmentMesh mesh,Vector3 a,Vector3 b,float radius,int color=WoodDark){mesh.Tube(new[]{a,b},radius,radius,color,8);}
        private static void Face(EnvironmentMesh mesh,Vector3 a,Vector3 b,Vector3 c,Vector3 d,int color)
        {mesh.Quad(a,b,c,d,color);mesh.Quad(d,c,b,a,color);}
        private static void Roof(EnvironmentMesh mesh,float width,float depth,float eave,float rise,bool tiled=true,Vector3 offset=default(Vector3))
        {
            for(int side=-1;side<=1;side+=2)
            {
                float z=side*depth*0.5f;
                Face(mesh,offset+new Vector3(-width/2,eave,z),offset+new Vector3(-width/2,eave+rise,0),offset+new Vector3(width/2,eave+rise,0),offset+new Vector3(width/2,eave,z),tiled?TileDark:Straw);
                RoofSkin(mesh,offset+new Vector3(0,eave+rise,0),new Vector3(0,-rise,z),width,tiled?Tile:Straw);
                Beam(mesh,offset+new Vector3(-width/2,eave,z),offset+new Vector3(width/2,eave,z),0.035f,tiled?TileDark:Wood);
            }
            Beam(mesh,offset+new Vector3(-width/2,eave+rise+0.025f,0),offset+new Vector3(width/2,eave+rise+0.025f,0),0.065f,tiled?Tile:WoodDark);
            RoofCaps(mesh,offset+new Vector3(0,eave+rise+.025f,0),width,tiled);
        }
        private static void Window(EnvironmentMesh mesh,float x,float y,float z,float width=0.56f)
        {
            Box(mesh,new Vector3(x,y,z),new Vector3(width,0.62f,0.025f),Water);
            for(int side=-1;side<=1;side+=2)
            {Box(mesh,new Vector3(x+side*width/2,y,z+0.02f),new Vector3(.045f,.69f,.05f),WoodDark);Box(mesh,new Vector3(x,y+side*.33f,z+.02f),new Vector3(width+.08f,.045f,.05f),WoodDark);}
            Box(mesh,new Vector3(x,y,z+.03f),new Vector3(.035f,.65f,.04f),WoodDark);
        }
        private static void House(EnvironmentMesh mesh)
        {
            Box(mesh,new Vector3(0,.10f,0),new Vector3(3,.20f,2.2f),Stone);
            Box(mesh,new Vector3(0,1.10f,-.17f),new Vector3(2.75f,1.8f,1.55f),Plaster);
            for(int side=-1;side<=1;side+=2)
                Face(mesh,new Vector3(side*1.38f,2,-.945f),new Vector3(side*1.38f,2.63f,-.17f),new Vector3(side*1.38f,2,.605f),new Vector3(side*1.38f,2,.605f),Plaster);
            Roof(mesh,3.15f,2.30f,2.02f,.61f,true,new Vector3(0,0,-.17f));
            Box(mesh,new Vector3(0,.98f,.62f),new Vector3(.67f,1.55f,.065f),WoodDark);
            for(int i=0;i<5;i++)Box(mesh,new Vector3(-.28f+i*.14f,.98f,.662f),new Vector3(.115f,1.50f,.015f),Wood);
            Box(mesh,new Vector3(.22f,.93f,.683f),new Vector3(.040f,.040f,.03f),Gold);
            Window(mesh,-.88f,1.19f,.62f);Window(mesh,.88f,1.19f,.62f);
            Roof(mesh,2.95f,.85f,1.73f,.18f,true,new Vector3(0,0,.88f));
            for(int side=-1;side<=1;side+=2)Beam(mesh,new Vector3(side*1.31f,.20f,1.13f),new Vector3(side*1.31f,1.79f,1.13f),.052f);
            Box(mesh,new Vector3(0,.07f,1.33f),new Vector3(1.1f,.14f,.4f),StoneLightColor());
            Box(mesh,new Vector3(0,.10f,1.15f),new Vector3(1.1f,.20f,.26f),Stone);
        }
        private static int StoneLightColor(){return Plaster;}
        private static void Barn(EnvironmentMesh mesh)
        {
            Box(mesh,new Vector3(0,.08f,0),new Vector3(2.6f,.16f,1.85f),Stone);
            Box(mesh,new Vector3(0,1.03f,-.06f),new Vector3(2.35f,1.9f,1.56f),WoodDark);
            for(int i=0;i<15;i++)Box(mesh,new Vector3(-1.1f+i*.157f,1.05f,.735f),new Vector3(.125f,1.8f,.025f),i%4==0?WoodDark:Wood);
            Box(mesh,new Vector3(0,.91f,.765f),new Vector3(.84f,1.54f,.05f),Dark);
            Beam(mesh,new Vector3(-.40f,.22f,.806f),new Vector3(.40f,1.66f,.806f),.032f,Wood);
            Beam(mesh,new Vector3(.40f,.22f,.808f),new Vector3(-.40f,1.66f,.808f),.032f,Wood);
            Roof(mesh,2.75f,2.10f,1.98f,.48f,true,new Vector3(0,0,-.06f));
            Bale(mesh,new Vector3(-.89f,.16f,1.07f),.6f);Crate(mesh,new Vector3(.80f,.16f,1.05f));
        }
        private static void Coop(EnvironmentMesh mesh,bool duck)
        {
            float width=duck?1.65f:1.40f,height=duck?0.87f:1.10f,floor=duck?.05f:.29f;
            Box(mesh,new Vector3(0,floor,0),new Vector3(width,.09f,1.20f),WoodDark);
            if(!duck)for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(mesh,new Vector3(x*.57f,0,z*.46f),new Vector3(x*.57f,.32f,z*.46f),.055f);
            Box(mesh,new Vector3(0,floor+height*.46f,-.1f),new Vector3(width-.1f,height,1.03f),Wood);
            Box(mesh,new Vector3(.10f,floor+.25f,.432f),new Vector3(.41f,.46f,.027f),Dark);
            Window(mesh,-.43f,floor+.51f,.438f,.29f);
            Roof(mesh,width+.22f,1.43f,floor+height+.04f,.31f,!duck,new Vector3(0,0,-.1f));
            Face(mesh,new Vector3(-.16f,floor+.02f,.46f),new Vector3(.35f,floor+.02f,.46f),new Vector3(.35f,.025f,1.15f),new Vector3(-.16f,.025f,1.15f),WoodDark);
            for(int i=0;i<5;i++)Beam(mesh,new Vector3(-.15f,Mathf.Lerp(floor,.025f,i/5f)+.025f,.50f+i*.13f),new Vector3(.34f,Mathf.Lerp(floor,.025f,i/5f)+.025f,.50f+i*.13f),.016f,Wood);
            if(!duck){Box(mesh,new Vector3(.62f,floor+.3f,-.08f),new Vector3(.44f,.35f,.7f),WoodDark);Roof(mesh,.47f,.73f,floor+.48f,.05f,false,new Vector3(.62f,0,-.08f));}
        }
        private static void Shed(EnvironmentMesh mesh)
        {
            Box(mesh,new Vector3(0,.05f,0),new Vector3(3.0f,.10f,2.2f),Stone);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(mesh,new Vector3(x*1.30f,.10f,z*.92f),new Vector3(x*1.30f,1.78f,z*.92f),.065f);
            Box(mesh,new Vector3(0,.66f,-.92f),new Vector3(2.65f,1.10f,.08f),Wood);
            for(int side=-1;side<=1;side+=2)Fence(mesh,new Vector3(side*1.30f,.10f,0),true,1.8f,.65f);
            Roof(mesh,3.20f,2.45f,1.68f,.48f,true);Trough(mesh,new Vector3(.15f,.10f,.63f),false);
            Bale(mesh,new Vector3(-.82f,.10f,-.50f),.8f);
        }
        private static void Pen(EnvironmentMesh mesh,bool tiled=false)
        {
            Box(mesh,new Vector3(0,.035f,0),new Vector3(2.65f,.07f,2.1f),Stone);
            Fence(mesh,new Vector3(0,.07f,-.96f),false,2.45f,.70f);
            Fence(mesh,new Vector3(-1.22f,.07f,0),true,1.95f,.70f);Fence(mesh,new Vector3(1.22f,.07f,0),true,1.95f,.70f);
            Fence(mesh,new Vector3(0,.07f,.96f),false,2.45f,.55f);
            for(int side=-1;side<=1;side+=2)Beam(mesh,new Vector3(side*1.14f,.08f,-.85f),new Vector3(side*1.14f,1.19f,-.85f),.055f);
            Roof(mesh,2.62f,1.10f,1.11f,.22f,tiled,new Vector3(0,0,-.53f));Trough(mesh,new Vector3(.33f,.08f,.60f),false);
        }
        private static void Greenhouse(EnvironmentMesh mesh,EnvironmentMesh glass)
        {
            Box(mesh,new Vector3(0,.05f,0),new Vector3(2.55f,.10f,2.1f),Stone);
            for(int side=-1;side<=1;side+=2)
            {
                Face(glass,new Vector3(side*1.18f,.12f,-.94f),new Vector3(side*1.18f,1.70f,-.94f),new Vector3(side*1.18f,1.70f,.94f),new Vector3(side*1.18f,.12f,.94f),0);
                for(int z=0;z<4;z++)Beam(mesh,new Vector3(side*1.18f,.12f,-.94f+z*.626f),new Vector3(side*1.18f,1.72f,-.94f+z*.626f),.022f,StoneDark);
                Face(glass,new Vector3(-1.18f,1.7f,side*.94f),new Vector3(-1.18f,2.15f,0),new Vector3(1.18f,2.15f,0),new Vector3(1.18f,1.7f,side*.94f),0);
                Face(glass,new Vector3(-1.18f,.12f,side*.94f),new Vector3(-1.18f,1.7f,side*.94f),new Vector3(1.18f,1.7f,side*.94f),new Vector3(1.18f,.12f,side*.94f),0);
                for(int x=0;x<5;x++)Beam(mesh,new Vector3(-1.18f+x*.59f,1.7f,side*.94f),new Vector3(-1.18f+x*.59f,2.15f,0),.021f,StoneDark);
                Beam(mesh,new Vector3(-1.18f,.92f,side*.94f),new Vector3(1.18f,.92f,side*.94f),.021f,StoneDark);
            }
            Beam(mesh,new Vector3(-1.20f,2.15f,0),new Vector3(1.20f,2.15f,0),.035f,StoneDark);
            for(int side=-1;side<=1;side+=2)
            {Box(mesh,new Vector3(side*.33f,.92f,.96f),new Vector3(.033f,1.62f,.04f),StoneDark);Box(mesh,new Vector3(0,1.73f,.96f),new Vector3(.71f,.035f,.04f),StoneDark);}
            Planters(mesh,new Vector3(0,.1f,0),true);
        }
        private static void Nursery(EnvironmentMesh mesh)
        {
            Box(mesh,new Vector3(0,.035f,0),new Vector3(2.3f,.07f,1.72f),Stone);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(mesh,new Vector3(x*1.03f,.07f,z*.72f),new Vector3(x*1.03f,1.44f,z*.72f),.042f);
            Roof(mesh,2.55f,1.9f,1.44f,.20f,false);Planters(mesh,new Vector3(0,.10f,0),false);
        }
        private static void Planters(EnvironmentMesh mesh,Vector3 offset,bool greenhouse)
        {
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 p=offset+new Vector3(side*.65f,.42f,0);
                Box(mesh,p,new Vector3(.48f,.10f,1.38f),Wood);for(int z=-1;z<=1;z+=2)Beam(mesh,p+new Vector3(0,-.42f,z*.53f),p+new Vector3(0,-.02f,z*.53f),.033f);
                for(int row=0;row<4;row++)
                {
                    Vector3 pot=p+new Vector3(0,.09f,-.49f+row*.32f);Box(mesh,pot,new Vector3(.29f,.075f,.23f),Dark);
                    for(int seed=0;seed<3;seed++)
                    {Vector3 basePoint=pot+new Vector3((seed-1)*.07f,.037f,0);mesh.Leaf(basePoint,new Vector3(1,0,.4f),greenhouse?.15f:.09f,.033f,Cloth);mesh.Leaf(basePoint,new Vector3(-1,0,.2f),greenhouse?.15f:.09f,.033f,Cloth);}
                }
            }
        }
        private static void Fence(EnvironmentMesh mesh,Vector3 offset,bool rotate,float length,float height)
        {
            Vector3 along=rotate?Vector3.forward:Vector3.right;int posts=Mathf.CeilToInt(length/.45f)+1;
            for(int i=0;i<posts;i++)
            {
                Vector3 p=offset+along*(-length/2+length*i/(posts-1));Beam(mesh,p,p+Vector3.up*height,.033f,Bamboo);
                for(int node=1;node<4;node++)Beam(mesh,p+Vector3.up*(height*node/4-.008f),p+Vector3.up*(height*node/4+.008f),.037f,Wood);
            }
            for(int rail=1;rail<=2;rail++)Beam(mesh,offset-along*length/2+Vector3.up*(height*rail/3),offset+along*length/2+Vector3.up*(height*rail/3),.029f,Bamboo);
        }
        private static void Gate(EnvironmentMesh mesh)
        {
            for(int side=-1;side<=1;side+=2)Beam(mesh,new Vector3(side*.68f,0,0),new Vector3(side*.68f,1.24f,0),.068f,Bamboo);
            Fence(mesh,new Vector3(0,.12f,0),false,1.12f,.75f);
            Beam(mesh,new Vector3(-.53f,.23f,.05f),new Vector3(.53f,.81f,.05f),.026f,WoodDark);
            Roof(mesh,1.68f,.64f,1.18f,.23f,true);
        }
        private static void Trough(EnvironmentMesh mesh,Vector3 p,bool water)
        {
            Box(mesh,p+new Vector3(0,.06f,0),new Vector3(1.0f,.12f,.46f),water?StoneDark:WoodDark);
            for(int side=-1;side<=1;side+=2)
            {Box(mesh,p+new Vector3(side*.47f,.22f,0),new Vector3(.065f,.29f,.46f),water?Stone:Wood);Box(mesh,p+new Vector3(0,.22f,side*.20f),new Vector3(1.0f,.29f,.065f),water?Stone:Wood);}
            Box(mesh,p+new Vector3(0,.22f,0),new Vector3(.85f,.018f,.30f),water?Water:Straw);
            if(!water)for(int i=0;i<9;i++)Beam(mesh,p+new Vector3(-.34f+i*.085f,.239f,-.10f),p+new Vector3(-.28f+i*.07f,.245f,.12f),.008f,Gold);
        }
        private static void Bale(EnvironmentMesh mesh,Vector3 p,float scale)
        {
            Box(mesh,p+new Vector3(0,.23f*scale,0),new Vector3(.78f,.46f,.55f)*scale,Straw);
            for(int band=-1;band<=1;band+=2)
            {
                Box(mesh,p+new Vector3(band*.20f*scale,.235f*scale,.282f*scale),new Vector3(.028f,.47f,.013f)*scale,WoodDark);
                Box(mesh,p+new Vector3(band*.20f*scale,.465f*scale,0),new Vector3(.028f,.014f,.55f)*scale,WoodDark);
            }
            for(int line=0;line<5;line++)Beam(mesh,p+new Vector3(-.36f*scale,(.04f+line*.075f)*scale,.285f*scale),p+new Vector3(.36f*scale,(.07f+line*.072f)*scale,.285f*scale),.004f*scale,Gold);
        }
        private static void Crate(EnvironmentMesh mesh,Vector3 p)
        {
            Box(mesh,p+new Vector3(0,.035f,0),new Vector3(.57f,.07f,.45f),WoodDark);
            for(int level=0;level<3;level++)for(int side=-1;side<=1;side+=2)
            {Box(mesh,p+new Vector3(0,.12f+level*.12f,side*.23f),new Vector3(.60f,.08f,.035f),Wood);Box(mesh,p+new Vector3(side*.28f,.12f+level*.12f,0),new Vector3(.035f,.08f,.45f),Wood);}
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(mesh,p+new Vector3(x*.28f,.02f,z*.21f),p+new Vector3(x*.28f,.41f,z*.21f),.024f,WoodDark);
            mesh.Disc(p+new Vector3(0,.30f,0),.18f,.16f,Gold,12);
        }
        private static void Scarecrow(EnvironmentMesh mesh)
        {
            Beam(mesh,new Vector3(0,0,0),new Vector3(0,1.36f,0),.045f,WoodDark);
            Beam(mesh,new Vector3(-.65f,.86f,0),new Vector3(.65f,.86f,0),.043f,Wood);
            Box(mesh,new Vector3(0,.82f,0),new Vector3(.40f,.41f,.18f),Cloth);
            Box(mesh,new Vector3(-.35f,.87f,0),new Vector3(.33f,.17f,.18f),Cloth);Box(mesh,new Vector3(.35f,.87f,0),new Vector3(.33f,.17f,.18f),Cloth);
            for(int side=-1;side<=1;side+=2)for(int straw=0;straw<3;straw++)Beam(mesh,new Vector3(side*.48f,.81f+straw*.05f,0),new Vector3(side*.63f,.79f+straw*.05f,.015f),.010f,Straw);
            var head=new Vector3[16];for(int i=0;i<head.Length;i++){float a=i*Mathf.PI*2/head.Length;head[i]=new Vector3(Mathf.Cos(a)*.135f,1.15f,Mathf.Sin(a)*.125f);}
            mesh.Tube(new[]{new Vector3(0,1.025f,0),new Vector3(0,1.255f,0)},.13f,.12f,Plaster,16);
            mesh.Disc(new Vector3(0,1.27f,0),.39f,.39f,Straw,24);
            for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24,b=(i+1)*Mathf.PI*2/24;mesh.Tri(new Vector3(Mathf.Cos(a)*.39f,1.27f,Mathf.Sin(a)*.39f),new Vector3(0,1.49f,0),new Vector3(Mathf.Cos(b)*.39f,1.27f,Mathf.Sin(b)*.39f),Straw);}
            for(int side=-1;side<=1;side+=2)Box(mesh,new Vector3(side*.042f,1.16f,.125f),new Vector3(.025f,.027f,.015f),Black);
            Beam(mesh,new Vector3(-.044f,1.09f,.127f),new Vector3(.044f,1.09f,.127f),.007f,Dark);
        }
        private static void Lantern(EnvironmentMesh mesh)
        {
            Beam(mesh,Vector3.zero,new Vector3(0,1.6f,0),.044f,WoodDark);
            Beam(mesh,new Vector3(0,1.50f,0),new Vector3(.43f,1.50f,0),.030f,WoodDark);
            Beam(mesh,new Vector3(.4f,1.51f,0),new Vector3(.4f,1.36f,0),.010f,Dark);
            mesh.Tube(new[]{new Vector3(.4f,1.04f,0),new Vector3(.4f,1.35f,0)},.112f,.112f,Red,16);
            mesh.Disc(new Vector3(.4f,1.355f,0),.12f,.12f,Gold,16);mesh.Disc(new Vector3(.4f,1.04f,0),.12f,.12f,Gold,16);
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Beam(mesh,new Vector3(.4f+Mathf.Cos(a)*.116f,1.04f,Mathf.Sin(a)*.116f),new Vector3(.4f+Mathf.Cos(a)*.116f,1.35f,Mathf.Sin(a)*.116f),.006f,Gold);}
            Beam(mesh,new Vector3(.4f,1.04f,0),new Vector3(.4f,.94f,0),.014f,Gold);
        }
        private static void Well(EnvironmentMesh mesh)
        {
            mesh.Tube(new[]{new Vector3(0,0,0),new Vector3(0,.13f,0)},.55f,.55f,StoneDark,24);
            for(int row=0;row<3;row++)for(int i=0;i<12;i++)
            {float a=i*Mathf.PI*2/12+row*.12f,b=(i+1)*Mathf.PI*2/12+row*.12f;
                Vector3 p=new Vector3(Mathf.Cos(a)*.55f,.13f+row*.16f,Mathf.Sin(a)*.55f),q=new Vector3(Mathf.Cos(b)*.55f,.13f+row*.16f,Mathf.Sin(b)*.55f);
                Face(mesh,p,q,q+Vector3.up*.160f,p+Vector3.up*.160f,i%3==0?StoneDark:Stone);
                Vector3 insideP=new Vector3(p.x*.78f,p.y,p.z*.78f),insideQ=new Vector3(q.x*.78f,q.y,q.z*.78f);
                Face(mesh,insideQ,insideP,insideP+Vector3.up*.160f,insideQ+Vector3.up*.160f,StoneDark);}
            mesh.Disc(new Vector3(0,.18f,0),.50f,.50f,Water,24);
            for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24,b=(i+1)*Mathf.PI*2/24;Face(mesh,new Vector3(Mathf.Cos(a)*.43f,.59f,Mathf.Sin(a)*.43f),new Vector3(Mathf.Cos(b)*.43f,.59f,Mathf.Sin(b)*.43f),new Vector3(Mathf.Cos(b)*.59f,.59f,Mathf.Sin(b)*.59f),new Vector3(Mathf.Cos(a)*.59f,.59f,Mathf.Sin(a)*.59f),Stone);}
            for(int side=-1;side<=1;side+=2)Beam(mesh,new Vector3(side*.62f,0,0),new Vector3(side*.62f,1.85f,0),.064f);
            Beam(mesh,new Vector3(-.72f,1.81f,0),new Vector3(.72f,1.81f,0),.055f,WoodDark);
            Beam(mesh,new Vector3(-.60f,1.18f,0),new Vector3(.60f,1.18f,0),.042f,Wood);
            Beam(mesh,new Vector3(0,1.18f,0),new Vector3(0,.41f,0),.012f,Straw);
            Roof(mesh,1.6f,1.24f,1.53f,.34f,true);
        }
    }
}
