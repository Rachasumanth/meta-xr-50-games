using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector
{
    public class SectorMapper
    {
        private readonly float[] _sectorReady = new float[12];
        private readonly List<float> _recentTapTimes = new List<float>();
        private float _fingerHeatLockUntil;

        public bool IsLatitudeValid(float phiDeg)
        {
            return Mathf.Abs(phiDeg) <= ElectronDeflectorConfig.PoleLatitudeMaxDeg;
        }

        public int MapLongitudeToSector(float lambdaDeg, int currentSector = -1)
        {
            float norm = Mathf.Repeat(lambdaDeg, 360f);
            int baseSector = Mathf.FloorToInt(norm / ElectronDeflectorConfig.SectorArcDeg) % 12;

            if (currentSector >= 0 && currentSector < 12)
            {
                // Boundary hysteresis within 3 deg
                float sectorCenter = currentSector * ElectronDeflectorConfig.SectorArcDeg + (ElectronDeflectorConfig.SectorArcDeg * 0.5f);
                float diff = Mathf.Abs(Mathf.DeltaAngle(norm, sectorCenter));
                if (diff <= (ElectronDeflectorConfig.SectorArcDeg * 0.5f) + 3.0f)
                {
                    return currentSector;
                }
            }

            return baseSector;
        }

        public bool TryTap(int sector, float nowSec, bool isSurging)
        {
            if (sector < 0 || sector >= 12) return false;
            if (nowSec < _fingerHeatLockUntil) return false;

            // Check finger heat
            _recentTapTimes.RemoveAll(t => (nowSec - t) > 0.300f);
            if (_recentTapTimes.Count >= 3)
            {
                _fingerHeatLockUntil = nowSec + 0.500f; // 0.5s lockout
                return false;
            }
            _recentTapTimes.Add(nowSec);

            if (nowSec < _sectorReady[sector]) return false;

            float cd = ElectronDeflectorConfig.SectorCooldownSec;
            if (isSurging) cd *= 0.5f;

            _sectorReady[sector] = nowSec + cd;
            return true;
        }

        public void Reset()
        {
            for (int i = 0; i < 12; i++) _sectorReady[i] = 0f;
            _recentTapTimes.Clear();
            _fingerHeatLockUntil = 0f;
        }
    }
}
