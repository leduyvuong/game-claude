using System;

namespace HarvestIsle.Core
{
    public enum HotpotMount { Floor, Wall, Ceiling }
    public sealed class HotpotFurnitureSpec
    {
        public readonly string id, name;
        public readonly int price, category;
        public readonly bool table;
        public readonly HotpotMount mount;
        public HotpotFurnitureSpec(string id,string name,int price,int category,bool table=false,HotpotMount mount=HotpotMount.Floor)
        {this.id=id;this.name=name;this.price=price;this.category=category;this.table=table;this.mount=mount;}
    }
    public static class HotpotFurnitureCatalog
    {
        // Persisted kind indices: never reorder. The first eight are the original save IDs.
        public const int ReferenceStart=8, FloorSlots=12, WallSlots=8, CeilingSlots=6, SlotCount=26;
        public static readonly string[] Categories={"Bàn ăn","Cây & cảnh","Đèn & tường","Phục vụ","Tiện ích"};
        public static readonly HotpotFurnitureSpec[] Items={
            new HotpotFurnitureSpec("LegacyBamboo","Bàn tre",25,0,true),
            new HotpotFurnitureSpec("LegacyWood","Bàn gỗ",90,0,true),
            new HotpotFurnitureSpec("LegacyDual","Bàn song vị",280,0,true),
            new HotpotFurnitureSpec("LegacyFeast","Bàn tiệc",800,0,true),
            new HotpotFurnitureSpec("LegacyVip","Bàn VIP",2200,0,true),
            new HotpotFurnitureSpec("LegacyGold","Bàn hoàng kim",6000,0,true),
            new HotpotFurnitureSpec("LegacyPlant","Chậu cây",15,1),
            new HotpotFurnitureSpec("LegacyLantern","Đèn lồng",25,2),
            new HotpotFurnitureSpec("RusticRound","Bàn tròn ghế đẩu",45,0,true),
            new HotpotFurnitureSpec("GrandRound","Bàn tròn đại gia đình",180,0,true),
            new HotpotFurnitureSpec("ClassicSquare","Bàn gỗ vuông",95,0,true),
            new HotpotFurnitureSpec("FamilyRectangle","Bàn gỗ hai nồi",240,0,true),
            new HotpotFurnitureSpec("CozyPair","Bàn đôi ghế đệm",140,0,true),
            new HotpotFurnitureSpec("StreetSteel","Bàn inox ghế đỏ",55,0,true),
            new HotpotFurnitureSpec("GardenPlastic","Bàn nhựa xanh",35,0,true),
            new HotpotFurnitureSpec("Industrial","Bàn ghế sắt",190,0,true),
            new HotpotFurnitureSpec("MarbleGold","Bàn đá chân vàng",6000,0,true),
            new HotpotFurnitureSpec("LeatherFeast","Bàn tiệc ghế da",2200,0,true),
            new HotpotFurnitureSpec("RedBooth","Bàn sofa đỏ",1200,0,true),
            new HotpotFurnitureSpec("TealCorner","Bàn sofa góc xanh",1800,0,true),
            new HotpotFurnitureSpec("Tatami","Bàn bệt đệm cói",320,0,true),
            new HotpotFurnitureSpec("Communal","Bàn dài ghế băng",700,0,true),
            new HotpotFurnitureSpec("CounterDining","Quầy lẩu cá nhân",950,0,true),
            new HotpotFurnitureSpec("BambooPatio","Bàn tre sân vườn",380,0,true),
            new HotpotFurnitureSpec("LanternPost","Cột ba đèn lồng",65,2),
            new HotpotFurnitureSpec("MenuEasel","Bảng thực đơn",35,3),
            new HotpotFurnitureSpec("BambooPlanter","Bồn trúc",90,1),
            new HotpotFurnitureSpec("CeramicTree","Cây chậu sứ",75,1),
            new HotpotFurnitureSpec("Bonsai","Bonsai đôn gỗ",180,1),
            new HotpotFurnitureSpec("FlowerBox","Bồn hoa dài",85,1),
            new HotpotFurnitureSpec("FoldingScreen","Bình phong",160,2),
            new HotpotFurnitureSpec("SlatDivider","Vách nan gỗ",110,2),
            new HotpotFurnitureSpec("RockFountain","Tiểu cảnh thác đá",600,1),
            new HotpotFurnitureSpec("Aquarium","Bể cá tủ gỗ",480,1),
            new HotpotFurnitureSpec("FloorLantern","Đèn đứng giấy",100,2),
            new HotpotFurnitureSpec("PendantTrio","Bộ ba đèn thả",170,2,false,HotpotMount.Ceiling),
            new HotpotFurnitureSpec("WallClock","Đồng hồ gỗ",60,2,false,HotpotMount.Wall),
            new HotpotFurnitureSpec("HotpotPoster","Tranh nồi lẩu",50,2,false,HotpotMount.Wall),
            new HotpotFurnitureSpec("CeramicShelf","Kệ gốm treo tường",120,2,false,HotpotMount.Wall),
            new HotpotFurnitureSpec("HangingPlants","Giàn cây treo",140,1,false,HotpotMount.Ceiling),
            new HotpotFurnitureSpec("LuckyCat","Mèo thần tài",220,2),
            new HotpotFurnitureSpec("NorenCurtain","Rèm đỏ",130,2),
            new HotpotFurnitureSpec("Reception","Quầy thu ngân",350,3),
            new HotpotFurnitureSpec("WaitingBench","Ghế chờ đệm đỏ",150,3),
            new HotpotFurnitureSpec("ServingCart","Xe đẩy món",180,3),
            new HotpotFurnitureSpec("SauceCabinet","Quầy nước chấm",230,3),
            new HotpotFurnitureSpec("ChilledDisplay","Tủ mát nguyên liệu",850,3),
            new HotpotFurnitureSpec("DrinksFridge","Tủ nước giải khát",750,3),
            new HotpotFurnitureSpec("DishCupboard","Tủ bát đũa",200,3),
            new HotpotFurnitureSpec("BambooSteamers","Bộ xửng tre",100,3),
            new HotpotFurnitureSpec("RiceCooker","Tủ nồi cơm",240,3),
            new HotpotFurnitureSpec("TeaTrolley","Xe trà",280,3),
            new HotpotFurnitureSpec("IceBucket","Xô đá ướp nước",160,3),
            new HotpotFurnitureSpec("StandingFan","Quạt đứng",120,4),
            new HotpotFurnitureSpec("CoatStand","Cây treo áo",65,4),
            new HotpotFurnitureSpec("UmbrellaBasket","Giỏ đựng ô",75,4),
            new HotpotFurnitureSpec("RecycleBins","Thùng phân loại rác",80,4),
            new HotpotFurnitureSpec("FireExtinguisher","Bình chữa cháy",100,4),
            new HotpotFurnitureSpec("PrepTable","Bàn sơ chế inox",420,3),
            new HotpotFurnitureSpec("ExhaustHood","Chụp hút khói",300,4,false,HotpotMount.Ceiling)
        };
        public static bool IsTable(int kind){return kind>=0&&kind<Items.Length&&Items[kind].table;}
        public static HotpotMount SlotMount(int slot){return slot<FloorSlots?HotpotMount.Floor:slot<FloorSlots+WallSlots?HotpotMount.Wall:HotpotMount.Ceiling;}
        public static bool Fits(int kind,int slot){return kind>=0&&kind<Items.Length&&slot>=0&&slot<SlotCount&&Items[kind].mount==SlotMount(slot);}
        public static string[] Names(){return Array.ConvertAll(Items,x=>x.name);}
        public static int[] Prices(){return Array.ConvertAll(Items,x=>x.price);}
        public static string Resource(int kind){return "HotpotFurniture/HP_"+Items[kind].id;}
    }
}
