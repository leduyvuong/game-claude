using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Cosmetic swimming inside a reserved ellipse within a pond or lake.</summary>
    public sealed class FarmFishSwim : MonoBehaviour
    {
        private FarmAnimalAvatar avatar;
        private Vector3 center;
        private float radiusX,radiusZ,speed,phase,nextFeed;
        public void Configure(Vector3 center,float radiusX,float radiusZ,float speed,int seed)
        {
            this.center=center;this.radiusX=radiusX;this.radiusZ=radiusZ;this.speed=speed;
            phase=seed*0.37f;avatar=GetComponent<FarmAnimalAvatar>();nextFeed=Time.time+8+seed%5;
            Sample(Time.time);
        }
        private void Sample(float time)
        {
            float angle=phase+time*speed;
            transform.localPosition=center+new Vector3(Mathf.Cos(angle)*radiusX,0,Mathf.Sin(angle)*radiusZ);
            transform.localRotation=Quaternion.LookRotation(new Vector3(-Mathf.Sin(angle)*radiusX,0,Mathf.Cos(angle)*radiusZ));
        }
        private void Update()
        {
            if(avatar==null)return;Sample(Time.time);avatar.SetWalking(true);
            if(Time.time>=nextFeed){avatar.Feed();nextFeed=Time.time+13;}
        }
    }
}
