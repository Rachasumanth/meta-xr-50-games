using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class CurlEstimator
    {
        private float[] _fistCalib = new float[] { 40f, 40f, 40f, 40f, 40f };
        private float[] _curls = new float[5];
        private float _meanCurl;
        private float _openness = 1.0f;

        public float MeanCurl => _meanCurl;
        public float Openness => _openness;

        public void SetFistCalibration(float[] calib)
        {
            if (calib != null && calib.Length == 5)
            {
                System.Array.Copy(calib, _fistCalib, 5);
            }
        }

        // Raw openness law (spec item 3): O = 1.0 at <= 20 deg mean curl, 0.99 at 21,
        // 0.50 at 70, 0.0 at >= 121. Linear ramp across [OpennessMinCurlDeg, OpennessMaxCurlDeg].
        // No pre-clamping of the input: the clamp01 on the OUTPUT is the law's defined range.
        public static float ComputeOpenness(float meanCurlDeg)
        {
            float span = ClamPearlGuardConfig.OpennessMaxCurlDeg - ClamPearlGuardConfig.OpennessMinCurlDeg;
            float t = (meanCurlDeg - ClamPearlGuardConfig.OpennessMinCurlDeg) / span;
            return Mathf.Clamp01(1.0f - t);
        }

        public void UpdateCurls(float[] rawCurls)
        {
            if (rawCurls == null || rawCurls.Length == 0) return;

            float sum = 0f;
            int count = Mathf.Min(rawCurls.Length, 5);
            for (int i = 0; i < count; i++)
            {
                _curls[i] = rawCurls[i];
                sum += _curls[i];
            }
            _meanCurl = sum / count;

            _openness = ComputeOpenness(_meanCurl);
        }
    }
}
