using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Glowfade
{
    /// Applies the scene's flat ambient colour on start, and lets L toggle bright "work lights"
    /// (grey ambient, a soft light along the view, no fog) so the dark greybox is easy to inspect.
    public class WorkLights : MonoBehaviour
    {
        public Color workAmbient = new Color(0.45f, 0.45f, 0.5f);
        public float headlampIntensity = 0.8f;

        Color gameAmbient;
        bool gameFog;
        bool on;
        Light headlamp;

        void Start()
        {
            gameAmbient = RenderSettings.ambientLight;
            gameFog = RenderSettings.fog;
            Apply(gameAmbient);

            var go = new GameObject("WorkLight");
            go.transform.SetParent(transform, false);
            headlamp = go.AddComponent<Light>();
            headlamp.type = LightType.Directional;
            headlamp.shadows = LightShadows.None;
            headlamp.intensity = headlampIntensity;
            headlamp.enabled = false;
        }

        void Update()
        {
            if (on && Camera.main != null)
                headlamp.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(12f, 20f, 0f);

            if (Keyboard.current == null || !Keyboard.current.lKey.wasPressedThisFrame) return;
            on = !on;
            Apply(on ? workAmbient : gameAmbient);
            RenderSettings.fog = !on && gameFog;
            headlamp.enabled = on;
        }

        /// Sets a flat ambient colour. The ambient probe has to be set too, or URP keeps lighting with the old one.
        public static void Apply(Color color)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = color;
            var probe = new SphericalHarmonicsL2();
            probe.AddAmbientLight(color);
            RenderSettings.ambientProbe = probe;
        }
    }
}
