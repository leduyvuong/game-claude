using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmModelPark
    {
        private GUIStyle titleStyle,textStyle,buttonStyle,smallStyle;
        private Rect guiArea;
        private float guiScale;
        private string search="";

        private void Coordinates()
        {
            Rect safe=Screen.safeArea;guiScale=Mathf.Min(safe.width/1240f,safe.height/760f);
            guiArea=new Rect(safe.x+(safe.width-1240*guiScale)/2,Screen.height-safe.yMax+(safe.height-760*guiScale)/2,1240,760);
        }
        private bool FarmPanelOpen => Game!=null&&(Game.Development.IsOpen||Game.Ranch.IsOpen||Game.Hotpot.IsOpen||Game.IsHarvesting);
        public bool BlocksPointer(Vector2 screenPoint)
        {
            Coordinates();Vector2 p=(new Vector2(screenPoint.x,Screen.height-screenPoint.y)-guiArea.position)/guiScale;
            return !Ready||menu||(!FarmPanelOpen&&(new Rect(12,88,1216,65).Contains(p)||(!farmView&&p.y<84)||selected>=0&&new Rect(245,645,750,105).Contains(p)));
        }
        private static void Fill(Rect r,string hex)
        {Color old=GUI.color;GUI.color=FarmWorld.Hex(hex);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        private bool ParkButton(Rect r,string text)
        {Color old=GUI.backgroundColor;GUI.backgroundColor=FarmWorld.Hex("E8D4A5");bool clicked=GUI.Button(r,text,buttonStyle);GUI.backgroundColor=old;return clicked;}
        private void OnGUI()
        {
            Coordinates();var old=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3(guiArea.x,guiArea.y,0),Quaternion.identity,Vector3.one*guiScale);
            if(titleStyle==null)
            {
                titleStyle=new GUIStyle(GUI.skin.label){fontSize=28,fontStyle=FontStyle.Bold};
                textStyle=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};
                smallStyle=new GUIStyle(textStyle){fontSize=15};
                buttonStyle=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true};
                titleStyle.normal.textColor=textStyle.normal.textColor=smallStyle.normal.textColor=FarmWorld.Hex("F5EAD1");
                buttonStyle.normal.textColor=FarmWorld.Hex("294F42");
                buttonStyle.normal.background=buttonStyle.hover.background=buttonStyle.active.background=Texture2D.whiteTexture;
                buttonStyle.hover.textColor=buttonStyle.active.textColor=FarmWorld.Hex("294F42");
            }
            if(!Ready)
            {
                Fill(new Rect(320,280,600,150),"274E44");GUI.Label(new Rect(345,300,550,48),"ĐANG MỞ CÔNG VIÊN MÙA VÀNG",titleStyle);
                GUI.Label(new Rect(345,355,550,65),Error??("Đã đặt "+Exhibits.Count+" / "+ExpectedCount+" biến thể…"),textStyle);
                GUI.matrix=old;return;
            }
            if(FarmPanelOpen){GUI.matrix=old;return;}
            if(!farmView)
            {
                Fill(new Rect(12,12,1216,70),"274E44");
                GUI.Label(new Rect(30,19,680,36),"MÙA VÀNG  /  CÔNG VIÊN MODEL",titleStyle);
                GUI.Label(new Rect(30,54,850,25),Exhibits.Count+" biến thể cùng hiện diện · "+Districts.Count+" khu vườn · Chạm mẫu để xem gần",smallStyle);
                GUI.Label(new Rect(903,32,310,30),"Kéo để di chuyển · Cuộn để zoom",smallStyle);
            }
            Fill(new Rect(12,88,1216,65),"365E50");
            if(ParkButton(new Rect(24,98,158,44),"Toàn bản đồ"))ShowOverview();
            if(ParkButton(new Rect(190,98,160,44),"Về trang trại"))ShowFarm();
            if(ParkButton(new Rect(358,98,210,44),menu?"Đóng danh mục":"25 khu / Tìm model"))menu=!menu;
            if(ParkButton(new Rect(576,98,195,44),"Nạp tiền & kho"))Refill();
            if(ParkButton(new Rect(779,98,64,44),"−"))Game.CameraController.ZoomBy(4);
            if(ParkButton(new Rect(851,98,64,44),"+"))Game.CameraController.ZoomBy(-4);
            GUI.Label(new Rect(931,108,285,30),"MÀN CHƠI THỬ RIÊNG",smallStyle);
            if(selected>=0&&!menu)
            {
                Fill(new Rect(245,645,750,105),"274E44");
                GUI.Label(new Rect(265,655,710,30),Exhibits[selected].name,textStyle);
                if(ParkButton(new Rect(265,696,150,40),"← Mẫu trước"))SelectExhibit((selected+Exhibits.Count-1)%Exhibits.Count);
                if(ParkButton(new Rect(425,696,150,40),"Xoay 45°"))Exhibits[selected].root.transform.Rotate(0,45,0);
                if(ParkButton(new Rect(585,696,150,40),"Mẫu sau →"))SelectExhibit((selected+1)%Exhibits.Count);
                if(ParkButton(new Rect(745,696,225,40),"Về khu này"))ShowDistrict(Districts.FindIndex(d=>d.name==Exhibits[selected].group));
            }
            if(menu)DrawDirectory();
            GUI.matrix=old;
        }
        private void DrawDirectory()
        {
            Fill(new Rect(180,167,880,573),"274E44");
            GUI.Label(new Rect(202,184,400,35),"CHỌN ĐIỂM ĐẾN",titleStyle);
            GUI.Label(new Rect(202,232,100,30),"Tìm mẫu",textStyle);
            search=GUI.TextField(new Rect(307,229,726,38),search,new GUIStyle(GUI.skin.textField){fontSize=20});
            int count=search.Length==0?Districts.Count:Exhibits.FindAll(e=>e.name.IndexOf(search,System.StringComparison.OrdinalIgnoreCase)>=0).Count;
            menuScroll=GUI.BeginScrollView(new Rect(200,285,842,432),menuScroll,new Rect(0,0,812,Mathf.Max(432,Mathf.Ceil(count/2f)*64)));
            int shown=0;
            if(search.Length==0)
            {
                for(int i=0;i<Districts.Count;i++)
                    if(ParkButton(new Rect(i%2*405,i/2*64,393,54),(i+1).ToString("00")+" · "+Districts[i].name+" ("+Districts[i].count+")"))ShowDistrict(i);
            }
            else for(int i=0;i<Exhibits.Count;i++)
            {
                if(Exhibits[i].name.IndexOf(search,System.StringComparison.OrdinalIgnoreCase)<0)continue;
                if(ParkButton(new Rect(shown%2*405,shown/2*64,393,54),Exhibits[i].name))SelectExhibit(i);shown++;
            }
            if(count==0)GUI.Label(new Rect(10,10,700,45),"Không có model trùng tên. Thử tên cây, công trình hoặc món đồ.",textStyle);
            GUI.EndScrollView();
        }
    }
}
