using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Small material/tone factory. Keeps the 50 procedurally built games
    /// visually consistent and avoids instantiating hundreds of materials.
    /// </summary>
    public static class MaterialLibrary
    {
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        private static Shader _standard;
        private static Font _font;

        public static Shader StandardShader
        {
            get
            {
                if (_standard == null)
                {
                    _standard = Shader.Find("Standard");
                    if (_standard == null) _standard = Shader.Find("Universal Render Pipeline/Lit");
                    if (_standard == null) _standard = Shader.Find("Legacy Shaders/Diffuse");
                    if (_standard == null) _standard = Shader.Find("Unlit/Color");
                    if (_standard == null) _standard = Shader.Find("Sprites/Default");
                }
                return _standard;
            }
        }

        public static Font UiFont
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null)
                    {
                        try { _font = Font.CreateDynamicFontFromOSFont("Segoe UI", 64); }
                        catch { }
                    }
                }
                return _font;
            }
        }

        public static Material Get(Color albedo, float metallic = 0.05f, float smoothness = 0.45f, Color? emission = null)
        {
            string key = ColorUtility.ToHtmlStringRGBA(albedo) + "|" + metallic.ToString("F2") + "|" +
                         smoothness.ToString("F2") + "|" + (emission.HasValue ? ColorUtility.ToHtmlStringRGBA(emission.Value) : "0");
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var mat = new Material(StandardShader) { name = "M_" + key };
            if (mat.HasProperty("_Color")) mat.color = albedo;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
            }
            Cache[key] = mat;
            return mat;
        }

        public static Material Transparent(Color albedo, float alpha = 0.4f, float smoothness = 0.7f)
        {
            Color c = albedo;
            c.a = alpha;
            string key = "T|" + ColorUtility.ToHtmlStringRGBA(c) + "|" + smoothness.ToString("F2");
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = StandardShader;
            var mat = new Material(shader) { name = "MT_" + key };
            if (mat.HasProperty("_Color")) mat.color = c;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            mat.renderQueue = 3000;
            Cache[key] = mat;
            return mat;
        }

        public static Material Glow(Color color, float intensity = 2f)
        {
            Color e = new Color(color.r * intensity, color.g * intensity, color.b * intensity, 1f);
            return Get(color, 0f, 0.9f, e);
        }

        public static Material Unlit(Color color)
        {
            string key = "U|" + ColorUtility.ToHtmlStringRGBA(color);
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = StandardShader;
            var mat = new Material(shader) { name = "MU_" + key };
            if (mat.HasProperty("_Color")) mat.color = color;
            Cache[key] = mat;
            return mat;
        }

        public static Material Sprite(Color color, bool additive = false)
        {
            string key = "SP|" + (additive ? "A|" : "N|") + ColorUtility.ToHtmlStringRGBA(color);
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = StandardShader;
            var mat = new Material(shader) { name = "MSP_" + key };
            if (mat.HasProperty("_Color")) mat.color = color;
            Cache[key] = mat;
            return mat;
        }

        public static void ClearCache()
        {
            Cache.Clear();
        }
    }
}


