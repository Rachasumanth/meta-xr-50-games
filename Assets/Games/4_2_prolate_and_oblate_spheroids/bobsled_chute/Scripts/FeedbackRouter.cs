using System.Collections.Generic;

namespace XRGames.Chapter4.BobsledChute
{
    public enum BobsledEvent
    {
        None,
        PropRegistered,
        GatePassed,
        RimScrape,
        SledFlip,
        HeatFinished,
        DescentFinished,
        GoldAchieved,
        GameOver
    }

    public class FeedbackRouter
    {
        public List<BobsledEvent> Events { get; } = new List<BobsledEvent>();

        public void Emit(BobsledEvent evt)
        {
            Events.Add(evt);
        }
    }
}
