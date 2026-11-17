using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Keyboard + mouse hand source used when no XR runtime is present (editor
    /// play without link, CI playmode tests, or a desktop preview build).
    /// It publishes the exact same HandState struct as OVRHand, which lets the
    /// whole app be exercised without a headset.
    /// </summary>
    [DefaultExecutionOrder(-250)]
    public sealed class DesktopHandSimulator : MonoBehaviour
    {
        public bool EnableMouseLook = true;
        public float HandDistance = 0.55f;
        public float HandStiffness = 18f;

        public HandState Left { get; private set; }
        public HandState Right { get; private set; }
        public bool SelectDown { get; private set; }
        public bool MenuDown { get; private set; }
        public bool BackDown { get; private set; }
        public int PresetGesture { get; private set; }

        private Camera _camera;
        private Vector3 _rightTarget;
        private Vector3 _leftTarget;
        private bool _prevRightPinch;
        private bool _prevLeftPinch;
        private Vector3 _camYawPitch;

        private Transform _rightVisual;
        private Transform _leftVisual;

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var go = new GameObject("DesktopCamera");
                _camera = go.AddComponent<Camera>();
                go.tag = "MainCamera";
                go.transform.position = new Vector3(0f, 1.45f, -0.55f);
                go.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1.08f, 0.35f) - go.transform.position);
            }

            _camYawPitch = _camera.transform.eulerAngles;
            _rightTarget = _camera.transform.position + _camera.transform.forward * HandDistance;
            _leftTarget = _rightTarget;

            _rightVisual = CreateCursor("DesktopRightHand", new Color(0.25f, 0.85f, 1f));
            _leftVisual = CreateCursor("DesktopLeftHand", new Color(1f, 0.55f, 0.85f));
        }

        private Transform CreateCursor(string name, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = Vector3.one * 0.018f;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var r = go.GetComponent<Renderer>();
            r.material = MaterialLibrary.Get(color, 0.1f, 0.7f, color * 0.4f);
            return go.transform;
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            HandleLook();
            HandleHands();
            HandleButtons();
        }

        private void HandleLook()
        {
            if (!EnableMouseLook) return;
            if (Input.GetMouseButton(2))
            {
                _camYawPitch.x -= Input.GetAxis("Mouse Y") * 2f;
                _camYawPitch.y += Input.GetAxis("Mouse X") * 2f;
                _camera.transform.rotation = Quaternion.Euler(_camYawPitch.x, _camYawPitch.y, 0f);
            }

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += _camera.transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= _camera.transform.forward;
            if (Input.GetKey(KeyCode.D)) move += _camera.transform.right;
            if (Input.GetKey(KeyCode.A)) move -= _camera.transform.right;
            if (Input.GetKey(KeyCode.E)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;
            if (move.sqrMagnitude > 0f)
                _camera.transform.position += move.normalized * (0.5f * Time.deltaTime);
        }

        private void HandleHands()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            Vector3 rightPos = ray.origin + ray.direction * HandDistance;
            Vector3 leftPos = _camera.transform.position + _camera.transform.forward * HandDistance
                              - _camera.transform.right * 0.16f - _camera.transform.up * 0.08f;

            if (shift)
            {
                // Shift lets the left hand trace too, useful for two-hand gestures.
                leftPos = rightPos + _camera.transform.right * 0.12f;
            }

            _rightTarget = Vector3.Lerp(_rightTarget, rightPos, 1f - Mathf.Exp(-HandStiffness * Time.deltaTime));
            _leftTarget = Vector3.Lerp(_leftTarget, leftPos, 1f - Mathf.Exp(-HandStiffness * Time.deltaTime));

            bool rightPinch = Input.GetMouseButton(0);
            bool leftPinch = Input.GetMouseButton(1);

            Quaternion rightRot = Quaternion.LookRotation((_rightTarget - _camera.transform.position).normalized, _camera.transform.up);
            Quaternion leftRot = Quaternion.LookRotation((_leftTarget - _camera.transform.position).normalized, _camera.transform.up);

            var prevRight = Right;
            var prevLeft = Left;

            Right = MakeState(_rightTarget, rightRot, rightPinch, prevRight);
            Left = MakeState(_leftTarget, leftRot, leftPinch, prevLeft);

            if (_rightVisual != null)
            {
                _rightVisual.position = Right.PointerPosition;
                _rightVisual.rotation = Right.PointerRotation;
                _rightVisual.localScale = Vector3.one * (Right.IsPinching ? 0.012f : 0.018f);
            }
            if (_leftVisual != null)
            {
                _leftVisual.position = Left.PointerPosition;
                _leftVisual.rotation = Left.PointerRotation;
                _leftVisual.localScale = Vector3.one * (Left.IsPinching ? 0.012f : 0.018f);
            }
        }

        private HandState MakeState(Vector3 pos, Quaternion rot, bool pinching, HandState previous)
        {
            Vector3 fwd = rot * Vector3.forward;
            var state = new HandState
            {
                IsTracked = true,
                IsController = false,
                IsPinching = pinching,
                PinchStarted = pinching && !previous.IsPinching,
                PinchEnded = !pinching && previous.IsPinching,
                PinchStrength = pinching ? 1f : 0f,
                HandScale = 1f,
                PointerPosition = pos,
                PointerForward = fwd,
                PointerRotation = rot,
                PalmPosition = pos - fwd * 0.04f,
                PalmNormal = rot * Vector3.up,
                PalmRotation = rot,
                IndexTip = pos + fwd * 0.02f,
                ThumbTip = pos + rot * new Vector3(0.02f, 0f, 0.01f),
                Wrist = pos - fwd * 0.10f,
                LastUpdateTime = Time.time
            };
            return state;
        }

        private void HandleButtons()
        {
            SelectDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                         Right.PinchStarted || Left.PinchStarted;
            MenuDown = Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab);
            BackDown = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace);

            PresetGesture = 0;
            if (Input.GetKeyDown(KeyCode.Alpha1)) PresetGesture = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2)) PresetGesture = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3)) PresetGesture = 3;
            if (Input.GetKeyDown(KeyCode.Alpha4)) PresetGesture = 4;
            if (Input.GetKeyDown(KeyCode.Alpha5)) PresetGesture = 5;
        }

        /// <summary>Used by automated playmode tests to force a hand state.</summary>
        public void ForceStates(HandState left, HandState right)
        {
            Left = left;
            Right = right;
        }
    }
}
