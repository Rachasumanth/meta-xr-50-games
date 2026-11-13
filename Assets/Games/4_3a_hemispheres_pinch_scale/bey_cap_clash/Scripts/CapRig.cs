using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.BeyCapClash
{
    public class CapRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform CapTransform;
        public Transform StadiumTransform;

        [Header("State")]
        public TopEntity PlayerTop = new TopEntity();
        public List<TopEntity> PhantomTops = new List<TopEntity>();

        public int MatchesWon = 0;
        public int MatchesLost = 0;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            PlayerTop.Step(dt);
            for (int i = 0; i < PhantomTops.Count; i++)
            {
                PhantomTops[i].Step(dt);
                ClashResolver.ResolveClash(PlayerTop, PhantomTops[i]);
            }
        }
    }
}
