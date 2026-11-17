using UnityEngine;

namespace MetaXRSim.App
{
    public enum HandSide
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// Engine-agnostic snapshot of one hand or controller. Meta XR OVRHand,
    /// OVRInput controllers and the desktop simulator all publish this struct,
    /// so gameplay code never branches on the input backend.
    /// </summary>
    public struct HandState
    {
        public bool IsTracked;
        public bool IsController;
        public bool IsPinching;
        public bool PinchStarted;
        public bool PinchEnded;
        public float PinchStrength;
        public float HandScale;

        public Vector3 PointerPosition;
        public Vector3 PointerForward;
        public Quaternion PointerRotation;

        public Vector3 PalmPosition;
        public Vector3 PalmNormal;
        public Quaternion PalmRotation;

        public Vector3 IndexTip;
        public Vector3 ThumbTip;
        public Vector3 Wrist;

        public float LastUpdateTime;

        public bool IsValid => IsTracked && (PointerRotation.x != 0f || PointerRotation.w != 0f);

        public Ray PointerRay => new Ray(PointerPosition, PointerForward.sqrMagnitude > 1e-6f ? PointerForward.normalized : Vector3.forward);

        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocityDeg { get; set; }

        public static HandState Neutral(Vector3 position, Quaternion rotation)
        {
            return new HandState
            {
                IsTracked = false,
                IsController = false,
                IsPinching = false,
                PinchStrength = 0f,
                HandScale = 1f,
                PointerPosition = position,
                PointerForward = rotation * Vector3.forward,
                PointerRotation = rotation,
                PalmPosition = position,
                PalmNormal = rotation * Vector3.up,
                PalmRotation = rotation,
                IndexTip = position,
                ThumbTip = position,
                Wrist = position,
                LastUpdateTime = Time.time
            };
        }
    }

    public sealed class HandHistory
    {
        private readonly Vector3[] _positions = new Vector3[8];
        private readonly Quaternion[] _rotations = new Quaternion[8];
        private int _count;
        private int _head;

        public void Push(Vector3 p, Quaternion r)
        {
            _head = (_head + 1) % _positions.Length;
            _positions[_head] = p;
            _rotations[_head] = r;
            if (_count < _positions.Length) _count++;
        }

        public bool TryVelocity(float now, out Vector3 linear, out Vector3 angularDeg)
        {
            linear = Vector3.zero;
            angularDeg = Vector3.zero;
            if (_count < 2) return false;
            int oldest = (_head - (_count - 1) + _positions.Length * 2) % _positions.Length;
            Vector3 dp = _positions[_head] - _positions[oldest];

            Quaternion dq = _rotations[_head] * Quaternion.Inverse(_rotations[oldest]);
            dq.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;

            float dt = Mathf.Max(1e-4f, Time.deltaTime * (_count - 1));
            linear = dp / dt;
            if (axis.sqrMagnitude > 1e-6f) angularDeg = axis.normalized * (angle / dt);
            return true;
        }
    }
}
