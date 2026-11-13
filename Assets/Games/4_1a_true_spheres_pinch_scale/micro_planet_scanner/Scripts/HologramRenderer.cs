using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class HologramRenderer
    {
        public float ScaleFactor => MicroPlanetScannerConfig.MoonScaleFactor;

        public Vector3 TransformPropToVirtual(Vector3 p_P, Vector3 marbleCenter)
        {
            return (p_P - marbleCenter) * ScaleFactor + marbleCenter;
        }

        public Vector3 GetCoreVirtualPosition(Vector3 marbleCenter)
        {
            return marbleCenter;
        }
    }
}
