using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.KatamariHarvest
{
    public enum FeedbackEvent
    {
        RollStart,
        CircularStrokeQuality,
        SheepCapture,
        FenceCapture,
        BarnCapture,
        ChainStep,
        ChainBreak,
        MineGuard,
        MineTrigger,
        TierUp,
        QuotaMet,
        SeasonEnd,
        OffTray
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.RollStart, "audio_roll_rumble" },
            { FeedbackEvent.CircularStrokeQuality, "audio_hum_180hz" },
            { FeedbackEvent.SheepCapture, "audio_sheep_bleat" },
            { FeedbackEvent.FenceCapture, "audio_fence_clank" },
            { FeedbackEvent.BarnCapture, "audio_barn_crack" },
            { FeedbackEvent.ChainStep, "audio_chain_chime" },
            { FeedbackEvent.ChainBreak, "audio_chain_click" },
            { FeedbackEvent.MineGuard, "audio_mine_guard_440hz" },
            { FeedbackEvent.MineTrigger, "audio_mine_boom" },
            { FeedbackEvent.TierUp, "audio_tier_up_third" },
            { FeedbackEvent.QuotaMet, "audio_fanfare_3note" },
            { FeedbackEvent.SeasonEnd, "audio_wind_down" },
            { FeedbackEvent.OffTray, "audio_squeak_offtray" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;
    }
}
