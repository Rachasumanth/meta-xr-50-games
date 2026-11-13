using System;
using UnityEngine;

namespace XRGames.Chapter4.MicroMortarAlchemist
{
    public class MortarRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform MortarTransform;
        public Transform PestleTransform;
        public Transform GolemTransform;

        [Header("State")]
        public Shard CurrentShard = new Shard();

        public float TotalDosePoured = 0f;
        public int ElixirsBrewed = 0;
        public int SpoilsCount = 0;
        public float TiltDeg = 0f;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            float flow = PourController.ComputeFlowRateGPerS(TiltDeg);
            TotalDosePoured += flow * dt;
        }
    }
}
