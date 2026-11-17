using System.Collections;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Two-source CC0 soundtrack director with interrupted crossfades. Music is
    /// streamed from Resources so the APK stays compact and playback starts
    /// without a long decompression hitch. All tracks loop seamlessly.
    /// </summary>
    public sealed class MusicDirector : MonoBehaviour
    {
        public static MusicDirector Instance { get; private set; }

        public const string HubTrack = "ShapeForgeMusic/hub_observing_the_star";
        public const string SphereArcadeTrack = "ShapeForgeMusic/arcade_fight_loop";
        public const string SpheroidTrack = "ShapeForgeMusic/curious_i_swear";
        public const string DomeTrack = "ShapeForgeMusic/mystic_enigmatic";

        public bool Enabled { get; private set; } = true;
        public float Volume { get; private set; } = 0.32f;
        public string CurrentTrack { get; private set; }

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private AudioSource _from;
        private AudioSource _to;
        private AudioSource _active;
        private Coroutine _fade;
        private string _lastRequested;
        private float _baseVolume;
        private bool _missingLogged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _baseVolume = PlayerPrefs.GetFloat("ShapeForge.MusicVolume", 0.32f);
            Enabled = _baseVolume > 0.001f;
            Volume = _baseVolume;

            _sourceA = CreateSource("MusicA");
            _sourceB = CreateSource("MusicB");
            _active = _sourceA;
        }

        private AudioSource CreateSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = 0f;
            src.priority = 64;
            src.ignoreListenerPause = true;
            return src;
        }

        private void Start()
        {
            PlayHub(0.8f);
        }

        public void PlayHub(float fade = 1.0f) => Request(HubTrack, fade);

        public void PlayFamily(GameFamily family, float fade = 0.8f)
        {
            switch (family)
            {
                case GameFamily.SpherePinch:
                case GameFamily.HemisphereHand:
                    Request(SphereArcadeTrack, fade);
                    break;
                case GameFamily.Spheroid:
                    Request(SpheroidTrack, fade);
                    break;
                default:
                    Request(DomeTrack, fade);
                    break;
            }
        }

        public void PlayGame(GameDefinition game, float fade = 0.8f)
        {
            if (game != null) PlayFamily(game.Family, fade);
        }

        private void Request(string resourcePath, float fade)
        {
            if (!Enabled) { _lastRequested = resourcePath; return; }
            if (string.IsNullOrEmpty(resourcePath)) return;
            if (CurrentTrack == resourcePath && _active != null && _active.isPlaying) return;

            _lastRequested = resourcePath;
            var clip = Resources.Load<AudioClip>(resourcePath);
            if (clip == null)
            {
                if (!_missingLogged)
                {
                    _missingLogged = true;
                    Debug.LogWarning("[MusicDirector] Missing audio clip at Resources/" + resourcePath);
                }
                return;
            }

            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(Crossfade(clip, resourcePath, Mathf.Max(0.05f, fade)));
        }

        private IEnumerator Crossfade(AudioClip clip, string resourcePath, float duration)
        {
            _from = _active;
            _to = ReferenceEquals(_active, _sourceA) ? _sourceB : _sourceA;

            _to.Stop();
            _to.clip = clip;
            _to.volume = 0f;
            _to.loop = true;
            _to.Play();
            _active = _to;
            CurrentTrack = resourcePath;

            float startFrom = _from != null ? _from.volume : 0f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                float e = Ease.Evaluate(EaseType.InOutSine, p);
                if (_from != null) _from.volume = Mathf.Lerp(startFrom, 0f, e);
                _to.volume = Mathf.Lerp(0f, Volume, e);
                yield return null;
            }

            if (_from != null)
            {
                _from.Stop();
                _from.clip = null;
                _from.volume = 0f;
            }
            _to.volume = Volume;
            _fade = null;
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            _baseVolume = enabled ? 0.32f : 0f;
            Volume = _baseVolume;
            PlayerPrefs.SetFloat("ShapeForge.MusicVolume", _baseVolume);
            PlayerPrefs.Save();

            if (!enabled)
            {
                if (_fade != null) StopCoroutine(_fade);
                _fade = null;
                if (_active != null) _active.Stop();
                if (_from != null && _from != _active) _from.Stop();
                CurrentTrack = null;
                return;
            }

            string path = !string.IsNullOrEmpty(_lastRequested) ? _lastRequested : HubTrack;
            CurrentTrack = null;
            Request(path, 0.6f);
        }

        public void Toggle() => SetEnabled(!Enabled);

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

