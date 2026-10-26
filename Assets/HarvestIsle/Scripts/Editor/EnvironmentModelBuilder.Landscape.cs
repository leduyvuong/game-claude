using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class EnvironmentModelBuilder
    {
        private const int RedEarth=15,Alluvium=16,Humus=17,GoldFlower=18,CreamFlower=19,Flame=20,Foliage=21;

        private static Color GroundColor(Vector3 p)
        {
            Color grass=FarmWorld.Hex("96B96A"),high=FarmWorld.Hex("74985D"),stone=FarmWorld.Hex("909587");
            Color c=Color.Lerp(grass,high,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,2.1f,p.y)));
            c=Color.Lerp(c,stone,Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.0f,3.6f,p.y)));
            float front=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-4.4f,-6.3f,p.z))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.8f,4.3f,Mathf.Abs(p.x))));
            c=Color.Lerp(c,FarmWorld.Hex("AD8E63"),front);
            float rear=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.4f,3.4f,p.z));
            c=Color.Lerp(c,FarmWorld.Hex("796949"),rear*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-6.7f,-8.3f,p.x)));
            c=Color.Lerp(c,FarmWorld.Hex("B77552"),rear*Mathf.SmoothStep(0,1,Mathf.InverseLerp(6.7f,8.4f,p.x)));
            return QualitySettings.activeColorSpace==ColorSpace.Linear?c.linear:c;
        }
        private static void GroundTriangle(EnvironmentMesh mesh,Vector3 a,Vector3 b,Vector3 c)
        {mesh.Tri(a,b,c,22,GroundColor(a),GroundColor(b),GroundColor(c));}

        private static void IslandSurface(EnvironmentMesh mesh,Vector3[] boundary)
        {
            const int rings=72;
            Vector3[] previous=null;
            for(int ring=1;ring<=rings;ring++)
            {
                float t=(float)ring/rings;var current=new Vector3[boundary.Length];
                for(int i=0;i<current.Length;i++)
                {
                    Vector3 p=boundary[i]*t;
                    // The beach stays low; inland ridges descend continuously into the coast.
                    p.y=IslandTerrainProfile.Height(p.x,p.z);current[i]=p;
                }
                for(int i=0;i<current.Length;i++)
                {
                    int n=(i+1)%current.Length;
                    if(previous==null)GroundTriangle(mesh,new Vector3(0,.15f,0),current[n],current[i]);
                    else {GroundTriangle(mesh,previous[i],previous[n],current[n]);GroundTriangle(mesh,previous[i],current[n],current[i]);}
                }
                previous=current;
            }
            // Dense short cover on the outer meadows, leaving paths, buildings and ponds clear.
            for(int i=0;i<360;i++)
            {
                float a=i*2.399963f,r=.81f+.07f*Mathf.Sin(i*7.1f);
                float x=Mathf.Cos(a)*9.1f*r,z=Mathf.Sin(a)*7.7f*r;
                if(z>4.7f||Mathf.Abs(z)<2.2f|| (z< -4.6f&&Mathf.Abs(x)<4))continue;
                Vector3 p=new Vector3(x,.15f,z);GrassTuft(mesh,p,i,.11f+(i%4)*.02f);
                if(i%4==0)WildFlower(mesh,p+new Vector3(.055f,0,.035f),i,.21f);
            }
        }

        private static void SmoothMound(EnvironmentMesh mesh,Vector3 origin,float rx,float rz,float height,int seed,bool rocky)
        {
            const int rings=24,sides=64;
            Vector3[] previous=null;
            for(int row=0;row<=rings;row++)
            {
                float t=(float)row/rings;
                float radius=Mathf.Max(.001f,1-t);
                float y=height*Mathf.SmoothStep(0,1,t);
                var current=new Vector3[sides];
                for(int i=0;i<sides;i++)
                {
                    float a=i*Mathf.PI*2/sides;
                    float shape=1+.06f*Mathf.Sin(a*3+seed)+.035f*Mathf.Cos(a*5+seed);
                    current[i]=origin+new Vector3(Mathf.Cos(a)*rx*radius*shape+.13f*rx*t,
                        y+.045f*height*Mathf.Sin(a*3+seed)*Mathf.Sin(t*Mathf.PI),Mathf.Sin(a)*rz*radius*shape);
                }
                if(previous!=null)mesh.Ring(current,previous,rocky&&t>.62f?Rock:t>.33f?GrassDark:Grass);
                previous=current;
            }
            Cap(mesh,previous,origin+new Vector3(.13f*rx,height,0),rocky?Rock:GrassDark);
        }

        private static void LandscapeTile(EnvironmentMesh mesh,string name)
        {
            int top=name=="RedEarthTile"?RedEarth:name=="AlluvialTile"?Alluvium:name=="HumusTile"||name=="ForestFloorTile"?Humus:name=="GravelTile"?Rock:name=="DryGrassTile"?Sand:Grass;
            Tile(mesh,top);
            bool grass=name=="MeadowTile"||name=="FlowerMeadowTile"||name=="ForestFloorTile"||name=="DryGrassTile";
            for(int i=0;i< (grass?85:24);i++)
            {
                Vector3 p=new Vector3(Mathf.Sin(i*12.43f)*.91f,.006f,Mathf.Cos(i*7.79f)*.91f);
                if(grass)GrassTuft(mesh,p,i,name=="DryGrassTile"?.12f:.17f,name=="DryGrassTile"?Reed:Leaf);
                if(name=="FlowerMeadowTile"&&i%3==0)WildFlower(mesh,p,i/3,.22f+(i%5)*.025f);
                if(name=="ForestFloorTile"&&i%4==0)mesh.Leaf(p,Direction(i*2.4f),.13f,.035f,Clay);
                if(name=="GravelTile")Boulder(mesh,p,new Vector3(.035f+i%3*.012f,.025f,.04f),i);
            }
        }
        private static void GrassTuft(EnvironmentMesh mesh,Vector3 p,int seed,float h,int color=Leaf)
        {
            for(int j=0;j<4;j++)
            {
                Vector3 d=Direction(seed+j*2.4f),side=Vector3.Cross(d,Vector3.up)*.015f;
                Vector3 tip=p+d*h*.5f+Vector3.up*h*(.7f+j*.12f);
                mesh.Tri(p-side,p+side,tip,color);mesh.Tri(p+side,p-side,tip,color);
            }
        }
        private static void WildFlower(EnvironmentMesh mesh,Vector3 p,int seed,float h)
        {
            Vector3 tip=p+Vector3.up*h;
            mesh.Tube(new[]{p,tip},.009f,.006f,Leaf,5);
            for(int j=0;j<5;j++)mesh.Leaf(tip,Direction(j*Mathf.PI*2/5+seed),.066f,.026f,seed%3==0?GoldFlower:seed%3==1?CreamFlower:Flower);
            mesh.Disc(tip+Vector3.up*.016f,.024f,.024f,GoldFlower,10);
        }
        private static void Crown(EnvironmentMesh mesh,Vector3 center,Vector3 size,int material,int seed)
        {
            const int sides=16,rows=10;Vector3[] previous=null;
            for(int row=0;row<=rows;row++)
            {
                float a=Mathf.PI*row/rows;var current=new Vector3[sides];
                for(int j=0;j<sides;j++)
                {float b=j*Mathf.PI*2/sides;float r=Mathf.Sin(a)*(1+.055f*Mathf.Sin(b*5+seed));current[j]=center+Vector3.Scale(size,new Vector3(Mathf.Cos(b)*r,Mathf.Cos(a),Mathf.Sin(b)*r));}
                if(previous!=null)mesh.Ring(previous,current,material);previous=current;
            }
        }
        private static void LandscapeTree(EnvironmentMesh mesh,string name)
        {
            bool young=name=="YoungForestTree",pine=name=="PineTree",areca=name=="ArecaPalm",banyan=name=="BanyanTree",willow=name=="WillowTree",flame=name=="FlameTree";
            float h=young?1.5f:areca?3.4f:pine?3.1f:2.8f;
            mesh.Tube(new[]{Vector3.zero,new Vector3(.08f,h*.44f,0),new Vector3(.12f,h*.83f,.04f)},young?.065f:areca?.075f:.17f,.035f,Soil,12);
            if(areca)
            {
                for(int j=0;j<9;j++)
                {
                    Vector3 d=Direction(j*2.4f),top=new Vector3(.12f,h*.83f,.04f);
                    mesh.Tube(new[]{top,top+d*.5f+Vector3.up*.25f,top+d*1.15f-Vector3.up*.2f},.018f,.006f,Leaf,6);
                    for(int k=1;k<9;k++)for(int side=-1;side<=1;side+=2)
                    {float t=k/9f;Vector3 p=top+d*t*1.15f+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.25f-t*.2f);mesh.Leaf(p,(d*.35f+Vector3.Cross(d,Vector3.up)*side).normalized,.38f*(1-t*.65f),.045f,Foliage);}
                }
                return;
            }
            if(pine)
            {
                for(int j=0;j<5;j++)
                {float y=.65f+j*.49f,r=1.03f-j*.16f;Vector3[] edge=Ring(r,r,y,3+j,24),tip=Ring(.015f,.015f,y+1.05f,3,24);mesh.Ring(tip,edge,j%2==0?Leaf:GrassDark);Cap(mesh,edge,new Vector3(0,y,0),Leaf);}
                return;
            }
            for(int j=0;j<(young?4:7);j++)
            {
                Vector3 d=Direction(j*2.39996f);float spread=young?.4f:willow?.95f:1.08f;
                Vector3 end=d*spread+Vector3.up*(h*.71f+(j%3)*.17f);
                mesh.Tube(new[]{new Vector3(.08f,h*.42f,0),end},young?.035f:.072f,.025f,Soil,8);
                Crown(mesh,end+Vector3.up*.2f,new Vector3(young?.43f:.86f,willow?.48f:young?.4f:.55f,young?.43f:.78f),flame?(j%3==0?Leaf:Flame):j%3==0?GrassDark:Foliage,j);
                if(banyan)mesh.Tube(new[]{end,end*.98f-new Vector3(0,end.y-.06f,0)},.017f,.012f,Soil,6);
                if(willow)for(int k=0;k<5;k++)
                {Vector3 p=end+Direction(k*2.4f)*.55f;mesh.Tube(new[]{p,p+d*.18f-Vector3.up*.8f,p+d*.2f-Vector3.up*1.3f},.012f,.005f,Leaf,5);for(int t=0;t<7;t++)mesh.Leaf(p+Vector3.down*t*.17f+d*.15f,Direction(k+t*2.4f),.2f,.045f,Foliage);}
            }
            if(banyan)for(int j=0;j<6;j++)mesh.Tube(new[]{Direction(j)*.62f,new Vector3(.05f,.5f,0)},.045f,.09f,Soil,8);
        }
    }
}
