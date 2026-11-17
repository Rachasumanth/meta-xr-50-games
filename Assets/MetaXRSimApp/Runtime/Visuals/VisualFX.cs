using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>Per-object emissive pulse. Uses a private material clone so the
    /// shared MaterialLibrary palette is never mutated.</summary>
    public sealed class PulseGlow : MonoBehaviour
    {
        public Color GlowColor = new Color(0.3f, 0.9f, 1f);
        public float Speed = 1.3f;
        public float Min = 0.55f;
        public float Max = 1.7f;
        public float Intensity = 1.9f;
        public float Phase = 0f;

        private Material _mat;

        private void Start()
        {
            var r = GetComponent<Renderer>();
            if (r == null) return;
            _mat = new Material(r.material);
            r.material = _mat;
            if (_mat.HasProperty("_EmissionColor")) _mat.EnableKeyword("_EMISSION");
            Apply(1f);
        }

        private void Update()
        {
            float k = Mathf.Lerp(Min, Max, 0.5f + 0.5f * Mathf.Sin((Time.time + Phase) * Speed));
            Apply(k);
        }

        private void Apply(float k)
        {
            if (_mat == null) return;
            Color c = GlowColor * (Intensity * k);
            if (_mat.HasProperty("_EmissionColor")) _mat.SetColor("_EmissionColor", c);
            if (_mat.HasProperty("_Color")) _mat.color = new Color(GlowColor.r, GlowColor.g, GlowColor.b, _mat.color.a);
        }

        private void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }
    }

    /// <summary>Cheap idle motion for rings, rims, hats and holograms.</summary>
    public sealed class AccessoryAnimator : MonoBehaviour
    {
        public float SpinDegPerSec = 18f;
        public Vector3 SpinAxis = Vector3.up;
        public float BobAmplitude = 0f;
        public float BobSpeed = 1.2f;
        public float PulseAmount = 0f;
        public float PulseSpeed = 1.8f;
        public float Phase = 0f;

        private Vector3 _basePos;
        private Vector3 _baseScale;
        private float _random;

        private void Start()
        {
            _basePos = transform.localPosition;
            _baseScale = transform.localScale;
            _random = (Phase == 0f ? Mathf.Abs(GetInstanceID() % 100) * 0.03f : Phase);
        }

        private void Update()
        {
            if (SpinDegPerSec != 0f && SpinAxis.sqrMagnitude > 0.0001f)
                transform.Rotate(SpinAxis.normalized, SpinDegPerSec * Time.deltaTime, Space.Self);
            if (BobAmplitude != 0f)
                transform.localPosition = _basePos + Vector3.up * (Mathf.Sin((Time.time + _random) * BobSpeed) * BobAmplitude);
            if (PulseAmount != 0f)
                transform.localScale = _baseScale * (1f + Mathf.Sin((Time.time + _random) * PulseSpeed) * PulseAmount);
        }
    }

    /// <summary>Short-lived emissive shard burst for scores, blocks and conversion flashes.</summary>
    public sealed class SparkleBurst : MonoBehaviour
    {
        private Transform[] _shards;
        private Vector3[] _dirs;
        private float[] _distances;
        private float[] _phases;
        private Vector3 _center;
        private float _radius;
        private float _duration;
        private float _size;
        private float _age;

        public static void Play(Transform parent, Vector3 worldPosition, Color color,
            int count = 12, float radius = 0.20f, float duration = 0.55f, float size = 0.006f)
        {
            var go = new GameObject("SparkleBurst");
            go.transform.SetParent(parent != null ? parent : null, false);
            go.transform.position = worldPosition;
            var burst = go.AddComponent<SparkleBurst>();
            burst.Init(color, count, radius, duration, size);
        }

        private void Init(Color color, int count, float radius, float duration, float size)
        {
            _center = transform.position;
            _radius = Mathf.Max(0.02f, radius);
            _duration = Mathf.Max(0.1f, duration);
            _size = Mathf.Max(0.002f, size);
            count = Mathf.Clamp(count, 4, 32);
            _shards = new Transform[count];
            _dirs = new Vector3[count];
            _distances = new float[count];
            _phases = new float[count];
            var mat = MaterialLibrary.Glow(color, 2.4f);

            for (int i = 0; i < count; i++)
            {
                var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                s.name = "Shard" + i;
                s.transform.SetParent(transform, false);
                s.transform.position = _center + Random.insideUnitSphere * 0.012f;
                s.transform.localScale = Vector3.one * _size;
                var c = s.GetComponent<Collider>();
                if (c != null) Object.Destroy(c);
                s.GetComponent<Renderer>().material = mat;

                _shards[i] = s.transform;
                _dirs[i] = Random.onUnitSphere;
                _distances[i] = _radius * Random.Range(0.55f, 1.15f);
                _phases[i] = Random.Range(0f, Mathf.PI * 2f);
            }
        }

        private void Update()
        {
            _age += Time.deltaTime;
            float t = Mathf.Clamp01(_age / _duration);
            float e = Ease.Evaluate(EaseType.OutCubic, t);
            float scale = _size * (1f - t);
            for (int i = 0; i < _shards.Length; i++)
            {
                var s = _shards[i];
                if (s == null) continue;
                Vector3 dir = _dirs[i];
                s.position = _center + dir * (_distances[i] * e) + Vector3.up * (0.05f * e);
                s.localScale = Vector3.one * Mathf.Max(0.0001f, scale);
                s.Rotate(Vector3.up, (60f + i * 3f) * Time.deltaTime, Space.World);
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }

    /// <summary>Floating "+12" style feedback over the board.</summary>
    public sealed class ScorePopup : MonoBehaviour
    {
        private WorldLabel _label;
        private Vector3 _start;
        private float _age;
        private float _duration;
        private float _height;

        public static void Show(Transform parent, Vector3 worldPosition, string text, Color color,
            float height = 0.12f, float duration = 0.9f, float textSize = 0.02f)
        {
            var go = new GameObject("ScorePopup");
            go.transform.SetParent(parent != null ? parent : null, false);
            go.transform.position = worldPosition;
            var popup = go.AddComponent<ScorePopup>();
            popup._start = worldPosition;
            popup._duration = Mathf.Max(0.3f, duration);
            popup._height = height;
            popup._label = WorldLabel.Create("PopupText", go.transform, text, textSize, color, Vector3.zero);
            popup._label.SetScaleMultiplier(0.65f);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            float t = Mathf.Clamp01(_age / _duration);
            transform.position = _start + Vector3.up * (_height * Ease.Evaluate(EaseType.OutCubic, t));
            _label.SetAlpha(1f - Ease.Evaluate(EaseType.InQuad, t));
            _label.SetScaleMultiplier(0.65f + 0.35f * Ease.Evaluate(EaseType.OutBack, Mathf.Clamp01(t * 2.4f)));
            if (t >= 1f) Destroy(gameObject);
        }
    }

    /// <summary>Small holographic icon for the five shape families.</summary>
    public static class HologramIcon
    {
        public static Transform Create(Transform parent, string name, Vector3 localPosition,
            ShapeKind kind, Color color, float diameter)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;
            diameter = Mathf.Max(0.012f, diameter);

            GameObject shell;
            switch (kind)
            {
                case ShapeKind.Hemisphere:
                    shell = MeshObject(ProceduralMesh.Dome, Vector3.one * diameter, color);
                    break;
                case ShapeKind.DomedBowl:
                    shell = MeshObject(ProceduralMesh.Bowl, Vector3.one * diameter, color);
                    break;
                case ShapeKind.ProlateSpheroid:
                    shell = SphereObject(new Vector3(0.72f, 1.0f, 0.72f) * diameter, color);
                    break;
                case ShapeKind.OblateSpheroid:
                    shell = SphereObject(new Vector3(1.0f, 0.40f, 0.86f) * diameter, color);
                    break;
                default:
                    shell = SphereObject(Vector3.one * diameter, color);
                    break;
            }
            shell.transform.SetParent(root.transform, false);

            var ring = new GameObject("IconRing");
            ring.transform.SetParent(root.transform, false);
            ring.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
            ring.AddComponent<MeshRenderer>().material = MaterialLibrary.Glow(color, 1.8f);
            ring.transform.localScale = new Vector3(diameter * 1.5f, 1f, diameter * 1.5f);
            ring.transform.localRotation = Quaternion.Euler(62f, 0f, 0f);

            var anim = root.AddComponent<AccessoryAnimator>();
            anim.SpinDegPerSec = 16f;
            anim.SpinAxis = Vector3.up;
            anim.BobAmplitude = 0.0035f;
            anim.BobSpeed = 1.4f;
            anim.PulseAmount = 0.045f;
            anim.PulseSpeed = 1.6f;

            var animRing = ring.AddComponent<AccessoryAnimator>();
            animRing.SpinDegPerSec = 46f;
            animRing.SpinAxis = Vector3.forward;
            animRing.BobAmplitude = 0f;

            return root.transform;
        }

        private static GameObject MeshObject(Mesh mesh, Vector3 scale, Color color)
        {
            var go = new GameObject("IconShell");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(color, 0.32f);
            go.transform.localScale = scale;
            return go;
        }

        private static GameObject SphereObject(Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "IconShell";
            go.transform.localScale = scale;
            var c = go.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
            go.GetComponent<Renderer>().material = MaterialLibrary.Transparent(color, 0.32f);
            return go;
        }
    }

    /// <summary>Living Forge Orb: rings, motes, light and a breathing core.</summary>
    public sealed class ForgeOrbFX : MonoBehaviour
    {
        private Transform _fxRoot;
        private Transform _ringA;
        private Transform _ringB;
        private Transform[] _motes;
        private float[] _moteAngle;
        private float[] _moteRadius;
        private float[] _moteHeight;
        private Vector3 _baseLocal;
        private float _phase;

        public void Build(Color accent)
        {
            _baseLocal = transform.localPosition;
            _phase = Random.Range(0f, 10f);

            _fxRoot = new GameObject("ForgeFX").transform;
            _fxRoot.SetParent(transform.parent, false);
            _fxRoot.localPosition = _baseLocal;

            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "ForgeCore";
            core.transform.SetParent(_fxRoot, false);
            core.transform.localScale = Vector3.one * 0.036f;
            var coreCollider = core.GetComponent<Collider>();
            if (coreCollider != null) Destroy(coreCollider);
            core.GetComponent<Renderer>().material = MaterialLibrary.Glow(accent, 2.6f);
            var corePulse = core.AddComponent<PulseGlow>();
            corePulse.GlowColor = accent;
            corePulse.Speed = 2.2f;

            _ringA = Torus(_fxRoot, "OrbRingA", new Vector3(0.19f, 1f, 0.19f), Quaternion.Euler(72f, 0f, 0f), accent);
            _ringB = Torus(_fxRoot, "OrbRingB", new Vector3(0.24f, 1f, 0.24f), Quaternion.Euler(18f, 0f, 36f), accent);
            var ringPulse = _ringA.gameObject.AddComponent<PulseGlow>();
            ringPulse.GlowColor = accent;
            ringPulse.Speed = 1.1f;

            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "OrbBeam";
            beam.transform.SetParent(_fxRoot, false);
            beam.transform.localPosition = new Vector3(0f, -0.036f, 0f);
            beam.transform.localScale = new Vector3(0.05f, 0.032f, 0.05f);
            var bc = beam.GetComponent<Collider>();
            if (bc != null) Destroy(bc);
            beam.GetComponent<Renderer>().material = MaterialLibrary.Transparent(accent, 0.18f);
            var beamAnim = beam.AddComponent<AccessoryAnimator>();
            beamAnim.PulseAmount = 0.08f;
            beamAnim.PulseSpeed = 2.4f;

            var lightGo = new GameObject("OrbLight");
            lightGo.transform.SetParent(_fxRoot, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = accent;
            light.range = 0.38f;
            light.intensity = 0.55f;
            light.shadows = LightShadows.None;

            _motes = new Transform[8];
            _moteAngle = new float[8];
            _moteRadius = new float[8];
            _moteHeight = new float[8];
            var moteMat = MaterialLibrary.Glow(accent, 2f);
            for (int i = 0; i < _motes.Length; i++)
            {
                var m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m.name = "Mote" + i;
                m.transform.SetParent(_fxRoot, false);
                m.transform.localScale = Vector3.one * Random.Range(0.0035f, 0.0065f);
                var mc = m.GetComponent<Collider>();
                if (mc != null) Destroy(mc);
                m.GetComponent<Renderer>().material = moteMat;
                _motes[i] = m.transform;
                _moteAngle[i] = Random.Range(0f, Mathf.PI * 2f);
                _moteRadius[i] = Random.Range(0.07f, 0.13f);
                _moteHeight[i] = Random.Range(-0.025f, 0.055f);
            }

            var floorRing = transform.parent.Find("ForgeOrbRing");
            if (floorRing != null)
            {
                var rp = floorRing.gameObject.AddComponent<PulseGlow>();
                rp.GlowColor = accent;
                rp.Speed = 0.9f;
                rp.Min = 0.7f;
                rp.Max = 1.8f;
            }
        }

        private static Transform Torus(Transform parent, string name, Vector3 scale, Quaternion rot, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localRotation = rot;
            go.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Torus;
            go.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(color, 0.55f);
            return go.transform;
        }

        private void Update()
        {
            if (_fxRoot == null) return;
            float t = Time.time + _phase;
            transform.localPosition = _baseLocal + Vector3.up * (Mathf.Sin(t * 1.35f) * 0.006f);
            _fxRoot.localPosition = transform.localPosition;
            if (_ringA != null) _ringA.localRotation = Quaternion.Euler(72f + Mathf.Sin(t * 0.7f) * 8f, t * 22f, 0f);
            if (_ringB != null) _ringB.localRotation = Quaternion.Euler(18f, -t * 17f, 36f + Mathf.Sin(t * 0.9f) * 10f);
            if (_motes != null)
            {
                for (int i = 0; i < _motes.Length; i++)
                {
                    if (_motes[i] == null) continue;
                    float a = _moteAngle[i] + t * (0.45f + i * 0.035f);
                    _motes[i].localPosition = new Vector3(Mathf.Cos(a) * _moteRadius[i],
                        _moteHeight[i] + Mathf.Sin(t * (1.2f + i * 0.08f)) * 0.012f,
                        Mathf.Sin(a) * _moteRadius[i]);
                }
            }
        }
    }
}

