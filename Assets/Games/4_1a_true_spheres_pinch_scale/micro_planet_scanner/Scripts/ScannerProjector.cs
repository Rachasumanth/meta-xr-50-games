using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class ScannerProjector
    {
        public float LatitudeDeg { get; private set; }
        public float LongitudeDeg { get; private set; }
        public Vector3 AimDirection { get; private set; } = Vector3.forward;

        public void ProjectTip(Vector3 tipDirectionInP)
        {
            if (tipDirectionInP.sqrMagnitude < 0.0001f) return;

            Vector3 u = tipDirectionInP.normalized;
            AimDirection = u;

            // z is sin(phi)
            float sinPhi = Mathf.Clamp(u.z, -1f, 1f);
            LatitudeDeg = Mathf.Asin(sinPhi) * Mathf.Rad2Deg;

            // x, y give longitude
            float lambda = Mathf.Atan2(u.y, u.x) * Mathf.Rad2Deg;
            if (lambda < 0f) lambda += 360f;
            LongitudeDeg = Mathf.Repeat(lambda, 360f);
        }
    }
}
