using System;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Tiny procedural sound kit so the APK ships with feedback without
    /// external audio assets. All clips are generated once at boot.
    /// </summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        [Range(0f, 1f)] public float Volume = 0.65f;
        public bool Enabled => Volume > 0.0001f;

        private AudioSource _source;
        private AudioClip _click;
        private AudioClip _hover;
        private AudioClip _chime;
        private AudioClip _whoosh;
        private AudioClip _error;
        private AudioClip _success;
        private AudioClip _hum;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Volume = PlayerPrefs.GetFloat("ShapeForge.SfxVolume", 0.65f);
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;

            _click = MakeTone("click", 880f, 0.055f, 0.35f, Wave.Sine);
            _hover = MakeTone("hover", 1320f, 0.025f, 0.10f, Wave.Sine);
            _chime = MakeChime("chime", new[] { 880f, 1320f, 1760f }, 0.22f);
            _whoosh = MakeSweep("whoosh", 90f, 900f, 0.55f, 0.25f);
            _error = MakeTone("error", 150f, 0.18f, 0.5f, Wave.Square);
            _success = MakeChime("success", new[] { 523f, 659f, 784f, 1046f }, 0.38f);
            _hum = MakeSweep("hum", 60f, 180f, 1.4f, 0.14f);
        }

        private enum Wave { Sine, Square, Noise }

        private static AudioClip MakeTone(string name, float freq, float seconds, float amp, Wave wave)
        {
            int rate = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(seconds * rate));
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, i / (rate * 0.01f)) * Mathf.Pow(1f - i / (float)count, 2f);
                float v;
                switch (wave)
                {
                    case Wave.Square: v = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)); break;
                    case Wave.Noise: v = UnityEngine.Random.Range(-1f, 1f); break;
                    default: v = Mathf.Sin(2f * Mathf.PI * freq * t); break;
                }
                data[i] = v * env * amp;
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeChime(string name, float[] freqs, float seconds)
        {
            int rate = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(seconds * rate));
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Exp(-t * 7f) * Mathf.Min(1f, t * 120f);
                float v = 0f;
                for (int f = 0; f < freqs.Length; f++)
                    v += Mathf.Sin(2f * Mathf.PI * freqs[f] * t) / (f + 1);
                data[i] = v * env * 0.16f;
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeSweep(string name, float f0, float f1, float seconds, float amp)
        {
            int rate = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(seconds * rate));
            var data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float freq = Mathf.Lerp(f0, f1, t);
                phase += 2f * Mathf.PI * freq / rate;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(phase) * env * amp;
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void Play(AudioClip clip)
        {
            if (_source != null && clip != null && Enabled) _source.PlayOneShot(clip, Mathf.Clamp01(Volume));
        }

        public void SetEnabled(bool enabled)
        {
            Volume = enabled ? 0.65f : 0f;
            PlayerPrefs.SetFloat("ShapeForge.SfxVolume", Volume);
            PlayerPrefs.Save();
        }

        public void Toggle() => SetEnabled(!Enabled);

        public void Click() => Play(_click);
        public void Hover() => Play(_hover);
        public void Chime() => Play(_chime);
        public void Whoosh() => Play(_whoosh);
        public void Error() => Play(_error);
        public void Success() => Play(_success);
        public void Hum() => Play(_hum);
    }
}
