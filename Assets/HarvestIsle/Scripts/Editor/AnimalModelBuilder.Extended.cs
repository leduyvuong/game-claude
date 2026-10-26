using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class AnimalModelBuilder
    {
        private static void ExtendedCreature(Kit kit,FarmAnimalAvatar avatar,Transform motion,FarmAnimalKind kind)
        {
            if(kind==FarmAnimalKind.Sparrow||kind==FarmAnimalKind.Bulbul)SongBird(kit,avatar,motion,kind);
            else if(kind==FarmAnimalKind.Koi||kind==FarmAnimalKind.Carp||kind==FarmAnimalKind.Perch)Fish(kit,avatar,motion,kind);
            else Ruminant(kit,avatar,motion,kind);
        }
        private static void Ruminant(Kit kit,FarmAnimalAvatar avatar,Transform motion,FarmAnimalKind kind)
        {
            bool sheep=kind==FarmAnimalKind.Sheep,buffalo=kind==FarmAnimalKind.Buffalo;
            int skin=sheep?Brown:buffalo?BuffaloSkin:DeerSkin;
            float bodyY=sheep?0.43f:buffalo?0.65f:0.56f;
            Vector3 radius=sheep?new Vector3(0.29f,0.24f,0.43f):buffalo?new Vector3(0.36f,0.30f,0.58f):new Vector3(0.21f,0.205f,0.39f);
            Transform body=Joint("Body",motion,new Vector3(0,bodyY,0));var torso=kit.Part("Body coat",body);
            torso.Ellipsoid(Vector3.zero,radius,sheep?Wool:skin,sides:28,rows:16,patchColor:kind==FarmAnimalKind.Deer?Cream:-1,patchMode:5);
            if(sheep)
            {
                for(int row=0;row<4;row++)for(int i=0;i<14;i++)
                {
                    float a=i*Mathf.PI*2/14+row*0.29f,y=-0.155f+row*0.103f;
                    float ring=Mathf.Sqrt(1-y*y/(radius.y*radius.y));
                    Vector3 p=new Vector3(Mathf.Cos(a)*radius.x*ring*0.94f,y,Mathf.Sin(a)*radius.z*ring*0.94f);
                    torso.Ellipsoid(p*.91f,new Vector3(0.088f,0.081f,0.093f),Wool,sides:18,rows:12);
                }
                for(int i=0;i<5;i++)torso.Ellipsoid(new Vector3((i%2-0.5f)*0.13f,0.19f,-0.28f+i*0.12f),new Vector3(0.11f,0.085f,0.12f),Wool,sides:12,rows:8);
            }
            if(kind==FarmAnimalKind.Deer)
            {
                var spots=kit.Part("Sika coat spots",body,false);
                for(int side=-1;side<=1;side+=2)for(int row=0;row<2;row++)for(int i=0;i<6;i++)
                {
                    float y=0.03f+row*0.073f,z=-0.24f+i*0.079f+(row==1?0.022f:0);
                    float x=radius.x*Mathf.Sqrt(Mathf.Max(0,1-y*y/(radius.y*radius.y)-z*z/(radius.z*radius.z)));
                    spots.Ellipsoid(new Vector3(side*(x+0.002f),y,z),new Vector3(0.022f,0.017f,0.003f),Cream,
                        Quaternion.Euler(0,side*90,0),12,8);
                }
                torso.Ellipsoid(new Vector3(0,0.13f,0.26f),new Vector3(0.10f,0.205f,0.115f),skin,Quaternion.Euler(-23,0,0));
            }
            Transform head=Joint("Head pivot",body,new Vector3(0,sheep?0.08f:buffalo?0.06f:0.22f,sheep?0.34f:buffalo?0.49f:0.32f));
            var face=kit.Part("Head and muzzle",head);
            face.Ellipsoid(new Vector3(0,0.055f,0.025f),sheep?new Vector3(0.145f,0.185f,0.165f):buffalo?new Vector3(0.25f,0.255f,0.25f):new Vector3(0.13f,0.17f,0.145f),skin,sides:24,rows:14);
            Vector3 muzzle=sheep?new Vector3(0,-0.054f,0.165f):buffalo?new Vector3(0,-0.079f,0.250f):new Vector3(0,-0.045f,0.155f);
            face.Ellipsoid(muzzle,sheep?new Vector3(0.115f,0.079f,0.11f):buffalo?new Vector3(0.20f,0.11f,0.14f):new Vector3(0.10f,0.067f,0.115f),buffalo?Dark:sheep?Dark:Cream);
            var eyes=new List<Transform>();
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 ear=new Vector3(side*(buffalo?0.265f:sheep?0.165f:0.125f),buffalo?0.10f:0.14f,0.010f);
                face.Ellipsoid(ear,new Vector3(buffalo?0.13f:0.10f,sheep?0.039f:0.052f,0.050f),skin,Quaternion.Euler(0,0,side*(sheep?24:-28)));
                face.Ellipsoid(ear+new Vector3(side*0.025f,0,0.037f),new Vector3(buffalo?0.08f:0.064f,0.025f,0.009f),PinkDark,Quaternion.Euler(0,0,side*(sheep?24:-28)),12,8);
                Vector3 eye=buffalo?new Vector3(side*0.178f,0.137f,0.180f):sheep?new Vector3(side*0.103f,0.10f,0.130f):new Vector3(side*0.094f,0.108f,0.117f);
                eyes.Add(Eye(kit,head,eye,new Vector3(side*0.66f,0.03f,0.75f),buffalo?1.05f:0.75f,false));
                face.Ellipsoid(muzzle+new Vector3(side*(buffalo?0.075f:0.038f),0.01f,buffalo?0.128f:0.101f),new Vector3(0.018f,0.013f,0.006f),Ink,sides:12,rows:8);
                if(buffalo)
                {
                    var horn=new Vector3[17];
                    Vector3 p0=new Vector3(side*0.19f,0.21f,-0.045f),p1=new Vector3(side*0.59f,0.13f,-0.10f),
                        p2=new Vector3(side*0.61f,0.39f,-0.15f),p3=new Vector3(side*0.39f,0.52f,-0.16f);
                    for(int i=0;i<horn.Length;i++){float t=(float)i/(horn.Length-1),u=1-t;horn[i]=u*u*u*p0+3*u*u*t*p1+3*u*t*t*p2+t*t*t*p3;}
                    face.Tube(horn,0.046f,0.004f,Hoof,12);
                }
                else if(!sheep)
                {
                    face.Tube(new[]{new Vector3(side*0.073f,0.185f,-0.025f),new Vector3(side*0.10f,0.29f,-0.06f),
                        new Vector3(side*0.15f,0.35f,-0.11f),new Vector3(side*0.19f,0.44f,-0.17f)},0.027f,0.005f,Brown,12);
                    for(int tine=0;tine<2;tine++)
                    {
                        Vector3 p=Vector3.Lerp(new Vector3(side*.10f,.29f,-.06f),new Vector3(side*.15f,.35f,-.11f),.25f+tine*.55f);
                        face.Tube(new[]{p,p+new Vector3(side*0.037f,0.065f,0.060f),p+new Vector3(side*0.035f,0.11f,0.09f)},0.014f,0.004f,Horn,10);
                    }
                }
            }
            if(sheep)for(int i=0;i<3;i++)face.Ellipsoid(new Vector3((i-1)*0.068f,0.207f,0.015f),new Vector3(0.069f,0.066f,0.080f),Wool,sides:12,rows:8);
            Transform tail=Joint("Tail",body,new Vector3(0,0.08f,-radius.z*0.97f));var tailMesh=kit.Part("Tail",tail);
            if(buffalo){tailMesh.Tube(new[]{Vector3.zero,new Vector3(0,-0.16f,-0.05f),new Vector3(0,-0.35f,-0.07f)},0.023f,0.013f,skin);
                tailMesh.Ellipsoid(new Vector3(0,-0.365f,-0.07f),new Vector3(0.049f,0.08f,0.043f),Dark);}
            else tailMesh.Ellipsoid(new Vector3(0,-0.025f,-0.044f),new Vector3(0.06f,sheep?0.094f:0.08f,0.064f),sheep?Wool:Cream,Quaternion.Euler(30,0,0));
            var legs=new Transform[4];var contacts=new List<Transform>();
            float hip=sheep?0.29f:buffalo?0.54f:0.43f,half=sheep?0.17f:buffalo?0.22f:0.125f,front=sheep?0.25f:buffalo?0.33f:0.24f;
            for(int i=0;i<4;i++)
            {
                int side=i%2==0?-1:1;Transform leg=Joint((i<2?"Front ":"Rear ")+(side<0?"left leg":"right leg"),motion,new Vector3(side*half,hip,i<2?front:-front));legs[i]=leg;
                var limb=kit.Part("Leg and cloven hoof",leg);float r=buffalo?0.070f:sheep?0.048f:0.037f,sole=buffalo?0.048f:0.033f;
                limb.Ellipsoid(new Vector3(0,-0.02f,0),new Vector3(r*1.25f,0.105f,r*1.3f),skin);
                limb.Tube(new[]{Vector3.zero,new Vector3(0,-hip*0.5f,-0.008f),new Vector3(0,-hip+sole*1.7f,0.012f)},r,r*0.73f,skin);
                Vector3 foot=new Vector3(0,-hip+sole,0.025f),size=new Vector3(r*1.1f,sole,r*1.35f);
                limb.Shoe(foot,size,Hoof);limb.Tube(new[]{foot+new Vector3(0,-sole*0.3f,size.z),foot+new Vector3(0,sole*0.6f,size.z*0.85f)},0.0025f,0.002f,Ink,5);
                contacts.Add(Joint("Toe contact",leg,new Vector3(0,-hip,foot.z+size.z)));
                contacts.Add(Joint("Heel contact",leg,new Vector3(0,-hip,foot.z-size.z)));
            }
            avatar.Configure(kind,motion,body,head,tail,legs,new Transform[0],eyes.ToArray(),contacts.ToArray());
        }
        private static void SongBird(Kit kit,FarmAnimalAvatar avatar,Transform motion,FarmAnimalKind kind)
        {
            bool bulbul=kind==FarmAnimalKind.Bulbul;float bodyY=bulbul?0.20f:0.17f;
            Transform body=Joint("Body",motion,new Vector3(0,bodyY,0));var torso=kit.Part("Songbird plumage",body);
            torso.Ellipsoid(Vector3.zero,new Vector3(0.105f,bulbul?0.128f:0.108f,bulbul?0.19f:0.17f),FeatherBrown,sides:28,rows:16,patchColor:Cream,patchMode:5);
            if(bulbul)torso.Ellipsoid(new Vector3(0,-0.08f,-0.14f),new Vector3(0.036f,0.026f,0.046f),Comb,sides:12,rows:8);
            Transform head=Joint("Head pivot",body,new Vector3(0,bulbul?0.105f:0.090f,bulbul?0.115f:0.095f));var face=kit.Part("Head and pointed beak",head);
            face.Ellipsoid(new Vector3(0,0.055f,0),new Vector3(0.090f,0.097f,0.088f),bulbul?Ink:Brown,sides:32,rows:18,patchColor:bulbul?-1:Cream,patchMode:6);
            if(bulbul)
            {
                face.Tube(new[]{new Vector3(0,0.12f,-0.018f),new Vector3(0,0.19f,-0.038f),new Vector3(0,0.26f,-0.031f)},0.041f,0.001f,Ink,12);
                face.Ellipsoid(new Vector3(0,-0.035f,0.006f),new Vector3(0.059f,0.066f,0.063f),Ink);
            }
            Vector3 a=new Vector3(-0.022f,0.015f,0.072f),b=new Vector3(0.022f,0.015f,0.072f),c=new Vector3(0,0.043f,0.078f),tip=new Vector3(0,0.018f,bulbul?0.161f:0.138f);
            face.Tri(a,c,tip,Dark,true);face.Tri(c,b,tip,Dark,true);face.Tri(b,a,tip,Dark,true);
            var eyes=new List<Transform>();
            for(int side=-1;side<=1;side+=2)
            {
                if(bulbul)
                {
                    face.Ellipsoid(new Vector3(side*0.085f,0.044f,0.027f),new Vector3(0.010f,0.041f,0.040f),White,Quaternion.Euler(0,side*12,0));
                    face.Ellipsoid(new Vector3(side*0.096f,0.041f,0.004f),new Vector3(0.005f,0.013f,0.013f),Comb,sides:12,rows:8);
                }
                else face.Ellipsoid(new Vector3(side*0.083f,0.026f,0.010f),new Vector3(0.008f,0.015f,0.012f),Ink,sides:12,rows:8);
                eyes.Add(Eye(kit,head,new Vector3(side*0.066f,0.078f,0.056f),new Vector3(side*0.78f,0.03f,0.63f),0.48f,false));
            }
            var wings=new Transform[2];
            for(int side=-1;side<=1;side+=2)
            {
                Transform wing=Joint(side<0?"Left wing":"Right wing",body,new Vector3(side*0.091f,0.021f,-0.025f));wings[side<0?0:1]=wing;
                var feathers=kit.Part("Folded flight feathers",wing);
                feathers.Ellipsoid(new Vector3(side*0.017f,-0.027f,-0.040f),new Vector3(0.040f,0.069f,bulbul?0.149f:0.118f),FeatherBrown,Quaternion.Euler(-15,0,side*-8));
                for(int i=0;i<3;i++)feathers.Ellipsoid(new Vector3(side*0.049f,-0.052f+i*0.027f,-0.055f-i*0.022f),new Vector3(0.006f,0.008f,bulbul?0.091f:0.072f),bulbul?Brown:Cream,Quaternion.Euler(-14,0,0),12,6);
            }
            Transform tail=Joint("Tail fan",body,new Vector3(0,0.025f,bulbul?-0.177f:-0.15f));var tailMesh=kit.Part("Long tail feathers",tail);
            for(int i=0;i<3;i++)
            {
                tailMesh.Ellipsoid(new Vector3((i-1)*0.023f,-0.02f,bulbul?-0.12f:-0.082f),new Vector3(0.023f,0.016f,bulbul?0.144f:0.104f),bulbul?Dark:FeatherBrown,Quaternion.Euler(-13,0,(i-1)*7),12,8);
                if(bulbul)tailMesh.Ellipsoid(new Vector3((i-1)*0.023f,-0.055f,-0.247f),new Vector3(0.023f,0.011f,0.022f),Cream,Quaternion.Euler(-13,0,0),10,6);
            }
            var legs=new Transform[2];var contacts=new List<Transform>();float hip=bulbul?0.12f:0.10f;
            for(int side=-1;side<=1;side+=2)
            {
                Transform leg=Joint(side<0?"Left leg":"Right leg",motion,new Vector3(side*0.037f,hip,0.017f));legs[side<0?0:1]=leg;var foot=kit.Part("Perching toes",leg);
                foot.Tube(new[]{Vector3.zero,new Vector3(0,-hip+0.009f,0)},0.007f,0.004f,Dark,6);
                for(int i=0;i<3;i++)foot.Tube(new[]{new Vector3(0,-hip+0.008f,0),new Vector3((i-1)*0.023f,-hip+0.004f,0.051f)},0.004f,0.002f,Dark,5);
                foot.Tube(new[]{new Vector3(0,-hip+0.007f,0),new Vector3(0,-hip+0.003f,-0.032f)},0.004f,0.002f,Dark,5);
                contacts.Add(Joint("Toe contact",leg,new Vector3(0,-hip,0.050f)));contacts.Add(Joint("Heel contact",leg,new Vector3(0,-hip,-0.03f)));
            }
            avatar.Configure(kind,motion,body,head,tail,legs,wings,eyes.ToArray(),contacts.ToArray());
        }
        private static void Fish(Kit kit,FarmAnimalAvatar avatar,Transform motion,FarmAnimalKind kind)
        {
            bool koi=kind==FarmAnimalKind.Koi,perch=kind==FarmAnimalKind.Perch;int skin=koi?White:perch?PerchOlive:CarpGold,fin=koi?Cream:perch?PerchOlive:FinGold;
            Vector3 radius=koi?new Vector3(0.108f,0.135f,0.39f):perch?new Vector3(0.086f,0.153f,0.28f):new Vector3(0.122f,0.175f,0.34f);
            Transform body=Joint("Body",motion,new Vector3(0,0.18f,0));var torso=kit.Part("Fish body",body);
            torso.Ellipsoid(Vector3.zero,radius,skin,sides:40,rows:22,patchColor:koi?KoiRed:-1,patchMode:4);
            Transform head=Joint("Head pivot",body,new Vector3(0,0,radius.z*0.65f));var face=kit.Part("Head and mouth",head);
            face.Ellipsoid(new Vector3(0,0,0.025f),new Vector3(radius.x*0.85f,radius.y*0.73f,0.13f),skin,sides:32,rows:20);
            var eyes=new List<Transform>();
            for(int side=-1;side<=1;side+=2)
            {
                eyes.Add(Eye(kit,head,new Vector3(side*radius.x*0.75f,0.041f,0.068f),new Vector3(side*0.94f,0.08f,0.33f),0.48f,false));
                if(!perch)face.Tube(new[]{new Vector3(side*0.050f,-0.036f,0.125f),new Vector3(side*0.075f,-0.051f,0.155f),new Vector3(side*0.095f,-0.047f,0.185f)},0.0035f,0.001f,fin,6);
            }
            var mouth=new Vector3[17];for(int i=0;i<mouth.Length;i++){float a=i*Mathf.PI*2/16;mouth[i]=new Vector3(Mathf.Cos(a)*0.026f,-0.015f+Mathf.Sin(a)*0.021f,0.147f);}
            face.Tube(mouth,0.004f,0.004f,fin,8);face.Ellipsoid(new Vector3(0,-0.015f,0.144f),new Vector3(0.021f,0.016f,0.003f),Dark,sides:12,rows:8);
            var details=kit.Part("Scales and gill lines",body,false);
            for(int side=-1;side<=1;side+=2)
            {
                details.Tube(new[]{new Vector3(side*radius.x*0.74f,radius.y*0.50f,radius.z*0.55f),
                    new Vector3(side*radius.x*0.93f,0,radius.z*0.50f),new Vector3(side*radius.x*0.78f,-radius.y*0.50f,radius.z*0.50f)},0.0022f,0.0015f,koi?Feather:Brown,6);
                if(!koi)for(int row=0;row<3;row++)for(int col=0;col<7;col++)
                {
                    var arc=new Vector3[5];float z=-radius.z*0.61f+col*radius.z*0.17f,y=(row-1)*radius.y*0.43f;
                    for(int j=0;j<arc.Length;j++)
                    {float a=-0.9f+j*0.45f,py=y+0.021f*Mathf.Sin(a),pz=z+0.020f*Mathf.Cos(a);
                        float x=radius.x*Mathf.Sqrt(Mathf.Max(0,1-py*py/(radius.y*radius.y)-pz*pz/(radius.z*radius.z)));
                        arc[j]=new Vector3(side*(x+0.0015f),py,pz);}
                    details.Tube(arc,0.0015f,0.0012f,perch?Hoof:FinGold,5);
                }
            }
            var dorsal=kit.Part("Dorsal and anal fins",body);
            Vector3 dorsalBase=new Vector3(0,radius.y*0.67f,0);var edge=new Vector3[perch?15:9];
            for(int i=0;i<edge.Length;i++)
            {
                float t=(float)i/(edge.Length-1),z=Mathf.Lerp(radius.z*0.47f,-radius.z*0.68f,t);
                float y=radius.y*(0.96f+0.50f*Mathf.Sin(t*Mathf.PI))+(perch&&i%2==1?0.034f:0);
                edge[i]=new Vector3(0,y,z);
            }
            FinFan(dorsal,dorsalBase,edge,fin,koi?Feather:Hoof);
            FinFan(dorsal,new Vector3(0,-radius.y*0.60f,-radius.z*0.28f),new[]{new Vector3(0,-radius.y*0.90f,-radius.z*0.05f),new Vector3(0,-radius.y*1.36f,-radius.z*0.32f),new Vector3(0,-radius.y*0.65f,-radius.z*0.63f)},fin,fin);
            var fins=new Transform[2];
            for(int side=-1;side<=1;side+=2)
            {
                Transform pivot=Joint(side<0?"Left pectoral fin":"Right pectoral fin",body,new Vector3(side*radius.x*0.73f,-radius.y*0.36f,radius.z*0.39f));fins[side<0?0:1]=pivot;
                var flipper=kit.Part("Pectoral fin rays",pivot,false);
                FinFan(flipper,Vector3.zero,new[]{new Vector3(side*0.017f,-0.008f,0.025f),new Vector3(side*0.14f,-0.044f,-0.037f),new Vector3(side*0.13f,-0.072f,-0.11f),new Vector3(side*0.025f,-0.024f,-0.075f)},fin,koi?Feather:Hoof);
            }
            Transform tail=Joint("Tail pivot",body,new Vector3(0,0,-radius.z*0.82f));var caudal=kit.Part("Tail fin",tail);
            caudal.Tube(new[]{Vector3.zero,new Vector3(0,0,-0.090f)},0.043f,0.019f,skin,10);
            if(perch)
            {
                var rounded=new Vector3[11];for(int i=0;i<rounded.Length;i++){float a=i*Mathf.PI/(rounded.Length-1);rounded[i]=new Vector3(0,Mathf.Cos(a)*0.119f,-0.09f-Mathf.Sin(a)*0.16f);}
                FinFan(caudal,new Vector3(0,0,-0.055f),rounded,fin,Hoof);
            }
            else FinFan(caudal,new Vector3(0,0,-0.08f),new[]{new Vector3(0,0.025f,-0.07f),new Vector3(0,0.154f,-0.26f),new Vector3(0,0.046f,-0.241f),new Vector3(0,0,-0.185f),new Vector3(0,-0.046f,-0.241f),new Vector3(0,-0.154f,-0.26f),new Vector3(0,-0.025f,-0.07f)},fin,koi?Feather:Brown);
            avatar.Configure(kind,motion,body,head,tail,new Transform[0],fins,eyes.ToArray(),new Transform[0]);
        }
        private static void FinFan(AnimalMesh mesh,Vector3 root,Vector3[] edge,int color,int rays)
        {
            for(int i=0;i<edge.Length-1;i++)mesh.Tri(root,edge[i],edge[i+1],color,true);
            for(int i=0;i<edge.Length;i++)mesh.Tube(new[]{root,Vector3.Lerp(root,edge[i],0.98f)},0.0016f,0.0010f,rays,5);
        }
    }
}
