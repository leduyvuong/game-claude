using UnityEngine;
using static HarvestIsle.Editor.CropMesh;

namespace HarvestIsle.Editor
{
    internal static class CropCollectionBuilder
    {
        internal const int Version = 5;
        internal static readonly string[] Names = { "Carrot", "Corn", "Pumpkin", "Greens", "Tomato", "Onion", "Beans", "Watermelon", "Sugarcane" };
        internal static readonly string[] Labels = { "Cà rốt", "Ngô", "Bí đỏ", "Cải ngọt", "Cà chua", "Hành", "Đậu leo", "Dưa hấu", "Mía" };
        internal static readonly string[] Ids = { "carrot", "corn", "pumpkin", "greens", "tomato", "onion", "beans", "watermelon", "sugarcane" };

        internal static Material[] Materials()
        {
            var materials = new Material[Colors.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
                material.name = "Crops " + Colors[i];
                material.SetColor("_BaseColor", FarmWorld.Hex(Colors[i]));
                material.SetColor("_SpecColor", Color.black); material.SetFloat("_Smoothness", 0.06f);
                materials[i] = material;
            }
            return materials;
        }

        internal static GameObject Create(int crop, int stage, Material[] materials, out Mesh mesh)
            => ReferenceCropBuilder.Create(Names[crop], stage, materials, out mesh);
    }
}
