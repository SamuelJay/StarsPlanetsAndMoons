using UnityEngine;
using UnityEngine.Rendering;

namespace StarsPlanetsAndMoons {
    public class LocalView : MonoBehaviour {
        [SerializeField] private SolarSimulation simulation;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Light sunLight;
        [SerializeField] private GameObject[] nightObjects;

        [Header("Sun")]
        [SerializeField] private float sunIntensity = 1.3f;
        [SerializeField] private Color horizonSunColor = new Color(1f, 0.55f, 0.3f);
        [SerializeField] private Color highSunColor = new Color(1f, 0.96f, 0.88f);

        [Header("Ambient")]
        [SerializeField] private Color daySkyAmbient = new Color(0.55f, 0.65f, 0.85f);
        [SerializeField] private Color dayEquatorAmbient = new Color(0.55f, 0.58f, 0.62f);
        [SerializeField] private Color dayGroundAmbient = new Color(0.3f, 0.3f, 0.28f);
        [SerializeField] private Color nightSkyAmbient = new Color(0.05f, 0.07f, 0.14f);
        [SerializeField] private Color nightEquatorAmbient = new Color(0.04f, 0.05f, 0.1f);
        [SerializeField] private Color nightGroundAmbient = new Color(0.02f, 0.02f, 0.035f);

        [Header("Fog")]
        [SerializeField] private float fogDensity = 0.0003f;
        [SerializeField] private float fogFalloffAltitude = 150f;
        [SerializeField] private Color dayFog = new Color(0.7f, 0.8f, 0.92f);
        [SerializeField] private Color twilightFog = new Color(0.9f, 0.5f, 0.35f);
        [SerializeField] private Color nightFog = new Color(0.02f, 0.025f, 0.05f);

        [Header("Thresholds (sine of sun elevation)")]
        [SerializeField] private float nightLightsBelow = 0.02f;
        [SerializeField] private float twilightWidth = 0.25f;

        private void OnEnable() {
            RenderSettings.sun = sunLight;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            Apply();
        }

        private void Update() {
            Apply();
        }

        private void Apply() {
            Vector3 sunLocal = simulation.HomeSunDirectionLocal;
            float elevation = sunLocal.y;

            sunLight.transform.rotation = Quaternion.LookRotation(-sunLocal);
            sunLight.intensity = sunIntensity * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.02f, 0.2f, elevation));
            sunLight.color = Color.Lerp(horizonSunColor, highSunColor, Mathf.InverseLerp(0f, 0.5f, elevation));

            float day = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.1f, 0.25f, elevation));
            float twilight = Mathf.Clamp01(1f - Mathf.Abs(elevation) / twilightWidth);

            RenderSettings.ambientSkyColor = Color.Lerp(nightSkyAmbient, daySkyAmbient, day);
            RenderSettings.ambientEquatorColor = Color.Lerp(nightEquatorAmbient, dayEquatorAmbient, day);
            RenderSettings.ambientGroundColor = Color.Lerp(nightGroundAmbient, dayGroundAmbient, day);
            RenderSettings.fogDensity = fogDensity / (1f + Mathf.Max(0f, viewCamera.transform.position.y) / fogFalloffAltitude);
            RenderSettings.fogColor = Color.Lerp(Color.Lerp(nightFog, dayFog, day), twilightFog, twilight * 0.7f);

            bool night = elevation < nightLightsBelow;
            foreach (GameObject nightObject in nightObjects) {
                if (nightObject.activeSelf != night) {
                    nightObject.SetActive(night);
                }
            }
        }
    }
}
