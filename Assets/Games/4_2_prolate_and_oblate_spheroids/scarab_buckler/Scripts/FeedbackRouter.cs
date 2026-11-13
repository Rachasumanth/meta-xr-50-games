using System.Collections.Generic;

namespace XRGames.Chapter4.ScarabBuckler
{
    public enum BucklerEvent
    {
        None,
        PropRegistered,
        BeamTelegraph,
        BeamDeflected,
        FanTargetHit,
        RimScatter,
        OverheatLockout,
        HeatVented,
        NovaFired,
        TorsoDamage,
        WaveCleared,
        GameOver
    }

    public class FeedbackRouter
    {
        public List<BucklerEvent> Events { get; } = new List<BucklerEvent>();

        public void Emit(BucklerEvent evt)
        {
            Events.Add(evt);
        }
    }
}
