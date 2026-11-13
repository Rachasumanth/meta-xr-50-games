using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class GustField
    {
        public Vector3 GetLateralAcceleration(float time)
        {
            float x = Mathf.Sin(time * 2.0f) * 0.5f;
            float z = Mathf.Cos(time * 1.5f) * 0.5f;
            return new Vector3(x, 0f, z);
        }
    }
}
