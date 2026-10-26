using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Eight visual levels. The owning gameplay system decides which level is unlocked.</summary>
    public sealed class HotpotKitchenLevelView : MonoBehaviour
    {
        [SerializeField,Range(1,8)] private int initialLevel=1;
        private GameObject visual;
        public int Level {get;private set;}
        public HotpotKitchenItem Item {get;private set;}
        private void Start(){if(visual==null)SetLevel(initialLevel);}
        public bool SetLevel(int level)
        {
            if(level<1||level>8)return false;
            if(visual!=null&&Level==level)return true;
            var prefab=Resources.Load<GameObject>(HotpotKitchenCatalog.Items[47+level].Resource);if(prefab==null)return false;
            var next=Instantiate(prefab,transform,false);
            foreach(var c in next.GetComponentsInChildren<Collider>())c.enabled=false;
            if(visual!=null){visual.SetActive(false);if(Application.isPlaying)Destroy(visual);else DestroyImmediate(visual);}
            visual=next;Item=next.GetComponent<HotpotKitchenItem>();Level=level;initialLevel=level;return true;
        }
    }
}
