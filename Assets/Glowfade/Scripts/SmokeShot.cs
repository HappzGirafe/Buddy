using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Glowfade
{
    /// For checking a built game without anyone at the keyboard. Start it with
    ///   GLOWFADE.exe -glowfade-screenshot "C:\path\shot.png" [-glowfade-quit]
    /// and it saves a picture of the screen a few seconds after the level starts, then (with -glowfade-quit)
    /// quits with exit code 0 if the picture was written and 1 if it wasn't. Without those arguments it does nothing.
    public static class SmokeShot
    {
        const string ShotArg = "-glowfade-screenshot", QuitArg = "-glowfade-quit";

        // keep running when the window loses focus (from before the splash screen), so an unattended run can't stall
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void KeepRunning()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), ShotArg) >= 0) Application.runInBackground = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, ShotArg);
            if (i < 0 || i + 1 >= args.Length) return;
            var runner = new GameObject("SmokeShot").AddComponent<Runner>();
            UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
            runner.StartCoroutine(runner.Take(args[i + 1], Array.IndexOf(args, QuitArg) >= 0));
        }

        class Runner : MonoBehaviour
        {
            public IEnumerator Take(string path, bool quit)
            {
                string full = null;
                try
                {
                    full = Path.GetFullPath(path);
                    Directory.CreateDirectory(Path.GetDirectoryName(full));
                    if (File.Exists(full)) File.Delete(full);
                }
                catch (Exception e) { Debug.LogError("[Glowfade] Screenshot path problem: " + e.Message); }

                yield return new WaitForSecondsRealtime(6f); // let the level settle and the first frames render
                if (full != null) ScreenCapture.CaptureScreenshot(full);
                yield return new WaitForSecondsRealtime(1.5f); // the file is written at the end of a frame

                bool ok = full != null && File.Exists(full);
                if (ok) Debug.Log("[Glowfade] Screenshot saved to " + full);
                else Debug.LogError("[Glowfade] Screenshot FAILED: nothing was written to " + (full ?? path));
                if (quit) Application.Quit(ok ? 0 : 1);
            }
        }
    }
}
