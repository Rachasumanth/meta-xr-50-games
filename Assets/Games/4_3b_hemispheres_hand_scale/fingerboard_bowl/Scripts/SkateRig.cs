using System;
using UnityEngine;

namespace XRGames.Chapter4.FingerboardBowl
{
    public class SkateRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform BowlTransform;
        public Transform SkaterTransform;

        [Header("State")]
        public SkaterParticle Skater = new SkaterParticle();

        public int Score = 0;
        public int BailsCount = 0;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;
        }
    }
}
