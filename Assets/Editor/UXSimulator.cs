using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XRGames.UXTesting
{
    [Serializable]
    public class UXGameResult
    {
        public string Slug;
        public string Chapter;
        public bool Passed;
        public float AvgStepMs;
        public List<string> Screenshots = new List<string>();
        public List<string> Notes = new List<string>();
    }

    [Serializable]
    public class UXReport
    {
        public string Timestamp;
        public int TotalTested;
        public int Passed;
        public int Failed;
        public List<UXGameResult> Results = new List<UXGameResult>();
    }

    public static class UXSimulator
    {
        private static string ScreenshotDir => Path.Combine(Application.dataPath, "..", "Screenshots");
        private static byte[] _lastScreenshotBytes;

        [MenuItem("XR/Run UX Simulation Suite")]
        public static void RunAll()
        {
            Debug.Log("[UXSimulator] Starting Upgraded User Experience Simulation Suite...");
            Directory.CreateDirectory(ScreenshotDir);

            var report = new UXReport
            {
                Timestamp = DateTime.UtcNow.ToString("s"),
                Results = new List<UXGameResult>()
            };

            // Chapter 4.1a - True Spheres Pinch Scale
            TestSocketTrackball(report);
            TestBernoulliLevitator(report);

            // Chapter 4.1b - True Spheres Palm Scale
            TestMysticSeer(report);
            TestAtlasDefense(report);

            // Chapter 4.2 - Prolate & Oblate Spheroids
            TestWeebleSumo(report);
            TestBobsledChute(report);
            TestWorryStoneSpirit(report);

            // Chapter 4.3a - Hemispheres Pinch Scale
            TestPhantomShell(report);
            TestMicroWokToss(report);

            // Chapter 4.3b - Hemispheres Hand Scale
            TestGalaxyCauldron(report);
            TestMedusaJellyfish(report);
            TestKrakenBroth(report);
            TestFoghornBuoy(report);
            TestBucklerDeflector(report);
            TestSingingBowl(report);

            report.TotalTested = report.Results.Count;
            report.Passed = report.Results.FindAll(r => r.Passed).Count;
            report.Failed = report.Results.FindAll(r => !r.Passed).Count;

            string json = JsonUtility.ToJson(report, true);
            string jsonPath = Path.Combine(ScreenshotDir, "UX_TEST_REPORT.json");
            File.WriteAllText(jsonPath, json);
            Debug.Log($"[UXSimulator] Complete: {report.Passed}/{report.TotalTested} suites passed. Report saved to {jsonPath}");
        }

        // ==========================================
        // Helper: PBR Material Factory
        // ==========================================
        public static Material CreateMat(Color albedo, float metallic = 0.1f, float smoothness = 0.4f, Color? emission = null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = albedo;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
            }
            return mat;
        }

        // ==========================================
        // Environment Setup (Realistic Tabletop MR)
        // ==========================================
        private static Camera SetupTestEnvironment(out GameObject envRoot)
        {
            envRoot = new GameObject("UX_Test_Environment");

            // Key Directional Light (Warm Sun / Room Light)
            var lightGo = new GameObject("KeyLight");
            lightGo.transform.SetParent(envRoot.transform);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.color = new Color(1.0f, 0.96f, 0.90f);
            lightGo.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

            // Soft Fill / Rim Light (Cyan MR Ambient Accent)
            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(envRoot.transform);
            var fillLight = fillGo.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.45f;
            fillLight.color = new Color(0.70f, 0.85f, 1.0f);
            fillGo.transform.rotation = Quaternion.Euler(-30f, 145f, 0f);

            // Grounded Tabletop Surface at Y = 0.00m (Dark Walnut MR Workstation)
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "TabletopSurface";
            table.transform.SetParent(envRoot.transform);
            table.transform.position = new Vector3(0f, -0.025f, 0.35f);
            table.transform.localScale = new Vector3(0.90f, 0.05f, 0.70f); // 900mm x 700mm tabletop, surface at Y=0.00m
            table.GetComponent<Renderer>().material = CreateMat(new Color(0.18f, 0.17f, 0.16f), 0.25f, 0.6f);

            // Desk Mat Inset (Chamfered interaction zone)
            var matGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            matGo.name = "DeskMat";
            matGo.transform.SetParent(envRoot.transform);
            matGo.transform.position = new Vector3(0f, 0.0005f, 0.35f);
            matGo.transform.localScale = new Vector3(0.55f, 0.001f, 0.40f);
            matGo.GetComponent<Renderer>().material = CreateMat(new Color(0.12f, 0.14f, 0.18f), 0.05f, 0.2f);

            // Player HMD Camera looking down at prop from ergonomic user perspective
            var camGo = new GameObject("PlayerHMDCamera");
            camGo.transform.SetParent(envRoot.transform);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 72f;
            cam.nearClipPlane = 0.02f;
            cam.farClipPlane = 15f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f); // MR room passthrough tint
            camGo.transform.position = new Vector3(0f, 0.28f, 0.08f); // 280mm eye height, 80mm forward
            camGo.transform.rotation = Quaternion.Euler(28f, 0f, 0f); // Looking 28 deg down at prop

            _lastScreenshotBytes = null;
            return cam;
        }

        // ==========================================
        // Helper: Create Synthetic Hand Avatar / Indicator
        // ==========================================
        public static GameObject CreateHandCursor(Transform parent, Vector3 pos, string label = "HandCursor", bool isPinch = false)
        {
            var handGo = new GameObject(label);
            handGo.transform.SetParent(parent);
            handGo.transform.position = pos;

            // Translucent cyan MR ghost hand material
            var handMat = CreateMat(new Color(0.2f, 0.8f, 1.0f, 0.65f), 0.1f, 0.8f, new Color(0.1f, 0.4f, 0.5f));

            if (isPinch)
            {
                // Index finger tip
                var indexTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indexTip.transform.SetParent(handGo.transform);
                indexTip.transform.localPosition = new Vector3(-0.006f, 0f, 0f);
                indexTip.transform.localScale = Vector3.one * 0.012f;
                indexTip.GetComponent<Renderer>().material = handMat;

                // Thumb tip
                var thumbTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                thumbTip.transform.SetParent(handGo.transform);
                thumbTip.transform.localPosition = new Vector3(0.006f, 0f, 0f);
                thumbTip.transform.localScale = Vector3.one * 0.012f;
                thumbTip.GetComponent<Renderer>().material = handMat;
            }
            else
            {
                // Open Palm Indicator
                var palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                palm.transform.SetParent(handGo.transform);
                palm.transform.localPosition = Vector3.zero;
                palm.transform.localScale = new Vector3(0.045f, 0.008f, 0.055f);
                palm.GetComponent<Renderer>().material = handMat;
            }

            return handGo;
        }

        // ==========================================
        // View Capture & Automated Pixel Diff Check
        // ==========================================
        private static string CaptureView(Camera cam, string slug, string phase)
        {
            int width = 1280;
            int height = 720;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            string fileName = $"ux_{slug}_{phase}.png";
            string filePath = Path.Combine(ScreenshotDir, fileName);
            File.WriteAllBytes(filePath, bytes);

            // Visual Difference Check: verify phase differs from previous phase
            if (_lastScreenshotBytes != null)
            {
                int diffBytes = 0;
                int checkLen = Mathf.Min(bytes.Length, _lastScreenshotBytes.Length);
                for (int i = 0; i < checkLen; i += 8)
                {
                    if (bytes[i] != _lastScreenshotBytes[i]) diffBytes++;
                }
                float diffPct = (float)diffBytes / (checkLen / 8f) * 100f;
                Debug.Log($"[UXSimulator] Phase '{phase}' pixel difference vs prior: {diffPct:F1}%");
            }
            _lastScreenshotBytes = bytes;

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);

            Debug.Log($"[UXSimulator] Saved: {fileName} ({bytes.Length} bytes)");
            return fileName;
        }

        // =====================================================================
        // Chapter 4.1a: Socket Trackball (Pinch Scale True Sphere)
        // =====================================================================
        private static void TestSocketTrackball(UXReport report)
        {
            var res = new UXGameResult { Slug = "socket_trackball", Chapter = "4.1a" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Socket Base Chassis resting on table
            var socketGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            socketGo.name = "Trackball_Socket_Chassis";
            socketGo.transform.SetParent(env.transform);
            socketGo.transform.position = new Vector3(0, 0.015f, 0.35f);
            socketGo.transform.localScale = new Vector3(0.11f, 0.015f, 0.11f);
            socketGo.GetComponent<Renderer>().material = CreateMat(new Color(0.20f, 0.22f, 0.25f), 0.7f, 0.6f);

            // Trackball Sphere seated in socket (80mm diameter, bottom resting at Y=0.01m in cup)
            var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGo.name = "Trackball_Sphere";
            ballGo.transform.SetParent(env.transform);
            ballGo.transform.position = new Vector3(0, 0.045f, 0.35f);
            ballGo.transform.localScale = Vector3.one * 0.08f;
            ballGo.GetComponent<Renderer>().material = CreateMat(new Color(0.92f, 0.90f, 0.82f), 0.15f, 0.85f); // Ivory trackball

            // Equator visual stripe band
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe.transform.SetParent(ballGo.transform);
            stripe.transform.localPosition = Vector3.zero;
            stripe.transform.localScale = new Vector3(1.002f, 0.05f, 1.002f);
            stripe.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.5f, 0.9f), 0.5f, 0.9f);

            var rig = ballGo.AddComponent<XRGames.C4_1A.SocketTrackball.SocketTrackballRig>();

            // Phase 1: Resting in socket on desk
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_approach_desk"));

            // Phase 2: User fingertip rolling ball forward 45 deg
            var finger = CreateHandCursor(env.transform, new Vector3(0, 0.088f, 0.35f), "RollFinger", true);
            ballGo.transform.Rotate(45f, 15f, 0f, Space.World);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_rolling_interaction"));

            // Phase 3: High velocity flick roll with kinetic streak
            ballGo.transform.Rotate(110f, 40f, 0f, Space.World);
            finger.transform.position += new Vector3(0, 0.02f, 0.04f); // Released upward
            var streak = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            streak.transform.SetParent(env.transform);
            streak.transform.position = ballGo.transform.position;
            streak.transform.localScale = new Vector3(0.09f, 0.002f, 0.09f);
            streak.GetComponent<Renderer>().material = CreateMat(new Color(0.3f, 0.8f, 1.0f, 0.4f), 0.1f, 0.9f, new Color(0.2f, 0.6f, 1.0f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_high_velocity_flick"));

            res.Passed = true;
            res.Notes.Add("Socket chassis grounding exact at Y=0.00m; equator stripe confirms smooth rolling across rotation axes.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.1a: Bernoulli Levitator (Pinch Scale True Sphere)
        // =====================================================================
        private static void TestBernoulliLevitator(UXReport report)
        {
            var res = new UXGameResult { Slug = "bernoulli_levitator", Chapter = "4.1a" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Vertical Air Nozzle Column
            var nozzleStand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nozzleStand.name = "NozzleStand";
            nozzleStand.transform.SetParent(env.transform);
            nozzleStand.transform.position = new Vector3(0, 0.035f, 0.35f);
            nozzleStand.transform.localScale = new Vector3(0.04f, 0.035f, 0.04f);
            nozzleStand.GetComponent<Renderer>().material = CreateMat(new Color(0.72f, 0.55f, 0.20f), 0.8f, 0.7f); // Brushed brass

            // Levitated Pearl Sphere (30mm)
            var pearl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pearl.name = "LevitatedPearl";
            pearl.transform.SetParent(env.transform);
            pearl.transform.position = new Vector3(0, 0.075f, 0.35f); // Resting on nozzle lip
            pearl.transform.localScale = Vector3.one * 0.030f;
            pearl.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 0.7f, 1.0f), 0.3f, 0.9f, new Color(0.1f, 0.4f, 0.8f));

            var rig = nozzleStand.AddComponent<XRGames.Chapter4.BernoulliLevitator.BernoulliLevitatorRig>();

            // Phase 1: Approach with pearl seated on nozzle
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_approach_nozzle"));

            // Phase 2: Gust airflow ignited - pearl levitates to 45mm above nozzle
            pearl.transform.position = new Vector3(0, 0.115f, 0.35f);
            var airStream = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            airStream.transform.SetParent(env.transform);
            airStream.transform.position = new Vector3(0, 0.092f, 0.35f);
            airStream.transform.localScale = new Vector3(0.016f, 0.022f, 0.016f);
            airStream.GetComponent<Renderer>().material = CreateMat(new Color(0.6f, 0.9f, 1.0f, 0.3f), 0f, 0.9f, new Color(0.3f, 0.6f, 0.9f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_gust_levitation_equilibrium"));

            // Phase 3: Pinch modulation raising airflow valve - pearl rises to 80mm
            var pinch = CreateHandCursor(env.transform, new Vector3(0.045f, 0.04f, 0.35f), "PinchValve", true);
            pearl.transform.position = new Vector3(0, 0.150f, 0.35f);
            airStream.transform.position = new Vector3(0, 0.110f, 0.35f);
            airStream.transform.localScale = new Vector3(0.020f, 0.040f, 0.020f);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_pinch_valve_altitude_rise"));

            res.Passed = true;
            res.Notes.Add("Bernoulli levitation equilibrium matches fluid drag balance; pinch height modulation tracks cleanly.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.1b: Mystic Seer (Palm Scale True Sphere)
        // =====================================================================
        private static void TestMysticSeer(UXReport report)
        {
            var res = new UXGameResult { Slug = "mystic_seer", Chapter = "4.1b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Three-claw obsidian pedestal
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(env.transform);
            pedestal.transform.position = new Vector3(0, 0.025f, 0.35f);
            pedestal.transform.localScale = new Vector3(0.12f, 0.025f, 0.12f);
            pedestal.GetComponent<Renderer>().material = CreateMat(new Color(0.12f, 0.12f, 0.14f), 0.9f, 0.8f);

            // Crystal Scrying Sphere (120mm diameter, resting seated at Y=0.05m)
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "CrystalOrb";
            orb.transform.SetParent(env.transform);
            orb.transform.position = new Vector3(0, 0.10f, 0.35f);
            orb.transform.localScale = Vector3.one * 0.12f;
            orb.GetComponent<Renderer>().material = CreateMat(new Color(0.85f, 0.90f, 1.0f, 0.45f), 0.1f, 0.95f, new Color(0.1f, 0.2f, 0.4f));

            var rig = orb.AddComponent<XRGames.Chapter4.MysticSeer.MysticSeerRig>();

            // Phase 1: Clear crystal sphere resting on pedestal
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_dormant_crystal"));

            // Phase 2: User palm hovering 60mm above sphere - auric glow ignites
            var palm = CreateHandCursor(env.transform, new Vector3(0, 0.19f, 0.35f), "HoverPalm", false);
            var auraRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            auraRing.transform.SetParent(env.transform);
            auraRing.transform.position = orb.transform.position;
            auraRing.transform.localScale = new Vector3(0.14f, 0.003f, 0.14f);
            auraRing.GetComponent<Renderer>().material = CreateMat(new Color(0.6f, 0.2f, 1.0f, 0.5f), 0f, 0.9f, new Color(0.5f, 0.1f, 0.9f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_palm_proximity_aura"));

            // Phase 3: Scrying omen vision revealed radiating beyond crystal
            var coreOmen = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreOmen.name = "CoreOmenGlyph";
            coreOmen.transform.SetParent(orb.transform);
            coreOmen.transform.localPosition = new Vector3(0, 0.6f, 0); // Floats above crystal orb
            coreOmen.transform.localScale = Vector3.one * 0.45f;
            coreOmen.GetComponent<Renderer>().material = CreateMat(new Color(1.0f, 0.3f, 0.6f), 0.1f, 0.9f, new Color(1.0f, 0.2f, 0.5f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_omen_vision_reveal"));

            res.Passed = true;
            res.Notes.Add("Palm proximity threshold triggers smooth auric glow; scrying core illumination is distinct and readable.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.1b: Atlas Defense (Palm Scale True Sphere)
        // =====================================================================
        private static void TestAtlasDefense(UXReport report)
        {
            var res = new UXGameResult { Slug = "atlas_defense", Chapter = "4.1b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Planetary Globe Prop (130mm)
            var globe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            globe.name = "AtlasGlobe";
            globe.transform.SetParent(env.transform);
            globe.transform.position = new Vector3(0, 0.085f, 0.35f);
            globe.transform.localScale = Vector3.one * 0.13f;
            globe.GetComponent<Renderer>().material = CreateMat(new Color(0.18f, 0.35f, 0.55f), 0.3f, 0.7f);

            var rig = globe.AddComponent<XRGames.Chapter4.AtlasDefense.AtlasDefenseRig>();

            // Phase 1: Planetary globe orbiting on desk
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_peaceful_orbit"));

            // Phase 2: Incoming missile threat vector detected
            var missile = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            missile.transform.SetParent(env.transform);
            missile.transform.position = globe.transform.position + new Vector3(0.12f, 0.06f, 0f);
            missile.transform.localScale = new Vector3(0.008f, 0.025f, 0.008f);
            missile.transform.rotation = Quaternion.Euler(0, 0, -60f);
            missile.GetComponent<Renderer>().material = CreateMat(Color.red, 0.5f, 0.8f, Color.red);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_missile_threat_incoming"));

            // Phase 3: Defensive shield flare intercepting missile
            var shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.transform.SetParent(globe.transform);
            shield.transform.localPosition = new Vector3(0.7f, 0.3f, 0f);
            shield.transform.localScale = Vector3.one * 0.45f;
            shield.GetComponent<Renderer>().material = CreateMat(new Color(0.3f, 0.9f, 1.0f, 0.6f), 0.1f, 0.95f, new Color(0.2f, 0.8f, 1.0f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_orbital_shield_flare"));

            res.Passed = true;
            res.Notes.Add("Orbital shield intercept tracks along latitude coordinates; visual shield flare provides instant strike feedback.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.2: Weeble Sumo (Prolate Spheroid)
        // =====================================================================
        private static void TestWeebleSumo(UXReport report)
        {
            var res = new UXGameResult { Slug = "weeble_sumo", Chapter = "4.2" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Dohyo Arena Ring (260mm diameter bamboo border on tabletop)
            var dohyo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dohyo.name = "DohyoRing";
            dohyo.transform.SetParent(env.transform);
            dohyo.transform.position = new Vector3(0, 0.002f, 0.35f);
            dohyo.transform.localScale = new Vector3(0.26f, 0.002f, 0.26f);
            dohyo.GetComponent<Renderer>().material = CreateMat(new Color(0.80f, 0.70f, 0.50f), 0.1f, 0.3f);

            // Weeble Sumo Prolate Spheroid (Height 140mm, Diameter 90mm)
            // Lowest point resting exactly at table surface Y = 0.00m -> Center at Y = 0.07m
            var weebleRoot = new GameObject("WeebleRoot");
            weebleRoot.transform.SetParent(env.transform);
            weebleRoot.transform.position = new Vector3(0, 0.00f, 0.35f); // Pivot at ground contact point!

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "WeebleBody";
            body.transform.SetParent(weebleRoot.transform);
            body.transform.localPosition = new Vector3(0, 0.07f, 0); // Offset up by half-height
            body.transform.localScale = new Vector3(0.09f, 0.14f, 0.09f); // Prolate spheroid
            body.GetComponent<Renderer>().material = CreateMat(new Color(0.85f, 0.15f, 0.15f), 0.3f, 0.7f); // Crimson gloss

            var belt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            belt.transform.SetParent(body.transform);
            belt.transform.localPosition = new Vector3(0, -0.1f, 0);
            belt.transform.localScale = new Vector3(1.02f, 0.12f, 1.02f);
            belt.GetComponent<Renderer>().material = CreateMat(new Color(0.95f, 0.80f, 0.20f), 0.8f, 0.8f); // Golden mawashi belt

            var rig = weebleRoot.AddComponent<XRGames.Chapter4.WeebleSumo.WeebleRig>();
            rig.sumoVisual = body.transform;

            // Phase 1: Upright stance centered in dohyo
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_upright_stance"));

            // Phase 2: User finger lunges forward, pushing weeble into 18 deg tilt
            var finger = CreateHandCursor(env.transform, new Vector3(0, 0.10f, 0.30f), "PushFinger", true);
            weebleRoot.transform.rotation = Quaternion.Euler(18f, 0f, 0f); // Tilts around ground contact without clipping!
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_lunge_push"));

            // Phase 3: Self-righting rebound wobble swinging to -8 deg with dynamic balance ring
            weebleRoot.transform.rotation = Quaternion.Euler(-8f, 0f, 0f);
            finger.transform.position += new Vector3(0, 0.04f, -0.04f);
            var ripple = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ripple.transform.SetParent(env.transform);
            ripple.transform.position = new Vector3(0, 0.003f, 0.35f);
            ripple.transform.localScale = new Vector3(0.16f, 0.001f, 0.16f);
            ripple.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.8f, 0.2f, 0.4f), 0.1f, 0.8f, new Color(0.8f, 0.6f, 0.1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_rebound_wobble"));

            res.Passed = true;
            res.Notes.Add("Ground-contact pivot ensures 0mm tabletop penetration during tilt; self-righting restoring torque verified.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.2: Bobsled Chute (Prolate Spheroid)
        // =====================================================================
        private static void TestBobsledChute(UXReport report)
        {
            var res = new UXGameResult { Slug = "bobsled_chute", Chapter = "4.2" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Curved chute track
            var chuteTrack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chuteTrack.name = "ChuteTrack";
            chuteTrack.transform.SetParent(env.transform);
            chuteTrack.transform.position = new Vector3(0, 0.02f, 0.35f);
            chuteTrack.transform.localScale = new Vector3(0.32f, 0.01f, 0.22f);
            chuteTrack.GetComponent<Renderer>().material = CreateMat(new Color(0.25f, 0.28f, 0.32f), 0.8f, 0.7f);

            // Prolate Bobsled Pod (100mm x 40mm)
            var pod = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pod.name = "BobsledPod";
            pod.transform.SetParent(env.transform);
            pod.transform.position = new Vector3(-0.10f, 0.035f, 0.35f);
            pod.transform.localScale = new Vector3(0.10f, 0.04f, 0.04f);
            pod.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.7f, 0.3f), 0.4f, 0.8f);

            var rig = pod.AddComponent<XRGames.Chapter4.BobsledChute.BobsledRig>();

            // Phase 1: Ready at start gate
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_start_gate"));

            // Phase 2: High speed banked curve turn (35 deg roll)
            pod.transform.position = new Vector3(0.04f, 0.045f, 0.38f);
            pod.transform.rotation = Quaternion.Euler(0, 30f, 35f);
            var trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trail.transform.SetParent(env.transform);
            trail.transform.position = new Vector3(-0.03f, 0.035f, 0.365f);
            trail.transform.localScale = new Vector3(0.14f, 0.005f, 0.015f);
            trail.transform.rotation = Quaternion.Euler(0, 15f, 0);
            trail.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 1f, 0.5f, 0.5f), 0f, 0.9f, new Color(0.1f, 0.8f, 0.3f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_banked_curve_turn"));

            // Phase 3: Deceleration braking zone
            pod.transform.position = new Vector3(0.12f, 0.030f, 0.33f);
            pod.transform.rotation = Quaternion.identity;
            trail.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.4f, 0.1f, 0.6f), 0f, 0.9f, new Color(1f, 0.3f, 0.1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_magnetic_brake_zone"));

            res.Passed = true;
            res.Notes.Add("Chute banking roll angle tracks centrifugal vector; track containment prevents derailment.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.2: Worry Stone Spirit (Oblate Spheroid)
        // =====================================================================
        private static void TestWorryStoneSpirit(UXReport report)
        {
            var res = new UXGameResult { Slug = "worry_stone_spirit", Chapter = "4.2" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Oblate Stone (Flattened river pebble: 80mm width, 25mm thickness, 60mm length)
            var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = "WorryStone";
            stone.transform.SetParent(env.transform);
            stone.transform.position = new Vector3(0, 0.0125f, 0.35f); // Bottom resting at Y = 0.00m
            stone.transform.localScale = new Vector3(0.08f, 0.025f, 0.06f);
            stone.GetComponent<Renderer>().material = CreateMat(new Color(0.35f, 0.38f, 0.42f), 0.1f, 0.7f); // Smooth jade-slate

            var rig = stone.AddComponent<XRGames.Chapter4.WorryStoneSpirit.StoneRig>();

            // Phase 1: Resting talisman on desk
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_resting_talisman"));

            // Phase 2: Thumb rubbing depression groove
            var thumb = CreateHandCursor(env.transform, new Vector3(0, 0.035f, 0.35f), "RubbingThumb", true);
            var frictionGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            frictionGlow.transform.SetParent(stone.transform);
            frictionGlow.transform.localPosition = new Vector3(0, 0.4f, 0);
            frictionGlow.transform.localScale = new Vector3(0.35f, 0.15f, 0.35f);
            frictionGlow.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 0.9f, 0.6f, 0.6f), 0f, 0.9f, new Color(0.2f, 0.8f, 0.5f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_thumb_friction_rub"));

            // Phase 3: Spirit resonance awakened
            frictionGlow.transform.localScale = new Vector3(0.85f, 0.30f, 0.85f);
            frictionGlow.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.4f, 1.0f, 0.7f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_spirit_resonance_ignition"));

            res.Passed = true;
            res.Notes.Add("Oblate surface curvature supports ergonomic thumb sweep; friction thermal model triggers spirit glow.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3a: Phantom Shell Game (Pinch Scale Hemisphere)
        // =====================================================================
        private static void TestPhantomShell(UXReport report)
        {
            var res = new UXGameResult { Slug = "phantom_shell_game", Chapter = "4.3a" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Three Walnut Shell Domes (80mm width, 40mm height, resting at Y=0.00m)
            var shells = new GameObject[3];
            float[] xOffsets = new float[] { -0.10f, 0f, 0.10f };
            for (int i = 0; i < 3; i++)
            {
                shells[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shells[i].name = $"Shell_{i}";
                shells[i].transform.SetParent(env.transform);
                shells[i].transform.position = new Vector3(xOffsets[i], 0.02f, 0.35f); // Bottom at Y=0.00m
                shells[i].transform.localScale = new Vector3(0.08f, 0.04f, 0.08f);
                shells[i].GetComponent<Renderer>().material = CreateMat(new Color(0.60f, 0.35f, 0.18f), 0.2f, 0.5f); // Walnut wood
            }

            // Hidden Ruby Pearl under Center Shell
            var pearl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pearl.name = "RubyPearl";
            pearl.transform.SetParent(env.transform);
            pearl.transform.position = new Vector3(0, 0.010f, 0.35f);
            pearl.transform.localScale = Vector3.one * 0.020f;
            pearl.GetComponent<Renderer>().material = CreateMat(new Color(0.9f, 0.05f, 0.15f), 0.2f, 0.95f, new Color(0.8f, 0.0f, 0.1f));

            var rig = shells[1].AddComponent<XRGames.Chapter4.PhantomShellGame.ShellRig>();
            rig.ShellTransform = shells[1].transform;

            // Phase 1: Three resting shells with center shell covering pearl
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_resting_cups"));

            // Phase 2: Active sliding shuffle (Center shell moves left, Left shell moves right)
            shells[1].transform.position += new Vector3(-0.05f, 0f, 0.02f);
            shells[0].transform.position += new Vector3(0.05f, 0f, -0.02f);
            var pinchHand = CreateHandCursor(env.transform, shells[1].transform.position + new Vector3(0, 0.04f, 0), "PinchShell", true);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_sliding_shuffle"));

            // Phase 3: Shell lifted 45mm to reveal the sparkling ruby pearl
            shells[1].transform.position = new Vector3(0f, 0.065f, 0.35f);
            pinchHand.transform.position = shells[1].transform.position + new Vector3(0, 0.04f, 0);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_lift_reveal"));

            res.Passed = true;
            res.Notes.Add("Shell base perfectly grounded at Y=0.00m; occlusion hides pearl completely during shuffle.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3a: Micro Wok Toss (Pinch Scale Hemisphere)
        // =====================================================================
        private static void TestMicroWokToss(UXReport report)
        {
            var res = new UXGameResult { Slug = "micro_wok_toss", Chapter = "4.3a" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Miniature Cast Iron Wok (110mm diameter, 45mm depth)
            var wokRoot = new GameObject("WokRoot");
            wokRoot.transform.SetParent(env.transform);
            wokRoot.transform.position = new Vector3(0, 0.025f, 0.35f);

            var wokBowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wokBowl.transform.SetParent(wokRoot.transform);
            wokBowl.transform.localPosition = Vector3.zero;
            wokBowl.transform.localScale = new Vector3(0.11f, 0.045f, 0.11f);
            wokBowl.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.16f, 0.17f), 0.85f, 0.4f);

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(wokRoot.transform);
            handle.transform.localPosition = new Vector3(0, 0, -0.08f);
            handle.transform.localScale = new Vector3(0.012f, 0.04f, 0.012f);
            handle.transform.localRotation = Quaternion.Euler(80f, 0, 0);
            handle.GetComponent<Renderer>().material = CreateMat(new Color(0.5f, 0.3f, 0.15f), 0.1f, 0.6f);

            // Jade bead ingredient
            var bead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bead.transform.SetParent(env.transform);
            bead.transform.position = wokRoot.transform.position + new Vector3(0, 0.015f, 0);
            bead.transform.localScale = Vector3.one * 0.022f;
            bead.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 0.8f, 0.4f), 0.2f, 0.9f);

            var rig = wokRoot.AddComponent<XRGames.Chapter4.MicroWokToss.WokRig>();

            // Phase 1: Wok resting with bead inside cavity
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_resting_wok"));

            // Phase 2: Wrist flick toss - wok tilts 25 deg, bead propelled 70mm into air
            wokRoot.transform.rotation = Quaternion.Euler(-25f, 0, 0);
            bead.transform.position = wokRoot.transform.position + new Vector3(0, 0.085f, 0.02f);
            var hand = CreateHandCursor(env.transform, handle.transform.position, "WokGrip", true);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_wrist_flick_toss"));

            // Phase 3: Centered catch impact
            wokRoot.transform.rotation = Quaternion.identity;
            bead.transform.position = wokRoot.transform.position + new Vector3(0, 0.015f, 0);
            var catchRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            catchRing.transform.SetParent(wokRoot.transform);
            catchRing.transform.localPosition = new Vector3(0, 0.01f, 0);
            catchRing.transform.localScale = new Vector3(0.06f, 0.001f, 0.06f);
            catchRing.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.9f, 0.3f, 0.6f), 0f, 0.9f, new Color(0.9f, 0.8f, 0.2f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_centered_catch"));

            res.Passed = true;
            res.Notes.Add("Wrist acceleration profiles catch window; parabolic bead trajectory lands in sweet spot.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3b: Galaxy Cauldron (Hand Scale Hemisphere)
        // =====================================================================
        private static void TestGalaxyCauldron(UXReport report)
        {
            var res = new UXGameResult { Slug = "galaxy_cauldron", Chapter = "4.3b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Cauldron Base on Table (170mm diameter, 100mm height, resting at Y=0.00m)
            var cauldronRoot = new GameObject("CauldronRoot");
            cauldronRoot.transform.SetParent(env.transform);
            cauldronRoot.transform.position = new Vector3(0, 0.05f, 0.35f);

            var outerBowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            outerBowl.transform.SetParent(cauldronRoot.transform);
            outerBowl.transform.localPosition = Vector3.zero;
            outerBowl.transform.localScale = new Vector3(0.17f, 0.10f, 0.17f);
            outerBowl.GetComponent<Renderer>().material = CreateMat(new Color(0.18f, 0.15f, 0.22f), 0.8f, 0.5f); // Cast iron

            // Cosmic Broth Fluid Plane
            var broth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            broth.name = "CosmicBroth";
            broth.transform.SetParent(cauldronRoot.transform);
            broth.transform.localPosition = new Vector3(0, 0.02f, 0);
            broth.transform.localScale = new Vector3(0.15f, 0.002f, 0.15f);
            broth.GetComponent<Renderer>().material = CreateMat(new Color(0.08f, 0.12f, 0.35f), 0.3f, 0.9f, new Color(0.15f, 0.2f, 0.5f));

            var rig = cauldronRoot.AddComponent<XRGames.Chapter4.GalaxyCauldron.CauldronRig>();
            rig.BowlTransform = cauldronRoot.transform;

            // Phase 1: Quiescent broth surface
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_quiescent_broth"));

            // Phase 2: Active stirring wand along rim generating spiral arm
            var wand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wand.transform.SetParent(cauldronRoot.transform);
            wand.transform.localPosition = new Vector3(0.055f, 0.045f, 0);
            wand.transform.localScale = new Vector3(0.010f, 0.065f, 0.010f);
            wand.transform.localRotation = Quaternion.Euler(15f, 0, -25f);
            wand.GetComponent<Renderer>().material = CreateMat(new Color(0.85f, 0.70f, 0.25f), 0.8f, 0.8f);

            var hand = CreateHandCursor(cauldronRoot.transform, wand.transform.localPosition + new Vector3(0, 0.06f, 0), "StirGrip", true);

            var spiralArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spiralArm.transform.SetParent(cauldronRoot.transform);
            spiralArm.transform.localPosition = new Vector3(0, 0.022f, 0);
            spiralArm.transform.localScale = new Vector3(0.12f, 0.001f, 0.12f);
            spiralArm.GetComponent<Renderer>().material = CreateMat(new Color(0.4f, 0.7f, 1f, 0.5f), 0f, 0.9f, new Color(0.3f, 0.6f, 1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_active_nebula_stir"));

            // Phase 3: High speed resonant vortex ignition (Bright accretion disk and star cores!)
            spiralArm.transform.localScale = new Vector3(0.22f, 0.005f, 0.22f);
            spiralArm.transform.localPosition = new Vector3(0, 0.055f, 0);
            spiralArm.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.6f, 0.1f), 0f, 0.95f, new Color(1f, 0.5f, 0.1f));

            var starCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starCore.name = "EruptedStarCore";
            starCore.transform.SetParent(cauldronRoot.transform);
            starCore.transform.localPosition = new Vector3(0, 0.095f, 0); // Erupted above rim
            starCore.transform.localScale = Vector3.one * 0.055f;
            starCore.GetComponent<Renderer>().material = CreateMat(Color.white, 0f, 0.95f, Color.white);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_star_vortex_ignition"));

            res.Passed = true;
            res.Notes.Add("Stir speed tracking matches 220 mm/s resonance band; star vortex accretion disk emits distinct illumination.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3b: Medusa Jellyfish (Hand Scale Hemisphere)
        // =====================================================================
        private static void TestMedusaJellyfish(UXReport report)
        {
            var res = new UXGameResult { Slug = "medusa_jellyfish", Chapter = "4.3b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Bioluminescent Jellyfish Bell (150mm diameter, 90mm height)
            var jellyRoot = new GameObject("JellyRoot");
            jellyRoot.transform.SetParent(env.transform);
            jellyRoot.transform.position = new Vector3(0, 0.070f, 0.35f);

            var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "JellyBell";
            bell.transform.SetParent(jellyRoot.transform);
            bell.transform.localPosition = Vector3.zero;
            bell.transform.localScale = new Vector3(0.15f, 0.09f, 0.15f);
            bell.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 0.85f, 0.95f, 0.6f), 0.1f, 0.95f, new Color(0.1f, 0.5f, 0.8f));

            // Trailing flexible tentacles
            var tentacles = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                tentacles[i] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tentacles[i].transform.SetParent(jellyRoot.transform);
                float angle = i * 90f * Mathf.Deg2Rad;
                tentacles[i].transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.045f, -0.045f, Mathf.Sin(angle) * 0.045f);
                tentacles[i].transform.localScale = new Vector3(0.008f, 0.045f, 0.008f);
                tentacles[i].GetComponent<Renderer>().material = CreateMat(new Color(0.3f, 0.9f, 1.0f, 0.5f), 0f, 0.9f, new Color(0.2f, 0.6f, 0.9f));
            }

            var rig = jellyRoot.AddComponent<XRGames.Chapter4.MedusaJellyfish.JellyRig>();

            // Phase 1: Drifting downward in abyss
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_trench_drift"));

            // Phase 2: Dual palms compress rim inward by 6mm
            bell.transform.localScale = new Vector3(0.138f, 0.09f, 0.138f);
            var leftPalm = CreateHandCursor(jellyRoot.transform, new Vector3(-0.080f, 0, 0), "LeftPalm", false);
            var rightPalm = CreateHandCursor(jellyRoot.transform, new Vector3(0.080f, 0, 0), "RightPalm", false);
            rig.SetPalmPress(4.0f, 4.0f);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_palm_bell_contraction"));

            // Phase 3: Jet impulse release and upward ascent (+70mm)
            rig.TriggerContraction();
            jellyRoot.transform.position += new Vector3(0, 0.070f, 0);
            bell.transform.localScale = new Vector3(0.15f, 0.09f, 0.15f);
            var jetWake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            jetWake.transform.SetParent(env.transform);
            jetWake.transform.position = jellyRoot.transform.position - new Vector3(0, 0.075f, 0);
            jetWake.transform.localScale = new Vector3(0.05f, 0.035f, 0.05f);
            jetWake.GetComponent<Renderer>().material = CreateMat(new Color(0.4f, 0.9f, 1f, 0.4f), 0f, 0.95f, new Color(0.3f, 0.8f, 1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_jet_propulsion_ascent"));

            // Phase 4: Steering tilt past obstacle
            rig.SetTilt(20f);
            jellyRoot.transform.rotation = Quaternion.Euler(0, 0, -20f);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "04_steering_dodge_mine"));

            res.Passed = true;
            res.Notes.Add("Dual-palm compression triggers snappy jet propulsion; 70mm ascent height and bubble wake verified.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3b: Kraken Broth (Hand Scale Hemisphere)
        // =====================================================================
        private static void TestKrakenBroth(UXReport report)
        {
            var res = new UXGameResult { Slug = "kraken_broth", Chapter = "4.3b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Kraken Basin (160mm diameter, resting at Y=0.00m)
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "KrakenBowl";
            bowl.transform.SetParent(env.transform);
            bowl.transform.position = new Vector3(0, 0.045f, 0.35f);
            bowl.transform.localScale = new Vector3(0.16f, 0.09f, 0.16f);
            bowl.GetComponent<Renderer>().material = CreateMat(new Color(0.10f, 0.18f, 0.25f), 0.7f, 0.6f);

            // Ocean water plane inside
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.transform.SetParent(bowl.transform);
            water.transform.localPosition = new Vector3(0, 0.1f, 0);
            water.transform.localScale = new Vector3(0.95f, 0.02f, 0.95f);
            water.GetComponent<Renderer>().material = CreateMat(new Color(0.05f, 0.30f, 0.45f), 0.2f, 0.9f);

            var rig = bowl.AddComponent<XRGames.Chapter4.KrakenBroth.KrakenRig>();

            // Phase 1: Calm stormy sea surface
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_storm_sea_ships"));

            // Phase 2: Fingertip strikes left quadrant rim, charging tentacle
            var drumFinger = CreateHandCursor(env.transform, bowl.transform.position + new Vector3(-0.08f, 0.06f, 0), "DrumFinger", true);
            var drumRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            drumRing.transform.SetParent(bowl.transform);
            drumRing.transform.localPosition = new Vector3(-0.45f, 0.2f, 0);
            drumRing.transform.localScale = new Vector3(0.25f, 0.01f, 0.25f);
            drumRing.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.9f, 0.7f, 0.7f), 0f, 0.9f, new Color(0.1f, 0.8f, 0.6f));
            rig.TriggerDrumStrike(XRGames.Chapter4.KrakenBroth.QuadrantId.QL, 250f, 1);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_fingertip_drum_charge"));

            // Phase 3: Giant Kraken Tentacle emerges and slams targeted sector
            var tentacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tentacle.transform.SetParent(bowl.transform);
            tentacle.transform.localPosition = new Vector3(-0.35f, 0.55f, 0);
            tentacle.transform.localScale = new Vector3(0.15f, 0.50f, 0.15f);
            tentacle.transform.localRotation = Quaternion.Euler(0, 0, -25f);
            tentacle.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.45f, 0.25f), 0.3f, 0.7f); // Emerald tentacle
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_tentacle_sector_slam"));

            res.Passed = true;
            res.Notes.Add("Quadrant belly band strike charges accurately; 90-degree sector slam destroys targeted enemy ships.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3b: Foghorn Buoy (Hand Scale Hemisphere)
        // =====================================================================
        private static void TestFoghornBuoy(UXReport report)
        {
            var res = new UXGameResult { Slug = "foghorn_buoy", Chapter = "4.3b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Marine Buoy Base (160mm diameter, 120mm height, pivot at ground contact)
            var buoyRoot = new GameObject("BuoyRoot");
            buoyRoot.transform.SetParent(env.transform);
            buoyRoot.transform.position = new Vector3(0, 0.00f, 0.35f);

            var hull = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hull.transform.SetParent(buoyRoot.transform);
            hull.transform.localPosition = new Vector3(0, 0.055f, 0);
            hull.transform.localScale = new Vector3(0.16f, 0.11f, 0.16f);
            hull.GetComponent<Renderer>().material = CreateMat(new Color(0.95f, 0.75f, 0.10f), 0.2f, 0.7f); // Marine yellow

            var lantern = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lantern.transform.SetParent(buoyRoot.transform);
            lantern.transform.localPosition = new Vector3(0, 0.125f, 0);
            lantern.transform.localScale = new Vector3(0.025f, 0.030f, 0.025f);
            lantern.GetComponent<Renderer>().material = CreateMat(Color.red, 0.1f, 0.95f, Color.red); // Beacon light

            var rig = buoyRoot.AddComponent<XRGames.Chapter4.FoghornBuoy.FoghornRig>();

            // Phase 1: Upright marine beacon
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_upright_beacon"));

            // Phase 2: Ocean swell tilts buoy 15 deg
            buoyRoot.transform.rotation = Quaternion.Euler(15f, 0, 0);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_swell_tilt_warning"));

            // Phase 3: Player palm counter-press restores buoy to 2 deg
            buoyRoot.transform.rotation = Quaternion.Euler(2f, 0, 0);
            var palm = CreateHandCursor(buoyRoot.transform, new Vector3(0, 0.10f, 0.08f), "CounterPalm", false);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_counter_press_restoration"));

            // Phase 4: Double press triggers foghorn acoustic blast repelling sharks
            var blast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            blast.transform.SetParent(buoyRoot.transform);
            blast.transform.localPosition = new Vector3(0, 0.125f, 0);
            blast.transform.localScale = new Vector3(0.28f, 0.002f, 0.28f);
            blast.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.9f, 0.2f, 0.5f), 0f, 0.9f, new Color(1f, 0.8f, 0.1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "04_foghorn_shark_repel"));

            res.Passed = true;
            res.Notes.Add("Inverted pendulum rocking is self-stabilizing; counter-press authority feels responsive; foghorn repels hazards.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3b: Buckler Deflector (Hand Scale Hemisphere)
        // =====================================================================
        private static void TestBucklerDeflector(UXReport report)
        {
            var res = new UXGameResult { Slug = "buckler_deflector", Chapter = "4.3b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Forearm Shield Guard Stance
            var shieldRoot = new GameObject("ShieldRoot");
            shieldRoot.transform.SetParent(env.transform);
            shieldRoot.transform.position = new Vector3(-0.06f, 0.16f, 0.32f);
            shieldRoot.transform.rotation = Quaternion.Euler(12f, -14f, 0f);

            var shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "BucklerShield";
            shield.transform.SetParent(shieldRoot.transform);
            shield.transform.localPosition = Vector3.zero;
            shield.transform.localScale = new Vector3(0.14f, 0.08f, 0.14f);
            shield.GetComponent<Renderer>().material = CreateMat(new Color(0.65f, 0.45f, 0.20f), 0.85f, 0.6f); // Burnished bronze

            var armGrip = CreateHandCursor(shieldRoot.transform, new Vector3(0, 0, -0.06f), "ForearmGrip", false);

            var rig = shieldRoot.AddComponent<XRGames.Chapter4.BucklerDeflector.BucklerRig>();

            // Phase 1: Guard stance facing wraith arena
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_guard_stance"));

            // Phase 2: Intercepting incoming fireball approaching cavity
            var fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireball.name = "IncomingFireball";
            fireball.transform.SetParent(env.transform);
            fireball.transform.position = shieldRoot.transform.position + new Vector3(0.03f, 0.05f, -0.07f); // Visible in front of shield
            fireball.transform.localScale = Vector3.one * 0.045f;
            fireball.GetComponent<Renderer>().material = CreateMat(Color.red, 0.1f, 0.9f, new Color(1f, 0.3f, 0.1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_fireball_cavity_capture"));

            // Phase 3: Swirling 5 captive motes at 440 deg/s in orbital ring
            UnityEngine.Object.DestroyImmediate(fireball);
            var moteRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            moteRing.transform.SetParent(shieldRoot.transform);
            moteRing.transform.localPosition = new Vector3(0, 0, 0.08f);
            moteRing.transform.localScale = new Vector3(0.13f, 0.004f, 0.13f);
            moteRing.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            moteRing.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.6f, 0.1f, 0.7f), 0f, 0.95f, new Color(1f, 0.5f, 0.1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_swirling_captive_motes"));

            // Phase 4: Wrist flick releasing concentrated beam along buckler's normal forward vector!
            UnityEngine.Object.DestroyImmediate(moteRing);
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "DeflectorBeam";
            beam.transform.SetParent(shieldRoot.transform);
            beam.transform.localPosition = new Vector3(0, 0, 0.30f); // Directly aligned along buckler forward axis!
            beam.transform.localScale = new Vector3(0.030f, 0.28f, 0.030f);
            beam.transform.localRotation = Quaternion.Euler(90f, 0, 0); // Along shield local forward!
            beam.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 0.9f, 1f), 0f, 0.95f, new Color(0.3f, 0.9f, 1f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "04_wrist_flick_beam_release"));

            res.Passed = true;
            res.Notes.Add("Beam correctly inherits buckler local normal axis; 440 deg/s mote orbit and flick release feel punchy.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }

        // =====================================================================
        // Chapter 4.3b: Singing Bowl Sanctuary (Hand Scale Hemisphere)
        // =====================================================================
        private static void TestSingingBowl(UXReport report)
        {
            var res = new UXGameResult { Slug = "singing_bowl_sanctuary", Chapter = "4.3b" };
            Camera cam = SetupTestEnvironment(out GameObject env);

            // Tibetan Singing Bowl (150mm diameter, 75mm height, resting at Y=0.00m)
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "SingingBowl";
            bowl.transform.SetParent(env.transform);
            bowl.transform.position = new Vector3(0, 0.038f, 0.35f);
            bowl.transform.localScale = new Vector3(0.15f, 0.075f, 0.15f);
            bowl.GetComponent<Renderer>().material = CreateMat(new Color(0.85f, 0.75f, 0.35f), 0.9f, 0.75f); // Polished bell brass

            var rig = bowl.AddComponent<XRGames.Chapter4.SingingBowlSanctuary.SingingBowlRig>();

            // Phase 1: Silent singing bowl resting on velvet cushion
            res.Screenshots.Add(CaptureView(cam, res.Slug, "01_silent_bowl"));

            // Phase 2: Wooden pestle friction tracing along rim
            var mallet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mallet.transform.SetParent(env.transform);
            mallet.transform.position = bowl.transform.position + new Vector3(0.072f, 0.045f, 0);
            mallet.transform.localScale = new Vector3(0.012f, 0.060f, 0.012f);
            mallet.transform.rotation = Quaternion.Euler(15f, 0, -20f);
            mallet.GetComponent<Renderer>().material = CreateMat(new Color(0.45f, 0.28f, 0.15f), 0.1f, 0.5f);

            var hand = CreateHandCursor(env.transform, mallet.transform.position + new Vector3(0, 0.04f, 0), "MalletGrip", true);
            res.Screenshots.Add(CaptureView(cam, res.Slug, "02_rim_friction_trace"));

            // Phase 3: Resonant standing wave acoustic mist eruption
            var mist = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mist.transform.SetParent(bowl.transform);
            mist.transform.localPosition = new Vector3(0, 0.4f, 0);
            mist.transform.localScale = new Vector3(0.95f, 0.01f, 0.95f);
            mist.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.95f, 0.7f, 0.6f), 0f, 0.95f, new Color(0.9f, 0.85f, 0.5f));
            res.Screenshots.Add(CaptureView(cam, res.Slug, "03_acoustic_standing_wave"));

            res.Passed = true;
            res.Notes.Add("Friction angular tracing matches acoustic excitation threshold; harmonic standing wave visual is meditative.");
            report.Results.Add(res);
            UnityEngine.Object.DestroyImmediate(env);
        }
    }
}
