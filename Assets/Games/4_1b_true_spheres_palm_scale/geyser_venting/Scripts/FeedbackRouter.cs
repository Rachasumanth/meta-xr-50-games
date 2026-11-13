using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.GeyserVenting
{
    public enum FeedbackEvent
    {
        CradleValid,
        Spin,
        VentBloom,
        VentAt90,
        PressStart,
        CapComplete,
        PreCap,
        WrongPress,
        Crack2Of3,
        Pressure70,
        Pressure85,
        Pressure95,
        Eruption,
        PhaseClear,
        Drop
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.CradleValid, "audio_stone_settle" },
            { FeedbackEvent.Spin, "audio_granular_rumble" },
            { FeedbackEvent.VentBloom, "audio_steam_whistle" },
            { FeedbackEvent.VentAt90, "audio_double_whistle" },
            { FeedbackEvent.PressStart, "audio_granular_press" },
            { FeedbackEvent.CapComplete, "audio_thump_sinter" },
            { FeedbackEvent.PreCap, "audio_glassy_chime" },
            { FeedbackEvent.WrongPress, "audio_crust_crack" },
            { FeedbackEvent.Crack2Of3, "audio_warning_toll" },
            { FeedbackEvent.Pressure70, "audio_klaxon_70" },
            { FeedbackEvent.Pressure85, "audio_pulse_85" },
            { FeedbackEvent.Pressure95, "audio_sine_95" },
            { FeedbackEvent.Eruption, "audio_subbass_boom" },
            { FeedbackEvent.PhaseClear, "audio_bronze_gong" },
            { FeedbackEvent.Drop, "audio_fruit_thud" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;
    }
}
