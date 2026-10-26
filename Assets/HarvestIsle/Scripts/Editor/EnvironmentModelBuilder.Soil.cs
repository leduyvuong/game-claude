using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class EnvironmentModelBuilder
    {
        // One continuous rounded surface: no boxes or separate floating furrow tubes.
        private static void TilledSoil(EnvironmentMesh mesh,int material)
        {
            const int steps=48;
            var points=new Vector3[steps+1,steps+1];
            var colors=new Color[steps+1,steps+1];
            for(int x=0;x<=steps;x++)for(int z=0;z<=steps;z++)
            {
                float u=x*2f/steps-1,v=z*2f/steps-1;
                float px=u*(.82f+.18f*Mathf.Sqrt(1-v*v));
                float pz=v*(.82f+.18f*Mathf.Sqrt(1-u*u));
                float edge=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.76f,1,Mathf.Max(Mathf.Abs(u),Mathf.Abs(v))));
                float wave=.5f+.5f*Mathf.Cos((px+.012f*Mathf.Sin(pz*9))*Mathf.PI*6);
                float grain=Mathf.PerlinNoise(px*19+10,pz*19+10)-.5f;
                float y=Mathf.Lerp(-.15f,-.075f+.075f*wave+grain*.012f,edge);
                points[x,z]=new Vector3(px,y,pz);
                float shade=.83f+.17f*wave+grain*.08f;
                colors[x,z]=new Color(shade,shade,shade,1);
            }
            for(int x=0;x<steps;x++)for(int z=0;z<steps;z++)
            {
                mesh.Tri(points[x,z],points[x,z+1],points[x+1,z+1],material,colors[x,z],colors[x,z+1],colors[x+1,z+1]);
                mesh.Tri(points[x,z],points[x+1,z+1],points[x+1,z],material,colors[x,z],colors[x+1,z+1],colors[x+1,z]);
            }
            for(int i=0;i<steps;i++)
            {
                SoilSkirt(mesh,points[0,i+1],points[0,i],material);
                SoilSkirt(mesh,points[steps,i],points[steps,i+1],material);
                SoilSkirt(mesh,points[i,0],points[i+1,0],material);
                SoilSkirt(mesh,points[i+1,steps],points[i,steps],material);
            }
        }
        private static void SoilSkirt(EnvironmentMesh mesh,Vector3 a,Vector3 b,int material)
        {
            Vector3 bottomA=new Vector3(a.x,-.22f,a.z),bottomB=new Vector3(b.x,-.22f,b.z);
            mesh.Quad(a,b,bottomB,bottomA,material);
            mesh.Tri(new Vector3(0,-.22f,0),bottomA,bottomB,material);
        }
    }
}
