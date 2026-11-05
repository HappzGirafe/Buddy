using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Builds the playable Windows game: Builds/Windows/GLOWFADE.exe (plus its GLOWFADE_Data folder),
    /// starting in the Starhouse. The Builds folder is ignored by git.
    public static class GameBuilder
    {
        public const string ExePath = "Builds/Windows/GLOWFADE.exe";

        [MenuItem("Glowfade/Build Windows Game (.exe)", priority = 20)]
        static void BuildMenu()
        {
            var report = Build();
            bool ok = report.summary.result == BuildResult.Succeeded;
            EditorUtility.DisplayDialog("GLOWFADE", ok
                ? $"Built {Path.GetFullPath(ExePath)}"
                : "The build failed. See the Console.", "OK");
            if (ok) EditorUtility.RevealInFinder(ExePath);
        }

        /// Entry point for -executeMethod. Exits 0 when the build succeeded, 1 when it didn't.
        public static void BuildWindowsFromCommandLine()
        {
            bool ok = false;
            try { ok = Build().summary.result == BuildResult.Succeeded; }
            catch (Exception e) { Debug.LogException(e); }
            EditorApplication.Exit(ok ? 0 : 1);
        }

        public static BuildReport Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StarhouseBuilder.ScenePath) == null)
                throw new InvalidOperationException("Build the Starhouse first (Glowfade > Build Starhouse Greybox).");

            // the window title and save folder come from Player Settings > Product Name (GLOWFADE)
            var options = new BuildPlayerOptions
            {
                scenes = new[] { StarhouseBuilder.ScenePath },
                locationPathName = ExePath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            Debug.Log($"[GLOWFADE] Build {s.result}: {Path.GetFullPath(ExePath)}, {s.totalSize / (1024f * 1024f):F0} MB, "
                      + $"{s.totalTime.TotalMinutes:F1} min, {s.totalErrors} error(s), {s.totalWarnings} warning(s)");
            return report;
        }
    }
}
