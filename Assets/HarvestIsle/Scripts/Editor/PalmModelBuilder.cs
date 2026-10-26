using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    /// <summary>Original coconut palm geometry, in metres, with its origin at ground level.</summary>
    internal static class PalmModelBuilder
    {
        internal const int Version = 1;
        private static readonly Color[] Palette = {
            FarmWorld.Hex("997353"), FarmWorld.Hex("B18A65"), FarmWorld.Hex("735840"),
            FarmWorld.Hex("426F45"), FarmWorld.Hex("659346"), FarmWorld.Hex("8DA958"),
            FarmWorld.Hex("B1B969")
        };

        internal static GameObject Create(out Mesh[] meshes, out Material[] materials)
        {
            var root = new GameObject("Coconut Palm - Dua dao");
            materials = new Material[Palette.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
                material.name = "Palm " + ColorUtility.ToHtmlStringRGB(Palette[i]);
                material.SetColor("_BaseColor", Palette[i]);
                material.SetColor("_SpecColor", Color.black);
                material.SetFloat("_Smoothness", 0.05f);
                materials[i] = material;
            }
            var trunk = new Geometry();
            var crown = new Geometry();
            BuildTrunk(trunk);
            Vector3 heart = TrunkPoint(1);
            for (int i = 0; i < 9; i++)
            {
                float angle = (i * 40 + 12) * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                float length = 1.55f + 0.20f * Mathf.Sin(i * 2.31f);
                float lift = 0.23f + 0.08f * Mathf.Cos(i * 1.9f);
                Frond(crown, heart + Vector3.up * (i % 3 * 0.025f), direction, length, lift, i, false);
            }
            // Two younger, upright fronds give the crown an uneven, natural silhouette.
            for (int i = 0; i < 2; i++)
            {
                float angle = (i * 145 + 63) * Mathf.Deg2Rad;
                Frond(crown, heart + Vector3.up * 0.03f, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)),
                    0.89f, 0.53f, i + 9, true);
            }
            for (int i = 0; i < 7; i++)
            {
                float a = (i * 137 + 22) * Mathf.Deg2Rad;
                float ring = i < 4 ? 0.275f : 0.170f;
                Vector3 center = heart + new Vector3(Mathf.Cos(a) * ring, -0.26f - (i % 3) * 0.085f, Mathf.Sin(a) * ring);
                crown.Ellipsoid(center, new Vector3(0.145f, 0.181f, 0.140f), 6, 12, 8);
                crown.Ellipsoid(center - Vector3.up * 0.175f, new Vector3(0.029f, 0.023f, 0.028f), 2, 8, 4);
                crown.Tube(new[] { heart - Vector3.up * 0.045f, center + Vector3.up * 0.12f }, 0.016f, 0.008f, 4, 6);
            }
            meshes = new[] { trunk.Finish("Curved ringed trunk"), crown.Finish("Feathered palm crown") };
            AddMesh(root.transform, meshes[0], materials);
            AddMesh(root.transform, meshes[1], materials);
            return root;
        }

        private static void AddMesh(Transform parent, Mesh mesh, Material[] materials)
        {
            var part = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
            part.transform.SetParent(parent, false);
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            part.GetComponent<MeshRenderer>().sharedMaterials = materials;
        }

        private static Vector3 TrunkPoint(float t)
        {
            return new Vector3(-0.08f * Mathf.Sin(t * Mathf.PI) + 0.34f * t * t,
                t * 2.36f, -0.07f * t * t);
        }

        private static float TrunkRadius(float t)
        {
            return Mathf.Lerp(0.149f, 0.099f, t) + 0.079f * Mathf.Exp(-t * 28);
        }

        private static void BuildTrunk(Geometry mesh)
        {
            const int bands = 19;
            for (int i = 0; i < bands; i++)
            {
                float t = (float)i / bands;
                float next = (float)(i + 1) / bands;
                float seam = Mathf.Lerp(t, next, 0.91f);
                mesh.TrunkSegment(t, seam, TrunkRadius(t) + (i == 0 ? 0 : 0.003f), TrunkRadius(seam) + 0.005f, i % 3 == 0 ? 1 : 0);
                mesh.TrunkSegment(seam, next, TrunkRadius(seam) + 0.005f, TrunkRadius(next) + 0.003f, 2);
            }
            mesh.Ellipsoid(TrunkPoint(1), new Vector3(0.14f, 0.13f, 0.14f), 4, 12, 6);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2 / 5;
                mesh.Tube(new[] { new Vector3(Mathf.Cos(a) * 0.31f, 0.015f, Mathf.Sin(a) * 0.31f),
                    new Vector3(Mathf.Cos(a) * 0.15f, 0.053f, Mathf.Sin(a) * 0.15f), TrunkPoint(0.065f) },
                    0.027f, 0.052f, 0, 7);
            }
        }

        private static Vector3 FrondPoint(Vector3 basePoint, Vector3 direction, float length, float lift, float t, bool upright)
        {
            float height = upright ? lift * Mathf.Sin(t * Mathf.PI * 0.68f)
                : lift * Mathf.Sin(t * Mathf.PI) - 0.40f * t * t;
            Vector3 sideways = Vector3.Cross(Vector3.up, direction);
            return basePoint + direction * (length * t) + Vector3.up * height + sideways * (0.07f * t * t);
        }

        private static void Frond(Geometry mesh, Vector3 basePoint, Vector3 direction, float length, float lift, int seed, bool upright)
        {
            var spine = new Vector3[15];
            for (int i = 0; i < spine.Length; i++)
                spine[i] = FrondPoint(basePoint, direction, length, lift, (float)i / (spine.Length - 1), upright);
            mesh.Tube(spine, 0.025f, 0.004f, 4, 6);
            Vector3 sideDirection = Vector3.Cross(Vector3.up, direction);
            const int count = 12;
            for (int i = 0; i < count; i++)
            for (int side = -1; side <= 1; side += 2)
            {
                float t = 0.13f + i * 0.066f + (side == 1 ? 0.011f : 0);
                float spread = Mathf.Sin(t * Mathf.PI) * (upright ? 0.28f : 0.42f) + 0.035f;
                Vector3 start = FrondPoint(basePoint, direction, length, lift, t, upright);
                Vector3 tip = start + sideDirection * side * spread + direction * (0.14f + t * 0.09f)
                    - Vector3.up * (0.12f + 0.10f * t);
                float width = (upright ? 0.036f : 0.055f) * Mathf.Sin(t * Mathf.PI) + 0.008f;
                mesh.Leaflet(start, tip, width, sideDirection * side, 3 + (seed + i + (side == 1 ? 1 : 0)) % 3);
            }
            mesh.Leaflet(spine[spine.Length - 3], spine[spine.Length - 1] + direction * 0.09f,
                0.031f, sideDirection, 4);
        }

        private sealed class Geometry
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<int>[] triangles = new List<int>[Palette.Length];
            public Geometry() { for (int i = 0; i < triangles.Length; i++) triangles[i] = new List<int>(); }
            private void Triangle(Vector3 a, Vector3 b, Vector3 c, int material)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a);
                if (normal.sqrMagnitude < 0.0000000001f) return;
                int first = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                triangles[material].Add(first); triangles[material].Add(first + 1); triangles[material].Add(first + 2);
            }
            private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int material)
            {
                Triangle(a, b, c, material); Triangle(a, c, d, material);
            }
            private void LeafQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int material)
            {
                if (Vector3.Cross(b - a, c - a).y < 0) Quad(d, c, b, a, material);
                else Quad(a, b, c, d, material);
                if (Vector3.Cross(b - a, c - a).y < 0) Quad(a, b, c, d, 3);
                else Quad(d, c, b, a, 3);
            }
            public void TrunkSegment(float low, float high, float r0, float r1, int material)
            {
                const int sides = 14;
                Vector3 p0 = TrunkPoint(low); Vector3 p1 = TrunkPoint(high);
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2 / sides; float b = (i + 1) * Mathf.PI * 2 / sides;
                    Vector3 ra = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                    Vector3 rb = new Vector3(Mathf.Cos(b), 0, Mathf.Sin(b));
                    Quad(p0 + ra * r0, p1 + ra * r1, p1 + rb * r1, p0 + rb * r0, material);
                }
            }
            public void Leaflet(Vector3 start, Vector3 end, float width, Vector3 side, int material)
            {
                Vector3 tangent = (end - start).normalized;
                Vector3 across = Vector3.Cross(tangent, Vector3.up).normalized;
                if (Vector3.Dot(across, side) < 0) across = -across;
                Vector3[] middle = new Vector3[5];
                Vector3[] left = new Vector3[5];
                Vector3[] right = new Vector3[5];
                for (int i = 0; i < 5; i++)
                {
                    float t = (float)i / 4;
                    Vector3 center = Vector3.Lerp(start, end, t) + Vector3.up * (0.05f * Mathf.Sin(t * Mathf.PI));
                    float w = width * Mathf.Sin(t * Mathf.PI);
                    middle[i] = center + Vector3.up * (w * 0.25f);
                    left[i] = center - across * w;
                    right[i] = center + across * w;
                }
                for (int i = 0; i < 4; i++)
                {
                    // A folded leaf, with separate underside vertices/normals for all viewing angles.
                    LeafQuad(middle[i], left[i], left[i + 1], middle[i + 1], material);
                    LeafQuad(middle[i], middle[i + 1], right[i + 1], right[i], material);
                }
            }
            public void Tube(Vector3[] path, float r0, float r1, int material, int sides)
            {
                var rings = new Vector3[path.Length, sides];
                for (int i = 0; i < path.Length; i++)
                {
                    Vector3 tangent = (path[Mathf.Min(i + 1, path.Length - 1)] - path[Mathf.Max(0, i - 1)]).normalized;
                    Vector3 across = Vector3.Cross(tangent, Mathf.Abs(tangent.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
                    Vector3 up = Vector3.Cross(tangent, across);
                    float r = Mathf.Lerp(r0, r1, (float)i / (path.Length - 1));
                    for (int j = 0; j < sides; j++)
                    {
                        float a = j * Mathf.PI * 2 / sides;
                        rings[i, j] = path[i] + r * (across * Mathf.Cos(a) + up * Mathf.Sin(a));
                    }
                }
                for (int i = 0; i < path.Length - 1; i++)
                for (int j = 0; j < sides; j++)
                {
                    int next = (j + 1) % sides;
                    Quad(rings[i, j], rings[i, next], rings[i + 1, next], rings[i + 1, j], material);
                }
                for (int j = 0; j < sides; j++)
                {
                    int next = (j + 1) % sides;
                    Triangle(path[0], rings[0, next], rings[0, j], material);
                    int last = path.Length - 1;
                    Triangle(path[last], rings[last, j], rings[last, next], material);
                }
            }
            public void Ellipsoid(Vector3 center, Vector3 radius, int material, int sides, int rows)
            {
                var rings = new Vector3[rows + 1, sides];
                for (int y = 0; y <= rows; y++)
                for (int i = 0; i < sides; i++)
                {
                    float latitude = -Mathf.PI * 0.5f + y * Mathf.PI / rows;
                    float a = i * Mathf.PI * 2 / sides;
                    rings[y, i] = center + Vector3.Scale(radius, new Vector3(Mathf.Cos(latitude) * Mathf.Cos(a),
                        Mathf.Sin(latitude), Mathf.Cos(latitude) * Mathf.Sin(a)));
                }
                for (int y = 0; y < rows; y++)
                for (int i = 0; i < sides; i++)
                {
                    int next = (i + 1) % sides;
                    Quad(rings[y, i], rings[y + 1, i], rings[y + 1, next], rings[y, next], material);
                }
            }
            public Mesh Finish(string name)
            {
                var mesh = new Mesh { name = name, indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                mesh.SetVertices(vertices);
                mesh.subMeshCount = triangles.Length;
                for (int i = 0; i < triangles.Length; i++) mesh.SetTriangles(triangles[i], i);
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
