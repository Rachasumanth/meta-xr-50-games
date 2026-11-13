using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    // Single definition of the clam shell renderer bookkeeping (drives the rig's
    // visual state; actual mesh work happens at runtime through Meta XR sim).
    public class ClamRenderer
    {
        private float _shellAngle;
        private bool _isOccluded;

        public float ShellAngle => _shellAngle;
        public bool IsOccluded => _isOccluded;

        // Map fist openness [0,1] to shell angle [0, 45] deg.
        public void SetShellOpenness(float openness)
        {
            _shellAngle = Mathf.Lerp(0f, 45f, Mathf.Clamp01(openness));
        }

        public void SetOcclusion(bool occluded)
        {
            _isOccluded = occluded;
        }

        // Smoothed per-frame shell update driven by the rig.
        public void UpdateShell(float openness, float deltaTime)
        {
            float target = Mathf.Lerp(0f, 45f, Mathf.Clamp01(openness));
            _shellAngle = Mathf.MoveTowards(_shellAngle, target, 90f * Mathf.Max(0f, deltaTime));
        }
    }
}
