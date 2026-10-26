using System;
namespace HarvestIsle
{
    [Serializable]public sealed class ModelLibraryCatalog{public int version,prefabCount;public ModelLibraryEntry[] models;}
    [Serializable]public sealed class ModelLibraryEntry{public string id,label,group;public ModelLibraryVariant[] variants;}
    [Serializable]public sealed class ModelLibraryVariant{public string label,prefab,resource,preview,views;public int triangles;}
}
