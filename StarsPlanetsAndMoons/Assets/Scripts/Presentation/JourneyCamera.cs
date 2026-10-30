using UnityEngine;

namespace StarsPlanetsAndMoons {
    public class JourneyCamera : MonoBehaviour {
        [SerializeField] private SolarSimulation simulation;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private GameObject localRoot;
        [SerializeField] private EarthView earthView;
        [SerializeField] private Transform houseFocus;
        [SerializeField] private CanvasGroup cloudFade;
        [SerializeField] private UnityEngine.UI.Graphic cloudFadeGraphic;

        [Header("Travel")]
        [SerializeField] private float smoothTime = 0.5f;
        [SerializeField] private float maxSpeed = 0.3f;
        [SerializeField, Range(0.2f, 0.8f)] private float handoffZoom = 0.5f;
        [SerializeField, Range(0.02f, 0.3f)] private float cloudHalfWidth = 0.1f;

        [Header("Local rise")]
        [SerializeField] private float eyeHeight = 1.7f;
        [SerializeField] private float startDistance = 18f;
        [SerializeField] private float topDistance = 1800f;
        [SerializeField, Range(0f, 0.5f)] private float tiltStartZoom = 0.05f;

        [Header("Planetary approach")]
        [SerializeField] private float handoffAltitudeInRadii = 0.2f;
        [SerializeField] private float spaceDistanceInRadii = 3.2f;
        [SerializeField] private float spaceViewYaw = 65f;
        [SerializeField] private float spaceViewElevation = 50f;
        [SerializeField, Range(0f, 0.9f)] private float swingStart = 0.35f;
        [SerializeField] private float aimSunwardInRadii = 1.5f;

        [Header("Cloud cover")]
        [SerializeField] private Color dayCloudColor = new Color(0.95f, 0.96f, 0.98f);
        [SerializeField] private Color nightCloudColor = new Color(0.06f, 0.07f, 0.1f);

        [Header("Clipping")]
        [SerializeField] private float minimumNear = 0.1f;
        [SerializeField] private float nearFractionOfDistance = 0.05f;
        [SerializeField] private float localFar = 12000f;

        private float velocity;

        public float Zoom { get; private set; }
        public float TargetZoom { get; private set; }
        public bool InPlanetView => Zoom >= handoffZoom;

        public void SetTargetZoom(float value) {
            TargetZoom = Mathf.Clamp01(value);
        }

        public void AddTargetZoom(float delta) {
            SetTargetZoom(TargetZoom + delta);
        }

        public void GoHome() {
            SetTargetZoom(0f);
        }

        public void GoToSpace() {
            SetTargetZoom(1f);
        }

        private void LateUpdate() {
            Zoom = Mathf.SmoothDamp(Zoom, TargetZoom, ref velocity, smoothTime, maxSpeed);

            bool planet = InPlanetView;
            if (localRoot.activeSelf == planet || earthView.gameObject.activeSelf != planet) {
                localRoot.SetActive(!planet);
                earthView.gameObject.SetActive(planet);
                viewCamera.clearFlags = planet ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            }

            if (planet) {
                PlaceInPlanetView();
            } else {
                PlaceInLocalView();
            }

            ApplyCloudFade();
        }

        private void PlaceInLocalView() {
            float t = Zoom / handoffZoom;
            Vector3 focus = houseFocus.position;

            float startPitch = Mathf.Asin(Mathf.Clamp((eyeHeight - focus.y) / startDistance, -1f, 1f)) * Mathf.Rad2Deg;
            float tilt = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(tiltStartZoom, 1f, t));
            float pitch = Mathf.Lerp(startPitch, 90f, tilt);
            float distance = startDistance * Mathf.Pow(topDistance / startDistance, t);
            float pitchRadians = pitch * Mathf.Deg2Rad;

            Vector3 position = focus + distance * new Vector3(0f, Mathf.Sin(pitchRadians), -Mathf.Cos(pitchRadians));
            viewCamera.transform.SetPositionAndRotation(position, Quaternion.Euler(pitch, 0f, 0f));

            viewCamera.nearClipPlane = Mathf.Max(minimumNear, position.y * nearFractionOfDistance);
            viewCamera.farClipPlane = Mathf.Max(localFar, position.y * 20f);
        }

        private void PlaceInPlanetView() {
            float u = Mathf.InverseLerp(handoffZoom, 1f, Zoom);
            float radius = earthView.Radius;

            float startAltitude = handoffAltitudeInRadii * radius;
            float endAltitude = (spaceDistanceInRadii - 1f) * radius;
            float altitude = startAltitude * Mathf.Pow(endAltitude / startAltitude, u);

            Vector3 spaceDirection = Quaternion.Euler(-spaceViewElevation, spaceViewYaw, 0f) * simulation.SunDirection;
            float swing = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(swingStart, 1f, u));
            Vector3 direction = Vector3.Slerp(simulation.HomeDirection, spaceDirection, swing);

            Vector3 position = earthView.Center + direction * (radius + altitude);
            Vector3 aim = earthView.Center + simulation.SunDirection * (aimSunwardInRadii * radius * swing);
            viewCamera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(aim - position, Vector3.up));

            viewCamera.nearClipPlane = Mathf.Max(minimumNear, altitude * nearFractionOfDistance);
            viewCamera.farClipPlane = spaceDistanceInRadii * radius + earthView.SunDistance + radius;
        }

        private void ApplyCloudFade() {
            float distance = Mathf.Clamp01(Mathf.Abs(Zoom - handoffZoom) / cloudHalfWidth);
            cloudFade.alpha = 1f - Mathf.SmoothStep(0f, 1f, distance);

            float daylight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.1f, 0.25f, simulation.HomeSunElevationSine));
            cloudFadeGraphic.color = Color.Lerp(nightCloudColor, dayCloudColor, daylight);
        }
    }
}
