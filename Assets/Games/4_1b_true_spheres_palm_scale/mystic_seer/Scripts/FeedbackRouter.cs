using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.MysticSeer
{
    public enum FeedbackEvent
    {
        HandEntersShell,
        Sweep,
        CurvingPassBonus,
        ThiefReveal,
        ThiefBreathing,
        Banish,
        WrongTap,
        Smother,
        EscapeWarning,
        ThiefEscapes,
        RoundClear,
        RelicLost,
        OrbMoved
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.HandEntersShell, "audio_airy_whoosh" },
            { FeedbackEvent.Sweep, "audio_filtered_noise" },
            { FeedbackEvent.CurvingPassBonus, "audio_chime_semitones" },
            { FeedbackEvent.ThiefReveal, "audio_whisper_sting" },
            { FeedbackEvent.ThiefBreathing, "audio_filtered_breath" },
            { FeedbackEvent.Banish, "audio_bell_reverb" },
            { FeedbackEvent.WrongTap, "audio_muffled_thud" },
            { FeedbackEvent.Smother, "audio_lowpass_swell" },
            { FeedbackEvent.EscapeWarning, "audio_ring_accelerating" },
            { FeedbackEvent.ThiefEscapes, "audio_gliss_crack" },
            { FeedbackEvent.RoundClear, "audio_4note_arpeggio" },
            { FeedbackEvent.RelicLost, "audio_glass_crack" },
            { FeedbackEvent.OrbMoved, "audio_wooden_slide" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;

        public float ComputeRevealGain(float fogDensity)
        {
            return 1.0f - fogDensity;
        }
    }
}
