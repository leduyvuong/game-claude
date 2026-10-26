using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Mesh-native Vietnamese female farmer: ao ba ba blouse, low hair bun, non la and sandals.
    /// Shares the FarmerAvatar rig layout with the male farmer so every pose works unchanged.
    /// Forward is +Z, ground is Y=0.</summary>
    public static class FarmerFemaleModelBuilder
    {
        public const int Version = 3;
        private static readonly Color Skin = FarmWorld.Hex("E8B38C");
        private static readonly Color Blush = FarmWorld.Hex("D98D74");
        private static readonly Color Ink = FarmWorld.Hex("2B2520");
        private static readonly Color Hair = FarmWorld.Hex("2E2721");
        private static readonly Color Blouse = FarmWorld.Hex("CB7F8B");
        private static readonly Color Trim = FarmWorld.Hex("8C4458");
        private static readonly Color Pants = FarmWorld.Hex("33373F");
        private static readonly Color PantsHem = FarmWorld.Hex("272A31");
        private static readonly Color Sandal = FarmWorld.Hex("7A523B");
        private static readonly Color Straw = FarmWorld.Hex("DAB778");
        private static readonly Color StrawDark = FarmWorld.Hex("BD995F");
        private static readonly Color Cream = FarmWorld.Hex("F0E3C8");
        private static readonly Color Lip = FarmWorld.Hex("A6484F");
        private static readonly Color Gold = FarmWorld.Hex("D9A441");
        private static readonly Color Flower = FarmWorld.Hex("D96A80");

        // Softer female head: narrower jaw and cheekbones than the male farmer, same height range
        // so the shared hair cap rows and the non la keep their fit.
        private static readonly Vector4[] HeadShape = {
            new Vector4(0.032f, -0.252f, 0.042f, 0.014f),
            new Vector4(0.095f, -0.230f, 0.096f, 0.014f),
            new Vector4(0.158f, -0.182f, 0.146f, 0.007f),
            new Vector4(0.202f, -0.095f, 0.178f, 0),
            new Vector4(0.214f, 0.012f, 0.185f, 0),
            new Vector4(0.210f, 0.102f, 0.179f, -0.003f),
            new Vector4(0.180f, 0.190f, 0.153f, -0.008f),
            new Vector4(0.122f, 0.240f, 0.108f, -0.008f),
            new Vector4(0.024f, 0.264f, 0.024f, -0.008f)
        };

        public static FarmerAvatar Create(Transform parent = null, bool outfitSource = false)
        {
            if (!outfitSource)
            {
                var prefab = Resources.Load<GameObject>("Characters/FarmerFemale");
                if (prefab != null) return Object.Instantiate(prefab,parent,false).GetComponent<FarmerAvatar>();
            }
            var root = new GameObject("Farmer Female - Mua Vang");
            root.transform.SetParent(parent, false);
            var avatar = root.AddComponent<FarmerAvatar>();
            var kit = new FarmerMeshKit("FarmerFemale");
            Transform body = Joint("Body rig", root.transform, Vector3.zero);
            Transform upperBody = Joint("Waist", body, new Vector3(0, 0.59f, 0));
            Transform head = Joint("Head", upperBody, new Vector3(0, 0.695f, 0));
            head.localScale = new Vector3(1.12f, 1.04f, 1.08f);
            Transform leftArm = Joint("Left shoulder", upperBody, new Vector3(-0.192f, 0.406f, 0));
            Transform rightArm = Joint("Right shoulder", upperBody, new Vector3(0.192f, 0.406f, 0));
            Transform leftLeg = Joint("Left hip", body, new Vector3(-0.097f, 0.59f, 0));
            Transform rightLeg = Joint("Right hip", body, new Vector3(0.097f, 0.59f, 0));

            BuildBlouse(kit, upperBody);
            BuildFace(kit, head);
            BuildHair(kit, head);
            BuildHat(kit, head);

            BuildArm(kit, leftArm, "Left sleeve", -1);
            Transform hand = BuildArm(kit, rightArm, "Right sleeve", 1);
            BuildLeg(kit, leftLeg, "Left trousers");
            BuildLeg(kit, rightLeg, "Right trousers");
            GameObject can = FarmerModelBuilder.BuildWateringCan(kit, hand);
            kit.Finish();
            avatar.ConfigureRig(body, upperBody, head, leftArm, rightArm, leftLeg, rightLeg, can);
            avatar.OwnResources(kit.Meshes.ToArray(), kit.Materials.ToArray());
            return avatar;
        }

        private static Vector4 HeadSection(float y)
        {
            for (int i = 1; i < HeadShape.Length; i++)
                if (y <= HeadShape[i].y) return Vector4.Lerp(HeadShape[i - 1], HeadShape[i],
                    Mathf.InverseLerp(HeadShape[i - 1].y, HeadShape[i].y, y));
            return HeadShape[HeadShape.Length - 1];
        }

        private static Vector3 OnFace(float x, float y, float offset)
        {
            Vector4 ring = HeadSection(y);
            float front = Mathf.Pow(Mathf.Max(0, 1 - Mathf.Pow(Mathf.Abs(x) / ring.x, 2.5f)), 0.4f);
            return new Vector3(x, y, ring.w + ring.z * front + offset);
        }

        private static void BuildBlouse(FarmerMeshKit kit, Transform upperBody)
        {
            var blouse = kit.Group("Ao ba ba blouse", upperBody, new Vector3(0, -0.59f, 0));
            // Fitted waist with a flared hem falling over the hips; the chest rings sit
            // slightly forward for a soft feminine silhouette.
            blouse.Loft(new[] {
                new Vector4(0.198f, 0.520f, 0.146f, 0),
                new Vector4(0.199f, 0.542f, 0.147f, 0),
                new Vector4(0.172f, 0.655f, 0.120f, 0.004f),
                new Vector4(0.150f, 0.775f, 0.102f, 0.004f),
                new Vector4(0.172f, 0.880f, 0.112f, 0.014f),
                new Vector4(0.182f, 0.940f, 0.110f, 0.012f),
                new Vector4(0.158f, 1.005f, 0.090f, 0.004f),
                new Vector4(0.064f, 1.040f, 0.058f, 0)
            }, Blouse, 24);
            blouse.Loft(new[] {
                new Vector4(0.201f, 0.512f, 0.149f, 0), new Vector4(0.202f, 0.528f, 0.150f, 0)
            }, Trim, 24);
            blouse.Profile(new[] {
                new Vector3(0.056f, 1.008f, 0.054f), new Vector3(0.053f, 1.101f, 0.051f)
            }, Skin, 16);
            // Stand collar, V-shaped placket and the button line of an ao ba ba.
            var collar = new Vector3[17];
            for (int i = 0; i < collar.Length; i++)
            {
                float a = i * Mathf.PI * 2 / 16;
                collar[i] = new Vector3(Mathf.Cos(a) * 0.072f, 1.032f, Mathf.Sin(a) * 0.066f + 0.008f);
            }
            blouse.Tube(collar, 0.0075f, Trim, 8);
            for (int side = -1; side <= 1; side += 2)
                blouse.Tube(new[] { new Vector3(side * 0.026f, 1.030f, 0.075f),
                    new Vector3(0, 0.960f, 0.115f) }, 0.0035f, Trim, 6);
            blouse.Tube(new[] { new Vector3(0, 0.960f, 0.116f), new Vector3(0, 0.940f, 0.124f),
                new Vector3(0, 0.880f, 0.128f), new Vector3(0, 0.775f, 0.108f),
                new Vector3(0, 0.655f, 0.126f), new Vector3(0, 0.545f, 0.149f) }, 0.003f, Trim, 6);
            float[] buttonHeights = { 0.925f, 0.855f, 0.785f, 0.715f, 0.645f };
            float[] buttonDepths = { 0.124f, 0.123f, 0.108f, 0.110f, 0.123f };
            for (int i = 0; i < buttonHeights.Length; i++)
                blouse.Ellipsoid(new Vector3(0, buttonHeights[i], buttonDepths[i]),
                    new Vector3(0.011f, 0.011f, 0.008f), Cream, 8, 5);
            blouse.Finish();
        }

        private static void BuildFace(FarmerMeshKit kit, Transform head)
        {
            var face = kit.Group("Sculpted face", head);
            face.Loft(HeadShape, Skin, 40, 0.8f);
            for (int side = -1; side <= 1; side += 2)
            {
                face.Ellipsoid(new Vector3(side * 0.205f, -0.038f, -0.004f), new Vector3(0.055f, 0.088f, 0.050f), Skin, 12, 8);
                float x = side * 0.085f;
                Quaternion turn = Quaternion.Euler(0, side * 14, side * -3);
                Vector3 eye = OnFace(x, 0.020f, 0.003f);
                face.Ellipsoid(eye, new Vector3(0.060f, 0.070f, 0.020f), Cream, 16, 10, turn);
                face.Ellipsoid(eye + new Vector3(-side * 0.004f, 0, 0.012f), new Vector3(0.036f, 0.054f, 0.015f), Ink, 16, 10, turn);
                face.Ellipsoid(eye + new Vector3(-0.008f, 0.014f, 0.021f), new Vector3(0.010f, 0.013f, 0.007f), Color.white, 8, 6);
                // Lash line with a small outward flick, and a thinner arched brow.
                face.Tube(new[] { OnFace(x - side * 0.034f, 0.057f, 0.0065f), OnFace(x, 0.067f, 0.0075f),
                    OnFace(x + side * 0.030f, 0.060f, 0.0065f), OnFace(x + side * 0.044f, 0.068f, 0.005f) }, 0.0035f, Ink, 6);
                face.Tube(new[] { OnFace(x - side * 0.028f, 0.106f, 0.005f), OnFace(x, 0.116f, 0.006f),
                    OnFace(x + side * 0.024f, 0.108f, 0.005f) }, 0.0042f, Hair, 6);
                face.Ellipsoid(OnFace(side * 0.128f, -0.058f, 0.001f), new Vector3(0.048f, 0.024f, 0.007f), Blush, 12, 6,
                    Quaternion.Euler(0, side * 24, 0));
                // Small golden hoop earring hanging from the ear lobe.
                var hoop = new Vector3[13];
                for (int i = 0; i < hoop.Length; i++)
                {
                    float a = i * Mathf.PI * 2 / 12;
                    hoop[i] = new Vector3(side * 0.210f, -0.088f + Mathf.Cos(a) * 0.016f - 0.016f, 0.004f + Mathf.Sin(a) * 0.014f);
                }
                face.Tube(hoop, 0.0035f, Gold, 6);
            }
            face.Ellipsoid(OnFace(0, -0.030f, 0.005f), new Vector3(0.042f, 0.052f, 0.040f), Skin, 16, 10);
            var smile = new Vector3[11];
            for (int i = 0; i < smile.Length; i++)
            {
                float x = -0.036f + i * 0.072f / (smile.Length - 1);
                smile[i] = OnFace(x, -0.122f + x * x * 4.6f, 0.004f);
            }
            face.Tube(smile, 0.0036f, Lip, 6);
            face.Ellipsoid(OnFace(0, -0.133f, 0.0035f), new Vector3(0.026f, 0.010f, 0.008f), Lip, 10, 5);
            face.Finish();
        }

        private static void BuildHair(FarmerMeshKit kit, Transform head)
        {
            var hair = kit.Group("Long hair and low bun", head);
            hair.HairCap(Hair, 40, 10, HeadSection);
            // Center-parted bangs sweeping to each side, and soft strands framing the cheeks.
            hair.Ellipsoid(new Vector3(-0.050f, 0.172f, 0.142f), new Vector3(0.205f, 0.070f, 0.082f), Hair, 20, 10, Quaternion.Euler(0, -10, -18));
            hair.Ellipsoid(new Vector3(0.095f, 0.188f, 0.124f), new Vector3(0.120f, 0.062f, 0.072f), Hair, 18, 10, Quaternion.Euler(0, 15, 22));
            for (int side = -1; side <= 1; side += 2)
                hair.Ellipsoid(new Vector3(side * 0.200f, -0.020f, 0.050f), new Vector3(0.042f, 0.155f, 0.058f), Hair, 12, 8,
                    Quaternion.Euler(0, 0, side * 7));
            // Low bun at the nape with a wrap band and one loose strand.
            hair.Ellipsoid(new Vector3(0, -0.050f, -0.192f), new Vector3(0.150f, 0.132f, 0.118f), Hair, 16, 10);
            var band = new Vector3[17];
            for (int i = 0; i < band.Length; i++)
            {
                float a = i * Mathf.PI * 2 / 16;
                band[i] = new Vector3(Mathf.Cos(a) * 0.058f, -0.050f + Mathf.Sin(a) * 0.058f, -0.192f);
            }
            hair.Tube(band, 0.008f, Trim, 6);
            hair.Tube(new[] { new Vector3(0.040f, -0.115f, -0.200f), new Vector3(0.055f, -0.180f, -0.185f),
                new Vector3(0.045f, -0.235f, -0.160f) }, 0.0075f, Hair, 6);
            // A small blossom tucked above the right ear.
            Vector3 center = new Vector3(0.182f, 0.070f, 0.072f);
            for (int petal = 0; petal < 5; petal++)
            {
                float a = petal * Mathf.PI * 2 / 5;
                hair.Ellipsoid(center + new Vector3(0.006f, Mathf.Cos(a) * 0.019f, Mathf.Sin(a) * 0.019f),
                    new Vector3(0.009f, 0.017f, 0.017f), Flower, 8, 6);
            }
            hair.Ellipsoid(center + new Vector3(0.010f, 0, 0), new Vector3(0.012f, 0.011f, 0.011f), Gold, 8, 5);
            hair.Finish();
        }

        private static void BuildHat(FarmerMeshKit kit, Transform head)
        {
            Transform hat = Joint("Non la", head, new Vector3(0, 0.210f, -0.028f));
            hat.localRotation = Quaternion.Euler(-13, 0, -3);
            var mesh = kit.Group("Woven palm leaf hat", hat);
            const float radius = 0.405f;
            const float height = 0.238f;
            mesh.ConeHat(radius, height, Straw, StrawDark, 64);
            mesh.Finish();
            mesh = kit.Group("Bamboo weave details", hat);
            mesh.CastShadows = false;
            for (int ringIndex = 0; ringIndex < 3; ringIndex++)
            {
                float r = ringIndex == 0 ? radius : ringIndex == 1 ? radius * 0.72f : radius * 0.40f;
                var ring = new Vector3[65];
                for (int i = 0; i < ring.Length; i++)
                {
                    float a = i * Mathf.PI * 2 / 64;
                    ring[i] = new Vector3(Mathf.Cos(a) * r, height * (1 - r / radius) + 0.0015f, Mathf.Sin(a) * r);
                }
                mesh.Tube(ring, ringIndex == 0 ? 0.006f : 0.002f, StrawDark, 6);
            }
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2 / 12;
                mesh.Tube(new[] { new Vector3(Mathf.Cos(a) * 0.032f, 0.221f, Mathf.Sin(a) * 0.032f),
                    new Vector3(Mathf.Cos(a) * 0.399f, 0.007f, Mathf.Sin(a) * 0.399f) }, 0.0017f, StrawDark, 5);
            }
            mesh.Finish();
            // A rose ribbon chin tie instead of the male farmer's plain strap.
            var strap = kit.Group("Ribbon hat ties", head);
            for (int side = -1; side <= 1; side += 2)
                strap.Tube(new[] { new Vector3(side * 0.222f, 0.191f, -0.011f),
                    new Vector3(side * 0.236f, -0.067f, -0.018f), new Vector3(side * 0.150f, -0.218f, 0.016f),
                    new Vector3(side * 0.018f, -0.254f, 0.078f) }, 0.005f, Trim, 6);
            strap.Finish();
        }

        private static Transform BuildArm(FarmerMeshKit kit, Transform pivot, string name, int side)
        {
            var arm = kit.Group(name, pivot);
            // Slim full-length ao ba ba sleeve ending in a narrow cuff at the wrist.
            arm.Loft(new[] {
                new Vector4(0.051f, -0.235f, 0.055f, 0.015f),
                new Vector4(0.056f, -0.180f, 0.060f, 0.008f),
                new Vector4(0.063f, -0.060f, 0.067f, 0),
                new Vector4(0.056f, 0.015f, 0.060f, 0),
                new Vector4(0.020f, 0.049f, 0.026f, 0)
            }, Blouse, 20);
            arm.Finish();
            Transform elbow=Joint("Elbow",pivot,new Vector3(0,-.23f,.015f));
            arm=kit.Group("Forearm sleeve",elbow,new Vector3(0,.23f,-.015f));
            arm.Loft(new[]{new Vector4(.040f,-.358f,.044f,.03f),new Vector4(.046f,-.30f,.05f,.022f),new Vector4(.051f,-.225f,.055f,.015f)},Blouse,20);
            arm.Loft(new[] { new Vector4(0.043f, -0.370f, 0.047f, 0.031f),
                new Vector4(0.046f, -0.350f, 0.050f, 0.029f) }, Trim, 20);
            arm.Loft(new[] { new Vector4(0.030f, -0.405f, 0.032f, 0.037f),
                new Vector4(0.034f, -0.372f, 0.036f, 0.032f), new Vector4(0.040f, -0.345f, 0.043f, 0.028f) }, Skin, 20);
            arm.Finish();
            Transform hand = Joint("Hand", elbow, new Vector3(0, -0.187f, 0.024f));
            var fist = kit.Group("Shaped hand", hand);
            fist.Ellipsoid(Vector3.zero, new Vector3(0.074f, 0.102f, 0.064f), Skin, 16, 10);
            fist.Ellipsoid(new Vector3(-side * 0.026f, 0.010f, 0.028f), new Vector3(0.036f, 0.058f, 0.034f), Skin, 12, 8,
                Quaternion.Euler(-20, 0, side * -20));
            fist.Finish();
            return hand;
        }

        private static void BuildLeg(FarmerMeshKit kit, Transform pivot, string name)
        {
            var thigh = kit.Group(name + " upper", pivot);
            thigh.Loft(new[] { new Vector4(.072f,-.28f,.078f,.008f),new Vector4(.068f,-.15f,.080f,0),new Vector4(.070f,.008f,.087f,0) },Pants,24);
            thigh.Ellipsoid(new Vector3(0,-.28f,.008f),new Vector3(.148f,.13f,.16f),Pants,20,12);
            thigh.Finish();
            Transform knee=Joint("Knee",pivot,new Vector3(0,-.28f,0));
            var leg = kit.Group(name + " lower", knee, new Vector3(0,.28f,0));
            // Loose dark trousers cropped above the ankle, bare feet in flat sandals.
            leg.Loft(new[] {
                new Vector4(0.058f, -0.408f, 0.064f, 0.004f), new Vector4(0.064f, -0.355f, 0.070f, 0.004f),
                new Vector4(0.068f, -0.263f, 0.075f, 0.012f)
            }, Pants, 20);
            leg.Profile(new[] { new Vector3(0.063f, -0.414f, 0.069f), new Vector3(0.067f, -0.405f, 0.073f),
                new Vector3(0.067f, -0.382f, 0.073f) }, PantsHem, 20);
            leg.Loft(new[] { new Vector4(0.040f, -0.520f, 0.044f, 0.010f),
                new Vector4(0.045f, -0.455f, 0.049f, 0.006f), new Vector4(0.049f, -0.400f, 0.053f, 0.004f) }, Skin, 16);
            leg.Loft(new[] { new Vector4(0.052f, -0.562f, 0.096f, 0.040f),
                new Vector4(0.048f, -0.532f, 0.078f, 0.026f), new Vector4(0.038f, -0.505f, 0.050f, 0.010f) }, Skin, 20, 0.75f);
            leg.Loft(new[] { new Vector4(0.072f, -0.590f, 0.115f, 0.042f),
                new Vector4(0.079f, -0.578f, 0.123f, 0.042f), new Vector4(0.076f, -0.562f, 0.118f, 0.042f) }, Sandal, 24, 0.75f);
            leg.Tube(new[] { new Vector3(-0.060f, -0.536f, 0.064f), new Vector3(0, -0.516f, 0.086f),
                new Vector3(0.060f, -0.536f, 0.064f) }, 0.010f, Trim, 6);
            leg.Tube(new[] { new Vector3(0, -0.518f, 0.084f), new Vector3(0, -0.552f, 0.128f) }, 0.007f, Trim, 6);
            leg.Finish();
        }

        private static Transform Joint(string name, Transform parent, Vector3 position)
        {
            return FarmerMeshKit.Joint(name, parent, position);
        }
    }
}
