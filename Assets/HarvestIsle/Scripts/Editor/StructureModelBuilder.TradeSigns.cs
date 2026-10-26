using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        // Relief pictograms face local +Z, like the working fronts of the machines.
        // Supports are part of the same baked mesh; no text, textures or new materials.
        private static void TradeSign(EnvironmentMesh m,string type)
        {
            Vector3 p;int ground=Black;bool apron=false;
            switch(type)
            {
                case "Windmill":
                    // Beside the door and behind the complete swept rotor plane.
                    p=new Vector3(1.08f,.62f,.55f);
                    Box(m,new Vector3(1.08f,.10f,.48f),new Vector3(.23f,.20f,.23f),Stone);
                    Beam(m,new Vector3(1.08f,.10f,.48f),new Vector3(1.08f,.88f,.48f),.045f);
                    Beam(m,new Vector3(.84f,.85f,.48f),new Vector3(1.32f,.85f,.48f),.028f);
                    break;
                case "BakeryOven":p=new Vector3(1.12f,.58f,.77f);apron=true;ground=TileDark;break;
                case "HerbalBench":p=new Vector3(-.45f,.58f,.54f);apron=true;ground=StoneDark;break;
                case "CompostBin":p=new Vector3(0,.46f,.66f);apron=true;ground=StoneDark;break;
                case "DairyChurn":case "JuicePress":case "SoyVat":
                    p=new Vector3(-.96f,1.48f,-.27f);
                    float post=type=="SoyVat"?-1.28f:type=="JuicePress"?-1.255f:-1.205f;
                    Beam(m,new Vector3(post,1.77f,-1.23f),new Vector3(post,1.77f,-.29f),.035f);
                    Beam(m,new Vector3(post,1.34f,-1.23f),new Vector3(post,1.77f,-.49f),.027f);
                    Beam(m,new Vector3(post,1.77f,-.29f),new Vector3(-.66f,1.77f,-.29f),.032f);
                    ground=type=="DairyChurn"?StoneDark:type=="JuicePress"?TileDark:Black;
                    break;
                case "Greenhouse":
                    p=new Vector3(-.78f,1.25f,1.02f);ground=StoneDark;
                    Beam(m,new Vector3(-1.18f,1.53f,.94f),new Vector3(-.48f,1.53f,1.0f),.026f,StoneDark);
                    Beam(m,new Vector3(-1.18f,1.23f,.94f),new Vector3(-.92f,1.53f,1.0f),.020f,StoneDark);
                    break;
                case "Nursery":
                    p=new Vector3(0,1.17f,1.01f);ground=StoneDark;
                    Beam(m,new Vector3(-1.03f,1.42f,.72f),new Vector3(1.03f,1.42f,.72f),.028f,Bamboo);
                    for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.21f,1.42f,.72f),new Vector3(side*.21f,1.42f,1.01f),.022f,Bamboo);
                    break;
                case "MarketStall":
                    p=new Vector3(0,1.76f,.96f);ground=StoneDark;apron=true;
                    for(int side=-1;side<=1;side+=2)Beam(m,new Vector3(side*.21f,1.50f,.88f),new Vector3(side*.21f,1.98f,.92f),.024f);
                    break;
                default:return;
            }
            const float width=.66f,height=.38f;
            Box(m,p,new Vector3(width,height,.065f),WoodDark);
            Box(m,p+Vector3.forward*.038f,new Vector3(width-.065f,height-.065f,.022f),ground);
            for(int side=-1;side<=1;side+=2)
            {
                Box(m,p+new Vector3(0,side*(height/2-.016f),.045f),new Vector3(width,.032f,.045f),Wood);
                Box(m,p+new Vector3(side*(width/2-.016f),0,.045f),new Vector3(.032f,height,.045f),Wood);
                if(apron)
                {
                    // Broad straps reach the table apron / enclosure behind the plate.
                    Box(m,p+new Vector3(side*.23f,0,-.055f),new Vector3(.055f,.46f,.10f),WoodDark);
                }
                else
                {
                    Box(m,p+new Vector3(side*.21f,.223f,-.014f),new Vector3(.035f,.112f,.037f),StoneDark);
                    Beam(m,p+new Vector3(side*.21f,.266f,-.09f),p+new Vector3(side*.21f,.266f,.012f),.018f,Gold);
                }
                for(int row=-1;row<=1;row+=2)
                    Beam(m,p+new Vector3(side*.286f,row*.145f,.061f),p+new Vector3(side*.286f,row*.145f,.077f),.014f,Gold);
            }
            TradeGlyph(m,p+Vector3.forward*.063f,type);
        }

        // Small convex silhouettes extruded in the XY plane. Coarse relief reads at
        // farm zoom and avoids the cost and visual noise of miniature real props.
        private static void SignRelief(EnvironmentMesh m,Vector3 p,int color,params Vector2[] outline)
        {
            const float relief=.019f;
            for(int i=1;i<outline.Length-1;i++)
            {
                Vector3 a=p+new Vector3(outline[0].x,outline[0].y,relief);
                Vector3 b=p+new Vector3(outline[i].x,outline[i].y,relief);
                Vector3 c=p+new Vector3(outline[i+1].x,outline[i+1].y,relief);
                Face(m,a,b,c,c,color);
            }
            for(int i=0;i<outline.Length;i++)
            {
                Vector2 a=outline[i],b=outline[(i+1)%outline.Length];
                Face(m,p+new Vector3(a.x,a.y,0),p+new Vector3(b.x,b.y,0),p+new Vector3(b.x,b.y,relief),p+new Vector3(a.x,a.y,relief),color);
            }
        }
        private static void SignLeaf(EnvironmentMesh m,Vector3 p,Vector2 from,Vector2 to,float width,int color)
        {
            Vector2 mid=(from+to)*.5f,side=new Vector2(-(to-from).y,(to-from).x).normalized*width;
            SignRelief(m,p,color,from,mid+side,to,mid-side);
        }
        private static void TradeGlyph(EnvironmentMesh m,Vector3 p,string type)
        {
            switch(type)
            {
                case "Windmill":
                    Beam(m,p+new Vector3(-.03f,-.13f,.025f),p+new Vector3(.04f,.13f,.025f),.013f,Gold);
                    for(int i=0;i<3;i++)
                    {
                        Vector2 stem=new Vector2(-.02f+i*.018f,-.055f+i*.066f);
                        SignLeaf(m,p,stem,stem+new Vector2(-.105f,.055f),.027f,Gold);
                        SignLeaf(m,p,stem+new Vector2(.012f,.012f),stem+new Vector2(.11f,.065f),.027f,Plaster);
                    }
                    break;
                case "BakeryOven":
                    SignRelief(m,p,Gold,new Vector2(-.22f,-.07f),new Vector2(-.23f,.015f),new Vector2(-.16f,.09f),new Vector2(.16f,.09f),new Vector2(.23f,.015f),new Vector2(.22f,-.07f));
                    for(int i=0;i<3;i++)Beam(m,p+new Vector3(-.13f+i*.11f,.066f,.027f),p+new Vector3(-.09f+i*.11f,-.015f,.027f),.013f,TileDark);
                    break;
                case "DairyChurn":
                    SignRelief(m,p,Plaster,new Vector2(-.09f,-.13f),new Vector2(.09f,-.13f),new Vector2(.09f,.045f),new Vector2(.045f,.09f),new Vector2(-.045f,.09f),new Vector2(-.09f,.045f));
                    Box(m,p+new Vector3(0,.113f,.014f),new Vector3(.115f,.042f,.03f),Water);
                    Box(m,p+new Vector3(0,-.025f,.03f),new Vector3(.105f,.065f,.016f),Water);
                    break;
                case "JuicePress":
                    SignRelief(m,p+Vector3.left*.07f,Gold,new Vector2(-.12f,-.10f),new Vector2(-.15f,-.015f),new Vector2(-.10f,.085f),new Vector2(.02f,.10f),new Vector2(.10f,.025f),new Vector2(.09f,-.08f),new Vector2(0,-.12f));
                    SignLeaf(m,p,new Vector2(-.05f,.085f),new Vector2(.065f,.14f),.027f,Plaster);
                    SignRelief(m,p+Vector3.right*.17f,Water,new Vector2(0,.075f),new Vector2(.052f,-.035f),new Vector2(.035f,-.09f),new Vector2(-.035f,-.09f),new Vector2(-.052f,-.035f));
                    break;
                case "SoyVat":
                    SignRelief(m,p,TileLight,new Vector2(-.10f,-.13f),new Vector2(.10f,-.13f),new Vector2(.14f,.02f),new Vector2(.085f,.10f),new Vector2(-.085f,.10f),new Vector2(-.14f,.02f));
                    Box(m,p+new Vector3(0,.115f,.015f),new Vector3(.23f,.035f,.03f),Gold);
                    SignLeaf(m,p+Vector3.forward*.025f,new Vector2(-.055f,-.045f),new Vector2(.055f,.035f),.04f,Plaster);
                    break;
                case "HerbalBench":
                    SignRelief(m,p,Plaster,new Vector2(-.14f,.01f),new Vector2(.14f,.01f),new Vector2(.10f,-.11f),new Vector2(-.10f,-.11f));
                    Beam(m,p+new Vector3(.02f,-.01f,.03f),p+new Vector3(.13f,.13f,.03f),.024f,Gold);
                    SignLeaf(m,p,new Vector2(-.08f,.035f),new Vector2(-.20f,.14f),.041f,Gold);
                    break;
                case "CompostBin":
                    SignRelief(m,p,TileLight,new Vector2(-.21f,-.12f),new Vector2(.21f,-.12f),new Vector2(.12f,-.015f),new Vector2(0,.035f),new Vector2(-.13f,-.02f));
                    SignLeaf(m,p+Vector3.forward*.025f,new Vector2(-.05f,-.01f),new Vector2(.12f,.14f),.06f,Gold);
                    break;
                case "Nursery":case "Greenhouse":
                    Beam(m,p+new Vector3(0,-.06f,.026f),p+new Vector3(0,.09f,.026f),.013f,Gold);
                    SignLeaf(m,p,new Vector2(0,.025f),new Vector2(-.15f,.12f),.043f,Plaster);
                    SignLeaf(m,p,new Vector2(0,.055f),new Vector2(.15f,.14f),.043f,Gold);
                    if(type=="Nursery")SignRelief(m,p,TileLight,new Vector2(-.10f,-.045f),new Vector2(.10f,-.045f),new Vector2(.075f,-.14f),new Vector2(-.075f,-.14f));
                    else
                    {
                        Beam(m,p+new Vector3(-.20f,-.13f,.015f),p+new Vector3(-.20f,.045f,.015f),.012f,Water);
                        Beam(m,p+new Vector3(.20f,-.13f,.015f),p+new Vector3(.20f,.045f,.015f),.012f,Water);
                        Beam(m,p+new Vector3(-.20f,.045f,.015f),p+new Vector3(0,.15f,.015f),.012f,Water);
                        Beam(m,p+new Vector3(.20f,.045f,.015f),p+new Vector3(0,.15f,.015f),.012f,Water);
                    }
                    break;
                case "MarketStall":
                    SignRelief(m,p,Gold,new Vector2(-.20f,.015f),new Vector2(.20f,.015f),new Vector2(.15f,-.12f),new Vector2(-.15f,-.12f));
                    for(int i=-1;i<=1;i++)Box(m,p+new Vector3(i*.09f,-.06f,.028f),new Vector3(.013f,.10f,.014f),WoodDark);
                    SignLeaf(m,p,new Vector2(-.07f,.025f),new Vector2(-.12f,.13f),.05f,Plaster);
                    SignLeaf(m,p,new Vector2(.04f,.025f),new Vector2(.15f,.11f),.055f,TileLight);
                    break;
            }
        }
    }
}
