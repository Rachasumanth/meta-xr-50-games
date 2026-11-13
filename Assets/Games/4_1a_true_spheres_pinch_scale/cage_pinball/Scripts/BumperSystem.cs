using UnityEngine;

namespace XRGames.Chapter4.CagePinball
{
    public class FingertipMap
    {
        public Vector2[] BumperPositions2D { get; } = new Vector2[5];

        public void UpdateFingertip(int fingerIndex, Vector2 pos2D)
        {
            if (fingerIndex >= 0 && fingerIndex < 5)
            {
                BumperPositions2D[fingerIndex] = pos2D;
            }
        }
    }

    public class BumperSystem
    {
        private readonly float[] _lastHitTime = new float[5] { -1f, -1f, -1f, -1f, -1f };
        private int _lastFinger = -1;
        private float _lastComboTime = -1f;

        public int ComboChain { get; private set; }
        public float Multiplier => 1.0f + Mathf.Min(ComboChain, 8) * 0.2f;

        public bool TryHitBumper(int fingerIndex, float approachSpeedMmS, float currentTimeSec, out float impulse)
        {
            impulse = 0f;
            if (fingerIndex < 0 || fingerIndex >= 5) return false;
            if (approachSpeedMmS < CagePinballConfig.BumperMinSpeedMmS) return false;

            if (_lastHitTime[fingerIndex] >= 0f && (currentTimeSec - _lastHitTime[fingerIndex]) < CagePinballConfig.BumperCooldownSec)
            {
                return false; // In cooldown
            }

            _lastHitTime[fingerIndex] = currentTimeSec;
            impulse = approachSpeedMmS * 0.05f;

            if (currentTimeSec - _lastComboTime > CagePinballConfig.ComboTimeoutSec)
            {
                ComboChain = 1;
            }
            else
            {
                if (fingerIndex != _lastFinger)
                {
                    ComboChain = Mathf.Min(ComboChain + 1, 8);
                }
                else
                {
                    ComboChain = 1; // same finger spam keeps chain at 1
                }
            }

            _lastFinger = fingerIndex;
            _lastComboTime = currentTimeSec;
            return true;
        }

        public void Reset()
        {
            for (int i = 0; i < 5; i++) _lastHitTime[i] = -1f;
            _lastFinger = -1;
            _lastComboTime = -1f;
            ComboChain = 0;
        }
    }
}
