using UnityEngine;
using UnityEngine.InputSystem;

namespace Glowfade
{
    /// On-screen help for the greybox: the controls, what your body is doing, the flashlight battery,
    /// and a pause menu. H (or the gamepad's View/Select button) shows and hides the controls;
    /// Esc (or Start) pauses, with buttons to keep playing or quit; on a gamepad B keeps playing and holding X quits.
    /// Runs after the player each frame, so a press that unpauses can't also reach the player.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(FirstPersonController))]
    public class PlayerHud : MonoBehaviour
    {
        public bool showControls = true;

        public bool Paused { get; private set; }

        const float QuitHoldSeconds = 1f;
        float quitHeldSince = -1f;

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
            { "Esc", "pause (keep playing or quit)" },
            { "H", "hide this help" },
        };

        const string GamepadLine = "Gamepad: sticks walk and look, A jump, B crouch, D-pad down lie down, X sit, Y flashlight, Start pause";

        FirstPersonController body;
        Flashlight flashlight;
        GUIStyle keyStyle, textStyle, smallStyle, titleStyle, buttonStyle;

        void Awake()
        {
            body = GetComponent<FirstPersonController>();
            flashlight = GetComponentInChildren<Flashlight>();
        }

        // while the HUD is on, Esc and grabbing the mouse are handled here; without it the player does it
        void OnEnable() => body.externalPauseMenu = true;

        void OnDisable()
        {
            if (Paused)
            {
                Paused = false;
                Time.timeScale = 1f;
            }
            body.externalPauseMenu = false;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pad = Gamepad.current;

            bool toggleHelp = (keyboard != null && keyboard.hKey.wasPressedThisFrame) || (pad != null && pad.selectButton.wasPressedThisFrame);
            if (toggleHelp) showControls = !showControls;

            bool pauseKey = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame);
            bool padResume = Paused && pad != null && pad.buttonEast.wasPressedThisFrame;
            if (pauseKey || padResume)
            {
                bool resuming = Paused;
                SetPaused(!Paused);
#if UNITY_EDITOR
                // after an Esc the editor's Game view won't lock the mouse again until it's clicked, so leave the
                // mouse free for now; that click grabs it (below)
                if (resuming) FirstPersonController.LockCursor(false);
#endif
            }

            // quitting with a gamepad: hold X for a second
            if (Paused && pad != null && pad.buttonWest.isPressed)
            {
                if (quitHeldSince < 0f) quitHeldSince = Time.unscaledTime;
                else if (Time.unscaledTime - quitHeldSince >= QuitHoldSeconds) Quit();
            }
            else quitHeldSince = -1f;

            // not paused but the mouse got away (e.g. after Alt+Tab): a click in the game grabs it again
            if (!Paused && mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                FirstPersonController.LockCursor(true);
        }

        public void SetPaused(bool paused)
        {
            Paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            FirstPersonController.LockCursor(!paused);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
                titleStyle = new GUIStyle(textStyle) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
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

            if (Paused) PauseMenu();
        }

        void PauseMenu()
        {
            const float w = 320f, h = 190f;
            var panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUI.Box(panel, GUIContent.none); // twice, so the panel reads darker over a lit scene
            GUI.Label(new Rect(panel.x, panel.y + 14f, w, 40f), "Paused", titleStyle);
            if (GUI.Button(new Rect(panel.x + 40f, panel.y + 70f, w - 80f, 44f), "Keep playing  (Esc / B)", buttonStyle)) SetPaused(false);
            string quitLabel = quitHeldSince < 0f
                ? "Quit game  (hold X)"
                : $"Quitting... {Mathf.Clamp01((Time.unscaledTime - quitHeldSince) / QuitHoldSeconds) * 100f:F0}%";
            if (GUI.Button(new Rect(panel.x + 40f, panel.y + 124f, w - 80f, 44f), quitLabel, buttonStyle)) Quit();
        }
    }
}
