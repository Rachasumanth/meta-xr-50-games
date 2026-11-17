using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MetaXRSim.EditorTools
{
    /// <summary>
    /// Reproducible APK + desktop self-test builds. Signing credentials are read
    /// from environment variables when available; otherwise a local development
    /// keystore is generated under the git-ignored Build/Keys folder.
    /// </summary>
    public static class ShapeForgeBuilder
    {
        public const string BuildRoot = "Build";
        public const string AndroidOutput = "Build/ShapeForge50_v1.0.0.apk";
        public const string WindowsOutput = "Build/Windows/ShapeForge50.exe";

        [MenuItem("ShapeForge/4. Build Android APK (Signed)")]
        public static void BuildAndroid()
        {
            string output = BuildAndroidInternal(false);
            if (!string.IsNullOrEmpty(output))
                EditorUtility.DisplayDialog("ShapeForge", "APK built:\n" + Path.GetFullPath(output), "OK");
        }

        [MenuItem("ShapeForge/5. Build Desktop Self-Test Player")]
        public static void BuildDesktop()
        {
            string output = BuildDesktopInternal();
            if (!string.IsNullOrEmpty(output))
                EditorUtility.DisplayDialog("ShapeForge", "Desktop player built:\n" + Path.GetFullPath(output), "OK");
        }

        // Batch entry points -------------------------------------------------
        public static void BuildAndroidCI()
        {
            if (string.IsNullOrEmpty(BuildAndroidInternal(true)))
                throw new Exception("Android build failed. See the Unity log.");
        }

        public static void BuildDesktopCI()
        {
            if (string.IsNullOrEmpty(BuildDesktopInternal()))
                throw new Exception("Desktop build failed. See the Unity log.");
        }

        public static void ConfigureCI()
        {
            ShapeForgeSetup.Configure();
            if (!ShapeForgeValidator.Validate(true))
                throw new Exception("Production validation failed.");
        }

        // ------------------------------------------------------------------
        private static string BuildAndroidInternal(bool batch)
        {
            Directory.CreateDirectory(BuildRoot);
            CleanMetaBindingCopy();
            ShapeForgeSetup.ConfigureForBuild();

            if (!ShapeForgeValidator.Validate(true))
            {
                UnityEngine.Debug.LogError("[ShapeForge] Validation failed; aborting APK build.");
                return null;
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                UnityEngine.Debug.LogError("[ShapeForge] Android build support is not installed.");
                return null;
            }

            bool signed = ConfigureSigning();
            if (!signed)
            {
                UnityEngine.Debug.LogWarning("[ShapeForge] No production keystore; falling back to the Unity debug keystore. " +
                                             "The APK will install but is not store-publishable.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ShapeForgeSetup.ScenePath },
                locationPathName = signed ? AndroidOutput : "Build/ShapeForge50_debug.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            WriteReport(report, options.locationPathName);
            if (report.summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log("[ShapeForge] APK OK: " + Path.GetFullPath(options.locationPathName) +
                                      " size=" + (report.summary.totalSize / (1024f * 1024f)).ToString("F1") + " MB signed=" + signed);
                return options.locationPathName;
            }

            UnityEngine.Debug.LogError("[ShapeForge] APK FAILED: " + report.summary.result);
            return null;
        }

        private static string BuildDesktopInternal()
        {
            string output = WindowsOutput;
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            CleanMetaBindingCopy();
            ShapeForgeSetup.ConfigureForBuild();

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            {
                UnityEngine.Debug.LogError("[ShapeForge] Windows build support is not installed.");
                return null;
            }

            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.resizableWindow = true;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ShapeForgeSetup.ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            WriteReport(report, options.locationPathName);
            if (report.summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log("[ShapeForge] Desktop player OK: " + Path.GetFullPath(output));
                return output;
            }

            UnityEngine.Debug.LogError("[ShapeForge] Desktop build FAILED: " + report.summary.result);
            return null;
        }

        private static void CleanMetaBindingCopy()
        {
            // Meta XR SDK copies RuntimeActionBindings.json next to the player.
            // Repeated builds fail that CopyTo unless the previous copy is gone.
            try
            {
                string[] candidates =
                {
                    Path.Combine(BuildRoot, "RuntimeActionBindings.json"),
                    Path.Combine(BuildRoot, "Windows", "RuntimeActionBindings.json")
                };
                foreach (var binding in candidates)
                    if (File.Exists(binding)) File.Delete(binding);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[ShapeForge] Could not clear stale runtime bindings: " + e.Message);
            }
        }

        private static bool ConfigureSigning()
        {
            string keystore = Environment.GetEnvironmentVariable("SHAPEFORGE_KEYSTORE");
            string storePass = Environment.GetEnvironmentVariable("SHAPEFORGE_KEYSTORE_PASS");
            string alias = Environment.GetEnvironmentVariable("SHAPEFORGE_KEY_ALIAS");
            string keyPass = Environment.GetEnvironmentVariable("SHAPEFORGE_KEY_PASS");

            if (string.IsNullOrEmpty(keystore))
            {
                string keysDir = Path.Combine(BuildRoot, "Keys");
                Directory.CreateDirectory(keysDir);
                keystore = Path.GetFullPath(Path.Combine(keysDir, "shapeforge.keystore"));
                string secretFile = Path.Combine(keysDir, "keystore.txt");

                if (!File.Exists(keystore))
                {
                    string keytool = FindKeytool();
                    if (string.IsNullOrEmpty(keytool))
                    {
                        UnityEngine.Debug.LogWarning("[ShapeForge] keytool not found. Install Android OpenJDK from Unity Hub.");
                        return false;
                    }

                    string generated = Guid.NewGuid().ToString("N");
                    alias = "shapeforge";
                    storePass = generated;
                    keyPass = generated;

                    string args = "-genkeypair -v -keystore \"" + keystore + "\" -alias " + alias +
                                  " -keyalg RSA -keysize 2048 -validity 10000" +
                                  " -storepass " + storePass + " -keypass " + keyPass +
                                  " -dname \"CN=ShapeForge 50, OU=XR, O=ShapeForge Labs, L=Local, ST=Local, C=US\"";
                    var psi = new ProcessStartInfo(keytool, args)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    using (var p = Process.Start(psi))
                    {
                        string stdout = p.StandardOutput.ReadToEnd();
                        string stderr = p.StandardError.ReadToEnd();
                        p.WaitForExit();
                        if (p.ExitCode != 0)
                        {
                            UnityEngine.Debug.LogError("[ShapeForge] keytool failed: " + stderr);
                            return false;
                        }
                    }
                    File.WriteAllText(secretFile,
                        "keystore=" + keystore + Environment.NewLine +
                        "alias=" + alias + Environment.NewLine +
                        "storePass=" + storePass + Environment.NewLine +
                        "keyPass=" + keyPass + Environment.NewLine +
                        "NOTE: Keep this file private. Replace with your own Play/Horizon signing key before publishing.");
                    UnityEngine.Debug.Log("[ShapeForge] Generated local signing key at " + keystore);
                }
                else if (File.Exists(secretFile))
                {
                    foreach (var line in File.ReadAllLines(secretFile))
                    {
                        if (line.StartsWith("alias=")) alias = line.Substring(6);
                        else if (line.StartsWith("storePass=")) storePass = line.Substring(10);
                        else if (line.StartsWith("keyPass=")) keyPass = line.Substring(8);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[ShapeForge] Keystore exists but credentials are missing.");
                    return false;
                }
            }

            if (string.IsNullOrEmpty(alias)) alias = "shapeforge";
            if (string.IsNullOrEmpty(storePass)) return false;
            if (string.IsNullOrEmpty(keyPass)) keyPass = storePass;

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keystorePass = storePass;
            PlayerSettings.Android.keyaliasPass = keyPass;
            AssetDatabase.SaveAssets();
            return true;
        }

        private static string FindKeytool()
        {
            string[] candidates =
            {
                Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/AndroidPlayer/OpenJDK/bin/keytool.exe"),
                Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/AndroidPlayer/OpenJDK/bin/keytool"),
                Path.Combine(Environment.GetEnvironmentVariable("JAVA_HOME") ?? "", "bin/keytool.exe"),
                Path.Combine(Environment.GetEnvironmentVariable("JAVA_HOME") ?? "", "bin/keytool")
            };
            foreach (var c in candidates)
                if (!string.IsNullOrEmpty(c) && File.Exists(c)) return c;

            string path = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (var dir in path.Split(Path.PathSeparator))
            {
                try
                {
                    string p = Path.Combine(dir, "keytool.exe");
                    if (File.Exists(p)) return p;
                }
                catch { }
            }
            return null;
        }

        private static void WriteReport(BuildReport report, string output)
        {
            try
            {
                var s = report.summary;
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"result\": \"" + s.result + "\",");
                sb.AppendLine("  \"output\": \"" + output.Replace("\\", "\\\\") + "\",");
                sb.AppendLine("  \"totalSizeBytes\": " + s.totalSize + ",");
                sb.AppendLine("  \"totalTimeSeconds\": " + s.totalTime.TotalSeconds.ToString("F1") + ",");
                sb.AppendLine("  \"errors\": " + s.totalErrors + ",");
                sb.AppendLine("  \"warnings\": " + s.totalWarnings);
                sb.AppendLine("}");
                File.WriteAllText(Path.Combine(BuildRoot, "build-report.json"), sb.ToString());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[ShapeForge] Could not write build report: " + e.Message);
            }
        }
    }
}

