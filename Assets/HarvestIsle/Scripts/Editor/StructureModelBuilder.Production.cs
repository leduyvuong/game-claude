using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        internal static readonly string[] ProductionGroups={"Chế biến","Chế biến","Chế biến","Chế biến","Chế biến","Canh tác","Canh tác","Canh tác","Canh tác","Giao thương","Giao thương","Giao thương","Giao thương","Giao thương","Công trình chung","Công trình chung"};
        internal static GameObject CreateProduction(string type,Material[] materials,List<Mesh> meshes)
        {
            var root=new GameObject(type);var m=new EnvironmentMesh(Colors.Length);
            switch(type)
            {
                case "Windmill": Mill(root,m,materials,meshes);break;
                case "BakeryOven": Oven(m);break;
                case "DairyChurn": Dairy(root,m,materials,meshes);break;
                case "JuicePress": Press(root,m,materials,meshes);break;
                case "SoyVat": Vat(m);break;
                case "HerbalBench": Workbench(m,false);break;
                case "CompostBin": Compost(m);break;
                case "GraftingBench": Workbench(m,true);break;
                case "BeeHive": Hive(m);break;
                case "MarketStall": Market(m);break;
                case "HandCart": Cart(m);break;
                case "WoodBridge": Bridge(m);break;
                case "BoatDock": Dock(m);break;
                case "CargoBoat": Boat(m);break;
                case "WeatherStation": Weather(root,m,materials,meshes);break;
                case "Lighthouse": Beacon(root,m,materials,meshes);break;
                default: throw new System.ArgumentException("Unknown production model: "+type);
            }
            Add(root,"Structure",m.Finish(type+" body"),Slice(materials),meshes,true);
            Architecture(root,type,materials,meshes);
            return root;
        }
        private static void Cylinder(EnvironmentMesh m,Vector3 p,float height,float bottom,float top,int color,int sides=20)
        {m.Tube(new[]{p,p+Vector3.up*height},bottom,top,color,sides);}
        private static void Hoop(EnvironmentMesh m,Vector3 p,Vector3 axis,float radius,float thickness,int color,int sides=24)
        {
            axis.Normalize();Vector3 u=Vector3.Cross(axis,Mathf.Abs(axis.y)<.9f?Vector3.up:Vector3.right).normalized,v=Vector3.Cross(axis,u);
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                for(int k=0;k<6;k++)
                {
                    float c=k*Mathf.PI/3,d=(k+1)*Mathf.PI/3;
                    Vector3 ra=u*Mathf.Cos(a)+v*Mathf.Sin(a),rb=u*Mathf.Cos(b)+v*Mathf.Sin(b);
                    m.Quad(p+ra*(radius+thickness*Mathf.Cos(c))+axis*thickness*Mathf.Sin(c),
                        p+rb*(radius+thickness*Mathf.Cos(c))+axis*thickness*Mathf.Sin(c),
                        p+rb*(radius+thickness*Mathf.Cos(d))+axis*thickness*Mathf.Sin(d),
                        p+ra*(radius+thickness*Mathf.Cos(d))+axis*thickness*Mathf.Sin(d),color);
                }
            }
        }
        private static void Ellipsoid(EnvironmentMesh m,Vector3 p,Vector3 size,int color,bool dome=false)
        {
            for(int row=dome?4:0;row<8;row++)for(int col=0;col<16;col++)
            {
                float a=-Mathf.PI/2+row*Mathf.PI/8,b=a+Mathf.PI/8,c=col*Mathf.PI/8,d=c+Mathf.PI/8;
                Vector3 q=new Vector3(Mathf.Cos(a)*Mathf.Cos(c),Mathf.Sin(a),Mathf.Cos(a)*Mathf.Sin(c));
                Vector3 r=new Vector3(Mathf.Cos(a)*Mathf.Cos(d),Mathf.Sin(a),Mathf.Cos(a)*Mathf.Sin(d));
                Vector3 s=new Vector3(Mathf.Cos(b)*Mathf.Cos(d),Mathf.Sin(b),Mathf.Cos(b)*Mathf.Sin(d));
                Vector3 t=new Vector3(Mathf.Cos(b)*Mathf.Cos(c),Mathf.Sin(b),Mathf.Cos(b)*Mathf.Sin(c));
                m.Quad(p+Vector3.Scale(q,size),p+Vector3.Scale(t,size),p+Vector3.Scale(s,size),p+Vector3.Scale(r,size),color);
            }
        }
        private static void Table(EnvironmentMesh m,Vector3 p,float width,float depth,float height)
        {
            Box(m,p+Vector3.up*height,new Vector3(width,.12f,depth),Wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                Beam(m,p+new Vector3(x*(width*.5f-.10f),0,z*(depth*.5f-.09f)),p+new Vector3(x*(width*.5f-.10f),height,z*(depth*.5f-.09f)),.045f);
            Box(m,p+Vector3.up*(height*.32f),new Vector3(width-.14f,.07f,depth-.12f),WoodDark);
        }
        private static void Sack(EnvironmentMesh m,Vector3 p,float size=1)
        {
            Ellipsoid(m,p+Vector3.up*.30f*size,new Vector3(.25f,.30f,.21f)*size,Straw);
            Cylinder(m,p+Vector3.up*.53f*size,.12f*size,.13f*size,.08f*size,Straw,12);
            Hoop(m,p+Vector3.up*.57f*size,Vector3.up,.10f*size,.014f*size,WoodDark,16);
        }
        private static void Jar(EnvironmentMesh m,Vector3 p,float scale,int color=TileDark)
        {
            Cylinder(m,p,.10f*scale,.17f*scale,.24f*scale,color);
            Cylinder(m,p+Vector3.up*.10f*scale,.32f*scale,.24f*scale,.17f*scale,color);
            Cylinder(m,p+Vector3.up*.42f*scale,.06f*scale,.18f*scale,.18f*scale,color);
            m.Disc(p+Vector3.up*.485f*scale,.17f*scale,.17f*scale,Wood,20);
        }
        private static void RotatingPart(GameObject root,string name,EnvironmentMesh m,Vector3 p,Vector3 axis,float speed,Material[] mats,List<Mesh> meshes)
        {
            Add(root,name,m.Finish(name),Slice(mats),meshes,false);Transform rotor=root.transform.GetChild(root.transform.childCount-1);rotor.localPosition=p;
            root.AddComponent<FarmMachineMotion>().Configure(rotor,axis,speed);
        }
        private static void Mill(GameObject root,EnvironmentMesh m,Material[] mats,List<Mesh> meshes)
        {
            Cylinder(m,Vector3.zero,.13f,1.01f,1.01f,Stone,24);
            Cylinder(m,new Vector3(0,.13f,0),2.35f,.87f,.62f,Plaster,16);
            Cylinder(m,new Vector3(0,2.47f,0),.66f,.89f,.02f,Tile,16);
            Box(m,new Vector3(0,.70f,.86f),new Vector3(.48f,1.17f,.16f),WoodDark);
            Box(m,new Vector3(-.44f,1.55f,.74f),new Vector3(.33f,.72f,.21f),WoodDark);Window(m,-.44f,1.55f,.855f,.25f);
            Sack(m,new Vector3(-.90f,.13f,.49f));Sack(m,new Vector3(-1.22f,.13f,.26f),.8f);
            Beam(m,new Vector3(0,2.22f,.59f),new Vector3(0,2.22f,1.00f),.13f,WoodDark);
            var rotor=new EnvironmentMesh(Colors.Length);Beam(rotor,new Vector3(0,0,-.07f),new Vector3(0,0,.07f),.13f,WoodDark);
            for(int blade=0;blade<4;blade++)
            {
                Quaternion turn=Quaternion.AngleAxis(blade*90,Vector3.forward);
                Beam(rotor,turn*new Vector3(0,.12f,0),turn*new Vector3(0,1.43f,0),.038f,WoodDark);
                for(int line=0;line<6;line++)
                {
                    float y=.45f+line*.16f;
                    Face(rotor,turn*new Vector3(-.20f,y,-.015f),turn*new Vector3(.10f,y,-.015f),turn*new Vector3(.10f,y+.115f,-.015f),turn*new Vector3(-.20f,y+.115f,-.015f),Bamboo);
                }
                Beam(rotor,turn*new Vector3(-.20f,.45f,0),turn*new Vector3(-.20f,1.39f,0),.018f);
                Beam(rotor,turn*new Vector3(.10f,.45f,0),turn*new Vector3(.10f,1.39f,0),.018f);
            }
            RotatingPart(root,"Mill sails",rotor,new Vector3(0,2.22f,1.01f),Vector3.forward,22,mats,meshes);
        }
        private static void Oven(EnvironmentMesh m)
        {
            Box(m,new Vector3(-.27f,.12f,0),new Vector3(1.72f,.24f,1.5f),Stone);
            Box(m,new Vector3(-.27f,.54f,0),new Vector3(1.58f,.70f,1.35f),TileDark);
            Ellipsoid(m,new Vector3(-.27f,.89f,0),new Vector3(.82f,.64f,.69f),Plaster,true);
            Box(m,new Vector3(-.27f,.68f,.688f),new Vector3(.69f,.47f,.036f),Black);
            for(int i=0;i<13;i++)
            {
                float a=i*Mathf.PI/12;Vector3 p=new Vector3(-.27f+Mathf.Cos(a)*.39f,.61f+Mathf.Sin(a)*.36f,.72f);
                Beam(m,p,p+new Vector3(0,0,.09f),.062f,i%3==0?TileLight:Tile);
            }
            Box(m,new Vector3(-.27f,.435f,.76f),new Vector3(.88f,.10f,.30f),StoneDark);
            Box(m,new Vector3(-.63f,1.63f,-.24f),new Vector3(.30f,1.34f,.32f),Tile);Box(m,new Vector3(-.63f,2.32f,-.24f),new Vector3(.39f,.09f,.41f),StoneDark);
            for(int i=0;i<6;i++)Box(m,new Vector3(-.63f,1.09f+i*.20f,-.072f),new Vector3(.32f,.023f,.02f),TileDark);
            Table(m,new Vector3(1.13f,0,.21f),.94f,1.0f,.79f);
            for(int i=0;i<3;i++)
            {
                Vector3 p=new Vector3(1.13f,.93f,-.13f+i*.29f);Ellipsoid(m,p,new Vector3(.30f,.10f,.105f),Gold);
                for(int cut=0;cut<3;cut++)Beam(m,p+new Vector3(-.12f+cut*.12f,.073f,-.05f),p+new Vector3(-.09f+cut*.12f,.073f,.05f),.009f,Straw);
            }
            Sack(m,new Vector3(1.62f,0,-.25f),.82f);Beam(m,new Vector3(.58f,.14f,.78f),new Vector3(.82f,1.51f,.85f),.023f,Wood);
        }
        private static void Dairy(GameObject root,EnvironmentMesh m,Material[] mats,List<Mesh> meshes)
        {
            Box(m,new Vector3(0,.06f,0),new Vector3(1.55f,.12f,1.10f),Stone);
            Cylinder(m,new Vector3(-.18f,.12f,0),.94f,.40f,.34f,Wood,24);
            for(int i=0;i<18;i++){float a=i*Mathf.PI/9;Beam(m,new Vector3(-.18f+Mathf.Cos(a)*.365f,.20f,Mathf.Sin(a)*.365f),new Vector3(-.18f+Mathf.Cos(a)*.342f,1.06f,Mathf.Sin(a)*.342f),.010f,WoodDark);}
            for(int i=0;i<3;i++)Hoop(m,new Vector3(-.18f,.28f+i*.30f,0),Vector3.up,.39f-i*.015f,.022f,StoneDark);
            m.Disc(new Vector3(-.18f,1.075f,0),.35f,.35f,WoodDark,24);
            var rotor=new EnvironmentMesh(Colors.Length);Beam(rotor,Vector3.zero,Vector3.up*.50f,.027f,WoodDark);Beam(rotor,new Vector3(-.25f,.48f,0),new Vector3(.25f,.48f,0),.035f,Wood);
            RotatingPart(root,"Churn handle",rotor,new Vector3(-.18f,1.07f,0),Vector3.up,48,mats,meshes);
            Cylinder(m,new Vector3(.58f,.12f,.25f),.44f,.16f,.14f,Plaster,16);Cylinder(m,new Vector3(.58f,.56f,.25f),.12f,.10f,.07f,StoneDark,16);
            Hoop(m,new Vector3(.58f,.59f,.25f),Vector3.forward,.13f,.014f,StoneDark,16);
            Box(m,new Vector3(.50f,.17f,-.35f),new Vector3(.37f,.10f,.26f),Gold);
        }
        private static void Press(GameObject root,EnvironmentMesh m,Material[] mats,List<Mesh> meshes)
        {
            Box(m,new Vector3(0,.065f,0),new Vector3(1.35f,.13f,1.04f),Stone);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.54f,.13f,0),new Vector3(side*.54f,1.69f,0),.065f);
            Box(m,new Vector3(0,1.66f,0),new Vector3(1.28f,.13f,.18f),WoodDark);
            Cylinder(m,new Vector3(0,.15f,0),.53f,.38f,.38f,Wood,20);
            for(int i=0;i<20;i++){float a=i*Mathf.PI/10;Beam(m,new Vector3(Mathf.Cos(a)*.387f,.19f,Mathf.Sin(a)*.387f),new Vector3(Mathf.Cos(a)*.387f,.68f,Mathf.Sin(a)*.387f),.008f,Dark);}
            Hoop(m,new Vector3(0,.24f,0),Vector3.up,.40f,.018f,StoneDark);Hoop(m,new Vector3(0,.61f,0),Vector3.up,.40f,.018f,StoneDark);
            Cylinder(m,new Vector3(0,.74f,0),.08f,.36f,.36f,WoodDark);Beam(m,new Vector3(0,.78f,0),new Vector3(0,1.83f,0),.034f,StoneDark);
            for(int i=0;i<15;i++)Hoop(m,new Vector3(0,.88f+i*.056f,0),Vector3.up,.039f,.008f,Stone,12);
            var rotor=new EnvironmentMesh(Colors.Length);Hoop(rotor,Vector3.zero,Vector3.up,.28f,.025f,WoodDark);Beam(rotor,new Vector3(-.28f,0,0),new Vector3(.28f,0,0),.019f);
            RotatingPart(root,"Press wheel",rotor,new Vector3(0,1.84f,0),Vector3.up,15,mats,meshes);
            Beam(m,new Vector3(0,.20f,.36f),new Vector3(0,.19f,.57f),.040f,TileDark);Jar(m,new Vector3(0,0,.73f),.64f,Tile);
            Crate(m,new Vector3(.63f,.13f,.45f));for(int i=0;i<5;i++)Ellipsoid(m,new Vector3(.51f+i%3*.13f,.47f,.36f+i/3*.16f),Vector3.one*.082f,i%2==0?Red:Gold);
        }
        private static void Vat(EnvironmentMesh m)
        {
            Box(m,new Vector3(0,.04f,0),new Vector3(1.6f,.08f,1.2f),Stone);
            for(int i=0;i<2;i++)
            {
                Vector3 p=new Vector3(-.39f+i*.79f,.08f,-.05f);Jar(m,p,1.65f,i==0?Tile:TileDark);
                m.Disc(p+Vector3.up*.815f,.33f,.33f,Straw,24);Hoop(m,p+Vector3.up*.825f,Vector3.up,.29f,.017f,Bamboo);
                Box(m,p+new Vector3(0,.46f,.39f),new Vector3(.17f,.18f,.024f),Plaster);
            }
            Sack(m,new Vector3(-.72f,.08f,.45f),.62f);Beam(m,new Vector3(.34f,.09f,.51f),new Vector3(.46f,1.05f,.47f),.021f,Wood);
        }
        private static void Workbench(EnvironmentMesh m,bool grafting)
        {
            Table(m,Vector3.zero,1.95f,.95f,.84f);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.88f,.84f,-.42f),new Vector3(side*.88f,1.54f,-.42f),.035f);
            Box(m,new Vector3(0,1.35f,-.44f),new Vector3(1.86f,.47f,.06f),WoodDark);
            if(grafting)
            {
                for(int i=0;i<3;i++)
                {
                    Vector3 p=new Vector3(-.64f+i*.31f,.91f,-.14f);Cylinder(m,p,.18f,.11f,.15f,Tile,12);m.Disc(p+Vector3.up*.181f,.12f,.12f,Dark,12);
                    Beam(m,p+Vector3.up*.18f,p+Vector3.up*.49f,.013f,Wood);m.Leaf(p+Vector3.up*.36f,Vector3.right,.16f,.04f,Cloth);m.Leaf(p+Vector3.up*.40f,Vector3.left,.14f,.038f,Cloth);
                    Beam(m,p+new Vector3(0,.32f,0),p+new Vector3(.07f,.40f,0),.010f,Wood);Hoop(m,p+Vector3.up*.34f,Vector3.up,.024f,.009f,Plaster,12);
                }
                Box(m,new Vector3(.48f,.924f,.11f),new Vector3(.62f,.025f,.44f),Straw);
                Beam(m,new Vector3(.25f,.95f,.11f),new Vector3(.60f,.95f,.20f),.013f,Wood);
                Beam(m,new Vector3(.67f,.96f,-.1f),new Vector3(.84f,.96f,.07f),.016f,StoneDark);
                for(int i=0;i<3;i++)m.Leaf(new Vector3(-.54f+i*.4f,1.47f,-.39f),Vector3.forward,.16f,.04f,Cloth);
            }
            else
            {
                Cylinder(m,new Vector3(.45f,.90f,.1f),.17f,.19f,.24f,StoneDark,16);m.Disc(new Vector3(.45f,1.073f,.1f),.19f,.19f,Cloth,16);
                Beam(m,new Vector3(.45f,1.05f,.1f),new Vector3(.64f,1.33f,.1f),.046f,Stone);
                for(int i=0;i<3;i++){Jar(m,new Vector3(-.68f+i*.26f,.90f,-.17f),.38f,i%2==0?Tile:Water);}
                for(int i=0;i<5;i++)m.Leaf(new Vector3(-.70f+i*.15f,.914f,.22f),new Vector3(.2f,0,-1),.22f,.048f,Cloth);
                for(int i=0;i<3;i++){Vector3 p=new Vector3(-.58f+i*.47f,1.15f,-.36f);Beam(m,p,p+Vector3.up*.33f,.007f,Straw);for(int leaf=0;leaf<3;leaf++)m.Leaf(p+Vector3.up*(leaf*.07f),Vector3.forward,.12f,.032f,Cloth);}
            }
            Crate(m,new Vector3(.35f,.30f,-.06f));
        }
        private static void Compost(EnvironmentMesh m)
        {
            Box(m,new Vector3(0,.055f,0),new Vector3(1.55f,.11f,1.28f),WoodDark);
            for(int side=-1;side<=1;side+=2)for(int row=0;row<5;row++)
            {
                Box(m,new Vector3(side*.72f,.19f+row*.15f,0),new Vector3(.065f,.115f,1.18f),Wood);
                Box(m,new Vector3(0,.19f+row*.15f,side*.57f),new Vector3(1.5f,.115f,.065f),Wood);
            }
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,new Vector3(x*.70f,0,z*.55f),new Vector3(x*.70f,.93f,z*.55f),.043f);
            Ellipsoid(m,new Vector3(0,.46f,0),new Vector3(.62f,.38f,.47f),Dark);
            for(int i=0;i<14;i++){float a=i*2.39f;Vector3 p=new Vector3(Mathf.Cos(a)*.42f,.75f-(i%3)*.07f,Mathf.Sin(a)*.32f);m.Leaf(p,new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),.15f,.047f,i%3==0?Gold:Cloth);}
            Beam(m,new Vector3(.95f,.04f,.13f),new Vector3(.82f,1.25f,.04f),.024f,Wood);
            Box(m,new Vector3(.96f,.10f,.13f),new Vector3(.23f,.20f,.035f),StoneDark);
        }
        private static void Hive(EnvironmentMesh m)
        {
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,new Vector3(x*.28f,0,z*.28f),new Vector3(x*.28f,.35f,z*.28f),.035f);
            Box(m,new Vector3(0,.32f,0),new Vector3(.86f,.08f,.81f),WoodDark);
            for(int i=0;i<3;i++){Box(m,new Vector3(0,.48f+i*.24f,0),new Vector3(.72f,.225f,.68f),i%2==0?Gold:Plaster);Box(m,new Vector3(0,.48f+i*.24f,.35f),new Vector3(.21f,.035f,.045f),Wood);}
            Roof(m,.92f,.90f,1.07f,.22f,true);Box(m,new Vector3(0,.386f,.348f),new Vector3(.39f,.038f,.018f),Dark);Box(m,new Vector3(0,.35f,.44f),new Vector3(.50f,.032f,.25f),Wood);
            Jar(m,new Vector3(.66f,0,.19f),.54f,Gold);Box(m,new Vector3(.66f,.15f,.32f),new Vector3(.12f,.12f,.015f),Plaster);
        }
        private static void Market(EnvironmentMesh m)
        {
            Box(m,new Vector3(0,.055f,0),new Vector3(2.25f,.11f,1.53f),Stone);
            Table(m,new Vector3(0,.11f,.25f),1.97f,.65f,.69f);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*1.03f,.11f,-.46f),new Vector3(side*1.03f,1.765f,-.46f),.052f);
            Beam(m,new Vector3(-1.08f,1.755f,-.46f),new Vector3(1.08f,1.755f,-.46f),.035f,WoodDark);
            for(int i=0;i<10;i++)
            {
                float x=-1.17f+i*.234f;
                for(int panel=0;panel<6;panel++)
                {
                    float a=panel/6f,b=(panel+1)/6f;
                    Vector3 p=new Vector3(x,1.87f-.30f*a-Mathf.Sin(a*Mathf.PI)*.065f,-.68f+1.57f*a),q=new Vector3(x,1.87f-.30f*b-Mathf.Sin(b*Mathf.PI)*.065f,-.68f+1.57f*b);
                    Face(m,p,p+Vector3.right*.234f,q+Vector3.right*.234f,q,i%2==0?Cloth:Plaster);
                    Beam(m,p+Vector3.up*.006f,q+Vector3.up*.006f,.006f,Bamboo);
                }
                for(int scallop=0;scallop<4;scallop++)
                {
                    float a=scallop/4f,b=(scallop+1)/4f;
                    Vector3 p=new Vector3(x+.234f*a,1.57f,.89f),q=new Vector3(x+.234f*b,1.57f,.89f);
                    Vector3 lowP=p-Vector3.up*(.11f+Mathf.Sin(a*Mathf.PI)*.055f),lowQ=q-Vector3.up*(.11f+Mathf.Sin(b*Mathf.PI)*.055f);
                    Face(m,p,q,lowQ,lowP,i%2==0?Cloth:Plaster);Beam(m,lowP,lowQ,.008f,Bamboo);
                }
            }
            Beam(m,new Vector3(-1.18f,1.87f,-.68f),new Vector3(1.18f,1.87f,-.68f),.032f);
            for(int i=0;i<3;i++){Vector3 p=new Vector3(-.66f+i*.64f,.86f,.25f);Box(m,p,new Vector3(.54f,.04f,.53f),WoodDark);for(int j=0;j<6;j++)Ellipsoid(m,p+new Vector3((j%3-1)*.13f,.10f,(j/3-.5f)*.20f),new Vector3(.075f,.078f,.075f),i==0?Red:i==1?Gold:Cloth);}
            Crate(m,new Vector3(-.73f,.11f,-.36f));Sack(m,new Vector3(.75f,.11f,-.35f),.72f);
            Box(m,new Vector3(0,1.72f,.88f),new Vector3(.50f,.23f,.045f),WoodDark);m.Leaf(new Vector3(-.12f,1.74f,.91f),Vector3.right,.22f,.045f,Gold);
        }
        private static void Cart(EnvironmentMesh m)
        {
            Box(m,new Vector3(0,.56f,0),new Vector3(1.09f,.10f,1.36f),WoodDark);
            for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)
            {Box(m,new Vector3(side*.51f,.70f+row*.13f,0),new Vector3(.045f,.085f,1.35f),Wood);Box(m,new Vector3(0,.70f+row*.13f,side*.65f),new Vector3(1.10f,.085f,.045f),Wood);}
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 wheel=new Vector3(side*.68f,.38f,-.10f);Hoop(m,wheel,Vector3.right,.35f,.043f,WoodDark);Hoop(m,wheel,Vector3.right,.37f,.012f,StoneDark);
                for(int spoke=0;spoke<8;spoke++){float a=spoke*Mathf.PI/4;Beam(m,wheel,wheel+new Vector3(0,Mathf.Cos(a)*.34f,Mathf.Sin(a)*.34f),.020f,Wood);}
                Beam(m,new Vector3(side*.40f,.52f,.54f),new Vector3(side*.40f,.56f,1.80f),.038f,WoodDark);
                Beam(m,new Vector3(side*.39f,.55f,.94f),new Vector3(side*.39f,0,.94f),.027f);
            }
            Beam(m,new Vector3(-.76f,.38f,-.10f),new Vector3(.76f,.38f,-.10f),.032f,StoneDark);
            Sack(m,new Vector3(-.16f,.61f,-.2f),.85f);Crate(m,new Vector3(.12f,.61f,.34f));
        }
        private static void Bridge(EnvironmentMesh m)
        {
            const int boards=19;for(int i=0;i<boards;i++){float z=-1.6f+i*3.2f/(boards-1),y=.20f+.30f*(1-z*z/(1.6f*1.6f));Box(m,new Vector3(0,y,z),new Vector3(1.30f,.095f,.152f),i%4==0?WoodDark:Wood);}
            for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++)
            {
                float z=-1.6f+i*3.2f/6,y=.20f+.30f*(1-z*z/2.56f);
                Beam(m,new Vector3(side*.60f,y,z),new Vector3(side*.60f,y+.66f,z),.036f);
                if(i<6){float z2=-1.6f+(i+1)*3.2f/6,y2=.20f+.30f*(1-z2*z2/2.56f);for(int rail=0;rail<2;rail++)Beam(m,new Vector3(side*.60f,y+.32f+rail*.28f,z),new Vector3(side*.60f,y2+.32f+rail*.28f,z2),.027f,Wood);}
                if(i%2==0)Beam(m,new Vector3(side*.49f,0,z),new Vector3(side*.49f,y,z),.055f);
            }
        }
        private static void Dock(EnvironmentMesh m)
        {
            for(int i=0;i<17;i++)Box(m,new Vector3(0,.36f,-1.45f+i*.18f),new Vector3(1.50f,.105f,.166f),i%3==0?WoodDark:Wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){Beam(m,new Vector3(x*.63f,0,z*1.35f),new Vector3(x*.63f,.68f,z*1.35f),.073f);Hoop(m,new Vector3(x*.63f,.59f,z*1.35f),Vector3.up,.074f,.012f,Straw);}
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.61f,.19f,-1.43f),new Vector3(side*.61f,.19f,1.43f),.049f);
            Crate(m,new Vector3(-.40f,.41f,-.97f));Beam(m,new Vector3(.61f,.4f,-.65f),new Vector3(.61f,1.91f,-.65f),.040f);
            Beam(m,new Vector3(.61f,1.85f,-.65f),new Vector3(.30f,1.85f,-.65f),.028f);HangingLantern(m,new Vector3(.30f,1.80f,-.65f));
            for(int i=0;i<3;i++)Hoop(m,new Vector3(.31f,.44f,.81f),Vector3.up,.14f+i*.036f,.009f,Straw);
        }
        private static void Boat(EnvironmentMesh m,bool basic=false)
        {
            const int segments=12;
            for(int side=-1;side<=1;side+=2)for(int i=0;i<segments;i++)
            {
                float z=-1.56f+i*3.12f/segments,z2=-1.56f+(i+1)*3.12f/segments;
                float width=.60f*Mathf.Sin((z+1.56f)/3.12f*Mathf.PI),width2=.60f*Mathf.Sin((z2+1.56f)/3.12f*Mathf.PI);
                float edge=.49f+.14f*Mathf.Abs(z)/1.56f,edge2=.49f+.14f*Mathf.Abs(z2)/1.56f;
                Face(m,new Vector3(side*width*.48f,.03f,z),new Vector3(side*width2*.48f,.03f,z2),new Vector3(side*width2,edge2,z2),new Vector3(side*width,edge,z),WoodDark);
                Face(m,new Vector3(0,.28f,z),new Vector3(side*width*.84f,.28f,z),new Vector3(side*width2*.84f,.28f,z2),new Vector3(0,.28f,z2),Wood);
                Beam(m,new Vector3(side*width,edge,z),new Vector3(side*width2,edge2,z2),.023f,Wood);
                for(int band=1;band<3;band++)Beam(m,new Vector3(side*width*(.48f+.52f*band/3),Mathf.Lerp(.03f,edge,band/3f),z),new Vector3(side*width2*(.48f+.52f*band/3),Mathf.Lerp(.03f,edge2,band/3f),z2),.007f,Wood);
            }
            for(int i=0;i<3;i++)Box(m,new Vector3(0,.45f,-.90f+i*.90f),new Vector3(.85f,.06f,.19f),Wood);
            if(basic)return;
            Crate(m,new Vector3(-.02f,.29f,-.72f));Sack(m,new Vector3(.15f,.29f,-.18f),.72f);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.39f,.44f,.30f),new Vector3(side*.39f,1.21f,.30f),.025f,Bamboo);
            Roof(m,1.02f,.96f,1.20f,.21f,false,new Vector3(0,0,.32f));
            Beam(m,new Vector3(.3f,.42f,1.20f),new Vector3(.75f,.27f,1.86f),.022f,Wood);
            Box(m,new Vector3(.74f,.25f,1.85f),new Vector3(.14f,.07f,.36f),WoodDark);
        }
        private static void Weather(GameObject root,EnvironmentMesh m,Material[] mats,List<Mesh> meshes)
        {
            Box(m,new Vector3(0,.06f,0),new Vector3(1.57f,.12f,1.21f),Stone);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.34f,.12f,-.15f),new Vector3(side*.34f,.93f,-.15f),.035f,StoneDark);
            Box(m,new Vector3(0,1.20f,-.15f),new Vector3(.92f,.65f,.57f),Plaster);
            for(int row=0;row<7;row++)Box(m,new Vector3(0,.94f+row*.079f,.148f),new Vector3(.89f,.044f,.047f),row%2==0?Wood:Plaster);
            Roof(m,1.11f,.79f,1.53f,.18f,false,new Vector3(0,0,-.15f));
            Beam(m,new Vector3(.62f,.12f,-.35f),new Vector3(.62f,2.18f,-.35f),.024f,StoneDark);
            var rotor=new EnvironmentMesh(Colors.Length);Beam(rotor,new Vector3(-.3f,0,0),new Vector3(.3f,0,0),.012f,StoneDark);
            Face(rotor,new Vector3(.17f,0,-.1f),new Vector3(.36f,0,0),new Vector3(.17f,0,.1f),new Vector3(.17f,0,.1f),Red);
            Box(rotor,new Vector3(-.24f,.05f,0),new Vector3(.17f,.17f,.015f),Cloth);
            RotatingPart(root,"Wind vane",rotor,new Vector3(.62f,2.18f,-.35f),Vector3.up,19,mats,meshes);
            Cylinder(m,new Vector3(-.58f,.12f,.27f),.49f,.13f,.13f,Water,16);m.Disc(new Vector3(-.58f,.612f,.27f),.10f,.10f,Dark,16);
            for(int i=0;i<5;i++)Box(m,new Vector3(-.58f,.2f+i*.075f,.407f),new Vector3(.10f,.012f,.012f),Plaster);
            Beam(m,new Vector3(-.58f,.12f,-.39f),new Vector3(-.58f,1.92f,-.39f),.027f,WoodDark);
            Face(m,new Vector3(-.58f,1.91f,-.39f),new Vector3(-.09f,1.84f,-.39f),new Vector3(-.13f,1.60f,-.39f),new Vector3(-.58f,1.76f,-.39f),Red);
        }
        private static void Beacon(GameObject root,EnvironmentMesh m,Material[] mats,List<Mesh> meshes)
        {
            Cylinder(m,Vector3.zero,.15f,.99f,.99f,StoneDark,24);Cylinder(m,Vector3.up*.15f,.18f,.87f,.83f,Stone,24);
            for(int band=0;band<6;band++){float y=.33f+band*.46f;Cylinder(m,Vector3.up*y,.46f,Mathf.Lerp(.73f,.45f,band/6f),Mathf.Lerp(.73f,.45f,(band+1)/6f),band%2==0?Plaster:Red,24);}
            Box(m,new Vector3(0,.86f,.72f),new Vector3(.37f,1.02f,.035f),WoodDark);
            for(int i=0;i<2;i++)Window(m,0,1.85f+i*.80f,Mathf.Lerp(.58f,.49f,i),.23f);
            Cylinder(m,Vector3.up*3.09f,.12f,.70f,.70f,StoneDark,24);
            for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Vector3 p=new Vector3(Mathf.Cos(a)*.65f,3.19f,Mathf.Sin(a)*.65f);Beam(m,p,p+Vector3.up*.40f,.014f,StoneDark);}
            Hoop(m,Vector3.up*3.55f,Vector3.up,.65f,.018f,StoneDark);
            Cylinder(m,Vector3.up*3.24f,.64f,.41f,.41f,Water,16);
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Beam(m,new Vector3(Mathf.Cos(a)*.42f,3.23f,Mathf.Sin(a)*.42f),new Vector3(Mathf.Cos(a)*.42f,3.91f,Mathf.Sin(a)*.42f),.024f,StoneDark);}
            Cylinder(m,Vector3.up*3.90f,.47f,.64f,.02f,Tile,16);Cylinder(m,Vector3.up*4.36f,.13f,.048f,.024f,Gold,12);
            var lamp=new EnvironmentMesh(Colors.Length);Ellipsoid(lamp,new Vector3(0,0,.44f),new Vector3(.15f,.20f,.10f),Gold);Beam(lamp,Vector3.zero,Vector3.forward*.40f,.036f,StoneDark);
            RotatingPart(root,"Beacon lamp",lamp,Vector3.up*3.58f,Vector3.up,28,mats,meshes);
            Box(m,new Vector3(0,.08f,.96f),new Vector3(.76f,.16f,.42f),Stone);Box(m,new Vector3(0,.18f,.79f),new Vector3(.76f,.18f,.27f),Stone);
        }
    }
}
