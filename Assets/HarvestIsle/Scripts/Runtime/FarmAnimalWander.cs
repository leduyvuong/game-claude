using UnityEngine;

namespace HarvestIsle
{
    /// <summary>Cosmetic wandering inside a small reserved patch; no economy or livestock production.</summary>
    public sealed class FarmAnimalWander : MonoBehaviour
    {
        private FarmAnimalAvatar avatar;
        private Vector3 center,target;
        private float radius,speed,nextChoice;
        private uint randomState;
        private bool moving;
        public void Configure(Vector3 center,float radius,float speed,int seed)
        {
            this.center=center;this.radius=radius;this.speed=speed;randomState=(uint)Mathf.Max(1,seed);
            avatar=GetComponent<FarmAnimalAvatar>();target=center;nextChoice=Time.time+1+Next()*3;
        }
        private float Next(){randomState=randomState*1664525u+1013904223u;return (randomState&0x00ffffff)/16777216f;}
        private void Update()
        {
            if(avatar==null)return;
            if(Time.time>=nextChoice)
            {
                float angle=Next()*Mathf.PI*2;
                target=center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(radius*Mathf.Sqrt(Next()));
                nextChoice=Time.time+3+Next()*4;moving=true;
            }
            Vector3 delta=target-transform.localPosition;delta.y=0;
            if(moving&&delta.sqrMagnitude>0.002f)
            {
                transform.localPosition=Vector3.MoveTowards(transform.localPosition,target,Time.deltaTime*speed);
                transform.localRotation=Quaternion.Slerp(transform.localRotation,Quaternion.LookRotation(delta),Time.deltaTime*3);
                avatar.SetWalking(true);
                avatar.SetFlying(avatar.IsSongBird);
            }
            else
            {
                if(moving){moving=false;avatar.Feed();}
                avatar.SetWalking(false);
                avatar.SetFlying(false);
            }
        }
    }
}
