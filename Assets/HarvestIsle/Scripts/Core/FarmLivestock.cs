using System;

namespace HarvestIsle.Core
{
    [Serializable]
    public sealed class LivestockState
    {
        public int count;
        public bool producing;
        public double fedAt;
    }
    public sealed class LivestockSpec
    {
        public readonly string id,name,product;
        public readonly int price,feedCost,seconds,sellPrice;
        public LivestockSpec(string id,string name,string product,int price,int feed,int seconds,int sell)
        {this.id=id;this.name=name;this.product=product;this.price=price;feedCost=feed;this.seconds=seconds;sellPrice=sell;}
    }
    public static class LivestockCatalog
    {
        public static readonly LivestockSpec[] All={
            new LivestockSpec("chicken","Gà","Trứng",30,3,60,5),
            new LivestockSpec("cow","Bò","Sữa",75,6,100,12),
            new LivestockSpec("duck","Vịt","Trứng",45,4,80,5),
            new LivestockSpec("pig","Heo","Phân ủ",120,8,150,25),
            new LivestockSpec("bee","Ong","Mật ong",150,0,120,18),
            new LivestockSpec("fish","Cá","Cá tươi",60,5,140,8)};
        public static LivestockSpec Find(string id){foreach(var item in All)if(item.id==id)return item;return null;}
    }
    public sealed partial class FarmModel
    {
        public LivestockState Animal(string id){return id=="chicken"?State.chicken:id=="cow"?State.cow:id=="duck"?State.duck:id=="pig"?State.pig:id=="bee"?State.bee:id=="fish"?State.fish:null;}
        // Flowering plots feed the bees and enable pollination (Hoa and Đậu groups).
        public int FloweringPlots()
        {
            int count=0;
            foreach(PlotState plot in State.plots){string group=CropGroup(plot.cropId);if(group=="Hoa"||group=="Đậu")count++;}
            return count;
        }
        public double AnimalRemaining(string id,double now)
        {
            var spec=LivestockCatalog.Find(id);var animal=Animal(id);if(spec==null||animal==null||!animal.producing)return 0;
            double observed=Math.Max(IsFinite(now)?now:State.lastSeenUtc,State.lastSeenUtc);
            return Math.Max(0,spec.seconds-(observed-animal.fedAt));
        }
        public ActionResult BuyAnimal(string id)
        {
            var spec=LivestockCatalog.Find(id);if(spec==null)return Fail("Loài vật không hợp lệ.");var animal=Animal(id);
            if(animal.count>=3)return Fail("Chuồng đủ 3 con.");
            if(animal.producing)return Fail("Thu sản phẩm của lứa hiện tại trước khi mua thêm.");
            if(State.coins<spec.price)return Fail("Chưa đủ tiền mua "+spec.name+".");
            State.coins-=spec.price;animal.count++;return Ok("Đã mua "+spec.name+". Cho ăn để bắt đầu sản xuất.");
        }
        public ActionResult FeedAnimal(string id,bool useCorn,double now)
        {
            var spec=LivestockCatalog.Find(id);if(spec==null)return Fail("Loài vật không hợp lệ.");var animal=Animal(id);
            if(animal.count==0)return Fail("Chuồng chưa có vật nuôi.");
            if(animal.producing)return Fail("Đã cho ăn. Chờ và thu sản phẩm trước lượt tiếp theo.");
            if(id=="bee")
            {
                // Bees cost nothing to release but need blooms to forage.
                if(FloweringPlots()<2)return Fail("Ong cần ít nhất 2 ô đang trồng hoa hoặc đậu để lấy mật.");
                animal.fedAt=ObserveTime(now);animal.producing=true;
                return Ok("Đàn ong tỏa đi lấy mật · "+spec.seconds+" giây cho "+(animal.count*2)+" mật ong.");
            }
            int cost=spec.feedCost*animal.count;
            if(useCorn){if(Count("corn")<1)return Fail("Cần 1 ngô trong kho cho cả đàn.");}
            else if(State.coins<cost)return Fail("Chưa đủ tiền mua thức ăn. Có thể dùng 1 ngô trong kho.");
            if(useCorn)Entry("corn").count--;else State.coins-=cost;
            animal.fedAt=ObserveTime(now);animal.producing=true;
            return Ok("Đã cho "+spec.name+" ăn · "+spec.seconds+" giây để có "+(animal.count*2)+" "+spec.product+".");
        }
        public ActionResult CollectAnimal(string id,double now)
        {
            var spec=LivestockCatalog.Find(id);if(spec==null)return Fail("Loài vật không hợp lệ.");var animal=Animal(id);
            if(!animal.producing)return Fail("Cho vật nuôi ăn trước khi thu sản phẩm.");
            now=ObserveTime(now);
            if(AnimalRemaining(id,now)>0)return Fail("Còn "+Math.Ceiling(AnimalRemaining(id,now))+" giây.");
            int amount=animal.count*2;
            // Product routing: chicken/duck lay eggs, cows give milk, pigs turn scraps
            // into compost that feeds the soil-quality loop.
            if(id=="pig")
            {
                if(ItemCount("compost")>1000000-amount)return Fail("Kho thành phẩm đầy. Bán phân ủ trước.");
                ChangeItem("compost",amount);
            }
            else if(id=="bee")
            {
                if(State.honey>1000000-amount)return Fail("Kho mật đầy. Bán mật ong trước.");
                State.honey+=amount;
            }
            else if(id=="fish")
            {
                if(State.fishStock>1000000-amount)return Fail("Kho cá đầy. Bán cá trước.");
                State.fishStock+=amount;
            }
            else if(id=="cow")
            {
                if(State.milk>1000000-amount)return Fail("Kho đầy. Bán sản phẩm trước.");
                State.milk+=amount;
            }
            else
            {
                if(State.eggs>1000000-amount)return Fail("Kho đầy. Bán sản phẩm trước.");
                State.eggs+=amount;
            }
            animal.producing=false;animal.fedAt=0;
            return Ok("+"+amount+" "+spec.product+" vào kho. Cho ăn để bắt đầu lượt mới.");
        }
        private static bool ValidLivestock(LivestockState animal,double lastSeen)
        {
            if(animal==null)return true; // Absent fields in older schema-1 saves migrate to empty pens.
            return animal.count>=0&&animal.count<=3&&IsFinite(animal.fedAt)&&animal.fedAt>=0&&animal.fedAt<=lastSeen&&
                (!animal.producing||animal.count>0)&&(animal.producing||animal.fedAt==0);
        }
    }
}
