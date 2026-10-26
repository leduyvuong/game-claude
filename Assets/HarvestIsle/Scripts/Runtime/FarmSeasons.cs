using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle
{
    /// <summary>
    /// Cosmetic season cycle for the island's decorative flora: spring and summer
    /// stay green, autumn turns canopies amber and drops leaves. Crops, characters
    /// and buildings are untouched; gameplay rules do not read the season.
    /// </summary>
    public sealed class FarmSeasons : MonoBehaviour
    {
        private const float SeasonSeconds = 150f;
        private static readonly string[] Names = { "Mùa xuân", "Mùa hè", "Mùa thu" };
        private static readonly string[] NatureHints = { "Tree", "Palm", "Fern", "Shrub", "Bamboo", "Hill", "Wildflowers", "Grafted", "Forest" };

        private FarmGame game;
        private readonly Dictionary<Material, Material> clones = new Dictionary<Material, Material>();
        private readonly Dictionary<Material, Color> original = new Dictionary<Material, Color>();
        private ParticleSystem leaves;
        private float weight;      // 0 = green, 1 = full autumn
        private float applied = -1;
        private int announced = -1;

        public void Initialize(FarmGame owner)
        {
            game = owner;
            CollectFoliage();
            BuildLeafFall();
            announced = SeasonIndex(Time.time); // no toast over the welcome message
        }

        private void CollectFoliage()
        {
            foreach (var renderer in game.World.GetComponentsInChildren<MeshRenderer>(true))
            {
                Transform root = renderer.transform;
                bool nature = false;
                while (root != null && root != game.World.transform)
                {
                    foreach (string hint in NatureHints) if (root.name.Contains(hint)) { nature = true; break; }
                    if (nature) break;
                    root = root.parent;
                }
                if (!nature || renderer.GetComponentInParent<FarmPlot>() != null) continue;
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                {
                    Material mat = slots[i];
                    if (mat == null || !mat.HasProperty("_BaseColor")) continue;
                    Color color = mat.GetColor("_BaseColor");
                    if (!(color.g > color.r * 1.12f && color.g > color.b * 1.12f)) continue;
                    Material clone;
                    if (!clones.TryGetValue(mat, out clone))
                    {
                        clone = new Material(mat) { name = mat.name + " season" };
                        clones.Add(mat, clone);
                        original.Add(clone, color);
                    }
                    slots[i] = clone;
                }
                renderer.sharedMaterials = slots;
            }
        }

        private void BuildLeafFall()
        {
            var item = new GameObject("Autumn leaves");
            item.transform.SetParent(game.World.transform, false);
            item.transform.localPosition = new Vector3(0, 7.5f, 0);
            leaves = item.AddComponent<ParticleSystem>();
            var main = leaves.main;
            main.startColor = new ParticleSystem.MinMaxGradient(FarmWorld.Hex("E8A23F"), FarmWorld.Hex("C96F33"));
            main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.16f);
            main.startLifetime = 9f; main.startSpeed = 0.1f; main.maxParticles = 220; main.loop = true;
            main.gravityModifier = 0.045f;
            var emission = leaves.emission; emission.rateOverTime = 16;
            var shape = leaves.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(34, 1, 30);
            var rotation = leaves.rotationOverLifetime; rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-2.4f, 2.4f);
            var renderer = item.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            renderer.sharedMaterial = new Material(shader) { name = "Autumn leaf" };
            leaves.Stop();
        }

        public static int SeasonIndex(float time) { return (int)(time / SeasonSeconds) % 3; }

        private void Update()
        {
            if (game == null) return;
            int season = SeasonIndex(Time.time);
            if (season != announced)
            {
                announced = season;
                game.Notify(Names[season] + (season == 2 ? " về — rừng ngả vàng, lá bắt đầu rơi." : " — tán cây xanh mướt trở lại."));
            }
            float target = season == 2 ? 1 : 0;
            weight = Mathf.MoveTowards(weight, target, Time.deltaTime * 0.2f);
            if (!Mathf.Approximately(weight, applied))
            {
                applied = weight;
                foreach (var pair in original)
                {
                    Color green = pair.Value;
                    Color autumn = new Color(Mathf.Min(1, green.r * 1.45f + 0.12f), green.g * 0.72f, green.b * 0.32f, green.a);
                    pair.Key.SetColor("_BaseColor", Color.Lerp(green, autumn, weight));
                }
            }
            if (weight > 0.45f && !leaves.isPlaying) leaves.Play();
            else if (weight <= 0.45f && leaves.isPlaying) leaves.Stop();
        }

        private void OnDestroy()
        {
            foreach (var clone in clones.Values) if (clone != null) Destroy(clone);
        }
    }
}
