using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmWorld
    {
        public const float IslandScale = 6f;
        private static float ExpandedHeight(float x,float z)=>IslandTerrainProfile.Height(x/IslandScale,z/IslandScale);
        private void BuildExpandedLand()
        {
            // Keep the inner farm unchanged and leave broad clearings for later construction.
            string[] trees={"RainTree","BanyanTree","PineTree","WillowTree","FlameTree","ArecaPalm","YoungForestTree"};
            for(int i=0;i<42;i++)
            {
                float angle=i*Mathf.PI*2/42;
                // Leave the southwest approach to the harbor open.
                if(angle>3.55f&&angle<4.05f)continue;
                float radius=.76f+.07f*Mathf.Sin(i*7.13f);
                Vector3 p=new Vector3(Mathf.Cos(angle)*18.2f*radius,0,Mathf.Sin(angle)*15.4f*radius);
                if(InFarmClearing(p)||InTownZone(p))continue;
                p.y=ExpandedHeight(p.x,p.z);
                EnvironmentModel(trees[i%trees.Length],p,.65f+(i%4)*.12f);
                if(i%4==0)Flowers(p+new Vector3(.7f,.015f,.25f));
            }
            EnvironmentModel("Lake",new Vector3(11.8f,.15f,3.7f),.92f);
            // The old pond spot now hosts the west-cape town; keep it as a quiet
            // lagoon by the south-east shore instead.
            EnvironmentModel("Pond",new Vector3(10.8f,.15f,-10.2f),.9f);
            for(int i=0;i<20;i++)
            {
                float angle=i*2.399963f;
                Vector3 p=new Vector3(Mathf.Cos(angle)*17,0,Mathf.Sin(angle)*14.3f);
                if(InFarmClearing(p)||InTownZone(p))continue;
                p.y=ExpandedHeight(p.x,p.z);
                EnvironmentModel(i%3==0?"RockCluster":"RockSmall",p,.45f+(i%3)*.15f);
            }
            // Broad outer landscape, with clear road corridors and room for future use.
            for(int i=0;i<230;i++)
            {
                float angle=i*2.399963f;
                float radius=Mathf.Sqrt(.12f+.68f*Mathf.Repeat(Mathf.Sin(i*127.1f+31.7f)*43758.5453f,1));
                var p=new Vector3(Mathf.Cos(angle)*54.6f*radius,0,Mathf.Sin(angle)*46.2f*radius);
                if(p.sqrMagnitude<225||InTownZone(p)||InFarmClearing(p))continue;
                if(Mathf.Abs(p.z-7)<2.5f||Mathf.Abs(p.x-18)<2||Mathf.Abs(p.z+17)<2)continue;
                if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(29,13))<9)continue;
                p.y=ExpandedHeight(p.x,p.z);
                EnvironmentModel(trees[i%trees.Length],p,.8f+(i%5)*.16f);
                if(i%6==0)EnvironmentModel("RockCluster",p+new Vector3(1.4f,0,.6f),.7f);
                if(i%8==0)Flowers(p+new Vector3(-1,.02f,.8f));
            }
            EnvironmentModel("Lake",new Vector3(29,.15f,13),3.1f);
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4;
                EnvironmentModel(i%2==0?"WillowTree":"ArecaPalm",new Vector3(29+Mathf.Cos(a)*8,.15f,13+Mathf.Sin(a)*6),1.2f);
            }
            Road(new Vector3(10,0,7),new Vector3(33,0,7));
            Road(new Vector3(18,0,7),new Vector3(18,0,-17));
            Road(new Vector3(-27,0,-17),new Vector3(34,0,-17));
            Road(new Vector3(-27,0,-17),new Vector3(-43,0,-28));
            for(int i=0;i<7;i++)
            {
                EnvironmentModel("ArecaPalm",new Vector3(20+i*3,.15f,-20),1.15f);
                Prop("Bench",new Vector3(21+i*3,.15f,-18.5f),.6f,180);
            }
        }
    }
}
