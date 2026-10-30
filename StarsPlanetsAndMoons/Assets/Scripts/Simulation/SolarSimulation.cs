using UnityEngine;

namespace StarsPlanetsAndMoons {
    [DefaultExecutionOrder(-100)]
    public class SolarSimulation : MonoBehaviour {
        [Header("Time")]
        [SerializeField] private double startHours = 10.0;
        [SerializeField] private float startHoursPerSecond;

        [Header("Home location")]
        [SerializeField, Range(-90f, 90f)] private float latitude = 52f;
        [SerializeField, Range(-180f, 180f)] private float longitude;

        public double Hours { get; private set; }
        public float HoursPerSecond { get; set; }
        public float Latitude => latitude;
        public float Longitude => longitude;

        public Vector3 SunDirection => EarthModel.SunDirection;
        public Quaternion EarthRotation => EarthModel.Rotation(Hours);
        public Vector3 HomeSurfaceDirection => EarthModel.SurfaceDirection(latitude, longitude);
        public Vector3 HomeDirection => EarthModel.HomeDirection(latitude, longitude, Hours);
        public float HomeSunElevationSine => EarthModel.SunElevationSine(latitude, longitude, Hours);
        public Vector3 HomeSunDirectionLocal => EarthModel.SunDirectionLocal(latitude, longitude, Hours);
        public float HomeLocalSolarHours => EarthModel.LocalSolarHours(longitude, Hours);
        public bool HomeIsDaytime => HomeSunElevationSine > 0f;

        private void Awake() {
            Hours = startHours;
            HoursPerSecond = startHoursPerSecond;
        }

        private void Update() {
            Hours += HoursPerSecond * Time.deltaTime;
        }

        public void SetHours(double hours) {
            Hours = hours;
        }
    }
}
