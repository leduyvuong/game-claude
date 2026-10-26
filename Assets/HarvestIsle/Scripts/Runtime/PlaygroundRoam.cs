using UnityEngine;

namespace HarvestIsle
{
    public sealed class PlaygroundRoam : MonoBehaviour
    {
        private PlaygroundDirector director;
        private FarmAnimalAvatar animal;
        private FarmerAvatar farmer;
        private Vector3 target;
        private float speed,nextChoice;
        private uint randomState;
        public float Radius { get; private set; }
        public bool Moving { get; private set; }
        public void Configure(PlaygroundDirector director,float radius,float speed,int seed)
        {
            this.director=director;Radius=radius;this.speed=speed;randomState=(uint)Mathf.Max(1,seed);
            animal=GetComponent<FarmAnimalAvatar>();farmer=GetComponent<FarmerAvatar>();
            ChooseTarget();nextChoice=Time.time+3+Next()*4;
        }
        private float Next(){randomState=randomState*1664525u+1013904223u;return (randomState&0x00ffffff)/16777216f;}
        private void ChooseTarget()
        {
            for(int attempt=0;attempt<30;attempt++)
            {
                float a=Next()*Mathf.PI*2,r=Mathf.Sqrt(Next());
                Vector3 candidate=new Vector3(Mathf.Cos(a)*3.8f*r,0.15f,Mathf.Sin(a)*3.1f*r);
                if(director.ValidGround(candidate,Radius)){target=candidate;return;}
            }
            target=new Vector3(0,0.15f,0);
        }
        private void Update()
        {
            if(director==null)return;
            Vector3 delta=target-transform.position;delta.y=0;
            if(Time.time>=nextChoice||delta.sqrMagnitude<0.04f){ChooseTarget();nextChoice=Time.time+4+Next()*5;delta=target-transform.position;delta.y=0;}
            Vector3 direction=delta.normalized;
            foreach(PlaygroundRoam other in director.Roamers)
            {
                if(other==null||other==this)continue;
                Vector3 away=transform.position-other.transform.position;away.y=0;float distance=away.magnitude,space=Radius+other.Radius+0.20f;
                if(distance<space&&distance>0.001f)direction+=away/distance*((space-distance)/space)*2.2f;
            }
            Vector3 step=direction.normalized*speed*Time.deltaTime;
            Vector3 next=transform.position+step;
            if(!director.ValidGround(next,Radius))
            {
                Vector3 sideways=Vector3.Cross(Vector3.up,direction).normalized*speed*Time.deltaTime;
                next=transform.position+sideways;
                if(!director.ValidGround(next,Radius)){ChooseTarget();SetMoving(false);return;}
            }
            next.y=0.15f;
            if(Physics.Raycast(new Vector3(next.x,4,next.z),Vector3.down,out RaycastHit hit,8,1<<0,QueryTriggerInteraction.Ignore))next.y=hit.point.y;
            Vector3 movement=next-transform.position;movement.y=0;
            Moving=movement.sqrMagnitude>0.000001f;
            if(Moving){transform.position=next;transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(movement),Time.deltaTime*5);}
            SetMoving(Moving);
        }
        private void SetMoving(bool moving)
        {
            Moving=moving;if(animal!=null){animal.SetWalking(moving);animal.SetFlying(moving&&animal.IsSongBird);}
            if(farmer!=null)farmer.SetWalking(moving);
        }
    }
}
