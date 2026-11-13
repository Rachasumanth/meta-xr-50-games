using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class PalmFrame
    {
        private Vector3 _palmCenter;
        private Quaternion _palmRotation = Quaternion.identity;
        private float _confidence;
        private bool _isDropped;

        public Vector3 PalmCenter => _palmCenter;
        public Quaternion PalmRotation => _palmRotation;
        public float Confidence => _confidence;
        public bool IsDropped => _isDropped;

        public void UpdatePalm(Vector3 wrist, Vector3 indexMcp, Vector3 middleMcp, Vector3 pinkyMcp, float rawConfidence, float deltaTime)
        {
            _confidence = Mathf.Clamp01(Mathf.Lerp(_confidence, rawConfidence, deltaTime * 20f));
            
            // Compute approximate center of MCPs
            Vector3 mcpCenter = (indexMcp + middleMcp + pinkyMcp) / 3f;
            Vector3 targetCenter = Vector3.Lerp(wrist, mcpCenter, 0.5f);

            if (deltaTime > 0f)
            {
                _palmCenter = Vector3.Lerp(_palmCenter, targetCenter, deltaTime * 25f);
            }
            else
            {
                _palmCenter = targetCenter;
            }

            // Construct palm orientation frame
            Vector3 forward = (middleMcp - wrist).normalized;
            Vector3 right = (indexMcp - pinkyMcp).normalized;
            if (forward != Vector3.zero && right != Vector3.zero)
            {
                Vector3 upward = Vector3.Cross(forward, right).normalized;
                right = Vector3.Cross(upward, forward).normalized;
                if (forward != Vector3.zero && upward != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(forward, upward);
                    _palmRotation = Quaternion.Slerp(_palmRotation, targetRot, deltaTime > 0f ? deltaTime * 25f : 1f);
                }
            }

            if (_confidence < 0.2f)
            {
                _isDropped = true;
            }
        }

        public void ResetDrop()
        {
            _isDropped = false;
            _confidence = 1f;
        }
    }
}
