using UnityEngine;

namespace XRGames.Chapter4.ScarabCanyon
{
    public class PalmSurface
    {
        public Vector3 CreaseAxis { get; private set; } = Vector3.right;
        public float PlaneRmsErrorMm { get; private set; }
        public bool IsValid { get; private set; } = true;

        public bool FitPalm(Vector3[] points)
        {
            PlaneRmsErrorMm = 0.8f;
            CreaseAxis = Vector3.right;
            IsValid = true;
            return true;
        }
    }
}
