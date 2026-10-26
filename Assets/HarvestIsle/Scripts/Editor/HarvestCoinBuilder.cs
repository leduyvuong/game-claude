using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    // Native geometry shared by the catalog, HUD render and reward effect.
    internal static class HarvestCoinBuilder
    {
        internal static GameObject Create(List<Mesh> meshes)
        {
            var m = new EnvironmentMesh(3);
            // Beveled rim, recessed face, and a warm copper edge. Diameter stays 24 cm.
            float[] radii = {0, .103f, .111f, .12f, .12f, .111f, .103f, 0};
            float[] heights = {-.013f, -.013f, -.019f, -.011f, .011f, .019f, .013f, .013f};
            for (int ring = 0; ring < radii.Length - 1; ring++)
            for (int i = 0; i < 64; i++)
            {
                float a = i * Mathf.PI / 32, b = (i + 1) * Mathf.PI / 32;
                m.Quad(Point(radii[ring], heights[ring], b), Point(radii[ring], heights[ring], a),
                    Point(radii[ring + 1], heights[ring + 1], a), Point(radii[ring + 1], heights[ring + 1], b),
                    ring == 0 || ring == 6 ? 1 : ring == 3 ? 2 : 0);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                // Two rice panicles: generous relief remains legible at HUD size.
                for (int stalk = -1; stalk <= 1; stalk += 2)
                {
                    var path = new Vector3[8];
                    for (int j = 0; j < path.Length; j++)
                        path[j] = Face(stalk * (.005f + .025f * j / 7f), -.042f + j * .015f, side);
                    m.Tube(path, .003f, .002f, 0, 6);
                    for (int j = 2; j < 7; j++)
                    {
                        float x = stalk * (.005f + .025f * j / 7f), z = -.042f + j * .015f;
                        Grain(m, x, z, x + stalk * .023f, z + .013f, side);
                        Grain(m, x, z + .003f, x - stalk * .014f, z + .019f, side);
                    }
                }
                // Two sea ripples below the rice root tie the mark to the island.
                for (int wave = 0; wave < 2; wave++)
                {
                    var path = new Vector3[13];
                    for (int j = 0; j < path.Length; j++)
                    {
                        float x = -.057f + j * .0095f;
                        path[j] = Face(x, -.056f - wave * .015f + Mathf.Sin(j * Mathf.PI / 6) * .004f, side);
                    }
                    m.Tube(path, .003f, .003f, 0, 6);
                }
                // Milled edge marks are geometry, so they survive every viewing angle.
                for (int i = 0; i < 32; i++)
                {
                    float a = i * Mathf.PI / 16;
                    m.Tube(new[] {Point(.112f, side * .017f, a), Point(.117f, side * .012f, a)}, .0012f, .0012f, 2, 4);
                }
            }
            Mesh mesh = m.Finish("Xu Mua Vang — rice and island relief");
            meshes.Add(mesh);
            var root = new GameObject("Coin");
            root.AddComponent<MeshFilter>().sharedMesh = mesh;
            var palette = new[] {"FFCD63", "B87521", "7B461D"};
            var mats = new Material[palette.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(Shader.Find("Universal Render Pipeline/Lit")) {name = "Harvest coin " + i};
                mats[i].color = FarmWorld.Hex(palette[i]);
                mats[i].SetFloat("_Metallic", .25f);
                mats[i].SetFloat("_Smoothness", .38f);
            }
            root.AddComponent<MeshRenderer>().sharedMaterials = mats;
            return root;
        }

        private static Vector3 Point(float r, float y, float angle)
        { return new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r); }
        private static Vector3 Face(float x, float z, int side)
        { return new Vector3(x, side * .017f, z * side); }
        private static void Grain(EnvironmentMesh m, float x, float z, float endX, float endZ, int side)
        {
            Vector3 a = Face(x, z, side), b = Face(endX, endZ, side);
            Vector3 across = Vector3.Cross((b - a).normalized, Vector3.up) * .0045f;
            Vector3 middle = Vector3.Lerp(a, b, .55f);
            Vector3 ridge = middle + Vector3.up * side * .004f;
            Vector3[] contour = {a, middle + across, b, middle - across};
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = contour[i], q = contour[(i + 1) % 4];
                if (Vector3.Cross(q - p, ridge - p).y * side < 0) m.Tri(q, p, ridge, 0);
                else m.Tri(p, q, ridge, 0);
            }
        }
    }
}
