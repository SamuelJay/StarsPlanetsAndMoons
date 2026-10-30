using UnityEngine;
using UnityEngine.InputSystem;

namespace StarsPlanetsAndMoons {
    public class PrototypeControls : MonoBehaviour {
        [SerializeField] private SolarSimulation simulation;
        [SerializeField] private JourneyCamera journey;

        [Header("Actions")]
        [SerializeField] private InputActionReference zoomWheel;
        [SerializeField] private InputActionReference zoomHold;
        [SerializeField] private InputActionReference timeFaster;
        [SerializeField] private InputActionReference timeSlower;
        [SerializeField] private InputActionReference togglePause;
        [SerializeField] private InputActionReference goHome;
        [SerializeField] private InputActionReference goToSpace;

        [Header("Travel")]
        [SerializeField] private float wheelZoomStep = 0.04f;
        [SerializeField] private float holdZoomSpeed = 0.3f;

        [Header("Time (simulated hours per second)")]
        [SerializeField] private float[] timeRates = { -12f, -4f, -1f, 0f, 1f, 4f, 12f };
        [SerializeField] private float defaultResumeRate = 1f;

        private float resumeRate;

        private void Awake() {
            resumeRate = defaultResumeRate;
        }

        private void OnEnable() {
            Enable(zoomWheel);
            Enable(zoomHold);
            Enable(timeFaster);
            Enable(timeSlower);
            Enable(togglePause);
            Enable(goHome);
            Enable(goToSpace);
        }

        private void OnDisable() {
            Disable(zoomWheel);
            Disable(zoomHold);
            Disable(timeFaster);
            Disable(timeSlower);
            Disable(togglePause);
            Disable(goHome);
            Disable(goToSpace);
        }

        private void Update() {
            float wheel = zoomWheel.action.ReadValue<float>();
            if (wheel != 0f) {
                journey.AddTargetZoom(-Mathf.Sign(wheel) * wheelZoomStep);
            }

            float hold = zoomHold.action.ReadValue<float>();
            if (hold != 0f) {
                journey.AddTargetZoom(hold * holdZoomSpeed * Time.unscaledDeltaTime);
            }

            if (goHome.action.WasPressedThisFrame()) GoHome();
            if (goToSpace.action.WasPressedThisFrame()) GoToSpace();
            if (timeFaster.action.WasPressedThisFrame()) FasterTime();
            if (timeSlower.action.WasPressedThisFrame()) SlowerTime();
            if (togglePause.action.WasPressedThisFrame()) TogglePause();
        }

        public void GoHome() {
            journey.GoHome();
        }

        public void GoToSpace() {
            journey.GoToSpace();
        }

        public void FasterTime() {
            float current = simulation.HoursPerSecond;
            foreach (float rate in timeRates) {
                if (rate > current + 0.001f) {
                    SetRate(rate);
                    return;
                }
            }
        }

        public void SlowerTime() {
            float current = simulation.HoursPerSecond;
            for (int i = timeRates.Length - 1; i >= 0; i--) {
                if (timeRates[i] < current - 0.001f) {
                    SetRate(timeRates[i]);
                    return;
                }
            }
        }

        public void TogglePause() {
            if (simulation.HoursPerSecond != 0f) {
                resumeRate = simulation.HoursPerSecond;
                simulation.HoursPerSecond = 0f;
            } else {
                simulation.HoursPerSecond = resumeRate;
            }
        }

        private void SetRate(float rate) {
            simulation.HoursPerSecond = rate;
            if (rate != 0f) {
                resumeRate = rate;
            }
        }

        private static void Enable(InputActionReference reference) {
            reference.action.Enable();
        }

        private static void Disable(InputActionReference reference) {
            reference.action.Disable();
        }
    }
}
