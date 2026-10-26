using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HarvestIsle
{
    public sealed class PlaygroundDirector : MonoBehaviour
    {
        [SerializeField] private Transform residents;
        private readonly List<PlaygroundRoam> roamers=new List<PlaygroundRoam>();
        private readonly List<Transform> actors=new List<Transform>();
        private readonly List<Vector3> startingPositions=new List<Vector3>();
        private readonly List<Vector3> previousPositions=new List<Vector3>();
        private readonly List<float> travel=new List<float>();
        private float started;
        private bool reported,paused;
        private BuildingLevelView[] buildings;
        private int selectedBuilding;
        private ProductionDisplay[] productionModels;
        private bool productionView;
        private ModelLibraryViewer library;
        public List<PlaygroundRoam> Roamers { get { return roamers; } }
        private void Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;Time.timeScale=1;started=Time.time;
            buildings=FindObjectsByType<BuildingLevelView>(FindObjectsSortMode.None);
            productionModels=FindObjectsByType<ProductionDisplay>(FindObjectsSortMode.None);
            library=FindFirstObjectByType<ModelLibraryViewer>();
            int index=0;
            foreach(Transform actor in residents)
            {
                var animal=actor.GetComponent<FarmAnimalAvatar>();var farmer=actor.GetComponent<FarmerAvatar>();
                if(animal==null&&farmer==null)continue;
                actors.Add(actor);
                if(animal!=null&&animal.IsAquatic)
                {
                    bool koi=animal.Kind==FarmAnimalKind.Koi;
                    Vector3 center=koi?new Vector3(-6,0.188f,-0.8f):new Vector3(6,0.162f,-0.4f);
                    actor.gameObject.AddComponent<FarmFishSwim>().Configure(center,koi?0.64f:0.77f,koi?0.39f:0.47f,0.54f+index*0.014f,20+index*7);
                }
                else
                {
                    float radius=farmer!=null?0.28f:animal.IsSongBird?0.13f:animal.Kind==FarmAnimalKind.Chicken||animal.Kind==FarmAnimalKind.Duck?0.24f:0.47f;
                    var roam=actor.gameObject.AddComponent<PlaygroundRoam>();roamers.Add(roam);
                    roam.Configure(this,radius,farmer!=null?1.05f:animal.IsSongBird?1.0f:0.75f+index%4*0.11f,101+index*43);
                }
                startingPositions.Add(actor.position);previousPositions.Add(actor.position);travel.Add(0);index++;
            }
            Debug.Log("PLAYGROUND_STARTED: "+actors.Count+" actors; "+roamers.Count+" roamers and "+(actors.Count-roamers.Count)+" swimmers.");
        }
        public bool ValidGround(Vector3 position,float radius)
        {
            if(position.x*position.x/(4.25f*4.25f)+position.z*position.z/(3.7f*3.7f)>1)return false;
            if(InsideEllipse(position,new Vector3(-6,0,-0.8f),1.60f+radius,1.18f+radius))return false;
            if(InsideEllipse(position,new Vector3(6,0,-0.4f),1.85f+radius,1.33f+radius))return false;
            if(buildings!=null)foreach(BuildingLevelView building in buildings)
            {
                Bounds b=building.WorldBounds;if(b.size.sqrMagnitude<=0.001f)continue;
                if(position.x>b.min.x-radius&&position.x<b.max.x+radius&&position.z>b.min.z-radius&&position.z<b.max.z+radius)return false;
            }
            return true;
        }
        private static bool InsideEllipse(Vector3 p,Vector3 c,float rx,float rz)
        {float x=(p.x-c.x)/rx,z=(p.z-c.z)/rz;return x*x+z*z<1;}
        private void Update()
        {
            for(int i=0;i<actors.Count;i++){travel[i]+=Vector3.Distance(previousPositions[i],actors[i].position);previousPositions[i]=actors[i].position;}
            if(Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)TogglePause();
            if(!reported&&Time.time-started>=9)
            {
                reported=true;int moved=0;var entries=new List<ActorStatus>();
                for(int i=0;i<actors.Count;i++)
                {
                    float distance=travel[i];if(distance>0.05f)moved++;
                    entries.Add(new ActorStatus{name=actors[i].name,distanceMoved=distance,position=actors[i].position});
                }
                string folder=Path.Combine(Path.GetDirectoryName(Application.dataPath),".tools","playground");Directory.CreateDirectory(folder);
                int movingMachines=0;foreach(FarmMachineMotion machine in FindObjectsByType<FarmMachineMotion>(FindObjectsSortMode.None))if(machine.DegreesMoved>1)movingMachines++;
                File.WriteAllText(Path.Combine(folder,"runtime-status.json"),JsonUtility.ToJson(new RuntimeStatus{isPlaying=Application.isPlaying,actors=actors.Count,moved=moved,roamers=roamers.Count,swimmers=actors.Count-roamers.Count,productionModels=productionModels.Length,movingMachines=movingMachines,entries=entries.ToArray()},true));
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Live.png"));
                Debug.Log("PLAYGROUND_RUNNING: "+moved+"/"+actors.Count+" actors changed position; live screenshot requested.");
                if(productionModels.Length>0)Debug.Log("PRODUCTION_RUNNING: "+productionModels.Length+" models; "+movingMachines+" machines moving.");
            }
        }
        private void TogglePause(){paused=!paused;Time.timeScale=paused?0:1;}
        public bool ShowProduction(bool visible)
        {
            if(productionModels==null||productionModels.Length==0||Camera.main==null)return false;
            var camera=Camera.main.GetComponent<PlaygroundCamera>();if(camera==null)return false;
            productionView=visible;camera.FocusOn(visible?productionModels[0].transform.parent.position:Vector3.zero);return true;
        }
        private void OnGUI()
        {
            if(library!=null&&library.Visible)return;
            var box=new GUIStyle(GUI.skin.box){fontSize=18,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(16,16,8,8)};
            GUI.Box(new Rect(18,18,450,70),"MÙA VÀNG · 14 NHÂN VẬT\nKéo chuột để nhìn · Cuộn để zoom",box);
            if(GUI.Button(new Rect(18,98,145,34),paused?"Chạy tiếp (Space)":"Tạm dừng (Space)"))TogglePause();
            if(!productionView&&buildings!=null&&buildings.Length>0)
            {
                BuildingLevelView building=buildings[selectedBuilding];
                GUI.Box(new Rect(18,146,340,120),"CẤP CÔNG TRÌNH");
                if(GUI.Button(new Rect(30,176,35,30),"<"))selectedBuilding=(selectedBuilding+buildings.Length-1)%buildings.Length;
                GUI.Label(new Rect(73,179,227,30),building.DisplayName+" · Cấp "+building.Level);
                if(GUI.Button(new Rect(309,176,35,30),">"))selectedBuilding=(selectedBuilding+1)%buildings.Length;
                for(int level=1;level<=3;level++)
                {
                    GUI.enabled=building.Level!=level;
                    if(GUI.Button(new Rect(30+(level-1)*106,220,100,32),"Cấp "+level))building.SetLevel(level);
                }
                GUI.enabled=true;
            }
            if(productionModels!=null&&productionModels.Length>0)
            {
                if(GUI.Button(new Rect(18,280,340,34),productionView?"Về nông trại":"Xem khu chế biến và giao thương"))
                {
                    ShowProduction(!productionView);
                }
                if(productionView)
                {
                    GUI.Box(new Rect(18,146,340,96),"KHU CHẾ BIẾN · 16 MẪU");
                    GUI.Label(new Rect(32,180,310,50),"Cuộn chuột để xem gần\nMáy móc có chuyển động khi chạy");
                    var labelStyle=new GUIStyle(GUI.skin.box){fontSize=12,alignment=TextAnchor.MiddleCenter};
                    foreach(ProductionDisplay model in productionModels)
                    {
                        Vector3 screen=Camera.main.WorldToScreenPoint(model.transform.position);
                        if(screen.z>0)GUI.Box(new Rect(screen.x-83,Screen.height-screen.y+9,166,25),model.DisplayName,labelStyle);
                    }
                }
            }
        }
        private void OnDestroy(){Time.timeScale=1;}
        [Serializable]private sealed class ActorStatus{public string name;public float distanceMoved;public Vector3 position;}
        [Serializable]private sealed class RuntimeStatus{public bool isPlaying;public int actors,moved,roamers,swimmers,productionModels,movingMachines;public ActorStatus[] entries;}
    }
}
