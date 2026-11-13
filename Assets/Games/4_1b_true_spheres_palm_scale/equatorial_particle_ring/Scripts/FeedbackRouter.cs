using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public enum FeedbackEvent
    {
        CradleLocked,
        TraceStart,
        ChargeRise,
        AimSteady,
        Fire,
        Hit,
        Graze,
        Bounce,
        Miss,
        HeatWarning,
        Overheat,
        Vent,
        GripSlip,
        RunClear
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.CradleLocked, "audio_cradle_hum_220hz" },
            { FeedbackEvent.TraceStart, "audio_trace_friction" },
            { FeedbackEvent.ChargeRise, "audio_charge_hum_120_680hz" },
            { FeedbackEvent.AimSteady, "audio_aim_tone_880hz" },
            { FeedbackEvent.Fire, "audio_fire_chirp_1100hz" },
            { FeedbackEvent.Hit, "audio_hit_boom_90hz" },
            { FeedbackEvent.Graze, "audio_graze_scrape" },
            { FeedbackEvent.Bounce, "audio_bounce_thud_300hz" },
            { FeedbackEvent.Miss, "audio_miss_fizzle_180hz" },
            { FeedbackEvent.HeatWarning, "audio_heat_double_beep" },
            { FeedbackEvent.Overheat, "audio_overheat_alarm_100hz" },
            { FeedbackEvent.Vent, "audio_vent_hiss_5khz" },
            { FeedbackEvent.GripSlip, "audio_grip_buzz_60hz" },
            { FeedbackEvent.RunClear, "audio_arpeggio_5note" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;
    }
}
