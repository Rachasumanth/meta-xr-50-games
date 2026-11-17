using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Real-time objective loop shared by all 50 boards. The original rig and
    /// its gate-tested simulation keep running; this layer supplies the physical
    /// player interaction, live scoring and win/lose flow of the unified app.
    /// </summary>
    [DefaultExecutionOrder(60)]
    public sealed class MiniGameSession : MonoBehaviour
    {
        public GameDefinition Game;
        public BuiltMiniGame Built;
        public HandInputService Input;
        public GrabInteractable Grab;
        public MiniGameHUD Hud;

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsWon { get; private set; }
        public bool IsLost { get; private set; }
        public string LoseReason { get; private set; } = "";
        public string LastPenalty { get; private set; } = "";
        public int PenaltyCount { get; private set; }
        public float Score { get; private set; }
        public float Target { get; private set; }
        public float TimeLeft { get; private set; }
        public float Integrity { get; private set; } = 100f;

        private float _positiveTime;
        private float _lastHaptic;
        private int _lastCombo = -1;
        private Vector3 _homePosition;
        private Quaternion _homeRotation;
        private Vector3 _prevPropPosition;
        private Quaternion _prevPropRotation;
        private float _propAngularSpeed;
        private float _handSpeed;
        private float _handNear;
        private bool _wasAirborne;
        private float _penaltyCooldown;
        private float _aimLockTime;
        private float _spawnTimer;
        private float _stirAngle;
        private float _stirLastAngle;
        private bool _stirAngleInit;
        private int _raceCheckpoint;
        private Transform _cam;

        private readonly List<GameObject> _dynamic = new List<GameObject>();
        private readonly List<Transform> _targets = new List<Transform>();
        private readonly List<GameObject> _pickups = new List<GameObject>();
        private readonly List<GameObject> _incoming = new List<GameObject>();
        private readonly List<Transform> _checkpoints = new List<Transform>();

        public void Setup(GameDefinition game, BuiltMiniGame built, HandInputService input)
        {
            Game = game;
            Built = built;
            Input = input;
            _cam = Camera.main != null ? Camera.main.transform : null;

            Grab = built.Prop != null ? built.Prop.GetComponent<GrabInteractable>() : null;
            _homePosition = built.Prop != null ? built.Prop.position : built.Root.transform.position;
            _homeRotation = built.Prop != null ? built.Prop.rotation : Quaternion.identity;
            if (Grab != null) Grab.SetHome(_homePosition, _homeRotation);

            Target = 100f + 25f * Mathf.Clamp(game.Difficulty, 1, 3);
            TimeLeft = 80f + 12f * Mathf.Clamp(game.Difficulty, 1, 3);
            Score = 0f;
            Integrity = 100f;
            IsPlaying = true;

            Hud = MiniGameHUD.Create(built.Root.transform, game.Title, built.SizeMeters, game.Accent);
            Hud.SetHint(game.Instruction);

            InitializeObjective();
            _prevPropPosition = built.Prop.position;
            _prevPropRotation = built.Prop.rotation;
            AudioDirector.Instance?.Chime();
        }

        private void InitializeObjective()
        {
            switch (Game.Objective)
            {
                case MiniGameObjective.Aim:
                    for (int i = 0; i < 3; i++)
                    {
                        var target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        target.name = "AimTarget" + i;
                        target.transform.SetParent(Built.Root.transform, false);
                        target.transform.localScale = Vector3.one * Mathf.Max(0.02f, Built.SizeMeters * 0.10f);
                        target.GetComponent<Renderer>().material = MaterialLibrary.Glow(Game.Accent, 2.2f);
                        var c = target.GetComponent<Collider>();
                        if (c != null) Destroy(c);
                        PlaceTarget(target.transform, i);
                        _targets.Add(target.transform);
                    }
                    break;
                case MiniGameObjective.Collect:
                    for (int i = 0; i < 8; i++)
                    {
                        var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        pickup.name = "Pickup" + i;
                        pickup.transform.SetParent(Built.Root.transform, false);
                        pickup.transform.localScale = Vector3.one * Mathf.Max(0.012f, Built.SizeMeters * 0.055f);
                        pickup.GetComponent<Renderer>().material = MaterialLibrary.Glow(Game.Accent, 2.4f);
                        var c = pickup.GetComponent<Collider>();
                        if (c != null) Destroy(c);
                        PlacePickup(pickup, i);
                        _pickups.Add(pickup);
                    }
                    break;
                case MiniGameObjective.Defend:
                    _spawnTimer = 1.2f;
                    break;
                case MiniGameObjective.Race:
                    for (int i = 0; i < 4; i++)
                    {
                        var cp = new GameObject("Checkpoint" + i);
                        cp.transform.SetParent(Built.Root.transform, false);
                        cp.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
                        cp.AddComponent<MeshRenderer>().material = MaterialLibrary.Glow(Game.Accent, i == 0 ? 2.4f : 1.1f);
                        float a = i * 90f * Mathf.Deg2Rad;
                        float r = Mathf.Min(Built.SizeMeters * 0.85f, 0.20f);
                        cp.transform.localPosition = new Vector3(Mathf.Cos(a) * r, Built.SizeMeters * 0.35f, Mathf.Sin(a) * r);
                        cp.transform.localScale = new Vector3(Built.SizeMeters * 0.45f, 1f, Built.SizeMeters * 0.45f);
                        cp.transform.localRotation = Quaternion.Euler(90f, i * 90f, 0f);
                        _checkpoints.Add(cp.transform);
                    }
                    break;
            }
        }

        private void PlaceTarget(Transform t, int index)
        {
            float angle = (index * 120f + 30f) * Mathf.Deg2Rad;
            float radius = Mathf.Min(Built.SizeMeters * 1.4f, 0.22f);
            float height = Built.SizeMeters * 1.2f + (index % 2) * Built.SizeMeters * 0.3f;
            t.localPosition = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
        }

        private void PlacePickup(GameObject pickup, int index)
        {
            float angle = (index * 47f) * Mathf.Deg2Rad;
            float radius = Mathf.Min(Built.SizeMeters * (0.7f + (index % 3) * 0.25f), 0.20f);
            pickup.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius,
                0.04f + Built.SizeMeters * (0.2f + (index % 4) * 0.15f), Mathf.Sin(angle) * radius);
        }

        public void TogglePause()
        {
            if (IsWon || IsLost) return;
            IsPaused = !IsPaused;
            if (Hud != null)
            {
                Hud.SetStatus(IsPaused ? "PAUSED  -  MENU TO RESUME" : "IN PLAY",
                    IsPaused ? new Color(1f, 0.85f, 0.5f) : new Color(0.6f, 1f, 0.8f));
            }
        }

        private void Update()
        {
            if (IsWon || IsLost)
            {
                if (Input != null && Input.BackDown) return;
                if (Input != null && (Input.SelectDown || Input.PresetGesture > 0))
                {
                    Restart();
                }
                return;
            }

            if (!IsPlaying) return;
            if (IsPaused) return;

            float dt = Time.deltaTime;
            TimeLeft -= dt;
            if (TimeLeft <= 0f)
            {
                Lose("TIME OUT");
                return;
            }

            if (Built.Prop == null) return;

            Vector3 propPos = Built.Prop.position;
            Quaternion propRot = Built.Prop.rotation;
            float angleDelta = Quaternion.Angle(_prevPropRotation, propRot);
            _propAngularSpeed = Mathf.Lerp(_propAngularSpeed, angleDelta / Mathf.Max(0.0001f, dt), 0.35f);

            // Hand proximity + speed.
            float bestHand = float.MaxValue;
            float handSpeed = 0f;
            if (Input != null)
            {
                bestHand = Mathf.Min(bestHand, Vector3.Distance(Input.Left.IndexTip, propPos));
                bestHand = Mathf.Min(bestHand, Vector3.Distance(Input.Right.IndexTip, propPos));
                var near = Vector3.Distance(Input.Left.IndexTip, propPos) < Vector3.Distance(Input.Right.IndexTip, propPos) ? Input.Left : Input.Right;
                handSpeed = near.Velocity.magnitude;
            }
            _handNear = bestHand;
            _handSpeed = handSpeed;

            if (_penaltyCooldown > 0f) _penaltyCooldown -= dt;

            // Out-of-bounds recovery.
            if (propPos.y < Built.Root.transform.position.y - 0.20f ||
                Vector3.Distance(propPos, Built.Root.transform.position) > 3.0f)
            {
                if (Grab != null) Grab.ReturnHome();
                Penalize("RESET");
            }

            switch (Game.Objective)
            {
                case MiniGameObjective.Spin: EvaluateSpin(dt); break;
                case MiniGameObjective.Balance: EvaluateBalance(dt); break;
                case MiniGameObjective.Catch: EvaluateCatch(dt); break;
                case MiniGameObjective.Stir: EvaluateStir(dt); break;
                case MiniGameObjective.Aim: EvaluateAim(dt); break;
                case MiniGameObjective.Defend: EvaluateDefend(dt); break;
                case MiniGameObjective.Collect: EvaluateCollect(dt); break;
                case MiniGameObjective.Steady: EvaluateSteady(dt); break;
                case MiniGameObjective.Pour: EvaluatePour(dt); break;
                case MiniGameObjective.Race: EvaluateRace(dt); break;
            }

            _prevPropPosition = propPos;
            _prevPropRotation = propRot;

            if (IsWon || IsLost) return;
            if (Score >= Target) Win();

            if (Hud != null)
            {
                Hud.SetScore(Score, Target);
                Hud.SetTimer(TimeLeft);
                if (!IsWon && !IsLost)
                {
                    int combo = ComboMultiplier();
                    if (combo != _lastCombo)
                    {
                        _lastCombo = combo;
                        Hud.SetStatus("IN PLAY  x" + combo, new Color(0.65f, 1f, 0.85f));
                    }
                }
            }
        }

        private int ComboMultiplier()
        {
            return Mathf.Clamp(1 + Mathf.FloorToInt(_positiveTime / 10f), 1, 3);
        }

        private void Award(float rate, string hint)
        {
            float dt = Time.deltaTime;
            _positiveTime += dt;
            Score += rate * dt * ComboMultiplier();
            if (Hud != null) Hud.SetHint(hint);
            if (Time.time - _lastHaptic > 1.2f)
            {
                _lastHaptic = Time.time;
                Input?.Haptic(HandSide.Right, 0.12f, 0.03f);
            }
        }

        private void Bonus(float amount, string reason)
        {
            Score += amount * ComboMultiplier();
            if (Hud != null) Hud.SetHint(reason);
            AudioDirector.Instance?.Chime();
            Input?.Haptic(HandSide.Right, 0.35f, 0.06f);

            if (Built != null && Built.Prop != null)
            {
                Vector3 pos = Built.Prop.position + Vector3.up * (Built.SizeMeters * 0.7f);
                ScorePopup.Show(Built.Root.transform, pos,
                    "+" + Mathf.RoundToInt(amount * ComboMultiplier()), Game.Accent,
                    0.11f, 0.85f, 0.018f);
                SparkleBurst.Play(Built.Root.transform, Built.Prop.position, Game.Accent,
                    9, 0.10f, 0.45f, 0.004f);
            }
        }

        private void Penalize(string reason)
        {
            if (_penaltyCooldown > 0f) return;
            _penaltyCooldown = 1.0f;
            LastPenalty = reason;
            PenaltyCount++;
            _positiveTime = Mathf.Max(0f, _positiveTime - 6f);
            IntegrateIntegrity(-5f);
            if (Hud != null) Hud.SetHint(reason);
            AudioDirector.Instance?.Error();
            if (Built != null && Built.Prop != null)
                SparkleBurst.Play(Built.Root.transform, Built.Prop.position,
                    new Color(1f, 0.32f, 0.25f), 6, 0.07f, 0.32f, 0.0035f);
        }

        private void IntegrateIntegrity(float delta)
        {
            Integrity = Mathf.Clamp(Integrity + delta, 0f, 100f);
            if (Integrity <= 0f && !IsLost) Lose("INTEGRITY LOST");
        }

        private void EvaluateSpin(float dt)
        {
            if (_propAngularSpeed > 220f)
                Award(3.2f, string.Format("SPIN {0:0} deg/s", _propAngularSpeed));
            else if (_propAngularSpeed < 40f)
                _positiveTime = Mathf.Max(0f, _positiveTime - dt);
            else
                Award(0.6f, "KEEP SPINNING");
        }

        private void EvaluateBalance(float dt)
        {
            // Balancing needs the player: with no hand near the object nothing is
            // scored (an idle board used to win by itself after ~45 s).
            if (_handNear > Mathf.Max(0.30f, Built.SizeMeters * 2.2f))
            {
                _positiveTime = Mathf.Max(0f, _positiveTime - dt);
                if (Hud != null) Hud.SetHint("BRING YOUR HAND CLOSE TO BALANCE");
                return;
            }

            float up = Mathf.Abs(Vector3.Dot(Built.Prop.up, Vector3.up));
            if (up > 0.72f) Award(2.6f, string.Format("BALANCED {0:0}%", up * 100f));
            else if (up < 0.38f)
            {
                _positiveTime = Mathf.Max(0f, _positiveTime - dt * 3f);
                IntegrateIntegrity(-dt * 4f);
                if (Hud != null) Hud.SetHint("TIP RISK");
            }
            else Award(0.4f, "CORRECT THE TILT");
        }

        private void EvaluateCatch(float dt)
        {
            float height = Built.Prop.position.y - Built.Root.transform.position.y;
            bool held = Grab != null && Grab.IsHeld;
            bool airborne = height > Built.SizeMeters * 0.85f && !held;

            if (airborne) _wasAirborne = true;

            if (_wasAirborne && held && height < Built.SizeMeters * 0.9f)
            {
                Bonus(14f, "CLEAN CATCH");
                _wasAirborne = false;
            }
            else if (!airborne && !held && height < 0.03f && _wasAirborne)
            {
                _wasAirborne = false;
                Penalize("MISSED CATCH");
            }
            else
            {
                if (Hud != null) Hud.SetHint(held ? "THROW HIGH, THEN CATCH" : "GRAB AND TOSS");
            }
        }

        private HandState ActiveHand()
        {
            if (Input == null) return default;
            var l = Input.Left;
            var r = Input.Right;
            if (!l.IsTracked) return r;
            if (!r.IsTracked) return l;
            Vector3 p = Built != null && Built.Prop != null ? Built.Prop.position : transform.position;
            return Vector3.Distance(l.IndexTip, p) <= Vector3.Distance(r.IndexTip, p) ? l : r;
        }

        private void EvaluateStir(float dt)
        {
            if (Input == null || Built.Prop == null) return;
            Vector3 hand = ActiveHand().IndexTip;
            Vector3 d = hand - Built.Prop.position;
            d.y = 0f;
            float r = d.magnitude;
            float angle = Mathf.Atan2(d.z, d.x);
            if (!_stirAngleInit) { _stirLastAngle = angle; _stirAngleInit = true; }
            float deltaAngle = Mathf.DeltaAngle(_stirLastAngle * Mathf.Rad2Deg, angle * Mathf.Rad2Deg);
            _stirLastAngle = angle;
            float speed = Mathf.Abs(deltaAngle) * Mathf.Deg2Rad * Mathf.Max(0.01f, r) / Mathf.Max(0.0001f, dt);
            bool inBand = r > Built.SizeMeters * 0.30f && r < Built.SizeMeters * 1.1f;

            // Rate-based (deg/s) so scoring does not depend on the display refresh rate.
            float angularRate = Mathf.Abs(deltaAngle) / Mathf.Max(0.0001f, dt);
            if (inBand && angularRate > 45f)
            {
                _stirAngle += Mathf.Abs(deltaAngle);
                Award(3.0f, string.Format("STIR {0:0} deg/s", speed));
            }
            else
            {
                _positiveTime = Mathf.Max(0f, _positiveTime - dt);
                if (Hud != null) Hud.SetHint("TRACE THE RIM");
            }
        }

        private void EvaluateAim(float dt)
        {
            if (_targets.Count == 0 || Input == null) return;
            Ray ray = ActiveHand().PointerRay;
            Transform best = null;
            float bestDot = 0.96f;
            for (int i = 0; i < _targets.Count; i++)
            {
                Vector3 to = _targets[i].position - ray.origin;
                float dot = Vector3.Dot(ray.direction, to.normalized);
                if (dot > bestDot) { bestDot = dot; best = _targets[i]; }
            }

            if (best != null)
            {
                _aimLockTime += dt;
                if (Hud != null) Hud.SetHint(string.Format("LOCK {0:0}%", bestDot * 100f));
                if (_aimLockTime > 0.35f)
                {
                    Bonus(12f, "TARGET LOCKED");
                    PlaceTarget(best, Random.Range(0, 100));
                    _aimLockTime = 0f;
                }
            }
            else
            {
                _aimLockTime = 0f;
                _positiveTime = Mathf.Max(0f, _positiveTime - dt * 0.5f);
                if (Hud != null) Hud.SetHint("POINT AT A GLOWING TARGET");
            }
        }

        private void EvaluateDefend(float dt)
        {
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = Mathf.Lerp(2.4f, 1.3f, Mathf.Clamp01((100f - Integrity) / 100f));
                SpawnIncoming();
            }

            for (int i = _incoming.Count - 1; i >= 0; i--)
            {
                var obj = _incoming[i];
                if (obj == null) { _incoming.RemoveAt(i); continue; }
                Vector3 to = Built.Prop.position - obj.transform.position;
                float dist = to.magnitude;
                obj.transform.position += to.normalized * Mathf.Min(dist, 0.55f * dt);
                obj.transform.Rotate(Vector3.up, 180f * dt, Space.World);

                if (dist < Mathf.Max(0.09f, Built.SizeMeters * 0.45f))
                {
                    bool shielded = _handNear < Mathf.Max(0.12f, Built.SizeMeters * 0.7f);
                    if (shielded)
                    {
                        Bonus(10f, "BLOCKED");
                        AudioDirector.Instance?.Click();
                    }
                    else
                    {
                        IntegrateIntegrity(-18f);
                        Penalize("HIT");
                    }
                    Destroy(obj);
                    _incoming.RemoveAt(i);
                }
            }

            if (Hud != null) Hud.SetHint(string.Format("DEFEND  integrity {0:0}%", Integrity));
        }

        private void SpawnIncoming()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Incoming";
            go.transform.SetParent(Built.Root.transform, false);
            go.transform.localScale = Vector3.one * Mathf.Max(0.012f, Built.SizeMeters * 0.07f);
            go.GetComponent<Renderer>().material = MaterialLibrary.Glow(new Color(1f, 0.25f, 0.18f), 2.5f);
            var c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            float a = Random.Range(0f, Mathf.PI * 2f);
            float r = Mathf.Min(Built.SizeMeters * 1.6f, 0.24f);
            go.transform.position = Built.Root.transform.position + new Vector3(Mathf.Cos(a) * r, Built.SizeMeters * 0.8f, Mathf.Sin(a) * r);
            _incoming.Add(go);
        }

        private void EvaluateCollect(float dt)
        {
            int collected = 0;
            for (int i = 0; i < _pickups.Count; i++)
            {
                var p = _pickups[i];
                if (p == null || !p.activeSelf) { collected++; continue; }
                if (Vector3.Distance(p.transform.position, Built.Prop.position) < Mathf.Max(0.06f, Built.SizeMeters * 0.42f))
                {
                    p.SetActive(false);
                    Bonus(9f, "COLLECTED");
                    collected++;
                }
            }

            if (collected >= _pickups.Count)
            {
                Bonus(20f, "TRAY CLEARED");
                for (int i = 0; i < _pickups.Count; i++)
                {
                    if (_pickups[i] == null) continue;
                    _pickups[i].SetActive(true);
                    PlacePickup(_pickups[i], i);
                }
            }
            else if (Hud != null)
            {
                Hud.SetHint(string.Format("ROLL OVER PICKUPS  {0}/{1}", collected, _pickups.Count));
            }
        }

        private void EvaluateSteady(float dt)
        {
            if (_handNear < Mathf.Max(0.20f, Built.SizeMeters * 0.9f) && _handSpeed < 0.16f)
                Award(2.4f, "STEADY");
            else
            {
                _positiveTime = Mathf.Max(0f, _positiveTime - dt * 0.8f);
                if (Hud != null) Hud.SetHint(_handNear > 0.5f ? "BRING YOUR HAND CLOSER" : "HOLD STILL");
            }
        }

        private void EvaluatePour(float dt)
        {
            float up = Mathf.Abs(Vector3.Dot(Built.Prop.up, Vector3.up));
            if (up < 0.55f && _handNear < Mathf.Max(0.25f, Built.SizeMeters))
                Award(2.8f, "POURING");
            else
            {
                _positiveTime = Mathf.Max(0f, _positiveTime - dt * 0.5f);
                if (Hud != null) Hud.SetHint("TILT OVER THE BASIN");
            }
        }

        private void EvaluateRace(float dt)
        {
            if (_checkpoints.Count == 0) return;
            Transform cp = _checkpoints[_raceCheckpoint % _checkpoints.Count];
            if (Vector3.Distance(Built.Prop.position, cp.position) < Mathf.Max(0.08f, Built.SizeMeters * 0.5f))
            {
                Bonus(12f, "CHECKPOINT " + (_raceCheckpoint + 1));
                _raceCheckpoint++;
                if (_raceCheckpoint % _checkpoints.Count == 0) Bonus(20f, "LAP COMPLETE");
            }
            else if (Hud != null)
            {
                float d = Vector3.Distance(Built.Prop.position, cp.position);
                Hud.SetHint(string.Format("NEXT GATE {0:0} mm", d * 1000f));
            }
        }

        private void Win()
        {
            IsWon = true;
            IsPlaying = false;
            if (Hud != null)
            {
                Hud.SetStatus("VICTORY  -  PRESS A / PINCH TO REPLAY", new Color(0.5f, 1f, 0.7f));
                Hud.SetHint(Game.Title + " mastered");
            }
            AudioDirector.Instance?.Success();
            Input?.Haptic(HandSide.Right, 0.8f, 0.35f);
            Input?.Haptic(HandSide.Left, 0.8f, 0.35f);
            if (Grab != null) Grab.CanGrab = false;
            if (Built != null && Built.Prop != null)
            {
                Vector3 p = Built.Prop.position;
                SparkleBurst.Play(Built.Root.transform, p, new Color(0.4f, 1f, 0.75f), 28, 0.36f, 0.95f, 0.007f);
                SparkleBurst.Play(Built.Root.transform, p, Game.Accent, 20, 0.26f, 0.80f, 0.006f);
                ScorePopup.Show(Built.Root.transform, p + Vector3.up * 0.12f, "VICTORY",
                    new Color(0.5f, 1f, 0.75f), 0.16f, 1.4f, 0.026f);
            }
        }

        private void Lose(string reason)
        {
            IsLost = true;
            LoseReason = reason;
            IsPlaying = false;
            if (Hud != null)
            {
                Hud.SetStatus(reason + "  -  PRESS A / PINCH TO RETRY", new Color(1f, 0.5f, 0.45f));
                Hud.SetHint("Take a breath, then try again");
            }
            AudioDirector.Instance?.Error();
            if (Grab != null) Grab.CanGrab = false;
            if (Built != null && Built.Prop != null)
                SparkleBurst.Play(Built.Root.transform, Built.Prop.position,
                    new Color(1f, 0.32f, 0.25f), 12, 0.16f, 0.55f, 0.005f);
        }

        public void Restart()
        {
            Score = 0f;
            TimeLeft = 80f + 12f * Mathf.Clamp(Game.Difficulty, 1, 3);
            Integrity = 100f;
            _positiveTime = 0f;
            _raceCheckpoint = 0;
            _stirAngle = 0f;
            _stirAngleInit = false;
            _aimLockTime = 0f;
            _wasAirborne = false;
            _lastCombo = -1;
            IsWon = false;
            IsLost = false;
            IsPaused = false;
            IsPlaying = true;

            for (int i = _incoming.Count - 1; i >= 0; i--) if (_incoming[i] != null) Destroy(_incoming[i]);
            _incoming.Clear();

            if (Grab != null)
            {
                Grab.CanGrab = true;
                Grab.ReturnHome();
            }
            if (Hud != null)
            {
                Hud.SetStatus("IN PLAY", new Color(0.65f, 1f, 0.85f));
                Hud.SetHint(Game.Instruction);
                Hud.SetScore(0f, Target);
                Hud.SetTimer(TimeLeft);
            }
        }

        private void OnDestroy()
        {
            if (Grab != null) Grab.CanGrab = false;
        }
    }
}

