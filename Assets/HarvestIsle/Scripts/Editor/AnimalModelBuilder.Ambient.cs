using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class AnimalModelBuilder
    {
        internal static readonly string[] AmbientNames={"Bee","Ladybug","Planthopper","Snail","FruitBorer","FieldMouse","Locust","Myna","GoldenFirefly"};
        internal static GameObject CreateAmbient(string type,Material[] materials,List<Mesh> meshes)
        {
            var root=new GameObject(type);var kit=new Kit(materials,meshes);var m=kit.Part("Connected body and details",root.transform);
            var wings=new List<Transform>();
            if(type=="FieldMouse")
            {
                m.Ellipsoid(new Vector3(0,.092f,-.025f),new Vector3(.105f,.088f,.16f),Brown,sides:28,rows:18);
                m.Ellipsoid(new Vector3(0,.108f,.113f),new Vector3(.089f,.083f,.086f),Brown,sides:28,rows:18);
                m.Ellipsoid(new Vector3(0,.075f,.183f),new Vector3(.044f,.031f,.055f),Cream,sides:24,rows:14);
                m.Ellipsoid(new Vector3(0,.086f,.229f),Vector3.one*.013f,PinkDark,sides:16,rows:10);
                for(int side=-1;side<=1;side+=2)
                {
                    m.Ellipsoid(new Vector3(side*.072f,.18f,.094f),new Vector3(.048f,.054f,.025f),Brown,sides:24,rows:16);
                    m.Ellipsoid(new Vector3(side*.072f,.182f,.116f),new Vector3(.032f,.038f,.007f),SoftPink,sides:20,rows:12);
                    AmbientEye(m,new Vector3(side*.055f,.131f,.177f),.013f);
                    for(int leg=0;leg<2;leg++)
                    {float z=leg==0?.086f:-.10f;m.Ellipsoid(new Vector3(side*.070f,.040f,z),new Vector3(.035f,.045f,.039f),Brown,sides:20,rows:12);m.Ellipsoid(new Vector3(side*.073f,.017f,z+.022f),new Vector3(.032f,.017f,.043f),SoftPink,sides:20,rows:12);}
                    for(int j=0;j<2;j++)m.Tube(new[]{new Vector3(side*.029f,.078f,.207f),new Vector3(side*.099f,.073f+j*.016f,.22f)},.002f,.0015f,SoftHoof,6);
                }
                var tail=new Vector3[25];for(int i=0;i<tail.Length;i++){float t=i/24f;tail[i]=new Vector3(Mathf.Sin(t*3)*.10f,.060f*(1-t)+.014f,-.16f-t*.22f);}m.Tube(tail,.012f,.004f,SoftPink,10);
            }
            else if(type=="Snail")
            {
                m.Ellipsoid(new Vector3(0,.035f,.015f),new Vector3(.096f,.035f,.22f),Cream,sides:32,rows:18);
                m.Ellipsoid(new Vector3(0,.095f,.151f),new Vector3(.065f,.09f,.067f),Cream,sides:28,rows:18);
                m.Ellipsoid(new Vector3(0,.149f,-.039f),new Vector3(.13f,.135f,.115f),Ginger,sides:32,rows:20);
                for(int side=-1;side<=1;side+=2)
                {
                    var spiral=new Vector3[45];
                    for(int i=0;i<spiral.Length;i++){float a=i*.19f,r=.008f+i*.0022f;float y=Mathf.Sin(a)*r,z=Mathf.Cos(a)*r;float x=.13f*Mathf.Sqrt(Mathf.Max(.03f,1-y*y/(.135f*.135f)-z*z/(.115f*.115f)));spiral[i]=new Vector3(side*(x+.002f),.149f+y,-.039f+z);}m.Tube(spiral,.005f,.005f,GingerDark,8);
                    Vector3 tip=new Vector3(side*.043f,.227f,.203f);m.Tube(new[]{new Vector3(side*.027f,.146f,.172f),tip},.010f,.006f,Cream,10);AmbientEye(m,tip,.012f);
                }
            }
            else if(type=="FruitBorer")
            {
                for(int i=0;i<8;i++)
                {Vector3 p=new Vector3(Mathf.Sin(i*.55f)*.017f,.048f,-.16f+i*.041f);m.Ellipsoid(p,new Vector3(.050f,.041f,.046f),i==7?Ginger:Cream,sides:24,rows:16);if(i<7)for(int side=-1;side<=1;side+=2)m.Ellipsoid(p+new Vector3(side*.026f,-.034f,0),new Vector3(.015f,.014f,.018f),Ginger,sides:12,rows:8);}
                float x=Mathf.Sin(7*.55f)*.017f;for(int side=-1;side<=1;side+=2)AmbientEye(m,new Vector3(x+side*.024f,.065f,.162f),.0085f);
            }
            else if(type=="Myna")
            {
                m.Ellipsoid(new Vector3(0,.151f,-.019f),new Vector3(.10f,.113f,.137f),CoatSpot,sides:28,rows:18);
                m.Ellipsoid(new Vector3(0,.252f,.084f),new Vector3(.083f,.087f,.079f),Ink,sides:28,rows:18);
                m.Tube(new[]{new Vector3(0,.234f,.146f),new Vector3(0,.231f,.216f)},.026f,.005f,Orange,14);
                for(int side=-1;side<=1;side+=2)
                {
                    m.Ellipsoid(new Vector3(side*.063f,.269f,.13f),new Vector3(.025f,.021f,.009f),Orange,sides:20,rows:12);AmbientEye(m,new Vector3(side*.065f,.272f,.138f),.012f);
                    Vector3 ankle=new Vector3(side*.038f,.012f,.005f);m.Tube(new[]{new Vector3(side*.038f,.076f,-.008f),ankle},.009f,.006f,Orange,8);
                    for(int j=0;j<3;j++)m.Tube(new[]{ankle,ankle+new Vector3((j-1)*.025f,-.006f,.052f)},.005f,.003f,Orange,6);
                    AmbientWing(kit,wings,root.transform,new Vector3(side*.081f,.165f,-.025f),new Vector3(side*.018f,0,-.015f),new Vector3(.032f,.065f,.11f),CoatSpot);
                }
                for(int j=0;j<3;j++)m.Ellipsoid(new Vector3((j-1)*.02f,.115f,-.171f),new Vector3(.025f,.017f,.10f),CoatSpot,Quaternion.Euler(-12,0,0),20,12);
            }
            else
            {
                bool bee=type=="Bee",lady=type=="Ladybug",locust=type=="Locust",hopper=type=="Planthopper",glow=type=="GoldenFirefly";
                int skin=bee?Orange:lady?Comb:locust?Iris:hopper?Brown:CoatSpot;
                Vector3 size=locust?new Vector3(.045f,.048f,.16f):hopper?new Vector3(.046f,.045f,.10f):new Vector3(.095f,.067f,.124f);
                Vector3 center=new Vector3(0,.105f,-.02f);m.Ellipsoid(center,size,skin,sides:32,rows:20);
                if(bee)for(int j=0;j<3;j++)
                {float z=-.065f+j*.052f,ring=Mathf.Sqrt(1-z*z/(size.z*size.z));var band=new Vector3[37];for(int i=0;i<band.Length;i++){float a=i*Mathf.PI*2/36;band[i]=center+new Vector3(Mathf.Cos(a)*size.x*ring,Mathf.Sin(a)*size.y*ring,z);}m.Tube(band,.006f,.006f,CoatSpot,8);}
                if(lady)
                {
                    for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++)
                    {float x=side*.048f,z=-.07f+j*.049f,y=size.y*Mathf.Sqrt(1-x*x/(size.x*size.x)-z*z/(size.z*size.z));m.Ellipsoid(center+new Vector3(x,y+.001f,z),new Vector3(.014f,.004f,.016f),Ink,sides:16,rows:10);}
                    var seam=new Vector3[25];for(int j=0;j<seam.Length;j++){float z=Mathf.Lerp(-.115f,.107f,j/24f);seam[j]=center+new Vector3(0,size.y*Mathf.Sqrt(1-z*z/(size.z*size.z))+.0015f,z);}m.Tube(seam,.0025f,.0025f,Ink,6);
                }
                if(glow)m.Ellipsoid(new Vector3(0,.107f,-.123f),new Vector3(.066f,.056f,.062f),Orange,sides:28,rows:18);
                float headZ=locust?.143f:hopper?.087f:.099f;
                m.Ellipsoid(new Vector3(0,.117f,headZ),new Vector3(.056f,.053f,.055f),lady||glow?CoatSpot:skin,sides:28,rows:18);
                for(int side=-1;side<=1;side+=2)
                {
                    AmbientEye(m,new Vector3(side*.032f,.133f,headZ+.046f),.011f);
                    m.Tube(new[]{new Vector3(side*.025f,.15f,headZ+.012f),new Vector3(side*.05f,.197f,headZ+.045f),new Vector3(side*.065f,.199f,headZ+.063f)},.004f,.003f,SoftHoof,8);
                    for(int j=0;j<3;j++)
                    {
                        Vector3 p=new Vector3(side*size.x*.72f,.09f,-.073f+j*.052f);
                        Vector3 knee=p+new Vector3(side*.048f,locust&&j==0?.052f:-.033f,locust&&j==0?-.025f:.014f);
                        Vector3 foot=new Vector3(side*(size.x+.053f),.006f,p.z+.036f);
                        m.Tube(new[]{p,knee,foot},locust&&j==0?.014f:.006f,.0035f,locust?Iris:SoftHoof,8);
                    }
                    if(!lady)
                    {
                        Vector3 pivot=new Vector3(side*.042f,.149f,.004f);
                        if(locust||hopper)m.Tube(new[]{new Vector3(side*.022f,.128f,.004f),pivot},.010f,.008f,skin,10);
                        AmbientWing(kit,wings,root.transform,pivot,bee||glow?new Vector3(side*.068f,0,-.018f):new Vector3(side*.012f,0,-.065f),bee||glow?new Vector3(.09f,.009f,.056f):new Vector3(.034f,.010f,locust?.12f:.086f),bee||glow?FishSilver:locust?Iris:Feather);
                    }
                }
            }
            kit.Finish();root.AddComponent<AmbientCreatureMotion>().Configure(wings.ToArray(),type=="Bee"||type=="GoldenFirefly");return root;
        }
        private static void AmbientEye(AnimalMesh m,Vector3 p,float r)
        {m.Ellipsoid(p,new Vector3(r,r*1.12f,r*.65f),Ink,sides:18,rows:12);m.Ellipsoid(p+new Vector3(-r*.24f,r*.34f,r*.55f),Vector3.one*r*.24f,White,sides:12,rows:8);}
        private static void AmbientWing(Kit kit,List<Transform> wings,Transform root,Vector3 pivot,Vector3 center,Vector3 size,int color)
        {Transform wing=Joint(wings.Count==0?"Left wing":"Right wing",root,pivot);kit.Part("Smooth wing",wing).Ellipsoid(center,size,color,sides:24,rows:14);wings.Add(wing);}
    }
}
