using UnityEngine;
using static HarvestIsle.Editor.CropMesh;

namespace HarvestIsle.Editor
{
    internal static class CompleteCropBuilder
    {
        internal static readonly string[] Names={"Peas","Strawberry","Marigold","Chili","Lemongrass","WaterSpinach","Taro","Lotus","Mushroom","Radish","Cabbage","Wheat","SnowTea","Garlic","Mango","Coconut","Orange","Lemon","Banana","Papaya"};
        internal static readonly string[] Labels={"Đậu Hà Lan","Dâu tây","Cúc vạn thọ","Ớt","Sả","Rau muống","Khoai môn","Sen","Nấm","Củ cải","Bắp cải","Lúa mì","Trà tuyết","Tỏi","Xoài","Dừa","Cam","Chanh","Chuối","Đu đủ"};
        internal static GameObject Create(string name, int stage, Material[] materials, out Mesh mesh)
            => ReferenceCropBuilder.Create(name, stage, materials, out mesh);
    }
}
