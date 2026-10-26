using System;

namespace HarvestIsle.Core
{
    // Three bounded rounds: one opportunity per round, no negative yield or repeat rewards.
    public sealed class HarvestChallenge
    {
        public readonly string CropId;
        public readonly int[] Sequence;
        public readonly double Window;
        public readonly double Speed;
        public int Round {get;private set;}
        public int Score {get;private set;}
        public bool Done {get{return Round>=3;}}
        public HarvestChallenge(string cropId,int harvested,int seed)
        {
            CropId=cropId;int level=Math.Min(6,Math.Max(0,harvested)/5);
            Window=.2-level*.015;Speed=1+level*.12;
            var random=new Random(seed);Sequence=new[]{random.Next(3),random.Next(3),random.Next(3)};
        }
        public void Timing(double value)
        {
            if(Done||CropId=="corn"||double.IsNaN(value)||double.IsInfinity(value))return;
            if(Math.Abs(value-.5)<=Window/2)Score++;Round++;
        }
        public void Choose(int choice)
        {
            if(Done||CropId!="corn"||choice<0||choice>2)return;
            if(choice==Sequence[Round])Score++;Round++;
        }
    }
}
