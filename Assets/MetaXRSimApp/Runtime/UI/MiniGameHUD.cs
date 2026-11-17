using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Floating board HUD for an active mini-game. Billboarded, world-space and
    /// readable in passthrough without a Canvas.
    /// </summary>
    public sealed class MiniGameHUD : MonoBehaviour
    {
        public WorldLabel Title;
        public WorldLabel Score;
        public WorldLabel Timer;
        public WorldLabel Hint;
        public WorldLabel Status;
        public Transform Panel;
        public Transform BarFill;

        private Camera _camera;
        private float _barWidth = 0.20f;
        private float _displayFill;
        private float _targetFill;
        private float _scorePop;
        private int _lastScore = int.MinValue;
        private int _lastTarget = int.MinValue;
        private int _lastTimer = int.MinValue;
        private string _lastHint;
        private string _lastStatus;

        public static MiniGameHUD Create(Transform parent, string title, float boardSize, Color accent)
        {
            var go = new GameObject("HUD");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.38f + boardSize * 1.15f, 0f);
            var hud = go.AddComponent<MiniGameHUD>();
            hud.Build(title, accent, boardSize);
            return hud;
        }

        private void Build(string title, Color accent, float boardSize)
        {
            float w = Mathf.Clamp(boardSize * 3.2f, 0.32f, 0.50f);
            float h = w * 0.62f;
            _barWidth = w * 0.82f;

            var panel = new GameObject("Panel");
            panel.transform.SetParent(transform, false);
            panel.transform.localPosition = new Vector3(0f, 0f, 0.004f);
            panel.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Quad;
            panel.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(new Color(0.03f, 0.045f, 0.07f, 1f), 0.72f);
            panel.transform.localScale = new Vector3(w, h, 1f);
            Panel = panel.transform;

            var edge = new GameObject("Edge");
            edge.transform.SetParent(transform, false);
            edge.transform.localPosition = new Vector3(0f, 0f, 0.003f);
            edge.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Quad;
            edge.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(accent, 0.30f);
            edge.transform.localScale = new Vector3(w * 1.035f, h * 1.06f, 1f);

            Title = WorldLabel.Create("Title", transform, title, h * 0.16f, Color.white,
                new Vector3(0f, h * 0.30f, 0f), Quaternion.Euler(0f, 0f, 0f));
            Score = WorldLabel.Create("Score", transform, "0 / 120", h * 0.11f, accent,
                new Vector3(0f, h * 0.11f, 0f));
            Timer = WorldLabel.Create("Timer", transform, "80 s", h * 0.10f, new Color(0.85f, 0.9f, 1f),
                new Vector3(0f, h * 0.02f, 0f));
            Hint = WorldLabel.Create("Hint", transform, "", h * 0.075f, new Color(0.75f, 0.82f, 0.9f),
                new Vector3(0f, -h * 0.10f, 0f));
            Status = WorldLabel.Create("Status", transform, "", h * 0.085f, Color.white,
                new Vector3(0f, -h * 0.23f, 0f));

            var barBg = new GameObject("BarBg");
            barBg.transform.SetParent(transform, false);
            barBg.transform.localPosition = new Vector3(0f, -h * 0.35f, 0f);
            barBg.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Quad;
            barBg.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(new Color(0.2f, 0.25f, 0.35f, 1f), 0.7f);
            barBg.transform.localScale = new Vector3(_barWidth, h * 0.035f, 1f);

            var fill = new GameObject("BarFill");
            fill.transform.SetParent(transform, false);
            fill.transform.localPosition = new Vector3(-_barWidth * 0.5f, -h * 0.35f, -0.002f);
            fill.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Quad;
            fill.AddComponent<MeshRenderer>().material = MaterialLibrary.Glow(accent, 1.2f);
            fill.transform.localScale = new Vector3(0.001f, h * 0.03f, 1f);
            BarFill = fill.transform;

            _camera = Camera.main;
            transform.localScale = Vector3.one * 0.84f;
            Tweener.Play(this, 0.5f,
                t => transform.localScale = Vector3.one * Mathf.Lerp(0.84f, 1f, t),
                EaseType.OutBack);
        }

        public void SetScore(float value, float target)
        {
            int rounded = Mathf.RoundToInt(value);
            int targetInt = Mathf.RoundToInt(target);
            _targetFill = Mathf.Clamp01(value / Mathf.Max(1f, target));
            if (_lastScore == rounded && _lastTarget == targetInt) return;
            if (_lastScore != int.MinValue) _scorePop = 1f;
            _lastScore = rounded;
            _lastTarget = targetInt;
            Score.SetText(rounded + " / " + targetInt);
        }

        public void SetTimer(float seconds)
        {
            int whole = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            if (_lastTimer != whole)
            {
                _lastTimer = whole;
                Timer.SetText(whole + " s");
                Timer.SetColor(seconds < 15f ? new Color(1f, 0.45f, 0.4f) : new Color(0.85f, 0.9f, 1f));
            }
        }

        public void SetHint(string text)
        {
            if (_lastHint == text) return;
            _lastHint = text;
            Hint.SetText(text);
        }

        public void SetStatus(string text, Color color)
        {
            string key = text + "|" + ColorUtility.ToHtmlStringRGB(color);
            if (_lastStatus == key) return;
            _lastStatus = key;
            Status.SetText(text);
            Status.SetColor(color);
        }

        private void Update()
        {
            if (BarFill == null) return;
            _displayFill = Mathf.MoveTowards(_displayFill, _targetFill, Time.deltaTime * 0.75f);
            BarFill.localScale = new Vector3(Mathf.Max(0.001f, _barWidth * _displayFill),
                BarFill.localScale.y, BarFill.localScale.z);
            BarFill.localPosition = new Vector3(-_barWidth * 0.5f + _barWidth * _displayFill * 0.5f,
                BarFill.localPosition.y, BarFill.localPosition.z);

            _scorePop = Mathf.Max(0f, _scorePop - Time.deltaTime * 3.4f);
            if (Score != null) Score.SetScaleMultiplier(1f + 0.10f * _scorePop * _scorePop);
            if (Timer != null)
            {
                bool urgent = _lastTimer >= 0 && _lastTimer <= 15;
                Timer.SetScaleMultiplier(urgent ? 1f + 0.06f * Mathf.Sin(Time.time * 7f) : 1f);
            }
        }

        private void OnDestroy()
        {
            Tweener.Cancel(this);
        }

        private void LateUpdate()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;
            Vector3 toCam = transform.position - _camera.transform.position;
            if (toCam.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);
        }
    }
}


