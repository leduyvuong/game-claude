using UnityEngine;

namespace HarvestIsle
{
    public enum FarmAnimalKind { Chicken, Duck, Cow, Pig, Cat, Sheep, Buffalo, Deer, Sparrow, Koi, Carp, Perch, Bulbul }
    public enum FarmAnimalPose { Idle, Walk, Feed, Special }

    /// <summary>Transform animation for the original animal prefabs. All mesh/material assets remain shared.</summary>
    public sealed class FarmAnimalAvatar : MonoBehaviour
    {
        [SerializeField] private FarmAnimalKind kind;
        [SerializeField] private Transform motion;
        [SerializeField] private Transform body;
        [SerializeField] private Transform head;
        [SerializeField] private Transform tail;
        [SerializeField] private Transform[] legs;
        [SerializeField] private Transform[] wings;
        [SerializeField] private Transform[] eyes;
        [SerializeField] private Transform[] contacts;
        private Vector3 bodyRest, headRest, motionRest;
        private Quaternion bodyRotation, headRotation, tailRotation;
        private Quaternion[] legRotations, wingRotations;
        private Vector3[] eyeScales;
        private bool cached, desiredWalking, desiredFlying;
        private float walkWeight, flightWeight, feedStarted=-100;
        public FarmAnimalKind Kind { get { return kind; } }
        public bool IsAquatic { get { return kind==FarmAnimalKind.Koi||kind==FarmAnimalKind.Carp||kind==FarmAnimalKind.Perch; } }
        public bool IsBird { get { return kind==FarmAnimalKind.Chicken||kind==FarmAnimalKind.Duck||IsSongBird; } }
        public bool IsSongBird { get { return kind==FarmAnimalKind.Sparrow||kind==FarmAnimalKind.Bulbul; } }
        public bool GroundedPose { get; private set; }

        public void Configure(FarmAnimalKind kind, Transform motion, Transform body, Transform head, Transform tail,
            Transform[] legs, Transform[] wings, Transform[] eyes, Transform[] contacts)
        {
            this.kind=kind;this.motion=motion;this.body=body;this.head=head;this.tail=tail;
            this.legs=legs;this.wings=wings;this.eyes=eyes;this.contacts=contacts;
            CacheRest();SetPose(0,0,0,0);
        }
        private void Awake(){CacheRest();}
        private void CacheRest()
        {
            if(motion==null||body==null||head==null)return;
            motionRest=motion.localPosition;bodyRest=body.localPosition;headRest=head.localPosition;
            bodyRotation=body.localRotation;headRotation=head.localRotation;tailRotation=tail!=null?tail.localRotation:Quaternion.identity;
            legRotations=new Quaternion[legs.Length];for(int i=0;i<legs.Length;i++)legRotations[i]=legs[i].localRotation;
            wingRotations=new Quaternion[wings.Length];for(int i=0;i<wings.Length;i++)wingRotations[i]=wings[i].localRotation;
            eyeScales=new Vector3[eyes.Length];for(int i=0;i<eyes.Length;i++)eyeScales[i]=eyes[i].localScale;
            cached=true;
        }
        public void SetWalking(bool walking){desiredWalking=walking;}
        public void SetFlying(bool flying){desiredFlying=flying&&IsSongBird;}
        public void Feed(){feedStarted=Time.time;}
        private void Update()
        {
            walkWeight=Mathf.MoveTowards(walkWeight,desiredWalking?1:0,Time.deltaTime*5);
            flightWeight=Mathf.MoveTowards(flightWeight,desiredFlying?1:0,Time.deltaTime*4);
            float elapsed=Time.time-feedStarted;
            float feeding=elapsed>=0&&elapsed<1.8f?Mathf.Sin(elapsed/1.8f*Mathf.PI):0;
            SetPose(Time.time,walkWeight,feeding,flightWeight);
        }
        public void PreviewPose(FarmAnimalPose pose,float time)
        {
            SetPose(time,pose==FarmAnimalPose.Walk?1:0,pose==FarmAnimalPose.Feed?1:0,pose==FarmAnimalPose.Special?1:0);
        }
        public void SetPose(float time,float walking,float feeding,float special)
        {
            if(!cached)CacheRest();if(!cached)return;
            GroundedPose=!IsAquatic&&!(IsSongBird&&special>0.001f);
            if(IsAquatic){SetFishPose(time,walking,feeding,special);return;}
            bool bird=IsBird;
            float stride=Mathf.Sin(time*(bird?10:7))*walking;
            motion.localPosition=motionRest;
            body.localPosition=bodyRest+Vector3.up*(Mathf.Sin(time*2.2f)*0.004f*(1-walking)+Mathf.Abs(stride)*0.009f);
            body.localRotation=bodyRotation*Quaternion.Euler(feeding*(bird?8:2),0,-stride*(bird?3:1.5f));
            bool grazing=kind==FarmAnimalKind.Cow||kind==FarmAnimalKind.Buffalo||kind==FarmAnimalKind.Deer||kind==FarmAnimalKind.Sheep;
            float nod=grazing?55:kind==FarmAnimalKind.Pig?60:bird?62:38;
            float drop=kind==FarmAnimalKind.Cow||kind==FarmAnimalKind.Buffalo?0.18f:IsSongBird?0.075f:bird?0.155f:kind==FarmAnimalKind.Cat?0.09f:grazing?0.15f:0.05f;
            head.localPosition=headRest+Vector3.down*(feeding*drop);
            float scan=Mathf.Sin(time*1.3f)*(1-walking)*(1-feeding)*(special>0?24:5);
            head.localRotation=headRotation*Quaternion.Euler(feeding*nod-stride*3,scan,special*(bird?0:8));
            for(int i=0;i<legs.Length;i++)
            {
                float sign=legs.Length==2?(i==0?1:-1):(i==0||i==3?1:-1);
                legs[i].localRotation=legRotations[i]*Quaternion.Euler(stride*sign*(bird?24:20)+(IsSongBird?special*30:0),0,0);
            }
            for(int i=0;i<wings.Length;i++)
                wings[i].localRotation=wingRotations[i]*Quaternion.Euler(0,0,(i==0?-1:1)*(special*(48+Mathf.Sin(time*12)*20)+Mathf.Abs(stride)*4));
            if(tail!=null)tail.localRotation=tailRotation*Quaternion.Euler(0,Mathf.Sin(time*(kind==FarmAnimalKind.Cat?2.4f:3.2f))*(kind==FarmAnimalKind.Cat?12:8)*(1+special),0);
            float blink=1;float cycle=Mathf.Repeat(time,4.7f);
            if(cycle>4.48f)blink=Mathf.Max(0.10f,Mathf.Abs(cycle-4.59f)/0.11f);
            for(int i=0;i<eyes.Length;i++)eyes[i].localScale=Vector3.Scale(eyeScales[i],new Vector3(1,blink,1));
            // Toe and heel markers keep the stance foot on the ground in walking previews and gameplay.
            float lowest=float.PositiveInfinity;
            foreach(Transform point in contacts)lowest=Mathf.Min(lowest,transform.InverseTransformPoint(point.position).y);
            if(!float.IsInfinity(lowest))motion.localPosition-=Vector3.up*lowest;
            if(IsSongBird)motion.localPosition+=Vector3.up*(special*0.16f);
        }
        private void SetFishPose(float time,float swimming,float feeding,float turning)
        {
            float strength=Mathf.Lerp(0.35f,1,swimming),wave=Mathf.Sin(time*6);
            motion.localPosition=motionRest;
            body.localPosition=bodyRest+Vector3.up*(Mathf.Sin(time*2.1f)*0.008f);
            body.localRotation=bodyRotation*Quaternion.Euler(-feeding*8,turning*24+wave*3*strength,turning*14+Mathf.Sin(time*3)*2);
            head.localPosition=headRest;head.localRotation=headRotation*Quaternion.Euler(-feeding*18,turning*-8,0);
            if(tail!=null)tail.localRotation=tailRotation*Quaternion.Euler(0,wave*26*strength+turning*12,0);
            for(int i=0;i<wings.Length;i++)wings[i].localRotation=wingRotations[i]*Quaternion.Euler(Mathf.Sin(time*5+i)*8,0,(i==0?-1:1)*(8+Mathf.Sin(time*6)*12*strength));
            for(int i=0;i<eyes.Length;i++)eyes[i].localScale=eyeScales[i];
        }
    }
}
