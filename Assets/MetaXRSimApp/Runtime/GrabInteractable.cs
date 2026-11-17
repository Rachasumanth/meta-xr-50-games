using System;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Physical hand/controller grab used by every mini-game. The rigs see the
    /// resulting transform motion, so their existing core simulations keep
    /// running; no game-specific input code is duplicated.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class GrabInteractable : MonoBehaviour
    {
        public HandInputService Input;
        public Rigidbody Body;
        public float GrabRadiusMeters = 0.14f;
        public float ThrowScale = 0.85f;
        public float MaxThrowSpeed = 5.5f;
        public bool CanGrab = true;

        public bool IsHeld => _heldBy.HasValue;
        public bool HasEverGrabbed { get; private set; }
        public HandSide HeldBy => _heldBy ?? HandSide.Right;
        public event Action Grabbed;
        public event Action Released;

        private HandSide? _heldBy;
        private Vector3 _localOffset;
        private Quaternion _grabRotation;
        private Quaternion _lastHandRotation;
        private Vector3 _lastPosition;
        private Vector3 _smoothVelocity;
        private float _grabStartTime;
        private Transform _home;
        private Vector3 _homePosition;
        private Quaternion _homeRotation;

        public void Setup(HandInputService input, Rigidbody body)
        {
            Input = input;
            Body = body;
            _home = transform;
            _homePosition = transform.position;
            _homeRotation = transform.rotation;
            if (Body != null)
            {
                Body.useGravity = true;
                Body.linearDamping = 0.15f;
                Body.angularDamping = 0.2f;
            }
        }

        public void SetHome(Vector3 position, Quaternion rotation)
        {
            _homePosition = position;
            _homeRotation = rotation;
        }

        public void ReturnHome()
        {
            if (Body != null)
            {
                Body.isKinematic = false;
                Body.useGravity = true;
                Body.linearVelocity = Vector3.zero;
                Body.angularVelocity = Vector3.zero;
            }
            transform.position = _homePosition;
            transform.rotation = _homeRotation;
        }

        private void Update()
        {
            if (Input == null || Body == null) return;

            if (_heldBy.HasValue)
            {
                var hand = Input.GetHand(_heldBy.Value);
                if (!hand.IsTracked || !hand.IsPinching)
                {
                    Release(hand);
                    return;
                }

                Vector3 target = hand.PointerPosition + hand.PointerRotation * _localOffset;
                Quaternion delta = hand.PointerRotation * Quaternion.Inverse(_lastHandRotation);
                _grabRotation = delta * _grabRotation;

                transform.position = Vector3.Lerp(transform.position, target, 0.45f);
                transform.rotation = Quaternion.Slerp(transform.rotation, _grabRotation, 0.4f);

                _smoothVelocity = Vector3.Lerp(_smoothVelocity, (transform.position - _lastPosition) / Mathf.Max(0.0001f, Time.deltaTime), 0.35f);
                _lastPosition = transform.position;
                _lastHandRotation = hand.PointerRotation;
                return;
            }

            if (!CanGrab) return;

            HandSide? best = null;
            float bestDist = float.MaxValue;
            TryHand(HandSide.Left, ref best, ref bestDist);
            TryHand(HandSide.Right, ref best, ref bestDist);

            if (best.HasValue)
            {
                var hand = Input.GetHand(best.Value);
                if (hand.PinchStarted || (hand.IsPinching && Vector3.Distance(hand.PointerPosition, transform.position) < GrabRadiusMeters * 0.65f))
                    Grab(best.Value, hand);
            }
        }

        private void TryHand(HandSide side, ref HandSide? best, ref float bestDist)
        {
            var hand = Input.GetHand(side);
            if (!hand.IsTracked) return;
            float d = Vector3.Distance(hand.PointerPosition, transform.position);
            if (d < GrabRadiusMeters && d < bestDist)
            {
                bestDist = d;
                best = side;
            }
        }

        public void Grab(HandSide side, HandState hand)
        {
            if (IsHeld) return;
            _heldBy = side;
            HasEverGrabbed = true;
            _grabStartTime = Time.time;
            _lastHandRotation = hand.PointerRotation;
            _lastPosition = transform.position;
            Vector3 worldOffset = transform.position - hand.PointerPosition;
            float maxLen = Mathf.Max(0.14f, GrabRadiusMeters);
            if (worldOffset.magnitude > maxLen) worldOffset = worldOffset.normalized * maxLen;
            _localOffset = Quaternion.Inverse(hand.PointerRotation) * worldOffset;
            _grabRotation = transform.rotation;

            Body.isKinematic = true;
            Body.useGravity = false;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Grabbed?.Invoke();
            if (AudioDirector.Instance != null) AudioDirector.Instance.Click();
            Input.Haptic(side, 0.5f, 0.05f);
        }

        public void Release(HandState hand)
        {
            if (!_heldBy.HasValue) return;
            var side = _heldBy.Value;
            _heldBy = null;

            Body.isKinematic = false;
            Body.useGravity = true;
            Vector3 v = _smoothVelocity + (hand.IsTracked ? hand.Velocity * ThrowScale : Vector3.zero);
            if (v.magnitude > MaxThrowSpeed) v = v.normalized * MaxThrowSpeed;
            Body.linearVelocity = v;
            Body.angularVelocity = hand.IsTracked ? hand.AngularVelocityDeg * Mathf.Deg2Rad : Vector3.zero;
            Released?.Invoke();
            if (Time.time - _grabStartTime > 0.15f && AudioDirector.Instance != null) AudioDirector.Instance.Whoosh();
            Input.Haptic(side, 0.25f, 0.04f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (AudioDirector.Instance != null && collision.relativeVelocity.magnitude > 0.5f)
                AudioDirector.Instance.Click();
        }
    }
}





