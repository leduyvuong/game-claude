using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    [Serializable] public sealed class KitchenStock
    {
        public string id;
        public int stars = 1, count;
    }
    [Serializable] public sealed class HotpotPortion
    {
        public string id;
        public int stars = 1, amount = 1;
        public HotpotPortion() { }
        public HotpotPortion(string id, int amount, int stars) { this.id=id; this.amount=amount; this.stars=stars; }
    }
    [Serializable] public sealed class HotpotRecipe
    {
        public int id, style, broth, salt = 1, heat, technique;
        public string name = "Lẩu vườn nhà";
        public List<HotpotPortion> portions = new List<HotpotPortion>();
        public bool onMenu;
        public int timesServed;
    }
    [Serializable] public sealed class HotpotFurniture
    {
        public int floor, slot, kind, rotation;
    }
    [Serializable] public sealed class HotpotState
    {
        public List<KitchenStock> stock = new List<KitchenStock>();
        public List<HotpotRecipe> recipes = new List<HotpotRecipe>();
        public List<HotpotFurniture> furniture = new List<HotpotFurniture>();
        public int nextRecipeId = 1, floors = 1;
        public int reviews;
        public long tasteTotal;
        public HotpotProgress progress;
    }
    public sealed class HotpotIngredient
    {
        public string id, name;
        public bool farm;
        public int price, sweet, savory, sour, vegetable;
        public HotpotIngredient(string id,string name,bool farm,int price,int sweet,int savory,int sour,int vegetable)
        {this.id=id;this.name=name;this.farm=farm;this.price=price;this.sweet=sweet;this.savory=savory;this.sour=sour;this.vegetable=vegetable;}
    }
    public sealed class HotpotTaste
    {
        public int balance, pairing, ratio, technique, quality;
        public int Score { get { return balance+pairing+ratio+technique+quality; } }
        public List<string> notes = new List<string>();
    }
    public static partial class HotpotRules
    {
        public static readonly string[] Styles = { "Thanh ngọt", "Chua cay", "Đậm đà" };
        public static readonly string[] Broths = { "Rau củ", "Chua cay", "Hầm đậm" };
        public static readonly string[] Techniques = { "Ninh củ trước, rau sau", "Nấu chung từ đầu", "Đun nhanh" };
        public static readonly string[] Furniture = HotpotFurnitureCatalog.Names();
        public static readonly int[] FurniturePrices = HotpotFurnitureCatalog.Prices();
        private static readonly HotpotIngredient[] BaseIngredients = {
            new HotpotIngredient("carrot","Cà rốt",true,5,2,0,0,1),
            new HotpotIngredient("corn","Ngô",true,7,3,1,0,1),
            new HotpotIngredient("pumpkin","Bí đỏ",true,12,4,0,0,1),
            new HotpotIngredient("greens","Cải ngọt",true,4,0,0,0,1),
            new HotpotIngredient("tomato","Cà chua",true,9,1,1,3,1),
            new HotpotIngredient("onion","Hành",true,7,1,2,0,1),
            new HotpotIngredient("beans","Đậu leo",true,6,0,1,0,1),
            new HotpotIngredient("mushroom","Nấm",true,10,0,4,0,1),
            new HotpotIngredient("taro","Khoai môn",true,12,2,1,0,1),
            new HotpotIngredient("waterspinach","Rau muống",true,6,0,0,0,1),
            new HotpotIngredient("lotus","Sen",true,15,1,1,0,1),
            new HotpotIngredient("cabbage","Bắp cải",true,10,1,0,0,1),
            new HotpotIngredient("fish","Cá tươi",true,8,0,4,0,0),
            new HotpotIngredient("eggs","Trứng",true,5,0,2,0,0),
            new HotpotIngredient("milk","Sữa",true,12,1,1,0,0),
            new HotpotIngredient("base","Nước dùng cơ bản",false,2,0,0,0,0),
            new HotpotIngredient("seasoning","Gia vị cơ bản",false,1,0,0,0,0),
            new HotpotIngredient("chickenMeat","Thịt gà",false,12,0,4,0,0),
            new HotpotIngredient("beef","Thịt bò",false,22,0,5,0,0),
            new HotpotIngredient("shrimp","Tôm",false,35,0,5,0,0),
            new HotpotIngredient("lobster","Tôm hùm",false,280,1,6,0,0),
            new HotpotIngredient("abalone","Bào ngư",false,450,0,6,0,0)
        };
        public static readonly HotpotIngredient[] Ingredients=ExpandIngredients(BaseIngredients);
        public static HotpotIngredient Find(string id) { return Array.Find(Ingredients,x=>x.id==id); }
        public static bool ValidRecipe(HotpotRecipe r)
        {
            if(r==null||String.IsNullOrWhiteSpace(r.name)||r.name.Length>32||r.name.IndexOfAny(new[]{'\n','\r','<','>'})>=0||r.style<0||r.style>2||r.broth<0||r.broth>2||r.salt<0||r.salt>3||r.heat<0||r.heat>3||r.technique<0||r.technique>2||r.portions==null||r.portions.Count<2||r.portions.Count>6)return false;
            var ids=new HashSet<string>(); bool farm=false;
            foreach(var p in r.portions)
            {
                var spec=p==null?null:Find(p.id);
                if(spec==null||p.id=="base"||p.id=="seasoning"||!ids.Add(p.id)||p.amount<1||p.amount>3||p.stars<1||p.stars>3||(!spec.farm&&p.stars!=1)||(CropCatalog.Find(p.id)==null&&p.stars!=1))return false;
                farm|=spec.farm;
            }
            return farm;
        }
        public static HotpotRecipe Copy(HotpotRecipe r)
        {
            var copy=new HotpotRecipe{id=r.id,name=r.name,style=r.style,broth=r.broth,salt=r.salt,heat=r.heat,technique=r.technique,onMenu=r.onMenu,timesServed=r.timesServed};
            foreach(var p in r.portions)copy.portions.Add(new HotpotPortion(p.id,p.amount,p.stars));
            return copy;
        }
        public static HotpotTaste Taste(HotpotRecipe r)
        {
            var result=new HotpotTaste();
            if(!ValidRecipe(r)){result.notes.Add("Cần 2–6 nguyên liệu khác nhau, có ít nhất một nông sản từ farm.");return result;}
            int total=0,sweet=0,savory=0,sour=0,veg=0,quality=0;
            var ids=new HashSet<string>();
            foreach(var p in r.portions)
            {
                var s=Find(p.id);total+=p.amount;sweet+=s.sweet*p.amount;savory+=s.savory*p.amount;sour+=s.sour*p.amount;veg+=s.vegetable*p.amount;
                if(s.farm)quality+=p.amount*(4+3*(p.stars-1));
                ids.Add(p.id);
            }
            result.quality=quality/total;
            result.balance=30-Math.Abs(r.salt-(r.style==2?2:1))*6-Math.Abs(r.heat-(r.style==1?2:0))*4;
            if(sweet>total*2){result.balance-=7;result.notes.Add("Vị ngọt lấn át: giảm bí/ngô, tăng rau hoặc nấm.");}
            if(r.salt>=3)result.notes.Add("Nước lẩu quá mặn: giảm lượng gia vị mặn.");
            if(r.style==1&&r.heat<2)result.notes.Add("Lẩu chua cay cần thêm vị cay.");
            result.pairing=r.style==r.broth?19:11;
            if(ids.Contains("corn")&&ids.Contains("mushroom")||ids.Contains("fish")&&ids.Contains("tomato"))result.pairing+=6;
            else if(veg>0&&savory>0)result.pairing+=3;
            if(ids.Contains("milk")&&(sour>0||r.broth==1)){result.pairing-=10;result.notes.Add("Sữa không hợp nền chua này: bỏ sữa hoặc đổi nước dùng.");}
            if(r.style!=r.broth)result.notes.Add("Đổi nước dùng cho hợp phong cách món.");
            result.ratio=total<=8?20:12;
            if(veg==0){result.ratio-=8;result.notes.Add("Thêm rau để cân bằng phần đạm.");}
            if(r.style==2&&savory<total){result.ratio-=5;result.notes.Add("Thêm nấm, cá hoặc đạm để tăng vị đậm đà.");}
            result.technique=r.technique==0?15:r.technique==1?8:10;
            if(r.technique==1)result.notes.Add("Cho rau vào cuối để tránh bị nhũn.");
            if(r.technique==2&&(ids.Contains("taro")||ids.Contains("pumpkin"))){result.technique=3;result.notes.Add("Khoai/bí cần ninh trước, không hợp đun nhanh.");}
            if(result.quality<7)result.notes.Add("Tăng tỷ lệ nông sản ★★/★★★ để nâng chất lượng.");
            result.balance=Math.Max(0,result.balance);result.pairing=Math.Max(0,result.pairing);
            if(result.notes.Count==0)result.notes.Add("Vị hài hòa, nguyên liệu và cách nấu phối hợp tốt.");
            return result;
        }
        public static bool Validate(HotpotState state)
        {
            if(state==null)return true;
            if(!ValidateProgress(state.progress))return false;
            if(state.stock==null||state.recipes==null||state.furniture==null||state.stock.Count>Ingredients.Length*3||state.recipes.Count>24||state.floors<1||state.floors>4||state.nextRecipeId<1||state.nextRecipeId>1000000||state.reviews<0||state.reviews>1000000000||state.tasteTotal<0||state.tasteTotal>(long)state.reviews*100)return false;
            var ids=new HashSet<string>();
            foreach(var s in state.stock)
            {
                var spec=s==null?null:Find(s.id);
                if(spec==null||s.count<0||s.count>1000000||s.stars<1||s.stars>3||(CropCatalog.Find(s.id)==null&&s.stars!=1)||!ids.Add(s.id+":"+s.stars))return false;
            }
            var recipes=new HashSet<int>();
            foreach(var r in state.recipes)if(!ValidRecipe(r)||r.id<1||r.id>=state.nextRecipeId||!recipes.Add(r.id)||r.timesServed<0||r.timesServed>1000000000)return false;
            ids.Clear();
            foreach(var f in state.furniture)
                if(f==null||f.floor<0||f.floor>=state.floors||!HotpotFurnitureCatalog.Fits(f.kind,f.slot)||(f.floor==0&&f.slot<6)||f.rotation<0||f.rotation>3||!ids.Add(f.floor+":"+f.slot))return false;
            return true;
        }
    }
    public sealed partial class FarmModel
    {
        public int KitchenCount(string id,int stars=1)
        { var s=State.hotpot.stock.Find(x=>x.id==id&&x.stars==stars);return s==null?0:s.count; }
        private void KitchenAdd(string id,int stars,int amount)
        {
            var s=State.hotpot.stock.Find(x=>x.id==id&&x.stars==stars);
            if(s==null){s=new KitchenStock{id=id,stars=stars};State.hotpot.stock.Add(s);}s.count+=amount;
        }
        public int FarmKitchenAvailable(string id,int stars)
        {
            if(CropCatalog.Find(id)!=null)return stars==3?CountPrime(id):stars==2?CountFine(id):Count(id);
            if(stars!=1)return 0;
            if(IslandExpansionCatalog.Item(id)!=null)return IslandCount(id);
            return id=="fish"?State.fishStock:id=="eggs"?State.eggs:id=="milk"?State.milk:id=="honey"?State.honey:DevelopmentCatalog.Recipe(id)!=null?ItemCount(id):0;
        }
        public ActionResult ImportKitchen(string id,int stars,int amount)
        {
            var s=HotpotRules.Find(id);
            if(s==null||!s.farm||stars<1||stars>3||amount<1||amount>1000||(CropCatalog.Find(id)==null&&stars!=1))return Fail("Lô nông sản không hợp lệ.");
            if(FarmKitchenAvailable(id,stars)<amount)return Fail("Nông trại chưa đủ "+s.name+" đúng phẩm chất.");
            if(KitchenCount(id,stars)>1000000-amount)return Fail("Kho bếp đã đầy.");
            if(CropCatalog.Find(id)!=null)(stars==3?Bucket(State.primeGoods,id):stars==2?Bucket(State.fineGoods,id):Entry(id)).count-=amount;
            else if(id=="fish")State.fishStock-=amount;else if(id=="eggs")State.eggs-=amount;else if(id=="milk")State.milk-=amount;else if(id=="honey")State.honey-=amount;else ChangeItem(id,-amount);
            KitchenAdd(id,stars,amount);return Ok("Đã nhập "+amount+" "+s.name+" vào kho bếp.");
        }
        public ActionResult BuyKitchen(string id,int amount)
        {
            var s=HotpotRules.Find(id);
            if(s==null||s.farm||amount<1||amount>1000)return Fail("Nông sản phải nhập từ nông trại.");
            if(HotpotRank<SupplierUnlockRank(id))return Fail("Nguyên liệu mở ở hạng "+SupplierUnlockRank(id)+".");
            int cost=KitchenUnitPrice(id)*amount;
            if(State.coins<cost)return Fail("Cần "+Money(cost)+" để nhập hàng.");
            if(KitchenCount(id)>1000000-amount)return Fail("Kho bếp đã đầy.");
            State.coins-=cost;KitchenAdd(id,1,amount);return Ok("Đã mua "+amount+" "+s.name+".");
        }
        // All recipes consume one base, plus the seasoning indicated by the controls.
        private List<HotpotPortion> RecipeNeeds(HotpotRecipe r)
        {
            var list=new List<HotpotPortion>(r.portions);
            list.Add(new HotpotPortion("base",1,1));
            list.Add(new HotpotPortion("seasoning",1+r.salt+r.heat,1));return list;
        }
        public string KitchenMissing(HotpotRecipe r)
        {
            if(!HotpotRules.ValidRecipe(r))return "Công thức cần 2–6 nguyên liệu, gồm nông sản của farm.";
            foreach(var p in RecipeNeeds(r))if(KitchenCount(p.id,p.stars)<p.amount)return "Thiếu "+HotpotRules.Find(p.id).name+" "+new string('★',p.stars)+" (cần "+p.amount+").";
            return "";
        }
        private void ConsumeRecipe(HotpotRecipe r) {foreach(var p in RecipeNeeds(r))KitchenAdd(p.id,p.stars,-p.amount);}
        public ActionResult TestHotpotRecipe(HotpotRecipe draft)
        {
            string missing=KitchenMissing(draft);if(missing!="")return Fail(missing);
            if(State.hotpot.recipes.Count>=24||State.hotpot.nextRecipeId>=1000000)return Fail("Sổ công thức đã đầy (24 món).");
            if(draft.technique==2&&KitchenLevel<3)return Fail("Bếp cấp 3 mở cách đun nhanh.");
            var saved=HotpotRules.Copy(draft);saved.id=State.hotpot.nextRecipeId;saved.onMenu=false;saved.timesServed=0;
            ConsumeRecipe(saved);State.hotpot.nextRecipeId++;State.hotpot.recipes.Add(saved);
            DiscoverHotpot(saved);
            return Ok("Nấu thử "+saved.name+": "+HotpotRules.Taste(saved).Score+"/100. Đã lưu phiên bản; chọn đưa lên thực đơn.");
        }
        public ActionResult SetHotpotMenu(int id,bool enabled)
        {
            var r=State.hotpot.recipes.Find(x=>x.id==id);if(r==null)return Fail("Không tìm thấy món đã nấu thử.");
            r.onMenu=enabled;return Ok(enabled?"Đã đưa món lên thực đơn.":"Đã tạm ngừng bán món.");
        }
        public ActionResult RemoveHotpotRecipe(int id)
        {
            var r=State.hotpot.recipes.Find(x=>x.id==id);
            if(r==null||r.onMenu)return Fail("Ngừng bán món trước khi xóa công thức.");
            State.hotpot.recipes.Remove(r);return Ok("Đã xóa công thức khỏi sổ; nguyên liệu nấu thử không hoàn lại.");
        }
        public int RecipePrice(HotpotRecipe r)
        {
            if(!HotpotRules.ValidRecipe(r))return 0;int cost=0;
            foreach(var p in RecipeNeeds(r))cost+=StarUnitPrice(HotpotRules.Find(p.id).price,p.stars)*p.amount;
            return HotpotRules.SafeInt(cost*(1.35+HotpotRules.Taste(r).Score*.006)*(1+.1*Math.Max(HotpotTier,HotpotRank-1))*
                (HotpotRules.Specialty(r)>=0?1.15:1)*(1+.03*HotpotRules.Mastery(r)));
        }
        public ActionResult ServeHotpotRecipe(int id)
        {
            var r=State.hotpot.recipes.Find(x=>x.id==id);
            if(r==null||!r.onMenu)return Fail("Món chưa có trên thực đơn.");
            string missing=KitchenMissing(r);if(missing!="")return Fail(missing);
            int price=RecipePrice(r);if(State.coins>1000000000-price)return Fail("Kho tiền đã đầy.");
            if(State.hotpot.reviews>=1000000000||State.hotpotServed>=1000000000)return Fail("Đã đạt giới hạn lượt phục vụ.");
            ConsumeRecipe(r);State.coins+=price;State.hotpotServed++;State.hotpot.reviews++;State.hotpot.tasteTotal+=HotpotRules.Taste(r).Score;GainExperience(6);
            RecordHotpotSale(r,HotpotRules.Taste(r).Score,price);
            return Ok("Khách thưởng thức "+r.name+" · +"+Money(price)+" · "+HotpotRules.Taste(r).Score+" điểm ngon.");
        }
        public int NextHotpotFloorCost {get{return State.hotpot.floors==1?300:State.hotpot.floors==2?1200:4000;}}
        public ActionResult ExpandHotpot()
        {
            if(State.hotpot.floors>=4)return Fail("Quán đã có đủ bốn tầng.");
            int target=State.hotpot.floors==1?15:State.hotpot.floors==2?40:100;
            int served=State.hotpot.progress.branch>0?State.hotpot.progress.branchServed:State.hotpotServed;
            if(served<target)return Fail("Cần phục vụ "+target+" nồi tại chi nhánh để mở tầng tiếp.");
            int cost=NextHotpotFloorCost;if(State.coins<cost)return Fail("Cần "+Money(cost)+" để mở tầng.");
            State.coins-=cost;State.hotpot.floors++;return Ok("Đã mở tầng "+State.hotpot.floors+"! Hãy đặt bàn và trang trí.");
        }
        // Twelve generous furniture bays, separated by fixed aisles: no placement can block access.
        public ActionResult PlaceHotpotFurniture(int floor,int slot,int kind,int rotation)
        {
            if(floor<0||floor>=State.hotpot.floors||!HotpotFurnitureCatalog.Fits(kind,slot)||rotation<0||rotation>3)return Fail("Chọn đúng vị trí sàn, tường hoặc trần cho món đồ.");
            if(State.hotpot.furniture.Exists(x=>x.floor==floor&&x.slot==slot))return Fail("Vị trí đã có đồ. Hãy di chuyển trước.");
            if(floor==0&&slot<6)return Fail("Đây là bàn cơ bản đã được cấp.");
            int cost=HotpotRules.FurniturePrices[kind];if(State.coins<cost)return Fail("Cần "+Money(cost)+".");
            State.coins-=cost;State.hotpot.furniture.Add(new HotpotFurniture{floor=floor,slot=slot,kind=kind,rotation=rotation});return Ok("Đã đặt "+HotpotRules.Furniture[kind]+".");
        }
        public ActionResult MoveHotpotFurniture(int floor,int from,int to,int rotation)
        {
            var f=State.hotpot.furniture.Find(x=>x.floor==floor&&x.slot==from);
            if(f==null||!HotpotFurnitureCatalog.Fits(f.kind,to)||rotation<0||rotation>3||(floor==0&&to<6))return Fail("Chọn vị trí trống đúng loại sàn, tường hoặc trần.");
            if(from!=to&&State.hotpot.furniture.Exists(x=>x.floor==floor&&x.slot==to))return Fail("Vị trí đã có đồ.");
            f.slot=to;f.rotation=rotation;return Ok("Đã di chuyển đồ miễn phí.");
        }
    }
}
