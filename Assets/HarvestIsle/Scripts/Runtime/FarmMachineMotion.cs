using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Decorative motion only; production recipes are handled separately.</summary>
    public sealed class FarmMachineMotion : MonoBehaviour
    {
        [SerializeField] private Transform rotor;
        [SerializeField] private Vector3 axis=Vector3.forward;
        [SerializeField] private float speed=20;
        public float DegreesMoved {get;private set;}
        public void Configure(Transform target,Vector3 rotationAxis,float degreesPerSecond)
        {rotor=target;axis=rotationAxis;speed=degreesPerSecond;}
        private void Update(){if(rotor!=null){float angle=speed*Time.deltaTime;rotor.Rotate(axis,angle,Space.Self);DegreesMoved+=Mathf.Abs(angle);}}
    }
}
