using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MetaXRSim.App
{
    public struct RigBindResult
    {
        public Component Rig;
        public int BoundTransforms;
        public List<Transform> Parts;
        public string Error;
        public bool Success => Rig != null && string.IsNullOrEmpty(Error);
    }

    /// <summary>
    /// Attaches the original section-15 rig for a game and resolves its serialized
    /// Transform fields against the procedurally built board and the live hand
    /// proxies. This is what makes the 50 independently gated games run inside the
    /// unified application without editing 50 rig scripts.
    /// </summary>
    public static class RigBinder
    {
        public static RigBindResult Bind(GameObject host, BuiltMiniGame built, GameDefinition game,
            HandInputService input)
        {
            var result = new RigBindResult();
            if (host == null || game == null) { result.Error = "Missing host or game"; return result; }

            Type rigType = GameCatalog.GetRigType(game.Slug);
            if (rigType == null) rigType = Type.GetType(game.RigTypeName);
            if (rigType == null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    rigType = asm.GetType(game.RigTypeName);
                    if (rigType != null) break;
                }
            }
            if (rigType == null)
            {
                result.Error = "Rig type not found: " + game.RigTypeName;
                return result;
            }
            if (!typeof(MonoBehaviour).IsAssignableFrom(rigType))
            {
                result.Error = "Rig is not a MonoBehaviour: " + game.RigTypeName;
                return result;
            }

            Component rig;
            try
            {
                rig = host.AddComponent(rigType);
            }
            catch (Exception e)
            {
                result.Error = "AddComponent failed: " + e.Message;
                return result;
            }
            if (rig == null) { result.Error = "AddComponent returned null"; return result; }

            var nameMap = BuildNameMap(built.Root.transform);
            int bound = 0;
            var parts = new List<Transform>();

            foreach (var field in AllFields(rigType))
            {
                if (field.IsStatic) continue;
                if (field.FieldType == typeof(Transform))
                {
                    var t = Resolve(field.Name, built, input, nameMap, rig.transform);
                    if (t != null)
                    {
                        try
                        {
                            field.SetValue(rig, t);
                            bound++;
                            if (t != built.Root.transform && t != built.Prop && t.IsChildOf(built.Root.transform) && !parts.Contains(t))
                                parts.Add(t);
                        }
                        catch (Exception) { }
                    }
                }
                else if (field.FieldType == typeof(Transform[]))
                {
                    var arr = ResolveArray(field.Name, built, input, nameMap);
                    if (arr != null)
                    {
                        try { field.SetValue(rig, arr); bound++; }
                        catch (Exception) { }
                    }
                }
            }

            result.Rig = rig;
            result.BoundTransforms = bound;
            result.Parts = parts;
            return result;
        }

        private static IEnumerable<FieldInfo> AllFields(Type type)
        {
            var seen = new HashSet<string>();
            while (type != null && type != typeof(MonoBehaviour) && type != typeof(object))
            {
                foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (seen.Add(f.Name)) yield return f;
                }
                type = type.BaseType;
            }
        }

        private static Dictionary<string, Transform> BuildNameMap(Transform root)
        {
            var map = new Dictionary<string, Transform>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                string key = Normalize(t.name);
                if (!string.IsNullOrEmpty(key) && !map.ContainsKey(key)) map[key] = t;
            }
            return map;
        }

        private static Transform Resolve(string fieldName, BuiltMiniGame built, HandInputService input,
            Dictionary<string, Transform> nameMap, Transform hostRoot)
        {
            string n = Normalize(fieldName);

            // Hands first.
            bool left = n.Contains("left") || n.Contains("nondominant") || n.Contains("secondary");
            bool right = n.Contains("right") || n.Contains("dominant") || n.Contains("primary");
            if (n.Contains("palm") || n.Contains("hand") || n.Contains("wrist"))
            {
                if (n.Contains("wrist")) return left ? input.LeftWrist : right ? input.RightWrist : input.RightWrist;
                if (n.Contains("palm")) return left ? input.LeftPalm : right ? input.RightPalm : input.RightPalm;
                return left ? input.LeftPalm : input.RightPalm;
            }
            if (n.Contains("index") || n.Contains("finger") || n.Contains("tap"))
                return left ? input.LeftIndexTip : input.RightIndexTip;
            if (n.Contains("thumb")) return left ? input.LeftThumbTip : input.RightThumbTip;

            // Exact/derived board part lookup.
            if (built.Parts.TryGetValue(n, out var exact)) return exact;

            string[] tokens = Tokenize(n);
            for (int i = tokens.Length - 1; i >= 0; i--)
            {
                if (built.Parts.TryGetValue(tokens[i], out var t)) return t;
                if (nameMap.TryGetValue(tokens[i], out t)) return t;
            }

            // Partial-name fallback: head -> TurtleHead, net -> Net0, etc.
            for (int i = tokens.Length - 1; i >= 0; i--)
            {
                string tok = tokens[i];
                if (tok.Length < 4) continue;
                foreach (var kv in nameMap)
                {
                    if (kv.Key.Contains(tok) || tok.Contains(kv.Key)) return kv.Value;
                }
            }

            // Common aliases.
            switch (n)
            {
                case "ship": return built.Find("ship") ?? hostRoot;
                case "gazebeam": return built.Find("beam");
                case "beam": return built.Find("beam");
                case "moonhologram": return built.Find("hologram");
                case "readheadindex": return input.RightIndexTip;
                case "gazebeamtransform": return built.Find("beam");
                case "vishologram": return built.Find("hologram");
                case "nozzle": return nameMap.TryGetValue("nozzle", out var noz) ? noz : built.Prop;
                case "tray": return built.Base;
                case "ring": return nameMap.TryGetValue("dohyo", out var ring) ? ring :
                    (nameMap.TryGetValue("arena", out ring) ? ring : built.Base);
                case "stadium": return nameMap.TryGetValue("arena", out var arena) ? arena : built.Base;
                case "reef": return nameMap.TryGetValue("reef", out var reef) ? reef : built.Base;
                case "courtyard": return built.Base;
            }

            // Fall back to the prop for the clearly interactive names, the base
            // for environment names, and leave the rest null so the rig's own
            // null guards keep working.
            if (n.Contains("visual") || n.Contains("prop") || n.Contains("main") ||
                n.Contains("marble") || n.Contains("ball") || n.Contains("orb") ||
                n.Contains("core") || n.Contains("bell") || n.Contains("bowl") ||
                n.Contains("shell") || n.Contains("cap") || n.Contains("disk") ||
                n.Contains("disc") || n.Contains("stone") || n.Contains("egg") ||
                n.Contains("pod") || n.Contains("globe") || n.Contains("pearl") ||
                n.Contains("cap") || n.Contains("dish") || n.Contains("buoy"))
                return built.Prop;

            if (n.Contains("base") || n.Contains("stand") || n.Contains("track") ||
                n.Contains("floor") || n.Contains("ring") || n.Contains("frame") ||
                n.Contains("tray") || n.Contains("stadium") || n.Contains("reef"))
                return built.Base;

            return null;
        }

        private static Transform[] ResolveArray(string fieldName, BuiltMiniGame built, HandInputService input,
            Dictionary<string, Transform> nameMap)
        {
            string n = Normalize(fieldName);
            bool left = n.Contains("left");
            if (n.Contains("fingertip"))
            {
                return left ? input.LeftFingerTips : input.RightFingerTips;
            }
            if (n.Contains("tentacle"))
            {
                var list = new List<Transform>();
                foreach (var kv in nameMap)
                    if (kv.Key.Contains("tentacle")) list.Add(kv.Value);
                if (list.Count == 0) return null;
                return list.ToArray();
            }
            return null;
        }

        private static string Normalize(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            string n = name.ToLowerInvariant();
            n = n.Replace("_", "").Replace(" ", "");
            foreach (var suffix in new[] { "transform", "transforms", "array", "go", "gameobject" })
                if (n.EndsWith(suffix)) n = n.Substring(0, n.Length - suffix.Length);
            return n;
        }

        private static string[] Tokenize(string camel)
        {
            if (string.IsNullOrEmpty(camel)) return new string[0];
            var parts = new List<string>();
            int start = 0;
            for (int i = 1; i < camel.Length; i++)
            {
                if (char.IsUpper(camel[i]) && char.IsLower(camel[i - 1]) ||
                    char.IsLower(camel[i]) && char.IsUpper(camel[i - 1]))
                {
                    parts.Add(camel.Substring(start, i - start).ToLowerInvariant());
                    start = i;
                }
            }
            parts.Add(camel.Substring(start).ToLowerInvariant());
            return parts.ToArray();
        }
    }
}
