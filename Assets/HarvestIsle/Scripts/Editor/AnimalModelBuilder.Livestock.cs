using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class AnimalModelBuilder
    {
        private const int SoftCream=29,SoftPink=30,Rose=31,Slate=32,SoftMuzzle=33,SoftHoof=34,CoatSpot=35,SoftHorn=36;

        private static void FriendlyLivestock(Kit kit,FarmAnimalAvatar avatar,Transform motion,FarmAnimalKind kind)
        {
            bool pig=kind==FarmAnimalKind.Pig,cow=kind==FarmAnimalKind.Cow;
            int skin=pig?SoftPink:cow?SoftCream:Slate;
            float bodyY=pig?.38f:cow?.51f:.53f;
            Vector3 bodySize=pig?new Vector3(.335f,.275f,.445f):new Vector3(.355f,.31f,cow?.50f:.54f);
            Transform body=Joint("Body",motion,new Vector3(0,bodyY,0));
            var torso=kit.Part("Rounded coat",body);
            torso.Ellipsoid(Vector3.zero,bodySize,skin,sides:40,rows:24,patchColor:cow?CoatSpot:-1,patchMode:7);
            // Broad shoulders connect the neck to the body instead of exposing ball joints.
            torso.Ellipsoid(new Vector3(0,.015f,.30f),new Vector3(.27f,.26f,.24f),skin,sides:28,rows:18);
            Transform head=Joint("Head pivot",body,new Vector3(0,pig?.035f:.055f,pig?.365f:.405f));
            var face=kit.Part("Soft face and ears",head);
            face.Ellipsoid(new Vector3(0,.055f,.04f),new Vector3(pig?.275f:.27f,.255f,.255f),skin,sides:36,rows:24);
            Vector3 muzzle=new Vector3(0,-.065f,pig?.275f:.26f);
            int noseColor=pig?Rose:cow?SoftPink:SoftMuzzle;
            face.Ellipsoid(muzzle,new Vector3(pig?.16f:.205f,pig?.098f:.117f,pig?.070f:.115f),noseColor,sides:32,rows:18);
            for(int side=-1;side<=1;side+=2)
            {
                // Small recessed-looking nostrils, kept away from the eyes.
                face.Ellipsoid(muzzle+new Vector3(side*(pig?.056f:.084f),.014f,pig?.065f:.103f),
                    new Vector3(pig?.018f:.022f,pig?.024f:.014f,.007f),pig?PinkDark:cow?Rose:SoftHoof,sides:16,rows:10);
                if(pig)
                {
                    Quaternion turn=Quaternion.Euler(-18,side*12,side*-30);
                    face.Ellipsoid(new Vector3(side*.213f,.247f,.015f),new Vector3(.098f,.132f,.049f),skin,turn,24,16);
                    face.Ellipsoid(new Vector3(side*.22f,.255f,.051f),new Vector3(.065f,.087f,.018f),Rose,turn,20,12);
                    face.Ellipsoid(new Vector3(side*.248f,.191f,.051f),new Vector3(.072f,.055f,.049f),skin,turn,20,12);
                }
                else
                {
                    Quaternion turn=Quaternion.Euler(0,side*-12,side*-14);
                    face.Ellipsoid(new Vector3(side*.286f,.155f,.025f),new Vector3(.127f,.065f,.073f),skin,turn,24,14);
                    face.Ellipsoid(new Vector3(side*.307f,.157f,.078f),new Vector3(.077f,.036f,.017f),cow?SoftPink:SoftMuzzle,turn,20,12);
                    Vector3[] horn=new Vector3[19];
                    Vector3 a=new Vector3(side*.18f,.246f,-.012f);
                    Vector3 b=cow?new Vector3(side*.25f,.29f,-.025f):new Vector3(side*.46f,.19f,-.065f);
                    Vector3 c=cow?new Vector3(side*.255f,.34f,-.018f):new Vector3(side*.51f,.36f,-.11f);
                    Vector3 d=cow?new Vector3(side*.235f,.37f,.005f):new Vector3(side*.39f,.435f,-.12f);
                    for(int j=0;j<horn.Length;j++){float t=(float)j/(horn.Length-1),u=1-t;horn[j]=u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;}
                    face.Tube(horn,cow?.035f:.05f,.010f,cow?SoftHorn:SoftHoof,12);
                    face.Ellipsoid(d,Vector3.one*.0105f,cow?SoftHorn:SoftHoof,sides:12,rows:8);
                }
            }
            if(!pig)for(int i=0;i<3;i++)
                face.Ellipsoid(new Vector3((i-1)*.047f,.277f,.097f),new Vector3(.047f,.057f,.027f),cow?SoftCream:SoftMuzzle,Quaternion.Euler(0,0,(i-1)*-18),20,12);
            // The mouth follows the surface of the muzzle, rather than protruding like a tube.
            face.Tube(new[]{new Vector3(-.065f,-.123f,pig?.322f:.346f),new Vector3(0,-.138f,pig?.328f:.351f),new Vector3(.065f,-.123f,pig?.322f:.346f)},.0035f,.0035f,pig||cow?PinkDark:SoftHoof,8);
            var eyes=new List<Transform>();
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 p=new Vector3(side*.146f,.112f,.248f);
                Transform eye=Joint("Eye",head,p);eye.localRotation=Quaternion.LookRotation(new Vector3(side*.42f,.05f,1));
                var detail=kit.Part("Gentle eye",eye,false);
                detail.Ellipsoid(Vector3.zero,new Vector3(.034f,.043f,.012f),Ink,sides:24,rows:16);
                detail.Ellipsoid(new Vector3(-.010f,.014f,.010f),new Vector3(.009f,.011f,.004f),White,sides:16,rows:10);
                detail.Ellipsoid(new Vector3(.011f,-.013f,.011f),new Vector3(.004f,.005f,.003f),White,sides:12,rows:8);
                eyes.Add(eye);
            }
            Transform tail=Joint("Tail",body,new Vector3(0,.07f,-bodySize.z*.96f));var tailMesh=kit.Part("Soft tail",tail);
            if(pig)
            {
                var curl=new Vector3[26];curl[0]=Vector3.zero;
                for(int j=1;j<curl.Length;j++){float t=(j-1f)/(curl.Length-2),a=t*Mathf.PI*2.25f,r=.047f*(1-t*.42f);curl[j]=new Vector3(Mathf.Sin(a)*r,.03f+Mathf.Cos(a)*r,-.055f-t*.04f);}
                tailMesh.Tube(curl,.015f,.009f,skin,12);
            }
            else
            {
                tailMesh.Tube(new[]{Vector3.zero,new Vector3(.016f,-.13f,-.07f),new Vector3(.026f,-.29f,-.065f)},.023f,.015f,skin,12);
                tailMesh.Ellipsoid(new Vector3(.026f,-.305f,-.065f),new Vector3(.044f,.059f,.04f),SoftHoof,sides:20,rows:14);
            }
            var legs=new Transform[4];var contacts=new List<Transform>();
            float hip=pig?.235f:.355f,half=pig?.208f:.231f,front=pig?.255f:.30f;
            for(int i=0;i<4;i++)
            {
                int side=i%2==0?-1:1;
                Transform leg=Joint((i<2?"Front ":"Rear ")+(side<0?"left leg":"right leg"),motion,new Vector3(side*half,hip,i<2?front:-front));legs[i]=leg;
                var limb=kit.Part("Rounded leg and hoof",leg);float r=pig?.083f:.091f,sole=.052f;
                limb.Ellipsoid(new Vector3(0,-hip*.32f,0),new Vector3(r,hip*.68f,r*1.06f),skin,sides:24,rows:16);
                Vector3 foot=new Vector3(0,-hip+sole*.5f,.018f);
                limb.Ellipsoid(foot,new Vector3(r*.98f,sole,r*1.12f),pig?PigShade:SoftHoof,sides:24,rows:12,flatBottom:true);
                limb.Tube(new[]{new Vector3(0,-hip+.014f,.018f+r*1.10f),new Vector3(0,-hip+.049f,.018f+r*.97f)},.0025f,.002f,pig?PinkDark:CoatSpot,6);
                contacts.Add(Joint("Toe contact",leg,new Vector3(0,-hip,.018f+r*.8f)));
                contacts.Add(Joint("Heel contact",leg,new Vector3(0,-hip,.018f-r*.8f)));
            }
            avatar.Configure(kind,motion,body,head,tail,legs,new Transform[0],eyes.ToArray(),contacts.ToArray());
        }
    }
}
