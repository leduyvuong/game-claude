using System;
using System.Collections;
using System.IO;
using HarvestIsle.Core;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace HarvestIsle.Editor
{
    public static class IslandExpansionReview
    {
        public static void Run(bool all=false)
        {
            var live=Object.FindFirstObjectByType<FarmGame>();if(live==null)throw new InvalidOperationException("Open a farm first");
            var runner=new GameObject("Island expansion review runner").AddComponent<FarmEstateReviewRunner>();runner.StartCoroutine(Review(live,runner,all));
        }
        static void Check(bool condition,string reason){if(!condition)throw new InvalidOperationException(reason);}
        static IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.5f);double deadline=EditorApplication.timeSinceStartup+90;
            while(ShaderUtil.anythingCompiling&&EditorApplication.timeSinceStartup<deadline)yield return null;
            Check(!ShaderUtil.anythingCompiling,"Shaders still compiling before screenshot");
            yield return new WaitForSecondsRealtime(.35f);ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/IslandExpansion/Play/"+name+".png"));yield return new WaitForSecondsRealtime(.5f);
        }
        static IEnumerator Review(FarmGame live,FarmEstateReviewRunner runner,bool all)
        {
            string folder=".tools/model-work",result=folder+"/island-"+(all?"all":"sample")+"-result.json";
            Directory.CreateDirectory("Art/Previews/IslandExpansion/Play");File.WriteAllText(result,"{\"status\":\"running\"}");
            string before=JsonUtility.ToJson(live.Model.State);Light sun=RenderSettings.sun;Color ambient=RenderSettings.ambientLight;
            live.gameObject.SetActive(false);var game=new GameObject("Isolated island expansion review").AddComponent<FarmGame>();bool success=false;
            try
            {
                double now=FarmGame.UtcNow();var state=FarmEstate.CreateState(now);state.coins=900000;state.development.experience=400;
                var store=new FarmSaveStore(Path.GetFullPath(folder+"/island-review-"+DateTime.UtcNow.Ticks));
                game.InitializeSession(store,()=>now,state,estate:true);game.enabled=false;var m=game.Model;m.Island.chapter=3;m.Island.completed=8;
                var legacy=FarmModel.NewState(now);legacy.islandExpansion=null;var migrated=new FarmModel(JsonUtility.FromJson<FarmState>(JsonUtility.ToJson(legacy)));Check(migrated.Island.chapter==0&&migrated.State.coins==120&&migrated.State.inventory.Count==60,"Legacy migration");
                var zero=JsonUtility.FromJson<FarmState>(JsonUtility.ToJson(legacy).Replace("\"islandExpansion\":null","\"islandExpansion\":{}"));Check(new FarmModel(zero).Island.nextSerial==1,"Unity zero-filled migration");
                foreach(string id in new[]{"B01","B02","B05"}){Check(m.BuyIslandThing(id).success,"Buy sample");var p=m.Island.owned[m.Island.owned.Count-1];Check(m.PlaceIslandThing(p.serial,-28+(int.Parse(id.Substring(1))-1)*8,-16,2).success,"Place sample "+id);}
                // B05 gets an independent courtyard east of the central axis.
                foreach(var p in m.State.plots){p.cropId="";p.plantedAt=0;p.watered=false;p.soilWater=0;p.harvests=0;p.disease=0;p.pest=0;}
                string[] samples={"peanut","sesame","pandan"};for(int i=0;i<samples.Length;i++)Check(m.Apply(i,FarmTool.Plant,samples[i],now).success,"Sample planting");
                now+=900;m.ObserveTime(now);game.Refresh();game.CameraController.SetView(game.World.PositionOfPlot(1),3.7f);yield return Shot("01-Sample-crops");
                Check(m.Harvest(0,1,now).success&&m.Harvest(1,1,now).success&&m.Harvest(2,1,now).success,"Sample harvest");Check(m.State.plots[2].cropId=="pandan","Leaf regrowth");
                m.State.inventory.Find(e=>e.cropId=="rice").count=3;Check(m.StartIslandRecipe("ix_noodles",now).success,"Noodle start");int serial=m.Island.jobs[0].serial;Check(!m.CollectIslandJob(serial,now).success,"Early collect rejected");
                Check(store.Save(m.State),"Save pending");var restored=new FarmModel(store.Load(now));Check(restored.Island.jobs.Count==1&&restored.Count("rice")==0,"Pending persistence");
                now+=100;Check(m.CollectIslandJob(serial,now).success&&!m.CollectIslandJob(serial,now).success,"Collect exactly once");Check(m.ImportIslandKitchen("ix_noodles",1).success&&m.KitchenCount("riceNoodles")==1,"Kitchen mapping");
                m.BuyIslandThing("D06");var decor=m.Island.owned[m.Island.owned.Count-1];Check(!m.PlaceIslandThing(decor.serial,-28,-16,0).success,"Overlap denied");Check(m.PlaceIslandThing(decor.serial,-23,-20,0).success,"Decor placement");
                Check(m.StoreIslandThing(decor.serial).success&&m.PlaceIslandThing(decor.serial,-23,-20,1).success,"Stored decor reused");
                game.IslandExpansion.RefreshModels();game.IslandExpansion.ShowDistrict();game.CameraController.SetView(FarmIslandExpansion.Origin+new Vector3(-23,0,-16),9);yield return Shot("02-Sample-courtyard");
                game.IslandExpansion.FocusBuilding("B05");yield return Shot("02b-Candy-courtyard");
                game.IslandExpansion.Open(1);string blocked=JsonUtility.ToJson(m.State);game.ActOnPlot(5);Check(blocked==JsonUtility.ToJson(m.State)&&game.IsManaging,"Window input guard");yield return Shot("03-Building-shop");
                game.IslandExpansion.ShowSection(2);yield return Shot("04-Recipes");game.IslandExpansion.ShowSection(4);yield return Shot("05-Owned-decor");
                game.IslandExpansion.BeginPlacement(decor.serial);Check(game.IsManaging,"Placement input guard");yield return Shot("06-Placement-preview");game.IslandExpansion.CancelPlacement();
                if(all)
                {
                    int loaded=0;
                    foreach(var entry in IslandExpansionAssets.CatalogEntries(false))foreach(var v in entry.variants)
                    {
                        var prefab=Resources.Load<GameObject>(v.resource);Check(prefab!=null,"Missing "+v.resource);
                        foreach(var renderer in prefab.GetComponentsInChildren<Renderer>())foreach(var mat in renderer.sharedMaterials)Check(mat!=null&&mat.shader!=null,"Missing material "+v.resource);
                        foreach(var filter in prefab.GetComponentsInChildren<MeshFilter>()){Check(filter.sharedMesh!=null&&filter.sharedMesh.vertexCount>0,"Empty mesh");foreach(var vertex in filter.sharedMesh.vertices)Check(!float.IsNaN(vertex.x)&&!float.IsInfinity(vertex.x),"Nonfinite geometry");}loaded++;
                    }
                    Check(loaded==330,"Expected 330 expansion prefabs");
                    int number=0;foreach(var spec in IslandExpansionCatalog.Things)
                    {
                        if(!spec.Building||m.Island.owned.Exists(p=>p.id==spec.id))continue;m.BuyIslandThing(spec.id);var p=m.Island.owned[m.Island.owned.Count-1];bool placed=false;
                        for(int z=-24;z<=24&&!placed;z+=8)for(int x=-36;x<=36&&!placed;x+=8)if(m.PlaceIslandThing(p.serial,x,z,2).success)placed=true;
                        Check(placed,"Could not arrange building "+spec.id);number++;
                    }
                    game.IslandExpansion.RefreshModels();game.IslandExpansion.ShowDistrict();yield return Shot("07-All-buildings");
                    game.IslandExpansion.Open(3);yield return Shot("08-Decor-shop");game.IslandExpansion.OpenService("B27");yield return Shot("09-Service-panel");
                    Check(number==27,"30 new buildings present");
                    foreach(var spec in IslandExpansionCatalog.Things)
                    {
                        if(spec.Building||m.Island.owned.Exists(p=>p.id==spec.id))continue;
                        Check(m.BuyIslandThing(spec.id).success,"Buy decor "+spec.id);var p=m.Island.owned[m.Island.owned.Count-1];bool placed=false;
                        if(spec.kind=="wall"||spec.kind=="table")
                        {
                            foreach(var b in m.Island.owned)if(!placed&&b.placed&&b.id.StartsWith("B"))for(int slot=0;slot<2&&!placed;slot++)
                            {float x,z;FarmModel.IslandMountPosition(b,slot,out x,out z);placed=m.PlaceIslandThing(p.serial,x,z,b.turn,b.serial).success;}
                        }
                        else for(int z=-26;z<=26&&!placed;z++)for(int x=-38;x<=38&&!placed;x++)placed=m.PlaceIslandThing(p.serial,x,z,0).success;
                        Check(placed,"Could not place decor "+spec.id+" ("+spec.kind+")");
                    }
                    game.IslandExpansion.Close();game.IslandExpansion.RefreshModels();game.IslandExpansion.FocusBuilding("B02");yield return Shot("10-Mounted-decor");
                    game.IslandExpansion.ShowDistrict();yield return Shot("11-All-content");
                    for(int i=0;i<30;i++)
                    {
                        var plot=m.State.plots[i];plot.cropId="";plot.plantedAt=0;plot.harvests=0;plot.watered=false;plot.disease=0;plot.pest=0;plot.soilWater=0;
                        Check(m.Apply(i,FarmTool.Plant,IslandCropCatalog.Ids[i],now).success,"Plant full set "+i);
                    }
                    now+=1000;m.ObserveTime(now);game.Refresh();game.CameraController.SetView(game.World.PositionOfPlot(15),8);yield return Shot("12-All-new-crops");
                    game.CameraController.SetView(game.World.PositionOfPlot(25),4);yield return Shot("13-Tropical-crops");
                    m.State.plots[25].disease=40;m.State.plots[26].disease=80;game.Refresh();yield return Shot("14-Crop-health");
                }
                Check(store.Save(m.State),"Final save");restored=new FarmModel(store.Load(now));Check(JsonUtility.ToJson(restored.Island)==JsonUtility.ToJson(m.Island),"Complete state round trip");
                Check(before==JsonUtility.ToJson(live.Model.State),"Player farm changed");success=true;
                File.WriteAllText(result,"{\"status\":\"complete\",\"sampleChain\":true,\"saveMigration\":true,\"placement\":true,\"playerPreserved\":true,\"allPrefabs\":"+(all?330:31)+"}");
            }
            finally
            {
                if(!success)File.WriteAllText(result,"{\"status\":\"failed\",\"detail\":\"See Editor log\"}");
                game.gameObject.SetActive(false);Object.Destroy(game.gameObject);live.gameObject.SetActive(true);RenderSettings.sun=sun;RenderSettings.ambientLight=ambient;Object.Destroy(runner.gameObject);
            }
        }
    }
}
