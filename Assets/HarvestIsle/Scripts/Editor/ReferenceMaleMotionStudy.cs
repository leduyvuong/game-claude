using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    public static class ReferenceMaleMotionStudy
    {
        public static string ScenePath => ReferenceMaleRigging.Folder + "/" + ReferenceHumanProfile.Asset + "Motion.unity";
        private static string Output => ReferenceHumanProfile.Output + "/Motion";
        private static string Frames => ".tools/model-work/" + ReferenceHumanProfile.Slug + "-frames";

        public static void CheckContacts()
        {
            var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ReferenceMaleRigging.Folder + "/" + ReferenceHumanProfile.Asset + ".prefab"));
            var actor = root.GetComponent<ReferenceMaleMotion>(); actor.Initialize();
            var skin = root.GetComponentInChildren<SkinnedMeshRenderer>(); var mesh = new Mesh();
            for (int action = 0; action < 11; action++)
            {
                float error = 0, floor = 100; int frame = 0; string bone = "";
                for (int i = 0; i < 120; i++)
                {
                    actor.Sample((ReferenceMaleAction)action, ReferenceMaleMotion.Duration((ReferenceMaleAction)action) * i / 120);
                    if (actor.MaximumTargetError > error) { error = actor.MaximumTargetError; frame = i; bone = actor.MaximumTargetBone; }
                    skin.BakeMesh(mesh); mesh.RecalculateBounds(); floor = Mathf.Min(floor, mesh.bounds.min.y);
                    if (i == 60) foreach (var b in actor.bones) if (b.name == "RightShin" || b.name == "RightFoot" || b.name == "RightHand") Debug.Log("CONTACT " + (ReferenceMaleAction)action + " " + b.name + " " + b.position.ToString("F3"));
                }
                Debug.Log("CONTACT_RESULT " + (ReferenceMaleAction)action + " reach=" + error + " frame=" + frame + " minY=" + floor + " bone=" + bone);
            }
            UnityEngine.Object.DestroyImmediate(mesh); UnityEngine.Object.DestroyImmediate(root);
        }

        [MenuItem("Harvest Isle/Characters/Build and capture reference male motions")]
        public static void Build()
        {
            ReferenceMaleStudy.Build();
            Directory.CreateDirectory(Output); Directory.CreateDirectory(Frames);
            Scene scene = EditorSceneManager.NewPreviewScene();
            bool previousAsync = ShaderUtil.allowAsyncCompilation; ShaderUtil.allowAsyncCompilation = false;
            var oldMode = RenderSettings.ambientMode; var oldAmbient = RenderSettings.ambientLight;
            RenderTexture target = null; Texture2D image = null;
            var bakedMeshes = new List<Mesh>(); var snapshots = new List<GameObject>();
            try
            {
                var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ReferenceMaleRigging.Folder + "/" + ReferenceHumanProfile.Asset + ".prefab"));
                root.name = ReferenceHumanProfile.DisplayName; SceneManager.MoveGameObjectToScene(root, scene);
                var actor = root.GetComponent<ReferenceMaleMotion>(); actor.Initialize(); actor.playing = false;
                RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.43f, .46f, .5f);
                Sun(scene, "Key", new Vector3(35, -140, 0), new Color(1, .95f, .88f), 1.15f);
                Sun(scene, "Fill", new Vector3(15, 120, 0), new Color(.83f, .89f, 1), .4f);
                Sun(scene, "Rim", new Vector3(12, 0, 0), Color.white, .7f);
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder); floor.name = "Be thu dong tac";
                SceneManager.MoveGameObjectToScene(floor, scene); floor.transform.position = new Vector3(0, -.032f, .18f);
                floor.transform.localScale = new Vector3(1.8f, .025f, 1.8f);
                floor.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ReferenceMaleRigging.Folder + "/Plinth.mat");
                var camera = new GameObject("Motion camera").AddComponent<Camera>(); SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>(); camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
                camera.orthographic = true; camera.orthographicSize = 1.35f;
                camera.clearFlags = CameraClearFlags.SolidColor; ColorUtility.TryParseHtmlString("#42494B", out var background); camera.backgroundColor = background;
                camera.nearClipPlane = .01f; camera.farClipPlane = 30; camera.allowHDR = false; camera.allowMSAA = false;
                camera.transform.position = new Vector3(3.3f, 1.65f, 5); camera.transform.LookAt(new Vector3(0, 1.13f, .18f));
                const int width = 512, height = 640, fps = 30;
                camera.aspect = (float)width / height; target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32); target.Create();
                image = new Texture2D(width, height, TextureFormat.RGB24, false);
                var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skin in skins)
                {
                    var mesh = new Mesh { name = "CPU pose snapshot", indexFormat = IndexFormat.UInt32 }; bakedMeshes.Add(mesh);
                    var snapshot = new GameObject("Review mesh", typeof(MeshFilter), typeof(MeshRenderer)); snapshot.transform.SetParent(skin.transform, false);
                    snapshot.GetComponent<MeshFilter>().sharedMesh = mesh; snapshot.GetComponent<MeshRenderer>().sharedMaterials = skin.sharedMaterials;
                    snapshots.Add(snapshot); skin.enabled = false;
                }
                float lowest = 100, highest = -100; int checkedFrames = 0;
                for (int motion = 0; motion < ReferenceMaleMotion.Labels.Length; motion++)
                {
                    float motionMin = 100, reachError = 0; int lowestFrame = 0;
                    string folder = Frames + "/" + ((ReferenceMaleAction)motion); Directory.CreateDirectory(folder);
                    foreach (string oldFrame in Directory.GetFiles(folder, "*.png")) File.Delete(oldFrame);
                    float seconds = ReferenceMaleMotion.Duration((ReferenceMaleAction)motion);
                    if (motion == 1 || motion == 2 || motion == 9) seconds *= 2;
                    int samples = Mathf.CeilToInt(seconds * fps);
                    for (int frame = 0; frame < samples; frame++)
                    {
                        actor.Sample((ReferenceMaleAction)motion, (float)frame / fps);
                        if (actor.MaximumTargetError > .012f) throw new InvalidOperationException("Unreachable contact: " + (ReferenceMaleAction)motion + " " + actor.MaximumTargetBone + " error=" + actor.MaximumTargetError);
                        reachError = Mathf.Max(reachError, actor.MaximumTargetError);
                        for (int part = 0; part < skins.Length; part++)
                        {
                            skins[part].BakeMesh(bakedMeshes[part]);
                            bakedMeshes[part].RecalculateBounds();
                            Bounds bounds = bakedMeshes[part].bounds;
                            if (float.IsNaN(bounds.size.x) || bounds.size.magnitude > 4) throw new InvalidOperationException("Invalid skin deformation: " + motion + "/" + frame);
                            if (bounds.min.y < -.008f) throw new InvalidOperationException("Outfit penetrates studio floor: " + skins[part].name + " " + motion + "/" + frame + " y=" + bounds.min.y);
                            if (part == 0)
                            {
                                if (bounds.min.y < -.008f) throw new InvalidOperationException("Foot penetrates studio floor: " + motion + "/" + frame + " y=" + bounds.min.y);
                                lowest = Mathf.Min(lowest, bounds.min.y); highest = Mathf.Max(highest, bounds.max.y);
                                if (bounds.min.y < motionMin) { motionMin = bounds.min.y; lowestFrame = frame; }
                            }
                        }
                        if (motion == 0 && frame == 0) Render(camera, target, image);
                        Render(camera, target, image);
                        byte[] png = image.EncodeToPNG(); File.WriteAllBytes(folder + "/" + frame.ToString("D3") + ".png", png);
                        if (frame == samples / 2)
                        {
                            File.WriteAllBytes(Output + "/" + ((ReferenceMaleAction)motion) + ".png", png);
                            foreach (var bone in actor.bones) if (bone.name == "RightShin" || bone.name == "RightFoot") Debug.Log("POSE " + (ReferenceMaleAction)motion + " " + bone.name + " " + bone.position.ToString("F3"));
                        }
                        checkedFrames++;
                    }
                    Debug.Log("REFERENCE_MOTION_CAPTURED: " + (ReferenceMaleAction)motion + " minY=" + motionMin + " frame=" + lowestFrame + " reachError=" + reachError);
                }
                foreach (var snapshot in snapshots) UnityEngine.Object.DestroyImmediate(snapshot); snapshots.Clear();
                foreach (var skin in skins) skin.enabled = true;
                actor.Sample(ReferenceMaleAction.Idle, 0); actor.action = ReferenceMaleAction.Idle; actor.playing = true;
                camera.cameraType = CameraType.Game; camera.overrideSceneCullingMask = 0; camera.orthographicSize = 1.35f;
                var viewer = root.AddComponent<ReferenceMaleMotionViewer>(); viewer.actor = actor; viewer.viewCamera = camera; viewer.title = ReferenceHumanProfile.DisplayName;
                SaveScene(scene);
                File.WriteAllText(".tools/model-work/" + ReferenceHumanProfile.Slug + "-motion-result.json", "{\"status\":\"complete\",\"bones\":" + actor.bones.Length + ",\"actions\":11,\"sampledFrames\":" + checkedFrames + ",\"minimumBodyY\":" + lowest.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\"maximumBodyY\":" + highest.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}");
                Debug.Log("REFERENCE_MALE_MOTIONS_COMPLETE: " + checkedFrames + " frames; minimum body y=" + lowest);
            }
            finally
            {
                foreach (var snapshot in snapshots) if (snapshot != null) UnityEngine.Object.DestroyImmediate(snapshot);
                foreach (var mesh in bakedMeshes) UnityEngine.Object.DestroyImmediate(mesh);
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                RenderSettings.ambientMode = oldMode; RenderSettings.ambientLight = oldAmbient; ShaderUtil.allowAsyncCompilation = previousAsync;
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        [MenuItem("Harvest Isle/Characters/Open reference male motion studio")]
        public static void Open()
        {
            EditorSceneManager.OpenScene(ScenePath); EditorApplication.isPlaying = true;
        }
        private static void SaveScene(Scene source)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene saved = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, Application.isBatchMode && string.IsNullOrEmpty(previous.path) ? NewSceneMode.Single : NewSceneMode.Additive);
            var replacements = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            foreach (var root in source.GetRootGameObjects())
            {
                var copy = UnityEngine.Object.Instantiate(root); copy.name = root.name; SceneManager.MoveGameObjectToScene(copy, saved);
                replacements[root] = copy;
            }
            ReferenceMaleMotionViewer viewer = null; Camera camera = null;
            foreach (var root in saved.GetRootGameObjects())
            {
                if (root.GetComponent<ReferenceMaleMotionViewer>() != null) viewer = root.GetComponent<ReferenceMaleMotionViewer>();
                if (root.GetComponent<Camera>() != null) camera = root.GetComponent<Camera>();
            }
            if (viewer != null) { viewer.actor = viewer.GetComponent<ReferenceMaleMotion>(); viewer.viewCamera = camera; }
            try
            {
                SceneManager.SetActiveScene(saved); RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.43f, .46f, .5f);
                EditorSceneManager.SaveScene(saved, ScenePath);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) { SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(saved, true); }
            }
        }
        private static void Sun(Scene scene, string name, Vector3 rotation, Color color, float intensity)
        {
            var light = new GameObject(name).AddComponent<Light>(); SceneManager.MoveGameObjectToScene(light.gameObject, scene);
            light.type = LightType.Directional; light.color = color; light.intensity = intensity; light.transform.rotation = Quaternion.Euler(rotation);
        }
        private static void Render(Camera camera, RenderTexture target, Texture2D image)
        {
            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
            var previous = RenderTexture.active;
            try { RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0, false); image.Apply(); }
            finally { RenderTexture.active = previous; }
        }
    }
}
