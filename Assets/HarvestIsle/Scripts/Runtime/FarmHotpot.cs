using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot : MonoBehaviour
    {
        private sealed class Table
        {
            public int floor, slot, recipeId;
            public bool occupied, served;
            public float remaining;
            public Transform guest, pot;
            public GameObject specialtyVisual;
            public int specialtyKind=-1;
        }
        private FarmGame game;
        private readonly List<Table> tables = new List<Table>();
        private float nextArrival;
        private float feedbackUntil;
        private Color farmAmbient;
        public bool IsOpen { get; private set; }
        public HotpotServiceSession Service {get;private set;}
        private bool reviewSuspended;
        public void SuspendForReview(bool value){reviewSuspended=value;}
        public void Configure(FarmGame owner) { game=owner; BuildExterior(); }
        // The restaurant anchors the west-cape town plaza (FarmWorld.Town); tapping
        // it still opens the same interior. Two floors echo the multi-floor quán.
        private void BuildExterior()
        {
            var world = game.World;
            var root = new GameObject("Hotpot shop");
            root.transform.SetParent(world.transform, false);
            root.transform.localPosition = game.IsEstate?new Vector3(-24, .15f, 15):new Vector3(-29f, 0.15f, 12.3f);
            root.transform.localRotation = Quaternion.Euler(0, 0, 0);
            world.Shape(PrimitiveType.Cube, "Shop walls", root.transform, new Vector3(0, 0.62f, 0), new Vector3(2.7f, 1.24f, 1.9f), FarmWorld.Hex("F3E2B8"));
            world.Shape(PrimitiveType.Cube, "Shop door", root.transform, new Vector3(-0.5f, 0.47f, -0.96f), new Vector3(0.56f, 0.94f, 0.05f), FarmWorld.Hex("7A4A2B"));
            world.GlowShape(PrimitiveType.Cube, "Shop window", root.transform, new Vector3(0.45f, 0.72f, -0.96f), new Vector3(0.52f, 0.44f, 0.05f), "window", FarmWorld.Hex("A9D4DE"), FarmWorld.Hex("FFD98A"));
            world.GlowShape(PrimitiveType.Cube, "Shop window", root.transform, new Vector3(1.36f, 0.72f, 0.1f), new Vector3(0.05f, 0.44f, 0.6f), "window", FarmWorld.Hex("A9D4DE"), FarmWorld.Hex("FFD98A"));
            world.Shape(PrimitiveType.Cube, "Shop upper walls", root.transform, new Vector3(0, 1.7f, 0.12f), new Vector3(2.45f, 0.92f, 1.66f), FarmWorld.Hex("F7EBC6"));
            world.GlowShape(PrimitiveType.Cube, "Upper window", root.transform, new Vector3(-0.55f, 1.76f, -0.72f), new Vector3(0.48f, 0.4f, 0.05f), "window", FarmWorld.Hex("A9D4DE"), FarmWorld.Hex("FFD98A"));
            world.GlowShape(PrimitiveType.Cube, "Upper window", root.transform, new Vector3(0.55f, 1.76f, -0.72f), new Vector3(0.48f, 0.4f, 0.05f), "window", FarmWorld.Hex("A9D4DE"), FarmWorld.Hex("FFD98A"));
            world.Shape(PrimitiveType.Cube, "Balcony floor", root.transform, new Vector3(0, 1.28f, -1.06f), new Vector3(2.1f, 0.08f, 0.5f), FarmWorld.Hex("A6794C"));
            for (int i = 0; i < 5; i++)
                world.Shape(PrimitiveType.Cube, "Balcony post", root.transform, new Vector3(-0.95f + i * 0.475f, 1.46f, -1.28f), new Vector3(0.05f, 0.3f, 0.05f), FarmWorld.Hex("97724D"));
            world.Shape(PrimitiveType.Cube, "Balcony rail", root.transform, new Vector3(0, 1.62f, -1.28f), new Vector3(2.05f, 0.05f, 0.05f), FarmWorld.Hex("97724D"));
            for (int side = -1; side <= 1; side += 2)
            {
                var roof = world.Shape(PrimitiveType.Cube, "Shop roof", root.transform, new Vector3(side * 0.68f, 2.42f, 0.12f), new Vector3(1.7f, 0.16f, 2.1f), FarmWorld.Hex("C04A3A"));
                roof.transform.localRotation = Quaternion.Euler(0, 0, side * -24);
            }
            world.Shape(PrimitiveType.Cube, "Roof ridge", root.transform, new Vector3(0, 2.62f, 0.12f), new Vector3(0.3f, 0.12f, 2.15f), FarmWorld.Hex("8A3B2E"));
            world.Shape(PrimitiveType.Cube, "Shop sign", root.transform, new Vector3(0, 1.16f, -1.0f), new Vector3(1.5f, 0.34f, 0.06f), FarmWorld.Hex("8A3B2E"));
            world.Shape(PrimitiveType.Cube, "Sign trim", root.transform, new Vector3(0, 1.16f, -1.02f), new Vector3(1.58f, 0.4f, 0.02f), FarmWorld.Hex("E8B84B"));
            for (int i = 0; i < 3; i++)
                world.GlowShape(PrimitiveType.Sphere, "Eave lantern", root.transform, new Vector3(-0.8f + i * 0.8f, 2.08f, -1.0f), Vector3.one * 0.13f, "lantern", FarmWorld.Hex("E05848"), FarmWorld.Hex("FF8A5C"));
            var lantern = world.Imported("Collection/FestivalLantern", root.transform, new Vector3(-1.55f, 0, -1.1f), 0.75f, 0.6f);
            if (lantern != null) lantern.name = "Shop lantern";
            var lanternRight = world.Imported("Collection/FestivalLantern", root.transform, new Vector3(1.6f, 0, -1.15f), 0.75f, 0.6f);
            if (lanternRight != null) lanternRight.name = "Shop lantern";
            // Sidewalk seating: two low tables with stools, and the big street pot.
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 table = new Vector3(side * 0.95f, 0, -1.85f);
                world.Shape(PrimitiveType.Cylinder, "Street table", root.transform, table + new Vector3(0, 0.26f, 0), new Vector3(0.52f, 0.03f, 0.52f), FarmWorld.Hex("D9B981"));
                world.Shape(PrimitiveType.Cylinder, "Table leg", root.transform, table + new Vector3(0, 0.13f, 0), new Vector3(0.08f, 0.13f, 0.08f), FarmWorld.Hex("97724D"));
                world.Shape(PrimitiveType.Cylinder, "Table pot", root.transform, table + new Vector3(0, 0.33f, 0), new Vector3(0.2f, 0.05f, 0.2f), FarmWorld.Hex("C8CDD4"));
                for (int s = 0; s < 3; s++)
                {
                    float a = s * 2.1f + side;
                    world.Shape(PrimitiveType.Cylinder, "Street stool", root.transform, table + new Vector3(Mathf.Cos(a) * 0.5f, 0.13f, Mathf.Sin(a) * 0.5f), new Vector3(0.16f, 0.13f, 0.16f), FarmWorld.Hex("B8905E"));
                }
                Steam(root.transform, table + new Vector3(0, 0.45f, 0));
            }
            var pot = world.Imported("Collection/VillageCookpot", root.transform, new Vector3(1.75f, 0, -0.55f), 0.62f, 1.2f);
            if (pot != null) pot.transform.localRotation = Quaternion.Euler(0, 40, 0);
            Steam(root.transform, new Vector3(1.75f, 0.7f, -0.55f));
            world.TownLampLight(root.transform, new Vector3(-0.5f, 1.05f, -1.3f), 3.6f);
            if(game.IsEstate)world.DetailEstateShop(root.transform);
            root.layer = FarmWorld.InteractionLayer;
            root.AddComponent<FarmHotpotTarget>();
            var hit = root.AddComponent<BoxCollider>();
            hit.center = new Vector3(0, 1.2f, -0.3f); hit.size = new Vector3(3.4f, 2.6f, 3.2f);
        }

        private static void Steam(Transform parent, Vector3 position)
        {
            var steam = new GameObject("Pot steam");
            steam.transform.SetParent(parent, false);
            steam.transform.localPosition = position;
            var system = steam.AddComponent<ParticleSystem>();
            var main = system.main;
            main.startColor = new Color(1, 1, 1, 0.35f);
            main.startSize = 0.16f; main.startLifetime = 2.2f; main.startSpeed = 0.35f;
            main.maxParticles = 40; main.loop = true;
            var emission = system.emission; emission.rateOverTime = 6;
            var shape = system.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.08f;
            var renderer = steam.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            renderer.sharedMaterial = new Material(shader) { name = "Hotpot steam" };
        }


        public void Open()
        {
            if(IsOpen||game.IsHarvesting||game.Ranch.IsOpen||game.Development.IsOpen)return;
            if(game.Online!=null){game.Notify("Tiệm lẩu chưa hỗ trợ chế độ online.",true);return;}
            IsOpen=true; tab=0; nextArrival=1.5f;
            Perform(game.Model.SettleHotpotOffline(game.Now));
            Service=new HotpotServiceSession(game.Model,HotpotRules.Hash((int)(game.Now/1800),game.Model.State.hotpot.progress.branch,17));
            farmAmbient=RenderSettings.ambientLight;RenderSettings.ambientLight=FarmWorld.Hex(game.Model.State.hotpot.progress.branch%4==3?"7B7895":"8D7B64");
            game.Hud.gameObject.SetActive(false);
            game.World.GameCamera.enabled=true;
            BuildRoom();
        }
        public void Close()
        {
            if(!IsOpen)return;
            if(Service!=null){Service.EndRush();Service=null;}
            foreach(var t in tables){t.occupied=false;t.served=false;SyncTable(t);}
            game.Model.State.hotpot.progress.lastBusinessUtc=game.Now;game.Save();
            IsOpen=false;
            DisposeDecorPreview();
            RenderSettings.ambientLight=farmAmbient;
            if(room!=null)room.SetActive(false);
            if(interiorCamera!=null)interiorCamera.enabled=false;
            game.World.GameCamera.enabled=true;
            game.Hud.gameObject.SetActive(true);game.Hud.Refresh();
        }
        private void OnDisable() { if(IsOpen&&!reviewSuspended)Close(); }
        private void Update()
        {
            if(!IsOpen)return;
            if(interiorCamera!=null)interiorCamera.rect=FarmWindow.Viewport(1200,720);
            if(decorStage!=null)decorStage.SetActive(tab==4&&!decorPlacing);
            game.Model.AdvanceHotpotSeason(game.Now);
            // All management tabs pause service, including patience, across every floor.
            if(tab!=0)return;
            UpdateEndlessService(Time.unscaledDeltaTime);
        }
        private void UpdateEndlessService(float dt)
        {
            if(Service==null)return;
            Service.Tick(dt);
            nextArrival-=dt;
            if(nextArrival<=0)
            {
                nextArrival=(float)Service.ArrivalSeconds*(FarmModel.FestivalActive(game.Now)?.5f:1f);
                var free=tables.Find(t=>!t.occupied);
                if(free!=null)Service.Seat(free.floor*26+free.slot);
            }
            SyncServiceTables();
        }
        private void SyncServiceTables()
        {
            foreach(var t in tables)
            {
                var order=Service.Find(t.floor*26+t.slot);t.occupied=order!=null;t.served=order!=null&&order.served;
                if(order!=null){t.recipeId=order.recipeId;t.remaining=(float)order.patience;}
                SyncTable(t);
            }
        }
        public bool ServeTable(int floor,int slot)
        {
            if(!IsOpen||tab!=0||game.Online!=null)return false;
            var t=tables.Find(x=>x.floor==floor&&x.slot==slot);
            if(t==null||!t.occupied||t.served)return false;
            int previousTier=game.Model.HotpotTier;
            bool success=Service!=null&&Service.Serve(floor*26+slot);
            if(Service!=null){message=Service.LastMessage;feedbackUntil=Time.unscaledTime+6;}
            if(success){game.Save();SyncServiceTables();if(previousTier!=game.Model.HotpotTier)BuildRoom();selectedTable=slot;}
            return success;
        }
        private void Perform(ActionResult result)
        {message=result.message;feedbackUntil=Time.unscaledTime+6;game.Notify(result.message,!result.success);if(result.success)game.Save();}
        public bool MoveFurniture(int floor,int from,int to,int rotation)
        {
            if(!IsOpen||tab!=4||game.Online!=null)return false;
            var result=game.Model.MoveHotpotFurniture(floor,from,to,rotation);Perform(result);
            if(result.success)
            {
                var movedTable=tables.Find(t=>t.floor==floor&&t.slot==from);
                if(movedTable!=null)movedTable.slot=to;
                if(Service!=null)Service.Move(floor*26+from,floor*26+to);
                BuildRoom();
            }
            return result.success;
        }
        private bool CanSeatRecipe(HotpotRecipe recipe)
        {
            var reserved=new Dictionary<string,int>();
            foreach(var t in tables)
                if(t.occupied&&!t.served)
                {var r=game.Model.State.hotpot.recipes.Find(x=>x.id==t.recipeId);if(r!=null)Reserve(r,reserved);}
            Reserve(recipe,reserved);
            foreach(var entry in reserved)
            {var parts=entry.Key.Split(':');if(game.Model.KitchenCount(parts[0],int.Parse(parts[1]))<entry.Value)return false;}
            return true;
        }
        private static void Reserve(HotpotRecipe r,Dictionary<string,int> reserved)
        {
            var portions=new List<HotpotPortion>(r.portions);portions.Add(new HotpotPortion("base",1,1));portions.Add(new HotpotPortion("seasoning",1+r.salt+r.heat,1));
            foreach(var p in portions){string key=p.id+":"+p.stars;int count;reserved.TryGetValue(key,out count);reserved[key]=count+p.amount;}
        }
        private bool HasWaitingOrders {get{return tables.Exists(t=>t.occupied&&!t.served);}}
    }
    public sealed class FarmHotpotTarget : MonoBehaviour { }
}
