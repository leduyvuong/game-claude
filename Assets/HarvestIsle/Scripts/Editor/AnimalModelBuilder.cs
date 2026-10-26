using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    internal static partial class AnimalModelBuilder
    {
        internal const int Version=8;
        internal static readonly string[] Names={"Chicken","Duck","Cow","Pig","Cat","Sheep","Buffalo","Deer","Sparrow","Koi","Carp","Perch","Bulbul"};
        internal static readonly string[] Labels={"Gà","Vịt","Bò","Heo","Mèo","Cừu","Trâu","Hươu","Chim sẻ","Cá koi","Cá chép","Cá rô đồng","Chim chào mào"};
        private static readonly string[] Colors={"EFDFBB","CDBF9A","463B32","20251F","FFFAE4","DDA24A","BD5B4B",
            "DFA29B","BE8581","907256","D7C7A1","484D45","E7BDA5","CE9D58","9D7346","9DB376","6B6352","D7A18D","F7EBD0",
            "F1E9CE","D1C9AF","656D62","C89962","826A4E","CE6650","B79C58","788455","CAD0AF","CFAE70",
            "F2E8D6","EAB3AE","D99195","77868B","A0ABAB","5E5754","454242","D9C5A1"};
        private const int Cream=0,Feather=1,Dark=2,Ink=3,White=4,Orange=5,Comb=6,Pink=7,PinkDark=8,
            Brown=9,Horn=10,CowBlack=11,PigSkin=12,Ginger=13,GingerDark=14,Iris=15,Hoof=16,PigShade=17,DuckWhite=18;
        private const int Wool=19,WoolShade=20,BuffaloSkin=21,DeerSkin=22,FeatherBrown=23,KoiRed=24,CarpGold=25,PerchOlive=26,FishSilver=27,FinGold=28;
        internal static Material[] CreateMaterials()
        {
            var materials=new Material[Colors.Length];
            for(int i=0;i<materials.Length;i++)
            {
                var m=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));m.name="Animals "+Colors[i];
                m.SetColor("_BaseColor",FarmWorld.Hex(Colors[i]));m.SetColor("_SpecColor",Color.black);m.SetFloat("_Smoothness",0.06f);materials[i]=m;
            }
            return materials;
        }
        internal static GameObject Create(int index,Material[] materials,List<Mesh> meshes)
        {
            var kit=new Kit(materials,meshes);
            var root=new GameObject(Names[index]);var avatar=root.AddComponent<FarmAnimalAvatar>();
            Transform motion=Joint("Motion",root.transform,Vector3.zero);
            if(index==2||index==3||index==6)FriendlyLivestock(kit,avatar,motion,(FarmAnimalKind)index);
            else if(index<2)Bird(kit,avatar,motion,index==1);
            else if(index<5)Quadruped(kit,avatar,motion,(FarmAnimalKind)index);
            else ExtendedCreature(kit,avatar,motion,(FarmAnimalKind)index);
            kit.Finish();return root;
        }
        private static Transform Joint(string name,Transform parent,Vector3 position)
        {var item=new GameObject(name);item.transform.SetParent(parent,false);item.transform.localPosition=position;return item.transform;}

        private static void Bird(Kit kit,FarmAnimalAvatar avatar,Transform motion,bool duck)
        {
            Transform body=Joint("Body",motion,new Vector3(0,duck?0.29f:0.31f,0));
            var torso=kit.Part("Feathered body",body);
            torso.Ellipsoid(Vector3.zero,duck?new Vector3(0.215f,0.19f,0.30f):new Vector3(0.19f,0.205f,0.235f),duck?DuckWhite:Cream);
            Transform head=Joint("Neck and head",body,new Vector3(0,duck?0.165f:0.14f,duck?0.215f:0.14f));
            var skull=kit.Part("Head and bill",head);
            skull.Ellipsoid(new Vector3(0,-0.065f,-0.028f),new Vector3(0.075f,duck?0.15f:0.105f,0.082f),duck?DuckWhite:Cream);
            skull.Ellipsoid(new Vector3(0,0.072f,0),new Vector3(0.125f,0.14f,0.125f),duck?DuckWhite:Cream);
            if(duck)
            {
                skull.Ellipsoid(new Vector3(0,0.013f,0.172f),new Vector3(0.065f,0.022f,0.096f),Orange,sides:18,rows:8);
                skull.Ellipsoid(new Vector3(0,0.002f,0.168f),new Vector3(0.060f,0.009f,0.087f),Brown,sides:16,rows:6);
                for(int side=-1;side<=1;side+=2)skull.Ellipsoid(new Vector3(side*0.025f,0.032f,0.18f),new Vector3(0.004f,0.002f,0.006f),Dark,sides:8,rows:4);
            }
            else
            {
                Vector3 left=new Vector3(-0.035f,0.018f,0.112f),right=new Vector3(0.035f,0.018f,0.112f),
                    upper=new Vector3(0,0.062f,0.119f),lower=new Vector3(0,0.005f,0.113f),tip=new Vector3(0,0.032f,0.21f);
                skull.Tri(left,upper,tip,Orange,true);skull.Tri(upper,right,tip,Orange,true);
                skull.Tri(right,lower,tip,Orange,true);skull.Tri(lower,left,tip,Orange,true);
                for(int i=0;i<3;i++)skull.Ellipsoid(new Vector3(0,0.20f+(i==1?0.010f:0),-0.05f+i*0.04f),new Vector3(0.026f,0.041f,0.035f),Comb,sides:12,rows:8);
                skull.Ellipsoid(new Vector3(0,-0.015f,0.100f),new Vector3(0.027f,0.044f,0.026f),Comb,sides:12,rows:8);
            }
            var eyes=new List<Transform>();
            for(int side=-1;side<=1;side+=2)
                eyes.Add(Eye(kit,head,new Vector3(side*0.097f,0.105f,0.080f),new Vector3(side*0.75f,0.08f,0.66f),0.68f,false));
            var wings=new Transform[2];
            for(int side=-1;side<=1;side+=2)
            {
                Transform pivot=Joint(side<0?"Left wing":"Right wing",body,new Vector3(side*(duck?0.18f:0.155f),0.025f,-0.025f));wings[side<0?0:1]=pivot;
                var wing=kit.Part("Layered wing feathers",pivot);
                wing.Ellipsoid(new Vector3(side*0.033f,-0.036f,-0.025f),new Vector3(0.066f,0.117f,duck?0.195f:0.151f),duck?DuckWhite:Feather,
                    Quaternion.Euler(-15,0,side*-10));
                for(int i=0;i<3;i++)wing.Ellipsoid(new Vector3(side*0.076f,-0.056f+i*0.026f,-0.083f-i*0.021f),
                    new Vector3(0.014f,0.017f,duck?0.098f:0.073f),Cream,Quaternion.Euler(-15,0,0),12,6);
            }
            Transform tail=Joint("Tail feathers",body,new Vector3(0,0.065f,duck?-0.275f:-0.217f));var feathers=kit.Part("Tail fan",tail);
            for(int i=0;i<3;i++)
                feathers.Ellipsoid(new Vector3((i-1)*0.036f,duck?0.019f:0.035f,-0.045f),new Vector3(0.032f,duck?0.045f:0.082f,duck?0.069f:0.088f),
                    i==1?Cream:Feather,Quaternion.Euler(duck?20:42,0,(i-1)*-22),12,8);
            var legs=new Transform[2];var contacts=new List<Transform>();
            for(int side=-1;side<=1;side+=2)
            {
                Transform leg=Joint(side<0?"Left leg":"Right leg",motion,new Vector3(side*(duck?0.083f:0.068f),0.155f,0.023f));legs[side<0?0:1]=leg;
                var foot=kit.Part(duck?"Webbed foot":"Chicken toes",leg);
                foot.Tube(new[]{Vector3.zero,new Vector3(0,-0.130f,0)},0.013f,0.010f,Orange);
                if(duck)
                {
                    Vector3 ankle=new Vector3(0,-0.132f,-0.015f),a=new Vector3(-0.060f,-0.150f,0.095f),
                        b=new Vector3(0,-0.149f,0.118f),c=new Vector3(0.061f,-0.150f,0.095f);
                    foot.Tri(ankle,b,a,Orange,true);foot.Tri(ankle,c,b,Orange,true);
                    foot.Tube(new[]{ankle,a},0.006f,0.004f,Orange);foot.Tube(new[]{ankle,b},0.006f,0.004f,Orange);foot.Tube(new[]{ankle,c},0.006f,0.004f,Orange);
                }
                else
                {
                    for(int toe=0;toe<3;toe++)foot.Tube(new[]{new Vector3(0,-0.134f,0),new Vector3((toe-1)*0.032f,-0.148f,0.077f-Mathf.Abs(toe-1)*0.01f)},0.006f,0.004f,Orange);
                    foot.Tube(new[]{new Vector3(0,-0.135f,0),new Vector3(0,-0.149f,-0.033f)},0.006f,0.003f,Orange);
                }
                contacts.Add(Joint("Toe contact",leg,new Vector3(0,-0.155f,duck?0.11f:0.07f)));
                contacts.Add(Joint("Heel contact",leg,new Vector3(0,-0.155f,-0.025f)));
            }
            avatar.Configure(duck?FarmAnimalKind.Duck:FarmAnimalKind.Chicken,motion,body,head,tail,legs,wings,eyes.ToArray(),contacts.ToArray());
        }

        private static void Quadruped(Kit kit,FarmAnimalAvatar avatar,Transform motion,FarmAnimalKind kind)
        {
            bool cow=kind==FarmAnimalKind.Cow,pig=kind==FarmAnimalKind.Pig;
            float bodyY=cow?0.64f:pig?0.48f:0.33f;
            Transform body=Joint("Body",motion,new Vector3(0,bodyY,0));var torso=kit.Part("Sculpted body",body);
            int skin=cow?Cream:pig?PigSkin:Ginger;
            torso.Ellipsoid(Vector3.zero,cow?new Vector3(0.34f,0.30f,0.53f):pig?new Vector3(0.31f,0.265f,0.45f):new Vector3(0.185f,0.17f,0.32f),skin,
                sides:48,rows:24,patchColor:cow?CowBlack:pig?-1:GingerDark,patchMode:cow?1:2);
            if(cow)
            {
                torso.Ellipsoid(new Vector3(0,-0.30f,-0.14f),new Vector3(0.13f,0.068f,0.12f),Pink);
                for(int i=0;i<4;i++)torso.Tube(new[]{new Vector3(i%2==0?-0.060f:0.060f,-0.34f,-0.09f-i/2*0.075f),
                    new Vector3(i%2==0?-0.06f:0.06f,-0.39f,-0.09f-i/2*0.075f)},0.015f,0.010f,PinkDark);
            }
            Transform head=Joint("Head pivot",body,new Vector3(0,cow?0.06f:pig?0.015f:0.08f,cow?0.43f:pig?0.365f:0.29f));
            var face=kit.Part("Head and face",head);
            Vector3 skullCenter=cow?new Vector3(0,0.08f,0.035f):pig?new Vector3(0,0.035f,0.045f):new Vector3(0,0.035f,0.015f);
            face.Ellipsoid(skullCenter,cow?new Vector3(0.235f,0.245f,0.24f):pig?new Vector3(0.255f,0.232f,0.235f):new Vector3(0.205f,0.190f,0.17f),skin,
                sides:kind==FarmAnimalKind.Cat?48:24,rows:kind==FarmAnimalKind.Cat?24:14,patchColor:kind==FarmAnimalKind.Cat?GingerDark:-1,patchMode:3);
            if(cow)
            {
                face.Ellipsoid(new Vector3(0,-0.081f,0.232f),new Vector3(0.205f,0.113f,0.142f),Pink);
                for(int side=-1;side<=1;side+=2)
                {
                    face.Ellipsoid(new Vector3(side*0.078f,-0.063f,0.361f),new Vector3(0.027f,0.020f,0.009f),PinkDark,sides:12,rows:8);
                    face.Ellipsoid(new Vector3(side*0.263f,0.155f,0.010f),new Vector3(0.128f,0.054f,0.068f),Cream,Quaternion.Euler(0,0,side*-18));
                    face.Ellipsoid(new Vector3(side*0.278f,0.155f,0.064f),new Vector3(0.080f,0.032f,0.012f),Pink,Quaternion.Euler(0,0,side*-18));
                    face.Tube(new[]{new Vector3(side*0.15f,0.253f,-0.044f),new Vector3(side*0.20f,0.327f,-0.030f),new Vector3(side*0.19f,0.379f,0.003f)},0.035f,0.003f,Horn,10);
                }
                face.Tube(new[]{new Vector3(-0.088f,-0.155f,0.324f),new Vector3(0,-0.165f,0.337f),new Vector3(0.088f,-0.155f,0.324f)},0.003f,0.003f,PinkDark);
            }
            else if(pig)
            {
                face.Ellipsoid(new Vector3(0,-0.013f,0.264f),new Vector3(0.155f,0.100f,0.103f),Pink,sides:20,rows:12);
                for(int side=-1;side<=1;side+=2)
                {
                    face.Ellipsoid(new Vector3(side*0.050f,-0.008f,0.357f),new Vector3(0.020f,0.029f,0.009f),PinkDark,sides:12,rows:8);
                    Ear(face,side,true);
                }
                face.Tube(new[]{new Vector3(-0.064f,-0.095f,0.291f),new Vector3(0,-0.107f,0.307f),new Vector3(0.064f,-0.095f,0.291f)},0.003f,0.003f,PinkDark);
            }
            else
            {
                for(int side=-1;side<=1;side+=2)
                {
                    Ear(face,side,false);
                    face.Ellipsoid(new Vector3(side*0.047f,-0.052f,0.162f),new Vector3(0.059f,0.045f,0.032f),Cream,sides:16,rows:10);
                    for(int i=0;i<3;i++)face.Tube(new[]{new Vector3(side*0.065f,-0.051f+i*0.009f,0.184f),
                        new Vector3(side*0.195f,-0.050f+(i-1)*0.030f,0.175f)},0.0025f,0.0015f,Dark,5);
                    face.Tube(new[]{new Vector3(0,-0.071f,0.197f),new Vector3(0,-0.088f,0.190f),new Vector3(side*0.022f,-0.096f,0.184f)},0.0025f,0.002f,Dark,5);
                }
                face.Tri(new Vector3(-0.018f,-0.047f,0.197f),new Vector3(0.018f,-0.047f,0.197f),new Vector3(0,-0.065f,0.203f),PinkDark,true);
            }
            var eyes=new List<Transform>();
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 p=cow?new Vector3(side*0.163f,0.155f,0.187f):pig?new Vector3(side*0.164f,0.12f,0.198f):new Vector3(side*0.083f,0.069f,0.172f);
                eyes.Add(Eye(kit,head,p,new Vector3(side*(cow||pig?0.60f:0.28f),0.05f,0.85f),cow?1.08f:pig?0.95f:1.17f,kind==FarmAnimalKind.Cat));
            }
            Transform tail=Joint("Tail",body,new Vector3(0,pig?0.065f:cow?0.10f:0.055f,cow?-0.50f:pig?-0.44f:-0.26f));var tailMesh=kit.Part("Shaped tail",tail);
            if(cow)
            {
                tailMesh.Tube(new[]{Vector3.zero,new Vector3(0,-0.16f,-0.06f),new Vector3(0,-0.34f,-0.075f)},0.025f,0.014f,Cream);
                tailMesh.Ellipsoid(new Vector3(0,-0.363f,-0.075f),new Vector3(0.050f,0.078f,0.040f),CowBlack);
            }
            else if(pig)
            {
                var curl=new Vector3[17];curl[0]=Vector3.zero;
                for(int i=1;i<curl.Length;i++)
                {float t=(float)(i-1)/(curl.Length-2),a=t*Mathf.PI*2.1f;float radius=0.052f*(1-t*0.45f);
                    curl[i]=new Vector3(Mathf.Sin(a)*radius,0.027f+Mathf.Cos(a)*radius,-0.055f-t*0.035f);}
                tailMesh.Tube(curl,0.013f,0.007f,PigSkin,8);
            }
            else
            {
                var curve=new Vector3[21];Vector3 p1=new Vector3(0,0.05f,-0.28f),p2=new Vector3(0.025f,0.42f,-0.31f),p3=new Vector3(0.035f,0.42f,-0.10f);
                for(int i=0;i<curve.Length;i++){float t=(float)i/(curve.Length-1),u=1-t;curve[i]=3*u*u*t*p1+3*u*t*t*p2+t*t*t*p3;}
                tailMesh.Tube(curve,0.043f,0.025f,Ginger,10);
                var tip=new Vector3[4];for(int i=0;i<tip.Length;i++)tip[i]=curve[curve.Length-tip.Length+i];
                tailMesh.Tube(tip,0.0275f,0.0255f,GingerDark,10);
            }
            var legs=new Transform[4];var contacts=new List<Transform>();
            float hipY=cow?0.54f:pig?0.30f:0.25f,halfWidth=cow?0.21f:pig?0.16f:0.103f;
            float front=cow?0.30f:pig?0.20f:0.165f;
            for(int i=0;i<4;i++)
            {
                int side=i%2==0?-1:1;
                Transform leg=Joint((i<2?"Front ":"Rear ")+(side<0?"left leg":"right leg"),motion,new Vector3(side*halfWidth,hipY,i<2?front:-front));legs[i]=leg;
                var limb=kit.Part("Leg and foot",leg);float radius=cow?0.065f:pig?0.060f:0.037f;
                float sole=cow?0.048f:pig?0.037f:0.032f;
                limb.Ellipsoid(new Vector3(0,-0.022f,0),new Vector3(radius*1.25f,cow?0.115f:pig?0.105f:0.075f,radius*1.32f),skin);
                limb.Tube(new[]{Vector3.zero,new Vector3(0,-hipY*0.48f,-0.006f),new Vector3(0,-hipY+sole*1.7f,0.012f)},radius,radius*0.79f,skin);
                Vector3 foot=new Vector3(0,-hipY+sole,0.023f);Vector3 size=new Vector3(cow?0.073f:pig?0.060f:0.048f,sole,cow?0.086f:pig?0.073f:0.059f);
                if(cow||pig)limb.Shoe(foot,size,cow?Hoof:PigShade);
                else limb.Ellipsoid(new Vector3(foot.x,-hipY+sole*0.5f,foot.z),size,Cream,sides:16,rows:10,flatBottom:true);
                if(cow||pig)limb.Tube(new[]{foot+new Vector3(0,-sole*0.25f,size.z*0.99f),foot+new Vector3(0,sole*0.50f,size.z*0.88f)},0.003f,0.002f,Dark,5);
                contacts.Add(Joint("Toe contact",leg,new Vector3(0,-hipY,foot.z+size.z*(cow||pig?1:0.8f))));
                contacts.Add(Joint("Heel contact",leg,new Vector3(0,-hipY,foot.z-size.z*(cow||pig?1:0.8f))));
            }
            avatar.Configure(kind,motion,body,head,tail,legs,new Transform[0],eyes.ToArray(),contacts.ToArray());
        }

        private static void Ear(AnimalMesh mesh,int side,bool pig)
        {
            Vector3 a=pig?new Vector3(side*0.10f,0.18f,0.025f):new Vector3(side*0.075f,0.167f,0.006f);
            Vector3 b=pig?new Vector3(side*0.235f,0.16f,-0.015f):new Vector3(side*0.202f,0.139f,-0.018f);
            Vector3 tip=pig?new Vector3(side*0.274f,0.315f,0.081f):new Vector3(side*0.164f,0.286f,-0.020f);
            Vector3 forward=Vector3.forward*0.043f,back=Vector3.back*0.018f;
            int skin=pig?PigSkin:Ginger;
            mesh.Tri(a+forward,b+forward,tip+forward,skin,true);mesh.Tri(tip+back,b+back,a+back,skin,true);
            mesh.Tri(a+back,tip+back,tip+forward,skin,true);mesh.Tri(a+back,tip+forward,a+forward,skin,true);
            mesh.Tri(b+forward,tip+forward,tip+back,skin,true);mesh.Tri(b+forward,tip+back,b+back,skin,true);
            Vector3 center=(a+b+tip)/3+forward;
            mesh.Tri(Vector3.Lerp(a+forward,center,0.25f)+Vector3.forward*0.003f,
                Vector3.Lerp(b+forward,center,0.25f)+Vector3.forward*0.003f,
                Vector3.Lerp(tip+forward,center,0.25f)+Vector3.forward*0.003f,Pink,true);
        }
        private static Transform Eye(Kit kit,Transform head,Vector3 center,Vector3 direction,float scale,bool cat)
        {
            Transform eye=Joint("Eye",head,center);eye.localRotation=Quaternion.LookRotation(direction.normalized);eye.localScale=Vector3.one*scale;
            var mesh=kit.Part("Eye detail",eye,false);
            mesh.Ellipsoid(Vector3.zero,new Vector3(0.029f,0.035f,0.010f),cat?White:Ink,sides:20,rows:14);
            if(cat)mesh.Ellipsoid(new Vector3(0,0,0.008f),new Vector3(0.021f,0.028f,0.008f),Iris,sides:16,rows:10);
            mesh.Ellipsoid(new Vector3(0,0,cat?0.015f:0.008f),new Vector3(cat?0.008f:0.016f,0.023f,0.007f),Ink,sides:16,rows:10);
            mesh.Ellipsoid(new Vector3(-0.007f,0.011f,cat?0.021f:0.015f),new Vector3(0.005f,0.006f,0.003f),White,sides:10,rows:6);
            return eye;
        }
        private sealed class Kit
        {
            private readonly Material[] materials;private readonly List<Mesh> meshes;private readonly List<MeshPart> parts=new List<MeshPart>();
            internal Kit(Material[] materials,List<Mesh> meshes){this.materials=materials;this.meshes=meshes;}
            internal AnimalMesh Part(string name,Transform parent,bool shadows=true)
            {var mesh=new AnimalMesh(materials.Length);parts.Add(new MeshPart{name=name,parent=parent,mesh=mesh,shadows=shadows});return mesh;}
            internal void Finish()
            {
                foreach(MeshPart part in parts)
                {
                    Mesh mesh=part.mesh.Finish(part.name);var item=new GameObject(part.name,typeof(MeshFilter),typeof(MeshRenderer));
                    item.transform.SetParent(part.parent,false);item.GetComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=item.GetComponent<MeshRenderer>();renderer.sharedMaterials=materials;renderer.shadowCastingMode=part.shadows?ShadowCastingMode.On:ShadowCastingMode.Off;
                    meshes.Add(mesh);
                }
            }
            private sealed class MeshPart{internal string name;internal Transform parent;internal AnimalMesh mesh;internal bool shadows;}
        }
    }
}
