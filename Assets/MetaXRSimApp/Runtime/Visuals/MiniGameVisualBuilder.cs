using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    public sealed class BuiltMiniGame
    {
        public GameObject Root;
        public Transform Prop;
        public Transform Base;
        public Transform Floor;
        public Transform[] SpawnPoints;
        public readonly Dictionary<string, Transform> Parts = new Dictionary<string, Transform>();
        public float SizeMeters;
        public Color Accent;

        public Transform Find(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (Parts.TryGetValue(key.ToLowerInvariant(), out var t)) return t;
            return null;
        }

        public void Register(string key, Transform t)
        {
            if (t == null || string.IsNullOrEmpty(key)) return;
            Parts[key.ToLowerInvariant()] = t;
        }
    }

    /// <summary>
    /// Builds a themed, physical mini-game board at real-world scale. The 50
    /// existing rig scripts are attached afterwards by MiniGameHost; this class
    /// only produces the prop, base and recognisable accessories.
    /// </summary>
    public static class MiniGameVisualBuilder
    {
        public static BuiltMiniGame Build(GameDefinition game, Transform root, float sizeMeters)
        {
            var built = new BuiltMiniGame
            {
                Root = root.gameObject,
                SizeMeters = sizeMeters,
                Accent = game.Accent
            };

            float s = Mathf.Max(0.02f, sizeMeters);
            Color primary = game.Primary;
            Color accent = game.Accent;

            // Pedestal / play surface.
            var baseGo = new GameObject("Base");
            baseGo.transform.SetParent(root, false);
            built.Base = baseGo.transform;

            if (game.Shape == ShapeKind.TrueSphere || game.Shape.IsSpheroid())
            {
                Pedestal(baseGo.transform, s, primary, accent);
            }
            else
            {
                RimStand(baseGo.transform, s, primary, accent);
            }

            // Primary prop.
            Transform prop;
            switch (game.Shape)
            {
                case ShapeKind.TrueSphere:
                    prop = SphereProp(root, s, primary, accent);
                    break;
                case ShapeKind.ProlateSpheroid:
                    prop = EllipsoidProp(root, new Vector3(0.74f, 1f, 0.74f), s, primary, accent);
                    break;
                case ShapeKind.OblateSpheroid:
                    prop = EllipsoidProp(root, new Vector3(1f, 0.38f, 0.84f), s, primary, accent);
                    break;
                case ShapeKind.Hemisphere:
                    prop = DomeProp(root, s, primary, accent, false);
                    break;
                case ShapeKind.DomedBowl:
                    prop = DomeProp(root, s, primary, accent, true);
                    break;
                default:
                    prop = SphereProp(root, s, primary, accent);
                    break;
            }
            built.Prop = prop;
            built.Register("prop", prop);
            built.Register("main", prop);

            // Catch floor just below the base so thrown props are recoverable.
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "CatchFloor";
            floor.transform.SetParent(root, false);
            floor.transform.localPosition = new Vector3(0f, -0.012f, 0f);
            floor.transform.localScale = new Vector3(Mathf.Max(0.8f, s * 3.2f), 0.02f, Mathf.Max(0.8f, s * 3.2f));
            var floorRenderer = floor.GetComponent<Renderer>();
            floorRenderer.enabled = false;
            built.Floor = floor.transform;
            built.Register("floor", floor.transform);

            AddAliases(built, prop);

            AddThemedParts(game, root, built, primary, accent, s);

            // Contact label on the base.
            var label = WorldLabel.Create("GameLabel", root, game.Title, Mathf.Clamp(s * 0.09f, 0.012f, 0.035f),
                accent, new Vector3(0f, 0.002f, -Mathf.Min(s * 0.9f, 0.15f)), Quaternion.Euler(90f, 0f, 0f));
            label.transform.localScale *= 1f;

            // Add a subtle interaction halo.
            Halo(root, s * 2.1f, accent);

            built.SpawnPoints = new[]
            {
                Anchor(root, "Spawn0", new Vector3(s * 0.9f, s * 0.35f, s * 0.8f)),
                Anchor(root, "Spawn1", new Vector3(-s * 0.9f, s * 0.35f, s * 0.8f)),
                Anchor(root, "Spawn2", new Vector3(0f, s * 0.85f, -s * 0.9f)),
                Anchor(root, "Spawn3", new Vector3(0f, s * 1.2f, s * 0.2f))
            };

            return built;
        }

        private static void AddAliases(BuiltMiniGame b, Transform prop)
        {
            string[] names =
            {
                "marble", "ball", "orb", "globe", "pearl", "stone", "egg", "pod", "bell", "bowl",
                "core", "cap", "dish", "shell", "tray", "sled", "craft", "trilobite", "disc", "sumo",
                "buckler", "skater", "buoy", "dragon", "apple", "foamBall", "orange", "stoneVisual", "mortar", "wok", "pan", "dome", "head", "moon", "satellite",
                "podVisual", "bellTransform", "bowlTransform", "dome", "cauldron", "jelly", "kraken"
            };
            foreach (var n in names) b.Register(n, prop);
            b.Register("ship", Anchor(prop, "ShipAnchor", new Vector3(0f, 0.15f, 0f)));
            b.Register("hologram", Anchor(prop, "HologramAnchor", new Vector3(0f, 0.35f, 0f)));
            b.Register("beam", Anchor(prop, "BeamAnchor", new Vector3(0f, 0f, 0.25f)));
        }

        private static Transform Anchor(Transform parent, string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        private static Transform Anchor(GameObject parent, string name, Vector3 localPos)
        {
            return Anchor(parent.transform, name, localPos);
        }

        // ------------------------------------------------------------------
        // Prop construction
        // ------------------------------------------------------------------
        private static Transform SphereProp(Transform root, float s, Color primary, Color accent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Prop_Sphere";
            go.transform.SetParent(root, false);
            go.transform.localScale = Vector3.one * s;
            go.transform.localPosition = new Vector3(0f, 0.028f + s * 0.5f, 0f);
            go.GetComponent<Renderer>().material = MaterialLibrary.Get(primary, 0.2f, 0.78f, primary * 0.08f);
            AddStripe(go.transform, accent);
            return PreparePhysical(go, s * 0.5f);
        }

        private static Transform EllipsoidProp(Transform root, Vector3 aspect, float s, Color primary, Color accent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Prop_Spheroid";
            go.transform.SetParent(root, false);
            Vector3 scale = Vector3.Scale(aspect, Vector3.one * s);
            if (aspect.y >= aspect.x) scale.x = s * 0.74f;
            go.transform.localScale = scale;
            go.transform.localPosition = new Vector3(0f, 0.028f + scale.y * 0.5f, 0f);
            go.GetComponent<Renderer>().material = MaterialLibrary.Get(primary, 0.18f, 0.72f, primary * 0.06f);
            AddStripe(go.transform, accent);
            return PreparePhysical(go, Mathf.Max(scale.x, scale.z) * 0.5f);
        }

        private static Transform DomeProp(Transform root, float s, Color primary, Color accent, bool bowl)
        {
            var go = new GameObject(bowl ? "Prop_Bowl" : "Prop_Dome");
            go.transform.SetParent(root, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = bowl ? ProceduralMesh.Bowl : ProceduralMesh.Dome;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.material = MaterialLibrary.Get(primary, 0.3f, 0.62f, primary * 0.1f);
            go.transform.localScale = new Vector3(s, s, s);
            go.transform.localPosition = bowl
                ? new Vector3(0f, 0.032f + s * 0.5f, 0f)
                : new Vector3(0f, 0.032f, 0f);

            var rim = new GameObject("Rim");
            rim.transform.SetParent(go.transform, false);
            var rimFilter = rim.AddComponent<MeshFilter>();
            rimFilter.sharedMesh = ProceduralMesh.Ring;
            var rimRenderer = rim.AddComponent<MeshRenderer>();
            rimRenderer.material = MaterialLibrary.Glow(accent, 1.6f);
            rim.transform.localScale = Vector3.one * 1.02f;

            return PreparePhysical(go, s * 0.5f);
        }

        private static void AddStripe(Transform prop, Color accent)
        {
            var stripe = new GameObject("EquatorStripe");
            stripe.transform.SetParent(prop, false);
            var mf = stripe.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMesh.Ring;
            var mr = stripe.AddComponent<MeshRenderer>();
            mr.material = MaterialLibrary.Glow(accent, 1.4f);
            stripe.transform.localScale = new Vector3(1.005f, 0.35f, 1.005f);
        }

        private static Transform PreparePhysical(GameObject go, float radius)
        {
            var body = go.AddComponent<Rigidbody>();
            body.mass = 0.22f;
            body.linearDamping = 0.12f;
            body.angularDamping = 0.18f;
            body.useGravity = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            if (go.GetComponent<Collider>() == null)
            {
                var sc = go.AddComponent<SphereCollider>();
                sc.radius = Mathf.Max(0.01f, radius / Mathf.Max(0.01f, go.transform.localScale.x));
            }
            return go.transform;
        }

        private static void Pedestal(Transform parent, float s, Color primary, Color accent)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            p.name = "Pedestal";
            p.transform.SetParent(parent, false);
            p.transform.localPosition = new Vector3(0f, 0.014f, 0f);
            p.transform.localScale = new Vector3(s * 1.55f, 0.014f, s * 1.55f);
            p.GetComponent<Renderer>().material = MaterialLibrary.Get(primary * 0.35f, 0.6f, 0.45f);
            var c = p.GetComponent<Collider>();
            if (c != null) c.isTrigger = true;

            var ring = new GameObject("BaseRing");
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = new Vector3(0f, 0.029f, 0f);
            var mf = ring.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMesh.Ring;
            ring.AddComponent<MeshRenderer>().material = MaterialLibrary.Glow(accent, 1.2f);
            ring.transform.localScale = new Vector3(s * 1.45f, 1f, s * 1.45f);
        }

        private static void RimStand(Transform parent, float s, Color primary, Color accent)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            p.name = "RimStand";
            p.transform.SetParent(parent, false);
            p.transform.localPosition = new Vector3(0f, 0.012f, 0f);
            p.transform.localScale = new Vector3(s * 1.65f, 0.012f, s * 1.65f);
            p.GetComponent<Renderer>().material = MaterialLibrary.Get(primary * 0.3f, 0.55f, 0.4f);
            var c = p.GetComponent<Collider>();
            if (c != null) c.isTrigger = true;

            var ring = new GameObject("StandRing");
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = new Vector3(0f, 0.026f, 0f);
            ring.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
            ring.AddComponent<MeshRenderer>().material = MaterialLibrary.Glow(accent, 1.3f);
            ring.transform.localScale = new Vector3(s * 1.5f, 1f, s * 1.5f);
        }

        private static void Halo(Transform root, float diameter, Color accent)
        {
            var go = new GameObject("Halo");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(0f, 0.004f, 0f);
            go.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Ring;
            go.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(accent, 0.28f);
            go.transform.localScale = new Vector3(diameter, 1f, diameter);
        }

        // ------------------------------------------------------------------
        // Themed accessories. Kept compact: each line is a real prop that the
        // rig's section-15 architecture expects, not decoration-only filler.
        // ------------------------------------------------------------------
        private static void AddThemedParts(GameDefinition game, Transform root, BuiltMiniGame b,
            Color primary, Color accent, float s)
        {
            switch (game.Slug)
            {
                case "socket_trackball":
                    Ring(root, "SocketCup", new Vector3(0f, 0.022f, 0f), new Vector3(s * 1.25f, 1f, s * 1.25f), new Color32(70, 76, 90, 255), 0.8f);
                    break;
                case "clam_pearl_guard":
                    ShellHalf(root, new Vector3(-s * 0.42f, 0.03f, 0f), s * 0.82f, primary, -28f);
                    ShellHalf(root, new Vector3(s * 0.42f, 0.03f, 0f), s * 0.82f, primary, 28f);
                    break;
                case "bernoulli_levitator":
                    Cylinder(root, "Nozzle", new Vector3(0f, 0.07f, 0f), 0.012f, 0.11f, new Color32(190, 150, 68, 255), 0.9f);
                    Cylinder(root, "AirColumn", new Vector3(0f, 0.17f, 0f), 0.017f, 0.13f, accent, 0f, true);
                    break;
                case "micro_planet_scanner":
                    Ring(root, "ScanRingA", new Vector3(0f, b.SizeMeters * 0.55f, 0f), new Vector3(s * 1.6f, 1f, s * 1.6f), accent, 1.5f, 25f);
                    Ring(root, "ScanRingB", new Vector3(0f, b.SizeMeters * 0.55f, 0f), new Vector3(s * 1.35f, 1f, s * 1.35f), accent, 1.2f, -40f);
                    SmallSphere(root, "Hologram", new Vector3(0f, s * 1.35f, 0f), s * 0.18f, accent, 2f);
                    break;
                case "cage_pinball":
                    for (int i = 0; i < 3; i++)
                        Ring(root, "CageRing" + i, new Vector3(0f, s * 0.5f, 0f), new Vector3(s * 1.45f, 1f, s * 1.45f), new Color32(210, 220, 240, 255), 0.4f, i * 60f);
                    WireCage(root, s, accent);
                    break;
                case "ocular_turret":
                    Disk(root, "Iris", new Vector3(0f, s * 0.55f, -s * 0.42f), s * 0.52f, accent, 1.6f, 90f);
                    Beam(root, new Vector3(0f, s * 0.55f, -s * 0.45f), accent, s * 1.8f);
                    break;
                case "fusion_core_stabilizer":
                    Ring(root, "FusionRingA", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.55f, 1f, s * 1.55f), accent, 1.8f, 20f);
                    Ring(root, "FusionRingB", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.4f, 1f, s * 1.4f), accent, 1.4f, -55f);
                    break;
                case "scarab_canyon":
                    Box(root, "CanyonWall", new Vector3(0f, 0.035f, 0f), new Vector3(s * 2.4f, 0.02f, s * 0.55f), new Color32(150, 120, 80, 255), 0.3f);
                    SmallSphere(root, "Scarab", new Vector3(s * 1.05f, 0.045f, 0f), s * 0.16f, accent, 1.1f);
                    break;
                case "chrono_sphere_scrub":
                    Disk(root, "ChronoDial", new Vector3(0f, 0.028f, 0f), s * 1.8f, accent, 1.1f);
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * 30f * Mathf.Deg2Rad;
                        Box(root, "Tick" + i, new Vector3(Mathf.Cos(a) * s * 0.8f, 0.03f, Mathf.Sin(a) * s * 0.8f),
                            new Vector3(0.006f, 0.004f, 0.02f), accent, 1.4f);
                    }
                    break;
                case "electron_deflector":
                    AddAtomRings(root, s, accent);
                    break;
                case "atlas_defense":
                    Continent(root, s, accent);
                    Ring(root, "ShieldOrbit", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.9f, 1f, s * 1.9f), accent, 1.5f, 18f);
                    break;
                case "pulse_core_reactor":
                    for (int i = 1; i <= 3; i++)
                        Ring(root, "PulseRing" + i, new Vector3(0f, s * 0.55f, 0f), new Vector3(s * (1f + i * 0.3f), 1f, s * (1f + i * 0.3f)), accent, 1.2f + i * 0.2f, i * 33f);
                    break;
                case "katamari_harvest":
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i * 36f * Mathf.Deg2Rad;
                        SmallBox(root, "Debris" + i, new Vector3(Mathf.Cos(a) * s * 0.42f, 0.04f + (i % 3) * s * 0.18f, Mathf.Sin(a) * s * 0.42f),
                            Vector3.one * s * 0.08f, accent, 1.1f);
                    }
                    break;
                case "mystic_seer":
                    Aura(root, s, accent);
                    break;
                case "equatorial_particle_ring":
                    Ring(root, "EquatorRing", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.7f, 1f, s * 1.7f), accent, 1.8f);
                    break;
                case "geyser_venting":
                    Cylinder(root, "Vent", new Vector3(0f, s * 0.58f, 0f), s * 0.12f, s * 0.4f, new Color32(220, 110, 70, 255), 0.2f, true);
                    break;
                case "trench_run_globe":
                    Ring(root, "TrenchRoute", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.55f, 1f, s * 1.55f), accent, 1.2f, 35f);
                    SmallBox(root, "TrenchShip", new Vector3(s * 0.8f, s * 0.4f, 0f), new Vector3(s * 0.16f, s * 0.05f, s * 0.06f), accent, 1.8f);
                    break;
                case "containment_dribble":
                    Cylinder(root, "Shaft", new Vector3(0f, s * 0.4f, 0f), s * 0.35f, s * 0.8f, new Color32(80, 190, 230, 255), 0.1f, true);
                    break;
                case "bio_sphere_terraformer":
                    Continent(root, s, new Color32(120, 240, 140, 255));
                    Cylinder(root, "Stem", new Vector3(0f, s * 1.05f, 0f), s * 0.03f, s * 0.25f, new Color32(110, 80, 50, 255), 0.1f);
                    break;
                case "gyro_sphere_mech":
                    AddGimbal(root, s, accent);
                    break;
                case "sub_surface_hovercraft":
                    Ring(root, "Skirt", new Vector3(0f, b.SizeMeters * 0.2f, 0f), new Vector3(s * 1.35f, 1f, s * 1.35f), new Color32(160, 230, 255, 255), 1.2f);
                    break;
                case "weeble_sumo":
                    Ring(root, "Dohyo", new Vector3(0f, 0.026f, 0f), new Vector3(s * 2.4f, 1f, s * 2.4f), accent, 0.8f);
                    Ring(root, "Belt", new Vector3(0f, s * 0.42f, 0f), new Vector3(s * 0.78f, 1f, s * 0.78f), accent, 1.3f);
                    break;
                case "worry_stone_spirit":
                    Aura(root, s * 0.8f, accent);
                    break;
                case "scarab_buckler":
                    Ring(root, "BucklerRim", new Vector3(0f, 0.032f, 0f), new Vector3(s * 1.15f, 1f, s * 1.15f), accent, 1.2f);
                    break;
                case "bobsled_chute":
                    Box(root, "Chute", new Vector3(0f, 0.03f, 0f), new Vector3(s * 3.2f, 0.025f, s * 0.8f), new Color32(80, 90, 110, 255), 0.6f).transform.localRotation = Quaternion.Euler(0f, 0f, 8f);
                    break;
                case "vortex_wobbler":
                    Cylinder(root, "TopStem", new Vector3(0f, s * 1.05f, 0f), s * 0.045f, s * 0.35f, accent, 0.5f);
                    Disk(root, "WobbleDisc", new Vector3(0f, 0.032f, 0f), s * 1.7f, accent, 1f);
                    break;
                case "elliptical_orbit":
                    Orbit(root, s, accent);
                    break;
                case "trilobite_march":
                    for (int i = 0; i < 5; i++)
                        SmallSphere(root, "Segment" + i, new Vector3(-s * 0.5f + i * s * 0.24f, s * 0.25f, 0f), s * 0.16f, accent, 1.1f);
                    break;
                case "phantom_stone_skipper":
                    Box(root, "Water", new Vector3(0f, 0.025f, 0f), new Vector3(s * 4f, 0.01f, s * 2.2f), new Color32(70, 170, 235, 255), 0.1f, true);
                    break;
                case "paddlewheel_pod":
                    Cylinder(root, "WheelL", new Vector3(-s * 0.65f, s * 0.4f, 0f), s * 0.32f, s * 0.06f, accent, 0.5f).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Cylinder(root, "WheelR", new Vector3(s * 0.65f, s * 0.4f, 0f), s * 0.32f, s * 0.06f, accent, 0.5f).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    break;
                case "phantom_shell_game":
                    for (int i = 0; i < 3; i++)
                        ShellHalf(root, new Vector3((i - 1) * s * 0.95f, 0.032f, 0f), s * 0.68f, primary, 0f);
                    SmallSphere(root, "RubyPearl", new Vector3(0f, s * 0.16f, 0f), s * 0.12f, new Color32(255, 50, 90, 255), 1.8f);
                    break;
                case "micro_wok_toss":
                    Cylinder(root, "WokHandle", new Vector3(0f, s * 0.55f, s * 0.42f), s * 0.035f, s * 0.5f, new Color32(110, 70, 40, 255), 0.2f).transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
                    break;
                case "snapping_turtle_bunker":
                    ShellHalf(root, "TurtleHead", new Vector3(0f, 0.04f, -s * 0.55f), s * 0.22f, new Color32(150, 190, 120, 255), 0f);
                    break;
                case "deep_space_dish":
                    Cylinder(root, "FeedHorn", new Vector3(0f, s * 1.0f, 0f), s * 0.06f, s * 0.32f, accent, 0.8f);
                    Beam(root, new Vector3(0f, s * 0.85f, 0f), accent, s * 1.2f);
                    break;
                case "bey_cap_clash":
                    Ring(root, "Arena", new Vector3(0f, 0.028f, 0f), new Vector3(s * 2.5f, 1f, s * 2.5f), accent, 1.2f);
                    break;
                case "tardigrade_pond":
                    Disk(root, "WaterSurface", new Vector3(0f, 0.052f, 0f), s * 1.5f, new Color32(90, 200, 240, 255), 1.1f);
                    SmallSphere(root, "Tardigrade", new Vector3(s * 0.35f, 0.055f, 0f), s * 0.08f, accent, 1.4f);
                    break;
                case "paratrooper_basket":
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i * 60f * Mathf.Deg2Rad;
                        Cylinder(root, "Net" + i, new Vector3(Mathf.Cos(a) * s * 0.36f, s * 0.4f, Mathf.Sin(a) * s * 0.36f), 0.003f, s * 0.55f, accent, 0.5f);
                    }
                    break;
                case "micro_mortar_alchemist":
                    Cylinder(root, "Pestle", new Vector3(s * 0.35f, s * 0.75f, 0f), s * 0.045f, s * 0.45f, new Color32(150, 140, 125, 255), 0.2f);
                    SmallSphere(root, "Golem", new Vector3(s * 1.0f, s * 0.2f, 0f), s * 0.12f, accent, 1f);
                    break;
                case "limpet_suction":
                    Box(root, "Reef", new Vector3(0f, 0.02f, 0f), new Vector3(s * 1.8f, 0.04f, s * 1.2f), new Color32(110, 105, 95, 255), 0.3f);
                    break;
                case "temple_bell":
                    Cylinder(root, "Mallet", new Vector3(s * 0.8f, s * 0.6f, 0f), s * 0.035f, s * 0.4f, new Color32(190, 150, 90, 255), 0.3f);
                    break;
                case "galaxy_cauldron":
                    Cylinder(root, "Broth", new Vector3(0f, b.SizeMeters * 0.48f, 0f), s * 0.36f, 0.003f, new Color32(50, 60, 160, 255), 0.2f, true);
                    StarField(root, s * 0.55f, accent);
                    break;
                case "singing_bowl_sanctuary":
                    Cylinder(root, "Mallet", new Vector3(s * 0.8f, s * 0.45f, 0f), s * 0.028f, s * 0.35f, new Color32(200, 170, 110, 255), 0.2f);
                    Aura(root, s * 0.9f, accent);
                    break;
                case "fingerboard_bowl":
                    SmallBox(root, "Skater", new Vector3(0f, 0.045f, 0f), new Vector3(s * 0.18f, s * 0.04f, s * 0.08f), accent, 1.5f);
                    break;
                case "observatory_dome":
                    Box(root, "ShutterLeft", new Vector3(-s * 0.11f, s * 0.45f, 0f), new Vector3(s * 0.12f, s * 0.55f, s * 0.06f), accent, 1.4f);
                    Box(root, "ShutterRight", new Vector3(s * 0.11f, s * 0.45f, 0f), new Vector3(s * 0.12f, s * 0.55f, s * 0.06f), accent, 1.4f);
                    Cylinder(root, "Telescope", new Vector3(0f, s * 0.55f, 0f), s * 0.06f, s * 0.65f, new Color32(180, 195, 240, 255), 0.7f).transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
                    break;
                case "medusa_jellyfish":
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i * 60f * Mathf.Deg2Rad;
                        Cylinder(root, "Tentacle" + i, new Vector3(Mathf.Cos(a) * s * 0.22f, s * 0.35f, Mathf.Sin(a) * s * 0.22f), s * 0.012f, s * 0.55f, accent, 0.2f, true);
                    }
                    break;
                case "kraken_broth":
                    for (int i = 0; i < 4; i++)
                    {
                        float a = i * 90f * Mathf.Deg2Rad;
                        Cylinder(root, "TentacleCoil" + i, new Vector3(Mathf.Cos(a) * s * 0.34f, s * 0.45f, Mathf.Sin(a) * s * 0.34f), s * 0.03f, s * 0.6f, accent, 0.3f, true).transform.localRotation = Quaternion.Euler(28f, i * 90f, 0f);
                    }
                    break;
                case "foghorn_buoy":
                    Cylinder(root, "Beacon", new Vector3(0f, s * 1.05f, 0f), s * 0.06f, s * 0.18f, accent, 2f);
                    break;
                case "arecibo_reflector":
                    Cylinder(root, "Receiver", new Vector3(0f, s * 1.15f, 0f), s * 0.05f, s * 0.35f, accent, 0.8f);
                    Beam(root, new Vector3(0f, s * 0.9f, 0f), accent, s * 1.3f);
                    for (int i = 0; i < 3; i++)
                        SmallBox(root, "Rover" + (char)(65 + i), new Vector3((i - 1) * s * 0.5f, 0.038f, s * 0.35f), Vector3.one * s * 0.06f, accent, 1.2f);
                    break;
                case "tortoise_bastion":
                    Cylinder(root, "Cannon", new Vector3(0f, s * 0.6f, -s * 0.25f), s * 0.05f, s * 0.4f, new Color32(110, 130, 90, 255), 0.5f).transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
                    break;
                case "buckler_deflector":
                    Ring(root, "BucklerRim", new Vector3(0f, 0.032f, 0f), new Vector3(s * 1.18f, 1f, s * 1.18f), accent, 1.5f);
                    Beam(root, new Vector3(0f, s * 0.55f, s * 0.35f), accent, s * 1.6f);
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Small builders
        // ------------------------------------------------------------------
        private static Transform Prim(PrimitiveType type, Transform parent, string name, Vector3 pos, Vector3 scale, Color color, float metallic = 0.1f, float smooth = 0.4f, bool emissive = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = emissive
                ? MaterialLibrary.Glow(color, 1.4f)
                : MaterialLibrary.Get(color, metallic, smooth, color * 0.05f);
            var c = go.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
            return go.transform;
        }

        private static Transform SmallSphere(Transform parent, string name, Vector3 pos, float diameter, Color color, float glow = 1f)
            => Prim(PrimitiveType.Sphere, parent, name, pos, Vector3.one * diameter, color, 0.1f, 0.7f, glow > 1f);

        private static Transform Cylinder(Transform parent, string name, Vector3 pos, float diameter, float height, Color color, float metallic = 0.1f, bool emissive = false)
            => Prim(PrimitiveType.Cylinder, parent, name, pos, new Vector3(diameter, height * 0.5f, diameter), color, metallic, 0.4f, emissive);

        private static Transform Box(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, float metallic = 0.1f, bool emissive = false)
            => Prim(PrimitiveType.Cube, parent, name, pos, scale, color, metallic, 0.4f, emissive);

        private static Transform SmallBox(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, float glow = 1f)
            => Box(parent, name, pos, scale, color, 0.1f, glow > 1f);

        private static Transform MeshPrim(Transform parent, string name, Mesh mesh, Vector3 pos, Vector3 scale, Quaternion rot, Color color, bool glow = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localRotation = rot;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().material = glow ? MaterialLibrary.Glow(color, 1.4f) : MaterialLibrary.Get(color, 0.2f, 0.5f, color * 0.06f);
            return go.transform;
        }

        private static Transform Ring(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, float glow = 1f, float tiltX = 0f)
            => MeshPrim(parent, name, ProceduralMesh.Ring, pos, scale, Quaternion.Euler(0f, 0f, tiltX), color, glow > 1f);

        private static Transform Disk(Transform parent, string name, Vector3 pos, float diameter, Color color, float glow = 1f, float tiltX = 0f)
            => MeshPrim(parent, name, ProceduralMesh.BuildDisc(32), pos, new Vector3(diameter, 1f, diameter), Quaternion.Euler(tiltX, 0f, 0f), color, glow > 1f);

        private static Transform ShellHalf(Transform parent, Vector3 pos, float diameter, Color color, float tiltZ)
            => ShellHalf(parent, "ShellHalf", pos, diameter, color, tiltZ);

        private static Transform ShellHalf(Transform parent, string name, Vector3 pos, float diameter, Color color, float tiltZ)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(diameter, diameter * 0.55f, diameter);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, tiltZ);
            go.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Dome;
            go.AddComponent<MeshRenderer>().material = MaterialLibrary.Get(color, 0.25f, 0.6f, color * 0.08f);
            return go.transform;
        }

        private static void Aura(Transform root, float diameter, Color accent)
        {
            var go = new GameObject("Aura");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            go.transform.localScale = new Vector3(diameter, 1f, diameter);
            go.AddComponent<MeshFilter>().sharedMesh = ProceduralMesh.Torus;
            go.AddComponent<MeshRenderer>().material = MaterialLibrary.Transparent(accent, 0.35f);
        }

        private static void StarField(Transform root, float radius, Color accent)
        {
            for (int i = 0; i < 14; i++)
            {
                float a = (i * 137.5f) * Mathf.Deg2Rad;
                float r = radius * Mathf.Sqrt((i % 7) / 7f);
                SmallSphere(root, "Star" + i, new Vector3(Mathf.Cos(a) * r, 0.08f + (i % 3) * 0.02f, Mathf.Sin(a) * r), 0.0045f, accent, 1.8f);
            }
        }

        private static void WireCage(Transform root, float s, Color accent)
        {
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                var bar = Cylinder(root, "CageBar" + i, new Vector3(Mathf.Cos(a) * s * 0.72f, s * 0.5f, Mathf.Sin(a) * s * 0.72f),
                    s * 0.012f, s * 1.0f, accent, 0.3f, false);
                bar.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

        private static void AddAtomRings(Transform root, float s, Color accent)
        {
            Ring(root, "AtomRingA", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.8f, 1f, s * 1.8f), accent, 1.4f, 20f);
            Ring(root, "AtomRingB", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.8f, 1f, s * 1.8f), accent, 1.4f, 80f);
            Ring(root, "AtomRingC", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.8f, 1f, s * 1.8f), accent, 1.4f, 140f);
        }

        private static void AddGimbal(Transform root, float s, Color accent)
        {
            Ring(root, "GimbalA", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.55f, 1f, s * 1.55f), accent, 1.2f, 25f);
            Ring(root, "GimbalB", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 1.55f, 1f, s * 1.55f), accent, 1.2f, -35f);
        }

        private static void Orbit(Transform root, float s, Color accent)
        {
            Ring(root, "OrbitRing", new Vector3(0f, s * 0.55f, 0f), new Vector3(s * 2.2f, 1f, s * 2.2f), accent, 1.5f, 15f);
            SmallSphere(root, "FocusA", new Vector3(-s * 0.55f, s * 0.55f, 0f), s * 0.1f, accent, 2f);
            SmallSphere(root, "FocusB", new Vector3(s * 0.55f, s * 0.55f, 0f), s * 0.1f, accent, 2f);
            SmallSphere(root, "Satellite", new Vector3(0f, s * 0.55f, s * 1.05f), s * 0.12f, Color.white, 1.6f);
        }

        private static void Continent(Transform root, float s, Color color)
        {
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                SmallBox(root, "Land" + i, new Vector3(Mathf.Cos(a) * s * 0.3f, s * 0.6f + Mathf.Sin(a * 1.7f) * s * 0.22f, Mathf.Sin(a) * s * 0.3f),
                    new Vector3(s * 0.22f, s * 0.03f, s * 0.16f), color, 1.1f);
            }
        }

        private static void Beam(Transform root, Vector3 pos, Color color, float length)
        {
            var go = new GameObject("Beam");
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, new Vector3(0f, length, 0f));
            lr.startWidth = 0.006f;
            lr.endWidth = 0.001f;
            lr.material = MaterialLibrary.Unlit(color);
            lr.startColor = color;
            lr.endColor = new Color(color.r, color.g, color.b, 0.1f);
        }
    }
}



