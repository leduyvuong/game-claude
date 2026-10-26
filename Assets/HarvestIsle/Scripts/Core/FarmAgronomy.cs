using System;
namespace HarvestIsle.Core
{
    public sealed partial class FarmModel
    {
        public static string CropGroup(string id)
        {
            if(string.IsNullOrEmpty(id))return "";
            string expansion=IslandCropCatalog.Group(id);if(expansion!=null)return expansion;
            if(id=="beans"||id=="peas")return "Đậu";
            if(id=="marigold"||id=="lotus")return "Hoa";
            if(id=="carrot"||id=="radish"||id=="taro"||id=="onion"||id=="garlic")return "Củ";
            if(id=="rice"||id=="corn"||id=="wheat"||id=="greens"||id=="cabbage"||id=="waterspinach"||id=="lemongrass"||id=="sugarcane"||id=="snowtea")return "Lá";
            return "Quả";
        }
        public bool IsMonoculture(int index)
        {
            var p=State.plots[index];string group=CropGroup(p.cropId);
            return group!=""&&group==CropGroup(p.previousCropId)&&group==CropGroup(p.secondPreviousCropId);
        }
        public int RotationYieldBonus(int index)
        {
            var p=State.plots[index];var crop=CropCatalog.Find(p.cropId);if(crop==null)return 0;
            string current=CropGroup(p.cropId),previous=CropGroup(p.previousCropId),second=CropGroup(p.secondPreviousCropId);
            if(previous==""||previous==current)return 0;
            bool triple=second!=""&&second!=current&&second!=previous;
            return (int)Math.Ceiling(crop.yield*(triple?.20:.10));
        }
        private bool HasNeighbour(int index,string crop,bool diagonal)
        {
            int x=index%Width,z=index/Width;
            for(int i=0;i<State.plots.Count;i++)
            {
                int dx=Math.Abs(i%Width-x),dz=Math.Abs(i/Width-z);
                if(i!=index&&dx<=1&&dz<=1&&(diagonal||dx+dz==1)&&State.plots[i].cropId==crop)return true;
            }
            return false;
        }
        public double CompanionProtection(int index)
        {
            string crop=State.plots[index].cropId;double result=HasNeighbour(index,"marigold",true)?.7:1;
            if(crop=="carrot"&&HasNeighbour(index,"onion",false)||crop=="onion"&&HasNeighbour(index,"carrot",false))result*=.5;
            return result;
        }
        public int CompanionYieldBonus(int index)
        {
            string crop=State.plots[index].cropId;
            if(crop!="corn"&&crop!="beans"&&crop!="pumpkin")return 0;
            foreach(string member in new[]{"corn","beans","pumpkin"})if(crop!=member&&!HasNeighbour(index,member,true))return 0;
            return (int)Math.Ceiling(CropCatalog.Find(crop).yield*.15);
        }
    }
}
