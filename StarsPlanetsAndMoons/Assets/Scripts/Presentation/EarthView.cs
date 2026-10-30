using UnityEngine;

namespace StarsPlanetsAndMoons {
    public class EarthView : MonoBehaviour {
        [SerializeField] private SolarSimulation simulation;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform earth;
        [SerializeField] private Transform homeMarker;
        [SerializeField] private Transform sunRig;
        [SerializeField] private Light sunLight;

        [Header("Scale")]
        [SerializeField] private float radius = 100f;
        [SerializeField] private float sunDistanceInRadii = 4f;
        [SerializeField] private float markerReferenceDistanceInRadii = 3.2f;
        [SerializeField, Range(0.05f, 1f)] private float markerMinimumScale = 0.2f;

        [Header("Space lighting")]
        [SerializeField] private Color ambientColor = new Color(0.07f, 0.08f, 0.11f);

        public float Radius => radius;
        public float SunDistance => sunDistanceInRadii * radius;
        public Vector3 Center => transform.position;

        private void OnEnable() {
            RenderSettings.fog = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            Apply();
        }

        private void Update() {
            Apply();
        }

        private void Apply() {
            earth.localRotation = simulation.EarthRotation;
            homeMarker.localPosition = simulation.HomeSurfaceDirection * radius;
            homeMarker.localRotation = Quaternion.FromToRotation(Vector3.up, simulation.HomeSurfaceDirection);

            float distance = Vector3.Distance(viewCamera.transform.position, homeMarker.position);
            float referenceDistance = markerReferenceDistanceInRadii * radius;
            homeMarker.localScale = Vector3.one * Mathf.Clamp(distance / referenceDistance, markerMinimumScale, 1f);

            sunRig.localRotation = Quaternion.LookRotation(simulation.SunDirection);
            sunRig.localPosition = Vector3.zero;
            sunLight.transform.rotation = Quaternion.LookRotation(-simulation.SunDirection);
        }
    }
}
