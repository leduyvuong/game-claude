using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    public static class WeatherAssets
    {
        [MenuItem("Harvest Isle/Weather/Add weather to playground")]
        public static void AddToPlayground()
        {
            const string folder="Assets/HarvestIsle/Resources/Weather";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            Shader shader=Shader.Find("Harvest Isle/Weather Particle");if(shader==null)throw new InvalidOperationException("Weather shader missing");
            string[] names={"Rain","Splash","Snow","Cloud","Lightning","Leaf"};
            Color[] colors={new Color(.73f,.88f,.98f,.88f),new Color(.75f,.92f,1,.44f),new Color(.96f,.98f,1,.92f),Color.white,new Color(.82f,.94f,1,1),Color.white};
            for(int i=0;i<names.Length;i++)
            {
                string path=folder+"/"+names[i]+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(shader){name="Weather "+names[i]};AssetDatabase.CreateAsset(mat,path);}
                mat.shader=shader;mat.SetColor("_BaseColor",colors[i]);mat.SetFloat("_Shape",i);EditorUtility.SetDirty(mat);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(PlaygroundSetup.ScenePath,OpenSceneMode.Single);
            var root=GameObject.Find("Weather controller");if(root==null)root=new GameObject("Weather controller");
            var weather=root.GetComponent<PlaygroundWeather>();if(weather==null)weather=root.AddComponent<PlaygroundWeather>();
            var data=new SerializedObject(weather);data.FindProperty("initialWeather").enumValueIndex=(int)FarmWeather.Rain;data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),PlaygroundSetup.ScenePath);
            Debug.Log("WEATHER_SCENE_READY: five weather modes and persistent URP materials; starts with rain.");
        }
        [MenuItem("Harvest Isle/Weather/Open playground with weather and Play")]
        public static void OpenAndPlay(){AddToPlayground();PlaygroundSetup.OpenAndPlay();}
        public static void VerifyLive()
        {
            var weather=UnityEngine.Object.FindFirstObjectByType<PlaygroundWeather>();if(weather==null)throw new InvalidOperationException("Weather controller missing in Play Mode");
            var viewer=UnityEngine.Object.FindFirstObjectByType<ModelLibraryViewer>();if(viewer!=null)viewer.Hide();
            var director=UnityEngine.Object.FindFirstObjectByType<PlaygroundDirector>();if(director!=null)director.ShowProduction(false);
            Directory.CreateDirectory("Art/Previews/Weather");Directory.CreateDirectory(".tools/weather");
            int phase=0;double started=EditorApplication.timeSinceStartup;bool selected=false;string pendingScreenshot=null;var checks=new List<Check>();
            EditorApplication.CallbackFunction tick=null;tick=()=>
            {
                try
                {
                    if(!EditorApplication.isPlaying||weather==null)throw new InvalidOperationException("Play Mode ended during weather check");
                    if(pendingScreenshot!=null)
                    {
                        if(EditorApplication.timeSinceStartup-started<.4||!File.Exists(pendingScreenshot))return;
                        pendingScreenshot=null;phase++;selected=false;return;
                    }
                    if(phase<5)
                    {
                        var mode=(FarmWeather)phase;
                        if(!selected){if(!weather.SetWeather(mode))return;selected=true;started=EditorApplication.timeSinceStartup;return;}
                        if(EditorApplication.timeSinceStartup-started<1.8)return;
                        if((mode==FarmWeather.Rain||mode==FarmWeather.Storm)&&(weather.RainParticles<25||weather.SplashParticles<2))throw new InvalidOperationException("Rain or splashes not emitting: "+mode);
                        if((mode==FarmWeather.Wind||mode==FarmWeather.Storm)&&weather.GustParticles<2)throw new InvalidOperationException("Wind leaves not emitting: "+mode);
                        if((mode==FarmWeather.Rain||mode==FarmWeather.Storm)&&weather.LowestRainHeight>7)throw new InvalidOperationException("Rain particles did not fall");
                        if((mode==FarmWeather.Wind||mode==FarmWeather.Storm)&&weather.TreeSwayAngle<.1f)throw new InvalidOperationException("Tree sway did not move");
                        if(mode==FarmWeather.Snow&&weather.SnowParticles<20)throw new InvalidOperationException("Snow not emitting");
                        if(mode==FarmWeather.Clear&&(weather.RainParticles>0||weather.SnowParticles>0||weather.GustParticles>0))throw new InvalidOperationException("Clear weather left precipitation behind");
                        if(weather.SwayingTrees<3)throw new InvalidOperationException("Palm wind rig missing");
                        if(mode==FarmWeather.Storm&&!weather.TriggerLightning())throw new InvalidOperationException("Lightning trigger failed");
                        pendingScreenshot=Path.GetFullPath("Art/Previews/Weather/"+mode+".png");if(File.Exists(pendingScreenshot))File.Delete(pendingScreenshot);
                        ScreenCapture.CaptureScreenshot(pendingScreenshot);checks.Add(new Check{mode=mode.ToString(),rain=weather.RainParticles,splashes=weather.SplashParticles,leaves=weather.GustParticles,snow=weather.SnowParticles,clouds=weather.CloudParticles,trees=weather.SwayingTrees,swayAngle=weather.TreeSwayAngle,lowestRainHeight=weather.LowestRainHeight});
                        started=EditorApplication.timeSinceStartup;return;
                    }
                    weather.SetWeather(FarmWeather.Storm);weather.TriggerLightning();started=EditorApplication.timeSinceStartup;phase++;
                }
                catch(Exception error){Finish(error);return;}
                EditorApplication.update-=tick;EditorApplication.CallbackFunction thunderTick=null;thunderTick=()=>
                {
                    if(EditorApplication.timeSinceStartup-started<.7)return;EditorApplication.update-=thunderTick;
                    try
                    {
                        if(!weather.ThunderPlaying||weather.LightningCount<2)throw new InvalidOperationException("Thunder playback failed");
                        if(weather.SetWeather((FarmWeather)99))throw new InvalidOperationException("Invalid weather accepted");
                        weather.SetSound(false);weather.SetSound(true);weather.SetWeather(FarmWeather.Rain);
                        File.WriteAllText(".tools/weather/status.json",JsonUtility.ToJson(new Report{status="complete",presets=checks.ToArray(),lightning=weather.LightningCount,thunderVerified=true},true));
                        Debug.Log("WEATHER_LIVE_VERIFIED: 5 presets, precipitation, tree sway, lightning, thunder and reset; rain left running.");
                    }
                    catch(Exception error){Finish(error);}
                };EditorApplication.update+=thunderTick;
            };
            void Finish(Exception error)
            {EditorApplication.update-=tick;File.WriteAllText(".tools/weather/status.json",JsonUtility.ToJson(new Report{status="failed",error=error.ToString()},true));Debug.LogException(error);}
            EditorApplication.update+=tick;
        }
        [Serializable]private sealed class Check{public string mode;public int rain,splashes,leaves,snow,clouds,trees;public float swayAngle,lowestRainHeight;}
        [Serializable]private sealed class Report{public string status,error;public Check[] presets;public int lightning;public bool thunderVerified;}
    }
}
