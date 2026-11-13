using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.TardigradePond
{
    public class PondRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform BowlTransform;
        public Transform WaterSurfaceTransform;

        [Header("State")]
        public WaveField Wave = new WaveField();
        public List<Creature> Creatures = new List<Creature>();
        public List<Mine> Mines = new List<Mine>();

        public int Score = 0;
        public float DropletTimerSec = 42.0f;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            Wave.StepDamping(dt);
            DropletTimerSec = Mathf.Max(0f, DropletTimerSec - dt);
        }
    }
}
