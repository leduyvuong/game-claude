using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed class FarmPlot : MonoBehaviour
    {
        public int Index { get; private set; }
        private FarmWorld world;
        private GameObject drySoil,wetSoil;
        public int RenderedStage => renderedStage;
        public bool IsWateredVisual => wetSoil.activeSelf;
        private LineRenderer selection;
        private GameObject cropVisual;
        private string renderedCrop = "";
        private int renderedStage;
        private int renderedHealth;
        private GameObject puddle;
        private GameObject permanentDrain;
        private GameObject pestVisual;
        public bool HasDrainageVisual => permanentDrain!=null&&permanentDrain.activeSelf;
        private Renderer puddleRenderer;
        private MaterialPropertyBlock puddleProperties;
        private static readonly int RainProperty=Shader.PropertyToID("_Rain");
        public bool HasPuddle => puddle.activeSelf;
        public int HealthVisual => renderedHealth;

        public void Initialize(FarmWorld world, int index)
        {
            this.world = world; Index = index;
            drySoil=CreateSoil("DirtTile");wetSoil=CreateSoil("WetDirtTile");wetSoil.SetActive(false);
            selection = CreateOutline();
            selection.gameObject.SetActive(false);
            puddle=Instantiate(Resources.Load<GameObject>("Effects/Puddles/Puddle"+(index%4)),transform,false);
            puddle.transform.localRotation=Quaternion.Euler(0,(index*137.5f)%360,0);
            puddleRenderer=puddle.GetComponent<Renderer>();
            puddleProperties=new MaterialPropertyBlock();
            puddle.SetActive(false);
        }

        public void Refresh(FarmModel model, double now, bool selected)
        {
            PlotState state = model.State.plots[Index];
            if(state.drainageBuilt&&permanentDrain==null)
            {
                permanentDrain=Instantiate(Resources.Load<GameObject>("Collection/DrainStraight"),transform,false);
                permanentDrain.name="Permanent plot drain";
                permanentDrain.transform.localPosition=new Vector3(.585f,-.07f,0);
                permanentDrain.transform.localRotation=Quaternion.Euler(0,90,0);
                permanentDrain.transform.localScale=new Vector3(.58f,.19f,.12f);
                foreach(var collider in permanentDrain.GetComponentsInChildren<Collider>())collider.enabled=false;
            }
            if(permanentDrain!=null)permanentDrain.SetActive(state.drainageBuilt);
            double progress = model.Progress(Index, now);
            int stage = string.IsNullOrEmpty(state.cropId) ? 0 : progress >= 1 ? 5 : 1 + Mathf.Clamp(Mathf.FloorToInt((float)progress * 4), 0, 3);
            if(state.harvests>0&&IslandCropCatalog.Index(state.cropId)>=0)stage=Mathf.Max(stage,3);
            bool wet=state.watered||state.soilWater>.2;
            drySoil.SetActive(!wet);wetSoil.SetActive(wet);
            puddle.SetActive(state.soilWater>=.55);
            float flooding=Mathf.InverseLerp(.55f,1,(float)state.soilWater);
            float extent=Mathf.Lerp(.62f,1,flooding);
            puddle.transform.localScale=new Vector3(extent,1,extent);
            puddle.transform.localPosition=new Vector3(0,Mathf.Lerp(-.030f,-.009f,flooding),0);
            if(puddle.activeSelf)
            {
                puddleProperties.SetFloat(RainProperty,model.RainLevel);
                puddleRenderer.SetPropertyBlock(puddleProperties);
            }
            bool pest=state.pest>0&&!string.IsNullOrEmpty(state.cropId);
            if(pest&&pestVisual==null)
            {
                var prefab=Resources.Load<GameObject>("Collection/Snail");
                if(prefab!=null)
                {
                    pestVisual=Instantiate(prefab,transform,false);
                    pestVisual.name="Pest on plot";
                    pestVisual.transform.localPosition=new Vector3(.36f,.02f,.34f);
                    pestVisual.transform.localRotation=Quaternion.Euler(0,Index*73%360,0);
                    pestVisual.transform.localScale=Vector3.one*.55f;
                    foreach(var collider in pestVisual.GetComponentsInChildren<Collider>())collider.enabled=false;
                }
            }
            if(pestVisual!=null)pestVisual.SetActive(pest);
            selection.gameObject.SetActive(selected);
            if(selected)selection.sharedMaterial=world.Material(stage>0&&progress>=1?world.Gold:FarmWorld.Hex("FFF1C6"));
            int health=stage==0?0:state.disease>=70?2:state.disease>=35?1:0;
            if (state.cropId == renderedCrop && stage == renderedStage&&health==renderedHealth) return;
            renderedCrop = state.cropId; renderedStage = stage;renderedHealth=health;
            if (cropVisual != null){cropVisual.SetActive(false);Destroy(cropVisual);}
            cropVisual=stage>0?BuildCropCluster(state.cropId,stage,health):null;
        }

        // Hay Day-style dense beds: field crops render as a cluster of smaller plants
        // filling the tilled soil (tall crops in two ordered rows, the rest as a 3x3
        // patch); fruit trees keep one large model per plot. Instances get saturated
        // material copies and a gentle breeze sway; baked prefabs stay untouched.
        private static bool IsTreeCrop(string id) { return FarmModel.IsPerennial(id); }
        private static bool IsRowCrop(string id)
        {return id=="corn"||id=="sugarcane"||id=="rice"||id=="wheat"||id=="lemongrass";}

        private GameObject BuildCropCluster(string cropId,int stage,int health)
        {
            var cluster=new GameObject("Crop cluster");
            cluster.transform.SetParent(transform,false);
            bool tree=IsTreeCrop(cropId)||IslandCropCatalog.Index(cropId)>=0,rowed=IsRowCrop(cropId);
            int columns=tree?1:rowed?5:4,rows=tree?1:4;
            float dx=rowed?.18f:.24f,dz=.24f;
            float stageScale=stage==5?1:stage==4?.9f:stage==3?.75f:stage==2?.5f:.3f;
            float clusterScale=(stage==5?.52f:stage==4?.50f:.48f)*(rowed?.9f:1);
            for(int gx=0;gx<columns;gx++)for(int gz=0;gz<rows;gz++)
            {
                GameObject item=null;
                if(health>0)
                {
                    string name=CropCatalog.Find(cropId).modelName;
                    string xid=IslandCropCatalog.DesignId(cropId);
                    var prefab=Resources.Load<GameObject>(xid!=null?"IslandExpansion/Crops/"+xid+(health==2?"_Wilted":"_Diseased"):"Crops/"+name+"/"+name+(health==2?"_Wilted":"_Diseased"));
                    if(prefab!=null){item=Instantiate(prefab,cluster.transform,false);item.transform.localPosition=Vector3.up*.008f;item.transform.localScale=Vector3.one*stageScale;if(xid!=null)FarmWorld.FitIslandCrop(item);}
                }
                if(item==null)item=world.Crop(cluster.transform,cropId,stage);
                if(item==null)continue;
                world.Vivify(item);
                if(tree)continue;
                int seed=Index*31+gx*7+gz*13;
                float jitter=rowed?.012f:.022f;
                float jx=Mathf.Sin(seed*12.9898f)*jitter,jz=Mathf.Sin(seed*78.233f)*jitter;
                Vector3 local=item.transform.localPosition;
                item.transform.localPosition=new Vector3((gx-(columns-1)*.5f)*dx+jx,local.y,(gz-(rows-1)*.5f)*dz+jz);
                item.transform.localRotation=Quaternion.Euler(0,rowed?seed*23%21-10:seed*47%360,0);
                item.transform.localScale*=clusterScale*(.9f+seed*29%19/100f);
            }
            cluster.AddComponent<FarmCropSway>().Initialize(Index,tree?.45f:1);
            return cluster;
        }
        private GameObject CreateSoil(string name)
        {
            var soil=Instantiate(Resources.Load<GameObject>("Environment/"+name),transform,false);
            soil.name=name;soil.transform.localScale=new Vector3(.54f,.55f,.54f);
            foreach(var collider in soil.GetComponentsInChildren<Collider>())collider.enabled=false;
            return soil;
        }
        private LineRenderer CreateOutline()
        {
            const int sideSteps=32;
            var line=world.Ring(transform,Vector3.zero,1,FarmWorld.Hex("FFF1C6"),.035f,sideSteps*4);
            line.name="Selected soil border";
            // Follow the same rounded-square perimeter as the baked tilled soil.
            for(int side=0;side<4;side++)for(int i=0;i<sideSteps;i++)
            {
                float t=-1+2f*i/sideSteps;
                float u=side==0?t:side==1?1:side==2?-t:-1;
                float v=side==0?-1:side==1?t:side==2?1:-t;
                float x=u*(.82f+.18f*Mathf.Sqrt(Mathf.Max(0,1-v*v)));
                float z=v*(.82f+.18f*Mathf.Sqrt(Mathf.Max(0,1-u*u)));
                line.SetPosition(side*sideSteps+i,new Vector3(x*.558f,.045f,z*.558f));
            }
            return line;
        }
    }

    /// <summary>Gentle breeze tilt for a whole crop bed; purely cosmetic.</summary>
    public sealed class FarmCropSway : MonoBehaviour
    {
        private float phase, strength;
        public void Initialize(int seed, float strength) { phase = seed * .73f; this.strength = strength; }
        private void Update()
        {
            float t = Time.time;
            transform.localRotation = Quaternion.Euler(
                Mathf.Sin(t * .8f + phase) * 1.1f * strength, 0,
                Mathf.Sin(t * .6f + phase * 1.7f) * 1.4f * strength);
        }
    }
}
