using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector
{
    public class ContainmentDirector
    {
        public int Charges { get; private set; } = 10;
        public int CollectedCount { get; private set; }
        public bool IsSurging { get; private set; }
        private float _surgeTimer;

        public bool TryAbsorb(NeutronAgent n, float distFromCenterMm)
        {
            if (n == null || n.IsAbsorbed || n.IsCollected) return false;

            if (distFromCenterMm <= ElectronDeflectorConfig.CoreAbsorbRadiusMm)
            {
                n.IsAbsorbed = true;
                int cost = n.IsHeavy ? 2 : 1;
                Charges = Mathf.Max(0, Charges - cost);
                return true;
            }
            return false;
        }

        public void RecordCollection()
        {
            CollectedCount++;
        }

        public void TriggerSurge()
        {
            IsSurging = true;
            _surgeTimer = ElectronDeflectorConfig.SurgeDurationSec;
        }

        public void Step(float dt)
        {
            if (IsSurging)
            {
                _surgeTimer -= dt;
                if (_surgeTimer <= 0f) IsSurging = false;
            }
        }
    }
}
