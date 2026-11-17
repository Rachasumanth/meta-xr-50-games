using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    [Serializable]
    public sealed class GameDefinition
    {
        public string Slug;
        public string Title;
        public GameFamily Family;
        public ShapeKind Shape;
        public float NominalSizeMm;
        public float MinSizeMm;
        public float MaxSizeMm;
        public string RigTypeName;
        public MiniGameObjective Objective;
        public string Instruction;
        public Color Primary;
        public Color Accent;
        public int Difficulty;
        public string ChapterSlug;
        public string[] RealObjectKeywords;

        public float NominalSizeMeters => NominalSizeMm * 0.001f;
        public bool IsProlate => Shape == ShapeKind.ProlateSpheroid;
        public bool IsOblate => Shape == ShapeKind.OblateSpheroid;
        public bool IsBowl => Shape == ShapeKind.DomedBowl;
        public bool IsDome => Shape == ShapeKind.Hemisphere;

        public override string ToString()
        {
            return Title + " (" + Slug + ")";
        }
    }

    /// <summary>
    /// Authoritative catalogue of the 50 shipped mini-games. Every entry points
    /// at the existing, gate-verified rig from Assets/Games. The unified app does
    /// not duplicate gameplay cores; it hosts them.
    /// </summary>
    public static class GameCatalog
    {
        public const int ExpectedGameCount = 50;
        private static List<GameDefinition> _games;

        public static IReadOnlyList<GameDefinition> All
        {
            get
            {
                if (_games == null) Build();
                return _games;
            }
        }

        public static List<GameDefinition> ByFamily(GameFamily family)
        {
            var result = new List<GameDefinition>();
            foreach (var g in All) if (g.Family == family) result.Add(g);
            return result;
        }

        public static GameDefinition BySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;
            foreach (var g in All)
                if (string.Equals(g.Slug, slug, StringComparison.OrdinalIgnoreCase)) return g;
            return null;
        }

        public static GameFamily FamilyForChapter(string chapter)
        {
            if (chapter.Contains("4_1a")) return GameFamily.SpherePinch;
            if (chapter.Contains("4_1b")) return GameFamily.SpherePalm;
            if (chapter.Contains("4_2")) return GameFamily.Spheroid;
            if (chapter.Contains("4_3a")) return GameFamily.HemispherePinch;
            if (chapter.Contains("4_3b")) return GameFamily.HemisphereHand;
            return GameFamily.SpherePinch;
        }

        private static void Build()
        {
            _games = new List<GameDefinition>(ExpectedGameCount);

            const string c41a = "4_1a_true_spheres_pinch_scale";
            const string c41b = "4_1b_true_spheres_palm_scale";
            const string c42 = "4_2_prolate_and_oblate_spheroids";
            const string c43a = "4_3a_hemispheres_pinch_scale";
            const string c43b = "4_3b_hemispheres_hand_scale";

            // ---------------- 4.1a True spheres, pinch scale ----------------
            Add("socket_trackball", "Socket Trackball", c41a, ShapeKind.TrueSphere, 55f, 30f, 80f,
                "XRGames.C4_1A.SocketTrackball.SocketTrackballRig", MiniGameObjective.Spin,
                "Pinch and roll the marble in its socket. Build angular speed for 12 seconds.",
                new Color32(238, 232, 214, 255), new Color32(64, 164, 255, 255), 1, "ball", "sphere", "trackball");
            Add("clam_pearl_guard", "Clam Pearl Guard", c41a, ShapeKind.TrueSphere, 60f, 35f, 85f,
                "XRGames.Chapter4.ClamPearlGuard.ClamPearlGuardRig", MiniGameObjective.Defend,
                "Cup your palm over the pearl when the barracuda telegraphs. Keep the pearl intact.",
                new Color32(250, 244, 232, 255), new Color32(255, 92, 122, 255), 2, "pearl", "ball", "shell");
            Add("bernoulli_levitator", "Bernoulli Levitator", c41a, ShapeKind.TrueSphere, 45f, 25f, 70f,
                "XRGames.Chapter4.BernoulliLevitator.BernoulliLevitatorRig", MiniGameObjective.Balance,
                "Pinch the air valve to hold the pearl in the air column without touching the nozzle.",
                new Color32(208, 236, 255, 255), new Color32(63, 224, 255, 255), 2, "ball", "sphere");
            Add("micro_planet_scanner", "Micro-Planet Scanner", c41a, ShapeKind.TrueSphere, 60f, 35f, 85f,
                "XRGames.Chapter4.MicroPlanetScanner.MicroPlanetScannerRig", MiniGameObjective.Aim,
                "Point your fingertip across the globe and sweep the scanner over the glowing latitude bands.",
                new Color32(82, 124, 210, 255), new Color32(92, 240, 209, 255), 2, "globe", "ball");
            Add("cage_pinball", "Cage Pinball", c41a, ShapeKind.TrueSphere, 55f, 30f, 80f,
                "XRGames.Chapter4.CagePinball.CagePinballRig", MiniGameObjective.Spin,
                "Flick the marble inside the wire cage. Keep it bouncing off the bumpers.",
                new Color32(255, 210, 107, 255), new Color32(255, 81, 129, 255), 2, "ball", "sphere");
            Add("ocular_turret", "Ocular Turret", c41a, ShapeKind.TrueSphere, 50f, 30f, 75f,
                "XRGames.Chapter4.OcularTurret.OcularTurretRig", MiniGameObjective.Aim,
                "Roll the eyeball to aim the gaze beam at each gargoyle before it wakes.",
                new Color32(226, 238, 244, 255), new Color32(255, 121, 64, 255), 1, "eyeball", "ball");
            Add("fusion_core_stabilizer", "Fusion Core Stabilizer", c41a, ShapeKind.TrueSphere, 40f, 22f, 60f,
                "XRGames.Chapter4.FusionCoreStabilizer.FusionCoreStabilizerRig", MiniGameObjective.Balance,
                "Hold both palms at the containment radius to keep the plasma core stable.",
                new Color32(255, 148, 94, 255), new Color32(255, 238, 128, 255), 3, "plasma", "ball");
            Add("scarab_canyon", "Scarab Canyon", c41a, ShapeKind.TrueSphere, 55f, 30f, 80f,
                "XRGames.Chapter4.ScarabCanyon.ScarabCanyonRig", MiniGameObjective.Race,
                "Tilt your palm to roll the scarab's boulder through the canyon without falling.",
                new Color32(215, 166, 87, 255), new Color32(111, 197, 108, 255), 2, "boulder", "ball");
            Add("chrono_sphere_scrub", "Chrono Sphere Scrub", c41a, ShapeKind.TrueSphere, 60f, 35f, 85f,
                "XRGames.Chapter4.ChronoSphereScrub.ChronoSphereScrubRig", MiniGameObjective.Spin,
                "Twist the chronosphere to scrub time forward and backward and hit the resonance band.",
                new Color32(173, 139, 255, 255), new Color32(108, 234, 255, 255), 2, "chrono", "ball");
            Add("electron_deflector", "Electron Deflector", c41a, ShapeKind.TrueSphere, 45f, 25f, 70f,
                "XRGames.Chapter4.ElectronDeflector.ElectronDeflectorRig", MiniGameObjective.Defend,
                "Rotate the nucleus to deflect incoming electrons with the magnetic pole bands.",
                new Color32(95, 235, 195, 255), new Color32(255, 255, 255, 255), 3, "atom", "ball");

            // ---------------- 4.1b True spheres, palm scale ----------------
            Add("atlas_defense", "Atlas Defense", c41b, ShapeKind.TrueSphere, 130f, 90f, 170f,
                "XRGames.Chapter4.AtlasDefense.AtlasDefenseRig", MiniGameObjective.Defend,
                "Rotate the globe so the orbital shield intercepts every incoming missile.",
                new Color32(61, 116, 181, 255), new Color32(255, 97, 78, 255), 2, "globe", "ball", "world");
            Add("pulse_core_reactor", "Pulse Core Reactor", c41b, ShapeKind.TrueSphere, 120f, 90f, 165f,
                "XRGames.Chapter4.PulseCoreReactor.PulseCoreReactorRig", MiniGameObjective.Steady,
                "Hold both palms at the resonance distance while the core cycles through frequencies.",
                new Color32(255, 95, 181, 255), new Color32(112, 238, 255, 255), 3, "reactor", "ball");
            Add("katamari_harvest", "Katamari Harvest", c41b, ShapeKind.TrueSphere, 150f, 100f, 190f,
                "XRGames.Chapter4.KatamariHarvest.KatamariHarvestRig", MiniGameObjective.Collect,
                "Roll the sticky star ball across the tray to pick up every glowing object.",
                new Color32(255, 192, 77, 255), new Color32(255, 108, 193, 255), 1, "ball", "sphere");
            Add("mystic_seer", "Mystic Seer", c41b, ShapeKind.TrueSphere, 120f, 90f, 160f,
                "XRGames.Chapter4.MysticSeer.MysticSeerRig", MiniGameObjective.Steady,
                "Hold your palm above the crystal and keep it still while the omen resolves.",
                new Color32(167, 108, 255, 255), new Color32(255, 103, 190, 255), 1, "crystal", "orb", "ball");
            Add("equatorial_particle_ring", "Equatorial Particle Ring", c41b, ShapeKind.TrueSphere, 135f, 95f, 175f,
                "XRGames.Chapter4.EquatorialParticleRing.EquatorialParticleRingRig", MiniGameObjective.Spin,
                "Tilt your palm to spin the equatorial accelerator ring and trap the particle stream.",
                new Color32(84, 119, 255, 255), new Color32(255, 220, 98, 255), 2, "globe", "ring", "ball");
            Add("geyser_venting", "Geyser Venting", c41b, ShapeKind.TrueSphere, 150f, 100f, 190f,
                "XRGames.Chapter4.GeyserVenting.VolcanoRig", MiniGameObjective.Pour,
                "Press both palms on the pressure sphere to vent the geyser before it ruptures.",
                new Color32(255, 119, 67, 255), new Color32(255, 227, 128, 255), 2, "volcano", "sphere");
            Add("trench_run_globe", "Trench Run Globe", c41b, ShapeKind.TrueSphere, 140f, 95f, 180f,
                "XRGames.Chapter4.TrenchRunGlobe.StationRig", MiniGameObjective.Race,
                "Roll the miniature globe along the trench course and thread the checkpoints.",
                new Color32(81, 177, 255, 255), new Color32(255, 96, 96, 255), 2, "globe", "ball");
            Add("containment_dribble", "Containment Dribble", c41b, ShapeKind.TrueSphere, 130f, 90f, 170f,
                "XRGames.Chapter4.ContainmentDribble.ShaftRig", MiniGameObjective.Catch,
                "Bounce the foam ball above the containment shaft and catch it before it drops.",
                new Color32(103, 231, 255, 255), new Color32(255, 144, 88, 255), 2, "ball", "sphere");
            Add("bio_sphere_terraformer", "Bio-Sphere Terraformer", c41b, ShapeKind.TrueSphere, 140f, 95f, 180f,
                "XRGames.Chapter4.BioSphereTerraformer.TerraformRig", MiniGameObjective.Collect,
                "Twist the stem and tap the sphere to seed each climate band before the season ends.",
                new Color32(85, 201, 113, 255), new Color32(255, 210, 102, 255), 2, "sphere", "planet", "ball");
            Add("gyro_sphere_mech", "Gyro-Sphere Mech", c41b, ShapeKind.TrueSphere, 150f, 100f, 190f,
                "XRGames.Chapter4.GyroSphereMech.DomeRig", MiniGameObjective.Balance,
                "Grip both palms on the gyro sphere and keep the cockpit level while the flywheel spools.",
                new Color32(156, 170, 201, 255), new Color32(84, 221, 255, 255), 3, "gyro", "sphere");

            // ---------------- 4.2 Spheroids ----------------
            Add("sub_surface_hovercraft", "Sub-Surface Hovercraft", c42, ShapeKind.OblateSpheroid, 150f, 100f, 220f,
                "XRGames.Chapter4.SubSurfaceHovercraft.HovercraftRig", MiniGameObjective.Race,
                "Skim the oblate craft over the tabletop air cushion and thread the gates.",
                new Color32(104, 204, 227, 255), new Color32(255, 205, 91, 255), 2, "hovercraft", "stone");
            Add("weeble_sumo", "Weeble Sumo", c42, ShapeKind.ProlateSpheroid, 140f, 90f, 200f,
                "XRGames.Chapter4.WeebleSumo.WeebleRig", MiniGameObjective.Balance,
                "Flick the prolate weeble and keep it inside the dohyo ring while it self-rights.",
                new Color32(224, 71, 71, 255), new Color32(255, 209, 92, 255), 1, "weeble", "egg");
            Add("worry_stone_spirit", "Worry-Stone Spirit", c42, ShapeKind.OblateSpheroid, 90f, 55f, 140f,
                "XRGames.Chapter4.WorryStoneSpirit.StoneRig", MiniGameObjective.Steady,
                "Rub your thumb across the flat pebble in a steady rhythm to wake the spirit.",
                new Color32(119, 139, 145, 255), new Color32(101, 255, 190, 255), 1, "stone", "pebble");
            Add("scarab_buckler", "Scarab Buckler", c42, ShapeKind.OblateSpheroid, 220f, 140f, 300f,
                "XRGames.Chapter4.ScarabBuckler.BucklerRig", MiniGameObjective.Defend,
                "Hold the oblate carapace shield in front of the incoming fire and deflect each volley.",
                new Color32(203, 157, 79, 255), new Color32(106, 226, 142, 255), 2, "shield", "stone");
            Add("bobsled_chute", "Bobsled Chute", c42, ShapeKind.ProlateSpheroid, 160f, 110f, 240f,
                "XRGames.Chapter4.BobsledChute.BobsledRig", MiniGameObjective.Race,
                "Steer the prolate bobsled through the banked chute and brake at the finish.",
                new Color32(67, 214, 151, 255), new Color32(255, 104, 71, 255), 2, "bobsled", "egg");
            Add("vortex_wobbler", "Vortex Wobbler", c42, ShapeKind.ProlateSpheroid, 130f, 85f, 190f,
                "XRGames.Chapter4.VortexWobbler.VortexRig", MiniGameObjective.Spin,
                "Spin the wobbler on its rim and keep the precession inside the target circle.",
                new Color32(149, 112, 255, 255), new Color32(255, 220, 102, 255), 3, "top", "egg");
            Add("elliptical_orbit", "Elliptical Orbit", c42, ShapeKind.ProlateSpheroid, 170f, 120f, 250f,
                "XRGames.Chapter4.EllipticalOrbit.CoreRig", MiniGameObjective.Aim,
                "Tilt the dual-foci orbit to sling the satellite through each target ring.",
                new Color32(112, 153, 255, 255), new Color32(255, 186, 88, 255), 3, "orbit", "egg");
            Add("trilobite_march", "Trilobite March", c42, ShapeKind.OblateSpheroid, 170f, 120f, 240f,
                "XRGames.Chapter4.TrilobiteMarch.TrilobiteRig", MiniGameObjective.Race,
                "Pat the segmented trilobite forward and keep its march in rhythm across the shelf.",
                new Color32(183, 161, 115, 255), new Color32(97, 219, 155, 255), 2, "trilobite", "stone");
            Add("phantom_stone_skipper", "Phantom Stone Skipper", c42, ShapeKind.OblateSpheroid, 100f, 65f, 160f,
                "XRGames.Chapter4.PhantomStoneSkipper.SkipRig", MiniGameObjective.Catch,
                "Flick the flat stone across the water and keep it skipping for as long as possible.",
                new Color32(126, 189, 255, 255), new Color32(209, 245, 255, 255), 1, "stone", "pebble");
            Add("paddlewheel_pod", "Paddlewheel Pod", c42, ShapeKind.ProlateSpheroid, 190f, 130f, 270f,
                "XRGames.Chapter4.PaddlewheelPod.PodRig", MiniGameObjective.Spin,
                "Turn the twin paddlewheels with your palms and drive the pod across the basin.",
                new Color32(116, 223, 199, 255), new Color32(255, 195, 96, 255), 2, "pod", "egg");

            // ---------------- 4.3a Hemispheres, pinch scale ----------------
            Add("phantom_shell_game", "Phantom Shell Game", c43a, ShapeKind.Hemisphere, 90f, 55f, 130f,
                "XRGames.Chapter4.PhantomShellGame.ShellRig", MiniGameObjective.Collect,
                "Lift the shells and follow the ruby pearl through the shuffle.",
                new Color32(189, 137, 79, 255), new Color32(255, 74, 110, 255), 1, "shell", "bowl");
            Add("micro_wok_toss", "Micro-Wok Toss", c43a, ShapeKind.DomedBowl, 110f, 70f, 150f,
                "XRGames.Chapter4.MicroWokToss.WokRig", MiniGameObjective.Catch,
                "Flick the tiny wok to toss the jade bead and catch it in the centre.",
                new Color32(108, 116, 128, 255), new Color32(107, 227, 148, 255), 2, "wok", "bowl", "pan");
            Add("snapping_turtle_bunker", "Snapping Turtle Bunker", c43a, ShapeKind.Hemisphere, 100f, 65f, 140f,
                "XRGames.Chapter4.SnappingTurtleBunker.TurtleRig", MiniGameObjective.Defend,
                "Snap the domed shell shut before each incoming wave reaches the turtle.",
                new Color32(165, 195, 137, 255), new Color32(255, 202, 92, 255), 2, "turtle", "shell", "dome");
            Add("deep_space_dish", "Deep-Space Dish", c43a, ShapeKind.DomedBowl, 120f, 75f, 160f,
                "XRGames.Chapter4.DeepSpaceDish.DishRig", MiniGameObjective.Aim,
                "Tilt the parabolic dish and track the pulsar across the sky.",
                new Color32(206, 224, 255, 255), new Color32(127, 136, 255, 255), 2, "dish", "bowl", "satellite");
            Add("bey_cap_clash", "Bey-Cap Clash", c43a, ShapeKind.Hemisphere, 85f, 55f, 125f,
                "XRGames.Chapter4.BeyCapClash.CapRig", MiniGameObjective.Spin,
                "Launch the battle cap into the dome arena and out-spin the rival.",
                new Color32(255, 118, 90, 255), new Color32(255, 226, 110, 255), 2, "cap", "top", "dome");
            Add("tardigrade_pond", "Tardigrade Pond", c43a, ShapeKind.DomedBowl, 100f, 65f, 140f,
                "XRGames.Chapter4.TardigradePond.PondRig", MiniGameObjective.Steady,
                "Keep the droplet pond calm while the tardigrades graze.",
                new Color32(119, 206, 255, 255), new Color32(126, 255, 197, 255), 1, "pond", "bowl");
            Add("paratrooper_basket", "Paratrooper Basket", c43a, ShapeKind.DomedBowl, 110f, 70f, 150f,
                "XRGames.Chapter4.ParatrooperBasket.BasketRig", MiniGameObjective.Catch,
                "Tilt the parachute basket to catch every descending trooper.",
                new Color32(230, 196, 124, 255), new Color32(126, 213, 255, 255), 2, "basket", "bowl");
            Add("micro_mortar_alchemist", "Micro-Mortar Alchemist", c43a, ShapeKind.DomedBowl, 95f, 60f, 135f,
                "XRGames.Chapter4.MicroMortarAlchemist.MortarRig", MiniGameObjective.Pour,
                "Grind the pestle and pour the glowing dose into the mortar without spilling.",
                new Color32(150, 141, 128, 255), new Color32(171, 255, 122, 255), 3, "mortar", "bowl");
            Add("limpet_suction", "Limpet Suction", c43a, ShapeKind.Hemisphere, 75f, 45f, 110f,
                "XRGames.Chapter4.LimpetSuction.LimpetRig", MiniGameObjective.Balance,
                "Press the limpet onto the reef and hold suction against the current.",
                new Color32(211, 193, 160, 255), new Color32(96, 225, 196, 255), 1, "limpet", "shell", "dome");
            Add("temple_bell", "Temple Bell", c43a, ShapeKind.Hemisphere, 95f, 60f, 135f,
                "XRGames.Chapter4.TempleBell.BellRig", MiniGameObjective.Spin,
                "Swing the bronze bell with your mallet and hold the resonant tone.",
                new Color32(207, 162, 87, 255), new Color32(255, 231, 170, 255), 1, "bell", "dome");

            // ---------------- 4.3b Hemispheres, hand scale ----------------
            Add("galaxy_cauldron", "Galaxy Cauldron", c43b, ShapeKind.DomedBowl, 240f, 170f, 320f,
                "XRGames.Chapter4.GalaxyCauldron.CauldronRig", MiniGameObjective.Stir,
                "Stir the cosmic broth along the rim until the vortex core ignites.",
                new Color32(120, 112, 196, 255), new Color32(255, 139, 215, 255), 2, "cauldron", "bowl", "pot");
            Add("singing_bowl_sanctuary", "Singing Bowl Sanctuary", c43b, ShapeKind.DomedBowl, 230f, 160f, 310f,
                "XRGames.Chapter4.SingingBowlSanctuary.SingingBowlRig", MiniGameObjective.Stir,
                "Trace the mallet around the rim and grow the acoustic wave through the sanctuary.",
                new Color32(204, 174, 112, 255), new Color32(142, 237, 255, 255), 2, "bowl", "cauldron");
            Add("fingerboard_bowl", "Fingerboard Bowl", c43b, ShapeKind.DomedBowl, 220f, 150f, 300f,
                "XRGames.Chapter4.FingerboardBowl.SkateRig", MiniGameObjective.Race,
                "Carve the deep bowl with two fingers and land the skater's line.",
                new Color32(142, 158, 187, 255), new Color32(255, 128, 92, 255), 2, "bowl", "skate");
            Add("observatory_dome", "Observatory Dome", c43b, ShapeKind.Hemisphere, 260f, 180f, 340f,
                "XRGames.Chapter4.ObservatoryDome.ObservatoryRig", MiniGameObjective.Aim,
                "Rotate the dome and open the slit to track each target across the night sky.",
                new Color32(142, 160, 204, 255), new Color32(255, 232, 142, 255), 3, "dome", "observatory");
            Add("medusa_jellyfish", "Medusa Jellyfish", c43b, ShapeKind.Hemisphere, 210f, 150f, 290f,
                "XRGames.Chapter4.MedusaJellyfish.JellyRig", MiniGameObjective.Balance,
                "Compress the bell with both palms and jet upward without hitting the trench walls.",
                new Color32(106, 221, 239, 255), new Color32(255, 126, 220, 255), 2, "jellyfish", "dome");
            Add("kraken_broth", "Kraken Broth", c43b, ShapeKind.DomedBowl, 250f, 175f, 330f,
                "XRGames.Chapter4.KrakenBroth.KrakenRig", MiniGameObjective.Stir,
                "Drum the sea basin in quadrants and keep the kraken's tentacles in their sectors.",
                new Color32(83, 168, 178, 255), new Color32(173, 113, 255, 255), 3, "kraken", "bowl", "pot");
            Add("foghorn_buoy", "Foghorn Buoy", c43b, ShapeKind.Hemisphere, 220f, 150f, 300f,
                "XRGames.Chapter4.FoghornBuoy.FoghornRig", MiniGameObjective.Balance,
                "Lean the buoy against the ocean swell and sound the foghorn at each crest.",
                new Color32(232, 118, 84, 255), new Color32(255, 228, 132, 255), 2, "buoy", "dome");
            Add("arecibo_reflector", "Arecibo Reflector", c43b, ShapeKind.DomedBowl, 300f, 210f, 400f,
                "XRGames.Chapter4.AreciboReflector.AreciboRig", MiniGameObjective.Aim,
                "Sweep the giant dish receiver across the sky and lock each radio burst.",
                new Color32(196, 213, 224, 255), new Color32(116, 255, 200, 255), 3, "dish", "bowl");
            Add("tortoise_bastion", "Tortoise Bastion", c43b, ShapeKind.Hemisphere, 280f, 190f, 360f,
                "XRGames.Chapter4.TortoiseBastion.TortoiseRig", MiniGameObjective.Defend,
                "Angle the vaulted shell to deflect incoming projectiles and protect the fortress.",
                new Color32(149, 173, 121, 255), new Color32(255, 203, 112, 255), 3, "shell", "tortoise", "dome");
            Add("buckler_deflector", "Buckler Deflector", c43b, ShapeKind.DomedBowl, 200f, 140f, 280f,
                "XRGames.Chapter4.BucklerDeflector.BucklerRig", MiniGameObjective.Defend,
                "Catch fireballs in the concave buckler and release the stored beam at the right moment.",
                new Color32(215, 181, 120, 255), new Color32(255, 107, 74, 255), 2, "shield", "bowl", "buckler");
        }

        private static void Add(string slug, string title, string chapter, ShapeKind shape, float nominalMm,
            float minMm, float maxMm, string rigType, MiniGameObjective objective, string instruction,
            Color primary, Color accent, int difficulty, params string[] objectKeywords)
        {
            var family = FamilyForChapter(chapter);
            var def = new GameDefinition
            {
                Slug = slug,
                Title = title,
                ChapterSlug = chapter,
                Family = family,
                Shape = shape,
                NominalSizeMm = nominalMm,
                MinSizeMm = minMm,
                MaxSizeMm = maxMm,
                RigTypeName = rigType,
                Objective = objective,
                Instruction = instruction,
                Primary = primary,
                Accent = accent,
                Difficulty = difficulty,
                RealObjectKeywords = objectKeywords ?? new string[0]
            };
            _games.Add(def);
        }

        /// <summary>
        /// Direct type table. Keeping typeof() references here (instead of only
        /// strings) guarantees IL2CPP linking preserves all 50 rigs and makes a
        /// missing rig a compile-time error rather than a runtime surprise.
        /// </summary>
        private static readonly Dictionary<string, Type> RigTypeMap = new Dictionary<string, Type>
        {
            { "socket_trackball", typeof(XRGames.C4_1A.SocketTrackball.SocketTrackballRig) },
            { "clam_pearl_guard", typeof(XRGames.Chapter4.ClamPearlGuard.ClamPearlGuardRig) },
            { "bernoulli_levitator", typeof(XRGames.Chapter4.BernoulliLevitator.BernoulliLevitatorRig) },
            { "micro_planet_scanner", typeof(XRGames.Chapter4.MicroPlanetScanner.MicroPlanetScannerRig) },
            { "cage_pinball", typeof(XRGames.Chapter4.CagePinball.CagePinballRig) },
            { "ocular_turret", typeof(XRGames.Chapter4.OcularTurret.OcularTurretRig) },
            { "fusion_core_stabilizer", typeof(XRGames.Chapter4.FusionCoreStabilizer.FusionCoreStabilizerRig) },
            { "scarab_canyon", typeof(XRGames.Chapter4.ScarabCanyon.ScarabCanyonRig) },
            { "chrono_sphere_scrub", typeof(XRGames.Chapter4.ChronoSphereScrub.ChronoSphereScrubRig) },
            { "electron_deflector", typeof(XRGames.Chapter4.ElectronDeflector.ElectronDeflectorRig) },
            { "atlas_defense", typeof(XRGames.Chapter4.AtlasDefense.AtlasDefenseRig) },
            { "pulse_core_reactor", typeof(XRGames.Chapter4.PulseCoreReactor.PulseCoreReactorRig) },
            { "katamari_harvest", typeof(XRGames.Chapter4.KatamariHarvest.KatamariHarvestRig) },
            { "mystic_seer", typeof(XRGames.Chapter4.MysticSeer.MysticSeerRig) },
            { "equatorial_particle_ring", typeof(XRGames.Chapter4.EquatorialParticleRing.EquatorialParticleRingRig) },
            { "geyser_venting", typeof(XRGames.Chapter4.GeyserVenting.VolcanoRig) },
            { "trench_run_globe", typeof(XRGames.Chapter4.TrenchRunGlobe.StationRig) },
            { "containment_dribble", typeof(XRGames.Chapter4.ContainmentDribble.ShaftRig) },
            { "bio_sphere_terraformer", typeof(XRGames.Chapter4.BioSphereTerraformer.TerraformRig) },
            { "gyro_sphere_mech", typeof(XRGames.Chapter4.GyroSphereMech.DomeRig) },
            { "sub_surface_hovercraft", typeof(XRGames.Chapter4.SubSurfaceHovercraft.HovercraftRig) },
            { "weeble_sumo", typeof(XRGames.Chapter4.WeebleSumo.WeebleRig) },
            { "worry_stone_spirit", typeof(XRGames.Chapter4.WorryStoneSpirit.StoneRig) },
            { "scarab_buckler", typeof(XRGames.Chapter4.ScarabBuckler.BucklerRig) },
            { "bobsled_chute", typeof(XRGames.Chapter4.BobsledChute.BobsledRig) },
            { "vortex_wobbler", typeof(XRGames.Chapter4.VortexWobbler.VortexRig) },
            { "elliptical_orbit", typeof(XRGames.Chapter4.EllipticalOrbit.CoreRig) },
            { "trilobite_march", typeof(XRGames.Chapter4.TrilobiteMarch.TrilobiteRig) },
            { "phantom_stone_skipper", typeof(XRGames.Chapter4.PhantomStoneSkipper.SkipRig) },
            { "paddlewheel_pod", typeof(XRGames.Chapter4.PaddlewheelPod.PodRig) },
            { "phantom_shell_game", typeof(XRGames.Chapter4.PhantomShellGame.ShellRig) },
            { "micro_wok_toss", typeof(XRGames.Chapter4.MicroWokToss.WokRig) },
            { "snapping_turtle_bunker", typeof(XRGames.Chapter4.SnappingTurtleBunker.TurtleRig) },
            { "deep_space_dish", typeof(XRGames.Chapter4.DeepSpaceDish.DishRig) },
            { "bey_cap_clash", typeof(XRGames.Chapter4.BeyCapClash.CapRig) },
            { "tardigrade_pond", typeof(XRGames.Chapter4.TardigradePond.PondRig) },
            { "paratrooper_basket", typeof(XRGames.Chapter4.ParatrooperBasket.BasketRig) },
            { "micro_mortar_alchemist", typeof(XRGames.Chapter4.MicroMortarAlchemist.MortarRig) },
            { "limpet_suction", typeof(XRGames.Chapter4.LimpetSuction.LimpetRig) },
            { "temple_bell", typeof(XRGames.Chapter4.TempleBell.BellRig) },
            { "galaxy_cauldron", typeof(XRGames.Chapter4.GalaxyCauldron.CauldronRig) },
            { "singing_bowl_sanctuary", typeof(XRGames.Chapter4.SingingBowlSanctuary.SingingBowlRig) },
            { "fingerboard_bowl", typeof(XRGames.Chapter4.FingerboardBowl.SkateRig) },
            { "observatory_dome", typeof(XRGames.Chapter4.ObservatoryDome.ObservatoryRig) },
            { "medusa_jellyfish", typeof(XRGames.Chapter4.MedusaJellyfish.JellyRig) },
            { "kraken_broth", typeof(XRGames.Chapter4.KrakenBroth.KrakenRig) },
            { "foghorn_buoy", typeof(XRGames.Chapter4.FoghornBuoy.FoghornRig) },
            { "arecibo_reflector", typeof(XRGames.Chapter4.AreciboReflector.AreciboRig) },
            { "tortoise_bastion", typeof(XRGames.Chapter4.TortoiseBastion.TortoiseRig) },
            { "buckler_deflector", typeof(XRGames.Chapter4.BucklerDeflector.BucklerRig) }
        };

        public static Type GetRigType(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;
            return RigTypeMap.TryGetValue(slug, out var t) ? t : null;
        }

        public static Color FamilyColor(GameFamily family)
        {
            switch (family)
            {
                case GameFamily.SpherePinch: return new Color32(81, 196, 255, 255);
                case GameFamily.SpherePalm: return new Color32(123, 148, 255, 255);
                case GameFamily.Spheroid: return new Color32(162, 134, 255, 255);
                case GameFamily.HemispherePinch: return new Color32(255, 185, 84, 255);
                case GameFamily.HemisphereHand: return new Color32(255, 116, 148, 255);
                default: return Color.white;
            }
        }

        public static string FamilyTitle(GameFamily family)
        {
            switch (family)
            {
                case GameFamily.SpherePinch: return "True Spheres / Pinch";
                case GameFamily.SpherePalm: return "True Spheres / Palm";
                case GameFamily.Spheroid: return "Prolate & Oblate";
                case GameFamily.HemispherePinch: return "Domes & Bowls / Pinch";
                case GameFamily.HemisphereHand: return "Domes & Bowls / Hand";
                default: return family.ToString();
            }
        }
    }
}
