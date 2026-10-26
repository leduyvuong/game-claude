using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class IslandExpansionModels
    {
        static void Crop(EnvironmentMesh m,int n,string variant)
        {
            bool product=variant=="Product",disease=variant=="Diseased",wilt=variant=="Wilted";
            int stage=product?5:disease||wilt?4:int.Parse(variant);float growth=new[]{.16f,.32f,.62f,.84f,1f}[stage-1];int leaf=wilt?Brown:disease?Sage:n==12?Purple:Green;
            if(product){CropFruit(m,n,V(0,.22f,0),.8f,true);return;}
            if(n==4)
            {
                for(int stem=0;stem<3;stem++)
                {float a=stem*2.094f;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a)),tip=d*.35f*growth+V(0,.42f*growth,0);Beam(m,V(0,0,0),tip,.016f,LeafDark);
                    for(int row=0;row<2;row++){Vector3 p=Vector3.Lerp(Vector3.zero,tip,.48f+row*.46f);for(int side=-1;side<=1;side+=2){Vector3 v=Quaternion.Euler(0,side*70,0)*d;Leaf(m,p,v,.24f*growth,.105f*growth,leaf,.12f);}}
                    if(stage>=4)Flower(m,d*.12f+V(0,.12f,0),.065f,Gold);if(stage==5)CropFruit(m,n,d*.26f+V(0,.035f,0),.34f,true);
                }
            }
            else if(n==6)
            {
                for(int stem=0;stem<3;stem++){Vector3 o=V((stem-1)*.22f,0,(stem%2)*.12f);float h=(1.1f-stem*.09f)*growth;Beam(m,o,o+V(0,h,0),.018f,LeafDark);
                    for(int row=0;row<stage+2;row++){Vector3 p=o+V(0,h*(row+1)/(stage+3),0);Leaf(m,p,V(row%2==0?1:-1,0,.3f),.34f*growth,.07f*growth,leaf,.3f);if(stage>=4){Vector3 pod=p+V(row%2==0?.055f:-.055f,.02f,.04f);Ball(m,pod,V(.04f,.10f,.035f)*growth,stage==5?Brown:LeafLight,8,5);Beam(m,pod+V(0,.08f,0),pod+V(0,.12f,0),.009f,Straw);}}
                    if(stage==4)Flower(m,o+V(0,h*.88f,0),.075f,White);
                }
            }
            else if(n==10)
            {
                for(int i=0;i<4+stage*3;i++){float a=i*2.4f;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a));Leaf(m,V(0,.02f,0),d,(.65f+(i%3)*.1f)*growth,.055f*growth,leaf,.5f+(i%3)*.25f);}Beam(m,V(0,0,0),V(0,.2f*growth,0),.035f,LeafDark);
            }
            else CropExtended(m,n,stage,growth,leaf);
            if(disease)for(int i=0;i<7;i++){float a=i*2.4f;Ball(m,V(Mathf.Cos(a)*.22f,.22f+(i%3)*.13f,Mathf.Sin(a)*.22f),V(.035f,.012f,.024f),Brown,6,3);}
        }
        static void CropFruit(EnvironmentMesh m,int n,Vector3 p,float s,bool mature)
        {
            if(n==4){Ball(m,p+V(-.09f,0,0)*s,V(.14f,.10f,.09f)*s,Straw,10,5);Ball(m,p+V(.09f,0,0)*s,V(.14f,.10f,.09f)*s,Straw,10,5);for(int i=0;i<6;i++)Ring(m,p+V(-.19f+i*.075f,0,0)*s,.085f*s,.008f*s,Brown,Quaternion.Euler(0,0,90),10);}
            else if(n==6){for(int i=0;i<7;i++)Ball(m,p+V((i%3-.9f)*.09f,(i/3)*.045f,(i%2)*.08f)*s,V(.055f,.025f,.035f)*s,Straw,8,4);}
            else if(n==10){for(int i=0;i<5;i++)Leaf(m,p+V(0,i*.025f,0),V(1,0,(i-2)*.15f),.75f*s,.05f*s,Green,.03f);Ring(m,p+V(.23f*s,.025f,0),.085f*s,.012f,Bamboo,Quaternion.Euler(0,0,90));}
            else FruitExtended(m,n,p,s,mature);
        }
        static void CropExtended(EnvironmentMesh m,int n,int stage,float g,int leaf)
        {
            float droop=leaf==Brown?-.5f:.15f;
            if(n==1||n==2||n==3||n==5||n==11||n==12||n==13||n==15||n==16)
            {
                int branches=n==3?3:n==1||n==13?5:3;float height=n==1?.22f:n==13?.32f:n==3?1.15f:n==16?1.05f:n==15?.8f:.55f;
                for(int branch=0;branch<branches;branch++)
                {
                    float a=branch*2.4f;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a));Vector3 tip=d*(n==1?.5f:n==13?.35f:.25f)*g+V(0,height*g,0);Beam(m,Vector3.zero,tip,.014f+(n==3?.012f:0),n==3||n==15?Purple:LeafDark);
                    int nodes=stage<=2?1:3;
                    for(int k=0;k<nodes;k++)
                    {
                        Vector3 p=tip*(.4f+k*.25f);Vector3 side=Quaternion.Euler(0,60+k*100,0)*d;
                        if(n==3){for(int l=0;l<5;l++)Leaf(m,p,Quaternion.Euler(0,-70+l*35,0)*side,.36f*g,.045f*g,leaf,droop+.1f);}
                        else for(int sign=-1;sign<=1;sign+=2)Leaf(m,p,side*sign,(n==1?.33f:n==15?.32f:n==13?.17f:.23f)*g,(n==1?.13f:n==15?.13f:n==12?.11f:n==13?.06f:.085f)*g,leaf,droop,n==12?4:n==16||n==13?3:0);
                        if(stage>=4&&(n==5||n==15||n==16)){Vector3 f=p+d*.1f+V(0,.02f,0);if(stage==4&&k==2)Flower(m,f,.10f,n==15?Purple:White);else CropFruit(m,n,f,n==15?.42f:n==16?.45f:.32f,stage==5);}
                    }
                    if(stage==4&&(n==1||n==2))Flower(m,tip,.065f,n==1?Purple:White);
                    if(stage==5&&n<=3)CropFruit(m,n,d*.27f+V(0,.02f,0),.33f,true);
                }
            }
            else if(n==7||n==8||n==9)
            {
                for(int stem=0;stem<4;stem++)
                {float a=stem*2.4f;Vector3 o=V(Mathf.Cos(a)*.1f,0,Mathf.Sin(a)*.1f),top=o+V(Mathf.Cos(a)*.16f, (n==9?1.05f:n==8?.83f:.7f)*g,Mathf.Sin(a)*.16f);Beam(m,o,top,.022f,n==9?Red:LeafDark);
                    for(int k=0;k<stage+1;k++){Vector3 p=Vector3.Lerp(o,top,(k+1f)/(stage+2));Leaf(m,p,V(Mathf.Cos(a+(k%2)*Mathf.PI),0,Mathf.Sin(a+(k%2)*Mathf.PI)),(n==8?.56f:.4f)*g,(n==8?.14f:.055f)*g,leaf,droop+.1f);}
                }
                if(stage==5)CropFruit(m,n,V(0,.025f,0),.45f,true);
            }
            else if(n==14||n==17||n==18||n==19||n==20)
            {
                float h=n==20?1.35f:1.05f;
                for(int side=-1;side<=1;side+=2)Beam(m,V(side*.58f,0,0),V(side*.55f,h,0),.025f,Bamboo);
                for(int k=0;k<3;k++)Beam(m,V(-.55f,.35f+k*.32f,0),V(.55f,.35f+k*.32f,0),.018f,Bamboo);
                var points=new Vector3[7];for(int i=0;i<points.Length;i++)points[i]=V(Mathf.Sin(i*1.5f)*.12f,h*g*i/6,.025f);m.Tube(points,.018f,.008f,LeafDark,6);
                for(int i=0;i<2+stage*2;i++){float a=i*2.4f;Vector3 p=V(Mathf.Sin(a)*.45f*g,.15f+(i/(float)(2+stage*2))*(h-.2f)*g,.03f);Beam(m,V(0,p.y,.03f),p,.009f,LeafDark);Leaf(m,p,V(Mathf.Cos(a),0,(i%2==0?1:-1)*.7f),.32f*g,(n==18?.15f:.12f)*g,leaf,droop,n==17?4:n==19?3:0);if(stage>=4&&i%3==0){Vector3 f=p+V(0,-.06f,.14f);if(stage==4)Flower(m,f,.09f,n==18?White:n==20?Purple:Gold);else CropFruit(m,n,f,.48f,true);}}
            }
            else if(n==21)
            {
                Beam(m,V(0,0,0),V(0,1.1f,0),.065f,Stone);Ring(m,V(0,1.05f,0),.27f,.032f,Wood);int count=stage+1;
                for(int i=0;i<count;i++){float a=i*2.4f;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a));var path=new[]{V(0,.02f,0),d*.12f+V(0,.8f*g,0),d*.38f*g+V(0,1.15f*g,0),d*.57f*g+V(0,.72f*g,0)};m.Tube(path,.075f*g,.04f*g,leaf,3);if(stage==4&&i==0)Flower(m,path[3],.18f,White,8);if(stage==5&&i%2==0)CropFruit(m,n,path[3]+V(0,-.06f,0),.65f,true);}
            }
            else
            {
                bool coffee=n==29;float h=(coffee?1.35f:n==27?2.05f:n==28?1.9f:1.65f)*g;
                Beam(m,Vector3.zero,V(0,h,0),(.045f+g*.025f)*(coffee?.7f:1),Wood);
                int count=stage<=2?2:coffee?8:6;
                for(int branch=0;branch<count;branch++)
                {
                    float a=branch*2.4f;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a));float level=.45f+(branch%3)*.17f;Vector3 start=V(0,h*level,0),tip=start+d*(coffee?.55f:.6f)*g+V(0,.18f*g,0);Beam(m,start,tip,.018f*g,Wood);
                    int leaves=stage<=2||coffee?2:3;for(int k=0;k<leaves;k++){Vector3 p=Vector3.Lerp(start,tip,(k+1f)/(leaves+1));Vector3 dir=Quaternion.Euler(0,k%2==0?65:-65,0)*d;Leaf(m,p,dir,(n==26?.34f:n==27?.36f:coffee?.29f:.33f)*g,(n==27?.065f:n==26?.13f:coffee?.10f:.105f)*g,leaf,droop+.06f);Leaf(m,p,dir*-1,.28f*g,.085f*g,branch%3==0?LeafLight:leaf,droop);}
                    bool heavyFruit=n==23||n==24||n==25||n==27||n==28||n==29||n==30;
                    if(stage>=4&&(!heavyFruit||branch%2==0))
                    {
                        Vector3 fruit=tip+V(0,-.08f,.02f);if(n==27||n==28||n==30)fruit=start+d*.10f+V(0,-.12f,0);
                        if(stage==4){Flower(m,tip,.06f,n==26?Red:White);if(branch%2==0)CropFruit(m,n,fruit,.32f,false);}
                        else if(n!=27||branch%2==0)CropFruit(m,n,fruit,n==29?.38f:n==27||n==28?.65f:.52f,true);
                    }
                }
            }
        }
        static void FruitExtended(EnvironmentMesh m,int n,Vector3 p,float s,bool mature)
        {
            int color=!mature?LeafLight:n==1?Purple:n==2?Straw:n==3?Brown:n==5?Gold:n==7?Straw:n==8?Orange:n==9?Brown:n==15?Purple:n==20?Purple:n==21?Red:n==23||n==25||n==29?Red:n==24?Brown:n==26?Purple:n==30?Orange:Green;
            if(n<=3)
            {
                Vector3 radius=n==1?V(.30f,.11f,.12f):n==2?V(.21f,.17f,.16f):V(.31f,.09f,.08f);Ball(m,p,radius*s,color,12,6,Quaternion.Euler(0,25,20));
                if(n==2)for(int i=0;i<5;i++){float a=i*2.4f;Ball(m,p+V(Mathf.Cos(a)*.17f,.11f,Mathf.Sin(a)*.12f)*s,V(.015f,.01f,.015f)*s,Brown,6,3);}
                else for(int i=0;i<3;i++)Beam(m,p+V((i-1)*.14f,.01f,.09f)*s,p+V((i-1)*.16f,-.035f,.14f)*s,.006f*s,Brown);
            }
            else if(n==5){for(int i=0;i<3;i++)Ball(m,p+V(0,(i-1)*.11f,0)*s,V(.09f,.09f,.065f)*s,color,10,5);Beam(m,p+V(0,.20f,0)*s,p+V(0,.25f,0)*s,.015f*s,LeafDark);}
            else if(n==7||n==8||n==9)
            {for(int i=0;i<4;i++){float a=i*1.5f;Vector3 d=V(Mathf.Cos(a),.25f,Mathf.Sin(a));m.Tube(new[]{p,p+d*.18f*s,p+d*.31f*s+V(0,.04f,0)},.075f*s,.055f*s,color,8);Ball(m,p+d*.23f*s,V(.09f,.065f,.08f)*s,color,10,5);if(n==9)Ring(m,p+d*.19f*s,.068f*s,.009f*s,Red,Quaternion.FromToRotation(Vector3.up,d));}}
            else if(n==11||n==12||n==13)
            {for(int i=0;i<(n==13?7:5);i++){float a=i*2.4f;Vector3 at=p+(n==13?V(Mathf.Cos(a)*.09f,(i%3)*.03f,Mathf.Sin(a)*.09f)*s:Vector3.zero);Leaf(m,at,V(Mathf.Cos(a),0,Mathf.Sin(a)),(n==13?.28f:.42f)*s,(n==13?.085f:.14f)*s,n==12?Purple:Green,.15f,n==12?4:n==13?3:0);}Beam(m,p+V(0,-.14f,0),p,.025f,LeafDark);}
            else if(n==14||n==15||n==16||n==17||n==19)
            {
                float r=n==15?.14f:n==17?.13f:.09f,h=n==16?.29f:.34f;
                if(n==16){m.Tube(new[]{p+V(0,-h,0)*s,p+V(0,h,0)*s},r*s,0,color,5);Beam(m,p+V(0,-h,0)*s,p+V(.015f,-h-.08f,0)*s,.016f*s,LeafDark);}
                else{Ball(m,p,V(r,h,r)*s,color,12,7);Beam(m,p+V(0,h,0)*s,p+V(.03f,h+.1f,0)*s,.018f*s,LeafDark);}
                if(n==15)for(int i=0;i<5;i++){float a=i*1.256f;Leaf(m,p+V(0,h*.9f,0)*s,V(Mathf.Cos(a),0,Mathf.Sin(a)),.12f*s,.045f*s,Green,-.2f);}
                if(n==14||n==17)for(int j=0;j<5;j++)for(int k=0;k<7;k++){float a=k*Mathf.PI*2/7;Ball(m,p+V(Mathf.Cos(a)*r,(j-2)*h*.32f,Mathf.Sin(a)*r)*s,V(n==17?.025f:.009f,.018f,n==17?.025f:.009f)*s,LeafLight,6,3);}
                if(n==16)for(int k=0;k<5;k++){float a=k*Mathf.PI*2/5;Beam(m,p+V(Mathf.Cos(a)*r,-h,Mathf.Sin(a)*r)*s,p+V(0,h,0)*s,.006f*s,LeafLight);}
                if(n==19)for(int k=0;k<6;k++){float a=k*Mathf.PI/3;var rib=new Vector3[9];for(int j=0;j<rib.Length;j++){float lat=-Mathf.PI/2+j*Mathf.PI/8;rib[j]=p+V(Mathf.Cos(a)*r*Mathf.Cos(lat)*1.04f,h*Mathf.Sin(lat),Mathf.Sin(a)*r*Mathf.Cos(lat)*1.04f)*s;}m.Tube(rib,.007f*s,.005f*s,LeafLight,4);}
            }
            else if(n==18){Ball(m,p+V(0,-.08f,0)*s,V(.17f,.23f,.17f)*s,LeafLight);Ball(m,p+V(0,.18f,0)*s,V(.105f,.18f,.105f)*s,LeafLight);Beam(m,p+V(0,.34f,0)*s,p+V(.04f,.40f,0)*s,.02f*s,LeafDark);}
            else if(n==20||n==22||n==26)
            {Ball(m,p,V(.21f,n==22?.25f:.20f,.21f)*s,color);if(n==20)Beam(m,p+V(0,.19f,0)*s,p+V(.025f,.27f,0)*s,.014f*s,LeafDark);else for(int i=0;i<(n==26?4:5);i++){float a=i*Mathf.PI*2/(n==26?4:5);Leaf(m,p+V(0,.19f,0)*s,V(Mathf.Cos(a),0,Mathf.Sin(a)),(n==26?.17f:.1f)*s,.06f*s,Green,-.1f);}if(n==22)Flower(m,p+V(0,-.23f,0)*s,.05f,Brown);}
            else if(n==21)
            {Ball(m,p,V(.20f,.26f,.19f)*s,color);for(int i=0;i<10;i++){float a=i*2.4f;Vector3 d=V(Mathf.Cos(a),0,Mathf.Sin(a));Leaf(m,p+d*.15f*s+V(0,(i%3-1)*.13f,0)*s,d,.16f*s,.04f*s,LeafLight,.7f);}}
            else if(n==23||n==24||n==25||n==29)
            {
                int count=n==29?4:mature?3:2;for(int i=0;i<count;i++)
                {float a=i*2.4f;Vector3 c=p+V(Mathf.Cos(a)*.14f,(i%3-1)*.10f,Mathf.Sin(a)*.12f)*s;float r=n==29?.10f:.13f;Ball(m,c,V(r,r,r)*s,color,8,4);Beam(m,p+V(0,.27f,0)*s,c,.008f*s,Wood);
                    if(n==23||n==25)for(int j=0;j<8;j++){float b=j*2.4f;Vector3 d=V(Mathf.Cos(b)*.8f,((j%3)-1)*.6f,Mathf.Sin(b)*.8f).normalized;Vector3 a0=c+d*r*s;if(n==25)m.Tube(new[]{a0,a0+d*.055f*s+V(0,.025f*s,0)},.008f*s,.002f*s,Gold,3);else Ball(m,a0,V(.018f,.018f,.018f)*s,Red,6,2);}
                }
            }
            else if(n==27||n==28||n==30)
            {
                Vector3 r=n==27?V(.27f,.36f,.27f):n==28?V(.24f,.40f,.23f):V(.18f,.35f,.17f);Ball(m,p,r*s,color,14,8);Beam(m,p+V(0,r.y,0)*s,p+V(0,r.y+.1f,0)*s,.023f*s,Wood);
                if(n==30)for(int k=0;k<8;k++){float a=k*Mathf.PI/4;var rib=new Vector3[9];for(int j=0;j<rib.Length;j++){float lat=-Mathf.PI/2+j*Mathf.PI/8;rib[j]=p+V(Mathf.Cos(a)*r.x*Mathf.Cos(lat)*1.025f,r.y*Mathf.Sin(lat),Mathf.Sin(a)*r.z*Mathf.Cos(lat)*1.025f)*s;}m.Tube(rib,.009f*s,.007f*s,Gold,4);}
                else for(int row=0;row<5;row++)for(int k=0;k<12;k++){float a=k*Mathf.PI/6+(row%2)*.2f,lat=(row-2)*.4f;Vector3 d=V(Mathf.Cos(lat)*Mathf.Cos(a),Mathf.Sin(lat),Mathf.Cos(lat)*Mathf.Sin(a)),a0=p+Vector3.Scale(r,d)*s;m.Tube(new[]{a0,a0+d*(n==27?.06f:.025f)*s},(n==27?.035f:.018f)*s,0,LeafLight,3);}
            }
        }
    }
}
