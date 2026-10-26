using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Original, mesh-native Vietnamese farmer. Forward is +Z, ground is Y=0.</summary>
    public static class FarmerModelBuilder
    {
        public const int Version = 5;
        private static readonly Color Skin = FarmWorld.Hex("E5AE86");
        private static readonly Color Blush = FarmWorld.Hex("C88469");
        private static readonly Color Ink = FarmWorld.Hex("302923");
        private static readonly Color Hair = FarmWorld.Hex("3D3027");
        private static readonly Color Shirt = FarmWorld.Hex("75A28A");
        private static readonly Color ShirtTrim = FarmWorld.Hex("486B5C");
        private static readonly Color Pants = FarmWorld.Hex("405B70");
        private static readonly Color Boots = FarmWorld.Hex("785840");
        private static readonly Color Straw = FarmWorld.Hex("DAB778");
        private static readonly Color StrawDark = FarmWorld.Hex("BD995F");
        private static readonly Color Cream = FarmWorld.Hex("EBDABB");

        // Sculpted cross-sections: width, height, depth and forward offset.
        // The flatter front lets facial details sit on the skin, including in side view.
        private static readonly Vector4[] HeadShape = {
            new Vector4(0.035f, -0.248f, 0.045f, 0.013f),
            new Vector4(0.105f, -0.225f, 0.105f, 0.013f),
            new Vector4(0.172f, -0.175f, 0.158f, 0.006f),
            new Vector4(0.213f, -0.090f, 0.187f, 0),
            new Vector4(0.224f, 0.015f, 0.193f, 0),
            new Vector4(0.219f, 0.105f, 0.186f, -0.003f),
            new Vector4(0.187f, 0.191f, 0.159f, -0.008f),
            new Vector4(0.126f, 0.241f, 0.112f, -0.008f),
            new Vector4(0.025f, 0.265f, 0.025f, -0.008f)
        };

        public static FarmerAvatar Create(Transform parent = null, bool outfitSource = false)
        {
            if (!outfitSource)
            {
                var prefab = Resources.Load<GameObject>("Characters/Farmer");
                if (prefab != null) return Object.Instantiate(prefab,parent,false).GetComponent<FarmerAvatar>();
            }
            var root = new GameObject("Farmer - Mua Vang");
            root.transform.SetParent(parent, false);
            var avatar = root.AddComponent<FarmerAvatar>();
            var kit = new FarmerMeshKit();
            Transform body = Joint("Body rig", root.transform, Vector3.zero);
            Transform upperBody = Joint("Waist", body, new Vector3(0, 0.59f, 0));
            Transform head = Joint("Head", upperBody, new Vector3(0, 0.695f, 0));
            head.localScale = new Vector3(1.12f, 1.04f, 1.08f);
            Transform leftArm = Joint("Left shoulder", upperBody, new Vector3(-0.207f, 0.406f, 0));
            Transform rightArm = Joint("Right shoulder", upperBody, new Vector3(0.207f, 0.406f, 0));
            Transform leftLeg = Joint("Left hip", body, new Vector3(-0.102f, 0.59f, 0));
            Transform rightLeg = Joint("Right hip", body, new Vector3(0.102f, 0.59f, 0));

            var shirt = kit.Group("Tailored work shirt", upperBody, new Vector3(0, -0.59f, 0));
            shirt.Profile(new[] {
                new Vector3(0.208f, 0.565f, 0.142f), new Vector3(0.209f, 0.588f, 0.143f),
                new Vector3(0.194f, 0.650f, 0.132f), new Vector3(0.172f, 0.785f, 0.113f),
                new Vector3(0.202f, 0.925f, 0.126f), new Vector3(0.216f, 0.982f, 0.118f),
                new Vector3(0.174f, 1.023f, 0.096f), new Vector3(0.072f, 1.047f, 0.065f)
            }, Shirt, 24);
            shirt.Profile(new[] {
                new Vector3(0.068f, 1.014f, 0.063f), new Vector3(0.064f, 1.101f, 0.061f)
            }, Skin, 16);
            // Understated seams and two stitched pockets on the curved front.
            shirt.Tube(new[] { new Vector3(0, 0.589f, 0.147f), new Vector3(0, 0.650f, 0.136f),
                new Vector3(0, 0.785f, 0.117f), new Vector3(0, 0.925f, 0.130f),
                new Vector3(0, 0.982f, 0.122f), new Vector3(0, 1.014f, 0.105f) }, 0.003f, ShirtTrim, 6);
            for (int i = 0; i < 4; i++)
            {
                float y = 0.935f - i * 0.085f;
                float z = i == 0 ? 0.130f : i == 1 ? 0.124f : i == 2 ? 0.121f : 0.133f;
                shirt.Ellipsoid(new Vector3(0, y, z), new Vector3(0.013f, 0.013f, 0.009f), Cream, 8, 5);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                shirt.Triangle(new Vector3(side * 0.060f, 1.040f, 0.067f),
                    new Vector3(side * 0.107f, 1.004f, 0.110f), new Vector3(side * 0.039f, 0.963f, 0.136f), ShirtTrim);
                var pocket = kit.Group("Stitched pocket", upperBody, new Vector3(side * 0.107f, 0.095f, 0.112f));
                pocket.Profile(new[] { new Vector3(0.025f, -0.043f, 0.003f),
                    new Vector3(0.044f, -0.025f, 0.006f), new Vector3(0.044f, 0.046f, 0.006f) }, ShirtTrim, 8);
                pocket.Box(new Vector3(0, 0.040f, 0.008f), new Vector3(0.082f, 0.009f, 0.005f), Shirt);
            }
            shirt.Finish();
            BuildScarf(kit, upperBody);

            var face = kit.Group("Sculpted face", head);
            face.Loft(HeadShape, Skin, 40, 0.8f);
            for (int side = -1; side <= 1; side += 2)
            {
                face.Ellipsoid(new Vector3(side * 0.218f, -0.036f, -0.001f), new Vector3(0.063f, 0.100f, 0.060f), Skin, 12, 8);
                face.Ellipsoid(new Vector3(side * 0.237f, -0.040f, 0.021f), new Vector3(0.020f, 0.049f, 0.013f), Blush, 10, 6);
                float x = side * 0.087f;
                Quaternion turn = Quaternion.Euler(0, side * 14, side * -3);
                Vector3 eye = OnFace(x, 0.023f, 0.003f);
                face.Ellipsoid(eye, new Vector3(0.065f, 0.070f, 0.020f), Cream, 16, 10, turn);
                face.Ellipsoid(eye + new Vector3(-side * 0.004f, 0, 0.012f), new Vector3(0.036f, 0.053f, 0.015f), Ink, 16, 10, turn);
                face.Ellipsoid(eye + new Vector3(-0.008f, 0.014f, 0.021f), new Vector3(0.010f, 0.013f, 0.007f), Color.white, 8, 6);
                face.Tube(new[] { OnFace(x - 0.030f, 0.098f, 0.005f), OnFace(x, 0.108f, 0.006f),
                    OnFace(x + 0.028f, 0.102f, 0.005f) }, 0.006f, Hair, 6);
                face.Ellipsoid(OnFace(side * 0.145f, -0.067f, 0.001f), new Vector3(0.048f, 0.022f, 0.006f), Blush, 12, 6,
                    Quaternion.Euler(0, side * 24, 0));
            }
            face.Ellipsoid(OnFace(0, -0.032f, 0.006f), new Vector3(0.050f, 0.062f, 0.047f), Skin, 16, 10);
            var smile = new Vector3[13];
            for (int i = 0; i < smile.Length; i++)
            {
                float x = -0.047f + i * 0.094f / (smile.Length - 1);
                smile[i] = OnFace(x, -0.126f + x * x * 5, 0.004f);
            }
            face.Tube(smile, 0.0038f, FarmWorld.Hex("8C5040"), 6);
            face.Finish();

            var hair = kit.Group("Full hair silhouette", head);
            hair.HairCap(Hair, 40, 10, HeadSection);
            // Swept locks connect to the cap instead of sitting as separate balls on the forehead.
            hair.Ellipsoid(new Vector3(-0.056f, 0.168f, 0.148f), new Vector3(0.220f, 0.076f, 0.087f), Hair, 20, 10, Quaternion.Euler(0, -12, -24));
            hair.Ellipsoid(new Vector3(0.098f, 0.186f, 0.129f), new Vector3(0.128f, 0.068f, 0.078f), Hair, 18, 10, Quaternion.Euler(0, 17, 25));
            hair.Finish();
            BuildHat(kit, head);

            BuildArm(kit, leftArm, "Left sleeve", -1);
            Transform hand = BuildArm(kit, rightArm, "Right sleeve", 1);
            BuildLeg(kit, leftLeg, "Left trousers");
            BuildLeg(kit, rightLeg, "Right trousers");
            GameObject can = BuildWateringCan(kit, hand);
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

        private static void BuildScarf(FarmerMeshKit kit, Transform upperBody)
        {
            var scarf = kit.Group("Woven neck scarf", upperBody);
            var loop = new Vector3[33];
            for (int i = 0; i < loop.Length; i++)
            {
                float a = i * Mathf.PI * 2 / 32;
                loop[i] = new Vector3(Mathf.Cos(a) * 0.084f, 0.449f - Mathf.Sin(a) * 0.025f, Mathf.Sin(a) * 0.076f);
            }
            scarf.Tube(loop, 0.018f, Cream, 8);
            Transform tail = Joint("Scarf tail", upperBody, new Vector3(0.073f, 0.391f, 0.118f));
            tail.localRotation = Quaternion.Euler(-15, 12, -12);
            var cloth = kit.Group("Check weave", tail);
            cloth.Box(Vector3.zero, new Vector3(0.062f, 0.126f, 0.012f), Cream);
            for (int i = 0; i < 5; i++)
                cloth.Box(new Vector3(0, -0.050f + i * 0.023f, 0.007f), new Vector3(0.063f, 0.004f, 0.002f), ShirtTrim);
            for (int i = 0; i < 3; i++)
                cloth.Box(new Vector3(-0.020f + i * 0.020f, 0, 0.008f), new Vector3(0.004f, 0.126f, 0.002f), ShirtTrim);
        }

        private static void BuildHat(FarmerMeshKit kit, Transform head)
        {
            Transform hat = Joint("Non la", head, new Vector3(0, 0.210f, -0.028f));
            hat.localRotation = Quaternion.Euler(-13, 0, -3);
            var mesh = kit.Group("Woven palm leaf hat", hat);
            const float radius = 0.425f;
            const float height = 0.248f;
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
            // Fine bamboo ribs, with enough clearance to avoid coplanar flicker.
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2 / 12;
                mesh.Tube(new[] { new Vector3(Mathf.Cos(a) * 0.034f, 0.231f, Mathf.Sin(a) * 0.034f),
                    new Vector3(Mathf.Cos(a) * 0.419f, 0.007f, Mathf.Sin(a) * 0.419f) }, 0.0017f, StrawDark, 5);
            }
            mesh.Finish();
            var strap = kit.Group("Soft hat ties", head);
            for (int side = -1; side <= 1; side += 2)
                strap.Tube(new[] { new Vector3(side * 0.232f, 0.191f, -0.011f),
                    new Vector3(side * 0.245f, -0.067f, -0.020f), new Vector3(side * 0.157f, -0.218f, 0.016f),
                    new Vector3(side * 0.018f, -0.256f, 0.080f) }, 0.005f, ShirtTrim, 6);
            strap.Finish();
        }

        private static Transform BuildArm(FarmerMeshKit kit, Transform pivot, string name, int side)
        {
            var arm = kit.Group(name, pivot);
            arm.Loft(new[] {
                new Vector4(0.054f, -0.246f, 0.059f, 0.017f),
                new Vector4(0.064f, -0.180f, 0.067f, 0.008f),
                new Vector4(0.076f, -0.067f, 0.078f, 0),
                new Vector4(0.065f, 0.015f, 0.068f, 0),
                new Vector4(0.022f, 0.049f, 0.029f, 0)
            }, Shirt, 20);
            arm.Loft(new[] { new Vector4(0.057f, -0.257f, 0.061f, 0.017f),
                new Vector4(0.062f, -0.245f, 0.065f, 0.017f), new Vector4(0.060f, -0.218f, 0.063f, 0.014f) }, Cream, 20);
            arm.Finish();
            Transform elbow=Joint("Elbow",pivot,new Vector3(0,-.246f,.017f));
            arm=kit.Group("Forearm",elbow,new Vector3(0,.246f,-.017f));
            arm.Loft(new[] { new Vector4(0.037f, -0.395f, 0.039f, 0.037f),
                new Vector4(0.044f, -0.338f, 0.044f, 0.028f), new Vector4(0.052f, -0.246f, 0.054f, 0.017f) }, Skin, 20);
            arm.Finish();
            Transform hand = Joint("Hand", elbow, new Vector3(0, -0.171f, 0.022f));
            var fist = kit.Group("Shaped hand", hand);
            fist.Ellipsoid(Vector3.zero, new Vector3(0.088f, 0.119f, 0.076f), Skin, 16, 10);
            fist.Ellipsoid(new Vector3(-side * 0.029f, 0.012f, 0.032f), new Vector3(0.042f, 0.067f, 0.040f), Skin, 12, 8,
                Quaternion.Euler(-20, 0, side * -20));
            fist.Finish();
            return hand;
        }

        private static void BuildLeg(FarmerMeshKit kit, Transform pivot, string name)
        {
            var thigh = kit.Group(name + " upper", pivot);
            thigh.Loft(new[] { new Vector4(.072f,-.28f,.078f,.008f),new Vector4(.081f,-.15f,.091f,0),new Vector4(.092f,.008f,.106f,0) },Pants,24);
            thigh.Ellipsoid(new Vector3(0,-.28f,.008f),new Vector3(.148f,.13f,.16f),Pants,20,12);
            thigh.Finish();
            Transform knee=Joint("Knee",pivot,new Vector3(0,-.28f,0));
            var leg = kit.Group(name + " lower", knee, new Vector3(0,.28f,0));
            leg.Loft(new[] {
                new Vector4(0.066f, -0.435f, 0.072f, 0.004f), new Vector4(0.072f, -0.389f, 0.077f, 0.004f),
                new Vector4(0.074f, -0.263f, 0.082f, 0.013f)
            }, Pants, 20);
            leg.Profile(new[] { new Vector3(0.071f, -0.441f, 0.079f), new Vector3(0.076f, -0.433f, 0.083f),
                new Vector3(0.076f, -0.407f, 0.083f) }, FarmWorld.Hex("4B6060"), 20);
            // Flat soles and a continuous upper give the shoes weight on the ground.
            leg.Loft(new[] { new Vector4(0.082f, -0.575f, 0.123f, 0.044f),
                new Vector4(0.088f, -0.552f, 0.130f, 0.044f), new Vector4(0.084f, -0.523f, 0.123f, 0.040f),
                new Vector4(0.075f, -0.490f, 0.097f, 0.017f), new Vector4(0.061f, -0.432f, 0.066f, 0) }, Boots, 24, 0.75f);
            leg.Loft(new[] { new Vector4(0.082f, -0.590f, 0.123f, 0.044f),
                new Vector4(0.089f, -0.577f, 0.131f, 0.044f), new Vector4(0.088f, -0.559f, 0.130f, 0.044f) }, Ink, 24, 0.75f);
            leg.Finish();
        }

        internal static GameObject BuildWateringCan(FarmerMeshKit kit, Transform hand)
        {
            Transform root = Joint("Watering can", hand, new Vector3(0.08f, -0.115f, 0.13f));
            var can = kit.Group("Watering can mesh", root);
            Color metal = FarmWorld.Hex("83B9B0");
            can.Profile(new[] { new Vector3(0.102f, -0.095f, 0.087f), new Vector3(0.11f, 0.055f, 0.09f), new Vector3(0.08f, 0.073f, 0.073f) }, metal, 10);
            can.Tube(new[] { new Vector3(0, -0.013f, 0.06f), new Vector3(0, 0.025f, 0.20f), new Vector3(0, 0.08f, 0.24f) }, 0.026f, metal, 8);
            can.Ellipsoid(new Vector3(0, 0.08f, 0.244f), new Vector3(0.085f, 0.036f, 0.063f), Cream, 8, 5);
            can.Tube(new[] { new Vector3(0, 0.04f, -0.08f), new Vector3(0, 0.155f, -0.067f), new Vector3(0, 0.155f, 0.035f), new Vector3(0, 0.057f, 0.047f) }, 0.012f, metal, 6);
            can.Finish();
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private static Transform Joint(string name, Transform parent, Vector3 position)
        {
            return FarmerMeshKit.Joint(name, parent, position);
        }
    }
}
