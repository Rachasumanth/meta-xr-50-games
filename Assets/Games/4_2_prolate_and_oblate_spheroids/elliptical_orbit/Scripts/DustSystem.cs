using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.EllipticalOrbit
{
    public class NudgeClassifier
    {
        public int Tokens = 6;

        public bool TrySpendToken()
        {
            if (Tokens > 0)
            {
                Tokens--;
                return true;
            }
            return false;
        }
    }

    public class DustParticle
    {
        public Vector2 PositionMm;
        public bool IsCaptured = false;

        public DustParticle(Vector2 pos)
        {
            PositionMm = pos;
        }
    }

    public class DustSystem
    {
        public List<DustParticle> Particles { get; } = new List<DustParticle>();
        public int CapturedCount = 0;

        public void Seed(int count)
        {
            Particles.Clear();
            CapturedCount = 0;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                Particles.Add(new DustParticle(new Vector2(Mathf.Cos(angle) * 80f, Mathf.Sin(angle) * 50f)));
            }
        }

        public bool CheckCapture(Vector2 eggPosMm, float captureRadiusMm = EllipticalOrbitConfig.DustCaptureRadiusMm)
        {
            bool anyCaptured = false;
            foreach (var p in Particles)
            {
                if (p.IsCaptured) continue;
                float dist = Vector2.Distance(eggPosMm, p.PositionMm);
                if (dist <= captureRadiusMm)
                {
                    p.IsCaptured = true;
                    CapturedCount++;
                    anyCaptured = true;
                }
            }
            return anyCaptured;
        }
    }
}
