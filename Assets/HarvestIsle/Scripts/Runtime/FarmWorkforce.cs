using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    /// <summary>
    /// Visual layer for hired workers: the worker house by the south road, one
    /// wandering character per hired worker, and a few ambient villagers so the
    /// island feels peopled. All rules live in FarmModel; this only mirrors state.
    /// </summary>
    public sealed partial class FarmWorkforce : MonoBehaviour
    {
        private static readonly Vector3 HousePosition = new Vector3(6.3f, 0.15f, -4.7f);
        private FarmGame game;
        private GameObject house;
        private int shownLevel = -1;
        private readonly List<GameObject> crew = new List<GameObject>();
        private float nextSync;
        private string legacySignature="";
        public Vector3 HousePoint {get{return game.IsEstate?new Vector3(-9,.15f,18):HousePosition;}}

        public void Configure(FarmGame owner)
        {
            game = owner;
            PlaceVillagers();
            Sync();
        }

        private void PlaceVillagers()
        {
            var world = game.World;
            var merchant = world.Imported("Collection/MerchantHaiAu", world.transform, new Vector3(-14.6f, 0, -10.0f), 1.0f, 0.8f);
            if (merchant != null) { merchant.name = "Villager Hải Âu"; merchant.transform.localRotation = Quaternion.Euler(0, 130, 0); }
            var elder = world.Imported("Collection/ElderTu", world.transform, new Vector3(6.35f, 0.15f, -0.15f), 1.0f, 0.8f);
            if (elder != null) { elder.name = "Villager Già làng Tư"; elder.transform.localRotation = Quaternion.Euler(0, -55, 0); }
            var stroller = world.Imported("Collection/FarmerFemale", world.transform, new Vector3(1.6f, 0.21f, 3.1f), 1.0f, 0.8f);
            if (stroller != null)
            {
                stroller.name = "Villager dạo đường";
                stroller.AddComponent<FarmAnimalWander>().Configure(new Vector3(1.6f, 0.21f, 3.1f), 1.3f, 0.08f, 23);
            }
        }

        private void Update()
        {
            if (game == null || Time.unscaledTime < nextSync) return;
            nextSync = Time.unscaledTime + 0.5f;
            Sync();
        }

        private void Sync()
        {
            int level = game.Model.State.workerHouse;
            if (level != shownLevel) BuildHouse(level);
            var workers = game.Model.State.workers;
            SyncSpecialists();
            string signature="";foreach(var worker in workers)signature+=worker.kind+":"+worker.level+";";
            if(signature!=legacySignature){foreach(var avatar in crew)if(avatar!=null)Destroy(avatar);crew.Clear();legacySignature=signature;}
            while (crew.Count > workers.Count) { var last = crew[crew.Count - 1]; crew.RemoveAt(crew.Count - 1); if (last != null) Destroy(last); }
            while (crew.Count < workers.Count)
            {
                var worker = workers[crew.Count];
                string prefab = worker.kind == 0 ? "Collection/ElderTu" : worker.kind == 1 ? "Collection/FarmerFemale" : "Characters/Farmer";
                GameObject avatar = worker.kind == 2
                    ? Instantiate(Resources.Load<GameObject>(prefab), game.World.transform)
                    : game.World.Imported(prefab, game.World.transform, Vector3.zero, 0.95f, 0.8f);
                if (avatar == null) break;
                avatar.name = "Worker " + FarmModel.WorkerKinds[worker.kind] + " L" + worker.level;
                Vector3 post = HousePoint + new Vector3(-1.1f - crew.Count * 0.8f, 0.02f, 0.4f + crew.Count * 0.5f);
                avatar.transform.localPosition = post;
                avatar.transform.localScale *= worker.kind == 2 ? 0.95f : 1f;
                foreach (var collider in avatar.GetComponentsInChildren<Collider>()) collider.enabled = false;
                avatar.AddComponent<FarmAnimalWander>().Configure(post, 0.5f, 0.06f, 7 + crew.Count * 13);
                crew.Add(avatar);
            }
        }

        private void BuildHouse(int level)
        {
            shownLevel = level;
            if (house != null) { house.SetActive(false); Destroy(house); house = null; }
            if (level < 1) return;
            var world = game.World;
            house = new GameObject("Worker house L" + level);
            house.transform.SetParent(world.transform, false);
            house.transform.localPosition = HousePoint;
            house.layer=FarmWorld.InteractionLayer;house.AddComponent<FarmWorkerTarget>();
            var hit=house.AddComponent<BoxCollider>();hit.center=new Vector3(0,.65f,0);hit.size=new Vector3(3,1.5f,2);
            house.transform.localRotation = Quaternion.Euler(0, 25, 0);
            world.Shape(PrimitiveType.Cube, "House walls", house.transform, new Vector3(0, 0.5f, 0), new Vector3(1.7f, 1.0f, 1.3f), FarmWorld.Hex("E8D5A4"));
            world.Shape(PrimitiveType.Cube, "House door", house.transform, new Vector3(0.25f, 0.38f, -0.66f), new Vector3(0.45f, 0.76f, 0.05f), FarmWorld.Hex("6E4A2C"));
            for (int side = -1; side <= 1; side += 2)
            {
                var roof = world.Shape(PrimitiveType.Cube, "House roof", house.transform, new Vector3(side * 0.5f, 1.2f, 0), new Vector3(1.15f, 0.13f, 1.65f), FarmWorld.Hex("A6683F"));
                roof.transform.localRotation = Quaternion.Euler(0, 0, side * -25);
            }
            if (level >= 2)
                world.Shape(PrimitiveType.Cube, "House annex", house.transform, new Vector3(-1.25f, 0.35f, 0.1f), new Vector3(0.8f, 0.7f, 1.0f), FarmWorld.Hex("E0C890"));
            if (level >= 3)
            {
                world.Shape(PrimitiveType.Cylinder, "House flagpole", house.transform, new Vector3(0.75f, 1.75f, 0.4f), new Vector3(0.04f, 0.45f, 0.04f), FarmWorld.Hex("8A6B45"));
                world.Shape(PrimitiveType.Cube, "House pennant", house.transform, new Vector3(0.92f, 2.05f, 0.4f), new Vector3(0.3f, 0.18f, 0.02f), FarmWorld.Hex("F7B733"));
            }
        }
    }
}
