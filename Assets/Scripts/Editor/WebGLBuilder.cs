using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuilder
{
    [MenuItem("Build/Build WebGL for Itch.io")]
    public static void Build()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string buildFolder = Path.Combine(projectRoot, "Builds", "WebGL");

        if (Directory.Exists(buildFolder))
        {
            Directory.Delete(buildFolder, true);
        }
        Directory.CreateDirectory(buildFolder);

        // Đảm bảo cấu hình WebGL phù hợp với itch.io
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.defaultWebScreenWidth = 540;
        PlayerSettings.defaultWebScreenHeight = 960;

        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildFolder,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        Debug.Log("[WebGLBuilder] Starting WebGL build to: " + buildFolder);
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[WebGLBuilder] Build succeeded! Size: {summary.totalSize} bytes, Time: {summary.totalTime}");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        else
        {
            Debug.LogError($"[WebGLBuilder] Build failed with result: {summary.result}, errors: {summary.totalErrors}");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
