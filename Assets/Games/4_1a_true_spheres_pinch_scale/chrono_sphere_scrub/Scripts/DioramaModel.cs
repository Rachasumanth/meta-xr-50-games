using UnityEngine;

namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class DioramaModel
    {
        public Vector3[] SampleFragments(float tMs)
        {
            Vector3[] poses = new Vector3[12];
            for (int i = 0; i < 12; i++)
            {
                float phase = (tMs * 0.001f) + (i * 0.5f);
                poses[i] = new Vector3(
                    Mathf.Sin(phase) * 50f,
                    Mathf.Cos(phase) * 50f,
                    i * 10f
                );
            }
            return poses;
        }
    }
}
