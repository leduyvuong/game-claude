using System;

namespace HarvestIsle
{
    public enum HotpotKitchenRole { Utensil, Ingredient, Dish, Kitchen }
    public sealed class HotpotKitchenSpec
    {
        public readonly string id,label,ingredientKey;
        public readonly HotpotKitchenRole role;
        public readonly int level;
        public HotpotKitchenSpec(string id,string label,HotpotKitchenRole role,string ingredientKey="",int level=0)
        {this.id="HP_KC_"+id;this.label=label;this.role=role;this.ingredientKey=ingredientKey;this.level=level;}
        public string Resource {get{return "HotpotKitchen/"+id;}}
    }
    // Rendering metadata; stock, supplier unlocks and kitchen progression live in Core.
    public static class HotpotKitchenCatalog
    {
        public const int IngredientStart=12,DishStart=36,KitchenStart=48;
        public static readonly string[] Groups={"Dụng cụ lẩu","Nguyên liệu lẩu","Nồi lẩu thành phẩm","Bếp quán lẩu"};
        public static readonly HotpotKitchenSpec[] Items={
            new HotpotKitchenSpec("GasStove","Bếp gas mini",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("Induction","Bếp từ",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("SteelPot","Nồi inox",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("YinYangPot","Nồi song vị",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("FourPot","Nồi bốn ngăn",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("ClayPot","Nồi đất",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("Lid","Nắp nồi",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("StockPot","Nồi hầm và nắp",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("Ladle","Vá múc",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("Skimmer","Vá vớt",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("Tongs","Kẹp gắp",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("PlaceSetting","Bát và đũa",HotpotKitchenRole.Utensil),
            new HotpotKitchenSpec("BeefRolls","Khay bò cuộn",HotpotKitchenRole.Ingredient,"beef"),
            new HotpotKitchenSpec("PorkRolls","Khay ba chỉ cuộn",HotpotKitchenRole.Ingredient,"pork"),
            new HotpotKitchenSpec("Chicken","Đĩa thịt gà",HotpotKitchenRole.Ingredient,"chickenMeat"),
            new HotpotKitchenSpec("Fish","Đĩa cá phi lê",HotpotKitchenRole.Ingredient,"fish"),
            new HotpotKitchenSpec("Shrimp","Đĩa tôm",HotpotKitchenRole.Ingredient,"shrimp"),
            new HotpotKitchenSpec("Squid","Đĩa mực",HotpotKitchenRole.Ingredient,"squid"),
            new HotpotKitchenSpec("Mussels","Đĩa vẹm",HotpotKitchenRole.Ingredient,"mussels"),
            new HotpotKitchenSpec("Meatballs","Viên thả lẩu",HotpotKitchenRole.Ingredient,"meatballs"),
            new HotpotKitchenSpec("Tofu","Đậu phụ",HotpotKitchenRole.Ingredient,"tofu"),
            new HotpotKitchenSpec("TofuSkin","Tàu hũ ky cuộn",HotpotKitchenRole.Ingredient,"tofuSkin"),
            new HotpotKitchenSpec("Enoki","Nấm kim châm",HotpotKitchenRole.Ingredient,"mushroom"),
            new HotpotKitchenSpec("Shiitake","Nấm hương",HotpotKitchenRole.Ingredient,"mushroom"),
            new HotpotKitchenSpec("OysterMushrooms","Nấm bào ngư",HotpotKitchenRole.Ingredient,"mushroom"),
            new HotpotKitchenSpec("NapaCabbage","Cải thảo",HotpotKitchenRole.Ingredient,"cabbage"),
            new HotpotKitchenSpec("WaterSpinach","Rau muống",HotpotKitchenRole.Ingredient,"waterspinach"),
            new HotpotKitchenSpec("BokChoy","Cải thìa",HotpotKitchenRole.Ingredient,"greens"),
            new HotpotKitchenSpec("Corn","Khoanh ngô",HotpotKitchenRole.Ingredient,"corn"),
            new HotpotKitchenSpec("Tomatoes","Cà chua bổ múi",HotpotKitchenRole.Ingredient,"tomato"),
            new HotpotKitchenSpec("EggNoodles","Mì trứng",HotpotKitchenRole.Ingredient,"eggNoodles"),
            new HotpotKitchenSpec("RiceNoodles","Bún gạo",HotpotKitchenRole.Ingredient,"riceNoodles"),
            new HotpotKitchenSpec("Sauces","Bộ ba nước chấm",HotpotKitchenRole.Ingredient,"seasoning"),
            new HotpotKitchenSpec("ChiliGarlic","Ớt và tỏi băm",HotpotKitchenRole.Ingredient,"seasoning"),
            new HotpotKitchenSpec("BrothJugs","Bình châm nước dùng",HotpotKitchenRole.Ingredient,"base"),
            new HotpotKitchenSpec("Herbs","Rau thơm và chanh",HotpotKitchenRole.Ingredient,"herbs"),
            new HotpotKitchenSpec("Thai","Lẩu Thái",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("Sichuan","Lẩu Tứ Xuyên",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("TomatoHotpot","Lẩu cà chua",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("MushroomHotpot","Lẩu nấm",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("Seafood","Lẩu hải sản",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("BeefHotpot","Lẩu bò",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("ChickenHerb","Lẩu gà lá é",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("SourFish","Lẩu cá chua cay",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("Goat","Lẩu dê",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("Crab","Lẩu cua đồng",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("Sukiyaki","Sukiyaki",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("Shabu","Shabu-shabu",HotpotKitchenRole.Dish),
            new HotpotKitchenSpec("Kitchen_L1","Bếp cấp 1",HotpotKitchenRole.Kitchen,"",1),
            new HotpotKitchenSpec("Kitchen_L2","Bếp cấp 2",HotpotKitchenRole.Kitchen,"",2),
            new HotpotKitchenSpec("Kitchen_L3","Bếp cấp 3",HotpotKitchenRole.Kitchen,"",3),
            new HotpotKitchenSpec("Kitchen_L4","Bếp cấp 4",HotpotKitchenRole.Kitchen,"",4),
            new HotpotKitchenSpec("Kitchen_L5","Bếp cấp 5",HotpotKitchenRole.Kitchen,"",5),
            new HotpotKitchenSpec("Kitchen_L6","Bếp cấp 6",HotpotKitchenRole.Kitchen,"",6),
            new HotpotKitchenSpec("Kitchen_L7","Bếp cấp 7",HotpotKitchenRole.Kitchen,"",7),
            new HotpotKitchenSpec("Kitchen_L8","Bếp cấp 8",HotpotKitchenRole.Kitchen,"",8)
        };
        public static HotpotKitchenSpec Find(string id){return Array.Find(Items,x=>x.id==id);}
    }
}
