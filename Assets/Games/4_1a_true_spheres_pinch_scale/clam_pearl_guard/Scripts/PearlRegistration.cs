using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class PearlRegistration
    {
        private Vector3 _offset = Vector3.zero;
        private float _currentRadius = ClamPearlGuardConfig.PearlRadiusTarget;
        private float _rmsError = 0.1f;
        private Vector3 _initialPosition;
        private bool _isDropped;

        public Vector3 Offset => _offset;
        public float CurrentRadius => _currentRadius;
        public float RmsError => _rmsError;
        public bool IsDropped => _isDropped;

        // Sphere fit (spec item 1): r in [8.38, 8.62] mm, RMS < 0.35 mm.
        public bool FitSphere(Vector3[] samplePoints, float sampleRadius)
        {
            if (samplePoints == null || samplePoints.Length == 0) return false;

            Vector3 sum = Vector3.zero;
            foreach (var pt in samplePoints) sum += pt;
            Vector3 center = sum / samplePoints.Length;

            float sqrSum = 0f;
            foreach (var pt in samplePoints)
            {
                float dist = Vector3.Distance(pt, center);
                float dev = Mathf.Abs(dist - sampleRadius);
                sqrSum += dev * dev;
            }

            _rmsError = Mathf.Sqrt(sqrSum / samplePoints.Length);
            _currentRadius = sampleRadius;

            return _rmsError <= ClamPearlGuardConfig.SphereFitRmsMax &&
                   _currentRadius >= ClamPearlGuardConfig.PearlRadiusMin &&
                   _currentRadius <= ClamPearlGuardConfig.PearlRadiusMax;
        }

        public void UpdateOffset(Vector3 newOffset)
        {
            _offset = newOffset;
        }

        // Shift / drop flow (spec item 11): 7 mm shift re-solves the registration
        // (no drop); at 12 mm the drop flow starts.
        public void CheckShiftAndDrop(Vector3 currentSpherePos, float deltaTime)
        {
            float displacement = Vector3.Distance(_initialPosition, currentSpherePos);
            if (displacement >= ClamPearlGuardConfig.DropThresholdMm)
            {
                _isDropped = true;
            }
            else if (displacement > ClamPearlGuardConfig.ShiftToleranceMm)
            {
                // Small shift within tolerance band: re-solve offset, keep tracking.
                _offset = currentSpherePos - _initialPosition;
            }
        }

        public void ResetDrop()
        {
            _isDropped = false;
            _initialPosition = Vector3.zero;
        }

        public void SetInitialPosition(Vector3 pos)
        {
            _initialPosition = pos;
            _isDropped = false;
        }
    }
}
