using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class ModelLibraryViewer
    {
        private readonly Dictionary<string,Texture2D> cardImages=new Dictionary<string,Texture2D>();
        private Rect libraryArea;
        private float libraryScale;
        private GUIStyle libraryText,libraryTitle,libraryButton,libraryCard;
        private Texture2D libraryButtonSurface,libraryHoverSurface;
        private static Texture2D LibrarySurface(string hex)
        {var texture=new Texture2D(1,1);texture.SetPixel(0,0,FarmWorld.Hex(hex));texture.Apply();return texture;}
        private void SetLibraryViewport()
        {
            var safe=Screen.safeArea;libraryScale=Mathf.Min(safe.width/1200f,safe.height/720f);
            libraryArea=new Rect(safe.x+(safe.width-1200*libraryScale)/2,Screen.height-safe.yMax+(safe.height-720*libraryScale)/2,1200,720);
            cameraComponent.rect=new Rect((libraryArea.x+615*libraryScale)/Screen.width,1-(libraryArea.y+593*libraryScale)/Screen.height,565*libraryScale/Screen.width,420*libraryScale/Screen.height);
        }
        private void LibraryFill(Rect rect,string hex)
        {var previous=GUI.color;GUI.color=FarmWorld.Hex(hex);GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=previous;}
        private bool LibraryButton(Rect rect,string text){return GUI.Button(rect,text,libraryButton);}
        private void OnGUI()
        {
            if(catalog==null)return;
            if(!Visible){if(GUI.Button(new Rect(18,325,340,34),"Xem TOÀN BỘ MODEL ("+catalog.models.Length+")"))Show();return;}
            // The camera renders only its preview rectangle; cover the rest so no stale farm frame remains.
            Rect viewport=cameraComponent.pixelRect;float top=Screen.height-viewport.yMax;
            LibraryFill(new Rect(0,0,Screen.width,top),"203128");
            LibraryFill(new Rect(0,top,viewport.x,viewport.height),"203128");
            LibraryFill(new Rect(viewport.xMax,top,Screen.width-viewport.xMax,viewport.height),"203128");
            LibraryFill(new Rect(0,top+viewport.height,Screen.width,Screen.height-top-viewport.height),"203128");
            if(libraryText==null)
            {
                libraryText=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};libraryText.normal.textColor=FarmWorld.Hex("F2E5C7");
                libraryTitle=new GUIStyle(libraryText){fontSize=25,fontStyle=FontStyle.Bold};
                libraryButtonSurface=LibrarySurface("405849");libraryHoverSurface=LibrarySurface("806B42");
                libraryButton=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true,border=new RectOffset(0,0,0,0)};
                libraryButton.normal.background=libraryButtonSurface;libraryButton.hover.background=libraryHoverSurface;
                libraryButton.active.background=libraryHoverSurface;libraryButton.focused.background=libraryButtonSurface;
                libraryButton.normal.textColor=libraryButton.hover.textColor=libraryButton.active.textColor=libraryButton.focused.textColor=FarmWorld.Hex("F2E5C7");
                libraryCard=new GUIStyle(libraryText){fontSize=17,alignment=TextAnchor.MiddleCenter};
            }
            var previous=GUI.matrix;GUI.matrix=Matrix4x4.TRS(new Vector3(libraryArea.x,libraryArea.y,0),Quaternion.identity,Vector3.one*libraryScale);
            LibraryFill(new Rect(10,12,1180,143),"283D34");LibraryFill(new Rect(10,155,594,550),"283D34");LibraryFill(new Rect(604,595,586,110),"283D34");
            GUI.Label(new Rect(28,26,870,39),"THƯ VIỆN MODEL · "+catalog.models.Length+" loại",libraryTitle);
            if(LibraryButton(new Rect(988,25,181,42),"Đóng thư viện")){Hide();GUI.matrix=previous;return;}
            GUI.Label(new Rect(28,79,58,30),"Tìm",libraryText);
            string next=GUI.TextField(new Rect(88,76,310,36),search,libraryButton);if(next!=search){search=next;Filter();}
            if(LibraryButton(new Rect(407,76,47,36),"×")){search="";Filter();}
            if(LibraryButton(new Rect(28,119,44,35),"‹")){group=(group+groups.Count-1)%groups.Count;Filter();}
            GUI.Label(new Rect(87,120,441,32),groups[group]+" · "+filtered.Count+" mẫu",libraryText);
            if(LibraryButton(new Rect(540,119,44,35),"›")){group=(group+1)%groups.Count;Filter();}
            const int perPage=9;int pages=Mathf.Max(1,(filtered.Count+perPage-1)/perPage);page=Mathf.Clamp(page,0,pages-1);
            for(int n=0;n<perPage&&page*perPage+n<filtered.Count;n++)
            {
                int index=filtered[page*perPage+n];var item=catalog.models[index];float x=28+n%3*187,y=171+n/3*153;
                LibraryFill(new Rect(x,y,176,142),index==selected?"806B42":"3A5043");
                Texture2D icon;if(!cardImages.TryGetValue(item.id,out icon)){icon=Resources.Load<Texture2D>("ModelThumbnails/"+item.id);cardImages.Add(item.id,icon);}
                if(icon!=null)GUI.DrawTextureWithTexCoords(new Rect(x+33,y+6,110,100),icon,new Rect(.1f,.14f,.8f,.73f));
                GUI.Label(new Rect(x+4,y+107,168,33),item.label,libraryCard);
                if(GUI.Button(new Rect(x,y,176,142),GUIContent.none,GUIStyle.none))Select(index,item.group=="Cây trồng"?Mathf.Min(4,item.variants.Length-1):0);
            }
            if(filtered.Count==0)GUI.Label(new Rect(45,224,520,75),"Không tìm thấy model. Thử từ khóa hoặc nhóm khác.",libraryText);
            if(LibraryButton(new Rect(28,651,64,39),"‹"))page=(page+pages-1)%pages;
            GUI.Label(new Rect(115,658,394,30),"Trang "+(page+1)+" / "+pages,libraryText);
            if(LibraryButton(new Rect(520,651,64,39),"›"))page=(page+1)%pages;
            var entry=catalog.models[selected];
            GUI.Label(new Rect(623,82,549,32),entry.label+" · "+entry.variants[variant].label,libraryText);
            float width=Mathf.Min(93,548f/entry.variants.Length);
            for(int i=0;i<entry.variants.Length;i++)
            {GUI.enabled=i!=variant;if(LibraryButton(new Rect(623+i*width,120,width-4,33),(i+1).ToString()))Select(selected,i);}
            GUI.enabled=true;
            if(LibraryButton(new Rect(623,609,130,39),"Xoay trái")&&model!=null)model.transform.Rotate(0,-30,0);
            if(LibraryButton(new Rect(762,609,130,39),"Xoay phải")&&model!=null)model.transform.Rotate(0,30,0);
            if(LibraryButton(new Rect(903,609,125,39),"Thu nhỏ"))cameraComponent.orthographicSize=Mathf.Min(120,cameraComponent.orthographicSize*1.15f);
            if(LibraryButton(new Rect(1038,609,131,39),"Phóng to"))cameraComponent.orthographicSize=Mathf.Max(.07f,cameraComponent.orthographicSize/1.15f);
            GUI.Label(new Rect(623,661,549,36),"Xem mẫu 3D · Mua và đặt đồ tại Tiệm lẩu → Trang trí",libraryText);
            GUI.matrix=previous;
        }
    }
}
