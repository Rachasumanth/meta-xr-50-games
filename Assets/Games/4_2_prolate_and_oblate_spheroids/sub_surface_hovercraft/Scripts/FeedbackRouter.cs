using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public enum HovercraftEvent
    {
        None,
        PropRegistered,
        ThrustApplied,
        ClutchEngaged,
        ClutchReleased,
        DepthChanged,
        HarpoonFired,
        HazardHit,
        DepthMismatchThud,
        DriftBoost,
        OverheatLockout,
        ComboIncremented,
        ComboBroken,
        HullDamage,
        ChannelCleared,
        GameOver
    }

    public class FeedbackRouter
    {
        public List<HovercraftEvent> EventHistory { get; } = new List<HovercraftEvent>();

        public void Emit(HovercraftEvent evt)
        {
            EventHistory.Add(evt);
        }

        public void Clear()
        {
            EventHistory.Clear();
        }
    }
}
