using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public class SplitTimer
    {
        public float Gate1Time = 0f; // 300mm
        public float Gate2Time = 0f; // 600mm
        public float Gate3Time = 0f; // 900mm
        public List<float> HeatTimes = new List<float>();

        public float BestHeatTime
        {
            get
            {
                if (HeatTimes.Count == 0) return 0f;
                float best = float.MaxValue;
                foreach (var t in HeatTimes)
                {
                    if (t < best) best = t;
                }
                return best;
            }
        }

        public void LatchGate(int gateIndex, float time)
        {
            if (gateIndex == 1 && Gate1Time == 0f) Gate1Time = time;
            if (gateIndex == 2 && Gate2Time == 0f) Gate2Time = time;
            if (gateIndex == 3 && Gate3Time == 0f) Gate3Time = time;
        }

        public void RecordHeat(float totalTime)
        {
            HeatTimes.Add(totalTime);
            Gate1Time = 0f;
            Gate2Time = 0f;
            Gate3Time = 0f;
        }
    }
}
