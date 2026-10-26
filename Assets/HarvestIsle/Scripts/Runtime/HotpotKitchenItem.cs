using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Reusable visual item; no inventory, pricing, recipe unlocks or production logic.</summary>
    public sealed class HotpotKitchenItem : MonoBehaviour
    {
        public string catalogId,ingredientKey;
        public HotpotKitchenRole role;
        public int level;
        public Vector3 size;
        public Transform contents;
        public Transform[] sockets;
        public void ShowContents(bool show){if(contents!=null)contents.gameObject.SetActive(show);}
    }
}
