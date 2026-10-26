using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class ReferenceMaleMotion
    {
        private Vector3[][] soles;
        private static float Ease(float p, float from, float to) { return Mathf.SmoothStep(0, 1, Mathf.InverseLerp(from, to, p)); }
        private static float Window(float p, float a, float b, float c, float d) { return Ease(p, a, b) * (1 - Ease(p, c, d)); }

        // Capture actual sole support points, not renderer bounds. Pitch rotates around
        // the ankle; the support correction keeps heel/toe contacts on the ground.
        private void CacheSoles()
        {
            if (soles != null) return;
            soles = new Vector3[2][];
            var skin = GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin == null) { soles[0] = soles[1] = new[] { new Vector3(0, -.074f, .1f) }; return; }
            var vertices = skin.sharedMesh.vertices; var weights = skin.sharedMesh.boneWeights;
            for (int side = 0; side < 2; side++)
            {
                string name = side == 0 ? "LeftFoot" : "RightFoot"; int bone = indices[name];
                var candidates = new List<Vector3>();
                for (int i = 0; i < vertices.Length; i++)
                    if (weights[i].boneIndex0 == bone && weights[i].weight0 > .95f) candidates.Add(vertices[i] - P(name));
                var support = new List<Vector3>();
                for (int degrees = -40; degrees <= 80; degrees += 5)
                {
                    float min = float.MaxValue; Vector3 point = Vector3.zero;
                    var rotation = Quaternion.Euler(degrees, 0, 0);
                    foreach (var v in candidates) if ((rotation * v).y < min) { min = (rotation * v).y; point = v; }
                    if (!support.Contains(point)) support.Add(point);
                }
                soles[side] = support.ToArray();
            }
        }
        private void GroundFoot(int side, ref Vector3 foot, float pitch)
        {
            float min = 0; var rotation = Quaternion.Euler(pitch, side == 0 ? 3 : -3, 0);
            foreach (var v in soles[side]) min = Mathf.Min(min, (rotation * v).y);
            foot.y += -min + .002f;
        }

        public void Sample(ReferenceMaleAction value, float seconds)
        {
            if (indices == null) Initialize();
            MaximumTargetError = 0;
            for (int i = 0; i < bones.Length; i++) { bones[i].localPosition = restLocal[i]; bones[i].localRotation = Quaternion.identity; }
            float p = Mathf.Repeat(seconds / Duration(value), 1), phase = p * Mathf.PI * 2;
            float breath = Mathf.Sin(seconds * Mathf.PI * .5f);
            Vector3 hip = new Vector3(.008f * Mathf.Sin(phase), .002f * breath, 0);
            float lean = 0, twist = 0, pelvisYaw = 0, roll = 0, lookDown = 0;
            Vector3 lf = new Vector3(.15f, 0, .014f), rf = new Vector3(-.15f, 0, .014f);
            Vector3 lh = new Vector3(.25f, .94f, .055f), rh = new Vector3(-.25f, .94f, .055f);
            Vector3 ld = Vector3.down, rd = Vector3.down, lp = Vector3.left, rp = Vector3.right;
            Vector3 leftPole = new Vector3(.3f, -.5f, -1), rightPole = new Vector3(-.3f, -.5f, -1);
            float lPitch = 0, rPitch = 0, lGrip = 15, rGrip = 15;
            Quaternion toolRotation = Quaternion.identity; Vector3 toolPosition = Vector3.zero;
            bool locomotion = value == ReferenceMaleAction.Walk || value == ReferenceMaleAction.Run || value == ReferenceMaleAction.Carry;
            if (locomotion)
            {
                bool run = value == ReferenceMaleAction.Run, carry = value == ReferenceMaleAction.Carry;
                float stride = run ? .29f : carry ? .15f : .23f;
                lf.x = .12f; rf.x = -.12f;
                Gait(p, run, stride, ref lf, out lPitch); Gait(p + .5f, run, stride, ref rf, out rPitch);
                float swing = Mathf.Cos(phase), delayed = Mathf.Cos(phase - .25f);
                hip = new Vector3(.022f * Mathf.Sin(phase + .6f), -.045f - (run ? .03f : .018f) * Mathf.Sin(phase * 2), run ? .035f : 0);
                pelvisYaw = swing * (run ? 7 : 4); twist = -swing * (run ? 12 : 7); roll = Mathf.Sin(phase) * 1.8f;
                lean = run ? 11 : carry ? -3 : 2;
                if (run)
                {
                    lh = new Vector3(.235f, 1.14f - .06f * delayed, .23f - .20f * delayed);
                    rh = new Vector3(-.235f, 1.14f + .06f * delayed, .23f + .20f * delayed);
                    ld = new Vector3(0, -.2f, 1); rd = ld; lGrip = rGrip = 62;
                }
                else
                {
                    lh.z -= .16f * delayed; rh.z += .16f * delayed;
                    lh.y += .025f * (1 - delayed); rh.y += .025f * (1 + delayed);
                    ld = new Vector3(0, -1, -.18f * delayed); rd = new Vector3(0, -1, .18f * delayed);
                }
                if (carry)
                {
                    toolPosition = new Vector3(.008f * Mathf.Sin(phase), 1.04f + hip.y * .5f, .34f);
                    toolRotation = Quaternion.Euler(0, twist * .18f, roll * .35f);
                    ld = rd = Vector3.forward; lp = Vector3.up; rp = Vector3.up; lGrip = rGrip = 32;
                    lh = WristAt("Left", toolPosition + toolRotation * new Vector3(.235f, -.045f, -.02f), ld, lp);
                    rh = WristAt("Right", toolPosition + toolRotation * new Vector3(-.235f, -.045f, -.02f), rd, rp);
                }
            }
            if (value == ReferenceMaleAction.Wave)
            {
                float lift = Window(p, .04f, .25f, .77f, .96f);
                float greeting = Mathf.Sin((p - .25f) * Mathf.PI * 12) * Window(p, .25f, .32f, .68f, .76f);
                hip.x = -.022f * lift; twist = -5 * lift; roll = 2 * lift;
                rh = Vector3.Lerp(rh, new Vector3(-.38f + greeting * .035f, 1.65f, .10f), lift);
                rd = Vector3.Slerp(Vector3.down, new Vector3(greeting * .25f, 1, 0), lift);
                rp = Vector3.Slerp(Vector3.right, Vector3.forward, lift); rGrip = Mathf.Lerp(15, 2, lift);
                rightPole = Vector3.Lerp(rightPole, new Vector3(-1, -.4f, 0), lift);
            }
            if (value == ReferenceMaleAction.Crouch)
            {
                float bend = Window(p, .04f, .36f, .62f, .96f);
                hip = new Vector3(0, -.49f * bend, -.14f * bend); lean = 37 * bend;
                lh = Vector3.Lerp(lh, new Vector3(.24f, .46f, .42f), bend);
                rh = Vector3.Lerp(rh, new Vector3(-.24f, .46f, .42f), bend);
                ld = rd = Vector3.Lerp(Vector3.down, new Vector3(0, -.6f, .8f), bend);
                lookDown = 8 * bend;
            }
            if (value == ReferenceMaleAction.Jump)
            {
                float load = Window(p, .04f, .22f, .22f, .33f);
                float airT = Mathf.InverseLerp(.33f, .66f, p);
                float air = p >= .33f && p <= .66f ? 4 * airT * (1 - airT) : 0;
                float land = Window(p, .66f, .73f, .73f, .94f);
                float tuck = air * air;
                hip = new Vector3(0, -.20f * load + .34f * air - .15f * land, -.065f * (load + land));
                lean = 20 * load + 17 * land;
                lf.y = rf.y = .34f * air + .08f * tuck;
                lf.z -= .10f * tuck; rf.z -= .10f * tuck; lPitch = rPitch = air * 22;
                float arm = Window(p, .20f, .38f, .60f, .87f);
                lh = new Vector3(.28f, .96f + .45f * arm, -.14f * load + .24f * arm);
                rh = new Vector3(-.28f, lh.y, lh.z); ld = rd = Vector3.Lerp(Vector3.down, Vector3.forward, arm);
                lGrip = rGrip = 20;
            }
            if (value == ReferenceMaleAction.Water)
            {
                float pour = Window(p, .10f, .33f, .64f, .90f);
                lf.z = .13f; rf.z = -.13f; hip.z = .025f * pour;
                lean = 6 + 12 * pour; twist = -8; lookDown = 16 * pour;
                toolRotation = Quaternion.Euler(8 + 32 * pour, -12 + 9 * Mathf.Sin(phase) * pour, 0);
                toolPosition = new Vector3(-.27f, .94f - .05f * pour, .33f + .07f * pour);
                rd = toolRotation * Vector3.forward; rp = toolRotation * Vector3.down; rGrip = 67;
                rh = WristAt("Right", toolPosition + toolRotation * new Vector3(0, .14f, 0), rd, rp);
                lh = new Vector3(.24f, 1.0f, -.015f); lGrip = 25;
            }
            if (value == ReferenceMaleAction.Hoe)
            {
                // Slow preparation, faster contact, then a deliberate pull through soil.
                float lift = Window(p, .02f, .28f, .30f, .48f);
                float pull = Window(p, .48f, .73f, .83f, 1);
                lf = new Vector3(.20f, 0, .16f); rf = new Vector3(-.20f, 0, -.19f);
                hip = new Vector3(.025f * pull, -.16f - .045f * pull, -.025f + .035f * pull);
                lean = 48 - 10 * lift + 8 * pull; twist = -9 + 12 * pull; lookDown = 12;
                toolPosition = new Vector3(.17f, .035f + .25f * lift, .90f - .20f * pull);
                toolRotation = Quaternion.Euler(-42 - 10 * lift + 3 * pull, 0, 12);
                Vector3 axis = toolRotation * Vector3.up;
                ld = toolRotation * Vector3.forward; rd = -ld; lp = -axis; rp = -axis;
                lh = WristAt("Left", toolPosition + axis * .95f, ld, lp);
                rh = WristAt("Right", toolPosition + axis * 1.17f, rd, rp);
                lGrip = rGrip = 72;
            }
            if (value == ReferenceMaleAction.Harvest || value == ReferenceMaleAction.Plant)
            {
                bool plant = value == ReferenceMaleAction.Plant;
                float reach = Window(p, .04f, .28f, .70f, .96f);
                float pull = Window(p, .35f, .55f, .68f, .91f);
                hip = new Vector3(-.025f, -.48f, -.16f);
                lf = new Vector3(.19f, 0, .27f); rf = new Vector3(-.17f, 0, -.47f); rPitch = 57;
                lean = 40 + 30 * reach - (plant ? 0 : 8 * pull); twist = plant ? 0 : -6 * reach; lookDown = 17;
                lh = new Vector3(.22f, .52f, .36f);
                rh = Vector3.Lerp(new Vector3(-.16f, .61f, .33f), new Vector3(-.05f, .36f, .46f), reach);
                ld = new Vector3(0, -.85f, .3f); rd = new Vector3(0, -.8f, .3f); lGrip = 12;
                if (plant)
                {
                    float press = Window(p, .42f, .51f, .56f, .65f) + Window(p, .66f, .73f, .78f, .87f);
                    lh = Vector3.Lerp(lh, new Vector3(.14f, .36f - .025f * press, .46f), reach);
                    rh.x = -.14f; rh.y -= .025f * press; lp = rp = new Vector3(0, -.8f, -.6f); ld = rd = new Vector3(0, -.6f, .8f);
                    lGrip = rGrip = 8;
                }
                else
                {
                    rh += new Vector3(-.04f, .27f, -.07f) * pull;
                    rd = Vector3.Lerp(rd, Vector3.forward, pull); rGrip = 15 + 50 * Window(p, .24f, .34f, .72f, .88f);
                }
            }

            // Fit the authored reaches to the female rig's shorter arms and thighs.
            if (femaleProfile)
            {
                if (value == ReferenceMaleAction.Idle || value == ReferenceMaleAction.Wave || value == ReferenceMaleAction.Water) hip.y -= .022f;
                if (value == ReferenceMaleAction.Crouch) { hip.y -= .018f; lh.y += .025f; rh.y += .025f; }
                if (value == ReferenceMaleAction.Hoe) hip.y -= .042f;
                if (value == ReferenceMaleAction.Harvest) rh.y += .018f;
            }
            GroundFoot(0, ref lf, lPitch); GroundFoot(1, ref rf, rPitch);
            B("Hips").localPosition += hip;
            B("Hips").localRotation = Quaternion.Euler(0, pelvisYaw, -roll * .5f);
            B("Spine").localRotation = Quaternion.Euler(lean * .45f + breath * .35f, twist * .4f, roll * .5f);
            B("Chest").localRotation = Quaternion.Euler(lean * .55f, twist * .6f, roll * .5f);
            B("Neck").localRotation = Quaternion.Euler(-lean * .25f + lookDown * .4f, -(twist + pelvisYaw) * .3f, 0);
            B("Head").localRotation = Quaternion.Euler(-lean * .20f + lookDown * .6f, -(twist + pelvisYaw) * .45f, 0);
            Solve("LeftThigh", "LeftShin", "LeftFoot", lf, new Vector3(.12f, 0, 1));
            Solve("RightThigh", "RightShin", "RightFoot", rf, new Vector3(-.12f, 0, 1));
            B("LeftFoot").rotation = transform.rotation * Quaternion.Euler(lPitch, 3, 0);
            B("RightFoot").rotation = transform.rotation * Quaternion.Euler(rPitch, -3, 0);
            Solve("LeftUpperArm", "LeftForearm", "LeftHand", lh, leftPole);
            Solve("RightUpperArm", "RightForearm", "RightHand", rh, rightPole);
            PoseHand("Left", ld, lp, lGrip); PoseHand("Right", rd, rp, rGrip);
            SetProp(wateringCan, value == ReferenceMaleAction.Water, toolPosition, toolRotation);
            SetProp(hoe, value == ReferenceMaleAction.Hoe, toolPosition, toolRotation);
            SetProp(crate, value == ReferenceMaleAction.Carry, toolPosition, toolRotation);
            AnimateDetails(value, p, seconds);
        }

        private void CorrectTransitionFeet()
        {
            for (int side = 0; side < 2; side++)
            {
                string prefix = side == 0 ? "Left" : "Right"; var foot = B(prefix + "Foot");
                float lowest = 100;
                foreach (var point in soles[side]) lowest = Mathf.Min(lowest, transform.InverseTransformPoint(foot.TransformPoint(point)).y);
                if (lowest >= .002f) continue;
                Vector3 target = transform.InverseTransformPoint(foot.position); target.y += .002f - lowest;
                var rotation = foot.rotation;
                Solve(prefix + "Thigh", prefix + "Shin", prefix + "Foot", target, Vector3.forward);
                foot.rotation = rotation;
            }
        }
        private void AnimateDetails(ReferenceMaleAction value, float p, float seconds)
        {
            bool gardening = value == ReferenceMaleAction.Harvest || value == ReferenceMaleAction.Plant;
            if (soilPatch != null) { soilPatch.SetActive(gardening); soilPatch.transform.localPosition = new Vector3(0, .018f, .49f); }
            if (workPlant != null)
            {
                workPlant.SetActive(gardening);
                Vector3 planted = new Vector3(0, .045f, .49f), at = planted;
                if (value == ReferenceMaleAction.Harvest)
                {
                    Vector3 palm = B("RightHand").TransformPoint((P("RightFinger3_1") - P("RightHand")) * .65f);
                    Vector3 held = transform.InverseTransformPoint(palm) - Vector3.up * .20f;
                    at = Vector3.Lerp(planted, held, Window(p, .25f, .36f, .71f, .91f));
                }
                workPlant.transform.localPosition = at;
            }
            if (waterDrops == null || wateringCan == null) return;
            float pour = Window(p, .10f, .33f, .64f, .90f);
            Vector3 source = transform.InverseTransformPoint(wateringCan.transform.TransformPoint(new Vector3(0, .04f, .29f)));
            Vector3 direction = transform.InverseTransformDirection(wateringCan.transform.forward);
            for (int i = 0; i < waterDrops.Length; i++)
            {
                float t = Mathf.Repeat(seconds * 1.4f + (float)i / waterDrops.Length, 1) * .8f;
                Vector3 at = source + direction * (.28f * t) + Vector3.down * (1.8f * t * t);
                at.x += Mathf.Sin(i * 2.3f) * t * .025f;
                waterDrops[i].gameObject.SetActive(value == ReferenceMaleAction.Water && pour > .45f && at.y > .015f);
                waterDrops[i].localPosition = at;
            }
        }

        private static void Gait(float phase, bool run, float stride, ref Vector3 foot, out float pitch)
        {
            float p = Mathf.Repeat(phase, 1), stance = run ? .40f : .62f;
            if (p < stance)
            {
                float t = p / stance; foot.z = Mathf.Lerp(stride, -stride, t);
                pitch = -8 * (1 - Ease(t, 0, .18f)) + (run ? 30 : 23) * Ease(t, .65f, 1);
            }
            else
            {
                float t = (p - stance) / (1 - stance);
                // Hermite endpoint velocity matches the stance instead of stopping abruptly.
                float tangent = -2 * stride * (1 - stance) / stance;
                foot.z = (2*t*t*t-3*t*t+1)*(-stride) + (t*t*t-2*t*t+t)*tangent + (-2*t*t*t+3*t*t)*stride + (t*t*t-t*t)*tangent;
                foot.y = Mathf.Pow(Mathf.Sin(t * Mathf.PI), 2) * (run ? .24f : .095f);
                pitch = Mathf.Lerp(run ? 30 : 23, -8, Ease(t, 0, .85f));
            }
        }
        private Quaternion HandRotation(string side, Vector3 direction, Vector3 palm)
        {
            Vector3 forward = (P(side + "Finger3_1") - P(side + "Hand")).normalized;
            Vector3 across = P(side + "Finger5_1") - P(side + "Finger2_1");
            Vector3 normal = Vector3.Cross(forward, across).normalized * (side == "Left" ? -1 : 1);
            return Quaternion.LookRotation(direction, palm) * Quaternion.Inverse(Quaternion.LookRotation(forward, normal));
        }
        private Vector3 WristAt(string side, Vector3 contact, Vector3 direction, Vector3 palm)
        {
            Vector3 palmOffset = (P(side + "Finger3_1") - P(side + "Hand")) * .65f;
            return contact - HandRotation(side, direction, palm) * palmOffset;
        }
        private void PoseHand(string side, Vector3 direction, Vector3 palm, float grip)
        {
            B(side + "Hand").rotation = transform.rotation * HandRotation(side, direction, palm);
            Vector3 forward = (P(side + "Finger3_1") - P(side + "Hand")).normalized;
            Vector3 normal = Vector3.Cross(forward, P(side + "Finger5_1") - P(side + "Finger2_1")).normalized * (side == "Left" ? -1 : 1);
            for (int finger = 1; finger <= 5; finger++) for (int joint = 1; joint <= 3; joint++)
            {
                string name = side + "Finger" + finger + "_" + joint; int index = indices[name];
                Vector3 axis = Vector3.Cross(bindTails[index] - bindPositions[index], normal).normalized;
                float angle = grip * (joint == 1 ? .65f : joint == 2 ? 1 : .65f);
                if (finger == 1) { axis = Vector3.Cross(bindTails[index] - bindPositions[index], P(side + "Finger5_1") - P(name)).normalized; angle *= .5f; }
                B(name).localRotation = Quaternion.AngleAxis(angle, axis);
            }
        }
        private static void SetProp(GameObject prop, bool active, Vector3 position, Quaternion rotation)
        {
            if (prop == null) return;
            prop.SetActive(active); prop.transform.localPosition = position; prop.transform.localRotation = rotation;
        }
    }
}
