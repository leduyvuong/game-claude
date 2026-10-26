using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle
{
    public sealed partial class FarmWorld
    {
        // Purely decorative dressing: existing prefabs plus lightweight ambient motion.
        // Colliders are disabled, so plot taps and farm-check raycasts stay unaffected.
        private void BuildScenery()
        {
            // Soft distance haze melts the horizon into the sea; it starts beyond the island
            // silhouette so farm colors stay saturated even at full zoom-out.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Hex("8CD4CE");
            RenderSettings.fogStartDistance = 170;
            RenderSettings.fogEndDistance = 360;

            for (int i = 0; i < 4; i++) Cloud(i);
            for (int i = 0; i < 3; i++) ShoreFoam(63.6f + i * .9f, -.68f + i * .004f, i);
            for (int i = 0; i < 3; i++) FlockBird(i);

            string[] beach = { "BeachShell", "Driftwood", "FallenCoconut", "BeachShell", "Driftwood", "FallenCoconut", "BeachShell", "FallenCoconut" };
            for (int i = 0; i < beach.Length; i++)
            {
                float angle = .35f + i * Mathf.PI * 2 / beach.Length + Mathf.Sin(i * 5.2f) * .14f;
                Vector3 p = new Vector3(Mathf.Cos(angle) * 58.2f, 0, Mathf.Sin(angle) * 49.2f);
                if (InFarmClearing(p) || InTownZone(p)) continue;
                p.y = Mathf.Max(.02f, ExpandedHeight(p.x, p.z));
                Prop(beach[i], p, beach[i] == "Driftwood" ? .2f : .15f, angle * 57.3f);
            }

            if(estateLayout)return;
            string[] flora = { "Fern", "Shrub", "Wildflowers", "BambooClump", "Fern", "Wildflowers", "Shrub", "Wildflowers", "Fern", "BambooClump", "Shrub", "Wildflowers" };
            for (int i = 0; i < flora.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / flora.Length + .55f;
                float radius = 11.6f + 2.6f * Mathf.Sin(i * 3.7f);
                Vector3 p = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius * .86f);
                if (InFarmClearing(p) || InTownZone(p)) continue;
                p.y = ExpandedHeight(p.x, p.z);
                Prop(flora[i], p, flora[i] == "BambooClump" ? 1.7f : flora[i] == "Wildflowers" ? .28f : .4f, i * 73f);
            }

            Prop("Bench", new Vector3(-2.9f, .17f, 4.35f), .5f, 8);
            Prop("Pinwheel", new Vector3(2.6f, .21f, 2.55f), .85f, -25);
            Prop("SteppingStones", new Vector3(-4.9f, .16f, -1.9f), .1f, 12);
            Prop("Basket", new Vector3(-45.2f, 0, -30.5f), .32f, 40);
            Prop("FishingRod", new Vector3(-46.4f, 0, -30.5f), .75f, 205);

            Pollinator("Bee", new Vector3(3.9f, .5f, -2.8f), .55f, 2.3f);
            Pollinator("Ladybug", new Vector3(-3.5f, .42f, 3.1f), .4f, 1.7f);
            Pollinator("GoldenFirefly", new Vector3(-5.9f, .85f, 4.8f), .7f, 1.1f);

            BuildVillageLayout();
            BuildTown();
        }

        // Hay Day-style zoning: dirt roads, a fenced sheep pen, an orchard, a village
        // corner and prop vignettes. Everything stays outside the ranch/development
        // clearings and the playable field; nothing here touches economy or save.
        private void BuildVillageLayout()
        {
            // One north-south lane east of the field; the old east-west branch made
            // way for the two unlockable parcels extending the field southward.
            Road(new Vector3(4.6f, 0, 2.6f), new Vector3(4.6f, 0, -5.0f));

            // Sheep pen with a gate gap on the north side.
            for (int i = 0; i < 3; i++)
            {
                if (i != 1) Fence(new Vector3(-6.7f + i * 1.1f, .17f, -2.8f), false);
                Fence(new Vector3(-6.7f + i * 1.1f, .17f, -5.0f), false);
            }
            for (int i = 0; i < 2; i++)
            {
                Fence(new Vector3(-7.25f, .17f, -3.35f - i * 1.1f), true);
                Fence(new Vector3(-3.95f, .17f, -3.35f - i * 1.1f), true);
            }
            Animal("Sheep", new Vector3(-18.3f, .15f, -13.8f), .3f, .1f, 17);
            Animal("Sheep", new Vector3(-17.0f, .15f, -14.3f), .3f, .09f, 29);
            Animal("Duck", new Vector3(-17.7f, .15f, -13.3f), .3f, .12f, 41);

            // Orchard rows of grafted fruit trees on the north-east rise.
            string[] orchard = { "GraftedMango", "GraftedOrange", "DualFruitTree" };
            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 3; column++)
            {
                Vector3 p = new Vector3(5.7f + column * 1.25f, 0, 4.9f + row * 1.35f);
                p.y = ExpandedHeight(p.x, p.z);
                Prop(orchard[(row + column) % 3], p, 1.55f, row * 40 + column * 25);
            }
            Prop("FallenCoconut", new Vector3(6.4f, ExpandedHeight(6.4f, 4.4f), 4.4f), .15f, 60);

            // Village corner: festival gate over the road, lanterns, order board,
            // campfire with benches and a cookpot, watchtower on the rise.
            Prop("FestivalArch", new Vector3(4.6f, .16f, 2.3f), 2f, 0);
            Prop("FestivalLantern", new Vector3(3.95f, .16f, 1.7f), .85f, 20);
            Prop("FestivalLantern", new Vector3(5.25f, .16f, 1.7f), .85f, -20);
            Prop("OrderBoard", new Vector3(3.8f, .16f, 2.4f), 1.1f, 205);
            Prop("Campfire", new Vector3(6.9f, .15f, .3f), .5f, 0);
            Prop("Bench", new Vector3(6.2f, .15f, -.3f), .48f, 135);
            Prop("Bench", new Vector3(7.6f, .15f, .9f), .48f, -45);
            Prop("VillageCookpot", new Vector3(7.5f, .15f, -.4f), .8f, -70);
            Prop("Watchtower", new Vector3(8.7f, ExpandedHeight(8.7f, 3.5f), 3.5f), 2.6f, 160);
            Prop("VillageWarehouse", new Vector3(-4.6f, .15f, -4.5f), 1.6f, 90);

            // Hedge of shrubs between the field and the road, stones at the crossing.
            for (int i = 0; i < 4; i++) Prop("Shrub", new Vector3(3.95f, .15f, -2.5f + i * 1.15f), .42f, i * 85);
            Prop("StonePile", new Vector3(5.3f, .15f, -4.9f), .4f, 30);

            // Farm-tool vignette at the north-west field corner.
            Prop("Toolbox", new Vector3(-4.7f, .16f, 2.1f), .26f, 25);
            Prop("WateringCan", new Vector3(-4.35f, .16f, 1.75f), .24f, 140);
            Prop("Bucket", new Vector3(-4.95f, .16f, 1.65f), .24f, 80);
            Prop("SeedBag", new Vector3(-4.45f, .16f, 2.45f), .3f, 190);
            Prop("WoodBundle", new Vector3(-5.3f, .16f, 2.3f), .3f, 10);

            // Flower beds flanking the well path.
            EnvironmentModel("FlowerMeadowTile", new Vector3(-2.2f, .152f, 5.6f), .7f);
            EnvironmentModel("FlowerMeadowTile", new Vector3(2.1f, .152f, 6.1f), .7f);
        }

        private void Road(Vector3 from, Vector3 to)
        {
            float yaw = Mathf.Atan2(to.x - from.x, to.z - from.z) * Mathf.Rad2Deg;
            int steps = Mathf.Max(2, Mathf.RoundToInt(Vector3.Distance(from, to) / .92f));
            for (int i = 0; i <= steps; i++)
            {
                Vector3 p = Vector3.Lerp(from, to, (float)i / steps);
                if (InFarmClearing(p)) continue;
                p.y = Mathf.Max(.02f, ExpandedHeight(p.x, p.z)) + .012f;
                var tile = Imported("Collection/DirtPath", transform, p, .05f, 1.05f);
                if (tile == null) continue;
                tile.transform.localRotation = Quaternion.Euler(0, yaw + (i % 3 - 1) * 6, 0);
                // Warm the tiles toward packed earth so the road separates from sand and stone.
                foreach (var renderer in tile.GetComponentsInChildren<Renderer>())
                {
                    var slots = renderer.sharedMaterials;
                    for (int s = 0; s < slots.Length; s++)
                    {
                        if (slots[s] == null) continue;
                        Color c = slots[s].HasProperty("_BaseColor") ? slots[s].GetColor("_BaseColor") : Color.white;
                        slots[s] = Material(new Color(c.r * .84f, c.g * .7f, c.b * .52f, 1));
                    }
                    renderer.sharedMaterials = slots;
                }
            }
        }

        private void Prop(string name, Vector3 position, float height, float yaw)
        {
            var item = Imported("Collection/" + name, transform, position, height, 1.8f);
            if (item != null) item.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        }

        private void Cloud(int seed)
        {
            var root = new GameObject("Drifting cloud " + seed);
            root.transform.SetParent(transform, false);
            // Drift lanes avoid the field strip (z -3..2) so close-up views stay clear.
            float[] lanes = { -12f, 7f, 10.5f, -8.5f };
            root.transform.localPosition = new Vector3(-20f + seed * 11f, 16f + seed % 3 * 1.6f, lanes[seed % 4]);
            for (int i = 0; i < 4; i++)
            {
                var puff = Shape(PrimitiveType.Sphere, "Cloud puff", root.transform,
                    new Vector3(i * .85f - 1.3f, Mathf.Sin(i * 2.1f) * .18f, Mathf.Cos(i * 1.3f) * .4f),
                    new Vector3(1.6f - Mathf.Abs(i - 1.5f) * .3f, .75f, 1.1f), Hex("FDFDF6"));
                // No shadows: moving cloud shade would read as a weather change on the field.
                puff.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
            root.AddComponent<FarmCloudDrift>().Initialize(.22f + seed * .05f, 26f);
        }

        private void ShoreFoam(float radius, float y, int seed)
        {
            var line = Ring(transform, new Vector3(0, y, 0), radius, Hex("EAF9F4"), .07f, 72);
            line.name = "Shore foam " + seed;
            line.transform.localScale = new Vector3(1, 1, .84f);
            line.gameObject.AddComponent<FarmShoreFoam>().Initialize(seed);
        }

        private void FlockBird(int seed)
        {
            var prefab = Resources.Load<GameObject>("Animals/" + (seed == 1 ? "Bulbul" : "Sparrow"));
            if (prefab == null) return;
            var bird = Instantiate(prefab, transform);
            bird.name = "Sky bird " + seed;
            bird.transform.localScale = Vector3.one * .85f;
            foreach (var collider in bird.GetComponentsInChildren<Collider>()) collider.enabled = false;
            bird.AddComponent<FarmBirdFlight>().Initialize(new Vector3(0, 6.2f + seed * .9f, -1.5f),
                11.5f + seed * 1.4f, 9.2f + seed * 1.1f, .16f + seed * .03f, seed * 2.09f);
        }

        private void Pollinator(string name, Vector3 center, float radius, float speed)
        {
            var item = Imported("Collection/" + name, transform, center, .16f, .3f);
            if (item == null) return;
            item.name = "Pollinator " + name;
            item.AddComponent<FarmFlutter>().Initialize(center, radius, speed);
        }
    }

    public sealed class FarmCloudDrift : MonoBehaviour
    {
        private float speed, limit;
        public void Initialize(float speed, float limit) { this.speed = speed; this.limit = limit; }
        private void Update()
        {
            Vector3 p = transform.localPosition;
            p.x += speed * Time.deltaTime;
            if (p.x > limit) p.x = -limit;
            transform.localPosition = p;
        }
    }

    public sealed class FarmShoreFoam : MonoBehaviour
    {
        private LineRenderer line;
        private float phase;
        private Vector3 baseScale;
        public void Initialize(int seed) { line = GetComponent<LineRenderer>(); phase = seed * 2.1f; baseScale = transform.localScale; }
        private void Update()
        {
            float wave = Mathf.Sin(Time.time * .8f + phase);
            line.widthMultiplier = .055f + .03f * (wave * .5f + .5f);
            transform.localScale = baseScale * (1f + .008f * wave);
        }
    }

    public sealed class FarmBirdFlight : MonoBehaviour
    {
        private FarmAnimalAvatar avatar;
        private Vector3 center;
        private float radiusX, radiusZ, speed, phase;
        public void Initialize(Vector3 center, float radiusX, float radiusZ, float speed, float phase)
        {
            this.center = center; this.radiusX = radiusX; this.radiusZ = radiusZ; this.speed = speed; this.phase = phase;
            avatar = GetComponentInChildren<FarmAnimalAvatar>();
        }
        private void Update()
        {
            float a = phase + Time.time * speed;
            float bob = Mathf.Sin(Time.time * 2.1f + phase) * .25f;
            transform.localPosition = center + new Vector3(Mathf.Cos(a) * radiusX, bob, Mathf.Sin(a) * radiusZ);
            transform.localRotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(a) * radiusX, 0, Mathf.Cos(a) * radiusZ)) * Quaternion.Euler(0, 0, -12);
            if (avatar != null) avatar.SetWalking(true);
        }
    }

    public sealed class FarmFlutter : MonoBehaviour
    {
        private Vector3 center;
        private float radius, speed, phase;
        public void Initialize(Vector3 center, float radius, float speed)
        { this.center = center; this.radius = radius; this.speed = speed; phase = center.x * 3.7f; }
        private void Update()
        {
            float t = Time.time * speed + phase;
            Vector3 p = center + new Vector3(Mathf.Sin(t) * radius, Mathf.Sin(t * 2.6f) * .12f + .08f, Mathf.Cos(t * .8f) * radius * .8f);
            Vector3 forward = p - transform.localPosition;
            if (forward.sqrMagnitude > 1e-6f)
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(forward), Time.deltaTime * 6);
            transform.localPosition = p;
        }
    }
}
