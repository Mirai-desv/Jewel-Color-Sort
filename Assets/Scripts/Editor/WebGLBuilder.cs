using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class WebGLBuilder : EditorWindow
{
    private const int DefaultWidth = 540;
    private const int DefaultHeight = 960;
    private const string ZipFileName = "JewelColorSort_WebGL.zip";

    [MenuItem("Tools/Itch.io WebGL Builder", priority = 50)]
    [MenuItem("Tools/Jewel Sort/Itch.io WebGL Builder", priority = 51)]
    [MenuItem("Window/Itch.io WebGL Builder", priority = 52)]
    [MenuItem("Build/Itch.io WebGL Builder Window", priority = 53)]
    [MenuItem("Assets/Build WebGL for Itch.io", priority = 1000)]
    public static void ShowWindow()
    {
        var window = GetWindow<WebGLBuilder>("Itch.io Builder");
        window.minSize = new Vector2(480, 520);
        window.Show();
    }

    [MenuItem("Build/Build WebGL for Itch.io (Auto-Zip)", priority = 100)]
    [MenuItem("Tools/Jewel Sort/Build WebGL (Auto-Zip)", priority = 101)]
    public static void BuildAndZip()
    {
        if (BuildWebGL(out string buildFolder))
        {
            string zipPath = CreateZip(buildFolder);
            if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
            {
                EditorUtility.DisplayDialog(
                    "Thành Công!",
                    $"Build và nén file zip thành công!\n\nFile zip: {Path.GetFileName(zipPath)}\nVị trí: {zipPath}\n\nBạn có thể tải trực tiếp file zip này lên itch.io.",
                    "Mở thư mục"
                );
                EditorUtility.RevealInFinder(zipPath);
            }
        }
    }

    [MenuItem("Build/Zip Current WebGL Build", priority = 101)]
    public static void ZipOnly()
    {
        string buildFolder = GetBuildFolder();
        if (!Directory.Exists(buildFolder) || !File.Exists(Path.Combine(buildFolder, "index.html")))
        {
            EditorUtility.DisplayDialog(
                "Chưa có bản build WebGL",
                $"Không tìm thấy bản build hợp lệ tại:\n{buildFolder}\n\nVui lòng bấm 'Build WebGL for Itch.io' trước.",
                "OK"
            );
            return;
        }

        string zipPath = CreateZip(buildFolder);
        if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
        {
            EditorUtility.DisplayDialog(
                "Đã tạo file Zip!",
                $"File zip: {Path.GetFileName(zipPath)}\n\nĐã sẵn sàng tải lên itch.io.",
                "Mở thư mục"
            );
            EditorUtility.RevealInFinder(zipPath);
        }
    }

    [MenuItem("Build/Open Builds Folder", priority = 102)]
    public static void OpenBuildsFolder()
    {
        string folder = Path.Combine(GetProjectRoot(), "Builds");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        EditorUtility.RevealInFinder(folder);
    }

    private void OnGUI()
    {
        GUILayout.Space(12);
        GUILayout.Label("🎮 Itch.io WebGL Build Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Tool này tự động cấu hình và xuất file WebGL tối ưu cho Itch.io:\n" +
            "• Tỷ lệ màn hình chuẩn dọc 9:16 (540 x 960)\n" +
            "• Tắt nén (Disabled) tránh lỗi giải nén trên trình duyệt\n" +
            "• Tự động đóng gói file zip với index.html nằm ngay thư mục gốc\n" +
            "• Tự động mở Finder làm nổi bật file zip để bạn kéo thả lên Itch.io",
            MessageType.Info
        );

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Cấu hình hiện tại", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Độ phân giải Web:", $"{DefaultWidth} x {DefaultHeight} (9:16 Portrait)");
        EditorGUILayout.LabelField("Compression Format:", "Disabled (Khuyên dùng cho Itch.io)");
        EditorGUILayout.LabelField("Nền tảng hiện tại:", EditorUserBuildSettings.activeBuildTarget.ToString());

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            EditorGUILayout.HelpBox("Platform hiện tại chưa phải WebGL. Bấm nút dưới để chuyển sang WebGL trước khi build.", MessageType.Warning);
            if (GUILayout.Button("Chuyển Platform sang WebGL", GUILayout.Height(32)))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }
        }

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Thao tác xuất bản", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
        if (GUILayout.Button("🚀 BUILD & TẠO FILE ZIP CHO ITCH.IO", GUILayout.Height(45)))
        {
            BuildAndZip();
        }

        GUI.backgroundColor = Color.white;
        GUILayout.Space(6);

        if (GUILayout.Button("📦 Nén lại bản Build WebGL hiện có (Không cần build lại)", GUILayout.Height(30)))
        {
            ZipOnly();
        }

        if (GUILayout.Button("📂 Mở thư mục Builds", GUILayout.Height(26)))
        {
            OpenBuildsFolder();
        }

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Hướng dẫn tải lên Itch.io", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. Lên trang dự án trên itch.io -> Edit game -> Uploads -> Tải file 'JewelColorSort_WebGL.zip' lên.\n" +
            "2. Đánh dấu tích vào ô: 'This file will be played in the browser'.\n" +
            "3. Ở mục Embed dimensions, nhập: 540 x 960.\n" +
            "4. Đánh dấu tích vào ô 'Mobile friendly' (nếu muốn hỗ trợ mobile).",
            MessageType.None
        );
    }

    public static bool BuildWebGL(out string buildFolder)
    {
        string projectRoot = GetProjectRoot();
        buildFolder = Path.Combine(projectRoot, "Builds", "WebGL");

        if (Directory.Exists(buildFolder))
        {
            try
            {
                Directory.Delete(buildFolder, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[WebGLBuilder] Could not clean folder: " + ex.Message);
            }
        }
        Directory.CreateDirectory(buildFolder);

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.defaultWebScreenWidth = DefaultWidth;
        PlayerSettings.defaultWebScreenHeight = DefaultHeight;

        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

        // Dọn dẹp cache player cũ để tránh lỗi script class layout mismatch giữa Editor và Player
        try
        {
            string playerCache = Path.Combine(GetProjectRoot(), "Library", "PlayerDataCache");
            string buildPlayerData = Path.Combine(GetProjectRoot(), "Library", "BuildPlayerData");
            if (Directory.Exists(playerCache)) Directory.Delete(playerCache, true);
            if (Directory.Exists(buildPlayerData)) Directory.Delete(buildPlayerData, true);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WebGLBuilder] Could not clean player data cache: " + ex.Message);
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildFolder,
            target = BuildTarget.WebGL,
            options = BuildOptions.CleanBuildCache
        };

        Debug.Log("[WebGLBuilder] Bắt đầu build WebGL vào: " + buildFolder);
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[WebGLBuilder] Build WebGL thành công! Dung lượng: {summary.totalSize} bytes, Thời gian: {summary.totalTime}");
            if (Application.isBatchMode)
            {
                CreateZip(buildFolder);
                EditorApplication.Exit(0);
            }
            return true;
        }
        else
        {
            Debug.LogError($"[WebGLBuilder] Build thất bại với kết quả: {summary.result}, {summary.totalErrors} lỗi.");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Build Thất Bại", $"Quá trình build WebGL gặp lỗi. Vui lòng kiểm tra Console.", "Đóng");
            }
            else
            {
                EditorApplication.Exit(1);
            }
            return false;
        }
    }

    public static string CreateZip(string buildFolder)
    {
        try
        {
            string projectRoot = GetProjectRoot();
            string buildsFolder = Path.Combine(projectRoot, "Builds");
            string zipPath = Path.Combine(buildsFolder, ZipFileName);

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            Debug.Log($"[WebGLBuilder] Đang đóng gói zip từ '{buildFolder}' vào '{zipPath}'...");
            ZipFile.CreateFromDirectory(buildFolder, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);
            Debug.Log($"[WebGLBuilder] Đã tạo file zip thành công: {zipPath}");
            return zipPath;
        }
        catch (Exception ex)
        {
            Debug.LogError("[WebGLBuilder] Lỗi khi tạo zip: " + ex.Message);
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Lỗi tạo Zip", "Không thể nén file zip: " + ex.Message, "Đóng");
            }
            return null;
        }
    }

    private static string GetProjectRoot()
    {
        return Path.GetDirectoryName(Application.dataPath);
    }

    private static string GetBuildFolder()
    {
        return Path.Combine(GetProjectRoot(), "Builds", "WebGL");
    }
}
