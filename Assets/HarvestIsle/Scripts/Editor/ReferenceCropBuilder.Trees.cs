using UnityEngine;
using static HarvestIsle.Editor.CropMesh;

namespace HarvestIsle.Editor
{
    internal static partial class ReferenceCropBuilder
    {
        private static void Orchard(CropMesh m,int s,string name)
        {
            if(s==1){Sprout(m,.16f);return;}
            bool mango=name=="Mango",lemon=name=="Lemon";
            float h=s==2?.48f:s==3?.94f:s==4?1.10f:1.20f;
            m.Tube(new[]{Vector3.zero,new Vector3(.025f,h*.45f,0),Vector3.up*h},.022f+s*.007f,.012f,Wood,8);
            int branches=s==2?3:s+5;
            for(int i=0;i<branches;i++)
            {
                Vector3 d=D(i*2.4f),start=Vector3.up*h*(.35f+i%3*.10f),end=d*(s==2?.14f:.35f+i%2*.12f)+Vector3.up*h*(.66f+i%3*.13f);
                Rod(m,start,end,.009f+s*.002f,Wood);
                for(int j=0;j<(s==2?3:10);j++)
                {
                    Vector3 p=Vector3.Lerp(start,end,.35f+j*.075f);
                    Vein(m,p,D(i*2.4f+(j%2==0?-1:1)*.9f),mango?.30f:.21f,mango?.058f:.068f,mango?-.10f:.35f,j%3==0?LeafLight:Green);
                }
                if(s>=4)
                {
                    Vector3 p=end-Vector3.up*(mango?.18f:.09f);
                    Rod(m,end,p,.004f);
                    float r=s==5?.115f:.07f;
                    int color=s==4?Young:mango?Orange:lemon?LeafLight:Orange;
                    m.Ellipsoid(p,new Vector3(r,mango?r*1.55f:lemon?r*1.18f:r,r*.9f),color,Quaternion.Euler(0,0,mango?20:0),sides:12,rows:8);
                    if(lemon)m.Ellipsoid(p-Vector3.up*r*1.15f,new Vector3(.02f,.025f,.02f),color,sides:6,rows:4);
                }
            }
        }
        private static void Tropical(CropMesh m,int s,string name)
        {
            bool coconut=name=="Coconut",banana=name=="Banana";
            if(s==1)
            {
                if(coconut)m.Ellipsoid(Vector3.up*.08f,new Vector3(.15f,.10f,.12f),Wood);
                if(banana)m.Profile(Vector3.zero,new[]{new Vector3(.035f,0,.035f),new Vector3(.032f,.17f,.032f),new Vector3(.008f,.25f,.008f)},Young);
                else if(!coconut){Sprout(m,.13f);return;}
                for(int i=0;i<2;i++)m.Leaf(Vector3.up*.10f,D(i*2.5f),.25f,.035f,1.1f,Green);
                return;
            }
            float h=s==2?.30f:s==3?.83f:s==4?1.13f:1.27f;
            int trunkColor=coconut?Wood:Young;
            m.Tube(new[]{Vector3.zero,new Vector3(.04f,h*.55f,0),new Vector3(0,h,0)},.04f+s*.009f,.032f,trunkColor,10);
            if(coconut)for(int i=0;i<s*3;i++)m.Ellipsoid(new Vector3(.025f,h*i/(s*3),0),new Vector3(.052f,.012f,.052f),OrangeDark,sides:10,rows:3);
            else if(!banana)for(int i=0;i<s*2;i++)m.Ellipsoid(D(i*2.4f)*.048f+Vector3.up*(h*i/(s*2)),new Vector3(.02f,.025f,.01f),Cream,sides:4,rows:3);
            for(int i=0;i<(s==2?3:s+3);i++)
            {
                Vector3 d=D(i*2.4f),p=Vector3.up*(h-i%3*.08f);
                float length=s==2?.38f:.67f+(i%2)*.13f;
                if(coconut)
                {
                    Vector3 end=p+d*length-Vector3.up*.12f;
                    m.Tube(new[]{p,p+d*length*.5f+Vector3.up*.12f,end},.012f,.003f,Stem);
                    Vector3 side=Vector3.Cross(Vector3.up,d);
                    for(int j=1;j<9;j++)for(int sign=-1;sign<=1;sign+=2)
                        m.Leaf(Vector3.Lerp(p,end,j/9f)+Vector3.up*Mathf.Sin(j/9f*Mathf.PI)*.12f,(d*.45f+side*sign).normalized,.23f*(1-j*.07f),.018f,-.4f,Green,bend:.25f);
                }
                else if(banana)
                {
                    Rod(m,p,p+d*.14f+Vector3.up*.13f,.017f);
                    Vein(m,p+d*.14f+Vector3.up*.13f,d,length,.10f+s*.014f,.15f,Green,5);
                }
                else
                {
                    Vector3 end=p+d*(s==2?.15f:.34f)+Vector3.up*.10f;
                    Rod(m,p,end,.008f);
                    for(int lobe=0;lobe<7;lobe++)Vein(m,end,D(i*2.4f+(lobe-3)*.52f),s==2?.16f:.28f,s==2?.035f:.064f,-.15f,Green,2);
                }
            }
            if(s<4)return;
            if(banana)
            {
                Vector3 origin=new Vector3(.20f,h*.64f,-.10f);
                Rod(m,Vector3.up*h,origin,.015f);
                for(int row=0;row<3;row++)for(int j=0;j<6;j++)
                {
                    Vector3 d=D(j*1.047f),p=origin+Vector3.down*row*.12f+d*.06f;
                    m.Tube(new[]{p,p+d*.075f+Vector3.up*.035f,p+d*.105f+Vector3.up*.14f},.033f,.012f,s==5?Yellow:Young,8);
                }
                m.Ellipsoid(origin+Vector3.down*.42f,new Vector3(.065f,.13f,.065f),Purple);
            }
            else for(int i=0;i<(s==5?7:4);i++)
            {
                Vector3 p=D(i*2.4f)*(coconut?.13f:.105f)+Vector3.up*(h-(coconut?.12f:.27f)-i%2*.13f);
                float r=s==5?.13f:.085f;
                m.Ellipsoid(p,new Vector3(r,coconut?r*1.1f:r*1.7f,r),s==5?(coconut?Yellow:i%3==0?Young:Orange):Young,sides:12,rows:6,lobes:coconut?0:5);
            }
        }
    }
}
