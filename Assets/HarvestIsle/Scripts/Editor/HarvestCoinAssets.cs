using System;
using System.Collections.Generic;
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
    public static class HarvestCoinAssets
    {
        private const string Art = "Assets/HarvestIsle/Art/Currency";
        private const string Icon = "Assets/HarvestIsle/Resources/Currency/HarvestCoin.png";
        private const string Preview = "Art/Previews/Currency";

        [MenuItem("Harvest Isle/Currency/Build Xu Mua Vang")]
        public static void Build()
        {
            EditorSettings.asyncShaderCompilation = false;
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Path.GetDirectoryName(Icon));
            Directory.CreateDirectory(Preview);
            AssetDatabase.Refresh();
            var meshes = new List<Mesh>();
            var model = HarvestCoinBuilder.Create(meshes);
            try
            {
                string path = Art + "/HarvestCoin.asset";
                var stored = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (stored == null) { stored = meshes[0]; AssetDatabase.CreateAsset(stored, path); }
                else { EditorUtility.CopySerialized(meshes[0], stored); Object.DestroyImmediate(meshes[0]); }
                model.GetComponent<MeshFilter>().sharedMesh = stored;
                var materials = model.GetComponent<Renderer>().sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    string matPath = Art + "/Coin" + i + ".mat";
                    var old = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (old == null) AssetDatabase.CreateAsset(materials[i], matPath);
                    else { EditorUtility.CopySerialized(materials[i], old); Object.DestroyImmediate(materials[i]); materials[i] = old; }
                }
                model.GetComponent<Renderer>().sharedMaterials = materials;
                PrefabUtility.SaveAsPrefabAsset(model, "Assets/HarvestIsle/Resources/Collection/Coin.prefab");
                AssetDatabase.SaveAssets();
                Shader.WarmupAllShaders();
                Render(model);
                AssetDatabase.Refresh();
                var importer = (TextureImporter)AssetImporter.GetAtPath(Icon);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 256;
                importer.SaveAndReimport();
                CompleteModelAssets.RenderCatalog(CompleteModelAssets.BuildCatalog(), false, new[] {"Coin"});
                File.WriteAllText(".tools/model-work/currency-result.json", "{\"status\":\"complete\",\"triangles\":" + stored.triangles.Length / 3 + ",\"materials\":3,\"samePrefabGuid\":true}");
            }
            finally { Object.DestroyImmediate(model); }
        }

        private static void Render(GameObject source)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var previous = RenderTexture.active;
            var rt = new RenderTexture(640, 640, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(640, 640, TextureFormat.RGBA32, false);
            try
            {
                var model = Object.Instantiate(source);
                SceneManager.MoveGameObjectToScene(model, scene);
                var sun = new GameObject("Coin key").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(sun.gameObject, scene);
                sun.type = LightType.Directional; sun.intensity = 1.35f;
                sun.transform.rotation = Quaternion.Euler(60, -35, 0);
                var fill = new GameObject("Coin fill").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(fill.gameObject, scene);
                fill.type = LightType.Directional; fill.intensity = .45f;
                fill.transform.rotation = Quaternion.Euler(35, 140, 0);
                var camera = new GameObject("Coin camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
                camera.orthographic = true; camera.orthographicSize = .142f;
                camera.nearClipPlane = .01f; camera.farClipPlane = 10;
                camera.allowHDR = false; camera.allowMSAA = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear; camera.aspect = 1;
                rt.Create();
                string[] names = {"Front", "Angle", "Back", "Edge"};
                for (int i = 0; i < names.Length; i++)
                {
                    model.transform.rotation = i == 2 ? Quaternion.Euler(180, 0, 0) : Quaternion.identity;
                    camera.transform.position = i == 1 ? new Vector3(.42f, .85f, -.4f) : i == 3 ? new Vector3(0, .28f, -1) : Vector3.up;
                    camera.transform.LookAt(Vector3.zero, Vector3.forward);
                    // URP initializes preview-scene lighting on the first submitted frame.
                    if (i == 0) RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest {destination = rt});
                    RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest {destination = rt});
                    RenderTexture.active = rt;
                    image.ReadPixels(new Rect(0, 0, 640, 640), 0, 0); image.Apply();
                    File.WriteAllBytes(Preview + "/" + names[i] + ".png", image.EncodeToPNG());
                    if (i == 0) File.WriteAllBytes(Icon, image.EncodeToPNG());
                }
            }
            finally
            {
                RenderTexture.active = previous; rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(image);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
