using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.GyroSphereMech
{
    public enum FeedbackEvent
    {
        CupValid,
        YawTurn,
        Drive,
        Squeeze,
        ChinTap,
        LaserFire,
        BugHit,
        MultiKill,
        BugContact,
        Armor2,
        WaveClear,
        ClutchRegrip,
        Rim,
        Win,
        Fail
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.CupValid, "audio_servo_lock" },
            { FeedbackEvent.YawTurn, "audio_gyro_whir" },
            { FeedbackEvent.Drive, "audio_rolling_rumble" },
            { FeedbackEvent.Squeeze, "audio_pneumatic_hiss" },
            { FeedbackEvent.ChinTap, "audio_trigger_click" },
            { FeedbackEvent.LaserFire, "audio_twin_zap" },
            { FeedbackEvent.BugHit, "audio_chitin_crack" },
            { FeedbackEvent.MultiKill, "audio_triple_chime" },
            { FeedbackEvent.BugContact, "audio_armor_crunch" },
            { FeedbackEvent.Armor2, "audio_warning_pulse" },
            { FeedbackEvent.WaveClear, "audio_bronze_fanfare" },
            { FeedbackEvent.ClutchRegrip, "audio_magnetic_recenter" },
            { FeedbackEvent.Rim, "audio_low_scrape" },
            { FeedbackEvent.Win, "audio_five_note_ascent" },
            { FeedbackEvent.Fail, "audio_gyro_collapse" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;
    }
}
