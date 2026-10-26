using System;
using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    // Authored silhouettes for the 30/30/60 designer brief. Geometry is baked once, never generated at runtime.
    internal static partial class IslandExpansionModels
    {
        internal static readonly string[] Colors={"E8D8B5","BC9668","775238","443C32","B96646","D28D59","8B4C36","D6BA72","949680","647462","79ABAE","C9C18B","73A64E","416E3C","A1BE62","EBC55C","E9E0C8","A34439","75465D","CD8750","B28B52","DAA24D","6A8C65","303C33"};
        const int Cream=0,Wood=1,DarkWood=2,Dark=3,Tile=4,TileLight=5,TileDark=6,Straw=7,Stone=8,StoneDark=9,Water=10,Bamboo=11,Green=12,LeafDark=13,LeafLight=14,Gold=15,White=16,Red=17,Purple=18,Orange=19,Brown=20,Yellow=21,Sage=22,Black=23;
        internal static readonly string[] Buildings={"Xưởng bún gạo","Lò bánh tráng","Nhà tinh bột","Xưởng ép dầu","Bếp kẹo vườn","Nhà rang cà phê","Xưởng ca cao","Bếp gia vị","Nhà sấy trái cây","Nhà muối chua","Xưởng gốm","Nhà dệt chiếu","Xưởng giấy dó","Xưởng mộc mỹ nghệ","Nhà gói quà","Thư viện hạt giống","Trạm kiểm đất","Trạm phối nước tưới","Nhà dưỡng thiên địch","Trạm chăm tán cây","Nhà hợp tác xã","Nhà lưu niệm Mùa Vàng","Trường nhà nông","Sân khấu đờn ca","Trạm chăm thú","Nhà khách miệt vườn","Nhà cứu hộ chim","Nhà tuyển nông sản","Bưu cục đảo","Trạm xe đạp"};
        internal static readonly string[] Crops={"Khoai lang","Khoai tây","Sắn","Lạc","Đậu nành","Mè","Gừng","Nghệ","Riềng","Lá dứa","Húng quế","Tía tô","Bạc hà","Dưa leo","Cà tím","Đậu bắp","Khổ qua","Bầu","Mướp","Chanh dây","Thanh long","Ổi","Vải","Nhãn","Chôm chôm","Măng cụt","Sầu riêng","Mít","Cà phê","Ca cao"};
        internal static readonly string[] Decorations={"Lu nước có gáo dừa","Đôi guốc bên thềm","Võng khung tre","Giá phơi nón","Cối giã gạo đòn chân","Nia sàng tựa vách","Lồng bàn tre","Bộ chổi rơm và ky","Ống bương gõ nước","Sân ô ăn quan","Giá phơi bánh tráng","Mâm ngũ quả","Đòn gánh đôi quang","Cối đá xay tay","Khuôn bánh chạm lá","Chuỗi tỏi ớt treo","Khay bánh lá chóp","Biển gốm Bếp nhà","Ấm đất trên bếp than","Mẹt ép đậu buộc đá","Guồng nước cảnh","Thuyền thúng úp","Cọc neo dây kép","Lờ cá tre","Vó bè thu nhỏ","Bè lục bình hoa","Cột đo con nước","Giá phao cứu sinh","Bục ngắm nước bán nguyệt","Vách khảm vỏ hàu","Vòm dây leo mắt cáo","Lều nghỉ dã ngoại","Chuông gió hạt đất","Đĩa chim tắm","Trụ số hàng vườn","Thang hái quả chữ A","Xe cút kít hoa","Đồng hồ nắng gốm","Tượng sóc ôm hạt","Vòng dây leo bện","Khung cửi trưng bày","Bàn xoay gốm chân đạp","Guồng se sợi","Khung xeo giấy","Bộ ống màu khoáng","Vách gạch hoa gió","Khung căng mặt trống","Bộ thước thợ treo","Thảm cói xoắn tròn","Quạt mo ghép hoa","Đèn kéo quân","Đầu lân trên đôn","Trống hội đại","Cây nêu dây tua","Bộ cà kheo","Dàn cồng ba chiếc","Rối nước trên bệ","Diều sáo trên giá","Khung ảnh dân gian","Bàn cờ tướng đá"};
        internal static GameObject Create(string id,string variant,Material[] mats)
        {
            var m=new EnvironmentMesh(Colors.Length);int n=int.Parse(id.Substring(1));
            if(id[0]=='B')Building(m,n);else if(id[0]=='D')Decor(m,n,Vector3.zero,1);else Crop(m,n,variant);
            Mesh mesh=m.Finish(id+"_"+variant,true);
            if(id[0]=='C'&&variant=="Wilted")
            {
                // Wilted leaves settle on the soil instead of continuing their droop through it.
                var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i].y=Mathf.Max(-.018f,vertices[i].y);
                mesh.vertices=vertices;mesh.RecalculateBounds();mesh.RecalculateNormals();
            }
            var used=new List<int>();var indices=new List<int[]>();
            for(int i=0;i<mesh.subMeshCount;i++)if(mesh.GetIndexCount(i)>0){used.Add(i);indices.Add(mesh.GetTriangles(i));}
            mesh.subMeshCount=used.Count;var materials=new Material[used.Count];
            for(int i=0;i<used.Count;i++){mesh.SetTriangles(indices[i],i);materials[i]=mats[used[i]];}
            var root=new GameObject(id+(string.IsNullOrEmpty(variant)?"":"_"+variant),typeof(MeshFilter),typeof(MeshRenderer));
            root.GetComponent<MeshFilter>().sharedMesh=mesh;root.GetComponent<MeshRenderer>().sharedMaterials=materials;
            var collider=root.AddComponent<BoxCollider>();collider.center=mesh.bounds.center;collider.size=mesh.bounds.size;
            if(id[0]=='B'){var door=new GameObject("Interaction");door.transform.SetParent(root.transform,false);door.transform.localPosition=new Vector3(0,.35f,mesh.bounds.max.z+.2f);}
            return root;
        }
        static Vector3 V(float x,float y,float z){return new Vector3(x,y,z);}
        static void Box(EnvironmentMesh m,Vector3 p,Vector3 size,int c){m.Cube(p,size,c,c);}
        static void Beam(EnvironmentMesh m,Vector3 a,Vector3 b,float r,int c=DarkWood){m.Tube(new[]{a,b},r,r,c,7);}
        static void Face(EnvironmentMesh m,Vector3 a,Vector3 b,Vector3 c,Vector3 d,int color){m.Quad(a,b,c,d,color);m.Quad(d,c,b,a,color);}
        static void Ball(EnvironmentMesh m,Vector3 p,Vector3 size,int c,int sides=12,int rows=6,Quaternion q=default(Quaternion))
        {
            if(Quaternion.Dot(q,q)<.001f)q=Quaternion.identity;
            for(int y=0;y<rows;y++)for(int x=0;x<sides;x++)
            {
                float a=x*Mathf.PI*2/sides,b=(x+1)*Mathf.PI*2/sides,lo=-Mathf.PI/2+y*Mathf.PI/rows,hi=-Mathf.PI/2+(y+1)*Mathf.PI/rows;
                Vector3 v0=p+q*Vector3.Scale(size,V(Mathf.Cos(lo)*Mathf.Cos(a),Mathf.Sin(lo),Mathf.Cos(lo)*Mathf.Sin(a)));
                Vector3 v1=p+q*Vector3.Scale(size,V(Mathf.Cos(hi)*Mathf.Cos(a),Mathf.Sin(hi),Mathf.Cos(hi)*Mathf.Sin(a)));
                Vector3 v2=p+q*Vector3.Scale(size,V(Mathf.Cos(hi)*Mathf.Cos(b),Mathf.Sin(hi),Mathf.Cos(hi)*Mathf.Sin(b)));
                Vector3 v3=p+q*Vector3.Scale(size,V(Mathf.Cos(lo)*Mathf.Cos(b),Mathf.Sin(lo),Mathf.Cos(lo)*Mathf.Sin(b)));
                m.Quad(v0,v1,v2,v3,c);
            }
        }
        static void Ring(EnvironmentMesh m,Vector3 p,float radius,float tube,int color,Quaternion q=default(Quaternion),int count=20)
        {
            if(Quaternion.Dot(q,q)<.001f)q=Quaternion.identity;var points=new Vector3[count+1];
            for(int i=0;i<=count;i++){float a=i*2*Mathf.PI/count;points[i]=p+q*V(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);}m.Tube(points,tube,tube,color,6);
        }
        static void Vessel(EnvironmentMesh m,Vector3 p,float r,float h,int c,bool lid=false)
        {
            const int count=16;float[] ys={0,.08f,.32f,.7f,.92f,1};float[] rs={.62f,.85f,1,.94f,.67f,.7f};
            for(int row=0;row<ys.Length-1;row++)for(int i=0;i<count;i++)
            {float a=i*2*Mathf.PI/count,b=(i+1)*2*Mathf.PI/count;m.Quad(p+V(Mathf.Cos(a)*r*rs[row],ys[row]*h,Mathf.Sin(a)*r*rs[row]),p+V(Mathf.Cos(a)*r*rs[row+1],ys[row+1]*h,Mathf.Sin(a)*r*rs[row+1]),p+V(Mathf.Cos(b)*r*rs[row+1],ys[row+1]*h,Mathf.Sin(b)*r*rs[row+1]),p+V(Mathf.Cos(b)*r*rs[row],ys[row]*h,Mathf.Sin(b)*r*rs[row]),c);}
            Ring(m,p+Vector3.up*h,r*.7f,.035f,c);m.Disc(p+Vector3.up*(lid?h+.01f:h*.82f),r*.68f,r*.68f,lid?DarkWood:Dark);
            if(lid)Beam(m,p+V(-r*.65f,h+.035f,0),p+V(r*.65f,h+.035f,0),.028f,Wood);
        }
        static void Bowl(EnvironmentMesh m,Vector3 p,float r,float depth,int color)
        {
            for(int i=0;i<18;i++){float a=i*Mathf.PI/9,b=(i+1)*Mathf.PI/9;Vector3 a0=p+V(Mathf.Cos(a)*r*.55f,0,Mathf.Sin(a)*r*.55f),b0=p+V(Mathf.Cos(b)*r*.55f,0,Mathf.Sin(b)*r*.55f),a1=p+V(Mathf.Cos(a)*r,depth,Mathf.Sin(a)*r),b1=p+V(Mathf.Cos(b)*r,depth,Mathf.Sin(b)*r);Face(m,a0,a1,b1,b0,color);}m.Disc(p+V(0,.008f,0),r*.55f,r*.55f,color);Ring(m,p+V(0,depth,0),r,.025f,color);
        }
        static void Leaf(EnvironmentMesh m,Vector3 p,Vector3 direction,float length,float width,int color=Green,float lift=.25f,int lobes=0)
        {
            direction.y=0;direction.Normalize();Vector3 across=Vector3.Cross(Vector3.up,direction);const int rows=6;
            for(int i=0;i<rows;i++)
            {float a=(float)i/rows,b=(float)(i+1)/rows;float wa=Mathf.Sin(a*Mathf.PI)*width,wb=Mathf.Sin(b*Mathf.PI)*width;if(lobes>0){wa*=.75f+.25f*Mathf.Cos(a*lobes*Mathf.PI*2);wb*=.75f+.25f*Mathf.Cos(b*lobes*Mathf.PI*2);}Vector3 ca=p+direction*length*a+Vector3.up*length*(lift*a+.2f*Mathf.Sin(a*Mathf.PI)-.18f*a*a),cb=p+direction*length*b+Vector3.up*length*(lift*b+.2f*Mathf.Sin(b*Mathf.PI)-.18f*b*b);Face(m,ca,ca-across*wa,cb-across*wb,cb,color);Face(m,ca,cb,cb+across*wb,ca+across*wa,color);}
            Beam(m,p,p+direction*length*.7f+Vector3.up*length*(lift*.7f+.1f),.006f,LeafLight);
        }
        static void Flower(EnvironmentMesh m,Vector3 p,float r,int c,int petals=5){Ball(m,p,V(r*.24f,.025f,r*.24f),Gold,8,4);for(int i=0;i<petals;i++){float a=i*Mathf.PI*2/petals;Leaf(m,p,V(Mathf.Cos(a),0,Mathf.Sin(a)),r,r*.37f,c,.05f);}}
        static void Table(EnvironmentMesh m,Vector3 p,float w,float d,float h,int c=Wood)
        {Box(m,p+V(0,h,0),V(w,.09f,d),c);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Beam(m,p+V(x*w*.42f,0,z*d*.4f),p+V(x*w*.42f,h,z*d*.4f),.045f,DarkWood);}
        static void Roof(EnvironmentMesh m,Vector3 p,float w,float d,float eave,float rise,int color=Tile)
        {
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 ridge=p+V(0,eave+rise,0),edge=p+V(0,eave,side*d/2);Face(m,ridge+V(-w/2,0,0),edge+V(-w/2,0,0),edge+V(w/2,0,0),ridge+V(w/2,0,0),color==Straw?Straw:TileDark);
                int cols=Mathf.CeilToInt(w/.32f),rows=Mathf.Max(2,Mathf.CeilToInt(d/.9f));
                for(int x=0;x<cols;x++)for(int z=0;z<rows;z++)
                {float t=(z+.5f)/rows;Vector3 c=Vector3.Lerp(ridge,edge,t)+V(-w/2+(x+.5f)*w/cols,.025f,0);Vector3 along=(edge-ridge).normalized*d/(rows*3.1f);Beam(m,c-along,c+along,color==Straw?.075f:.065f,(x+z)%4==0?(color==Straw?Bamboo:TileLight):color);}
                Beam(m,edge+V(-w/2,0,0),edge+V(w/2,0,0),.055f,DarkWood);
                for(int x=0;x<cols;x+=2)Beam(m,ridge+V(-w/2+x*w/cols,-.08f,0),edge+V(-w/2+x*w/cols,-.08f,0),.035f,Wood);
            }
            Beam(m,p+V(-w/2,eave+rise+.04f,0),p+V(w/2,eave+rise+.04f,0),.10f,color);
        }
        static void Hut(EnvironmentMesh m,float w,float d,float h,int style=0)
        {
            Box(m,V(0,.08f,0),V(w,.16f,d),Stone);Box(m,V(0,.04f,d/2+.18f),V(w*.55f,.08f,.36f),StoneDark);
            Box(m,V(0,h*.48f,-d*.29f),V(w*.88f,h*.9f,d*.33f),style==2?Wood:Cream);
            for(int i=0;i<7;i++)Box(m,V(-w*.38f+i*w*.126f,h*.5f,-d*.119f),V(.035f,h*.82f,.035f),Wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){Vector3 p=V(x*w*.43f,.16f,z*d*.39f);Beam(m,p,p+Vector3.up*(h-.15f),.075f,DarkWood);Box(m,p+V(0,.09f,0),V(.22f,.18f,.22f),StoneDark);}
            Roof(m,V(0,0,-d*.05f),w+ .35f,d+.3f,h,.55f+(style%3)*.12f,style==1?Straw:Tile);
            for(int side=-1;side<=1;side+=2){float x=side*w*.442f;Box(m,V(x,h*.52f,-d*.3f),V(.026f,.55f,.65f),DarkWood);for(int z=0;z<5;z++)Box(m,V(x+side*.018f,h*.52f,-d*.3f-.26f+z*.13f),V(.025f,.51f,.065f),Bamboo);}
            Box(m,V(w*.18f,h*.4f,-d*.46f),V(.65f,h*.72f,.04f),DarkWood);for(int i=0;i<5;i++)Box(m,V(w*.18f-.26f+i*.13f,h*.4f,-d*.467f),V(.11f,h*.69f,.026f),Wood);
            for(int i=0;i<7;i++)Box(m,V(-w*.39f+i*w*.13f,.28f,-d*.462f),V(w*.12f,.16f,.035f),TileDark);
            // Large readable sign frame above the work apron, rather than tiny UI text.
            Box(m,V(0,h-.29f,d*.4f),V(1,.42f,.10f),DarkWood);Box(m,V(0,h-.29f,d*.46f),V(.9f,.32f,.04f),Straw);
        }
    }
}
