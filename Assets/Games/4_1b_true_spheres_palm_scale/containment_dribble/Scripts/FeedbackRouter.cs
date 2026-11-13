using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.ContainmentDribble
{
    public enum FeedbackEvent
    {
        SeatValid,
        Tap,
        DoubleTap,
        PerfectRhythm,
        Launch,
        Apex,
        FireflyCollect,
        LaneShift,
        SpikeWarning,
        SpikeHit,
        NoTapBounce,
        AirTap,
        ShieldLost,
        Win,
        Fail
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.SeatValid, "audio_foam_settle" },
            { FeedbackEvent.Tap, "audio_pock" },
            { FeedbackEvent.DoubleTap, "audio_double_pock" },
            { FeedbackEvent.PerfectRhythm, "audio_glassy_chime" },
            { FeedbackEvent.Launch, "audio_rising_whoosh" },
            { FeedbackEvent.Apex, "audio_float_tone" },
            { FeedbackEvent.FireflyCollect, "audio_bright_bell" },
            { FeedbackEvent.LaneShift, "audio_wing_flap" },
            { FeedbackEvent.SpikeWarning, "audio_low_creak" },
            { FeedbackEvent.SpikeHit, "audio_clang_roar" },
            { FeedbackEvent.NoTapBounce, "audio_dull_thud" },
            { FeedbackEvent.AirTap, "audio_hollow_knock" },
            { FeedbackEvent.ShieldLost, "audio_heart_lost" },
            { FeedbackEvent.Win, "audio_rising_fanfare" },
            { FeedbackEvent.Fail, "audio_falling_twonote" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;
    }
}
