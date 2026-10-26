using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed class FarmHarvestGame:MonoBehaviour
    {
        private FarmGame game;private int plot;private string crop;private double planted;
        private float start,chargeStart;private bool charging;
        public HarvestChallenge Challenge {get;private set;}
        public bool Active {get;private set;}
        public void Configure(FarmGame owner){game=owner;}
        public void Begin(int index)
        {
            if(Active)return;plot=index;var state=game.Model.State.plots[index];crop=state.cropId;planted=state.plantedAt;
            Challenge=new HarvestChallenge(crop,game.Model.State.harvested,index+game.Model.State.harvested*31);
            start=Time.unscaledTime;charging=false;Active=true;game.Hud.gameObject.SetActive(false);
        }
        public void Complete()
        {
            if(!Active||!Challenge.Done)return;
            var state=game.Model.State.plots[plot];Active=false;game.Hud.gameObject.SetActive(true);
            if(state.cropId==crop&&state.plantedAt==planted)game.FinishHarvest(plot,Challenge.Score);
        }
        public void Cancel(){if(!Active)return;Active=false;game.Hud.gameObject.SetActive(true);game.Notify("Đã dừng. Cây vẫn ở trên ruộng.");}
        private void Update(){if(Active&&Challenge.Done)Complete();}
        private void OnApplicationPause(bool paused){if(paused)Cancel();}
        private void OnApplicationFocus(bool focused){if(!focused)Cancel();}
        private void OnGUI()
        {
            if(!Active)return;
            Color previousColor=GUI.color,previousBackground=GUI.backgroundColor;
            GUI.color=new Color(.15f,.18f,.12f,.55f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1000,area.height/700);var old=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3(area.x+(area.width-1000*scale)/2,Screen.height-area.yMax+(area.height-700*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            var title=new GUIStyle(GUI.skin.label){fontSize=28,alignment=TextAnchor.MiddleCenter};var body=new GUIStyle(title){fontSize=20};var button=new GUIStyle(GUI.skin.button){fontSize=24};
            title.normal.textColor=body.normal.textColor=button.normal.textColor=button.hover.textColor=button.active.textColor=FarmWorld.Hex("5A4327");
            button.normal.background=button.hover.background=button.active.background=Texture2D.whiteTexture;
            GUI.backgroundColor=FarmWorld.Hex("ECD8A9");GUI.color=FarmWorld.Hex("FFF6DF");GUI.DrawTexture(new Rect(100,85,800,530),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(130,108,740,50),crop=="corn"?"HÁI NGÔ · NHỚ THỨ TỰ":crop=="pumpkin"?"CẮT BÍ · CANH LỰC":"THU "+CropCatalog.Find(crop).displayName.ToUpperInvariant()+" · CANH NHỊP",title);
            GUI.Label(new Rect(130,160,740,40),"Lượt "+Mathf.Min(3,Challenge.Round+1)+"/3 · Đúng "+Challenge.Score+" · Độ khó tăng theo số vụ",body);
            float elapsed=Time.unscaledTime-start;
            if(crop=="corn")
            {
                bool memorize=elapsed<4/(float)Challenge.Speed;
                GUI.Label(new Rect(130,225,740,55),memorize?"Ghi nhớ: "+(Challenge.Sequence[0]+1)+" → "+(Challenge.Sequence[1]+1)+" → "+(Challenge.Sequence[2]+1):"Chạm ba bắp theo đúng thứ tự vừa xem",body);
                GUI.enabled=!memorize;
                for(int i=0;i<3;i++)if(GUI.Button(new Rect(230+i*185,310,165,95),"Bắp "+(i+1),button))Challenge.Choose(i);
                GUI.enabled=true;
            }
            else
            {
                float value=crop!="pumpkin"?Mathf.PingPong(elapsed*(float)Challenge.Speed*.65f,1):charging?Mathf.Clamp01((Time.unscaledTime-chargeStart)*(float)Challenge.Speed*.42f):0;
                GUI.Label(new Rect(130,215,740,55),crop!="pumpkin"?"Chạm THU khi vạch trắng nằm trong vùng xanh":"Bấm LẤY ĐÀ, rồi CẮT khi lực vào vùng xanh",body);
                GUI.color=new Color(.25f,.3f,.3f);GUI.DrawTexture(new Rect(230,290,540,42),Texture2D.whiteTexture);
                GUI.color=new Color(.35f,.8f,.4f);GUI.DrawTexture(new Rect(230+540*(.5f-(float)Challenge.Window/2),290,540*(float)Challenge.Window,42),Texture2D.whiteTexture);
                GUI.color=Color.white;GUI.DrawTexture(new Rect(230+value*534,284,6,54),Texture2D.whiteTexture);
                if(GUI.Button(new Rect(340,365,320,65),crop!="pumpkin"?"THU":charging?"CẮT":"LẤY ĐÀ",button))
                {if(crop=="pumpkin"&&!charging){charging=true;chargeStart=Time.unscaledTime;}else{Challenge.Timing(value);charging=false;}}
            }
            GUI.Label(new Rect(130,458,740,50),"0 đúng: −1 · 1 đúng: chuẩn · 2 đúng: +1 · 3 đúng: +2\nBệnh và úng giảm sản lượng; tối thiểu vẫn thu 1.",body);
            if(GUI.Button(new Rect(350,540,300,48),"Dừng, giữ cây trên ruộng",button))Cancel();
            GUI.matrix=old;GUI.color=previousColor;GUI.backgroundColor=previousBackground;
        }
    }
}
