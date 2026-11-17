using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

namespace MetaXRSim.EditorTools
{
    /// <summary>
    /// One-click project hardening: XR loader, Meta feature permissions, Android
    /// player settings and the production scene. All steps are idempotent.
    /// </summary>
    public static class ShapeForgeSetup
    {
        public const string ScenePath = "Assets/Scenes/ShapeForge.unity";
        public const string AppId = "com.shapeforge.xr50";
        public const string ProductName = "ShapeForge 50";

        [MenuItem("ShapeForge/1. Configure Production Project")]
        public static void Configure()
        {
            EnsureFolders();
            ConfigurePlayer();
            ConfigureXR();
            ConfigureMeta();
            ConfigureQuality();
            EnsureIcon();
            ConfigureMusic();
            BuildScene();
            Debug.Log("[ShapeForge] Project configured for production.");
        }

        [MenuItem("ShapeForge/2. Build Production Scene")]
        public static void BuildScene()
        {
            EnsureFolders();
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            var root = new GameObject("ShapeForgeApp");
            root.AddComponent<MetaXRSim.App.AppBootstrap>();

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[ShapeForge] Production scene written to " + ScenePath);
        }

        [MenuItem("ShapeForge/3. Validate Production Readiness")]
        public static void ValidateMenu()
        {
            bool ok = ShapeForgeValidator.Validate(true);
            EditorUtility.DisplayDialog("ShapeForge Validation", ok ? "All production checks passed." : "Validation failed - see Console.", "OK");
        }

        public static void ConfigureForBuild()
        {
            ConfigurePlayer();
            ConfigureXR();
            ConfigureMeta();
            ConfigureQuality();
            EnsureIcon();
            ConfigureMusic();
            if (!File.Exists(ScenePath)) BuildScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/XR")) AssetDatabase.CreateFolder("Assets", "XR");
            if (!AssetDatabase.IsValidFolder("Assets/MetaXRSimApp/Art")) AssetDatabase.CreateFolder("Assets/MetaXRSimApp", "Art");
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "ShapeForge Labs";
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.MTRendering = true;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AppId);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, AppId);

            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.androidIsGame = true;
            PlayerSettings.Android.androidTVCompatibility = false;
            PlayerSettings.Android.useAPKExpansionFiles = false;
            PlayerSettings.Android.forceInternetPermission = false;

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSize);

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            PlayerSettings.Android.blitType = AndroidBlitType.Always;

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            PlayerSettings.SplashScreen.backgroundColor = Color.black;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }

        private static void ConfigureXR()
        {
            var perBuild = GetPerBuildTargetSettings();
            if (perBuild == null)
            {
                Debug.LogWarning("[ShapeForge] XR management settings could not be created; is com.unity.xr.management installed?");
                return;
            }

            ConfigureGroup(perBuild, BuildTargetGroup.Android);
            ConfigureGroup(perBuild, BuildTargetGroup.Standalone);

            EditorUtility.SetDirty(perBuild);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureGroup(XRGeneralSettingsPerBuildTarget perBuild, BuildTargetGroup group)
        {
            perBuild.CreateDefaultManagerSettingsForBuildTarget(group);
            var settings = perBuild.SettingsForBuildTarget(group);
            if (settings == null)
            {
                Debug.LogWarning("[ShapeForge] No XRGeneralSettings for " + group);
                return;
            }
            settings.InitManagerOnStart = true;
            if (settings.Manager == null)
            {
                perBuild.CreateDefaultManagerSettingsForBuildTarget(group);
                settings = perBuild.SettingsForBuildTarget(group);
            }
            if (settings.Manager != null)
            {
                bool assigned = XRPackageMetadataStore.AssignLoader(settings.Manager,
                    "Unity.XR.Oculus.OculusLoader", group);
                Debug.Log("[ShapeForge] Oculus loader assigned for " + group + ": " + assigned);
                EditorUtility.SetDirty(settings.Manager);
            }
            EditorUtility.SetDirty(settings);
        }

        private static XRGeneralSettingsPerBuildTarget GetPerBuildTargetSettings()
        {
            var type = typeof(XRGeneralSettingsPerBuildTarget);
            var method = type.GetMethod("GetOrCreate", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                // Fallback for package versions that expose the accessor differently.
                foreach (var m in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (m.Name.Contains("GetOrCreate")) { method = m; break; }
                }
            }
            if (method == null) return null;
            try { return method.Invoke(null, null) as XRGeneralSettingsPerBuildTarget; }
            catch (Exception e)
            {
                Debug.LogWarning("[ShapeForge] XR settings create failed: " + e.Message);
                return null;
            }
        }

        private static void ConfigureMeta()
        {
            var cfg = OVRProjectConfig.CachedProjectConfig;
            if (cfg == null)
            {
                Debug.LogWarning("[ShapeForge] OVRProjectConfig unavailable.");
                return;
            }

            cfg.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
            cfg.handTrackingFrequency = OVRProjectConfig.HandTrackingFrequency.HIGH;
            cfg.anchorSupport = OVRProjectConfig.AnchorSupport.Enabled;
            cfg.sceneSupport = OVRProjectConfig.FeatureSupport.Required;
            cfg.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Required;
            cfg.renderModelSupport = OVRProjectConfig.RenderModelSupport.Enabled;
            cfg.experimentalFeaturesEnabled = false;
            cfg.focusAware = true;
            cfg.skipUnneededShaders = false;

            if (cfg.targetDeviceTypes == null) cfg.targetDeviceTypes = new List<OVRProjectConfig.DeviceType>();
            cfg.targetDeviceTypes.Clear();
            cfg.targetDeviceTypes.Add(OVRProjectConfig.DeviceType.Quest2);
            cfg.targetDeviceTypes.Add(OVRProjectConfig.DeviceType.QuestPro);
            cfg.targetDeviceTypes.Add(OVRProjectConfig.DeviceType.Quest3);
            cfg.targetDeviceTypes.Add(OVRProjectConfig.DeviceType.Quest3S);

            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();

            // The Meta manifest tool is normally a menu action. Run it in CI so
            // the APK contains the hand-tracking, scene, anchor and passthrough
            // permissions that OVRProjectConfig just enabled.
            try
            {
                OVRManifestPreprocessor.GenerateOrUpdateAndroidManifest(true);
                Debug.Log("[ShapeForge] AndroidManifest.xml synchronized with Meta feature flags.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ShapeForge] Could not update AndroidManifest.xml: " + e.Message);
            }

            Debug.Log("[ShapeForge] Meta features: hands+controllers, passthrough, anchors, scene.");
        }

        private static void ConfigureQuality()
        {
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.billboardsFaceCameraPosition = true;
            QualitySettings.skinWeights = SkinWeights.TwoBones;
            EnsureAlwaysIncludedShaders("Sprites/Default", "Unlit/Color", "Legacy Shaders/Diffuse");
        }

        private static void EnsureAlwaysIncludedShaders(params string[] shaderNames)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var graphics = assets[0];
            if (graphics == null) return;

            var serialized = new SerializedObject(graphics);
            var list = serialized.FindProperty("m_AlwaysIncludedShaders");
            if (list == null) return;

            var present = new HashSet<Shader>();
            bool changed = false;
            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var s = list.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
                if (s == null) continue;
                if (s.name == "Standard" || s.name == "Particles/Standard Unlit")
                {
                    list.DeleteArrayElementAtIndex(i);
                    changed = true;
                    continue;
                }
                present.Add(s);
            }

            foreach (var name in shaderNames)
            {
                var shader = Shader.Find(name);
                if (shader == null)
                {
                    Debug.LogWarning("[ShapeForge] Shader not found for always-include: " + name);
                    continue;
                }
                if (!present.Add(shader)) continue;
                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = shader;
                changed = true;
            }

            if (changed)
            {
                serialized.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log("[ShapeForge] Always-included runtime shaders updated.");
            }
        }

        private static void ConfigureMusic()
        {
            string[] clips =
            {
                "Assets/Resources/ShapeForgeMusic/hub_observing_the_star.ogg",
                "Assets/Resources/ShapeForgeMusic/mystic_enigmatic.mp3",
                "Assets/Resources/ShapeForgeMusic/arcade_fight_loop.ogg",
                "Assets/Resources/ShapeForgeMusic/curious_i_swear.ogg"
            };

            int configured = 0;
            foreach (var path in clips)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning("[ShapeForge] Music file missing: " + path);
                    continue;
                }

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.62f;
                importer.defaultSampleSettings = settings;
                importer.forceToMono = false;
                importer.loadInBackground = true;
                importer.SaveAndReimport();
                configured++;
            }

            string credits = "Assets/Resources/ShapeForgeMusic/CREDITS.txt";
            if (File.Exists(credits)) AssetDatabase.ImportAsset(credits, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[ShapeForge] Music import configured: " + configured + "/4 streaming CC0 tracks.");
        }

        private static void EnsureIcon()
        {
            const string path = "Assets/MetaXRSimApp/Art/AppIcon.png";
            if (!File.Exists(path)) GenerateIcon(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (icon == null) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }
            try
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new[] { icon });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new[] { icon });
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ShapeForge] Icon assignment skipped: " + e.Message);
            }
        }

        private static void GenerateIcon(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color bgA = new Color(0.035f, 0.05f, 0.085f);
            Color bgB = new Color(0.07f, 0.16f, 0.22f);
            Color ring = new Color(0.25f, 0.9f, 1f);
            Color accent = new Color(1f, 0.62f, 0.28f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);
                    float v = y / (float)(size - 1);
                    Color c = Color.Lerp(bgA, bgB, Mathf.Clamp01(v * 0.9f + u * 0.2f));

                    float dx = u - 0.5f;
                    float dy = v - 0.52f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d < 0.29f && d > 0.245f) c = Color.Lerp(c, ring, 0.9f);
                    if (d < 0.19f && d > 0.155f) c = Color.Lerp(c, ring * 0.8f, 0.7f);

                    // Ellipse pair = sphere + spheroid language.
                    float e1 = (dx * dx) / (0.11f * 0.11f) + (dy * dy) / (0.11f * 0.11f);
                    if (e1 < 1f && e1 > 0.55f) c = Color.Lerp(c, accent, 0.55f);
                    float e2x = u - 0.5f;
                    float e2y = v - 0.28f;
                    float e2 = (e2x * e2x) / (0.13f * 0.065f) + (e2y * e2y) / (0.065f * 0.065f);
                    if (e2 < 1f && e2 > 0.55f) c = Color.Lerp(c, accent, 0.45f);

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
        }
    }
}


