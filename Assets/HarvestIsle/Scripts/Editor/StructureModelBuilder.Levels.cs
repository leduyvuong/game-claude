using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        internal static GameObject CreateLevel(string type,int level,Material[] materials,List<Mesh> meshes)
        {
            GameObject root;
            if(level==1)
            {
                root=new GameObject(type+"_L1");var mesh=new EnvironmentMesh(Colors.Length);EnvironmentMesh glass=null;
                switch(type)
                {
                    case "FarmHouse": SimpleHut(mesh,1.9f,1.6f,1.7f,true);break;
                    case "RiceBarn": SimpleHut(mesh,1.7f,1.4f,1.55f,false);Bale(mesh,new Vector3(.53f,0,.92f),.45f);break;
                    case "ChickenCoop": SmallCoop(mesh,false);break;
                    case "DuckShelter": SmallCoop(mesh,true);break;
                    case "CowShed":
                        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(mesh,new Vector3(x*.92f,0,z*.63f),new Vector3(x*.92f,1.40f,z*.63f),.048f);
                        Roof(mesh,2.2f,1.65f,1.4f,.28f,false);Trough(mesh,new Vector3(0,0,.53f),false);break;
                    case "PigPen":
                        Fence(mesh,new Vector3(0,0,-.65f),false,1.8f,.55f);Fence(mesh,new Vector3(0,0,.65f),false,1.8f,.55f);
                        Fence(mesh,new Vector3(-.9f,0,0),true,1.3f,.55f);Fence(mesh,new Vector3(.9f,0,0),true,1.3f,.55f);break;
                    case "Greenhouse":
                        glass=new EnvironmentMesh(1);LowColdFrame(mesh,glass);break;
                    case "Nursery":
                        Box(mesh,new Vector3(0,.20f,0),new Vector3(1.35f,.1f,.62f),Wood);
                        for(int x=-1;x<=1;x+=2)Beam(mesh,new Vector3(x*.55f,0,0),new Vector3(x*.55f,.22f,0),.033f);
                        for(int i=0;i<4;i++){Vector3 p=new Vector3(-.46f+i*.30f,.27f,0);Box(mesh,p,new Vector3(.25f,.07f,.35f),Dark);mesh.Leaf(p+Vector3.up*.04f,Vector3.right,.11f,.035f,Cloth);}break;
                    case "VillageWell":
                        mesh.Tube(new[]{new Vector3(0,0,0),new Vector3(0,.13f,0)},.53f,.53f,StoneDark,24);
                        for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;
                            Face(mesh,new Vector3(Mathf.Cos(a)*.52f,.13f,Mathf.Sin(a)*.52f),new Vector3(Mathf.Cos(b)*.52f,.13f,Mathf.Sin(b)*.52f),new Vector3(Mathf.Cos(b)*.52f,.49f,Mathf.Sin(b)*.52f),new Vector3(Mathf.Cos(a)*.52f,.49f,Mathf.Sin(a)*.52f),Stone);}
                        mesh.Disc(new Vector3(0,.18f,0),.49f,.49f,Water,24);break;
                }
                Add(root,"Basic structure",mesh.Finish(type+" L1"),Slice(materials),meshes,true);
                if(glass!=null)Add(root,"Cold frame cover",glass.Finish(type+" L1 glass"),new[]{materials[Colors.Length]},meshes,false);
                return root;
            }
            root=Create(type,materials,meshes,level==3);root.name=type+"_L"+level;
            if(level==2)return root;
            var extra=new EnvironmentMesh(Colors.Length);EnvironmentMesh extraGlass=null;
            switch(type)
            {
                case "FarmHouse":
                    Box(extra,new Vector3(-1.92f,.11f,-.12f),new Vector3(1.18f,.22f,1.9f),Stone);
                    Box(extra,new Vector3(-1.94f,.95f,-.12f),new Vector3(1.12f,1.5f,1.64f),Plaster);
                    Roof(extra,1.35f,2.05f,1.72f,.38f,true,new Vector3(-1.94f,0,-.12f));Window(extra,-1.94f,1.05f,.71f,.50f);
                    for(int side=-1;side<=1;side+=2){Box(extra,new Vector3(side*1.05f,.38f,1.03f),new Vector3(.58f,.09f,.23f),Wood);Beam(extra,new Vector3(side*1.05f,.23f,1.03f),new Vector3(side*1.05f,.37f,1.03f),.032f);}
                    HangingLantern(extra,new Vector3(1.12f,1.58f,1.12f));break;
                case "RiceBarn":
                    for(int x=-1;x<=1;x+=2)Beam(extra,new Vector3(1.8f+x*.42f,0,0),new Vector3(1.8f+x*.42f,1.41f,0),.04f);
                    Roof(extra,1.26f,1.75f,1.4f,.21f,false,new Vector3(1.85f,0,0));
                    Bale(extra,new Vector3(1.85f,.05f,-.42f),.72f);Bale(extra,new Vector3(1.85f,.05f,.20f),.72f);Bale(extra,new Vector3(1.85f,.42f,-.15f),.60f);
                    Crate(extra,new Vector3(-.96f,.16f,1.19f));break;
                case "ChickenCoop":case "DuckShelter":
                    Fence(extra,new Vector3(0,0,2.02f),false,2.25f,.65f);Fence(extra,new Vector3(-1.12f,0,1.45f),true,1.15f,.65f);Fence(extra,new Vector3(1.12f,0,1.45f),true,1.15f,.65f);
                    Trough(extra,new Vector3(.50f,0,1.55f),type=="DuckShelter");
                    for(int side=-1;side<=1;side+=2)Box(extra,new Vector3(side*.70f,.22f,-.19f),new Vector3(.40f,.15f,.60f),Straw);
                    float annexHeight=type=="DuckShelter"?.72f:.86f;
                    Box(extra,new Vector3(0,.06f,-1.08f),new Vector3(1.38f,.12f,.72f),WoodDark);
                    Box(extra,new Vector3(0,annexHeight*.5f+.06f,-1.08f),new Vector3(1.32f,annexHeight,.65f),Wood);
                    Roof(extra,1.55f,.86f,annexHeight+.10f,.18f,true,new Vector3(0,0,-1.08f));break;
                case "CowShed":
                    Fence(extra,new Vector3(0,.10f,1.47f),false,3.7f,.76f);Fence(extra,new Vector3(-1.80f,.1f,.72f),true,1.5f,.76f);Fence(extra,new Vector3(1.80f,.1f,.72f),true,1.5f,.76f);
                    Trough(extra,new Vector3(-.65f,.1f,.84f),true);Bale(extra,new Vector3(1.0f,.1f,-.5f),.85f);HangingLantern(extra,new Vector3(1.27f,1.38f,.90f));break;
                case "PigPen":
                    Fence(extra,new Vector3(0,.07f,1.75f),false,3.25f,.76f);Fence(extra,new Vector3(-1.58f,.07f,.38f),true,2.75f,.76f);Fence(extra,new Vector3(1.58f,.07f,.38f),true,2.75f,.76f);
                    Trough(extra,new Vector3(-.60f,.08f,1.20f),true);break;
                case "Greenhouse":
                    extraGlass=new EnvironmentMesh(1);GlassAnnex(extra,extraGlass);HangingLantern(extra,new Vector3(-1.2f,1.46f,.91f));break;
                case "Nursery":
                    Box(extra,new Vector3(0,.2f,-1.55f),new Vector3(1.9f,.40f,.47f),WoodDark);
                    for(int i=0;i<7;i++){Vector3 p=new Vector3(-.78f+i*.26f,.43f,-1.55f);Box(extra,p,new Vector3(.20f,.11f,.30f),Dark);extra.Leaf(p+Vector3.up*.07f,Vector3.forward,.16f,.05f,Cloth);}
                    for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(extra,new Vector3(x*1.08f,0,-1.55f+z*.43f),new Vector3(x*1.08f,1.30f,-1.55f+z*.43f),.04f);
                    Roof(extra,2.54f,1.04f,1.30f,.21f,true,new Vector3(0,0,-1.55f));Crate(extra,new Vector3(1.27f,.07f,.53f));break;
                case "VillageWell":
                    Box(extra,new Vector3(0,-.05f,0),new Vector3(1.70f,.1f,1.55f),Stone);
                    Trough(extra,new Vector3(1.05f,0,.12f),true);Beam(extra,new Vector3(.59f,.38f,0),new Vector3(.77f,.38f,0),.035f,WoodDark);
                    HangingLantern(extra,new Vector3(-.6f,1.28f,.25f));break;
            }
            Add(root,"Level 3 additions",extra.Finish(type+" L3 additions"),Slice(materials),meshes,true);
            if(extraGlass!=null)Add(root,"Extended glass",extraGlass.Finish(type+" L3 glass"),new[]{materials[Colors.Length]},meshes,false);
            return root;
        }
        private static void SimpleHut(EnvironmentMesh mesh,float width,float depth,float height,bool home)
        {
            Box(mesh,new Vector3(0,.06f,0),new Vector3(width+.12f,.12f,depth+.12f),WoodDark);
            Box(mesh,new Vector3(0,height*.5f+.12f,0),new Vector3(width,height,depth),Wood);
            Roof(mesh,width+.30f,depth+.30f,height+.13f,.34f,false);
            Box(mesh,new Vector3(0,.75f,depth*.5f+.015f),new Vector3(.53f,1.24f,.045f),Dark);
            if(home)Window(mesh,-.60f,1.02f,depth*.5f+.03f,.30f);
        }
        private static void SmallCoop(EnvironmentMesh mesh,bool duck)
        {
            Box(mesh,new Vector3(0,.43f,0),new Vector3(.94f,.86f,.80f),Wood);
            Roof(mesh,1.15f,1.08f,.90f,.20f,false);Box(mesh,new Vector3(0,.23f,.407f),new Vector3(.34f,.40f,.035f),Dark);
            if(!duck)Box(mesh,new Vector3(-.26f,.58f,.417f),new Vector3(.20f,.17f,.025f),Water);
        }
        private static void LowColdFrame(EnvironmentMesh mesh,EnvironmentMesh glass)
        {
            Box(mesh,new Vector3(0,.10f,0),new Vector3(1.45f,.20f,1.02f),Wood);
            for(int side=-1;side<=1;side+=2)
            {Beam(mesh,new Vector3(side*.72f,.21f,-.5f),new Vector3(side*.72f,.65f,-.5f),.023f);Beam(mesh,new Vector3(side*.72f,.21f,.5f),new Vector3(side*.72f,.40f,.5f),.023f);
                Face(glass,new Vector3(side*.72f,.20f,-.5f),new Vector3(side*.72f,.65f,-.5f),new Vector3(side*.72f,.40f,.5f),new Vector3(side*.72f,.20f,.5f),0);}
            Face(glass,new Vector3(-.72f,.65f,-.5f),new Vector3(.72f,.65f,-.5f),new Vector3(.72f,.40f,.5f),new Vector3(-.72f,.40f,.5f),0);
            for(int x=0;x<4;x++)Beam(mesh,new Vector3(-.72f+x*.48f,.65f,-.5f),new Vector3(-.72f+x*.48f,.40f,.5f),.018f,WoodDark);
            for(int i=0;i<4;i++)mesh.Leaf(new Vector3(-.50f+i*.32f,.22f,0),Vector3.forward,.19f,.048f,Cloth);
        }
        private static void GlassAnnex(EnvironmentMesh mesh,EnvironmentMesh glass)
        {
            Vector3 o=new Vector3(1.68f,0,-.06f);Box(mesh,o+new Vector3(0,.05f,0),new Vector3(.96f,.10f,2.00f),Stone);
            for(int side=-1;side<=1;side+=2)
            {Beam(mesh,o+new Vector3(side*.46f,.10f,-.94f),o+new Vector3(side*.46f,1.63f,-.94f),.023f,StoneDark);Beam(mesh,o+new Vector3(side*.46f,.1f,.94f),o+new Vector3(side*.46f,1.63f,.94f),.023f,StoneDark);
                Face(glass,o+new Vector3(side*.46f,.10f,-.94f),o+new Vector3(side*.46f,1.63f,-.94f),o+new Vector3(side*.46f,1.63f,.94f),o+new Vector3(side*.46f,.10f,.94f),0);}
            Face(glass,o+new Vector3(-.46f,1.63f,-.94f),o+new Vector3(.46f,1.63f,-.94f),o+new Vector3(.46f,1.63f,.94f),o+new Vector3(-.46f,1.63f,.94f),0);
            Beam(mesh,o+new Vector3(-.46f,1.64f,.94f),o+new Vector3(.46f,1.64f,.94f),.026f,StoneDark);
            for(int i=0;i<5;i++){Vector3 p=o+new Vector3(0,.39f,-.70f+i*.34f);Box(mesh,p,new Vector3(.65f,.12f,.27f),Wood);mesh.Leaf(p+Vector3.up*.07f,Vector3.right,.17f,.048f,Cloth);}
        }
        private static void HangingLantern(EnvironmentMesh mesh,Vector3 p)
        {
            Beam(mesh,p+Vector3.up*.04f,p+Vector3.down*.08f,.008f,Dark);
            mesh.Tube(new[]{p+Vector3.down*.28f,p+Vector3.down*.08f},.085f,.085f,Red,12);
            mesh.Disc(p+Vector3.down*.08f,.087f,.087f,Gold,12);Beam(mesh,p+Vector3.down*.28f,p+Vector3.down*.36f,.010f,Gold);
        }
    }
}
