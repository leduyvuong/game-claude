using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    public static class ReferenceMaleRigging
    {
        public static string Folder => ReferenceHumanProfile.Folder;
        public static ReferenceMaleMotion Attach(GameObject root)
        {
            ClothingMaterial("Shirt", ReferenceHumanProfile.Female ? "B67E73" : "5E8275");
            ClothingMaterial("ShirtTrim", ReferenceHumanProfile.Female ? "8D5B52" : "405F54");
            ClothingMaterial("Trousers", "42566D"); ClothingMaterial("TrouserTrim", "35465A");
            string path = Folder + "/" + ReferenceHumanProfile.Asset + "Skin.asset";
            var data = AssetDatabase.LoadAssetAtPath<FarmerModelAsset>(path);
            if (data == null) { data = ScriptableObject.CreateInstance<FarmerModelAsset>(); AssetDatabase.CreateAsset(data, path); }
            var previousMeshes = data.meshes;
            foreach (Transform child in new List<Transform>(Children(root.transform))) UnityEngine.Object.DestroyImmediate(child.gameObject);
            var sourceRenderer = root.GetComponent<MeshRenderer>(); if (sourceRenderer != null) UnityEngine.Object.DestroyImmediate(sourceRenderer);
            var sourceFilter = root.GetComponent<MeshFilter>(); if (sourceFilter != null) UnityEngine.Object.DestroyImmediate(sourceFilter);
            var actor = root.GetComponent<ReferenceMaleMotion>(); if (actor == null) actor = root.AddComponent<ReferenceMaleMotion>();
            actor.femaleProfile = ReferenceHumanProfile.Female;
            var meshes = new List<Mesh>();
            using (var stream = new BinaryReader(File.OpenRead(Folder + "/" + ReferenceHumanProfile.Asset + "Skin.bytes")))
            {
                if (Encoding.ASCII.GetString(stream.ReadBytes(4)) != "HM03") throw new InvalidDataException("Invalid human skin export");
                int count = stream.ReadInt32();
                actor.bones = new Transform[count]; actor.bindPositions = new Vector3[count]; actor.bindTails = new Vector3[count];
                var bindposes = new Matrix4x4[count];
                for (int i = 0; i < count; i++)
                {
                    string name = ReadString(stream); int parent = stream.ReadInt32();
                    Vector3 head = ReadVector(stream), tail = ReadVector(stream);
                    Transform bone = new GameObject(name).transform;
                    bone.SetParent(parent < 0 ? root.transform : actor.bones[parent], false);
                    bone.localPosition = head - (parent < 0 ? Vector3.zero : actor.bindPositions[parent]);
                    actor.bones[i] = bone; actor.bindPositions[i] = head; actor.bindTails[i] = tail;
                    bindposes[i] = bone.worldToLocalMatrix * root.transform.localToWorldMatrix;
                }
                int parts = stream.ReadInt32();
                for (int part = 0; part < parts; part++)
                {
                    string materialName = ReadString(stream); int vertexCount = stream.ReadInt32();
                    var vertices = new Vector3[vertexCount]; var normals = new Vector3[vertexCount]; var uv = new Vector2[vertexCount]; var weights = new BoneWeight[vertexCount];
                    for (int i = 0; i < vertexCount; i++)
                    {
                        vertices[i] = ReadVector(stream); normals[i] = ReadVector(stream); uv[i] = new Vector2(stream.ReadSingle(), stream.ReadSingle());
                        var w = new BoneWeight { boneIndex0 = stream.ReadInt32(), boneIndex1 = stream.ReadInt32(), boneIndex2 = stream.ReadInt32(), boneIndex3 = stream.ReadInt32(),
                            weight0 = stream.ReadSingle(), weight1 = stream.ReadSingle(), weight2 = stream.ReadSingle(), weight3 = stream.ReadSingle() };
                        if (Mathf.Abs(w.weight0 + w.weight1 + w.weight2 + w.weight3 - 1) > .0001f) throw new InvalidDataException("Unnormalized skin weight");
                        weights[i] = w;
                    }
                    int indexCount = stream.ReadInt32(); var triangles = new int[indexCount]; for (int i = 0; i < indexCount; i++) triangles[i] = stream.ReadInt32();
                    var mesh = new Mesh { name = "Reference male " + materialName + " " + part, indexFormat = IndexFormat.UInt32 };
                    mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.triangles = triangles; mesh.boneWeights = weights; mesh.bindposes = bindposes;
                    mesh.RecalculateBounds(); AssetDatabase.AddObjectToAsset(mesh, data); meshes.Add(mesh);
                    var item = new GameObject(mesh.name); item.transform.SetParent(root.transform, false);
                    var skin = item.AddComponent<SkinnedMeshRenderer>(); skin.sharedMesh = mesh; skin.bones = actor.bones; skin.rootBone = actor.bones[0];
                    skin.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/" + materialName + ".mat");
                    if (skin.sharedMaterial == null) throw new InvalidDataException("Missing studio material " + materialName);
                    skin.quality = SkinQuality.Bone4; skin.updateWhenOffscreen = true; skin.receiveShadows = false;
                    skin.localBounds = new Bounds(new Vector3(0, 1, .2f), new Vector3(3, 3, 3));
                }
                if (stream.BaseStream.Position != stream.BaseStream.Length) throw new InvalidDataException("Trailing skin data");
            }
            actor.Initialize(); AddProps(actor);
            data.version = 3; data.meshes = meshes.ToArray(); data.materials = Array.Empty<Material>(); EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets(); PrefabUtility.SaveAsPrefabAsset(root, Folder + "/" + ReferenceHumanProfile.Asset + ".prefab");
            if (previousMeshes != null) foreach (var mesh in previousMeshes) if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh, true);
            AssetDatabase.SaveAssets(); return actor;
        }
        private static IEnumerable<Transform> Children(Transform root) { for (int i = 0; i < root.childCount; i++) yield return root.GetChild(i); }
        private static void ClothingMaterial(string name, string hex)
        {
            string path = Folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", .06f);
            material.SetFloat("_Cull", 0); material.SetFloat("_ReceiveShadows", 0);
            material.EnableKeyword("_RECEIVE_SHADOWS_OFF"); EditorUtility.SetDirty(material);
        }
        private static string ReadString(BinaryReader reader) { return Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32())); }
        private static Vector3 ReadVector(BinaryReader reader) { return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()); }
        private static Material Material(string name, string hex)
        {
            string path = Folder + "/Motion-" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            ColorUtility.TryParseHtmlString("#" + hex, out var color); material.color = color;
            material.SetFloat("_Smoothness", .15f); return material;
        }
        private static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 at, Vector3 scale, Material material)
        {
            var item = GameObject.CreatePrimitive(type); item.name = name; item.transform.SetParent(parent, false);
            item.transform.localPosition = at; item.transform.localScale = scale; item.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>()); return item;
        }
        private static void Bar(Transform parent, string name, Vector3 a, Vector3 b, float radius, Material material)
        {
            var bar = Part(parent, PrimitiveType.Cylinder, name, (a + b) * .5f, new Vector3(radius * 2, Vector3.Distance(a, b) * .5f, radius * 2), material);
            bar.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }
        private static void AddProps(ReferenceMaleMotion actor)
        {
            Material metal = Material("Metal", "6E9B91"), wood = Material("Wood", "8D6543"), steel = Material("Steel", "717C82"), green = Material("Leaf", "628E49");
            actor.wateringCan = new GameObject("Binh tuoi"); actor.wateringCan.transform.SetParent(actor.transform, false); var can = actor.wateringCan.transform;
            Part(can, PrimitiveType.Cylinder, "Than binh", Vector3.zero, new Vector3(.19f, .085f, .17f), metal);
            Bar(can, "Voi", new Vector3(0, -.035f, .06f), new Vector3(0, .04f, .26f), .022f, metal);
            Part(can, PrimitiveType.Sphere, "Dau tuoi", new Vector3(0, .04f, .26f), new Vector3(.065f, .035f, .055f), steel);
            Bar(can, "Quai sau", new Vector3(0, .06f, -.06f), new Vector3(0, .14f, -.06f), .011f, metal);
            Bar(can, "Quai cam", new Vector3(0, .14f, -.06f), new Vector3(0, .14f, .045f), .011f, metal);
            Bar(can, "Quai truoc", new Vector3(0, .14f, .045f), new Vector3(0, .06f, .045f), .011f, metal);
            actor.hoe = new GameObject("Cuoc"); actor.hoe.transform.SetParent(actor.transform, false);
            Bar(actor.hoe.transform, "Can cuoc", Vector3.zero, Vector3.up * 1.30f, .013f, wood);
            Part(actor.hoe.transform, PrimitiveType.Cube, "Luoi cuoc", new Vector3(0, .025f, .055f), new Vector3(.15f, .025f, .15f), steel);
            actor.crate = new GameObject("Gio nong san"); actor.crate.transform.SetParent(actor.transform, false); var crate = actor.crate.transform;
            Part(crate, PrimitiveType.Cube, "Day", new Vector3(0, -.095f, 0), new Vector3(.49f, .025f, .31f), wood);
            for (int i = 0; i < 3; i++)
            {
                float y = -.065f + i * .065f;
                foreach (int side in new[] { -1, 1 })
                {
                    Part(crate, PrimitiveType.Cube, "Van ngang", new Vector3(0, y, side * .15f), new Vector3(.49f, .048f, .018f), wood);
                    Part(crate, PrimitiveType.Cube, "Van ben", new Vector3(side * .24f, y, 0), new Vector3(.018f, .048f, .30f), wood);
                }
            }
            for (int i = 0; i < 5; i++) Part(crate, PrimitiveType.Sphere, "Rau", new Vector3(-.16f + i * .08f, .065f, (i % 2) * .06f - .03f), new Vector3(.11f, .13f, .12f), green);
            Material soil = Material("Soil", "69533C"), water = Material("Water", "A0D7DF");
            actor.soilPatch = Part(actor.transform, PrimitiveType.Sphere, "Soil for motion review", Vector3.zero, new Vector3(.40f, .045f, .28f), soil);
            actor.workPlant = new GameObject("Seedling for motion review"); actor.workPlant.transform.SetParent(actor.transform, false);
            Part(actor.workPlant.transform, PrimitiveType.Sphere, "Root ball", new Vector3(0, .02f, 0), new Vector3(.085f, .07f, .08f), soil);
            Bar(actor.workPlant.transform, "Stem", Vector3.zero, Vector3.up * .24f, .006f, green);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 137 * Mathf.Deg2Rad; Vector3 tip = new Vector3(Mathf.Cos(a) * .09f, .14f + i * .016f, Mathf.Sin(a) * .07f);
                var leaf = Part(actor.workPlant.transform, PrimitiveType.Sphere, "Leaf", tip, new Vector3(.045f, .105f, .012f), green);
                leaf.transform.localRotation = Quaternion.FromToRotation(Vector3.up, tip - new Vector3(0, .09f, 0));
            }
            actor.waterDrops = new Transform[16];
            for (int i = 0; i < actor.waterDrops.Length; i++)
            {
                actor.waterDrops[i] = Part(actor.transform, PrimitiveType.Sphere, "Water drop", Vector3.zero, new Vector3(.009f, .018f, .009f), water).transform;
                actor.waterDrops[i].gameObject.SetActive(false);
            }
            actor.workPlant.SetActive(false); actor.soilPatch.SetActive(false);
            actor.wateringCan.SetActive(false); actor.hoe.SetActive(false); actor.crate.SetActive(false);
        }
    }
}
