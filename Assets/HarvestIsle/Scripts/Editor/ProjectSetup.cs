using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/HarvestIsle/Scenes/HarvestIsle.unity";
        private const string SettingsPath = "Assets/HarvestIsle/Settings";
        private const string PipelinePath = SettingsPath + "/HarvestIsleURP.asset";
        private const string WorldMaterialPath = "Assets/HarvestIsle/Resources/Materials/World.mat";

        static ProjectSetup() { EditorApplication.delayCall += FirstOpen; }

        private static void FirstOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            // Asset checks make setup idempotent and avoid modifying settings on every reload.
            if (!File.Exists(PipelinePath) || !File.Exists(WorldMaterialPath)) Configure();
            if (!File.Exists(ScenePath)) CreateScene();
            if (!Application.isBatchMode && SceneManager.GetActiveScene().path == "" && !SceneManager.GetActiveScene().isDirty)
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Harvest Isle/1. Configure project")]
        public static void Configure()
        {
            Directory.CreateDirectory(SettingsPath);
            Directory.CreateDirectory("Assets/HarvestIsle/Resources/Materials");
            AssetDatabase.Refresh();
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, SettingsPath + "/HarvestIsleRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "Harvest Isle Mobile";
                pipeline.msaaSampleCount = 2;
                pipeline.renderScale = 1;
                pipeline.supportsHDR = false;
                pipeline.shadowDistance = 35;
                pipeline.mainLightShadowmapResolution = 1024;
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            // Assign every quality tier, including the tier selected by Android at startup.
            int originalQuality = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(originalQuality, false);
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = 35;

            if (AssetDatabase.LoadAssetAtPath<Material>(WorldMaterialPath) == null)
            {
                // A Resources material also retains the dynamically used shader in player builds.
                var material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
                material.name = "World";
                material.SetFloat("_Smoothness", 0);
                AssetDatabase.CreateAsset(material, WorldMaterialPath);
            }
            PlayerSettings.companyName = "Harvest Isle Studio";
            PlayerSettings.productName = "Mùa Vàng — Đảo Nông Trại";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.harvestisle.prototype");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            // Use the new Input System only. Changing it before opening Play Mode needs an editor restart.
            var serialized = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = serialized.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 1; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Harvest Isle configured. Open HarvestIsle scene and press Play.");
        }

        [MenuItem("Harvest Isle/2. Open game")]
        public static void OpenGame()
        {
            if (!File.Exists(ScenePath)) CreateScene();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        private static void CreateScene()
        {
            Directory.CreateDirectory("Assets/HarvestIsle/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var root = new GameObject("Harvest Isle");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.AddComponent<FarmGame>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.Refresh();
        }

        [MenuItem("Harvest Isle/3. Build Android APK")]
        public static void BuildAndroid()
        {
            Configure();
            if (!File.Exists(ScenePath)) CreateScene();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Install Android Build Support, SDK/NDK and OpenJDK in Unity Hub.");
            Directory.CreateDirectory("Builds/Android");
            EditorUserBuildSettings.buildAppBundle = false;
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Android/HarvestIsle.apk",
                target = BuildTarget.Android, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android build failed: " + report.summary.result);
            Debug.Log("Android APK: " + Path.GetFullPath("Builds/Android/HarvestIsle.apk"));
        }

        [MenuItem("Harvest Isle/4. Build Windows demo")]
        public static void BuildWindows()
        {
            Configure();
            Directory.CreateDirectory("Builds/Windows");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/HarvestIsle.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
        }
    }
}
