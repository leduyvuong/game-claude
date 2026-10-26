using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle
{
    /// <summary>Shared sculpting kit for the code-built characters. Each group bakes one mesh per color.</summary>
    internal sealed class FarmerMeshKit
    {
        public readonly List<Mesh> Meshes = new List<Mesh>();
        public readonly List<Material> Materials = new List<Material>();
        private readonly Dictionary<Color, Material> palette = new Dictionary<Color, Material>();
        private readonly List<FarmerMeshGroup> groups = new List<FarmerMeshGroup>();
        private readonly string materialPrefix;

        public FarmerMeshKit(string materialPrefix = "Farmer") { this.materialPrefix = materialPrefix; }

        public FarmerMeshGroup Group(string name, Transform parent)
        {
            return Group(name, parent, Vector3.zero);
        }
        public FarmerMeshGroup Group(string name, Transform parent, Vector3 offset)
        {
            var group = new FarmerMeshGroup(this, name, parent, offset); groups.Add(group); return group;
        }
        public Material Material(Color color)
        {
            Material material;
            if (palette.TryGetValue(color, out material)) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = materialPrefix + " " + ColorUtility.ToHtmlStringRGB(color) };
            material.SetColor("_BaseColor", color); material.color = color;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.1f);
            if (material.HasProperty("_SpecColor")) material.SetColor("_SpecColor", Color.black);
            palette.Add(color, material); Materials.Add(material); return material;
        }
        public void Finish() { foreach (FarmerMeshGroup group in groups) group.Finish(); }

        public static Transform Joint(string name, Transform parent, Vector3 position)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            return root.transform;
        }
    }

    internal sealed class FarmerMeshGroup
    {
        private readonly FarmerMeshKit kit;
        private readonly Transform root;
        private readonly Dictionary<Color, FarmerMeshSurface> surfaces = new Dictionary<Color, FarmerMeshSurface>();
        private bool finished;
        public bool CastShadows = true;
        public FarmerMeshGroup(FarmerMeshKit kit, string name, Transform parent, Vector3 offset)
        {
            this.kit = kit; root = FarmerMeshKit.Joint(name, parent, offset);
        }
        private FarmerMeshSurface Surface(Color color)
        {
            FarmerMeshSurface surface;
            if (!surfaces.TryGetValue(color, out surface)) { surface = new FarmerMeshSurface(); surfaces.Add(color, surface); }
            return surface;
        }
        public void Ellipsoid(Vector3 center, Vector3 size, Color color, int segments, int rings, Quaternion rotation = default(Quaternion))
        {
            if (rotation == default(Quaternion)) rotation = Quaternion.identity;
            var surface = Surface(color); int first = surface.Vertices.Count;
            for (int y = 0; y <= rings; y++)
            {
                float latitude = -Mathf.PI * 0.5f + y * Mathf.PI / rings;
                for (int x = 0; x < segments; x++)
                {
                    float longitude = x * Mathf.PI * 2 / segments;
                    surface.Vertices.Add(center + rotation * Vector3.Scale(size * 0.5f,
                        new Vector3(Mathf.Cos(latitude) * Mathf.Cos(longitude), Mathf.Sin(latitude), Mathf.Cos(latitude) * Mathf.Sin(longitude))));
                }
            }
            for (int y = 0; y < rings; y++)
            for (int x = 0; x < segments; x++)
            {
                int a = first + y * segments + x;
                int b = first + y * segments + (x + 1) % segments;
                int c = a + segments; int d = b + segments;
                if (y > 0) surface.Tri(a, c, b);
                if (y < rings - 1) surface.Tri(b, c, d);
            }
        }
        public void Profile(Vector3[] profile, Color color, int segments)
        {
            var sections = new Vector4[profile.Length];
            for (int i = 0; i < profile.Length; i++) sections[i] = new Vector4(profile[i].x, profile[i].y, profile[i].z, 0);
            Loft(sections, color, segments);
        }
        public void Loft(Vector4[] profile, Color color, int segments, float roundness = 1)
        {
            var surface = Surface(color); int first = surface.Vertices.Count;
            foreach (Vector4 ring in profile)
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                float c = Mathf.Cos(angle); float s = Mathf.Sin(angle);
                surface.Vertices.Add(new Vector3(Mathf.Sign(c) * Mathf.Pow(Mathf.Abs(c), roundness) * ring.x,
                    ring.y, Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), roundness) * ring.z + ring.w));
            }
            for (int row = 0; row < profile.Length - 1; row++)
            for (int x = 0; x < segments; x++)
            {
                int a = first + row * segments + x;
                int b = first + row * segments + (x + 1) % segments;
                surface.Tri(a, a + segments, b); surface.Tri(b, a + segments, b + segments);
            }
            int bottom = surface.Vertices.Count; surface.Vertices.Add(new Vector3(0, profile[0].y, profile[0].w));
            int top = surface.Vertices.Count; surface.Vertices.Add(new Vector3(0, profile[profile.Length - 1].y, profile[profile.Length - 1].w));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                surface.Tri(bottom, first + i, first + next);
                int last = first + (profile.Length - 1) * segments;
                surface.Tri(top, last + next, last + i);
            }
        }
        public void HairCap(Color color, int segments, int rows, Func<float, Vector4> section)
        {
            var surface = Surface(color); int first = surface.Vertices.Count;
            for (int row = 0; row <= rows; row++)
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float front = Mathf.Max(0, Mathf.Sin(a));
                float low = Mathf.Sin(a) >= 0 ? Mathf.Lerp(0.024f, 0.171f, front * front)
                    : Mathf.Lerp(0.024f, -0.085f, -Mathf.Sin(a));
                float y = Mathf.Lerp(low, 0.264f, (float)row / rows);
                Vector4 ring = section(y);
                float c = Mathf.Cos(a); float s = Mathf.Sin(a);
                surface.Vertices.Add(new Vector3(Mathf.Sign(c) * Mathf.Pow(Mathf.Abs(c), 0.8f) * (ring.x + 0.010f),
                    y + 0.004f, Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), 0.8f) * (ring.z + 0.010f) + ring.w));
            }
            for (int row = 0; row < rows; row++)
            for (int i = 0; i < segments; i++)
            {
                int a = first + row * segments + i;
                int b = first + row * segments + (i + 1) % segments;
                surface.Tri(a, a + segments, b); surface.Tri(b, a + segments, b + segments);
            }
        }
        public void Box(Vector3 center, Vector3 size, Color color)
        {
            var surface = Surface(color); Vector3 h = size * 0.5f;
            Vector3[] v = {
                center + new Vector3(-h.x, -h.y, -h.z), center + new Vector3(h.x, -h.y, -h.z),
                center + new Vector3(h.x, h.y, -h.z), center + new Vector3(-h.x, h.y, -h.z),
                center + new Vector3(-h.x, -h.y, h.z), center + new Vector3(h.x, -h.y, h.z),
                center + new Vector3(h.x, h.y, h.z), center + new Vector3(-h.x, h.y, h.z)
            };
            int[] faces = { 0, 3, 2, 1, 4, 5, 6, 7, 0, 4, 7, 3, 1, 2, 6, 5, 3, 7, 6, 2, 0, 1, 5, 4 };
            for (int i = 0; i < faces.Length; i += 4)
            {
                int first = surface.Vertices.Count;
                for (int k = 0; k < 4; k++) surface.Vertices.Add(v[faces[i + k]]);
                surface.Tri(first, first + 1, first + 2); surface.Tri(first, first + 2, first + 3);
            }
        }
        public void Triangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            var surface = Surface(color); int first = surface.Vertices.Count;
            surface.Vertices.Add(a); surface.Vertices.Add(b); surface.Vertices.Add(c);
            surface.Vertices.Add(c); surface.Vertices.Add(b); surface.Vertices.Add(a);
            surface.Tri(first, first + 1, first + 2); surface.Tri(first + 3, first + 4, first + 5);
        }
        public void Tube(Vector3[] path, float radius, Color color, int sides)
        {
            var surface = Surface(color); int first = surface.Vertices.Count;
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 tangent = (path[Mathf.Min(i + 1, path.Length - 1)] - path[Mathf.Max(0, i - 1)]).normalized;
                Vector3 normal = Vector3.Cross(tangent, Mathf.Abs(tangent.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
                Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
                for (int side = 0; side < sides; side++)
                {
                    float a = side * Mathf.PI * 2 / sides;
                    surface.Vertices.Add(path[i] + radius * (normal * Mathf.Cos(a) + binormal * Mathf.Sin(a)));
                }
            }
            for (int i = 0; i < path.Length - 1; i++)
            for (int side = 0; side < sides; side++)
            {
                int a = first + i * sides + side;
                int b = first + i * sides + (side + 1) % sides;
                surface.Tri(a, b, a + sides); surface.Tri(b, b + sides, a + sides);
            }
        }
        public void ConeHat(float radius, float height, Color outside, Color inside, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments; float b = (i + 1) * Mathf.PI * 2 / segments;
                Vector3 p = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
                Vector3 q = new Vector3(Mathf.Cos(b) * radius, 0, Mathf.Sin(b) * radius);
                var top = Surface(outside); int start = top.Vertices.Count;
                top.Vertices.Add(p); top.Vertices.Add(Vector3.up * height); top.Vertices.Add(q);
                top.Tri(start, start + 1, start + 2);
                var under = Surface(inside); start = under.Vertices.Count;
                under.Vertices.Add(p); under.Vertices.Add(q); under.Vertices.Add(Vector3.up * (height - 0.012f));
                under.Tri(start, start + 1, start + 2);
            }
        }
        public void Finish()
        {
            if (finished) return;
            finished = true;
            foreach (var entry in surfaces)
            {
                var mesh = new Mesh { name = root.name + " " + ColorUtility.ToHtmlStringRGB(entry.Key) };
                mesh.SetVertices(entry.Value.Vertices); mesh.SetTriangles(entry.Value.Triangles, 0);
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                var item = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
                item.transform.SetParent(root, false);
                item.GetComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = item.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = kit.Material(entry.Key);
                renderer.shadowCastingMode = CastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                kit.Meshes.Add(mesh);
            }
        }
    }

    internal sealed class FarmerMeshSurface
    {
        public readonly List<Vector3> Vertices = new List<Vector3>();
        public readonly List<int> Triangles = new List<int>();
        public void Tri(int a, int b, int c) { Triangles.Add(a); Triangles.Add(b); Triangles.Add(c); }
    }
}
