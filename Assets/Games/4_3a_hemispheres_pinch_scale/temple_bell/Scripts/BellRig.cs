using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.TempleBell
{
    public class BellRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform BellTransform;
        public Transform CourtyardTransform;

        [Header("State")]
        public PendulumDynamics Pendulum = new PendulumDynamics();
        public List<AcousticRing> ActiveRings = new List<AcousticRing>();

        public int Score = 0;
        public int FallsCount = 0;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            Pendulum.Step(dt);
            for (int i = 0; i < ActiveRings.Count; i++)
            {
                ActiveRings[i].Step(dt);
            }
        }
    }
}
