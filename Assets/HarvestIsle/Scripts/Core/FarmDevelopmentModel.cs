using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    [Serializable] public sealed class BuildingState { public string id; public int level; }
    [Serializable] public sealed class ProductStock { public string id; public int count; }
    [Serializable] public sealed class ProductionJob { public int id; public string recipe; public double readyAt; }
    [Serializable] public sealed class SeedlingState { public string cropId; public double readyAt; }
    [Serializable] public sealed class DevelopmentState
    {
        public int experience,wood=6,ore=3,ordersCompleted,crafted,chapter,nextJobId=1;
        public int lastLoginDay,loginStreak;
        public List<BuildingState> buildings=new List<BuildingState>();
        public List<ProductStock> products=new List<ProductStock>();
        public List<ProductionJob> jobs=new List<ProductionJob>();
        public List<SeedlingState> seedlings=new List<SeedlingState>();
    }
    public sealed class BuildingSpec
    {
        public readonly string id,name,model;public readonly int price,level;
        public BuildingSpec(string id,string name,string model,int price,int level)
        {this.id=id;this.name=name;this.model=model;this.price=price;this.level=level;}
    }
    public sealed class RecipeSpec
    {
        public readonly string id,name,building,first,second;public readonly int firstCount,secondCount,seconds,price;
        public RecipeSpec(string id,string name,string building,string first,int count,string second,int secondCount,int seconds,int price)
        {this.id=id;this.name=name;this.building=building;this.first=first;firstCount=count;this.second=second;this.secondCount=secondCount;this.seconds=seconds;this.price=price;}
    }
    public static class DevelopmentCatalog
    {
        public static readonly BuildingSpec[] Buildings={
            new BuildingSpec("mill","Cối xay","Windmill",35,2),
            new BuildingSpec("bakery","Lò bánh","BakeryOven",55,2),
            new BuildingSpec("kitchen","Nồi chế biến","SoyVat",65,3),
            new BuildingSpec("dairy","Xưởng sữa","DairyChurn",60,3),
            new BuildingSpec("preserves","Xưởng trái cây","JuicePress",65,4),
            new BuildingSpec("herbs","Bàn thảo dược","HerbalBench",50,5),
            new BuildingSpec("compost","Thùng ủ phân","CompostBin",30,2),
            new BuildingSpec("nursery","Vườn ươm","Nursery",70,5),
            new BuildingSpec("greenhouse","Nhà kính","Greenhouse",100,6)
        };
        public static readonly RecipeSpec[] Recipes={
            new RecipeSpec("flour","Bột mì","mill","wheat",3,"",0,45,25),
            new RecipeSpec("sugar","Đường","mill","sugarcane",3,"",0,55,42),
            new RecipeSpec("bread","Bánh mì","bakery","flour",2,"",0,65,65),
            new RecipeSpec("sandwich","Bánh kẹp","kitchen","bread",1,"tomato",2,80,100),
            new RecipeSpec("cheese","Phô mai","dairy","milk",3,"",0,75,50),
            new RecipeSpec("cake","Bánh trứng","bakery","flour",1,"eggs",2,90,55),
            new RecipeSpec("jam","Mứt dâu","preserves","strawberry",3,"sugar",1,100,100),
            new RecipeSpec("ricecake","Bánh gạo","kitchen","rice",4,"sugar",1,90,80),
            new RecipeSpec("herbal","Thuốc thảo mộc","herbs","garlic",2,"chili",2,60,40),
            new RecipeSpec("compost","Phân ủ","compost","greens",3,"corn",1,50,25),
            new RecipeSpec("butter","Bơ","dairy","milk",2,"",0,65,35),
            new RecipeSpec("soySauce","Tương","kitchen","peas",3,"wheat",1,85,40),
            new RecipeSpec("riceBag","Bao gạo","mill","rice",4,"",0,50,32),
            new RecipeSpec("teaLeaves","Trà thành phẩm","herbs","snowtea",2,"",0,80,50),
            new RecipeSpec("tofu","Đậu phụ","kitchen","peas",3,"",0,65,30),
            new RecipeSpec("meatballs","Viên cá thả lẩu","kitchen","flour",1,"fish",2,75,48)
        };
        public static BuildingSpec Building(string id){foreach(var b in Buildings)if(b.id==id)return b;return null;}
        public static RecipeSpec Recipe(string id){foreach(var r in Recipes)if(r.id==id)return r;return null;}
        public static string ItemName(string id)
        {
            var c=CropCatalog.Find(id);if(c!=null)return c.displayName;
            var r=Recipe(id);if(r!=null)return r.name;
            return id=="eggs"?"Trứng":id=="milk"?"Sữa":id=="fish"?"Cá tươi":id=="honey"?"Mật ong":id;
        }
    }
    public sealed partial class FarmModel
    {
        public int Level {get{return Math.Min(10,1+State.development.experience/40);}}
        public void GainExperience(int amount){State.development.experience=(int)Math.Min(1000000000L,(long)State.development.experience+Math.Max(0,amount));}
        public int BuildingLevel(string id){var b=State.development.buildings.Find(x=>x.id==id);return b==null?0:b.level;}
        public int ItemCount(string id)
        {
            if(IslandExpansionCatalog.Item(id)!=null){var good=Island.goods.Find(p=>p.id==id);return good==null?0:good.count;}
            if(id=="eggs")return State.eggs;if(id=="milk")return State.milk;if(id=="fish")return State.fishStock;if(id=="honey")return State.honey;
            if(CropCatalog.Find(id)!=null)return Count(id);
            var stock=State.development.products.Find(p=>p.id==id);return stock==null?0:stock.count;
        }
        private void ChangeItem(string id,int amount)
        {
            if(IslandExpansionCatalog.Item(id)!=null){ChangeIslandItem(id,amount);return;}
            if(id=="eggs"){State.eggs+=amount;return;}if(id=="milk"){State.milk+=amount;return;}
            if(id=="fish"){State.fishStock+=amount;return;}if(id=="honey"){State.honey+=amount;return;}
            if(CropCatalog.Find(id)!=null){Entry(id).count+=amount;return;}
            var stock=State.development.products.Find(p=>p.id==id);
            if(stock==null){stock=new ProductStock{id=id};State.development.products.Add(stock);}stock.count+=amount;
        }
        public ActionResult UpgradeBuilding(string id)
        {
            var spec=DevelopmentCatalog.Building(id);if(spec==null)return Fail("Công trình không hợp lệ.");
            int level=BuildingLevel(id);if(level>=3)return Fail("Đã đạt cấp tối đa.");
            if(Level<spec.level)return Fail("Mở công trình ở cấp "+spec.level+".");
            int cost=spec.price*(level+1),wood=3*(level+1),ore=level*2;
            if(State.coins<cost||State.development.wood<wood||State.development.ore<ore)return Fail("Cần "+Money(cost)+", "+wood+" gỗ và "+ore+" quặng. Nhận tài nguyên từ đơn hàng.");
            State.coins-=cost;State.development.wood-=wood;State.development.ore-=ore;
            var b=State.development.buildings.Find(x=>x.id==id);
            if(b==null){b=new BuildingState{id=id};State.development.buildings.Add(b);}b.level++;
            GainExperience(10);return Ok(spec.name+" cấp "+b.level+(id=="greenhouse"?" · che mưa cho "+b.level*4+" ô.":id=="nursery"?" · có "+b.level*2+" khay ươm.":" · thêm chỗ hàng chờ và tăng tốc chế biến."));
        }
        public ActionResult StartProduction(string recipe,double now)
        {
            var r=DevelopmentCatalog.Recipe(recipe);if(r==null)return Fail("Công thức không hợp lệ.");
            int level=BuildingLevel(r.building);if(level==0)return Fail("Xây "+DevelopmentCatalog.Building(r.building).name+" trước.");
            int queued=0;double end=ObserveTime(now);
            foreach(var job in State.development.jobs)if(DevelopmentCatalog.Recipe(job.recipe).building==r.building){queued++;end=Math.Max(end,job.readyAt);}
            if(queued>=level)return Fail("Hàng chờ đã đầy. Thu thành phẩm hoặc nâng cấp máy.");
            if(State.development.nextJobId>=1000000000)return Fail("Không thể tạo thêm lượt chế biến.");
            if(ItemCount(r.first)<r.firstCount||r.secondCount>0&&ItemCount(r.second)<r.secondCount)return Fail("Thiếu nguyên liệu: "+RecipeIngredients(r)+".");
            ChangeItem(r.first,-r.firstCount);if(r.secondCount>0)ChangeItem(r.second,-r.secondCount);
            State.development.jobs.Add(new ProductionJob{id=State.development.nextJobId++,recipe=recipe,readyAt=end+ProductionDuration(r,level)});
            return Ok("Đã xếp "+r.name+" · nguyên liệu đã trừ, vẫn chạy khi thoát game.");
        }
        public static string RecipeIngredients(RecipeSpec r){return r.firstCount+" "+DevelopmentCatalog.ItemName(r.first)+(r.secondCount>0?" + "+r.secondCount+" "+DevelopmentCatalog.ItemName(r.second):"");}
        public ActionResult CollectProduction(int id,double now)
        {
            var job=State.development.jobs.Find(j=>j.id==id);if(job==null)return Fail("Lượt này đã được thu.");
            if(ObserveTime(now)<job.readyAt)return Fail("Chưa chế biến xong.");
            if(ItemCount(job.recipe)>=1000000)return Fail("Kho thành phẩm đầy.");
            ChangeItem(job.recipe,1);State.development.jobs.Remove(job);State.development.crafted=Math.Min(1000000000,State.development.crafted+1);
            RecordJourneyProduct(job.recipe);
            GainExperience(10);return Ok("+1 "+DevelopmentCatalog.ItemName(job.recipe)+" vào kho.");
        }
        public ActionResult SellProduct(string id)
        {
            var r=DevelopmentCatalog.Recipe(id);if(r==null)return Fail("Thành phẩm không hợp lệ.");
            int count=ItemCount(id);if(count==0)return Fail("Chưa có thành phẩm.");
            long value=(long)count*r.price;if(State.coins+value>1000000000)return Fail("Kho tiền đã đầy.");
            ChangeItem(id,-count);State.coins+=(int)value;return Ok("Đã bán "+count+" "+r.name+": +"+Money(value)+".");
        }
        public ActionResult UseCareProduct(string id,int index)
        {
            if(index<0||index>=State.plots.Count)return Fail("Chọn ô đất trước khi mở Phát triển đảo.");
            if(id!="compost"&&id!="herbal")return Fail("Vật phẩm không dùng để chăm cây.");
            if(ItemCount(id)<1)return Fail("Chưa có "+DevelopmentCatalog.ItemName(id)+".");
            var p=State.plots[index];
            if(id=="compost"&&p.soilQuality>=2)return Fail("Đất đã đạt độ phì tối đa.");
            if(id=="herbal"&&(string.IsNullOrEmpty(p.cropId)||p.disease<1))return Fail("Cây chưa cần trị bệnh.");
            ChangeItem(id,-1);
            if(id=="compost")p.soilQuality++;else {p.disease=Math.Max(0,p.disease-35);p.treatmentProtection=45;}
            return Ok(id=="compost"?"Đất tăng một bậc độ phì · +1 sản lượng chuẩn.":"Thuốc thảo mộc giảm 35 điểm bệnh, bảo vệ 45 giây.");
        }
        public ActionResult StartNursery(string id,double now)
        {
            var crop=CropCatalog.Find(id);if(crop==null)return Fail("Giống không hợp lệ.");
            int capacity=BuildingLevel("nursery")*2;
            if(capacity==0)return Fail("Xây vườn ươm trước.");
            if(State.development.seedlings.Count>=capacity)return Fail("Khay ươm đầy. Cấy cây đã sẵn sàng ra ruộng.");
            if(Level<crop.unlockLevel||!IslandCropUnlocked(id)||State.coins<crop.seedCost)return Fail("Chưa mở giống hoặc thiếu tiền.");
            State.coins-=crop.seedCost;State.development.seedlings.Add(new SeedlingState{cropId=id,readyAt=ObserveTime(now)+NurseryDuration(crop)});
            return Ok("Đã ươm "+crop.displayName+". Khi khay sẵn sàng, chọn giống đó gieo ra ruộng: không mất thêm tiền, bắt đầu ở 25% tiến độ.");
        }
        public ActionResult ToggleGreenhouse(int index)
        {
            if(index<0||index>=State.plots.Count)return Fail("Chọn ô đất trước khi mở Phát triển đảo.");
            var p=State.plots[index];if(p.greenhouse){p.greenhouse=false;return Ok("Đã bỏ mái che khỏi ô.");}
            int capacity=BuildingLevel("greenhouse")*4;
            if(State.plots.FindAll(x=>x.greenhouse).Count>=capacity)return Fail("Nhà kính chưa xây hoặc đã hết chỗ. Mỗi cấp che được 4 ô.");
            p.greenhouse=true;return Ok("Ô được nhà kính che mưa. Nhớ tưới tay và xử lý nước còn đọng.");
        }
        public string OrderItem {get{string[] items={"carrot","corn","pumpkin","rice","wheat","bread"};int count=Level<2?3:BuildingLevel("bakery")==0?5:6;return items[State.development.ordersCompleted%count];}}
        public int OrderQuantity {get{return OrderItem=="bread"?1:3;}}
        public int OrderReward {get{var c=CropCatalog.Find(OrderItem);int price=c==null?DevelopmentCatalog.Recipe(OrderItem).price:c.sellPrice;return OrderQuantity*price+12;}}
        public ActionResult FulfillOrder()
        {
            string item=OrderItem;int amount=OrderQuantity,reward=OrderReward;
            if(ItemCount(item)<amount)return Fail("Đơn cần "+amount+" "+DevelopmentCatalog.ItemName(item)+".");
            if(State.coins>1000000000-reward||State.development.ordersCompleted>=1000000000)return Fail("Đã đạt giới hạn.");
            ChangeItem(item,-amount);State.coins+=reward;State.development.ordersCompleted++;
            State.development.wood=Math.Min(1000000,State.development.wood+3);State.development.ore=Math.Min(1000000,State.development.ore+1);
            GainExperience(20);return Ok("Giao đơn thành công: +"+Money(reward)+", 3 gỗ, 1 quặng, 20 XP.");
        }
        public ActionResult ClaimDaily(double now)
        {
            int day=(int)(ObserveTime(now)/86400);var d=State.development;
            if(day<=d.lastLoginDay)return Fail("Hôm nay đã nhận quà.");
            int reward=10+Math.Min(6,day==d.lastLoginDay+1?d.loginStreak:0)*2;
            if(State.coins>1000000000-reward)return Fail("Kho tiền đã đầy.");
            d.loginStreak=day==d.lastLoginDay+1?Math.Min(7,d.loginStreak+1):1;d.lastLoginDay=day;
            State.coins+=reward;d.wood=Math.Min(1000000,d.wood+2);return Ok("Quà ngày "+d.loginStreak+": +"+Money(reward)+" và 2 gỗ.");
        }
        public string ChapterTitle {get{string[] titles={"Lá thư thừa kế","Vụ mùa trở lại","Bếp lửa trên đảo","Chuyến thuyền hy vọng","Ánh sáng Mùa Vàng","Nhật ký đang viết tiếp"};return titles[Math.Min(5,State.development.chapter)];}}
        public string ChapterTask {get{string[] tasks={"Gieo cây đầu tiên","Thu hoạch 3 vụ","Thu 3 thành phẩm","Giao 5 đơn hàng","Đạt cấp 6","Đã hoàn tất năm trang nhật ký khởi đầu"};return tasks[Math.Min(5,State.development.chapter)];}}
        public ActionResult ClaimChapter()
        {
            int chapter=State.development.chapter;bool done=chapter==0?State.plots.Exists(p=>!string.IsNullOrEmpty(p.cropId))||State.harvested>0:chapter==1?State.harvested>=3:chapter==2?State.development.crafted>=3:chapter==3?State.development.ordersCompleted>=5:chapter==4&&Level>=6;
            if(!done||chapter>=5)return Fail("Chưa hoàn thành trang nhật ký này.");
            if(State.coins>999999975)return Fail("Kho tiền đầy.");
            State.development.chapter++;State.coins+=25;State.development.wood=Math.Min(1000000,State.development.wood+5);
            GainExperience(20);return Ok("Đã khôi phục một trang Nhật Ký Mùa Vàng · +"+Money(25)+", 5 gỗ, 20 XP.");
        }
        private static bool ValidDevelopment(DevelopmentState d,double now)
        {
            if(d==null)return true;
            if(d.experience<0||d.experience>1000000000||d.wood<0||d.wood>1000000||d.ore<0||d.ore>1000000||
                d.ordersCompleted<0||d.ordersCompleted>1000000000||d.crafted<0||d.crafted>1000000000||d.chapter<0||d.chapter>5||
                d.nextJobId<1||d.nextJobId>1000000000||d.lastLoginDay<0||d.lastLoginDay>now/86400||d.loginStreak<0||d.loginStreak>7||
                d.buildings==null||d.products==null||d.jobs==null||d.seedlings==null||d.seedlings.Count>6||d.buildings.Count>DevelopmentCatalog.Buildings.Length||
                d.products.Count>DevelopmentCatalog.Recipes.Length||d.jobs.Count>27)return false;
            var seen=new HashSet<string>();foreach(var b in d.buildings)if(b==null||DevelopmentCatalog.Building(b.id)==null||b.level<1||b.level>3||!seen.Add(b.id))return false;
            seen.Clear();foreach(var p in d.products)if(p==null||DevelopmentCatalog.Recipe(p.id)==null||p.count<0||p.count>1000000||!seen.Add(p.id))return false;
            var nursery=d.buildings.Find(b=>b.id=="nursery");
            if(d.seedlings.Count>(nursery==null?0:nursery.level*2))return false;
            foreach(var s in d.seedlings)if(s==null||CropCatalog.Find(s.cropId)==null||!IsFinite(s.readyAt)||s.readyAt<0||s.readyAt>now+86400)return false;
            var ids=new HashSet<int>();var counts=new Dictionary<string,int>();
            foreach(var j in d.jobs)
            {
                if(j==null||j.id<1||j.id>=d.nextJobId||!ids.Add(j.id)||!IsFinite(j.readyAt)||j.readyAt<0||j.readyAt>now+86400)return false;
                var r=DevelopmentCatalog.Recipe(j.recipe);if(r==null)return false;
                var b=d.buildings.Find(x=>x.id==r.building);if(b==null)return false;
                if(!counts.ContainsKey(b.id))counts[b.id]=0;if(++counts[b.id]>b.level)return false;
            }
            return true;
        }
    }
}
