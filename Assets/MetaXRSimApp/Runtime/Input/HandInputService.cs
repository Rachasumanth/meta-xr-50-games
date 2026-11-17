using System;
using System.Reflection;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Single input facade for the whole app. Publishes hands and controllers as
    /// HandState, drives proxy transforms consumed by the legacy game rigs, and
    /// offers a desktop fallback so the build is testable without a headset.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class HandInputService : MonoBehaviour
    {
        [Header("Runtime references (auto-resolved when empty)")]
        public OVRCameraRig Rig;
        public OVRHand LeftHand;
        public OVRHand RightHand;
        public DesktopHandSimulator Desktop;

        [Header("Behaviour")]
        public bool ForceDesktopInput = false;
        public bool UseSyntheticHands = false;
        public HandState SyntheticLeft { get; private set; }
        public HandState SyntheticRight { get; private set; }
        public bool ShowHandCursors = true;
        public float PinchDownThreshold = 0.72f;
        public float PinchUpThreshold = 0.44f;
        public float Smoothing = 26f;

        public HandState Left { get; private set; }
        public HandState Right { get; private set; }
        public bool SelectDown { get; private set; }
        public bool MenuDown { get; private set; }
        public bool BackDown { get; private set; }
        public int PresetGesture { get; private set; }
        public bool IsXRActive { get; private set; }
        public Camera HeadCamera { get; private set; }

        public Transform LeftPointer => _leftPointer;
        public Transform RightPointer => _rightPointer;
        public Transform LeftPalm => _leftPalm;
        public Transform RightPalm => _rightPalm;
        public Transform LeftIndexTip => _leftIndex;
        public Transform RightIndexTip => _rightIndex;
        public Transform LeftThumbTip => _leftThumb;
        public Transform RightThumbTip => _rightThumb;
        public Transform LeftWrist => _leftWrist;
        public Transform RightWrist => _rightWrist;
        public Transform[] LeftFingerTips => _leftFingers;
        public Transform[] RightFingerTips => _rightFingers;

        private Transform _leftPointer, _rightPointer;
        private Transform _leftPalm, _rightPalm;
        private Transform _leftIndex, _rightIndex;
        private Transform _leftThumb, _rightThumb;
        private Transform _leftWrist, _rightWrist;
        private Transform[] _leftFingers = new Transform[5];
        private Transform[] _rightFingers = new Transform[5];

        private readonly HandHistory _leftHistory = new HandHistory();
        private readonly HandHistory _rightHistory = new HandHistory();
        private Vector3 _leftSmoothPos, _rightSmoothPos;
        private Quaternion _leftSmoothRot = Quaternion.identity, _rightSmoothRot = Quaternion.identity;
        private bool _leftSmoothInit, _rightSmoothInit;

        private OVRInput.Controller _leftController = OVRInput.Controller.LTouch;
        private OVRInput.Controller _rightController = OVRInput.Controller.RTouch;
        private bool _initialized;
        private GameObject _cursorRoot;

        private void Awake()
        {
            Application.targetFrameRate = 72;
            QualitySettings.vSyncCount = 0;
        }

        public void Initialize(OVRCameraRig rig)
        {
            Rig = rig;
            _initialized = true;

            IsXRActive = rig != null && OVRManager.isHmdPresent;

            if (IsXRActive && !ForceDesktopInput)
            {
                HeadCamera = rig.centerEyeAnchor != null ? rig.centerEyeAnchor.GetComponent<Camera>() : Camera.main;
                EnsureHands();
            }
            else
            {
                EnsureDesktop();
            }

            BuildProxies();
        }

        private void Start()
        {
            if (!_initialized)
            {
                var rig = FindFirstObjectByType<OVRCameraRig>();
                Initialize(rig);
            }
        }

        private void EnsureDesktop()
        {
            if (Desktop == null) Desktop = FindFirstObjectByType<DesktopHandSimulator>();
            if (Desktop == null) Desktop = gameObject.AddComponent<DesktopHandSimulator>();

            if (HeadCamera == null) HeadCamera = Camera.main;
            if (HeadCamera == null)
            {
                var go = new GameObject("DesktopCamera");
                go.tag = "MainCamera";
                HeadCamera = go.AddComponent<Camera>();
                go.transform.position = new Vector3(0f, 1.45f, -0.55f);
                go.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1.08f, 0.35f) - go.transform.position);
            }
        }

        private void EnsureHands()
        {
            if (LeftHand == null) LeftHand = CreateHandTracker(HandSide.Left);
            if (RightHand == null) RightHand = CreateHandTracker(HandSide.Right);
        }

        private OVRHand CreateHandTracker(HandSide side)
        {
            var go = new GameObject(side + "HandTracker");
            go.transform.SetParent(transform, false);
            var hand = go.AddComponent<OVRHand>();

            // OVRHand.HandType is internal in the SDK. There is no public setter
            // and the building-block prefabs are editor-only assets, so assign
            // the field once at creation. This is the supported data path for
            // OVRPlugin hand state; no behaviour is faked.
            try
            {
                var field = typeof(OVRHand).GetField("HandType",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                    field.SetValue(hand, side == HandSide.Left ? OVRHand.Hand.HandLeft : OVRHand.Hand.HandRight);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[HandInputService] Unable to set OVRHand.HandType: " + e.Message);
            }

            try
            {
                if (Rig != null && Rig.trackingSpace != null)
                {
                    var pointer = hand.PointerPose;
                    if (pointer != null) pointer.SetParent(Rig.trackingSpace, false);
                }
            }
            catch (Exception)
            {
                // PointerPose is lazily created; the world-space fallback below
                // still works if parenting is unavailable.
            }

            return hand;
        }

        private void BuildProxies()
        {
            _cursorRoot = new GameObject("HandProxies");
            _cursorRoot.transform.SetParent(transform, false);

            _leftPointer = New("LeftPointer", _cursorRoot.transform);
            _rightPointer = New("RightPointer", _cursorRoot.transform);
            _leftPalm = New("LeftPalm", _cursorRoot.transform);
            _rightPalm = New("RightPalm", _cursorRoot.transform);
            _leftIndex = New("LeftIndexTip", _cursorRoot.transform);
            _rightIndex = New("RightIndexTip", _cursorRoot.transform);
            _leftThumb = New("LeftThumbTip", _cursorRoot.transform);
            _rightThumb = New("RightThumbTip", _cursorRoot.transform);
            _leftWrist = New("LeftWrist", _cursorRoot.transform);
            _rightWrist = New("RightWrist", _cursorRoot.transform);

            for (int i = 0; i < 5; i++)
            {
                _leftFingers[i] = New("LeftFinger" + i, _cursorRoot.transform);
                _rightFingers[i] = New("RightFinger" + i, _cursorRoot.transform);
            }

            if (ShowHandCursors) CreateCursorVisual(_leftPointer, new Color(1f, 0.55f, 0.85f));
            if (ShowHandCursors) CreateCursorVisual(_rightPointer, new Color(0.25f, 0.85f, 1f));
        }

        private static Transform New(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private void CreateCursorVisual(Transform target, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = target.name + "_Visual";
            sphere.transform.SetParent(target, false);
            sphere.transform.localScale = Vector3.one * 0.012f;
            var c = sphere.GetComponent<Collider>();
            if (c != null) Destroy(c);
            sphere.GetComponent<Renderer>().material = MaterialLibrary.Get(color, 0.1f, 0.8f, color * 0.35f);
        }

        private void Update()
        {
            float dt = Mathf.Max(1e-4f, Time.deltaTime);
            bool desktop = Desktop != null && (ForceDesktopInput || !IsXRActive);
            desktop = desktop || (Rig == null) || (!OVRManager.isHmdPresent);

            HandState left, right;
            if (UseSyntheticHands)
            {
                left = WithEdges(SyntheticLeft.IsTracked ? SyntheticLeft : Fallback(HandSide.Left, Left), Left);
                right = WithEdges(SyntheticRight.IsTracked ? SyntheticRight : Fallback(HandSide.Right, Right), Right);
            }
            else if (desktop)
            {
                if (Desktop == null) EnsureDesktop();
                left = Desktop.Left;
                right = Desktop.Right;
            }
            else
            {
                if (LeftHand == null || RightHand == null) EnsureHands();
                left = SampleOVR(HandSide.Left, LeftHand, _leftController, Left, dt);
                right = SampleOVR(HandSide.Right, RightHand, _rightController, Right, dt);
            }

            if (!left.IsTracked) left = Fallback(HandSide.Left, left);
            if (!right.IsTracked) right = Fallback(HandSide.Right, right);

            UpdateHistory(left, right, dt);
            if (_leftHistory.TryVelocity(Time.time, out var lv, out var la)) { left.Velocity = lv; left.AngularVelocityDeg = la; }
            if (_rightHistory.TryVelocity(Time.time, out var rv, out var ra)) { right.Velocity = rv; right.AngularVelocityDeg = ra; }

            Left = Smooth(left, ref _leftSmoothPos, ref _leftSmoothRot, ref _leftSmoothInit, dt);
            Right = Smooth(right, ref _rightSmoothPos, ref _rightSmoothRot, ref _rightSmoothInit, dt);

            UpdateProxies();
            UpdateButtons(desktop);
            UpdateDebugToggles();
        }

        private HandState SampleOVR(HandSide side, OVRHand hand, OVRInput.Controller controller, HandState previous, float dt)
        {
            bool handTracked = hand != null && hand.IsTracked && hand.IsDataValid;
            if (handTracked)
            {
                Transform pointer = hand.PointerPose;
                if (pointer != null)
                {
                    Vector3 pos = pointer.position;
                    Quaternion rot = pointer.rotation;
                    if (Rig != null && Rig.trackingSpace != null && pointer.parent != Rig.trackingSpace)
                    {
                        // Older SDKs keep the pointer pose at the scene root.
                        pos = Rig.trackingSpace.TransformPoint(pos);
                        rot = Rig.trackingSpace.rotation * rot;
                    }

                    float strength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                    if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index)) strength = Mathf.Max(strength, 1f);

                    bool pinching = previous.IsPinching ? strength > PinchUpThreshold : strength > PinchDownThreshold;
                    Vector3 fwd = rot * Vector3.forward;
                    return new HandState
                    {
                        IsTracked = true,
                        IsController = false,
                        IsPinching = pinching,
                        PinchStarted = pinching && !previous.IsPinching,
                        PinchEnded = !pinching && previous.IsPinching,
                        PinchStrength = strength,
                        HandScale = hand.HandScale <= 0f ? 1f : hand.HandScale,
                        PointerPosition = pos,
                        PointerForward = fwd,
                        PointerRotation = rot,
                        PalmPosition = pos - fwd * 0.045f,
                        PalmNormal = rot * Vector3.up,
                        PalmRotation = rot,
                        IndexTip = pos + fwd * 0.025f,
                        ThumbTip = pos + rot * new Vector3(0.022f, -0.012f, 0.012f),
                        Wrist = pos - fwd * 0.09f,
                        LastUpdateTime = Time.time
                    };
                }
            }

            // Controllers next.
            if (OVRInput.IsControllerConnected(controller))
            {
                Vector3 localPos = OVRInput.GetLocalControllerPosition(controller);
                Quaternion localRot = OVRInput.GetLocalControllerRotation(controller);
                Vector3 pos = localPos;
                Quaternion rot = localRot;
                if (Rig != null && Rig.trackingSpace != null)
                {
                    pos = Rig.trackingSpace.TransformPoint(localPos);
                    rot = Rig.trackingSpace.rotation * localRot;
                }

                float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
                float grip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
                bool pinching = previous.IsPinching ? (trigger > 0.30f || grip > 0.30f)
                                                     : (trigger > 0.62f || grip > 0.62f);
                Vector3 fwd = rot * Vector3.forward;
                return new HandState
                {
                    IsTracked = true,
                    IsController = true,
                    IsPinching = pinching,
                    PinchStarted = pinching && !previous.IsPinching,
                    PinchEnded = !pinching && previous.IsPinching,
                    PinchStrength = Mathf.Max(trigger, grip),
                    HandScale = 1f,
                    PointerPosition = pos,
                    PointerForward = fwd,
                    PointerRotation = rot,
                    PalmPosition = pos,
                    PalmNormal = rot * Vector3.up,
                    PalmRotation = rot,
                    IndexTip = pos + fwd * 0.022f,
                    ThumbTip = pos + rot * new Vector3(0.018f, -0.012f, 0.01f),
                    Wrist = pos - fwd * 0.07f,
                    LastUpdateTime = Time.time
                };
            }

            return Fallback(side, previous);
        }

        private HandState Fallback(HandSide side, HandState previous)
        {
            Vector3 origin = HeadCamera != null ? HeadCamera.transform.position : new Vector3(0f, 1.45f, 0f);
            Quaternion rot = HeadCamera != null ? HeadCamera.transform.rotation : Quaternion.identity;
            Vector3 sideOffset = (side == HandSide.Left ? -1f : 1f) * 0.16f * (rot * Vector3.right);
            Vector3 pos = origin + rot * (Vector3.forward * 0.45f + Vector3.down * 0.18f) + sideOffset;
            var state = HandState.Neutral(pos, rot);
            state.PinchStarted = false;
            state.PinchEnded = previous.IsPinching;
            return state;
        }

        public void SetSyntheticHand(HandSide side, Vector3 position, Quaternion rotation, bool pinching)
        {
            Vector3 fwd = rotation * Vector3.forward;
            var state = new HandState
            {
                IsTracked = true,
                IsController = false,
                IsPinching = pinching,
                PinchStrength = pinching ? 1f : 0f,
                HandScale = 1f,
                PointerPosition = position,
                PointerForward = fwd,
                PointerRotation = rotation,
                PalmPosition = position - fwd * 0.045f,
                PalmNormal = rotation * Vector3.up,
                PalmRotation = rotation,
                IndexTip = position + fwd * 0.025f,
                ThumbTip = position + rotation * new Vector3(0.022f, -0.012f, 0.012f),
                Wrist = position - fwd * 0.09f,
                LastUpdateTime = Time.time
            };
            if (side == HandSide.Left) SyntheticLeft = state;
            else SyntheticRight = state;
        }

        private static HandState WithEdges(HandState raw, HandState previous)
        {
            raw.PinchStarted = raw.IsPinching && !previous.IsPinching;
            raw.PinchEnded = !raw.IsPinching && previous.IsPinching;
            return raw;
        }

        private void UpdateHistory(HandState left, HandState right, float dt)
        {
            if (left.IsTracked) _leftHistory.Push(left.PointerPosition, left.PointerRotation);
            if (right.IsTracked) _rightHistory.Push(right.PointerPosition, right.PointerRotation);
        }

        private HandState Smooth(HandState raw, ref Vector3 pos, ref Quaternion rot, ref bool init, float dt)
        {
            if (!raw.IsTracked)
            {
                init = false;
                return raw;
            }

            float k = 1f - Mathf.Exp(-Smoothing * dt);
            if (!init)
            {
                pos = raw.PointerPosition;
                rot = raw.PointerRotation;
                init = true;
            }
            else
            {
                pos = Vector3.Lerp(pos, raw.PointerPosition, k);
                rot = Quaternion.Slerp(rot, raw.PointerRotation, k);
            }

            raw.PointerPosition = pos;
            raw.PointerRotation = rot;
            raw.PointerForward = rot * Vector3.forward;
            raw.PalmPosition = pos - raw.PointerForward * 0.045f;
            raw.PalmRotation = rot;
            raw.PalmNormal = rot * Vector3.up;
            raw.IndexTip = pos + raw.PointerForward * 0.025f;
            raw.ThumbTip = pos + rot * new Vector3(0.022f, -0.012f, 0.012f);
            raw.Wrist = pos - raw.PointerForward * 0.09f;
            return raw;
        }

        private void UpdateProxies()
        {
            Apply(_leftPointer, Left, Left.PointerPosition, Left.PointerRotation);
            Apply(_rightPointer, Right, Right.PointerPosition, Right.PointerRotation);
            Apply(_leftPalm, Left, Left.PalmPosition, Left.PalmRotation);
            Apply(_rightPalm, Right, Right.PalmPosition, Right.PalmRotation);
            Apply(_leftIndex, Left, Left.IndexTip, Left.PointerRotation);
            Apply(_rightIndex, Right, Right.IndexTip, Right.PointerRotation);
            Apply(_leftThumb, Left, Left.ThumbTip, Left.PointerRotation);
            Apply(_rightThumb, Right, Right.ThumbTip, Right.PointerRotation);
            Apply(_leftWrist, Left, Left.Wrist, Left.PointerRotation);
            Apply(_rightWrist, Right, Right.Wrist, Right.PointerRotation);

            float now = Time.time;
            for (int i = 0; i < 5; i++)
            {
                float spread = (i - 2) * 0.012f;
                Vector3 l = Left.IndexTip + Left.PointerRotation * new Vector3(spread, i == 0 ? 0.005f : 0f, 0f);
                Vector3 r = Right.IndexTip + Right.PointerRotation * new Vector3(spread, i == 0 ? 0.005f : 0f, 0f);
                Apply(_leftFingers[i], Left, l, Left.PointerRotation);
                Apply(_rightFingers[i], Right, r, Right.PointerRotation);
            }
        }

        private static void Apply(Transform t, HandState s, Vector3 pos, Quaternion rot)
        {
            if (t == null) return;
            t.position = pos;
            t.rotation = rot;
        }

        private void UpdateButtons(bool desktop)
        {
            bool select = false;
            bool menu = false;
            bool back = false;
            int preset = 0;

            if (desktop && Desktop != null)
            {
                select = Desktop.SelectDown;
                menu = Desktop.MenuDown;
                back = Desktop.BackDown;
                preset = Desktop.PresetGesture;
            }
            else
            {
                select = Left.PinchStarted || Right.PinchStarted ||
                         OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                         OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
                menu = OVRInput.GetDown(OVRInput.Button.Start);
                back = OVRInput.GetDown(OVRInput.Button.Two);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) select = true;
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab)) menu = true;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) back = true;
            if (Input.GetKeyDown(KeyCode.Alpha1)) preset = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2)) preset = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3)) preset = 3;
            if (Input.GetKeyDown(KeyCode.Alpha4)) preset = 4;
            if (Input.GetKeyDown(KeyCode.Alpha5)) preset = 5;

            SelectDown = select;
            MenuDown = menu;
            BackDown = back;
            PresetGesture = preset;

            if (Input.GetKeyDown(KeyCode.F12)) Screenshot.Capture("manual");
        }

        private void UpdateDebugToggles()
        {
            if (Input.GetKeyDown(KeyCode.F1) && !ForceDesktopInput && IsXRActive)
            {
                ForceDesktopInput = true;
                EnsureDesktop();
                Debug.Log("[HandInputService] Input backend: desktop simulation");
            }
            else if (Input.GetKeyDown(KeyCode.F1) && ForceDesktopInput)
            {
                ForceDesktopInput = false;
                Debug.Log("[HandInputService] Input backend: XR (or fallback)");
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                ShowHandCursors = !ShowHandCursors;
                SetCursorVisuals(ShowHandCursors);
            }
        }

        private void SetCursorVisuals(bool visible)
        {
            if (_leftPointer != null)
            {
                var v = _leftPointer.Find(_leftPointer.name + "_Visual");
                if (v != null) v.gameObject.SetActive(visible);
            }
            if (_rightPointer != null)
            {
                var v = _rightPointer.Find(_rightPointer.name + "_Visual");
                if (v != null) v.gameObject.SetActive(visible);
            }
        }

        public void Haptic(HandSide side, float amplitude, float seconds)
        {
            if (!IsXRActive) return;
            var controller = side == HandSide.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            if (OVRInput.IsControllerConnected(controller))
                OVRInput.SetControllerVibration(Mathf.Clamp01(amplitude), Mathf.Clamp01(amplitude), controller);
        }

        public HandState GetHand(HandSide side)
        {
            return side == HandSide.Left ? Left : Right;
        }

        public Ray PreferredRay()
        {
            if (Right.IsTracked && Right.PinchStrength >= 0f) return Right.PointerRay;
            if (Left.IsTracked) return Left.PointerRay;
            return new Ray(HeadCamera != null ? HeadCamera.transform.position : Vector3.zero,
                           HeadCamera != null ? HeadCamera.transform.forward : Vector3.forward);
        }
    }
}

