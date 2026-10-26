using UnityEngine;

namespace HarvestIsle
{
    public sealed class FarmClimate:MonoBehaviour
    {
        private FarmGame game;private ParticleSystem rain;private int level=-1;
        public void Configure(FarmGame owner)
        {
            game=owner;var root=new GameObject("Farm rainfall");root.transform.SetParent(transform,false);root.transform.position=Vector3.up*13;
            rain=root.AddComponent<ParticleSystem>();rain.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=rain.main;main.loop=true;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=1800;main.startLifetime=1.1f;main.startSpeed=0;main.startSize=.04f;
            var shape=rain.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=owner.IsEstate?new Vector3(76,1,66):new Vector3(38,1,32);
            var velocity=rain.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.y=-14;velocity.x=-1.5f;
            var emission=rain.emission;emission.rateOverTime=0;
            var collision=rain.collision;collision.enabled=true;collision.type=ParticleSystemCollisionType.World;collision.collidesWith=1;collision.lifetimeLoss=1;collision.quality=ParticleSystemCollisionQuality.Low;
            var renderer=rain.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("Weather/Rain");renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=2;renderer.velocityScale=.025f;
            rain.Play();
        }
        private void Update()
        {
            if(game==null||game.Model==null)return;int next=game.Model.RainLevel;if(next==level)return;level=next;
            var emission=rain.emission;emission.rateOverTime=level==0?0:level==1?750:1400;
            if(RenderSettings.sun!=null){RenderSettings.sun.intensity=level==0?1.1f:level==1?.72f:.5f;RenderSettings.sun.color=level==0?FarmWorld.Hex("FFF0CB"):FarmWorld.Hex("CBDCEB");}
        }
    }
}
