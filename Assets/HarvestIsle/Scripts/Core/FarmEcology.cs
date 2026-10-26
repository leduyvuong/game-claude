using System;

namespace HarvestIsle.Core
{
    public sealed partial class FarmModel
    {
        public const int DrainageCost=12;
        public const int TreatCost=3;
        public const string CurrencyName = "Xu Mùa Vàng";
        // One stored coin is one xu. Prices and existing balances retain their purchasing power.
        public static string Money(long coins)
        {
            return coins.ToString("#,0",System.Globalization.CultureInfo.InvariantCulture).Replace(",",".")+" xu";
        }
        public static int FloodYieldLoss(double soilWater){return soilWater>=.85?2:soilWater>=.55?1:0;}
        public static bool IsWaterCrop(string id){return id=="lotus"||id=="waterspinach"||id=="taro";}
        // The weather clock only advances during active gameplay. Growth still uses UTC.
        public int RainLevel { get { return State.climateTime<90||State.climateTime>=230?0:State.climateTime<190?1:2; } }
        public double WeatherRemaining { get { return (State.climateTime<90?90:State.climateTime<190?190:State.climateTime<230?230:300)-State.climateTime; } }
        public static bool IsLowPlot(int index){return index/Width==0||index%Width==1||index%Width==4;}
        public void AdvanceEcology(double seconds)
        {
            if(!IsFinite(seconds)||seconds<=0)return;
            State.ecologyRemainder+=Math.Min(seconds,300);
            int steps=(int)State.ecologyRemainder;State.ecologyRemainder-=steps;
            for(int step=0;step<steps;step++)
            {
                State.climateTime=(State.climateTime+1)%300;
                int total=State.plots.Count;
                double[] old=new double[total];for(int i=0;i<total;i++)old[i]=State.plots[i].disease;
                for(int i=0;i<total;i++)
                {
                    PlotState p=State.plots[i];
                    p.drainProtection=Math.Max(0,p.drainProtection-1);p.treatmentProtection=Math.Max(0,p.treatmentProtection-1);
                    double inflow=RainLevel*(IsLowPlot(i)?.015:.007);
                    if(p.drainProtection>0)inflow*=.12;
                    if(p.drainageBuilt)inflow*=.25;
                    if(p.greenhouse)inflow=0;
                    p.soilWater=Math.Max(0,Math.Min(1,p.soilWater+inflow-(RainLevel==0?.009:.003)-(p.drainageBuilt?.005:0)));
                    if(string.IsNullOrEmpty(p.cropId))continue;
                    if(RainLevel>0&&!p.greenhouse)p.watered=true;
                    double tolerance=p.cropId=="corn"?.72:p.cropId=="pumpkin"?.62:.55;
                    double risk=!IsWaterCrop(p.cropId)&&p.soilWater>tolerance?.8:0;
                    if(p.soilWater>.25)
                        for(int j=0;j<total;j++)
                            if(!string.IsNullOrEmpty(State.plots[j].cropId)&&old[j]>=35&&Math.Abs(i%Width-j%Width)+Math.Abs(i/Width-j/Width)==1)risk+=.3;
                    if(IsMonoculture(i))risk*=2;
                    else if(!string.IsNullOrEmpty(p.previousCropId)&&CropGroup(p.previousCropId)!=CropGroup(p.cropId))risk*=.5;
                    risk*=CompanionProtection(i);
                    if(p.treatmentProtection>0)risk=0;
                    p.disease=Math.Max(0,Math.Min(100,p.disease+risk-(RainLevel==0&&p.soilWater<.25?.12:0)));
                    // Pests sneak onto warm dry plots; deterministic so the rules stay testable.
                    if(p.pest==0&&RainLevel==0&&p.soilWater<.35&&p.disease<35&&p.treatmentProtection<=0&&
                        ((int)State.climateTime*31+i*17+(int)(p.plantedAt%97))%173==7)p.pest=1;
                }
            }
        }
        private ActionResult Care(int index,FarmTool tool)
        {
            PlotState p=State.plots[index];
            if(tool==FarmTool.BuildDrain)
            {
                if(p.drainageBuilt)return Fail("Ô này đã có rãnh cố định.");
                if(State.coins<DrainageCost)return Fail("Cần "+Money(DrainageCost)+" xây rãnh. Có thể thoát nước miễn phí trước.");
                State.coins-=DrainageCost;p.drainageBuilt=true;
                return Ok("Đã xây rãnh cố định · giảm 75% nước mưa vào ô và tự rút nước. Dùng Thoát nước để cứu úng ngay.");
            }
            if(tool==FarmTool.Drain)
            {
                if(p.soilWater<.15&&p.drainProtection>0)return Fail("Rãnh vẫn đang thoát nước tốt.");
                p.soilWater=.05;p.drainProtection=45;return Ok("Đã thoát nước · rãnh bảo vệ 45 giây, miễn phí.");
            }
            if(string.IsNullOrEmpty(p.cropId)||p.disease<1)return Fail("Cây chưa cần trị bệnh.");
            if(State.coins<TreatCost)return Fail("Cần "+Money(TreatCost)+" trị bệnh. Có thể thoát nước miễn phí để giảm rủi ro.");
            State.coins-=TreatCost;p.disease=Math.Max(0,p.disease-50);p.treatmentProtection=45;
            return Ok("Đã trị bệnh · giảm 50 điểm bệnh, bảo vệ 45 giây.");
        }
        public int HarvestYield(int index,int score)
        {
            PlotState p=State.plots[index];CropSpec crop=CropCatalog.Find(p.cropId);if(crop==null)return 0;
            int rotation=RotationYieldBonus(index);
            int loss=(p.disease>=70?2:p.disease>=35?1:0)+(IsWaterCrop(p.cropId)?0:FloodYieldLoss(p.soilWater))+p.pest;
            // Owned bees pollinate fruiting plots whenever any flowers are in bloom.
            int pollination=State.bee.count>0&&FloweringPlots()>0&&CropGroup(p.cropId)=="Quả"?1:0;
            return Math.Max(1,crop.yield+rotation+CompanionYieldBonus(index)+p.soilQuality+pollination+Math.Max(0,Math.Min(3,score))-1-loss)+IslandPruneBonus(index);
        }
        private static bool ValidEcology(PlotState p)
        {
            return IsFinite(p.soilWater)&&p.soilWater>=0&&p.soilWater<=1&&IsFinite(p.disease)&&p.disease>=0&&p.disease<=100&&
                IsFinite(p.drainProtection)&&p.drainProtection>=0&&p.drainProtection<=45&&IsFinite(p.treatmentProtection)&&p.treatmentProtection>=0&&p.treatmentProtection<=45;
        }
    }
}
