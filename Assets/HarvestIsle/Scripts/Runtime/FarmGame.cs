using System;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed class FarmGame : MonoBehaviour
    {
        public FarmModel Model { get; private set; }
        public bool IsEstate { get; private set; }
        public FarmTool Tool { get; private set; } = FarmTool.Inspect;
        public string SelectedCrop { get; private set; } = "carrot";
        public int SelectedPlot { get; private set; } = -1;
        public FarmWorld World { get; private set; }
        public FarmHud Hud { get; private set; }
        public FarmCamera CameraController { get; private set; }
        public double Now { get; private set; }
        private FarmSaveStore save;
        private Func<double> clock = UtcNow;
        private float nextUiUpdate;
        private float nextSave;
        private float notificationUntil;
        private bool ready;
        public FarmBatchActions Batch {get;private set;}
        public FarmHarvestGame HarvestGame {get;private set;}
        public FarmRanch Ranch {get;private set;}
        public FarmDevelopment Development {get;private set;}
        public FarmHotpot Hotpot {get;private set;}
        public FarmWorkforce Workforce {get;private set;}
        public FarmFieldGame FieldGame {get;private set;}
        public FarmDayNight DayNight {get;private set;}
        private float nextWorkerTick;
        private bool festivalWas;
        public FarmOnlineHub Online {get;private set;}
        public FarmIslandExpansion IslandExpansion {get;private set;}
        public bool IsManaging => Development!=null&&Development.IsOpen||FarmOnlineHub.IsPanelOpen||IslandExpansion!=null&&IslandExpansion.BlocksWorld;
        public bool IsHarvesting => Batch!=null&&Batch.Active|| HarvestGame!=null&&HarvestGame.Active||FieldGame!=null&&FieldGame.Active;
        public string Notification { get; private set; }
        public bool HasNotification => Time.unscaledTime < notificationUntil;

        public void ClearSelection() { SelectedPlot = -1; Refresh(); }
        public bool NotificationIsError { get; private set; }
        public string SaveStatus { get; private set; } = "Đang mở nông trại…";

        private void Start()
        {
            if(ready)return;
            InitializeSession(new FarmSaveStore(Application.persistentDataPath),UtcNow);
            // Only the real player session cycles day and night; isolated review
            // farms and the model park keep their own fixed lighting.
            DayNight=gameObject.AddComponent<FarmDayNight>();DayNight.Configure(this);
        }

        public void InitializeSession(FarmSaveStore store,Func<double> timeSource,FarmState initialState=null,FarmOnlineHub online=null,bool estate=false)
        {
            if(ready)throw new InvalidOperationException("Session already initialized");
            IsEstate=estate;
            save=store;clock=timeSource;Online=online;
            Application.targetFrameRate = 30;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Model = new FarmModel(initialState??save.Load(clock()));
            Now = Model.ObserveTime(clock());
            World = new GameObject("Harvest Isle · World").AddComponent<FarmWorld>();
            World.transform.SetParent(transform,false);
            World.Build(this);
            CameraController = World.GameCamera.gameObject.AddComponent<FarmCamera>();
            CameraController.Initialize(this);
            Hud = new GameObject("Harvest Isle · HUD").AddComponent<FarmHud>();
            Hud.transform.SetParent(transform,false);
            Hud.Build(this);
            Batch=gameObject.AddComponent<FarmBatchActions>();Batch.Configure(this);
            HarvestGame=gameObject.AddComponent<FarmHarvestGame>();HarvestGame.Configure(this);
            FieldGame=gameObject.AddComponent<FarmFieldGame>();FieldGame.Configure(this);
            gameObject.AddComponent<FarmClimate>().Configure(this);
            Ranch=gameObject.AddComponent<FarmRanch>();Ranch.Configure(this);
            Development=gameObject.AddComponent<FarmDevelopment>();Development.Configure(this);
            Hotpot=gameObject.AddComponent<FarmHotpot>();Hotpot.Configure(this);
            Workforce=gameObject.AddComponent<FarmWorkforce>();Workforce.Configure(this);
            IslandExpansion=gameObject.AddComponent<FarmIslandExpansion>();IslandExpansion.Configure(this);
            gameObject.AddComponent<FarmSeasons>().Initialize(this);
            ready = true;
            SaveStatus = Online==null?"Lưu trên thiết bị":"Lưu trên máy chủ local";
            Notify(save?.LastError ?? "Chào mừng về Đảo Mùa Vàng! Chọn giống rồi chạm ô đất để gieo.", save?.LastError != null);
            festivalWas = FarmModel.FestivalActive(Now); // no toast on load mid-festival
            nextSave = Time.unscaledTime + 15;
            Refresh();
        }

        private void Update()
        {
            if (!ready) return;
            Now = Model.ObserveTime(clock());
            if(Online==null&&!IsHarvesting&&!IsManaging&&!Ranch.IsOpen&&!Hotpot.IsOpen&&!Hud.IsReading&&Model.TickIslandWater(Now)){Save();Refresh();}
            if(!IsHarvesting&&!IsManaging&&!Ranch.IsOpen&&!Hotpot.IsOpen&&!Hud.IsReading&&Model.State.harvested>=5&&Online==null)Model.AdvanceEcology(Math.Min(2,Time.unscaledDeltaTime));
            if(Online==null&&!IsHarvesting&&!IsManaging&&!Ranch.IsOpen&&!Hotpot.IsOpen&&!Hud.IsReading&&Model.HasWorkingCrew&&Time.unscaledTime>=nextWorkerTick)
            {
                nextWorkerTick=Time.unscaledTime+4;
                if(Model.RunWorkers(clock())>0){Now=Model.State.lastSeenUtc;Workforce.ShowActions(Model.LastWorkerActions);Save();Refresh();}
            }
            bool festival=FarmModel.FestivalActive(Now);
            if(festival!=festivalWas)
            {
                festivalWas=festival;
                Notify(festival?"Lễ hội Đom Đóm bắt đầu! Giá bán +20% và khách lẩu nườm nượp trong 2 phút.":"Lễ hội Đom Đóm khép lại · hẹn gặp lại sau 8 phút.");
            }
            if (Time.unscaledTime >= nextUiUpdate)
            {
                nextUiUpdate = Time.unscaledTime + 0.25f;
                Refresh();
            }
            if (Time.unscaledTime >= nextSave)
            {
                nextSave = Time.unscaledTime + 15;
                Save();
            }
        }

        public void SelectCrop(string id)
        {
            if(IsHarvesting||IsManaging||(Ranch!=null&&Ranch.IsOpen)||(Hotpot!=null&&Hotpot.IsOpen))return;
            if (CropCatalog.Find(id) == null) return;
            SelectedCrop = id;
            Tool = FarmTool.Plant;
            Refresh();
        }

        public void SelectTool(FarmTool tool) { if(IsHarvesting||IsManaging||(Ranch!=null&&Ranch.IsOpen)||(Hotpot!=null&&Hotpot.IsOpen))return;Tool = tool; Refresh(); }

        public void ActOnPlot(int index)
        {
            if(IsHarvesting||IsManaging||(Ranch!=null&&Ranch.IsOpen)||(Hotpot!=null&&Hotpot.IsOpen))return;
            if(index<0||index>=Model.State.plots.Count)return;
            Hud.ClosePanels();
            SelectedPlot = index;
            if(Tool==FarmTool.Inspect){Now=Model.ObserveTime(clock());Refresh();return;}
            if(Online!=null){Online.Command(Tool==FarmTool.Harvest?"harvest":"plot",SelectedCrop,index,(int)Tool);return;}
            // Transplanting a ready seedling opens the timing minigame first.
            if(Tool==FarmTool.Plant&&string.IsNullOrEmpty(Model.State.plots[index].cropId)
                &&Model.State.development.seedlings.Exists(s=>s.cropId==SelectedCrop&&s.readyAt<=Now)){FieldGame.BeginTransplant(index,SelectedCrop);return;}
            if(Tool==FarmTool.Harvest&&Model.Progress(index,clock())>=1){HarvestGame.Begin(index);return;}
            ActionResult result = Model.Apply(index, Tool, SelectedCrop, clock());
            Now = Model.State.lastSeenUtc;
            string message = result.message;
            // Early-game safety: an all-in sowing spree leaves no coins for Treat/BuildDrain when rain hits.
            if (result.success && Tool == FarmTool.Plant && Model.State.coins < FarmModel.DrainageCost)
                message += "\nVí còn " + FarmModel.Money(Model.State.coins) + " — nên giữ " + FarmModel.Money(FarmModel.TreatCost) + " Trị bệnh phòng mưa úng.";
            Notify(message, !result.success);
            if (result.success)
            {
                World.Pulse(index, Tool);
                World.MoveFarmerTo(index);
                Save();
            }
            Refresh();
        }

        public void Sell()
        {
            if(IsHarvesting)return;
            if(Online!=null){Online.Command("sell");return;}
            ActionResult result = Model.SellAll();
            Notify(result.message, !result.success);
            if (result.success) Save();
            Refresh();
        }

        public void Notify(string message, bool error = false)
        {
            Notification = message;
            NotificationIsError = error;
            notificationUntil = Time.unscaledTime + 6;
        }

        public string CurrentHint()
        {
            if (Time.unscaledTime < notificationUntil) return Notification;
            switch (Tool)
            {
                case FarmTool.Inspect: return "Chạm để xem ô đất · chọn giống để gieo · kéo để di chuyển, chụm để zoom.";
                case FarmTool.Water: return "Chạm cây để tưới · mỗi vụ tưới một lần · cây lớn nhanh hơn 35%.";
                case FarmTool.Harvest: return "Chạm cây đã chín để thu hoạch · ô đang chọn có viền vàng khi chín.";
                default: return "Chạm ô trống để gieo · kéo nền để di chuyển · cuộn hoặc chụm để zoom.";
            }
        }

        public void Save()
        {
            if(Online!=null){SaveStatus="Lưu trên máy chủ local";return;}
            if (save == null || Model == null) return;
            Model.ObserveTime(clock());
            SaveStatus = save.Save(Model.State) ? "Đã lưu " + DateTime.Now.ToString("HH:mm") : "Chưa lưu được";
            if (save.LastError != null) Notify(save.LastError, true);
        }

        public void Refresh()
        {
            if (World != null) World.Refresh(Model, Now, SelectedPlot);
            if (Hud != null) Hud.Refresh();
        }

        public void CompleteTransplant(int index,int score)
        {
            ActionResult result=Model.Apply(index,FarmTool.Plant,SelectedCrop,clock(),score);
            Now=Model.State.lastSeenUtc;
            Notify(result.message,!result.success);
            if(result.success){World.Pulse(index,FarmTool.Plant);World.MoveFarmerTo(index);Save();}
            Refresh();
        }

        public void CompletePestCatch(int index,int caught)
        {
            ActionResult result=Model.CatchPests(index,caught);
            Now=Model.State.lastSeenUtc;
            Notify(result.message,!result.success);
            if(result.success){World.Pulse(index,FarmTool.Treat);Save();}
            Refresh();
        }

        public void QuickHarvest(int index)
        {
            if(IsHarvesting||IsManaging||Ranch.IsOpen||Hotpot.IsOpen||index<0||index>=Model.State.plots.Count)return;
            if(Online!=null){Online.Command("harvest",SelectedCrop,index,(int)FarmTool.Harvest);return;}
            string id=Model.State.plots[index].cropId;
            if(CropCatalog.Find(id)==null||Model.Progress(index,clock())<1)return;
            int count=Model.HarvestYield(index,1);
            int before=Model.State.harvested;
            FinishHarvest(index,1);
            if(Model.State.harvested>before)Hud.ShowHarvestReward(index,id,count);
            SelectTool(FarmTool.Inspect);
        }

        public void FinishHarvest(int index,int score)
        {
            if(Online!=null){Online.Command("harvest","",index);return;}
            var result=Model.Harvest(index,score,clock());Now=Model.State.lastSeenUtc;
            Notify(result.message,!result.success);
            if(result.success){World.Pulse(index,FarmTool.Harvest);World.MoveFarmerTo(index);Save();}
            Refresh();
        }

        public void ApplyServerState(FarmState state)
        {
            if(Online==null)return;
            Model=new FarmModel(state);Now=Model.ObserveTime(clock());Refresh();
        }

        private void OnApplicationPause(bool paused) { if (ready && paused) Save(); }
        private void OnApplicationFocus(bool focused) { if (ready && !focused) Save(); }
        private void OnApplicationQuit() { if (ready) Save(); }
        public static double UtcNow() { return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; }
    }
}
