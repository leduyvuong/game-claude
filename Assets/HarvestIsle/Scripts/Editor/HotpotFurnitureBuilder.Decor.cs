using UnityEngine;

namespace HarvestIsle.Editor
{
    internal sealed partial class HotpotFurnitureBuilder
    {
        void Cabinet(Transform p,float w=1.5f,float h=.95f,float d=.60f,string mat=Wood,bool open=false)
        {
            B(p,0,.08f,0,w,.12f,d,Dark);B(p,0,h,0,w+.08f,.09f,d+.06f,mat);
            foreach(int s in new[]{-1,1})B(p,s*(w/2-.04f),h/2,0,.08f,h,d,mat);
            B(p,0,h/2,d*.46f,w,h,.045f,mat);B(p,0,h*.46f,0,w,.045f,d,mat);
            if(!open)foreach(int s in new[]{-1,1}){B(p,s*w*.245f,h*.53f,-d*.50f,w*.46f,h*.75f,.05f,mat);B(p,s*.075f,h*.65f,-d*.55f,.035f,.16f,.035f,Gold);}
        }
        void Cart(Transform p,string mat=Wood)
        {
            foreach(float y in new[]{.23f,.80f})B(p,0,y,0,1.14f,.06f,.66f,mat);
            foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})
            {Rod(p,new Vector3(x*.54f,.15f,z*.30f),new Vector3(x*.54f,1.06f,z*.30f),.045f,Black);var wheel=C(p,x*.54f,.1f,z*.30f,.18f,.06f,.18f,Black);wheel.localRotation=Quaternion.Euler(90,0,0);}
            Rod(p,new Vector3(-.54f,1.06f,-.30f),new Vector3(-.54f,1.06f,.30f),.045f,Black);Rod(p,new Vector3(.54f,1.06f,-.30f),new Vector3(.54f,1.06f,.30f),.045f,Black);
        }
        void Bottle(Transform p,float x,float y,float z,int color=0)
        {string m=color%3==0?Green:color%3==1?"Amber":Red;C(p,x,y+.12f,z,.10f,.24f,.10f,m);C(p,x,y+.28f,z,.045f,.09f,.045f,m);C(p,x,y+.33f,z,.05f,.02f,.05f,Gold);B(p,x,y+.13f,z-.051f,.075f,.08f,.006f,Cream);}
        void PlateStack(Transform p,float x,float y,float z,int n=6)
        {for(int i=0;i<n;i++)Plate(p,x,y+i*.035f,z,.28f);}
        void Decoration(int type)
        {
            switch(type)
            {
                case 0: // Lantern post
                    B(body,-.52f,1.15f,0,.12f,2.3f,.13f,Dark);B(body,.05f,2.3f,0,1.25f,.10f,.13f,Dark);
                    Rod(body,new Vector3(-.5f,1.8f,0),new Vector3(.08f,2.28f,0),.07f,Wood);
                    for(int i=0;i<3;i++){float x=-.18f+i*.36f,h=1.65f+(i%2)*.32f;Rod(body,new Vector3(x,2.27f,0),new Vector3(x,h+.3f,0),.02f,Gold);Lantern(body,new Vector3(x,h,0),true);}break;
                case 1: // A-frame menu with sculpted hotpot illustration
                    foreach(int s in new[]{-1,1}){var f=Group("Easel side",body,new Vector3(0,0,s*.3f));f.localRotation=Quaternion.Euler(s*-12,0,0);B(f,0,.85f,0,.87f,1.25f,.08f,Wood);B(f,0,.88f,-.05f,.73f,1.08f,.02f,"Paper");foreach(int x in new[]{-1,1})B(f,x*.47f,.7f,0,.07f,1.4f,.09f,Dark);}
                    var art=Group("Menu illustration",body,new Vector3(0,1,-.49f));art.localRotation=Quaternion.Euler(-78,0,0);Pot(art,0,0,0,.45f);Tray(art,-.17f,0,-.38f,true);Tray(art,.15f,0,.36f,false);break;
                case 2: // bamboo planter
                    B(body,0,.2f,0,1.05f,.4f,.48f,"Stone");B(body,0,.41f,0,.92f,.015f,.35f,"Soil");
                    for(int i=0;i<9;i++){float x=-.42f+(i%5)*.2f,z=(i/5)*.18f-.08f,h=1.45f+(i%3)*.17f;C(body,x,.4f+h/2,z,.047f,h,.047f,Bamboo);for(int j=0;j<5;j++){C(body,x,.6f+j*.24f,z,.057f,.025f,.057f,Green);var leaf=E(body,x+.12f,.8f+j*.22f,z,.34f,.035f,.10f,Leaf);leaf.localRotation=Quaternion.Euler(0,i*75+j*30,25);}}break;
                case 3: Plant(body,Vector3.zero,1.4f,Cream);break;
                case 4: // bonsai
                    B(body,0,.2f,0,1.05f,.10f,.7f,Dark);Legs(body,1,.65f,.18f,Dark);E(body,0,.34f,0,.75f,.25f,.5f,"Stone");
                    Rod(body,new Vector3(0,.38f,0),new Vector3(-.20f,.72f,0),.13f,Dark);Rod(body,new Vector3(-.20f,.72f,0),new Vector3(.05f,1.03f,.04f),.10f,Dark);
                    for(int i=0;i<7;i++){Vector3 end=new Vector3(Mathf.Sin(i*2.4f)*.48f,1.04f+(i%3)*.13f,Mathf.Cos(i*2.4f)*.2f);Rod(body,new Vector3(-.05f,.85f,0),end,.06f,Dark);E(body,end.x,end.y,end.z,.47f,.22f,.35f,Leaf);for(int k=0;k<4;k++)E(body,end.x+Mathf.Cos(k)*.12f,end.y+.06f,end.z+Mathf.Sin(k)*.12f,.22f,.16f,.22f,"LightLeaf");}break;
                case 5:
                    B(body,0,.23f,0,1.65f,.46f,.50f,Wood);for(int i=0;i<3;i++)B(body,0,.09f+i*.15f,-.255f,1.67f,.025f,.025f,Dark);for(int i=0;i<4;i++)Plant(body,new Vector3(-.60f+i*.40f,.18f,0),.65f,Wood,true);break;
                case 6: // four panel folding screen
                    for(int i=0;i<4;i++){var s=Group("Screen panel",body,new Vector3((i-1.5f)*.53f,0,i%2*.14f));s.localRotation=Quaternion.Euler(0,i%2==0?-16:16,0);B(s,0,1,0,.5f,1.7f,.05f,"Paper");foreach(int x in new[]{-1,1})B(s,x*.25f,.98f,0,.055f,1.96f,.08f,Dark);foreach(float y in new[]{.2f,1.6f,1.9f})B(s,0,y,0,.51f,.06f,.07f,Dark);for(int j=0;j<4;j++)B(s,-.18f+j*.12f,1.75f,-.03f,.025f,.25f,.05f,Wood);for(int j=0;j<4;j++){var mountain=B(s,-.13f+j*.08f,.60f+j*.09f,-.035f,.18f,.6f,.012f,"Stone");mountain.localRotation=Quaternion.Euler(0,0,j*13-20);}}break;
                case 7:
                    B(body,0,.06f,0,1.8f,.12f,.3f,"Stone");foreach(float y in new[]{.18f,1.9f})B(body,0,y,0,1.75f,.11f,.11f,Dark);for(int i=0;i<16;i++)B(body,-.80f+i*.107f,1.02f,0,.045f,1.73f,.055f,Wood);break;
                case 8: // stepped rock waterfall, pool and ripples
                    E(body,0,.12f,0,1.7f,.24f,1.4f,"Stone");E(body,0,.24f,-.2f,1.24f,.06f,.92f,"Water");
                    for(int i=0;i<13;i++){float a=i*Mathf.PI*2/13;E(body,Mathf.Sin(a)*.73f,.27f,Mathf.Cos(a)*.58f,.35f,.32f,.32f,"Stone");}
                    for(int j=0;j<3;j++){E(body,0,.48f+j*.34f,.30f+j*.12f,1.02f-j*.20f,.55f,.61f,"Stone");B(body,0,.47f+j*.34f,.02f+j*.12f,.30f-j*.055f,.41f,.03f,"Water");for(int k=0;k<3;k++)E(body,-.18f+k*.17f,.28f+j*.34f,-.15f+j*.12f,.22f,.045f,.17f,Cream);}
                    Plant(body,new Vector3(-.58f,.32f,.36f),.5f,"Stone");Plant(body,new Vector3(.56f,.2f,.2f),.5f,"Stone");break;
                case 9: // open glass aquarium: tinted back, transparent front and fish
                    Cabinet(body,1.55f,.70f,.63f);B(body,0,.79f,0,1.58f,.12f,.68f,Black);B(body,0,1.5f,0,1.58f,.08f,.68f,Black);B(body,0,.87f,0,1.47f,.10f,.57f,"Sand");B(body,0,1.17f,.30f,1.5f,.63f,.018f,"Water");
                    foreach(int s in new[]{-1,1})B(body,s*.76f,1.15f,0,.03f,.66f,.64f,Black);
                    for(int i=0;i<5;i++){float x=-.55f+i*.27f;E(body,x,.91f,.08f,.2f,.12f,.18f,"Stone");Rod(body,new Vector3(x,.89f,.18f),new Vector3(x+.08f,1.32f,.18f),.045f,Leaf);var fish=E(body,x,1.06f+i%2*.2f,-.10f,.20f,.09f,.08f,"Amber");E(body,x-.12f,fish.localPosition.y,-.1f,.10f,.14f,.025f,"Amber");E(body,x+.06f,fish.localPosition.y+.02f,-.145f,.017f,.017f,.012f,Black);}B(body,0,1.17f,-.31f,1.5f,.63f,.008f,"Glass");break;
                case 10:
                    B(body,0,.07f,0,.48f,.14f,.48f,Dark);B(body,0,1,0,.32f,1.65f,.32f,"Glow");foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})B(body,x*.19f,1,z*.19f,.045f,1.83f,.045f,Wood);foreach(float y in new[]{.20f,1.80f})B(body,0,y,0,.43f,.055f,.43f,Wood);break;
                case 11:
                    B(body,0,-.035f,0,1.9f,.07f,.12f,Wood);for(int i=0;i<3;i++){float x=(i-1)*.68f;Rod(body,new Vector3(x,0,0),new Vector3(x,-.55f-(i%2)*.15f,0),.018f,Dark);Lantern(body,new Vector3(x,-.9f-(i%2)*.15f,0),false);}break;
                case 12: // clock root at wall contact, outward facing -Z
                    var clock=Group("Clock face",body,new Vector3(0,0,-.10f));clock.localRotation=Quaternion.Euler(-90,0,0);C(clock,0,0,0,1.05f,.13f,1.05f,Dark);C(clock,0,.075f,0,.91f,.015f,.91f,"Paper");R(clock,0,.08f,0,1,Gold);
                    for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var mark=B(clock,Mathf.Sin(a)*.37f,.09f,Mathf.Cos(a)*.37f,.025f,.015f,.07f,Dark);mark.localRotation=Quaternion.Euler(0,i*30,0);}
                    Rod(clock,new Vector3(0,.11f,0),new Vector3(-.23f,.11f,-.12f),.03f,Black);Rod(clock,new Vector3(0,.115f,0),new Vector3(.19f,.115f,.29f),.02f,Black);break;
                case 13:
                    B(body,0,0,-.05f,.96f,1.12f,.1f,Wood);B(body,0,0,-.11f,.84f,1,.025f,"Poster");var picture=Group("Hotpot relief",body,new Vector3(0,-.05f,-.13f));picture.localRotation=Quaternion.Euler(-90,0,0);Pot(picture,0,0,0,.57f);Tray(picture,0,0,.30f,false);for(int i=0;i<3;i++)Rod(body,new Vector3((i-1)*.13f,.20f,-.16f),new Vector3((i-1)*.13f+.04f,.40f,-.16f),.018f,Cream);break;
                case 14:
                    foreach(float y in new[]{-.4f,.35f})B(body,0,y,-.22f,1.5f,.075f,.45f,Wood);foreach(int s in new[]{-1,1})B(body,s*.72f,0,-.22f,.06f,.94f,.43f,Dark);
                    for(int i=0;i<4;i++){C(body,-.5f+i*.32f,.5f,-.24f,.22f,.24f,.22f,i%2==0?Green:Cream);Bowl(body,-.5f+i*.32f,-.35f,-.24f,.24f);}Plant(body,new Vector3(.53f,.35f,-.2f),.4f,Dark);break;
                case 15:
                    Rod(body,new Vector3(-.85f,-.06f,0),new Vector3(.85f,-.06f,0),.065f,Bamboo);for(int i=0;i<3;i++){float x=(i-1)*.62f;foreach(int s in new[]{-1,1})Rod(body,new Vector3(x,-.05f,0),new Vector3(x+s*.17f,-.70f,0),.018f,Gold);Plant(body,new Vector3(x,-1.02f,0),.30f,Bamboo);for(int j=0;j<4;j++)E(body,x+Mathf.Sin(j)*.15f,-1.05f-j*.10f,.12f,.16f,.10f,.1f,Leaf);}break;
                case 16:
                    Cabinet(body,.65f,.46f,.55f);B(body,0,.5f,0,.67f,.075f,.58f,Red);E(body,0,.81f,0,.46f,.58f,.36f,Cream);E(body,0,1.16f,-.02f,.48f,.40f,.36f,Cream);
                    foreach(int s in new[]{-1,1}){var ear=E(body,s*.16f,1.36f,0,.16f,.24f,.10f,Cream);ear.localRotation=Quaternion.Euler(0,0,s*-20);E(body,s*.16f,1.37f,-.055f,.07f,.14f,.015f,Red);E(body,s*.105f,1.17f,-.20f,.065f,.025f,.01f,Black);E(body,s*.17f,.61f,-.13f,.17f,.12f,.14f,Cream);}E(body,0,1.08f,-.215f,.05f,.035f,.02f,Red);C(body,0,.98f,0,.35f,.035f,.34f,Red);E(body,.3f,1.03f,0,.14f,.37f,.16f,Cream);E(body,.32f,1.22f,0,.16f,.18f,.14f,Cream);E(body,0,.78f,-.2f,.21f,.27f,.03f,Gold);break;
                case 17:
                    foreach(int s in new[]{-1,1}){B(body,s*.95f,.1f,0,.3f,.2f,.36f,"Stone");B(body,s*.95f,1,0,.075f,2,.075f,Dark);}Rod(body,new Vector3(-1,1.87f,0),new Vector3(1,1.87f,0),.07f,Wood);
                    foreach(int s in new[]{-1,1}){B(body,s*.45f,1.13f,0,.84f,1.38f,.035f,Red);for(int j=0;j<8;j++)C(body,s*.45f-.35f+j*.10f,1.13f,-.012f,.04f,1.35f,.04f,"RedShade");}break;
                case 18:
                    Cabinet(body,1.9f,.97f,.65f);for(int i=0;i<22;i++)B(body,-.86f+i*.082f,.55f,-.36f,.03f,.78f,.045f,Gold);B(body,.35f,1.05f,0,.38f,.10f,.3f,Black);B(body,.35f,1.17f,.08f,.08f,.25f,.08f,Black);var screen=B(body,.35f,1.37f,.07f,.45f,.32f,.05f,Black);screen.localRotation=Quaternion.Euler(12,0,0);B(body,.35f,1.37f,.035f,.38f,.25f,.012f,"Screen");Plant(body,new Vector3(-.65f,1.02f,0),.25f,Cream);Lantern(body,new Vector3(.75f,1.4f,0),false);break;
                case 19:
                    B(body,0,.5f,0,1.95f,.12f,.62f,Wood);Legs(body,1.9f,.6f,.47f);B(body,0,.61f,0,1.81f,.15f,.58f,Red);B(body,0,1.15f,.29f,1.96f,.09f,.08f,Wood);for(int i=0;i<16;i++)B(body,-.88f+i*.118f,.9f,.29f,.04f,.47f,.04f,Wood);foreach(int s in new[]{-1,1}){B(body,s*.93f,.77f,0,.09f,.08f,.72f,Wood);B(body,s*.93f,.56f,-.25f,.065f,.42f,.065f,Wood);E(body,s*.65f,.84f,.15f,.40f,.38f,.16f,Red);}break;
                case 20:
                    Cart(body);Tray(body,.23f,.84f,0,false);PlateStack(body,-.3f,.85f,0);Tray(body,0,.27f,0,true);break;
                case 21:
                    Cabinet(body,1.7f,.9f,.62f,Wood,true);for(int i=0;i<5;i++){Bowl(body,-.64f+i*.32f,.95f,0,.27f,i%3==0?Red:i%3==1?Leaf:"Broth");PlateStack(body,-.62f+i*.31f,.49f,-.06f);}B(body,0,1.09f,.30f,1.75f,.23f,.06f,Wood);break;
                case 22:
                    Cabinet(body,1.9f,.67f,.9f,Steel);for(int i=0;i<6;i++){float x=-.64f+(i%3)*.63f,z=i/3*.36f-.2f;B(body,x,.74f,z,.55f,.06f,.30f,Black);Tray(body,x,.79f,z,i%2==0);}B(body,0,1.17f,.43f,1.94f,.05f,.06f,Steel);B(body,0,.97f,.44f,1.9f,.4f,.015f,"Glass");var glass=B(body,0,1.08f,-.18f,1.9f,.025f,.65f,"Glass");glass.localRotation=Quaternion.Euler(22,0,0);for(int i=0;i<7;i++)B(body,.58f,.23f+i*.035f,-.465f,.48f,.015f,.01f,Black);break;
                case 23:
                    B(body,0,1,0,1.2f,2,.58f,Black);B(body,0,1,-.305f,1.04f,1.79f,.03f,"Screen");for(int j=0;j<4;j++){B(body,0,.22f+j*.43f,-.41f,1.06f,.035f,.28f,Steel);for(int i=0;i<6;i++)Bottle(body,-.43f+i*.17f,.24f+j*.43f,-.43f,i+j);}foreach(int s in new[]{-1,1}){B(body,s*.29f,1,-.58f,.53f,1.77f,.008f,"Glass");B(body,s*.035f,1,-.61f,.025f,.45f,.025f,Steel);}B(body,0,1,-.59f,.025f,1.85f,.025f,Steel);break;
                case 24:
                    Cabinet(body,1.55f,1.35f,.50f,Wood,true);B(body,0,.80f,0,1.5f,.04f,.48f,Wood);for(int j=0;j<3;j++)for(int i=0;i<3;i++)PlateStack(body,-.51f+i*.40f,.13f+j*.39f,-.06f);for(int i=0;i<3;i++){C(body,.56f,.97f,-.08f,.16f,.25f,.16f,Bamboo);for(int k=0;k<6;k++)Rod(body,new Vector3(.51f+k*.017f,.98f,-.08f),new Vector3(.51f+k*.017f,1.31f,-.08f),.011f,Dark);}break;
                case 25:
                    B(body,0,.06f,0,1.4f,.12f,.8f,Wood);for(int i=0;i<4;i++){float x=i%2*.66f-.33f,z=i/2*.40f-.20f;int count=i==0?5:i==1?3:2;for(int j=0;j<count;j++){C(body,x,.20f+j*.17f,z,.56f,.17f,.56f,Bamboo);R(body,x,.27f+j*.17f,z,.57f,Wood);}C(body,x,.15f+count*.17f,z,.51f,.04f,.51f,Bamboo);for(int k=0;k<5;k++)B(body,x-.18f+k*.09f,.18f+count*.17f,z,.02f,.015f,.43f,Wood);}break;
                case 26:
                    Cabinet(body,1.1f,.68f,.58f);C(body,0,1.0f,0,.60f,.54f,.60f,Steel);E(body,0,1.29f,0,.65f,.15f,.65f,Black);B(body,0,1.38f,0,.28f,.07f,.07f,Black);B(body,0,.93f,-.30f,.24f,.17f,.035f,Black);E(body,0,.94f,-.325f,.06f,.06f,.012f,Red);Bowl(body,-.40f,.72f,-.12f);break;
                case 27:
                    Cart(body,Bamboo);E(body,.15f,1.04f,0,.43f,.40f,.4f,Black);C(body,.15f,1.24f,0,.28f,.045f,.28f,Black);var handle=Shape("Kettle handle",body,new Vector3(.15f,1.36f,0),new Vector3(.38f,.38f,.38f),Dark,ring);handle.localRotation=Quaternion.Euler(90,0,0);Rod(body,new Vector3(.3f,1.04f,0),new Vector3(.51f,1.18f,0),.07f,Black);Bowl(body,-.30f,.84f,-.10f);Bowl(body,-.30f,.84f,.15f);for(int i=0;i<3;i++)C(body,-.32f+i*.32f,.42f,0,.20f,.30f,.20f,Cream);B(body,-.56f,.63f,-.14f,.02f,.50f,.27f,"Upholstery");break;
                case 28:
                    foreach(int s in new[]{-1,1})Rod(body,new Vector3(s*.28f,.05f,-.20f),new Vector3(s*.28f,.89f,-.20f),.035f,Gold);Rod(body,new Vector3(0,.05f,.3f),new Vector3(0,.85f,.3f),.035f,Gold);E(body,0,.92f,0,.72f,.63f,.65f,Steel);C(body,0,1.15f,0,.60f,.03f,.55f,"Ice");R(body,0,1.17f,0,.69f,Steel);for(int i=0;i<6;i++)Bottle(body,Mathf.Sin(i*2.4f)*.22f,1.13f,Mathf.Cos(i*2.4f)*.20f,i);break;
                case 29:
                    C(body,0,.05f,0,.60f,.10f,.60f,Black);C(body,0,.65f,0,.06f,1.2f,.06f,Steel);var fan=Group("Fan cage",body,new Vector3(0,1.46f,0));fan.localRotation=Quaternion.Euler(90,0,0);for(int j=1;j<=6;j++)R(fan,0,0,0,j*.125f,Black);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Rod(fan,Vector3.zero,new Vector3(Mathf.Sin(a)*.38f,0,Mathf.Cos(a)*.38f),.012f,Steel);}for(int i=0;i<3;i++){var blade=E(fan,Mathf.Sin(i*2.094f)*.15f,-.025f,Mathf.Cos(i*2.094f)*.15f,.20f,.03f,.32f,Black);blade.localRotation=Quaternion.Euler(0,i*120,0);}E(fan,0,.035f,0,.15f,.07f,.15f,Gold);break;
                case 30:
                    C(body,0,.90f,0,.08f,1.8f,.08f,Dark);E(body,0,1.85f,0,.16f,.16f,.16f,Wood);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Rod(body,new Vector3(0,.30f,0),new Vector3(Mathf.Sin(a)*.34f,.04f,Mathf.Cos(a)*.34f),.06f,Dark);Vector3 hook=new Vector3(Mathf.Sin(a)*.27f,1.64f,Mathf.Cos(a)*.27f);Rod(body,new Vector3(0,1.45f,0),hook,.045f,Wood);E(body,hook.x,hook.y+.02f,hook.z,.08f,.08f,.08f,Wood);}break;
                case 31:
                    C(body,0,.06f,0,.52f,.12f,.52f,Gold);R(body,0,.65f,0,.52f,Gold);for(int i=0;i<18;i++){float a=i*Mathf.PI/9;Rod(body,new Vector3(Mathf.Sin(a)*.23f,.1f,Mathf.Cos(a)*.23f),new Vector3(Mathf.Sin(a+.6f)*.23f,.66f,Mathf.Cos(a+.6f)*.23f),.021f,Gold);}
                    for(int i=0;i<4;i++){float x=(i%2)*.2f-.1f,z=i/2*.18f-.09f;Rod(body,new Vector3(x,.16f,z),new Vector3(x+.08f,1.14f,z),.022f,Dark);E(body,x,.63f,z,.14f,.63f,.14f,i%2==0?Red:"Navy");var h=Shape("Umbrella hook",body,new Vector3(x+.09f,1.18f,z),new Vector3(.12f,.12f,.12f),Dark,ring);h.localRotation=Quaternion.Euler(90,0,0);}break;
                case 32:
                    for(int i=0;i<2;i++){float x=i*.61f-.305f;string m=i==0?Black:"Upholstery";B(body,x,.4f,0,.52f,.8f,.50f,m);B(body,x,.84f,0,.58f,.09f,.55f,m);B(body,x,.075f,-.30f,.20f,.055f,.10f,Steel);for(int j=0;j<3;j++){var arrow=B(body,x+Mathf.Sin(j*2.094f)*.09f,.47f+Mathf.Cos(j*2.094f)*.09f,-.26f,.15f,.028f,.012f,Cream);arrow.localRotation=Quaternion.Euler(0,0,j*120);}}break;
                case 33:
                    B(body,0,.07f,0,.50f,.14f,.43f,Red);B(body,0,.46f,.17f,.08f,.85f,.08f,Red);E(body,0,.61f,0,.34f,.83f,.32f,Red);C(body,0,1.06f,0,.085f,.15f,.085f,Gold);B(body,0,1.16f,0,.24f,.06f,.06f,Black);B(body,0,.65f,-.163f,.21f,.25f,.015f,Cream);Rod(body,new Vector3(.11f,1.1f,0),new Vector3(.25f,.81f,0),.035f,Black);Rod(body,new Vector3(.25f,.81f,0),new Vector3(.21f,.38f,0),.045f,Black);break;
                case 34:
                    B(body,0,.92f,0,1.9f,.09f,.82f,Steel);Legs(body,1.88f,.8f,.91f,Steel);B(body,0,.23f,0,1.75f,.05f,.71f,Steel);for(int i=0;i<3;i++){Tray(body,-.62f+i*.60f,.99f,-.1f,i==2);C(body,-.6f+i*.6f,1.1f,.22f,.32f,.27f,.30f,Steel);PlateStack(body,-.60f+i*.60f,.27f,-.1f);}break;
                case 35:
                    // Ceiling pivot. Tapered hood and elbow exhaust pipe point down.
                    Rod(body,new Vector3(-.48f,0,0),new Vector3(-.48f,-.55f,0),.16f,Steel);Rod(body,new Vector3(-.48f,-.55f,0),new Vector3(0,-.55f,0),.16f,Steel);Rod(body,new Vector3(0,-.55f,0),new Vector3(0,-.88f,0),.16f,Steel);
                    for(int i=0;i<7;i++)C(body,0,-.90f-i*.05f,0,.22f+i*.075f,.06f,.22f+i*.075f,Steel);C(body,0,-1.26f,0,.68f,.035f,.68f,Black);R(body,0,-1.28f,0,.72f,Gold);break;
            }
        }
    }
}
