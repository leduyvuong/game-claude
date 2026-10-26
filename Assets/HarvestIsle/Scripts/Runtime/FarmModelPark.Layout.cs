using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmModelPark
    {
        private GameObject Block(string name,Vector3 position,Vector3 size,string color)
        {return Game.World.Shape(PrimitiveType.Cube,name,park,position,size,FarmWorld.Hex(color));}

        private TextMesh Sign(string text,Vector3 position,float size,Color color)
        {
            var go=new GameObject(text);go.transform.SetParent(park,false);go.transform.position=position;
            go.transform.rotation=Quaternion.Euler(56,0,0);
            var label=go.AddComponent<TextMesh>();label.font=font;label.fontSize=64;label.characterSize=size;
            label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.text=text;label.color=color;
            go.GetComponent<MeshRenderer>().sharedMaterial=font.material;
            return label;
        }

        private void Ornament(string resource,Vector3 position,float height,float rotation=0)
        {
            var prefab=Resources.Load<GameObject>(resource);if(prefab==null)return;
            var model=Instantiate(prefab,park,false);model.transform.position=position;
            model.transform.rotation=Quaternion.Euler(0,rotation,0);
            foreach(var c in model.GetComponentsInChildren<Collider>(true))c.enabled=false;
            var renderers=model.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return;
            Bounds b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
            model.transform.localScale*=height/Mathf.Max(.01f,b.size.y);
            b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
            model.transform.position+=new Vector3(position.x-b.center.x,position.y-b.min.y,position.z-b.center.z);
        }

        private void BuildDistrict(District district,int index)
        {
            Rect a=district.area;Vector3 center=new Vector3(a.center.x,0,a.center.y);
            string[] greens={"9BAF79","A5BA88","B6C394","94B7A3","B4BC87"};
            Block("Viền vườn · "+district.name,center+Vector3.up*.02f,new Vector3(a.width,.24f,a.height),"698B70");
            Block("Thảm cỏ · "+district.name,center+Vector3.up*.16f,new Vector3(a.width-.5f,.12f,a.height-.5f),greens[index%greens.Length]);
            Block("Lối vào · "+district.name,new Vector3(a.center.x,.24f,a.y+1.2f),new Vector3(a.width-1,.1f,1.9f),"F1DEB8");
            Sign((index+1).ToString("00")+"   "+district.name.ToUpperInvariant(),new Vector3(a.center.x,.44f,a.y+1.1f),Mathf.Min(.11f,a.width/(district.name.Length+6)*.08f),FarmWorld.Hex("284C41"));
            // A pair of tall plants frames every garden without covering the exhibits.
            Ornament(index%2==0?"Environment/ArecaPalm":"Environment/YoungForestTree",new Vector3(a.x+.6f,.23f,a.yMax-.7f),2.5f,index*31);
            Ornament("Collection/Fern",new Vector3(a.xMax-.65f,.23f,a.yMax-.6f),.7f,index*53);
            for(float p=a.x+2;p<a.xMax-1;p+=6)
            {
                Block("Bordure",new Vector3(p,.32f,a.yMax-.4f),new Vector3(2.8f,.22f,.25f),"DFD5AD");
                Ornament("Collection/Wildflowers",new Vector3(p,.43f,a.yMax-.35f),.35f);
            }
        }

        private void BuildPromenade()
        {
            // Small resting gardens occupy the gaps behind shorter exhibition terraces.
            foreach(var district in Districts)
            {
                float back=district.area.yMax;
                foreach(var other in Districts)if(Mathf.Approximately(other.area.y,district.area.y))back=Mathf.Max(back,other.area.yMax);
                if(back-district.area.yMax<9)continue;
                float x=district.area.center.x,z=(district.area.yMax+back)*.5f;
                Block("Rest garden",new Vector3(x,.02f,z),new Vector3(district.area.width-1,.14f,back-district.area.yMax-3),"8BA586");
                Game.World.Shape(PrimitiveType.Cylinder,"Reflecting pool",park,new Vector3(x,.17f,z),new Vector3(Mathf.Min(8,district.area.width-4),.11f,4),FarmWorld.Hex("CFBD94"));
                Game.World.Shape(PrimitiveType.Cylinder,"Still water",park,new Vector3(x,.29f,z),new Vector3(Mathf.Min(7.3f,district.area.width-4.7f),.015f,3.3f),FarmWorld.Hex("5FA8A6"));
                Ornament("Collection/Bench",new Vector3(x-3,.17f,z-3),.75f);
                Ornament("Collection/Bench",new Vector3(x+3,.17f,z-3),.75f);
                Ornament("Environment/FlameTree",new Vector3(x-3,.17f,z+3),3);
                Ornament("Environment/ArecaPalm",new Vector3(x+3,.17f,z+3),3.4f);
            }
            float length=northEdge-59;
            Block("Turquoise lagoon",new Vector3(0,-1.05f,northEdge*.35f),new Vector3(2000,.2f,2000),"73BCC0");
            Block("Park foundation",new Vector3(0,-.64f,59+length*.5f),new Vector3(267,.7f,length+9),"A4865E");
            Block("Sand bank",new Vector3(0,-.25f,59+length*.5f),new Vector3(265,.25f,length+7),"EAD7AB");
            Block("Stone promenade",new Vector3(0,-.06f,59+length*.5f),new Vector3(262,.14f,length+4),"C6BA9F");
            for(int side=-1;side<=1;side+=2)
            for(float z=63;z<northEdge;z+=8)
            {
                Ornament("Environment/ArecaPalm",new Vector3(side*129,.04f,z),3.2f,z*12);
                Ornament("Collection/FestivalLantern",new Vector3(side*127.8f,.04f,z+3.5f),1.1f);
            }
            Block("Cầu dạo ven hồ",new Vector3(0,.05f,58),new Vector3(5,.3f,13),"C99D67");
            for(int i=0;i<13;i++)Block("Ván cầu",new Vector3(0,.23f,52+i),new Vector3(5,.07f,.82f),i%2==0?"DEC39A":"E8CEAA");
            for(int side=-1;side<=1;side+=2)
            for(int i=0;i<5;i++)
            {
                Block("Cột cầu",new Vector3(side*2.5f,.65f,53+i*2.2f),new Vector3(.15f,1.1f,.15f),"856947");
                Ornament("Collection/FestivalLantern",new Vector3(side*3.5f,.1f,63),1.3f);
            }
            Ornament("Collection/FestivalArch",new Vector3(0,.3f,64),3.8f);
            Sign("MÙA VÀNG\nCÔNG VIÊN MODEL",new Vector3(0,.9f,60.8f),.55f,FarmWorld.Hex("335B4C"));
        }

        private void DressFarm()
        {
            // Courtyards around the actual production buildings, outside the 36 playable plots.
            for(int row=0;row<2;row++)
            {
                Block("Lối dạo xưởng",new Vector3(0,.19f,-7.9f-row*3),new Vector3(15,.08f,.65f),"EAD6A9");
                for(int column=0;column<5;column++)
                {
                    float x=(column-2)*2.8f,z=-6.6f-row*3;
                    Block("Sân xưởng",new Vector3(x,.19f,z),new Vector3(2.45f,.06f,2.15f),"D3BA8C");
                    Ornament("Collection/Wildflowers",new Vector3(x+1,.25f,z-.8f),.32f);
                }
            }
            for(int i=0;i<9;i++)
            {
                float x=-5.6f+i*1.4f;
                Ornament("Collection/Wildflowers",new Vector3(x,.22f,-11.5f),.4f,i*27);
                if(i%2==0)Ornament("Collection/FestivalLantern",new Vector3(x,.18f,-12),1f);
            }
            for(int side=-1;side<=1;side+=2)
            {
                Block("Lối vườn",new Vector3(side*8,.2f,-3),new Vector3(.7f,.07f,15),"DCC79E");
                Ornament("Collection/Bench",new Vector3(side*7,.2f,-10.8f),.65f,side*75);
                Ornament("Environment/FlameTree",new Vector3(side*8,.15f,-10.5f),2.4f);
            }
            Ornament("Collection/FestivalArch",new Vector3(0,.2f,-12.7f),2.7f);
            Ornament("Collection/FestivalLantern",new Vector3(-2,.2f,-12.7f),1.2f);
            Ornament("Collection/FestivalLantern",new Vector3(2,.2f,-12.7f),1.2f);
        }
    }
}
