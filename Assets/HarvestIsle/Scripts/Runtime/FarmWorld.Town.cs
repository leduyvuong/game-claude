using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmWorld
    {
        // Khu phố Bờ Tây: a small neighborhood on the north-west cape, far from the
        // field — hotpot house (built by FarmHotpot), produce market, cottages,
        // street lamps and strolling hotpot regulars. Purely decorative: colliders
        // stay off and nothing here reads or writes the economy or save.
        public static readonly Vector3 TownCenter = new Vector3(-29f, 0.15f, 9f);
        public static bool InTownZone(Vector3 p) => p.x > -46f && p.x < -15f && p.z > -7f && p.z < 26f;

        public struct NightGlow { public Material material; public Color day, night; }
        public readonly List<NightGlow> TownGlows = new List<NightGlow>();
        public readonly List<Light> TownLights = new List<Light>();
        public readonly List<GameObject> TownNightOnly = new List<GameObject>();
        private readonly Dictionary<string, Material> glowMaterials = new Dictionary<string, Material>();
        private readonly List<Mesh> townMeshes = new List<Mesh>();
        public Transform TownRoot { get; private set; }

        // One shared emissive material per key; FarmDayNight lerps day/night colors.
        public Material GlowMaterial(string key, Color day, Color night)
        {
            Material material;
            if (glowMaterials.TryGetValue(key, out material)) return material;
            material = new Material(Material(day)) { name = "Glow " + key };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            glowMaterials.Add(key, material);
            TownGlows.Add(new NightGlow { material = material, day = day, night = night });
            return material;
        }

        public GameObject GlowShape(PrimitiveType primitive, string name, Transform parent, Vector3 position, Vector3 scale, string key, Color day, Color night)
        {
            var item = Shape(primitive, name, parent, position, scale, day);
            item.GetComponent<Renderer>().sharedMaterial = GlowMaterial(key, day, night);
            return item;
        }

        public Light TownLampLight(Transform parent, Vector3 position, float range)
        {
            var lamp = new GameObject("Night light").AddComponent<Light>();
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = position;
            lamp.type = LightType.Point;
            lamp.range = range;
            lamp.intensity = 0;
            lamp.color = Hex("FFC387");
            lamp.shadows = LightShadows.None;
            lamp.enabled = false;
            TownLights.Add(lamp);
            return lamp;
        }

        private void BuildTown()
        {
            TownRoot = new GameObject("Khu phố Bờ Tây").transform;
            TownRoot.SetParent(transform, false);

            // A connected neighborhood: gardens face lanes, shops face the square.
            TownStreet(new Vector3(-44,0,7),new Vector3(-16,0,7),2.6f);
            TownStreet(new Vector3(-43,0,17),new Vector3(-17,0,17),1.8f);
            TownStreet(new Vector3(-43,0,-3),new Vector3(-17,0,-3),1.8f);
            TownStreet(new Vector3(-43,0,-3),new Vector3(-43,0,17),1.8f);
            TownStreet(new Vector3(-17,0,-3),new Vector3(-17,0,17),1.8f);
            TownStreet(new Vector3(-36,0,-3),new Vector3(-36,0,17),1.6f);
            TownStreet(new Vector3(-22,0,-3),new Vector3(-22,0,17),1.6f);
            Road(new Vector3(-16,0,7),new Vector3(-7,0,8));
            Road(new Vector3(-7,0,8),new Vector3(-6.2f,0,2.7f));
            Shape(PrimitiveType.Cube,"Town square border",TownRoot,new Vector3(-29,.17f,7),new Vector3(12,.07f,7),Hex("BCAB8D"));
            Shape(PrimitiveType.Cube,"Terracotta square",TownRoot,new Vector3(-29,.215f,7),new Vector3(11.6f,.04f,6.6f),Hex("DABF96"));
            for(int i=0;i<12;i++)
                Shape(PrimitiveType.Cube,"Square paving seam",TownRoot,new Vector3(-34.5f+i,.24f,7),new Vector3(.025f,.006f,6.5f),Hex("EAD8B6"));
            for(int i=0;i<7;i++)
                Shape(PrimitiveType.Cube,"Square paving seam",TownRoot,new Vector3(-29,.24f,4+i),new Vector3(11.5f,.006f,.025f),Hex("EAD8B6"));
            // Shaded central planter leaves wide walking space on either side.
            Shape(PrimitiveType.Cylinder,"Square planter",TownRoot,new Vector3(-29,.35f,6.4f),new Vector3(2.9f,.18f,2.9f),Hex("C5AB82"));
            EnvironmentModel("FlameTree",new Vector3(-29,.52f,6.4f),.8f);
            Prop("Bench",new Vector3(-31.2f,.25f,6.4f),.65f,90);
            Prop("Bench",new Vector3(-26.8f,.25f,6.4f),.65f,-90);
            BuildMarket();
            Color[] walls={Hex("F4E6C9"),Hex("D9E4D2"),Hex("F0D4B8"),Hex("D4E4E5")};
            Color[] roofs={Hex("B96549"),Hex("778F82"),Hex("C78757"),Hex("667E8A")};
            for(int i=0;i<4;i++)
            {
                float x=-39+i*6;
                TownHouse(i+1,new Vector3(x,.15f,21),0,walls[i],roofs[i],1.5f);
                TownHouse(i+5,new Vector3(x,.15f,-.1f),180,walls[(i+2)%4],roofs[(i+1)%4],1.35f);
            }
            TownHouse(9,new Vector3(-40,.15f,11.5f),90,walls[1],roofs[0],1.4f);
            TownHouse(10,new Vector3(-19,.15f,11.5f),-90,walls[2],roofs[3],1.4f);
            for(int i=0;i<5;i++)
            {
                LampPost(new Vector3(-42+i*6,.15f,8.7f));
                LampPost(new Vector3(-42+i*6,.15f,15.6f));
            }
            for(int i=0;i<5;i++)
            {
                EnvironmentModel("ArecaPalm",new Vector3(-44.8f,.15f,-1+i*5),.9f);
                EnvironmentModel("ArecaPalm",new Vector3(-15.2f,.15f,-1+i*5),.9f);
            }
            // Pocket gardens give the plaza a softer edge and a reason to stroll.
            for(int side=-1;side<=1;side+=2)
            {
                float x=-29+side*4.5f;
                Shape(PrimitiveType.Cube,"Pocket garden edging",TownRoot,new Vector3(x,.22f,12.8f),new Vector3(3.5f,.14f,4.2f),Hex("B8A98B"));
                Shape(PrimitiveType.Cube,"Pocket garden lawn",TownRoot,new Vector3(x,.3f,12.8f),new Vector3(3.3f,.04f,4),Hex("83A66D"));
                EnvironmentModel("ArecaPalm",new Vector3(x,.33f,14),1.05f);
                Prop("Bench",new Vector3(x,.34f,11.7f),.58f,0);
                for(int j=0;j<3;j++)Prop("Wildflowers",new Vector3(x-1+j,.34f,13.2f),.25f,j*45);
            }
            Prop("OrderBoard",new Vector3(-24,.24f,8.5f),1.4f,0);
            Prop("FestivalLantern",new Vector3(-32,.24f,10),1.2f,0);
            Prop("FestivalLantern",new Vector3(-26,.24f,10),1.2f,0);
            var fireflies=new GameObject("Town fireflies");fireflies.transform.SetParent(TownRoot,false);
            for(int i=0;i<6;i++)
            {
                var center=new Vector3(-41+i*4.5f,.8f,16);
                var bug=Imported("Collection/GoldenFirefly",fireflies.transform,center,.15f,.3f);
                if(bug!=null)bug.AddComponent<FarmFlutter>().Initialize(center,1,1.2f+i*.15f);
            }
            fireflies.SetActive(false);TownNightOnly.Add(fireflies);

            BuildTownCustomers();
            // Batch each static house separately so distant houses can still be culled.
            foreach(Transform child in TownRoot)
                if(child.name.StartsWith("Nhà dân")) BatchTownHouse(child);
        }

        private void BatchTownHouse(Transform parent)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();
            foreach(var filter in parent.GetComponentsInChildren<MeshFilter>())
            {
                var renderer=filter.GetComponent<MeshRenderer>();
                if(renderer==null||!renderer.enabled||filter.sharedMesh==null||!filter.sharedMesh.isReadable)continue;
                var slots=renderer.sharedMaterials;
                if(slots.Length!=filter.sharedMesh.subMeshCount)continue;
                bool valid=true;foreach(var material in slots)if(material==null)valid=false;
                if(!valid)continue;
                for(int i=0;i<slots.Length;i++)
                {
                    if(!groups.TryGetValue(slots[i],out var parts)){parts=new List<CombineInstance>();groups.Add(slots[i],parts);}
                    parts.Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=i,transform=parent.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                }
                renderer.enabled=false;
            }
            foreach(var group in groups)
            {
                var mesh=new Mesh{name="Town house batch",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
                mesh.CombineMeshes(group.Value.ToArray(),true,true);townMeshes.Add(mesh);
                var item=new GameObject("House combined",typeof(MeshFilter),typeof(MeshRenderer));item.transform.SetParent(parent,false);
                item.GetComponent<MeshFilter>().sharedMesh=mesh;item.GetComponent<MeshRenderer>().sharedMaterial=group.Key;
            }
        }

        private void TownStreet(Vector3 from,Vector3 to,float width)
        {
            Vector3 center=(from+to)*.5f;center.y=.18f;
            float length=Vector3.Distance(from,to);
            var street=Shape(PrimitiveType.Cube,"Town street",TownRoot,center,new Vector3(width+.6f,.06f,length+.6f),Hex("DDCBB0"));
            street.transform.localRotation=Quaternion.LookRotation(to-from);
            Shape(PrimitiveType.Cube,"Packed street",street.transform,new Vector3(0,.55f,0),new Vector3(width/(width+.6f),.2f,.99f),Hex("BDA886"));
        }

        private void BuildMarket()
        {
            var market = new GameObject("Chợ nông sản");
            market.transform.SetParent(TownRoot, false);
            string[][] goods =
            {
                new[] { "Product_Carrot", "Product_Cabbage", "Product_Chili" },
                new[] { "Product_Corn", "Product_Rice", "Product_Mushroom" },
                new[] { "Product_Mango", "Product_Banana", "Product_Watermelon" }
            };
            Color[] awnings = { Hex("E06A4F"), Hex("5F9E63"), Hex("E8B84B") };
            for (int i = 0; i < 6; i++)
                MarketStall(market.transform, new Vector3(-34+i*2,.25f,3.7f), awnings[i%3], goods[i%3], i);
            Prop("Basket",new Vector3(-34.7f,.25f,3.5f),.42f,25);
            Prop("Basket",new Vector3(-23.3f,.25f,3.5f),.42f,65);
        }

        private void MarketStall(Transform parent, Vector3 position, Color awning, string[] goods, int seed)
        {
            var stall = new GameObject("Market stall " + (seed + 1));
            stall.transform.SetParent(parent, false);
            stall.transform.localPosition = position;
            stall.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Shape(PrimitiveType.Cube, "Stall counter", stall.transform, new Vector3(0, .3f, 0), new Vector3(1.1f, .5f, .6f), Hex("B8905E"));
            Shape(PrimitiveType.Cube, "Stall top", stall.transform, new Vector3(0, .57f, 0), new Vector3(1.2f, .06f, .7f), Hex("D9B981"));
            for (int side = -1; side <= 1; side += 2)
            {
                Shape(PrimitiveType.Cube, "Stall post", stall.transform, new Vector3(side * .54f, .72f, -.28f), new Vector3(.07f, 1.3f, .07f), Hex("97724D"));
                Shape(PrimitiveType.Cube, "Stall post", stall.transform, new Vector3(side * .54f, .72f, .28f), new Vector3(.07f, 1.3f, .07f), Hex("97724D"));
            }
            var cloth = Shape(PrimitiveType.Cube, "Stall awning", stall.transform, new Vector3(0, 1.42f, .08f), new Vector3(1.35f, .05f, .95f), awning);
            cloth.transform.localRotation = Quaternion.Euler(-9, 0, 0);
            Shape(PrimitiveType.Cube, "Awning trim", stall.transform, new Vector3(0, 1.33f, -.4f), new Vector3(1.37f, .09f, .06f), Hex("FFF1CE"));
            for (int g = 0; g < goods.Length; g++)
            {
                var produce = Imported("Collection/" + goods[g], stall.transform, new Vector3(-.33f + g * .33f, .6f, .05f), .22f, .3f);
                if (produce != null) produce.transform.localRotation = Quaternion.Euler(0, seed * 50 + g * 70, 0);
            }
        }

        private void TownHouse(int number, Vector3 position, float yaw, Color wall, Color roof, float scale)
        {
            var house = new GameObject("Nhà dân " + number);
            house.transform.SetParent(TownRoot, false);
            house.transform.localPosition = position;
            house.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            house.transform.localScale = Vector3.one * scale;
            Shape(PrimitiveType.Cube,"Garden edging",house.transform,new Vector3(0,.025f,-.35f),new Vector3(3.1f,.05f,3.3f),Hex("C5BA92"));
            Shape(PrimitiveType.Cube,"Garden lawn",house.transform,new Vector3(0,.06f,-.35f),new Vector3(2.95f,.04f,3.15f),Hex("9DBA77"));
            Shape(PrimitiveType.Cube,"House foundation",house.transform,new Vector3(0,.10f,0),new Vector3(1.98f,.2f,1.58f),Hex("BEAD91"));
            Shape(PrimitiveType.Cube,"Porch deck",house.transform,new Vector3(0,.14f,-1.02f),new Vector3(2.05f,.18f,.75f),Hex("DDC9A2"));
            for(int side=-1;side<=1;side+=2)
            {
                Shape(PrimitiveType.Cube,"Porch column",house.transform,new Vector3(side*.86f,.84f,-1.12f),new Vector3(.085f,1.48f,.085f),Hex("8B684B"));
                Shape(PrimitiveType.Cube,"Corner trim",house.transform,new Vector3(side*.88f,.91f,-.745f),new Vector3(.07f,1.7f,.07f),Hex("FFF0D2"));
                Shape(PrimitiveType.Cube,"Garden low wall",house.transform,new Vector3(side*1.48f,.23f,-.35f),new Vector3(.09f,.38f,3.1f),Hex("E5D6BA"));
                for(int j=0;j<4;j++)Shape(PrimitiveType.Cube,"Garden fence",house.transform,new Vector3(side*(.55f+j*.28f),.34f,-1.9f),new Vector3(.06f,.52f,.065f),Hex("F4E7CD"));
                Shape(PrimitiveType.Cube,"Garden fence rail",house.transform,new Vector3(side*.98f,.4f,-1.9f),new Vector3(.98f,.06f,.055f),Hex("F4E7CD"));
                var pot=Imported("Collection/Shrub",house.transform,new Vector3(side*1.15f,.09f,-1.3f),.42f,.65f);
                if(pot!=null)pot.name="Porch garden";
            }
            var canopy=Shape(PrimitiveType.Cube,"Porch canopy",house.transform,new Vector3(0,1.62f,-.92f),new Vector3(2.13f,.075f,.55f),roof);
            canopy.transform.localRotation=Quaternion.Euler(-12,0,0);
            Shape(PrimitiveType.Cube,"Garden entrance path",house.transform,new Vector3(-.32f,.09f,-1.64f),new Vector3(.58f,.03f,.72f),Hex("E4D3B4"));
            Shape(PrimitiveType.Cube, "House walls", house.transform, new Vector3(0, .88f, 0), new Vector3(1.8f, 1.76f, 1.4f), wall);
            Shape(PrimitiveType.Cube, "House door", house.transform, new Vector3(-.32f, .71f, -.735f), new Vector3(.48f, 1.22f, .06f), Hex("6E4A2C"));
            GlowShape(PrimitiveType.Cube, "House window", house.transform, new Vector3(.42f, 1.03f, -.735f), new Vector3(.42f, .4f, .05f), "window", Hex("A9D4DE"), Hex("FFD98A"));
            GlowShape(PrimitiveType.Cube, "House window", house.transform, new Vector3(.92f, 1.03f, .1f), new Vector3(.05f, .4f, .5f), "window", Hex("A9D4DE"), Hex("FFD98A"));
            for (int side = -1; side <= 1; side += 2)
            {
                var slab = Shape(PrimitiveType.Cube, "House roof", house.transform, new Vector3(side * .52f, 1.98f, 0), new Vector3(1.25f, .14f, 1.8f), roof);
                slab.transform.localRotation = Quaternion.Euler(0, 0, side * -26);
            }
            Shape(PrimitiveType.Cube,"Roof ridge",house.transform,new Vector3(0,2.28f,0),new Vector3(.13f,.10f,1.92f),roof*.82f);
            for(int side=-1;side<=1;side+=2)
            for(int j=0;j<9;j++)
            {
                var tile=Shape(PrimitiveType.Cube,"Roof tile rib",house.transform,new Vector3(side*.52f,2.06f,-.83f+j*.205f),new Vector3(1.25f,.022f,.024f),roof*.87f);
                tile.transform.localRotation=Quaternion.Euler(0,0,side*-26);
            }
            for(int side=-1;side<=1;side+=2)
                Shape(PrimitiveType.Cube,"Window shutter",house.transform,new Vector3(.42f+side*.29f,1.03f,-.78f),new Vector3(.13f,.48f,.07f),Hex("527D72"));
            Shape(PrimitiveType.Cube,"Window sill",house.transform,new Vector3(.42f,.80f,-.79f),new Vector3(.58f,.06f,.16f),Hex("FFF0D2"));
            Shape(PrimitiveType.Cube,"Window mullion",house.transform,new Vector3(.42f,1.03f,-.78f),new Vector3(.035f,.4f,.025f),Hex("FFF0D2"));
            // Side and rear windows keep both sides of the residential lanes lively.
            for(int side=-1;side<=1;side+=2)
            {
                GlowShape(PrimitiveType.Cube,"Side window",house.transform,new Vector3(side*.92f,1.04f,.12f),new Vector3(.06f,.48f,.52f),"window",Hex("A9D4DE"),Hex("FFD98A"));
                Shape(PrimitiveType.Cube,"Side sill",house.transform,new Vector3(side*.95f,.78f,.12f),new Vector3(.12f,.06f,.64f),Hex("FFF0D2"));
                GlowShape(PrimitiveType.Cube,"Rear window",house.transform,new Vector3(side*.47f,1.03f,.735f),new Vector3(.44f,.48f,.06f),"window",Hex("A9D4DE"),Hex("FFD98A"));
            }
            Shape(PrimitiveType.Sphere,"Door handle",house.transform,new Vector3(-.14f,.7f,-.785f),Vector3.one*.055f,Hex("D9AF62"));
            if(number%3==0)
            {
                Shape(PrimitiveType.Cube,"Roof dormer",house.transform,new Vector3(.25f,2.06f,-.57f),new Vector3(.67f,.62f,.5f),wall);
                GlowShape(PrimitiveType.Cube,"Dormer window",house.transform,new Vector3(.25f,2.07f,-.83f),new Vector3(.35f,.34f,.035f),"window",Hex("A9D4DE"),Hex("FFD98A"));
                for(int side=-1;side<=1;side+=2)
                {
                    var cap=Shape(PrimitiveType.Cube,"Dormer roof",house.transform,new Vector3(.25f+side*.2f,2.42f,-.58f),new Vector3(.5f,.08f,.72f),roof);
                    cap.transform.localRotation=Quaternion.Euler(0,0,side*-28);
                }
            }
            Shape(PrimitiveType.Cube, "House step", house.transform, new Vector3(-.32f, .05f, -.82f), new Vector3(.55f, .1f, .25f), Hex("C9B18B"));
        }

        private void LampPost(Vector3 position)
        {
            var lamp = new GameObject("Street lamp");
            lamp.transform.SetParent(TownRoot, false);
            lamp.transform.localPosition = position;
            Shape(PrimitiveType.Cylinder, "Lamp pole", lamp.transform, new Vector3(0, .62f, 0), new Vector3(.06f, .62f, .06f), Hex("4A4038"));
            Shape(PrimitiveType.Cube, "Lamp cap", lamp.transform, new Vector3(0, 1.33f, 0), new Vector3(.22f, .05f, .22f), Hex("4A4038"));
            GlowShape(PrimitiveType.Sphere, "Lamp bulb", lamp.transform, new Vector3(0, 1.24f, 0), Vector3.one * .17f, "lamp", Hex("F2E8C8"), Hex("FFDF8F"));
            TownLampLight(lamp.transform, new Vector3(0, 1.24f, 0), 3.2f);
        }

        // The hotpot house's regulars: different characters pacing the street,
        // window-shopping at the market and loitering by the restaurant door.
        private void BuildTownCustomers()
        {
            Vector3[] street={new Vector3(-42,.25f,7),new Vector3(-35,.25f,7),new Vector3(-33,.25f,9),new Vector3(-25,.25f,9),new Vector3(-22,.25f,7),new Vector3(-17,.25f,7)};
            Vector3[] marketLoop={new Vector3(-34,.25f,5),new Vector3(-32,.25f,5),new Vector3(-26,.25f,5),new Vector3(-24,.25f,5)};
            Vector3[] laneLoop={new Vector3(-43,.25f,7),new Vector3(-43,.25f,17),new Vector3(-36,.25f,17),new Vector3(-22,.25f,17),new Vector3(-17,.25f,17),new Vector3(-17,.25f,7)};
            Vector3[] doorLoiter={new Vector3(-30,.25f,10),new Vector3(-28,.25f,10),new Vector3(-26,.25f,9)};
            TownCustomer("Characters/Farmer", false, "Khách mê lẩu", doorLoiter, .45f, .97f, 5);
            TownCustomer("Collection/MerchantHaiAu", true, "Khách du lịch", street, .55f, 1f, 11);
            TownCustomer("Collection/FarmerFemale", true, "Cô hàng xóm", marketLoop, .5f, 1f, 19);
            TownCustomer("Collection/ElderTu", true, "Già làng ghé quán", laneLoop, .38f, 1f, 27);
            TownCustomer("Characters/FarmerFemale", false, "Khách đói bụng", Reversed(street, .28f), .52f, 1f, 33);
            TownCustomer("Collection/FarmerFemale", true, "Bé con khu phố", Reversed(marketLoop, -.26f), .6f, .72f, 41);
            TownCustomer("Collection/MerchantHaiAu",true,"Người đi chợ",Reversed(laneLoop,.3f),.7f,1,53);
            TownCustomer("Characters/Farmer",false,"Người làm vườn",laneLoop,.65f,.95f,67);
            TownCustomer("Collection/ElderTu",true,"Ông hàng xóm",Reversed(street,-.3f),.5f,1,79);
            TownCustomer("Collection/FarmerFemale",true,"Khách dạo phố",street,.75f,1,89);
        }

        private static Vector3[] Reversed(Vector3[] route, float zOffset)
        {
            var result = new Vector3[route.Length];
            for (int i = 0; i < route.Length; i++)
            {
                result[i] = route[route.Length - 1 - i];
                result[i].z += zOffset;
            }
            return result;
        }

        private void TownCustomer(string path, bool imported, string label, Vector3[] route, float speed, float scale, int seed)
        {
            GameObject npc;
            if (imported)
            {
                npc = Imported(path, TownRoot, route[0], 1f, .8f);
            }
            else
            {
                var prefab = Resources.Load<GameObject>(path);
                if (prefab == null) return;
                npc = Instantiate(prefab, TownRoot, false);
                npc.transform.localPosition = new Vector3(route[0].x, .21f, route[0].z);
            }
            if (npc == null) return;
            npc.name = label;
            npc.transform.localScale *= scale;
            foreach (var collider in npc.GetComponentsInChildren<Collider>()) collider.enabled = false;
            npc.AddComponent<FarmTownStroller>().Configure(route, speed, seed);
        }
    }

    /// <summary>Walks back and forth along street waypoints with small pauses; cosmetic only.</summary>
    public sealed class FarmTownStroller : MonoBehaviour
    {
        private FarmerAvatar avatar;
        private Vector3[] points;
        private int index = 1, direction = 1;
        private float speed, waitUntil, height;
        private uint randomState;
        public void Configure(Vector3[] route, float speed, int seed)
        {
            points = route;
            this.speed = speed;
            randomState = (uint)Mathf.Max(1, seed);
            avatar = GetComponentInChildren<FarmerAvatar>();
            height = transform.localPosition.y;
            waitUntil = Time.time + Next() * 2f;
        }
        private float Next() { randomState = randomState * 1664525u + 1013904223u; return (randomState & 0x00ffffff) / 16777216f; }
        private void Update()
        {
            if (points == null || points.Length < 2) return;
            if (Time.time < waitUntil)
            {
                if (avatar != null) avatar.SetWalking(false);
                return;
            }
            Vector3 target = points[index]; target.y = height;
            Vector3 delta = target - transform.localPosition; delta.y = 0;
            if (delta.sqrMagnitude < .004f)
            {
                if (index == points.Length - 1) direction = -1;
                else if (index == 0) direction = 1;
                index += direction;
                // Pause at roughly every other stop, as if chatting or reading the menu.
                if (Next() > .45f) waitUntil = Time.time + 1f + Next() * 3f;
                return;
            }
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.LookRotation(delta), Time.deltaTime * 150);
            float facing=Mathf.Clamp01(Vector3.Dot(transform.forward,transform.parent.TransformDirection(delta.normalized)));
            float arrival=Mathf.Clamp01(delta.magnitude/.35f);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, Time.deltaTime * speed * Mathf.Max(.12f,arrival) * facing);
            if (avatar != null) avatar.SetWalking(true);
        }
    }
}
