using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HarvestIsle.Editor
{
    /// <summary>
    /// Play Mode review for the west-cape town and the island day-night cycle.
    /// Runs on an isolated farm, forces day and night phases for screenshots and
    /// restores the live farm untouched.
    /// </summary>
    internal static class FarmTownReview
    {
        internal static void Verify()
        {
            var live = Object.FindFirstObjectByType<FarmGame>();
            if (live == null) throw new InvalidOperationException("Farm not open");
            var game = new GameObject("Isolated town review").AddComponent<FarmGame>();
            game.StartCoroutine(Run(live, game));
        }

        private static System.Collections.IEnumerator Run(FarmGame live, FarmGame game)
        {
            string before = JsonUtility.ToJson(live.Model.State);
            Light previousSun = RenderSettings.sun;
            Color previousAmbient = RenderSettings.ambientLight, previousFog = RenderSettings.fogColor;
            double now = FarmGame.UtcNow();
            var store = new FarmSaveStore(Path.GetFullPath(".tools/model-work/town-" + DateTime.UtcNow.Ticks));
            bool complete=false;File.WriteAllText(".tools/model-work/town-result.json","{\"status\":\"running\"}");
            live.gameObject.SetActive(false);
            try
            {
                game.InitializeSession(store, () => now);
                yield return null;
                var world = game.World;
                Check(world.TownRoot != null, "Town built");
                var shop = GameObject.Find("Hotpot shop");
                Check(shop != null && FarmWorld.InTownZone(shop.transform.position), "Hotpot shop stands in the town");
                Check(world.TownRoot.Find("Chợ nông sản") != null, "Produce market present");
                int stalls = 0, houses = 0;
                foreach (Transform child in world.TownRoot)
                {
                    if (child.name.StartsWith("Nhà dân")) houses++;
                    if (child.name == "Chợ nông sản") foreach (Transform item in child) if (item.name.StartsWith("Market stall")) stalls++;
                }
                Check(stalls == 6, "Market has six stalls");
                Check(houses == 10, "Town has ten garden houses");
                var island=world.transform.Find("IslandBase(Clone)");
                Check(island!=null&&Mathf.Approximately(island.localScale.x,6),"Island has nine times the previous area");
                Check(world.TownLights.Count >= 5 && world.TownGlows.Count >= 2, "Lamps and glow materials registered");
                var walkers = Object.FindObjectsByType<FarmTownStroller>(FindObjectsSortMode.None);
                Check(walkers.Length >= 6, "Six hotpot customers stroll the street");

                var dayNight = game.gameObject.AddComponent<FarmDayNight>();
                dayNight.Configure(game);
                dayNight.SetPhase(.5f);
                yield return null;
                Check(RenderSettings.ambientLight.g > .6f, "Noon ambient is bright");
                float noonAmbient=RenderSettings.ambientLight.g, noonSun=RenderSettings.sun.intensity;
                game.CameraController.ShowTown();
                yield return null;
                var shopHit=shop.GetComponent<BoxCollider>();
                Vector3 shopScreen=world.GameCamera.WorldToScreenPoint(shopHit.bounds.center);
                Check(shopScreen.z>0&&shopScreen.x>0&&shopScreen.x<Screen.width&&shopScreen.y>0&&shopScreen.y<Screen.height,"Shop visible from town camera");
                RaycastHit hit;
                Check(Physics.Raycast(world.GameCamera.ScreenPointToRay(shopScreen),out hit,world.GameCamera.farClipPlane,1<<FarmWorld.InteractionLayer)&&hit.collider==shopHit,"Shop remains selectable");
                Directory.CreateDirectory("Art/Previews/Farm");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Town-day.png"));
                yield return new WaitForSecondsRealtime(.3f);

                var start = new Vector3[walkers.Length];
                for (int i = 0; i < walkers.Length; i++) start[i] = walkers[i].transform.position;
                yield return new WaitForSecondsRealtime(2.5f);
                int moved = 0;
                for (int i = 0; i < walkers.Length; i++)
                    if ((walkers[i].transform.position - start[i]).sqrMagnitude > .002f) moved++;
                Check(moved >= 3, "Customers walk along the street");

                dayNight.SetPhase(.95f);
                yield return null;
                Check(RenderSettings.ambientLight.g < noonAmbient*.9f && RenderSettings.ambientLight.g > .45f, "Night remains readable and dimmer than noon");
                Check(RenderSettings.sun != null && RenderSettings.sun.intensity < noonSun*.85f && RenderSettings.sun.intensity > .35f, "Night sun dimmed but readable");
                bool lampsLit = world.TownLights.Count > 0 && world.TownLights[0] != null && world.TownLights[0].enabled;
                Check(lampsLit, "Street lamps lit at night");
                bool windowsGlow = false;
                foreach (var glow in world.TownGlows)
                    if (glow.material != null && glow.material.GetColor("_EmissionColor").maxColorComponent > .2f) windowsGlow = true;
                Check(windowsGlow, "Windows glow at night");
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Town-night.png"));
                yield return new WaitForSecondsRealtime(.3f);

                dayNight.SetPhase(.5f);
                yield return null;
                game.CameraController.ShowIsland();
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Island-expanded.png"));
                yield return new WaitForSecondsRealtime(.3f);
                game.CameraController.SetView(new Vector3(-36,.15f,20),6);
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Town-gardens.png"));
                yield return new WaitForSecondsRealtime(.3f);
                game.CameraController.SetView(new Vector3(-49,.15f,-33),6);
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Farm/Island-harbor.png"));
                yield return new WaitForSecondsRealtime(.3f);
                Check(JsonUtility.ToJson(live.Model.State) == before, "Existing farm untouched");
                File.WriteAllText(".tools/model-work/town-result.json",
                    "{\"status\":\"complete\",\"stalls\":" + stalls + ",\"houses\":" + houses +
                    ",\"customers\":" + walkers.Length + ",\"moved\":" + moved +
                    ",\"lamps\":" + world.TownLights.Count + ",\"dayNight\":true,\"existingFarmUntouched\":true}");
                complete=true;
                Debug.Log("TOWN_VERIFIED: " + stalls + " stalls, " + houses + " houses, " + walkers.Length + " customers (" + moved + " moving), day-night cycle applied.");
            }
            finally
            {
                if(!complete)File.WriteAllText(".tools/model-work/town-result.json","{\"status\":\"failed\"}");
                Object.Destroy(game.gameObject);
                RenderSettings.sun = previousSun;
                RenderSettings.ambientLight = previousAmbient;
                RenderSettings.fogColor = previousFog;
                live.gameObject.SetActive(true);
            }
        }

        private static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException("Town check failed: " + label); }
    }
}
