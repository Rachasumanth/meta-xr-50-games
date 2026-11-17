using System;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Physics-collider backed world button. No EventSystem or Canvas is needed;
    /// the hub raycasts hand/controller pointers directly, which keeps the same
    /// input path working in the XR simulator and the desktop fallback.
    /// </summary>
    public sealed class WorldButton : MonoBehaviour
    {
        public const int UiLayer = 5; // Unity's built-in UI layer.

        public Action Clicked;
        public string Id;
        public bool Interactable = true;
        public bool IsHovered { get; private set; }
        public bool IsSelected { get; private set; }

        public Vector3 Size { get; private set; }

        private Renderer _face;
        private Renderer _edge;
        private WorldLabel _label;
        private Color _baseColor;
        private Color _accent;
        private Collider _collider;
        private float _hoverLerp;
        private Vector3 _baseScale;
        private Material _edgeNormalMat;
        private Material _edgeHoverMat;
        private bool _edgeHoverState;
        private float _userScale = 1f;
        private float _lastHoverSound;

        public static WorldButton Create(string id, Transform parent, string text, Vector3 localPosition,
            Quaternion localRotation, Vector2 size, Color baseColor, Color accent, float labelHeight = 0.02f)
        {
            var go = new GameObject("Button_" + id);
            go.layer = UiLayer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            var button = go.AddComponent<WorldButton>();
            button.Build(id, text, size, baseColor, accent, labelHeight);
            return button;
        }

        private void Build(string id, string text, Vector2 size, Color baseColor, Color accent, float labelHeight)
        {
            Id = id;
            Size = new Vector3(size.x, size.y, 0.008f);
            _baseColor = baseColor;
            _accent = accent;
            _baseScale = transform.localScale;

            var face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            face.name = "Face";
            face.layer = UiLayer;
            face.transform.SetParent(transform, false);
            face.transform.localScale = Size;
            _face = face.GetComponent<Renderer>();
            _face.material = MaterialLibrary.Get(baseColor, 0.15f, 0.55f, baseColor * 0.15f);
            _collider = face.GetComponent<Collider>();
            if (_collider != null) _collider.isTrigger = true;

            var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = "Edge";
            edge.layer = UiLayer;
            edge.transform.SetParent(transform, false);
            edge.transform.localPosition = new Vector3(0f, 0f, 0.006f);
            edge.transform.localScale = new Vector3(Size.x * 1.035f, Size.y * 1.09f, 0.003f);
            _edge = edge.GetComponent<Renderer>();
            _edgeNormalMat = MaterialLibrary.Get(accent * 0.55f, 0.05f, 0.7f, accent * 0.20f);
            _edgeHoverMat = MaterialLibrary.Get(accent * 1.5f, 0.05f, 0.7f, accent * 0.85f);
            _edge.material = _edgeNormalMat;
            var edgeCollider = edge.GetComponent<Collider>();
            if (edgeCollider != null) Destroy(edgeCollider);

            _label = WorldLabel.Create("Label", transform, text, labelHeight, Color.white, new Vector3(0f, 0f, -0.006f),
                Quaternion.Euler(0f, 180f, 0f));
        }

        public void SetText(string text)
        {
            if (_label != null) _label.SetText(text);
        }

        public void SetColors(Color baseColor, Color accent)
        {
            _baseColor = baseColor;
            _accent = accent;
            if (_face != null) _face.material = MaterialLibrary.Get(baseColor, 0.15f, 0.55f, baseColor * 0.15f);
            if (_edge != null)
            {
                _edgeNormalMat = MaterialLibrary.Get(accent * 0.55f, 0.05f, 0.7f, accent * 0.20f);
                _edgeHoverMat = MaterialLibrary.Get(accent * 1.5f, 0.05f, 0.7f, accent * 0.85f);
                _edge.material = _hoverLerp > 0.5f ? _edgeHoverMat : _edgeNormalMat;
            }
        }

        public void SetInteractable(bool value)
        {
            Interactable = value;
            if (_face != null)
            {
                var c = _baseColor;
                if (!value) c *= 0.55f;
                _face.material = MaterialLibrary.Get(c, 0.15f, 0.55f, c * 0.15f);
            }
        }

        public void SetLocalPosition(Vector3 p)
        {
            transform.localPosition = p;
        }

        public void SetLocalRotation(Quaternion r)
        {
            transform.localRotation = r;
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }

        public void SetScaleMultiplier(float multiplier)
        {
            _userScale = Mathf.Max(0.001f, multiplier);
            transform.localScale = _baseScale * _userScale;
        }

        /// <summary>Ray test against the button face. Returns true and the hit distance.</summary>
        public bool Raycast(Ray ray, out float distance)
        {
            distance = float.MaxValue;
            if (!Interactable || _collider == null) return false;
            if (_collider.Raycast(ray, out var hit, 6f))
            {
                distance = hit.distance;
                return true;
            }
            return false;
        }

        public void SetHover(bool hovered)
        {
            if (hovered && !IsHovered && Time.time - _lastHoverSound > 0.12f)
            {
                _lastHoverSound = Time.time;
                AudioDirector.Instance?.Hover();
            }
            IsHovered = hovered;
        }

        public void Press()
        {
            if (!Interactable) return;
            IsSelected = true;
            AudioDirector.Instance?.Click();
            SparkleBurst.Play(transform.parent, transform.position + Vector3.up * 0.012f,
                _accent, 7, 0.055f, 0.38f, 0.0035f);
            Clicked?.Invoke();
        }

        private void Update()
        {
            float target = IsHovered && Interactable ? 1f : 0f;
            _hoverLerp = Mathf.MoveTowards(_hoverLerp, target, Time.deltaTime * 8f);
            float pulse = 1f + 0.022f * _hoverLerp;
            transform.localScale = _baseScale * pulse * _userScale;

            if (_edge != null)
            {
                bool hover = _hoverLerp > 0.5f;
                if (hover != _edgeHoverState)
                {
                    _edgeHoverState = hover;
                    _edge.material = hover ? _edgeHoverMat : _edgeNormalMat;
                }
            }
            IsSelected = false;
        }
    }
}

