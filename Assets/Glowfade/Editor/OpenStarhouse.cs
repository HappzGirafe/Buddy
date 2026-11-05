using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Gets you into the Starhouse quickly: a Glowfade menu item that opens the scene, and, the first time the
    /// project opens in the editor, swapping the empty template scene for the Starhouse so Play drops you in.
    [InitializeOnLoad]
    static class OpenStarhouse
    {
        const string TemplateScene = "Assets/Scenes/SampleScene.unity";

        static string OpenedOnceKey => "Glowfade.OpenedStarhouseOnce." + Application.dataPath;

        static OpenStarhouse()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += OpenOnFirstLaunch;
        }

        [MenuItem("Glowfade/Open Starhouse Scene", priority = 0)]
        static void OpenMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StarhouseBuilder.ScenePath) == null)
            {
                EditorUtility.DisplayDialog("Starhouse", "The Starhouse scene hasn't been built yet. Use Glowfade > Build Starhouse Greybox first.", "OK");
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(StarhouseBuilder.ScenePath);
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
