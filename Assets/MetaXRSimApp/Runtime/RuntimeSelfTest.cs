using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MetaXRSim.App
{
    [Serializable]
    public sealed class SelfTestEntry
    {
        public string slug;
        public string rigType;
        public bool spawned;
        public bool rigBound;
        public int boundTransforms;
        public string error;
        public float sizeMeters;
        public float scoreAfterTick;
        public float timeLeftAfterTick;
        public float propAngularSpeedDeg;
        public bool sessionPlaying;
        public bool grabbed;
        public string objective;
    }

    [Serializable]
    public sealed class SelfTestReport
    {
        public string timestamp;
        public int total;
        public int spawned;
        public int rigBound;
        public int failed;
        public string[] gesturePresets;
        public string[] gestureMatches;
        public List<SelfTestEntry> entries = new List<SelfTestEntry>();
    }

    /// <summary>
    /// Headless "real player" pass used by CI and by the final APK smoke test.
    /// It launches every one of the 50 games through the same hub path a player
    /// uses, waits for the board and rig to spawn, ticks physics/scoring, exits,
    /// and writes a machine-readable report.
    /// </summary>
    public sealed class RuntimeSelfTest : MonoBehaviour
    {
        public bool AutoQuit = true;
        public bool CaptureScreenshots = false;
        public float SpawnTimeoutSeconds = 5f;
        public float TickSeconds = 0.35f;

        private ShapeForgeHub _hub;
        private MiniGameHost _host;
        private SelfTestReport _report;

        public void Begin(ShapeForgeHub hub, MiniGameHost host)
        {
            _hub = hub;
            _host = host;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return new WaitForSeconds(1.0f);
            if (_hub == null || _host == null)
            {
                Debug.LogError("[SELFTEST] Missing hub/host");
                Finish(false);
                yield break;
            }

            if (CaptureScreenshots)
            {
                Screenshot.Capture("hub");
                yield return new WaitForSeconds(0.35f);
            }

            _report = new SelfTestReport
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                total = GameCatalog.All.Count
            };
            Debug.Log("[SELFTEST] Starting 50-game unified runtime test");

            // Gesture recogniser sanity.
            string[] expected = { "TrueSphere", "ProlateSpheroid", "OblateSpheroid", "Hemisphere", "DomedBowl" };
            var presets = new List<string>();
            var matches = new List<string>();
            for (int p = 1; p <= 5; p++)
            {
                _hub.SimulatePreset(p);
                yield return null;
                yield return null;
                string kind = _hub.PreviewShape.Kind.ToString();
                presets.Add(kind);
                matches.Add(_hub.PreviewMatch.HasGame ? _hub.PreviewMatch.Game.Slug : "none");
                Debug.Log("[SELFTEST] preset " + p + " -> " + kind + " (expected " + expected[p - 1] +
                          ") -> " + matches[p - 1]);
                if (CaptureScreenshots)
                {
                    Screenshot.Capture("preview_" + p + "_" + kind);
                    yield return new WaitForSeconds(0.25f);
                }
                _hub.CancelPreview();
                yield return null;
            }
            _report.gesturePresets = presets.ToArray();
            _report.gestureMatches = matches.ToArray();

            foreach (var game in GameCatalog.All)
            {
                var entry = new SelfTestEntry
                {
                    slug = game.Slug,
                    rigType = game.RigTypeName,
                    objective = game.Objective.ToString()
                };

                _hub.LaunchGame(game);
                float t = 0f;
                while (_host.Session == null && t < SpawnTimeoutSeconds)
                {
                    t += Time.deltaTime;
                    yield return null;
                }

                if (_host.Session == null)
                {
                    entry.spawned = false;
                    entry.error = "timeout waiting for session";
                    Debug.LogError("[SELFTEST] FAIL spawn " + game.Slug);
                }
                else
                {
                    entry.spawned = true;
                    entry.sizeMeters = _host.Session.Built != null ? _host.Session.Built.SizeMeters : 0f;
                    var report = _host.LastReport;
                    entry.rigBound = report != null && report.Success;
                    entry.boundTransforms = report != null ? report.BoundTransforms : 0;
                    entry.error = report != null ? report.Error : "";

                    if (CaptureScreenshots)
                    {
                        Screenshot.Capture("game_" + game.Slug);
                        yield return new WaitForSeconds(0.20f);
                    }

                    // Nudge the physical prop and let the objective loop consume it.
                    var prop = _host.Session.Built.Prop;
                    var rb = prop != null ? prop.GetComponent<Rigidbody>() : null;
                    var grab = prop != null ? prop.GetComponent<GrabInteractable>() : null;
                    var input = _hub != null ? _hub.Input : null;

                    // First drive the real pinch path through the unified input layer.
                    if (input != null && prop != null)
                    {
                        input.UseSyntheticHands = true;
                        Quaternion rot = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        Vector3 start = prop.position + Vector3.up * 0.02f;
                        input.SetSyntheticHand(HandSide.Right, start, rot, true);
                        for (int f = 0; f < 8; f++) yield return null;

                        Vector3 end = start + Vector3.up * 0.10f + Vector3.right * 0.05f;
                        input.SetSyntheticHand(HandSide.Right, end, rot, true);
                        for (int f = 0; f < 8; f++) yield return null;

                        input.SetSyntheticHand(HandSide.Right, end, rot, false);
                        for (int f = 0; f < 5; f++) yield return null;
                        input.UseSyntheticHands = false;
                        entry.grabbed = grab != null && grab.HasEverGrabbed;
                    }

                    try
                    {
                        if (rb != null)
                        {
                            rb.WakeUp();
                            rb.angularVelocity = Vector3.up * 30f;
                            rb.AddTorque(Vector3.up * 3f, ForceMode.Impulse);
                            rb.AddForce(Vector3.up * 0.8f, ForceMode.Impulse);
                            entry.propAngularSpeedDeg = rb.angularVelocity.magnitude * Mathf.Rad2Deg;
                        }
                    }
                    catch (Exception e)
                    {
                        entry.error = (entry.error ?? "") + " nudge:" + e.Message;
                    }

                    float tick = 0f;
                    while (tick < TickSeconds)
                    {
                        tick += Time.deltaTime;
                        yield return null;
                    }
                    entry.scoreAfterTick = _host.Session != null ? _host.Session.Score : -1f;
                    entry.timeLeftAfterTick = _host.Session != null ? _host.Session.TimeLeft : -1f;
                    entry.sessionPlaying = _host.Session != null && _host.Session.IsPlaying;

                    if (!entry.rigBound)
                        Debug.LogError("[SELFTEST] FAIL rig " + game.Slug + ": " + entry.error);
                    else
                        Debug.Log("[SELFTEST] OK " + game.Slug + " bindings=" + entry.boundTransforms +
                                  " grabbed=" + entry.grabbed +
                                  " score=" + entry.scoreAfterTick.ToString("F1"));
                }

                _report.entries.Add(entry);
                if (entry.spawned) _report.spawned++;
                if (entry.rigBound) _report.rigBound++;
                if (!entry.spawned || !entry.rigBound) _report.failed++;

                if (_host.InGame) _host.Exit();
                yield return new WaitForSeconds(0.15f);
            }

            Finish(_report.failed == 0);
        }

        private void Finish(bool success)
        {
            if (_report != null)
            {
                try
                {
                    string path = Path.Combine(Application.persistentDataPath, "shapeforge_selftest.json");
                    File.WriteAllText(path, JsonUtility.ToJson(_report, true));
                    Debug.Log("[SELFTEST] report: " + path);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[SELFTEST] could not write report: " + e.Message);
                }
                Debug.Log(string.Format("[SELFTEST] RESULT success={0} spawned={1}/{2} rigBound={3} failed={4}",
                    success, _report.spawned, _report.total, _report.rigBound, _report.failed));
            }

            if (!AutoQuit) return;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(success ? 0 : 1);
#endif
        }
    }
}
