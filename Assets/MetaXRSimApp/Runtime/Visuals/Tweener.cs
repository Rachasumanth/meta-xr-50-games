using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    public enum EaseType
    {
        Linear,
        InQuad,
        OutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        OutBack,
        OutElastic,
        InOutSine,
        OutBounce
    }

    public static class Ease
    {
        public static float Evaluate(EaseType type, float t)
        {
            t = Mathf.Clamp01(t);
            switch (type)
            {
                case EaseType.Linear: return t;
                case EaseType.InQuad: return t * t;
                case EaseType.OutQuad: return 1f - (1f - t) * (1f - t);
                case EaseType.InCubic: return t * t * t;
                case EaseType.OutCubic: return 1f - Mathf.Pow(1f - t, 3f);
                case EaseType.InOutCubic:
                    return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
                case EaseType.OutBack:
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                case EaseType.OutElastic:
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    const float c4 = (2f * Mathf.PI) / 3f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
                case EaseType.InOutSine:
                    return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
                case EaseType.OutBounce:
                    return OutBounce(t);
                default: return t;
            }
        }

        private static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }
            if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }

    /// <summary>
    /// Tiny allocation-light tween runner used by the hub, HUD and game feedback.
    /// No external DOTween dependency; deterministic and safe to cancel by owner.
    /// </summary>
    public sealed class Tweener : MonoBehaviour
    {
        private sealed class Item
        {
            public object Owner;
            public float Delay;
            public float Duration;
            public float Elapsed;
            public EaseType EaseType;
            public Action<float> Update;
            public Action Complete;
            public bool Loop;
            public bool PingPong;
            public bool Unscaled;
            public bool Done;
            public bool CompleteInvoked;
        }

        private static Tweener _instance;
        private readonly List<Item> _items = new List<Item>(32);

        public static Tweener Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("~Tweener");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _instance = go.AddComponent<Tweener>();
                }
                return _instance;
            }
        }

        public static void Play(object owner, float duration, Action<float> update,
            EaseType ease = EaseType.OutCubic, float delay = 0f, bool loop = false,
            Action complete = null, bool pingPong = false, bool unscaled = false)
        {
            if (update == null) return;
            Instance.AddInternal(new Item
            {
                Owner = owner,
                Delay = Mathf.Max(0f, delay),
                Duration = Mathf.Max(0.0001f, duration),
                EaseType = ease,
                Update = update,
                Complete = complete,
                Loop = loop,
                PingPong = pingPong,
                Unscaled = unscaled
            });
        }

        public static void Cancel(object owner)
        {
            if (_instance == null || owner == null) return;
            var items = _instance._items;
            for (int i = items.Count - 1; i >= 0; i--)
                if (ReferenceEquals(items[i].Owner, owner)) items.RemoveAt(i);
        }

        public static void CancelAll()
        {
            if (_instance != null) _instance._items.Clear();
        }

        private void AddInternal(Item item)
        {
            _items.Add(item);
            if (!enabled) enabled = true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float udt = Time.unscaledDeltaTime;
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                if (item.Done)
                {
                    _items.RemoveAt(i);
                    continue;
                }

                float delta = item.Unscaled ? udt : dt;
                if (item.Delay > 0f)
                {
                    item.Delay -= delta;
                    if (item.Delay > 0f) continue;
                    delta = -item.Delay;
                    item.Delay = 0f;
                }

                item.Elapsed += delta;
                float t = Mathf.Clamp01(item.Elapsed / item.Duration);
                float e = Ease.Evaluate(item.EaseType, item.PingPong && !item.Loop ? PingPong(t) : t);
                try { item.Update(e); } catch (Exception ex) { Debug.LogWarning("[Tweener] " + ex.Message); item.Done = true; }

                if (t >= 1f)
                {
                    if (item.Loop)
                    {
                        item.Elapsed = 0f;
                    }
                    else
                    {
                        if (!item.CompleteInvoked)
                        {
                            item.CompleteInvoked = true;
                            try { item.Complete?.Invoke(); } catch (Exception ex) { Debug.LogWarning("[Tweener] complete: " + ex.Message); }
                        }
                        item.Done = true;
                    }
                }
            }
        }

        private static float PingPong(float t)
        {
            return 1f - Mathf.Abs(2f * t - 1f);
        }
    }
}
