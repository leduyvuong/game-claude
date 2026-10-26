using UnityEngine;
using static HarvestIsle.Editor.CropMesh;

namespace HarvestIsle.Editor
{
    internal static partial class ReferenceCropBuilder
    {
        private static void Vine(CropMesh m, int s, bool melon)
        {
            if (s == 1) { Sprout(m); return; }
            for (int k = 0; k < s; k++)
            {
                Vector3 d = D(k * 2.4f), end = d * (.10f + s * .065f) + Vector3.up * .045f;
                m.Tube(new[] { Vector3.up * .035f, end * .5f + Vector3.up * .055f, end }, .012f, .006f, Stem);
                for (int j = 0; j < 3; j++)
                    Vein(m, end * (.3f + j * .3f), D(k * 2.4f + j - 1), .12f + s * .035f, melon ? .07f : .12f, .4f, Green, melon ? 4 : 2);
                var curl = new Vector3[10];
                for (int j = 0; j < curl.Length; j++) curl[j] = end + D(j * .65f) * (.04f * (1 - j * .07f)) + Vector3.up * (.035f + j * .003f);
                m.Tube(curl, .003f, .001f, LeafLight, 4);
            }
            if (s < 4) return;
            for (int k = 0; k < (s == 5 ? 2 : 1); k++)
            {
                float r = s == 5 ? .21f : .09f;
                Vector3 p = D(k * 2.8f + .3f) * .19f + Vector3.up * r * .75f;
                m.Ellipsoid(p, new Vector3(r, r * .9f, r), melon ? MelonDark : s == 5 ? Orange : Young,
                    sides: 24, rows: 8, lobes: melon ? 0 : 10, stripeColor: melon ? MelonLight : -1);
                Rod(m, p + Vector3.up * r * .8f, p + Vector3.up * (r + .035f), .02f, Wood);
            }
            m.Flower(new Vector3(-.2f,.15f,-.16f), .06f, Yellow);
        }
        private static void Climber(CropMesh m, int s, bool peas)
        {
            if (!peas || s >= 3)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Rod(m, new Vector3(side * .24f,0,0), new Vector3(side * .24f,.91f,0), .018f, Wood);
                    m.Tube(new[] { new Vector3(side*.24f-.035f,.75f,-.025f),new Vector3(side*.24f+.035f,.80f,.025f) }, .01f,.01f,Cream);
                }
                Rod(m, new Vector3(-.28f,.79f,0), new Vector3(.28f,.79f,0), .016f, Wood);
            }
            if (s == 1) { Sprout(m); return; }
            float h = .12f + s * .145f;
            for (int k = 0; k < 2; k++)
            {
                var path = new Vector3[12];
                for (int j = 0; j < path.Length; j++) path[j] = new Vector3((k == 0 ? -.15f : .15f) + Mathf.Sin(j * 1.5f) * .035f, h * j / 11, Mathf.Cos(j * 1.5f) * .035f);
                m.Tube(path,.009f,.004f,Stem);
                for (int j = 1; j < s * 2; j++)
                {
                    Vector3 p = new Vector3(k == 0 ? -.15f : .15f,h * j / (s * 2),0);
                    for (int side = -1; side <= 1; side += 2) Vein(m,p,D(j * .8f + side * 1.6f),.19f,.075f,.3f,Green,peas ? 0 : 1);
                }
            }
            if (s < 4) return;
            for (int k = 0; k < (s == 5 ? 8 : 3); k++)
            {
                Vector3 p = new Vector3((k % 3 - 1) * .17f,.30f + k / 3 * .17f,k % 2 == 0 ? .20f : -.20f);
                m.Ellipsoid(p,new Vector3(peas ? .044f : .026f,s == 5 ? .17f : .08f,.028f),LeafLight);
                Rod(m,p+Vector3.up*.13f,p+Vector3.up*.17f,.004f);
                if (peas) for (int j = 0; j < 4; j++) m.Ellipsoid(p + new Vector3(0,-.08f+j*.052f,-.016f),Vector3.one*.026f,Young,sides:8,rows:4);
                if (s == 4) m.Flower(p+new Vector3(.07f,.13f,0),.045f,White);
            }
        }
        private static void Rosette(CropMesh m, int s, bool cabbage)
        {
            for (int i = 0; i < s * 2 + 1; i++)
                Vein(m,Vector3.up*.04f,D(i*2.4f),.11f+s*.075f,.025f+s*.024f,.4f+(i%3)*.25f,
                    cabbage ? Dark : i%2==0?Green:LeafLight, cabbage?1:2);
            if (cabbage && s >= 4)
            {
                float r = s == 4 ? .14f : .23f;
                m.Ellipsoid(Vector3.up*(r*.85f),new Vector3(r,r*.9f,r),LeafLight,sides:16,rows:8,lobes:4);
                for (int i=0;i<7;i++) Vein(m,Vector3.up*.04f,D(i*2.4f),r*1.4f,r*.6f,1.3f,Young);
            }
        }
        private static void BushFruit(CropMesh m, int s, bool chili)
        {
            if(s==1){Sprout(m);return;}
            float h=.12f+s*.12f;
            Rod(m,Vector3.zero,Vector3.up*h,.013f);
            if(!chili && s>=3)Rod(m,new Vector3(.045f,0,.03f),new Vector3(.045f,h+.14f,.03f),.015f,Wood);
            for(int i=0;i<s+2;i++)
            {
                Vector3 p=Vector3.up*h*(.25f+i*.09f),d=D(i*2.4f),end=p+d*(.08f+s*.027f)+Vector3.up*.1f;
                Rod(m,p,end,.007f);
                for(int j=0;j<3;j++)Vein(m,Vector3.Lerp(p,end,.4f+j*.25f),D(i*2.4f+(j-1)*.9f),.16f,.045f,.25f,Green,chili?0:3);
                if(s>=4)
                {
                    Vector3 q=end-Vector3.up*.055f;
                    if(chili)m.Tube(new[]{q-Vector3.up*.08f,q+d*.045f,q+Vector3.up*.12f+d*.06f},s==5?.035f:.018f,.001f,s==5?Red:Young,8);
                    else {m.Ellipsoid(q,new Vector3(s==5?.10f:.055f,s==5?.09f:.05f,s==5?.10f:.055f),s==5?Red:Young,lobes:5);for(int k=0;k<5;k++)m.Leaf(q+Vector3.up*(s==5?.087f:.046f),D(k*1.26f),.045f,.01f,.1f,Dark);}
                    if(s==4)m.Flower(end+Vector3.up*.045f,.033f,chili?White:Yellow);
                }
            }
        }
        private static void Strawberry(CropMesh m,int s)
        {
            for(int i=0;i<s+1;i++)
            {
                Vector3 p=D(i*2.4f)*(.02f+s*.016f)+Vector3.up*(.05f+s*.025f);
                Rod(m,Vector3.zero,p,.006f);
                for(int j=0;j<3;j++)Vein(m,p,D(i*2.4f+(j-1)*.8f),.06f+s*.026f,.025f+s*.008f,.15f,Green,5);
            }
            if(s<4)return;
            for(int i=0;i<(s==5?5:3);i++)
            {
                Vector3 p=D(i*2.4f)*.22f+Vector3.up*.10f;
                m.Profile(p,new[]{new Vector3(.005f,-.08f,.005f),new Vector3(.065f,.035f,.065f),new Vector3(.045f,.075f,.045f)},s==5?Red:Cream);
                for(int j=0;j<12;j++)m.Ellipsoid(p+D(j*2.4f)*.055f+Vector3.up*(j%3*.035f-.02f),new Vector3(.004f,.007f,.004f),Yellow,sides:5,rows:3);
                for(int j=0;j<4;j++)m.Leaf(p+Vector3.up*.075f,D(j*1.57f),.05f,.014f,.2f,Green);
            }
            m.Flower(new Vector3(.1f,.26f,-.1f),.055f,White);
        }
        private static void Marigold(CropMesh m,int s)
        {
            if(s==1){Sprout(m,.06f);return;}
            for(int i=0;i<s+1;i++)
            {
                Vector3 d=D(i*2.4f),top=d*.16f+Vector3.up*(.12f+s*.07f+i%2*.10f);
                Rod(m,Vector3.zero,top,.007f);
                for(int j=0;j<3;j++)Fern(m,top*(.3f+j*.15f),D(i+j*2),.19f,.25f);
                if(s==4 && i>0)m.Ellipsoid(top,new Vector3(.036f,.065f,.036f),Young);
                else if(s>=4)for(int layer=0;layer<4;layer++)m.Flower(top+Vector3.up*layer*.015f,(s==5?.11f:.075f)*(1-layer*.18f),i%2==0?Yellow:Orange,10);
            }
        }
        private static void Spinach(CropMesh m,int s)
        {
            for(int i=0;i<s*2;i++)
            {
                Vector3 p=D(i*2.4f)*(.025f+s*.02f)+Vector3.up*(.06f+s*.065f+i%2*.035f);
                Rod(m,D(i*2.4f)*.025f,p,.009f);
                for(int j=0;j<(s<3?1:3);j++)Vein(m,p-Vector3.up*j*.06f,D(i*2.4f+j*2),.07f+s*.035f,.018f+s*.008f,.25f);
            }
        }
        private static void Taro(CropMesh m,int s)
        {
            m.Ellipsoid(Vector3.up*.025f,new Vector3(.02f+s*.02f,.025f+s*.025f,.02f+s*.02f),Wood);
            for(int i=0;i<(s==1?1:s);i++)
            {
                Vector3 d=D(i*2.4f),p=d*.10f+Vector3.up*(.07f+s*.10f+i%2*.07f);
                Rod(m,Vector3.zero,p,.01f+s*.002f);
                Vein(m,p,d,.07f+s*.075f,.015f+s*.03f,-.15f,Green,1);
                if(s>1)for(int side=-1;side<=1;side+=2)Vein(m,p,D(i*2.4f+side*.65f),.04f+s*.045f,.015f+s*.024f,.1f,Green);
            }
        }
        private static void Lotus(CropMesh m,int s)
        {
            if(s==1){m.Ellipsoid(Vector3.up*.08f,new Vector3(.027f,.085f,.027f),Young);return;}
            for(int i=0;i<s-1;i++)
            {
                Vector3 p=D(i*2.4f)*.20f+Vector3.up*(.05f+i%2*.28f);
                Rod(m,new Vector3(p.x,0,p.z),p,.009f);
                m.Ellipsoid(p,new Vector3(.10f+s*.019f,.015f,.10f+s*.019f),Green,sides:16,rows:4);
                for(int j=0;j<8;j++)Rod(m,p+Vector3.up*.017f,p+D(j*.785f)*(.09f+s*.016f)+Vector3.up*.008f,.0015f,LeafLight);
            }
            if(s<4)return;
            Vector3 flower=new Vector3(-.1f,.42f,-.12f);Rod(m,new Vector3(-.1f,0,-.12f),flower,.011f);
            for(int layer=0;layer<3;layer++)for(int j=0;j<8;j++)m.Leaf(flower+Vector3.up*layer*.018f,D(j*.785f+layer*.3f),.17f-layer*.03f,.048f,.5f+layer*.3f,Purple);
            m.Ellipsoid(flower+Vector3.up*.035f,new Vector3(.042f,.022f,.042f),Yellow);
            if(s==5){Vector3 pod=new Vector3(.08f,.52f,.09f);Rod(m,new Vector3(.08f,0,.09f),pod,.01f);m.Profile(pod,new[]{new Vector3(.03f,-.07f,.03f),new Vector3(.07f,0,.07f)},Young);for(int i=0;i<7;i++)m.Ellipsoid(pod+D(i*2.4f)*.045f+Vector3.up*.003f,Vector3.one*.009f,Dark,sides:6,rows:3);}
        }
        private static void Mushroom(CropMesh m,int s)
        {
            if(s==1)
            {
                for(int i=0;i<18;i++){Vector3 d=D(i*2.4f);m.Tube(new[]{Vector3.up*.008f,d*.09f+Vector3.up*.012f,d*.20f+D(i)*.04f+Vector3.up*.008f},.0025f,.0008f,White,4);for(int j=1;j<4;j++)Rod(m,d*j*.04f+Vector3.up*.01f,d*j*.04f+D(i+j)*.06f+Vector3.up*.01f,.001f,White);}return;
            }
            for(int i=0;i<s+1;i++)
            {
                float r=(.025f+(s-2)*.035f)*(1-i%3*.18f),h=.025f+(s-2)*.055f*(1-i%2*.2f);
                Vector3 p=D(i*2.4f)*(.08f+i%2*.09f);
                Rod(m,p,p+Vector3.up*h,r*.38f,White);
                m.Profile(p+Vector3.up*h,new[]{new Vector3(r,.005f,r),new Vector3(r*1.03f,.025f,r*1.03f),new Vector3(r*.72f,r*.65f,r*.72f),new Vector3(.005f,r*.86f,.005f)},s==2?Cream:Wood,16);
                if(s>=3)for(int j=0;j<5;j++)m.Ellipsoid(p+Vector3.up*(h+r*.69f)+D(j*2.4f)*r*.4f,new Vector3(.012f,.005f,.010f),Cream,sides:6,rows:3);
            }
        }
        private static void Tea(CropMesh m,int s)
        {
            if(s==1){Sprout(m);return;}
            for(int i=0;i<s+1;i++)
            {
                Vector3 top=D(i*2.4f)*(.05f+s*.04f)+Vector3.up*(.12f+s*.08f-i%2*.08f);
                Rod(m,Vector3.zero,top,.012f,Wood);
                for(int j=0;j<s+1;j++)Vein(m,top*(.35f+j*.11f),D(i*2.4f+j*2),.16f,.049f,.2f,Green,3);
                if(s>=4)for(int j=0;j<3;j++)m.Leaf(top,D(i+j*2),.09f,.021f,1f,Cream);
            }
        }
    }
}
