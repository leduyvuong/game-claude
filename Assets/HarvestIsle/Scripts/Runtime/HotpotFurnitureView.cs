using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Baked reusable decoration. Origin is floor centre, wall contact, or ceiling attachment.</summary>
    public sealed class HotpotFurnitureView : MonoBehaviour
    {
        public int kind;
        public Vector3 footprint;
        public Transform[] seats;
        public Transform serving;
    }
}
