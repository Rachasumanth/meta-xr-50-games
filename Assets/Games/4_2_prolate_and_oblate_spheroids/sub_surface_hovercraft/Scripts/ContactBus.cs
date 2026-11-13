using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public struct ContactInfo
    {
        public string JointName;
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 RelativeVelocity;
        public float Age;
        public bool IsValid;
    }

    public class ContactBus
    {
        public const float EnterDistanceMm = 5.0f;
        public const float ExitDistanceMm = 8.0f;
        public const float DebounceTimeSec = 0.030f;

        private float _scrollAccumulatorMm = 0f;
        private float _lastScrollTime = -1f;
        private float _lastClickTime = -1f;

        public event Action<int> OnDepthScrolled; // +1 or -1
        public event Action OnLeftClicked;        // Fire
        public event Action OnRightClicked;       // Drift

        public float ScrollAccumulator => _scrollAccumulatorMm;

        public bool ProcessScroll(float deltaMm, float dt, float currentTime, out int depthChange)
        {
            depthChange = 0;
            // 40 ms debounce
            if (_lastScrollTime >= 0f && (currentTime - _lastScrollTime) < 0.040f)
            {
                return false;
            }

            _scrollAccumulatorMm += deltaMm;
            if (Mathf.Abs(_scrollAccumulatorMm) >= 3.0f)
            {
                depthChange = _scrollAccumulatorMm > 0 ? 1 : -1;
                _scrollAccumulatorMm = 0f;
                _lastScrollTime = currentTime;
                OnDepthScrolled?.Invoke(depthChange);
                return true;
            }
            return false;
        }

        public void TriggerLeftClick()
        {
            OnLeftClicked?.Invoke();
        }

        public void TriggerRightClick()
        {
            OnRightClicked?.Invoke();
        }
    }
}
