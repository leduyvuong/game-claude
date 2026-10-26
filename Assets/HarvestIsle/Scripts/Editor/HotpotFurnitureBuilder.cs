using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace HarvestIsle.Editor
{
    // Geometry is baked once. No primitive construction or material allocation in gameplay.
    internal sealed partial class HotpotFurnitureBuilder
    {
        readonly Dictionary<string,Material> materials;
        readonly Mesh round,ball,foliage,ring,bowl,cushion,halfDisc;
        Transform root,body,food;
        readonly List<Transform> seats=new List<Transform>();
        const string Wood="Wood",Dark="DarkWood",Bamboo="Bamboo",Steel="Steel",Black="Black",Cream="Ceramic",Red="Red",Green="Green",Gold="Gold",Leaf="Leaf";
        internal HotpotFurnitureBuilder(Dictionary<string,Material> materials)
        {
            this.materials=materials;
            round=Lathe(new[]{new Vector2(0,-.5f),new Vector2(.5f,-.5f),new Vector2(.5f,.5f),new Vector2(0,.5f)},24);
            var profile=new List<Vector2>();for(int i=0;i<=10;i++){float a=i*Mathf.PI/10;profile.Add(new Vector2(Mathf.Sin(a)*.5f,-Mathf.Cos(a)*.5f));}ball=Lathe(profile.ToArray(),16);
            profile.Clear();for(int i=0;i<=6;i++){float a=i*Mathf.PI/6;profile.Add(new Vector2(Mathf.Sin(a)*.5f,-Mathf.Cos(a)*.5f));}foliage=Lathe(profile.ToArray(),8);
            profile.Clear();for(int i=0;i<=8;i++){float a=i*Mathf.PI/4;profile.Add(new Vector2(.46f+.04f*Mathf.Cos(a),.04f*Mathf.Sin(a)));}ring=Lathe(profile.ToArray(),24);
            bowl=Lathe(new[]{new Vector2(0,0),new Vector2(.30f,0),new Vector2(.46f,.18f),new Vector2(.5f,.5f),new Vector2(.46f,.5f),new Vector2(.42f,.18f),new Vector2(.28f,.07f),new Vector2(0,.07f)},24);
            cushion=RoundedBox();var dv=new List<Vector3>{Vector3.zero};var dt=new List<int>();
            for(int i=0;i<=24;i++){float a=i*Mathf.PI/24;dv.Add(new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f));if(i>0){dt.Add(0);dt.Add(i+1);dt.Add(i);}}
            halfDisc=new Mesh();halfDisc.SetVertices(dv);halfDisc.SetTriangles(dt,0);halfDisc.RecalculateNormals();halfDisc.RecalculateBounds();
        }
        internal void Dispose(){foreach(var mesh in new[]{round,ball,foliage,ring,bowl,cushion,halfDisc})Object.DestroyImmediate(mesh);foreach(var mesh in kitchenMeshes)Object.DestroyImmediate(mesh);kitchenMeshes.Clear();}
        static Mesh RoundedBox()
        {
            var v=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            Vector3[] ns={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};float[] grid={-.5f,-.46f,-.38f,0,.38f,.46f,.5f};
            foreach(var normal in ns)
            {
                Vector3 u=Vector3.Cross(normal,Mathf.Abs(normal.y)>.5f?Vector3.forward:Vector3.up),w=Vector3.Cross(normal,u);int start=v.Count;
                for(int y=0;y<7;y++)for(int x=0;x<7;x++){Vector3 p=normal*.5f+u*grid[x]+w*grid[y],core=new Vector3(Mathf.Clamp(p.x,-.38f,.38f),Mathf.Clamp(p.y,-.38f,.38f),Mathf.Clamp(p.z,-.38f,.38f));Vector3 n=(p-core).normalized;v.Add(core+n*.12f);normals.Add(n);uv.Add(new Vector2((float)x/6,(float)y/6));}
                for(int y=0;y<6;y++)for(int x=0;x<6;x++){int a=start+y*7+x;tris.Add(a);tris.Add(a+1);tris.Add(a+7);tris.Add(a+1);tris.Add(a+8);tris.Add(a+7);}
            }
            var m=new Mesh();m.SetVertices(v);m.SetNormals(normals);m.SetUVs(0,uv);m.SetTriangles(tris,0);m.RecalculateBounds();return m;
        }
        static Mesh Lathe(Vector2[] profile,int sides)
        {
            var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            for(int j=0;j<profile.Length;j++)for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Cos(a)*profile[j].x,profile[j].y,Mathf.Sin(a)*profile[j].x));uv.Add(new Vector2((float)i/sides,(float)j/(profile.Length-1)));}
            for(int j=0;j<profile.Length-1;j++)for(int i=0;i<sides;i++){int a=j*(sides+1)+i,b=a+sides+1;t.Add(a);t.Add(b);t.Add(a+1);t.Add(a+1);t.Add(b);t.Add(b+1);}
            var m=new Mesh();m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        Transform Group(string name,Transform p,Vector3 pos)
        {var o=new GameObject(name).transform;o.SetParent(p,false);o.localPosition=pos;return o;}
        Transform Shape(string name,Transform p,Vector3 at,Vector3 size,string mat,Mesh mesh=null)
        {
            GameObject o;
            if(mesh==null){o=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.DestroyImmediate(o.GetComponent<Collider>());}
            else {o=new GameObject();o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>();}
            o.name=name;o.transform.SetParent(p,false);o.transform.localPosition=at;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=materials[mat];return o.transform;
        }
        Transform B(Transform p,float x,float y,float z,float w,float h,float d,string m=Wood)
        {
            if(m=="Glass")
            {
                // Stylized clear panes: corner highlights leave contents readable and avoid
                // stacked transparent surfaces on the mobile restaurant camera.
                var pane=Group("Clear glass edge highlights",p,new Vector3(x,y,z));
                foreach(int side in new[]{-1,1})
                    if(h>d)Shape("Glass glint",pane,new Vector3(side*w*.45f,h*.15f,-d*.5f),new Vector3(.012f,h*.40f,.009f),Cream);
                    else Shape("Glass glint",pane,new Vector3(side*w*.45f,h*.5f,0),new Vector3(.012f,.009f,d*.4f),Cream);
                return pane;
            }
            return Shape(m,p,new Vector3(x,y,z),new Vector3(w,h,d),m);
        }
        Transform C(Transform p,float x,float y,float z,float w,float h,float d,string m=Steel){return Shape(m,p,new Vector3(x,y,z),new Vector3(w,h,d),m,round);}
        Transform E(Transform p,float x,float y,float z,float w,float h,float d,string m=Leaf){return Shape(m,p,new Vector3(x,y,z),new Vector3(w,h,d),m,m==Leaf||m=="LightLeaf"||m=="Flower"?foliage:ball);}
        Transform U(Transform p,float x,float y,float z,float w,float h,float d,string m){return Shape(m,p,new Vector3(x,y,z),new Vector3(w,h,d),m,cushion);}
        void R(Transform p,float x,float y,float z,float w,string m=Steel){Shape("Rim",p,new Vector3(x,y,z),new Vector3(w,w,w),m,ring);}
        void Rod(Transform p,Vector3 a,Vector3 b,float width,string mat=Wood)
        {var o=C(p,0,0,0,width,Vector3.Distance(a,b),width,mat);o.localPosition=(a+b)/2;o.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
        void Legs(Transform p,float w,float d,float height,string mat=Wood)
        {foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})B(p,x*w*.43f,height/2,z*d*.43f,.09f,height,.09f,mat);}
        void Plate(Transform p,float x,float y,float z,float size=.24f)
        {C(p,x,y,z,size,.035f,size,Cream);R(p,x,y+.02f,z,size,Cream);}
        void Bowl(Transform p,float x,float y,float z,float size=.23f,string fill="Broth")
        {Shape("Bowl",p,new Vector3(x,y,z),new Vector3(size,.16f,size),Cream,bowl);C(p,x,y+.075f,z,size*.83f,.008f,size*.83f,fill);R(p,x,y+.08f,z,size,Cream);}
        void Setting(Transform p,float x,float y,float z)
        {Plate(p,x,y,z,.32f);Bowl(p,x,y+.025f,z);B(p,x+.22f,y+.018f,z,.015f,.016f,.29f,Dark);B(p,x+.25f,y+.018f,z,.015f,.016f,.29f,Dark);C(p,x-.23f,y+.07f,z,.12f,.14f,.12f,Cream);}
        void Tray(Transform p,float x,float y,float z,bool meat)
        {
            B(p,x,y,z,.44f,.035f,.28f,Cream);
            for(int i=0;i<6;i++){float dx=x-.14f+(i%3)*.14f,dz=z-.065f+(i/3)*.13f;E(p,dx,y+.04f,dz,.15f,.055f,.11f,meat?"Meat":Leaf);if(meat)B(p,dx,y+.063f,dz,.085f,.009f,.013f,Cream);}
        }
        void Pot(Transform p,float x,float y,float z,float size=.62f)
        {
            C(p,x,y+.01f,z,size*.86f,.07f,size*.86f,Black);Shape("Open hotpot",p,new Vector3(x,y+.02f,z),new Vector3(size,.38f,size),Steel,bowl);
            C(p,x,y+.20f,z,size*.9f,.016f,size*.9f,"Broth");R(p,x,y+.21f,z,size,Steel);
            Shape("Spicy half",p,new Vector3(x,y+.210f,z),new Vector3(size*.87f,1,size*.87f),Red,halfDisc);
            B(p,x,y+.21f,z,size*.86f,.025f,.028f,Steel);
            for(int k=0;k<9;k++){float a=k*2.4f;E(p,x+Mathf.Cos(a)*size*.29f,y+.22f,z+Mathf.Sin(a)*size*.3f,.07f,.028f,.09f,k%3==0?Red:k%3==1?Leaf:Cream);}
            foreach(int side in new[]{-1,1}){var h=Shape("Pot handle",p,new Vector3(x+side*size*.54f,y+.15f,z),new Vector3(.16f,.16f,.16f),Steel,ring);h.localRotation=Quaternion.Euler(90,0,0);}
        }
        void Chair(Transform p,Vector3 pos,float angle,int style,string mat=Wood)
        {
            var c=Group("Chair",p,pos);c.localRotation=Quaternion.Euler(0,angle,0);
            string leg=style==3?Black:mat;
            if(style==2)
            {
                foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Rod(c,new Vector3(x*.29f,.04f,z*.29f),new Vector3(x*.23f,.52f,z*.23f),.09f,mat);
                U(c,0,.56f,0,.67f,.10f,.67f,mat);B(c,0,.614f,0,.10f,.005f,.045f,Black);foreach(int s in new[]{-1,1}){B(c,0,.39f,s*.23f,.48f,.075f,.04f,mat);B(c,s*.23f,.39f,0,.04f,.075f,.48f,mat);}
            }
            else if(style==0){Legs(c,.56f,.56f,.53f,leg);C(c,0,.56f,0,.66f,.09f,.66f,mat);}
            else
            {
                Legs(c,.56f,.56f,.53f,leg);
                B(c,0,.56f,0,.62f,.09f,.60f,mat);
                foreach(int s in new[]{-1,1})B(c,s*.27f,.93f,-.24f,.06f,.8f,.06f,leg);
                if(style==4||style==5){U(c,0,.64f,0,.63f,.16f,.61f,mat);U(c,0,1.05f,-.24f,.63f,.82f,.17f,mat);for(int k=0;k<3;k++)B(c,(k-1)*.17f,1.05f,-.135f,.011f,.6f,.01f,style==4?"Seam":Gold);}
                else {B(c,0,1.30f,-.24f,.62f,.09f,.07f,leg);for(int k=0;k<4;k++)B(c,-.20f+k*.135f,1.04f,-.24f,.035f,.46f,.035f,leg);}
            }
        }
        void Seat(Transform p,Vector3 pos,float angle,float height=.56f)
        {var a=Group("Seat "+seats.Count,p,pos+Vector3.up*(height-.48f));a.localRotation=Quaternion.Euler(0,angle,0);seats.Add(a);}
        void Plant(Transform p,Vector3 pos,float height=1,string pot=Dark,bool flowers=false)
        {
            var g=Group("Plant",p,pos);C(g,0,.15f,0,.40f,.30f,.40f,pot);R(g,0,.31f,0,.43f,pot);C(g,0,.31f,0,.35f,.015f,.35f,"Soil");
            for(int i=0;i<7;i++){float a=i*2.4f;Vector3 end=new Vector3(Mathf.Sin(a)*.25f,.4f+height*(.4f+(i%3)*.15f),Mathf.Cos(a)*.25f);Rod(g,new Vector3(0,.3f,0),end,.025f,Leaf);for(int k=0;k<3;k++){var leaf=E(g,end.x+Mathf.Sin(a+k)*.10f,end.y-k*.12f,end.z+Mathf.Cos(a+k)*.10f,.23f,.06f,.11f,k%2==0?Leaf:"LightLeaf");leaf.localRotation=Quaternion.Euler(15,a*57+k*50,25);}if(flowers)for(int k=0;k<5;k++)E(g,end.x+Mathf.Cos(k*1.26f)*.06f,end.y+.025f,end.z+Mathf.Sin(k*1.26f)*.06f,.1f,.05f,.1f,"Flower");}
        }
        void Lantern(Transform p,Vector3 pos,bool red)
        {
            var g=Group("Lantern",p,pos);string m=red?Red:"Glow";
            E(g,0,0,0,.38f,.60f,.38f,m);C(g,0,.30f,0,.20f,.07f,.20f,Gold);C(g,0,-.30f,0,.20f,.07f,.20f,Gold);
            for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Rod(g,new Vector3(Mathf.Cos(a)*.165f,-.2f,Mathf.Sin(a)*.165f),new Vector3(Mathf.Cos(a)*.165f,.2f,Mathf.Sin(a)*.165f),.012f,red?Gold:Wood);}
            Rod(g,new Vector3(0,-.34f,0),new Vector3(0,-.57f,0),.022f,red?Red:Dark);
        }
        internal GameObject Create(int kind)
        {
            var spec=HotpotFurnitureCatalog.Items[kind];root=new GameObject("HP_"+spec.id).transform;body=Group("Structure",root,Vector3.zero);food=Group("Serving",root,Vector3.zero);seats.Clear();
            if(spec.table)Dining(kind-8);else Decoration(kind-24);
            var view=root.gameObject.AddComponent<HotpotFurnitureView>();view.kind=kind;view.serving=spec.table?food:null;view.seats=seats.ToArray();
            if(!spec.table)Object.DestroyImmediate(food.gameObject);
            Bounds bounds=new Bounds();bool first=true;foreach(var r in root.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
            // Every floor bay remains rotatable without crossing the 3.15 m aisle spacing.
            float scale=Mathf.Min(1,2.65f/Mathf.Max(bounds.size.x,bounds.size.z));
            foreach(Transform child in root){child.localPosition*=scale;child.localScale*=scale;}
            view.footprint=bounds.size*scale;
            return root.gameObject;
        }
        void Dining(int type)
        {
            bool circular=type==0||type==1||type==8;float w=type==1?2.15f:type==3||type==9||type==13?2.8f:type==14?3f:1.7f;
            float d=type==14?.8f:type==3||type==9||type==13?1.45f:1.7f;float y=type==12?.52f:type==14?1.35f:1.08f;
            string mat=type==5?Steel:type==6?Green:type==8||type==10||type==11?"Marble":type==9?Dark:type==15?Bamboo:Wood;
            if(type==14)mat="Stone";
            if(circular){C(body,0,y,0,w,.13f,w,mat);R(body,0,y+.055f,0,w,type==8?Gold:Dark);}
            else {B(body,0,y,0,w,.13f,d,mat);if(mat==Wood||mat==Dark||mat==Bamboo)for(int i=0;i<8;i++)B(body,0,y+.067f,-d*.45f+i*d*.9f/7,w*.97f,.008f,.012f,Dark);}
            if(type==8){C(body,0,.08f,0,.96f,.14f,.96f,Gold);C(body,0,.56f,0,.28f,.95f,.28f,Gold);}
            else if(type==10||type==11){B(body,0,.07f,0,.8f,.12f,.65f,Black);B(body,0,.57f,0,.18f,1,.18f,Steel);}
            else if(type==14){B(body,0,y/2,0,w,.0f+y,.58f,"Stone");B(body,0,y+.23f,.35f,w,.32f,.075f,Steel);}
            else Legs(body,w,d,y,mat=="Marble"?Gold:mat);
            if(type==10||type==11)
            {
                for(int side=0;side<2;side++)
                {
                    var sofa=Group("Upholstered booth",body,new Vector3(0,0,side==0?-1.17f:1.17f));
                    if(type==11&&side==1){sofa.localPosition=new Vector3(1.17f,0,0);sofa.localRotation=Quaternion.Euler(0,-90,0);}
                    else sofa.localRotation=Quaternion.Euler(0,side==0?0:180,0);
                    string leather=type==10?Red:"Teal";B(sofa,0,.22f,0,2.35f,.44f,.65f,Wood);
                    for(int k=0;k<6;k++){float x=-.96f+k*.385f;U(sofa,x,.55f,0,.39f,.24f,.70f,leather);U(sofa,x,1.01f,-.28f,.39f,1.02f,.24f,leather);}B(sofa,-1.16f,.8f,0,.12f,.65f,.75f,Wood);B(sofa,1.16f,.8f,0,.12f,.65f,.75f,Wood);
                    Seat(sofa,new Vector3(-.45f,0,0),0);Seat(sofa,new Vector3(.45f,0,0),0);
                }
            }
            else if(type==12)
            {for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Vector3 pos=new Vector3(Mathf.Sin(a)*1.3f,0,Mathf.Cos(a)*1.3f);B(body,pos.x,.08f,pos.z,.70f,.16f,.70f,"Navy");B(body,pos.x,.165f,pos.z,.61f,.015f,.61f,Bamboo);Seat(body,pos,i*90+180,.14f);}}
            else if(type==13)
            {foreach(int s in new[]{-1,1}){var bench=Group("Communal bench",body,new Vector3(0,0,s*1.05f));B(bench,0,.56f,0,w,.12f,.43f,Wood);Legs(bench,w,.40f,.53f);for(int i=0;i<3;i++)Seat(bench,new Vector3((i-1)*.85f,0,0),s<0?0:180);}}
            else
            {
                int count=type==1?6:type==3||type==9?6:type==4?2:type==14?3:4;
                for(int i=0;i<count;i++)
                {
                    Vector3 pos;float angle;
                    if(type==14){pos=new Vector3((i-1)*.92f,0,-.89f);angle=0;}
                    else if(type==3||type==9){pos=new Vector3((i%3-1)*.90f,0,i<3?-1.10f:1.10f);angle=i<3?0:180;}
                    else {float a=i*Mathf.PI*2/count;pos=new Vector3(Mathf.Sin(a)*(w*.5f+.42f),0,Mathf.Cos(a)*(d*.5f+.42f));angle=i*360f/count+180;}
                    int style=type==0||type==14?0:type==5||type==6?2:type==7?3:type==4||type==9?4:type==8?5:1;
                    string cm=type==5?Red:type==6?Green:type==7?Black:type==8?"Upholstery":type==9?"Leather":type==15?Bamboo:Wood;
                    var chair=Group("Dining seat",body,pos);chair.localRotation=Quaternion.Euler(0,angle,0);Chair(chair,Vector3.zero,0,style,cm);
                    if(type==14){chair.localScale=new Vector3(.9f,1.55f,.9f);R(chair,0,.26f,0,.50f,Black);}
                    if(type==14)Seat(body,pos,angle,.87f);else Seat(chair,Vector3.zero,0);
                }
            }
            int pots=type==3?2:type==13||type==14?3:1;
            for(int i=0;i<pots;i++)Pot(food,(i-(pots-1)*.5f)*(pots==2?1.2f:.92f),y+.08f,0,type==14?.5f:.62f);
            int settings=type==4?2:type==13?6:type==14?3:4;
            for(int i=0;i<settings;i++)
            {
                float x,z;if(type==14){x=(i-1)*.92f;z=-.27f;}else {x=(i%2==0?-1:1)*w*.32f;z=(i/2%2==0?-1:1)*d*.35f;}
                Setting(body,x,y+.09f,z);
            }
            Tray(food,-w*.30f,y+.1f,0,true);Tray(food,w*.30f,y+.1f,.13f,false);
            if(type==15)
            {
                var umbrella=Group("Folded parasol",body,new Vector3(1.35f,0,1.1f));B(umbrella,0,.08f,0,.42f,.16f,.42f,"Stone");Rod(umbrella,new Vector3(0,.12f,0),new Vector3(0,2.9f,0),.045f,Bamboo);
                for(int i=0;i<10;i++){float a=i*Mathf.PI/5;Rod(umbrella,new Vector3(0,2.85f,0),new Vector3(Mathf.Sin(a)*.22f,1.5f,Mathf.Cos(a)*.22f),.09f,"Upholstery");}
            }
        }
    }
}
