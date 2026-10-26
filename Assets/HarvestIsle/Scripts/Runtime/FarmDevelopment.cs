using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmDevelopment:MonoBehaviour
    {
        private FarmGame game;
        private int tab;
        private string buildingFilter="";
        private int confirmChop=-1;
        public void OpenBuilding(string id){Open(id=="nursery"||id=="greenhouse"?6:2);buildingFilter=id;}
        private readonly Dictionary<string,GameObject> models=new Dictionary<string,GameObject>();
        private readonly Dictionary<string,int> levels=new Dictionary<string,int>();
        private GUIStyle heading,body,button,small;
        private float nextSync;
        public bool IsOpen {get;private set;}
        public int VisibleBuildings => models.Count;
        public void Configure(FarmGame owner)
        {
            game=owner;SyncBuildings();
        }
        public void Open(int page)
        {
            if(game.IsHarvesting||game.Ranch.IsOpen||(game.Hotpot!=null&&game.Hotpot.IsOpen))return;
            buildingFilter="";tab=Mathf.Clamp(page,0,8);ResetPages();IsOpen=true;game.Hud.gameObject.SetActive(false);
        }
        public void Close(){confirmChop=-1;IsOpen=false;game.Hud.gameObject.SetActive(true);game.Hud.Refresh();}
        public void Perform(Func<ActionResult> command)
        {
            if(game.Online!=null)throw new InvalidOperationException("Online actions require a server command");
            var result=command();game.Notify(result.message,!result.success);
            if(result.success){game.Save();SyncBuildings();}
        }
        private void Execute(string kind,Func<ActionResult> local,string id="",int job=0)
        {
            if(game.Online!=null)game.Online.Command(kind,id,game.SelectedPlot,job:job);
            else Perform(local);
        }
        // Land, workers and the hotpot shop have no server commands yet.
        private void Local(Func<ActionResult> command)
        {
            if(game.Online!=null){game.Notify("Tính năng này chưa hỗ trợ chế độ online.",true);return;}
            Perform(command);
        }
        private void Update(){if(game!=null&&Time.unscaledTime>=nextSync){nextSync=Time.unscaledTime+.5f;SyncBuildings();}}
        public static bool InClearing(Vector3 p){return Mathf.Abs(p.x)<8&&p.z< -5.1f&&p.z> -14f;}
        private void SyncBuildings()
        {
            for(int i=0;i<DevelopmentCatalog.Buildings.Length;i++)
            {
                var spec=DevelopmentCatalog.Buildings[i];int level=game.Model.BuildingLevel(spec.id);
                if(level==0||levels.TryGetValue(spec.id,out int shown)&&shown==level)continue;
                if(models.TryGetValue(spec.id,out var old)){old.SetActive(false);Destroy(old);}
                var prefab=Resources.Load<GameObject>("Structures/Levels/"+spec.model+"_L"+level);
                if(prefab==null)prefab=Resources.Load<GameObject>("Collection/Levels/"+spec.model+"_L"+level);
                if(prefab==null){Debug.LogError("Missing building prefab: "+spec.model);continue;}
                var model=Instantiate(prefab,game.World.transform,false);
                float x=(i%5-2)*2.8f,z=-6.6f-(i/5)*3;
                model.transform.localPosition=game.IsEstate?FarmEstate.BuildingPosition(i):new Vector3(x,IslandTerrainProfile.Height(x/2,z/2),z);
                model.transform.localScale=Vector3.one*(game.IsEstate?1.5f:.6f);
                if(game.IsEstate)model.transform.localRotation=Quaternion.Euler(0,180,0);
                foreach(var c in model.GetComponentsInChildren<Collider>())c.enabled=false;
                model.layer=FarmWorld.InteractionLayer;
                var target=model.AddComponent<FarmDevelopmentTarget>();target.BuildingId=spec.id;target.Page=spec.id=="nursery"||spec.id=="greenhouse"?6:2;
                var hit=model.AddComponent<BoxCollider>();hit.center=new Vector3(0,1,0);hit.size=new Vector3(3,2,3);
                models[spec.id]=model;levels[spec.id]=level;
            }
        }
        private bool ActionButton(Rect rect,string text,bool enabled,Action action)
        {
            bool oldEnabled=GUI.enabled;Color oldBackground=GUI.backgroundColor;
            GUI.backgroundColor=FarmWorld.Hex("F3E6C8");
            GUI.enabled=enabled&&oldEnabled;bool clicked=GUI.Button(rect,text,button);GUI.enabled=oldEnabled;GUI.backgroundColor=oldBackground;
            if(clicked)action();return clicked;
        }
        private void OnGUI()
        {
            if(!IsOpen)return;
            Rect frame=FarmWindow.Frame(1120,740);
            if(FarmWindow.DismissRequested(frame)){Close();return;}
            var previousColor=GUI.color;var previousContent=GUI.contentColor;var previousBackground=GUI.backgroundColor;bool previousEnabled=GUI.enabled;
            GUI.color=GUI.contentColor=GUI.backgroundColor=Color.white;GUI.enabled=true;
            FarmWindow.Draw(frame);
            var old=GUI.matrix;GUI.matrix=FarmWindow.Matrix(frame,1120);
            PrepareStyles();
            DrawHeader();
            Paint(new Rect(28,172,1064,464),"FFF8E8");
            GUI.BeginGroup(new Rect(28,172,1064,464));
            switch(tab)
            {
                case 0: DrawOrdersPage();break;
                case 1: DrawBuildingPage();break;
                case 2: DrawProductionPage();break;
                case 3: DrawJournalPage();break;
                case 4: DrawInventoryPage();break;
                case 5: DrawSeedsPage();break;
                case 6: DrawAgronomyPage();break;
                case 7: DrawWorkersPage();break;
                case 8: DrawJourneyPage();break;
            }
            GUI.EndGroup();
            DrawFooter();
            GUI.matrix=old;GUI.color=previousColor;GUI.contentColor=previousContent;GUI.backgroundColor=previousBackground;GUI.enabled=previousEnabled;
        }
    }
    public sealed class FarmDevelopmentTarget:MonoBehaviour { public int Page; public string BuildingId; }
}
