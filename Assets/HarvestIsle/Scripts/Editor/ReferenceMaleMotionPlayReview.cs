using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HarvestIsle.Editor
{
    [InitializeOnLoad]
    public static class ReferenceMaleMotionPlayReview
    {
        private const string Key = "HarvestIsle.ReferenceMalePlayReview";
        private static int stage = -1, ticks;
        private static float savedTime;
        private static double started, stageStarted;
        private static Vector3 previousHead;
        private static Mesh baked;
        private static float minimumBodyY = 100;
        private static int poseFrames;
        private const string ProfileKey = "HarvestIsle.ReferenceReviewProfile";
        static ReferenceMaleMotionPlayReview() { EditorApplication.update += Tick; }

        public static void Begin()
        {
            SessionState.SetBool(Key, true);
            SessionState.SetString(ProfileKey, ReferenceHumanProfile.Slug);
            EditorSceneManager.OpenScene(ReferenceMaleMotionStudy.ScenePath);
            EditorApplication.isPlaying = true;
        }
        private static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            try
            {
                var actor = UnityEngine.Object.FindFirstObjectByType<ReferenceMaleMotion>();
                var viewer = UnityEngine.Object.FindFirstObjectByType<ReferenceMaleMotionViewer>();
                if (actor == null || viewer == null || viewer.actor != actor || viewer.viewCamera == null) throw new InvalidOperationException("Motion scene references unavailable in Play");
                if (stage < 0) { started = stageStarted = EditorApplication.timeSinceStartup; stage = 0; ticks = 0; actor.Select(ReferenceMaleAction.Idle); savedTime = actor.MotionTime; previousHead = actor.bones[4].position; }
                if (baked == null) baked = new Mesh();
                foreach (var skin in actor.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    skin.BakeMesh(baked); baked.RecalculateBounds();
                    minimumBodyY = Mathf.Min(minimumBodyY, baked.bounds.min.y);
                    if (baked.bounds.min.y < -.015f) throw new InvalidOperationException("Transition penetrates floor: " + actor.action + " " + skin.name + " y=" + baked.bounds.min.y);
                }
                poseFrames++;
                if (EditorApplication.timeSinceStartup - started > 90) throw new TimeoutException("Motion playback stalled");
                if (stage < 11 && actor.MotionTime < .9f) return;
                if (stage >= 11 && EditorApplication.timeSinceStartup - stageStarted < .5) return;
                if (++ticks < 20) return;
                ticks = 0;
                if (stage < 11)
                {
                    if (actor.MotionTime <= savedTime) throw new InvalidOperationException("Animation time did not advance: " + actor.action);
                    if (actor.action != (ReferenceMaleAction)stage) throw new InvalidOperationException("Selection did not persist");
                    foreach (var bone in actor.bones) if (float.IsNaN(bone.position.x)) throw new InvalidOperationException("Invalid pose");
                    if (stage == 4 && Vector3.Distance(previousHead, actor.bones[4].position) < .015f) throw new InvalidOperationException("Crouch did not deform the model");
                    previousHead = actor.bones[4].position;
                    stage++;
                    if (stage < 11) { actor.Select((ReferenceMaleAction)stage); savedTime = actor.MotionTime; }
                    else { actor.playing = false; savedTime = actor.MotionTime; stageStarted = EditorApplication.timeSinceStartup; }
                }
                else if (stage == 11)
                {
                    if (actor.MotionTime != savedTime) throw new InvalidOperationException("Pause did not freeze motion");
                    actor.playing = true; actor.speed = .5f; stage++; stageStarted = EditorApplication.timeSinceStartup;
                }
                else
                {
                    if (actor.MotionTime <= savedTime) throw new InvalidOperationException("Resume did not restart motion");
                    actor.speed = 1; actor.Select(ReferenceMaleAction.Walk);
                    Finish(true, "11 action selections, advancing runtime poses, pause/resume and half speed verified");
                }
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }
        private static void Finish(bool success, string message)
        {
            SessionState.SetBool(Key, false);
            File.WriteAllText(".tools/model-work/" + SessionState.GetString(ProfileKey, "reference-male") + "-play-result.json", JsonUtility.ToJson(new Result { status = success ? "complete" : "failed", message = message, actions = 11, playMode = true, minimumBodyY = minimumBodyY, poseFrames = poseFrames }, true));
            if (baked != null) UnityEngine.Object.DestroyImmediate(baked);
            if (success) Debug.Log("REFERENCE_MALE_PLAY_VERIFIED: " + message); else Debug.LogError(message);
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
        }
        [Serializable] private class Result { public string status, message; public int actions, poseFrames; public float minimumBodyY; public bool playMode; }
    }
}
