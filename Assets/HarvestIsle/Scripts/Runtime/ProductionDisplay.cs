using UnityEngine;

namespace HarvestIsle
{
    public sealed class ProductionDisplay : MonoBehaviour
    {
        public static readonly string[] Types={"Windmill","BakeryOven","DairyChurn","JuicePress","SoyVat","HerbalBench","CompostBin","GraftingBench","BeeHive","MarketStall","HandCart","WoodBridge","BoatDock","CargoBoat","WeatherStation","Lighthouse"};
        public static readonly string[] Labels={"Cối xay gió","Lò bánh mì","Máy chế biến sữa","Máy ép trái cây","Nồi ủ tương","Bàn thảo dược","Thùng ủ phân","Bàn ghép cây","Tổ ong","Quầy chợ","Xe kéo","Cầu gỗ","Bến thuyền","Thuyền chở hàng","Trạm thời tiết","Hải đăng"};
        [SerializeField] private string displayName;
        public string DisplayName {get{return displayName;}}
        public void Configure(string label){displayName=label;}
    }
}
