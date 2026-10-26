using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Selects a baked serving visual without changing the saved recipe or ingredients.</summary>
    public sealed class HotpotDishDisplay : MonoBehaviour
    {
        private GameObject visual;
        public int Dish {get;private set;}=-1;
        public bool SetDish(int dish)
        {
            if(dish<0||dish>=12)return false;if(visual!=null&&Dish==dish)return true;
            var prefab=Resources.Load<GameObject>(HotpotKitchenCatalog.Items[36+dish].Resource);if(prefab==null)return false;
            var next=Instantiate(prefab,transform,false);foreach(var c in next.GetComponentsInChildren<Collider>())c.enabled=false;
            if(visual!=null){visual.SetActive(false);if(Application.isPlaying)Destroy(visual);else DestroyImmediate(visual);}
            visual=next;Dish=dish;return true;
        }
    }
}
