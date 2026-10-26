using UnityEngine;
namespace HarvestIsle
{
    /// <summary>Standalone motion-review controls; no dependency on farm saves or UI.</summary>
    public sealed class ReferenceMaleMotionViewer : MonoBehaviour
    {
        public ReferenceMaleMotion actor;
        public Camera viewCamera;
        public string title = "MODEL NAM — XEM CHUYỂN ĐỘNG";
        private float yaw = 25;
        private void Awake()
        {
            if (viewCamera == null) return;
            viewCamera.orthographicSize = 1.55f;
            yaw = Mathf.Atan2(viewCamera.transform.position.x, viewCamera.transform.position.z) * Mathf.Rad2Deg;
        }
        private void OnGUI()
        {
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            Matrix4x4 previous = GUI.matrix; GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            GUI.Box(new Rect(16, 14, 1248, 60), title);
            GUI.Label(new Rect(30, 44, 1100, 25), "Chọn động tác · Đi/chạy tại chỗ · Mẫu thử riêng");
            for (int i = 0; i < ReferenceMaleMotion.Labels.Length; i++)
                if (GUI.Button(new Rect(18 + (i % 6) * 160, 608 + (i / 6) * 46, 151, 40), (int)actor.action == i ? "• " + ReferenceMaleMotion.Labels[i] : ReferenceMaleMotion.Labels[i])) actor.Select((ReferenceMaleAction)i);
            if (GUI.Button(new Rect(1010, 608, 110, 40), actor.playing ? "Tạm dừng" : "Tiếp tục")) actor.playing = !actor.playing;
            if (GUI.Button(new Rect(1130, 608, 110, 40), actor.speed < .75f ? "Tốc độ 1×" : "Chậm 0,5×")) actor.speed = actor.speed < .75f ? 1 : .5f;
            if (GUI.Button(new Rect(1010, 654, 110, 40), "Xoay trái")) { yaw -= 30; CameraAngle(); }
            if (GUI.Button(new Rect(1130, 654, 110, 40), "Xoay phải")) { yaw += 30; CameraAngle(); }
            GUI.matrix = previous;
        }
        private void CameraAngle()
        {
            viewCamera.transform.position = Quaternion.Euler(0, yaw, 0) * new Vector3(0, 1.35f, 5);
            viewCamera.transform.LookAt(new Vector3(0, .9f, .15f));
        }
    }
}
