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
    /// <summary>Isolated reference sculpt; never rebuilds the game's character catalog.</summary>
    public static class ReferenceMaleStudy
    {
        private static string Folder => ReferenceHumanProfile.Folder;
        private static string Output => ReferenceHumanProfile.Output;

        [MenuItem("Harvest Isle/Characters/Render reference male study")]
        public static void Build()
        {
            Directory.CreateDirectory(Output);
            AssetDatabase.Refresh();
            var importer = AssetImporter.GetAtPath(Folder + "/" + ReferenceHumanProfile.Asset + ".obj") as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Generate the reference mesh with Tools/build_reference_male.py first.");
            importer.globalScale = 1;
            importer.isReadable = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.normalSmoothingAngle = 90;
            importer.SaveAndReimport();
            Scene scene = EditorSceneManager.NewPreviewScene();
            var oldMode = RenderSettings.ambientMode;
            var oldAmbient = RenderSettings.ambientLight;
            RenderTexture target = null;
            Texture2D sheet = null;
            Material floorMaterial = null;
            bool previousAsyncCompilation = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try
            {
                var model = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/" + ReferenceHumanProfile.Asset + ".obj"));
                SceneManager.MoveGameObjectToScene(model, scene);
                model.name = ReferenceHumanProfile.DisplayName;
                foreach (var renderer in model.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.receiveShadows = false;
                    var slots = renderer.sharedMaterials;
                    for (int i = 0; i < slots.Length; i++)
                    {
                        string key = slots[i] != null ? slots[i].name : "Clay";
                        string path = Folder + "/" + key + ".mat";
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                        if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
                        Color color = key.Contains("Pupil") ? Hex("272724") : key.Contains("Iris") ? Hex("777A70") :
                            key.Contains("Eye") ? Hex("EAE8E2") : key.Contains("Brow") ? Hex("554B40") :
                            key.Contains("Crease") ? Hex("7C7067") : Hex("BFB6A6");
                        mat.SetColor("_BaseColor", color);
                        mat.SetFloat("_ReceiveShadows", 0);
                        mat.EnableKeyword("_RECEIVE_SHADOWS_OFF");
                        mat.SetTexture("_BaseMap", key.Contains("Eye") ? AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/Eyes.png") : null);
                        bool eye = key.Contains("Eye");
                        mat.SetFloat("_AlphaClip", eye ? 1 : 0);
                        mat.SetFloat("_Cutoff", .1f);
                        if (eye) mat.EnableKeyword("_ALPHATEST_ON"); else mat.DisableKeyword("_ALPHATEST_ON");
                        mat.SetFloat("_Smoothness", key.Contains("Eye") || key.Contains("Iris") ? .35f : .18f);
                        EditorUtility.SetDirty(mat);
                        slots[i] = mat;
                    }
                    renderer.sharedMaterials = slots;
                }
                AssetDatabase.SaveAssets();
                if (File.Exists(Folder + "/" + ReferenceHumanProfile.Asset + "Skin.bytes")) ReferenceMaleRigging.Attach(model);
                PrefabUtility.SaveAsPrefabAsset(model, Folder + "/" + ReferenceHumanProfile.Asset + ".prefab");
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(.43f, .46f, .5f);
                Light(scene, "Key", new Vector3(35, -140, 0), new Color(1, .95f, .88f), 1.15f);
                Light(scene, "Fill", new Vector3(15, 120, 0), new Color(.83f, .89f, 1), .4f);
                Light(scene, "Rim", new Vector3(12, 0, 0), Color.white, .7f);
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                SceneManager.MoveGameObjectToScene(floor, scene);
                floor.name = "Be trung bay";
                floor.transform.position = new Vector3(0, -.018f, .015f);
                floor.transform.localScale = new Vector3(1.3f, .025f, .8f);
                floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                floorMaterial.SetColor("_BaseColor", Hex("858D8A"));
                floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
                var camera = new GameObject("Reference study camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
                camera.orthographic = true;
                camera.orthographicSize = 1.055f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Hex("42494B");
                camera.nearClipPlane = .01f; camera.farClipPlane = 30;
                camera.allowHDR = false; camera.allowMSAA = false;
                const int width = 850, height = 1200;
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                target.Create(); camera.aspect = (float)width / height;
                sheet = new Texture2D(width * 4, height, TextureFormat.RGB24, false);
                Vector3 center = new Vector3(0, .935f, .015f);
                Vector3[] views = {new Vector3(0, .97f, 6), new Vector3(3.6f, 1.15f, 5),
                    new Vector3(6, .97f, 0), new Vector3(0, .97f, -6)};
                string[] names = {"Front", "Three-quarter", "Side", "Back"};
                camera.transform.position = views[0]; camera.transform.LookAt(center);
                Render(camera, target, sheet, 0, width, height);
                for (int i = 0; i < views.Length; i++)
                {
                    camera.transform.position = views[i]; camera.transform.LookAt(center);
                    Render(camera, target, sheet, i * width, width, height);
                    var single = new Texture2D(width, height, TextureFormat.RGB24, false);
                    single.SetPixels(sheet.GetPixels(i * width, 0, width, height)); single.Apply();
                    File.WriteAllBytes(Output + "/" + names[i] + ".png", single.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(single);
                }
                sheet.Apply(); File.WriteAllBytes(Output + "/Turnaround.png", sheet.EncodeToPNG());
                camera.orthographicSize = .225f;
                camera.transform.position = new Vector3(.65f, 1.74f, 3);
                camera.transform.LookAt(new Vector3(0, 1.681f, 0));
                var close = new Texture2D(width, height, TextureFormat.RGB24, false);
                Render(camera, target, close, 0, width, height); close.Apply();
                File.WriteAllBytes(Output + "/Face.png", close.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(close);
                // Save an inspectable scene containing only the experimental model and studio.
                camera.cameraType = CameraType.Game; camera.overrideSceneCullingMask = 0;
                camera.orthographicSize = 1.055f;
                camera.transform.position = views[1]; camera.transform.LookAt(center);
                string floorPath = Folder + "/Plinth.mat";
                var savedFloor = AssetDatabase.LoadAssetAtPath<Material>(floorPath);
                if (savedFloor == null) { AssetDatabase.CreateAsset(floorMaterial, floorPath); savedFloor = floorMaterial; floorMaterial = null; }
                floor.GetComponent<Renderer>().sharedMaterial = savedFloor;
                var previousScene = SceneManager.GetActiveScene();
                var mode = Application.isBatchMode && string.IsNullOrEmpty(previousScene.path)
                    ? NewSceneMode.Single : NewSceneMode.Additive;
                var savedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, mode);
                try
                {
                    foreach (var item in scene.GetRootGameObjects())
                    {
                        var copy = UnityEngine.Object.Instantiate(item);
                        copy.name = item.name;
                        SceneManager.MoveGameObjectToScene(copy, savedScene);
                    }
                    SceneManager.SetActiveScene(savedScene);
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    RenderSettings.ambientLight = new Color(.43f, .46f, .5f);
                    EditorSceneManager.SaveScene(savedScene, Folder + "/" + ReferenceHumanProfile.Asset + "Study.unity");
                }
                finally
                {
                    if (previousScene.IsValid() && previousScene.isLoaded)
                    {
                        SceneManager.SetActiveScene(previousScene);
                        EditorSceneManager.CloseScene(savedScene, true);
                    }
                }
                int triangles = 0;
                foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null) throw new InvalidOperationException("Missing mesh");
                    triangles += filter.sharedMesh.triangles.Length / 3;
                }
                foreach (var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>()) triangles += skin.sharedMesh.triangles.Length / 3;
                File.WriteAllText(".tools/model-work/" + ReferenceHumanProfile.Slug + "-result.json", "{\"status\":\"complete\",\"triangles\":" + triangles + ",\"views\":5,\"experimental\":true}");
                Debug.Log("REFERENCE_MALE_STUDY_COMPLETE: " + triangles + " triangles; " + Output);
            }
            finally
            {
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (sheet != null) UnityEngine.Object.DestroyImmediate(sheet);
                if (floorMaterial != null) UnityEngine.Object.DestroyImmediate(floorMaterial);
                RenderSettings.ambientMode = oldMode; RenderSettings.ambientLight = oldAmbient;
                ShaderUtil.allowAsyncCompilation = previousAsyncCompilation;
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
        private static void Light(Scene scene, string name, Vector3 rotation, Color color, float intensity)
        {
            var light = new GameObject(name).AddComponent<Light>();
            SceneManager.MoveGameObjectToScene(light.gameObject, scene);
            light.type = LightType.Directional; light.color = color; light.intensity = intensity;
            light.shadows = LightShadows.Soft; light.transform.rotation = Quaternion.Euler(rotation);
        }
        private static void Render(Camera camera, RenderTexture target, Texture2D image, int x, int width, int height)
        {
            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest {destination = target});
            var previous = RenderTexture.active;
            try { RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, width, height), x, 0, false); }
            finally { RenderTexture.active = previous; }
        }
    }
}
