using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HarvestIsle.Editor
{
    // Refit the original game garments, including their seams and accessories, to
    // the reference anatomy. The study assets remain immutable inputs to this bake.
    public static class GameHumanFitting
    {
        public static FarmerAvatar Create(bool female)
        {
            var legacy = female ? FarmerFemaleModelBuilder.Create(null, true) : FarmerModelBuilder.Create(null, true);
            foreach (var t in legacy.GetComponentsInChildren<Transform>(true))
                if (t.name == "Body rig" || t.name == "Waist" || t.name.Contains("shoulder") || t.name.Contains("hip") || t.name == "Knee" || t.name == "Elbow")
                    t.localRotation = Quaternion.identity;
            var root = new GameObject(female ? "Farmer Female - Mua Vang" : "Farmer - Mua Vang");
            var avatar = root.AddComponent<FarmerAvatar>();
            string id = female ? "ReferenceFemale" : "ReferenceMale";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HarvestIsle/Experiments/" + id + "/" + id + ".prefab");
            if (source == null) throw new InvalidOperationException("Build the reference human first: " + id);
            var model = Object.Instantiate(source, root.transform); model.name = "Fitted human";
            model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            var motion = model.GetComponent<ReferenceMaleMotion>(); motion.playing = false; motion.enabled = false;
            for (int i = 0; i < motion.bones.Length; i++)
            {
                int parent = Array.IndexOf(motion.bones, motion.bones[i].parent);
                motion.bones[i].localPosition = motion.bindPositions[i] - (parent < 0 ? Vector3.zero : motion.bindPositions[parent]);
                motion.bones[i].localRotation = Quaternion.identity;
            }
            var meshes = new List<Mesh>(); var materials = new List<Material>();
            var palette = new Dictionary<Material, Material>();
            foreach (var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                string name = skin.sharedMaterial.name;
                if (name == "Shirt" || name == "ShirtTrim" || name == "Trousers" || name == "TrouserTrim") { Object.DestroyImmediate(skin.gameObject); continue; }
                if (name == "Clay")
                {
                    var material = new Material(skin.sharedMaterial) { name = female ? "Female skin" : "Male skin" };
                    material.color = FarmWorld.Hex(female ? "E8B38C" : "E5AE86"); materials.Add(material); skin.sharedMaterial = material;
                }
            }
            if (motion.workPlant != null) Object.DestroyImmediate(motion.workPlant);
            if (motion.soilPatch != null) Object.DestroyImmediate(motion.soilPatch);
            motion.workPlant = null; motion.soilPatch = null;
            var bindposes = new Matrix4x4[motion.bones.Length];
            for (int i = 0; i < bindposes.Length; i++) bindposes[i] = motion.bones[i].worldToLocalMatrix * model.transform.localToWorldMatrix;
            var headSocket = new GameObject("Outfit head").transform;
            headSocket.SetParent(Bone(motion, "Head"), false);
            headSocket.position = model.transform.TransformPoint(new Vector3(0, 1.701f, .062f));
            headSocket.localScale = new Vector3(.56f, .57f, .60f);
            // Preserve the names used by village hats, and retain separate switchable ties.
            foreach (var filter in legacy.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null) continue;
                string color = ColorUtility.ToHtmlStringRGB(renderer.sharedMaterial.color);
                bool oldHair=Ancestor(filter.transform,"Full hair silhouette")!=null||Ancestor(filter.transform,"Long hair and low bun")!=null;
                if(oldHair&&(color=="3D3027"||color=="2E2721"||color=="8C4458"))continue;
                bool face = Ancestor(filter.transform, "Sculpted face") != null;
                if (color == (female ? "E8B38C" : "E5AE86") || face && color != "D9A441") continue;
                if (Ancestor(filter.transform, "Watering can") != null) continue;
                Transform oldHead = Ancestor(filter.transform, "Head");
                var mesh = Object.Instantiate(filter.sharedMesh); meshes.Add(mesh);
                var item = new GameObject(filter.transform.parent.name + " " + color);
                if (!palette.TryGetValue(renderer.sharedMaterial, out var mat))
                { mat = new Material(renderer.sharedMaterial); palette.Add(renderer.sharedMaterial, mat); materials.Add(mat); }
                if (oldHead != null)
                {
                    bool ties=Ancestor(filter.transform,"Soft hat ties")!=null||Ancestor(filter.transform,"Ribbon hat ties")!=null;
                    string group = Ancestor(filter.transform, "Non la") != null ? "Non la" : ties ? "Soft hat ties" : "Hair and jewelry";
                    Transform holder = headSocket.Find(group);
                    if (holder == null) { holder = new GameObject(group).transform; holder.SetParent(headSocket, false); }
                    item.transform.SetParent(holder, false);
                    var points = mesh.vertices;
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = oldHead.InverseTransformPoint(filter.transform.TransformPoint(points[i]));
                        if(ties){points[i].x*=.86f;points[i].y+=.045f;points[i].z+=.025f;}
                        if(face&&color=="D9A441"){points[i].x*=.82f;points[i].y+=.025f;points[i].z+=.005f;}
                        if(oldHair){points[i].x*=.88f;}
                    }
                    mesh.vertices = points; mesh.RecalculateNormals(); mesh.RecalculateBounds();
                    item.AddComponent<MeshFilter>().sharedMesh = mesh; item.AddComponent<MeshRenderer>().sharedMaterial = mat;
                    continue;
                }
                item.transform.SetParent(model.transform, false);
                var vertices = mesh.vertices; var weights = new BoneWeight[vertices.Length];
                Transform arm = Ancestor(filter.transform, "Left shoulder") ?? Ancestor(filter.transform, "Right shoulder");
                Transform leg = Ancestor(filter.transform, "Left hip") ?? Ancestor(filter.transform, "Right hip");
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 p = legacy.transform.InverseTransformPoint(filter.transform.TransformPoint(vertices[i]));
                    if (arm != null) vertices[i] = FitArm(p, arm.localPosition.x < 0 ? "Right" : "Left", female, motion, out weights[i]);
                    else if (leg != null) vertices[i] = FitLeg(p, leg.localPosition.x < 0 ? "Right" : "Left", female, motion, out weights[i]);
                    else { vertices[i] = FitTorso(p, female); weights[i] = Blend(motion, "Hips", "Chest", Mathf.InverseLerp(1.04f, 1.39f, vertices[i].y)); }
                }
                mesh.vertices = vertices; mesh.boneWeights = weights; mesh.bindposes = bindposes; mesh.RecalculateNormals(); mesh.RecalculateBounds();
                var fitted = item.AddComponent<SkinnedMeshRenderer>(); fitted.sharedMesh = mesh; fitted.sharedMaterial = mat;
                fitted.bones = motion.bones; fitted.rootBone = motion.bones[0]; fitted.quality = SkinQuality.Bone4;
                fitted.localBounds = new Bounds(new Vector3(0, 1, .2f), Vector3.one * 3); fitted.updateWhenOffscreen = true;
            }
            FittedHumanHair.Build(motion,female,meshes,materials);
            model.transform.localScale = Vector3.one * .85f;
            avatar.ConfigureHuman(motion); avatar.OwnResources(meshes.ToArray(), materials.ToArray());
            var measured = new Mesh();
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skin.rootBone=model.transform;
                skin.BakeMesh(measured,true);measured.RecalculateBounds();skin.localBounds=measured.bounds;
            }
            Object.DestroyImmediate(measured);
            Object.DestroyImmediate(legacy.gameObject);
            return avatar;
        }
        private static Transform Ancestor(Transform t, string name) { for (; t != null; t = t.parent) if (t.name == name) return t; return null; }
        private static Transform Bone(ReferenceMaleMotion m, string name) { return m.bones[Array.FindIndex(m.bones, b => b.name == name)]; }
        private static Vector3 Position(ReferenceMaleMotion m, string name) { return m.bindPositions[Array.FindIndex(m.bones, b => b.name == name)]; }
        private static BoneWeight Blend(ReferenceMaleMotion m, string a, string b, float t)
        { return new BoneWeight { boneIndex0 = Array.FindIndex(m.bones, x => x.name == a), boneIndex1 = Array.FindIndex(m.bones, x => x.name == b), weight0 = 1-t, weight1 = t }; }
        private static Vector3 FitArm(Vector3 p, string side, bool female, ReferenceMaleMotion m, out BoneWeight w)
        {
            float sign = side == "Left" ? 1 : -1, shoulder = female ? .192f : .207f;
            float elbowY = female ? .766f : .750f;
            var a = Position(m, side + "UpperArm"); var b = Position(m, side + "Forearm"); var c = Position(m, side + "Hand");
            Vector3 axis = (p.y >= elbowY ? b-a : c-b).normalized;
            Vector3 center = p.y >= elbowY ? Vector3.LerpUnclamped(a,b,(.996f-p.y)/(.996f-elbowY)) : Vector3.LerpUnclamped(b,c,(elbowY-p.y)/(elbowY-.579f));
            Vector3 across = Vector3.Cross(axis, Vector3.forward).normalized;
            if (across.x < 0) across = -across;
            float z = Mathf.Lerp(0,.037f,Mathf.InverseLerp(.996f,.579f,p.y));
            w = Blend(m,side+"UpperArm",side+"Forearm",Mathf.SmoothStep(0,1,Mathf.InverseLerp(elbowY+.045f,elbowY-.045f,p.y)));
            float radius = female && p.y < elbowY ? 1.30f : 1.02f;
            return center + across * (p.x-sign*shoulder) * radius + Vector3.forward*(p.z-z)*radius;
        }
        private static Vector3 FitLeg(Vector3 p, string side, bool female, ReferenceMaleMotion m, out BoneWeight w)
        {
            float sign = side == "Left" ? 1 : -1, hip = female ? .097f : .102f;
            var a = Position(m,side+"Thigh"); var b = Position(m,side+"Shin"); var c = Position(m,side+"Foot");
            float t = Mathf.InverseLerp(.59f,.31f,p.y), u = Mathf.InverseLerp(.31f,.075f,p.y);
            Vector3 center; float radial;
            if (p.y >= .31f) { center = Vector3.LerpUnclamped(a,b,(.59f-p.y)/.28f); radial = Mathf.Lerp(1.15f,.9f,t); w = Blend(m,side+"Thigh",side+"Shin",Mathf.SmoothStep(0,.5f,Mathf.InverseLerp(.4f,.31f,p.y))); }
            else if (p.y > .155f) { center = Vector3.Lerp(b,c,u); if(female) center.y=Mathf.LerpUnclamped(b.y,.13f,(.31f-p.y)/.134f); radial = .85f; w = Blend(m,side+"Shin",side+"Foot",0); }
            else { center = c; center.y = p.y; radial = .83f; w = Blend(m,side+"Foot",side+"Foot",0); }
            return center + new Vector3((p.x-sign*hip)*radial,0,p.z*(female?1.12f:1.27f));
        }
        private static Vector3 FitTorso(Vector3 p, bool female)
        {
            // Old and new body landmarks; garments retain their cut, pockets, buttons and weave.
            float[] oldY = { .51f,.59f,.775f,.88f,.98f,1.047f,1.10f };
            float[] newY = { .94f,1.015f,1.24f,1.38f,1.48f,1.565f,1.60f };
            float[] wide = { 1.02f,1.02f,.98f,1.18f,1.12f,1.04f,1.04f };
            float[] deep = { 1.0f,1.0f,1.10f,female?1.40f:1.22f,1.22f,1.04f,1.04f };
            int row = 1; while (row < oldY.Length-1 && p.y > oldY[row]) row++;
            float t = (p.y-oldY[row-1])/(oldY[row]-oldY[row-1]);
            return new Vector3(p.x*Mathf.LerpUnclamped(wide[row-1],wide[row],t), Mathf.LerpUnclamped(newY[row-1],newY[row],t), p.z*Mathf.LerpUnclamped(deep[row-1],deep[row],t)+.020f*Mathf.InverseLerp(.88f,1.04f,p.y));
        }
    }
}
