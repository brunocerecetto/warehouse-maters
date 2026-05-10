// Source: RESEARCH Pattern 6, support.unity.com 211195263 (BuildPlayer exit codes).
// Phase 1 ships a skeleton: produces the Xcode project, no signing, no archive.
// Phase 11 will extend with signing team, provisioning, post-build hooks.
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WM.Editor
{
    public static class BuildScript
    {
        // Invoked from CLI:
        //   <UNITY> -batchmode -quit -projectPath . -buildTarget iOS \
        //           -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -
        public static void BuildIOS()
        {
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "build", "ios");
            Directory.CreateDirectory(outputPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Warehouse_MVP.unity" },
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildIOS] Succeeded. Output: {outputPath}, size: {summary.totalSize} bytes, time: {summary.totalTime}");
            }
            else
            {
                Debug.LogError($"[BuildIOS] Build result: {summary.result}. Errors: {summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }
    }
}
