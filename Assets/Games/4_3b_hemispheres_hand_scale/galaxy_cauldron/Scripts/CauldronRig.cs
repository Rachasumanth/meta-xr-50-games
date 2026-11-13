using System;
using UnityEngine;

namespace XRGames.Chapter4.GalaxyCauldron
{
    public class CauldronRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform BowlTransform;
        public Transform NebulaTransform;

        [Header("State")]
        public StirAnalyzer Stir = new StirAnalyzer();

        public int Luminosity = 100;
        public int StarsCreated = 0;
        public int Score = 0;
        public float GalaxyTimerSec = 90.0f;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            GalaxyTimerSec = Mathf.Max(0f, GalaxyTimerSec - dt);
        }
    }
}
