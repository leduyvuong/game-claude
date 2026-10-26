using UnityEngine;
namespace HarvestIsle
{
    public sealed class WeatherTreeSway : MonoBehaviour
    {
        private PlaygroundWeather weather;
        private Quaternion rest;
        private float phase;
        public float CurrentAngle {get;private set;}
        public void Configure(PlaygroundWeather controller,float offset)
        {weather=controller;rest=transform.localRotation;phase=offset;}
        private void LateUpdate()
        {
            if(weather==null)return;
            float wind=weather.Suspended?0:weather.WindStrength;
            CurrentAngle=wind*(1.5f+Mathf.Sin(Time.time*1.7f+phase)*1.3f+Mathf.Sin(Time.time*.63f+phase)*.6f);
            transform.localRotation=rest*Quaternion.Euler(CurrentAngle*.35f,0,-CurrentAngle);
        }
        private void OnDestroy(){if(transform!=null)transform.localRotation=rest;}
    }
}
