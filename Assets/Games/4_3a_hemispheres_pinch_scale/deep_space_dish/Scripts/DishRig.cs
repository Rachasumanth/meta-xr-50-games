using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.DeepSpaceDish
{
    public class DishRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform DishTransform;
        public Transform FeedHornTransform;

        [Header("State")]
        public AimFilter Aim = new AimFilter();
        public List<Satellite> Satellites = new List<Satellite>();

        public int ShatteredCount = 0;
        public int EscapedCount = 0;
        public int Score = 0;
        public int Chain = 0;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            for (int i = 0; i < Satellites.Count; i++)
            {
                Satellites[i].StepEscape(false, dt);
            }
        }
    }
}
