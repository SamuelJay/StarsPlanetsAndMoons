using UnityEngine;
using UnityEngine.UI;

namespace StarsPlanetsAndMoons {
    public class ControlsHud : MonoBehaviour {
        [SerializeField] private SolarSimulation simulation;
        [SerializeField] private JourneyCamera journey;
        [SerializeField] private Text statusText;
        [SerializeField] private float atHomeBelow = 0.005f;
        [SerializeField] private float inSpaceAbove = 0.995f;

        private void Update() {
            float solarHours = simulation.HomeLocalSolarHours;
            int hour = Mathf.FloorToInt(solarHours);
            int minute = Mathf.FloorToInt((solarHours - hour) * 60f);
            string daylight = simulation.HomeIsDaytime ? "Day" : "Night";

            float rate = simulation.HoursPerSecond;
            string time = rate == 0f ? "paused" : $"{rate:+0.#;-0.#} hours/second";

            float zoom = journey.Zoom;
            string view = zoom < atHomeBelow ? "At home" : zoom > inSpaceAbove ? "In space" : "Travelling";

            statusText.text = $"Home local solar time  {hour:00}:{minute:00}  ({daylight})\nTime: {time}\nView: {view}";
        }
    }
}
