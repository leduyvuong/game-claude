using System;
using System.IO;
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
    public static class RiceAssets
    {
        public const string PrefabFolder = "Assets/HarvestIsle/Resources/Crops/Rice";
        private const string GeometryPath = "Assets/HarvestIsle/Art/Crops/RiceModels.asset";
        public static string PrefabPath(int stage) { return PrefabFolder + "/Rice_" + stage + ".prefab"; }

        static RiceAssets() { EditorApplication.delayCall += Ensure; }
        private static void Ensure()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var data = AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            bool missing = data == null || data.version != RiceModelBuilder.Version;
            for (int stage = 1; stage <= 5; stage++) missing |= !File.Exists(PrefabPath(stage));
            if (missing) Rebuild();
        }

        [MenuItem("Harvest Isle/Crops/Rebuild rice stages")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(PrefabFolder);
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
            Mesh[] meshes;
            Material[] materials;
            var stages = RiceModelBuilder.Create(out meshes, out materials);
            try
            {
                foreach (Mesh mesh in meshes) AssetDatabase.AddObjectToAsset(mesh, data);
                foreach (Material material in materials) AssetDatabase.AddObjectToAsset(material, data);
                data.version = RiceModelBuilder.Version;
                data.meshes = meshes; data.materials = materials;
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
                for (int i = 0; i < stages.Length; i++)
                {
                    if (PrefabUtility.SaveAsPrefabAsset(stages[i], PrefabPath(i + 1)) == null)
                        throw new InvalidOperationException("Could not save rice prefab " + (i + 1));
                    Debug.Log("Rice stage " + (i + 1) + " baked: " + meshes[i].triangles.Length / 3 + " triangles. " + PrefabPath(i + 1));
                }
                if (previousMeshes != null) foreach (Mesh mesh in previousMeshes) if (mesh != null) Object.DestroyImmediate(mesh, true);
                if (previousMaterials != null) foreach (Material material in previousMaterials) if (material != null) Object.DestroyImmediate(material, true);
                AssetDatabase.SaveAssets();
            }
            finally { foreach (GameObject stage in stages) Object.DestroyImmediate(stage); }
        }

        [MenuItem("Harvest Isle/Crops/Inspect ripe rice")]
        public static void Inspect()
        {
            Ensure();
            var stage = PrefabStageUtility.OpenPrefab(PrefabPath(5));
            Selection.activeGameObject = stage.prefabContentsRoot;
            EditorApplication.delayCall += () => {
                var view = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView : EditorWindow.GetWindow<SceneView>();
                view.sceneLighting = false;
                view.LookAt(new Vector3(0, 0.4f, 0), Quaternion.LookRotation(new Vector3(-2, -1, -4)), 0.7f, true, true);
            };
        }

        [MenuItem("Harvest Isle/Crops/Render rice stages")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();
            Rebuild();
            Scene preview = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D board = null;
            Texture2D single = null;
            Material soil = null;
            try
            {
                var stages = new GameObject[5];
                for (int i = 0; i < stages.Length; i++)
                {
                    stages[i] = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(i + 1)), preview);
                    stages[i].SetActive(false);
                }
                var sun = new GameObject("Rice sun").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(sun.gameObject, preview);
                sun.type = LightType.Directional; sun.intensity = 1.15f;
                sun.color = new Color(1, 0.96f, 0.87f); sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(48, -30, 0);
                var fill = new GameObject("Rice fill").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(fill.gameObject, preview);
                fill.type = LightType.Directional; fill.intensity = 0.52f;
                fill.color = new Color(0.85f, 0.92f, 1); fill.transform.rotation = Quaternion.Euler(30, 150, 0);
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                SceneManager.MoveGameObjectToScene(tile, preview);
                tile.name = "Rice plot display";
                tile.transform.position = new Vector3(0, -0.040f, 0);
                tile.transform.localScale = new Vector3(0.93f, 0.080f, 0.93f);
                soil = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
                soil.SetColor("_BaseColor", FarmWorld.Hex("99734F"));
                soil.SetColor("_SpecColor", Color.black);
                tile.GetComponent<Renderer>().sharedMaterial = soil;
                var camera = new GameObject("Rice camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, preview);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(preview);
                camera.orthographic = true; camera.orthographicSize = 0.80f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = FarmWorld.Hex("F2EDE0");
                camera.nearClipPlane = 0.1f; camera.farClipPlane = 30;
                camera.allowHDR = false; camera.allowMSAA = false;
                const int width = 600; const int height = 720;
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                target.Create(); camera.aspect = (float)width / height;
                camera.transform.position = new Vector3(2.4f, 1.8f, 3.8f);
                camera.transform.LookAt(new Vector3(0, 0.36f, 0));
                board = new Texture2D(width * 5, height, TextureFormat.RGB24, false);
                single = new Texture2D(width, height, TextureFormat.RGB24, false);
                Directory.CreateDirectory("Art/Previews");
                for (int i = 0; i < stages.Length; i++)
                {
                    stages[i].SetActive(true);
                    RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                    RenderTexture previous = RenderTexture.active;
                    try
                    {
                        RenderTexture.active = target;
                        board.ReadPixels(new Rect(0, 0, width, height), i * width, 0, false);
                        single.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                    }
                    finally { RenderTexture.active = previous; }
                    single.Apply();
                    File.WriteAllBytes("Art/Previews/Rice_" + (i + 1) + ".png", single.EncodeToPNG());
                    stages[i].SetActive(false);
                }
                board.Apply();
                File.WriteAllBytes("Art/Previews/Rice-stages.png", board.EncodeToPNG());
                // Show the same mature prefab repeated at plot spacing, as it would read in-game.
                tile.transform.localScale = new Vector3(3.05f, 0.08f, 3.05f);
                for (int row = 0; row < 3; row++)
                for (int column = 0; column < 3; column++)
                {
                    var crop = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(5)), preview);
                    crop.transform.position = new Vector3((column - 1) * 0.93f, 0, (row - 1) * 0.93f);
                    crop.transform.rotation = Quaternion.Euler(0, (column + row) * 37, 0);
                }
                camera.orthographicSize = 2.08f;
                camera.transform.position = new Vector3(3, 4.4f, 5);
                camera.transform.LookAt(new Vector3(0, 0.27f, 0));
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture oldTarget = RenderTexture.active;
                try { RenderTexture.active = target; single.ReadPixels(new Rect(0, 0, width, height), 0, 0, false); }
                finally { RenderTexture.active = oldTarget; }
                single.Apply(); File.WriteAllBytes("Art/Previews/Rice-field.png", single.EncodeToPNG());
                Debug.Log("Rice previews rendered: " + Path.GetFullPath("Art/Previews/Rice-stages.png"));
            }
            finally
            {
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
                if (board != null) Object.DestroyImmediate(board);
                if (single != null) Object.DestroyImmediate(single);
                if (soil != null) Object.DestroyImmediate(soil);
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }
    }
}
