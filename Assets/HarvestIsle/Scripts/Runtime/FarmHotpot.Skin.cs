using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private Texture2D hotpotPanel,hotpotButton,hotpotHover,hotpotActive,hotpotDisc;
        private GUIStyle panelStyle,navStyle,activeNavStyle,smallStyle;
        private void EnsureHotpotSkin()
        {
            if(hotpotPanel!=null)return;
            hotpotPanel=RoundedSurface("FFF6DF",.95f);
            hotpotButton=RoundedSurface("E8D7AF",1);
            hotpotHover=RoundedSurface("F5E8C8",1);
            hotpotActive=RoundedSurface("E8B94D",1);
            hotpotDisc=new Texture2D(32,32,TextureFormat.RGBA32,false);
            var pixels=new Color[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++){float a=Mathf.Clamp01(15.5f-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f)));pixels[y*32+x]=new Color(1,1,1,a);}
            hotpotDisc.SetPixels(pixels);hotpotDisc.Apply(false,true);
            textStyle=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true,richText=false};textStyle.normal.textColor=FarmWorld.Hex("5A4327");
            textStyle.hover.textColor=textStyle.active.textColor=textStyle.focused.textColor=textStyle.normal.textColor;
            headingStyle=new GUIStyle(textStyle){fontSize=24,fontStyle=FontStyle.Bold};
            smallStyle=new GUIStyle(textStyle){fontSize=14,alignment=TextAnchor.MiddleCenter};
            buttonStyle=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true,richText=false,border=new RectOffset(12,12,12,12),padding=new RectOffset(8,8,5,5)};
            buttonStyle.normal.background=hotpotButton;buttonStyle.hover.background=hotpotHover;buttonStyle.active.background=hotpotActive;
            buttonStyle.normal.textColor=buttonStyle.hover.textColor=buttonStyle.active.textColor=FarmWorld.Hex("5A4327");
            navStyle=new GUIStyle(buttonStyle);navStyle.normal.background=hotpotPanel;
            activeNavStyle=new GUIStyle(navStyle);activeNavStyle.normal.background=hotpotActive;
            panelStyle=new GUIStyle{border=new RectOffset(12,12,12,12)};panelStyle.normal.background=hotpotPanel;
        }
        private Texture2D RoundedSurface(string hex,float opacity)
        {
            var t=new Texture2D(32,32,TextureFormat.RGBA32,false);t.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color[1024];Color c=FarmWorld.Hex(hex);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {
                float dx=Mathf.Max(0,Mathf.Abs(x-15.5f)-6.5f),dy=Mathf.Max(0,Mathf.Abs(y-15.5f)-6.5f);
                float d=Mathf.Sqrt(dx*dx+dy*dy);float edge=Mathf.Clamp01(9-d);
                Color p=c*(.92f+y*.004f);if(d>7.3f)p=Color.Lerp(p,FarmWorld.Hex("A39369"),.35f);p.a=edge*opacity;pixels[y*32+x]=p;
            }
            t.SetPixels(pixels);t.Apply(false,true);return t;
        }
        private void Ink(Rect rect,Color color,bool disc=false)
        {var previous=GUI.color;GUI.color=color;GUI.DrawTexture(rect,disc?hotpotDisc:Texture2D.whiteTexture);GUI.color=previous;}
        private void NavIcon(Rect r,int icon)
        {
            Color ink=FarmWorld.Hex("695135");float x=r.x,y=r.y;
            if(icon==0)
            {
                Ink(new Rect(x+4,y+3,28,5),ink);Ink(new Rect(x+7,y+9,22,17),ink);
                Ink(new Rect(x+4,y+28,28,5),ink);Ink(new Rect(x+7,y+32,4,10),ink);Ink(new Rect(x+25,y+32,4,10),ink);
            }
            else if(icon==1)
            {Ink(new Rect(x+3,y+7,32,25),ink);Ink(new Rect(x+7,y+12,24,4),FarmWorld.Hex("E8D7AF"));Ink(new Rect(x+16,y+9,4,20),FarmWorld.Hex("E8D7AF"));Ink(new Rect(x+6,y+33,4,7),ink);Ink(new Rect(x+28,y+33,4,7),ink);}
            else if(icon==2)
            {Ink(new Rect(x+3,y+21,32,17),ink,true);Ink(new Rect(x,y+20,38,4),ink);Ink(new Rect(x+10,y+5,3,11),ink);Ink(new Rect(x+21,y+2,3,14),ink);}
            else if(icon==3)
            {Ink(new Rect(x+3,y+5,30,34),ink);Ink(new Rect(x+8,y+11,20,3),FarmWorld.Hex("E8D7AF"));Ink(new Rect(x+8,y+19,20,3),FarmWorld.Hex("E8D7AF"));Ink(new Rect(x+8,y+27,14,3),FarmWorld.Hex("E8D7AF"));}
            else
            {Ink(new Rect(x+9,y+28,22,12),ink);Ink(new Rect(x+17,y+12,4,20),ink);Ink(new Rect(x+1,y+12,18,12),ink,true);Ink(new Rect(x+21,y+7,16,14),ink,true);Ink(new Rect(x+14,y+1,11,16),ink,true);}
        }
        private void DrawCompactNavigation()
        {
            string[] names={"Phòng ăn","Nhập hàng","Nấu món","Thực đơn","Trang trí","Quản lý","Sổ tay","Cao điểm"};
            for(int i=0;i<names.Length;i++)
            {
                Rect r=new Rect(16+i*88,633,78,75);
                if(GUI.Button(r,GUIContent.none,tab==i?activeNavStyle:navStyle)){CancelDecorPlacement();tab=i;scroll=Vector2.zero;moveFrom=-1;}
                if(i<5)NavIcon(new Rect(r.x+20,r.y+5,38,42),i);else GUI.Label(new Rect(r.x,r.y+5,r.width,42),i==5?"QL":i==6?"SỔ":"90s",headingStyle);
                GUI.Label(new Rect(r.x,r.y+49,r.width,22),names[i],smallStyle);
            }
        }
        private void DisposeHotpotSkin()
        {
            foreach(var t in new[]{hotpotPanel,hotpotButton,hotpotHover,hotpotActive,hotpotDisc})if(t!=null)Destroy(t);
        }
    }
}
