using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    [InitializeOnLoad]
    public static class PalmAssets
    {
        public const string PrefabPath = "Assets/HarvestIsle/Resources/Nature/CoconutPalm.prefab";
        private const string GeometryPath = "Assets/HarvestIsle/Art/Nature/CoconutPalm.asset";

        static PalmAssets() { EditorApplication.delayCall += Ensure; }
        private static void Ensure()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var data = AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if (data == null || data.version != PalmModelBuilder.Version || !File.Exists(PrefabPath)) Rebuild();
        }

        [MenuItem("Harvest Isle/Nature/Rebuild coconut palm")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            Directory.CreateDirectory(Path.GetDirectoryName(GeometryPath));
            AssetDatabase.Refresh();
            var data = AssetDatabase.LoadAssetAtPath<NatureModelAsset>(GeometryPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<NatureModelAsset>();
                AssetDatabase.CreateAsset(data, GeometryPath);
            }
            Mesh[] oldMeshes = data.meshes;
            Material[] oldMaterials = data.materials;
            Mesh[] meshes;
            Material[] materials;
            GameObject model = PalmModelBuilder.Create(out meshes, out materials);
            try
            {
                foreach (Mesh mesh in meshes) AssetDatabase.AddObjectToAsset(mesh, data);
                foreach (Material material in materials) AssetDatabase.AddObjectToAsset(material, data);
                data.version = PalmModelBuilder.Version;
                data.meshes = meshes;
                data.materials = materials;
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
                PrefabUtility.SaveAsPrefabAsset(model, PrefabPath);
                if (oldMeshes != null) foreach (Mesh mesh in oldMeshes) if (mesh != null) Object.DestroyImmediate(mesh, true);
                if (oldMaterials != null) foreach (Material material in oldMaterials) if (material != null) Object.DestroyImmediate(material, true);
                AssetDatabase.SaveAssets();
                int count = 0;
                foreach (Mesh mesh in meshes) count += mesh.triangles.Length / 3;
                Debug.Log("Coconut palm baked: " + count + " triangles, " + materials.Length + " shared materials. " + PrefabPath);
            }
            finally { Object.DestroyImmediate(model); }
        }

        [MenuItem("Harvest Isle/Nature/Inspect coconut palm")]
        public static void Inspect()
        {
            Ensure();
            var stage = PrefabStageUtility.OpenPrefab(PrefabPath);
            Selection.activeGameObject = stage.prefabContentsRoot;
            EditorApplication.delayCall += () => {
                var view = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView : EditorWindow.GetWindow<SceneView>();
                view.sceneLighting = false;
                view.LookAt(new Vector3(0.15f, 1.5f, 0), Quaternion.LookRotation(new Vector3(-4, -2, -6)), 2.3f, true, true);
            };
        }

        [MenuItem("Harvest Isle/Nature/Render coconut palm preview")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();
            Rebuild();
            Scene preview = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D image = null;
            Texture2D hero = null;
            Material sand = null;
            try
            {
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), preview);
                var sun = new GameObject("Warm sun").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(sun.gameObject, preview);
                sun.type = LightType.Directional;
                sun.intensity = 1.25f;
                sun.color = new Color(1, 0.96f, 0.86f);
                sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(45, -35, 0);
                var fill = new GameObject("Soft sky fill").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(fill.gameObject, preview);
                fill.type = LightType.Directional;
                fill.intensity = 0.52f;
                fill.color = new Color(0.83f, 0.92f, 1);
                fill.transform.rotation = Quaternion.Euler(32, 140, 0);
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                SceneManager.MoveGameObjectToScene(floor, preview);
                floor.name = "Sandy display base";
                floor.transform.position = new Vector3(0, -0.055f, 0);
                floor.transform.localScale = new Vector3(1.5f, 0.055f, 1.5f);
                sand = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
                sand.SetColor("_BaseColor", FarmWorld.Hex("D7C69E"));
                floor.GetComponent<Renderer>().sharedMaterial = sand;
                var camera = new GameObject("Palm preview camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, preview);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(preview);
                camera.orthographic = true;
                camera.orthographicSize = 2.15f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = FarmWorld.Hex("F1EEE3");
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 30;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                const int width = 800;
                const int height = 900;
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                target.Create();
                camera.aspect = (float)width / height;
                image = new Texture2D(width * 3, height, TextureFormat.RGB24, false);
                hero = new Texture2D(width, height, TextureFormat.RGB24, false);
                Vector3[] views = { new Vector3(4, 3.7f, 6), new Vector3(-6, 3.6f, 2), new Vector3(4, 7, 5) };
                Directory.CreateDirectory("Art/Previews");
                for (int i = 0; i < views.Length; i++)
                {
                    camera.transform.position = views[i];
                    camera.transform.LookAt(new Vector3(0.14f, 1.45f, 0));
                    RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                    RenderTexture previous = RenderTexture.active;
                    try
                    {
                        RenderTexture.active = target;
                        image.ReadPixels(new Rect(0, 0, width, height), width * i, 0, false);
                        if (i == 0) hero.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                    }
                    finally { RenderTexture.active = previous; }
                }
                image.Apply(); hero.Apply();
                File.WriteAllBytes("Art/Previews/CoconutPalm.png", hero.EncodeToPNG());
                File.WriteAllBytes("Art/Previews/CoconutPalm-views.png", image.EncodeToPNG());
                Debug.Log("Coconut palm previews rendered: " + Path.GetFullPath("Art/Previews/CoconutPalm.png"));
            }
            finally
            {
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
                if (image != null) Object.DestroyImmediate(image);
                if (hero != null) Object.DestroyImmediate(hero);
                if (sand != null) Object.DestroyImmediate(sand);
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }
    }
}
