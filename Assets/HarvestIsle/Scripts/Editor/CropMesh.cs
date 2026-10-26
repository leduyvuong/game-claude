using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    /// <summary>Small mesh toolkit for original crop assets; no runtime geometry generation.</summary>
    internal sealed class CropMesh
    {
        internal static readonly string[] Colors = {
            "699C48", "3E7441", "91B959", "5C894A", "D6D7A0", "F28C2B", "C57A2E",
            "D85C43", "F1CF62", "83A65A", "AF8558", "E9DEBA", "A1789D", "437343",
            "A2BC67", "A1AF65", "748950"
        };
        internal const int Green = 0, Dark = 1, LeafLight = 2, Stem = 3, Cream = 4, Orange = 5,
            OrangeDark = 6, Red = 7, Yellow = 8, Young = 9, Wood = 10, White = 11,
            Purple = 12, MelonDark = 13, MelonLight = 14, Cane = 15, CaneRing = 16;
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int>[] triangles = new List<int>[Colors.Length];
        internal CropMesh() { for (int i = 0; i < triangles.Length; i++) triangles[i] = new List<int>(); }

        private void Tri(Vector3 a, Vector3 b, Vector3 c, int color)
        {
            if (Vector3.Cross(b - a, c - a).sqrMagnitude < 0.000000000001f) return;
            int start = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            triangles[color].Add(start); triangles[color].Add(start + 1); triangles[color].Add(start + 2);
        }
        private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int color)
        {
            Tri(a, b, c, color); Tri(a, c, d, color);
        }
        private void LeafSurface(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int color)
        {
            bool flip = Vector3.Cross(b - a, c - a).y < 0;
            if (flip) Quad(d, c, b, a, color); else Quad(a, b, c, d, color);
            if (flip) Quad(a, b, c, d, Dark); else Quad(d, c, b, a, Dark);
        }

        internal void Tube(Vector3[] path, float radius, float tipRadius, int color, int sides = 6)
        {
            var rings = new Vector3[path.Length, sides];
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 tangent = (path[Mathf.Min(i + 1, path.Length - 1)] - path[Mathf.Max(0, i - 1)]).normalized;
                Vector3 across = Vector3.Cross(tangent, Mathf.Abs(tangent.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
                Vector3 normal = Vector3.Cross(tangent, across);
                float r = Mathf.Lerp(radius, tipRadius, (float)i / (path.Length - 1));
                for (int j = 0; j < sides; j++)
                {
                    float a = j * Mathf.PI * 2 / sides;
                    rings[i, j] = path[i] + r * (Mathf.Cos(a) * across + Mathf.Sin(a) * normal);
                }
            }
            for (int i = 0; i < path.Length - 1; i++)
            for (int j = 0; j < sides; j++)
                Quad(rings[i, j], rings[i, (j + 1) % sides], rings[i + 1, (j + 1) % sides], rings[i + 1, j], color);
            for (int j = 0; j < sides; j++)
            {
                Tri(path[0], rings[0, (j + 1) % sides], rings[0, j], color);
                int last = path.Length - 1;
                Tri(path[last], rings[last, j], rings[last, (j + 1) % sides], color);
            }
        }

        internal void Leaf(Vector3 start, Vector3 direction, float length, float width, float lift,
            int color = Green, int lobes = 0, float bend = 0.15f)
        {
            direction.y = 0; direction.Normalize();
            Vector3 across = Vector3.Cross(Vector3.up, direction);
            const int rows = 9;
            var middle = new Vector3[rows]; var left = new Vector3[rows]; var right = new Vector3[rows];
            for (int i = 0; i < rows; i++)
            {
                float t = (float)i / (rows - 1);
                Vector3 p = start + direction * (length * t * (1 - lift * 0.25f))
                    + Vector3.up * length * (lift * t + 0.22f * Mathf.Sin(t * Mathf.PI) - bend * t * t);
                float w = width * Mathf.Pow(Mathf.Max(0, Mathf.Sin(t * Mathf.PI)), width > length * 0.22f ? 0.5f : 0.8f);
                if (lobes > 0) w *= 0.72f + 0.28f * Mathf.Cos(t * lobes * Mathf.PI * 2);
                middle[i] = p + Vector3.up * (w * 0.22f);
                left[i] = p - across * w; right[i] = p + across * w;
            }
            for (int i = 0; i < rows - 1; i++)
            {
                LeafSurface(middle[i], left[i], left[i + 1], middle[i + 1], color);
                LeafSurface(middle[i], middle[i + 1], right[i + 1], right[i], color);
            }
        }

        internal void Ellipsoid(Vector3 center, Vector3 radius, int color, Quaternion rotation = default(Quaternion),
            int sides = 12, int rows = 6, int lobes = 0, int stripeColor = -1)
        {
            if (Quaternion.Dot(rotation, rotation) < 0.000001f) rotation = Quaternion.identity;
            var ring = new Vector3[rows + 1, sides];
            for (int y = 0; y <= rows; y++)
            for (int i = 0; i < sides; i++)
            {
                float latitude = -Mathf.PI * 0.5f + y * Mathf.PI / rows;
                float a = i * Mathf.PI * 2 / sides;
                float r = 1 + (lobes > 0 ? 0.08f * Mathf.Cos(a * lobes) : 0);
                ring[y, i] = center + rotation * Vector3.Scale(radius, new Vector3(r * Mathf.Cos(latitude) * Mathf.Cos(a),
                    Mathf.Sin(latitude), r * Mathf.Cos(latitude) * Mathf.Sin(a)));
            }
            for (int y = 0; y < rows; y++)
            for (int i = 0; i < sides; i++)
            {
                int c = stripeColor >= 0 && i % 4 < 1 ? stripeColor : color;
                Quad(ring[y, i], ring[y + 1, i], ring[y + 1, (i + 1) % sides], ring[y, (i + 1) % sides], c);
            }
        }

        internal void Profile(Vector3 center, Vector3[] sections, int color, int sides = 12)
        {
            var rings = new Vector3[sections.Length, sides];
            for (int row = 0; row < sections.Length; row++)
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2 / sides;
                rings[row, i] = center + new Vector3(Mathf.Cos(a) * sections[row].x, sections[row].y, Mathf.Sin(a) * sections[row].z);
            }
            for (int row = 0; row < sections.Length - 1; row++)
            for (int i = 0; i < sides; i++)
                Quad(rings[row, i], rings[row + 1, i], rings[row + 1, (i + 1) % sides], rings[row, (i + 1) % sides], color);
            for (int i = 0; i < sides; i++)
            {
                Tri(center + Vector3.up * sections[0].y, rings[0, i], rings[0, (i + 1) % sides], color);
                int last = sections.Length - 1;
                Tri(center + Vector3.up * sections[last].y, rings[last, (i + 1) % sides], rings[last, i], color);
            }
        }

        internal void Flower(Vector3 center, float radius, int color, int petals = 5)
        {
            Ellipsoid(center, new Vector3(radius * 0.24f, radius * 0.19f, radius * 0.24f), Yellow, sides: 8, rows: 4);
            for (int i = 0; i < petals; i++)
            {
                float a = i * Mathf.PI * 2 / petals;
                Vector3 d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Leaf(center, d, radius, radius * 0.42f, 0.18f, color, bend: 0.1f);
            }
        }

        internal Mesh Finish(string name)
        {
            var mesh = new Mesh { name = name, indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(vertices); mesh.subMeshCount = triangles.Length;
            for (int i = 0; i < triangles.Length; i++) mesh.SetTriangles(triangles[i], i);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
