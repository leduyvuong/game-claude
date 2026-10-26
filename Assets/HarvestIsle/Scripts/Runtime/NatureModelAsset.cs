using UnityEngine;

namespace HarvestIsle
{
    public sealed class NatureModelAsset : ScriptableObject
    {
        public int version;
        public Mesh[] meshes;
        public Material[] materials;
    }
}
