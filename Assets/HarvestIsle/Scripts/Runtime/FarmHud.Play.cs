using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HarvestIsle
{
    public sealed partial class FarmHud
    {
        private readonly Dictionary<string,Texture2D> playImages=new Dictionary<string,Texture2D>();
        private RectTransform seedContent, compactCard;
        private RawImage contextIcon,weatherIcon;
        private Text nextGoal,xpLabel;
        private Image xpRing;
        private GameObject sunIcon;
        private bool details;
        private GameObject detailExtra, orderPanel;
        private readonly List<GameObject> tradeRows=new List<GameObject>();
        private int tradePage;
        private Text tradePageLabel;
        private string tradeSignature="";
        private AudioSource rewardAudio;
        private AudioClip rewardClip;
        private readonly List<GameObject> flyingRewards=new List<GameObject>();
        public bool IsReading => inventoryPanel.activeSelf||menuPanel.activeSelf||helpPanel.activeSelf||seedPanel.activeSelf||(orderPanel!=null&&orderPanel.activeSelf);
        public bool BlocksWorld => inventoryPanel.activeSelf||(orderPanel!=null&&orderPanel.activeSelf);
        public void SetCareExpanded(bool value){details=value;Refresh();PositionPlayInterface();}
        public void TapPlot(int index)
        {
            if(game.Tool==FarmTool.Plant&&string.IsNullOrEmpty(game.Model.State.plots[index].cropId)){game.ActOnPlot(index);return;}
            game.SelectTool(FarmTool.Inspect);game.ActOnPlot(index);
            if(string.IsNullOrEmpty(game.Model.State.plots[index].cropId))Toggle(seedPanel);
        }
        public void OpenTrading(){ClosePanels();inventoryPanel.SetActive(true);tradeSignature="";Refresh();}
        private Texture2D PlayImage(string name)
        {
            if(!playImages.TryGetValue(name,out var texture)){texture=Resources.Load<Texture2D>(name.Contains("IX_")?"IslandExpansion/Thumbnails/"+name.Replace("IX_",""):"ModelThumbnails/"+name);playImages[name]=texture;}
            return texture;
        }
        private RawImage Picture(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var r=Rect("Illustration",parent,new Vector2(0,1),new Vector2(0,1),position,size);
            var image=r.gameObject.AddComponent<RawImage>();image.texture=PlayImage(name);image.raycastTarget=false;
            return image;
        }
        private void MoveRect(Component item,float x,float y,float width,float height)
        {var r=item.GetComponent<RectTransform>();r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(width,height);}
        private void BuildPlayInterface()
        {
            Button("Gieo kéo",safeArea,new Vector2(.5f,0),new Vector2(-214,68),new Vector2(138,48),paper,ink,()=>game.Batch.SetMode(1),out _).name="Batch sow";
            Button("Thu hàng loạt",safeArea,new Vector2(.5f,0),new Vector2(-69,68),new Vector2(138,48),gold,ink,()=>game.Batch.SetMode(2),out _).name="Batch harvest";
            Button("Di chuyển",safeArea,new Vector2(.5f,0),new Vector2(76,68),new Vector2(138,48),paper,ink,()=>game.Batch.SetMode(0),out _).name="Batch move";
            safeArea.Find("Tools").gameObject.SetActive(false);
            // Existing resource illustrations, no duplicate sprite assets.
            var seedButton=Button("Giống cây",safeArea,new Vector2(0,0),new Vector2(22,111),new Vector2(96,94),paper,ink,()=>Toggle(seedPanel),out var seedText);
            Picture(seedButton.transform,"SeedBag",new Vector2(22,-4),new Vector2(52,52));MoveRect(seedText,4,-58,88,28);
            var home=Button("Về ruộng",safeArea,new Vector2(1,0),new Vector2(-118,111),new Vector2(96,94),paper,ink,()=>{game.Batch.SetMode(0);ClosePanels();game.ClearSelection();game.SelectTool(FarmTool.Inspect);game.CameraController.GoHome();},out var homeText);
            Picture(home.transform,"Farmer",new Vector2(22,-4),new Vector2(52,52));MoveRect(homeText,4,-58,88,28);
            var stock=safeArea.Find("Kho");if(stock!=null){Picture(stock,"Basket",new Vector2(20,-2),new Vector2(44,38));MoveRect(stock.GetComponentInChildren<Text>(),3,-37,78,21);}
            xpRing=Circle("XP",safeArea,new Vector2(13,-9),72,accent).GetComponent<Image>();xpRing.type=Image.Type.Filled;xpRing.fillMethod=Image.FillMethod.Radial360;xpRing.fillOrigin=2;xpRing.transform.SetAsFirstSibling();
            var weather=safeArea.Find("Weather");weatherIcon=Picture(weather,"RainCloud",new Vector2(5,-8),new Vector2(35,35));MoveRect(weatherTitle,42,-4,103,44);
            sunIcon=Rect("Sun",weather,new Vector2(0,1),new Vector2(0,1),new Vector2(7,-9),new Vector2(32,32)).gameObject;
            Circle("Sun center",sunIcon.transform,new Vector2(8,-8),16,gold);
            for(int ray=0;ray<8;ray++){float angle=ray*Mathf.PI/4;var r=Rect("Sun ray",sunIcon.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(Mathf.Cos(angle)*13,Mathf.Sin(angle)*13),new Vector2(5,3));r.pivot=Vector2.one*.5f;r.localRotation=Quaternion.Euler(0,0,ray*45);var img=r.gameObject.AddComponent<Image>();img.color=gold;img.raycastTarget=false;}
            Button("Âm thanh",menuPanel.transform,new Vector2(0,1),new Vector2(18,-492),new Vector2(268,44),paper,ink,()=>{bool mute=PlayerPrefs.GetInt("HarvestIsle.muteFx",0)==0;PlayerPrefs.SetInt("HarvestIsle.muteFx",mute?1:0);game.Notify(mute?"Đã tắt âm thanh thu hoạch":"Đã bật âm thanh thu hoạch");},out _);
            Button("Đảo trăm nghề",menuPanel.transform,new Vector2(0,1),new Vector2(18,-542),new Vector2(268,44),gold,ink,()=>{ClosePanels();game.IslandExpansion.Open();},out _);
            xpLabel=Label("",safeArea,new Vector2(18,-80),new Vector2(74,24),12,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
            var goal=Button("",safeArea,new Vector2(0,1),new Vector2(20,-116),new Vector2(238,100),paper,ink,FollowGoal,out nextGoal);
            goal.name="Order board";Picture(goal.transform,"MerchantHaiAu",new Vector2(9,-12),new Vector2(57,68));MoveRect(nextGoal,72,-12,156,76);nextGoal.fontSize=16;nextGoal.alignment=TextAnchor.MiddleLeft;
            foreach(Transform child in seedPanel.transform)child.gameObject.SetActive(false);
            MoveRect(seedPanel.transform,-430,245,860,220);
            Label("GIỐNG CÂY  ·  Kéo để chọn",seedPanel.transform,new Vector2(18,-13),new Vector2(660,28),18,ink,FontStyle.Bold);
            Button("×",seedPanel.transform,new Vector2(0,1),new Vector2(800,-6),new Vector2(48,42),paper,ink,()=>{ClosePanels();Refresh();},out _);
            var viewport=Panel("Seed viewport",seedPanel.transform,new Vector2(0,1),new Vector2(12,-50),new Vector2(836,158),paper);viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=true;scroll.vertical=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=viewport;
            seedContent=Rect("Seed strip",viewport,new Vector2(0,1),new Vector2(0,1),Vector2.zero,new Vector2(CropCatalog.All.Length*154,156));scroll.content=seedContent;
            cropCards=new Image[CropCatalog.All.Length];cropLabels=new Text[cropCards.Length];
            for(int i=0;i<cropCards.Length;i++)
            {
                int index=i;var c=CropCatalog.All[i];
                var button=Button(c.displayName,seedContent,new Vector2(0,1),new Vector2(i*154,0),new Vector2(146,153),paper,ink,()=>ChooseSeed(index),out cropLabels[i]);cropCards[i]=button.GetComponent<Image>();
                Picture(button.transform,"Product_"+c.modelName,new Vector2(41,-7),new Vector2(64,64));MoveRect(cropLabels[i],5,-76,136,69);cropLabels[i].fontSize=15;
            }
            compactCard=detailPanel.GetComponent<RectTransform>();compactCard.sizeDelta=new Vector2(304,186);
            MoveRect(plotTitle,68,-12,174,26);plotTitle.fontSize=16;contextIcon=Picture(compactCard,"SeedBag",new Vector2(10,-10),new Vector2(52,52));
            MoveRect(plotBody,68,-44,219,47);plotBody.fontSize=16;
            MoveRect(progress.transform.parent,17,-99,270,7);MoveRect(plotAction,16,-116,272,46);
            detailExtra=Button("Chi tiết & chăm sóc",compactCard,new Vector2(0,1),new Vector2(15,-168),new Vector2(274,36),paper,ink,()=>{details=!details;Refresh();},out _).gameObject;
            MoveRect(floodStatus,17,-212,270,46);MoveRect(drainButton,17,-264,130,48);MoveRect(treatButton,157,-264,130,48);MoveRect(buildDrainButton,17,-321,270,48);MoveRect(pestButton,17,-378,270,42);
            var bonus=Button("Chơi thu hoạch thưởng",compactCard,new Vector2(0,1),new Vector2(17,-428),new Vector2(270,40),gold,ink,()=>{if(game.SelectedPlot>=0){game.SelectTool(FarmTool.Harvest);game.ActOnPlot(game.SelectedPlot);}},out _);bonus.name="Bonus harvest";
            BuildTrading();BuildOrders();BuildWorldBadges();
            rewardAudio=gameObject.AddComponent<AudioSource>();rewardAudio.playOnAwake=false;rewardAudio.volume=.18f;
            var samples=new float[8000];for(int n=0;n<samples.Length;n++){float t=n/16000f;samples[n]=Mathf.Sin(t*2*Mathf.PI*(t<.18f?660:880))*.2f*(1-n/(float)samples.Length);}
            rewardClip=AudioClip.Create("Harvest chime",samples.Length,1,16000,false);rewardClip.SetData(samples,0);
        }
        private void ChooseSeed(int index)
        {
            var c=CropCatalog.All[index];if(game.Model.Level<c.unlockLevel||!game.Model.IslandCropUnlocked(c.id)||game.Online!=null&&IslandCropCatalog.Index(c.id)>=0)return;
            int plot=game.SelectedPlot;ClosePanels();game.SelectCrop(c.id);
            if(plot>=0&&string.IsNullOrEmpty(game.Model.State.plots[plot].cropId))game.ActOnPlot(plot);
        }
        private void RefreshPlayInterface()
        {
            if(xpRing==null)return;
            xpLabel.text=game.Model.Level>=10?"Cấp 10":(game.Model.State.development.experience%40)+"/40 XP";
            weatherIcon.gameObject.SetActive(game.Model.RainLevel>0);sunIcon.SetActive(game.Model.RainLevel==0);
            if(game.Model.State.harvested<5)weatherTitle.text="Bình yên\n"+(5-game.Model.State.harvested)+" vụ đầu";
            string id=game.Model.OrderItem;nextGoal.text="HẢI ÂU CẦN\n"+game.Model.OrderQuantity+" "+DevelopmentCatalog.ItemName(id)+"\n"+Math.Min(game.Model.OrderQuantity,game.Model.ItemCount(id))+"/"+game.Model.OrderQuantity+"  ·  Xem đơn →";
            if(!game.Model.State.hasSold&&game.Model.State.harvested==0)nextGoal.text="VỤ MÙA ĐẦU\n"+(game.Model.State.plots.Exists(p=>!string.IsNullOrEmpty(p.cropId))?"Chăm cây vừa gieo":"Chọn giống để gieo")+"\nChạm để tiếp tục →";
            else if(!game.Model.State.hasSold&&game.Model.SellValue()>0)nextGoal.text="BÁN VỤ ĐẦU\nMở giỏ, chọn hàng\nGiữ lại hàng làm đơn";
            for(int i=0;i<cropLabels.Length;i++)
            {
                var c=CropCatalog.All[i];bool unlocked=game.Model.Level>=c.unlockLevel&&game.Model.IslandCropUnlocked(c.id)&&(game.Online==null||IslandCropCatalog.Index(c.id)<0);
                cropLabels[i].text=c.displayName+"\n"+(unlocked?FarmModel.Money(c.seedCost)+" · "+c.growSeconds+"s":IslandCropCatalog.Index(c.id)>=0?"M"+IslandCropCatalog.Chapter(c.id)+" · Đảo trăm nghề":"Mở cấp "+c.unlockLevel);
                cropCards[i].GetComponent<Button>().interactable=unlocked;
            }
            if(inventoryPanel.activeSelf)RefreshTrading();
            if(orderPanel.activeSelf)RefreshOrders();
            RefreshWorldBadges();
        }
        private void PositionPlayInterface()
        {
            if(compactCard==null)return;
            if(game.Batch!=null){string[] names={"Batch move","Batch sow","Batch harvest"};for(int i=0;i<3;i++)safeArea.Find(names[i]).GetComponent<Image>().color=game.Batch.Mode==i?gold:paper;}
            xpRing.fillAmount=Mathf.MoveTowards(xpRing.fillAmount,game.Model.Level>=10?1:(game.Model.State.development.experience%40)/40f,Time.unscaledDeltaTime*2);
            if(game.SelectedPlot>=0&&detailPanel.activeSelf)
            {
                var state=game.Model.State.plots[game.SelectedPlot];var crop=CropCatalog.Find(state.cropId);bool ready=crop!=null&&game.Model.Progress(game.SelectedPlot,game.Now)>=1;
                compactCard.sizeDelta=new Vector2(304,details?480:212);
                foreach(var c in new Component[]{floodStatus,drainButton,treatButton,buildDrainButton})c.gameObject.SetActive(details);
                pestButton.gameObject.SetActive(details&&state.pest>0&&game.Online==null);
                var bonus=compactCard.Find("Bonus harvest");bonus.gameObject.SetActive(details&&ready&&game.Online==null);
                contextIcon.texture=PlayImage(crop==null?"SeedBag":ready?"Sickle":"WateringCan");
                plotBody.text=crop==null?"Đất trống · chọn giống để gieo":ready?"Đã chín · thu ngay vào giỏ":"Còn "+Math.Ceiling(game.Model.RemainingSeconds(game.SelectedPlot,game.Now))+" giây"+(state.watered?" · Đủ nước":" · Cần tưới");
                if(crop==null){plotActionLabel.text="Chọn giống cây";plotAction.interactable=true;}
                else if(ready)plotActionLabel.text="Thu ngay · +"+game.Model.HarvestYield(game.SelectedPlot,1);
                var point=game.World.GameCamera.WorldToScreenPoint(game.World.PositionOfPlot(game.SelectedPlot));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(safeArea,point,null,out var local);
                local+=new Vector2(-safeArea.rect.width/2,safeArea.rect.height/2);
                compactCard.anchorMin=compactCard.anchorMax=new Vector2(.5f,.5f);
                compactCard.anchoredPosition=new Vector2(Mathf.Clamp(local.x+55,-safeArea.rect.width/2+12,safeArea.rect.width/2-316),Mathf.Clamp(local.y+100,-safeArea.rect.height/2+compactCard.sizeDelta.y+12,safeArea.rect.height/2-104));
            }
            PositionWorldBadges();
        }
        public void ShowHarvestReward(int index,string id,int count){StartCoroutine(FlyReward(index,id,count));}
        private void FollowGoal()
        {
            if(game.Model.State.harvested==0)
            {
                int index=game.Model.State.plots.FindIndex(p=>!string.IsNullOrEmpty(p.cropId));
                if(index<0)Toggle(seedPanel);else {ClosePanels();game.CameraController.FocusOn(game.World.PositionOfPlot(index));game.SelectTool(FarmTool.Inspect);game.ActOnPlot(index);}
            }
            else if(!game.Model.State.hasSold&&game.Model.SellValue()>0)OpenTrading();else OpenOrders();
        }
        private System.Collections.IEnumerator FlyReward(int index,string id,int count)
        {
            if(rewardAudio!=null&&PlayerPrefs.GetInt("HarvestIsle.muteFx",0)==0)rewardAudio.PlayOneShot(rewardClip);
            var screen=game.World.GameCamera.WorldToScreenPoint(game.World.PositionOfPlot(index));RectTransformUtility.ScreenPointToLocalPointInRectangle(safeArea,screen,null,out var start);
            start+=new Vector2(-safeArea.rect.width/2,safeArea.rect.height/2);
            var r=Panel("Harvest reward",safeArea,new Vector2(.5f,.5f),start,new Vector2(98,86),paper);r.GetComponent<Image>().raycastTarget=false;
            flyingRewards.Add(r.gameObject);
            Picture(r,"Product_"+CropCatalog.Find(id).modelName,new Vector2(23,-3),new Vector2(52,52));Label("+"+count,r,new Vector2(0,-53),new Vector2(98,29),22,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
            var end=new Vector2(safeArea.rect.width/2-180,safeArea.rect.height/2-30);
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/1.1f){if(r==null)yield break;r.anchoredPosition=Vector2.Lerp(start,end,t*t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*65;r.localScale=Vector3.one*(1-.4f*t);yield return null;}
            flyingRewards.Remove(r.gameObject);Destroy(r.gameObject);
        }
        private void OnDisable(){foreach(var item in flyingRewards)if(item!=null)Destroy(item);flyingRewards.Clear();if(rewardAudio!=null)rewardAudio.Stop();}
        private void DisposePlayInterface(){if(rewardClip!=null)Destroy(rewardClip);}
    }
}
