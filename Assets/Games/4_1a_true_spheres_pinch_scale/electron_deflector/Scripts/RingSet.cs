using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector
{
    public class RingSet
    {
        public bool TryCollect(NeutronAgent n, float radialDistMm)
        {
            if (n == null || n.IsCollected || n.IsAbsorbed) return false;

            if (radialDistMm <= ElectronDeflectorConfig.RingTubeRadiusMm)
            {
                n.IsCollected = true;
                return true;
            }
            return false;
        }
    }
}
