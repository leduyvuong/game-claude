using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    // Selection is a preview only. Releasing commits sowing or opens the harvest challenge.
    public sealed class FarmBatchActions : MonoBehaviour
    {
        public int Mode { get; private set; } // 0 camera, 1 sow, 2 harvest
        public bool Active { get; private set; }
        public int Count => selected.Count;
        private FarmGame game;
        private readonly List<int> selected = new List<int>();
        private readonly List<string> crops = new List<string>();
        private readonly List<double> planted = new List<double>();
        private bool drawing;
        private float started;
        private int round, hits, mistakes;
        private bool answered;
        private Texture2D basket;
        public void Configure(FarmGame owner) { game = owner; basket=Resources.Load<Texture2D>("ModelThumbnails/Basket"); }
        public void SetMode(int mode)
        {
            if (Active || game.IsManaging || game.IsHarvesting || game.Ranch.IsOpen || game.Hotpot.IsOpen) return;
            CancelSelection();
            if (game.Online != null) { game.Notify("Thao tác hàng loạt hiện dành cho chơi một mình."); return; }
            Mode = mode; game.Hud.ClosePanels(); game.ClearSelection();
            game.Notify(mode == 1 ? "Kéo qua đất trống để gieo giống đang chọn; thả để gieo." : mode == 2 ? "Kéo qua cây chín; thả để chơi Nhịp mùa vàng." : "Kéo để di chuyển góc nhìn.");
        }
        public void Begin() { CancelSelection(); drawing = Mode != 0; }
        public void Add(int index)
        {
            if (!drawing || selected.Contains(index) || index < 0 || index >= game.Model.State.plots.Count) return;
            var p = game.Model.State.plots[index];
            if (Mode == 1 ? !string.IsNullOrEmpty(p.cropId) : string.IsNullOrEmpty(p.cropId) || game.Model.Progress(index, game.Now) < 1) return;
            selected.Add(index);
        }
        public void CancelSelection() { if (Active) return; drawing = false; selected.Clear(); }
        public void Release()
        {
            if (!drawing) return;
            drawing = false;
            if (selected.Count == 0) return;
            if (Mode == 1)
            {
                int done = 0; string reason="";
                foreach (int index in selected)
                {
                    var result = game.Model.Apply(index, FarmTool.Plant, game.SelectedCrop, game.Now);
                    if (!result.success) { reason=result.message; break; }
                    done++; game.World.Pulse(index, FarmTool.Plant);
                }
                game.Save(); game.Refresh();
                game.Notify("Đã gieo " + done + "/" + selected.Count + " ô. " + reason,done==0);
                selected.Clear(); return;
            }
            crops.Clear(); planted.Clear();
            foreach (int index in selected) { var p = game.Model.State.plots[index]; crops.Add(p.cropId); planted.Add(p.plantedAt); }
            game.Hud.ClosePanels(); game.Hud.gameObject.SetActive(false);
            Active = true; round = hits = mistakes = 0; answered = false; started = Time.unscaledTime;
        }
        public int Rounds => Mathf.Clamp(Count + 3, 6, 12);
        public float RoundSeconds => Mathf.Max(.85f, 1.65f - Count * .025f - round * .035f);
        public float Beat => (Time.unscaledTime - started) / RoundSeconds;
        public int Target => (round * 7 + Count * 3 + round / 3) % 3;
        public void Strike(int lane)
        {
            if (!Active || answered) return;
            answered = true;
            if (lane == Target && Mathf.Abs(Beat - .72f) <= .105f) hits++; else mistakes++;
        }
        private void Update()
        {
            if (!Active || Beat < 1) return;
            if (!answered) mistakes++;
            round++; answered = false; started = Time.unscaledTime;
            if (round >= Rounds) Complete();
        }
        private void Complete()
        {
            if (!Active) return;
            // Disable first: completion is idempotent, even if a callback refreshes the HUD.
            Active = false; game.Hud.gameObject.SetActive(true);
            int score = hits >= Mathf.CeilToInt(Rounds * .9f) ? 3 : hits >= Mathf.CeilToInt(Rounds * .7f) ? 2 : hits >= Mathf.CeilToInt(Rounds * .45f) ? 1 : 0;
            int done = 0;
            for (int i = 0; i < selected.Count; i++)
            {
                int index = selected[i]; var p = game.Model.State.plots[index];
                if (p.cropId != crops[i] || p.plantedAt != planted[i]) continue;
                int before = game.Model.State.harvested;
                game.FinishHarvest(index, score);
                if (game.Model.State.harvested > before) done++;
            }
            selected.Clear(); Mode = 0;
            game.Notify("Thu " + done + " ô · " + hits + " nhịp đúng · " + score + "/3 điểm kỹ năng");
        }
        public void Cancel()
        {
            bool wasActive = Active; Active = false; Mode = 0; CancelSelection();
            if (game != null && game.Hud != null) { game.Hud.gameObject.SetActive(true); if (wasActive) game.Notify("Đã dừng; cây vẫn ở ruộng."); }
        }
        private void OnApplicationFocus(bool focus) { if (!focus) { if (Active) Cancel(); else CancelSelection(); } }
        private void OnApplicationPause(bool pause) { if (pause) { if (Active) Cancel(); else CancelSelection(); } }
        private void OnGUI()
        {
            if (game == null || game.Model == null) return;
            var old = GUI.color;
            var label = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            label.normal.textColor = new Color(.22f,.27f,.17f);
            if (!Active)
            {
                if (Mode == 0 || game.IsManaging || game.Ranch.IsOpen || game.Hotpot.IsOpen || game.Hud.IsReading) return;
                foreach (int index in selected)
                {
                    Vector3 p = game.World.GameCamera.WorldToScreenPoint(game.World.PositionOfPlot(index));
                    GUI.color = new Color(.9f,1,.5f,.9f); GUI.DrawTexture(new Rect(p.x-18,Screen.height-p.y-18,36,36),Texture2D.whiteTexture);
                    GUI.color=Color.white;GUI.Label(new Rect(p.x-18,Screen.height-p.y-18,36,36),"✓",label);
                }
                GUI.color = new Color(.98f,.94f,.81f,.96f); GUI.DrawTexture(new Rect(Screen.width/2-220,100,440,58),Texture2D.whiteTexture);GUI.color=old;
                GUI.Label(new Rect(Screen.width/2-215,100,430,58), (Mode == 1 ? "GIEO · " + CropCatalog.Find(game.SelectedCrop).displayName : "THU HOẠCH · kéo chọn cây chín") + " · " + Count + " ô" + (Mode==1?"\nChi phí tối đa: "+FarmModel.Money(Count*CropCatalog.Find(game.SelectedCrop).seedCost):""), label);
                return;
            }
            var matrix = GUI.matrix; var safe = Screen.safeArea;
            float scale = Mathf.Min(safe.width / 1000, safe.height / 650);
            GUI.matrix = Matrix4x4.TRS(new Vector3(safe.x+(safe.width-1000*scale)/2,Screen.height-safe.yMax+(safe.height-650*scale)/2),Quaternion.identity,Vector3.one*scale);
            GUI.color = new Color(.97f,.91f,.76f); GUI.DrawTexture(new Rect(0,0,1000,650),Texture2D.whiteTexture); GUI.color=Color.white;
            label.fontSize=30; GUI.Label(new Rect(30,25,940,55),"NHỊP MÙA VÀNG · " + Count + " ô",label);
            label.fontSize=19; GUI.Label(new Rect(45,85,910,60),"Chạm ĐÚNG GIỎ khi vạch chạy vào vùng xanh. Nhịp tăng dần; chạm sai mất lượt.\nDưới 45%: sản lượng giảm · 70%: thưởng +1 · 90%: thưởng +2 mỗi ô",label);
            var button=new GUIStyle(GUI.skin.button){fontSize=21,fontStyle=FontStyle.Bold};
            button.normal.background=Texture2D.whiteTexture;button.hover.background=Texture2D.whiteTexture;button.active.background=Texture2D.whiteTexture;
            button.normal.textColor=button.hover.textColor=button.active.textColor=new Color(.22f,.27f,.17f);
            GUI.Label(new Rect(60,155,880,40),"Nhịp " + (round+1) + "/" + Rounds + "    Đúng " + hits + "    Lỡ " + mistakes,label);
            for(int i=0;i<3;i++)
            {
                float x=65+i*300;
                GUI.color=i==Target?new Color(.95f,.71f,.26f):new Color(.78f,.81f,.73f);
                GUI.DrawTexture(new Rect(x,220,270,270),Texture2D.whiteTexture); GUI.color=Color.white;
                if(basket!=null)GUI.DrawTexture(new Rect(x+12,230,52,52),basket,ScaleMode.ScaleToFit);
                GUI.Label(new Rect(x+62,230,200,50),i==Target?"↓ GIỎ CẦN THU":"Chờ nhịp sau",label);
                GUI.color=new Color(.23f,.48f,.26f); GUI.DrawTexture(new Rect(x+25+220*.615f,310,220*.21f,65),Texture2D.whiteTexture);
                GUI.color=Color.white; GUI.DrawTexture(new Rect(x+25+220*Mathf.Clamp01(Beat)-3,300,6,85),Texture2D.whiteTexture);
                GUI.enabled=!answered;
                if(GUI.Button(new Rect(x+25,410,220,60),"GIỎ " +(i+1),button))Strike(i);
                GUI.enabled=true;
            }
            GUI.Label(new Rect(100,510,800,45),answered?"Đã chạm · chuẩn bị nhịp tiếp theo":"Canh vạch trắng vào vùng xanh rồi chạm giỏ vàng",label);
            if(GUI.Button(new Rect(350,575,300,48),"Dừng · giữ nguyên cây",button))Cancel();
            GUI.matrix=matrix;GUI.color=old;
        }
    }
}
