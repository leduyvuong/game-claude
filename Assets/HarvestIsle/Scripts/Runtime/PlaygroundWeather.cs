using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle
{
    public enum FarmWeather { Clear,Wind,Rain,Storm,Snow }
    /// <summary>Visual/audio weather demonstration; does not modify farming production or save data.</summary>
    public sealed class PlaygroundWeather : MonoBehaviour
    {
        [SerializeField] private FarmWeather initialWeather=FarmWeather.Rain;
        public static readonly string[] Labels={"Nắng","Gió","Mưa","Bão","Tuyết"};
        public FarmWeather Mode {get;private set;}
        public float WindStrength {get;private set;}
        public bool Suspended {get;private set;}
        public bool SoundEnabled {get;private set;}=true;
        public int LightningCount {get;private set;}
        public int RainParticles {get{return rain==null?0:rain.particleCount;}}
        public int SplashParticles {get{return splashes==null?0:splashes.particleCount;}}
        public int GustParticles {get{return leaves==null?0:leaves.particleCount;}}
        public int SnowParticles {get{return snow==null?0:snow.particleCount;}}
        public int CloudParticles {get{return clouds==null?0:clouds.particleCount;}}
        public int SwayingTrees {get{return sway.Count;}}
        public bool ThunderPlaying {get{return thunderAudio!=null&&thunderAudio.isPlaying;}}
        public float TreeSwayAngle {get{float max=0;foreach(var tree in sway)if(tree!=null)max=Mathf.Max(max,Mathf.Abs(tree.CurrentAngle));return max;}}
        public float LowestRainHeight
        {
            get{if(rain==null||rain.particleCount==0)return 100;var particles=new ParticleSystem.Particle[rain.particleCount];int count=rain.GetParticles(particles);float min=100;for(int i=0;i<count;i++)min=Mathf.Min(min,particles[i].position.y);return min;}
        }
        private ParticleSystem rain,splashes,leaves,snow,clouds;
        private GameObject effects;
        private LineRenderer lightning;
        private Light sun,fill;
        private float sunRest,fillRest,flashUntil,nextLightning,thunderAt=-1,splashDebt;
        private Color sunColorRest,ambientRest;
        private AmbientMode ambientModeRest;
        private bool fogRest;
        private Color fogColorRest;
        private FogMode fogModeRest;
        private float fogStartRest,fogEndRest;
        private ModelLibraryViewer library;
        private AudioSource rainAudio,windAudio,thunderAudio;
        private readonly List<AudioClip> clips=new List<AudioClip>();
        private readonly List<WeatherTreeSway> sway=new List<WeatherTreeSway>();
        private readonly Dictionary<Renderer,Material[]> originalMaterials=new Dictionary<Renderer,Material[]>();
        private readonly Dictionary<Material,Material> wetMaterials=new Dictionary<Material,Material>();
        private Vector3 center;
        private readonly System.Random random=new System.Random(42071);
        private bool initialized;
        private void Start()
        {
            library=FindFirstObjectByType<ModelLibraryViewer>();sun=RenderSettings.sun;
            foreach(Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.type==LightType.Directional&&light!=sun){fill=light;break;}
            if(sun!=null){sunRest=sun.intensity;sunColorRest=sun.color;}if(fill!=null)fillRest=fill.intensity;
            ambientRest=RenderSettings.ambientLight;ambientModeRest=RenderSettings.ambientMode;fogRest=RenderSettings.fog;fogColorRest=RenderSettings.fogColor;fogModeRest=RenderSettings.fogMode;fogStartRest=RenderSettings.fogStartDistance;fogEndRest=RenderSettings.fogEndDistance;
            effects=new GameObject("Weather particles and lightning");effects.transform.SetParent(transform,false);
            rain=Particles("Falling rain","Rain",ParticleSystemRenderMode.Stretch,2400);
            var rainMain=rain.main;rainMain.startLifetime=1.15f;rainMain.startSize=new ParticleSystem.MinMaxCurve(.032f,.055f);rainMain.startSpeed=0;
            var rainShape=rain.shape;rainShape.enabled=true;rainShape.shapeType=ParticleSystemShapeType.Box;rainShape.scale=new Vector3(22,.4f,18);
            var collision=rain.collision;collision.enabled=true;collision.type=ParticleSystemCollisionType.World;collision.mode=ParticleSystemCollisionMode.Collision3D;collision.collidesWith=1;collision.quality=ParticleSystemCollisionQuality.Low;collision.dampen=1;collision.bounce=0;collision.lifetimeLoss=1;collision.radiusScale=.1f;
            var rainRenderer=rain.GetComponent<ParticleSystemRenderer>();rainRenderer.velocityScale=.024f;rainRenderer.lengthScale=1.6f;
            splashes=Particles("Ground splashes","Splash",ParticleSystemRenderMode.HorizontalBillboard,300);
            var splashMain=splashes.main;splashMain.startLifetime=new ParticleSystem.MinMaxCurve(.28f,.50f);splashMain.startSize=new ParticleSystem.MinMaxCurve(.10f,.22f);splashMain.startSpeed=0;var splashShape=splashes.shape;splashShape.enabled=false;
            var grow=splashes.sizeOverLifetime;grow.enabled=true;grow.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.2f),new Keyframe(1,1.7f)));Fade(splashes);
            leaves=Particles("Windblown leaves","Leaf",ParticleSystemRenderMode.Billboard,150);
            var leafMain=leaves.main;leafMain.startLifetime=new ParticleSystem.MinMaxCurve(1.8f,2.8f);leafMain.startSize=new ParticleSystem.MinMaxCurve(.07f,.15f);leafMain.startColor=new ParticleSystem.MinMaxGradient(FarmWorld.Hex("CAD484"),FarmWorld.Hex("79965D"));
            var leafShape=leaves.shape;leafShape.enabled=true;leafShape.shapeType=ParticleSystemShapeType.Box;leafShape.scale=new Vector3(16,.9f,12);Fade(leaves);
            var leafRotation=leaves.rotationOverLifetime;leafRotation.enabled=true;leafRotation.z=new ParticleSystem.MinMaxCurve(2.0f,5.5f);
            snow=Particles("Snowflakes","Snow",ParticleSystemRenderMode.Billboard,900);
            var snowMain=snow.main;snowMain.startLifetime=new ParticleSystem.MinMaxCurve(3,5);snowMain.startSize=new ParticleSystem.MinMaxCurve(.035f,.085f);snowMain.startSpeed=0;
            var snowShape=snow.shape;snowShape.enabled=true;snowShape.shapeType=ParticleSystemShapeType.Box;snowShape.scale=new Vector3(21,.6f,17);Fade(snow);
            clouds=Particles("Moving mist and cloud wisps","Cloud",ParticleSystemRenderMode.Billboard,70);
            var cloudMain=clouds.main;cloudMain.startLifetime=new ParticleSystem.MinMaxCurve(6,9);cloudMain.startSize=new ParticleSystem.MinMaxCurve(2.0f,4.2f);cloudMain.startSpeed=0;
            var cloudShape=clouds.shape;cloudShape.enabled=true;cloudShape.shapeType=ParticleSystemShapeType.Box;cloudShape.scale=new Vector3(23,1.5f,17);Fade(clouds);
            lightning=new GameObject("Lightning bolt").AddComponent<LineRenderer>();lightning.transform.SetParent(effects.transform,false);lightning.sharedMaterial=Resources.Load<Material>("Weather/Lightning");lightning.useWorldSpace=true;lightning.startWidth=.055f;lightning.endWidth=.020f;lightning.positionCount=7;lightning.enabled=false;lightning.shadowCastingMode=ShadowCastingMode.Off;
            rainAudio=Audio("Rain sound",NoiseClip("Rain",false));windAudio=Audio("Wind sound",NoiseClip("Wind",true));thunderAudio=Audio("Thunder sound",ThunderClip());thunderAudio.loop=false;thunderAudio.Stop();
            foreach(MeshFilter mesh in FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                if(mesh.sharedMesh!=null&&mesh.sharedMesh.name=="Feathered palm crown")
                {Transform tree=mesh.transform.parent;if(tree.GetComponent<WeatherTreeSway>()==null){var motion=tree.gameObject.AddComponent<WeatherTreeSway>();motion.Configure(this,sway.Count*1.7f);sway.Add(motion);}}
                if(mesh.name=="Land and details"||mesh.name=="Water surface")
                {
                    var renderer=mesh.GetComponent<Renderer>();if(renderer==null||originalMaterials.ContainsKey(renderer))continue;originalMaterials.Add(renderer,renderer.sharedMaterials);
                    foreach(Material mat in renderer.sharedMaterials)if(mat!=null&&mat.HasProperty("_BaseColor")&&!wetMaterials.ContainsKey(mat))
                    {var wet=new Material(mat){name=mat.name+" wet weather"};wet.SetColor("_BaseColor",mat.GetColor("_BaseColor")*.79f);if(wet.HasProperty("_Smoothness"))wet.SetFloat("_Smoothness",.32f);wetMaterials.Add(mat,wet);}
                }
            }
            initialized=true;SetWeather(initialWeather);Debug.Log("WEATHER_STARTED: rain, splashes, wind, snow, clouds, lightning and audio; "+sway.Count+" trees.");
        }
        private ParticleSystem Particles(string name,string material,ParticleSystemRenderMode mode,int max)
        {
            var go=new GameObject(name);go.transform.SetParent(effects.transform,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=max;main.startSpeed=0;
            var emission=ps.emission;emission.rateOverTime=0;var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=mode;renderer.sharedMaterial=Resources.Load<Material>("Weather/"+material);renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;ps.Play();return ps;
        }
        private static void Fade(ParticleSystem ps)
        {var color=ps.colorOverLifetime;color.enabled=true;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.75f,.14f),new GradientAlphaKey(.60f,.6f),new GradientAlphaKey(0,1)});color.color=g;}
        private static void Rate(ParticleSystem ps,float count){var emission=ps.emission;emission.rateOverTime=count;}
        private static void Velocity(ParticleSystem ps,Vector3 v)
        {var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=v.x;velocity.y=v.y;velocity.z=v.z;}
        public bool SetWeather(FarmWeather mode)
        {
            if(!initialized||!Enum.IsDefined(typeof(FarmWeather),mode))return false;Mode=mode;WindStrength=mode==FarmWeather.Storm?2.0f:mode==FarmWeather.Wind?1.15f:mode==FarmWeather.Rain?.38f:mode==FarmWeather.Snow?.22f:0;
            foreach(ParticleSystem ps in new[]{rain,splashes,leaves,snow,clouds}){ps.Clear();ps.Play();}
            Rate(rain,mode==FarmWeather.Storm?1450:mode==FarmWeather.Rain?760:0);Velocity(rain,new Vector3(WindStrength*4,-16,-WindStrength*1.6f));
            Rate(leaves,mode==FarmWeather.Storm?30:mode==FarmWeather.Wind?16:0);Velocity(leaves,new Vector3(WindStrength*3.4f,.14f,-WindStrength*1.2f));
            Rate(snow,mode==FarmWeather.Snow?160:0);Velocity(snow,new Vector3(.40f,-2,.08f));
            Rate(clouds,mode==FarmWeather.Clear?0:mode==FarmWeather.Wind?.65f:mode==FarmWeather.Storm?3:1.1f);Velocity(clouds,new Vector3(Mathf.Max(.3f,WindStrength),0,-.16f));
            var cloudMain=clouds.main;cloudMain.startColor=mode==FarmWeather.Storm?new Color(.19f,.25f,.30f,.28f):new Color(.78f,.87f,.91f,.16f);
            bool wet=mode==FarmWeather.Rain||mode==FarmWeather.Storm;
            foreach(var pair in originalMaterials)
            {
                if(pair.Key==null)continue;var slots=(Material[])pair.Value.Clone();
                for(int i=0;i<slots.Length;i++)
                {
                    Material original=slots[i];if(original==null||!wetMaterials.TryGetValue(original,out Material mat))continue;
                    bool water=original.HasProperty("_WaveStrength");
                    if(water&&mode!=FarmWeather.Clear)
                    {
                        mat.SetColor("_BaseColor",original.GetColor("_BaseColor")*(mode==FarmWeather.Storm?.74f:wet?.90f:1));
                        mat.SetFloat("_WaveStrength",mode==FarmWeather.Storm?1:mode==FarmWeather.Wind?.72f:mode==FarmWeather.Rain?.58f:.18f);
                        mat.SetFloat("_Speed",mode==FarmWeather.Storm?1.25f:mode==FarmWeather.Wind?.85f:mode==FarmWeather.Rain?.68f:.25f);slots[i]=mat;
                    }
                    else if(wet&&!water)slots[i]=mat;
                }
                pair.Key.sharedMaterials=slots;
            }
            splashDebt=0;flashUntil=0;lightning.enabled=false;thunderAt=-1;if(mode!=FarmWeather.Storm)thunderAudio.Stop();nextLightning=Time.time+6;UpdateAudio();return true;
        }
        public bool TriggerLightning()
        {
            if(!initialized||Mode!=FarmWeather.Storm||Suspended)return false;LightningCount++;flashUntil=Time.time+.23f;thunderAt=Time.time+.35f;nextLightning=Time.time+8+(float)random.NextDouble()*7;
            Vector3 p=center+new Vector3(4.5f,0,3.0f);var points=new Vector3[7];for(int i=0;i<points.Length;i++)points[i]=p+new Vector3((float)(random.NextDouble()-.5)*.55f,7.3f-i*.95f,(float)(random.NextDouble()-.5)*.26f);
            lightning.SetPositions(points);lightning.enabled=true;return true;
        }
        public void SetSound(bool enabled){SoundEnabled=enabled;UpdateAudio();}
        private void Update()
        {
            if(!initialized)return;bool hidden=library!=null&&library.Visible;
            if(hidden!=Suspended){Suspended=hidden;effects.SetActive(!hidden);if(!hidden)SetWeather(Mode);UpdateAudio();}
            if(hidden){RestoreLighting();return;}
            Camera camera=Camera.main;if(camera==null)return;Ray ray=camera.ViewportPointToRay(new Vector3(.5f,.5f,0));var plane=new Plane(Vector3.up,Vector3.up*.15f);if(plane.Raycast(ray,out float distance))center=ray.GetPoint(distance);
            rain.transform.position=center+Vector3.up*8.5f;snow.transform.position=center+Vector3.up*6.5f;leaves.transform.position=center+new Vector3(-4,1.15f,0);clouds.transform.position=center+Vector3.up*5.5f;
            if(Mode==FarmWeather.Rain||Mode==FarmWeather.Storm)
            {
                splashDebt+=Time.deltaTime*(Mode==FarmWeather.Storm?150:90);
                while(splashDebt>=1)
                {
                    splashDebt--;Vector3 p=center+new Vector3((float)(random.NextDouble()-.5)*17,0,(float)(random.NextDouble()-.5)*12);
                    if(Physics.Raycast(p+Vector3.up*5,Vector3.down,out RaycastHit hit,8,1))splashes.Emit(new ParticleSystem.EmitParams{position=hit.point+Vector3.up*.018f},1);
                }
            }
            if(Mode==FarmWeather.Storm&&Time.time>=nextLightning)TriggerLightning();
            if(lightning.enabled&&Time.time>=flashUntil)lightning.enabled=false;
            if(thunderAt>=0&&Time.time>=thunderAt){thunderAt=-1;if(SoundEnabled)thunderAudio.Play();}
            UpdateLighting(camera);
        }
        private void UpdateLighting(Camera camera)
        {
            float brightness=Mode==FarmWeather.Storm?.49f:Mode==FarmWeather.Rain?.72f:Mode==FarmWeather.Snow?.85f:1;
            float flash=Time.time<flashUntil?(flashUntil-Time.time)/.23f:0;
            if(sun!=null){sun.intensity=sunRest*brightness+flash*1.4f;sun.color=Mode==FarmWeather.Clear||Mode==FarmWeather.Wind?sunColorRest:new Color(.76f,.84f,.94f);}
            if(fill!=null)fill.intensity=fillRest*(.70f+brightness*.30f);
            RenderSettings.ambientLight=ambientRest*(.65f+brightness*.35f);RenderSettings.fog=Mode==FarmWeather.Rain||Mode==FarmWeather.Storm||Mode==FarmWeather.Snow;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=Mode==FarmWeather.Snow?FarmWorld.Hex("BACDD4"):Mode==FarmWeather.Storm?FarmWorld.Hex("506D7B"):FarmWorld.Hex("83A7AF");RenderSettings.fogStartDistance=38;RenderSettings.fogEndDistance=Mode==FarmWeather.Storm?85:120;
            camera.backgroundColor=Mode==FarmWeather.Clear||Mode==FarmWeather.Wind?FarmWorld.Hex("62B7B6"):RenderSettings.fogColor;
        }
        private void RestoreLighting()
        {
            if(sun!=null){sun.intensity=sunRest;sun.color=sunColorRest;}if(fill!=null)fill.intensity=fillRest;RenderSettings.ambientLight=ambientRest;RenderSettings.ambientMode=ambientModeRest;RenderSettings.fog=fogRest;RenderSettings.fogColor=fogColorRest;RenderSettings.fogMode=fogModeRest;RenderSettings.fogStartDistance=fogStartRest;RenderSettings.fogEndDistance=fogEndRest;
        }
        private AudioSource Audio(string name,AudioClip clip)
        {var source=new GameObject(name).AddComponent<AudioSource>();source.transform.SetParent(transform,false);source.clip=clip;source.loop=true;source.spatialBlend=0;source.playOnAwake=false;source.volume=0;source.Play();return source;}
        private AudioClip NoiseClip(string name,bool wind)
        {
            const int rate=24000;var samples=new float[rate*3];float low=0,slow=0;var rng=new System.Random(wind?136:918);
            for(int i=0;i<samples.Length;i++){float noise=(float)(rng.NextDouble()*2-1);low=Mathf.Lerp(low,noise,wind?.025f:.35f);slow=Mathf.Lerp(slow,noise,.006f);float phase=(float)i/rate;samples[i]=wind?(low*.7f+slow*.7f)*(.7f+.25f*Mathf.Sin(phase*2*Mathf.PI/3)):noise*.11f+low*.15f;}
            var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);clips.Add(clip);return clip;
        }
        private AudioClip ThunderClip()
        {
            const int rate=24000;var samples=new float[(int)(rate*2.8f)];float low=0;var rng=new System.Random(2026);
            for(int i=0;i<samples.Length;i++){float t=(float)i/rate;low=Mathf.Lerp(low,(float)(rng.NextDouble()*2-1),.025f);float envelope=Mathf.Min(1,t*35)*Mathf.Exp(-t*1.7f);samples[i]=(low*.65f+Mathf.Sin(t*2*Mathf.PI*43)*.09f+Mathf.Sin(t*2*Mathf.PI*61)*.07f)*envelope;}
            var clip=AudioClip.Create("Thunder",samples.Length,1,rate,false);clip.SetData(samples,0);clips.Add(clip);return clip;
        }
        private void UpdateAudio()
        {
            if(rainAudio==null)return;float audible=SoundEnabled&&!Suspended?1:0;rainAudio.volume=audible*(Mode==FarmWeather.Storm?.48f:Mode==FarmWeather.Rain?.30f:0);windAudio.volume=audible*WindStrength*.30f;thunderAudio.volume=audible*.65f;if(audible==0)thunderAudio.Stop();
        }
        private void OnGUI()
        {
            if(!initialized||(library!=null&&library.Visible))return;float x=Mathf.Max(380,Screen.width-328);
            GUI.Box(new Rect(x,18,310,142),"THỜI TIẾT · "+Labels[(int)Mode]);
            for(int i=0;i<Labels.Length;i++){GUI.enabled=(int)Mode!=i;if(GUI.Button(new Rect(x+10+i*59,54,55,31),Labels[i]))SetWeather((FarmWeather)i);}GUI.enabled=true;
            if(GUI.Button(new Rect(x+10,98,137,31),SoundEnabled?"Tắt tiếng thời tiết":"Bật tiếng thời tiết"))SetSound(!SoundEnabled);
            GUI.enabled=Mode==FarmWeather.Storm;if(GUI.Button(new Rect(x+160,98,137,31),"Sét thử"))TriggerLightning();GUI.enabled=true;
        }
        private void OnDestroy()
        {
            if(initialized)RestoreLighting();foreach(var pair in originalMaterials)if(pair.Key!=null)pair.Key.sharedMaterials=pair.Value;
            foreach(var mat in wetMaterials.Values)if(mat!=null)Destroy(mat);foreach(var clip in clips)if(clip!=null)Destroy(clip);foreach(var motion in sway)if(motion!=null)Destroy(motion);
        }
    }
}
