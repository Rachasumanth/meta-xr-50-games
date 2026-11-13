using System;
using UnityEngine;

namespace XRGames.Chapter4.LimpetSuction
{
    public class LimpetRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform ShellTransform;
        public Transform ReefTransform;

        [Header("State")]
        public SealDetector Seal = new SealDetector();

        public int Integrity = 3;
        public int DislodgedCount = 0;
        public int ConsumedAlgaeCells = 0;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;
        }
    }
}
