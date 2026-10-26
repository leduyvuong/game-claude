using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HarvestIsle
{
    public sealed partial class FarmHud : MonoBehaviour
    {
        private GameObject menuPanel, helpPanel, seedPanel, detailPanel, toastPanel;
        private FarmGame game;
        private Font font;
        private RectTransform safeArea;
        private Sprite rounded;
        private Texture2D roundedTexture;
        private Sprite circle;
        private Texture2D circleTexture;
        private Text levelBadge;
        private Text coins;
        private Text saveStatus;
        private Text objective;
        private Text plotTitle;
        private Text plotBody;
        private Text hint;
        private Text inventory;
        private Text sellLabel;
        private Button plotAction;
        private Text plotActionLabel;
        private Image inspectCard;
        private Text weatherTitle,weatherForecast;
        private Button drainButton,treatButton,buildDrainButton,pestButton;
        private Text floodStatus,buildDrainLabel;
        private Text[] cropLabels;
        private Image[] cropCards;
        private Image[] toolCards;
        private Image progress;
        private GameObject inventoryPanel;
        private Rect previousSafeArea;
        private Vector2Int previousResolution;
        private int shownCoins = -1;
        private float coinPop;
        // Hay Day-inspired palette: warm brown ink, saturated grass green and candy gold.
        private readonly Color ink = FarmWorld.Hex("5A4327");
        private readonly Color muted = FarmWorld.Hex("8A7A5C");
        private readonly Color paper = FarmWorld.Hex("FFF6DF");
        private readonly Color accent = FarmWorld.Hex("5FAE3E");
        private readonly Color gold = FarmWorld.Hex("F7B733");

        public void Build(FarmGame game)
        {
            this.game = game;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rounded = MakeRoundedSprite();
            circle = MakeCircleSprite();
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if (EventSystem.current == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                // The module creates default pointer actions automatically on enable.
                var module = events.GetComponent<InputSystemUIInputModule>();
                module.AssignDefaultActions();
            }
            safeArea = Rect("Safe area", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ApplySafeArea();

            // Hay Day-style status row: round level token, dark coin pill, weather chip.
            var badgeRim = Circle("Level badge", safeArea, new Vector2(18, -14), 62, gold);
            var badgeFill = Circle("Level fill", badgeRim, new Vector2(5, -5), 52, accent);
            levelBadge = Label("1", badgeFill, Vector2.zero, new Vector2(52, 50), 24, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            Decorate(levelBadge, FarmWorld.Hex("376B1E"));
            var wallet = Panel("Status", safeArea, new Vector2(0, 1), new Vector2(92, -20), new Vector2(236, 52), new Color(.28f, .19f, .10f, .93f));
            var coinIcon = Circle("Coin icon", wallet, new Vector2(7, -5), 42, Color.white);
            coinIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("Currency/HarvestCoin");
            coins = Label("", wallet, new Vector2(54, -6), new Vector2(172, 40), 20, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            coins.resizeTextForBestFit = true;
            coins.resizeTextMinSize = 14;
            coins.resizeTextMaxSize = 20;
            Decorate(coins, FarmWorld.Hex("3A2410"));
            var weatherChip = Panel("Weather", safeArea, new Vector2(0, 1), new Vector2(340, -20), new Vector2(150, 52), paper);
            weatherTitle = Label("", weatherChip, new Vector2(6, -4), new Vector2(138, 44), 14, ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            Button("Kho", safeArea, Vector2.one, new Vector2(-204, -20), new Vector2(84, 60), paper, ink, () => Toggle(inventoryPanel), out _);
            Button("Menu", safeArea, Vector2.one, new Vector2(-108, -20), new Vector2(88, 60), paper, ink, () => Toggle(menuPanel), out _);

            menuPanel = Panel("Menu panel", safeArea, Vector2.one, new Vector2(-324, -94), new Vector2(304, 596), paper).gameObject;
            Label("NÔNG TRẠI", menuPanel.transform, new Vector2(18, -18), new Vector2(230, 26), 19, ink, FontStyle.Bold);
            weatherForecast = Label("", menuPanel.transform, new Vector2(18, -52), new Vector2(268, 45), 15, muted);
            Button("Chăn nuôi", menuPanel.transform, new Vector2(0, 1), new Vector2(18, -104), new Vector2(128, 52), gold, ink,
                () => { ClosePanels(); game.Ranch.Open(); }, out _);
            Button("Tiệm lẩu", menuPanel.transform, new Vector2(0, 1), new Vector2(158, -104), new Vector2(128, 52), gold, ink,
                () => { ClosePanels(); game.Hotpot.Open(); }, out _);
            Button("Về ruộng", menuPanel.transform, new Vector2(0, 1), new Vector2(18, -168), new Vector2(128, 52), paper, ink,
                () => { ClosePanels(); game.CameraController.GoHome(); }, out _);
            Button("Toàn đảo", menuPanel.transform, new Vector2(0, 1), new Vector2(158, -168), new Vector2(128, 52), paper, ink,
                () => { ClosePanels(); game.CameraController.ShowIsland(); }, out _);
            Button("−", menuPanel.transform, new Vector2(0, 1), new Vector2(18, -232), new Vector2(60, 52), paper, ink, () => game.CameraController.ZoomBy(1), out _);
            Button("+", menuPanel.transform, new Vector2(0, 1), new Vector2(86, -232), new Vector2(60, 52), paper, ink, () => game.CameraController.ZoomBy(-1), out _);
            Button("Khu phố", menuPanel.transform, new Vector2(0, 1), new Vector2(158, -232), new Vector2(128, 52), paper, ink,
                () => { ClosePanels(); game.CameraController.ShowTown(); }, out _);
            Button("Hướng dẫn & tiến độ", menuPanel.transform, new Vector2(0, 1), new Vector2(18, -296), new Vector2(268, 52), accent, paper,
                () => Toggle(helpPanel), out _);
            Button("Phát triển đảo",menuPanel.transform,new Vector2(0,1),new Vector2(18,-360),new Vector2(268,52),gold,ink,()=>{ClosePanels();game.Development.Open(0);},out _);
            Button("Online local",menuPanel.transform,new Vector2(0,1),new Vector2(18,-424),new Vector2(128,52),accent,paper,()=>{ClosePanels();if(game.IsEstate)game.Notify("Hãy về đảo của tôi để mở Online.");else FarmOnlineHub.Open(game);},out _);
            Button(game.IsEstate?"Về đảo của tôi":"Thăm An Nhiên",menuPanel.transform,new Vector2(0,1),new Vector2(158,-424),new Vector2(128,52),gold,ink,()=>FarmEstate.Travel(game),out _);
            saveStatus = Label("", menuPanel.transform, new Vector2(18, -563), new Vector2(268, 22), 13, muted);

            helpPanel = Panel("Help", safeArea, Vector2.one, new Vector2(-324, -94), new Vector2(304, 310), paper).gameObject;
            Label("VỤ MÙA ĐẦU TIÊN", helpPanel.transform, new Vector2(18, -18), new Vector2(268, 25), 19, ink, FontStyle.Bold);
            objective = Label("", helpPanel.transform, new Vector2(18, -62), new Vector2(268, 150), 17, ink);
            Label("Đồng tiền: "+FarmModel.CurrencyName+".\nĐổi nhóm cây: +10–20% sản lượng.\nCây vẫn lớn khi bạn thoát game.", helpPanel.transform, new Vector2(18, -224), new Vector2(268, 72), 14, muted);

            var detail = Panel("Plot detail", safeArea, Vector2.one, new Vector2(-324, -94), new Vector2(304, 474), paper);
            detailPanel = detail.gameObject;
            plotTitle = Label("", detail, new Vector2(17, -17), new Vector2(220, 28), 16, ink, FontStyle.Bold);
            Button("×", detail, new Vector2(0, 1), new Vector2(246, -6), new Vector2(52, 48), paper, ink, game.ClearSelection, out _);
            plotBody = Label("", detail, new Vector2(17, -54), new Vector2(270, 100), 15, muted);
            floodStatus = Label("", detail, new Vector2(17,-159), new Vector2(270,46),14,ink);
            var track = Panel("Growth track", detail, new Vector2(0, 1), new Vector2(17, -211), new Vector2(270, 7), FarmWorld.Hex("DEDDBE"));
            progress = Panel("Growth fill", track, new Vector2(0, 1), Vector2.zero, new Vector2(0, 7), accent).GetComponent<Image>();
            plotAction = Button("Chọn một ô đất", detail, new Vector2(0, 1), new Vector2(17, -230), new Vector2(270, 50), accent, paper, ActOnSelected, out plotActionLabel);
            plotActionLabel.fontSize = 16;
            drainButton = Button("Thoát nước\nMiễn phí", detail, new Vector2(0, 1), new Vector2(17, -292), new Vector2(130, 48), paper, ink, () => Care(FarmTool.Drain), out var drainLabel);
            treatButton = Button("Trị bệnh · " + FarmModel.Money(FarmModel.TreatCost), detail, new Vector2(0, 1), new Vector2(157, -292), new Vector2(130, 48), paper, ink, () => Care(FarmTool.Treat), out var treatLabel);
            drainLabel.fontSize = treatLabel.fontSize = 14;
            buildDrainButton=Button("Xây rãnh · "+FarmModel.Money(FarmModel.DrainageCost),detail,new Vector2(0,1),new Vector2(17,-354),new Vector2(270,52),gold,ink,()=>Care(FarmTool.BuildDrain),out buildDrainLabel);
            buildDrainLabel.fontSize=14;
            pestButton=Button("Bắt sâu! Sâu đang phá cây",detail,new Vector2(0,1),new Vector2(17,-414),new Vector2(270,46),FarmWorld.Hex("C9563C"),paper,
                ()=>{if(game.SelectedPlot>=0)game.FieldGame.BeginPest(game.SelectedPlot);},out var pestLabel);
            pestLabel.fontSize=14;

            inventoryPanel = Panel("Inventory", safeArea, Vector2.one, new Vector2(-324, -94), new Vector2(304, 336), paper).gameObject;
            Label("KHO NÔNG SẢN", inventoryPanel.transform, new Vector2(18, -18), new Vector2(268, 26), 19, ink, FontStyle.Bold);
            inventory = Label("", inventoryPanel.transform, new Vector2(18, -65), new Vector2(268, 136), 19, ink);
            Button("Xem toàn bộ kho",inventoryPanel.transform,new Vector2(0,1),new Vector2(18,-202),new Vector2(268,44),paper,ink,()=>{ClosePanels();game.Development.Open(4);},out _);
            Button("Bán hàng", inventoryPanel.transform, new Vector2(0, 1), new Vector2(18, -258), new Vector2(268, 56), gold, ink, game.Sell, out sellLabel);

            var toolbar = Panel("Tools", safeArea, new Vector2(.5f, 0), new Vector2(-280, 88), new Vector2(560, 68), paper);
            inspectCard = Button("Xem ô", toolbar, new Vector2(0, 1), new Vector2(8, -8), new Vector2(126, 52), paper, ink,
                () => { ClosePanels(); game.SelectTool(FarmTool.Inspect); }, out _).GetComponent<Image>();
            toolCards = new Image[3];
            string[] toolNames = { "Giống cây", "Tưới", "Thu hoạch" };
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var button = Button(toolNames[i], toolbar, new Vector2(0, 1), new Vector2(142 + i * 138, -8), new Vector2(132, 52), paper, ink,
                    () => { if (index == 0) Toggle(seedPanel); else { ClosePanels(); game.SelectTool((FarmTool)index); } }, out _);
                toolCards[i] = button.GetComponent<Image>();
            }
            seedPanel = Panel("Seeds", safeArea, new Vector2(.5f, 0), new Vector2(-280, 254), new Vector2(560, 154), paper).gameObject;
            Label("CHỌN GIỐNG ĐỂ GIEO", seedPanel.transform, new Vector2(16, -14), new Vector2(520, 25), 17, ink, FontStyle.Bold);
            Button("30 giống →",seedPanel.transform,new Vector2(0,1),new Vector2(398,-5),new Vector2(150,44),gold,ink,()=>{ClosePanels();game.Development.Open(5);},out _);
            cropCards = new Image[3]; cropLabels = new Text[cropCards.Length];
            for (int i = 0; i < cropCards.Length; i++)
            {
                int index = i;
                var crop = CropCatalog.All[i];
                cropCards[i] = Button(crop.displayName + "\n" + FarmModel.Money(crop.seedCost) + " · " + crop.growSeconds + "s", seedPanel.transform,
                    new Vector2(0, 1), new Vector2(12 + i * 182, -52), new Vector2(172, 88), paper, ink,
                    () => { ClosePanels(); game.SelectCrop(CropCatalog.All[index].id); }, out cropLabels[i]).GetComponent<Image>();
            }
            var toast = Panel("Notification", safeArea, new Vector2(.5f, 0), new Vector2(-310, 158), new Vector2(620, 58), new Color(.24f, .16f, .08f, .94f));
            toastPanel = toast.gameObject;
            toast.GetComponent<Image>().raycastTarget = false;
            hint = Label("", toast, new Vector2(14, -6), new Vector2(592, 46), 16, paper, FontStyle.Normal, TextAnchor.MiddleCenter);
            BuildPlayInterface();
            ClosePanels();
            detailPanel.SetActive(false);
            toastPanel.SetActive(false);

        }

        public void Refresh()
        {
            if (game == null || game.Model == null || coins == null) return;
            var model = game.Model;
            string forecast=(model.State.climateTime<90?"Mưa sau ":model.State.climateTime<190?"Mưa lớn sau ":model.State.climateTime<230?"Tạnh sau ":"Nắng sau ")+Math.Ceiling(model.WeatherRemaining)+"s";
            weatherTitle.text=(model.RainLevel==0?"Nắng":model.RainLevel==1?"Mưa":"Mưa lớn")+"\n"+(game.Online!=null?"Online":forecast);
            weatherTitle.color=model.RainLevel==0?FarmWorld.Hex("C9862F"):model.RainLevel==1?FarmWorld.Hex("3E7AA0"):FarmWorld.Hex("2F5E86");
            weatherForecast.text=(model.State.climateTime<90?"Mưa sau ":model.State.climateTime<190?"Mưa lớn sau ":model.State.climateTime<230?"Tạnh sau ":"Chu kỳ mới sau ")+Math.Ceiling(model.WeatherRemaining)+" giây";
            inspectCard.color=game.Tool==FarmTool.Inspect?gold:paper;
            bool popup = IsReading;
            detailPanel.SetActive(game.SelectedPlot >= 0 && !popup);
            toastPanel.SetActive(game.HasNotification && !popup);
            coins.text = FarmModel.Money(model.State.coins);
            if (model.State.coins != shownCoins)
            {
                if (shownCoins >= 0) coinPop = .35f;
                shownCoins = model.State.coins;
            }
            saveStatus.text = "Cấp "+model.Level+" · "+game.SaveStatus;
            if (levelBadge != null) levelBadge.text = model.Level.ToString();
            int planted = 0; int watered = 0; int mature = 0;
            for (int i = 0; i < model.State.plots.Count; i++)
            {
                if (!string.IsNullOrEmpty(model.State.plots[i].cropId)) planted++;
                if (model.State.plots[i].watered) watered++;
                if (!string.IsNullOrEmpty(model.State.plots[i].cropId) && model.Progress(i, game.Now) >= 1) mature++;
            }
            objective.text = (planted > 0 || model.State.harvested > 0 ? "✓" : "01") + "  Gieo cây vào ruộng\n" +
                (watered > 0 || model.State.hasWatered ? "✓" : "02") + "  Tưới để cây lớn nhanh\n" +
                (model.State.harvested > 0 ? "✓" : "03") + "  Thu hoạch vào kho\n" +
                (model.State.hasSold ? "✓" : "04") + "  Bán hàng lấy tiền\n" +
                "\n" + planted + "/" + model.State.plots.Count + " ô trồng · " + mature + " ô đã chín";
            for (int i = 0; i < cropCards.Length; i++)
            {
                bool selected = game.SelectedCrop == CropCatalog.All[i].id && game.Tool == FarmTool.Plant;
                cropCards[i].color = selected ? accent : FarmWorld.Hex("F0E8CF");
                cropLabels[i].color = selected ? paper : ink;
            }
            for (int i = 0; i < toolCards.Length; i++)
            {
                bool selected = (int)game.Tool == i;
                toolCards[i].color = selected ? accent : FarmWorld.Hex("E5E8D4");
                toolCards[i].GetComponentInChildren<Text>().color = selected ? paper : ink;
            }
            toolCards[0].GetComponentInChildren<Text>().text = game.Tool == FarmTool.Plant ? CropCatalog.Find(game.SelectedCrop).displayName + " ▾" : "Giống cây";
            sellLabel.text = model.SellValue() > 0 ? "Bán hàng\n+" + FarmModel.Money(model.SellValue()) : "Bán hàng";
            inventory.text = "Cà rốt     " + model.Count("carrot") + "\nNgô         " + model.Count("corn") + "\nBí đỏ       " + model.Count("pumpkin")+"\nTrứng      "+model.State.eggs+"\nSữa         "+model.State.milk;
            RefreshPlayInterface();
            hint.text = game.Notification;
            hint.color = game.NotificationIsError && hint.text == game.Notification ? gold : paper;
            plotAction.interactable=false;
            drainButton.interactable=false;treatButton.interactable=false;
            if (game.SelectedPlot >= 0)
            {
                var state = model.State.plots[game.SelectedPlot];
                buildDrainButton.interactable=!state.drainageBuilt&&model.State.coins>=FarmModel.DrainageCost;
                buildDrainLabel.text=state.drainageBuilt?"Đã có rãnh cố định":"Xây rãnh · "+FarmModel.Money(FarmModel.DrainageCost)+"\nGiảm 75% nước mưa · tự rút nước";
                pestButton.gameObject.SetActive(state.pest>0&&!string.IsNullOrEmpty(state.cropId)&&game.Online==null);
                int floodLoss=FarmModel.IsWaterCrop(state.cropId)?0:FarmModel.FloodYieldLoss(state.soilWater);
                int diseaseLoss=state.disease>=70?2:state.disease>=35?1:0;
                floodStatus.text=(floodLoss>0?"Phạt úng: −"+floodLoss+" nông sản":"Không bị phạt úng")+(diseaseLoss>0?" · Bệnh: −"+diseaseLoss:"")+(state.pest>0?" · Sâu: −1":"")+"\n"+
                    (state.greenhouse?"Nhà kính đang che mưa":state.drainageBuilt?"Rãnh cố định đang tự rút nước":state.drainProtection>0?"Rãnh tạm: còn "+Math.Ceiling(state.drainProtection)+" giây":"Chưa có rãnh bảo vệ");
                floodStatus.color=floodLoss>0?FarmWorld.Hex("A85830"):muted;
                drainButton.interactable=state.soilWater>=.15||state.drainProtection<=0;
                treatButton.interactable=!string.IsNullOrEmpty(state.cropId)&&state.disease>=1&&model.State.coins>=FarmModel.TreatCost;
                var crop = CropCatalog.Find(state.cropId);
                plotTitle.text = "Ô ĐẤT " + (game.SelectedPlot + 1) + (crop == null ? " · TRỐNG" : " · " + crop.displayName.ToUpperInvariant());
                float grown = (float)model.Progress(game.SelectedPlot, game.Now);
                progress.rectTransform.sizeDelta = new Vector2(270 * grown, 7);
                if (crop == null)
                {
                    var seed=CropCatalog.Find(game.SelectedCrop);plotActionLabel.text=(state.soilWater>=.65&&!FarmModel.IsWaterCrop(seed.id))?"Thoát nước trước khi gieo":"Gieo "+seed.displayName+" · "+FarmModel.Money(seed.seedCost);plotAction.interactable=(model.State.coins>=seed.seedCost||model.State.development.seedlings.Exists(s=>s.cropId==seed.id&&s.readyAt<=game.Now))&&(state.soilWater<.65||FarmModel.IsWaterCrop(seed.id));
                    var previous = CropCatalog.Find(state.previousCropId);
                    plotBody.text = "Chọn giống rồi chạm ô để gieo." + (previous == null ? "\nĐất sẵn sàng cho vụ đầu." : "\nVụ trước: " + previous.displayName + " · "+FarmModel.CropGroup(previous.id)+"\nĐổi nhóm: +10–20% sản lượng.");
                }
                else
                {
                    plotAction.interactable=grown>=1||!state.watered;
                    plotActionLabel.text=grown>=1?"Thu hoạch vào kho":state.watered?"Đã tưới · cây đang lớn":"Tưới cây";
                    int stage=grown>=1?5:1+Mathf.Clamp(Mathf.FloorToInt(grown*4),0,3);
                    plotBody.text="Giai đoạn "+stage+"/5 · "+(grown>=1?"Đã chín!":"Còn "+Math.Ceiling(model.RemainingSeconds(game.SelectedPlot,game.Now))+" giây")+"\n"+
                        (state.watered?"Đủ nước · thời gian giảm 35%":"Chưa tưới")+"\nThu chuẩn: "+model.HarvestYield(game.SelectedPlot,1)+" · Bán: "+FarmModel.Money(crop.sellPrice);
                }
                plotBody.text+="\n"+(FarmModel.IsLowPlot(game.SelectedPlot)?"Ô trũng":"Ô cao")+" · Nước "+Math.Round(state.soilWater*100)+"% · Bệnh "+Math.Round(state.disease)+"%";
            }
        }

        public void ClosePanels()
        {
            if(windowBackdrop!=null)windowBackdrop.SetActive(false);
            foreach (var panel in new[] { inventoryPanel, menuPanel, helpPanel, seedPanel, orderPanel })
                if (panel != null) panel.SetActive(false);
        }

        private void Toggle(GameObject panel)
        {
            bool open = !panel.activeSelf;
            ClosePanels();
            panel.SetActive(open);
            Refresh();
        }

        private void Care(FarmTool tool){if(game.SelectedPlot<0)return;game.SelectTool(tool);game.ActOnPlot(game.SelectedPlot);game.SelectTool(FarmTool.Inspect);}

        public void ActOnSelected()
        {
            if(game.SelectedPlot<0)return;
            var plot=game.Model.State.plots[game.SelectedPlot];
            if(string.IsNullOrEmpty(plot.cropId)){Toggle(seedPanel);return;}
            if(game.Model.Progress(game.SelectedPlot,game.Now)>=1){game.QuickHarvest(game.SelectedPlot);return;}
            game.SelectTool(string.IsNullOrEmpty(plot.cropId)?FarmTool.Plant:game.Model.Progress(game.SelectedPlot,game.Now)>=1?FarmTool.Harvest:FarmTool.Water);
            game.ActOnPlot(game.SelectedPlot);
        }

        private void Update()
        {
            PositionPlayInterface();
            if (previousSafeArea != Screen.safeArea || previousResolution.x != Screen.width || previousResolution.y != Screen.height) ApplySafeArea();
            PositionReadingWindow();
            if (coinPop > 0 && coins != null)
            {
                coinPop = Mathf.Max(0, coinPop - Time.unscaledDeltaTime);
                float scale = 1 + .3f * Mathf.Sin(Mathf.PI * coinPop / .35f);
                coins.rectTransform.localScale = new Vector3(scale, scale, 1);
            }
        }

        private void ApplySafeArea()
        {
            if (safeArea == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect area = Screen.safeArea;
            safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            previousSafeArea = area;
            previousResolution = new Vector2Int(Screen.width, Screen.height);
        }

        private RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var rect = Rect(name, parent, anchor, anchor, position, size);
            rect.pivot = new Vector2(0, 1);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            return rect;
        }

        private Text Label(string text, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color,
            FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            var rect = Rect("Text", parent, new Vector2(0, 1), new Vector2(0, 1), position, size);
            rect.pivot = new Vector2(0, 1);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private Button Button(string text, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color background,
            Color foreground, Action onClick, out Text label)
        {
            var rect = Panel(text, parent, anchor, position, size, background);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.93f, 0.98f, 0.89f);
            colors.pressedColor = new Color(0.8f, 0.87f, 0.78f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick());
            label = Label(text, rect, new Vector2(8, -7), size - new Vector2(16, 14), 18, foreground, FontStyle.Bold, TextAnchor.MiddleCenter);
            // Hay Day-style candy buttons carry white text with a dark rim of their own hue.
            if (foreground == paper)
                Decorate(label, new Color(background.r * .42f, background.g * .42f, background.b * .42f, 1));
            return button;
        }

        private RectTransform Circle(string name, Transform parent, Vector2 position, float size, Color color)
        {
            var rect = Rect(name, parent, new Vector2(0, 1), new Vector2(0, 1), position, new Vector2(size, size));
            rect.pivot = new Vector2(0, 1);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = circle;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static void Decorate(Text label, Color rim)
        {
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = rim;
            outline.effectDistance = new Vector2(1.6f, -1.6f);
            var shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, .3f);
            shadow.effectDistance = new Vector2(0, -2);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform));
            var rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max;
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        private Sprite MakeRoundedSprite()
        {
            // Baked candy bevel: lighter top, darker bottom and a soft dark rim.
            // Every panel and button tints this sprite, so the whole HUD gets the relief.
            roundedTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "UI rounded corner" };
            roundedTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Max(12 - x, x - 51, 0);
                float dy = Mathf.Max(12 - y, y - 51, 0);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(12.5f - distance);
                float shade = Mathf.Lerp(.8f, 1f, y / 63f);
                if (y > 49 && y <= 57) shade = Mathf.Min(1, shade * 1.06f);
                shade *= Mathf.Lerp(.55f, 1, Mathf.Clamp01((12.5f - distance) / 2.4f));
                roundedTexture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
            }
            roundedTexture.Apply();
            return Sprite.Create(roundedTexture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect, new Vector4(16, 16, 16, 16));
        }

        private Sprite MakeCircleSprite()
        {
            circleTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "UI circle" };
            circleTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 31.5f, dy = y - 31.5f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(30.5f - distance);
                float shade = distance > 26 ? .55f : Mathf.Lerp(1f, .84f, distance / 26f);
                float hx = dx + 9, hy = dy - 9;
                if (Mathf.Sqrt(hx * hx + hy * hy) < 9) shade = Mathf.Min(1, shade * 1.14f);
                circleTexture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
            }
            circleTexture.Apply();
            return Sprite.Create(circleTexture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        }

        private void OnDestroy()
        {
            DisposePlayInterface();
            if (rounded != null) Destroy(rounded);
            if (roundedTexture != null) Destroy(roundedTexture);
            if (circle != null) Destroy(circle);
            if (circleTexture != null) Destroy(circleTexture);
        }
    }
}
