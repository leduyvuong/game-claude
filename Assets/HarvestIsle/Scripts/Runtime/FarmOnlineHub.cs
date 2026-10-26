using System;
using System.Collections;
using System.Text;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace HarvestIsle
{
    public sealed class FarmOnlineHub:MonoBehaviour
    {
        [Serializable] public sealed class Member {public string id,name;}
        [Serializable] public sealed class Reply
        {
            public bool success,replayed;
            public string message,token,userId,name,islandCode;
            public double serverTime;
            public FarmState farm;
            public Member[] members;
        }
        [Serializable] private sealed class Credentials {public string name,password;}
        [Serializable] private sealed class JoinRequest {public string code;}
        [Serializable] private sealed class CommandRequest
        {
            public string requestId,kind,id,target;
            public int index,tool,job;
            public bool useCorn;
        }
        private static FarmOnlineHub instance;
        public static bool IsPanelOpen => instance!=null&&instance.panelOpen;
        public bool Busy {get;private set;}
        public FarmGame OnlineGame {get;private set;}
        public Reply LastReply {get;private set;}
        public double ServerNow => serverTime+Math.Max(0,Time.unscaledTime-syncedAt);
        private const string Address="http://127.0.0.1:8787";
        private FarmGame localGame;
        private Light localSun;
        private bool panelOpen;
        private string nameInput="",passwordInput="",joinCode="",token="",userName="",islandCode="",message="";
        private Member[] members=new Member[0];
        private FarmState visited;
        private string visitedId="",visitedName="";
        private int visitedPlot;
        private CommandRequest pending;
        private double serverTime;
        private float syncedAt,nextPoll;
        private Vector2 memberScroll;
        public static FarmOnlineHub Open(FarmGame game)
        {
            if(instance==null){instance=new GameObject("Harvest Isle online connection").AddComponent<FarmOnlineHub>();instance.localGame=game;}
            instance.panelOpen=true;game.Hud.gameObject.SetActive(false);
            if(instance.OnlineGame!=null&&!instance.Busy)instance.LoadMembers();
            return instance;
        }
        public void ClosePanel()
        {
            panelOpen=false;var active=OnlineGame!=null?OnlineGame:localGame;
            if(active!=null)active.Hud.gameObject.SetActive(true);
        }
        public void Authenticate(string name,string password,bool register)
        {
            if(Busy)return;
            StartCoroutine(Send(register?"/api/register":"/api/login",JsonUtility.ToJson(new Credentials{name=name,password=password}),reply=>{
                if(!reply.success)return;
                token=reply.token;userName=reply.name;islandCode=reply.islandCode;serverTime=reply.serverTime;syncedAt=Time.unscaledTime;
                passwordInput="";pending=null;
                if(OnlineGame==null)
                {
                    localGame.Save();localSun=RenderSettings.sun;localGame.gameObject.SetActive(false);
                    OnlineGame=new GameObject("Online farm").AddComponent<FarmGame>();
                    OnlineGame.InitializeSession(null,()=>ServerNow,reply.farm,this);
                }
                else OnlineGame.ApplyServerState(reply.farm);
                OnlineGame.Hud.gameObject.SetActive(!panelOpen);nextPoll=Time.unscaledTime+4;LoadMembers();
            }));
        }
        private void ApplyOwn(Reply reply,bool notify=true)
        {
            if(notify)OnlineGame?.Notify(reply.message??"Đã đồng bộ.",!reply.success);
            if(reply.farm==null||OnlineGame==null)return;
            serverTime=reply.serverTime;syncedAt=Time.unscaledTime;
            if(!string.IsNullOrEmpty(reply.islandCode))islandCode=reply.islandCode;
            OnlineGame.ApplyServerState(reply.farm);
        }
        public void Command(string kind,string id="",int index=0,int tool=0,int job=0,bool useCorn=false,string target="")
        {
            if(Busy){OnlineGame?.Notify("Đang đồng bộ, chờ thao tác trước hoàn tất.");return;}
            if(pending!=null){OnlineGame?.Notify("Lệnh trước chưa xác nhận. Mở Online local để thử lại.",true);return;}
            pending=new CommandRequest{requestId=Guid.NewGuid().ToString(),kind=kind,id=id,index=index,tool=tool,job=job,useCorn=useCorn,target=target};
            Retry();
        }
        public void Retry()
        {
            if(Busy||pending==null)return;
            bool helping=pending.kind=="help";
            StartCoroutine(Send("/api/command",JsonUtility.ToJson(pending),reply=>{ApplyOwn(reply);if(helping&&reply.success&&!string.IsNullOrEmpty(visitedId))Visit(visitedId);},true));
        }
        private IEnumerator Send(string path,string json,Action<Reply> completed,bool isCommand=false)
        {
            Busy=true;
            using(var request=new UnityWebRequest(Address+path,json==null?"GET":"POST"))
            {
                request.downloadHandler=new DownloadHandlerBuffer();request.timeout=10;
                if(json!=null){request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));request.SetRequestHeader("Content-Type","application/json");}
                if(!string.IsNullOrEmpty(token))request.SetRequestHeader("Authorization","Bearer "+token);
                yield return request.SendWebRequest();
                Reply reply=null;
                if(request.result!=UnityWebRequest.Result.ConnectionError&&request.result!=UnityWebRequest.Result.DataProcessingError)
                {
                    try{reply=JsonUtility.FromJson<Reply>(request.downloadHandler.text);}catch(ArgumentException){}
                    if(isCommand&&request.responseCode<500)pending=null;
                }
                if(reply==null)reply=new Reply{success=false,message="Không kết nối được máy chủ local. Kiểm tra máy chủ; lệnh chưa xác nhận có thể thử lại."};
                message=reply.message??"Đã tải dữ liệu đảo.";LastReply=reply;Busy=false;
                completed?.Invoke(reply);
            }
        }
        private void Update()
        {
            if(OnlineGame!=null&&!Busy&&pending==null&&Time.unscaledTime>=nextPoll)
            {
                nextPoll=Time.unscaledTime+4;StartCoroutine(Send("/api/state",null,r=>ApplyOwn(r,false)));
            }
        }
        private void LoadMembers(){StartCoroutine(Send("/api/island",null,r=>{if(r.members!=null)members=r.members;}));}
        private void Visit(string id)
        {
            if(Busy)return;
            StartCoroutine(Send("/api/visit/"+id,null,r=>{if(r.success){visited=r.farm;visitedId=r.userId;visitedName=r.name;visitedPlot=0;}}));
        }
        public void ReturnToSolo()
        {
            if(Busy)return;
            if(!string.IsNullOrEmpty(token)){StartCoroutine(Send("/api/logout","{}",r=>RestoreSolo()));return;}
            RestoreSolo();
        }
        private void RestoreSolo()
        {
            if(OnlineGame!=null){OnlineGame.gameObject.SetActive(false);Destroy(OnlineGame.gameObject);OnlineGame=null;}
            token="";pending=null;visited=null;members=new Member[0];panelOpen=false;
            if(localGame!=null){RenderSettings.sun=localSun!=null?localSun:RenderSettings.sun;localGame.gameObject.SetActive(true);localGame.Hud.gameObject.SetActive(true);}
        }
        private void OnDestroy(){if(instance==this)instance=null;}
        private void OnGUI()
        {
            if(!panelOpen)return;
            Rect frame=FarmWindow.Frame(1120,760);
            if(FarmWindow.DismissRequested(frame)){ClosePanel();return;}
            FarmWindow.Draw(frame);var old=GUI.matrix;GUI.matrix=FarmWindow.Matrix(frame,1120);
            var text=new GUIStyle(GUI.skin.label){fontSize=20,wordWrap=true};text.normal.textColor=new Color(.16f,.30f,.27f);
            var title=new GUIStyle(text){fontSize=28,fontStyle=FontStyle.Bold};
            var button=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true};var input=new GUIStyle(GUI.skin.textField){fontSize=21};
            button.normal.background=Texture2D.whiteTexture;button.hover.background=Texture2D.whiteTexture;button.active.background=Texture2D.whiteTexture;
            button.normal.textColor=button.hover.textColor=button.active.textColor=text.normal.textColor;
            var background=GUI.backgroundColor;GUI.backgroundColor=new Color(.85f,.90f,.79f);
            GUI.Label(new Rect(28,22,800,48),"ĐẢO CHUNG · MÁY CHỦ LOCAL",title);
            GUI.Label(new Rect(28,74,1000,44),OnlineGame==null?"Phiên online có nông trại riêng, giữ nguyên bản lưu solo.":userName+" · Mã mời đảo: "+islandCode,text);
            GUI.enabled=!Busy;
            if(OnlineGame==null)
            {
                GUI.Label(new Rect(28,151,250,40),"Tên tài khoản",text);nameInput=GUI.TextField(new Rect(280,150,500,45),nameInput,24,input);
                GUI.Label(new Rect(28,213,250,40),"Mật khẩu",text);passwordInput=GUI.PasswordField(new Rect(280,212,500,45),passwordInput,'•',128,input);
                if(GUI.Button(new Rect(280,289,240,56),"Đăng nhập",button))Authenticate(nameInput,passwordInput,false);
                if(GUI.Button(new Rect(540,289,240,56),"Tạo tài khoản",button))Authenticate(nameInput,passwordInput,true);
                GUI.Label(new Rect(28,380,1020,45),"Tên 3–24 chữ/số/gạch dưới · Mật khẩu ít nhất 8 ký tự",text);
                GUI.Label(new Rect(28,450,1020,90),"Bản local chạy trong Unity trên máy này. Mỗi tài khoản bắt đầu với "+FarmModel.Money(120)+" và 24 ô; tiền, kho và thời gian do máy chủ kiểm tra.",text);
            }
            else
            {
                joinCode=GUI.TextField(new Rect(28,136,320,45),joinCode,12,input);
                if(GUI.Button(new Rect(360,136,180,45),"Vào đảo bằng mã",button))StartCoroutine(Send("/api/join",JsonUtility.ToJson(new JoinRequest{code=joinCode}),r=>{ApplyOwn(r);if(r.success){visited=null;LoadMembers();}}));
                if(GUI.Button(new Rect(555,136,200,45),"Tải danh sách bạn",button))LoadMembers();
                memberScroll=GUI.BeginScrollView(new Rect(28,207,350,342),memberScroll,new Rect(0,0,325,Mathf.Max(330,members.Length*52)));
                for(int i=0;i<members.Length;i++){var member=members[i];if(GUI.Button(new Rect(0,i*52,315,44),"Thăm "+member.name,button))Visit(member.id);}
                GUI.EndScrollView();
                if(visited!=null)
                {
                    GUI.Label(new Rect(400,207,680,42),"Nông trại "+visitedName+" · Chọn ô để giúp",text);
                    for(int i=0;i<24;i++)
                    {
                        var plot=visited.plots[i];var crop=CropCatalog.Find(plot.cropId);
                        GUI.backgroundColor=i==visitedPlot?new Color(1,.76f,.32f):new Color(.85f,.90f,.79f);
                        if(GUI.Button(new Rect(400+i%6*110,258+i/6*56,102,50),(i+1)+": "+(crop==null?"Trống":crop.displayName),button))visitedPlot=i;
                    }
                    GUI.backgroundColor=new Color(.85f,.90f,.79f);
                    GUI.Label(new Rect(400,488,680,35),"Ô "+(visitedPlot+1)+" · Nước "+Math.Round(visited.plots[visitedPlot].soilWater*100)+"%",text);
                    if(GUI.Button(new Rect(400,532,260,50),"Tưới giúp",button))Command("help","",visitedPlot,1,0,false,visitedId);
                    if(GUI.Button(new Rect(680,532,280,50),"Thoát nước giúp",button))Command("help","",visitedPlot,4,0,false,visitedId);
                }
                if(GUI.Button(new Rect(28,584,320,52),"Trở lại nông trại solo",button))ReturnToSolo();
            }
            if(pending!=null&&GUI.Button(new Rect(800,136,285,45),"Thử lại lệnh chưa xác nhận",button))Retry();
            if(GUI.Button(new Rect(884,22,208,52),"Về nông trại",button))ClosePanel();
            GUI.enabled=true;
            GUI.Label(new Rect(28,661,1060,78),Busy?"Đang liên lạc với máy chủ…":message,text);GUI.matrix=old;GUI.backgroundColor=background;
        }
    }
}
