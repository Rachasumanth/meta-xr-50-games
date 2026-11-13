using UnityEngine;

namespace XRGames.Chapter4.PulseCoreReactor
{
    public class DeformationModel
    {
        public const float HertzK = 0.5677f;

        public float ComputeForce(float deltaMm)
        {
            return HertzK * Mathf.Pow(Mathf.Max(0f, deltaMm), 1.5f);
        }
    }
}
