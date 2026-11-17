using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

namespace MetaXRSim.EditorTools
{
    /// <summary>
    /// Static production gate for the unified app. Run from the menu, from CI, or
    /// automatically before an APK build. Fails loudly instead of shipping a
    /// half-configured player.
    /// </summary>
    public static class ShapeForgeValidator
    {
        public sealed class Result
        {
            public bool Ok = true;
            public readonly List<string> Passed = new List<string>();
            public readonly List<string> Failed = new List<string>();

            public void Check(string name, bool condition, string detail = null)
            {
                if (condition) Passed.Add(name);
                else
                {
                    Ok = false;
                    Failed.Add(name + (string.IsNullOrEmpty(detail) ? "" : " :: " + detail));
                }
            }
        }

        public static bool Validate(bool logEverything)
        {
            var r = Run();
            if (logEverything)
            {
                foreach (var p in r.Passed) Debug.Log("[VALIDATE][PASS] " + p);
                foreach (var f in r.Failed) Debug.LogError("[VALIDATE][FAIL] " + f);
                Debug.Log("[VALIDATE] " + r.Passed.Count + " checks passed, " + r.Failed.Count + " failed.");
            }
            return r.Ok;
        }

        public static void ValidateCI()
        {
            if (!Validate(true))
                throw new Exception("ShapeForge production validation failed.");
        }

        public static Result Run()
        {
            var r = new Result();

            // ---- Catalogue ----
            var all = MetaXRSim.App.GameCatalog.All;
            r.Check("Catalogue has exactly 50 games", all.Count == 50, "count=" + all.Count);
            var slugs = new HashSet<string>();
            var perFamily = new Dictionary<MetaXRSim.App.GameFamily, int>();
            foreach (var g in all)
            {
                if (g == null) { r.Check("Catalogue entry not null", false); continue; }
                r.Check("Unique slug " + g.Slug, slugs.Add(g.Slug));
                var rig = MetaXRSim.App.GameCatalog.GetRigType(g.Slug);
                r.Check("Rig type for " + g.Slug, rig != null && typeof(MonoBehaviour).IsAssignableFrom(rig),
                    rig == null ? "null" : rig.FullName);
                if (!perFamily.ContainsKey(g.Family)) perFamily[g.Family] = 0;
                perFamily[g.Family]++;
                r.Check("Nominal size sane " + g.Slug, g.NominalSizeMm >= 20f && g.NominalSizeMm <= 400f);
                r.Check("Instruction authored " + g.Slug, !string.IsNullOrEmpty(g.Instruction));
            }
            r.Check("Five families present", perFamily.Count == 5, "families=" + perFamily.Count);
            foreach (var kv in perFamily)
                r.Check("Family " + kv.Key + " has 10 games", kv.Value == 10, "count=" + kv.Value);

            // ---- Scene / build settings ----
            r.Check("Production scene asset exists", File.Exists(ShapeForgeSetup.ScenePath), ShapeForgeSetup.ScenePath);
            bool inBuild = false;
            foreach (var s in EditorBuildSettings.scenes)
                if (s != null && s.path == ShapeForgeSetup.ScenePath && s.enabled) inBuild = true;
            r.Check("Production scene in build settings", inBuild);

            // ---- XR loader ----
            var androidXr = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            r.Check("Android XR general settings exist", androidXr != null);
            if (androidXr != null)
            {
                r.Check("XR manager initialized on start", androidXr.InitManagerOnStart);
                r.Check("Android XR manager exists", androidXr.Manager != null);
                if (androidXr.Manager != null)
                {
                    bool oculus = false;
                    foreach (var loader in androidXr.Manager.activeLoaders)
                    {
                        if (loader == null) continue;
                        if (loader.GetType().FullName == "Unity.XR.Oculus.OculusLoader" ||
                            loader.GetType().Name.Contains("Oculus")) { oculus = true; break; }
                    }
                    r.Check("Oculus XR loader active for Android", oculus);
                }
            }

            var standaloneXr = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            r.Check("Standalone XR settings exist (simulator)", standaloneXr != null);
            if (standaloneXr != null && standaloneXr.Manager != null)
            {
                bool oculus = false;
                foreach (var loader in standaloneXr.Manager.activeLoaders)
                    if (loader != null && loader.GetType().Name.Contains("Oculus")) { oculus = true; break; }
                r.Check("Oculus XR loader active for Standalone", oculus);
            }

            // ---- Meta project config ----
            var cfg = OVRProjectConfig.CachedProjectConfig;
            r.Check("OVRProjectConfig exists", cfg != null);
            if (cfg != null)
            {
                r.Check("Hand tracking + controllers enabled",
                    cfg.handTrackingSupport == OVRProjectConfig.HandTrackingSupport.ControllersAndHands);
                r.Check("High frequency hand tracking", cfg.handTrackingFrequency == OVRProjectConfig.HandTrackingFrequency.HIGH);
                r.Check("Passthrough required", cfg.insightPassthroughSupport == OVRProjectConfig.FeatureSupport.Required);
                r.Check("Scene support required", cfg.sceneSupport == OVRProjectConfig.FeatureSupport.Required);
                r.Check("Anchors enabled", cfg.anchorSupport == OVRProjectConfig.AnchorSupport.Enabled);
                r.Check("Quest 3 targeted", cfg.targetDeviceTypes != null &&
                    cfg.targetDeviceTypes.Contains(OVRProjectConfig.DeviceType.Quest3));
            }

            // ---- Android manifest ----
            string manifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
            r.Check("Custom Meta AndroidManifest exists", File.Exists(manifestPath));
            if (File.Exists(manifestPath))
            {
                string manifest = File.ReadAllText(manifestPath);
                r.Check("Manifest has HAND_TRACKING permission", manifest.Contains("com.oculus.permission.HAND_TRACKING"));
                r.Check("Manifest has USE_SCENE permission", manifest.Contains("com.oculus.permission.USE_SCENE"));
                r.Check("Manifest has USE_ANCHOR_API permission", manifest.Contains("com.oculus.permission.USE_ANCHOR_API"));
                r.Check("Manifest declares passthrough", manifest.Contains("com.oculus.feature.PASSTHROUGH"));
                r.Check("Manifest declares VR head tracking", manifest.Contains("android.hardware.vr.headtracking"));
            }

            // ---- Player settings ----
            r.Check("Android application id", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == ShapeForgeSetup.AppId);
            r.Check("Product name", PlayerSettings.productName == ShapeForgeSetup.ProductName);
            r.Check("Android IL2CPP", PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP);
            r.Check("Android ARM64", (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0);
            r.Check("Android min SDK 32+", (int)PlayerSettings.Android.minSdkVersion >= 32);
#if UNITY_2022_1_OR_NEWER
            r.Check("Android target SDK 34+", (int)PlayerSettings.Android.targetSdkVersion >= 34);
#endif
            r.Check("Linear color", PlayerSettings.colorSpace == ColorSpace.Linear);
            r.Check("Multithreaded rendering", PlayerSettings.MTRendering);
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            bool vulkan = false;
            foreach (var api in apis) if (api == GraphicsDeviceType.Vulkan) vulkan = true;
            r.Check("Vulkan graphics API", vulkan);
            r.Check("APK (not AAB)", !EditorUserBuildSettings.buildAppBundle);

            // ---- Android build support ----
            r.Check("Android build target installed",
                BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android));

            // ---- IL2CPP preservation ----
            r.Check("link.xml preserves gameplay assembly", File.Exists("Assets/link.xml") &&
                File.ReadAllText("Assets/link.xml").Contains("Assembly-CSharp"));

            // ---- CC0 soundtrack ----
            string[] musicFiles =
            {
                "Assets/Resources/ShapeForgeMusic/hub_observing_the_star.ogg",
                "Assets/Resources/ShapeForgeMusic/mystic_enigmatic.mp3",
                "Assets/Resources/ShapeForgeMusic/arcade_fight_loop.ogg",
                "Assets/Resources/ShapeForgeMusic/curious_i_swear.ogg"
            };
            foreach (var music in musicFiles)
            {
                r.Check("Music asset " + Path.GetFileName(music), File.Exists(music));
                var audioImporter = AssetImporter.GetAtPath(music) as AudioImporter;
                r.Check("Music streaming import " + Path.GetFileName(music),
                    audioImporter != null &&
                    audioImporter.defaultSampleSettings.loadType == AudioClipLoadType.Streaming);
            }
            r.Check("Music credits included", File.Exists("Assets/Resources/ShapeForgeMusic/CREDITS.txt"));

            // ---- Runtime classes present ----
            r.Check("AppBootstrap present", TypeExists("MetaXRSim.App.AppBootstrap"));
            r.Check("ShapeForgeHub present", TypeExists("MetaXRSim.App.ShapeForgeHub"));
            r.Check("HandInputService present", TypeExists("MetaXRSim.App.HandInputService"));
            r.Check("MiniGameHost present", TypeExists("MetaXRSim.App.MiniGameHost"));
            r.Check("ShapeGestureRecognizer present", TypeExists("MetaXRSim.App.ShapeGestureRecognizer"));
            r.Check("RuntimeSelfTest present", TypeExists("MetaXRSim.App.RuntimeSelfTest"));
            r.Check("MusicDirector present", TypeExists("MetaXRSim.App.MusicDirector"));
            r.Check("Tweener present", TypeExists("MetaXRSim.App.Tweener"));
            r.Check("WelcomeOverlay present", TypeExists("MetaXRSim.App.WelcomeOverlay"));
            r.Check("RuntimeStressTest present", TypeExists("MetaXRSim.App.RuntimeStressTest"));
            r.Check("RigDrivenPropGuard present", TypeExists("MetaXRSim.App.RigDrivenPropGuard"));
            r.Check("RigPartClamp present", TypeExists("MetaXRSim.App.RigPartClamp"));

            return r;
        }

        private static bool TypeExists(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetType(name) != null) return true;
            }
            return false;
        }
    }
}


