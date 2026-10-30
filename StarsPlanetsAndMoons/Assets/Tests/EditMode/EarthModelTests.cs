using NUnit.Framework;
using UnityEngine;

namespace StarsPlanetsAndMoons.Tests {
    public class EarthModelTests {
        private const float Tolerance = 1e-4f;
        private const float Latitude = 52f;
        private const float Longitude = 0f;

        [Test]
        public void PrimeMeridianFacesSunAtNoonUtc() {
            Assert.AreEqual(1f, EarthModel.SunElevationSine(0f, 0f, 12.0), Tolerance);
            Assert.AreEqual(-1f, EarthModel.SunElevationSine(0f, 0f, 0.0), Tolerance);
        }

        [Test]
        public void EarthRotatesOnceEveryTwentyFourHours() {
            Assert.AreEqual(
                EarthModel.SunElevationSine(Latitude, Longitude, 5.5),
                EarthModel.SunElevationSine(Latitude, Longitude, 5.5 + 24.0),
                Tolerance);
            Assert.AreEqual(180f, Mathf.Abs(EarthModel.RotationDegrees(24.0) - EarthModel.RotationDegrees(12.0)), Tolerance);
        }

        [Test]
        public void HomeIsDayAtLocalNoonAndNightAtLocalMidnight() {
            Assert.Greater(EarthModel.SunElevationSine(Latitude, Longitude, 12.0), 0.5f);
            Assert.Less(EarthModel.SunElevationSine(Latitude, Longitude, 0.0), -0.5f);
        }

        [Test]
        public void AdvancingTwelveHoursMovesHomeFromDayToNightAndBackAgain() {
            double start = 10.0;
            Assert.Greater(EarthModel.SunElevationSine(Latitude, Longitude, start), 0f);
            Assert.Less(EarthModel.SunElevationSine(Latitude, Longitude, start + 12.0), 0f);
            Assert.Greater(EarthModel.SunElevationSine(Latitude, Longitude, start + 12.0 - 12.0), 0f);
        }

        [Test]
        public void EquinoxDaylightLastsTwelveHoursEverywhere() {
            foreach (float latitude in new[] { 0f, 30f, 52f, -45f }) {
                Assert.Greater(EarthModel.SunElevationSine(latitude, 0f, 6.01), 0f);
                Assert.Less(EarthModel.SunElevationSine(latitude, 0f, 5.99), 0f);
                Assert.Greater(EarthModel.SunElevationSine(latitude, 0f, 17.99), 0f);
                Assert.Less(EarthModel.SunElevationSine(latitude, 0f, 18.01), 0f);
            }
        }

        [Test]
        public void SolarNoonMovesEarlierTowardsEastLongitudes() {
            Assert.AreEqual(12f, EarthModel.LocalSolarHours(0f, 12.0), Tolerance);
            Assert.AreEqual(12f, EarthModel.LocalSolarHours(90f, 6.0), Tolerance);
            Assert.AreEqual(1f, EarthModel.LocalSolarHours(-90f, 7.0), Tolerance);
            Assert.AreEqual(1f, EarthModel.LocalSolarHours(0f, 25.0), Tolerance);

            float eastNoon = EarthModel.SunElevationSine(0f, 90f, 6.0);
            float westNoon = EarthModel.SunElevationSine(0f, -90f, 6.0);
            Assert.AreEqual(1f, eastNoon, Tolerance);
            Assert.AreEqual(-1f, westNoon, Tolerance);
        }

        [Test]
        public void LocalSunRisesInEastAndSetsInWest() {
            Vector3 morning = EarthModel.SunDirectionLocal(Latitude, Longitude, 8.0);
            Vector3 noon = EarthModel.SunDirectionLocal(Latitude, Longitude, 12.0);
            Vector3 evening = EarthModel.SunDirectionLocal(Latitude, Longitude, 16.0);

            Assert.Greater(morning.x, 0f);
            Assert.Less(evening.x, 0f);
            Assert.AreEqual(0f, noon.x, Tolerance);
            Assert.Less(noon.z, 0f, "Sun is south of the zenith at northern midlatitudes.");
        }

        [Test]
        public void LocalSunElevationEqualsPlanetaryIllumination() {
            for (double hours = -30.0; hours < 60.0; hours += 1.7) {
                Vector3 local = EarthModel.SunDirectionLocal(Latitude, 13f, hours);
                Assert.AreEqual(EarthModel.SunElevationSine(Latitude, 13f, hours), local.y, Tolerance);
                Assert.AreEqual(1f, local.magnitude, Tolerance);
            }
        }

        [Test]
        public void HomeStaysFixedOnTheRotatingSurface() {
            Vector3 surface = EarthModel.SurfaceDirection(Latitude, Longitude);
            for (double hours = 0.0; hours < 48.0; hours += 3.1) {
                Vector3 inSpace = EarthModel.HomeDirection(Latitude, Longitude, hours);
                Vector3 backOnSurface = Quaternion.Inverse(EarthModel.Rotation(hours)) * inSpace;
                Assert.AreEqual(0f, Vector3.Distance(surface, backOnSurface), Tolerance);
            }
        }

        [Test]
        public void EarthRotatesEastwardsSoTheSurfaceMovesTowardsEast() {
            Vector3 frameEast = EarthModel.SurfaceFrame(0f, 0f) * Vector3.right;
            Vector3 before = EarthModel.HomeDirection(0f, 0f, 12.0);
            Vector3 after = EarthModel.HomeDirection(0f, 0f, 13.0);
            Assert.Greater(Vector3.Dot(after - before, frameEast), 0f);
        }

        [Test]
        public void SurfaceFrameIsOrthonormalWithUpAwayFromCentre() {
            Quaternion frame = EarthModel.SurfaceFrame(Latitude, 40f);
            Assert.AreEqual(0f, Vector3.Distance(frame * Vector3.up, EarthModel.SurfaceDirection(Latitude, 40f)), Tolerance);
            Assert.AreEqual(0f, Vector3.Dot(frame * Vector3.forward, frame * Vector3.up), Tolerance);
            Assert.Greater((frame * Vector3.forward).y, 0f, "North points towards the pole.");
        }

        [Test]
        public void ReversingTimeRestoresTheSameState() {
            double start = 10.0;
            float before = EarthModel.SunElevationSine(Latitude, Longitude, start);
            EarthModel.SunElevationSine(Latitude, Longitude, start + 37.0);
            float after = EarthModel.SunElevationSine(Latitude, Longitude, start + 37.0 - 37.0);
            Assert.AreEqual(before, after, Tolerance);
        }
    }
}
