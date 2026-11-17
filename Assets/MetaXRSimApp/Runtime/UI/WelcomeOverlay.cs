using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Polished boot card: branded title, family glyphs, enter action and audio
    /// toggles. It blocks hub gestures until dismissed so the first select press
    /// never starts a stray shape trace.
    /// </summary>
    public sealed class WelcomeOverlay : MonoBehaviour
    {
        public ShapeForgeHub Hub;

        private HandInputService _input;
        private Transform _panel;
        private Vector3 _baseScale = Vector3.one;
        private WorldButton _enter;
        private WorldButton _music;
        private WorldButton _sfx;
        private bool _built;
        private bool _closing;
        private bool _selectLatch;

        public void Begin(ShapeForgeHub hub, bool skip)
        {
            Hub = hub;
            if (skip || hub == null)
            {
                Destroy(gameObject);
                return;
            }

            _input = hub.Input;
            Hub.InputBlocked = true;
            Build();
        }

        private void Build()
        {
            var camera = _input != null ? _input.HeadCamera : Camera.main;
            if (camera != null)
            {
                Vector3 forward = camera.transform.forward;
                transform.position = camera.transform.position + forward * 0.56f - camera.transform.up * 0.02f;
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
            else
            {
                transform.position = new Vector3(0f, 1.35f, 0.55f);
                transform.rotation = Quaternion.identity;
            }

            _panel = new GameObject("WelcomePanel").transform;
            _panel.SetParent(transform, false);
            _panel.localPosition = new Vector3(0f, 0f, 0f);

            var shadow = Quad("WelcomeShadow", _panel, new Vector3(0f, 0f, 0.012f),
                new Vector2(0.70f, 0.46f), MaterialLibrary.Transparent(new Color(0.01f, 0.02f, 0.035f), 0.92f));
            shadow.name = "WelcomeShadow";

            var edge = Quad("WelcomeEdge", _panel, new Vector3(0f, 0f, 0.009f),
                new Vector2(0.66f, 0.42f), MaterialLibrary.Transparent(new Color(0.18f, 0.86f, 1f), 0.42f));
            edge.name = "WelcomeEdge";

            Quad("WelcomeFace", _panel, new Vector3(0f, 0f, 0.008f),
                new Vector2(0.64f, 0.40f), MaterialLibrary.Transparent(new Color(0.025f, 0.04f, 0.06f), 0.94f));

            WorldLabel.Create("WelcomeTitle", _panel, "SHAPEFORGE", 0.034f,
                new Color(0.78f, 0.97f, 1f), new Vector3(0f, 0.128f, -0.004f),
                Quaternion.Euler(0f, 180f, 0f));
            WorldLabel.Create("WelcomeTag", _panel, "FIFTY SPATIAL GAMES  /  ONE GESTURE", 0.011f,
                new Color(0.45f, 0.85f, 0.95f), new Vector3(0f, 0.090f, -0.004f),
                Quaternion.Euler(0f, 180f, 0f));

            var rule = Quad("WelcomeRule", _panel, new Vector3(0f, 0.066f, -0.002f),
                new Vector2(0.44f, 0.0015f), MaterialLibrary.Glow(new Color(0.25f, 0.9f, 1f), 1.8f));
            rule.name = "WelcomeRule";

            Color[] familyColors =
            {
                GameCatalog.FamilyColor(GameFamily.SpherePinch),
                GameCatalog.FamilyColor(GameFamily.SpherePalm),
                GameCatalog.FamilyColor(GameFamily.Spheroid),
                GameCatalog.FamilyColor(GameFamily.HemispherePinch),
                GameCatalog.FamilyColor(GameFamily.HemisphereHand)
            };
            GameFamily[] families =
            {
                GameFamily.SpherePinch, GameFamily.SpherePalm, GameFamily.Spheroid,
                GameFamily.HemispherePinch, GameFamily.HemisphereHand
            };
            for (int i = 0; i < families.Length; i++)
            {
                HologramIcon.Create(_panel, "WelcomeFamily" + i,
                    new Vector3(-0.20f + i * 0.10f, 0.024f, -0.002f),
                    ShapeKindExtensions.FromFamily(families[i]), familyColors[i], 0.034f);
            }

            WorldLabel.Create("WelcomePrompt", _panel,
                "Pinch a shape in the air, or place a real object on the table.", 0.0095f,
                new Color(0.75f, 0.84f, 0.92f), new Vector3(0f, -0.020f, -0.004f),
                Quaternion.Euler(0f, 180f, 0f));

            _enter = WorldButton.Create("WelcomeEnter", _panel, "ENTER THE FORGE",
                new Vector3(0f, -0.064f, -0.004f), Quaternion.identity,
                new Vector2(0.28f, 0.046f), new Color32(24, 78, 66, 255),
                new Color32(104, 255, 198, 255), 0.013f);
            _enter.Clicked = Dismiss;

            _music = WorldButton.Create("WelcomeMusic", _panel,
                MusicDirector.Instance != null && MusicDirector.Instance.Enabled ? "MUSIC  ON" : "MUSIC  OFF",
                new Vector3(-0.115f, -0.124f, -0.004f), Quaternion.identity,
                new Vector2(0.18f, 0.028f), new Color32(34, 44, 60, 255),
                new Color32(120, 210, 255, 255), 0.008f);
            _music.Clicked = ToggleMusic;

            _sfx = WorldButton.Create("WelcomeSfx", _panel,
                AudioDirector.Instance != null && AudioDirector.Instance.Enabled ? "SOUND  ON" : "SOUND  OFF",
                new Vector3(0.115f, -0.124f, -0.004f), Quaternion.identity,
                new Vector2(0.18f, 0.028f), new Color32(34, 44, 60, 255),
                new Color32(255, 204, 120, 255), 0.008f);
            _sfx.Clicked = ToggleSfx;

            WorldLabel.Create("WelcomeFooter", _panel,
                "CC0 soundtrack  /  passthrough  /  hand tracking  /  Meta Quest 2 - 3S", 0.0065f,
                new Color(0.50f, 0.62f, 0.72f), new Vector3(0f, -0.170f, -0.004f),
                Quaternion.Euler(0f, 180f, 0f));

            _baseScale = _panel.localScale;
            _panel.localScale = _baseScale * 0.82f;
            _built = true;
            Tweener.Play(this, 0.55f, t =>
            {
                if (_panel != null)
                    _panel.localScale = _baseScale * Mathf.Lerp(0.82f, 1f, Ease.Evaluate(EaseType.OutBack, t));
            }, EaseType.Linear, 0.05f);
            AudioDirector.Instance?.Chime();
        }

        private static GameObject Quad(string name, Transform parent, Vector3 localPosition,
            Vector2 size, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Quad;
            go.AddComponent<MeshRenderer>().material = material;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            return go;
        }

        private void ToggleMusic()
        {
            MusicDirector.Instance?.Toggle();
            if (_music != null)
                _music.SetText(MusicDirector.Instance != null && MusicDirector.Instance.Enabled ? "MUSIC  ON" : "MUSIC  OFF");
        }

        private void ToggleSfx()
        {
            AudioDirector.Instance?.Toggle();
            if (_sfx != null)
                _sfx.SetText(AudioDirector.Instance != null && AudioDirector.Instance.Enabled ? "SOUND  ON" : "SOUND  OFF");
        }
        public void DismissNow() => Dismiss();

        private void Dismiss()
        {
            if (_closing) return;
            _closing = true;
            Tweener.Cancel(this);
            AudioDirector.Instance?.Success();
            SparkleBurst.Play(transform, transform.position + transform.forward * 0.09f,
                new Color(0.35f, 1f, 0.85f), 18, 0.22f, 0.65f, 0.006f);
            Tweener.Play(this, 0.34f, t =>
            {
                if (_panel != null)
                    _panel.localScale = _baseScale * Mathf.Lerp(1f, 0.86f, Ease.Evaluate(EaseType.InCubic, t));
            }, EaseType.Linear, 0f, false, () =>
            {
                if (Hub != null)
                {
                    Hub.InputBlocked = false;
                    if (Hub.Menu != null) Hub.Menu.PlayEntrance();
                }
                Destroy(gameObject);
            });
        }

        private void Update()
        {
            if (!_built || _closing) return;

            PlaceInFrontOfPlayer();

            WorldButton best = null;
            float bestDistance = float.MaxValue;
            Consider(_enter, ref best, ref bestDistance);
            Consider(_music, ref best, ref bestDistance);
            Consider(_sfx, ref best, ref bestDistance);
            if (best != null) best.SetHover(true);

            if (best != null && _input != null && _input.SelectDown && !_selectLatch)
            {
                _selectLatch = true;
                best.Press();
            }
            if (_input == null || !_input.SelectDown) _selectLatch = false;

            if (_input != null && _input.BackDown) Dismiss();
        }

        private void PlaceInFrontOfPlayer()
        {
            var camera = _input != null ? _input.HeadCamera : Camera.main;
            if (camera == null) return;
            Vector3 target = camera.transform.position + camera.transform.forward * 0.56f - camera.transform.up * 0.02f;
            transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-14f * Time.deltaTime));
            Quaternion look = Quaternion.LookRotation(camera.transform.forward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-14f * Time.deltaTime));
        }

        private void OnDestroy()
        {
            Tweener.Cancel(this);
        }

        private void Consider(WorldButton button, ref WorldButton best, ref float bestDistance)
        {
            if (button == null) return;
            button.SetHover(false);
            bool hit = false;
            float distance = float.MaxValue;
            if (_input != null)
            {
                if (button.Raycast(_input.Left.PointerRay, out float dl)) { distance = dl; hit = true; }
                if (button.Raycast(_input.Right.PointerRay, out float dr) && dr < distance) { distance = dr; hit = true; }
            }
            if (hit && distance < bestDistance)
            {
                bestDistance = distance;
                best = button;
            }
        }
    }
}

