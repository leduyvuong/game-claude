using UnityEngine;

namespace HarvestIsle
{
    /// <summary>
    /// Cosmetic island-wide day-night cycle: sun angle/color/intensity, ambient,
    /// fog, sky and the town's window/lamp glow all follow one normalized phase
    /// (0 = midnight, 0.5 = noon). Rain dimming from FarmClimate is folded in here
    /// so the two systems never fight over the sun. Gameplay rules ignore the hour;
    /// the cycle pauses while the hotpot interior owns the lighting.
    /// </summary>
    public sealed class FarmDayNight : MonoBehaviour
    {
        public const float DaySeconds = 480f;
        private FarmGame game;
        private float phase = .42f; // load into late morning
        private Gradient sunColor, ambientColor, skyColor, fogColor;
        private AnimationCurve sunPower, sunElevation, nightCurve;
        public float Phase => phase;
        public float NightWeight { get; private set; }

        public void Configure(FarmGame owner)
        {
            game = owner;
            sunColor = Ramp("5A6B9E", "5A6B9E", "FFC284", "FFF0CB", "FFF0CB", "FFB36B", "5A6B9E", "5A6B9E");
            ambientColor = Ramp("9AAFC1", "9AAFC1", "D8CBB4", "C8D6CF", "C8D6CF", "E2C3A0", "9AAFC1", "9AAFC1");
            skyColor = Ramp("15233D", "15233D", "F0BC8E", "78C8C8", "78C8C8", "E89B72", "15233D", "15233D");
            fogColor = Ramp("24354E", "24354E", "D8B59A", "8CD4CE", "8CD4CE", "C98F74", "24354E", "24354E");
            sunPower = Curve(.7f, .7f, .9f, .95f, .95f, .9f, .7f, .7f);
            sunElevation = Curve(38, 30, 16, 52, 52, 16, 30, 38);
            nightCurve = Curve(1, 1, .25f, 0, 0, .3f, 1, 1);
            Apply(phase);
        }

        // Eight stops: midnight, pre-dawn (.22), sunrise (.3), morning (.4),
        // afternoon (.68), sunset (.78), dusk (.86), midnight again.
        private static readonly float[] Stops = { 0f, .22f, .3f, .4f, .68f, .78f, .86f, 1f };
        private static Gradient Ramp(params string[] hex)
        {
            var keys = new GradientColorKey[8];
            for (int i = 0; i < 8; i++) keys[i] = new GradientColorKey(FarmWorld.Hex(hex[i]), Stops[i]);
            var gradient = new Gradient();
            gradient.SetKeys(keys, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
            return gradient;
        }
        private static AnimationCurve Curve(params float[] values)
        {
            var curve = new AnimationCurve();
            for (int i = 0; i < values.Length; i++) curve.AddKey(Stops[i], values[i]);
            return curve;
        }

        public void SetPhase(float value) { phase = Mathf.Repeat(value, 1f); Apply(phase); }

        private void Update()
        {
            if (game == null || game.World == null) return;
            // The hotpot interior saves/restores its own ambient; freeze until it closes.
            if (game.Hotpot != null && game.Hotpot.IsOpen) return;
            phase = Mathf.Repeat(phase + Time.deltaTime / DaySeconds, 1f);
            Apply(phase);
        }

        private void Apply(float t)
        {
            var sun = RenderSettings.sun;
            if (sun == null || game == null || game.World == null) return;
            float night = Mathf.Clamp01(nightCurve.Evaluate(t));
            NightWeight = night;
            int rain = game.Model != null ? game.Model.RainLevel : 0;
            float rainDim = rain == 0 ? 1f : rain == 1 ? .82f : .72f;
            Color color = sunColor.Evaluate(t);
            if (rain > 0) color = Color.Lerp(color, FarmWorld.Hex("CBDCEB"), (1 - night) * .75f);
            sun.color = color;
            sun.intensity = sunPower.Evaluate(t) * rainDim;
            sun.transform.rotation = Quaternion.Euler(sunElevation.Evaluate(t), -32, 0);
            RenderSettings.ambientLight = ambientColor.Evaluate(t) * (rain > 0 ? .88f : 1f);
            RenderSettings.fogColor = fogColor.Evaluate(t);
            if (game.World.GameCamera != null) game.World.GameCamera.backgroundColor = skyColor.Evaluate(t);
            foreach (var glow in game.World.TownGlows)
            {
                if (glow.material == null) continue;
                Color lit = Color.Lerp(glow.day, glow.night, night);
                glow.material.SetColor("_BaseColor", lit);
                if (glow.material.HasProperty("_Color")) glow.material.color = lit;
                glow.material.SetColor("_EmissionColor", glow.night * (night * .9f));
            }
            bool lampsOn = night > .05f;
            foreach (var lamp in game.World.TownLights)
            {
                if (lamp == null) continue;
                lamp.enabled = lampsOn;
                lamp.intensity = night * 1.25f;
            }
            bool nightLife = night > .4f;
            foreach (var item in game.World.TownNightOnly)
                if (item != null && item.activeSelf != nightLife) item.SetActive(nightLife);
        }
    }
}
