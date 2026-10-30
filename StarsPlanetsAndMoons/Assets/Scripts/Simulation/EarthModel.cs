using UnityEngine;

namespace StarsPlanetsAndMoons {
    public static class EarthModel {
        public const double HoursPerDay = 24.0;
        public const double NoonAtPrimeMeridianHours = 12.0;
        public const float DegreesPerHourOfLongitude = 15f;

        public static readonly Vector3 SunDirection = Vector3.forward;

        public static float RotationDegrees(double hours) {
            double days = (hours - NoonAtPrimeMeridianHours) / HoursPerDay;
            return (float)((days - System.Math.Floor(days)) * 360.0);
        }

        public static Quaternion Rotation(double hours) {
            return Quaternion.AngleAxis(-RotationDegrees(hours), Vector3.up);
        }

        public static Vector3 SurfaceDirection(float latitude, float longitude) {
            float lat = latitude * Mathf.Deg2Rad;
            float lon = longitude * Mathf.Deg2Rad;
            return new Vector3(-Mathf.Cos(lat) * Mathf.Sin(lon), Mathf.Sin(lat), Mathf.Cos(lat) * Mathf.Cos(lon));
        }

        public static Quaternion SurfaceFrame(float latitude, float longitude) {
            float lat = latitude * Mathf.Deg2Rad;
            float lon = longitude * Mathf.Deg2Rad;
            Vector3 north = new Vector3(Mathf.Sin(lat) * Mathf.Sin(lon), Mathf.Cos(lat), -Mathf.Sin(lat) * Mathf.Cos(lon));
            return Quaternion.LookRotation(north, SurfaceDirection(latitude, longitude));
        }

        public static Vector3 HomeDirection(float latitude, float longitude, double hours) {
            return Rotation(hours) * SurfaceDirection(latitude, longitude);
        }

        public static float SunElevationSine(float latitude, float longitude, double hours) {
            return Vector3.Dot(HomeDirection(latitude, longitude, hours), SunDirection);
        }

        public static Vector3 SunDirectionLocal(float latitude, float longitude, double hours) {
            Quaternion localToSim = Rotation(hours) * SurfaceFrame(latitude, longitude);
            return Quaternion.Inverse(localToSim) * SunDirection;
        }

        public static float LocalSolarHours(float longitude, double hours) {
            double local = hours + longitude / DegreesPerHourOfLongitude;
            return (float)(local - System.Math.Floor(local / HoursPerDay) * HoursPerDay);
        }
    }
}
