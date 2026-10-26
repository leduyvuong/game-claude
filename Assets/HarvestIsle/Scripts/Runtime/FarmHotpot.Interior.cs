using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private void CombineStatic(Transform parent)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();
            foreach(var filter in parent.GetComponentsInChildren<MeshFilter>(true))
            {
                // Keep rigs and cutaway walls separate so hiding/posing them stays cheap.
                bool dynamic=false;
                for(var t=filter.transform;t!=null&&t!=parent;t=t.parent)
                    if(t.name=="Back cutaway"||t.name=="Left cutaway"||t.name=="Kitchen cook"||t.name=="Seated diner"||t.GetComponent<HotpotKitchenLevelView>()!=null)dynamic=true;
                var renderer=filter.GetComponent<MeshRenderer>();
                if(dynamic||renderer==null||!renderer.enabled||filter.sharedMesh==null||!filter.sharedMesh.isReadable)continue;
                var materials=renderer.sharedMaterials;
                for(int s=0;s<filter.sharedMesh.subMeshCount&&s<materials.Length;s++)
                {
                    var material=materials[s];if(material==null)continue;
                    List<CombineInstance> parts;if(!groups.TryGetValue(material,out parts)){parts=new List<CombineInstance>();groups.Add(material,parts);}
                    parts.Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=s,transform=parent.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                }
                renderer.enabled=false;
            }
            foreach(var group in groups)
            {
                var mesh=new Mesh{name="Hotpot combined decor",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(group.Value.ToArray(),true,true);roomMeshes.Add(mesh);
                var combined=Group("Batched decor",parent,Vector3.zero);combined.AddComponent<MeshFilter>().sharedMesh=mesh;combined.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
            }
        }
        private GameObject Group(string name,Transform parent,Vector3 p)
        {var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localPosition=p;return o;}
        private GameObject Box(string name,Transform parent,Vector3 p,Vector3 size,string color)
        {return Part(PrimitiveType.Cube,name,parent,p,size,color);}
        private void BuildRestaurantShell(Transform parent,int floor)
        {
            Box("Street stone",parent,new Vector3(0,-.3f,0),new Vector3(36,.3f,32),"A7A495");
            for(int x=-17;x<=17;x++)Box("Paving joint",parent,new Vector3(x,-.139f,0),new Vector3(.025f,.012f,32),"8E8C80");
            for(int z=-16;z<=16;z++)Box("Paving joint",parent,new Vector3(0,-.139f,z),new Vector3(36,.012f,.025f),"8E8C80");
            Box("Terracotta grout",parent,new Vector3(0,-.11f,0),new Vector3(16,.20f,17),"9D795C");
            string[] tiles={"AC6C47","B2734D","B7774F","A96744","BA7B53"};
            int branch=game.Model.State.hotpot.progress.branch%4;
            if(branch==1)tiles=new[]{"89B9B1","9DC7BC","86B5B2","A1C3AF","94BCB4"};
            if(branch==2)tiles=new[]{"85765A","97846A","A08D72","8F8066","A09077"};
            if(branch==3)tiles=new[]{"735866","82606C","8D6771","6C535E","976F77"};
            for(int x=0;x<20;x++)for(int z=0;z<21;z++)
                Box("Terracotta tile",parent,new Vector3(-7.6f+x*.8f,.002f,-8+z*.8f),new Vector3(.78f,.018f,.78f),tiles[(x*7+z*3)%tiles.Length]);
            var back=Group("Back cutaway",parent,Vector3.zero).transform;
            var left=Group("Left cutaway",parent,Vector3.zero).transform;
            Box("Cream plaster",back,new Vector3(0,1.85f,8.4f),new Vector3(16,3.7f,.20f),"DFC79C");
            Box("Cream plaster",left,new Vector3(-8,1.85f,0),new Vector3(.20f,3.7f,17),"DFC79C");
            Box("Wood dado",back,new Vector3(0,.45f,8.23f),new Vector3(16,.9f,.12f),"755336");
            Box("Wood dado",left,new Vector3(-7.86f,.45f,0),new Vector3(.12f,.9f,17),"755336");
            Box("Wood crown",back,new Vector3(0,3.72f,8.4f),new Vector3(16,.22f,.4f),"5B3F2B");
            Box("Wood crown",left,new Vector3(-8,3.72f,0),new Vector3(.4f,.22f,17),"5B3F2B");
            for(int x=-7;x<=7;x+=7)Box("Wood post",back,new Vector3(x,1.8f,8.13f),new Vector3(.22f,3.6f,.3f),"765233");
            for(int z=-7;z<=7;z+=7)
            {
                Box("Wood post",left,new Vector3(-7.77f,1.8f,z),new Vector3(.3f,3.6f,.22f),"765233");
                var frame=Group("Landscape frame",left,new Vector3(-7.74f,2.05f,z+1.7f)).transform;
                Box("Wood picture frame",frame,Vector3.zero,new Vector3(.13f,1.5f,2.3f),"865B35");
                Box("Painted landscape",frame,new Vector3(.075f,0,0),new Vector3(.025f,1.3f,2.1f),"B5BA92");
                for(int hill=0;hill<3;hill++)Part(PrimitiveType.Sphere,"Painted hill",frame,new Vector3(.09f,-.23f,-.65f+hill*.65f),new Vector3(.025f,.6f+hill*.13f,.86f),hill==1?"758A65":"8E9C77");
                Box("Sconce brass",left,new Vector3(-7.68f,2.1f,z),new Vector3(.18f,.62f,.27f),"8B6738");
                Box("Sconce warm glass",left,new Vector3(-7.55f,2.12f,z),new Vector3(.17f,.45f,.18f),"FFDC88");
            }
            BuildKitchen(parent);
            // The front is open, with a reception counter and an entrance between planter rails.
            Rail(parent,new Vector3(-4.9f,0,-7.85f),5.7f);
            Rail(parent,new Vector3(5.3f,0,-7.85f),4.6f);
            Box("Wood reception",parent,new Vector3(-2.8f,.62f,-7.1f),new Vector3(2.7f,1.24f,.8f),"825536");
            Box("Wood reception top",parent,new Vector3(-2.8f,1.27f,-7.1f),new Vector3(2.85f,.14f,.95f),"B68754");
            Box("Register screen",parent,new Vector3(-3.35f,1.48f,-7.1f),new Vector3(.45f,.32f,.12f),"303B32");
            Box("Entrance mat",parent,new Vector3(.15f,.02f,-8.05f),new Vector3(2.2f,.04f,.7f),"52624A");
            var sign=Group("Welcome chalkboard",parent,new Vector3(-1.1f,.9f,-8.45f)).transform;
            Box("Wood board frame",sign,Vector3.zero,new Vector3(.85f,1.5f,.12f),"8B623D");
            Box("Chalk board",sign,new Vector3(0,0,-.08f),new Vector3(.72f,1.36f,.04f),"344039");
            var words=Group("Menu lettering",sign,new Vector3(0,.48f,-.12f)).AddComponent<TextMesh>();
            words.text="LẨU\nMÙA VÀNG\n——\nRau vườn\nNấm tươi";words.fontSize=40;words.characterSize=.055f;words.anchor=TextAnchor.UpperCenter;words.alignment=TextAlignment.Center;words.color=FarmWorld.Hex("EFE2BD");
            for(int x=-7;x<=7;x+=2)if(Mathf.Abs(x)>1)Planter(parent,new Vector3(x,0,-8.7f),.85f);
            Planter(parent,new Vector3(-7.1f,0,4.5f),1.7f);Planter(parent,new Vector3(7.1f,0,4.5f),1.7f);
            Planter(parent,new Vector3(-7.05f,0,-2),1.4f);Planter(parent,new Vector3(7.1f,0,-2),1.2f);
            for(int x=-5;x<=5;x+=5)Pendant(parent,new Vector3(x,3.5f,2.1f));
            Guest(parent,new Vector3(-4.8f,0,6.8f),180,false,false);
            // This cook is ambient scenery; kitchen staff automation is still a separate system.
        }
        private void BuildKitchen(Transform p)
        {
            Box("Kitchen stone floor",p,new Vector3(0,.035f,6.8f),new Vector3(15.6f,.06f,2.7f),"AAA99C");
            Box("Wood serving counter",p,new Vector3(0,.68f,5.25f),new Vector3(12.4f,1.36f,.95f),"815637");
            Box("Counter slab",p,new Vector3(0,1.41f,5.25f),new Vector3(12.65f,.13f,1.12f),"D1B186");
            for(int x=-6;x<=6;x++)
            {
                Box("Wood counter panel",p,new Vector3(x,.66f,4.74f),new Vector3(.04f,1.23f,.04f),"563E2B");
                for(int i=0;i<4;i++)Part(PrimitiveType.Cylinder,"Stacked dishes",p,new Vector3(x,1.5f+i*.045f,5.15f),new Vector3(.38f,.022f,.38f),"E8DEC7");
            }
            var kitchenNode=Group("Upgradeable kitchen",p,new Vector3(-1.9f,.04f,7.5f));
            var kitchenView=kitchenNode.AddComponent<HotpotKitchenLevelView>();
            if(!kitchenView.SetLevel(game.Model.KitchenLevel))Debug.LogError("Missing baked kitchen level");
            Box("Steel fridge",p,new Vector3(6.15f,1.42f,7.55f),new Vector3(1.65f,2.84f,1.25f),"A0A69E");
            Box("Fridge seam",p,new Vector3(6.15f,1.45f,6.9f),new Vector3(.025f,2.6f,.02f),"48524B");
            for(int i=-1;i<=1;i+=2)Box("Steel handle",p,new Vector3(6.15f+i*.16f,1.45f,6.82f),new Vector3(.055f,.65f,.09f),"D0D0BC");
            Box("Steel sink unit",p,new Vector3(3,.66f,7.55f),new Vector3(2.6f,1.32f,1.2f),"A7ADA5");
            Box("Sink basin",p,new Vector3(3,1.34f,7.55f),new Vector3(1.55f,.045f,.85f),"586760");
            Box("Steel faucet",p,new Vector3(3,1.65f,7.95f),new Vector3(.07f,.6f,.07f),"D4D3C4");
            Box("Steel tap",p,new Vector3(3,1.93f,7.78f),new Vector3(.07f,.07f,.4f),"D4D3C4");
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*6.8f;
                for(int y=0;y<3;y++)
                {
                    Box("Wood vegetable shelf",p,new Vector3(x,.38f+y*.55f,5.9f),new Vector3(1.35f,.10f,1),"94714A");
                    FoodTray(p,new Vector3(x,.46f+y*.55f,5.9f),y%2);
                }
                for(int z=-1;z<=1;z+=2)Box("Steel shelf upright",p,new Vector3(x,1,5.9f+z*.5f),new Vector3(.07f,1.8f,.07f),"737F72");
            }
        }
        private void Rail(Transform p,Vector3 position,float length)
        {
            var rail=Group("Wood planter railing",p,position).transform;
            Box("Wood rail cap",rail,new Vector3(0,1.05f,0),new Vector3(length,.15f,.22f),"7D5637");
            Box("Wood rail base",rail,new Vector3(0,.18f,0),new Vector3(length,.14f,.22f),"7D5637");
            for(float x=-length/2;x<=length/2;x+=.38f)Box("Wood rail spindle",rail,new Vector3(x,.6f,0),new Vector3(.10f,.9f,.13f),"9A754A");
            for(float x=-length/2+.5f;x<length/2;x+=1.1f)Planter(rail,new Vector3(x,0,.32f),.95f);
        }
        private void Planter(Transform p,Vector3 position,float height)
        {
            Part(PrimitiveType.Cylinder,"Terracotta planter",p,position+Vector3.up*.24f,new Vector3(.52f,.24f,.52f),"A26F4C");
            Part(PrimitiveType.Cylinder,"Pot soil",p,position+Vector3.up*.49f,new Vector3(.47f,.012f,.47f),"443B2B");
            game.World.Imported("Collection/Fern",p,position+Vector3.up*.45f,height,.95f);
        }
        private void Pendant(Transform p,Vector3 position)
        {
            Part(PrimitiveType.Cylinder,"Pendant cord",p,position+Vector3.up*.4f,new Vector3(.028f,.4f,.028f),"4D4232");
            Part(PrimitiveType.Sphere,"Woven pendant",p,position,new Vector3(.85f,.55f,.85f),"C39552");
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;var rib=Part(PrimitiveType.Cylinder,"Bamboo shade rib",p,position+new Vector3(Mathf.Cos(a)*.36f,-.015f,Mathf.Sin(a)*.36f),new Vector3(.025f,.20f,.025f),"ECD199");
                rib.transform.localRotation=Quaternion.Euler(Mathf.Sin(a)*22,0,-Mathf.Cos(a)*22);
            }
            Part(PrimitiveType.Sphere,"Warm bulb",p,position-Vector3.up*.18f,new Vector3(.33f,.2f,.33f),"FFE6A3");
            var glow=Group("Pendant glow",p,position-Vector3.up*.38f).AddComponent<Light>();
            glow.type=LightType.Point;glow.color=FarmWorld.Hex("FFD79A");glow.intensity=2;glow.range=4;glow.shadows=LightShadows.None;
        }
        private void BuildDiningTable(Transform p,int kind,int slot)
        {
            string wood=kind>=4?"91643A":kind>=2?"845334":"9E7245";
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                Box("Wood table leg",p,new Vector3(x*.74f,.52f,z*.57f),new Vector3(.13f,1.04f,.13f),"785030");
            Box("Wood table apron",p,new Vector3(0,.92f,0),new Vector3(1.6f,.24f,1.25f),wood);
            for(int i=0;i<5;i++)Box("Plank table top",p,new Vector3(0,1.10f,-.62f+i*.31f),new Vector3(1.9f,.13f,.30f),wood);
            Part(PrimitiveType.Cylinder,"Steel inset burner",p,new Vector3(0,1.18f,0),new Vector3(.68f,.024f,.68f),"525950");
            Part(PrimitiveType.Cylinder,"Burner center",p,new Vector3(0,1.208f,0),new Vector3(.51f,.015f,.51f),"262E28");
            for(int side=-1;side<=1;side+=2)
            {
                var chair=Group("Chair",p,new Vector3(side*1.28f,0,0)).transform;chair.localRotation=Quaternion.Euler(0,side<0?90:-90,0);
                Box("Chair seat",chair,new Vector3(0,.55f,0),new Vector3(.67f,.12f,.72f),wood);
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Chair leg",chair,new Vector3(x*.26f,.26f,z*.29f),new Vector3(.09f,.52f,.09f),"6B4930");
                Box("Chair back rail",chair,new Vector3(0,1.14f,-.31f),new Vector3(.73f,.14f,.10f),wood);
                for(int i=-1;i<=1;i++)Box("Chair back spindle",chair,new Vector3(i*.24f,.91f,-.31f),new Vector3(.07f,.67f,.085f),wood);
                Part(PrimitiveType.Cylinder,"Ceramic saucer",p,new Vector3(side*.69f,1.18f,0),new Vector3(.30f,.025f,.30f),"F4E7CB");
                Part(PrimitiveType.Sphere,"Ceramic bowl",p,new Vector3(side*.69f,1.24f,0),new Vector3(.26f,.12f,.26f),"F4E7CB");
                for(int stick=0;stick<2;stick++)Box("Chopsticks",p,new Vector3(side*.71f+stick*.06f,1.185f,.31f),new Vector3(.018f,.02f,.36f),"573A26");
            }
        }
        private void FoodTray(Transform p,Vector3 position,int kind)
        {
            Box("Ceramic ingredient tray",p,position,new Vector3(.60f,.055f,.40f),"E9E0C9");
            for(int i=0;i<6;i++)
                Part(PrimitiveType.Sphere,kind==0?"Leafy vegetables":"Sliced ingredients",p,position+new Vector3(-.20f+(i%3)*.20f,.065f,-.10f+(i/3)*.20f),new Vector3(.18f,.06f,.16f),kind==0?(i%2==0?"7B9D48":"A4B662"):(i%2==0?"C97D66":"E8B798"));
        }
        private void Cookpot(Transform p,Vector3 position,float width,bool steam)
        {
            Part(PrimitiveType.Cylinder,"Steel hotpot",p,position,new Vector3(width,.14f,width),"ADB0A0");
            Part(PrimitiveType.Cylinder,"Simmering broth",p,position+Vector3.up*.145f,new Vector3(width*.85f,.012f,width*.85f),"B87035");
            for(int side=-1;side<=1;side+=2)Box("Steel pot handle",p,position+new Vector3(side*width*.59f,.04f,0),new Vector3(.17f,.065f,.12f),"646D61");
            for(int i=0;i<5;i++)
            {float a=i*2.4f;Part(PrimitiveType.Sphere,"Broth garnish",p,position+new Vector3(Mathf.Cos(a)*width*.25f,.17f,Mathf.Sin(a)*width*.25f),new Vector3(.11f,.04f,.09f),i%2==0?"8C9A4F":"EDD3A0");}
            if(!steam)return;
            var ps=Group("Hotpot steam",p,position+Vector3.up*.22f).AddComponent<ParticleSystem>();
            var main=ps.main;main.startLifetime=1.7f;main.startSpeed=.28f;main.startSize=.22f;main.startColor=new Color(1,.95f,.83f,.15f);main.maxParticles=14;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var emission=ps.emission;emission.rateOverTime=5;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.12f;shape.angle=8;
            ps.transform.localRotation=Quaternion.Euler(-90,0,0);
            if(steamMaterial==null)
            {
                steamMap=new Texture2D(32,32,TextureFormat.RGBA32,false);steamMap.wrapMode=TextureWrapMode.Clamp;
                var pixels=new Color[1024];
                for(int y=0;y<32;y++)for(int x=0;x<32;x++)
                {float falloff=Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15);pixels[y*32+x]=new Color(1,1,1,falloff*falloff);}
                steamMap.SetPixels(pixels);steamMap.Apply(false,true);
                steamMaterial=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));steamMaterial.SetTexture("_BaseMap",steamMap);
                steamMaterial.SetFloat("_Surface",1);steamMaterial.SetFloat("_Blend",0);steamMaterial.SetFloat("_SrcBlend",5);steamMaterial.SetFloat("_DstBlend",10);steamMaterial.SetFloat("_ZWrite",0);steamMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");steamMaterial.renderQueue=3000;
            }
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=steamMaterial;
        }
        private void Guest(Transform p,Vector3 position,float yaw,bool female,bool seated=true)
        {
            var person=game.World.Imported(female?"Characters/FarmerFemale":"Characters/Farmer",p,position,seated?2.35f:2.1f);
            if(person==null)return;
            person.name=seated?"Seated diner":"Kitchen cook";person.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var avatar=person.GetComponent<FarmerAvatar>();if(avatar!=null)avatar.enabled=false;
            foreach(Transform bone in person.GetComponentsInChildren<Transform>(true))
            {
                string n=bone.name.ToLowerInvariant();
                if(n.Contains("hat")||n.Contains("bamboo weave")||n.Contains("watering can")||n.Contains("scarf tail"))bone.gameObject.SetActive(false);
                if(seated&&(n=="left hip"||n=="right hip"))bone.localRotation=Quaternion.Euler(-75,0,0);
                if(n=="left shoulder"||n=="right shoulder")bone.localRotation=Quaternion.Euler(-40,0,n.StartsWith("left")?-8:8);
            }
            if(seated)person.transform.localPosition-=Vector3.up*.15f;
            else
            {
                Part(PrimitiveType.Cylinder,"Chef hat",p,position+Vector3.up*1.87f,new Vector3(.43f,.18f,.43f),"F6EDDC");
                Box("Cook apron",p,position+new Vector3(0,.9f,-.22f),new Vector3(.48f,.75f,.035f),"E9E0CB");
            }
        }
    }
}
