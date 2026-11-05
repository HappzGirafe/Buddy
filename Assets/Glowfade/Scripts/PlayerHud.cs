using UnityEngine;
using UnityEngine.InputSystem;

namespace Glowfade
{
    /// On-screen help for the greybox: the controls, what your body is doing, and the flashlight battery.
    /// H (or the gamepad's View/Select button) shows and hides the list of controls.
    [RequireComponent(typeof(FirstPersonController))]
    public class PlayerHud : MonoBehaviour
    {
        public bool showControls = true;

        static readonly string[,] Controls =
        {
            { "WASD + mouse", "walk and look around" },
            { "Shift", "run" },
            { "Space", "jump, or get up" },
            { "C", "crouch" },
            { "Z", "lie down: crawl through low gaps" },
            { "X", "sit: on a seat in front of you, or on the floor" },
            { "F", "flashlight" },
            { "L", "work lights: see the whole level" },
            { "Esc", "free the mouse (click to grab it again)" },
            { "H", "hide this help" },
        };

        const string GamepadLine = "Gamepad: sticks walk and look, A jump, B crouch, D-pad down lie down, X sit, Y flashlight";

        FirstPersonController body;
        Flashlight flashlight;
        GUIStyle keyStyle, textStyle, smallStyle;

        void Awake()
        {
            body = GetComponent<FirstPersonController>();
            flashlight = GetComponentInChildren<Flashlight>();
        }

        void Update()
        {
            bool toggle = (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame);
            if (toggle) showControls = !showControls;
        }

        public static string StanceName(Stance stance, bool onSeat)
        {
            switch (stance)
            {
                case Stance.Crouching: return "Crouching";
                case Stance.LyingDown: return "Lying down";
                case Stance.Sitting: return onSeat ? "Sitting on a seat" : "Sitting on the floor";
                default: return "Standing";
            }
        }

        void OnGUI()
        {
            if (keyStyle == null)
            {
                keyStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
                keyStyle.normal.textColor = new Color(1f, 0.85f, 0.45f);
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                textStyle.normal.textColor = Color.white;
                smallStyle = new GUIStyle(textStyle) { fontSize = 12, wordWrap = true };
                smallStyle.normal.textColor = new Color(0.8f, 0.8f, 0.85f);
            }

            const float row = 22f, keyWidth = 120f, width = 480f, pad = 10f;
            int rows = Controls.GetLength(0);
            if (showControls)
            {
                var area = new Rect(12f, 12f, width, rows * row + 2f * row + 2f * pad);
                GUI.Box(area, GUIContent.none);
                for (int i = 0; i < rows; i++)
                {
                    float y = area.y + pad + i * row;
                    GUI.Label(new Rect(area.x + pad, y, keyWidth, row), Controls[i, 0], keyStyle);
                    GUI.Label(new Rect(area.x + pad + keyWidth, y, width - keyWidth - 2f * pad, row), Controls[i, 1], textStyle);
                }
                GUI.Label(new Rect(area.x + pad, area.y + pad + rows * row, width - 2f * pad, 2f * row), GamepadLine, smallStyle);
            }

            string status = StanceName(body.CurrentStance, body.IsOnSeat);
            if (flashlight != null)
                status += $"     Flashlight {(flashlight.IsOn ? "on" : "off")}, battery {Mathf.RoundToInt(flashlight.charge * 100f)}%";
            if (!showControls) status += "     H: controls";
            GUI.Label(new Rect(12f, Screen.height - 34f, 900f, 24f), status, textStyle);
        }
    }
}
