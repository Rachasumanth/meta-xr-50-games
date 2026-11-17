using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// World-space text without requiring a Canvas. Used by the hub, menu and
    /// mini-game HUD. The font is the built-in LegacyRuntime font, which is
    /// included in player builds and works on Android.
    /// </summary>
    public sealed class WorldLabel : MonoBehaviour
    {
        public TextMesh Mesh { get; private set; }
        public MeshRenderer Renderer { get; private set; }
        public float TargetHeight { get; private set; }
        public string Text => Mesh != null ? Mesh.text : string.Empty;

        private Vector3 _baseScale = Vector3.one;
        private float _scaleMultiplier = 1f;

        public static WorldLabel Create(string name, Transform parent, string text, float worldHeight,
            Color color, Vector3 localPosition, Quaternion? localRotation = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<WorldLabel>();
            label.Build(text, worldHeight, color, localPosition, localRotation ?? Quaternion.identity);
            return label;
        }

        private void Build(string text, float worldHeight, Color color, Vector3 localPosition, Quaternion localRotation)
        {
            Mesh = gameObject.AddComponent<TextMesh>();
            Renderer = gameObject.GetComponent<MeshRenderer>();
            var font = MaterialLibrary.UiFont;
            if (font != null)
            {
                Mesh.font = font;
                Renderer.sharedMaterial = font.material;
            }

            Mesh.text = text;
            Mesh.anchor = TextAnchor.MiddleCenter;
            Mesh.alignment = TextAlignment.Center;
            Mesh.fontSize = 64;
            Mesh.characterSize = 0.1f;
            Mesh.color = color;

            transform.localPosition = localPosition;
            transform.localRotation = localRotation;

            TargetHeight = Mathf.Max(0.004f, worldHeight);
            Fit();
        }

        private void Fit()
        {
            if (Renderer == null || Mesh == null) return;
            float h = Renderer.localBounds.size.y;
            if (h < 1e-5f)
            {
                float parentScale = Mathf.Abs(transform.lossyScale.y);
                h = Renderer.bounds.size.y / Mathf.Max(1e-5f, parentScale);
            }
            if (h < 1e-5f) h = TargetHeight;
            float s = TargetHeight / h;
            _baseScale = Vector3.one * s;
            transform.localScale = _baseScale * _scaleMultiplier;
        }

        public void SetText(string text)
        {
            if (Mesh == null) return;
            Mesh.text = text;
            Fit();
        }

        public void SetColor(Color color)
        {
            if (Mesh != null) Mesh.color = color;
        }

        public void SetAlpha(float alpha)
        {
            if (Mesh == null) return;
            Color c = Mesh.color;
            c.a = Mathf.Clamp01(alpha);
            Mesh.color = c;
        }

        public void SetScaleMultiplier(float multiplier)
        {
            _scaleMultiplier = Mathf.Max(0.001f, multiplier);
            transform.localScale = _baseScale * _scaleMultiplier;
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }
    }
}
