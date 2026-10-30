using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace StarsPlanetsAndMoons.Editor {
    public static class HomeEarthNightSceneBuilder {
        private const string ScenePath = "Assets/Scenes/HomeEarthNight.unity";
        private const string MaterialFolder = "Assets/Materials";
        private const string MeshFolder = "Assets/Meshes";
        private const string InputPath = "Assets/Input/Journey.inputactions";

        private const float HomeLatitude = 52f;
        private const float HomeLongitude = 0f;
        private const float EarthRadius = 100f;
        private const float SunDistanceInRadii = 4f;
        private static readonly Vector3 PlanetRootPosition = new Vector3(0f, 20000f, 0f);

        private static readonly List<GameObject> nightGroups = new List<GameObject>();
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private static Mesh gableMesh;

        [MenuItem("Stars, Planets and Moons/Rebuild Home-Earth-Night Scene")]
        public static void Build() {
            nightGroups.Clear();
            materials.Clear();

            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory(MeshFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(InputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));

            PlaceholderTextures.Generate(EarthModel.SurfaceDirection(HomeLatitude, HomeLongitude));
            gableMesh = BuildGableMesh();
            InputActionAsset inputActions = BuildInputActions();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SolarSimulation simulation = BuildSimulation();
            Camera camera = BuildCamera();
            (GameObject localRoot, LocalView localView, Transform houseFocus) = BuildLocal(simulation, camera);
            (GameObject planetRoot, EarthView earthView) = BuildPlanet(simulation, camera);
            (Canvas canvas, CanvasGroup cloudFade, Image cloudFadeImage, Text status, PrototypeControls controls, JourneyCamera journey) =
                BuildJourneyAndHud(simulation, camera, localRoot, earthView, houseFocus, inputActions);

            planetRoot.SetActive(false);

            RenderSettings.skybox = Mat("SkyProcedural", Color.white, shader: "Skybox/Procedural");
            RenderSettings.sun = localView.GetComponentInChildren<Light>();
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.reflectionIntensity = 0.2f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0003f;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static SolarSimulation BuildSimulation() {
            GameObject go = new GameObject("Simulation");
            SolarSimulation simulation = go.AddComponent<SolarSimulation>();
            SerializedObject so = new SerializedObject(simulation);
            so.FindProperty("latitude").floatValue = HomeLatitude;
            so.FindProperty("longitude").floatValue = HomeLongitude;
            so.ApplyModifiedPropertiesWithoutUndo();
            return simulation;
        }

        private static Camera BuildCamera() {
            GameObject go = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 12000f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = Color.black;
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>();
            return camera;
        }

        private static (GameObject, LocalView, Transform) BuildLocal(SolarSimulation simulation, Camera camera) {
            GameObject root = new GameObject("LocalRoot");
            LocalView view = root.AddComponent<LocalView>();

            GameObject sunGo = new GameObject("LocalSun");
            sunGo.transform.SetParent(root.transform, false);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.intensity = 1.3f;
            sunGo.transform.rotation = Quaternion.Euler(50f, 30f, 0f);

            GameObject focus = new GameObject("HouseFocus");
            focus.transform.SetParent(root.transform, false);
            focus.transform.localPosition = new Vector3(0f, 3f, 0f);

            Transform environment = new GameObject("Environment").transform;
            environment.SetParent(root.transform, false);

            BuildGroundAndRoads(environment);
            BuildHome(environment);
            BuildNeighbourhood(environment);
            BuildTrees(environment);
            BuildClouds(environment);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("simulation").objectReferenceValue = simulation;
            so.FindProperty("viewCamera").objectReferenceValue = camera;
            so.FindProperty("sunLight").objectReferenceValue = sun;
            SerializedProperty array = so.FindProperty("nightObjects");
            array.arraySize = nightGroups.Count;
            for (int i = 0; i < nightGroups.Count; i++) {
                array.GetArrayElementAtIndex(i).objectReferenceValue = nightGroups[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            return (root, view, focus.transform);
        }

        private static void BuildGroundAndRoads(Transform parent) {
            Transform group = Group("GroundAndRoads", parent);

            Prim("Ground", PrimitiveType.Cylinder, group, new Vector3(0f, -0.1f, 0f), new Vector3(40000f, 0.1f, 40000f), "Grass", new Color(0.3f, 0.5f, 0.2f));
            Prim("Road", PrimitiveType.Cube, group, new Vector3(0f, 0.02f, -13.5f), new Vector3(400f, 0.04f, 6.5f), "Asphalt", new Color(0.16f, 0.16f, 0.18f));
            Prim("CrossRoad", PrimitiveType.Cube, group, new Vector3(72f, 0.02f, 90f), new Vector3(6.5f, 0.04f, 210f), "Asphalt", new Color(0.16f, 0.16f, 0.18f));
            Prim("PavementNorth", PrimitiveType.Cube, group, new Vector3(0f, 0.04f, -9.5f), new Vector3(400f, 0.08f, 1.5f), "Pavement", new Color(0.62f, 0.62f, 0.6f));
            Prim("PavementSouth", PrimitiveType.Cube, group, new Vector3(0f, 0.04f, -17.5f), new Vector3(400f, 0.08f, 1.5f), "Pavement", new Color(0.62f, 0.62f, 0.6f));

            Transform dashes = Group("RoadMarkings", group);
            for (int i = -24; i <= 24; i++) {
                Prim("Dash", PrimitiveType.Cube, dashes, new Vector3(i * 8f, 0.05f, -13.5f), new Vector3(3.5f, 0.02f, 0.18f), "RoadPaint", new Color(0.9f, 0.9f, 0.85f));
            }
        }

        private static void BuildHome(Transform parent) {
            Transform home = Group("Home", parent);

            BuildHouse(home, "House", Vector3.zero, 0f, 1f, new Color(0.9f, 0.84f, 0.7f), new Color(0.55f, 0.22f, 0.16f), true);

            Prim("GardenPath", PrimitiveType.Cube, home, new Vector3(0f, 0.04f, -6.4f), new Vector3(1.4f, 0.08f, 4.8f), "Pavement", new Color(0.62f, 0.62f, 0.6f));
            Prim("Driveway", PrimitiveType.Cube, home, new Vector3(6.8f, 0.03f, -6.2f), new Vector3(3.4f, 0.06f, 5.4f), "Asphalt", new Color(0.3f, 0.3f, 0.32f));
            Prim("HedgeWest", PrimitiveType.Cube, home, new Vector3(-5.9f, 0.45f, -8.2f), new Vector3(9.6f, 0.9f, 0.8f), "Hedge", new Color(0.12f, 0.35f, 0.12f));
            Prim("HedgeEast", PrimitiveType.Cube, home, new Vector3(10.2f, 0.45f, -8.2f), new Vector3(4.8f, 0.9f, 0.8f), "Hedge", new Color(0.12f, 0.35f, 0.12f));

            BuildCar(home, "FamilyCar", new Vector3(6.8f, 0f, -6.2f), 0f, new Color(0.75f, 0.1f, 0.1f));
            BuildCar(home, "StreetCar", new Vector3(-20f, 0f, -11.8f), 90f, new Color(0.15f, 0.3f, 0.7f));

            Prim("MailboxPost", PrimitiveType.Cube, home, new Vector3(-1.4f, 0.55f, -9.1f), new Vector3(0.1f, 1.1f, 0.1f), "Wood", new Color(0.35f, 0.22f, 0.12f));
            Prim("Mailbox", PrimitiveType.Cube, home, new Vector3(-1.4f, 1.2f, -9.1f), new Vector3(0.35f, 0.3f, 0.5f), "MailboxRed", new Color(0.7f, 0.1f, 0.08f));

            BuildLamp(home, new Vector3(-7f, 0f, -9.2f));
            BuildTree(home, new Vector3(-9f, 0f, 3f), 1.2f, 0);
            BuildTree(home, new Vector3(9.5f, 0f, 5f), 1f, 1);
            BuildTree(home, new Vector3(-13f, 0f, -4f), 0.9f, 2);
        }

        private static void BuildNeighbourhood(Transform parent) {
            Transform group = Group("Neighbourhood", parent);
            Color[] walls = {
                new Color(0.85f, 0.75f, 0.6f), new Color(0.7f, 0.8f, 0.85f), new Color(0.82f, 0.62f, 0.5f),
                new Color(0.9f, 0.9f, 0.82f), new Color(0.7f, 0.55f, 0.45f)
            };
            Color[] roofs = { new Color(0.35f, 0.18f, 0.15f), new Color(0.25f, 0.27f, 0.32f), new Color(0.5f, 0.25f, 0.18f) };

            float[] northX = { -96f, -48f, -24f, 24f, 48f, 96f, 120f };
            for (int i = 0; i < northX.Length; i++) {
                BuildHouse(group, "NorthHouse" + i, new Vector3(northX[i], 0f, 0f), 0f, 0.9f + 0.05f * (i % 3), walls[i % walls.Length], roofs[i % roofs.Length], false);
            }

            float[] southX = { -72f, -36f, -12f, 12f, 36f, 60f, 100f };
            for (int i = 0; i < southX.Length; i++) {
                BuildHouse(group, "SouthHouse" + i, new Vector3(southX[i], 0f, -34f), 180f, 0.9f + 0.05f * (i % 3), walls[(i + 2) % walls.Length], roofs[(i + 1) % roofs.Length], false);
            }

            for (int i = 0; i < 6; i++) {
                BuildHouse(group, "CrossRoadHouse" + i, new Vector3(i % 2 == 0 ? 50f : 94f, 0f, 30f + (i / 2) * 36f), i % 2 == 0 ? 90f : -90f, 0.95f, walls[(i + 1) % walls.Length], roofs[i % roofs.Length], false);
            }
        }

        private static void BuildTrees(Transform parent) {
            Transform group = Group("Trees", parent);
            Random random = new Random(12345);
            int placed = 0;
            while (placed < 260) {
                float x = (float)(random.NextDouble() * 2.0 - 1.0) * 1800f;
                float z = (float)(random.NextDouble() * 2.0 - 1.0) * 1800f;
                float nearness = Mathf.Max(Mathf.Abs(x) / 160f, Mathf.Abs(z + 15f) / 120f);
                if (random.NextDouble() > Mathf.Lerp(0.9f, 0.08f, Mathf.Clamp01(nearness / 4f))) continue;

                bool onRoad = Mathf.Abs(z + 13.5f) < 14f && Mathf.Abs(x) < 220f;
                bool onCross = Mathf.Abs(x - 72f) < 12f && z > -20f && z < 200f;
                bool inTown = Mathf.Abs(z + 14f) < 50f && Mathf.Abs(x) < 220f;
                bool nearCross = Mathf.Abs(x - 72f) < 60f && z > -20f && z < 200f;
                if (onRoad || onCross || inTown || nearCross) continue;

                BuildTree(group, new Vector3(x, 0f, z), 0.8f + (float)random.NextDouble() * 1.6f, random.Next(3));
                placed++;
            }
        }

        private static void BuildClouds(Transform parent) {
            Transform group = Group("Clouds", parent);
            Random random = new Random(777);
            for (int i = 0; i < 90; i++) {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = 100f + Mathf.Sqrt((float)random.NextDouble()) * 4500f;
                float altitude = 1500f + (float)random.NextDouble() * 800f;
                float size = 150f + (float)random.NextDouble() * 350f;
                Prim("Cloud", PrimitiveType.Sphere, group,
                    new Vector3(Mathf.Cos(angle) * distance, altitude, Mathf.Sin(angle) * distance),
                    new Vector3(size, size * 0.35f, size * 0.8f), "Cloud", new Color(0.97f, 0.97f, 1f), smoothness: 0f, castShadows: false);
            }
        }

        private static void BuildHouse(Transform parent, string name, Vector3 position, float yaw, float scale, Color wall, Color roof, bool detailed) {
            Transform house = Group(name, parent);
            house.localPosition = position;
            house.localRotation = Quaternion.Euler(0f, yaw, 0f);
            house.localScale = Vector3.one * scale;

            Prim("Walls", PrimitiveType.Cube, house, new Vector3(0f, 2.5f, 0f), new Vector3(10f, 5f, 8f), "Wall_" + ColorKey(wall), wall);
            GameObject roofGo = new GameObject("Roof");
            roofGo.transform.SetParent(house, false);
            roofGo.transform.localPosition = new Vector3(0f, 5f, 0f);
            roofGo.AddComponent<MeshFilter>().sharedMesh = gableMesh;
            roofGo.AddComponent<MeshRenderer>().sharedMaterial = Mat("Roof_" + ColorKey(roof), roof);
            Prim("Door", PrimitiveType.Cube, house, new Vector3(0f, 1.1f, -4.05f), new Vector3(1.2f, 2.2f, 0.12f), "Door", new Color(0.3f, 0.17f, 0.1f));

            GameObject glow = new GameObject("NightWindowGlow");
            glow.transform.SetParent(house, false);
            foreach (float x in new[] { -3.2f, 3.2f }) {
                Prim("Window", PrimitiveType.Cube, house, new Vector3(x, 2.6f, -4.03f), new Vector3(1.8f, 1.5f, 0.1f), "Glass", new Color(0.35f, 0.5f, 0.6f), smoothness: 0.8f);
                Prim("WindowGlow", PrimitiveType.Cube, glow.transform, new Vector3(x, 2.6f, -4.1f), new Vector3(1.7f, 1.4f, 0.05f), "WarmLight", new Color(1f, 0.78f, 0.35f), shader: "Universal Render Pipeline/Unlit");
            }
            Prim("WindowSide", PrimitiveType.Cube, house, new Vector3(5.03f, 2.6f, 0f), new Vector3(0.1f, 1.5f, 1.8f), "Glass", new Color(0.35f, 0.5f, 0.6f), smoothness: 0.8f);
            Prim("WindowSideGlow", PrimitiveType.Cube, glow.transform, new Vector3(5.1f, 2.6f, 0f), new Vector3(0.05f, 1.4f, 1.7f), "WarmLight", new Color(1f, 0.78f, 0.35f), shader: "Universal Render Pipeline/Unlit");
            nightGroups.Add(glow);

            if (!detailed) return;

            Prim("Chimney", PrimitiveType.Cube, house, new Vector3(-2.8f, 7.4f, 1.2f), new Vector3(0.9f, 2.4f, 0.9f), "Brick", new Color(0.55f, 0.28f, 0.22f));
            Prim("DoorStep", PrimitiveType.Cube, house, new Vector3(0f, 0.08f, -4.6f), new Vector3(2.2f, 0.16f, 1f), "Pavement", new Color(0.62f, 0.62f, 0.6f));
            Prim("PorchRoof", PrimitiveType.Cube, house, new Vector3(0f, 2.8f, -4.7f), new Vector3(2.8f, 0.15f, 1.4f), "Wood", new Color(0.35f, 0.22f, 0.12f));

            GameObject porch = new GameObject("PorchLight");
            porch.transform.SetParent(glow.transform, false);
            porch.transform.localPosition = new Vector3(0f, 2.4f, -5.2f);
            Light porchLight = porch.AddComponent<Light>();
            porchLight.type = LightType.Point;
            porchLight.color = new Color(1f, 0.78f, 0.45f);
            porchLight.range = 9f;
            porchLight.intensity = 6f;
        }

        private static void BuildCar(Transform parent, string name, Vector3 position, float yaw, Color paint) {
            Transform car = Group(name, parent);
            car.localPosition = position;
            car.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Prim("Body", PrimitiveType.Cube, car, new Vector3(0f, 0.65f, 0f), new Vector3(1.8f, 0.7f, 4.2f), "Paint_" + ColorKey(paint), paint, smoothness: 0.6f);
            Prim("Cabin", PrimitiveType.Cube, car, new Vector3(0f, 1.2f, -0.2f), new Vector3(1.55f, 0.6f, 2.2f), "CarGlass", new Color(0.12f, 0.16f, 0.2f), smoothness: 0.8f);
            foreach (float x in new[] { -0.9f, 0.9f }) {
                foreach (float z in new[] { -1.3f, 1.3f }) {
                    Prim("Wheel", PrimitiveType.Cylinder, car, new Vector3(x, 0.32f, z), new Vector3(0.64f, 0.1f, 0.64f), "Tyre", new Color(0.05f, 0.05f, 0.05f), euler: new Vector3(0f, 0f, 90f));
                }
            }
        }

        private static void BuildLamp(Transform parent, Vector3 position) {
            Transform lamp = Group("StreetLamp", parent);
            lamp.localPosition = position;
            Prim("Pole", PrimitiveType.Cylinder, lamp, new Vector3(0f, 2.6f, 0f), new Vector3(0.14f, 2.6f, 0.14f), "Metal", new Color(0.2f, 0.22f, 0.24f));
            Prim("Arm", PrimitiveType.Cube, lamp, new Vector3(0f, 5.15f, 0.4f), new Vector3(0.1f, 0.1f, 0.9f), "Metal", new Color(0.2f, 0.22f, 0.24f));
            Prim("Head", PrimitiveType.Cube, lamp, new Vector3(0f, 5.1f, 0.8f), new Vector3(0.4f, 0.12f, 0.6f), "Metal", new Color(0.2f, 0.22f, 0.24f));

            GameObject glow = new GameObject("NightLampLight");
            glow.transform.SetParent(lamp, false);
            glow.transform.localPosition = new Vector3(0f, 4.95f, 0.8f);
            Prim("Bulb", PrimitiveType.Sphere, glow.transform, Vector3.zero, Vector3.one * 0.35f, "WarmLight", new Color(1f, 0.78f, 0.35f), shader: "Universal Render Pipeline/Unlit");
            Light light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.5f);
            light.range = 18f;
            light.intensity = 25f;
            nightGroups.Add(glow);
        }

        private static void BuildTree(Transform parent, Vector3 position, float scale, int variety) {
            Color[] leaves = { new Color(0.12f, 0.38f, 0.14f), new Color(0.2f, 0.45f, 0.15f), new Color(0.1f, 0.3f, 0.18f) };
            Transform tree = Group("Tree", parent);
            tree.localPosition = position;
            tree.localScale = Vector3.one * scale;
            Prim("Trunk", PrimitiveType.Cylinder, tree, new Vector3(0f, 1.3f, 0f), new Vector3(0.45f, 1.3f, 0.45f), "Bark", new Color(0.3f, 0.2f, 0.12f));
            Prim("Canopy", PrimitiveType.Sphere, tree, new Vector3(0f, 4f, 0f), new Vector3(3.8f, 3.8f, 3.8f), "Leaves" + variety, leaves[variety % leaves.Length]);
        }

        private static (GameObject, EarthView) BuildPlanet(SolarSimulation simulation, Camera camera) {
            GameObject root = new GameObject("PlanetRoot");
            root.transform.position = PlanetRootPosition;
            EarthView view = root.AddComponent<EarthView>();

            GameObject sunGo = new GameObject("SpaceSun");
            sunGo.transform.SetParent(root.transform, false);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.6f;
            sun.color = new Color(1f, 0.97f, 0.9f);
            sun.shadows = LightShadows.None;

            Transform earth = Group("Earth", root.transform);
            Material surface = Mat("EarthSurface", Color.white, smoothness: 0.15f);
            surface.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(PlaceholderTextures.EarthPath));
            EditorUtility.SetDirty(surface);

            Material clouds = Mat("EarthClouds", Color.white, smoothness: 0f);
            clouds.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(PlaceholderTextures.CloudsPath));
            MakeTransparent(clouds);

            Prim("Globe", PrimitiveType.Sphere, earth, Vector3.zero, Vector3.one * EarthRadius * 2f, "EarthSurface", Color.white);
            Prim("CloudShell", PrimitiveType.Sphere, earth, Vector3.zero, Vector3.one * (EarthRadius * 2f + 3f), "EarthClouds", Color.white, castShadows: false);

            Transform marker = Group("HomeMarker", earth);
            Prim("Stalk", PrimitiveType.Cylinder, marker, new Vector3(0f, 2f, 0f), new Vector3(1.6f, 2f, 1.6f), "MarkerRed", new Color(1f, 0.2f, 0.1f), shader: "Universal Render Pipeline/Unlit");
            Prim("Head", PrimitiveType.Sphere, marker, new Vector3(0f, 4f, 0f), Vector3.one * 7f, "MarkerRed", new Color(1f, 0.2f, 0.1f), shader: "Universal Render Pipeline/Unlit");

            Transform sunRig = Group("SunRig", root.transform);
            float sunDistance = EarthRadius * SunDistanceInRadii;
            Prim("SunBody", PrimitiveType.Sphere, sunRig, new Vector3(0f, 0f, sunDistance), Vector3.one * EarthRadius * 1.2f, "SunBody", new Color(1f, 0.85f, 0.3f), shader: "Universal Render Pipeline/Unlit", castShadows: false);
            Transform beams = Group("SunBeams", sunRig);
            float beamStart = EarthRadius * 1.3f;
            float beamLength = sunDistance - EarthRadius * 1.6f - beamStart;
            Vector2[] offsets = { Vector2.zero, new Vector2(0.55f, 0f), new Vector2(-0.55f, 0f), new Vector2(0f, 0.55f), new Vector2(0f, -0.55f) };
            foreach (Vector2 offset in offsets) {
                Prim("Beam", PrimitiveType.Cylinder, beams, new Vector3(offset.x * EarthRadius, offset.y * EarthRadius, beamStart + beamLength * 0.5f),
                    new Vector3(1.6f, beamLength * 0.5f, 1.6f), "SunBeam", new Color(1f, 0.9f, 0.5f), shader: "Universal Render Pipeline/Unlit", euler: new Vector3(90f, 0f, 0f), castShadows: false);
            }

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("simulation").objectReferenceValue = simulation;
            so.FindProperty("viewCamera").objectReferenceValue = camera;
            so.FindProperty("earth").objectReferenceValue = earth;
            so.FindProperty("homeMarker").objectReferenceValue = marker;
            so.FindProperty("sunRig").objectReferenceValue = sunRig;
            so.FindProperty("sunLight").objectReferenceValue = sun;
            so.FindProperty("radius").floatValue = EarthRadius;
            so.FindProperty("sunDistanceInRadii").floatValue = SunDistanceInRadii;
            so.ApplyModifiedPropertiesWithoutUndo();

            return (root, view);
        }

        private static (Canvas, CanvasGroup, Image, Text, PrototypeControls, JourneyCamera) BuildJourneyAndHud(
            SolarSimulation simulation, Camera camera, GameObject localRoot, EarthView earthView, Transform houseFocus, InputActionAsset inputActions) {
            GameObject canvasGo = new GameObject("HUD");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject fadeGo = new GameObject("CloudFade");
            fadeGo.transform.SetParent(canvasGo.transform, false);
            Image fadeImage = fadeGo.AddComponent<Image>();
            fadeImage.color = Color.white;
            fadeImage.raycastTarget = false;
            CanvasGroup fadeGroup = fadeGo.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
            Stretch(fadeGo.GetComponent<RectTransform>());

            Text status = MakeText("Status", canvasGo.transform, 30, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -24f), new Vector2(900f, 140f));
            Text help = MakeText("Help", canvasGo.transform, 22, TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 20f), new Vector2(1400f, 120f));
            help.text = "Mouse wheel / E,Q / PgUp,PgDn: travel out to space and back down\n" +
                        "Home or H: go home    End or X: go to space\n" +
                        "Right,D / Left,A: faster / slower (negative = reverse) time    Space: pause";

            GameObject journeyGo = new GameObject("Journey");
            JourneyCamera journey = journeyGo.AddComponent<JourneyCamera>();
            PrototypeControls controls = journeyGo.AddComponent<PrototypeControls>();
            ControlsHud hud = canvasGo.AddComponent<ControlsHud>();

            SerializedObject js = new SerializedObject(journey);
            js.FindProperty("simulation").objectReferenceValue = simulation;
            js.FindProperty("viewCamera").objectReferenceValue = camera;
            js.FindProperty("localRoot").objectReferenceValue = localRoot;
            js.FindProperty("earthView").objectReferenceValue = earthView;
            js.FindProperty("houseFocus").objectReferenceValue = houseFocus;
            js.FindProperty("cloudFade").objectReferenceValue = fadeGroup;
            js.FindProperty("cloudFadeGraphic").objectReferenceValue = fadeImage;
            js.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject cs = new SerializedObject(controls);
            cs.FindProperty("simulation").objectReferenceValue = simulation;
            cs.FindProperty("journey").objectReferenceValue = journey;
            cs.FindProperty("zoomWheel").objectReferenceValue = ActionReference("ZoomWheel");
            cs.FindProperty("zoomHold").objectReferenceValue = ActionReference("ZoomHold");
            cs.FindProperty("timeFaster").objectReferenceValue = ActionReference("TimeFaster");
            cs.FindProperty("timeSlower").objectReferenceValue = ActionReference("TimeSlower");
            cs.FindProperty("togglePause").objectReferenceValue = ActionReference("TogglePause");
            cs.FindProperty("goHome").objectReferenceValue = ActionReference("GoHome");
            cs.FindProperty("goToSpace").objectReferenceValue = ActionReference("GoToSpace");
            cs.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject hs = new SerializedObject(hud);
            hs.FindProperty("simulation").objectReferenceValue = simulation;
            hs.FindProperty("journey").objectReferenceValue = journey;
            hs.FindProperty("statusText").objectReferenceValue = status;
            hs.ApplyModifiedPropertiesWithoutUndo();

            GameObject buttonsGo = new GameObject("Buttons", typeof(RectTransform));
            buttonsGo.transform.SetParent(canvasGo.transform, false);
            Transform buttons = buttonsGo.transform;
            RectTransform buttonsRect = buttonsGo.GetComponent<RectTransform>();
            buttonsRect.anchorMin = buttonsRect.anchorMax = new Vector2(1f, 0f);
            buttonsRect.pivot = new Vector2(1f, 0f);
            buttonsRect.anchoredPosition = new Vector2(-30f, 20f);
            buttonsRect.sizeDelta = new Vector2(1000f, 60f);
            HorizontalLayoutGroup layout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.LowerRight;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            MakeButton("Time <<", buttons, controls.SlowerTime);
            MakeButton("Pause / Play", buttons, controls.TogglePause);
            MakeButton("Time >>", buttons, controls.FasterTime);
            MakeButton("Go Home", buttons, controls.GoHome);
            MakeButton("Go To Space", buttons, controls.GoToSpace);

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();

            return (canvas, fadeGroup, fadeImage, status, controls, journey);
        }

        private static InputActionAsset BuildInputActions() {
            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "Journey";
            InputActionMap map = asset.AddActionMap("Journey");

            map.AddAction("ZoomWheel", InputActionType.Value, "<Mouse>/scroll/y", expectedControlLayout: "Axis");

            InputAction hold = map.AddAction("ZoomHold", InputActionType.Value, expectedControlLayout: "Axis");
            hold.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/pageDown").With("Positive", "<Keyboard>/pageUp");
            hold.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/q").With("Positive", "<Keyboard>/e");

            AddButton(map, "TimeFaster", "<Keyboard>/rightArrow", "<Keyboard>/d");
            AddButton(map, "TimeSlower", "<Keyboard>/leftArrow", "<Keyboard>/a");
            AddButton(map, "TogglePause", "<Keyboard>/space");
            AddButton(map, "GoHome", "<Keyboard>/home", "<Keyboard>/h");
            AddButton(map, "GoToSpace", "<Keyboard>/end", "<Keyboard>/x");

            File.WriteAllText(InputPath, asset.ToJson());
            Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(InputPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
        }

        private static void AddButton(InputActionMap map, string name, params string[] bindings) {
            InputAction action = map.AddAction(name, InputActionType.Button);
            foreach (string binding in bindings) {
                action.AddBinding(binding);
            }
        }

        private static InputActionReference ActionReference(string actionName) {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(InputPath)) {
                if (asset is InputActionReference reference && reference.name == "Journey/" + actionName) {
                    return reference;
                }
            }
            throw new System.InvalidOperationException("Missing input action reference " + actionName);
        }

        private static Text MakeText(string name, Transform parent, int size, TextAnchor alignment, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 dimensions) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            go.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.85f);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            return text;
        }

        private static void MakeButton(string label, Transform parent, UnityAction action) {
            GameObject go = new GameObject(label);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.1f, 0.12f, 0.2f, 0.8f);
            Button button = go.AddComponent<Button>();
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 56f);
            UnityEventTools.AddPersistentListener(button.onClick, action);

            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            Text text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            Stretch(textGo.GetComponent<RectTransform>());
        }

        private static void Stretch(RectTransform rect) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Transform Group(string name, Transform parent) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Prim(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, string materialName, Color color,
            float smoothness = 0.1f, string shader = "Universal Render Pipeline/Lit", Vector3 euler = default, bool castShadows = true) {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Mat(materialName, color, smoothness, shader);
            if (!castShadows) renderer.shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        private static Material Mat(string name, Color color, float smoothness = 0.1f, string shader = "Universal Render Pipeline/Lit") {
            if (materials.TryGetValue(name, out Material cached)) return cached;

            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader found = Shader.Find(shader);
            if (found == null) throw new System.InvalidOperationException("Shader not found: " + shader);
            if (material == null) {
                material = new Material(found);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = found;
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(material);
            materials[name] = material;
            return material;
        }

        private static void MakeTransparent(Material material) {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetShaderPassEnabled("ShadowCaster", false);
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
        }

        private static string ColorKey(Color color) {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        private static Mesh BuildGableMesh() {
            const float width = 10.8f;
            const float length = 8.8f;
            const float rise = 2.8f;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            Vector3 a = new Vector3(-width / 2f, 0f, -length / 2f);
            Vector3 b = new Vector3(width / 2f, 0f, -length / 2f);
            Vector3 c = new Vector3(0f, rise, -length / 2f);
            Vector3 d = new Vector3(-width / 2f, 0f, length / 2f);
            Vector3 e = new Vector3(width / 2f, 0f, length / 2f);
            Vector3 f = new Vector3(0f, rise, length / 2f);

            Vector3 westNormal = new Vector3(-rise, width / 2f, 0f).normalized;
            Vector3 eastNormal = new Vector3(rise, width / 2f, 0f).normalized;

            AddFace(vertices, triangles, normals, Vector3.back, a, b, c);
            AddFace(vertices, triangles, normals, Vector3.forward, d, e, f);
            AddFace(vertices, triangles, normals, westNormal, a, d, f, c);
            AddFace(vertices, triangles, normals, eastNormal, b, e, f, c);
            AddFace(vertices, triangles, normals, Vector3.down, a, b, e, d);

            Mesh mesh = new Mesh { name = "GableRoof" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();

            string path = MeshFolder + "/GableRoof.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }

        private static void AddFace(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, Vector3 outward, params Vector3[] corners) {
            int start = vertices.Count;
            vertices.AddRange(corners);
            for (int i = 0; i < corners.Length; i++) normals.Add(outward);

            for (int i = 1; i < corners.Length - 1; i++) {
                Vector3 normal = Vector3.Cross(corners[i] - corners[0], corners[i + 1] - corners[0]);
                if (Vector3.Dot(normal, outward) >= 0f) {
                    triangles.Add(start);
                    triangles.Add(start + i);
                    triangles.Add(start + i + 1);
                } else {
                    triangles.Add(start);
                    triangles.Add(start + i + 1);
                    triangles.Add(start + i);
                }
            }
        }
    }
}
