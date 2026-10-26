using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    internal static partial class EnvironmentModelBuilder
    {
        internal const int Version=6;
        internal static readonly string[] Names={"GrassTile","DirtTile","WetDirtTile","SandTile","RaisedBed",
            "Shoreline","ShoreCorner","WaterTile","RockSmall","RockLarge","RockCluster","Pond","Lake",
            "Hill","Mountain","MountainRange","IslandBase","SeaSurface",
            "RedEarthTile","AlluvialTile","HumusTile","GravelTile","MeadowTile","FlowerMeadowTile","ForestFloorTile","DryGrassTile",
            "RainTree","BanyanTree","PineTree","WillowTree","FlameTree","ArecaPalm","YoungForestTree"};
        internal static readonly string[] Labels={"Đất cỏ","Đất khô","Đất ẩm","Bãi cát","Luống cao",
            "Bờ nước thẳng","Bờ nước cong","Mặt nước","Đá nhỏ","Tảng đá","Cụm đá","Ao nhỏ","Hồ",
            "Đồi cỏ","Núi","Dãy núi","Nền đảo","Mặt biển",
            "Đất đỏ bazan","Đất phù sa","Đất mùn","Đất sỏi","Thảm cỏ mọc","Đồng hoa dại","Nền rừng","Cỏ khô",
            "Cây me tây tán rộng","Cây đa rễ buông","Cây thông","Cây liễu rủ","Cây phượng","Cây cau","Cây rừng non"};
        internal static readonly string[] Groups={"Đất","Đất","Đất","Đất","Đất","Nước","Nước","Nước",
            "Đá","Đá","Đá","Ao hồ","Ao hồ","Núi","Núi","Núi","Đảo","Nước",
            "Đất","Đất","Đất","Đất","Đất","Đất","Đất","Đất",
            "Thiên nhiên","Thiên nhiên","Thiên nhiên","Thiên nhiên","Thiên nhiên","Thiên nhiên","Thiên nhiên"};
        private static readonly string[] Colors={"96B96A","A9C87D","74985D","987151","B28960","D3BA83",
            "EDD8A0","909587","B2B3A0","717B72","487F4D","96AA63","DCA2A3","675440","487D78",
            "B77552","AD8E63","594A38","EBC55B","F2E6C3","D87849","6B9859","FFFFFF"};
        private const int Grass=0,GrassLight=1,GrassDark=2,Soil=3,Clay=4,Sand=5,SandLight=6,
            Rock=7,RockLight=8,RockDark=9,Leaf=10,Reed=11,Flower=12,WetSoil=13,WaterBed=14;

        internal static Material[] CreateMaterials()
        {
            var materials=new Material[Colors.Length+2];
            for(int i=0;i<Colors.Length;i++)
            {
                bool ground=i<=6||i==13||i==15||i==16||i==17||i==22;
                var m=new Material(Shader.Find(ground?"Harvest Isle/Soft Ground":"Universal Render Pipeline/Simple Lit"));
                m.name="Environment "+Colors[i];m.SetColor("_BaseColor",FarmWorld.Hex(Colors[i]));
                m.SetColor("_SpecColor",Color.black);m.SetFloat("_Smoothness",0.03f);materials[i]=m;
            }
            var water=new Material(Shader.Find("Harvest Isle/Stylized Water"));
            water.name="Island water - animated ripples";water.SetColor("_BaseColor",FarmWorld.Hex("62B7B6"));
            water.SetColor("_FoamColor",FarmWorld.Hex("D4ECE0"));water.SetFloat("_WaveStrength",0.55f);water.SetFloat("_Speed",0.45f);
            water.SetFloat("_PreviewTime",-1);water.SetFloat("_Opacity",1);water.SetFloat("_ZWrite",1);materials[Colors.Length]=water;
            var pondWater=new Material(water){name="Clear pond water",renderQueue=3000};
            pondWater.SetOverrideTag("RenderType","Transparent");pondWater.SetFloat("_Opacity",0.43f);pondWater.SetFloat("_ZWrite",0);
            materials[Colors.Length+1]=pondWater;
            return materials;
        }

        internal static GameObject Create(string name,Material[] materials,List<Mesh> meshes)
        {
            var root=new GameObject(name);
            var solid=new EnvironmentMesh(Colors.Length);
            EnvironmentMesh water=null;
            switch(name)
            {
                case "GrassTile": Tile(solid,Grass);break;
                case "DirtTile": TilledSoil(solid,Soil);break;
                case "WetDirtTile": TilledSoil(solid,WetSoil);break;
                case "SandTile": Tile(solid,SandLight);break;
                case "RaisedBed": Bed(solid);break;
                case "Shoreline": case "ShoreCorner":
                    water=new EnvironmentMesh(1);Shore(solid,water,name=="ShoreCorner");break;
                case "WaterTile":
                    solid=null;water=new EnvironmentMesh(1);WaterPlane(water,2,2,-0.10f,8);break;
                case "SeaSurface":
                    solid=null;water=new EnvironmentMesh(1);WaterPlane(water,120,120,-0.72f,8);break;
                case "RockSmall": Boulder(solid,Vector3.zero,new Vector3(0.33f,0.31f,0.27f),2);break;
                case "RockLarge": Boulder(solid,Vector3.zero,new Vector3(0.79f,0.94f,0.64f),3);break;
                case "RockCluster":
                    Boulder(solid,new Vector3(-0.15f,0,-0.06f),new Vector3(0.59f,0.81f,0.51f),5);
                    Boulder(solid,new Vector3(0.52f,0,0.13f),new Vector3(0.34f,0.42f,0.33f),8);
                    Boulder(solid,new Vector3(-0.52f,0,0.36f),new Vector3(0.28f,0.29f,0.24f),12);break;
                case "Pond":case "Lake":
                    water=new EnvironmentMesh(1);Basin(solid,water,name=="Lake");break;
                case "Hill": SmoothMound(solid,Vector3.zero,1.8f,1.45f,.85f,3,false);break;
                case "Mountain": Peak(solid,Vector3.zero,1.85f,1.45f,2.45f,2);break;
                case "MountainRange":
                    Peak(solid,new Vector3(-1.15f,0,0.08f),1.70f,1.26f,2.05f,4);
                    Peak(solid,new Vector3(1.16f,0,-0.06f),1.62f,1.18f,2.60f,8);
                    Peak(solid,new Vector3(0.24f,0,0.82f),1.15f,0.74f,1.05f,12);break;
                case "IslandBase":
                    water=new EnvironmentMesh(1);Island(solid,water);break;
                default:
                    if(name.EndsWith("Tile"))LandscapeTile(solid,name);
                    else LandscapeTree(solid,name);
                    break;
            }
            if(solid!=null)AddPart(root,"Land and details",solid.Finish(name+" geometry",name=="IslandBase"||name=="Hill"||name.StartsWith("Mountain")||name.EndsWith("Tile")),Subset(materials),meshes,false);
            if(water!=null)AddPart(root,"Water surface",water.Finish(name+" water"),new[]{materials[Colors.Length+(name=="Pond"||name=="Lake"?1:0)]},meshes,true);
            return root;
        }
        private static Material[] Subset(Material[] materials)
        {
            var solid=new Material[Colors.Length];System.Array.Copy(materials,solid,solid.Length);return solid;
        }
        private static void AddPart(GameObject root,string name,Mesh mesh,Material[] materials,List<Mesh> meshes,bool water)
        {
            var part=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(root.transform,false);
            part.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=part.GetComponent<MeshRenderer>();renderer.sharedMaterials=materials;
            renderer.shadowCastingMode=water?ShadowCastingMode.Off:ShadowCastingMode.On;renderer.receiveShadows=!water;
            if(!water)part.AddComponent<MeshCollider>().sharedMesh=mesh;
            meshes.Add(mesh);
        }
        private static Vector3 Direction(float a){return new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));}
        private static float Shape(float a,int seed){return 1+Mathf.Sin(a*3+seed)*0.055f+Mathf.Cos(a*5+seed*0.8f)*0.035f;}
        private static Vector3[] Ring(float rx,float rz,float y,int seed,int sides=32,Vector3 offset=default(Vector3))
        {
            var ring=new Vector3[sides];for(int i=0;i<sides;i++)
            {float a=i*Mathf.PI*2/sides;float shape=Shape(a,seed);ring[i]=offset+new Vector3(Mathf.Cos(a)*rx*shape,y,Mathf.Sin(a)*rz*shape);}
            return ring;
        }
        private static void Cap(EnvironmentMesh mesh,Vector3[] ring,Vector3 center,int material)
        {for(int i=0;i<ring.Length;i++)mesh.Tri(center,ring[(i+1)%ring.Length],ring[i],material);}
        private static void WaterDisc(EnvironmentMesh mesh,Vector3[] boundary,Vector3 center)
        {
            Vector3[] inner=new Vector3[boundary.Length];
            for(int i=0;i<inner.Length;i++)inner[i]=Vector3.Lerp(center,boundary[i],0.78f);
            Color deep=new Color(0.88f,0.96f,0.97f,0),shallow=new Color(1.12f,1.12f,1.03f,0.95f);
            for(int i=0;i<boundary.Length;i++)
            {
                int next=(i+1)%boundary.Length;
                mesh.Tri(center,inner[next],inner[i],0,deep,deep,deep);
                mesh.Tri(inner[i],inner[next],boundary[next],0,deep,deep,shallow);
                mesh.Tri(inner[i],boundary[next],boundary[i],0,deep,shallow,shallow);
            }
        }
        private static void WaterPlane(EnvironmentMesh mesh,float width,float depth,float y,int divisions)
        {
            Color c=new Color(1,1,1,0);
            for(int x=0;x<divisions;x++)for(int z=0;z<divisions;z++)
            {
                float x0=-width/2+width*x/divisions,x1=-width/2+width*(x+1)/divisions;
                float z0=-depth/2+depth*z/divisions,z1=-depth/2+depth*(z+1)/divisions;
                Vector3 a=new Vector3(x0,y,z0),b=new Vector3(x0,y,z1),d=new Vector3(x1,y,z0),e=new Vector3(x1,y,z1);
                mesh.Tri(a,b,e,0,c,c,c);mesh.Tri(a,e,d,0,c,c,c);
            }
        }
        private static void Tile(EnvironmentMesh mesh,int top)
        {
            mesh.Cube(new Vector3(0,-0.14f,0),new Vector3(2,0.28f,2),top,Soil,false);
            mesh.Grid(2,2,24,(x,z)=>.012f*Mathf.Sin(x*4)*Mathf.Sin(z*3)*(1-Mathf.Abs(x))*(1-Mathf.Abs(z)),(x,z)=>top);
        }
        private static void Bed(EnvironmentMesh mesh)
        {
            mesh.Cube(new Vector3(0,0.035f,0),new Vector3(1.1f,0.22f,1.1f),Soil,Clay);
            for(int i=0;i<4;i++)
                mesh.Tube(new[]{new Vector3(-0.37f+i*0.25f,0.14f,-0.46f),new Vector3(-0.37f+i*0.25f,0.14f,0.46f)},0.074f,0.074f,Soil,8);
        }
        private static void Shore(EnvironmentMesh mesh,EnvironmentMesh water,bool corner)
        {
            System.Func<float,float,float> distance=(x,z)=>corner?Mathf.Sqrt((x+1)*(x+1)+(z+1)*(z+1)):x+1;
            System.Func<float,float,float> height=(x,z)=>Mathf.Lerp(0.04f,-0.16f,Mathf.InverseLerp(0.75f,1.65f,distance(x,z)));
            mesh.Grid(2,2,16,height,(x,z)=>distance(x,z)<0.84f?Grass:distance(x,z)<1.20f?SandLight:Sand);
            Vector3[] perimeter={new Vector3(-1,height(-1,-1),-1),new Vector3(-1,height(-1,1),1),
                new Vector3(1,height(1,1),1),new Vector3(1,height(1,-1),-1)};
            for(int i=0;i<perimeter.Length;i++)mesh.Quad(perimeter[(i+1)%4],perimeter[i],
                new Vector3(perimeter[i].x,-0.28f,perimeter[i].z),new Vector3(perimeter[(i+1)%4].x,-0.28f,perimeter[(i+1)%4].z),Soil);
            WaterPlane(water,2,2,-0.10f,8);
        }
        private static void Boulder(EnvironmentMesh mesh,Vector3 center,Vector3 size,int seed)
        {
            const int sides=7;var rings=new Vector3[3][];
            float[] heights={0,0.28f,0.73f};float[] radii={0.71f,1,0.64f};
            for(int row=0;row<3;row++)
            {
                rings[row]=new Vector3[sides];for(int i=0;i<sides;i++)
                {float a=i*Mathf.PI*2/sides+seed*0.21f;float r=radii[row]*Shape(a,seed+row);
                    rings[row][i]=center+new Vector3(Mathf.Cos(a)*size.x*r+(row==2?size.x*0.08f:0),
                        size.y*(heights[row]+(row==0?0:Mathf.Sin(a*2+seed)*0.047f)),Mathf.Sin(a)*size.z*r);}
            }
            for(int row=0;row<2;row++)for(int i=0;i<sides;i++)
                mesh.Quad(rings[row][i],rings[row+1][i],rings[row+1][(i+1)%sides],rings[row][(i+1)%sides],(i+seed+row)%4==0?RockDark:row==1?RockLight:Rock);
            Cap(mesh,rings[2],center+new Vector3(size.x*0.15f,size.y,-size.z*0.09f),RockLight);
        }
        private static void Basin(EnvironmentMesh land,EnvironmentMesh water,bool lake)
        {
            float rx=lake?2.75f:1.55f,rz=lake?1.95f:1.14f;int seed=lake?7:2;
            Vector3[] edge=Ring(rx,rz,0,seed),lip=Ring(rx*0.85f,rz*0.85f,lake?0.30f:0.28f,seed),
                waterline=Ring(rx*0.73f,rz*0.73f,lake?0.22f:0.20f,seed),bottom=Ring(rx,rz,-0.23f,seed);
            land.Ring(lip,edge,Grass);land.Ring(waterline,lip,lake?SandLight:Clay);
            land.Ring(edge,bottom,Soil);
            WaterDisc(water,waterline,new Vector3(0,waterline[0].y,0));
            Cap(land,Ring(rx*0.75f,rz*0.75f,0.012f,seed),new Vector3(0,0.012f,0),WaterBed);
            for(int i=0;i<(lake?5:4);i++)
            {
                float a=i*2.41f+0.4f;Vector3 p=new Vector3(Mathf.Cos(a)*rx*0.78f,0.23f,Mathf.Sin(a)*rz*0.78f);
                Boulder(land,p,new Vector3(0.16f+(i%2)*0.06f,0.17f,0.12f),i+3);
            }
            for(int i=0;i<(lake?5:3);i++)
            {
                float a=2.6f+i*0.20f;
                Vector3 p=new Vector3(Mathf.Cos(a)*rx*0.83f,0.25f,Mathf.Sin(a)*rz*0.83f);
                for(int reed=0;reed<4;reed++)
                {
                    Vector3 d=Direction(reed*2.4f);float h=0.28f+(reed%2)*0.11f;
                    land.Tube(new[]{p+d*0.035f,p+d*0.065f+Vector3.up*h},0.008f,0.005f,Reed);
                    land.Leaf(p+Vector3.up*0.04f,d,0.18f,0.023f,Leaf);
                    if(reed==1)land.Tube(new[]{p+d*0.065f+Vector3.up*h*0.70f,p+d*0.065f+Vector3.up*(h+0.01f)},0.019f,0.017f,WetSoil);
                }
            }
            float waterY=waterline[0].y;
            for(int i=0;i<(lake?2:3);i++)
            {
                Vector3 p=new Vector3(-rx*0.24f+i*0.17f,waterY+0.003f,rz*0.24f-i*0.13f);
                for(int sector=1;sector<13;sector++)
                {
                    float a=sector*Mathf.PI*2/14,b=(sector+1)*Mathf.PI*2/14;
                    land.Tri(p,p+Direction(b)*0.13f,p+Direction(a)*0.13f,Leaf);
                }
                if(i==1)
                    for(int petal=0;petal<6;petal++)land.Leaf(p+Vector3.up*0.005f,Direction(petal*Mathf.PI/3),0.073f,0.023f,Flower);
            }
        }
        private static void Peak(EnvironmentMesh mesh,Vector3 origin,float rx,float rz,float h,int seed)
        {
            SmoothMound(mesh,origin,rx*1.35f,rz*1.35f,h,seed,true);
        }
        private static void Island(EnvironmentMesh land,EnvironmentMesh water)
        {
            const int sides=72;int seed=4;
            Vector3[] grass=Ring(9.10f,7.70f,0.15f,seed,sides),sand=Ring(9.48f,8.02f,0.13f,seed,sides),
                beach=Ring(10.25f,8.69f,-0.30f,seed,sides),shore=Ring(10.70f,9.08f,-0.715f,seed,sides),
                rock=Ring(10.65f,9.04f,-1.04f,seed,sides);
            IslandSurface(land,grass);
            land.Ring(grass,sand,SandLight);land.Ring(sand,beach,SandLight);land.Ring(beach,shore,Sand);
            land.Ring(shore,rock,Clay);
            var foam=Ring(11.30f,9.59f,-0.714f,seed,sides);
            for(int i=0;i<sides;i++)
            {
                int next=(i+1)%sides;Color near=new Color(1.12f,1.10f,1.02f,1),far=new Color(1,1,1,0);
                water.Tri(shore[i],shore[next],foam[next],0,near,near,far);water.Tri(shore[i],foam[next],foam[i],0,near,far,far);
            }
        }
    }
}
