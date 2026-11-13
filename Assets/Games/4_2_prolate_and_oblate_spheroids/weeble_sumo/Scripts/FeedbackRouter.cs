using System.Collections.Generic;

namespace XRGames.Chapter4.WeebleSumo
{
    public enum WeebleEvent
    {
        None,
        PropRegistered,
        PressInitiated,
        ChargeUpdated,
        LungeExecuted,
        ImpBumped,
        ImpRingOut,
        ImpPushedSumo,
        SumoToppled,
        Reseated,
        StabilityZero,
        BoutWon,
        GameOver
    }

    public class FeedbackRouter
    {
        public List<WeebleEvent> Events { get; } = new List<WeebleEvent>();

        public void Emit(WeebleEvent evt)
        {
            Events.Add(evt);
        }
    }
}
