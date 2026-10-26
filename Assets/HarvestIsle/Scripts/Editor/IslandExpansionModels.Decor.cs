using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class IslandExpansionModels
    {
        static void Decor(EnvironmentMesh m,int n,Vector3 p,float s)
        {
            if(n==6)
            {
                Quaternion q=Quaternion.Euler(72,0,0);Vector3 c=p+V(0,.52f,.04f)*s;
                Ring(m,c,.48f*s,.035f*s,Bamboo,q);Ring(m,c,.43f*s,.012f*s,Wood,q);
                for(int i=-6;i<=6;i++){float f=i*.065f,len=Mathf.Sqrt(.43f*.43f-f*f);Beam(m,c+q*V(-len,0,f)*s,c+q*V(len,0,f)*s,.008f*s,Bamboo);Beam(m,c+q*V(f,.01f,-len)*s,c+q*V(f,.01f,len)*s,.008f*s,Straw);}
            }
            else if(n==11)
            {
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,p+V(x*.8f,0,z*.4f)*s,p+V(x*.8f,1.65f,z*.4f)*s,.035f*s,DarkWood);
                for(int row=0;row<3;row++)
                {float y=.35f+row*.52f;for(int z=0;z<7;z++)Beam(m,p+V(-.85f,y,-.42f+z*.14f)*s,p+V(.85f,y,-.42f+z*.14f)*s,.012f*s,Bamboo);for(int x=-1;x<=1;x++){Vector3 c=p+V(x*.53f,y+.018f,0)*s;m.Disc(c,.23f*s,.23f*s,White,20);Ring(m,c,.23f*s,.008f*s,Straw);}}
            }
            else if(n==13)
            {
                m.Tube(new[]{p+V(-1,1.1f,0)*s,p+V(0,1.28f,0)*s,p+V(1,1.1f,0)*s},.045f*s,.045f*s,Bamboo,8);
                for(int side=-1;side<=1;side+=2){Vector3 c=p+V(side*.78f,.035f,0)*s;Bowl(m,c,.34f*s,.28f*s,Bamboo);for(int j=-1;j<=1;j+=2)Beam(m,c+V(0,.25f,j*.28f)*s,p+V(side*.78f,1.14f,0)*s,.016f*s,DarkWood);Ring(m,c+V(0,.2f,0)*s,.31f*s,.025f*s,Wood);}
            }
            else if(n==17)
            {
                Box(m,p+V(0,.04f,0)*s,V(.75f,.08f,.55f)*s,Wood);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                {Vector3 c=p+V(x*.17f,.08f,z*.12f)*s;Vector3 a=c+V(-.14f,0,-.10f)*s,b=c+V(.14f,0,-.1f)*s,d=c+V(0,0,.16f)*s,top=c+V(0,.30f,0)*s;m.Tri(a,top,b,LeafDark);m.Tri(b,top,d,Green);m.Tri(d,top,a,LeafLight);Beam(m,c+V(-.13f,.08f,-.06f)*s,c+V(.12f,.08f,-.06f)*s,.009f*s,Straw);}
            }
            else DecorExtended(m,n,p,s);
        }
        static void DecorExtended(EnvironmentMesh m,int n,Vector3 p,float s)
        {
            void B(Vector3 at,Vector3 size,int c){Box(m,p+at*s,size*s,c);}
            void T(Vector3 a,Vector3 b,float r,int c=DarkWood){Beam(m,p+a*s,p+b*s,r*s,c);}
            void S(Vector3 at,Vector3 radius,int c){Ball(m,p+at*s,radius*s,c);}
            void R(Vector3 at,float r,float thickness,int c,bool vertical=false){Ring(m,p+at*s,r*s,thickness*s,c,vertical?Quaternion.Euler(90,0,0):Quaternion.identity);}
            void Pot(Vector3 at,float radius,float height,int c,bool lid=false){Vessel(m,p+at*s,radius*s,height*s,c,lid);}
            void L(Vector3 at,Vector3 dir,float length,float width,int c=Green,float lift=.15f){Leaf(m,p+at*s,dir,length*s,width*s,c,lift);}
            switch(n)
            {
                case 1:
                    Pot(V(0,0,0),.42f,.95f,Tile,true);T(V(.28f,.80f,0),V(.62f,1.12f,.05f),.024f,Bamboo);Bowl(m,p+V(.64f,1.07f,.05f)*s,.13f*s,.08f*s,Brown);break;
                case 2:
                    for(int i=-1;i<=1;i+=2){S(V(i*.13f,.05f,0),V(.10f,.045f,.22f),Wood);m.Tube(new[]{p+V(i*.13f-.08f,.05f,.03f)*s,p+V(i*.13f,.14f,.03f)*s,p+V(i*.13f+.08f,.05f,.03f)*s},.025f*s,.025f*s,Red,6);}break;
                case 3:
                    for(int side=-1;side<=1;side+=2){T(V(side*1.05f,0,-.4f),V(side*.95f,1.2f,0),.045f,Bamboo);T(V(side*1.05f,0,.4f),V(side*.95f,1.2f,0),.045f,Bamboo);T(V(side*.95f,1.18f,0),V(side*.7f,.65f,0),.018f,DarkWood);}
                    for(int i=0;i<10;i++){float z=-.30f+i*.067f;m.Tube(new[]{p+V(-.7f,.65f,z*.3f)*s,p+V(-.35f,.38f,z)*s,p+V(.35f,.38f,z)*s,p+V(.7f,.65f,z*.3f)*s},.025f*s,.025f*s,(i%3==0)?Red:Straw,6);}break;
                case 4:
                    T(V(-.65f,0,0),V(-.65f,1.5f,0),.045f);T(V(.65f,0,0),V(.65f,1.5f,0),.045f);T(V(-.7f,1.45f,0),V(.7f,1.45f,0),.045f);
                    for(int i=-1;i<=1;i++){Vector3 c=V(i*.46f,1.18f,.06f);m.Tube(new[]{p+c*s,p+(c+V(0,0,.27f))*s},.27f*s,.015f*s,Straw,18);R(c,.27f,.012f,Bamboo,true);}break;
                case 5:
                    Pot(V(.72f,0,0),.33f,.42f,StoneDark);T(V(-1,.35f,0),V(.85f,.75f,0),.08f,Wood);T(V(-.2f,0,-.3f),V(-.2f,.55f,0),.065f);T(V(-.2f,0,.3f),V(-.2f,.55f,0),.065f);T(V(.8f,.75f,0),V(.8f,.25f,0),.08f);B(V(-.8f,.37f,0),V(.35f,.055f,.38f),Wood);break;
                case 7:
                    R(V(0,.025f,0),.4f,.025f,Bamboo);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a));m.Tube(new[]{p+d*.4f*s+V(0,.025f*s,0),p+d*.28f*s+V(0,.32f*s,0),p+V(0,.45f,0)*s},.012f*s,.012f*s,Bamboo,5);}R(V(0,.25f,0),.32f,.012f,Straw);S(V(0,.47f,0),V(.05f,.04f,.05f),DarkWood);break;
                case 8:
                    T(V(-.25f,.15f,0),V(-.35f,1.35f,0),.025f,Bamboo);for(int i=0;i<15;i++)T(V(-.28f,.38f,0),V(-.52f+i*.035f,.02f,(i%3)*.025f),.009f,Straw);B(V(.25f,.025f,0),V(.45f,.05f,.42f),Bamboo);B(V(.25f,.18f,-.20f),V(.45f,.36f,.035f),Wood);T(V(.25f,.15f,-.2f),V(.25f,.82f,-.2f),.023f);break;
                case 9:
                    Bowl(m,p,.34f*s,.12f*s,Stone);T(V(-.38f,0,0),V(-.38f,.65f,0),.035f);T(V(.38f,0,0),V(.38f,.65f,0),.035f);T(V(-.4f,.56f,0),V(.4f,.56f,0),.025f);m.Tube(new[]{p+V(0,.8f,-.4f)*s,p+V(0,.4f,.4f)*s},.075f*s,.075f*s,Bamboo,10);break;
                case 10:
                    B(V(0,.015f,0),V(2.2f,.03f,1.2f),TileLight);for(int i=0;i<6;i++)T(V(-.8f+i*.32f,.034f,-.42f),V(-.8f+i*.32f,.034f,.42f),.008f,TileDark);for(int z=-1;z<=1;z++)T(V(-.8f,.034f,z*.42f),V(.8f,.034f,z*.42f),.008f,TileDark);for(int i=0;i<10;i++)S(V(-.65f+(i%5)*.32f,.055f,i<5?-.2f:.2f),V(.045f,.025f,.038f),Stone);break;
                case 12:
                    Bowl(m,p+V(0,.14f,0)*s,.4f*s,.1f*s,Red);T(V(0,0,0),V(0,.16f,0),.16f,Wood);for(int i=0;i<5;i++){float a=i*1.256f;CropFruit(m,20+i,V(Mathf.Cos(a)*.2f,.33f,Mathf.Sin(a)*.2f)*s+p,.55f*s,true);}break;
                case 14:
                    for(int i=-1;i<=1;i+=2)T(V(i*.25f,0,-.15f),V(i*.25f,.40f,-.15f),.05f,Wood);T(V(0,0,.26f),V(0,.4f,.26f),.05f,Wood);m.Tube(new[]{p+V(0,.4f,0)*s,p+V(0,.72f,0)*s},.39f*s,.36f*s,Stone,18);R(V(0,.55f,0),.39f,.018f,StoneDark);T(V(.25f,.71f,0),V(.25f,.94f,0),.025f,Wood);T(V(.25f,.94f,0),V(.47f,.94f,0),.025f,Wood);break;
                case 15:
                    B(V(0,.05f,0),V(.6f,.1f,.45f),Wood);B(V(0,.107f,0),V(.43f,.014f,.30f),DarkWood);for(int i=0;i<6;i++)L(V(0,.12f,0),V(Mathf.Cos(i*1.047f),0,Mathf.Sin(i*1.047f)),.18f,.06f,Straw,.02f);break;
                case 16:
                    T(V(0,.05f,0),V(0,1.15f,0),.018f,Wood);for(int i=0;i<8;i++){S(V((i%2==0?-.1f:.1f),.12f+i*.12f,0),V(.09f,.075f,.07f),White);m.Tube(new[]{p+V(.1f,.18f+i*.12f,.04f)*s,p+V(.2f,.14f+i*.12f,.03f)*s,p+V(.18f,.06f+i*.12f,.03f)*s},.035f*s,.004f*s,Red,7);}break;
                case 18:
                    B(V(0,.24f,0),V(.88f,.4f,.08f),TileLight);for(int side=-1;side<=1;side+=2){R(V(side*.33f,.39f,.05f),.035f,.012f,Dark,true);T(V(side*.33f,.40f,.02f),V(0,.68f,.02f),.012f,Bamboo);}B(V(0,.25f,.055f),V(.55f,.05f,.03f),Cream);B(V(0,.15f,.055f),V(.37f,.04f,.03f),Cream);break;
                case 19:
                    Pot(V(0,0,0),.25f,.32f,TileDark);R(V(0,.31f,0),.22f,.04f,Orange);Pot(V(0,.33f,0),.24f,.29f,Tile);m.Tube(new[]{p+V(.15f,.42f,0)*s,p+V(.30f,.49f,0)*s,p+V(.36f,.62f,0)*s},.07f*s,.035f*s,Tile,9);R(V(-.22f,.51f,0),.14f,.024f,DarkWood,true);R(V(0,.65f,0),.15f,.015f,Wood);break;
                case 20:
                    B(V(0,.05f,0),V(.85f,.1f,.65f),Bamboo);B(V(0,.18f,0),V(.65f,.2f,.5f),White);B(V(0,.31f,0),V(.75f,.05f,.55f),Wood);S(V(0,.42f,0),V(.25f,.12f,.18f),StoneDark);for(int side=-1;side<=1;side+=2)T(V(side*.35f,.08f,-.2f),V(side*.35f,.35f,.2f),.015f,Bamboo);break;
                case 21:
                    for(int side=-1;side<=1;side+=2)T(V(side*.28f,0,-.3f),V(side*.28f,1.1f,0),.07f,DarkWood);R(V(0,1.1f,0),.95f,.05f,Wood,true);R(V(0,1.1f,0),.75f,.025f,Bamboo,true);T(V(0,1.1f,-.42f),V(0,1.1f,.42f),.08f,DarkWood);
                    for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Vector3 d=V(Mathf.Cos(a),Mathf.Sin(a),0);T(V(0,1.1f,0),V(0,1.1f,0)+d*.93f,.028f);B(V(0,1.1f,0)+d*.9f,V(.18f,.16f,.45f),Wood);}break;
                case 22:
                    for(int row=0;row<6;row++)for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10,lo=row*Mathf.PI/12,hi=(row+1)*Mathf.PI/12;Vector3 a0=V(Mathf.Cos(a)*Mathf.Cos(lo)*.86f,.025f+Mathf.Sin(lo)*.7f,Mathf.Sin(a)*Mathf.Cos(lo)*.86f),a1=V(Mathf.Cos(a)*Mathf.Cos(hi)*.86f,.025f+Mathf.Sin(hi)*.7f,Mathf.Sin(a)*Mathf.Cos(hi)*.86f),b1=V(Mathf.Cos(b)*Mathf.Cos(hi)*.86f,.025f+Mathf.Sin(hi)*.7f,Mathf.Sin(b)*Mathf.Cos(hi)*.86f),b0=V(Mathf.Cos(b)*Mathf.Cos(lo)*.86f,.025f+Mathf.Sin(lo)*.7f,Mathf.Sin(b)*Mathf.Cos(lo)*.86f);Face(m,p+a0*s,p+a1*s,p+b1*s,p+b0*s,Bamboo);}R(V(0,.045f,0),.86f,.035f,DarkWood);for(int i=1;i<6;i++)R(V(0,.025f+Mathf.Sin(i*Mathf.PI/12)*.7f,0),Mathf.Cos(i*Mathf.PI/12)*.86f,.008f,Wood);T(V(-.9f,.1f,.65f),V(.7f,.1f,.65f),.027f,DarkWood);B(V(.65f,.1f,.65f),V(.35f,.045f,.18f),Wood);break;
                case 23:
                    T(V(0,0,0),V(0,.75f,0),.13f,DarkWood);for(int i=0;i<4;i++)R(V(0,.4f+i*.055f,0),.17f,.023f,Bamboo);m.Tube(new[]{p+V(.15f,.4f,0)*s,p+V(.33f,.08f,.1f)*s,p+V(.22f,.03f,.25f)*s},.025f*s,.025f*s,Bamboo,6);break;
                case 24:
                    for(int row=0;row<4;row++)Ring(m,p+V(0,.23f,-.3f+row*.2f)*s,(row==3?.12f:.21f)*s,.018f*s,Bamboo,Quaternion.Euler(90,0,0));for(int i=0;i<14;i++){float a=i*Mathf.PI/7;T(V(Mathf.Cos(a)*.21f,.23f+Mathf.Sin(a)*.21f,-.3f),V(Mathf.Cos(a)*.12f,.23f+Mathf.Sin(a)*.12f,.3f),.008f,Bamboo);}break;
                case 25:
                    T(V(0,0,-.8f),V(0,1.9f,-.5f),.07f,DarkWood);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)m.Tube(new[]{p+V(0,2,-.5f)*s,p+V(x*.65f,1.5f,z*.65f)*s,p+V(x*1.1f,.7f,z*1.1f)*s},.025f*s,.018f*s,Bamboo,6);
                    for(int i=-6;i<=6;i++){float v=i/6f*1.1f;m.Tube(new[]{p+V(-1.1f,.7f,v)*s,p+V(0,.3f,v)*s,p+V(1.1f,.7f,v)*s},.006f*s,.006f*s,White,4);m.Tube(new[]{p+V(v,.7f,-1.1f)*s,p+V(v,.3f,0)*s,p+V(v,.7f,1.1f)*s},.006f*s,.006f*s,White,4);}break;
                case 26:
                    for(int i=0;i<12;i++){float a=i*2.4f;Vector3 at=V(Mathf.Cos(a)*.55f,.04f,Mathf.Sin(a)*.36f);S(at,V(.17f,.045f,.13f),Green);S(at+V(0,.06f,0),V(.05f,.09f,.05f),LeafLight);if(i%4==0)Flower(m,p+(at+V(0,.2f,0))*s,.14f*s,Purple);}break;
                case 27:
                    B(V(0,.85f,0),V(.24f,1.7f,.20f),White);for(int i=0;i<11;i++)B(V(0,.12f+i*.14f,.108f),V(i%2==0?.15f:.09f,.025f,.02f),i<4?Red:Water);B(V(0,1.73f,0),V(.34f,.08f,.30f),Wood);break;
                case 28:
                    T(V(0,0,0),V(0,1.4f,0),.045f,DarkWood);R(V(0,.91f,.08f),.35f,.09f,White,true);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;S(V(Mathf.Cos(a)*.35f,.91f+Mathf.Sin(a)*.35f,.08f),V(.105f,.10f,.105f),Red);}B(V(0,.04f,0),V(.7f,.08f,.4f),Wood);break;
                case 29:
                    for(int i=0;i<12;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;m.Tri(p+V(0,.19f,0)*s,p+V(Mathf.Cos(b)*1.45f,.19f,Mathf.Sin(b)*1.45f)*s,p+V(Mathf.Cos(a)*1.45f,.19f,Mathf.Sin(a)*1.45f)*s,Wood);}
                    for(int i=0;i<13;i++){float a=i*Mathf.PI/12;T(V(0,.2f,0),V(Mathf.Cos(a)*1.45f,.2f,Mathf.Sin(a)*1.45f),.05f,Wood);if(i%2==0)T(V(Mathf.Cos(a)*1.45f,.2f,Mathf.Sin(a)*1.45f),V(Mathf.Cos(a)*1.45f,.65f,Mathf.Sin(a)*1.45f),.03f,DarkWood);}for(int i=0;i<12;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;T(V(Mathf.Cos(a)*1.45f,.65f,Mathf.Sin(a)*1.45f),V(Mathf.Cos(b)*1.45f,.65f,Mathf.Sin(b)*1.45f),.025f,Wood);}for(int i=0;i<3;i++){float a=i*Mathf.PI/2;T(V(Mathf.Cos(a)*1.05f,0,Mathf.Sin(a)*1.05f),V(Mathf.Cos(a)*1.05f,.19f,Mathf.Sin(a)*1.05f),.11f,StoneDark);}break;
                case 30:
                    B(V(0,.55f,0),V(1.45f,1.1f,.22f),Cream);for(int x=0;x<7;x++)for(int y=0;y<5;y++)S(V(-.62f+x*.20f+(y%2)*.05f,.13f+y*.2f,.14f),V(.12f,.08f,.045f),((x+y)%2==0)?White:Stone);break;
                case 31:
                    for(int x=-1;x<=1;x+=2){T(V(x*1.1f,0,-.35f),V(x*1.1f,1.9f,-.35f),.045f);T(V(x*1.1f,0,.35f),V(x*1.1f,1.9f,.35f),.045f);for(int i=0;i<5;i++)T(V(x*1.1f,.3f+i*.31f,-.35f),V(x*1.1f,.6f+i*.31f,.35f),.014f,Bamboo);}
                    for(int z=-1;z<=1;z+=2)m.Tube(new[]{p+V(-1.1f,1.9f,z*.35f)*s,p+V(-.55f,2.2f,z*.35f)*s,p+V(.55f,2.2f,z*.35f)*s,p+V(1.1f,1.9f,z*.35f)*s},.04f*s,.04f*s,Wood,7);for(int i=0;i<10;i++)L(V(-1+i*.22f,2.1f,.2f),V(i%2==0?1:-1,0,1),.28f,.1f,Green,-.3f);break;
                case 32:
                    Face(m,p+V(-1.15f,0,-.9f)*s,p+V(0,1.7f,-.9f)*s,p+V(0,1.7f,.9f)*s,p+V(-1.15f,0,.9f)*s,Cream);Face(m,p+V(0,1.7f,-.9f)*s,p+V(1.15f,0,-.9f)*s,p+V(1.15f,0,.9f)*s,p+V(0,1.7f,.9f)*s,Straw);T(V(0,0,1),V(0,1.75f,1),.035f,Wood);B(V(0,.025f,0),V(1.9f,.05f,1.7f),Bamboo);break;
                case 33:
                    R(V(0,.86f,0),.2f,.022f,Bamboo);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Vector3 d=V(Mathf.Cos(a)*.16f,0,Mathf.Sin(a)*.16f);T(d+V(0,.13f,0),d+V(0,.86f,0),.005f,DarkWood);for(int j=0;j<4;j++)S(d+V(0,.22f+j*.16f,0),V(.045f,.055f,.045f),j%2==0?Tile:Water);L(d+V(0,.1f,0),V(1,0,0),.12f,.045f,Gold,-.4f);}break;
                case 34:
                    T(V(0,0,0),V(0,.63f,0),.13f,TileLight);R(V(0,.06f,0),.28f,.055f,Tile);Bowl(m,p+V(0,.62f,0)*s,.43f*s,.12f*s,Water);m.Disc(p+V(0,.7f,0)*s,.36f*s,.36f*s,Water);break;
                case 35:
                    B(V(0,.32f,0),V(.28f,.64f,.25f),Stone);m.Tube(new[]{p+V(0,.64f,0)*s,p+V(0,.75f,0)*s},.20f*s,0,Stone,4);B(V(0,.37f,.14f),V(.035f,.23f,.03f),Cream);break;
                case 36:
                    for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)T(V(x*.45f,0,z*.45f),V(x*.32f,1.8f,0),.035f,Wood);for(int i=0;i<6;i++){float y=.17f+i*.28f,z=.45f*(1-y/1.8f);B(V(0,y,z),V(.85f,.04f,.13f),Bamboo);}break;
                case 37:
                    Ring(m,p+V(0,.27f,.57f)*s,.25f*s,.05f*s,DarkWood,Quaternion.Euler(0,0,90));Bowl(m,p+V(0,.38f,-.05f)*s,.52f*s,.28f*s,Wood);for(int side=-1;side<=1;side+=2){T(V(side*.25f,.4f,-.2f),V(side*.32f,.65f,-.8f),.03f,DarkWood);T(V(side*.25f,.4f,-.2f),V(side*.08f,.27f,.57f),.03f,DarkWood);T(V(side*.27f,0,-.28f),V(side*.25f,.4f,-.2f),.03f,DarkWood);}T(V(-.1f,.27f,.57f),V(.1f,.27f,.57f),.025f,StoneDark);for(int i=0;i<7;i++){float a=i*2.4f;Vector3 at=V(Mathf.Cos(a)*.3f,.75f,Mathf.Sin(a)*.3f-.05f);Flower(m,p+at*s,.14f*s,i%2==0?Gold:White);}break;
                case 38:
                    T(V(0,0,0),V(0,.48f,0),.14f,Tile);m.Disc(p+V(0,.53f,0)*s,.40f*s,.40f*s,TileLight);R(V(0,.53f,0),.40f,.035f,Tile);Face(m,p+V(-.16f,.55f,0)*s,p+V(.14f,.55f,0)*s,p+V(.08f,.85f,0)*s,p+V(.08f,.85f,0)*s,DarkWood);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;T(V(Mathf.Cos(a)*.28f,.55f,Mathf.Sin(a)*.28f),V(Mathf.Cos(a)*.35f,.55f,Mathf.Sin(a)*.35f),.007f,DarkWood);}break;
                case 39:
                    S(V(0,.3f,0),V(.20f,.28f,.16f),Tile);S(V(0,.63f,.05f),V(.16f,.16f,.14f),TileLight);S(V(0,.48f,-.22f),V(.18f,.32f,.16f),Tile);S(V(0,.34f,.19f),V(.13f,.16f,.1f),Brown);for(int x=-1;x<=1;x+=2){S(V(x*.1f,.76f,.03f),V(.05f,.10f,.05f),Tile);S(V(x*.06f,.66f,.18f),V(.023f,.023f,.012f),Black);}break;
                case 40:
                    R(V(0,.47f,0),.40f,.04f,DarkWood,true);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;L(V(Mathf.Cos(a)*.40f,.47f+Mathf.Sin(a)*.40f,0),V(Mathf.Cos(a),0,.8f),.15f,.05f,Green,.15f);}break;
                case 41:
                    for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)T(V(x*.65f,0,z*.45f),V(x*.65f,1.35f,z*.45f),.045f);T(V(-.65f,1.3f,-.45f),V(.65f,1.3f,-.45f),.05f);T(V(-.65f,.65f,.45f),V(.65f,.65f,.45f),.05f);for(int i=0;i<22;i++)T(V(-.57f+i*.054f,1.3f,-.45f),V(-.57f+i*.054f,.65f,.45f),.008f,i%4==0?Red:Straw);B(V(0,.55f,.38f),V(1.14f,.025f,.35f),Bamboo);break;
                case 42:
                    T(V(0,0,0),V(0,.75f,0),.08f,DarkWood);m.Disc(p+V(0,.16f,0)*s,.43f*s,.43f*s,Wood);m.Disc(p+V(0,.68f,0)*s,.37f*s,.37f*s,Stone);Pot(V(0,.69f,0),.18f,.3f,Tile);for(int i=0;i<3;i++){float a=i*2.094f;T(V(Mathf.Cos(a)*.32f,0,Mathf.Sin(a)*.32f),V(0,.55f,0),.04f,Wood);}break;
                case 43:
                    R(V(-.15f,.67f,0),.55f,.035f,Wood,true);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;T(V(-.15f,.67f,0),V(-.15f+Mathf.Cos(a)*.55f,.67f+Mathf.Sin(a)*.55f,0),.02f,Bamboo);}T(V(-.15f,0,-.22f),V(-.15f,.67f,0),.05f);T(V(-.15f,0,.22f),V(-.15f,.67f,0),.05f);T(V(.55f,.1f,0),V(.55f,.65f,0),.045f);S(V(.55f,.54f,0),V(.12f,.13f,.13f),White);T(V(-.15f,1.2f,0),V(.55f,.6f,0),.006f,White);break;
                case 44:
                    B(V(0,.06f,0),V(.94f,.05f,.72f),White);for(int x=-1;x<=1;x+=2)B(V(x*.48f,.07f,0),V(.04f,.09f,.8f),Wood);for(int z=-1;z<=1;z+=2)B(V(0,.07f,z*.38f),V(1,.09f,.04f),Wood);for(int i=0;i<7;i++)T(V(-.4f+i*.13f,.092f,-.33f),V(-.4f+i*.13f,.092f,.33f),.004f,Straw);break;
                case 45:
                    B(V(0,.025f,0),V(.72f,.05f,.50f),Wood);for(int i=0;i<6;i++){Vector3 at=V((i%3-1)*.22f,.05f,(i/3-.5f)*.22f);Pot(at,.09f,.32f+(i%2)*.08f,Bamboo);m.Disc(p+(at+V(0,.31f+(i%2)*.08f,0))*s,.06f*s,.06f*s,new[]{Red,Gold,Water,White,Purple,Green}[i]);}break;
                case 46:
                    for(int x=0;x<4;x++)for(int y=0;y<4;y++){Vector3 at=V(-.60f+x*.4f,.2f+y*.4f,0);B(at+V(0,.17f,0),V(.4f,.06f,.24f),TileLight);B(at+V(0,-.17f,0),V(.4f,.06f,.24f),TileLight);B(at+V(.17f,0,0),V(.06f,.3f,.24f),Tile);B(at+V(-.17f,0,0),V(.06f,.3f,.24f),Tile);T(at+V(-.15f,-.15f,0),at+V(.15f,.15f,0),.025f,Cream);}break;
                case 47:
                    for(int side=-1;side<=1;side+=2)T(V(side*.5f,0,0),V(side*.5f,1.3f,0),.035f,Wood);R(V(0,.78f,0),.44f,.028f,DarkWood,true);S(V(0,.78f,0),V(.37f,.37f,.012f),Straw);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;T(V(Mathf.Cos(a)*.36f,.78f+Mathf.Sin(a)*.36f,.015f),V(Mathf.Cos(a)*.47f,.78f+Mathf.Sin(a)*.47f,.015f),.007f,Bamboo);}break;
                case 48:
                    B(V(0,.85f,0),V(.75f,.08f,.08f),Wood);B(V(-.23f,.48f,.025f),V(.06f,.7f,.045f),Straw);B(V(.07f,.16f,.025f),V(.65f,.06f,.045f),Straw);B(V(.25f,.52f,.025f),V(.07f,.65f,.035f),Wood);for(int i=0;i<7;i++)B(V(-.22f,.2f+i*.08f,.052f),V(.035f,.008f,.01f),DarkWood);T(V(.40f,.82f,0),V(.40f,.1f,0),.006f,DarkWood);S(V(.4f,.08f,0),V(.04f,.07f,.04f),StoneDark);break;
                case 49:
                    m.Disc(p+V(0,.02f,0)*s,.87f*s,.87f*s,Straw);for(int i=1;i<=20;i++)R(V(0,.035f,0),i*.043f,.009f,i%4==0?Wood:Bamboo);break;
                case 50:
                    for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;Vector3 d=V(Mathf.Cos(a),Mathf.Sin(a),0),at=V(0,.55f,0)+d*.3f;S(at,V(.20f,.25f,.035f),Straw);T(V(0,.55f,.03f),at,.016f,DarkWood);}S(V(0,.55f,.05f),V(.1f,.1f,.035f),Wood);break;
                case 51:
                    for(int i=0;i<6;i++){float a=i*Mathf.PI/3;T(V(Mathf.Cos(a)*.3f,.12f,Mathf.Sin(a)*.3f),V(Mathf.Cos(a)*.3f,.95f,Mathf.Sin(a)*.3f),.025f,Red);}R(V(0,.13f,0),.32f,.03f,Red);R(V(0,.95f,0),.32f,.03f,Red);m.Tube(new[]{p+V(0,.95f,0)*s,p+V(0,1.2f,0)*s},.38f*s,0,Gold,6);T(V(0,.1f,0),V(0,.95f,0),.018f,Wood);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;S(V(Mathf.Cos(a)*.18f,.65f,Mathf.Sin(a)*.18f),V(.05f,.06f,.05f),DarkWood);B(V(Mathf.Cos(a)*.18f,.5f,Mathf.Sin(a)*.18f),V(.08f,.2f,.04f),DarkWood);}break;
                case 52:
                    B(V(0,.25f,0),V(.65f,.5f,.60f),DarkWood);S(V(0,.85f,0),V(.48f,.38f,.36f),Red);S(V(0,.68f,.28f),V(.38f,.14f,.19f),Gold);for(int x=-1;x<=1;x+=2){S(V(x*.22f,1.02f,.26f),V(.17f,.18f,.10f),White);S(V(x*.22f,1.03f,.35f),V(.09f,.10f,.035f),Black);S(V(x*.38f,.94f,0),V(.16f,.18f,.12f),Gold);}S(V(0,.87f,.37f),V(.13f,.08f,.08f),LeafDark);break;
                case 53:
                    for(int x=-1;x<=1;x+=2){T(V(x*.5f,0,-.3f),V(x*.5f,.55f,0),.06f);T(V(x*.5f,0,.3f),V(x*.5f,.55f,0),.06f);}S(V(0,.9f,0),V(.65f,.48f,.40f),Red);R(V(0,.9f,.39f),.47f,.04f,Gold,true);S(V(0,.9f,.41f),V(.46f,.46f,.018f),Straw);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;S(V(Mathf.Cos(a)*.45f,.9f+Mathf.Sin(a)*.45f,.44f),V(.018f,.018f,.014f),Gold);}T(V(-.35f,.35f,.4f),V(.2f,.9f,.45f),.022f,Wood);break;
                case 54:
                    m.Tube(new[]{p,p+V(0,2.3f,0)*s,p+V(.28f,3.15f,0)*s},.055f*s,.025f*s,Bamboo,8);R(V(.28f,2.85f,0),.32f,.024f,Wood);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Vector3 at=V(.28f+Mathf.Cos(a)*.32f,2.85f,Mathf.Sin(a)*.32f);Face(m,p+at*s,p+(at+V(.06f,0,0))*s,p+(at+V(.1f,-.7f,.08f))*s,p+(at+V(.04f,-.7f,.08f))*s,i%2==0?Red:Gold);}break;
                case 55:
                    for(int x=-1;x<=1;x+=2){T(V(x*.22f,0,0),V(x*.22f,1.95f,0),.025f,Bamboo);B(V(x*.22f,.55f,.1f),V(.15f,.05f,.23f),Wood);}B(V(0,.025f,0),V(.7f,.05f,.5f),DarkWood);break;
                case 56:
                    for(int x=-1;x<=1;x+=2)T(V(x*.85f,0,0),V(x*.85f,1.5f,0),.045f);T(V(-.9f,1.45f,0),V(.9f,1.45f,0),.045f);for(int i=-1;i<=1;i++){float r=.2f+(i+1)*.04f;T(V(i*.55f,1.45f,0),V(i*.55f,.95f+r,0),.012f,Bamboo);S(V(i*.55f,.95f,0),V(r,r,.035f),Gold);S(V(i*.55f,.95f,.04f),V(r*.3f,r*.3f,.05f),Brown);}break;
                case 57:
                    B(V(0,.08f,0),V(.6f,.16f,.5f),Water);S(V(0,.39f,0),V(.15f,.23f,.12f),Red);S(V(0,.71f,0),V(.13f,.14f,.12f),Straw);T(V(.12f,.5f,0),V(.3f,.68f,.03f),.035f,Straw);S(V(.33f,.78f,.04f),V(.14f,.12f,.025f),Gold);for(int x=-1;x<=1;x+=2)S(V(x*.045f,.73f,.12f),V(.012f,.014f,.008f),Black);break;
                case 58:
                    T(V(0,0,0),V(0,1.6f,0),.025f,Wood);Face(m,p+V(-.75f,1.1f,0)*s,p+V(0,1.65f,0)*s,p+V(.75f,1.1f,0)*s,p+V(0,.75f,0)*s,Cream);T(V(-.75f,1.1f,.015f),V(.75f,1.1f,.015f),.018f,Bamboo);for(int i=0;i<3;i++)T(V(-.2f+i*.2f,1.15f,.04f),V(-.2f+i*.2f,1.5f,.04f),.035f,Bamboo);m.Tube(new[]{p+V(0,.75f,0)*s,p+V(.2f,.35f,.02f)*s,p+V(-.12f,.08f,.04f)*s},.014f*s,.008f*s,Red,5);break;
                case 59:
                    B(V(0,.48f,0),V(1.9f,.96f,.10f),Water);B(V(0,1.66f,0),V(1.9f,.24f,.1f),Cream);for(int x=-1;x<=1;x+=2){B(V(x*.86f,1.25f,0),V(.18f,.65f,.1f),Cream);B(V(x*.08f,1.25f,0),V(.16f,.65f,.1f),Cream);T(V(x*.7f,0,-.35f),V(x*.7f,1.5f,0),.035f,DarkWood);S(V(x*.45f,.6f,.075f),V(.28f,.38f,.035f),Red);L(V(x*.45f,.95f,.07f),V(1,0,0),.3f,.12f,Gold,.05f);}break;
                case 60:
                    Table(m,p,1.25f*s,1.25f*s,.65f*s,Stone);for(int i=0;i<9;i++)T(V(-.5f+i*.125f,.706f,-.5f),V(-.5f+i*.125f,.706f,.5f),.005f,DarkWood);for(int i=0;i<10;i++)T(V(-.5f,.706f,-.5f+i*.111f),V(.5f,.706f,-.5f+i*.111f),.005f,DarkWood);for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++){Vector3 at=V(-.42f+i*.14f,.73f,side*.39f);m.Tube(new[]{p+at*s,p+(at+V(0,.035f,0))*s},.055f*s,.055f*s,side==1?Red:Dark,12);}break;
                default:throw new System.ArgumentOutOfRangeException(nameof(n));
            }
        }
    }
}
