using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ParatrooperBasket
{
    public class BasketRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform CapTransform;
        public Transform NetTransform;

        [Header("State")]
        public List<Trooper> Troopers = new List<Trooper>();

        public int TroopersRecovered = 0;
        public int TroopersLost = 0;
        public int Score = 0;
        public float TiltDeg = 0f;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            float rEff = NetCollider.ComputeEffectiveRadius(TiltDeg);
            Vector2 capPos = new Vector2(CapTransform != null ? CapTransform.localPosition.x * 1000f : 0f, CapTransform != null ? CapTransform.localPosition.y * 1000f : 0f);

            for (int i = 0; i < Troopers.Count; i++)
            {
                Troopers[i].Step(dt);
                if (!Troopers[i].IsCaught)
                {
                    if (Troopers[i].EvaluateLanding(capPos, rEff, out float mult))
                    {
                        Score += (int)(100 * mult);
                    }
                    else if (Troopers[i].IsMissed)
                    {
                        TroopersLost++;
                    }
                }
            }
        }
    }
}
