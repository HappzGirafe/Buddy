using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Gets you into the Starhouse quickly: Glowfade menu items that open the scene or open it and press Play,
    /// and, the first time the project opens in the editor, swapping the empty template scene for the Starhouse.
    [InitializeOnLoad]
    public static class OpenStarhouse
    {
        const string TemplateScene = "Assets/Scenes/SampleScene.unity";
        const string PictureAfterPlayKey = "Glowfade.PictureAfterPlay";

        static string OpenedOnceKey => "Glowfade.OpenedStarhouseOnce." + Application.dataPath;
        static double pictureTime;

        static OpenStarhouse()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += OpenOnFirstLaunch;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Glowfade/Open Starhouse Scene", priority = 0)]
        static void OpenMenu()
        {
            if (!SceneExists()) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(StarhouseBuilder.ScenePath);
        }

        [MenuItem("Glowfade/Play Starhouse", priority = 1)]
        static void PlayMenu()
        {
            if (!SceneExists() || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(StarhouseBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
        }

        /// For -executeMethod in a normal (windowed) editor: opens the Starhouse and presses Play, then saves
        /// a picture of the Game view to Design/Screenshots/play_mode.png so you can see it started.
        public static void PlayFromCommandLine()
        {
            Debug.Log("[Glowfade] Opening the Starhouse and pressing Play");
            EditorSceneManager.OpenScene(StarhouseBuilder.ScenePath);
            SessionState.SetBool(PictureAfterPlayKey, true); // survives the reload that entering Play Mode does
            EditorApplication.delayCall += () =>
            {
                EditorApplication.EnterPlaymode();
                // Play Mode can be refused (e.g. compile errors); then don't leave the picture request for a later Play
                if (!EditorApplication.isPlayingOrWillChangePlaymode) SessionState.EraseBool(PictureAfterPlayKey);
            };
        }

        static bool SceneExists()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StarhouseBuilder.ScenePath) != null) return true;
            EditorUtility.DisplayDialog("Starhouse", "The Starhouse scene hasn't been built yet. Use Glowfade > Build Starhouse Greybox first.", "OK");
            return false;
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PictureAfterPlayKey, false)) return;
            SessionState.EraseBool(PictureAfterPlayKey);
            Debug.Log("[Glowfade] The Starhouse is playing. Click in the Game view to look around; Esc frees the mouse.");
            pictureTime = EditorApplication.timeSinceStartup + 4.0; // let the first frames settle
            EditorApplication.update += TakePictureWhenDue;
        }

        static void TakePictureWhenDue()
        {
            if (!EditorApplication.isPlaying) { EditorApplication.update -= TakePictureWhenDue; return; }
            if (EditorApplication.timeSinceStartup < pictureTime) return;
            EditorApplication.update -= TakePictureWhenDue;
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "Screenshots", "play_mode.png"));
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[Glowfade] Game view picture saved to " + path);
        }

        static void OpenOnFirstLaunch()
        {
            if (EditorPrefs.GetBool(OpenedOnceKey, false) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            // the first check uses up the switch, so it can never happen later in a session (e.g. after a rebuild)
            EditorPrefs.SetBool(OpenedOnceKey, true);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StarhouseBuilder.ScenePath) == null) return;

            // only replace the untouched template scene, never a scene someone is working in
            var active = EditorSceneManager.GetActiveScene();
            bool onTemplate = EditorSceneManager.sceneCount == 1 && !active.isDirty
                && (active.path == TemplateScene || string.IsNullOrEmpty(active.path));
            if (onTemplate) EditorSceneManager.OpenScene(StarhouseBuilder.ScenePath);
        }
    }
}
