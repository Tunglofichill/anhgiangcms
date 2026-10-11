using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Forge.EditorTools
{
    public class ForgeBuildTools : EditorWindow
    {
        private const string SCENE_PATH = "Assets/_Forge/Scenes/ForgeDesk.unity";
        private const string DEFAULT_PRODUCT_NAME = "Legend of the Forge";
        private const string COMPANY_NAME = "Truong Giang";
        private const string DEFAULT_BUILD_SUBDIR = "Builds/Windows";
        private const string EXE_NAME = "LegendOfTheForge.exe";

        private bool _isDevelopmentBuild = false;
        private bool _autoRunAfterBuild = true;
        private bool _windowedMode = true;
        private string _customBuildDir = "";

        [MenuItem("Forge/Build/🛠️ Build Manager Window...", false, 190)]
        public static void OpenBuildWindow()
        {
            var win = GetWindow<ForgeBuildTools>("Forge Build Manager");
            win.minSize = new Vector2(460, 480);
            win.Show();
        }

        [MenuItem("Forge/Build/🚀 Build Windows App (.exe)", false, 191)]
        public static void MenuBuildWindows()
        {
            BuildWindows(autoRun: false, isDevBuild: false);
        }

        [MenuItem("Forge/Build/⚡ Build & Run Windows App", false, 192)]
        public static void MenuBuildAndRunWindows()
        {
            BuildWindows(autoRun: true, isDevBuild: false);
        }

        [MenuItem("Forge/Build/📂 Open Build Folder", false, 193)]
        public static void MenuOpenBuildFolder()
        {
            string dir = GetBuildDirectory();
            if (Directory.Exists(dir))
            {
                Process.Start("explorer.exe", dir.Replace('/', '\\'));
            }
            else
            {
                EditorUtility.DisplayDialog("Chưa có bản build", $"Thư mục build chưa tồn tại:\n{dir}\n\nVui lòng bấm Build trước!", "OK");
            }
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_customBuildDir))
            {
                _customBuildDir = Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_BUILD_SUBDIR).Replace('\\', '/');
            }
        }

        private GUIStyle _headerStyle;

        private void OnGUI()
        {
            // Header
            EditorGUILayout.Space(10);
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.95f, 0.75f, 0.30f) }
                };
            }
            EditorGUILayout.LabelField("🔨 FORGE BUILD MANAGER 🚀", _headerStyle);
            EditorGUILayout.LabelField("Công cụ đóng gói game độc quyền • Trường Giang", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(12);

            // Information Box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 Thông Tin Dự Án", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Tên Game:", DEFAULT_PRODUCT_NAME);
            EditorGUILayout.LabelField("Tác giả / Khách hàng:", "Trường Giang");
            EditorGUILayout.LabelField("Nền tảng:", "Windows Standalone (x64 / .exe)");
            EditorGUILayout.LabelField("Scene chính:", SCENE_PATH);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Settings Box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚙️ Cấu Hình Đóng Gói", EditorStyles.boldLabel);

            // Output Folder
            EditorGUILayout.BeginHorizontal();
            _customBuildDir = EditorGUILayout.TextField("Thư mục xuất:", _customBuildDir);
            if (GUILayout.Button("📂", GUILayout.Width(35)))
            {
                string chosen = EditorUtility.OpenFolderPanel("Chọn thư mục xuất Build", _customBuildDir, "");
                if (!string.IsNullOrEmpty(chosen))
                {
                    _customBuildDir = chosen;
                }
            }
            EditorGUILayout.EndHorizontal();

            _isDevelopmentBuild = EditorGUILayout.Toggle("Development Build (Debug)", _isDevelopmentBuild);
            _autoRunAfterBuild = EditorGUILayout.Toggle("Tự động mở game sau khi build", _autoRunAfterBuild);
            _windowedMode = EditorGUILayout.Toggle("Chế độ Cửa Sổ (1920x1080 Windowed)", _windowedMode);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // Status of current build
            string exePath = Path.Combine(_customBuildDir, EXE_NAME).Replace('\\', '/');
            bool exeExists = File.Exists(exePath);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📦 Trạng Thái Bản Build Hiện Tại", EditorStyles.boldLabel);
            if (exeExists)
            {
                var fi = new FileInfo(exePath);
                double mb = fi.Length / (1024.0 * 1024.0);
                EditorGUILayout.LabelField("Trạng thái:", "✅ Đã sẵn sàng chạy");
                EditorGUILayout.LabelField("Dung lượng exe:", $"{mb:F2} MB");
                EditorGUILayout.LabelField("Lần build cuối:", fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss"));
            }
            else
            {
                EditorGUILayout.LabelField("Trạng thái:", "⚠️ Chưa có file build nào.");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // Action Buttons
            GUI.backgroundColor = new Color(0.95f, 0.65f, 0.20f);
            if (GUILayout.Button("🚀 BẮT ĐẦU BUILD APP WINDOWS (.EXE)", GUILayout.Height(42)))
            {
                BuildWindows(autoRun: _autoRunAfterBuild, isDevBuild: _isDevelopmentBuild, customDir: _customBuildDir, windowed: _windowedMode);
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📂 Mở Thư Mục Build", GUILayout.Height(30)))
            {
                MenuOpenBuildFolder();
            }

            if (exeExists)
            {
                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
                if (GUILayout.Button("🎮 Chạy File EXE Ngay", GUILayout.Height(30)))
                {
                    RunExistingBuild();
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        public static string GetBuildDirectory(string customDir = "")
        {
            if (!string.IsNullOrEmpty(customDir)) return customDir;
            return Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_BUILD_SUBDIR).Replace('\\', '/');
        }

        public static void RunExistingBuild(string customDir = "")
        {
            string dir = GetBuildDirectory(customDir);
            string exePath = Path.Combine(dir, EXE_NAME).Replace('\\', '/');
            if (File.Exists(exePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath.Replace('/', '\\'),
                    WorkingDirectory = dir.Replace('/', '\\')
                });
            }
            else
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy file {exePath}!", "OK");
            }
        }

        public static bool BuildWindows(bool autoRun = false, bool isDevBuild = false, string customDir = "", bool windowed = true)
        {
            string outputDir = GetBuildDirectory(customDir);
            string exePath = Path.Combine(outputDir, EXE_NAME).Replace('\\', '/');

            Debug.Log($"[ForgeBuild] === Bắt đầu Build Game cho Windows x64 ===");
            Debug.Log($"[ForgeBuild] Đường dẫn xuất: {exePath}");

            // 1. Tạo thư mục build nếu chưa có
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 2. Đảm bảo Scene ForgeDesk có trong Build Settings
            if (!File.Exists(SCENE_PATH))
            {
                Debug.LogError($"[ForgeBuild] Không tìm thấy scene: {SCENE_PATH}");
                EditorUtility.DisplayDialog("Lỗi Build", $"Không tìm thấy file Scene:\n{SCENE_PATH}", "OK");
                return false;
            }

            var scenesInBuild = EditorBuildSettings.scenes.ToList();
            if (!scenesInBuild.Any(s => s.path == SCENE_PATH))
            {
                scenesInBuild.Insert(0, new EditorBuildSettingsScene(SCENE_PATH, true));
                EditorBuildSettings.scenes = scenesInBuild.ToArray();
            }

            // 3. Cấu hình PlayerSettings chuẩn chỉnh
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = DEFAULT_PRODUCT_NAME;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = windowed ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;

            // Gán Icon nếu có (Unity 6: dùng NamedBuildTarget và IconKind.Application)
            var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Forge/Art/Textures/Icons/Icon_G1_Hammer.png");
            if (iconTex != null)
            {
                try
                {
                    PlayerSettings.SetIcons(
                        UnityEditor.Build.NamedBuildTarget.Standalone,
                        new[] { iconTex },
                        UnityEditor.IconKind.Application
                    );
                }
                catch { }
            }

            // 4. Thiết lập Build Options (Unity 6: targetGroup bị deprecated, bỏ đi)
            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = new[] { SCENE_PATH },
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            if (isDevBuild)
            {
                opt.options |= BuildOptions.Development;
            }

            if (autoRun)
            {
                opt.options |= BuildOptions.AutoRunPlayer;
            }

            // 5. Thực thi Build
            BuildReport report = BuildPipeline.BuildPlayer(opt);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                double sizeMb = summary.totalSize / (1024.0 * 1024.0);
                double elapsedSec = summary.totalTime.TotalSeconds;

                Debug.Log($"[ForgeBuild] ✅ BUILD THÀNH CÔNG VƯỢT TRỘI!");
                Debug.Log($"[ForgeBuild] Kích thước file: {sizeMb:F2} MB");
                Debug.Log($"[ForgeBuild] Thời gian build: {elapsedSec:F1} giây");
                Debug.Log($"[ForgeBuild] File thực thi: {exePath}");

                if (!Application.isBatchMode)
                {
                    bool openFolder = EditorUtility.DisplayDialog(
                        "Build Thành Công! 🎉",
                        $"Đã đóng gói hoàn tất game:\n\n" +
                        $"🎮 File: {EXE_NAME}\n" +
                        $"📦 Dung lượng: {sizeMb:F2} MB\n" +
                        $"⏱️ Thời gian: {elapsedSec:F1}s\n" +
                        $"📁 Thư mục: {outputDir}\n\n" +
                        $"Sếp có muốn mở thư mục chứa game ngay không?",
                        "Mở Thư Mục", "Đóng"
                    );

                    if (openFolder)
                    {
                        EditorUtility.RevealInFinder(exePath);
                    }
                }

                return true;
            }
            else
            {
                Debug.LogError($"[ForgeBuild] ❌ BUILD THẤT BẠI! Lỗi: {summary.totalErrors}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Build Thất Bại ❌",
                        $"Quá trình đóng gói gặp {summary.totalErrors} lỗi.\nVui lòng kiểm tra tab Console để xem chi tiết.",
                        "Xem Lại"
                    );
                }

                return false;
            }
        }

        // Dành cho dòng lệnh Batchmode nếu cần
        public static void BuildWindowsCli()
        {
            BuildWindows(autoRun: false, isDevBuild: false);
        }
    }
}
