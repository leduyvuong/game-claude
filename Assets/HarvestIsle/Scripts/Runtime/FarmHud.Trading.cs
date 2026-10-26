using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.UI;
namespace HarvestIsle
{
    public sealed partial class FarmHud
    {
        private sealed class StockRow{public string id;public int stars,count;}
        private readonly Dictionary<string,int> tradeAmounts=new Dictionary<string,int>();
        private readonly List<Text> orderTexts=new List<Text>();
        private readonly List<Button> orderButtons=new List<Button>();
        private int shownOrderSerial;
        private void BuildTrading()
        {
            foreach(Transform child in inventoryPanel.transform)child.gameObject.SetActive(false);
            var root=inventoryPanel.GetComponent<RectTransform>();root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.anchoredPosition=new Vector2(-370,265);root.sizeDelta=new Vector2(740,530);
            Picture(root,"Basket",new Vector2(18,-10),new Vector2(48,48));Label("GIỎ NÔNG SẢN",root,new Vector2(78,-20),new Vector2(520,32),24,ink,FontStyle.Bold);
            Label("Chọn lượng bán · Ghim để giữ hàng cho đơn và bếp",root,new Vector2(22,-66),new Vector2(680,30),17,muted);
            Button("×",root,new Vector2(0,1),new Vector2(682,-12),new Vector2(44,44),paper,ink,()=>{ClosePanels();Refresh();},out _);
            Button("←",root,new Vector2(0,1),new Vector2(22,-471),new Vector2(58,42),paper,ink,()=>{tradePage=Math.Max(0,tradePage-1);tradeSignature="";RefreshTrading();},out _);
            Button("→",root,new Vector2(0,1),new Vector2(660,-471),new Vector2(58,42),paper,ink,()=>{tradePage++;tradeSignature="";RefreshTrading();},out _);
            tradePageLabel=Label("",root,new Vector2(95,-477),new Vector2(540,30),17,ink,FontStyle.Normal,TextAnchor.MiddleCenter);
        }
        private List<StockRow> TradingStock()
        {
            var result=new List<StockRow>();
            foreach(var crop in CropCatalog.All)for(int stars=1;stars<=3;stars++){int count=game.Model.StockCount(crop.id,stars);if(count>0)result.Add(new StockRow{id=crop.id,stars=stars,count=count});}
            foreach(var id in new[]{"eggs","milk","honey","fish"})if(game.Model.ItemCount(id)>0)result.Add(new StockRow{id=id,stars=1,count=game.Model.ItemCount(id)});
            foreach(var r in DevelopmentCatalog.Recipes)if(game.Model.ItemCount(r.id)>0)result.Add(new StockRow{id=r.id,stars=1,count=game.Model.ItemCount(r.id)});
            return result;
        }
        internal static string ProductPicture(string id)
        {
            var c=CropCatalog.Find(id);if(c!=null)return "Product_"+c.modelName;
            switch(id){case "eggs":return "Product_Egg";case "milk":return "Product_Milk";case "fish":return "Product_FishBasket";case "honey":return "Product_Honey";case "flour":return "Product_Flour";case "bread":return "Product_Bread";case "cheese":return "Product_Cheese";case "riceBag":return "Product_RiceBag";case "butter":return "Product_Butter";case "soySauce":return "Product_SoySauce";case "teaLeaves":return "Product_TeaLeaves";case "compost":return "Product_Compost";case "herbal":return "Product_HerbalPotion";case "sandwich":return "Product_Sandwich";case "sugar":return "Product_Sugarcane";case "jam":return "Product_Strawberry";case "cake":return "Product_Bread";case "ricecake":return "Product_RiceBag";case "tofu":return "Product_Peas";case "meatballs":return "Product_FishBasket";default:return "Basket";}
        }
        private void RefreshTrading()
        {
            var stock=TradingStock();int pages=Math.Max(1,(stock.Count+3)/4);tradePage=Mathf.Clamp(tradePage,0,pages-1);
            string signature=tradePage+":"+game.Model.State.coins+":"+FarmModel.FestivalActive(game.Now);
            foreach(var row in stock)signature+="/"+row.id+row.stars+":"+row.count;
            if(signature==tradeSignature)return;tradeSignature=signature;
            foreach(var row in tradeRows){row.SetActive(false);Destroy(row);}tradeRows.Clear();
            tradePageLabel.text=stock.Count==0?"Kho trống · Hãy thu hoạch một vụ cây":"Trang "+(tradePage+1)+" / "+pages+" · "+stock.Count+" loại / phẩm chất";
            for(int n=0;n<4&&tradePage*4+n<stock.Count;n++)
            {
                var item=stock[tradePage*4+n];string key=item.id+":"+item.stars;string pinKey="HarvestIsle.keep."+key;
                bool pinned=PlayerPrefs.GetInt(pinKey,0)==1;int amount=tradeAmounts.ContainsKey(key)?Math.Min(item.count,tradeAmounts[key]):1;tradeAmounts[key]=amount;
                var row=Panel("Stock "+key,inventoryPanel.transform,new Vector2(0,1),new Vector2(18,-108-n*87),new Vector2(704,80),Color.white);tradeRows.Add(row.gameObject);
                Picture(row,ProductPicture(item.id),new Vector2(7,-9),new Vector2(57,57));
                Label(DevelopmentCatalog.ItemName(item.id)+(item.stars>1?" "+new string('★',item.stars):"")+"\nKho: "+item.count,row,new Vector2(74,-12),new Vector2(195,62),16,ink,FontStyle.Bold);
                Button(pinned?"Đã ghim":"Ghim",row,new Vector2(0,1),new Vector2(272,-20),new Vector2(70,40),pinned?gold:paper,ink,()=>{PlayerPrefs.SetInt(pinKey,pinned?0:1);PlayerPrefs.Save();tradeSignature="";RefreshTrading();},out _);
                Button("−",row,new Vector2(0,1),new Vector2(350,-20),new Vector2(42,40),paper,ink,()=>{tradeAmounts[key]=Math.Max(1,amount-1);tradeSignature="";RefreshTrading();},out _);
                var number=Panel("Sale amount",row,new Vector2(0,1),new Vector2(395,-20),new Vector2(42,40),paper);
                var numberText=Label(amount.ToString(),number,new Vector2(1,-8),new Vector2(40,27),16,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
                var input=number.gameObject.AddComponent<InputField>();input.textComponent=numberText;input.targetGraphic=number.GetComponent<Image>();input.contentType=InputField.ContentType.IntegerNumber;input.text=amount.ToString();input.characterLimit=7;
                input.onEndEdit.AddListener(value=>{if(int.TryParse(value,out int requested))tradeAmounts[key]=Mathf.Clamp(requested,1,item.count);tradeSignature="";RefreshTrading();});
                Button("+",row,new Vector2(0,1),new Vector2(440,-20),new Vector2(42,40),paper,ink,()=>{tradeAmounts[key]=Math.Min(item.count,amount+1);tradeSignature="";RefreshTrading();},out _);
                var sell=Button(pinned?"Đang giữ hàng":"Bán "+amount+" · "+FarmModel.Money(game.Model.StockQuote(item.id,item.stars,amount)),row,new Vector2(0,1),new Vector2(492,-12),new Vector2(200,56),accent,paper,()=>{if(game.Online!=null){game.Online.Command("sellStock",item.id,tool:item.stars,job:amount);return;}var result=game.Model.SellStock(item.id,item.stars,amount);game.Notify(result.message,!result.success);if(result.success)game.Save();tradeSignature="";Refresh();},out var sellText);
                sell.interactable=!pinned;sellText.fontSize=16;
            }
        }
        private void BuildOrders()
        {
            var root=Panel("Orders",safeArea,new Vector2(.5f,.5f),new Vector2(-430,230),new Vector2(860,460),paper);orderPanel=root.gameObject;
            Label("BẢNG ĐƠN HÀNG",root,new Vector2(24,-20),new Vector2(700,32),25,ink,FontStyle.Bold);
            Label("Chọn một đơn · Bảng đổi sau mỗi lần giao thành công",root,new Vector2(24,-63),new Vector2(780,32),17,muted);
            Button("×",root,new Vector2(0,1),new Vector2(800,-10),new Vector2(44,44),paper,ink,()=>{ClosePanels();Refresh();},out _);
            string[] portraits={"MerchantHaiAu","FarmerFemale","Farmer"};
            for(int i=0;i<3;i++)
            {
                int slot=i;var card=Panel("Order "+i,root,new Vector2(0,1),new Vector2(20+i*280,-108),new Vector2(260,275),Color.white);
                Picture(card,portraits[i],new Vector2(97,-10),new Vector2(66,66));
                var text=Label("",card,new Vector2(12,-86),new Vector2(236,124),18,ink,FontStyle.Bold,TextAnchor.MiddleCenter);orderTexts.Add(text);
                var b=Button("Giao đơn",card,new Vector2(0,1),new Vector2(12,-221),new Vector2(236,42),accent,paper,()=>{if(game.Online!=null){game.Online.Command("choiceOrder",index:slot,job:shownOrderSerial);return;}var r=game.Model.FulfillChoiceOrder(slot,shownOrderSerial);game.Notify(r.message,!r.success);if(r.success)game.Save();Refresh();},out _);orderButtons.Add(b);
            }
            Button("Nhật ký & quà ngày →",root,new Vector2(0,1),new Vector2(242,-402),new Vector2(376,40),gold,ink,()=>{ClosePanels();game.Development.Open(0);},out _);
            orderPanel.SetActive(false);
        }
        public void OpenOrders(){ClosePanels();orderPanel.SetActive(true);RefreshOrders();}
        private void RefreshOrders()
        {
            shownOrderSerial=game.Model.State.development.ordersCompleted;string[] names={"Hải Âu","Cô hàng xóm","Bếp làng"};
            for(int i=0;i<3;i++)
            {
                string id=game.Model.ChoiceOrderItem(i);int quantity=game.Model.ChoiceOrderQuantity(i);
                orderTexts[i].text=names[i]+"\n"+quantity+" "+DevelopmentCatalog.ItemName(id)+" · Có "+game.Model.ItemCount(id)+"\n+"+FarmModel.Money(game.Model.ChoiceOrderReward(i))+"\n3 gỗ · 1 quặng · 20 XP";
                orderButtons[i].interactable=game.Model.ItemCount(id)>=quantity;
            }
        }
    }
}
