using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace MetaXRSim.App
{
    [Serializable]
    public sealed class StressGameEntry
    {
        public string slug;
        public string objective;
        public float sizeMeters;
        public bool spawned;
        public bool grabbedByAutoplay;
        public float firstRewardSeconds = -1f;
        public float scoreAt30s;
        public float target;
        public bool won;
        public bool lost;
        public string loseReason;
        public string lastPenalty;
        public int penalties;
        public float integrityAt30s;
        public float maxPropDistance;
        public bool propNaN;
        public float frameAvgMs;
        public float frameP99Ms;
        public float frameMaxMs;
        public float gcAllocKbPerFrame;
        // Design criteria measurements (bus stop / airplane seat / enter-exit)
        public float playRadiusMeters;
        public string farthestRenderer;
        public float headDistanceMeters;
        public float angularSpanDeg;
        public float maxHandFromHeadMeters;
        public float exitClearanceMeters;
        public bool exitButtonWorked;
        public bool seatedOk;
    }

    [Serializable]
    public sealed class StressReport
    {
        public string timestamp;
        public float playSecondsPerGame;
        public float timeScale;
        public int games;
        public int spawned;
        public int rewardedWithin30s;
        public int grabbedByAutoplay;
        public int wins;
        public int propNaN;
        public int churnLaunches;
        public int churnPasses;
        public int abuseCases;
        public int abuseFailures;
        public int errorLogs;
        public string[] errorSamples;
        public int hangs;
        public float frameAvgMs;
        public float frameP95Ms;
        public float frameP99Ms;
        public float frameMaxMs;
        public int framesOver16_6ms;
        public int gcCollectionsPhase1;
        public float gcPerGameSecond;
        public int busStopPass;
        public int airplaneSeatPass;
        public int exitButtonPass;
        public float worstPlayRadiusMeters;
        public float worstAngularSpanDeg;
        public int enterPresetsOk;
        public int enterPresetsTotal;
        public float enterToPlaySeconds;
        public int totalFrames;
        public int transformsBaseline;
        public int transformsAfter;
        public int materialsBaseline;
        public int materialsAfter;
        public int meshesBaseline;
        public int meshesAfter;
        public float managedMemMbBaseline;
        public float managedMemMbAfter;
        public float nativeMemMbBaseline;
        public float nativeMemMbAfter;
        public bool passed;
        public string[] failures;
        public List<StressGameEntry> entries = new List<StressGameEntry>();
    }

    /// <summary>
    /// End-to-end soak + churn test. For every one of the 50 games a scripted
    /// "player" drives the unified input layer (synthetic hands) for 30 seconds,
    /// measuring time to first reward, then the launcher is hammered with rapid
    /// enter/exit cycles and misuse (double launch, exit mid-conversion...) while
    /// frame cost, errors and leaked objects are recorded.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class RuntimeStressTest : MonoBehaviour
    {
        public float PlaySeconds = 30f;
        public float TimeScale = 3f;
        public int ChurnPasses = 3;
        public bool AutoQuit = true;

        private ShapeForgeHub _hub;
        private MiniGameHost _host;
        private HandInputService _input;
        private StressReport _report;

        private readonly List<float> _allFrames = new List<float>(60000);
        private readonly List<float> _gameFrames = new List<float>(4000);
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _failures = new List<string>();
        private int _errorCount;
        private int _lastFrameStamp;
        private bool _recording;
        private string[] _only;
        private bool _verbose;

        public void Begin(ShapeForgeHub hub, MiniGameHost host)
        {
            _hub = hub;
            _host = host;
            _input = hub != null ? hub.Input : null;
            Application.logMessageReceived += OnLog;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "-shapeforge-stress-games", StringComparison.OrdinalIgnoreCase))
                {
                    _only = args[i + 1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    _verbose = true;
                }
            }
            StartCoroutine(Run());
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
        }

        private void OnLog(string condition, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (condition != null && condition.StartsWith("[STRESS]")) return;
            _errorCount++;
            if (_errors.Count < 40)
            {
                string m = condition ?? "";
                if (m.Length > 220) m = m.Substring(0, 220);
                if (!_errors.Contains(m)) _errors.Add(m);
            }
        }

        private long _lastMem;
        private double _gcAllocBytes;
        private StressGameEntry _curEntry;
        private Transform _curRoot;
        private Vector3 _curHead;
        private float _sampleClock;
        private float _sampleNext;

        // Measured after every Update/LateUpdate (rigs, guards, clamps) so the numbers
        // describe what the player actually sees this frame.
        private void LateUpdate()
        {
            if (_curEntry == null || _curRoot == null) return;
            _sampleClock += Time.deltaTime;
            if (_sampleClock < _sampleNext) return;
            _sampleNext += 0.5f;
            float radius = MeasurePlayRadius(_curRoot, out string who);
            if (radius > _curEntry.playRadiusMeters) { _curEntry.playRadiusMeters = radius; _curEntry.farthestRenderer = who; }
            float hd = Vector3.Distance(_input.Right.IndexTip, _curHead);
            if (hd > _curEntry.maxHandFromHeadMeters) _curEntry.maxHandFromHeadMeters = hd;
        }

        private void Update()
        {
            if (!_recording) return;
            if (_curEntry != null)
            {
                long mem = GC.GetTotalMemory(false);
                if (_lastMem > 0 && mem > _lastMem) _gcAllocBytes += mem - _lastMem;
                _lastMem = mem;
            }
            float ms = Time.unscaledDeltaTime * 1000f;
            _allFrames.Add(ms);
            _gameFrames.Add(ms);
            _lastFrameStamp = Time.frameCount;
        }

        // ------------------------------------------------------------------
        private IEnumerator Run()
        {
            yield return new WaitForSeconds(1.0f);
            if (_hub == null || _host == null || _input == null)
            {
                Debug.LogWarning("[STRESS] missing hub/host/input");
                Finish();
                yield break;
            }

            Application.targetFrameRate = -1;   // measure real per-frame cost
            Time.timeScale = TimeScale;
            Time.maximumDeltaTime = 0.1f;
            _recording = true;

            _report = new StressReport
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                playSecondsPerGame = PlaySeconds,
                timeScale = TimeScale,
                games = GameCatalog.All.Count
            };

            Debug.Log("[STRESS] Phase 1: 30 s scripted play on all " + _report.games + " games");
            int gcStart = GC.CollectionCount(0);
            float phase1Start = Time.time;
            foreach (var game in GameCatalog.All)
            {
                if (_only != null && Array.IndexOf(_only, game.Slug) < 0) continue;
                yield return PlayGame(game);
                yield return ExitAndSettle();
            }

            if (_only != null)
            {
                Time.timeScale = 1f;
                _report.games = _report.entries.Count;
                Finish();
                yield break;
            }

            _report.gcCollectionsPhase1 = GC.CollectionCount(0) - gcStart;
            _report.gcPerGameSecond = _report.gcCollectionsPhase1 / Mathf.Max(1f, Time.time - phase1Start);
            Debug.Log("[STRESS] Phase 1 gen0 GCs=" + _report.gcCollectionsPhase1 + " per game-second=" + _report.gcPerGameSecond.ToString("F3"));

            Debug.Log("[STRESS] Phase 1b: gesture -> preview -> confirm entry flow");
            yield return EnterFlow();

            Debug.Log("[STRESS] Phase 2: enter/exit churn x" + ChurnPasses);
            // Warm-up pass so one-time caches do not count as leaks.
            yield return ChurnPass(false);
            yield return Settle();
            CaptureMemory(true);

            for (int pass = 0; pass < ChurnPasses; pass++)
            {
                yield return ChurnPass(true);
                _report.churnPasses++;
            }

            Debug.Log("[STRESS] Phase 3: misuse / abuse cases");
            yield return AbuseCases();

            yield return Settle();
            CaptureMemory(false);

            Time.timeScale = 1f;
            Finish();
        }

        // ------------------------------------------------------------------
        private IEnumerator PlayGame(GameDefinition game)
        {
            var entry = new StressGameEntry { slug = game.Slug, objective = game.Objective.ToString() };
            _gameFrames.Clear();

            _input.UseSyntheticHands = false;
            _hub.LaunchGame(game);
            float wait = 0f;
            while (_host.Session == null && wait < 6f)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }

            var session = _host.Session;
            if (session == null || session.Built == null || session.Built.Prop == null)
            {
                entry.spawned = false;
                _report.hangs++;
                _failures.Add("spawn timeout: " + game.Slug);
                _report.entries.Add(entry);
                yield break;
            }

            entry.spawned = true;
            entry.sizeMeters = session.Built.SizeMeters;
            entry.target = session.Target;
            var prop = session.Built.Prop;
            var grab = prop.GetComponent<GrabInteractable>();
            _input.UseSyntheticHands = true;

            float t = 0f;
            float lastScore = 0f;
            float nextDiag = 0f;
            float nextSample = 1.5f;   // let spawn sparkles expire before sampling the play volume
            Vector3 headPos = _input.HeadCamera != null ? _input.HeadCamera.transform.position : new Vector3(0f, 1.45f, -0.55f);
            entry.headDistanceMeters = Vector3.Distance(headPos, session.Built.Root.transform.position);
            _gcAllocBytes = 0;
            _curEntry = entry;
            _curRoot = session.Built.Root.transform;
            _curHead = headPos;
            _sampleClock = 0f;
            _sampleNext = nextSample;
            var auto = new AutoPlayer(_input, session, game.Objective);
            while (t < PlaySeconds && session != null && !session.IsWon && !session.IsLost)
            {
                float dt = Time.deltaTime;
                t += dt;
                if (prop == null) break;
                auto.Step(t, dt);

                if (entry.firstRewardSeconds < 0f && (session.Score > 0.05f)) entry.firstRewardSeconds = t;
                if (_verbose && t >= nextDiag)
                {
                    nextDiag += 3f;
                    var rbd = prop.GetComponent<Rigidbody>();
                    Debug.Log("[STRESS] diag " + game.Slug + " t=" + t.ToString("F1") + " local=" + session.Built.Root.transform.InverseTransformPoint(prop.position).ToString("F3") +
                              " held=" + (grab != null && grab.IsHeld) + " kin=" + (rbd != null && rbd.isKinematic) + " vel=" + (rbd != null ? rbd.linearVelocity.magnitude.ToString("F2") : "-") +
                              " score=" + session.Score.ToString("F1") + " integ=" + session.Integrity.ToString("F0") + " pen=" + session.PenaltyCount + ":" + session.LastPenalty +
                              " size=" + session.Built.SizeMeters.ToString("F3") + " auto=" + auto.Describe());
                }
                lastScore = session.Score;

                Vector3 p = prop.position;
                if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) || float.IsInfinity(p.x + p.y + p.z))
                {
                    entry.propNaN = true;
                    break;
                }
                float dist = Vector3.Distance(p, session.Built.Root.transform.position);
                if (dist > entry.maxPropDistance) entry.maxPropDistance = dist;
                yield return null;
            }

            if (session != null)
            {
                entry.scoreAt30s = session.Score;
                entry.won = session.IsWon;
                entry.lost = session.IsLost;
                entry.loseReason = session.LoseReason;
                entry.lastPenalty = session.LastPenalty;
                entry.penalties = session.PenaltyCount;
                entry.integrityAt30s = session.Integrity;
                if (entry.won && entry.firstRewardSeconds < 0f) entry.firstRewardSeconds = t;
            }
            entry.grabbedByAutoplay = grab != null && grab.HasEverGrabbed;

            entry.gcAllocKbPerFrame = (float)(_gcAllocBytes / 1024.0 / Mathf.Max(1, _gameFrames.Count));
            _gcAllocBytes = 0;
            _curEntry = null;
            FillFrameStats(entry);
            _input.UseSyntheticHands = false;
            EvaluateDesignCriteria(entry, session, grab);
            _report.entries.Add(entry);
            if (entry.firstRewardSeconds >= 0f && entry.firstRewardSeconds <= 30f) _report.rewardedWithin30s++;
            if (entry.grabbedByAutoplay) _report.grabbedByAutoplay++;
            if (entry.won) _report.wins++;
            if (entry.propNaN) _report.propNaN++;
            _report.spawned++;

            Debug.Log("[STRESS] game " + game.Slug + " (" + game.Objective + ") reward@" +
                      entry.firstRewardSeconds.ToString("F1") + "s score=" + entry.scoreAt30s.ToString("F1") +
                      "/" + entry.target.ToString("F0") + " grabbed=" + entry.grabbedByAutoplay + (entry.lost ? " LOST:" + entry.loseReason : "") +
                      " p99=" + entry.frameP99Ms.ToString("F1") + "ms radius=" + entry.playRadiusMeters.ToString("F2") + " far=" + entry.farthestRenderer);
        }

        private static float MeasurePlayRadius(Transform root, out string who)
        {
            float best = 0f;
            who = "";
            Vector3 c = root.position;
            var rends = root.GetComponentsInChildren<Renderer>(false);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (r == null || !r.enabled || !(r is MeshRenderer || r is SkinnedMeshRenderer)) continue;
                Bounds b = r.bounds;
                Vector3 d = b.center - c;
                d.y = 0f;
                float reach = d.magnitude + Mathf.Max(b.extents.x, b.extents.z);
                if (reach > best) { best = reach; who = r.transform.parent != null ? r.transform.parent.name + "/" + r.name : r.name; }
            }
            return best;
        }

        // Bus-stop: small, discreet footprint, one hand, content close together.
        // Airplane seat: everything within arm reach of a seated player, narrow
        // angular span, no room-scale movement. Enter/exit: the on-board EXIT
        // button is reachable without touching the prop and really leaves.
        private void EvaluateDesignCriteria(StressGameEntry entry, MiniGameSession session, GrabInteractable grab)
        {
            float dist = Mathf.Max(0.05f, entry.headDistanceMeters);
            entry.angularSpanDeg = 2f * Mathf.Atan(entry.playRadiusMeters / dist) * Mathf.Rad2Deg;

            // Play volume may extend at most 18 cm beyond the real object (min 32 cm radius).
            float allowed = Mathf.Max(0.32f, entry.sizeMeters * 0.5f + 0.18f);
            bool compact = entry.playRadiusMeters <= allowed;
            bool oneHandCloseBy = entry.maxHandFromHeadMeters <= 0.85f;     // seated arm reach
            bool narrow = entry.angularSpanDeg <= 60f;                      // no head turning
            bool inReach = entry.headDistanceMeters <= 0.85f;
            entry.seatedOk = compact && oneHandCloseBy && narrow && inReach;

            if (session != null && session.Built != null)
            {
                var root = session.Built.Root.transform;
                var buttons = root.GetComponentsInChildren<WorldButton>(true);
                WorldButton exit = null;
                for (int i = 0; i < buttons.Length; i++)
                    if (buttons[i].gameObject.name == "GameExit" || buttons[i].Id == "GameExit") exit = buttons[i];

                if (exit != null && session.Built.Prop != null)
                {
                    float gr = grab != null ? grab.GrabRadiusMeters : 0.1f;
                    float half = exit.Size.x * 0.5f;
                    entry.exitClearanceMeters = Vector3.Distance(exit.transform.position, session.Built.Prop.position) - half - gr;
                    exit.Press();
                    entry.exitButtonWorked = !_host.InGame;
                }
            }

            if (entry.seatedOk) _report.airplaneSeatPass++;
            if (compact && entry.maxHandFromHeadMeters <= 0.85f) _report.busStopPass++;
            if (entry.exitButtonWorked) _report.exitButtonPass++;
            if (entry.playRadiusMeters > _report.worstPlayRadiusMeters) _report.worstPlayRadiusMeters = entry.playRadiusMeters;
            if (entry.angularSpanDeg > _report.worstAngularSpanDeg) _report.worstAngularSpanDeg = entry.angularSpanDeg;
        }

        private void FillFrameStats(StressGameEntry entry)
        {
            if (_gameFrames.Count == 0) return;
            var copy = new List<float>(_gameFrames);
            copy.Sort();
            float sum = 0f;
            for (int i = 0; i < copy.Count; i++) sum += copy[i];
            entry.frameAvgMs = sum / copy.Count;
            entry.frameP99Ms = copy[Mathf.Clamp((int)(copy.Count * 0.99f), 0, copy.Count - 1)];
            entry.frameMaxMs = copy[copy.Count - 1];
        }

        // Easy enter: draw a shape (preset), see the matched game, confirm once.
        private IEnumerator EnterFlow()
        {
            float totalPlay = 0f;
            for (int preset = 1; preset <= 5; preset++)
            {
                _report.enterPresetsTotal++;
                _input.UseSyntheticHands = false;
                if (_host.InGame) _host.Exit();
                yield return null;
                _hub.SimulatePreset(preset);
                yield return null;
                yield return null;
                if (!_hub.HasPreview || !_hub.PreviewMatch.HasGame)
                {
                    _failures.Add("preset " + preset + " produced no matched game");
                    _hub.CancelPreview();
                    continue;
                }

                float t = 0f;
                _hub.ConfirmConversion();
                while (_host.Session == null && t < 4f)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
                if (_host.Session == null)
                {
                    _failures.Add("preset " + preset + " confirm did not start a game");
                    continue;
                }
                totalPlay += t;
                _report.enterPresetsOk++;
                yield return null;
                _host.Exit();
                yield return null;
            }
            _report.enterToPlaySeconds = _report.enterPresetsOk > 0 ? totalPlay / _report.enterPresetsOk : -1f;
        }

        private IEnumerator ExitAndSettle()
        {
            if (_host.InGame) _host.Exit();
            _input.UseSyntheticHands = false;
            float t = 0f;
            while (_host.InGame && t < 3f) { t += Time.unscaledDeltaTime; yield return null; }
            yield return new WaitForSecondsRealtime(0.05f);
        }

        private IEnumerator Settle()
        {
            if (_host.InGame) _host.Exit();
            yield return new WaitForSecondsRealtime(0.5f);
            Resources.UnloadUnusedAssets();
            GC.Collect();
            yield return new WaitForSecondsRealtime(0.3f);
        }

        // ------------------------------------------------------------------
        private IEnumerator ChurnPass(bool counted)
        {
            foreach (var game in GameCatalog.All)
            {
                _hub.LaunchGame(game);
                float t = 0f;
                while (_host.Session == null && t < 6f)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
                if (_host.Session == null)
                {
                    _report.hangs++;
                    _failures.Add("churn spawn timeout: " + game.Slug);
                }
                else
                {
                    for (int f = 0; f < 6; f++) yield return null;
                }

                if (_host.InGame) _host.Exit();
                if (counted) _report.churnLaunches++;
                yield return null;
            }
        }

        private IEnumerator AbuseCases()
        {
            var games = GameCatalog.All;
            int before = _errorCount;

            // 1. Exit while not in a game.
            Abuse("exit-when-idle", () => { _host.Exit(); });
            yield return null;

            // 2. Launch the same game twice in the same frame.
            Abuse("double-launch", () => { _hub.LaunchGame(games[0]); _hub.LaunchGame(games[0]); });
            yield return WaitSession(5f);
            yield return Settle();

            // 3. Launch a different game while another is running.
            Abuse("launch-over-running", () => { _hub.LaunchGame(games[1]); });
            yield return WaitSession(5f);
            Abuse("launch-over-running-2", () => { _hub.LaunchGame(games[2]); });
            yield return WaitSession(5f);
            yield return Settle();

            // 4. Exit during conversion (immediately after launch).
            Abuse("exit-during-conversion", () => { _hub.LaunchGame(games[3]); _host.Exit(); });
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Settle();

            // 5. Exit then relaunch the same frame.
            Abuse("exit-relaunch", () => { _hub.LaunchGame(games[4]); });
            yield return WaitSession(5f);
            Abuse("exit-relaunch-2", () => { _host.Exit(); _hub.LaunchGame(games[5]); });
            yield return WaitSession(5f);
            yield return Settle();

            // 6. Emergency cleanup while running.
            Abuse("emergency-cleanup", () => { _hub.LaunchGame(games[6]); });
            yield return WaitSession(5f);
            Abuse("emergency-cleanup-2", () => { _host.EmergencyCleanup(); });
            yield return new WaitForSecondsRealtime(0.3f);

            // 7. Tracking loss mid-game: hands vanish and return.
            Abuse("hand-tracking-loss", () => { _hub.LaunchGame(games[7]); });
            yield return WaitSession(5f);
            _input.UseSyntheticHands = false;
            yield return new WaitForSecondsRealtime(0.5f);

            // 8. Pause toggles and win/lose restart spam.
            if (_host.Session != null)
            {
                Abuse("pause-spam", () => { for (int i = 0; i < 9; i++) _host.Session.TogglePause(); });
                Abuse("restart-spam", () => { for (int i = 0; i < 5; i++) _host.Session.Restart(); });
            }
            yield return Settle();

            // 9. Preview gestures spammed (all five presets, no launch).
            Abuse("preset-spam", () => { for (int p = 1; p <= 5; p++) { _hub.SimulatePreset(p); _hub.CancelPreview(); } });
            yield return null;

            if (_errorCount > before)
                _failures.Add("abuse produced " + (_errorCount - before) + " error log(s)");
        }

        private IEnumerator WaitSession(float timeout)
        {
            float t = 0f;
            while (_host.Session == null && t < timeout)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            for (int i = 0; i < 4; i++) yield return null;
        }

        private void Abuse(string name, Action action)
        {
            _report.abuseCases++;
            try
            {
                action();
            }
            catch (Exception e)
            {
                _report.abuseFailures++;
                _failures.Add("abuse '" + name + "' threw " + e.GetType().Name + ": " + e.Message);
            }
        }

        // ------------------------------------------------------------------
        private void CaptureMemory(bool baseline)
        {
            GC.Collect();
            int tf = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            int mats = Resources.FindObjectsOfTypeAll<Material>().Length;
            int meshes = Resources.FindObjectsOfTypeAll<Mesh>().Length;
            float managed = GC.GetTotalMemory(true) / (1024f * 1024f);
            float native = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            if (baseline)
            {
                _report.transformsBaseline = tf;
                _report.materialsBaseline = mats;
                _report.meshesBaseline = meshes;
                _report.managedMemMbBaseline = managed;
                _report.nativeMemMbBaseline = native;
            }
            else
            {
                _report.transformsAfter = tf;
                _report.materialsAfter = mats;
                _report.meshesAfter = meshes;
                _report.managedMemMbAfter = managed;
                _report.nativeMemMbAfter = native;
            }
        }

        private void Finish()
        {
            _recording = false;
            Application.logMessageReceived -= OnLog;
            Time.timeScale = 1f;

            if (_report != null)
            {
                _report.errorLogs = _errorCount;
                _report.errorSamples = _errors.ToArray();
                _report.totalFrames = _allFrames.Count;

                if (_allFrames.Count > 0)
                {
                    var sorted = new List<float>(_allFrames);
                    sorted.Sort();
                    float sum = 0f;
                    int over = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        sum += sorted[i];
                        if (sorted[i] > 16.6f) over++;
                    }
                    _report.frameAvgMs = sum / sorted.Count;
                    _report.frameP95Ms = sorted[Mathf.Clamp((int)(sorted.Count * 0.95f), 0, sorted.Count - 1)];
                    _report.frameP99Ms = sorted[Mathf.Clamp((int)(sorted.Count * 0.99f), 0, sorted.Count - 1)];
                    _report.frameMaxMs = sorted[sorted.Count - 1];
                    _report.framesOver16_6ms = over;
                }

                if (_only == null)
                {
                    if (_report.exitButtonPass != _report.spawned) _failures.Add("EXIT button failed in " + (_report.spawned - _report.exitButtonPass) + " games");
                    if (_report.enterPresetsOk != _report.enterPresetsTotal) _failures.Add("gesture entry flow failed: " + _report.enterPresetsOk + "/" + _report.enterPresetsTotal);
                    if (_report.rewardedWithin30s != _report.spawned) _failures.Add("no reward within 30 s in " + (_report.spawned - _report.rewardedWithin30s) + " games");
                    if (_report.busStopPass != _report.spawned) _failures.Add("bus-stop footprint/reach exceeded in " + (_report.spawned - _report.busStopPass) + " games");
                    if (_report.airplaneSeatPass != _report.spawned) _failures.Add("airplane-seat criteria failed in " + (_report.spawned - _report.airplaneSeatPass) + " games");
                    foreach (var en in _report.entries)
                        if (en.gcAllocKbPerFrame > 2f) _failures.Add("GC allocation " + en.gcAllocKbPerFrame.ToString("F2") + " KB/frame in " + en.slug);
                    if (_report.frameP99Ms > 3f) _failures.Add("p99 CPU frame cost " + _report.frameP99Ms.ToString("F2") + " ms is too high for a ~4x slower Quest CPU at 72 Hz");
                }
                if (_report.spawned != _report.games) _failures.Add("spawned " + _report.spawned + "/" + _report.games);
                if (_report.propNaN > 0) _failures.Add("NaN prop transform in " + _report.propNaN + " games");
                if (_report.errorLogs > 0) _failures.Add(_report.errorLogs + " error/exception log(s) during run");
                if (_report.abuseFailures > 0) _failures.Add(_report.abuseFailures + " abuse case exception(s)");
                if (_report.transformsAfter - _report.transformsBaseline > 3)
                    _failures.Add("transform leak: " + _report.transformsBaseline + " -> " + _report.transformsAfter);
                if (_report.materialsAfter - _report.materialsBaseline > 30)
                    _failures.Add("material leak: " + _report.materialsBaseline + " -> " + _report.materialsAfter);
                if (_report.meshesAfter - _report.meshesBaseline > 30)
                    _failures.Add("mesh leak: " + _report.meshesBaseline + " -> " + _report.meshesAfter);

                _report.failures = _failures.ToArray();
                _report.passed = _failures.Count == 0;

                try
                {
                    string path = Path.Combine(Application.persistentDataPath, "shapeforge_stress.json");
                    File.WriteAllText(path, JsonUtility.ToJson(_report, true));
                    Debug.Log("[STRESS] report: " + path);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[STRESS] could not write report: " + e.Message);
                }

                Debug.Log(string.Format(
                    "[STRESS] RESULT passed={0} spawned={1}/{2} rewarded30s={3} grabbed={4} wins={5} errors={6} hangs={7} " +
                    "frame avg={8:F2}ms p99={9:F2}ms max={10:F1}ms over16.6={11}/{12} transforms={13}->{14} materials={15}->{16}",
                    _report.passed, _report.spawned, _report.games, _report.rewardedWithin30s,
                    _report.grabbedByAutoplay, _report.wins, _report.errorLogs, _report.hangs,
                    _report.frameAvgMs, _report.frameP99Ms, _report.frameMaxMs, _report.framesOver16_6ms,
                    _report.totalFrames, _report.transformsBaseline, _report.transformsAfter,
                    _report.materialsBaseline, _report.materialsAfter));
                Debug.Log(string.Format("[STRESS] DESIGN busStop={0}/{1} airplaneSeat={2}/{1} exitButton={3}/{1} worstRadius={4:F2}m worstSpan={5:F0}deg enter={6}/{7} enterToPlay={8:F2}s",
                    _report.busStopPass, _report.spawned, _report.airplaneSeatPass, _report.exitButtonPass,
                    _report.worstPlayRadiusMeters, _report.worstAngularSpanDeg, _report.enterPresetsOk, _report.enterPresetsTotal, _report.enterToPlaySeconds));
                foreach (var f in _failures) Debug.Log("[STRESS] FAILURE: " + f);
            }

            if (!AutoQuit) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(_report != null && _report.passed ? 0 : 1);
#endif
        }

        // ------------------------------------------------------------------
        /// <summary>Scripted player. Drives synthetic hands exactly like a person would.</summary>
        private sealed class AutoPlayer
        {
            private readonly HandInputService _in;
            private readonly MiniGameSession _s;
            private readonly MiniGameObjective _obj;
            private readonly Transform _prop;
            private readonly Transform _root;
            private readonly float _size;
            private Vector3 _hand;
            private bool _hasHand;
            private float _phaseClock;
            private int _phase;
            private float _orbit;

            public AutoPlayer(HandInputService input, MiniGameSession session, MiniGameObjective obj)
            {
                _in = input;
                _s = session;
                _obj = obj;
                _prop = session.Built.Prop;
                _root = session.Built.Root.transform;
                _size = Mathf.Max(0.03f, session.Built.SizeMeters);
            }

            private void Hand(Vector3 pos, Quaternion rot, bool pinch)
            {
                _hand = pos;
                _hasHand = true;
                _in.SetSyntheticHand(HandSide.Right, pos, rot, pinch);
            }

            private Vector3 MoveToward(Vector3 target, float speed, float dt)
            {
                if (!_hasHand) _hand = target;
                return Vector3.MoveTowards(_hand, target, speed * dt);
            }

            private Transform FindChild(string prefix, bool activeOnly, Vector3 near)
            {
                Transform best = null;
                float bd = float.MaxValue;
                for (int i = 0; i < _root.childCount; i++)
                {
                    var c = _root.GetChild(i);
                    if (!c.name.StartsWith(prefix)) continue;
                    if (activeOnly && !c.gameObject.activeSelf) continue;
                    float d = Vector3.Distance(c.position, near);
                    if (d < bd) { bd = d; best = c; }
                }
                return best;
            }

            public void Step(float t, float dt)
            {
                if (_prop == null) return;
                Vector3 p = _prop.position;
                Quaternion id = Quaternion.identity;

                switch (_obj)
                {
                    case MiniGameObjective.Spin:
                        // Grab, then twist the wrist.
                        if (t < 0.4f) Hand(p, id, false);
                        else Hand(p, Quaternion.Euler(0f, t * 600f, 0f), true);
                        break;

                    case MiniGameObjective.Balance:
                        // Leave it alone and just hover nearby.
                        Hand(p + new Vector3(0f, 0.12f, -0.10f), id, false);
                        break;

                    case MiniGameObjective.Catch:
                        PlayCatch(t, dt, p);
                        break;

                    case MiniGameObjective.Stir:
                        _orbit += 420f * dt;
                        float r = _size * 0.65f;
                        Vector3 c = p;
                        Hand(c + new Vector3(Mathf.Cos(_orbit * Mathf.Deg2Rad) * r, 0f, Mathf.Sin(_orbit * Mathf.Deg2Rad) * r),
                            id, false);
                        break;

                    case MiniGameObjective.Aim:
                        PlayAim(t, dt);
                        break;

                    case MiniGameObjective.Defend:
                        Hand(p + Vector3.up * (_size * 0.2f), id, false);
                        break;

                    case MiniGameObjective.Collect:
                        PlayCarry("Pickup", true, t, dt, p);
                        break;

                    case MiniGameObjective.Steady:
                        Hand(p + new Vector3(0f, _size * 0.4f, 0f), id, false);
                        break;

                    case MiniGameObjective.Pour:
                        if (t < 0.4f) Hand(p, id, false);
                        else
                        {
                            Quaternion tilt = Quaternion.Euler(0f, 0f, Mathf.Min(110f, (t - 0.4f) * 200f));
                            Hand(p, tilt, true);
                        }
                        break;

                    case MiniGameObjective.Race:
                        PlayRace(t, dt, p);
                        break;

                    default:
                        Hand(p, id, t > 0.4f);
                        break;
                }
            }

            private void PlayCarry(string prefix, bool activeOnly, float t, float dt, Vector3 p)
            {
                Quaternion id = Quaternion.identity;
                if (t < 0.4f) { Hand(p, id, false); return; }
                var target = FindChild(prefix, activeOnly, p);
                Vector3 dest = target != null ? target.position : p;
                Hand(MoveToward(dest, 0.55f, dt), id, true);
            }

            private int _raceIdx;
            public string Describe()
            {
                var pk = FindChild("Pickup", true, _prop.position);
                int active = 0;
                for (int i = 0; i < _root.childCount; i++) if (_root.GetChild(i).name.StartsWith("Pickup") && _root.GetChild(i).gameObject.activeSelf) active++;
                return _obj + " raceIdx=" + _raceIdx + " hand=" + _hand.ToString("F3") +
                       (pk != null ? " nearestPickup=" + pk.name + "@" + Vector3.Distance(pk.position, _prop.position).ToString("F3") + " active=" + active : "");
            }

            private void PlayRace(float t, float dt, Vector3 p)
            {
                Quaternion id = Quaternion.identity;
                if (t < 0.4f) { Hand(p, id, false); return; }
                Transform cp = _root.Find("Checkpoint" + (_raceIdx % 4));
                Vector3 dest = cp != null ? cp.position : p;
                if (cp != null && Vector3.Distance(p, dest) < Mathf.Max(0.08f, _size * 0.5f) * 0.8f) _raceIdx++;
                Hand(MoveToward(dest, 0.55f, dt), id, true);
            }

            private void PlayAim(float t, float dt)
            {
                var tgt = FindChild("AimTarget", true, _prop.position);
                Vector3 origin = _prop.position + new Vector3(0f, _size * 0.9f, -_size * 1.2f);
                Vector3 look = tgt != null ? tgt.position - origin : Vector3.forward;
                if (look.sqrMagnitude < 1e-6f) look = Vector3.forward;
                Hand(origin, Quaternion.LookRotation(look.normalized, Vector3.up), false);
            }

            private void PlayCatch(float t, float dt, Vector3 p)
            {
                Quaternion id = Quaternion.identity;
                _phaseClock += dt;
                var grab = _prop.GetComponent<GrabInteractable>();
                switch (_phase)
                {
                    case 0: // approach + pinch
                        Hand(p, id, _phaseClock > 0.3f);
                        if (_phaseClock > 0.6f) { _phase = 1; _phaseClock = 0f; }
                        break;
                    case 1: // lift
                        Hand(_hand + Vector3.up * (1.8f * dt) + (p - _hand) * 0f, id, true);
                        if (_hand.y > _s.Built.Root.transform.position.y + _size * 1.5f + 0.04f) { _phase = 2; _phaseClock = 0f; }
                        break;
                    case 2: // release (toss)
                        Hand(_hand + Vector3.up * (1.4f * dt), id, false);
                        if (_phaseClock > 0.25f) { _phase = 3; _phaseClock = 0f; }
                        break;
                    case 3: // wait for fall, hover beneath
                        Hand(MoveToward(p + Vector3.up * 0.02f, 2.5f, dt), id, false);
                        if (_phaseClock > 0.35f && (grab == null || !grab.IsHeld))
                        {
                            float d = Vector3.Distance(_hand, p);
                            if (d < 0.05f || _phaseClock > 1.8f) { _phase = 0; _phaseClock = 0f; }
                        }
                        break;
                }
            }
        }
    }
}
