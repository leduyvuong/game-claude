using System;
using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    // Editor cameras do not advance skinning between manual pose samples.
    internal sealed class HumanPoseSnapshot : IDisposable
    {
        private readonly List<SkinnedMeshRenderer> skins = new List<SkinnedMeshRenderer>();
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly List<Mesh> meshes = new List<Mesh>();
        public HumanPoseSnapshot(GameObject model)
        {
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if(!skin.enabled)continue;
                // Include the renderer's scale correction: its ancestor is .85x,
                // while the snapshot will inherit that scale again as a child.
                var mesh=new Mesh();skin.BakeMesh(mesh,true);mesh.RecalculateBounds();meshes.Add(mesh);
                var item=new GameObject("Pose snapshot");item.transform.SetParent(skin.transform,false);objects.Add(item);
                item.AddComponent<MeshFilter>().sharedMesh=mesh;
                item.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;
                skin.enabled=false;skins.Add(skin);
            }
        }
        public void Dispose()
        {
            foreach(var skin in skins)if(skin!=null)skin.enabled=true;
            foreach(var item in objects)UnityEngine.Object.DestroyImmediate(item);
            foreach(var mesh in meshes)UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
