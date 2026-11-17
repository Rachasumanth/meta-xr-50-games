using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// The hub menu: five family portals, a fanned 10-game browser, five quick
    /// shape triggers and the live status board. Interaction is direct pointer
    /// raycasting; there is no EventSystem dependency.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class ForgeMenu : MonoBehaviour
    {
        public ShapeForgeHub Hub;
        public Transform Orb { get; private set; }
        public bool ConsumedSelect { get; private set; }
        public bool HasHover { get; private set; }
        public GameFamily? OpenFamily { get; private set; }

        private readonly List<WorldButton> _familyButtons = new List<WorldButton>();
        private readonly Dictionary<GameFamily, List<WorldButton>> _gameButtons =
            new Dictionary<GameFamily, List<WorldButton>>();
        private readonly List<WorldButton> _presetButtons = new List<WorldButton>();
        private readonly List<WorldButton> _audioButtons = new List<WorldButton>();
        private WorldButton _backButton;
        private Transform _gamePanel;
        private WorldLabel _header;
        private WorldLabel _hint;
        private WorldLabel _shapeStatus;
        private WorldLabel _objectStatus;
        private WorldLabel _candidate;
        private WorldLabel _familyTitle;
        private WorldLabel _presetTitle;
        private WorldLabel _quickStatus;
        private bool _selectLatch;
        private readonly List<WorldButton> _rayButtons = new List<WorldButton>(24);

        public void Build(ShapeForgeHub hub)
        {
            Hub = hub;

            BuildBoard();
            BuildFamilyButtons();
            BuildGamePanels();
            BuildPresetButtons();
            BuildLabels();
            ShowFamily(null);
        }

        private void BuildBoard()
        {
            // Back board for the status copy.
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "InfoBoard";
            board.transform.SetParent(transform, false);
            board.transform.localPosition = new Vector3(0f, 0.20f, 0.22f);
            board.transform.localScale = new Vector3(0.78f, 0.30f, 0.008f);
            board.GetComponent<Renderer>().material = MaterialLibrary.Get(new Color(0.035f, 0.05f, 0.075f), 0.2f, 0.3f);
            var bc = board.GetComponent<Collider>();
            if (bc != null) Destroy(bc);

            var glow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glow.name = "BoardEdge";
            glow.transform.SetParent(transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.20f, 0.216f);
            glow.transform.localScale = new Vector3(0.80f, 0.32f, 0.004f);
            glow.GetComponent<Renderer>().material = MaterialLibrary.Transparent(new Color(0.2f, 0.9f, 1f), 0.18f);
            var gc = glow.GetComponent<Collider>();
            if (gc != null) Destroy(gc);

            // The forge pedestal.
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "ForgeTable";
            table.transform.SetParent(transform, false);
            table.transform.localPosition = new Vector3(0f, -0.018f, 0f);
            table.transform.localScale = new Vector3(0.90f, 0.035f, 0.55f);
            table.GetComponent<Renderer>().material = MaterialLibrary.Get(new Color(0.055f, 0.065f, 0.085f), 0.35f, 0.55f);
            var tc = table.GetComponent<Collider>();
            if (tc != null) tc.isTrigger = true;

            // Forge orb (the gesture target).
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "ForgeOrb";
            orb.transform.SetParent(transform, false);
            orb.transform.localPosition = new Vector3(0f, 0.065f, -0.14f);
            orb.transform.localScale = Vector3.one * 0.095f;
            orb.GetComponent<Renderer>().material = MaterialLibrary.Transparent(new Color(0.25f, 0.8f, 1f), 0.30f);
            var oc = orb.GetComponent<Collider>();
            if (oc != null) oc.isTrigger = true;
            Orb = orb.transform;

            var ring = new GameObject("ForgeOrbRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.02f, -0.14f);
            ring.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
            ring.AddComponent<MeshRenderer>().material = MaterialLibrary.Glow(new Color(0.2f, 0.9f, 1f), 1.8f);
            ring.transform.localScale = new Vector3(0.19f, 1f, 0.19f);

            var orbFx = orb.AddComponent<ForgeOrbFX>();
            orbFx.Build(new Color(0.28f, 0.92f, 1f));
        }

        private void BuildFamilyButtons()
        {
            var families = (GameFamily[])Enum.GetValues(typeof(GameFamily));
            float[] xs = { -0.28f, -0.14f, 0f, 0.14f, 0.28f };
            for (int i = 0; i < families.Length && i < xs.Length; i++)
            {
                var f = families[i];
                var defs = GameCatalog.ByFamily(f);
                string text = ShortFamily(f) + "\n" + defs.Count + " GAMES";
                var btn = WorldButton.Create("Family_" + f, transform, text,
                    new Vector3(xs[i], 0.115f, 0.02f), Quaternion.identity,
                    new Vector2(0.125f, 0.060f), new Color32(40, 54, 72, 255),
                    GameCatalog.FamilyColor(f), 0.010f);
                btn.Clicked = () => ToggleFamily(f);
                _familyButtons.Add(btn);

                HologramIcon.Create(transform, "FamilyIcon_" + f,
                    new Vector3(xs[i], 0.158f, 0.018f), ShapeKindExtensions.FromFamily(f),
                    GameCatalog.FamilyColor(f), 0.030f);
            }
        }

        private static string ShortFamily(GameFamily f)
        {
            switch (f)
            {
                case GameFamily.SpherePinch: return "SPHERES / PINCH";
                case GameFamily.SpherePalm: return "SPHERES / PALM";
                case GameFamily.Spheroid: return "EGGS & PEBBLES";
                case GameFamily.HemispherePinch: return "DOMES / PINCH";
                case GameFamily.HemisphereHand: return "DOMES / HAND";
                default: return f.ToString();
            }
        }

        private void BuildGamePanels()
        {
            var panel = new GameObject("GamePanel");
            panel.transform.SetParent(transform, false);
            panel.transform.localPosition = new Vector3(0f, 0.30f, 0.024f);
            panel.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Quad;
            panel.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(new Color(0.02f, 0.035f, 0.055f), 0.82f);
            panel.transform.localScale = new Vector3(0.76f, 0.21f, 1f);
            _gamePanel = panel.transform;

            _familyTitle = WorldLabel.Create("FamilyTitle", transform, "", 0.016f, Color.white,
                new Vector3(0f, 0.395f, 0.016f), Quaternion.Euler(0f, 180f, 0f));

            var families = (GameFamily[])Enum.GetValues(typeof(GameFamily));
            foreach (var f in families)
            {
                var list = new List<WorldButton>();
                var defs = GameCatalog.ByFamily(f);
                for (int i = 0; i < defs.Count; i++)
                {
                    int col = i % 5;
                    int row = i / 5;
                    float x = -0.285f + col * 0.1425f;
                    float y = 0.338f - row * 0.058f;
                    var def = defs[i];
                    var btn = WorldButton.Create("Game_" + def.Slug, transform, ShortTitle(def.Title),
                        new Vector3(x, y, 0.018f), Quaternion.identity,
                        new Vector2(0.132f, 0.048f), new Color32(52, 64, 84, 255),
                        def.Accent, 0.009f);
                    btn.Clicked = () => Hub.LaunchGame(def);
                    list.Add(btn);
                }
                _gameButtons[f] = list;
            }

            _backButton = WorldButton.Create("Back", transform, "BACK",
                new Vector3(0.335f, 0.245f, 0.012f), Quaternion.identity,
                new Vector2(0.095f, 0.035f), new Color32(60, 42, 48, 255),
                new Color32(255, 135, 120, 255), 0.010f);
            _backButton.Clicked = () => ShowFamily(null);
        }

        private static string ShortTitle(string title)
        {
            if (title.Length <= 18) return title;
            return title.Substring(0, 17) + ".";
        }

        private void BuildPresetButtons()
        {
            _presetTitle = WorldLabel.Create("PresetTitle", transform, "QUICK SHAPES", 0.011f,
                new Color(0.7f, 0.9f, 1f), new Vector3(0.36f, 0.125f, -0.01f), Quaternion.Euler(0f, 180f, 0f));

            string[] labels = { "CIRCLE", "TALL EGG", "FLAT PEBBLE", "DOME", "BOWL" };
            for (int i = 0; i < labels.Length; i++)
            {
                int preset = i + 1;
                var btn = WorldButton.Create("Preset" + preset, transform, labels[i],
                    new Vector3(0.36f, 0.105f - i * 0.026f, -0.01f), Quaternion.identity,
                    new Vector2(0.125f, 0.024f), new Color32(42, 50, 64, 255),
                    new Color32(120, 200, 255, 255), 0.0075f);
                btn.Clicked = () => Hub.SimulatePreset(preset);
                _presetButtons.Add(btn);
            }

            _quickStatus = WorldLabel.Create("QuickStatus", transform, "", 0.009f,
                new Color(0.65f, 0.75f, 0.85f), new Vector3(0.36f, -0.024f, -0.01f), Quaternion.Euler(0f, 180f, 0f));

            var music = WorldButton.Create("MusicToggle", transform, "MUSIC ON",
                new Vector3(-0.405f, 0.098f, -0.01f), Quaternion.identity,
                new Vector2(0.085f, 0.025f), new Color32(30, 40, 54, 255),
                new Color32(112, 208, 255, 255), 0.0065f);
            music.Clicked = ToggleMusic;
            _audioButtons.Add(music);

            var sfx = WorldButton.Create("SfxToggle", transform, "SOUND ON",
                new Vector3(-0.405f, 0.068f, -0.01f), Quaternion.identity,
                new Vector2(0.085f, 0.025f), new Color32(30, 40, 54, 255),
                new Color32(255, 204, 112, 255), 0.0065f);
            sfx.Clicked = ToggleSfx;
            _audioButtons.Add(sfx);

            RefreshAudioLabels();
        }

        private void ToggleMusic()
        {
            MusicDirector.Instance?.Toggle();
            RefreshAudioLabels();
        }

        private void ToggleSfx()
        {
            AudioDirector.Instance?.Toggle();
            RefreshAudioLabels();
        }

        public void RefreshAudioLabels()
        {
            if (_audioButtons.Count < 2) return;
            _audioButtons[0].SetText(MusicDirector.Instance != null && MusicDirector.Instance.Enabled
                ? "MUSIC ON" : "MUSIC OFF");
            _audioButtons[1].SetText(AudioDirector.Instance != null && AudioDirector.Instance.Enabled
                ? "SOUND ON" : "SOUND OFF");
        }

        private void BuildLabels()
        {
            _header = WorldLabel.Create("Header", transform, "SHAPEFORGE  -  50 SPATIAL GAMES", 0.026f,
                new Color(0.75f, 0.95f, 1f), new Vector3(0f, 0.315f, 0.208f), Quaternion.Euler(0f, 180f, 0f));
            _hint = WorldLabel.Create("Hint", transform,
                "Pinch and trace a shape in the air around the Forge Orb. Release to identify it, then pinch to convert.",
                0.0125f, new Color(0.78f, 0.87f, 0.95f), new Vector3(0f, 0.275f, 0.208f), Quaternion.Euler(0f, 180f, 0f));
            _shapeStatus = WorldLabel.Create("ShapeStatus", transform, "Shape: waiting for gesture", 0.012f,
                new Color(0.6f, 1f, 0.85f), new Vector3(0f, 0.232f, 0.208f), Quaternion.Euler(0f, 180f, 0f));
            _objectStatus = WorldLabel.Create("ObjectStatus", transform, "Real object: none selected", 0.011f,
                new Color(0.7f, 0.85f, 1f), new Vector3(0f, 0.208f, 0.208f), Quaternion.Euler(0f, 180f, 0f));
            _candidate = WorldLabel.Create("Candidate", transform, "", 0.015f,
                new Color(1f, 0.85f, 0.4f), new Vector3(0f, 0.176f, 0.208f), Quaternion.Euler(0f, 180f, 0f));
        }

        public void ShowFamily(GameFamily? family)
        {
            OpenFamily = family;
            bool open = family.HasValue;
            _gamePanel.gameObject.SetActive(open);
            _backButton.SetVisible(open);
            _familyTitle.SetVisible(open);

            foreach (var kv in _gameButtons)
            {
                bool show = open && kv.Key == family.Value;
                for (int i = 0; i < kv.Value.Count; i++) kv.Value[i].SetVisible(show);
            }

            if (open)
            {
                _familyTitle.SetText(GameCatalog.FamilyTitle(family.Value).ToUpperInvariant() + "  -  PICK A GAME");
                AnimateFamilyPanel(true);
            }
            else
            {
                Tweener.Cancel(_gamePanel);
            }
        }

        private void AnimateFamilyPanel(bool opening)
        {
            if (!gameObject.activeInHierarchy || !OpenFamily.HasValue) return;
            Tweener.Cancel(_gamePanel);
            var baseScale = new Vector3(0.76f, 0.21f, 1f);
            _gamePanel.localScale = new Vector3(baseScale.x, 0.001f, 1f);
            _gamePanel.localPosition = new Vector3(0f, 0.30f, 0.024f);
            Tweener.Play(_gamePanel, 0.36f, t =>
            {
                float e = Ease.Evaluate(EaseType.OutCubic, t);
                _gamePanel.localScale = new Vector3(baseScale.x, Mathf.Lerp(0.001f, baseScale.y, e), 1f);
                _gamePanel.localPosition = new Vector3(0f, Mathf.Lerp(0.286f, 0.30f, e), 0.024f);
            }, EaseType.Linear);

            _familyTitle.SetAlpha(0f);
            Tweener.Play(_familyTitle, 0.45f, t => _familyTitle.SetAlpha(t), EaseType.OutCubic, 0.08f);

            if (_gameButtons.TryGetValue(OpenFamily.Value, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var b = list[i];
                    Tweener.Cancel(b);
                    b.SetScaleMultiplier(0.01f);
                    float delay = 0.05f + i * 0.028f;
                    Tweener.Play(b, 0.34f,
                        t => b.SetScaleMultiplier(Mathf.Lerp(0.01f, 1f, Ease.Evaluate(EaseType.OutBack, t))),
                        EaseType.Linear, delay);
                }
            }

            Tweener.Cancel(_backButton);
            _backButton.SetScaleMultiplier(0.01f);
            Tweener.Play(_backButton, 0.30f,
                t => _backButton.SetScaleMultiplier(Mathf.Lerp(0.01f, 1f, Ease.Evaluate(EaseType.OutBack, t))),
                EaseType.Linear, 0.22f);
        }

        public void PlayEntrance()
        {
            if (!gameObject.activeInHierarchy) return;
            Tweener.Cancel(this);
            transform.localScale = Vector3.one * 0.90f;
            Tweener.Play(this, 0.48f, t =>
            {
                transform.localScale = Vector3.one * Mathf.Lerp(0.90f, 1f, Ease.Evaluate(EaseType.OutBack, t));
            }, EaseType.Linear);

            var labels = new[] { _header, _hint, _shapeStatus, _objectStatus, _candidate, _presetTitle, _quickStatus };
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == null) continue;
                labels[i].SetAlpha(0f);
                float delay = 0.05f + i * 0.045f;
                var label = labels[i];
                Tweener.Play(label, 0.42f, t => label.SetAlpha(t), EaseType.OutCubic, delay);
            }

            int index = 0;
            for (int i = 0; i < _familyButtons.Count; i++)
            {
                AnimateButtonIn(_familyButtons[i], 0.08f + index++ * 0.045f);
            }
            for (int i = 0; i < _audioButtons.Count; i++)
            {
                AnimateButtonIn(_audioButtons[i], 0.12f + index++ * 0.045f);
            }
            for (int i = 0; i < _presetButtons.Count; i++)
            {
                AnimateButtonIn(_presetButtons[i], 0.16f + index++ * 0.035f);
            }
            if (OpenFamily.HasValue) AnimateFamilyPanel(true);
        }

        private static void AnimateButtonIn(WorldButton button, float delay)
        {
            if (button == null) return;
            Tweener.Cancel(button);
            button.SetScaleMultiplier(0.01f);
            Tweener.Play(button, 0.38f,
                t => button.SetScaleMultiplier(Mathf.Lerp(0.01f, 1f, Ease.Evaluate(EaseType.OutBack, t))),
                EaseType.Linear, delay);
        }

        private void ToggleFamily(GameFamily family)
        {
            bool same = OpenFamily.HasValue && OpenFamily.Value == family;
            ShowFamily(same ? (GameFamily?)null : family);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (visible)
            {
                RefreshAudioLabels();
                PlayEntrance();
            }
            else
            {
                Tweener.Cancel(this);
                Tweener.Cancel(_gamePanel);
            }
        }

        public void SetHeader(string text) => _header.SetText(text);
        public void SetHint(string text) => _hint.SetText(text);
        public void SetShapeStatus(string text, Color color)
        {
            _shapeStatus.SetText(text);
            _shapeStatus.SetColor(color);
        }
        public void SetObjectStatus(string text, Color color)
        {
            _objectStatus.SetText(text);
            _objectStatus.SetColor(color);
        }
        public void SetCandidate(string text, bool valid)
        {
            _candidate.SetText(text);
            _candidate.SetColor(valid ? new Color(1f, 0.85f, 0.4f) : new Color(0.65f, 0.7f, 0.78f));
        }
        public void SetQuickStatus(string text) => _quickStatus.SetText(text);

        private void Update()
        {
            if (Hub == null) return;

            if (Hub.InputBlocked)
            {
                for (int i = 0; i < _familyButtons.Count; i++) _familyButtons[i]?.SetHover(false);
                for (int i = 0; i < _presetButtons.Count; i++) _presetButtons[i]?.SetHover(false);
                for (int i = 0; i < _audioButtons.Count; i++) _audioButtons[i]?.SetHover(false);
                if (_backButton != null) _backButton.SetHover(false);
                HasHover = false;
                ConsumedSelect = false;
                return;
            }

            WorldButton best = null;
            float bestDist = float.MaxValue;
            var all = _rayButtons;
            all.Clear();
            foreach (var b in _familyButtons) if (b != null && b.gameObject.activeInHierarchy) all.Add(b);
            foreach (var kv in _gameButtons)
                if (kv.Key == OpenFamily)
                    foreach (var b in kv.Value) if (b != null) all.Add(b);
            foreach (var b in _presetButtons) if (b != null) all.Add(b);
            foreach (var b in _audioButtons) if (b != null) all.Add(b);
            if (_backButton != null && _backButton.gameObject.activeInHierarchy) all.Add(_backButton);

            for (int i = 0; i < all.Count; i++)
            {
                var b = all[i];
                b.SetHover(false);
                if (BestRay(b, out float d) && d < bestDist)
                {
                    bestDist = d;
                    best = b;
                }
            }

            HasHover = best != null;
            if (best != null) best.SetHover(true);

            ConsumedSelect = false;
            if (best != null && Hub.Input != null && Hub.Input.SelectDown && !_selectLatch)
            {
                _selectLatch = true;
                best.Press();
                ConsumedSelect = true;
            }
            if (Hub.Input == null || !Hub.Input.SelectDown) _selectLatch = false;
        }

        private void OnDestroy()
        {
            Tweener.Cancel(this);
            Tweener.Cancel(_gamePanel);
        }

        private bool BestRay(WorldButton b, out float distance)
        {
            distance = float.MaxValue;
            bool hit = false;
            var left = Hub.Input != null ? Hub.Input.Left.PointerRay : new Ray();
            var right = Hub.Input != null ? Hub.Input.Right.PointerRay : new Ray();
            if (b.Raycast(left, out float dl)) { distance = dl; hit = true; }
            if (b.Raycast(right, out float dr) && dr < distance) { distance = dr; hit = true; }
            return hit;
        }
    }
}




