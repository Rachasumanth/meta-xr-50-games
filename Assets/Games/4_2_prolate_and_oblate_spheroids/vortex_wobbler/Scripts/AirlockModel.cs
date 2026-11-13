using System;
using UnityEngine;

namespace XRGames.Chapter4.VortexWobbler
{
    public class AirlockModel
    {
        public int HullPips { get; private set; } = 3;
        public float Heat { get; private set; } = 0f;
        public int Score { get; private set; } = 0;
        public int PodsDocked { get; private set; } = 0;

        public event Action<int> OnHullChanged;
        public event Action<float> OnHeatChanged;
        public event Action OnPodDocked;

        public void HandleDebrisBreach()
        {
            HullPips = Mathf.Max(0, HullPips - 1);
            Heat = Mathf.Min(100f, Heat + 18.0f);
            OnHullChanged?.Invoke(HullPips);
            OnHeatChanged?.Invoke(Heat);
        }

        public void HandlePodDock()
        {
            Score += 100;
            PodsDocked++;
            OnPodDocked?.Invoke();
        }

        public void StepOverdriveHeat(float dt)
        {
            // Raises heat at 6.0/s
            Heat = Mathf.Min(100f, Heat + 6.0f * dt);
            OnHeatChanged?.Invoke(Heat);
        }
    }
}
