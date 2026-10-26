using System;
using UnityEngine;
using static HarvestIsle.Editor.CropMesh;

namespace HarvestIsle.Editor
{
    // Authored geometry for the five-column reference sheets. No soil is baked into plants.
    internal static partial class ReferenceCropBuilder
    {
        internal static GameObject Create(string name, int stage, Material[] materials, out Mesh mesh)
        {
            if (stage < 1 || stage > 5) throw new ArgumentOutOfRangeException(nameof(stage));
            var m = new CropMesh();
            switch (name)
            {
                case "Rice": case "Wheat": Grain(m, stage, name == "Rice"); break;
                case "Corn": Corn(m, stage); break;
                case "Sugarcane": CanePlant(m, stage); break;
                case "Carrot": case "Radish": Root(m, stage, name == "Carrot"); break;
                case "Onion": case "Garlic": case "Lemongrass": Blades(m, stage, name); break;
                case "Pumpkin": case "Watermelon": Vine(m, stage, name == "Watermelon"); break;
                case "Beans": case "Peas": Climber(m, stage, name == "Peas"); break;
                case "Greens": case "Cabbage": Rosette(m, stage, name == "Cabbage"); break;
                case "Tomato": case "Chili": BushFruit(m, stage, name == "Chili"); break;
                case "Strawberry": Strawberry(m, stage); break;
                case "Marigold": Marigold(m, stage); break;
                case "WaterSpinach": Spinach(m, stage); break;
                case "Taro": Taro(m, stage); break;
                case "Lotus": Lotus(m, stage); break;
                case "Mushroom": Mushroom(m, stage); break;
                case "SnowTea": Tea(m, stage); break;
                case "Mango": case "Orange": case "Lemon": Orchard(m, stage, name); break;
                case "Coconut": case "Banana": case "Papaya": Tropical(m, stage, name); break;
                default: throw new ArgumentException("Unknown reference crop: " + name);
            }
            mesh = m.Finish(name + "_" + stage);
            var root = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
            root.GetComponent<MeshFilter>().sharedMesh = mesh;
            root.GetComponent<MeshRenderer>().sharedMaterials = materials;
            return root;
        }
        private static Vector3 D(float a) => new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
        private static void Rod(CropMesh m, Vector3 a, Vector3 b, float r, int color = Stem)
            => m.Tube(new[] { a, b }, r, r * .65f, color);
        private static void Sprout(CropMesh m, float h = .12f, int leaves = 2)
        {
            Rod(m, Vector3.zero, Vector3.up * h, .009f);
            for (int i = 0; i < leaves; i++) Vein(m, Vector3.up * h, D(i * 2.4f), .14f, .055f, .35f, LeafLight);
        }
        private static void Vein(CropMesh m, Vector3 p, Vector3 d, float length, float width, float lift,
            int color = Green, int lobes = 0)
        {
            m.Leaf(p, d, length, width, lift, color, lobes);
            Vector3 end = p + d * length * (1 - lift * .25f) + Vector3.up * length * (lift - .15f);
            m.Tube(new[] { p, (p + end) * .5f + Vector3.up * length * .23f, end }, .0035f, .001f, LeafLight, 4);
        }
        private static void Fern(CropMesh m, Vector3 p, Vector3 d, float length, float lift)
        {
            Vector3 end = p + d * length * .7f + Vector3.up * length * lift;
            Rod(m, p, end, .005f);
            Vector3 side = Vector3.Cross(Vector3.up, d);
            for (int j = 1; j < 6; j++) for (int sign = -1; sign <= 1; sign += 2)
                m.Leaf(Vector3.Lerp(p, end, j / 6f), (d * .35f + side * sign).normalized,
                    length * .25f * (1 - j * .1f), length * .05f, .1f, j % 2 == 0 ? Green : LeafLight, 2);
        }
        private static void Root(CropMesh m, int s, bool carrot)
        {
            float shoulder = s < 3 ? .025f : .045f + (s - 2) * .045f;
            if (s >= 2) m.Profile(Vector3.zero, new[] {
                new Vector3(.004f, -.08f, .004f), new Vector3(shoulder * .7f, .02f, shoulder * .7f),
                new Vector3(shoulder, .07f + s * .014f, shoulder), new Vector3(shoulder * .65f, .10f + s * .018f, shoulder * .65f)
            }, carrot ? Orange : White);
            var p = Vector3.up * (s >= 2 ? .10f + s * .018f : .04f);
            for (int i = 0; i < 1 + s * 2; i++)
            {
                var d = D(i * 2.4f);
                if (carrot) Fern(m, p, d, .10f + s * .075f, .7f + i % 3 * .2f);
                else Vein(m, p, d, .09f + s * .085f, .02f + s * .017f, .7f, i % 2 == 0 ? Green : LeafLight, 3);
            }
        }
        private static void Blades(CropMesh m, int s, string name)
        {
            bool garlic = name == "Garlic", onion = name == "Onion";
            int shoots = garlic ? 1 : s < 3 ? 1 : s - 1;
            for (int k = 0; k < shoots; k++)
            {
                Vector3 p = D(k * 2.4f) * (k == 0 ? 0 : .055f);
                if (garlic) m.Ellipsoid(p + Vector3.up * (.025f + s * .008f), new Vector3(.025f + s * .018f, .03f + s * .018f, .025f + s * .018f), White, sides: 24, lobes: 8);
                else Rod(m, p, p + Vector3.up * (.04f + s * .02f), .008f + s * .004f, Cream);
                for (int j = 0; j < s + 1; j++)
                {
                    Vector3 d = D(j * 2.4f + k);
                    float h = .1f + s * .09f;
                    if (onion) m.Tube(new[] { p, p + Vector3.up * h * .6f + d * .04f, p + Vector3.up * h + d * (.04f + s * .02f) }, .009f + s * .002f, .001f, Green);
                    else m.Leaf(p + Vector3.up * .08f, d, .12f + s * .11f, .012f + s * .004f, j % 3 == 0 ? 1.25f : .7f, garlic && s == 5 && j == 1 ? Yellow : Green, bend: .3f);
                }
            }
        }
        private static void Grain(CropMesh m, int s, bool rice)
        {
            int count = s == 1 ? 2 : 2 + s;
            for (int k = 0; k < count; k++)
            {
                Vector3 d = D(k * 2.4f), p = d * (k == 0 ? 0 : .045f + s * .006f);
                float h = .07f + s * .115f - k % 3 * .025f;
                Vector3 top = p + Vector3.up * h + d * .04f;
                Rod(m, p, top, .008f, s == 5 ? OrangeDark : Stem);
                for (int j = 0; j < (s == 1 ? 2 : 3); j++)
                    m.Leaf(Vector3.Lerp(p, top, .15f + j * .24f), D(k * 2.4f + j * 2), .10f + s * .055f, .012f + s * .002f, .6f, s == 5 ? Yellow : Green, bend: .35f);
                if (s < 4) continue;
                int c = s == 5 ? Yellow : Young;
                if (rice)
                {
                    Vector3 end = top + d * .22f - Vector3.up * .08f;
                    m.Tube(new[] { top, top + d * .11f + Vector3.up * .05f, end }, .005f, .002f, c);
                    for (int j = 0; j < 5; j++)
                    {
                        Vector3 q = Vector3.Lerp(top, end, (j + 1) / 5f) + Vector3.up * .03f;
                        Vector3 e = q + D(k + j * 2) * .055f - Vector3.up * .065f;
                        Rod(m, q, e, .002f, c);
                        for (int g = 0; g < 3; g++) m.Ellipsoid(Vector3.Lerp(q, e, .35f + g * .3f), new Vector3(.012f, .021f, .01f), c, sides: 6, rows: 4);
                    }
                }
                else for (int j = 0; j < 5; j++) for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 q = top + Vector3.up * j * .027f + d * side * .018f;
                    m.Ellipsoid(q, new Vector3(.02f, .031f, .016f), c, sides: 8, rows: 4);
                    Rod(m, q, q + Vector3.up * .09f + d * side * .025f, .0015f, c);
                }
            }
        }
        private static void Corn(CropMesh m, int s)
        {
            float h = new[] { 0f, .10f, .28f, .68f, .93f, 1.12f }[s];
            Rod(m, Vector3.zero, Vector3.up * h, .009f + s * .004f);
            for (int i = 0; i < s + 2; i++)
                Vein(m, Vector3.up * h * (.12f + i * .1f), D(i * 2.7f), .10f + s * .09f, .012f + s * .015f, .5f);
            if (s < 4) return;
            for (int i = 0; i < 5; i++)
            {
                Vector3 tip = Vector3.up * (h + .10f) + D(i * 2.4f) * .12f;
                Rod(m, Vector3.up * (h - .04f), tip, .003f, Yellow);
                for (int j = 0; j < 3; j++) m.Ellipsoid(Vector3.Lerp(Vector3.up * h, tip, j / 3f), new Vector3(.008f, .027f, .008f), Yellow, sides: 6, rows: 3);
            }
            for (int k = 0; k < 2; k++)
            {
                Vector3 p = D(k * 3.1f) * .075f + Vector3.up * h * (.45f + k * .17f);
                m.Ellipsoid(p, new Vector3(.055f, .16f, .055f), s == 5 ? Yellow : Young);
                for (int j = 0; j < 3; j++) m.Leaf(p - Vector3.up * .15f, D(j * 2.1f), .28f, .042f, 1f, Green);
                if (s == 5) for (int row = 0; row < 7; row++) for (int col = 0; col < 8; col++)
                    m.Ellipsoid(p + D(col * Mathf.PI / 4) * .052f + Vector3.up * (-.10f + row * .03f), new Vector3(.015f, .014f, .012f), Yellow, sides: 6, rows: 3);
            }
        }
        private static void CanePlant(CropMesh m, int s)
        {
            int count = s < 3 ? 1 : s;
            for (int k = 0; k < count; k++)
            {
                Vector3 p = D(k * 2.4f) * (k == 0 ? 0 : .12f);
                float h = .08f + s * .15f - k % 2 * .10f;
                Rod(m, p, p + Vector3.up * h, .012f + s * .008f, Cane);
                for (int j = 1; j <= s; j++) m.Ellipsoid(p + Vector3.up * (h * j / (s + 1)), new Vector3(.014f + s * .008f, .012f, .014f + s * .008f), Cream, sides: 8, rows: 4);
                for (int j = 0; j < s + 1; j++) m.Leaf(p + Vector3.up * h * .8f, D(j * 2.4f + k), .12f + s * .08f, .02f + s * .006f, .7f, Green, bend: .6f);
            }
        }
    }
}
