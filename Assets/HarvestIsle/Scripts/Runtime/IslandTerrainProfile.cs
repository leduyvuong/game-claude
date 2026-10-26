using UnityEngine;

namespace HarvestIsle
{
    // Shared by the baked island surface and scenery placement. Farm plots remain level.
    public static class IslandTerrainProfile
    {
        public static float Height(float x,float z)
        {
            float rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.85f,7.0f,z));
            float width=Mathf.Exp(-x*x/10f);
            float crest=3.85f+.55f*Mathf.Cos(x*1.5f)+.24f*Mathf.Sin(z*2+x);
            float a=Mathf.Atan2(z/7.7f,x/9.1f);
            float shape=1+Mathf.Sin(a*3+4)*.055f+Mathf.Cos(a*5+3.2f)*.035f;
            float radial=Mathf.Sqrt(x*x/(9.1f*9.1f)+z*z/(7.7f*7.7f))/shape;
            float coast=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.86f,1,radial));
            return .15f+rise*width*crest*coast;
        }
    }
}
