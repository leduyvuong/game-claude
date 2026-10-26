using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        // Sloped surfaces share one coordinate system: crest + downhill*t.
        // Detail is actual geometry, authored into the existing static meshes.
        private static void RoofSkin(EnvironmentMesh m,Vector3 crest,Vector3 downhill,float width,int material)
        {
            bool tile=material==Tile,straw=material==Straw,metal=material==Water;
            int cols=Mathf.CeilToInt(width/(tile?.23f:straw?.15f:.22f));
            int rows=metal?1:Mathf.Max(2,Mathf.CeilToInt(downhill.magnitude/(tile?.25f:.29f)));
            float step=width/cols;
            for(int c=0;c<cols;c++)for(int row=0;row<rows;row++)
            {
                float x=-width/2+(c+.5f)*step,t0=(float)row/rows,t1=(float)(row+1)/rows;
                Vector3 start=crest+Vector3.right*x+downhill*t0,end=crest+Vector3.right*x+downhill*t1;
                int color=tile?((c*7+row*11)%19<2?TileLight:(c*7+row*11)%19<5?TileDark:Tile):straw?((c+row*3)%7==0?Bamboo:Straw):material;
                if(tile||metal)
                {
                    int arcs=4;float curve=tile?.043f:.024f;
                    for(int a=0;a<arcs;a++)
                    {
                        float aa=a*Mathf.PI/arcs,ab=(a+1)*Mathf.PI/arcs;
                        Vector3 left=new Vector3(Mathf.Cos(aa)*step*.49f,Mathf.Sin(aa)*curve,0),right=new Vector3(Mathf.Cos(ab)*step*.49f,Mathf.Sin(ab)*curve,0);
                        Vector3 p=start+left+Vector3.up*.018f,q=start+right+Vector3.up*.018f,r=end+right+Vector3.up*.047f,s=end+left+Vector3.up*.047f;
                        Face(m,p,q,r,s,color);
                        Face(m,s,r,r-Vector3.up*.027f,s-Vector3.up*.027f,tile?TileDark:Stone);
                    }
                }
                else
                {
                    float extra=straw?((c*13+row)%5)*.006f:0;
                    Vector3 left=Vector3.left*(step*.49f),right=-left;
                    Face(m,start+left+Vector3.up*.018f,start+right+Vector3.up*.018f,end+right+Vector3.up*(.04f+extra),end+left+Vector3.up*(.04f+extra),color);
                    for(int reed=0;reed<(straw?3:1);reed++)
                    {
                        float dx=(reed-1)*step*.23f;
                        Vector3 p=start+Vector3.right*dx+Vector3.up*.022f,q=end+Vector3.right*dx+Vector3.up*(.045f+extra);
                        Face(m,p-Vector3.right*.004f,p+Vector3.right*.004f,q+Vector3.right*.004f,q-Vector3.right*.004f,straw?Bamboo:StoneDark);
                    }
                }
            }
            // End grain and eave depth remain visible from the gameplay view.
            Vector3 eave=crest+downhill;
            Box(m,eave-Vector3.up*.045f,new Vector3(width+.05f,.10f,.08f),WoodDark);
            int tails=Mathf.Max(3,Mathf.CeilToInt(width/.40f));
            for(int i=0;i<=tails;i++)
            {
                Vector3 p=crest+Vector3.right*(-width/2+i*width/tails);
                Beam(m,p+downhill*.84f-Vector3.up*.08f,p+downhill*1.025f-Vector3.up*.08f,.036f,Wood);
            }
        }
        private static void RoofCaps(EnvironmentMesh m,Vector3 center,float width,bool tiled)
        {
            int count=Mathf.CeilToInt(width/.25f);float step=width/count;
            for(int i=0;i<count;i++)
            {
                float x=-width/2+(i+.5f)*step;
                if(tiled)
                {
                    for(int a=0;a<6;a++)
                    {
                        float aa=a*Mathf.PI/6,ab=(a+1)*Mathf.PI/6;
                        Vector3 p=center+new Vector3(x-step*.46f,Mathf.Sin(aa)*.093f,Mathf.Cos(aa)*.10f);
                        Vector3 q=center+new Vector3(x-step*.46f,Mathf.Sin(ab)*.093f,Mathf.Cos(ab)*.10f);
                        Face(m,p,q,q+Vector3.right*step*.94f,p+Vector3.right*step*.94f,i%5==0?TileLight:Tile);
                    }
                }
                else
                    Hoop(m,center+Vector3.right*x,Vector3.right,.075f,.013f,Bamboo,10);
            }
        }
    }
}
