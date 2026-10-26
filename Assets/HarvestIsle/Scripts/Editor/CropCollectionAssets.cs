using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HarvestIsle.Editor
{
    [InitializeOnLoad]
    public static class CropCollectionAssets
    {
        private const string GeometryPath = "Assets/HarvestIsle/Art/Crops/CropCollection.asset";
        private const string ReviewFolder = "Art/Previews/Crops";
        public static string PrefabPath(string name, int stage)
        {
            return "Assets/HarvestIsle/Resources/Crops/" + name + "/" + name + "_" + stage + ".prefab";
        }

        static CropCollectionAssets() { EditorApplication.delayCall += Ensure; }
        private static void Ensure()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var data = AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            bool missing = data == null || data.version != CropCollectionBuilder.Version;
            foreach (string name in CropCollectionBuilder.Names)
                for (int stage = 1; stage <= 5; stage++) missing |= !File.Exists(PrefabPath(name, stage));
            if (missing) Rebuild();
        }

        [MenuItem("Harvest Isle/Crops/Rebuild full crop collection")]
        public static void Rebuild()
        {
            foreach (string name in CropCollectionBuilder.Names)
                Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath(name, 1)));
            Directory.CreateDirectory(Path.GetDirectoryName(GeometryPath));
            AssetDatabase.Refresh();
            var data = AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<NatureModelAsset>();
                AssetDatabase.CreateAsset(data, GeometryPath);
            }
            Mesh[] previousMeshes = data.meshes;
            Material[] previousMaterials = data.materials;
            Material[] materials = CropCollectionBuilder.Materials();
            var meshes = new List<Mesh>();
            var objects = new List<GameObject>();
            try
            {
                foreach (Material material in materials) AssetDatabase.AddObjectToAsset(material, data);
                for (int crop = 0; crop < CropCollectionBuilder.Names.Length; crop++)
                for (int stage = 1; stage <= 5; stage++)
                {
                    Mesh mesh;
                    GameObject model = CropCollectionBuilder.Create(crop, stage, materials, out mesh);
                    objects.Add(model);
                    ValidateMesh(mesh);
                    meshes.Add(mesh);
                    AssetDatabase.AddObjectToAsset(mesh, data);
                }
                data.version = CropCollectionBuilder.Version;
                data.meshes = meshes.ToArray(); data.materials = materials;
                EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
                for (int i = 0; i < objects.Count; i++)
                {
                    string path = PrefabPath(CropCollectionBuilder.Names[i / 5], i % 5 + 1);
                    if (PrefabUtility.SaveAsPrefabAsset(objects[i], path) == null)
                        throw new InvalidOperationException("Could not save " + path);
                    Debug.Log("Crop baked: " + objects[i].name + ", " + meshes[i].triangles.Length / 3 + " triangles.");
                }
                if (previousMeshes != null) foreach (Mesh mesh in previousMeshes) if (mesh != null) Object.DestroyImmediate(mesh, true);
                if (previousMaterials != null) foreach (Material material in previousMaterials) if (material != null) Object.DestroyImmediate(material, true);
                AssetDatabase.SaveAssets();
                Debug.Log("Crop collection saved: 45 prefabs, 17 shared materials.");
            }
            finally { foreach (GameObject model in objects) Object.DestroyImmediate(model); }
        }

        private static void ValidateMesh(Mesh mesh)
        {
            if (mesh.vertexCount == 0 || mesh.triangles.Length == 0) throw new InvalidOperationException("Empty crop mesh: " + mesh.name);
            foreach (Vector3 vertex in mesh.vertices)
                if (float.IsNaN(vertex.x) || float.IsInfinity(vertex.x) || float.IsNaN(vertex.y) || float.IsInfinity(vertex.y)
                    || float.IsNaN(vertex.z) || float.IsInfinity(vertex.z)) throw new InvalidOperationException("Invalid vertex: " + mesh.name);
            if (mesh.bounds.size.x > 1.25f || mesh.bounds.size.z > 1.25f || mesh.bounds.max.y > 1.6f)
                throw new InvalidOperationException("Crop does not fit farm plots: " + mesh.name + " " + mesh.bounds);
        }

        [MenuItem("Harvest Isle/Crops/Render full crop collection")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();
            RiceAssets.Rebuild();
            Rebuild();
            var entries = Entries();
            Directory.CreateDirectory(ReviewFolder);
            Scene preview = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null, overviewTarget = null;
            Texture2D board = null, single = null, overview = null;
            Material soil = null;
            try
            {
                var sun = new GameObject("Collection sun").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(sun.gameObject, preview);
                sun.type = LightType.Directional; sun.intensity = 1.15f;
                sun.color = new Color(1, 0.96f, 0.87f); sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(48, -30, 0);
                var fill = new GameObject("Collection fill").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(fill.gameObject, preview);
                fill.type = LightType.Directional; fill.intensity = 0.55f;
                fill.color = new Color(0.85f, 0.92f, 1); fill.transform.rotation = Quaternion.Euler(30, 150, 0);
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                SceneManager.MoveGameObjectToScene(tile, preview);
                tile.name = "Crop display plot";
                tile.transform.position = new Vector3(0, -0.04f, 0);
                tile.transform.localScale = new Vector3(0.93f, 0.08f, 0.93f);
                soil = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
                soil.SetColor("_BaseColor", FarmWorld.Hex("99734F")); soil.SetColor("_SpecColor", Color.black);
                tile.GetComponent<Renderer>().sharedMaterial = soil;
                var camera = new GameObject("Collection preview camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, preview);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(preview);
                camera.orthographic = true; camera.orthographicSize = 0.92f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = FarmWorld.Hex("F2EDE0");
                camera.nearClipPlane = 0.1f; camera.farClipPlane = 30;
                camera.allowHDR = false; camera.allowMSAA = false;
                camera.transform.position = new Vector3(2.4f, 1.8f, 3.8f);
                camera.transform.LookAt(new Vector3(0, 0.45f, 0));
                const int width = 600, height = 720;
                const int overviewWidth = 400, overviewHeight = 480;
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                target.Create(); camera.aspect = (float)width / height;
                overviewTarget = new RenderTexture(overviewWidth, overviewHeight, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                overviewTarget.Create();
                board = new Texture2D(width * 5, height, TextureFormat.RGB24, false);
                single = new Texture2D(width, height, TextureFormat.RGB24, false);
                overview = new Texture2D(overviewWidth * 5, overviewHeight * 2, TextureFormat.RGB24, false);
                for (int crop = 0; crop < entries.Length; crop++)
                {
                    ReviewEntry entry = entries[crop];
                    for (int stage = 1; stage <= 5; stage++)
                    {
                        string path = PrefabPath(entry.name, stage);
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab == null) throw new InvalidOperationException("Missing prefab: " + path);
                        var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, preview);
                        try
                        {
                            Mesh mesh = model.GetComponent<MeshFilter>().sharedMesh;
                            ValidateMesh(mesh);
                            entry.triangles[stage - 1] = mesh.triangles.Length / 3;
                            entry.prefabs[stage - 1] = path;
                            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                            Read(target, board, (stage - 1) * width, 0);
                            Read(target, single, 0, 0); single.Apply();
                            File.WriteAllBytes(ReviewFolder + "/" + entry.name + "_" + stage + ".png", single.EncodeToPNG());
                            if (stage == 5)
                            {
                                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = overviewTarget });
                                Read(overviewTarget, overview, crop % 5 * overviewWidth, (1 - crop / 5) * overviewHeight);
                            }
                        }
                        finally { Object.DestroyImmediate(model); }
                    }
                    board.Apply();
                    File.WriteAllBytes(ReviewFolder + "/" + entry.name + "-stages.png", board.EncodeToPNG());
                    Debug.Log("Crop previews rendered: " + entry.name);
                }
                overview.Apply(); File.WriteAllBytes(ReviewFolder + "/Overview.png", overview.EncodeToPNG());
                string json = JsonUtility.ToJson(new ReviewManifest { crops = entries }, true);
                File.WriteAllText(ReviewFolder + "/models.json", json, new UTF8Encoding(false));
                string html = File.ReadAllText("Tools/Templates/crop-gallery.html", Encoding.UTF8).Replace("__CROP_DATA__", json);
                File.WriteAllText(ReviewFolder + "/index.html", html, new UTF8Encoding(false));
                Debug.Log("Crop collection verified: 10 species, 50 stage prefabs. Review: " + Path.GetFullPath(ReviewFolder + "/index.html"));
            }
            finally
            {
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
                if (overviewTarget != null) { overviewTarget.Release(); Object.DestroyImmediate(overviewTarget); }
                if (board != null) Object.DestroyImmediate(board);
                if (single != null) Object.DestroyImmediate(single);
                if (overview != null) Object.DestroyImmediate(overview);
                if (soil != null) Object.DestroyImmediate(soil);
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void Read(RenderTexture source, Texture2D destination, int x, int y)
        {
            RenderTexture previous = RenderTexture.active;
            try { RenderTexture.active = source; destination.ReadPixels(new Rect(0, 0, source.width, source.height), x, y, false); }
            finally { RenderTexture.active = previous; }
        }

        [MenuItem("Harvest Isle/Crops/Open crop gallery")]
        public static void OpenGallery()
        {
            string path = Path.GetFullPath(ReviewFolder + "/index.html");
            if (!File.Exists(path)) RebuildAndPreview();
            Application.OpenURL(new Uri(path).AbsoluteUri);
        }

        private static ReviewEntry[] Entries()
        {
            var entries = new ReviewEntry[10];
            string[] stageLabels = { "Mầm non", "Cây non", "Phát triển thân lá", "Hoa / quả non / gần trưởng thành", "Sẵn sàng thu hoạch" };
            entries[0] = new ReviewEntry { name = "Rice", label = "Lúa", stages = stageLabels };
            for (int i = 0; i < CropCollectionBuilder.Names.Length; i++)
                entries[i + 1] = new ReviewEntry { name = CropCollectionBuilder.Names[i], label = CropCollectionBuilder.Labels[i], stages = stageLabels };
            return entries;
        }

        [Serializable] private sealed class ReviewManifest { public ReviewEntry[] crops; }
        [Serializable] private sealed class ReviewEntry
        {
            public string name;
            public string label;
            public string[] stages;
            public string[] prefabs = new string[5];
            public int[] triangles = new int[5];
        }
    }
}
