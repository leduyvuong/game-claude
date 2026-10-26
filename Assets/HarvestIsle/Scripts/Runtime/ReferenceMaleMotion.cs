using System;
using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle
{
    public enum ReferenceMaleAction { Idle, Walk, Run, Wave, Crouch, Jump, Water, Hoe, Harvest, Carry, Plant }

    /// <summary>Deterministic, in-place motion study on a weighted 49-bone mesh.</summary>
    public sealed partial class ReferenceMaleMotion : MonoBehaviour
    {
        public Transform[] bones;
        public Vector3[] bindPositions;
        public Vector3[] bindTails;
        public GameObject wateringCan, hoe, crate;
        public GameObject workPlant, soilPatch;
        public Transform[] waterDrops;
        public bool playing = true;
        [Range(.25f, 2)] public float speed = 1;
        public ReferenceMaleAction action;
        public bool femaleProfile;
        public static readonly string[] Labels = { "Đứng nghỉ", "Đi bộ", "Chạy", "Vẫy tay", "Ngồi xuống", "Nhảy", "Tưới cây", "Cuốc đất", "Thu hoạch", "Bê giỏ", "Trồng cây" };
        private Dictionary<string, int> indices;
        private Vector3[] restLocal, transitionPositions;
        private Quaternion[] transitionRotations;
        private float time, transition = 1;
        public float MotionTime { get { return time; } }
        public float MaximumTargetError { get; private set; }
        public string MaximumTargetBone { get; private set; }
        public static float Duration(ReferenceMaleAction value)
        {
            switch (value)
            {
                case ReferenceMaleAction.Walk: return 1.2f;
                case ReferenceMaleAction.Run: return .76f;
                case ReferenceMaleAction.Carry: return 1.5f;
                case ReferenceMaleAction.Wave: return 3.6f;
                case ReferenceMaleAction.Crouch: return 3.4f;
                case ReferenceMaleAction.Jump: return 2.4f;
                case ReferenceMaleAction.Hoe: return 2.8f;
                case ReferenceMaleAction.Harvest: return 4.2f;
                case ReferenceMaleAction.Plant: return 4.8f;
                default: return 4;
            }
        }

        public void Initialize()
        {
            indices = new Dictionary<string, int>();
            restLocal = new Vector3[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                indices[bones[i].name] = i;
                int parent = Array.IndexOf(bones, bones[i].parent);
                restLocal[i] = bindPositions[i] - (parent >= 0 ? bindPositions[parent] : Vector3.zero);
            }
            CacheSoles();
        }
        private Transform B(string name) { return bones[indices[name]]; }
        private Vector3 P(string name) { return bindPositions[indices[name]]; }
        private void Awake() { if (bones != null && bones.Length > 0) Initialize(); }
        private void Update()
        {
            if (!playing || bones == null || bones.Length == 0) return;
            time += Time.deltaTime * speed;
            Sample(action, time);
            if (transition < 1 && transitionRotations != null)
            {
                transition = Mathf.Min(1, transition + Time.deltaTime / .32f);
                float w = Mathf.SmoothStep(0, 1, transition);
                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i].localPosition = Vector3.Lerp(transitionPositions[i], bones[i].localPosition, w);
                    bones[i].localRotation = Quaternion.Slerp(transitionRotations[i], bones[i].localRotation, w);
                }
                CorrectTransitionFeet();
                if (transition < .95f)
                {
                    if (wateringCan != null) wateringCan.SetActive(false);
                    if (hoe != null) hoe.SetActive(false);
                    if (crate != null) crate.SetActive(false);
                    if (workPlant != null) workPlant.SetActive(false);
                    if (waterDrops != null) foreach (var drop in waterDrops) drop.gameObject.SetActive(false);
                }
            }
        }
        public void Select(ReferenceMaleAction value)
        {
            transitionPositions = new Vector3[bones.Length]; transitionRotations = new Quaternion[bones.Length];
            for (int i = 0; i < bones.Length; i++) { transitionPositions[i] = bones[i].localPosition; transitionRotations[i] = bones[i].localRotation; }
            action = value; time = 0; transition = 0;
        }

        private void Solve(string upperName, string lowerName, string endName, Vector3 localTarget, Vector3 localPole)
        {
            Transform upper = B(upperName), lower = B(lowerName), end = B(endName);
            Vector3 start = upper.position, target = transform.TransformPoint(localTarget);
            float scale = transform.lossyScale.x;
            float a = Vector3.Distance(P(upperName), P(lowerName)) * scale;
            float b = Vector3.Distance(P(lowerName), P(endName)) * scale;
            Vector3 delta = target - start; float distance = Mathf.Clamp(delta.magnitude, Mathf.Abs(a - b) + .001f, a + b - .001f);
            Vector3 direction = delta.normalized;
            Vector3 pole = Vector3.ProjectOnPlane(transform.TransformDirection(localPole), direction).normalized;
            if (pole.sqrMagnitude < .1f) pole = Vector3.ProjectOnPlane(transform.right, direction).normalized;
            float along = (a * a - b * b + distance * distance) / (2 * distance);
            Vector3 middle = start + direction * along + pole * Mathf.Sqrt(Mathf.Max(0, a * a - along * along));
            upper.rotation = Quaternion.FromToRotation(transform.TransformDirection(P(lowerName) - P(upperName)), middle - start) * transform.rotation;
            lower.rotation = Quaternion.FromToRotation(transform.TransformDirection(P(endName) - P(lowerName)), start + direction * distance - middle) * transform.rotation;
            float error = Vector3.Distance(end.position, target) / scale;
            if (error > MaximumTargetError) { MaximumTargetError = error; MaximumTargetBone = endName; }
        }
    }
}
