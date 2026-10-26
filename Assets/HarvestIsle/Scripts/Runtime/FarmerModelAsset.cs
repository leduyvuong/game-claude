using UnityEngine;

namespace HarvestIsle
{
    public sealed class FarmerModelAsset : ScriptableObject
    {
        public int version;
        public Mesh[] meshes;
        public Material[] materials;
    }
}
