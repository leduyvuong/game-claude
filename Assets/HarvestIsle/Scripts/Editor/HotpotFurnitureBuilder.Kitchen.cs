using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal sealed partial class HotpotFurnitureBuilder
    {
        readonly List<Mesh> kitchenMeshes=new List<Mesh>();
        readonly List<Transform> kitchenSockets=new List<Transform>();
        const string Pink="RawChicken",WhiteFish="FishFlesh",Pork="Pork",Orange="Shrimp",Yellow="Corn",Tofu="Tofu",Mushroom="Mushroom",Clay="Clay",Soup="Soup",Chili="Chili",Noodle="Noodle";
        Transform Small(Transform p,Vector3 at,Vector3 size,string mat){return Shape(mat,p,at,size,mat,foliage);}
        void Path(Transform p,List<Vector3> points,float radius,string mat,int sides=6)
        {
            var vertices=new List<Vector3>();var tris=new List<int>();
            for(int j=0;j<points.Count;j++)
            {
                Vector3 tangent=(points[Mathf.Min(j+1,points.Count-1)]-points[Mathf.Max(0,j-1)]).normalized;
                Vector3 u=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.9f?Vector3.forward:Vector3.up).normalized,w=Vector3.Cross(tangent,u);
                for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;vertices.Add(points[j]+(u*Mathf.Cos(a)+w*Mathf.Sin(a))*radius);}
                if(j>0)for(int i=0;i<sides;i++){int a=(j-1)*sides+i,b=(j-1)*sides+(i+1)%sides,c=a+sides,d=b+sides;tris.Add(a);tris.Add(b);tris.Add(c);tris.Add(b);tris.Add(d);tris.Add(c);}
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();kitchenMeshes.Add(mesh);Shape("Curved "+mat,p,Vector3.zero,Vector3.one,mat,mesh);
        }
        Transform Socket(Transform p,Vector3 at,string name)
        {var socket=Group("Socket "+name,p,at);kitchenSockets.Add(socket);return socket;}
        internal GameObject CreateKitchen(int index)
        {
            var spec=HotpotKitchenCatalog.Items[index];root=new GameObject(spec.id).transform;body=Group("Structure",root,Vector3.zero);food=Group("Contents",root,Vector3.zero);kitchenSockets.Clear();
            if(index<12)Tool(index,body);
            else if(index<36)IngredientTray(index-12,body);
            else if(index<48)Dish(index-36,body,food);
            else Kitchen(spec.level,body,food);
            var item=root.gameObject.AddComponent<HotpotKitchenItem>();item.catalogId=spec.id;item.ingredientKey=spec.ingredientKey;item.role=spec.role;item.level=spec.level;item.contents=food;item.sockets=kitchenSockets.ToArray();
            if(food.childCount==0){Object.DestroyImmediate(food.gameObject);item.contents=null;}
            var rs=root.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);item.size=bounds.size;
            return root.gameObject;
        }
        void Vessel(Transform p,float diameter=1.25f,float height=.5f,string mat=Steel,bool handles=true)
        {
            Shape("Open vessel",p,Vector3.zero,new Vector3(diameter,height*2,diameter),mat,bowl);R(p,0,height,0,diameter,mat);
            if(handles)foreach(int side in new[]{-1,1})
            {
                var points=new List<Vector3>{new Vector3(side*diameter*.47f,height*.8f,-diameter*.12f),new Vector3(side*diameter*.65f,height*.8f,-diameter*.12f),new Vector3(side*diameter*.66f,height*.8f,diameter*.12f),new Vector3(side*diameter*.47f,height*.8f,diameter*.12f)};
                Path(p,points,.035f,mat==Steel?Black:mat,8);
            }
        }
        void Stove(Transform p,bool induction=false)
        {
            U(p,0,.09f,0,1.03f,.18f,.83f,Black);B(p,0,.19f,0,.95f,.025f,.75f,induction?"Cooktop":Black);
            if(induction){R(p,0,.214f,0,.59f,Steel);C(p,.3f,.215f,-.27f,.08f,.008f,.08f,Red);for(int i=0;i<3;i++)C(p,-.25f+i*.08f,.215f,-.3f,.025f,.008f,.025f,Cream);}
            else
            {
                C(p,0,.23f,0,.36f,.07f,.36f,Gold);R(p,0,.27f,0,.29f,Black);
                for(int k=0;k<20;k++){float a=k*Mathf.PI/10;Small(p,new Vector3(Mathf.Sin(a)*.15f,.28f,Mathf.Cos(a)*.15f),new Vector3(.018f,.025f,.018f),Black);}
                for(int i=0;i<4;i++){float a=i*Mathf.PI/2;var support=B(p,Mathf.Sin(a)*.29f,.29f,Mathf.Cos(a)*.29f,.06f,.11f,.26f,Steel);support.localRotation=Quaternion.Euler(0,i*90,0);}
                var can=C(p,.55f,.15f,0,.26f,.85f,.26f,Black);can.localRotation=Quaternion.Euler(90,0,0);var knob=C(p,.55f,.15f,-.46f,.15f,.04f,.15f,Black);knob.localRotation=Quaternion.Euler(90,0,0);B(p,.55f,.15f,-.485f,.025f,.12f,.02f,Steel);
            }
        }
        void PotLid(Transform p,float size=1.15f)
        {E(p,0,.08f,0,size,.18f,size,Steel);R(p,0,.035f,0,size,Steel);C(p,0,.19f,0,.19f,.12f,.19f,Black);E(p,0,.25f,0,.27f,.08f,.27f,Black);}
        void Tool(int type,Transform p)
        {
            switch(type)
            {
                case 0:Stove(p);break;
                case 1:Stove(p,true);break;
                case 2:case 3:case 5:
                    Vessel(p,1.25f,.46f,type==5?Clay:Steel);if(type==3)Divider(p,1.15f,.44f);Socket(root,new Vector3(0,.42f,0),"Broth");break;
                case 4:
                    U(p,0,.045f,0,1.28f,.09f,1.08f,Steel);foreach(int s in new[]{-1,1}){B(p,s*.63f,.27f,0,.055f,.49f,1.06f,Steel);B(p,0,.27f,s*.52f,1.28f,.49f,.055f,Steel);Path(p,new List<Vector3>{new Vector3(s*.63f,.32f,-.13f),new Vector3(s*.80f,.32f,-.13f),new Vector3(s*.80f,.32f,.13f),new Vector3(s*.63f,.32f,.13f)},.03f,Steel);}
                    B(p,0,.27f,0,.035f,.46f,1.02f,Steel);B(p,0,.27f,0,1.23f,.46f,.035f,Steel);for(int i=0;i<4;i++)Socket(root,new Vector3(i%2*.62f-.31f,.45f,i/2*.5f-.25f),"Compartment "+i);break;
                case 6:PotLid(p);break;
                case 7:
                    Vessel(p,.98f,1,Steel);var lid=Group("Leaning lid",p,new Vector3(.76f,.49f,0));lid.localRotation=Quaternion.Euler(0,0,-65);PotLid(lid,.98f);Socket(root,new Vector3(0,.93f,0),"Broth");break;
                case 8:case 9:
                    Shape("Ladle bowl",p,new Vector3(0,0,-.48f),new Vector3(.47f,.30f,.47f),Steel,bowl);
                    Rod(p,new Vector3(0,.13f,-.28f),new Vector3(0,.35f,.43f),.045f,Steel);Rod(p,new Vector3(0,.35f,.43f),new Vector3(0,.48f,.91f),.085f,Wood);
                    if(type==9)for(int i=0;i<19;i++){float a=i*2.4f,r=.025f+.032f*(i%5);C(p,Mathf.Cos(a)*r,.115f,-.48f+Mathf.Sin(a)*r,.025f,.003f,.025f,Black);}break;
                case 10:
                    for(int s=-1;s<=1;s+=2){Rod(p,new Vector3(0,.20f,.55f),new Vector3(s*.22f,.07f,-.48f),.04f,Steel);for(int i=0;i<5;i++)E(p,s*.22f,.08f,-.32f-i*.06f,.14f,.05f,.095f,Steel);}Path(p,new List<Vector3>{new Vector3(-.02f,.2f,.55f),new Vector3(0,.24f,.65f),new Vector3(.02f,.2f,.55f)},.03f,Steel);break;
                case 11:
                    for(int i=0;i<2;i++){float x=i*.6f-.3f;Shape("Rice bowl",p,new Vector3(x,0,.12f),new Vector3(.48f,.45f,.48f),Cream,bowl);R(p,x,.225f,.12f,.49f,Wood);B(p,x,.04f,-.39f,.35f,.08f,.08f,Wood);for(int k=0;k<2;k++)Rod(p,new Vector3(x-.23f,.09f,-.31f-k*.075f),new Vector3(x+.32f,.09f,-.41f-k*.075f),.018f,Dark);}break;
            }
        }
        void Divider(Transform p,float size,float height)
        {
            // Curved metal partition reaching from base to rim.
            for(int i=0;i<20;i++){float z=-size*.47f+(i+.5f)*size*.94f/20,x=Mathf.Sin(z/size*Mathf.PI*2)*size*.13f,bottom=.08f+Mathf.Pow(Mathf.Abs(z)/(size*.5f),4)*height*.6f;var wall=B(p,x,(height+bottom)/2,z,.025f,height-bottom,size/19,Steel);wall.localRotation=Quaternion.Euler(0,Mathf.Cos(z/size*Mathf.PI*2)*35,0);}
        }
        void MeatRoll(Transform p,Vector3 at,bool pork=false,float scale=1)
        {
            var g=Group("Rolled meat",p,at);g.localScale=Vector3.one*scale;
            var roll=Lathe(new[]{new Vector2(.095f,-.20f),new Vector2(.14f,-.20f),new Vector2(.14f,.20f),new Vector2(.095f,.20f),new Vector2(.095f,-.20f)},16);kitchenMeshes.Add(roll);
            var shape=Shape("Meat tube",g,Vector3.zero,Vector3.one,pork?Pork:"Meat",roll);shape.localRotation=Quaternion.Euler(90,0,0);
            for(int j=0;j<3;j++){var curve=new List<Vector3>();for(int k=0;k<=10;k++){float a=k*Mathf.PI/10;curve.Add(new Vector3(Mathf.Cos(a)*.142f,Mathf.Sin(a)*.142f,-.15f+j*.14f+.02f*Mathf.Sin(k)));}Path(g,curve,.012f,pork?Cream:"Fat",5);}
        }
        void ShrimpPiece(Transform p,Vector3 at,float angle=0,float scale=1)
        {
            var g=Group("Segmented shrimp",p,at);g.localRotation=Quaternion.Euler(0,angle,0);g.localScale=Vector3.one*scale;
            for(int i=0;i<7;i++){float a=-.9f+i*.37f;Small(g,new Vector3(Mathf.Cos(a)*.21f,.05f,Mathf.Sin(a)*.21f),new Vector3(.15f-i*.009f,.13f-i*.008f,.13f),i%2==0?Orange:"ShrimpLight");}
            E(g,.13f,.055f,-.21f,.17f,.13f,.26f,Orange);Small(g,new Vector3(.18f,.11f,-.28f),Vector3.one*.025f,Black);Small(g,new Vector3(.085f,.11f,-.28f),Vector3.one*.025f,Black);
            for(int s=-1;s<=1;s+=2){Path(g,new List<Vector3>{new Vector3(.12f,.075f,-.29f),new Vector3(.10f+s*.08f,.10f,-.43f),new Vector3(.12f+s*.14f,.075f,-.53f)},.007f,Orange,4);E(g,-.1f,.06f,.19f,.14f,.03f,.09f,Orange);}
        }
        void MushroomPiece(Transform p,Vector3 at,int type=0,float scale=1)
        {
            var g=Group("Mushroom",p,at);g.localScale=Vector3.one*scale;
            if(type==0){C(g,0,.08f,0,.09f,.16f,.09f,Tofu);E(g,0,.20f,0,.38f,.19f,.36f,Mushroom);for(int i=0;i<3;i++){var slash=B(g,0,.298f,0,.22f,.009f,.022f,Tofu);slash.localRotation=Quaternion.Euler(0,i*60,0);}}
            else if(type==1)for(int i=0;i<14;i++){float x=(i%5-2)*.045f,z=(i/5-1)*.047f,h=.40f+(i%3)*.05f;Rod(g,new Vector3(x,0,z),new Vector3(x*1.3f,h,z*1.3f),.017f,Tofu);E(g,x*1.3f,h,z*1.3f,.065f,.047f,.065f,Tofu);}
            else{Rod(g,Vector3.zero,new Vector3(0,.16f,0),.09f,Tofu);E(g,0,.19f,0,.45f,.13f,.30f,Tofu);for(int i=0;i<7;i++)Rod(g,new Vector3(0,.14f,.10f),new Vector3((i-3)*.055f,.21f,-.11f),.009f,Mushroom);}
        }
        void GreensPiece(Transform p,Vector3 at,int kind=0,float scale=1)
        {
            var g=Group("Leaf vegetable",p,at);g.localScale=Vector3.one*scale;
            for(int i=0;i<5;i++)
            {
                float a=i*2.4f;Vector3 tip=new Vector3(Mathf.Sin(a)*.16f,.28f+(i%2)*.12f,Mathf.Cos(a)*.16f);
                Rod(g,Vector3.zero,tip,.035f,kind==0?"CabbageStem":Green);
                var leaf=Small(g,tip,new Vector3(kind==2?.10f:.23f,.33f,.055f),kind==0?"CabbageLeaf":Leaf);leaf.localRotation=Quaternion.Euler(20,i*137,25);
                Rod(g,tip-Vector3.up*.12f,tip+Vector3.up*.12f,.01f,kind==0?Cream:"LightLeaf");
            }
        }
        void CornPiece(Transform p,Vector3 at,float scale=1)
        {
            var g=Group("Corn cross section",p,at);g.localScale=Vector3.one*scale;C(g,0,.13f,0,.24f,.26f,.24f,Tofu);
            for(int j=0;j<4;j++)for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Small(g,new Vector3(Mathf.Cos(a)*.145f,.035f+j*.065f,Mathf.Sin(a)*.145f),new Vector3(.087f,.061f,.082f),Yellow);}C(g,0,.267f,0,.20f,.015f,.20f,"CornCore");
        }
        void TomatoPiece(Transform p,Vector3 at,float angle=0,float scale=1)
        {
            var g=Group("Tomato wedge",p,at);g.localScale=Vector3.one*scale;g.localRotation=Quaternion.Euler(0,angle,0);
            E(g,0,.09f,0,.34f,.20f,.23f,Red);E(g,0,.16f,0,.29f,.05f,.19f,"TomatoFlesh");
            for(int i=0;i<6;i++)Small(g,new Vector3((i%3-1)*.067f,.186f,(i/3-.5f)*.075f),new Vector3(.035f,.012f,.02f),Yellow);
        }
        void Noodles(Transform p,Vector3 at,bool rice=false,float scale=1)
        {
            var g=Group("Noodle nest",p,at);g.localScale=Vector3.one*scale;
            for(int j=0;j<12;j++){var line=new List<Vector3>();for(int k=0;k<30;k++){float a=k*.37f+j*1.1f,r=.17f+.025f*(j%5)+.035f*Mathf.Sin(k*.9f+j);line.Add(new Vector3(Mathf.Cos(a)*r,.025f+j*.009f,Mathf.Sin(a)*r));}Path(g,line,rice?.006f:.013f,rice?Cream:Noodle,5);}
        }
        void IngredientTray(int type,Transform p)
        {
            bool rectangle=type==0||type==1||type==9||type==10||type==15;
            bool basket=type>=12&&type<=14;
            float y=.10f;
            if(type==20||type==21||type==22){Condiments(type,p);return;}
            if(rectangle){U(p,0,.035f,0,1.15f,.07f,.89f,type<2?Cream:Wood);foreach(int s in new[]{-1,1}){B(p,s*.56f,.075f,0,.04f,.08f,.86f,type<2?Cream:Wood);B(p,0,.075f,s*.42f,1.12f,.08f,.04f,type<2?Cream:Wood);}}
            else if(basket){C(p,0,.04f,0,1.10f,.08f,1.10f,Bamboo);R(p,0,.09f,0,1.14f,Bamboo);for(int i=0;i<28;i++){float a=i*Mathf.PI/14;var weave=B(p,Mathf.Cos(a)*.54f,.07f,Mathf.Sin(a)*.54f,.065f,.12f,.018f,Wood);weave.localRotation=Quaternion.Euler(0,-i*360/28,0);}}
            else{Shape("Serving dish",p,Vector3.zero,new Vector3(1.1f,.27f,1.1f),type==5||type<7?Cream:type==11?Wood:Black,bowl);R(p,0,.135f,0,1.1f,type==5||type<7?Cream:type==11?Wood:Black);y=.14f;}
            if(type<12&&type!=0&&type!=1&&type!=10&&type!=11)for(int i=0;i<9;i++){float a=i*.70f;Small(p,new Vector3(Mathf.Cos(a)*.37f,y,Mathf.Sin(a)*.37f),new Vector3(.24f,.045f,.24f),Leaf);}
            switch(type)
            {
                case 0:case 1:for(int i=0;i<12;i++)MeatRoll(p,new Vector3((i%4-1.5f)*.25f,.23f,(i/4-1)*.25f),type==1,.78f);break;
                case 2:case 3:
                    for(int i=0;i<8;i++){float a=i*2.4f;var fillet=E(p,Mathf.Cos(a)*.28f,y+.055f+i*.008f,Mathf.Sin(a)*.28f,.37f,.11f,.22f,type==2?Pink:WhiteFish);fillet.localRotation=Quaternion.Euler(0,i*30,0);if(type==3)for(int j=0;j<4;j++)Rod(fillet,new Vector3(-.27f+j*.16f,.42f,-.25f),new Vector3(-.16f+j*.16f,.42f,.25f),.012f,Cream);}break;
                case 4:for(int i=0;i<6;i++)ShrimpPiece(p,new Vector3((i%3-1)*.27f,y+.03f,(i/3-.5f)*.34f),i*33,.65f);break;
                case 5:
                    for(int i=0;i<9;i++){float a=i*2.4f;Shape("Squid ring",p,new Vector3(Mathf.Cos(a)*.27f,y+.05f+i%2*.045f,Mathf.Sin(a)*.27f),new Vector3(.29f,.65f,.29f),"Squid",ring);}for(int i=0;i<4;i++)Path(p,new List<Vector3>{new Vector3(.2f,y,.1f),new Vector3(.38f,y+.04f,.23f+i*.04f),new Vector3(.44f,y,.31f+i*.04f)},.035f,"Squid",6);break;
                case 6:
                    for(int i=0;i<7;i++){float a=i*2.4f;var shell=Group("Half shell",p,new Vector3(Mathf.Cos(a)*.30f,y,Mathf.Sin(a)*.30f));shell.localRotation=Quaternion.Euler(0,i*40,25);E(shell,0,.025f,0,.25f,.055f,.43f,"Shell");E(shell,0,.049f,0,.20f,.025f,.36f,TealMaterial());E(shell,0,.085f,0,.12f,.065f,.23f,Orange);}break;
                case 7:
                    for(int i=0;i<12;i++){float a=i*2.4f;Vector3 at=new Vector3(Mathf.Cos(a)*(.18f+i%2*.14f),y+.12f+i%3*.055f,Mathf.Sin(a)*(.18f+i%2*.14f));E(p,at.x,at.y,at.z,.22f,.22f,.22f,i%3==0?Mushroom:Tofu);for(int k=0;k<3;k++)Small(p,at+new Vector3((k-1)*.04f,.105f,.04f),Vector3.one*.018f,k==0?Leaf:Orange);}break;
                case 8:for(int i=0;i<9;i++)U(p,(i%3-1)*.22f,y+.1f+(i/3==1?.13f:0),(i/3-1)*.22f,.23f,.20f,.23f,Tofu);break;
                case 9:for(int i=0;i<8;i++){var g=Group("Tofu skin roll",p,new Vector3((i%4-1.5f)*.24f,.2f,(i/4-.5f)*.35f));var m=Lathe(new[]{new Vector2(.085f,-.24f),new Vector2(.115f,-.24f),new Vector2(.115f,.24f),new Vector2(.085f,.24f),new Vector2(.085f,-.24f)},12);kitchenMeshes.Add(m);var o=Shape("Crisp tofu tube",g,Vector3.zero,Vector3.one,Yellow,m);o.localRotation=Quaternion.Euler(90,0,0);for(int j=0;j<4;j++)R(g,0,0,0,.02f,Yellow);}break;
                case 10:for(int i=0;i<4;i++){var bundle=Group("Enoki bundle",p,new Vector3((i%2-.5f)*.45f,y,(i/2-.5f)*.35f));bundle.localRotation=Quaternion.Euler(55,0,0);MushroomPiece(bundle,Vector3.zero,1,.85f);}break;
                case 11:for(int i=0;i<6;i++)MushroomPiece(p,new Vector3(Mathf.Sin(i*2.4f)*.28f,y,Mathf.Cos(i*2.4f)*.28f),0,.9f);break;
                case 12:for(int i=0;i<7;i++)MushroomPiece(p,new Vector3(Mathf.Sin(i*2.4f)*.27f,y,Mathf.Cos(i*2.4f)*.27f),2,1);break;
                case 13:for(int i=0;i<5;i++)GreensPiece(p,new Vector3((i-2)*.16f,y,0),0,1.2f);break;
                case 14:for(int i=0;i<9;i++){var g=Group("Water spinach",p,new Vector3(Mathf.Sin(i*2.4f)*.27f,y,Mathf.Cos(i*2.4f)*.27f));g.localRotation=Quaternion.Euler(60,i*40,0);GreensPiece(g,Vector3.zero,2,1.15f);}break;
                case 15:for(int i=0;i<5;i++){var g=Group("Bok choy",p,new Vector3((i-2)*.19f,y,-.18f+(i%2)*.13f));g.localRotation=Quaternion.Euler(35,0,0);E(g,0,.07f,0,.15f,.25f,.15f,"CabbageStem");GreensPiece(g,new Vector3(0,.09f,0),1,1);}break;
                case 16:for(int i=0;i<5;i++)CornPiece(p,new Vector3(Mathf.Sin(i*2.4f)*.29f,y,Mathf.Cos(i*2.4f)*.29f));break;
                case 17:for(int i=0;i<7;i++)TomatoPiece(p,new Vector3(Mathf.Sin(i*2.4f)*.27f,y,Mathf.Cos(i*2.4f)*.27f),i*40);break;
                case 18:case 19:Noodles(p,new Vector3(0,y,0),type==19,1.35f);break;
                case 23:
                    for(int i=0;i<4;i++)GreensPiece(p,new Vector3(-.23f+i*.12f,y,.16f),1,.55f);for(int i=0;i<5;i++){var lime=E(p,-.25f+i*.13f,y+.08f,-.20f,.20f,.12f,.16f,"Lime");for(int j=0;j<3;j++){var rib=B(lime,0,.5f,0,.78f,.025f,.045f,Tofu);rib.localRotation=Quaternion.Euler(0,j*60,0);}}for(int i=0;i<4;i++)Shape("Onion ring",p,new Vector3(.3f,y+.025f*i,.17f),new Vector3(.27f,.4f,.27f),Cream,ring);break;
            }
        }
        string TealMaterial(){return "Teal";}
        void Condiments(int type,Transform p)
        {
            if(type==22)
            {for(int i=0;i<2;i++){var jug=Group("Broth pitcher",p,new Vector3((i-.5f)*.6f,0,0));Vessel(jug,.46f,.50f,i==0?Black:Cream,false);Rod(jug,new Vector3(0,.2f,0),new Vector3(.17f,.75f,0),.035f,Wood);C(jug,0,.43f,0,.39f,.025f,.39f,i==0?"Broth":Soup);}return;}
            B(p,0,.04f,0,type==20?1.5f:1.08f,.08f,.52f,Wood);
            int count=type==20?3:2;for(int i=0;i<count;i++)
            {float x=(i-(count-1)*.5f)*.48f;Shape("Condiment bowl",p,new Vector3(x,.085f,0),new Vector3(.43f,.43f,.43f),Cream,bowl);C(p,x,.265f,0,.36f,.015f,.36f,type==20?(i==0?Soup:i==1?Dark:Chili):i==0?Red:Tofu);
                for(int k=0;k<16;k++){float a=k*2.4f,r=.035f+k%4*.035f;Small(p,new Vector3(x+Mathf.Cos(a)*r,.28f,Mathf.Sin(a)*r),Vector3.one*(type==21?.05f:.02f),type==21?(i==0?Red:Tofu):i==0?Leaf:Yellow);}}
        }
    }
}
