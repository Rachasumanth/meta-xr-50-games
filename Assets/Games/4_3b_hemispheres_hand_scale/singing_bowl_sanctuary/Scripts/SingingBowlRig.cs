using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.SingingBowlSanctuary
{
    public class SingingBowlRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform BowlTransform;
        public Transform DomeTransform;
        public Transform LotusTransform;

        [Header("State")]
        public ResonanceModel Resonance = new ResonanceModel();
        public List<Locust> Locusts = new List<Locust>();

        public int Petals = 6;
        public int Score = 0;
        public float DomeRadiusMm = 90.0f;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;
        }
    }
}
