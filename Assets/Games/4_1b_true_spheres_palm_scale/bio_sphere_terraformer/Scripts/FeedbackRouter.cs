using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.BioSphereTerraformer
{
    public enum FeedbackEvent
    {
        StemPinch,
        Spin,
        DayTick,
        Plant,
        Germination,
        TreeMature,
        Harvest,
        WeedSeed,
        WeedSpread,
        WeedBurned,
        SunRichWarning,
        RainRichWarning,
        Coverage25,
        Coverage35,
        SeasonWin
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.StemPinch, "audio_woody_click" },
            { FeedbackEvent.Spin, "audio_globe_whir" },
            { FeedbackEvent.DayTick, "audio_clock_tick" },
            { FeedbackEvent.Plant, "audio_seed_press" },
            { FeedbackEvent.Germination, "audio_sprout_pop" },
            { FeedbackEvent.TreeMature, "audio_wooden_chime" },
            { FeedbackEvent.Harvest, "audio_fruit_pluck" },
            { FeedbackEvent.WeedSeed, "audio_wet_blip" },
            { FeedbackEvent.WeedSpread, "audio_creeping_rustle" },
            { FeedbackEvent.WeedBurned, "audio_crisp_sizzle" },
            { FeedbackEvent.SunRichWarning, "audio_dry_crackle" },
            { FeedbackEvent.RainRichWarning, "audio_drip_loop" },
            { FeedbackEvent.Coverage25, "audio_blight_tone" },
            { FeedbackEvent.Coverage35, "audio_blight_bloom" },
            { FeedbackEvent.SeasonWin, "audio_four_note_theme" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;
    }
}
