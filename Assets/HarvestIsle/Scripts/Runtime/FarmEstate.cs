using System;
using System.IO;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HarvestIsle
{
    // Authored, persistent farm. The ordinary player profile is never used here.
    public sealed class FarmEstate : MonoBehaviour
    {
        public const string ScenePath = "Assets/HarvestIsle/Scenes/AnNhienFarm.unity";
        public FarmGame Game { get; private set; }
        public bool Ready { get; private set; }
        public string SaveDirectory { get; set; }
        private FarmGame visitor;
        private GameObject navigation;
        private Light previousSun;
        private Color previousAmbient;

        private void Start()
        {
            double now = FarmGame.UtcNow();
            string directory = SaveDirectory ?? Path.Combine(Application.persistentDataPath, "AnNhienFarm");
            var store = new FarmSaveStore(directory);
            var state = store.Load(now, CreateState(now));
            Game = gameObject.AddComponent<FarmGame>();
            Game.InitializeSession(store, FarmGame.UtcNow, state, estate:true);
            var day = gameObject.AddComponent<FarmDayNight>(); day.Configure(Game); day.SetPhase(.42f);
            navigation = Game.Hud.AddEstateNavigation(ShowZone);
            Game.CameraController.ConfigureMap(new Vector2(-42,-36), new Vector2(42,36), 65);
            ShowZone(0);
            Game.Notify("Chào mừng đến An Nhiên · chọn một khu để ghé thăm, chạm ruộng để chăm cây.");
            Game.Save(); Ready = true;
        }

        private void Update()
        {
            if (navigation != null) navigation.SetActive(!Game.Hud.IsReading && !Game.IsManaging && !Game.IsHarvesting && !Game.Ranch.IsOpen && !Game.Hotpot.IsOpen);
        }
        public void ShowZone(int index)
        {
            Game.Hud.ClosePanels(); Game.ClearSelection();
            Game.Hud.SetEstateZone(index);
            Vector3[] centers = {new Vector3(0,0,-1),new Vector3(0,0,-5.5f),new Vector3(-23,0,-5),new Vector3(23,0,-13),new Vector3(22,0,15)};
            Game.CameraController.SetView(centers[Mathf.Clamp(index,0,4)],index==0?38:index==1?15:11);
        }
        public static Vector3 PenPosition(int i) { return new Vector3(-27+(i%2)*8,.15f,3-(i/2)*8); }
        public static Vector3 BuildingPosition(int i) { return new Vector3(17+(i%3)*6.5f,.15f,-5-(i/3)*8); }
        public static Vector3 PlotPosition(int index)
        {
            int row=index/FarmModel.Width, col=index%FarmModel.Width;
            return new Vector3((col-2.5f)*2.6f+(col/2-1)*2, .245f, 3-row*2.6f-(row/2)*2);
        }

        public static FarmState CreateState(double now)
        {
            var model = new FarmModel(FarmModel.NewState(now)); var s=model.State;
            s.coins=50000; s.harvested=60; s.hasSold=true; s.hasWatered=true;
            s.development.experience=400; s.development.ordersCompleted=10; s.development.crafted=20;
            s.development.wood=3000; s.development.ore=3000;
            Require(model.UnlockParcel()); Require(model.UnlockParcel());
            foreach(var spec in DevelopmentCatalog.Buildings)for(int level=0;level<2;level++)Require(model.UpgradeBuilding(spec.id));
            string[] crops={"rice","carrot","corn","tomato","pumpkin","strawberry","marigold","snowtea","orange"};
            for(int i=0;i<s.plots.Count;i++)
            {
                int bed=(i/FarmModel.Width/2)*3+(i%FarmModel.Width/2);
                var crop=CropCatalog.Find(crops[bed]); Require(model.Apply(i,FarmTool.Plant,crop.id,now));
                s.plots[i].plantedAt=now-crop.growSeconds*(i%3==0?1.1:.65);
                s.plots[i].watered=true; s.plots[i].soilWater=.25; s.plots[i].soilQuality=2; s.plots[i].drainageBuilt=true;
            }
            foreach(var spec in LivestockCatalog.All)for(int i=0;i<2;i++)Require(model.BuyAnimal(spec.id));
            foreach(var item in s.inventory)item.count=20;
            s.coins=2500; s.development.wood=120; s.development.ore=80;
            string reason; if(!FarmModel.Validate(s,out reason))throw new InvalidOperationException(reason);
            return s;
        }
        private static void Require(ActionResult result)
        { if(!result.success)throw new InvalidOperationException("An Nhien default: "+result.message); }

        public static void Travel(FarmGame current)
        {
            current.Hud.ClosePanels();
            if(current.IsEstate) { current.GetComponent<FarmEstate>().Return(); return; }
            if(current.Online!=null) { current.Notify("Về chế độ solo để ghé An Nhiên."); return; }
            current.Save();
            var root=new GameObject("Nông trại An Nhiên");
            var estate=root.AddComponent<FarmEstate>();
            estate.visitor=current; estate.previousSun=RenderSettings.sun; estate.previousAmbient=RenderSettings.ambientLight;
            current.gameObject.SetActive(false);
        }
        public void Return()
        {
            Game.Save();
            if(visitor!=null)
            {
                gameObject.SetActive(false); visitor.gameObject.SetActive(true);
                RenderSettings.sun=previousSun; RenderSettings.ambientLight=previousAmbient;
                Destroy(gameObject);
            }
            else SceneManager.LoadScene("HarvestIsle");
        }
    }
}
