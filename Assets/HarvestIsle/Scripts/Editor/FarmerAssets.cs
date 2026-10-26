using System;
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
    public static class FarmerAssets
    {
        public const string PrefabPath = "Assets/HarvestIsle/Resources/Characters/Farmer.prefab";
        private const string GeometryPath = "Assets/HarvestIsle/Art/Farmer/FarmerModel.asset";

        static FarmerAssets() { EditorApplication.delayCall += Ensure; }
        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isBatchMode) return;
            var data = AssetDatabase.LoadAssetAtPath<FarmerModelAsset>(GeometryPath);
            if (data == null || data.version != FarmerModelBuilder.Version || !File.Exists(PrefabPath)) Rebuild();
        }

        [MenuItem("Harvest Isle/Characters/Inspect farmer prefab")]
        public static void Inspect()
        {
            Ensure();
            var stage = PrefabStageUtility.OpenPrefab(PrefabPath);
            Selection.activeGameObject = stage.prefabContentsRoot;
            EditorApplication.delayCall += () => {
                SceneView view = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView : EditorWindow.GetWindow<SceneView>();
                view.sceneLighting = false;
                view.LookAt(new Vector3(0, 0.84f, 0), Quaternion.LookRotation(new Vector3(-2, -1.1f, -5)), 1.25f, true, true);
            };
        }

        [MenuItem("Harvest Isle/Characters/Rebuild farmer model")]
        public static void Rebuild()
        {
            Directory.CreateDirectory("Assets/HarvestIsle/Resources/Characters");
            Directory.CreateDirectory("Assets/HarvestIsle/Art/Farmer");
            AssetDatabase.Refresh();
            FarmerModelAsset data = AssetDatabase.LoadAssetAtPath<FarmerModelAsset>(GeometryPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<FarmerModelAsset>();
                AssetDatabase.CreateAsset(data, GeometryPath);
            }
            var avatar = GameHumanFitting.Create(false);
            Mesh[] meshes = avatar.GeneratedMeshes;
            Material[] materials = avatar.GeneratedMaterials;
            Mesh[] previousMeshes = data.meshes;
            Material[] previousMaterials = data.materials;
            foreach (Mesh mesh in meshes) AssetDatabase.AddObjectToAsset(mesh, data);
            foreach (Material material in materials) AssetDatabase.AddObjectToAsset(material, data);
            data.version = FarmerModelBuilder.Version;
            data.meshes = meshes; data.materials = materials;
            avatar.ReleaseResourceOwnership();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            PrefabUtility.SaveAsPrefabAsset(avatar.gameObject, PrefabPath);
            // The generated mesh asset is owned by this baker; retire only its previous sub-assets.
            if (previousMeshes != null) foreach (Mesh mesh in previousMeshes) if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh, true);
            if (previousMaterials != null) foreach (Material material in previousMaterials) if (material != null) UnityEngine.Object.DestroyImmediate(material, true);
            UnityEngine.Object.DestroyImmediate(avatar.gameObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            HumanRefresh.Sync();
            int triangles = 0;
            foreach (Mesh mesh in meshes) triangles += mesh.triangles.Length / 3;
            Debug.Log("Farmer model baked: " + triangles + " triangles, " + materials.Length + " shared materials. " + PrefabPath);
        }

        [MenuItem("Harvest Isle/Characters/Render farmer preview")]
        public static void RebuildAndPreview()
        {
            ProjectSetup.Configure();
            Rebuild();
            Scene preview = EditorSceneManager.NewPreviewScene();
            AmbientMode previousAmbientMode = RenderSettings.ambientMode;
            Color previousAmbientColor = RenderSettings.ambientLight;
            RenderTexture target = null;
            Texture2D image = null;
            try
            {
                var avatar = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), preview) as GameObject;
                avatar.transform.position = Vector3.zero;
                avatar.GetComponent<FarmerAvatar>().SetPose(0, 0, 0, HarvestIsle.Core.FarmTool.Plant);
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.68f, 0.68f, 0.64f);
                var light = new GameObject("Preview sun").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(light.gameObject, preview);
                light.type = LightType.Directional;
                light.intensity = 1.35f;
                light.color = new Color(1, 0.94f, 0.84f);
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(40, -30, 0);
                var fill = new GameObject("Preview fill").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(fill.gameObject, preview);
                fill.type = LightType.Directional;
                fill.intensity = 0.35f;
                fill.color = new Color(0.76f, 0.87f, 1);
                fill.transform.rotation = Quaternion.Euler(30, 150, 0);
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                SceneManager.MoveGameObjectToScene(floor, preview);
                floor.name = "Display plinth";
                floor.transform.position = new Vector3(0, -0.035f, 0);
                floor.transform.localScale = new Vector3(1.4f, 0.033f, 1.4f);
                var floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
                floorMaterial.SetColor("_BaseColor", FarmWorld.Hex("DEDACB"));
                floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
                var camera = new GameObject("Preview camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, preview);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(preview);
                camera.orthographic = true;
                camera.orthographicSize = 1.03f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = FarmWorld.Hex("F4EFE3");
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 30;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                const int width = 720; const int height = 960;
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                target.Create();
                camera.aspect = (float)width / height;
                image = new Texture2D(width * 3, height, TextureFormat.RGB24, false);
                Vector3[] views = { new Vector3(2, 1.9f, 5), new Vector3(-5, 1.7f, 1.3f), new Vector3(-2.8f, 1.9f, -5) };
                for (int i = 0; i < views.Length; i++)
                {
                    camera.transform.position = views[i];
                    camera.transform.LookAt(new Vector3(0, 0.84f, 0));
                    var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                    using (var snapshot = new HumanPoseSnapshot(avatar))
                        for (int warmup = 0; warmup < 3; warmup++) RenderPipeline.SubmitRenderRequest(camera, request);
                    RenderTexture previousTarget = RenderTexture.active;
                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, width, height), i * width, 0, false);
                    RenderTexture.active = previousTarget;
                }
                image.Apply();
                Directory.CreateDirectory("Art/Previews");
                File.WriteAllBytes("Art/Previews/Farmer.png", image.EncodeToPNG());
                var model = avatar.GetComponent<FarmerAvatar>();
                for (int i = 0; i < 3; i++)
                {
                    model.SetPose(i == 0 ? 0 : Mathf.PI / 18, i == 1 ? 1 : 0, i == 2 ? 1 : 0, HarvestIsle.Core.FarmTool.Water);
                    camera.transform.position = views[0];
                    camera.transform.LookAt(new Vector3(0, 0.84f, 0));
                    var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                    using (var snapshot = new HumanPoseSnapshot(avatar))
                        for (int warmup = 0; warmup < 3; warmup++) RenderPipeline.SubmitRenderRequest(camera, request);
                    RenderTexture previousTarget = RenderTexture.active;
                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, width, height), i * width, 0, false);
                    RenderTexture.active = previousTarget;
                }
                image.Apply();
                File.WriteAllBytes("Art/Previews/Farmer-actions.png", image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(floorMaterial);
                Debug.Log("Farmer preview: " + Path.GetFullPath("Art/Previews/Farmer.png"));
            }
            finally
            {
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                RenderSettings.ambientMode = previousAmbientMode;
                RenderSettings.ambientLight = previousAmbientColor;
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }
    }
}
