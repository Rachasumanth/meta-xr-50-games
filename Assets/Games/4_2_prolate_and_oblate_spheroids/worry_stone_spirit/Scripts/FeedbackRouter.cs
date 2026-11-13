using System.Collections.Generic;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public enum StoneEvent
    {
        None,
        PropRegistered,
        LayerBroken,
        PolishAchieved,
        FragmentRevealed,
        FragmentLost,
        ConstellationRecorded,
        TideEnded,
        GameWon,
        GameOver
    }

    public class FeedbackRouter
    {
        public List<StoneEvent> Events { get; } = new List<StoneEvent>();

        public void Emit(StoneEvent evt)
        {
            Events.Add(evt);
        }
    }
}
