using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// The single application scene: shape gesture gate, real-object conversion,
    /// 50-game browser and mini-game host. Everything the player touches is
    /// authored from this component at runtime, which keeps the shipped scene
    /// small and makes the QA self-test deterministic.
    /// </summary>
    [DefaultExecutionOrder(0)]
    public sealed class ShapeForgeHub : MonoBehaviour
    {
        public HandInputService Input;
        public RealObjectProbe Probe;
        public MiniGameHost Host;
        public ForgeMenu Menu;

        public ShapeSignature PreviewShape { get; private set; }
        public RealObjectProfile PreviewObject { get; private set; }
        public GameMatch PreviewMatch { get; private set; }
        public bool IsDrawing { get; private set; }
        public bool HasPreview => PreviewShape.IsUsable;
        public Transform ForgeOrb { get; private set; }
        public bool InputBlocked { get; set; }

        private readonly ShapeGestureRecognizer _recognizer = new ShapeGestureRecognizer();
        private readonly GameMatchmaker _matchmaker = new GameMatchmaker();
        private LineRenderer _trail;
        private LineRenderer _trailGlow;
        private GameObject _previewVisual;
        private GameObject _convertRoot;
        private WorldButton _convertButton;
        private WorldButton _cancelButton;
        private HandSide _drawHand;
        private float _lastPreviewRefresh;
        private bool _selectLatch;
        private int _seed;
        private float _previewPulse;
        private Vector3 _orbLocal = new Vector3(0f, 0.065f, -0.16f);
        private bool _converting;
        private bool _placed;
        private float _bootTime;
        private float _objectStatusTimer;

        public void Initialize(HandInputService input, RealObjectProbe probe, MiniGameHost host)
        {
            Input = input;
            Probe = probe;
            Host = host;
            _seed = Environment.TickCount;
            _bootTime = Time.time;

            PlaceInFrontOfPlayer();
            _placed = Input != null && Input.HeadCamera != null && Input.HeadCamera.transform.position.y > 0.4f;

            // The menu lives on its own child so hiding it never disables the
            // hub root (which is the parent/PlayArea of the active mini-game).
            var menuGo = new GameObject("ForgeMenuUI");
            menuGo.transform.SetParent(transform, false);
            Menu = menuGo.AddComponent<ForgeMenu>();
            Menu.Build(this);

            ForgeOrb = Menu.Orb != null ? Menu.Orb : transform;
            BuildTrail();
            BuildPreviewButtons();

            if (Host != null)
            {
                Host.Initialize(input, transform, transform);
                Host.Exited += OnHostExited;
                Host.Launched += OnHostLaunched;
            }

            Menu.SetHeader("SHAPEFORGE  -  50 SPATIAL GAMES  -  READY");
            Menu.SetHint("Pinch and trace a circle / oval / arc in the air. Release to identify, pinch CONVERT to launch.");
            Menu.SetShapeStatus("Shape: waiting for a gesture", new Color(0.6f, 1f, 0.9f));
            Menu.SetObjectStatus("Real object: point at a scene anchor to convert it", new Color(0.7f, 0.85f, 1f));
            Menu.SetCandidate("", false);
            Menu.SetQuickStatus("Controller users: use QUICK SHAPES");
        }

        private void PlaceInFrontOfPlayer()
        {
            Transform head = Input != null && Input.HeadCamera != null ? Input.HeadCamera.transform : null;
            Vector3 forward = head != null ? head.forward : Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            Vector3 eye = head != null ? head.position : new Vector3(0f, 1.45f, 0f);
            transform.position = eye + forward * 0.62f + Vector3.down * 0.34f;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        public void Recenter()
        {
            PlaceInFrontOfPlayer();
            CancelPreview();
            Menu.SetHint("Rec entered in front of you. Draw a shape around the Forge Orb.");
        }

        private void BuildTrail()
        {
            var go = new GameObject("GestureTrail");
            go.transform.SetParent(transform, false);
            _trail = go.AddComponent<LineRenderer>();
            _trail.useWorldSpace = true;
            _trail.positionCount = 0;
            _trail.startWidth = 0.0055f;
            _trail.endWidth = 0.002f;
            _trail.material = MaterialLibrary.Sprite(new Color(0.35f, 0.95f, 1f));
            _trail.startColor = new Color(0.35f, 0.95f, 1f);
            _trail.endColor = new Color(0.7f, 0.3f, 1f);
            _trail.numCapVertices = 4;
            _trail.textureMode = LineTextureMode.Stretch;

            var glowGo = new GameObject("GestureTrailGlow");
            glowGo.transform.SetParent(go.transform, false);
            _trailGlow = glowGo.AddComponent<LineRenderer>();
            _trailGlow.useWorldSpace = true;
            _trailGlow.positionCount = 0;
            _trailGlow.startWidth = 0.015f;
            _trailGlow.endWidth = 0.006f;
            _trailGlow.material = MaterialLibrary.Sprite(new Color(0.28f, 0.72f, 1f, 0.22f));
            _trailGlow.startColor = new Color(0.25f, 0.8f, 1f, 0.24f);
            _trailGlow.endColor = new Color(0.6f, 0.25f, 1f, 0.03f);
            _trailGlow.numCapVertices = 4;

            go.SetActive(false);
        }

        private void BuildPreviewButtons()
        {
            _convertRoot = new GameObject("PreviewActions");
            _convertRoot.transform.SetParent(transform, false);

            _convertButton = WorldButton.Create("Convert", _convertRoot.transform, "CONVERT",
                new Vector3(-0.075f, 0.165f, -0.02f), Quaternion.identity,
                new Vector2(0.15f, 0.042f), new Color32(36, 86, 72, 255),
                new Color32(96, 255, 180, 255), 0.012f);
            _convertButton.Clicked = ConfirmConversion;

            _cancelButton = WorldButton.Create("Cancel", _convertRoot.transform, "CANCEL",
                new Vector3(0.105f, 0.165f, -0.02f), Quaternion.identity,
                new Vector2(0.10f, 0.038f), new Color32(70, 46, 52, 255),
                new Color32(255, 128, 116, 255), 0.010f);
            _cancelButton.Clicked = CancelPreview;

            _convertRoot.SetActive(false);
        }

        private void Update()
        {
            if (Input == null) return;

            if (!_placed)
            {
                bool headValid = Input.HeadCamera != null && Input.HeadCamera.transform.position.y > 0.4f;
                if (headValid || Time.time - _bootTime > 1.5f)
                {
                    PlaceInFrontOfPlayer();
                    _placed = true;
                }
            }

            if (Host != null && (Host.InGame || Host.IsConverting)) return;

            if (InputBlocked) return;

            if (_converting) return;

            // Real object candidacy.
            Vector3 orbWorld = GetOrbWorld();
            if (Probe != null) Probe.Refresh(orbWorld);
            PreviewObject = Probe != null ? Probe.Current : RealObjectProfile.None(orbWorld);
            if (Time.time - _objectStatusTimer > 0.2f)
            {
                _objectStatusTimer = Time.time;
                UpdateObjectStatus();
            }

            if (HasPreview)
            {
                UpdatePreview();
                UpdatePreviewButtons();
                if (Input.BackDown)
                {
                    CancelPreview();
                    return;
                }
            }

            if (IsDrawing)
            {
                ContinueDrawing();
            }
            else if (!HasPreview && Input.SelectDown && !Menu.ConsumedSelect && !_selectLatch)
            {
                _selectLatch = true;
                TryStartDrawing();
            }

            if (!Input.SelectDown) _selectLatch = false;

            if (Input.MenuDown)
            {
                Recenter();
            }
        }

        private void UpdateObjectStatus()
        {
            if (Menu == null) return;
            if (PreviewObject.IsValid)
            {
                Menu.SetObjectStatus("Real object: " + PreviewObject.Describe(), new Color(0.55f, 1f, 0.85f));
            }
            else
            {
                Menu.SetObjectStatus("Real object: none detected (air shape only)", new Color(0.65f, 0.75f, 0.85f));
            }
        }

        private void TryStartDrawing()
        {
            Vector3 orb = GetOrbWorld();
            HandSide side = Input.Right.PinchStarted ? HandSide.Right :
                            Input.Left.PinchStarted ? HandSide.Left :
                            Input.Right.IsPinching ? HandSide.Right : HandSide.Left;

            var hand = Input.GetHand(side);
            float distance = Vector3.Distance(hand.IndexTip, orb);
            if (distance > 0.85f)
            {
                Menu.SetHint("Start your pinch near the Forge Orb (within arm's reach).");
                Menu.SetShapeStatus("Shape: outside the gesture volume", new Color(1f, 0.75f, 0.45f));
                AudioDirector.Instance?.Error();
                return;
            }

            _drawHand = side;
            IsDrawing = true;
            _recognizer.Begin();
            _recognizer.AddSample(hand.IndexTip, Time.time);
            _trail.gameObject.SetActive(true);
            _trail.positionCount = 1;
            _trail.SetPosition(0, hand.IndexTip);
            if (_trailGlow != null)
            {
                _trailGlow.positionCount = 1;
                _trailGlow.SetPosition(0, hand.IndexTip);
            }
            CancelPreviewVisualOnly();
            Menu.ShowFamily(null);
            Menu.SetShapeStatus("Shape: tracing...", new Color(0.4f, 0.95f, 1f));
            Menu.SetHint("Trace the outline in the air. Release your pinch when the shape is complete.");
            AudioDirector.Instance?.Click();
        }

        private void ContinueDrawing()
        {
            var hand = Input.GetHand(_drawHand);
            float now = Time.time;

            if (hand.IsTracked && _recognizer.AddSample(hand.IndexTip, now))
            {
                int count = _recognizer.PointCount;
                _trail.positionCount = count;
                _trail.SetPosition(count - 1, hand.IndexTip);
                if (_trailGlow != null)
                {
                    _trailGlow.positionCount = count;
                    _trailGlow.SetPosition(count - 1, hand.IndexTip);
                }

                float pulse = 0.85f + 0.15f * Mathf.Sin(now * 9f);
                _trail.startWidth = 0.0055f * pulse;
                Color a = Color.Lerp(new Color(0.25f, 0.95f, 1f), new Color(0.75f, 0.35f, 1f),
                    0.5f + 0.5f * Mathf.Sin(now * 2.2f));
                _trail.startColor = a;
                if (_trailGlow != null)
                {
                    _trailGlow.startColor = new Color(a.r, a.g, a.b, 0.22f);
                    _trailGlow.startWidth = 0.015f * pulse;
                }
            }

            if (!hand.IsPinching || !hand.IsTracked)
            {
                FinishDrawing();
                return;
            }

            var preview = _recognizer.Peek(GetOrbWorld());
            if (preview.IsUsable)
            {
                Menu.SetShapeStatus("Shape: " + preview.ShortSummary(), new Color(0.55f, 1f, 0.85f));
            }
        }

        private void FinishDrawing()
        {
            IsDrawing = false;
            _trail.gameObject.SetActive(false);
            var sig = _recognizer.Complete(GetOrbWorld());
            if (!sig.IsUsable)
            {
                Menu.SetShapeStatus("Shape: not recognisable - try a cleaner outline", new Color(1f, 0.65f, 0.5f));
                Menu.SetHint("Circle = sphere, tall oval = prolate, wide oval = oblate, arc = dome/bowl.");
                AudioDirector.Instance?.Error();
                return;
            }

            ShowPreview(sig);
        }

        public void SimulatePreset(int preset)
        {
            if (Input == null || _converting) return;
            if (Host != null && Host.InGame) return;

            Vector3 center = GetOrbWorld();
            Vector3 right = Input.HeadCamera != null ? Input.HeadCamera.transform.right : Vector3.right;
            Vector3 up = Input.HeadCamera != null ? Input.HeadCamera.transform.up : Vector3.up;
            right.Normalize();
            up.Normalize();

            _recognizer.Begin();
            int steps = 70;
            float radius = 0.05f;
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)(steps - 1);
                Vector3 p;
                switch (preset)
                {
                    case 2:
                        p = center + right * (Mathf.Cos(t * Mathf.PI * 2f) * radius * 0.62f)
                                   + up * (Mathf.Sin(t * Mathf.PI * 2f) * radius * 1.25f);
                        break;
                    case 3:
                        p = center + right * (Mathf.Cos(t * Mathf.PI * 2f) * radius * 1.35f)
                                   + up * (Mathf.Sin(t * Mathf.PI * 2f) * radius * 0.62f);
                        break;
                    case 4:
                        p = center + right * (Mathf.Cos(Mathf.PI - t * Mathf.PI) * radius * 1.15f)
                                   + up * (Mathf.Sin(Mathf.PI - t * Mathf.PI) * radius * 1.15f);
                        break;
                    case 5:
                        p = center + right * (Mathf.Cos(t * Mathf.PI) * radius * 1.15f)
                                   - up * (Mathf.Sin(t * Mathf.PI) * radius * 1.15f);
                        break;
                    default:
                        p = center + right * (Mathf.Cos(t * Mathf.PI * 2f) * radius)
                                   + up * (Mathf.Sin(t * Mathf.PI * 2f) * radius);
                        break;
                }
                _recognizer.AddSample(p, 0f + i * 0.012f);
            }
            var sig = _recognizer.Complete(center);
            if (!sig.IsUsable)
            {
                Menu.SetQuickStatus("Preset unrecognised");
                return;
            }
            ShowPreview(sig);
            Menu.SetQuickStatus("Preset: " + sig.Kind.ToDisplayName());
        }

        private void ShowPreview(ShapeSignature sig)
        {
            PreviewShape = sig;
            CancelPreviewVisualOnly();
            _previewVisual = BuildPreviewVisual(sig);
            _previewPulse = 0f;
            Menu.SetShapeStatus("Shape: " + sig.ShortSummary(), new Color(0.55f, 1f, 0.85f));
            UpdateCandidate(true);
            _convertRoot.SetActive(true);
            AudioDirector.Instance?.Chime();
            Input.Haptic(HandSide.Right, 0.3f, 0.05f);

            Color accent = PreviewMatch.Game != null ? PreviewMatch.Game.Accent : new Color(0.35f, 1f, 0.9f);
            SparkleBurst.Play(transform, sig.CenterWorld, accent, 16, 0.22f, 0.6f, 0.005f);
        }

        private void UpdatePreview()
        {
            if (_previewVisual != null)
            {
                _previewPulse += Time.deltaTime;
                float s = 1f + 0.035f * Mathf.Sin(_previewPulse * 6f);
                _previewVisual.transform.localScale = Vector3.one * s;
                _previewVisual.transform.Rotate(Vector3.up, 18f * Time.deltaTime, Space.World);
            }

            if (Time.time - _lastPreviewRefresh > 0.25f)
            {
                _lastPreviewRefresh = Time.time;
                UpdateCandidate(false);
            }
        }

        private void UpdateCandidate(bool announce)
        {
            if (!HasPreview) return;
            PreviewMatch = _matchmaker.Match(PreviewShape, PreviewObject, _seed);
            if (!PreviewMatch.HasGame)
            {
                Menu.SetCandidate("No family matched - refine the gesture", false);
                return;
            }

            string text = "CONVERT TO: " + PreviewMatch.Game.Title + "  (" + GameCatalog.FamilyTitle(PreviewMatch.ResolvedFamily) + ")";
            Menu.SetCandidate(text, true);
            if (announce)
            {
                Menu.SetHint(PreviewMatch.Reason + "  -  pinch CONVERT or press the button");
            }
        }

        private GameObject BuildPreviewVisual(ShapeSignature sig)
        {
            var go = new GameObject("ShapePreview");
            go.transform.SetParent(transform, false);
            go.transform.position = sig.CenterWorld;

            Color accent = new Color(0.3f, 0.95f, 1f);
            float s = Mathf.Clamp(sig.DiameterMeters, 0.02f, 0.5f);
            if (PreviewMatch.Game != null) accent = PreviewMatch.Game.Accent;

            switch (sig.Kind)
            {
                case ShapeKind.TrueSphere:
                    SpherePreview(go.transform, s, new Vector3(1f, 1f, 1f), accent);
                    break;
                case ShapeKind.ProlateSpheroid:
                    SpherePreview(go.transform, s, new Vector3(0.72f, 1.15f, 0.72f), accent);
                    break;
                case ShapeKind.OblateSpheroid:
                    SpherePreview(go.transform, s, new Vector3(1.15f, 0.42f, 0.92f), accent);
                    break;
                case ShapeKind.Hemisphere:
                    MeshPreview(go.transform, ProceduralMesh.Dome, s, accent, false, 0.55f);
                    break;
                case ShapeKind.DomedBowl:
                    MeshPreview(go.transform, ProceduralMesh.Bowl, s, accent, false, 0.55f);
                    break;
            }

            var wire = new GameObject("PreviewWire");
            wire.transform.SetParent(go.transform, false);
            wire.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
            wire.AddComponent<MeshRenderer>().material = MaterialLibrary.Glow(accent, 2f);
            wire.transform.localScale = new Vector3(s * 1.18f, 1f, s * 1.18f);
            wire.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);

            var label = WorldLabel.Create("PreviewLabel", go.transform,
                sig.Kind.ToDisplayName() + "  " + Mathf.RoundToInt(sig.DiameterMeters * 1000f) + " mm",
                Mathf.Clamp(s * 0.18f, 0.01f, 0.035f), accent,
                new Vector3(0f, s * 0.75f, 0f));
            label.SetColor(accent);
            return go;
        }

        private static void SpherePreview(Transform parent, float diameter, Vector3 aspect, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PreviewSphere";
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.Scale(aspect, Vector3.one * diameter);
            var c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            go.GetComponent<Renderer>().material = MaterialLibrary.Transparent(color, 0.28f);
        }

        private static void MeshPreview(Transform parent, Mesh mesh, float diameter, Color color, bool closed, float yScale)
        {
            var go = new GameObject("Shell");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(color, 0.25f);
            go.transform.localScale = new Vector3(diameter, diameter * yScale, diameter);
            if (closed) go.transform.localPosition = new Vector3(0f, -diameter * yScale * 0.45f, 0f);
        }

        private void UpdatePreviewButtons()
        {
            bool hover = false;
            if (_convertButton != null && _convertRoot.activeSelf)
            {
                bool h = RayHover(_convertButton);
                _convertButton.SetHover(h);
                hover |= h;
                if (h && Input.SelectDown && !Menu.ConsumedSelect && !_selectLatch && !_converting)
                {
                    _selectLatch = true;
                    ConfirmConversion();
                    return;
                }
            }
            if (_cancelButton != null && _convertRoot.activeSelf)
            {
                bool h = RayHover(_cancelButton);
                _cancelButton.SetHover(h);
                hover |= h;
                if (h && Input.SelectDown && !Menu.ConsumedSelect && !_selectLatch && !_converting)
                {
                    _selectLatch = true;
                    CancelPreview();
                    return;
                }
            }

            if (!hover && Input.SelectDown && !Menu.ConsumedSelect && !_selectLatch && !_converting && HasPreview)
            {
                _selectLatch = true;
                ConfirmConversion();
            }
        }

        private bool RayHover(WorldButton b)
        {
            if (b == null) return false;
            if (b.Raycast(Input.Left.PointerRay, out _)) return true;
            if (b.Raycast(Input.Right.PointerRay, out _)) return true;
            return false;
        }

        public void ConfirmConversion()
        {
            if (!HasPreview || _converting) return;
            if (!PreviewMatch.HasGame)
            {
                UpdateCandidate(true);
                if (!PreviewMatch.HasGame) return;
            }

            _converting = true;
            _convertRoot.SetActive(false);
            Menu.SetVisible(false);
            AudioDirector.Instance?.Whoosh();
            SparkleBurst.Play(transform, GetOrbWorld(),
                PreviewMatch.Game != null ? PreviewMatch.Game.Accent : new Color(0.4f, 1f, 0.9f),
                22, 0.26f, 0.75f, 0.006f);

            Vector3 fallback = transform.TransformPoint(new Vector3(0f, 0f, -0.12f));
            Quaternion fallbackRot = transform.rotation;
            Host.Launch(PreviewMatch.Game, PreviewShape, PreviewObject, fallback, fallbackRot);
            // Conversion lifetime is owned by MiniGameHost.IsConverting.
        }


        public void LaunchGame(GameDefinition game)
        {
            if (game == null || _converting) return;
            if (Host != null && Host.InGame) return;

            ShapeSignature sig = HasPreview && PreviewMatch.HasGame && PreviewMatch.Game == game
                ? PreviewShape
                : DefaultSignatureFor(game);

            var obj = Probe != null ? Probe.Current : RealObjectProfile.None(GetOrbWorld());
            _converting = true;
            _convertRoot.SetActive(false);
            Menu.ShowFamily(null);
            Menu.SetVisible(false);
            AudioDirector.Instance?.Whoosh();
            SparkleBurst.Play(transform, GetOrbWorld(), game.Accent, 22, 0.26f, 0.75f, 0.006f);

            Vector3 fallback = transform.TransformPoint(new Vector3(0f, 0f, -0.12f));
            Host.Launch(game, sig, obj, fallback, transform.rotation);
            // Conversion lifetime is owned by MiniGameHost.IsConverting.
        }

        public ShapeSignature DefaultSignatureFor(GameDefinition game)
        {
            float size = game.NominalSizeMeters;
            Vector3 c = GetOrbWorld();
            var sig = ShapeSignature.Unknown(c);
            sig.Kind = game.Shape;
            sig.DiameterMeters = size;
            sig.SizeMeters = Vector3.one * size;
            sig.Confidence = 0.75f;
            sig.PointCount = 64;
            sig.AspectRatio = game.Shape == ShapeKind.ProlateSpheroid ? 1.5f :
                              game.Shape == ShapeKind.OblateSpheroid ? 1.6f : 1f;
            sig.Closure = 1f;
            sig.SweepDegrees = 360f;
            sig.PrincipalAxisWorld = Vector3.up;
            sig.Basis = Quaternion.identity;
            return sig;
        }

        public void CancelPreview()
        {
            CancelPreviewVisualOnly();
            PreviewShape = ShapeSignature.Unknown(GetOrbWorld());
            PreviewMatch = default;
            if (_convertRoot != null) _convertRoot.SetActive(false);
            Menu.SetCandidate("", false);
            Menu.SetShapeStatus("Shape: waiting for a gesture", new Color(0.6f, 1f, 0.9f));
            Menu.SetHint("Pinch and trace a circle / oval / arc in the air around the Forge Orb.");
        }

        private void CancelPreviewVisualOnly()
        {
            if (_previewVisual != null)
            {
                Destroy(_previewVisual);
                _previewVisual = null;
            }
        }

        private Vector3 GetOrbWorld()
        {
            if (ForgeOrb != null) return ForgeOrb.position;
            return transform.TransformPoint(_orbLocal);
        }

        private void OnHostExited()
        {
            _converting = false;
            Menu.SetVisible(true);
            Menu.RefreshAudioLabels();
            MusicDirector.Instance?.PlayHub(1.1f);
            Menu.SetHeader("SHAPEFORGE  -  50 SPATIAL GAMES  -  READY");
            Menu.SetHint("Pinch and trace a new shape, or pick a family to browse all 50 games.");
            Menu.SetQuickStatus("Session ended. Draw again.");
            CancelPreview();
        }

        private void OnHostLaunched(LaunchReport report)
        {
            if (report != null && report.Success) return;
            _converting = false;
            Menu.SetVisible(true);
            Menu.RefreshAudioLabels();
            MusicDirector.Instance?.PlayHub(0.9f);
            Menu.SetHeader("SHAPEFORGE  -  LAUNCH FAILED");
            Menu.SetHint("The board could not attach its rig. Check the console for details.");
            Menu.SetQuickStatus(report != null ? report.Error : "Unknown launch error");
            AudioDirector.Instance?.Error();
        }

        private void OnDestroy()
        {
            if (Host != null)
            {
                Host.Exited -= OnHostExited;
                Host.Launched -= OnHostLaunched;
            }
        }
    }
}




