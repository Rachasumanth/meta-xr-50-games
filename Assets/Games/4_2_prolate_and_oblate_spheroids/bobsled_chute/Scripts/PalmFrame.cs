using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public struct PalmAxes
    {
        public Vector3 XHat;
        public Vector3 YHat;
        public Vector3 ZHat;
        public Vector3 Centre;

        public PalmAxes(Vector3 x, Vector3 y, Vector3 z, Vector3 c)
        {
            XHat = x;
            YHat = y;
            ZHat = z;
            Centre = c;
        }
    }

    public class PalmFrame
    {
        public static PalmAxes Build(Vector3 lWrist, Vector3 lMiddleMcp, Vector3 lIndexMcp, Vector3 lPinkyMcp)
        {
            Vector3 xHat = (lMiddleMcp - lWrist).normalized;
            Vector3 v1 = lIndexMcp - lWrist;
            Vector3 v2 = lPinkyMcp - lWrist;
            Vector3 zHat = Vector3.Cross(v1, v2).normalized;
            Vector3 yHat = Vector3.Cross(zHat, xHat).normalized;
            Vector3 centre = (lWrist + lMiddleMcp + lIndexMcp + lPinkyMcp) * 0.25f;

            return new PalmAxes(xHat, yHat, zHat, centre);
        }
    }
}
