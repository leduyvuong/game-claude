using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HarvestIsle
{
    public sealed partial class FarmWorld : MonoBehaviour
    {
        private static bool InFarmClearing(Vector3 p){return FarmRanch.InClearing(p)||FarmDevelopment.InClearing(p);}
        public const int InteractionLayer = 8;
        public Camera GameCamera { get; private set; }
        private readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        private readonly Dictionary<Material, Material> importedMaterials = new Dictionary<Material, Material>();
        private readonly Dictionary<Material, Material> vividMaterials = new Dictionary<Material, Material>();
        private FarmPlot[] plots;
        private Transform farmer;
        private FarmerAvatar farmerAvatar;
        private Vector3 farmerTarget;
        private Transform boat;
        private readonly List<GameObject> effects = new List<GameObject>();
        private float nextCleanup;
        public readonly Color SoilDry = Hex("946346");
        public readonly Color SoilWet = Hex("604B39");
        public readonly Color Leaf = Hex("60964A");
        public readonly Color Gold = Hex("FFCD62");

        public void Build(FarmGame game)
        {
            estateLayout=game.IsEstate;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Hex("CFDFC8");
            RenderSettings.fog = false;
            var light = new GameObject("Afternoon sun").AddComponent<Light>();
            light.transform.SetParent(transform);
            light.type = LightType.Directional;
            light.color = Hex("FFF0CB");
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            light.shadowBias = 0.045f;
            light.transform.rotation = Quaternion.Euler(52, -32, 0);
            RenderSettings.sun = light;

            GameCamera = new GameObject("Main Camera").AddComponent<Camera>();
            GameCamera.tag = "MainCamera";
            GameCamera.transform.SetParent(transform);
            GameCamera.backgroundColor = Hex("78C8C8");
            GameCamera.clearFlags = CameraClearFlags.SolidColor;
            GameCamera.nearClipPlane = 0.1f;
            GameCamera.farClipPlane = 400;
            GameCamera.allowHDR = false;
            GameCamera.gameObject.AddComponent<AudioListener>();
            var cameraData = GameCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = false;

            if (EnvironmentModel("SeaSurface", Vector3.zero, 4) == null)
                Shape(PrimitiveType.Cube, "Calm sea", transform, new Vector3(0, -0.78f, 0), new Vector3(450, 0.12f, 450), Hex("78C8C8"));
            var mainIsland=EnvironmentModel("IslandBase", Vector3.zero, 1);
            if(mainIsland!=null)mainIsland.transform.localScale=new Vector3(IslandScale,1,IslandScale);
            if (mainIsland == null)
            {
                Disc("Island rock", 60.5f, -0.3f, 1.1f, Hex("B79973"), 14);
                Disc("Soft sand shoreline", 60.7f, -0.02f, 0.2f, Hex("F2DC9D"), 18);
                Disc("Island grass", 54.4f, 0.05f, 0.2f, Hex("A6BF73"), 18);
                for (int i = 0; i < 3; i++)
                    Ring(transform, new Vector3(0, -0.64f + i * 0.006f, 0), 62.2f + i * 0.65f, new Color(0.73f, 0.92f, 0.87f, 1), 0.035f, 48);
            }
            if(estateLayout){BuildEstate(game);return;}
            for (int i = 0; i < 8; i++)
            {
                float x = -5.3f + i * 1.5f;
                Shape(PrimitiveType.Cube, "Stone path", transform, new Vector3(x, 0.17f, 3.1f), new Vector3(1.18f, 0.08f, 0.7f), Hex("E4D4AA"));
            }

            // Rows grow southward so unlocked parcels extend the field with correct
            // disease-spread adjacency (row r sits just south of row r-1).
            plots = new FarmPlot[FarmModel.MaxPlots];
            for (int index = 0; index < game.Model.State.plots.Count; index++) CreatePlot(index);
            SyncParcelMarkers(game.Model);

            if (StructureModel("FarmHouse", new Vector3(-4.5f, 0.15f, 4.7f), 1, 180) == null &&
                Imported("Art/FarmBuildings/Models/SmallBarn", transform, new Vector3(-4.5f, 0.17f, 4.7f), 2.7f) == null)
                Cottage(new Vector3(-4.5f, 0.17f, 4.7f));
            if (StructureModel("VillageWell", new Vector3(0.8f, 0.15f, 4.5f), 0.85f, 0) == null &&
                Imported("Art/FarmBuildings/Models/Well", transform, new Vector3(0.8f, 0.17f, 4.5f), 1.6f) == null)
                Well(new Vector3(0.8f, 0.17f, 4.5f));
            if (Imported("Art/FarmBuildings/Models/Windmill", transform, new Vector3(4.4f, 0.17f, 4.5f), 3.6f) == null)
                Windmill(new Vector3(4.4f, 0.17f, 4.5f));

            // The fence encloses the full claim, including the two southern parcels.
            for (int i = 0; i < 8; i++) Fence(new Vector3(-4.15f, 0.17f, -5.06f + i * 0.92f), true);
            for (int i = 0; i < 6; i++) Fence(new Vector3(-3.7f + i * 1.1f, 0.17f, -5.5f), false);
            Palm(new Vector3(-7, 0.1f, 2.4f), 1);
            Palm(new Vector3(6.7f, 0.1f, -1.8f), 0.85f);
            Palm(new Vector3(4.7f, 0.1f, -6.1f), 0.8f);
            Tree(new Vector3(-6.3f, 0.15f, 5.3f), 1.2f);
            Tree(new Vector3(5.8f, 0.15f, 5.4f), 0.85f);
            Tree(new Vector3(7.4f, 0.1f, 2), 1);
            EnvironmentModel("Pond", new Vector3(-5.8f, 0.15f, -0.7f), 1);
            EnvironmentModel("Lake", new Vector3(5.8f, 0.15f, -3.3f), 0.56f);
            string[] sceneryTrees={"RainTree","BanyanTree","PineTree","WillowTree","FlameTree","ArecaPalm","YoungForestTree"};
            Vector3[] sceneryPositions={new Vector3(-5.8f,0,5.3f),new Vector3(6.1f,0,4.8f),new Vector3(-1.8f,0,6.4f),new Vector3(-7.5f,0,.7f),new Vector3(7.1f,0,-3.5f),new Vector3(-6.6f,0,-5.2f),new Vector3(2.7f,0,5.8f)};
            for(int i=0;i<sceneryTrees.Length;i++)
            {var p=sceneryPositions[i];if(InFarmClearing(p))continue;p.y=ExpandedHeight(p.x,p.z);EnvironmentModel(sceneryTrees[i],p,.68f);}
            EnvironmentModel("Hill", new Vector3(-6.4f, 0.15f, 4.2f), 0.80f);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 2.39996f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 8.3f, 0.15f, Mathf.Sin(angle) * 7.0f);
                if(InFarmClearing(position))continue;
                position.y=ExpandedHeight(position.x,position.z);
                if (EnvironmentModel(i % 4 == 0 ? "RockLarge" : "RockSmall", position, i % 4 == 0 ? 0.45f : 0.9f) == null)
                    Shape(PrimitiveType.Sphere, "Beach rock", transform, position, new Vector3(0.5f, 0.25f, 0.4f), Hex("C4B99B"));
            }
            Flowers(new Vector3(3.9f, 0.17f, -2.8f));
            Flowers(new Vector3(-3.5f, 0.17f, 3.1f));
            BuildExpandedLand();
            MakeBoat();
            farmer = Farmer(new Vector3(-1.9f, 0.21f, 3.1f));
            farmerTarget = farmer.position;
            Animal("Duck", new Vector3(-6.2f, 0.15f, -2.7f), 0.20f, 0.20f, 22);
            Animal("Pig", new Vector3(-5.3f, 0.15f, -3.5f), 0.30f, 0.18f, 44);
            Animal("Cat", new Vector3(2.2f, 0.21f, 3.1f), 0.10f, 0.12f, 55);
            Animal("Sheep", new Vector3(6.0f, 0.15f, 2.9f), 0.14f, 0.18f, 66);
            Animal("Buffalo", new Vector3(-0.6f, 0.15f, -5.8f), 0.14f, 0.17f, 77);
            Animal("Deer", new Vector3(3.9f, 0.15f, 5.9f), 0.12f, 0.23f, 88);
            Animal("Sparrow", new Vector3(-3.0f, 0.15f, 2.5f), 0.14f, 0.22f, 99);
            Animal("Bulbul", new Vector3(7.0f, 0.15f, 0.6f), 0.16f, 0.23f, 111);
            Fish("Koi", new Vector3(-5.8f, 0.188f, -0.7f), 0.68f, 0.39f, 0.40f, 21);
            Fish("Carp", new Vector3(5.8f, 0.160f, -3.3f), 0.61f, 0.38f, 0.30f, 32);
            Fish("Perch", new Vector3(5.8f, 0.160f, -3.3f), 0.52f, 0.30f, 0.30f, 43);
            BuildScenery();
        }

        public static Vector3 PlotPosition(int index)
        {
            int row = index / FarmModel.Width, column = index % FarmModel.Width;
            return new Vector3((column - 2.5f) * 1.18f, 0.245f, (1.5f - row) * 1.18f - 0.4f);
        }

        private void CreatePlot(int index)
        {
            var root = new GameObject("Plot " + (index + 1));
            root.transform.SetParent(transform);
            root.transform.position = PositionOfPlot(index);
            if(estateLayout)root.transform.localScale=Vector3.one*1.7f;
            root.layer = InteractionLayer;
            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 0.06f, 0);
            collider.size = new Vector3(1.1f, 0.25f, 1.1f);
            plots[index] = root.AddComponent<FarmPlot>();
            plots[index].Initialize(this, index);
        }

        // Called after UnlockParcel: builds the new tilled rows and clears markers.
        public void SyncPlots(FarmModel model)
        {
            for (int index = 0; index < model.State.plots.Count; index++)
                if (plots[index] == null) CreatePlot(index);
            SyncParcelMarkers(model);
        }

        private readonly List<GameObject>[] parcelMarkers = { new List<GameObject>(), new List<GameObject>() };
        private void SyncParcelMarkers(FarmModel model)
        {
            for (int parcel = 0; parcel < 2; parcel++)
            {
                bool locked = model.State.plots.Count < FarmModel.PlotCount + (parcel + 1) * FarmModel.Width;
                var markers = parcelMarkers[parcel];
                if (!locked)
                {
                    foreach (var marker in markers) if (marker != null) Destroy(marker);
                    markers.Clear();
                    continue;
                }
                if (markers.Count > 0) continue;
                float z = PlotPosition(FarmModel.PlotCount + parcel * FarmModel.Width).z;
                var sign = new GameObject("Land parcel sign " + (parcel + 1));
                sign.transform.SetParent(transform, false);
                sign.transform.localPosition = new Vector3(0, 0.17f, z);
                Shape(PrimitiveType.Cube, "Sign post", sign.transform, new Vector3(0, 0.3f, 0), new Vector3(0.09f, 0.6f, 0.09f), Hex("9A7A4E"));
                Shape(PrimitiveType.Cube, "Sign plank", sign.transform, new Vector3(0, 0.62f, 0), new Vector3(0.85f, 0.4f, 0.07f), Hex("D9B981"));
                markers.Add(sign);
                for (int i = 0; i < 4; i++)
                    markers.Add(Shape(PrimitiveType.Cube, "Claim rope", transform,
                        new Vector3(-2.65f + i * 1.77f, 0.19f, z), new Vector3(1.5f, 0.025f, 0.025f), Hex("C8A06A")));
            }
        }

        public void Refresh(FarmModel model, double now, int selected)
        {
            for (int i = 0; i < plots.Length; i++) if (plots[i] != null) plots[i].Refresh(model, now, i == selected);
        }

        private GameObject EnvironmentModel(string name, Vector3 position, float scale)
        {
            var prefab = Resources.Load<GameObject>("Environment/" + name);
            if (prefab == null) return null;
            var instance = Instantiate(prefab, transform);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * scale;
            return instance;
        }

        private GameObject StructureModel(string name, Vector3 position, float scale, float yaw)
        {
            var prefab = Resources.Load<GameObject>("Structures/" + name);
            if (prefab == null) return null;
            var wrapper = new GameObject(name);
            wrapper.transform.SetParent(transform, false);
            wrapper.transform.localPosition = position;
            wrapper.transform.localScale = Vector3.one * scale;
            wrapper.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            var model = Instantiate(prefab, wrapper.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            if(System.Array.IndexOf(BuildingLevelView.Types,name)>=0 && Resources.Load<GameObject>("Structures/Levels/"+name+"_L2")!=null)
                wrapper.AddComponent<BuildingLevelView>().Configure(name,2,model);
            return wrapper;
        }

        private void Animal(string name, Vector3 position, float radius, float speed, int seed)
        {
            var prefab = Resources.Load<GameObject>("Animals/" + name);
            if (prefab == null) return;
            var instance = Instantiate(prefab, transform);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0, 145 + seed, 0);
            instance.AddComponent<FarmAnimalWander>().Configure(position, radius, speed, seed);
        }

        private void Fish(string name, Vector3 center, float radiusX, float radiusZ, float scale, int seed)
        {
            var prefab = Resources.Load<GameObject>("Animals/" + name);
            if (prefab == null) return;
            var instance = Instantiate(prefab, transform);
            instance.transform.localScale = Vector3.one * scale;
            instance.AddComponent<FarmFishSwim>().Configure(center, radiusX, radiusZ, 0.25f + seed % 3 * 0.04f, seed);
        }

        public void MoveFarmerTo(int index)
        {
            // Cosmetic movement along the outside path; the avatar never blocks plot taps.
            farmerTarget = new Vector3(plots[index].transform.position.x, 0.21f, estateLayout?6.2f:3.1f);
        }

        public void Pulse(int index, FarmTool tool)
        {
            if (farmerAvatar != null) farmerAvatar.Perform(tool);
            // Blue = water, leaf green = plant, gold = harvest; gold alone made sowing look like dropped produce.
            Color color = tool == FarmTool.Water ? Hex("79CEDA") : tool == FarmTool.Plant ? Hex("8FD36E") : Gold;
            Vector3 origin = plots[index].transform.position;
            for (int i = 0; i < 6; i++)
            {
                var dot = Shape(PrimitiveType.Sphere, "Action sparkle", transform,
                    origin + new Vector3(Mathf.Cos(i * 1.047f) * 0.32f, 0.4f, Mathf.Sin(i * 1.047f) * 0.32f), Vector3.one * 0.07f, color);
                var drift = dot.AddComponent<FarmSparkle>();
                drift.Initialize(new Vector3(Mathf.Cos(i) * 0.15f, 0.55f, Mathf.Sin(i) * 0.15f));
                effects.Add(dot);
            }
            if (tool != FarmTool.Harvest || GameCamera == null) return;
            // Hay Day-style payoff: golden coins arc from the plot toward the wallet
            // corner of the screen (top-left), synced with the HUD coin pop.
            Vector3 target = GameCamera.transform.position + GameCamera.transform.forward * 2.2f
                + GameCamera.transform.up * 1.0f - GameCamera.transform.right * 1.6f;
            for (int i = 0; i < 3; i++)
            {
                var coin = Instantiate(Resources.Load<GameObject>("Collection/Coin"), transform);
                coin.name = "Flying coin";
                coin.transform.position = origin + new Vector3((i - 1) * 0.18f, 0.5f, 0);
                coin.transform.rotation = Quaternion.FromToRotation(Vector3.up, -GameCamera.transform.forward);
                coin.transform.localScale = Vector3.one * .7f;
                coin.AddComponent<FarmCoinFly>().Initialize(target, 0.55f + i * 0.12f);
                effects.Add(coin);
            }
        }

        private void Update()
        {
            if (farmer != null)
            {
                Vector3 delta = farmerTarget - farmer.position;
                if (farmerAvatar != null) farmerAvatar.SetWalking(delta.sqrMagnitude > 0.002f);
                if (delta.sqrMagnitude > 0.002f)
                {
                    farmer.position = Vector3.MoveTowards(farmer.position, farmerTarget, Time.deltaTime * 1.15f);
                    farmer.rotation = Quaternion.Slerp(farmer.rotation, Quaternion.LookRotation(delta), Time.deltaTime * 8);
                }
            }
            if (boat != null) boat.localPosition = new Vector3(-7.4f, -0.48f + Mathf.Sin(Time.time * 1.2f) * 0.045f, -5.3f);
            if (Time.unscaledTime > nextCleanup)
            {
                nextCleanup = Time.unscaledTime + 5;
                effects.RemoveAll(item => item == null);
            }
        }

        // Hay Day-style vibrancy: crop instances swap to saturated copies of their
        // baked materials. Copies are cached per original and never written back to
        // the prefab assets, so the baked library stays untouched.
        public void Vivify(GameObject root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                {
                    Material original = slots[i];
                    if (original == null || !original.HasProperty("_BaseColor")) continue;
                    Material vivid;
                    if (!vividMaterials.TryGetValue(original, out vivid))
                    {
                        float h, s, v;
                        Color.RGBToHSV(original.GetColor("_BaseColor"), out h, out s, out v);
                        Color boosted = Color.HSVToRGB(h, Mathf.Min(1, s * 1.28f + .02f), Mathf.Min(1, v * 1.08f));
                        vivid = new Material(original) { name = original.name + " vivid" };
                        vivid.SetColor("_BaseColor", boosted);
                        if (vivid.HasProperty("_Color")) vivid.color = boosted;
                        vividMaterials.Add(original, vivid);
                    }
                    slots[i] = vivid;
                }
                renderer.sharedMaterials = slots;
            }
        }

        public Material Material(Color color)
        {
            Material material;
            if (materials.TryGetValue(color, out material)) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = "Island " + ColorUtility.ToHtmlStringRGB(color) };
            material.SetColor("_BaseColor", color);
            material.color = color;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0);
            if (material.HasProperty("_SpecColor")) material.SetColor("_SpecColor", Color.black);
            materials.Add(color, material);
            return material;
        }

        public GameObject Shape(PrimitiveType primitive, string name, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var item = GameObject.CreatePrimitive(primitive);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            var collider = item.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; RemoveTransient(collider); }
            var renderer = item.GetComponent<Renderer>();
            renderer.sharedMaterial = Material(color);
            renderer.shadowCastingMode = ShadowCastingMode.On;
            return item;
        }

        public GameObject Imported(string path, Transform parent, Vector3 position, float targetHeight, float maxWidth = float.PositiveInfinity)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null) return null;
            var item = Instantiate(prefab, parent, false);
            item.name = prefab.name;
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) { RemoveTransient(item); return null; }
            bool human = item.GetComponent<FarmerAvatar>() != null;
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
                Material[] source = renderer.sharedMaterials;
                for (int i = 0; i < source.Length; i++)
                {
                    Material original = source[i];
                    // Human eyes use an alpha-tested texture; flattening their
                    // materials to the scenery palette loses the iris and eyelids.
                    if (human && original != null) continue;
                    if (original == null) { source[i] = Material(Leaf); continue; }
                    Material replacement;
                    if (!importedMaterials.TryGetValue(original, out replacement))
                    {
                        Color color = original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") :
                            original.HasProperty("_Color") ? original.GetColor("_Color") : Leaf;
                        replacement = Material(color);
                        importedMaterials.Add(original, replacement);
                    }
                    source[i] = replacement;
                }
                renderer.sharedMaterials = source;
                renderer.shadowCastingMode = ShadowCastingMode.On;
            }
            float ratio = targetHeight / Mathf.Max(0.01f, bounds.size.y);
            ratio = Mathf.Min(ratio, maxWidth / Mathf.Max(0.01f, Mathf.Max(bounds.size.x, bounds.size.z)));
            // Scale about the prefab origin, then move its bottom-center onto the requested ground point.
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            Vector3 offset = bottomCenter - item.transform.position;
            item.transform.localScale *= ratio;
            item.transform.localPosition = position - offset * ratio;
            foreach (Collider collider in item.GetComponentsInChildren<Collider>()) { collider.enabled = false; RemoveTransient(collider); }
            return item;
        }

        private void Disc(string name, float radius, float y, float depth, Color color, int segments)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float b = (i + 1) * Mathf.PI * 2 / segments;
                float ra = radius * (1 + Mathf.Sin(i * 3.1f) * 0.035f);
                float rb = radius * (1 + Mathf.Sin(((i + 1) % segments) * 3.1f) * 0.035f);
                Vector3 p = new Vector3(Mathf.Cos(a) * ra, y, Mathf.Sin(a) * ra * 0.85f);
                Vector3 q = new Vector3(Mathf.Cos(b) * rb, y, Mathf.Sin(b) * rb * 0.85f);
                int start = vertices.Count;
                vertices.Add(new Vector3(0, y, 0)); vertices.Add(q); vertices.Add(p);
                vertices.Add(p); vertices.Add(q); vertices.Add(q - Vector3.up * depth);
                vertices.Add(p); vertices.Add(q - Vector3.up * depth); vertices.Add(p - Vector3.up * depth);
                for (int j = 0; j < 9; j++) triangles.Add(start + j);
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals();
            var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(transform, false);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = Material(color);
        }

        public LineRenderer Ring(Transform parent, Vector3 center, float radius, Color color, float thickness, int segments = 24)
        {
            var item = new GameObject("Ground ring");
            item.transform.SetParent(parent, false);
            var line = item.AddComponent<LineRenderer>();
            line.useWorldSpace = false; line.loop = true; line.positionCount = segments;
            line.widthMultiplier = thickness;
            line.sharedMaterial = Material(color);
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                line.SetPosition(i, center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius);
            }
            return line;
        }

        internal static void FitIslandCrop(GameObject plant)
        {
            var renderers=plant.GetComponentsInChildren<Renderer>();
            if(renderers.Length>0){Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);float width=Mathf.Max(bounds.size.x,bounds.size.z);plant.transform.localScale*=Mathf.Min(1,1.02f/Mathf.Max(.01f,width));}
            foreach(var collider in plant.GetComponentsInChildren<Collider>())collider.enabled=false;
        }
        public GameObject Crop(Transform parent, string cropId, int stage)
        {
            string expansionId=IslandCropCatalog.DesignId(cropId);
            if(expansionId!=null)
            {
                var prefab=Resources.Load<GameObject>("IslandExpansion/Crops/"+expansionId+"_"+Mathf.Clamp(stage,1,5));
                if(prefab==null)return null;
                var plant=Instantiate(prefab,parent,false);plant.name="IX_"+expansionId+"_"+Mathf.Clamp(stage,1,5);plant.transform.localPosition=Vector3.up*.008f;
                FitIslandCrop(plant);
                foreach(var c in plant.GetComponentsInChildren<Collider>())c.enabled=false;return plant;
            }
            string nativeName = CropCatalog.Find(cropId)?.modelName;
            if (nativeName != null)
            {
                var nativePrefab = Resources.Load<GameObject>("Crops/" + nativeName + "/" + nativeName + "_" + Mathf.Clamp(stage, 1, 5));
                if (nativePrefab != null)
                {
                    var native = Instantiate(nativePrefab, parent);
                    native.transform.localPosition = new Vector3(0, 0.008f, 0);
                    native.transform.localRotation = Quaternion.identity;
                    native.transform.localScale = Vector3.one;
                    return native;
                }
            }
            string modelName = cropId == "carrot" ? "Carrot" : cropId == "corn" ? "Corn" : "Pumpkin";
            // Each growth model is normalized independently; stage heights intentionally increase.
            float height = cropId == "corn" ? 0.25f + stage * 0.26f : 0.15f + stage * 0.14f;
            var imported = Imported("Art/UltimateCrops/Models/" + modelName + "_" + stage, parent, new Vector3(0, 0.13f, 0), height, 0.95f);
            if (imported != null) return imported;
            var root = new GameObject(modelName + " stage " + stage);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0, 0.14f, 0);
            float size = 0.25f + stage * 0.15f;
            if (cropId == "pumpkin")
            {
                if (stage > 1) Shape(PrimitiveType.Sphere, "Pumpkin", root.transform, new Vector3(0, size * 0.25f, 0), new Vector3(size, size * 0.55f, size), Hex("E99C43"));
                Shape(PrimitiveType.Cylinder, "Stem", root.transform, new Vector3(0, size * 0.6f, 0), new Vector3(0.07f, 0.12f, 0.07f), Leaf);
            }
            else if (cropId == "corn")
            {
                Shape(PrimitiveType.Cylinder, "Corn stem", root.transform, new Vector3(0, height * 0.5f, 0), new Vector3(0.065f, height * 0.5f, 0.065f), Leaf);
                if (stage >= 3) Shape(PrimitiveType.Capsule, "Corn cob", root.transform, new Vector3(0.11f, height * 0.65f, 0), new Vector3(0.17f, 0.2f, 0.17f), Gold);
            }
            else Shape(PrimitiveType.Sphere, "Carrot shoulder", root.transform, new Vector3(0, 0.06f, 0), new Vector3(size * 0.5f, size * 0.32f, size * 0.5f), Hex("F39B4B"));
            for (int i = 0; i < 4; i++)
            {
                var leaf = Shape(PrimitiveType.Sphere, "Crop leaf", root.transform, new Vector3(Mathf.Cos(i * 1.57f) * size * 0.18f, height * 0.55f, Mathf.Sin(i * 1.57f) * size * 0.18f), new Vector3(size * 0.24f, size * 0.7f, size * 0.18f), Leaf);
                leaf.transform.localRotation = Quaternion.Euler(Mathf.Sin(i * 1.57f) * 35, 0, Mathf.Cos(i * 1.57f) * 35);
            }
            return root;
        }

        private void Cottage(Vector3 position)
        {
            var root = new GameObject("Farm cottage"); root.transform.SetParent(transform); root.transform.position = position;
            Shape(PrimitiveType.Cube, "Cream walls", root.transform, new Vector3(0, 0.85f, 0), new Vector3(2.6f, 1.7f, 2), Hex("F1DBA8"));
            Shape(PrimitiveType.Cube, "Door", root.transform, new Vector3(-0.4f, 0.6f, -1.01f), new Vector3(0.55f, 1.2f, 0.05f), Hex("64877D"));
            Shape(PrimitiveType.Cube, "Window", root.transform, new Vector3(0.65f, 0.9f, -1.02f), new Vector3(0.5f, 0.55f, 0.05f), Hex("89CDD0"));
            for (int side = -1; side <= 1; side += 2)
            {
                var roof = Shape(PrimitiveType.Cube, "Terracotta roof", root.transform, new Vector3(side * 0.72f, 1.95f, 0), new Vector3(1.65f, 0.18f, 2.5f), Hex("BB6A4E"));
                roof.transform.localRotation = Quaternion.Euler(0, 0, side * -27);
            }
        }

        private void Well(Vector3 position)
        {
            var root = new GameObject("Village well"); root.transform.SetParent(transform); root.transform.position = position;
            Shape(PrimitiveType.Cylinder, "Well stone", root.transform, new Vector3(0, 0.35f, 0), new Vector3(1, 0.35f, 1), Hex("B7B5A0"));
            Shape(PrimitiveType.Cylinder, "Well water", root.transform, new Vector3(0, 0.71f, 0), new Vector3(0.65f, 0.01f, 0.65f), Hex("62B8C6"));
            for (int i = -1; i <= 1; i += 2) Shape(PrimitiveType.Cube, "Well post", root.transform, new Vector3(i * 0.48f, 0.9f, 0), new Vector3(0.1f, 1.6f, 0.1f), Hex("977652"));
            Shape(PrimitiveType.Cube, "Well roof", root.transform, new Vector3(0, 1.75f, 0), new Vector3(1.4f, 0.15f, 1.1f), Hex("BB6A4E"));
        }

        private void Windmill(Vector3 position)
        {
            var root = new GameObject("Windmill"); root.transform.SetParent(transform); root.transform.position = position;
            Shape(PrimitiveType.Cylinder, "Windmill tower", root.transform, new Vector3(0, 1.3f, 0), new Vector3(1.4f, 1.3f, 1.4f), Hex("E4D4AA"));
            var rotor = new GameObject("Rotor"); rotor.transform.SetParent(root.transform, false); rotor.transform.localPosition = new Vector3(0, 2.4f, -0.8f);
            Shape(PrimitiveType.Cube, "Vertical blade", rotor.transform, Vector3.zero, new Vector3(0.2f, 2.8f, 0.08f), Hex("F8ECC6"));
            Shape(PrimitiveType.Cube, "Horizontal blade", rotor.transform, Vector3.zero, new Vector3(2.8f, 0.2f, 0.08f), Hex("F8ECC6"));
            rotor.AddComponent<FarmRotor>();
        }

        private void Fence(Vector3 position, bool rotate)
        {
            var root = new GameObject("Wood fence"); root.transform.SetParent(transform); root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0, rotate ? 90 : 0, 0);
            for (int i = -1; i <= 1; i += 2) Shape(PrimitiveType.Cube, "Fence post", root.transform, new Vector3(i * 0.48f, 0.3f, 0), new Vector3(0.1f, 0.6f, 0.12f), Hex("DDC394"));
            Shape(PrimitiveType.Cube, "Fence rail", root.transform, new Vector3(0, 0.38f, 0), new Vector3(1.1f, 0.1f, 0.09f), Hex("E9D3A9"));
            Shape(PrimitiveType.Cube, "Fence rail", root.transform, new Vector3(0, 0.18f, 0), new Vector3(1.1f, 0.08f, 0.09f), Hex("E9D3A9"));
        }

        private void Palm(Vector3 position, float scale)
        {
            if(InFarmClearing(position))return;
            var prefab = Resources.Load<GameObject>("Nature/CoconutPalm");
            if (prefab != null)
            {
                var palm = Instantiate(prefab, transform);
                palm.transform.localPosition = position;
                palm.transform.localScale = Vector3.one * scale;
                palm.transform.localRotation = Quaternion.Euler(0, position.x * 17, 0);
                return;
            }
            var root = new GameObject("Island palm"); root.transform.SetParent(transform); root.transform.position = position; root.transform.localScale = Vector3.one * scale;
            Shape(PrimitiveType.Cylinder, "Palm trunk", root.transform, new Vector3(0, 1.15f, 0), new Vector3(0.23f, 1.15f, 0.23f), Hex("AF875E"));
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3;
                var leaf = Shape(PrimitiveType.Sphere, "Palm frond", root.transform, new Vector3(Mathf.Sin(angle) * 0.65f, 2.3f, Mathf.Cos(angle) * 0.65f), new Vector3(0.55f, 0.15f, 1.8f), Hex("588F60"));
                leaf.transform.localRotation = Quaternion.Euler(12, i * 60, 0);
            }
            Shape(PrimitiveType.Sphere, "Coconut", root.transform, new Vector3(0.17f, 2.08f, -0.12f), Vector3.one * 0.23f, Hex("926D4B"));
        }

        private void Tree(Vector3 position, float scale)
        {
            if(InFarmClearing(position))return;
            var root = new GameObject("Fruit tree"); root.transform.SetParent(transform); root.transform.position = position; root.transform.localScale = Vector3.one * scale;
            Shape(PrimitiveType.Cylinder, "Trunk", root.transform, new Vector3(0, 0.6f, 0), new Vector3(0.25f, 0.6f, 0.25f), Hex("8A7151"));
            Shape(PrimitiveType.Sphere, "Tree crown", root.transform, new Vector3(0, 1.5f, 0), new Vector3(1.7f, 1.4f, 1.7f), Hex("79A45B"));
            for (int i = 0; i < 3; i++) Shape(PrimitiveType.Sphere, "Golden fruit", root.transform, new Vector3(Mathf.Cos(i * 2.1f) * 0.67f, 1.3f, Mathf.Sin(i * 2.1f) * 0.67f), Vector3.one * 0.18f, Gold);
        }

        private void Flowers(Vector3 position)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(Mathf.Sin(i * 2.4f) * 0.45f, 0, Mathf.Cos(i * 2.4f) * 0.45f);
                Shape(PrimitiveType.Cylinder, "Flower stem", transform, position + offset + Vector3.up * 0.13f, new Vector3(0.04f, 0.14f, 0.04f), Leaf);
                Shape(PrimitiveType.Sphere, "Island flower", transform, position + offset + Vector3.up * 0.3f, new Vector3(0.18f, 0.08f, 0.18f), i % 2 == 0 ? Gold : Hex("D89793"));
            }
        }

        private Transform Farmer(Vector3 position)
        {
            GameObject prefab = Resources.Load<GameObject>("Characters/Farmer");
            farmerAvatar = prefab != null ? Instantiate(prefab, transform, false).GetComponent<FarmerAvatar>() : FarmerModelBuilder.Create(transform);
            farmerAvatar.transform.position = position;
            farmerAvatar.transform.rotation = Quaternion.Euler(0, 225, 0);
            return farmerAvatar.transform;
        }

        private void MakeBoat()
        {
            var harbor=new GameObject("Expanded coast harbor").transform;harbor.SetParent(transform,false);harbor.localPosition=new Vector3(-45,0,-30);
            // Walkway reaches from the beach path to the pier beyond the new shoreline.
            var walkway=Shape(PrimitiveType.Cube,"Harbor boardwalk",transform,new Vector3(-47.1f,.12f,-31.25f),new Vector3(1.25f,.18f,10.5f),Hex("B58D62"));
            walkway.transform.localRotation=Quaternion.Euler(0,51.6f,0);
            Shape(PrimitiveType.Cube, "Trading pier", harbor, new Vector3(-6.1f, -0.12f, -4.5f), new Vector3(3.2f, 0.16f, 1.1f), Hex("A68158"));
            for (int i = 0; i < 8; i++) Shape(PrimitiveType.Cube, "Pier plank", harbor, new Vector3(-4.7f - i * 0.38f, -0.02f, -4.5f), new Vector3(0.31f, 0.04f, 1.1f), Hex("C19D6C"));
            var root = new GameObject("Thương nhân Hải Âu"); root.transform.SetParent(harbor,false); boat = root.transform;
            boat.localPosition = new Vector3(-7.4f, -0.48f, -5.3f);
            root.layer = InteractionLayer;
            root.AddComponent<FarmMarket>();
            var collider = root.AddComponent<BoxCollider>(); collider.center = Vector3.up * 0.7f; collider.size = new Vector3(1.5f, 1.8f, 2.7f);
            Shape(PrimitiveType.Sphere, "Boat hull", boat, Vector3.zero, new Vector3(1.2f, 0.45f, 2.6f), Hex("8B6952"));
            Shape(PrimitiveType.Cube, "Boat deck", boat, new Vector3(0, 0.2f, 0), new Vector3(1.1f, 0.12f, 2), Hex("D9B981"));
            Shape(PrimitiveType.Cylinder, "Mast", boat, new Vector3(0, 1.05f, 0), new Vector3(0.065f, 0.85f, 0.065f), Hex("896B4C"));
            Shape(PrimitiveType.Cube, "Cream sail", boat, new Vector3(0.36f, 1.3f, 0), new Vector3(0.7f, 0.8f, 0.035f), Hex("FFF1CE"));
            Shape(PrimitiveType.Cube, "Cargo crate", boat, new Vector3(-0.18f, 0.47f, -0.6f), new Vector3(0.45f, 0.45f, 0.45f), Hex("B9915E"));
        }

        private void OnDestroy()
        {
            foreach(var mesh in townMeshes) if(mesh!=null) RemoveTransient(mesh);
            foreach (Material material in materials.Values)
                if (material != null) RemoveTransient(material);
            foreach (Material material in vividMaterials.Values)
                if (material != null) RemoveTransient(material);
            foreach (Material material in glowMaterials.Values)
                if (material != null) RemoveTransient(material);
        }
        private static void RemoveTransient(Object item)
        {
            if (Application.isPlaying) Destroy(item);
            else DestroyImmediate(item);
        }
        public static Color Hex(string hex) { Color color; ColorUtility.TryParseHtmlString("#" + hex, out color); return color; }
    }

    public sealed class FarmMarket : MonoBehaviour { }
    public sealed class FarmRotor : MonoBehaviour
    {
        private void Update() { transform.Rotate(0, 0, Time.deltaTime * 18); }
    }
    public sealed class FarmCoinFly : MonoBehaviour
    {
        private Vector3 start, target;
        private float duration, age;
        public void Initialize(Vector3 target, float duration)
        { start = transform.position; this.target = target; this.duration = duration; }
        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            float eased = t * t * (3 - 2 * t);
            Vector3 p = Vector3.Lerp(start, target, eased);
            p.y += Mathf.Sin(t * Mathf.PI) * 0.9f; // playful arc
            transform.position = p;
            transform.localScale = Vector3.one * Mathf.Lerp(.7f, .18f, t);
            transform.Rotate(Vector3.up, Time.deltaTime * 180, Space.Self);
            if (t >= 1) Destroy(gameObject);
        }
    }

    public sealed class FarmSparkle : MonoBehaviour
    {
        private Vector3 velocity;
        private float remaining = 0.7f;
        public void Initialize(Vector3 velocity) { this.velocity = velocity; }
        private void Update()
        {
            remaining -= Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Max(0, remaining * 0.1f);
            if (remaining <= 0) Destroy(gameObject);
        }
    }
}
