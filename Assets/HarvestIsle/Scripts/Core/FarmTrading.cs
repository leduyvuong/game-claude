using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    public sealed partial class FarmModel
    {
        public int StockCount(string id, int stars)
        {
            if(stars<1||stars>3)return 0;
            if(CropCatalog.Find(id)!=null)return stars==1?Count(id):stars==2?CountFine(id):CountPrime(id);
            return stars==1?ItemCount(id):0;
        }
        public int StockPrice(string id,int stars)
        {
            if(stars<1||stars>3)return 0;
            var crop=CropCatalog.Find(id);
            if(crop!=null)return stars==1?crop.sellPrice:StarUnitPrice(crop.sellPrice,stars);
            if(stars!=1)return 0;
            if(id=="eggs")return 5;if(id=="milk")return 12;if(id=="honey")return 18;if(id=="fish")return 8;
            var recipe=DevelopmentCatalog.Recipe(id);return recipe==null?0:recipe.price;
        }
        public int StockQuote(string id,int stars,int quantity)
        {
            if(quantity<=0||StockPrice(id,stars)==0)return 0;
            long value=(long)StockPrice(id,stars)*quantity;
            if(DevelopmentCatalog.Recipe(id)==null&&FestivalActive(State.lastSeenUtc))value=value*12/10;
            return (int)Math.Min(1000000000L,value);
        }
        public ActionResult SellStock(string id,int stars,int quantity)
        {
            if(quantity<=0||StockPrice(id,stars)==0||StockCount(id,stars)<quantity)return Fail("Số lượng bán không hợp lệ hoặc kho không đủ.");
            int value=StockQuote(id,stars,quantity);
            if((long)State.coins+value>1000000000)return Fail("Kho tiền đã đầy.");
            if(CropCatalog.Find(id)!=null&&stars>1)Bucket(stars==2?State.fineGoods:State.primeGoods,id).count-=quantity;
            else ChangeItem(id,-quantity);
            State.coins+=value;State.hasSold=true;
            return Ok("Đã bán "+quantity+" "+DevelopmentCatalog.ItemName(id)+": +"+Money(value)+".");
        }
        public string ChoiceOrderItem(int slot)
        {
            if(slot==0)return OrderItem;
            var choices=new List<string>();
            foreach(var c in CropCatalog.All)if(c.unlockLevel<=Level&&IslandCropUnlocked(c.id)&&c.id!=OrderItem)choices.Add(c.id);
            foreach(var r in DevelopmentCatalog.Recipes)if(r.id!=OrderItem&&JourneyIngredientAvailable(r.id,0))choices.Add(r.id);
            return choices[(State.development.ordersCompleted+Math.Max(1,slot)-1)%choices.Count];
        }
        public int ChoiceOrderQuantity(int slot){return slot==0?OrderQuantity:DevelopmentCatalog.Recipe(ChoiceOrderItem(slot))!=null?1:slot==1?4:6;}
        public int ChoiceOrderReward(int slot){return slot==0?OrderReward:ChoiceOrderQuantity(slot)*StockPrice(ChoiceOrderItem(slot),1)+12+slot*4;}
        public ActionResult FulfillChoiceOrder(int slot,int serial)
        {
            if(slot<0||slot>2||serial!=State.development.ordersCompleted)return Fail("Bảng đơn đã đổi. Hãy chọn đơn mới.");
            if(slot==0)return FulfillOrder();
            string id=ChoiceOrderItem(slot);int quantity=ChoiceOrderQuantity(slot),reward=ChoiceOrderReward(slot);
            if(ItemCount(id)<quantity)return Fail("Chưa đủ hàng giao đơn.");
            if((long)State.coins+reward>1000000000||serial>=1000000000)return Fail("Đã đạt giới hạn.");
            ChangeItem(id,-quantity);State.coins+=reward;State.development.ordersCompleted++;
            State.development.wood=Math.Min(1000000,State.development.wood+3);State.development.ore=Math.Min(1000000,State.development.ore+1);
            GainExperience(20);return Ok("Giao đơn thành công: +"+Money(reward)+", 3 gỗ, 1 quặng, 20 XP.");
        }
    }
}
