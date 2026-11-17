using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MetaXRSim.App
{
    /// <summary>
    /// Entry point for the single unified Meta XR application. Creates either the
    /// OVR rig + passthrough + scene probe (headset / simulator) or a desktop
    /// fallback camera, then wires the shape hub and the 50-game host.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class AppBootstrap : MonoBehaviour
    {
        public static AppBootstrap Instance { get; private set; }
        public static bool SelfTestRequested { get; private set; }
        public static bool CaptureRequested { get; private set; }
        public static bool UiSmokeRequested { get; private set; }
        public static bool StressRequested { get; private set; }

        public HandInputService Input { get; private set; }
        public RealObjectProbe Probe { get; private set; }
        public MiniGameHost Host { get; private set; }
        public ShapeForgeHub Hub { get; private set; }
        public OVRCameraRig Rig { get; private set; }
        public OVRManager Manager { get; private set; }
        public OVRPassthroughLayer Passthrough { get; private set; }
        public bool XRActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ParseArgs();
            QualitySettings.vSyncCount = 0;
            Application.runInBackground = true;
            Application.targetFrameRate = 72;

            SetupLighting();
            SetupXR();
            SetupAudio();
            SetupServices();

            var welcomeGo = new GameObject("WelcomeOverlay");
            welcomeGo.transform.SetParent(transform, false);
            var welcome = welcomeGo.AddComponent<WelcomeOverlay>();
            welcome.Begin(Hub, SelfTestRequested || StressRequested);

            if (SelfTestRequested)
            {
                var st = gameObject.AddComponent<RuntimeSelfTest>();
                st.CaptureScreenshots = CaptureRequested;
                st.Begin(Hub, Host);
            }
            else if (StressRequested)
            {
                var stress = gameObject.AddComponent<RuntimeStressTest>();
                stress.Begin(Hub, Host);
            }
            else if (UiSmokeRequested)
            {
                StartCoroutine(UiSmoke(welcome));
            }
        }

        private IEnumerator UiSmoke(WelcomeOverlay welcome)
        {
            yield return new WaitForSeconds(1.8f);
            var clip = Resources.Load<AudioClip>(MusicDirector.HubTrack);
            var buttons = FindObjectsByType<WorldButton>(FindObjectsSortMode.None);
            Debug.Log("[UI_SMOKE] built=" + (welcome != null) + " menuButtons=" + buttons.Length +
                      " musicClip=" + (clip != null) + " music=" + (MusicDirector.Instance != null && MusicDirector.Instance.Enabled) +
                      " blocked=" + (Hub != null && Hub.InputBlocked));
            if (welcome != null) welcome.DismissNow();
            yield return new WaitForSeconds(1.2f);
            Debug.Log("[UI_SMOKE] dismissed blocked=" + (Hub != null && Hub.InputBlocked) +
                      " menuActive=" + (Hub != null && Hub.Menu != null && Hub.Menu.gameObject.activeInHierarchy));
            Application.Quit(0);
        }

        private static void ParseArgs()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.Equals(args[i], "-shapeforge-capture", StringComparison.OrdinalIgnoreCase))
                        CaptureRequested = true;
                    if (string.Equals(args[i], "-shapeforge-selftest", StringComparison.OrdinalIgnoreCase))
                        SelfTestRequested = true;
                    if (string.Equals(args[i], "-shapeforge-stress", StringComparison.OrdinalIgnoreCase))
                        StressRequested = true;
                    if (string.Equals(args[i], "-shapeforge-ui-smoke", StringComparison.OrdinalIgnoreCase))
                        UiSmokeRequested = true;
                }
            }
            catch { }
        }

        private void SetupLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.55f);
            RenderSettings.ambientIntensity = 1f;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;

            var key = new GameObject("KeyLight");
            key.transform.SetParent(transform, false);
            var light = key.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.None;
            key.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            var fill = new GameObject("FillLight");
            fill.transform.SetParent(transform, false);
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.65f, 0.82f, 1f);
            fillLight.intensity = 0.55f;
            fillLight.shadows = LightShadows.None;
            fill.transform.rotation = Quaternion.Euler(-28f, 150f, 0f);
        }

        private void SetupXR()
        {
            bool hmd = OVRManager.isHmdPresent;
            XRActive = hmd && UnityEngine.XR.XRSettings.isDeviceActive;

            if (!XRActive)
            {
                CreateDesktopCamera();
                Debug.Log("[AppBootstrap] Desktop preview mode (no active XR device).");
                return;
            }

            var rigGo = new GameObject("XR_Rig");
            rigGo.transform.SetParent(transform, false);

            Manager = rigGo.AddComponent<OVRManager>();
            Manager.isInsightPassthroughEnabled = true;
            Manager.enableMixedReality = false;
            Manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;

            Rig = rigGo.AddComponent<OVRCameraRig>();
            try { Rig.EnsureGameObjectIntegrity(); } catch { }

            Passthrough = rigGo.AddComponent<OVRPassthroughLayer>();
            Passthrough.hidden = false;
            Passthrough.overridePerLayerColorScaleAndOffset = false;

            if (Rig.centerEyeAnchor != null)
            {
                var cam = Rig.centerEyeAnchor.GetComponent<Camera>() ?? Rig.centerEyeAnchor.gameObject.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.nearClipPlane = 0.02f;
                cam.farClipPlane = 30f;
                if (Rig.centerEyeAnchor.GetComponent<AudioListener>() == null)
                    Rig.centerEyeAnchor.gameObject.AddComponent<AudioListener>();
            }

            Debug.Log("[AppBootstrap] XR rig active (OVRPlugin + passthrough).");
        }

        private void CreateDesktopCamera()
        {
            var go = new GameObject("DesktopCamera");
            go.tag = "MainCamera";
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(0f, 1.45f, -0.55f);
            go.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1.08f, 0.35f) - go.transform.position);
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 68f;
            cam.nearClipPlane = 0.02f;
            cam.farClipPlane = 30f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.055f, 0.065f, 0.085f);
            go.AddComponent<AudioListener>();
        }

        private void SetupAudio()
        {
            if (AudioDirector.Instance == null)
            {
                var go = new GameObject("AudioDirector");
                go.transform.SetParent(transform, false);
                go.AddComponent<AudioDirector>();
            }

            if (MusicDirector.Instance == null)
            {
                var music = new GameObject("MusicDirector");
                music.transform.SetParent(transform, false);
                music.AddComponent<MusicDirector>();
            }
        }

        private void SetupServices()
        {
            var inputGo = new GameObject("HandInputService");
            inputGo.transform.SetParent(transform, false);
            Input = inputGo.AddComponent<HandInputService>();
            Input.Initialize(Rig);

            var probeGo = new GameObject("RealObjectProbe");
            probeGo.transform.SetParent(transform, false);
            Probe = probeGo.AddComponent<RealObjectProbe>();
            Probe.Initialize(Input);

            var hostGo = new GameObject("MiniGameHost");
            hostGo.transform.SetParent(transform, false);
            Host = hostGo.AddComponent<MiniGameHost>();

            var hubGo = new GameObject("ShapeForgeHub");
            hubGo.transform.SetParent(transform, false);
            Hub = hubGo.AddComponent<ShapeForgeHub>();
            Hub.Initialize(Input, Probe, Host);

            if (ManifestSanityCheck() == false)
            {
                Debug.LogError("[AppBootstrap] Catalogue sanity check failed. Expected 50 games.");
            }
        }

        private static bool ManifestSanityCheck()
        {
            if (GameCatalog.All.Count != GameCatalog.ExpectedGameCount) return false;
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var g in GameCatalog.All)
            {
                if (g == null || string.IsNullOrEmpty(g.Slug) || !seen.Add(g.Slug)) return false;
                if (GameCatalog.GetRigType(g.Slug) == null)
                {
                    Debug.LogError("[AppBootstrap] Missing rig type: " + g.RigTypeName);
                    return false;
                }
            }
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (Instance != null) return;
            if (UnityEngine.Object.FindFirstObjectByType<AppBootstrap>() != null) return;
            var go = new GameObject("ShapeForgeAutoBootstrap");
            go.AddComponent<AppBootstrap>();
            Debug.Log("[AppBootstrap] Scene had no bootstrap; created a runtime fallback.");
        }
    }
}


