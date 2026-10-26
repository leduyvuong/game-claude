using UnityEngine;
using System.Linq;

namespace HarvestIsle.Editor
{
    internal static partial class IslandExpansionModels
    {
        static void Building(EnvironmentMesh m,int n)
        {
            if(n==1)
            {
                Hut(m,4.8f,3.7f,2.3f);Vessel(m,V(-1.55f,.16f,.5f),.42f,.8f,Water);
                Table(m,V(.3f,.16f,.65f),1.6f,.8f,.72f);Beam(m,V(.3f,1.0f,.65f),V(.3f,1.65f,.65f),.10f,StoneDark);Bowl(m,V(.3f,1.63f,.65f),.3f,.25f,Stone);
                for(int i=0;i<9;i++)m.Tube(new[]{V(-.15f+i*.1f,1.02f,.78f),V(-.15f+i*.1f,.76f,.8f),V(-.12f+i*.1f,.65f,.83f)},.015f,.015f,White,5);
                for(int x=-1;x<=1;x+=2)Beam(m,V(x*1.6f,.16f,1.45f),V(x*1.6f,1.5f,1.45f),.045f,Bamboo);
                Beam(m,V(-1.6f,1.48f,1.45f),V(1.6f,1.48f,1.45f),.045f,Bamboo);
                for(int i=0;i<12;i++)m.Tube(new[]{V(-1.35f+i*.24f,.92f,1.4f),V(-1.35f+i*.24f,1.5f,1.45f),V(-1.32f+i*.24f,.93f,1.51f)},.018f,.018f,White,5);
                Bowl(m,V(0,1.97f,1.77f),.22f,.08f,White);
            }
            else if(n==2)
            {
                Hut(m,4.8f,4.7f,2.15f);Vessel(m,V(-1.4f,.16f,.2f),.5f,.8f,Tile);m.Disc(V(-1.4f,.99f,.2f),.44f,.44f,Dark);m.Disc(V(-1.4f,1.01f,.2f),.35f,.35f,White);Beam(m,V(-1.7f,.16f,-1.4f),V(-1.7f,3.08f,-1.4f),.18f,TileDark);
                Decor(m,11,V(.75f,.16f,.85f),1.2f);Table(m,V(-1.45f,.16f,1.3f),1,.7f,.65f);Ring(m,V(0,1.84f,2.25f),.16f,.035f,White,Quaternion.Euler(90,0,0));
            }
            else if(n==5)
            {
                Hut(m,3.8f,3.7f,2.05f,1);Table(m,V(.65f,.16f,.85f),1.65f,.9f,.75f);Vessel(m,V(-1.05f,.16f,.7f),.5f,.6f,TileDark);Bowl(m,V(-1.05f,.78f,.7f),.58f,.16f,Orange);m.Disc(V(-1.05f,.91f,.7f),.50f,.50f,Brown);
                Beam(m,V(-1.15f,.95f,.65f),V(-.6f,1.55f,.5f),.035f,Wood);
                for(int x=0;x<4;x++)for(int z=0;z<3;z++)Box(m,V(.12f+x*.32f,.99f,.59f+z*.25f),V(.25f,.08f,.17f),(x+z)%2==0?Gold:Straw);
                for(int i=0;i<3;i++)Vessel(m,V(-1.2f+i*.44f,.16f,-.6f),.18f,.38f,Tile,true);
                Ball(m,V(0,1.77f,1.77f),V(.25f,.09f,.05f),Gold);Beam(m,V(-.35f,1.77f,1.77f),V(.35f,1.77f,1.77f),.035f,White);
            }
            else BuildingExtended(m,n);
        }
        static void BuildingExtended(EnvironmentMesh m,int n)
        {
            if(n==3)
            {
                Hut(m,4.8f,3.7f,2.25f);for(int i=0;i<3;i++){Vector3 p=V(-1.3f+i*.9f,.16f,.8f);Box(m,p+V(0,.3f-i*.045f,0),V(.76f,.60f-i*.09f,.85f),TileLight);m.Disc(p+V(0,.61f-i*.09f,0),.31f,.35f,White);}
                for(int side=-1;side<=1;side+=2){Beam(m,V(side*.8f,.16f,1.6f),V(side*.8f,1.65f,1.6f),.045f);Ball(m,V(side*.5f,1.1f,1.6f),V(.18f,.30f,.16f),White);Beam(m,V(side*.5f,1.38f,1.6f),V(side*.5f,1.65f,1.6f),.014f,Bamboo);}Beam(m,V(-.8f,1.65f,1.6f),V(.8f,1.65f,1.6f),.04f,Wood);
            }
            else if(n==4)
            {
                Hut(m,3.8f,3.7f,2.1f);Table(m,V(0,.16f,.6f),2.3f,.8f,.7f);
                Beam(m,V(-.9f,1.05f,.6f),V(.8f,1.05f,.6f),.18f,Wood);for(int i=0;i<11;i++)Ring(m,V(-.9f+i*.15f,1.05f,.6f),.20f,.022f,DarkWood,Quaternion.Euler(0,0,90));Bowl(m,V(-.6f,1.18f,.6f),.28f,.4f,Straw);Ring(m,V(.95f,1.05f,.6f),.43f,.038f,DarkWood,Quaternion.Euler(0,0,90));Vessel(m,V(1.2f,.16f,1.2f),.25f,.52f,Tile);Vessel(m,V(-1.2f,.16f,1.3f),.27f,.6f,Tile,true);
            }
            else if(n==6)
            {
                Hut(m,4.8f,3.7f,2.35f,2);Ball(m,V(-.65f,1,.7f),V(.65f,.5f,.44f),Orange);Ring(m,V(-.65f,1,1.13f),.40f,.035f,DarkWood,Quaternion.Euler(90,0,0));Beam(m,V(-.65f,1.45f,.7f),V(-.65f,2.75f,.7f),.12f,StoneDark);Bowl(m,V(-.65f,.45f,1.1f),.5f,.13f,Stone);Table(m,V(1.0f,.16f,.8f),1,.7f,.55f);for(int i=0;i<3;i++)Vessel(m,V(-1.55f+i*.45f,.16f,-.65f),.20f,.42f,Straw,true);
            }
            else if(n==7)
            {
                OpenFrame(m,V(0,0,-.65f),4.8f,2.4f,2.1f,.4f);Box(m,V(0,.08f,0),V(4.8f,.16f,4.7f),Stone);for(int i=0;i<3;i++){Box(m,V(-1.5f+i*.9f,.48f,-.55f),V(.8f,.64f,.8f),Wood);Box(m,V(-1.5f+i*.9f,.81f,-.55f),V(.68f,.035f,.68f),Brown);}Decor(m,11,V(-.65f,.16f,1.1f),.85f);Ring(m,V(1.5f,.85f,.9f),.62f,.13f,Stone,Quaternion.Euler(90,0,0));Beam(m,V(1.5f,.16f,.8f),V(1.5f,.85f,.8f),.10f,DarkWood);Table(m,V(1.2f,.16f,1.85f),1.1f,.5f,.42f);
            }
            else if(n==8)
            {
                Hut(m,3.8f,2.7f,2.05f);Table(m,V(0,.16f,.65f),2.7f,.85f,.64f);for(int i=0;i<3;i++){Bowl(m,V(-.8f+i*.8f,.88f,.6f),.28f,.12f,Stone);CropFruit(m,7+i,V(-.8f+i*.8f,1.04f,.6f),.48f,true);}Beam(m,V(-.9f,1.0f,.5f),V(-.55f,1.55f,.5f),.055f,Wood);Decor(m,16,V(-1.35f,.55f,-.2f),.65f);Decor(m,16,V(1.35f,.55f,-.2f),.65f);
            }
            else if(n==9)
            {
                Box(m,V(0,.08f,0),V(4.8f,.16f,3.8f),Stone);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,V(x*2.05f,.16f,z*1.5f),V(x*2.05f,2.1f+(z<0?.55f:0),z*1.5f),.06f,DarkWood);
                Face(m,V(-2.25f,2.7f,-1.75f),V(2.25f,2.7f,-1.75f),V(2.25f,2.1f,1.75f),V(-2.25f,2.1f,1.75f),Water);for(int i=0;i<8;i++)Beam(m,V(-2.15f+i*.61f,2.71f,-1.75f),V(-2.15f+i*.61f,2.11f,1.75f),.035f,Cream);
                Decor(m,11,V(-1.05f,.16f,0),1.05f);Decor(m,11,V(1.05f,.16f,0),1.05f);Table(m,V(0,.16f,1.4f),3.8f,.6f,.7f);for(int i=0;i<6;i++)CropFruit(m,i%2==0?28:22,V(-1.6f+i*.62f,i%2==0?1.07f:1.005f,1.4f),.4f,true);
            }
            else if(n==10)
            {
                Hut(m,3.8f,3.7f,2.05f);for(int i=0;i<5;i++)Vessel(m,V(-1.35f+i*.67f,.16f,.65f),.29f,.73f,Tile,true);for(int i=0;i<3;i++)Vessel(m,V(-.8f+i*.8f,.16f,-.55f),.33f,.9f,TileDark,true);Table(m,V(0,.16f,1.45f),1.5f,.55f,.5f);CropFruit(m,14,V(0,.74f,1.45f),.45f,true);
            }
            else if(n==11)
            {
                Box(m,V(0,.08f,0),V(4.8f,.16f,4.8f),Stone);Ball(m,V(-1.15f,.96f,-.65f),V(1,.8f,1.15f),Tile);Box(m,V(-1.15f,.4f,.42f),V(.6f,.5f,.08f),Dark);Beam(m,V(-1.65f,1.0f,-1.45f),V(-1.65f,2.5f,-1.45f),.23f,TileDark);OpenFrame(m,V(1.0f,0,-.1f),2.6f,3.3f,2.05f,.38f);Decor(m,42,V(.8f,.16f,.7f),1.2f);for(int i=0;i<4;i++)Vessel(m,V(-1.8f+i*.8f,.16f,1.7f),.24f,.55f,TileLight);
            }
            else if(n==12)
            {
                OpenFrame(m,Vector3.zero,4.8f,3.7f,2.15f,.35f);Decor(m,41,V(-.7f,.16f,.25f),1.35f);Decor(m,43,V(1.35f,.16f,.6f),.95f);for(int i=0;i<4;i++)Beam(m,V(-1.5f+i*.42f,.3f,-1.2f),V(-1.5f+i*.42f,1.45f,-1.2f),.13f,i%2==0?Straw:Red);
            }
            else if(n==13)
            {
                OpenFrame(m,V(0,0,-.85f),4.8f,2,2.1f,.32f);Box(m,V(0,.08f,0),V(4.8f,.16f,3.8f),Stone);Box(m,V(-1.25f,.47f,-.55f),V(1.5f,.62f,1),TileLight);Box(m,V(-1.25f,.79f,-.55f),V(1.35f,.025f,.85f),Water);Table(m,V(1.1f,.16f,-.4f),1.4f,.85f,.6f);Decor(m,44,V(1.1f,.79f,-.4f),1.1f);
                for(int i=0;i<5;i++){float x=-1.65f+i*.8f;Beam(m,V(x,.16f,1),V(x,1.55f,1.3f),.035f,Wood);Face(m,V(x-.3f,.45f,1.08f),V(x+.3f,.45f,1.08f),V(x+.3f,1.45f,1.29f),V(x-.3f,1.45f,1.29f),White);}
            }
            else if(n==14)
            {
                Hut(m,4.8f,4.7f,2.25f,2);Table(m,V(0,.16f,1),2.8f,1,.7f);for(int i=0;i<4;i++)Beam(m,V(-1.2f,1.0f,.7f+i*.18f),V(.4f,1.0f,.7f+i*.18f),.065f,Wood);Decor(m,48,V(1.4f,.86f,-.6f),.9f);for(int i=0;i<5;i++)Beam(m,V(-1.8f,.2f+i*.12f,-1.3f),V(-1.8f,.2f+i*.12f,.4f),.06f,Wood);Decor(m,36,V(1.7f,.16f,.9f),.65f);
            }
            else if(n==15)
            {
                Hut(m,3.8f,2.7f,2.05f);Table(m,V(0,.16f,.75f),2.7f,.8f,.7f);for(int i=0;i<4;i++){Vector3 at=V(-.95f+i*.62f,1.02f,.75f);Box(m,at,V(.43f,.28f,.36f),i%2==0?Cream:Red);Box(m,at+V(0,.145f,0),V(.05f,.025f,.39f),Gold);Box(m,at+V(0,.145f,0),V(.46f,.025f,.05f),Gold);}for(int i=0;i<4;i++)Beam(m,V(-1.1f+i*.6f,1.6f,-.45f),V(-1.1f+i*.6f,1.9f,-.45f),.13f,i%2==0?Water:Straw);
            }
            else if(n==16)
            {
                m.Tube(new[]{V(0,0,0),V(0,.16f,0)},2,2,Stone,8);m.Tube(new[]{V(0,.16f,0),V(0,1.65f,0)},1.5f,1.5f,Wood,8);m.Tube(new[]{V(0,1.75f,0),V(0,2.6f,0)},2.05f,0,Tile,8);
                for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a));for(int row=0;row<3;row++){Vector3 at=d*1.48f+V(0,.42f+row*.4f,0);Ball(m,at,V(.12f,.12f,.12f),row%2==0?Gold:White);}}Table(m,V(0,.16f,1.6f),1.1f,.55f,.58f);Bowl(m,V(0,.81f,1.6f),.28f,.1f,Straw);
            }
            else if(n==17)
            {
                OpenFrame(m,Vector3.zero,2.8f,3.7f,2.0f,.28f);Table(m,V(0,.16f,.65f),2.2f,.9f,.68f);for(int i=0;i<4;i++){Vector3 at=V(-.75f+i*.5f,.9f,.65f);for(int row=0;row<3;row++)Box(m,at+V(0,row*.12f,0),V(.36f,.12f,.3f),new[]{Brown,StoneDark,Gold}[row]);}Box(m,V(0,1.35f,-.8f),V(1.65f,.9f,.08f),DarkWood);for(int i=0;i<4;i++)Box(m,V(-.6f+i*.4f,1.25f,-.75f),V(.22f,.3f+i*.1f,.03f),LeafLight);
            }
            else if(n==18)
            {
                OpenFrame(m,V(0,0,-.45f),3.8f,2.5f,2.15f,.3f);Box(m,V(0,.08f,0),V(3.8f,.16f,3.8f),Stone);for(int i=0;i<3;i++){float x=-1+i;Beam(m,V(x,.16f,-.5f),V(x,.85f,.6f),.10f,StoneDark);Ring(m,V(x,.95f,.65f),.23f,.035f,Red,Quaternion.Euler(90,0,0));Beam(m,V(x-.23f,.95f,.65f),V(x+.23f,.95f,.65f),.024f,Red);Box(m,V(x,.28f,1.1f),V(.65f,.24f,1),TileLight);Box(m,V(x,.41f,1.1f),V(.5f,.015f,.9f),Water);}Vessel(m,V(0,.16f,-1.1f),.6f,.85f,Water);
            }
            else if(n==19||n==27)
            {
                float w=n==19?3.8f:4.8f;OpenFrame(m,Vector3.zero,w,3.7f,2.1f,.4f);
                for(int side=-1;side<=1;side+=2)for(int i=0;i<15;i++)Beam(m,V(side*(w*.44f),.2f,-1.5f+i*.21f),V(side*w*.44f,2.05f,-1.5f+i*.21f),.009f,Bamboo);
                for(int y=0;y<6;y++)for(int side=-1;side<=1;side+=2)Beam(m,V(side*w*.44f,.3f+y*.31f,-1.5f),V(side*w*.44f,.3f+y*.31f,1.5f),.009f,Bamboo);
                if(n==19){for(int i=0;i<3;i++){Table(m,V(-1+i,.16f,.3f),.75f,1.2f,.55f);for(int k=0;k<3;k++){Leaf(m,V(-1+i,.78f,-.15f+k*.4f),V(1,0,.2f),.4f,.14f,Green);Ball(m,V(-.85f+i,.88f,-.15f+k*.4f),V(.065f,.04f,.05f),Red);}}}
                else{for(int side=-1;side<=1;side+=2){Beam(m,V(side*1.3f,.16f,0),V(side*1.3f,1.7f,0),.045f,Wood);Beam(m,V(side*1.3f-.5f,1.4f,0),V(side*1.3f+.5f,1.4f,0),.035f,Wood);Bowl(m,V(side*1.3f,.2f,1.1f),.3f,.12f,Water);}for(int i=0;i<3;i++)Bird(m,V(-1.3f+i*1.3f,1.55f,0),.35f);}
            }
            else if(n==20)
            {
                OpenFrame(m,V(0,0,-.4f),3.8f,2.3f,2.05f,.4f);Box(m,V(0,.08f,0),V(3.8f,.16f,2.8f),Stone);Table(m,V(0,.16f,.6f),2.6f,.7f,.65f);for(int side=-1;side<=1;side+=2){Vessel(m,V(side*.75f,.86f,.6f),.17f,.22f,Tile);Beam(m,V(side*.75f,1.0f,.6f),V(side*.75f,1.6f,.6f),.025f,Wood);for(int i=0;i<(side<0?3:6);i++){float a=i*2.4f;Leaf(m,V(side*.75f,1.4f,.6f),V(Mathf.Cos(a),0,Mathf.Sin(a)),.35f,.11f,Green);}}Decor(m,36,V(1.4f,.16f,-.6f),.7f);
            }
            else if(n==21)
            {
                Hut(m,5.8f,4.7f,2.6f);Roof(m,V(0,0,1.85f),5.3f,1.3f,1.85f,.22f);for(int i=0;i<3;i++){Table(m,V(-1.7f+i*1.7f,.16f,1.55f),1.25f,.8f,.6f);Vessel(m,V(-1.7f+i*1.7f,.82f,1.55f),.22f,.3f,Straw);}Beam(m,V(0,1.6f,1.5f),V(0,2.0f,1.5f),.035f,Wood);Beam(m,V(-.5f,1.92f,1.5f),V(.5f,1.92f,1.5f),.025f,Gold);
            }
            else if(n==22)
            {
                Box(m,V(0,.35f,0),V(5.8f,.18f,4.7f),DarkWood);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,V(x*2.5f,0,z*2),V(x*2.5f,2.35f,z*2),.09f,Wood);
                Box(m,V(0,1.25f,-.75f),V(5.1f,1.65f,1.9f),Cream);Roof(m,V(0,0,-.65f),5.6f,3.1f,2.35f,.7f);Roof(m,V(0,0,.8f),5.9f,2.7f,1.95f,.25f);for(int i=0;i<5;i++)Box(m,V(-1.8f+i*.9f,1.35f,.22f),V(.58f,.78f,.04f),DarkWood);for(int side=-1;side<=1;side+=2){Table(m,V(side*1.7f,.44f,1.45f),1.1f,.6f,.65f);Decor(m,57,V(side*1.7f,1.14f,1.45f),.65f);}for(int i=0;i<3;i++)Box(m,V(0,.08f+i*.1f,2.5f-i*.2f),V(1.4f,.16f,.3f),Stone);
            }
            else if(n==23)
            {
                OpenFrame(m,Vector3.zero,4.8f,3.7f,2.2f,.35f);Box(m,V(0,1.25f,-1.3f),V(2.4f,1.2f,.10f),DarkWood);Box(m,V(0,1.25f,-1.23f),V(2.2f,1,.03f),LeafDark);for(int i=0;i<4;i++)Beam(m,V(-.85f,1.55f-i*.18f,-1.2f),V(.4f-i*.15f,1.55f-i*.18f,-1.2f),.01f,White);
                for(int x=-1;x<=1;x+=2)for(int z=0;z<2;z++){Table(m,V(x*1.05f,.16f,.1f+z*.9f),1.3f,.5f,.6f);Table(m,V(x*1.05f,.16f,.43f+z*.9f),1.15f,.25f,.32f);}Box(m,V(0,.25f,1.55f),V(.55f,.18f,.55f),Brown);Leaf(m,V(0,.35f,1.55f),V(1,0,0),.3f,.12f,Green);
            }
            else if(n==24)
            {
                Box(m,V(0,.25f,0),V(5.8f,.5f,3.8f),Wood);OpenFrame(m,V(0,.5f,-.2f),5.7f,3.4f,2.05f,.45f);for(int i=0;i<5;i++)Box(m,V(-2.4f+i*1.2f,1.5f,-1.55f),V(.95f,2.0f,.08f),i%2==0?Red:TileDark);Decor(m,56,V(-1.55f,.5f,0),1);Decor(m,53,V(1.5f,.5f,.1f),.8f);for(int i=0;i<3;i++)Box(m,V(0,.08f+i*.13f,2.1f-i*.18f),V(1.65f,.16f,.3f),Stone);
            }
            else if(n==25)
            {
                Hut(m,3.8f,3.7f,2.1f,1);Table(m,V(-.6f,.16f,.6f),1.6f,.9f,.6f);Box(m,V(-.6f,.82f,.6f),V(1.5f,.08f,.8f),Cream);Bowl(m,V(1.2f,.16f,1.0f),.42f,.15f,Water);Decor(m,49,V(1.0f,.17f,-.3f),.65f);for(int i=0;i<5;i++)Beam(m,V(-.8f+i*.12f,1.1f,-.55f),V(-.8f+i*.12f,1.4f,-.55f),.03f,Bamboo);
            }
            else if(n==26)
            {
                Box(m,V(0,.08f,0),V(5.8f,.16f,4.8f),Stone);for(int side=-1;side<=1;side+=2){Box(m,V(side*1.65f,1.1f,-.45f),V(2.1f,1.9f,2.6f),Cream);Roof(m,V(side*1.65f,0,-.4f),2.55f,3.1f,2.05f,.6f);Box(m,V(side*1.65f,.95f,.88f),V(.65f,1.55f,.05f),Wood);Box(m,V(side*2.25f,1.3f,.88f),V(.38f,.5f,.05f),Water);}Roof(m,V(0,0,1.4f),5.7f,1.4f,1.75f,.20f);for(int side=-1;side<=1;side+=2)Beam(m,V(side*2.5f,.16f,1.9f),V(side*2.5f,1.78f,1.9f),.06f,Wood);Decor(m,3,V(0,.16f,1.2f),.6f);Vessel(m,V(-2.4f,.16f,1.65f),.22f,.4f,Tile);
            }
            else if(n==28)
            {
                OpenFrame(m,V(0,0,-.6f),4.8f,2.5f,2.1f,.35f);Box(m,V(0,.08f,0),V(4.8f,.16f,3.8f),Stone);Table(m,V(0,.16f,.2f),3.8f,1.0f,.6f);for(int i=0;i<16;i++)Beam(m,V(-1.75f+i*.23f,.84f,-.2f),V(-1.75f+i*.23f,.84f,.6f),.06f,StoneDark);for(int i=0;i<3;i++){Box(m,V(-1.3f+i*1.3f,.38f,1.25f),V(1,.44f,.75f),Wood);CropFruit(m,22+i,V(-1.3f+i*1.3f,.7f,1.25f),.6f,true);for(int star=0;star<=i;star++)Ball(m,V(-1.55f+i*1.3f+star*.18f,.45f,1.64f),V(.06f,.06f,.025f),Gold);}
            }
            else if(n==29)
            {
                Hut(m,3.8f,3.7f,2.1f);Box(m,V(-1.2f,1.5f,-.7f),V(.95f,2.7f,1),TileLight);Roof(m,V(-1.2f,0,-.7f),1.25f,1.3f,2.9f,.55f);Box(m,V(-1.2f,2.45f,-.16f),V(.55f,.24f,.05f),DarkWood);Table(m,V(.5f,.16f,.8f),1.8f,.75f,.7f);for(int i=0;i<3;i++)Box(m,V(-.2f+i*.6f,1.03f,.8f),V(.42f,.2f,.3f),Cream);Vessel(m,V(-1.25f,.16f,1.1f),.30f,.8f,Straw);Beam(m,V(-1.4f,.9f,1.1f),V(-1.1f,.9f,1.1f),.015f,DarkWood);
            }
            else if(n==30)
            {
                OpenFrame(m,Vector3.zero,4.8f,2.8f,2.05f,.3f);for(int i=0;i<3;i++)Bicycle(m,V(-1.45f+i*1.45f,.16f,.35f),.75f);Box(m,V(0,1.32f,-1.05f),V(2.3f,.95f,.08f),DarkWood);Box(m,V(0,1.32f,-1),V(2.1f,.78f,.03f),Cream);Ring(m,V(0,1.32f,-.97f),.27f,.035f,LeafDark,Quaternion.Euler(90,0,0));
            }
            else throw new System.ArgumentOutOfRangeException(nameof(n));
            TradeRelief(m,n);
        }
        static void OpenFrame(EnvironmentMesh m,Vector3 p,float w,float d,float h,float rise)
        {
            Box(m,p+V(0,.08f,0),V(w,.16f,d),Stone);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){Vector3 a=p+V(x*w*.43f,.16f,z*d*.4f),b=p+V(x*w*.43f,h,z*d*.4f);Beam(m,a,b,.065f,DarkWood);Beam(m,b-Vector3.up*.35f,b+V(-x*.35f,0,0),.035f,Wood);}
            Roof(m,p,w+.2f,d+.2f,h,rise);Beam(m,p+V(-w*.43f,h-.12f,-d*.4f),p+V(w*.43f,h-.12f,-d*.4f),.055f,Wood);
        }
        static void Bird(EnvironmentMesh m,Vector3 p,float s)
        {Ball(m,p,V(.25f,.14f,.13f)*s,Brown);Ball(m,p+V(.2f,.12f,0)*s,V(.12f,.12f,.11f)*s,DarkWood);m.Tube(new[]{p+V(.28f,.12f,0)*s,p+V(.43f,.1f,0)*s},.04f*s,0,Gold,6);Leaf(m,p,V(-1,0,.3f),.42f*s,.10f*s,DarkWood,.2f);}
        static void Bicycle(EnvironmentMesh m,Vector3 p,float s)
        {
            for(int x=-1;x<=1;x+=2){Ring(m,p+V(x*.65f,.36f,0)*s,.35f*s,.028f*s,Dark,Quaternion.Euler(90,0,0));for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Beam(m,p+V(x*.65f,.36f,0)*s,p+V(x*.65f+Mathf.Cos(a)*.34f,.36f+Mathf.Sin(a)*.34f,0)*s,.006f*s,Stone);}}
            Vector3[] frame={V(-.65f,.36f,0),V(-.2f,.84f,0),V(.05f,.36f,0),V(-.65f,.36f,0),V(.4f,.85f,0),V(.65f,.36f,0),V(.05f,.36f,0),V(.4f,.85f,0)};m.Tube(frame.Select(v=>p+v*s).ToArray(),.025f*s,.025f*s,Water,7);Beam(m,p+V(-.2f,.84f,0)*s,p+V(-.2f,1.0f,0)*s,.025f*s,DarkWood);Box(m,p+V(-.2f,1.02f,0)*s,V(.28f,.05f,.16f)*s,Dark);Beam(m,p+V(.4f,.85f,0)*s,p+V(.35f,1.15f,0)*s,.025f*s,Stone);Beam(m,p+V(.35f,1.15f,-.18f)*s,p+V(.35f,1.15f,.18f)*s,.025f*s,Dark);
        }
        static void TradeRelief(EnvironmentMesh m,int n)
        {
            float y=n==21?2.2f:n==22?1.68f:1.7f,z=n==21?2.1f:n==22?2.05f:n==23?1.75f:n==24?1.55f:n==26?2.0f:1.55f;
            Vector3 p=V(0,y,z);Box(m,p,V(.85f,.4f,.08f),DarkWood);Box(m,p+V(0,0,.05f),V(.76f,.31f,.03f),Straw);
            if(n<=10){int crop=n==3?1:n==4?4:n==6?29:n==7?30:n==8?7:n==9?28:14;CropFruit(m,crop,p+V(0,0,.13f),.38f,true);}
            else if(n==11){Vessel(m,p+V(0,-.13f,.10f),.10f,.24f,Tile);}
            else if(n==12||n==13||n==14){for(int i=0;i<5;i++)Beam(m,p+V(-.22f+i*.11f,-.10f,.09f),p+V(-.22f+i*.11f,.10f,.09f),.015f,Wood);}
            else if(n==15||n==29){Box(m,p+V(0,0,.09f),V(.4f,.20f,.03f),White);Beam(m,p+V(-.2f,.1f,.12f),p+V(0,-.02f,.12f),.012f,Red);Beam(m,p+V(.2f,.1f,.12f),p+V(0,-.02f,.12f),.012f,Red);}
            else if(n==16){for(int i=0;i<3;i++)Ball(m,p+V(-.16f+i*.16f,0,.12f),V(.06f,.1f,.04f),Gold);}
            else if(n==17||n==28){for(int i=0;i<3;i++)Box(m,p+V(-.18f+i*.18f,0,.11f),V(.13f,.08f+i*.05f,.04f),i==0?Brown:i==1?Gold:White);}
            else if(n==18||n==30){Ring(m,p+V(-.16f,0,.1f),.10f,.02f,Water,Quaternion.Euler(90,0,0));Ring(m,p+V(.16f,0,.1f),.10f,.02f,Water,Quaternion.Euler(90,0,0));}
            else if(n==19||n==27){Bird(m,p+V(-.04f,-.03f,.13f),.5f);}
            else if(n==20){Beam(m,p+V(-.2f,-.1f,.10f),p+V(.2f,.1f,.10f),.022f,Stone);Beam(m,p+V(.2f,-.1f,.10f),p+V(-.2f,.1f,.10f),.022f,Stone);}
            else if(n==25){Ball(m,p+V(0,-.03f,.1f),V(.10f,.07f,.025f),Tile);for(int i=0;i<3;i++)Ball(m,p+V(-.12f+i*.12f,.08f,.1f),V(.04f,.04f,.02f),Tile);}
            else{Box(m,p+V(-.12f,0,.10f),V(.22f,.19f,.03f),White);Box(m,p+V(.12f,0,.10f),V(.22f,.19f,.03f),Cream);}
        }
    }
}
