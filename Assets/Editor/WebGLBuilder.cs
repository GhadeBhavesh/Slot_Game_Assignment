#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuilder
{
    [MenuItem("SlotGame/Build WebGL")]
    public static void Build()
    {
        string buildDir = "Build/WebGL";
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        string[] scenes = { "Assets/Scenes/SlotGame.unity" };
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        Debug.Log($"[WebGLBuilder] Starting WebGL build to: {buildDir}");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[WebGLBuilder] Build SUCCEEDED: {summary.totalSize} bytes in {summary.totalTime.TotalSeconds:F1}s.");
        }
        else
        {
            Debug.LogError($"[WebGLBuilder] Build FAILED with result: {summary.result}, errors: {summary.totalErrors}");
        }
    }
}
#endif
