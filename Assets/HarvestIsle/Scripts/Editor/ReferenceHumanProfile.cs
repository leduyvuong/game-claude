using System;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HarvestIsle.Editor
{
    /// <summary>Scopes shared study tools to separate male/female assets and reports.</summary>
    public static class ReferenceHumanProfile
    {
        public static bool Female { get; private set; }
        public static string Asset => Female ? "ReferenceFemale" : "ReferenceMale";
        public static string Slug => Female ? "reference-female" : "reference-male";
        public static string Folder => "Assets/HarvestIsle/Experiments/" + Asset;
        public static string Output => "Art/Previews/" + Asset;
        public static string DisplayName => Female ? "Nữ — Mẫu thử theo ảnh tham chiếu" : "Nam — Mẫu thử theo ảnh tham chiếu";
        public static void WithFemale(Action action)
        {
            bool previous = Female; Female = true;
            try { action(); } finally { Female = previous; }
        }
    }

    public static class ReferenceFemaleStudy
    {
        [MenuItem("Harvest Isle/Characters/Render reference female study")]
        public static void Build() { ReferenceHumanProfile.WithFemale(ReferenceMaleStudy.Build); }
        [MenuItem("Harvest Isle/Characters/Build and capture reference female motions")]
        public static void BuildMotions() { ReferenceHumanProfile.WithFemale(ReferenceMaleMotionStudy.Build); }
        [MenuItem("Harvest Isle/Characters/Open reference female motion studio")]
        public static void Open() { ReferenceHumanProfile.WithFemale(ReferenceMaleMotionStudy.Open); }
        public static void CheckContacts() { ReferenceHumanProfile.WithFemale(ReferenceMaleMotionStudy.CheckContacts); }
        public static void Review() { ReferenceHumanProfile.WithFemale(ReferenceMaleMotionPlayReview.Begin); }
    }
}
