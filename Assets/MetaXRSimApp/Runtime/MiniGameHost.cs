using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    public struct GameLaunchContext
    {
        public GameDefinition Game;
        public ShapeSignature Shape;
        public RealObjectProfile Object;
        public Vector3 Position;
        public Quaternion Rotation;
        public float SizeMeters;
    }

    public sealed class LaunchReport
    {
        public bool Success;
        public string Slug;
        public string RigType;
        public int BoundTransforms;
        public string Error;
        public float SizeMeters;
    }

    /// <summary>
    /// Converts a selected real object / gesture volume into a live mini-game
    /// board, attaches the original rig, and manages the session lifecycle.
    /// </summary>
    public sealed class MiniGameHost : MonoBehaviour
    {
        public HandInputService Input;
        public Transform PlayArea;
        public Transform HubAnchor;

        public event Action Exited;
        public event Action<LaunchReport> Launched;

        public bool InGame => _activeRoot != null;
        public bool IsConverting => _converting;
        public MiniGameSession Session => _session;
        public LaunchReport LastReport { get; private set; }

        private GameObject _activeRoot;
        private BuiltMiniGame _built;
        private MiniGameSession _session;
        private Coroutine _conversion;
        private readonly List<WorldButton> _gameButtons = new List<WorldButton>();
        private bool _converting;
        private bool _suppressSelectThisFrame;

        public void Initialize(HandInputService input, Transform playArea, Transform hubAnchor)
        {
            Input = input;
            PlayArea = playArea;
            HubAnchor = hubAnchor;
            if (PlayArea == null)
            {
                var go = new GameObject("PlayArea");
                go.transform.SetParent(transform, false);
                PlayArea = go.transform;
            }
        }

        public void Launch(GameDefinition game, ShapeSignature shape, RealObjectProfile realObject,
            Vector3 fallbackPosition, Quaternion fallbackRotation)
        {
            if (game == null) return;
            if (_conversion != null) StopCoroutine(_conversion);
            if (_cage != null) { Destroy(_cage); _cage = null; }
            if (_activeRoot != null) DestroyGameImmediate();

            MusicDirector.Instance?.PlayFamily(game.Family, 0.7f);

            float physical = GameMatchmaker.PhysicalSizeMeters(shape, realObject);
            float min = game.MinSizeMm * 0.001f * 0.80f;
            float max = game.MaxSizeMm * 0.001f * 1.20f;
            float size = Mathf.Clamp(physical, min, max);
            if (game.Shape.IsDome())
                size = Mathf.Clamp(size, min, max);

            Vector3 position;
            Quaternion rotation;
            ResolvePose(realObject, fallbackPosition, fallbackRotation, out position, out rotation);

            var ctx = new GameLaunchContext
            {
                Game = game,
                Shape = shape,
                Object = realObject,
                Position = position,
                Rotation = rotation,
                SizeMeters = size
            };

            _converting = true;
            _conversion = StartCoroutine(ConvertAndSpawn(ctx));
        }

        public void ResolvePose(RealObjectProfile obj, Vector3 fallbackPosition, Quaternion fallbackRotation,
            out Vector3 position, out Quaternion rotation)
        {
            if (obj.IsValid)
            {
                position = obj.PositionWorld + obj.SurfaceNormalWorld.normalized * 0.003f;
            }
            else
            {
                position = fallbackPosition;
            }

            Vector3 forward = position - (Input != null && Input.HeadCamera != null
                ? Input.HeadCamera.transform.position
                : new Vector3(0f, 1.4f, -0.5f));
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private IEnumerator ConvertAndSpawn(GameLaunchContext ctx)
        {
            AudioDirector.Instance?.Whoosh();
            if (Input != null) Input.Haptic(HandSide.Right, 0.4f, 0.15f);

            GameObject cage = BuildConversionCage(ctx);
            _cage = cage;
            SparkleBurst.Play(cage.transform, ctx.Position, ctx.Game.Accent, 20, 0.28f, 0.7f, 0.0055f);
            float t = 0f;
            const float duration = 0.78f;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (cage != null)
                {
                    float p = Mathf.Clamp01(t / duration);
                    float pulse = 1f + 0.10f * Mathf.Sin(p * Mathf.PI * 6f) * (1f - p);
                    cage.transform.localScale = Vector3.one * pulse;
                    cage.transform.Rotate(Vector3.up, 140f * Time.deltaTime, Space.World);
                    var renderers = cage.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        var m = renderers[i].material;
                        if (m.HasProperty("_Color"))
                        {
                            Color c = ctx.Game.Accent;
                            c.a = Mathf.Lerp(0.9f, 0.08f, p);
                            m.color = c;
                        }
                    }
                }
                yield return null;
            }
            if (cage != null) Destroy(cage);
            _cage = null;

            SpawnGame(ctx);
            SparkleBurst.Play(_activeRoot != null ? _activeRoot.transform : transform,
                ctx.Position, ctx.Game.Accent, 26, 0.30f, 0.8f, 0.006f);
            _conversion = null;
            _converting = false;
            AudioDirector.Instance?.Chime();
        }

        private GameObject BuildConversionCage(GameLaunchContext ctx)
        {
            var cage = new GameObject("ConversionCage");
            cage.transform.SetParent(PlayArea != null ? PlayArea : transform, false);
            cage.transform.position = ctx.Position;
            cage.transform.rotation = ctx.Rotation;

            if (ctx.Object.IsValid)
            {
                Vector3 size = ctx.Object.SizeMeters;
                cage.transform.localScale = Vector3.one;
                BoxWire(cage.transform, size, ctx.Game.Accent);
                for (int i = 0; i < 4; i++)
                {
                    var ring = new GameObject("ScanRing" + i);
                    ring.transform.SetParent(cage.transform, false);
                    ring.transform.localPosition = new Vector3(0f, size.y * (i * 0.25f), 0f);
                    ring.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
                    ring.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(ctx.Game.Accent, 0.6f);
                    float d = Mathf.Max(size.x, size.z) * 1.05f;
                    ring.transform.localScale = new Vector3(d, 1f, d);
                    var scan = ring.AddComponent<AccessoryAnimator>();
                    scan.SpinDegPerSec = 70f + i * 26f;
                    scan.SpinAxis = Vector3.up;
                    scan.PulseAmount = 0.05f;
                    scan.PulseSpeed = 2f + i * 0.4f;
                }
            }
            else
            {
                BoxWire(cage.transform, Vector3.one * Mathf.Max(0.05f, ctx.SizeMeters), ctx.Game.Accent);
                for (int i = 0; i < 3; i++)
                {
                    var ring = new GameObject("ScanRing" + i);
                    ring.transform.SetParent(cage.transform, false);
                    ring.transform.localPosition = new Vector3(0f, 0.02f * i, 0f);
                    ring.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
                    ring.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(ctx.Game.Accent, 0.55f);
                    ring.transform.localScale = new Vector3(ctx.SizeMeters * 1.3f, 1f, ctx.SizeMeters * 1.3f);
                    var scan = ring.AddComponent<AccessoryAnimator>();
                    scan.SpinDegPerSec = 82f + i * 30f;
                    scan.SpinAxis = Vector3.up;
                    scan.PulseAmount = 0.06f;
                    scan.PulseSpeed = 2.2f + i * 0.4f;
                }
            }
            return cage;
        }

        private static void BoxWire(Transform parent, Vector3 size, Color color)
        {
            Vector3 h = size * 0.5f;
            Vector3[] corners =
            {
                new Vector3(-h.x, 0f, -h.z), new Vector3(h.x, 0f, -h.z),
                new Vector3(h.x, 0f, h.z), new Vector3(-h.x, 0f, h.z),
                new Vector3(-h.x, size.y, -h.z), new Vector3(h.x, size.y, -h.z),
                new Vector3(h.x, size.y, h.z), new Vector3(-h.x, size.y, h.z)
            };
            int[,] edges =
            {
                {0,1},{1,2},{2,3},{3,0},{4,5},{5,6},{6,7},{7,4},{0,4},{1,5},{2,6},{3,7}
            };

            var go = new GameObject("Wire");
            go.transform.SetParent(parent, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = edges.GetLength(0) * 2;
            lr.startWidth = 0.0035f;
            lr.endWidth = 0.0035f;
            lr.material = MaterialLibrary.Unlit(color);
            lr.startColor = color;
            lr.endColor = color;
            for (int i = 0; i < edges.GetLength(0); i++)
            {
                lr.SetPosition(i * 2, corners[edges[i, 0]]);
                lr.SetPosition(i * 2 + 1, corners[edges[i, 1]]);
            }
        }

        private void SpawnGame(GameLaunchContext ctx)
        {
            var report = new LaunchReport
            {
                Slug = ctx.Game.Slug,
                RigType = ctx.Game.RigTypeName,
                SizeMeters = ctx.SizeMeters
            };

            _activeRoot = new GameObject("MiniGame_" + ctx.Game.Slug);
            _activeRoot.transform.SetParent(PlayArea != null ? PlayArea : transform, false);
            _activeRoot.transform.position = ctx.Position;
            _activeRoot.transform.rotation = ctx.Rotation;
            _activeRoot.transform.localScale = Vector3.one * 0.90f;
            var spawnedRoot = _activeRoot;
            Tweener.Play(spawnedRoot, 0.42f, t =>
            {
                if (spawnedRoot != null)
                    spawnedRoot.transform.localScale = Vector3.one * Mathf.Lerp(0.90f, 1f, Ease.Evaluate(EaseType.OutBack, t));
            }, EaseType.Linear, 0f, false, () =>
            {
                if (spawnedRoot != null) spawnedRoot.transform.localScale = Vector3.one;
            });

            try
            {
                _built = MiniGameVisualBuilder.Build(ctx.Game, _activeRoot.transform, ctx.SizeMeters);
                if (_built.Prop == null)
                {
                    report.Error = "Prop build failed";
                    report.Success = false;
                    Debug.LogError("[MiniGameHost] " + report.Error);
                    Destroy(_activeRoot);
                    _activeRoot = null;
                    LastReport = report;
                    Launched?.Invoke(report);
                    return;
                }

                var rb = _built.Prop.GetComponent<Rigidbody>();
                if (rb == null) rb = _built.Prop.gameObject.AddComponent<Rigidbody>();
                var grab = _built.Prop.gameObject.AddComponent<GrabInteractable>();
                grab.GrabRadiusMeters = Mathf.Clamp(ctx.SizeMeters * 0.9f, 0.10f, 0.24f);
                grab.Setup(Input, rb);

                var guard = _built.Prop.gameObject.AddComponent<RigDrivenPropGuard>();
                guard.Grab = grab;
                guard.Body = rb;

                var bind = RigBinder.Bind(_activeRoot, _built, ctx.Game, Input);
                report.BoundTransforms = bind.BoundTransforms;
                if (bind.Parts != null && bind.Parts.Count > 0)
                {
                    var clamp = _activeRoot.AddComponent<RigPartClamp>();
                    clamp.Init(_activeRoot.transform, bind.Parts, Mathf.Max(0.25f, ctx.SizeMeters * 1.5f));
                }
                report.Error = bind.Error;
                report.Success = bind.Success;
                if (!bind.Success)
                    Debug.LogWarning("[MiniGameHost] " + ctx.Game.Slug + ": " + bind.Error);
                else
                    Debug.Log("[MiniGameHost] " + ctx.Game.Slug + " launched (" + bind.BoundTransforms + " transform bindings)");

                _session = _activeRoot.AddComponent<MiniGameSession>();
                _session.Setup(ctx.Game, _built, Input);

                CreateGameButtons(_activeRoot.transform, _built);

                if (!string.IsNullOrEmpty(report.Error) && _session.Hud != null)
                    _session.Hud.SetStatus("RIG PARTIAL - " + report.Error, new Color(1f, 0.7f, 0.4f));
            }
            catch (Exception e)
            {
                report.Success = false;
                report.Error = e.Message;
                Debug.LogError("[MiniGameHost] Spawn failed: " + e);
                if (_activeRoot != null) Destroy(_activeRoot);
                _activeRoot = null;
            }

            LastReport = report;
            Launched?.Invoke(report);
        }

        private void CreateGameButtons(Transform root, BuiltMiniGame built)
        {
            _gameButtons.Clear();
            float s = built.SizeMeters;
            // Buttons sit beside the object, clear of the grab zone for small props and
            // never further out than a seated player's comfortable reach for large ones.
            float bx = Mathf.Clamp(s * 1.15f, 0.16f, 0.21f);
            float bz = -Mathf.Clamp(s * 0.45f, 0.05f, 0.10f);
            float bw = Mathf.Clamp(s * 0.55f, 0.09f, 0.11f);
            Vector3 pos = new Vector3(bx, 0.035f, bz);
            var exit = WorldButton.Create("GameExit", root, "EXIT", pos,
                Quaternion.identity, new Vector2(bw, 0.032f),
                new Color32(52, 62, 78, 255), new Color32(255, 120, 110, 255), 0.011f);
            exit.Clicked = Exit;
            _gameButtons.Add(exit);

            var restart = WorldButton.Create("GameRestart", root, "RESET",
                new Vector3(-bx, 0.035f, bz), Quaternion.identity,
                new Vector2(bw, 0.032f),
                new Color32(52, 62, 78, 255), new Color32(120, 220, 255, 255), 0.011f);
            restart.Clicked = () => { _session?.Restart(); };
            _gameButtons.Add(restart);
        }

        public void Exit()
        {
            if (_conversion != null) { StopCoroutine(_conversion); _conversion = null; }
            _converting = false;
            DestroyGameImmediate();
            Exited?.Invoke();
        }

        private GameObject _cage;

        private void DestroyGameImmediate()
        {
            // An exit during the scan animation must not orphan the conversion cage.
            if (_cage != null) { Destroy(_cage); _cage = null; }
            if (_session != null)
            {
                _session.enabled = false;
                _session = null;
            }
            if (_activeRoot != null)
            {
                Destroy(_activeRoot);
                _activeRoot = null;
            }
            _built = null;
            _gameButtons.Clear();
        }

        private void Update()
        {
            if (Input == null) return;

            if (!InGame && !_converting) return;

            if (InGame)
            {
                if (Input.MenuDown && _session != null)
                {
                    _session.TogglePause();
                    AudioDirector.Instance?.Click();
                }
                if (Input.BackDown)
                {
                    Exit();
                    return;
                }
                UpdateGameButtons();
            }
        }

        private void UpdateGameButtons()
        {
            WorldButton best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < _gameButtons.Count; i++)
            {
                var b = _gameButtons[i];
                if (b == null) continue;
                b.SetHover(false);
                if (BestRayDistance(b, out float d) && d < bestDist)
                {
                    bestDist = d;
                    best = b;
                }
            }
            if (best != null) best.SetHover(true);
            if (best != null && Input.SelectDown && !_suppressSelectThisFrame)
            {
                best.Press();
                _suppressSelectThisFrame = true;
            }
            if (!Input.SelectDown) _suppressSelectThisFrame = false;
        }

        private bool BestRayDistance(WorldButton button, out float distance)
        {
            distance = float.MaxValue;
            bool hit = false;
            var left = Input.Left.PointerRay;
            var right = Input.Right.PointerRay;
            if (button.Raycast(left, out float dl)) { distance = dl; hit = true; }
            if (button.Raycast(right, out float dr) && dr < distance) { distance = dr; hit = true; }
            return hit;
        }

        public void EmergencyCleanup()
        {
            if (_conversion != null) StopCoroutine(_conversion);
            DestroyGameImmediate();
        }
    }
}



