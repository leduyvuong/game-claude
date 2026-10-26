using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    /// <summary>A small transform rig: no Animator Controller or third-party animation dependency.</summary>
    public sealed class FarmerAvatar : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform upperBody;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private GameObject wateringCan;
        [SerializeField] private ReferenceMaleMotion humanMotion;
        private Vector3[] humanIdlePositions;
        private Quaternion[] humanIdleRotations;
        private Vector3 bodyRest;
        private Quaternion headRest;
        private bool restPoseCached;
        private Transform leftKnee, rightKnee, leftElbow, rightElbow;
        private Vector3 previousPosition;
        private float gaitTime;
        private float walking;
        private float desiredWalking;
        private float actionStarted = -100;
        private FarmTool action;
        private Mesh[] ownedMeshes;
        private Material[] ownedMaterials;

        public Mesh[] GeneratedMeshes { get { return ownedMeshes; } }
        public Material[] GeneratedMaterials { get { return ownedMaterials; } }

        public void ConfigureHuman(ReferenceMaleMotion motion)
        {
            humanMotion = motion; motion.playing = false; motion.enabled = false;
            motion.Initialize(); SetPose(0, 0, 0, FarmTool.Plant);
        }

        public void ConfigureRig(Transform body, Transform upperBody, Transform head, Transform leftArm, Transform rightArm,
            Transform leftLeg, Transform rightLeg, GameObject wateringCan)
        {
            this.body = body; this.upperBody = upperBody; this.head = head; this.leftArm = leftArm; this.rightArm = rightArm;
            this.leftLeg = leftLeg; this.rightLeg = rightLeg; this.wateringCan = wateringCan;
            CacheRestPose();
            SetPose(0, 0, 0, FarmTool.Plant);
        }

        public void OwnResources(Mesh[] meshes, Material[] materials)
        {
            ownedMeshes = meshes; ownedMaterials = materials;
        }

        // Assets baked by the Editor are shared, and must never be destroyed with an avatar instance.
        public void ReleaseResourceOwnership() { ownedMeshes = null; ownedMaterials = null; }

        private void Awake() { CacheRestPose(); previousPosition=transform.position; }
        private void OnEnable() {previousPosition=transform.position;walking=0;}
        private void CacheRestPose()
        {
            if(leftArm!=null)leftElbow=leftArm.Find("Elbow");
            if(rightArm!=null)rightElbow=rightArm.Find("Elbow");
            if(leftLeg!=null)leftKnee=leftLeg.Find("Knee");
            if(rightLeg!=null)rightKnee=rightLeg.Find("Knee");
            if (body != null) bodyRest = body.localPosition;
            if (head != null) headRest = head.localRotation;
            restPoseCached = body != null && upperBody != null && head != null;
        }

        public void SetWalking(bool moving) { desiredWalking = moving ? 1 : 0; }
        public void Perform(FarmTool tool) { action = tool; actionStarted = Time.time; }

        private void LateUpdate()
        {
            float distance=Vector3.Distance(transform.position,previousPosition);previousPosition=transform.position;
            float scale=Mathf.Max(.1f,transform.lossyScale.y);
            float speed=Time.deltaTime>0?distance/Time.deltaTime/scale:0;
            if(distance<scale*2)gaitTime+=distance/scale/ .62f * (Mathf.PI*2/9);
            float weight=desiredWalking>0&&speed>.015f?Mathf.Clamp01(speed/.5f):0;
            walking = Mathf.MoveTowards(walking, weight, Time.deltaTime * 5);
            float elapsed = Time.time - actionStarted;
            float actionWeight = elapsed >= 0 && elapsed < 0.85f ? Mathf.Sin(elapsed / 0.85f * Mathf.PI) : 0;
            if (humanMotion != null)
            {
                PoseHuman(walking>.01f?gaitTime:Time.time, walking, actionWeight, action, Mathf.Clamp01(elapsed/.85f));
                return;
            }
            SetPose(walking>.01f?gaitTime:Time.time, walking, actionWeight, action);
        }

        public void SetPose(float time, float walkWeight, float actionWeight, FarmTool tool)
        {
            if (humanMotion != null) { PoseHuman(time, walkWeight, actionWeight, tool, .5f); return; }
            if (body == null || upperBody == null || head == null) return;
            if (!restPoseCached) CacheRestPose();
            float stride = Mathf.Sin(time * 9) * walkWeight;
            float breathing = Mathf.Sin(time * 2.2f) * 0.008f * (1 - walkWeight);
            body.localPosition = bodyRest + Vector3.up * (breathing + -Mathf.Abs(stride) * 0.012f);
            float lean = actionWeight * (tool == FarmTool.Water ? 10 : 19);
            body.localRotation = Quaternion.Euler(0, 0, -stride * 2);
            upperBody.localRotation = Quaternion.Euler(lean+walkWeight*3, -stride*4, 0);
            head.localRotation = headRest * Quaternion.Euler(-lean * 0.3f, Mathf.Sin(time * 1.4f) * (1 - walkWeight) * 3, stride);
            leftLeg.localRotation = Quaternion.Euler(stride * 18, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-stride * 18, 0, 0);
            if(leftKnee!=null)leftKnee.localRotation=Quaternion.Euler(Mathf.Max(0,-Mathf.Cos(time*9))*38*walkWeight,0,0);
            if(rightKnee!=null)rightKnee.localRotation=Quaternion.Euler(Mathf.Max(0,Mathf.Cos(time*9))*38*walkWeight,0,0);
            if(leftElbow!=null)leftElbow.localRotation=Quaternion.Euler(-8-walkWeight*8-actionWeight*14,0,0);
            if(rightElbow!=null)rightElbow.localRotation=Quaternion.Euler(-8-walkWeight*8-actionWeight*14,0,0);
            float reach = actionWeight * (tool == FarmTool.Water ? 34 : 48);
            leftArm.localRotation = Quaternion.Euler(-stride * 20 - (tool == FarmTool.Water ? actionWeight * 12 : reach), 0, -5);
            rightArm.localRotation = Quaternion.Euler(stride * 20 - reach, 0, 5);
            if (wateringCan != null) wateringCan.SetActive(tool == FarmTool.Water && actionWeight > 0.03f);
        }

        private void PoseHuman(float time, float walkWeight, float actionWeight, FarmTool tool, float progress)
        {
            var m = humanMotion;
            if (actionWeight > .001f)
            {
                var pose = tool == FarmTool.Water ? ReferenceMaleAction.Water : tool == FarmTool.Harvest ? ReferenceMaleAction.Harvest : ReferenceMaleAction.Plant;
                m.Sample(pose, Mathf.Min(.999f, progress) * ReferenceMaleMotion.Duration(pose));
                return;
            }
            m.Sample(ReferenceMaleAction.Idle, time);
            if (walkWeight <= .001f) return;
            if (humanIdlePositions == null)
            { humanIdlePositions = new Vector3[m.bones.Length]; humanIdleRotations = new Quaternion[m.bones.Length]; }
            for (int i=0;i<m.bones.Length;i++) { humanIdlePositions[i]=m.bones[i].localPosition; humanIdleRotations[i]=m.bones[i].localRotation; }
            m.Sample(ReferenceMaleAction.Walk, time * 9 / (Mathf.PI * 2) * ReferenceMaleMotion.Duration(ReferenceMaleAction.Walk));
            for (int i=0;i<m.bones.Length;i++)
            {
                m.bones[i].localPosition=Vector3.Lerp(humanIdlePositions[i],m.bones[i].localPosition,walkWeight);
                m.bones[i].localRotation=Quaternion.Slerp(humanIdleRotations[i],m.bones[i].localRotation,walkWeight);
            }
        }

        private void OnDestroy()
        {
            if (ownedMeshes != null)
                foreach (Mesh mesh in ownedMeshes) Dispose(mesh);
            if (ownedMaterials != null)
                foreach (Material material in ownedMaterials) Dispose(material);
        }

        private static void Dispose(Object item)
        {
            if (item == null) return;
            if (Application.isPlaying) Destroy(item);
            else DestroyImmediate(item);
        }
    }
}
