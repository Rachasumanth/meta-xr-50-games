using System.Collections.Generic;
using UnityEngine;

// The OVR scene model API is deprecated in favour of MR Utility Kit, but it is
// the only scene service shipped with this project's pinned Meta XR Core SDK.
// We use its volume/plane anchors deliberately and keep the app self-contained.
#pragma warning disable 0618

namespace MetaXRSim.App
{
    /// <summary>
    /// Turns the real room (Meta scene model) into playable object candidates.
    /// The player points at or gestures around a real table/couch/bowl; this probe
    /// publishes a measured profile that the matchmaker uses to pick the
    /// mini-game and the host uses as the conversion anchor.
    /// </summary>
    public sealed class RealObjectProbe : MonoBehaviour
    {
        public HandInputService Input;
        public bool SceneModelRequested { get; private set; }
        public bool SceneModelLoaded { get; private set; }
        public RealObjectProfile Current { get; private set; }
        public OVRSceneManager SceneManager { get; private set; }

        [Header("Detection")]
        public float MaxSelectionDistance = 1.4f;
        public float MaxPointerAngleDegrees = 28f;

        private readonly List<OVRSceneAnchor> _anchors = new List<OVRSceneAnchor>();
        private Transform _highlight;
        private LineRenderer _highlightLines;
        private WorldLabel _highlightLabel;
        private Transform _templateRoot;
        private float _nextRefresh;

        public void Initialize(HandInputService input)
        {
            Input = input;
            Current = RealObjectProfile.None(transform.position);
            BuildTemplates();
            if (OVRManager.isHmdPresent && UnityEngine.XR.XRSettings.isDeviceActive)
                SetupSceneManager();
            else
                Debug.Log("[RealObjectProbe] No XR scene service; real-object mode uses gesture volumes.");
            BuildHighlight();

#if UNITY_EDITOR
            SceneModelRequested = true;
#endif
        }

        private void BuildTemplates()
        {
            _templateRoot = new GameObject("SceneAnchorTemplates").transform;
            _templateRoot.SetParent(transform, false);

            var plane = new GameObject("PlaneTemplate");
            plane.transform.SetParent(_templateRoot, false);
            plane.SetActive(false);
            plane.AddComponent<OVRSceneAnchor>();
            plane.AddComponent<OVRScenePlane>();
            plane.AddComponent<MeshFilter>();
            var pr = plane.AddComponent<MeshRenderer>();
            pr.enabled = false;
            plane.AddComponent<MeshCollider>();
            plane.AddComponent<OVRScenePlaneMeshFilter>();

            var volume = new GameObject("VolumeTemplate");
            volume.transform.SetParent(_templateRoot, false);
            volume.SetActive(false);
            volume.AddComponent<OVRSceneAnchor>();
            volume.AddComponent<OVRSceneVolume>();
            volume.AddComponent<MeshFilter>();
            var vr = volume.AddComponent<MeshRenderer>();
            vr.enabled = false;
            volume.AddComponent<MeshCollider>();
            volume.AddComponent<OVRSceneVolumeMeshFilter>();
        }

        private void SetupSceneManager()
        {
            SceneManager = GetComponent<OVRSceneManager>();
            if (SceneManager == null) SceneManager = gameObject.AddComponent<OVRSceneManager>();

#pragma warning disable 0618
            SceneManager.PlanePrefab = _templateRoot.Find("PlaneTemplate").GetComponent<OVRSceneAnchor>();
            SceneManager.VolumePrefab = _templateRoot.Find("VolumeTemplate").GetComponent<OVRSceneAnchor>();
#pragma warning restore 0618
            SceneManager.SceneModelLoadedSuccessfully += OnSceneLoaded;
            SceneManager.NoSceneModelToLoad += OnNoSceneModel;

            SceneModelRequested = true;
            bool ok = SceneManager.LoadSceneModel();
            Debug.Log("[RealObjectProbe] Scene model load requested: " + ok);
        }

        private void OnSceneLoaded()
        {
            SceneModelLoaded = true;
            Debug.Log("[RealObjectProbe] Scene model loaded.");
        }

        private void OnNoSceneModel()
        {
            SceneModelLoaded = false;
            Debug.Log("[RealObjectProbe] No scene model available; using gesture volumes.");
        }

        private void BuildHighlight()
        {
            var go = new GameObject("RealObjectHighlight");
            go.transform.SetParent(transform, false);
            _highlight = go.transform;
            _highlightLines = go.AddComponent<LineRenderer>();
            _highlightLines.useWorldSpace = true;
            _highlightLines.loop = false;
            _highlightLines.positionCount = 24;
            _highlightLines.startWidth = 0.0035f;
            _highlightLines.endWidth = 0.0035f;
            _highlightLines.material = MaterialLibrary.Unlit(new Color(0.5f, 1f, 0.9f));
            _highlightLines.startColor = new Color(0.5f, 1f, 0.9f);
            _highlightLines.endColor = new Color(0.5f, 1f, 0.9f);

            _highlightLabel = WorldLabel.Create("ObjectLabel", _highlight, "", 0.022f,
                new Color(0.6f, 1f, 0.95f), new Vector3(0f, 0.02f, 0f));
            _highlight.gameObject.SetActive(false);
        }

        public void Refresh(Vector3 gestureCenter)
        {
            if (Time.time < _nextRefresh) return;
            _nextRefresh = Time.time + 0.08f;

            RealObjectProfile best = RealObjectProfile.None(gestureCenter);
            float bestScore = float.MaxValue;
            Ray ray = Input != null ? Input.Right.PointerRay : new Ray(gestureCenter, Vector3.forward);

            _anchors.Clear();
#pragma warning disable 0618
            OVRSceneAnchor.GetSceneAnchors(_anchors);
#pragma warning restore 0618

            for (int i = 0; i < _anchors.Count; i++)
            {
                var anchor = _anchors[i];
                if (anchor == null) continue;

                float score;
                RealObjectProfile profile;
                if (!TryBuildProfile(anchor, ray, gestureCenter, out profile, out score)) continue;
                if (score > MaxSelectionDistance) continue;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = profile;
                }
            }

            // If no scene anchor is close, a raw scene mesh hit still gives a
            // valid conversion surface (size comes from the gesture).
            if (!best.IsValid && Physics.Raycast(ray, out var hit, 2.5f, ~(1 << WorldButton.UiLayer)))
            {
                // Never treat the app's own hub/boards/hands as a real object.
                bool appOwned = hit.collider != null && AppBootstrap.Instance != null &&
                                hit.collider.transform.IsChildOf(AppBootstrap.Instance.transform);
                if (!appOwned)
                {
                string n = hit.collider != null ? hit.collider.gameObject.name.ToLowerInvariant() : "surface";
                if (n.Contains("plane") || n.Contains("volume") || n.Contains("table") || n.Contains("scene"))
                {
                    best = new RealObjectProfile
                    {
                        HasObject = true,
                        Label = n.Contains("table") ? "TABLE" : "SURFACE",
                        SizeMeters = new Vector3(0.12f, 0.02f, 0.12f),
                        PositionWorld = hit.point,
                        RotationWorld = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.identity,
                        SurfaceNormalWorld = hit.normal,
                        Source = RealObjectSource.SurfaceRaycast,
                        AnchorUuid = string.Empty,
                        Confidence = 0.4f
                    };
                    bestScore = hit.distance;
                }
                }
            }

            Current = best;
            UpdateHighlight();
        }

        private bool TryBuildProfile(OVRSceneAnchor anchor, Ray ray, Vector3 gestureCenter,
            out RealObjectProfile profile, out float score)
        {
            profile = default;
            score = float.MaxValue;

            Vector3 size = Vector3.zero;
            RealObjectSource source;
#pragma warning disable 0618
            var vol = anchor.GetComponentInChildren<OVRSceneVolume>();
            var plane = anchor.GetComponentInChildren<OVRScenePlane>();
#pragma warning restore 0618

            if (vol != null)
            {
                size = vol.Dimensions;
                source = RealObjectSource.SceneVolume;
            }
            else if (plane != null)
            {
                Vector2 d = plane.Dimensions;
                size = new Vector3(d.x, d.y, 0.02f);
                source = RealObjectSource.ScenePlane;
            }
            else
            {
                return false;
            }

            if (size.magnitude < 0.01f) return false;

            // Prefer the object the ray actually hits.
            Collider hitCollider = null;
            Vector3 hitPoint = Vector3.zero;
            Vector3 hitNormal = anchor.transform.up;
            var colliders = anchor.GetComponentsInChildren<Collider>();
            for (int c = 0; c < colliders.Length; c++)
            {
                if (colliders[c] == null) continue;
                if (colliders[c].Raycast(ray, out var hit, MaxSelectionDistance + 1.5f))
                {
                    hitCollider = colliders[c];
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    break;
                }
            }

            float angle = 89f;
            Vector3 toAnchor = (anchor.transform.position - ray.origin).normalized;
            if (toAnchor.sqrMagnitude > 0.0001f)
                angle = Vector3.Angle(ray.direction, toAnchor);

            float distance;
            if (hitCollider != null)
                distance = Vector3.Distance(hitPoint, ray.origin);
            else
                distance = Vector3.Distance(anchor.transform.position, gestureCenter);

            if (hitCollider == null && angle > MaxPointerAngleDegrees) return false;

            Vector3 position = hitCollider != null ? hitPoint : anchor.transform.position;
            Quaternion rotation = anchor.transform.rotation;
            string label = GetLabel(anchor);
            float confidence = hitCollider != null ? 0.85f : 0.55f;

            profile = new RealObjectProfile
            {
                HasObject = true,
                Label = label,
                SizeMeters = size,
                PositionWorld = position,
                RotationWorld = rotation,
                SurfaceNormalWorld = hitNormal,
                Source = source,
                AnchorUuid = anchor.Uuid.ToString(),
                Confidence = confidence
            };
            score = distance;
            return true;
        }

        private static string GetLabel(OVRSceneAnchor anchor)
        {
            try
            {
                if (anchor.Anchor.TryGetComponent<OVRSemanticLabels>(out var labels))
                {
                    string l = labels.Labels;
                    if (!string.IsNullOrEmpty(l))
                    {
                        int comma = l.IndexOf(',');
                        return (comma > 0 ? l.Substring(0, comma) : l).Trim().ToUpperInvariant();
                    }
                }
            }
            catch { }
            return "OBJECT";
        }

        private void UpdateHighlight()
        {
            if (_highlight == null) return;
            if (!Current.IsValid)
            {
                _highlight.gameObject.SetActive(false);
                return;
            }

            _highlight.gameObject.SetActive(true);
            Vector3 c = Current.PositionWorld;
            Vector3 s = Current.SizeMeters;
            Vector3 h = s * 0.5f;
            Quaternion r = Current.RotationWorld;

            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                Vector3 local = new Vector3(
                    (i == 0 || i == 3 || i == 4 || i == 7) ? -h.x : h.x,
                    (i < 4) ? -h.y : h.y,
                    (i == 0 || i == 1 || i == 4 || i == 5) ? -h.z : h.z);
                corners[i] = c + r * local;
            }

            int[,] edges =
            {
                {0,1},{1,2},{2,3},{3,0},{4,5},{5,6},{6,7},{7,4},{0,4},{1,5},{2,6},{3,7}
            };
            for (int e = 0; e < 12; e++)
            {
                _highlightLines.SetPosition(e * 2, corners[edges[e, 0]]);
                _highlightLines.SetPosition(e * 2 + 1, corners[edges[e, 1]]);
            }
            _highlightLabel.transform.position = c + Vector3.up * (h.y + 0.03f);
            _highlightLabel.SetText(Current.Describe());
            _highlightLabel.SetColor(Current.Source == RealObjectSource.None ? Color.white : new Color(0.6f, 1f, 0.95f));
        }
    }
}
