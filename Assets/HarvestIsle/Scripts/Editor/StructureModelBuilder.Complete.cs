using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        internal static GameObject CreateExtra(string type,Material[] mats,List<Mesh> meshes)
        {
            if(type=="Coin")return HarvestCoinBuilder.Create(meshes);
            if(System.Array.IndexOf(AnimalModelBuilder.AmbientNames,type)>=0)
                return AnimalModelBuilder.CreateAmbient(type,AnimalModelBuilder.CreateMaterials(),meshes);
            var root=new GameObject(type);var m=new EnvironmentMesh(Colors.Length);
            if(type.StartsWith("Product_"))Product(m,type.Substring(8));
            else switch(type)
            {
                case "ElderTu":case "MerchantHaiAu":case "FarmerFemale":return Human(type,mats,meshes);
                case "WildRootstock":case "ThornLemonRootstock":case "MangoBranch":case "OrangeBranch":case "GraftedMango":case "GraftedOrange":case "DualFruitTree":return GraftModel(type,mats,meshes);
                case "RainCloud":case "StormCloud":
                    for(int i=0;i<7;i++)Ellipsoid(m,new Vector3((i-3)*.21f,.67f+i%2*.07f,0),new Vector3(.29f,.20f,.23f),type=="RainCloud"?Plaster:StoneDark);
                    for(int i=0;i<12;i++)Beam(m,new Vector3((i%4-1.5f)*.20f,.47f,(i/4-1)*.14f),new Vector3((i%4-1.5f)*.20f-.06f,.25f,(i/4-1)*.14f),.012f,Water);break;
                case "FrostPatch":for(int i=0;i<18;i++)Ellipsoid(m,new Vector3(Mathf.Cos(i*2.4f)*.40f,.022f,Mathf.Sin(i*2.4f)*.35f),new Vector3(.14f,.023f,.10f),Plaster);break;
                case "FungusPatch":for(int i=0;i<14;i++)Ellipsoid(m,new Vector3(Mathf.Cos(i*2.4f)*.22f,.018f+i%3*.012f,Mathf.Sin(i*2.4f)*.20f),new Vector3(.08f,.018f,.075f),i%3==0?Black:Plaster);break;
                case "GiantPumpkin":Ellipsoid(m,new Vector3(0,.51f,0),new Vector3(.67f,.51f,.64f),TileLight);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;m.Tube(new[]{new Vector3(Mathf.Cos(a)*.60f,.35f,Mathf.Sin(a)*.59f),new Vector3(Mathf.Cos(a)*.56f,.78f,Mathf.Sin(a)*.54f),new Vector3(Mathf.Cos(a)*.23f,.99f,Mathf.Sin(a)*.22f)},.014f,.008f,Tile,8);}Beam(m,Vector3.up*.98f,new Vector3(.05f,1.19f,0),.055f,WoodDark);break;
                case "FestivalArch":for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.85f,0,0),new Vector3(side*.85f,1.80f,0),.067f,Bamboo);Beam(m,new Vector3(-.9f,1.72f,0),new Vector3(.9f,1.72f,0),.05f,Bamboo);for(int i=0;i<5;i++)HangingLantern(m,new Vector3((i-2)*.34f,1.67f,0));break;
                case "ConstructionSite":Box(m,Vector3.up*.055f,new Vector3(1.6f,.11f,1.25f),Stone);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,new Vector3(x*.70f,.11f,z*.50f),new Vector3(x*.70f,1.32f,z*.50f),.035f);Beam(m,new Vector3(-.70f,1.3f,-.50f),new Vector3(.7f,1.3f,-.50f),.035f);Sack(m,new Vector3(.32f,.11f,.15f));ToolOrDecoration(m,"WoodBundle");break;
                case "BrokenFence":for(int i=0;i<4;i++)Beam(m,new Vector3(-.5f+i*.28f,0,0),new Vector3(-.40f+i*.28f,.38f-i*.09f,.12f),.030f,Bamboo);Beam(m,new Vector3(-.53f,.08f,.15f),new Vector3(.55f,.11f,-.06f),.028f,Bamboo);break;
                case "DamagedRoof":Roof(m,1.43f,.78f,.09f,.16f,true);for(int i=0;i<5;i++)Box(m,new Vector3((i-2)*.19f,.02f,.64f+i%2*.12f),new Vector3(.16f,.04f,.23f),Tile);break;
                case "TerracePlot":for(int i=0;i<3;i++){Box(m,new Vector3(0,.08f+i*.08f,(i-1)*.49f),new Vector3(1.75f,.16f+i*.16f,.46f),Wood);Box(m,new Vector3(0,.17f+i*.16f,(i-1)*.49f),new Vector3(1.69f,.02f,.40f),Cloth);}break;
                case "GoldenFirefly":case "Bee":case "Ladybug":case "Planthopper":case "Snail":case "FruitBorer":case "FieldMouse":case "Locust":case "Myna":Creature(root,m,type,mats,meshes);break;
                case "BroadleafTree":case "BambooClump":case "Shrub":case "Fern":case "DeadTree":case "Reeds":case "Wildflowers":Vegetation(m,type);break;
                case "Volcano":case "SnowMountain":MountainExtra(m,type=="Volcano");break;
                case "Driftwood":case "WoodLog":case "TreeStump":case "FallenCoconut":case "BeachShell":NatureProp(m,type);break;
                case "DrainStraight":case "DrainCorner":case "DrainJunction":case "IrrigationCanal":Channel(m,type);break;
                case "FloodDike":case "DikeCorner":Dike(m,type=="DikeCorner");break;
                case "SandbagStack":for(int row=0;row<3;row++)for(int i=0;i<4-row;i++)Ellipsoid(m,new Vector3((i-(3-row)*.5f)*.38f,.12f+row*.18f,0),new Vector3(.23f,.13f,.19f),Straw);break;
                case "Watchtower":Tower(m);break;
                case "RatProofStore":Store(m,false);break;
                case "VillageWarehouse":Store(m,true);break;
                case "RainTank":Cylinder(m,Vector3.zero,.17f,.63f,.63f,Stone);Cylinder(m,Vector3.up*.17f,1.30f,.51f,.51f,Water);Cylinder(m,Vector3.up*1.47f,.18f,.60f,.08f,StoneDark);Beam(m,new Vector3(.45f,.29f,0),new Vector3(.76f,.29f,0),.025f,StoneDark);break;
                case "HeatingLamp":Beam(m,Vector3.zero,Vector3.up*1.22f,.027f,StoneDark);Cylinder(m,Vector3.zero,.05f,.31f,.31f,Stone);Cylinder(m,Vector3.up*1.05f,.28f,.25f,.04f,Red);Ellipsoid(m,Vector3.up*1.02f,new Vector3(.09f,.12f,.09f),Gold);break;
                case "StrawCover":Cover(m,false);break;
                case "PestNet":Cover(m,true);break;
                case "InsectTrap":Trap(m);break;
                case "Campfire":Campfire(m,false);break;
                case "VillageCookpot":Campfire(m,true);break;
                case "RodentTrap":Box(m,new Vector3(0,.04f,0),new Vector3(.45f,.08f,.23f),Wood);Hoop(m,new Vector3(-.1f,.09f,0),Vector3.up,.07f,.012f,StoneDark);Box(m,new Vector3(.13f,.10f,0),new Vector3(.06f,.05f,.05f),Gold);break;
                default:ToolOrDecoration(m,type);break;
            }
            Add(root,"Model",m.Finish(type),Slice(mats),meshes,true);Architecture(root,type,mats,meshes);return root;
        }
        private static void Attachment(GameObject root,Transform joint,string label,EnvironmentMesh m,Material[] mats,List<Mesh> meshes)
        {Add(root,label,m.Finish(label),Slice(mats),meshes,false);root.transform.GetChild(root.transform.childCount-1).SetParent(joint,false);}
        private static GameObject GraftModel(string type,Material[] mats,List<Mesh> meshes)
        {
            var plantMats=CropCollectionBuilder.Materials();GameObject root;Mesh mesh;
            if(type.EndsWith("Branch"))
            {
                var crop=new CropMesh();crop.Tube(new[]{Vector3.zero,new Vector3(.04f,.32f,0)},.012f,.005f,CropMesh.Wood);
                for(int i=0;i<4;i++)crop.Leaf(new Vector3(0,.10f+i*.05f,0),new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f)),.16f,.04f,.3f,CropMesh.Green);
                mesh=crop.Finish(type);root=new GameObject(type,typeof(MeshFilter),typeof(MeshRenderer));root.GetComponent<MeshFilter>().sharedMesh=mesh;root.GetComponent<MeshRenderer>().sharedMaterials=plantMats;
            }
            else root=CompleteCropBuilder.Create(type=="GraftedOrange"?"Orange":type=="ThornLemonRootstock"?"Lemon":"Mango",type.EndsWith("Rootstock")?2:4,plantMats,out mesh);
            root.name=type;meshes.Add(mesh);var m=new EnvironmentMesh(Colors.Length);
            if(type=="ThornLemonRootstock")for(int i=0;i<7;i++)Beam(m,new Vector3(0,.10f+i*.04f,0),new Vector3((i%2==0?1:-1)*.055f,.13f+i*.04f,0),.006f,WoodDark);
            else if(type=="DualFruitTree")for(int i=0;i<5;i++){float a=i*2.4f;Ellipsoid(m,new Vector3(Mathf.Cos(a)*.35f,.86f+i%3*.16f,Mathf.Sin(a)*.35f),Vector3.one*.082f,TileLight);}
            else if(type.StartsWith("Grafted"))Hoop(m,Vector3.up*.38f,Vector3.up,.072f,.016f,Plaster,16);
            else Cylinder(m,Vector3.zero,.015f,.029f,.029f,Plaster,12);
            Add(root,"Grafting detail",m.Finish(type+" detail"),Slice(mats),meshes,false);return root;
        }
        private static Transform Find(GameObject root,string name)
        {foreach(Transform t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;return root.transform;}
        private static GameObject Human(string type,Material[] mats,List<Mesh> meshes)
        {
            if(type=="FarmerFemale"){var female=UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(FarmerFemaleAssets.PrefabPath));female.name=type;return female;}
            var root=UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(FarmerAssets.PrefabPath));root.name=type;
            Find(root,"Non la").gameObject.SetActive(false);Find(root,"Soft hat ties").gameObject.SetActive(false);
            Transform head=Find(root,"Outfit head");var details=new EnvironmentMesh(Colors.Length);
            if(type=="ElderTu")
            {
                Cylinder(details,new Vector3(0,.22f,-.01f),.08f,.25f,.22f,Cloth,24);
                for(int side=-1;side<=1;side+=2){Hoop(details,new Vector3(side*.086f,.022f,.214f),Vector3.forward,.055f,.005f,StoneDark,20);Beam(details,new Vector3(side*.14f,.02f,.21f),new Vector3(side*.21f,.015f,.02f),.004f,StoneDark);}
                Beam(details,new Vector3(-.03f,.026f,.219f),new Vector3(.03f,.026f,.219f),.004f,StoneDark);
                Ellipsoid(details,new Vector3(0,-.18f,.15f),new Vector3(.10f,.13f,.06f),Plaster);Ellipsoid(details,new Vector3(0,-.10f,.206f),new Vector3(.083f,.025f,.017f),Plaster);
                var cane=new EnvironmentMesh(Colors.Length);Beam(cane,new Vector3(.34f,0,.10f),new Vector3(.34f,.87f,.10f),.019f);Beam(cane,new Vector3(.27f,.87f,.10f),new Vector3(.37f,.87f,.10f),.022f);Attachment(root,root.transform,"Walking cane",cane,mats,meshes);
            }
            else if(type=="MerchantHaiAu")
            {
                Cylinder(details,new Vector3(0,.19f,0),.13f,.23f,.21f,Straw,24);Ellipsoid(details,new Vector3(0,.195f,.14f),new Vector3(.23f,.03f,.18f),Wood);
                var bag=new EnvironmentMesh(Colors.Length);Box(bag,new Vector3(.26f,.70f,-.05f),new Vector3(.22f,.27f,.16f),WoodDark);Beam(bag,new Vector3(-.15f,1.02f,.13f),new Vector3(.27f,.77f,.03f),.016f,Wood);Attachment(root,root.transform,"Merchant satchel",bag,mats,meshes);
            }
            else
            {
                Ellipsoid(details,new Vector3(0,.22f,-.012f),new Vector3(.18f,.065f,.16f),Dark);
                for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)Ellipsoid(details,new Vector3(side*.19f,-.10f-i*.06f,-.09f),new Vector3(.041f,.048f,.04f),Dark);
                for(int side=-1;side<=1;side+=2)Hoop(details,new Vector3(side*.24f,-.07f,.012f),Vector3.forward,.026f,.004f,Gold,16);
                Ellipsoid(details,new Vector3(.12f,.20f,.11f),new Vector3(.067f,.018f,.035f),Red);
            }
            Attachment(root,head,"Character details",details,mats,meshes);return root;
        }
        private static void Creature(GameObject root,EnvironmentMesh m,string type,Material[] mats,List<Mesh> meshes)
        {
            bool firefly=type=="GoldenFirefly",bee=type=="Bee",lady=type=="Ladybug",snail=type=="Snail",mouse=type=="FieldMouse",myna=type=="Myna",worm=type=="FruitBorer";
            int skin=bee?Gold:firefly?Black:lady?Red:mouse||snail?Wood:type=="Planthopper"?WoodDark:type=="Locust"?Cloth:Black;
            if(snail)
            {
                Ellipsoid(m,new Vector3(0,.055f,.02f),new Vector3(.12f,.055f,.24f),Wood);Ellipsoid(m,new Vector3(0,.17f,-.045f),new Vector3(.16f,.16f,.105f),Straw);
                var path=new Vector3[45];for(int i=0;i<path.Length;i++){float a=i*.20f,r=.01f+i*.0028f;path[i]=new Vector3(Mathf.Cos(a)*r,.17f+Mathf.Sin(a)*r,.065f);}m.Tube(path,.012f,.009f,WoodDark,6);
            }
            else if(worm)
            {for(int i=0;i<9;i++)Ellipsoid(m,new Vector3(Mathf.Sin(i*.6f)*.035f,.048f,-.18f+i*.04f),new Vector3(.051f,.04f,.04f),i==8?WoodDark:Cloth);}
            else
            {
                Ellipsoid(m,new Vector3(0,.14f,0),new Vector3(mouse?.095f:myna?.12f:.10f,myna?.15f:.075f,mouse?.18f:.135f),skin);
                Ellipsoid(m,new Vector3(0,myna?.28f:.14f,.14f),new Vector3(myna?.082f:.07f,myna?.08f:.063f,.070f),skin);
                if(bee)for(int i=0;i<3;i++)Hoop(m,new Vector3(0,.14f,-.07f+i*.055f),Vector3.forward,.075f,.015f,Black,18);
                if(firefly){Ellipsoid(m,new Vector3(0,.145f,-.105f),new Vector3(.085f,.07f,.065f),Gold);for(int side=-1;side<=1;side+=2)Ellipsoid(m,new Vector3(side*.058f,.195f,.13f),new Vector3(.017f,.016f,.025f),Plaster);}
                if(lady)for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++)Ellipsoid(m,new Vector3(side*.055f,.203f,-.06f+i*.053f),Vector3.one*.013f,Black);
                if(mouse)
                {for(int side=-1;side<=1;side+=2)Ellipsoid(m,new Vector3(side*.061f,.205f,.13f),new Vector3(.043f,.046f,.019f),Red);m.Tube(new[]{new Vector3(0,.09f,-.17f),new Vector3(.12f,.055f,-.28f),new Vector3(.17f,.04f,-.38f)},.014f,.004f,Straw,8);}
                if(myna){Beam(m,new Vector3(0,.27f,.19f),new Vector3(0,.25f,.27f),.021f,Gold);for(int side=-1;side<=1;side+=2)Ellipsoid(m,new Vector3(side*.066f,.30f,.17f),new Vector3(.018f,.025f,.012f),Gold);}
                if(!mouse)for(int side=-1;side<=1;side+=2)for(int leg=0;leg<(myna?1:3);leg++){Vector3 p=new Vector3(side*.08f,.12f,-.07f+leg*.07f);m.Tube(new[]{p,p+new Vector3(side*.07f,-.04f,.02f),p+new Vector3(side*.09f,-.12f,.04f)},.007f,.003f,WoodDark,6);}
                else for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Ellipsoid(m,new Vector3(x*.07f,.028f,z*.09f),new Vector3(.037f,.03f,.048f),WoodDark);
            }
            for(int side=-1;side<=1;side+=2)
            {Vector3 p=new Vector3(side*.043f,myna?.31f:.16f,snail?.20f:.18f);if(snail)m.Tube(new[]{p,p+new Vector3(side*.02f,.10f,.04f)},.005f,.003f,WoodDark,6);Ellipsoid(m,p+(snail?Vector3.up*.10f:Vector3.zero),Vector3.one*.010f,Black);}
            var wings=new List<Transform>();bool flies=bee||firefly||type=="Locust"||type=="Planthopper"||myna;
            if(flies)for(int side=-1;side<=1;side+=2)
            {
                var w=new EnvironmentMesh(Colors.Length);Ellipsoid(w,new Vector3(side*.09f,0,-.02f),new Vector3(.115f,.012f,myna?.14f:.065f),myna?Black:Water);
                Add(root,side<0?"Left wing":"Right wing",w.Finish(type+" wing"),Slice(mats),meshes,false);Transform part=root.transform.GetChild(root.transform.childCount-1);part.localPosition=new Vector3(side*.075f,myna?.18f:.17f,0);wings.Add(part);
            }
            root.AddComponent<AmbientCreatureMotion>().Configure(wings.ToArray(),firefly||bee);
        }
        private static void Vegetation(EnvironmentMesh m,string type)
        {
            if(type=="BroadleafTree"||type=="DeadTree")
            {
                Cylinder(m,Vector3.zero,1.8f,.17f,.06f,WoodDark,12);
                for(int i=0;i<7;i++){Vector3 d=new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f)),p=new Vector3(0,.8f+i%3*.3f,0),tip=p+d*.65f+Vector3.up*.6f;Beam(m,p,tip,.04f,Wood);if(type!="DeadTree")for(int j=0;j<3;j++)Ellipsoid(m,tip+new Vector3((j-1)*.24f,j%2*.15f,0),new Vector3(.47f,.34f,.41f),j==1?Cloth:StoneDark);}
            }
            else if(type=="BambooClump")for(int stem=0;stem<6;stem++)
            {Vector3 p=new Vector3(Mathf.Cos(stem*2.4f)*.2f,0,Mathf.Sin(stem*2.4f)*.2f);float h=1.55f+stem%3*.2f;Cylinder(m,p,h,.032f,.018f,Bamboo,10);for(int j=1;j<7;j++){Vector3 n=p+Vector3.up*j*h/7;Hoop(m,n,Vector3.up,.031f,.006f,WoodDark,12);if(j>3)for(int k=0;k<4;k++)m.Leaf(n,new Vector3(Mathf.Cos(k*2.4f+stem),0,Mathf.Sin(k*2.4f+stem)),.24f,.027f,Cloth);}}
            else if(type=="Shrub")for(int i=0;i<7;i++)Ellipsoid(m,new Vector3(Mathf.Cos(i*2.4f)*.22f,.28f+i%2*.07f,Mathf.Sin(i*2.4f)*.22f),new Vector3(.28f,.25f,.24f),i%2==0?Cloth:StoneDark);
            else if(type=="Reeds")for(int i=0;i<13;i++){Vector3 p=new Vector3(Mathf.Cos(i*2.4f)*.23f,0,Mathf.Sin(i*2.4f)*.23f);Beam(m,p,p+Vector3.up*(.65f+i%3*.1f),.009f,Cloth);Ellipsoid(m,p+Vector3.up*(.63f+i%3*.1f),new Vector3(.028f,.1f,.028f),WoodDark);m.Leaf(p+Vector3.up*.06f,new Vector3(Mathf.Cos(i),0,Mathf.Sin(i)),.39f,.02f,Cloth);}
            else if(type=="Fern")for(int i=0;i<9;i++){Vector3 d=new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f)),tip=d*.46f+Vector3.up*.23f;Beam(m,Vector3.zero,tip,.008f,Cloth);for(int j=1;j<7;j++)for(int side=-1;side<=1;side+=2)m.Leaf(tip*j/7,Vector3.Cross(Vector3.up,d)*side+d*.3f,.12f*Mathf.Sin(j*Mathf.PI/8),.023f,Cloth);}
            else for(int i=0;i<11;i++){Vector3 p=new Vector3(Mathf.Cos(i*2.4f)*.25f,.16f+i%3*.04f,Mathf.Sin(i*2.4f)*.25f);Beam(m,new Vector3(p.x,0,p.z),p,.006f,Cloth);for(int petal=0;petal<6;petal++){float a=petal*Mathf.PI/3;Ellipsoid(m,p+new Vector3(Mathf.Cos(a)*.032f,0,Mathf.Sin(a)*.032f),new Vector3(.025f,.009f,.02f),i%2==0?Gold:Plaster);}}
        }
        private static void MountainExtra(EnvironmentMesh m,bool volcano)
        {
            for(int i=0;i<18;i++)
            {float a=i*Mathf.PI/9,b=(i+1)*Mathf.PI/9;Vector3 p=new Vector3(Mathf.Cos(a)*1.6f,0,Mathf.Sin(a)*1.4f),q=new Vector3(Mathf.Cos(b)*1.6f,0,Mathf.Sin(b)*1.4f),r=new Vector3(Mathf.Cos(a)*.40f,1.7f,Mathf.Sin(a)*.37f),s=new Vector3(Mathf.Cos(b)*.4f,1.7f,Mathf.Sin(b)*.37f);Face(m,p,q,s,r,i%3==0?StoneDark:Stone);
                if(volcano)Face(m,r,s,new Vector3(s.x*.65f,1.49f,s.z*.65f),new Vector3(r.x*.65f,1.49f,r.z*.65f),Black);
                else Face(m,r,s,new Vector3(0,2.34f,0),new Vector3(0,2.34f,0),Plaster);}
            if(volcano)m.Disc(new Vector3(0,1.50f,0),.28f,.26f,Red,24);
        }
        private static void NatureProp(EnvironmentMesh m,string type)
        {
            if(type=="FallenCoconut"){Ellipsoid(m,new Vector3(0,.12f,0),new Vector3(.15f,.12f,.13f),Straw);for(int i=0;i<3;i++)Ellipsoid(m,new Vector3((i-1)*.025f,.204f,.085f),Vector3.one*.012f,Dark);return;}
            if(type=="BeachShell"){Ellipsoid(m,new Vector3(0,.025f,0),new Vector3(.14f,.04f,.10f),Plaster);for(int i=0;i<8;i++)Beam(m,new Vector3(0,.062f,-.05f),new Vector3((i-3.5f)*.025f,.06f,.07f),.003f,Straw);return;}
            if(type=="TreeStump"){Cylinder(m,Vector3.zero,.45f,.29f,.23f,WoodDark,12);m.Disc(Vector3.up*.452f,.23f,.23f,Wood,20);for(int i=0;i<3;i++)Hoop(m,Vector3.up*.454f,Vector3.up,.045f+i*.058f,.004f,WoodDark,20);return;}
            m.Tube(new[]{new Vector3(-.60f,.14f,0),new Vector3(.10f,.18f,0),new Vector3(.63f,.12f,.02f)},.14f,.10f,WoodDark,12);Beam(m,new Vector3(.14f,.18f,0),new Vector3(.20f,.48f,-.11f),.048f,Wood);
            if(type=="Driftwood")for(int i=0;i<4;i++)Beam(m,new Vector3(-.48f,.23f+i*.017f,.045f),new Vector3(.54f,.20f+i*.017f,.055f),.005f,Plaster);
        }
        private static void Channel(EnvironmentMesh m,string type)
        {
            bool corner=type=="DrainCorner",junction=type=="DrainJunction";ChannelArm(m,Vector3.zero,false,1.8f,type=="IrrigationCanal");if(corner||junction)ChannelArm(m,new Vector3(0,0,.45f),true,junction?1.8f:.90f,false);
        }
        private static void ChannelArm(EnvironmentMesh m,Vector3 p,bool turn,float length,bool water)
        {
            Vector3 size=turn?new Vector3(.48f,.045f,length):new Vector3(length,.045f,.48f);Box(m,p+Vector3.up*.024f,size,water?Water:Dark);
            for(int side=-1;side<=1;side+=2)Box(m,p+(turn?new Vector3(side*.29f,.13f,0):new Vector3(0,.13f,side*.29f)),turn?new Vector3(.10f,.26f,length):new Vector3(length,.26f,.10f),Stone);
        }
        private static void Dike(EnvironmentMesh m,bool corner)
        {
            Box(m,new Vector3(0,.28f,0),new Vector3(2.0f,.56f,.76f),Stone);Box(m,new Vector3(0,.61f,0),new Vector3(2.05f,.12f,.57f),Wood);
            for(int i=0;i<8;i++){Beam(m,new Vector3(-.87f+i*.25f,.16f,.388f),new Vector3(-.87f+i*.25f,.53f,.388f),.035f,StoneDark);}
            if(corner)Box(m,new Vector3(.65f,.28f,.57f),new Vector3(.70f,.56f,1.10f),Stone);
        }
        private static void Tower(EnvironmentMesh m)
        {
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){Beam(m,new Vector3(x*.61f,0,z*.57f),new Vector3(x*.48f,2.05f,z*.44f),.055f);}
            Box(m,Vector3.up*1.72f,new Vector3(1.22f,.13f,1.14f),Wood);Roof(m,1.62f,1.50f,2.49f,.37f,true);
            for(int side=-1;side<=1;side+=2)Fence(m,new Vector3(side*.57f,1.78f,0),true,1.10f,.49f);Fence(m,new Vector3(0,1.78f,-.52f),false,1.1f,.49f);
            for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.24f,0,.94f),new Vector3(side*.24f,1.85f,.48f),.025f);for(int i=0;i<8;i++)Beam(m,new Vector3(-.24f,i*.22f,.94f-i*.055f),new Vector3(.24f,i*.22f,.94f-i*.055f),.025f,Wood);
        }
        private static void Store(EnvironmentMesh m,bool village)
        {
            float width=village?3.2f:1.9f,depth=village?2.3f:1.65f,y=village?.17f:.43f;
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){Vector3 p=new Vector3(x*(width*.44f),0,z*(depth*.43f));Cylinder(m,p,y,.095f,.095f,StoneDark,12);m.Disc(p+Vector3.up*(y-.06f),.21f,.21f,Stone,16);}
            Box(m,new Vector3(0,y,0),new Vector3(width,.15f,depth),Stone);Box(m,new Vector3(0,y+.85f,0),new Vector3(width-.10f,1.62f,depth-.16f),Wood);
            Roof(m,width+.28f,depth+.30f,y+1.70f,.40f,true);Box(m,new Vector3(0,y+.68f,depth*.5f-.065f),new Vector3(.74f,1.3f,.07f),WoodDark);
            Beam(m,new Vector3(-.32f,y+.14f,depth*.5f),new Vector3(.32f,y+1.23f,depth*.5f),.032f,Plaster);
            if(village){Crate(m,new Vector3(-1.09f,y,depth*.5f+.29f));Sack(m,new Vector3(1.09f,y,depth*.5f+.24f));}
        }
        private static void Cover(EnvironmentMesh m,bool net)
        {
            for(int i=0;i<5;i++){float z=-.52f+i*.26f;var arc=new Vector3[9];for(int j=0;j<9;j++){float a=j*Mathf.PI/8;arc[j]=new Vector3(Mathf.Cos(a)*.57f,Mathf.Sin(a)*.46f,z);}m.Tube(arc,.009f,.009f,net?StoneDark:Bamboo,6);}
            if(net)for(int i=0;i<15;i++){float a=i*Mathf.PI/14;Beam(m,new Vector3(Mathf.Cos(a)*.57f,Mathf.Sin(a)*.46f,-.52f),new Vector3(Mathf.Cos(a)*.57f,Mathf.Sin(a)*.46f,.52f),.004f,Plaster);}
            else Roof(m,1.24f,1.22f,.26f,.30f,false);
        }
        private static void Trap(EnvironmentMesh m)
        {Cylinder(m,Vector3.zero,.08f,.20f,.20f,Stone);Beam(m,Vector3.up*.08f,Vector3.up*1.28f,.022f,StoneDark);Cylinder(m,Vector3.up*.90f,.27f,.13f,.13f,Gold,12);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Beam(m,new Vector3(Mathf.Cos(a)*.17f,.85f,Mathf.Sin(a)*.17f),new Vector3(Mathf.Cos(a)*.17f,1.20f,Mathf.Sin(a)*.17f),.01f,StoneDark);}Cylinder(m,Vector3.up*1.20f,.14f,.25f,.01f,Tile,16);}
        private static void Campfire(EnvironmentMesh m,bool pot)
        {
            for(int i=0;i<10;i++){float a=i*Mathf.PI/5;Ellipsoid(m,new Vector3(Mathf.Cos(a)*.34f,.09f,Mathf.Sin(a)*.34f),new Vector3(.12f,.09f,.10f),Stone);}
            for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Vector3 d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));Beam(m,-d*.22f+Vector3.up*.07f,d*.22f+Vector3.up*.13f,.038f,WoodDark);}
            for(int i=0;i<5;i++)Cylinder(m,new Vector3(Mathf.Cos(i*2.4f)*.10f,.10f,Mathf.Sin(i*2.4f)*.10f),.24f+i%2*.09f,.055f,.002f,i%2==0?Gold:Red,8);
            if(pot){for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.49f,0,0),new Vector3(side*.49f,.78f,0),.028f);Beam(m,new Vector3(-.5f,.77f,0),new Vector3(.5f,.77f,0),.025f);Cylinder(m,Vector3.up*.37f,.26f,.24f,.32f,StoneDark,20);m.Disc(Vector3.up*.635f,.28f,.28f,Gold,20);Hoop(m,Vector3.up*.68f,Vector3.forward,.19f,.014f,StoneDark,20);}
        }
        private static void ToolOrDecoration(EnvironmentMesh m,string type)
        {
            switch(type)
            {
                case "Hoe":case "Shovel":case "Axe":case "Hammer":
                    Beam(m,new Vector3(0,.04f,0),new Vector3(.04f,1.0f,0),.020f,Wood);
                    Box(m,new Vector3(.045f,type=="Shovel"?.17f:.94f,.035f),type=="Shovel"?new Vector3(.23f,.31f,.035f):type=="Hoe"?new Vector3(.28f,.07f,.18f):type=="Axe"?new Vector3(.28f,.19f,.045f):new Vector3(.23f,.12f,.10f),StoneDark);break;
                case "Sickle":Beam(m,new Vector3(0,0,0),Vector3.up*.40f,.027f,Wood);var blade=new Vector3[13];for(int i=0;i<13;i++){float a=i*Mathf.PI/12;blade[i]=new Vector3(.13f+Mathf.Cos(a)*.13f,.42f+Mathf.Sin(a)*.20f,0);}m.Tube(blade,.017f,.003f,StoneDark,6);break;
                case "WateringCan":Cylinder(m,Vector3.zero,.30f,.17f,.17f,Cloth,16);Beam(m,new Vector3(.15f,.15f,0),new Vector3(.39f,.36f,0),.034f,Cloth);Hoop(m,new Vector3(-.18f,.18f,0),Vector3.forward,.12f,.022f,WoodDark);Cylinder(m,new Vector3(.40f,.35f,0),.04f,.075f,.075f,StoneDark,12);break;
                case "Bucket":case "Bailer":Cylinder(m,Vector3.zero,.27f,.14f,.19f,Wood,16);m.Disc(Vector3.up*.274f,.165f,.165f,Dark,16);Hoop(m,Vector3.up*.32f,Vector3.forward,.15f,.012f,StoneDark,16);if(type=="Bailer")Beam(m,new Vector3(.12f,.15f,0),new Vector3(.46f,.52f,0),.022f,Wood);break;
                case "FishingRod":Beam(m,Vector3.zero,new Vector3(.2f,1.32f,0),.016f,Bamboo);m.Tube(new[]{new Vector3(.2f,1.32f,0),new Vector3(.39f,.70f,0),new Vector3(.42f,.13f,0)},.003f,.003f,Plaster,4);Ellipsoid(m,new Vector3(.40f,.45f,0),new Vector3(.022f,.065f,.022f),Red);break;
                case "FishingNet":Beam(m,Vector3.zero,Vector3.up*.70f,.02f,Bamboo);Hoop(m,Vector3.up*.95f,Vector3.forward,.23f,.018f,Bamboo);for(int i=0;i<7;i++){float x=(i-3)*.055f,y=Mathf.Sqrt(Mathf.Max(0,.23f*.23f-x*x));Beam(m,new Vector3(x,.95f-y,0),new Vector3(x,.95f+y,0),.003f,Plaster);Beam(m,new Vector3(-y,.95f+x,0),new Vector3(y,.95f+x,0),.003f,Plaster);}break;
                case "Basket":Cylinder(m,Vector3.zero,.24f,.21f,.27f,Bamboo,20);m.Disc(Vector3.up*.245f,.23f,.23f,WoodDark,20);Hoop(m,Vector3.up*.31f,Vector3.forward,.23f,.015f,Wood);for(int i=0;i<5;i++)Hoop(m,Vector3.up*(.03f+i*.048f),Vector3.up,.22f+i*.009f,.006f,Wood,20);break;
                case "SeedBag":case "FertilizerBag":case "FeedBag":Sack(m,Vector3.zero);Box(m,new Vector3(0,.28f,.214f),new Vector3(.20f,.20f,.025f),type=="SeedBag"?Cloth:type=="FeedBag"?Gold:Plaster);break;
                case "HerbSprayer":Cylinder(m,Vector3.zero,.33f,.12f,.12f,Cloth,16);Beam(m,Vector3.up*.33f,new Vector3(.15f,.37f,0),.02f,StoneDark);m.Tube(new[]{new Vector3(.14f,.35f,0),new Vector3(.30f,.23f,.01f),new Vector3(.32f,.42f,0)},.012f,.012f,Dark,8);break;
                case "MedicineBottle":Cylinder(m,Vector3.zero,.27f,.10f,.10f,Water,16);Cylinder(m,Vector3.up*.27f,.07f,.058f,.058f,TileDark,12);Box(m,new Vector3(0,.15f,.10f),new Vector3(.12f,.12f,.013f),Plaster);break;
                case "NailBundle":for(int i=0;i<6;i++){Beam(m,new Vector3((i%3)*.04f,0,i/3*.04f),new Vector3((i%3)*.04f,.18f,i/3*.04f),.008f,StoneDark);Cylinder(m,new Vector3(i%3*.04f,.18f,i/3*.04f),.01f,.017f,.017f,StoneDark,8);}break;
                case "WoodBundle":for(int i=0;i<5;i++)Beam(m,new Vector3(-.3f,.04f+i*.045f,0),new Vector3(.3f,.04f+i*.045f,0),.036f,Wood);Hoop(m,Vector3.up*.13f,Vector3.right,.12f,.01f,Straw);break;
                case "StonePile":for(int i=0;i<7;i++)Ellipsoid(m,new Vector3(Mathf.Cos(i*2.4f)*.15f,.10f+i%2*.08f,Mathf.Sin(i*2.4f)*.14f),new Vector3(.12f,.10f,.10f),i%2==0?Stone:StoneDark);break;
                case "RopeCoil":for(int i=0;i<5;i++)Hoop(m,Vector3.up*(.016f+i*.012f),Vector3.up,.09f+i*.035f,.012f,Straw,24);break;
                case "Journal":case "MapScroll":Box(m,new Vector3(0,.034f,0),new Vector3(.36f,.067f,.47f),type=="Journal"?WoodDark:Plaster);Box(m,new Vector3(0,.074f,0),new Vector3(.28f,.012f,.39f),Straw);for(int i=0;i<4;i++)Box(m,new Vector3(0,.082f,-.12f+i*.07f),new Vector3(.18f,.008f,.006f),Wood);break;
                case "Radio":Box(m,new Vector3(0,.16f,0),new Vector3(.53f,.32f,.19f),WoodDark);for(int i=0;i<6;i++)Box(m,new Vector3(-.10f,.16f,.10f),new Vector3(.26f,.012f,.010f),Straw);Ellipsoid(m,new Vector3(.16f,.12f,.106f),new Vector3(.035f,.035f,.017f),Gold);Beam(m,new Vector3(.19f,.32f,0),new Vector3(.23f,.65f,0),.004f,StoneDark);break;
                case "Pearl":Ellipsoid(m,Vector3.up*.075f,Vector3.one*.075f,Plaster);break;
                case "GiftBox":Box(m,Vector3.up*.18f,new Vector3(.36f,.36f,.36f),Red);Box(m,new Vector3(0,.19f,.185f),new Vector3(.046f,.37f,.018f),Gold);Box(m,new Vector3(0,.37f,0),new Vector3(.046f,.02f,.36f),Gold);Hoop(m,new Vector3(-.07f,.40f,0),Vector3.forward,.06f,.012f,Gold,16);Hoop(m,new Vector3(.07f,.40f,0),Vector3.forward,.06f,.012f,Gold,16);break;
                case "OrderBoard":for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.35f,0,0),new Vector3(side*.35f,1.10f,0),.035f);Box(m,Vector3.up*.86f,new Vector3(.91f,.56f,.07f),Wood);for(int i=0;i<3;i++)Box(m,new Vector3((i-1)*.26f,.86f,.045f),new Vector3(.19f,.30f,.018f),Plaster);break;
                case "FestivalLantern":Cylinder(m,Vector3.up*.10f,.33f,.16f,.16f,Red,16);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Beam(m,new Vector3(Mathf.Cos(a)*.164f,.1f,Mathf.Sin(a)*.164f),new Vector3(Mathf.Cos(a)*.164f,.43f,Mathf.Sin(a)*.164f),.005f,Gold);}Beam(m,Vector3.zero,Vector3.up*.1f,.014f,Gold);break;
                case "Pinwheel":Beam(m,Vector3.zero,Vector3.up*.83f,.019f,Bamboo);for(int i=0;i<4;i++){Quaternion q=Quaternion.AngleAxis(i*90,Vector3.forward);Vector3 p=Vector3.up*.83f;Face(m,p,p+q*new Vector3(.18f,.17f,0),p+q*new Vector3(-.02f,.26f,0),p,i%2==0?Red:Gold);}break;
                case "Bench":Box(m,Vector3.up*.42f,new Vector3(1.15f,.10f,.39f),Wood);Box(m,new Vector3(0,.67f,-.18f),new Vector3(1.15f,.34f,.06f),Wood);for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.43f,0,0),new Vector3(side*.43f,.44f,0),.045f);break;
                case "PathTile":case "DirtPath":case "SteppingStones":
                    if(type=="SteppingStones")for(int i=0;i<4;i++)Ellipsoid(m,new Vector3((i%2-.5f)*.44f,.055f,(i/2-.5f)*.45f),new Vector3(.22f,.055f,.18f),Stone);
                    else{Box(m,Vector3.up*.025f,new Vector3(1.2f,.05f,1.2f),type=="DirtPath"?Wood:Stone);if(type=="PathTile")for(int i=0;i<4;i++)Box(m,new Vector3((i%2-.5f)*.58f,.053f,(i/2-.5f)*.58f),new Vector3(.53f,.012f,.53f),i%2==0?StoneDark:Stone);}break;
                case "Toolbox":Box(m,Vector3.up*.16f,new Vector3(.58f,.32f,.29f),WoodDark);Hoop(m,Vector3.up*.38f,Vector3.forward,.14f,.023f,Wood);Box(m,new Vector3(0,.20f,.15f),new Vector3(.06f,.06f,.018f),Gold);break;
                default:throw new System.ArgumentException("Unknown extra model "+type);
            }
        }
        private static void Product(EnvironmentMesh m,string name)
        {
            switch(name)
            {
                case "Carrot":case "Radish":m.Tube(new[]{new Vector3(0,.02f,0),new Vector3(0,.27f,0)},.012f,.080f,name=="Carrot"?TileLight:Plaster,16);for(int i=0;i<4;i++)m.Leaf(Vector3.up*.27f,new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f)),.14f,.03f,Cloth);break;
                case "Rice":case "Wheat":for(int i=0;i<9;i++){Vector3 p=new Vector3((i%3-1)*.05f,0,(i/3-1)*.05f);Beam(m,p,p+Vector3.up*.25f,.006f,Straw);for(int j=0;j<4;j++)Ellipsoid(m,p+new Vector3(.016f,.19f+j*.035f,0),new Vector3(.020f,.025f,.012f),Gold);}break;
                case "Corn":Ellipsoid(m,Vector3.up*.18f,new Vector3(.082f,.18f,.082f),Gold);for(int y=0;y<6;y++)for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Ellipsoid(m,new Vector3(Mathf.Cos(a)*.08f,.055f+y*.044f,Mathf.Sin(a)*.08f),new Vector3(.018f,.02f,.018f),Straw);}break;
                case "Pumpkin":Ellipsoid(m,Vector3.up*.14f,new Vector3(.18f,.14f,.18f),TileLight);for(int i=0;i<10;i++){float a=i*Mathf.PI/5;Beam(m,new Vector3(Mathf.Cos(a)*.17f,.16f,Mathf.Sin(a)*.17f),new Vector3(Mathf.Cos(a)*.10f,.26f,Mathf.Sin(a)*.10f),.007f,Tile);}Beam(m,Vector3.up*.26f,new Vector3(.02f,.34f,0),.020f,WoodDark);break;
                case "Watermelon":Ellipsoid(m,Vector3.up*.14f,new Vector3(.22f,.14f,.16f),Cloth);for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Beam(m,new Vector3(Mathf.Cos(a)*.205f,.14f,Mathf.Sin(a)*.15f),new Vector3(Mathf.Cos(a)*.12f,.255f,Mathf.Sin(a)*.08f),.014f,StoneDark);}break;
                case "Tomato":case "Orange":case "Lemon":case "Mango":case "Papaya":case "Coconut":case "Onion":case "Garlic":case "Taro":
                    int c=name=="Tomato"?Red:name=="Orange"||name=="Papaya"?TileLight:name=="Lemon"||name=="Mango"?Gold:name=="Coconut"||name=="Taro"?Wood:name=="Onion"?Straw:Plaster;
                    Ellipsoid(m,Vector3.up*.14f,name=="Mango"||name=="Papaya"?new Vector3(.10f,.14f,.09f):Vector3.one*.13f,c);Beam(m,Vector3.up*.26f,Vector3.up*.31f,.009f,Cloth);if(name=="Garlic")for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;Ellipsoid(m,new Vector3(Mathf.Cos(a)*.07f,.10f,Mathf.Sin(a)*.07f),new Vector3(.046f,.1f,.046f),Plaster);}break;
                case "Strawberry":Cylinder(m,Vector3.zero,.20f,.025f,.10f,Red,16);for(int i=0;i<12;i++){float a=i*2.4f;Ellipsoid(m,new Vector3(Mathf.Cos(a)*.074f,.06f+i%3*.055f,Mathf.Sin(a)*.074f),Vector3.one*.006f,Gold);}for(int i=0;i<5;i++)m.Leaf(Vector3.up*.20f,new Vector3(Mathf.Cos(i),0,Mathf.Sin(i)),.06f,.023f,Cloth);break;
                case "Chili":case "Banana":for(int i=0;i<(name=="Banana"?5:3);i++)m.Tube(new[]{new Vector3(i*.028f,.02f,0),new Vector3(i*.028f+.09f,.10f,0),new Vector3(i*.028f+.08f,.24f,0)},.033f,.010f,name=="Chili"?Red:Gold,12);break;
                case "Beans":case "Peas":for(int i=0;i<4;i++)Ellipsoid(m,new Vector3((i-1.5f)*.05f,.04f,0),new Vector3(.031f,.036f,.15f),Cloth);break;
                case "Mushroom":Cylinder(m,Vector3.zero,.16f,.032f,.025f,Plaster,12);Ellipsoid(m,Vector3.up*.16f,new Vector3(.12f,.06f,.12f),Wood);break;
                case "Lotus":Cylinder(m,Vector3.zero,.17f,.025f,.025f,Cloth,12);for(int i=0;i<10;i++)m.Leaf(Vector3.up*.17f,new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f)),.14f,.047f,i%2==0?Red:Plaster);break;
                case "Marigold":for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Ellipsoid(m,new Vector3(Mathf.Cos(a)*.066f,.11f,Mathf.Sin(a)*.066f),new Vector3(.045f,.035f,.033f),i%2==0?Gold:TileLight);}break;
                case "Greens":case "WaterSpinach":case "Cabbage":case "SnowTea":case "TeaLeaves":case "Lemongrass":case "Sugarcane":
                    for(int i=0;i<8;i++){Vector3 p=new Vector3(Mathf.Cos(i*2.4f)*.04f,.02f,Mathf.Sin(i*2.4f)*.04f);if(name=="Sugarcane")Cylinder(m,p,.32f,.018f,.018f,Bamboo,8);else m.Leaf(p,new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f)),name=="Lemongrass"?.35f:.19f,name=="Lemongrass"?.016f:.065f,Cloth);}if(name=="Cabbage")Ellipsoid(m,Vector3.up*.10f,Vector3.one*.10f,Cloth);break;
                case "Egg":Ellipsoid(m,Vector3.up*.10f,new Vector3(.065f,.10f,.065f),Plaster);break;
                case "Milk":case "SoySauce":case "HerbalPotion":case "Honey":Jar(m,Vector3.zero,.65f,name=="Milk"?Plaster:name=="Honey"?Gold:name=="HerbalPotion"?Cloth:WoodDark);Box(m,new Vector3(0,.15f,.152f),new Vector3(.12f,.12f,.025f),Plaster);break;
                case "Flour":case "RiceBag":case "Feed":case "Compost":Sack(m,Vector3.zero,.7f);Box(m,new Vector3(0,.2f,.15f),new Vector3(.16f,.14f,.020f),name=="Flour"?Plaster:name=="Compost"?Cloth:Gold);break;
                case "Bread":case "Sandwich":Ellipsoid(m,Vector3.up*.09f,new Vector3(.24f,.09f,.085f),Straw);if(name=="Sandwich"){Box(m,new Vector3(0,.09f,.08f),new Vector3(.37f,.035f,.04f),Cloth);Box(m,new Vector3(0,.11f,.095f),new Vector3(.33f,.022f,.03f),Red);}else for(int i=0;i<4;i++)Beam(m,new Vector3(-.15f+i*.10f,.17f,-.05f),new Vector3(-.13f+i*.10f,.17f,.05f),.008f,Wood);break;
                case "Cheese":case "Butter":Box(m,Vector3.up*.075f,new Vector3(.30f,.15f,.21f),Gold);if(name=="Cheese")for(int i=0;i<4;i++)Ellipsoid(m,new Vector3((i-1.5f)*.065f,.09f,.108f),new Vector3(.022f,.023f,.005f),Straw);break;
                case "FishBasket":ToolOrDecoration(m,"Basket");for(int i=0;i<3;i++){Vector3 p=new Vector3((i-1)*.095f,.28f,0);Ellipsoid(m,p,new Vector3(.052f,.03f,.17f),Water);Beam(m,p+new Vector3(0,0,-.12f),p+new Vector3(.025f,0,-.21f),.025f,StoneDark);}break;
                default:throw new System.ArgumentException("Unknown product "+name);
            }
        }
        internal static void AddUpgrade(GameObject root,string type,int level,Material[] mats,List<Mesh> meshes)
        {
            if(level==2)return;
            var m=new EnvironmentMesh(Colors.Length);var rs=root.GetComponentsInChildren<Renderer>();Bounds b=rs[0].bounds;foreach(Renderer r in rs)b.Encapsulate(r.bounds);
            if(level==1)
            {
                // A small manual attachment distinguishes the starter variant without enlarging its footprint.
                Box(m,new Vector3(b.min.x+.12f,.06f,b.max.z+.07f),new Vector3(.28f,.12f,.20f),WoodDark);Beam(m,new Vector3(b.min.x+.12f,.10f,b.max.z+.07f),new Vector3(b.min.x+.12f,.36f,b.max.z+.07f),.023f,Wood);
                root.transform.localScale=Vector3.one*.82f;
            }
            else
            {
                Vector3 p=new Vector3(b.max.x+.25f,0,b.min.z+.23f);Crate(m,p);Sack(m,p+new Vector3(0,0,.57f),.72f);
                for(int side=-1;side<=1;side+=2)Beam(m,p+new Vector3(side*.40f,0,-.11f),p+new Vector3(side*.40f,1.02f,-.11f),.034f);
                Roof(m,1.03f,1.20f,1.04f,.18f,true,p+new Vector3(0,0,.30f));HangingLantern(m,p+new Vector3(.34f,.95f,.62f));
            }
            Add(root,"Upgrade equipment",m.Finish(type+" L"+level+" equipment"),Slice(mats),meshes,true);
        }
        internal static GameObject CreateExpandedLevel(string type,int level,Material[] mats,List<Mesh> meshes)
        {
            if(level>1)
            {
                var full=System.Array.IndexOf(ProductionDisplay.Types,type)>=0?CreateProduction(type,mats,meshes):CreateExtra(type,mats,meshes);
                full.name=type+"_L"+level;if(level==3)AddUpgrade(full,type,3,mats,meshes);return full;
            }
            var root=new GameObject(type+"_L1");var m=new EnvironmentMesh(Colors.Length);
            switch(type)
            {
                case "Windmill":Cylinder(m,Vector3.zero,.18f,.38f,.38f,Stone);Cylinder(m,Vector3.up*.18f,.12f,.34f,.34f,StoneDark);Beam(m,new Vector3(0,.30f,0),new Vector3(.50f,.30f,0),.026f,Wood);Sack(m,new Vector3(-.46f,0,0),.7f);break;
                case "BakeryOven":Cylinder(m,Vector3.zero,.13f,.51f,.51f,Stone);Ellipsoid(m,Vector3.up*.13f,new Vector3(.49f,.44f,.44f),Plaster,true);Box(m,new Vector3(0,.25f,.44f),new Vector3(.28f,.25f,.022f),Black);break;
                case "DairyChurn":Cylinder(m,Vector3.zero,.53f,.23f,.20f,Wood);Hoop(m,Vector3.up*.12f,Vector3.up,.23f,.015f,StoneDark);Hoop(m,Vector3.up*.43f,Vector3.up,.21f,.015f,StoneDark);Beam(m,Vector3.up*.53f,Vector3.up*.91f,.016f);Beam(m,new Vector3(-.13f,.88f,0),new Vector3(.13f,.88f,0),.018f,Wood);break;
                case "JuicePress":Table(m,Vector3.zero,.77f,.64f,.35f);Cylinder(m,Vector3.up*.40f,.34f,.18f,.18f,Wood);Beam(m,new Vector3(-.30f,.35f,0),new Vector3(-.30f,.94f,0),.04f);Beam(m,new Vector3(.30f,.35f,0),new Vector3(.30f,.94f,0),.04f);Beam(m,new Vector3(-.32f,.91f,0),new Vector3(.32f,.91f,0),.04f);Cylinder(m,Vector3.up*.75f,.07f,.17f,.17f,WoodDark);break;
                case "SoyVat":Jar(m,Vector3.zero,1.1f);break;
                case "HerbalBench":Table(m,Vector3.zero,1.0f,.61f,.55f);Cylinder(m,new Vector3(.17f,.61f,0),.13f,.12f,.16f,StoneDark);Beam(m,new Vector3(.17f,.71f,0),new Vector3(.24f,.88f,0),.025f,Stone);break;
                case "CompostBin":Box(m,Vector3.up*.025f,new Vector3(.85f,.05f,.70f),WoodDark);for(int side=-1;side<=1;side+=2){Box(m,new Vector3(side*.40f,.19f,0),new Vector3(.05f,.32f,.70f),Wood);Box(m,new Vector3(0,.19f,side*.33f),new Vector3(.85f,.32f,.05f),Wood);}Ellipsoid(m,Vector3.up*.15f,new Vector3(.33f,.15f,.26f),Dark);break;
                case "GraftingBench":Table(m,Vector3.zero,.91f,.61f,.55f);Cylinder(m,Vector3.up*.61f,.14f,.10f,.13f,Tile);Beam(m,Vector3.up*.75f,Vector3.up*.94f,.008f,Wood);m.Leaf(Vector3.up*.84f,Vector3.right,.14f,.03f,Cloth);break;
                case "BeeHive":Box(m,Vector3.up*.22f,new Vector3(.53f,.44f,.50f),Wood);Roof(m,.67f,.65f,.46f,.15f,false);Box(m,new Vector3(0,.07f,.257f),new Vector3(.26f,.026f,.019f),Black);break;
                case "MarketStall":Table(m,Vector3.zero,1.31f,.70f,.63f);Crate(m,new Vector3(-.34f,.69f,0));break;
                case "HandCart":Box(m,Vector3.up*.33f,new Vector3(.80f,.06f,1.0f),Wood);for(int side=-1;side<=1;side+=2){Vector3 p=new Vector3(side*.48f,.25f,0);Hoop(m,p,Vector3.right,.22f,.032f,WoodDark);Beam(m,p-Vector3.up*.2f,p+Vector3.up*.2f,.018f,Wood);Beam(m,new Vector3(side*.3f,.30f,.36f),new Vector3(side*.3f,.33f,1.15f),.023f);}break;
                case "WoodBridge":for(int i=0;i<11;i++)Box(m,new Vector3(0,.13f,-.95f+i*.19f),new Vector3(.90f,.10f,.17f),Wood);break;
                case "BoatDock":for(int i=0;i<9;i++)Box(m,new Vector3(0,.19f,-.73f+i*.18f),new Vector3(.90f,.08f,.16f),Wood);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,new Vector3(x*.33f,0,z*.65f),new Vector3(x*.33f,.36f,z*.65f),.049f);break;
                case "CargoBoat":Boat(m,true);break;
                case "WeatherStation":Beam(m,Vector3.zero,Vector3.up*1.07f,.027f,Wood);Box(m,Vector3.up*.73f,new Vector3(.39f,.48f,.09f),Plaster);for(int i=0;i<5;i++)Box(m,new Vector3(.09f,.56f+i*.073f,.05f),new Vector3(.07f,.007f,.008f),Red);break;
                case "Lighthouse":Cylinder(m,Vector3.zero,1.23f,.34f,.25f,Stone);Cylinder(m,Vector3.up*1.23f,.29f,.24f,.24f,Gold);Cylinder(m,Vector3.up*1.52f,.27f,.38f,.02f,Tile);break;
                case "Watchtower":for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,new Vector3(x*.39f,0,z*.37f),new Vector3(x*.34f,1.16f,z*.32f),.039f);Box(m,Vector3.up*.88f,new Vector3(.92f,.09f,.85f),Wood);Roof(m,1.05f,.99f,1.55f,.22f,false);break;
                case "VillageWarehouse":SimpleHut(m,1.70f,1.35f,1.35f,false);break;
                case "RatProofStore":for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Cylinder(m,new Vector3(x*.51f,0,z*.38f),.31f,.08f,.08f,StoneDark,12);Box(m,new Vector3(0,.78f,0),new Vector3(1.21f,.93f,.93f),Wood);Roof(m,1.42f,1.20f,1.27f,.22f,false);Box(m,new Vector3(0,.70f,.48f),new Vector3(.42f,.62f,.04f),Dark);break;
                case "FloodDike":Box(m,Vector3.up*.12f,new Vector3(1.55f,.24f,.60f),Wood);break;
                case "RainTank":Cylinder(m,Vector3.zero,.70f,.28f,.28f,Wood,16);Hoop(m,Vector3.up*.15f,Vector3.up,.28f,.017f,StoneDark);Hoop(m,Vector3.up*.55f,Vector3.up,.28f,.017f,StoneDark);m.Disc(Vector3.up*.702f,.26f,.26f,Water,16);break;
            }
            Add(root,"Starter model",m.Finish(type+" L1"),Slice(mats),meshes,true);return root;
        }
    }
}
