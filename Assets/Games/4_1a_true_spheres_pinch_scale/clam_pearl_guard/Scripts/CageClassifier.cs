using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class CageClassifier
    {
        private bool _wasClosed;
        private float _firstTipCrossTime;
        private float _closureMs;
        private bool _isCageClosed;
        private float _closedTimer;

        public bool IsCageClosed => _isCageClosed;
        public float ClosureMs => _closureMs;
        public bool WasClosed => _wasClosed;

        // Fingertip-to-palm classification with hysteresis (spec item 6):
        // closes when ALL normalized tip distances < CageCloseThresholdMm sustained
        // over CageClosureDebounceMs; releases only when ALL > CageOpenThresholdMm
        // (immediate release - the release has no debounce window).
        // Normalization per spec: d[i] = |tips[i] - H| * 35 / fist_cal[i] in mm.
        public void Evaluate(Vector3[] fingertipPositions, Vector3 palmCenter, float[] fistCalib, float timeNow, float deltaTime)
        {
            if (fingertipPositions == null || fingertipPositions.Length < 5 || fistCalib == null || fistCalib.Length < 5)
                return;

            float maxNormDist = 0f;
            bool allBelowClose = true;
            bool allAboveOpen = true;

            for (int i = 0; i < 5; i++)
            {
                float rawDistMm = Vector3.Distance(fingertipPositions[i], palmCenter) * 1000f;
                float normDist = rawDistMm * 35f / fistCalib[i];
                if (normDist > maxNormDist) maxNormDist = normDist;

                if (normDist >= ClamPearlGuardConfig.CageCloseThresholdMm) allBelowClose = false;
                if (normDist <= ClamPearlGuardConfig.CageOpenThresholdMm) allAboveOpen = false;
            }

            if (allAboveOpen)
            {
                // Release only above the open threshold; immediate (no debounce).
                _wasClosed = false;
                _isCageClosed = false;
                _firstTipCrossTime = 0f;
                _closedTimer = 0f;
                return;
            }

            if (allBelowClose)
            {
                if (_firstTipCrossTime == 0f) _firstTipCrossTime = timeNow;
                _closedTimer += deltaTime * 1000f;
                if (_closedTimer >= ClamPearlGuardConfig.CageClosureDebounceMs)
                {
                    if (!_wasClosed)
                    {
                        float closure = (timeNow - _firstTipCrossTime) * 1000f;
                        _closureMs = Mathf.Max(0f, closure);
                        _wasClosed = true;
                    }
                    _isCageClosed = true;
                }
            }
            else
            {
                // Between thresholds: hold state (hysteresis band 35..42 mm).
                if (!_isCageClosed && maxNormDist < 50f && _firstTipCrossTime == 0f)
                {
                    _firstTipCrossTime = timeNow;
                }
                if (!_isCageClosed)
                {
                    _closedTimer = 0f;
                }
            }
        }

        public void ResetState()
        {
            _wasClosed = false;
            _isCageClosed = false;
            _firstTipCrossTime = 0f;
            _closureMs = 0f;
            _closedTimer = 0f;
        }
    }
}
