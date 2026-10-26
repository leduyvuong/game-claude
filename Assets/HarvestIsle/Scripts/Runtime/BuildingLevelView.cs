using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Visual building levels. Does not spend coins or change farming production.</summary>
    public sealed class BuildingLevelView : MonoBehaviour
    {
        public static readonly string[] Types={"FarmHouse","RiceBarn","ChickenCoop","DuckShelter","CowShed","PigPen","Greenhouse","Nursery","VillageWell"};
        public static readonly string[] Labels={"Nhà chính","Kho lúa","Chuồng gà","Chuồng vịt","Chuồng bò","Chuồng heo","Nhà kính","Vườn ươm","Giếng"};
        [SerializeField] private string buildingType;
        [SerializeField,Range(1,3)] private int initialLevel=2;
        [SerializeField] private GameObject visual;
        public int Level { get; private set; }
        public string DisplayName { get { int i=System.Array.IndexOf(Types,buildingType);return i>=0?Labels[i]:buildingType; } }
        public Bounds WorldBounds { get; private set; }
        public void Configure(string type,int level,GameObject current)
        {buildingType=type;initialLevel=Mathf.Clamp(level,1,3);visual=current;Level=initialLevel;RefreshBounds();}
        private void Start(){SetLevel(initialLevel);}
        public bool SetLevel(int level)
        {
            if(level<1||level>3)return false;
            var prefab=Resources.Load<GameObject>("Structures/Levels/"+buildingType+"_L"+level);
            if(prefab==null){Debug.LogError("Missing building level: "+buildingType+" L"+level,this);return false;}
            var replacement=Instantiate(prefab,transform);replacement.name="Visual - L"+level;
            replacement.transform.localPosition=Vector3.zero;replacement.transform.localRotation=Quaternion.identity;replacement.transform.localScale=Vector3.one;
            if(visual!=null){visual.SetActive(false);if(Application.isPlaying)Destroy(visual);else DestroyImmediate(visual);}
            visual=replacement;Level=level;RefreshBounds();return true;
        }
        private void RefreshBounds()
        {
            if(visual==null)return;var renderers=visual.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return;
            Bounds bounds=renderers[0].bounds;foreach(Renderer renderer in renderers)bounds.Encapsulate(renderer.bounds);WorldBounds=bounds;
        }
    }
}
