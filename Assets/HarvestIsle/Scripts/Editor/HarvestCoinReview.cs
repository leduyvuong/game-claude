using System;
using System.IO;
using HarvestIsle.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HarvestIsle.Editor
{
    [InitializeOnLoad]
    public static class HarvestCoinReview
    {
        private const string Key = "HarvestIsle.CurrencyReview";
        private static int stage;
        private static double deadline;
        private static string pending;
        private static FarmGame game;
        static HarvestCoinReview() { EditorApplication.update += Tick; }

        [MenuItem("Harvest Isle/Currency/Review Xu Mua Vang in Play")]
        public static void Begin()
        {
            SessionState.SetBool(Key, true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!Application.isBatchMode)
                EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            try
            {
                if (pending != null)
                {
                    string result = File.ReadAllText(pending);
                    if (result.Contains("failed")) throw new InvalidOperationException(result);
                    if (!result.Contains("complete"))
                    { if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException(pending); return; }
                    pending = null; stage++; deadline = EditorApplication.timeSinceStartup + 2;
                    return;
                }
                if (EditorApplication.timeSinceStartup < deadline) return;
                switch (stage)
                {
                    case 0:
                        game = new GameObject("Currency isolated review farm").AddComponent<FarmGame>();
                        game.InitializeSession(new FarmSaveStore(Path.GetFullPath(".tools/model-work/currency-farm-" + DateTime.UtcNow.Ticks)), FarmGame.UtcNow);
                        stage++; deadline = EditorApplication.timeSinceStartup + 5;
                        break;
                    case 1:
                        var icon = game.Hud.transform.Find("Safe area/Status/Coin icon").GetComponent<Image>();
                        if (icon.sprite == null || icon.sprite != Resources.Load<Sprite>("Currency/HarvestCoin"))
                            throw new InvalidOperationException("Wallet has no currency sprite");
                        var prefab = Resources.Load<GameObject>("Collection/Coin");
                        if (prefab == null || prefab.GetComponent<MeshFilter>().sharedMesh.triangles.Length < 100)
                            throw new InvalidOperationException("Currency geometry missing");
                        Start("farm-loop-result.json", FarmLoopReview.Verify);
                        break;
                    case 2: Start("play-interface-result.json", PlayInterfaceReview.Run); break;
                    case 3:
                        game.Hud.ClosePanels(); game.ClearSelection(); game.CameraController.GoHome();
                        game.Model.State.coins = 1000000000;
                        stage++; deadline = EditorApplication.timeSinceStartup + 2;
                        break;
                    case 4:
                        ScreenCapture.CaptureScreenshot(Path.GetFullPath("Art/Previews/Currency/Wallet-max.png"));
                        stage++; deadline = EditorApplication.timeSinceStartup + 2;
                        break;
                    default:
                        if (!File.Exists("Art/Previews/Currency/Wallet-max.png"))
                            throw new InvalidOperationException("Wallet screenshot missing; run the review in a graphical Editor.");
                        Finish(true); break;
                }
            }
            catch (Exception e) { Debug.LogException(e); Finish(false); }
        }

        private static void Start(string name, Action action)
        {
            pending = ".tools/model-work/" + name;
            File.WriteAllText(pending, "{\"status\":\"running\"}");
            deadline = EditorApplication.timeSinceStartup + 180;
            action();
        }
        private static void Finish(bool success)
        {
            SessionState.SetBool(Key, false); stage = 0; pending = null;
            File.WriteAllText(".tools/model-work/currency-play-result.json", success
                ? "{\"status\":\"complete\",\"walletSprite\":true,\"farmCheck\":true,\"playInterfaceCheck\":true,\"isolatedSave\":true}"
                : "{\"status\":\"failed\"}");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
