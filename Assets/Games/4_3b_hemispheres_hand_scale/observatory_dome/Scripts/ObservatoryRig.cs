using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ObservatoryDome
{
    public class ObservatoryRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform DomeTransform;
        public Transform ShutterLeft;
        public Transform ShutterRight;

        [Header("State")]
        public List<Target> Targets = new List<Target>();

        public int TargetsCaptured = 0;
        public int TargetsLost = 0;
        public int Score = 0;
        public float DomeYawDeg = 0f;
        public float SlitWidthDeg = 20.0f;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            for (int i = 0; i < Targets.Count; i++)
            {
                Targets[i].Step(DomeYawDeg, SlitWidthDeg, 0f, dt);
            }
        }
    }
}
