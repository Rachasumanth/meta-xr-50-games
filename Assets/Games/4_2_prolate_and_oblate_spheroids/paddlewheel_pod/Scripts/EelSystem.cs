using System;
using UnityEngine;

namespace XRGames.Chapter4.PaddlewheelPod
{
    public class Eel
    {
        public float WindupTimeSec = 0.35f;
        public float LungeTravelMm = 40.0f;
        public float LungeSpeedMmS = 220.0f;
        public bool IsWindingUp = false;
        public bool IsLunging = false;

        public static string EvaluatePass(float missDistanceMm, out int hullDmg, out int steamDmg, out float invulnSec)
        {
            if (missDistanceMm <= 55.0f) // e.g. 50 mm
            {
                hullDmg = 1;
                steamDmg = 15;
                invulnSec = 1.2f;
                return "Hit";
            }
            else // e.g. 60 mm
            {
                hullDmg = 0;
                steamDmg = 0;
                invulnSec = 0f;
                return "Dodge";
            }
        }
    }

    public class KelpField
    {
        public static float ModifyDetectionRadius(float baseRadiusMm, bool insideKelp)
        {
            return insideKelp ? baseRadiusMm * 0.5f : baseRadiusMm;
        }

        public static float ModifyAscent(float vUpMmS, bool insideKelp)
        {
            return insideKelp ? vUpMmS * 0.75f : vUpMmS;
        }
    }

    public class PodModel
    {
        public int Hull = 3;
        public float Steam = 100.0f;
        public float DepthMm = 2000.0f; // 2000 mm depth
        public float InvulnerabilityTimer = 0f;
        public bool IsInKelp = false;

        public void ApplyEelHit(int hullDmg, int steamDmg, float invulnSec)
        {
            if (InvulnerabilityTimer > 0f) return;

            Hull = Mathf.Max(0, Hull - hullDmg);
            Steam = Mathf.Max(0f, Steam - steamDmg);
            InvulnerabilityTimer = invulnSec;
        }

        public void EnterKelp()
        {
            IsInKelp = true;
            Steam = Mathf.Max(0f, Steam - 3.0f); // 3 steam on entry
        }

        public void RegenerateSteam(float dt)
        {
            Steam = Mathf.Min(100.0f, Steam + 5.0f * dt);
        }

        public void Step(float vUpMmS, float steamDrain, float dt)
        {
            if (InvulnerabilityTimer > 0f)
            {
                InvulnerabilityTimer -= dt;
            }

            Steam = Mathf.Max(0f, Steam - steamDrain);

            // Ascent decreases depth
            DepthMm = Mathf.Max(0f, DepthMm - vUpMmS * dt);
        }
    }
}
