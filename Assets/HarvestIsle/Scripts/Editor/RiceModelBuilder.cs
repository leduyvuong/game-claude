using UnityEngine;
namespace HarvestIsle.Editor
{
    internal static class RiceModelBuilder
    {
        internal const int Version = 3;
        internal static readonly string[] StageNames = { "Rice_1_Seedling", "Rice_2_Tillering", "Rice_3_LeafGrowth", "Rice_4_GreenPanicles", "Rice_5_Ripe" };
        internal static GameObject[] Create(out Mesh[] meshes, out Material[] materials)
        {
            materials = CropCollectionBuilder.Materials();
            meshes = new Mesh[5];
            var stages = new GameObject[5];
            for (int stage = 1; stage <= 5; stage++)
                stages[stage - 1] = ReferenceCropBuilder.Create("Rice", stage, materials, out meshes[stage - 1]);
            return stages;
        }
    }
}
