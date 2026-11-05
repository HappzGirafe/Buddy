using UnityEngine;
using UnityEngine.InputSystem;

namespace Glowfade
{
    /// Battery flashlight. F (or the gamepad's north button) toggles it; the battery drains while it is on
    /// and the beam dims and flickers when it is nearly flat.
    [RequireComponent(typeof(Light))]
    public class Flashlight : MonoBehaviour
    {
        public float batterySeconds = 240f;
        [Range(0f, 1f)] public float charge = 1f;
        [Tooltip("URP light intensity. Light fades with distance squared and the rooms are dark, so a beam that reads "
               + "on the floor a few metres ahead needs a big number (6 lit nothing you could see).")]
        public float fullIntensity = 100f;

        Light beam;

        public bool IsOn => beam != null && beam.enabled;

        void Awake() => beam = GetComponent<Light>();

        void Update()
        {
            bool toggle = (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);
            if (toggle && Time.timeScale > 0f) beam.enabled = beam.enabled ? false : charge > 0f; // not while paused

            if (beam.enabled)
            {
                charge = Mathf.Max(0f, charge - Time.deltaTime / batterySeconds);
                if (charge <= 0f) beam.enabled = false;
            }

            float level = 1f;
            if (charge < 0.15f) level = Mathf.Lerp(0.3f, 1f, charge / 0.15f) * (Random.value > 0.03f ? 1f : 0.2f);
            beam.intensity = fullIntensity * level;
        }

        public void Recharge(float amount) => charge = Mathf.Clamp01(charge + amount);
    }
}
