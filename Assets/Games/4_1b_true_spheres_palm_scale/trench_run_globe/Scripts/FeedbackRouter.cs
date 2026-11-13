using System;
using System.Collections.Generic;

namespace XRGames.Chapter4.TrenchRunGlobe
{
    public enum FeedbackEvent
    {
        ReadHeadContact,
        SpinUp,
        ForwardScroll,
        Steer,
        TowerCharge,
        BoltFire,
        BoltHit,
        NearMiss,
        WallScrape,
        OffTrench,
        PortArmed,
        PortAligned,
        Dive,
        Overshoot,
        SectorClear
    }

    public class FeedbackRouter
    {
        private readonly Dictionary<FeedbackEvent, string> _audioMap = new Dictionary<FeedbackEvent, string>
        {
            { FeedbackEvent.ReadHeadContact, "audio_felt_press" },
            { FeedbackEvent.SpinUp, "audio_turbine_whine" },
            { FeedbackEvent.ForwardScroll, "audio_rail_clack" },
            { FeedbackEvent.Steer, "audio_airy_whoosh" },
            { FeedbackEvent.TowerCharge, "audio_two_tone" },
            { FeedbackEvent.BoltFire, "audio_sharp_zap" },
            { FeedbackEvent.BoltHit, "audio_shield_crack" },
            { FeedbackEvent.NearMiss, "audio_close_whoosh" },
            { FeedbackEvent.WallScrape, "audio_metal_grind" },
            { FeedbackEvent.OffTrench, "audio_control_wobble" },
            { FeedbackEvent.PortArmed, "audio_arming_thunk" },
            { FeedbackEvent.PortAligned, "audio_lock_tone" },
            { FeedbackEvent.Dive, "audio_torpedo_launch" },
            { FeedbackEvent.Overshoot, "audio_miss_tone" },
            { FeedbackEvent.SectorClear, "audio_bronze_fanfare" }
        };

        public bool HasMapping(FeedbackEvent evt) => _audioMap.ContainsKey(evt);

        public string GetClipName(FeedbackEvent evt) => _audioMap.TryGetValue(evt, out var clip) ? clip : null;

        public int TotalMappedEvents => _audioMap.Count;
    }
}
