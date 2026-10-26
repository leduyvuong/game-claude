using HarvestIsle.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HarvestIsle
{
    /// <summary>
    /// Two small field minigames sharing one overlay: catching pests (tap the worm
    /// before it hides, three rounds) and transplanting nursery seedlings (stop the
    /// bar in the green zone for a deeper head start). Results flow through FarmModel.
    /// </summary>
    public sealed class FarmFieldGame : MonoBehaviour
    {
        private FarmGame game;
        private int plot;
        private string crop;
        private bool pestMode;
        private int round, caught;
        private float roundStart, start;
        private Rect worm;
        private bool wormVisible;
        public bool Active { get; private set; }

        public void Configure(FarmGame owner) { game = owner; }

        public void BeginPest(int index)
        {
            if (Active) return;
            plot = index; pestMode = true; round = 0; caught = 0;
            start = Time.unscaledTime;
            NextWorm();
            Active = true;
            game.Hud.gameObject.SetActive(false);
        }

        public void BeginTransplant(int index, string cropId)
        {
            if (Active) return;
            plot = index; crop = cropId; pestMode = false;
            start = Time.unscaledTime;
            Active = true;
            game.Hud.gameObject.SetActive(false);
        }

        private void NextWorm()
        {
            roundStart = Time.unscaledTime;
            wormVisible = true;
            worm = new Rect(Random.Range(150f, 720f), Random.Range(250f, 430f), 130, 86);
        }

        private void Finish()
        {
            Active = false;
            game.Hud.gameObject.SetActive(true);
            if (pestMode) game.CompletePestCatch(plot, caught);
        }

        public void Cancel()
        {
            if (!Active) return;
            Active = false;
            game.Hud.gameObject.SetActive(true);
            game.Notify(pestMode ? "Tạm dừng bắt sâu · sâu vẫn còn trên ô." : "Đã cấy theo cách thường (25% tiến độ).");
            if (!pestMode) game.CompleteTransplant(plot, 0);
        }

        private void Update()
        {
            if (!Active || !pestMode) return;
            // A worm stays up briefly; miss it and the round is lost.
            if (wormVisible && Time.unscaledTime - roundStart > 1.25f)
            {
                wormVisible = false;
                round++;
                if (round >= 3) Finish(); else NextWorm();
            }
        }

        private void OnApplicationPause(bool paused) { if (paused) Cancel(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Cancel(); }

        private void OnGUI()
        {
            if (!Active) return;
            GUI.color = new Color(0, 0, 0, .85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            Rect area = Screen.safeArea;
            float scale = Mathf.Min(area.width / 1000, area.height / 700);
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(area.x + (area.width - 1000 * scale) / 2, Screen.height - area.yMax + (area.height - 700 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            var title = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
            var body = new GUIStyle(title) { fontSize = 20 };
            var button = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            GUI.Box(new Rect(100, 85, 800, 530), "");
            if (pestMode)
            {
                GUI.Label(new Rect(130, 108, 740, 50), "BẮT SÂU · CHẠM TRƯỚC KHI SÂU TRỐN", title);
                GUI.Label(new Rect(130, 160, 740, 40), "Con " + Mathf.Min(3, round + 1) + "/3 · Bắt được " + caught + " · Cần 2 con để đuổi sâu, 3 con để được bảo vệ", body);
                if (wormVisible && GUI.Button(new Rect(worm.x, worm.y, worm.width, worm.height), "SÂU!", button))
                {
                    caught++; wormVisible = false;
                    round++;
                    if (round >= 3) Finish(); else NextWorm();
                }
                GUI.Label(new Rect(130, 470, 740, 40), "Sâu phá −1 nông sản mỗi vụ cho tới khi bị đuổi.", body);
            }
            else
            {
                float value = Mathf.PingPong((Time.unscaledTime - start) * .9f, 1f);
                GUI.Label(new Rect(130, 108, 740, 50), "CẤY MẠ · CANH VẠCH VÀO VÙNG XANH", title);
                GUI.Label(new Rect(130, 170, 740, 40), "Cấy khéo: cây con " + (CropCatalog.Find(crop) == null ? "" : CropCatalog.Find(crop).displayName) + " bắt đầu ở 35% thay vì 25%.", body);
                GUI.color = new Color(.25f, .3f, .3f); GUI.DrawTexture(new Rect(230, 290, 540, 42), Texture2D.whiteTexture);
                GUI.color = new Color(.35f, .8f, .4f); GUI.DrawTexture(new Rect(230 + 540 * .38f, 290, 540 * .24f, 42), Texture2D.whiteTexture);
                GUI.color = Color.white; GUI.DrawTexture(new Rect(230 + value * 534, 284, 6, 54), Texture2D.whiteTexture);
                if (GUI.Button(new Rect(340, 380, 320, 65), "CẤY", button))
                {
                    int score = value >= .38f && value <= .62f ? 1 : 0;
                    Active = false;
                    game.Hud.gameObject.SetActive(true);
                    game.CompleteTransplant(plot, score);
                }
            }
            if (GUI.Button(new Rect(350, 540, 300, 48), pestMode ? "Để sau" : "Cấy thường, bỏ qua", button)) Cancel();
            GUI.matrix = old;
        }
    }
}
