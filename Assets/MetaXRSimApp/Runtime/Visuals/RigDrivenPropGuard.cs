using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Several of the original 50 rigs were authored as pure simulations that
    /// write their prop transform in world space every frame from an internal
    /// tracker that is never fed in the unified hub (the gate contract never
    /// required live scene wiring). Left alone that teleports the board's prop to
    /// the world origin: held props flicker, the session's out-of-bounds check
    /// fires every second and drains integrity (socket_trackball, paddlewheel_pod,
    /// bobsled_chute...), and thrown props snap away.
    ///
    /// The app, not the rig, owns the prop transform. The snapshot component runs
    /// before every default-order script (the rigs) and remembers where the prop
    /// was left by physics and the player's hand. This guard runs right after the
    /// rigs but before the hand/session logic and undoes a rig write that
    /// teleports the prop. Small, well-behaved rig wobbles are left untouched.
    /// </summary>
    [DefaultExecutionOrder(10)]
    public sealed class RigDrivenPropGuard : MonoBehaviour
    {
        public GrabInteractable Grab;
        public Rigidbody Body;
        /// <summary>Per-frame movement caused by a rig that counts as a teleport.</summary>
        public float DriftThresholdMeters = 0.04f;

        private Vector3 _pre;
        private Quaternion _preRot = Quaternion.identity;
        private bool _hasPre;
        private bool _guardActive;
        private int _undoCount;

        public bool GuardActive => _guardActive;
        public int UndoCount => _undoCount;

        private void Awake()
        {
            var snap = gameObject.AddComponent<RigDrivenPropSnapshot>();
            snap.Guard = this;
        }

        internal void CapturePreRig()
        {
            _pre = transform.position;
            _preRot = transform.rotation;
            _hasPre = true;
        }

        private void Update()
        {
            if (!_hasPre) return;

            if (!_guardActive)
            {
                float jump = Vector3.Distance(transform.position, _pre);
                if (jump > Mathf.Max(0.02f, DriftThresholdMeters)) _guardActive = true;
            }

            // After the player has taken the prop the hand and physics own it, even
            // if the rig only nudges it a few millimetres per frame (the shell-game
            // rig resets it to the board centre every frame, which pinned a held
            // prop away from every pickup).
            bool owned = Grab != null && Grab.HasEverGrabbed;

            if (_guardActive || owned)
            {
                if (transform.position != _pre || transform.rotation != _preRot) _undoCount++;
                transform.SetPositionAndRotation(_pre, _preRot);
            }
        }
    }

    /// <summary>Runs before every rig so the guard knows the true prop pose.</summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class RigDrivenPropSnapshot : MonoBehaviour
    {
        public RigDrivenPropGuard Guard;

        private void Update()
        {
            if (Guard != null) Guard.CapturePreRig();
        }
    }

    /// <summary>
    /// Keeps every rig-driven board part (holograms, ships, beams...) inside a
    /// seated-reach play volume. Several rigs place "virtual" parts metres away
    /// in world space; clamping the radial distance keeps the whole experience
    /// within arm's reach (bus-stop / airplane-seat test) without freezing the
    /// part's own animation.
    /// </summary>
    [DefaultExecutionOrder(20)]
    public sealed class RigPartClamp : MonoBehaviour
    {
        private Transform _root;
        private System.Collections.Generic.List<Transform> _parts;
        private float _radius;
        private int _clamps;

        public int ClampCount => _clamps;

        public void Init(Transform root, System.Collections.Generic.List<Transform> parts, float radiusMeters)
        {
            _root = root;
            _parts = parts;
            _radius = Mathf.Max(0.15f, radiusMeters);
            _rends = new Renderer[parts.Count][];
            for (int i = 0; i < parts.Count; i++)
                _rends[i] = parts[i] != null ? parts[i].GetComponentsInChildren<Renderer>(true) : new Renderer[0];
        }

        private Renderer[][] _rends;

        private static float LargestHalfExtent(Renderer[] rends)
        {
            float best = 0f;
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] == null || !rends[i].enabled) continue;
                Vector3 e = rends[i].bounds.extents;
                float m = Mathf.Max(e.x, Mathf.Max(e.y, e.z));
                if (m > best) best = m;
            }
            return best;
        }

        private void LateUpdate()
        {
            if (_root == null || _parts == null) return;
            Vector3 c = _root.position;
            for (int i = 0; i < _parts.Count; i++)
            {
                var t = _parts[i];
                if (t == null) continue;
                Vector3 off = t.position - c;
                if (float.IsNaN(off.x) || float.IsNaN(off.y) || float.IsNaN(off.z))
                {
                    t.position = c;
                    _clamps++;
                    continue;
                }
                if (off.sqrMagnitude > _radius * _radius)
                {
                    t.position = c + off.normalized * _radius;
                    _clamps++;
                }

                // Rigs also rewrite localScale (e.g. the micro-planet "moon" hologram is
                // scaled by a 30x factor, filling the room): keep the part inside the volume.
                float half = LargestHalfExtent(_rends[i]);
                if (half > _radius)
                {
                    t.localScale *= _radius / half;
                    _clamps++;
                }
            }
        }
    }
}
