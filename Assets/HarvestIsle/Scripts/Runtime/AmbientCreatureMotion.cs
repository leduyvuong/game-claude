using UnityEngine;
namespace HarvestIsle
{
    public sealed class AmbientCreatureMotion : MonoBehaviour
    {
        [SerializeField] private Transform[] wings;
        [SerializeField] private bool flying;
        private Vector3 rest;
        private Quaternion[] wingRest;
        private bool cached;
        public void Configure(Transform[] parts,bool flies){wings=parts;flying=flies;}
        private void Start(){CacheRest();}
        private void CacheRest()
        {if(cached)return;rest=transform.localPosition;wingRest=new Quaternion[wings==null?0:wings.Length];for(int i=0;i<wingRest.Length;i++)wingRest[i]=wings[i].localRotation;cached=true;}
        private void Update()
        {PreviewMotion(Time.time);}
        public void PreviewMotion(float time)
        {
            CacheRest();
            if(wings!=null)for(int i=0;i<wings.Length;i++)if(wings[i]!=null)wings[i].localRotation=wingRest[i]*Quaternion.Euler(0,0,Mathf.Sin(time*18)*(i%2==0?-1:1)*32);
            if(flying)transform.localPosition=rest+Vector3.up*Mathf.Sin(time*2.4f)*.025f;
        }
    }
}
