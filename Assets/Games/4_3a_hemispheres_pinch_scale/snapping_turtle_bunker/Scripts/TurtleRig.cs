using System;
using UnityEngine;

namespace XRGames.Chapter4.SnappingTurtleBunker
{
    public class TurtleRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform ShellTransform;
        public Transform HeadTransform;

        [Header("State")]
        public TiltDynamics Tilt = new TiltDynamics();
        public ApexSealDetector Apex = new ApexSealDetector();
        public ChainTracker Chain = new ChainTracker();

        public int Integrity = 3;
        public int Score = 0;
        public int CurrentTide = 1;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            Chain.Step(dt);

            if (ShellTransform != null)
            {
                ShellTransform.localRotation = Quaternion.Euler(Tilt.CurrentTiltDeg, Tilt.ContactAzimuthDeg, 0f);
            }
        }
    }
}
