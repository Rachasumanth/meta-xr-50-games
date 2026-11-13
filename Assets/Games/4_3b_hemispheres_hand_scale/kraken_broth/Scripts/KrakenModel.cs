using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.KrakenBroth
{
    public enum QuadrantId
    {
        QF, // Front 315° to 45° (bisector 0°)
        QL, // Left 45° to 135° (bisector 90°)
        QB, // Back 135° to 225° (bisector 180°)
        QR  // Right 225° to 315° (bisector 270°)
    }

    public class Ship
    {
        public int Id;
        public float LambdaDeg; // azimuth around rim
        public int Hp = 1;
        public bool IsAlive = true;
        public float LapProgressDeg = 0f;

        public Ship(int id, float lambdaDeg, int hp = 1)
        {
            Id = id;
            LambdaDeg = lambdaDeg;
            Hp = hp;
        }

        public bool Advance(float deltaDeg)
        {
            LapProgressDeg += deltaDeg;
            LambdaDeg = (LambdaDeg + deltaDeg) % 360f;
            if (LambdaDeg < 0f) LambdaDeg += 360f;

            if (LapProgressDeg >= 360f)
            {
                return true; // Escaped!
            }
            return false;
        }
    }

    public class QuadrantState
    {
        public QuadrantId Id;
        public float BisectorDeg;
        public float Charge = 0f;
        public bool Armed = false;
        public float ArmTimer = 0f;
        public float CooldownTimer = 0f;
        public float LockedSectorDeg = 0f;
        public int LastFinger = -1;
        public float LastStrikeTime = -99f;

        public QuadrantState(QuadrantId id, float bisectorDeg)
        {
            Id = id;
            BisectorDeg = bisectorDeg;
        }
    }

    public class DrumClassifier
    {
        public static bool IsInBellyBand(float zMm)
        {
            return zMm >= KrakenBrothConfig.BellyBandZMinMm && zMm <= KrakenBrothConfig.BellyBandZMaxMm;
        }

        public static QuadrantId GetQuadrantWithHysteresis(float lambdaDeg, QuadrantId currentPrior)
        {
            float norm = (lambdaDeg % 360f + 360f) % 360f;
            float hyst = 4.0f; // +/- 4 deg hysteresis

            // Normal boundaries: 45, 135, 225, 315
            if (currentPrior == QuadrantId.QF)
            {
                if (norm >= 315f - hyst || norm <= 45f + hyst) return QuadrantId.QF;
            }
            else if (currentPrior == QuadrantId.QL)
            {
                if (norm >= 45f - hyst && norm <= 135f + hyst) return QuadrantId.QL;
            }
            else if (currentPrior == QuadrantId.QB)
            {
                if (norm >= 135f - hyst && norm <= 225f + hyst) return QuadrantId.QB;
            }
            else if (currentPrior == QuadrantId.QR)
            {
                if (norm >= 225f - hyst && norm <= 315f + hyst) return QuadrantId.QR;
            }

            // Default without hysteresis
            if (norm >= 315f || norm < 45f) return QuadrantId.QF;
            if (norm >= 45f && norm < 135f) return QuadrantId.QL;
            if (norm >= 135f && norm < 225f) return QuadrantId.QB;
            return QuadrantId.QR;
        }

        public static float ComputeStrikeEnergy(float vn)
        {
            // vn from 40 to 400 mm/s
            // AT-05 Pass: Charge +5.6 at 40 mm/s, +13.1 at 200 mm/s, +16.8 at 400 mm/s (+/- 0.5)
            float v = Mathf.Clamp(vn, 40f, 400f);
            if (v <= 200f)
            {
                float t = (v - 40f) / 160f;
                return Mathf.Lerp(5.6f, 13.1f, t);
            }
            else
            {
                float t = (v - 200f) / 200f;
                return Mathf.Lerp(13.1f, 16.8f, t);
            }
        }
    }

    public class KrakenSimulation
    {
        public Dictionary<QuadrantId, QuadrantState> Quadrants = new Dictionary<QuadrantId, QuadrantState>();
        public List<Ship> Fleet = new List<Ship>();

        public float Surge = 0f;
        public int Score = 0;
        public float BrothTimer = 55.0f;
        public int TargetQuota = 10;
        public int ShipsDestroyed = 0;
        public bool BrothCleared = false;
        public bool IsPaused = false;
        public float LiftZMm = 0f;

        public KrakenSimulation()
        {
            Quadrants[QuadrantId.QF] = new QuadrantState(QuadrantId.QF, 0f);
            Quadrants[QuadrantId.QL] = new QuadrantState(QuadrantId.QL, 90f);
            Quadrants[QuadrantId.QB] = new QuadrantState(QuadrantId.QB, 180f);
            Quadrants[QuadrantId.QR] = new QuadrantState(QuadrantId.QR, 270f);
        }

        public bool RecordStrike(QuadrantId qId, float vn, int finger, float nowTime, out float energyAdded)
        {
            energyAdded = 0f;
            var q = Quadrants[qId];
            if (q.CooldownTimer > 0f) return false;

            float baseEnergy = DrumClassifier.ComputeStrikeEnergy(vn);
            bool alt = (finger != q.LastFinger && q.LastFinger != -1) && (nowTime - q.LastStrikeTime >= 0.090f && nowTime - q.LastStrikeTime <= 0.260f);
            float multiplier = alt ? 1.15f : 1.0f;
            energyAdded = baseEnergy * multiplier;

            q.Charge = Mathf.Clamp(q.Charge + energyAdded, 0f, 100f);
            q.LastFinger = finger;
            q.LastStrikeTime = nowTime;

            if (q.Charge >= 100f && !q.Armed)
            {
                Arm(q);
            }
            return true;
        }

        public void Arm(QuadrantState q)
        {
            q.Armed = true;
            q.LockedSectorDeg = q.BisectorDeg;
            q.ArmTimer = KrakenBrothConfig.ArmTelegraphSec;
        }

        public int ResolveSlam(QuadrantState q, out float mult)
        {
            mult = 1.0f;
            List<Ship> killed = new List<Ship>();
            foreach (var s in Fleet)
            {
                if (!s.IsAlive) continue;
                float diff = Mathf.Abs(Mathf.DeltaAngle(s.LambdaDeg, q.LockedSectorDeg));
                if (diff <= 45.0f) // 90° sector
                {
                    s.Hp--;
                    if (s.Hp <= 0)
                    {
                        s.IsAlive = false;
                        killed.Add(s);
                    }
                }
            }

            int count = killed.Count;
            if (count == 0)
            {
                // Empty slam
                Surge = Mathf.Clamp(Surge + 5f, 0f, 100f);
                Score -= 40;
            }
            else
            {
                if (count == 1) mult = 1.0f;
                else if (count == 2) mult = 1.25f;
                else mult = 1.50f; // 3+ ships: 1.5 multiplier

                Score += Mathf.RoundToInt(count * 120 * mult);
                Surge = Mathf.Clamp(Surge - 6f, 0f, 100f);
                ShipsDestroyed += count;
            }

            q.Armed = false;
            q.Charge = 0f;
            q.CooldownTimer = KrakenBrothConfig.CooldownSec;

            if (ShipsDestroyed >= TargetQuota && !BrothCleared)
            {
                BrothCleared = true;
                int timeBonus = Mathf.RoundToInt(BrothTimer * 6.0f); // 12s -> +72
                Score += 400 + timeBonus;
            }

            return count;
        }

        public void Step(float dt, float liftMm = 0f)
        {
            LiftZMm = liftMm;
            if (liftMm > 100.0f) // Lift pause (e.g. 130 mm)
            {
                IsPaused = true;
                return;
            }
            IsPaused = false;

            if (BrothCleared) return;

            BrothTimer -= dt;
            // Passive surge: 2.2% per second at level 3
            Surge = Mathf.Clamp(Surge + KrakenBrothConfig.PassiveSurgeRatePerSecLvl3 * dt, 0f, 100f);

            foreach (var kvp in Quadrants)
            {
                var q = kvp.Value;
                if (q.CooldownTimer > 0f)
                {
                    q.CooldownTimer = Mathf.Max(0f, q.CooldownTimer - dt);
                }

                if (q.Armed)
                {
                    q.ArmTimer -= dt;
                    if (q.ArmTimer <= 0f)
                    {
                        ResolveSlam(q, out _);
                    }
                }
            }

            // Ship movement and escape
            foreach (var s in Fleet)
            {
                if (!s.IsAlive) continue;
                bool escaped = s.Advance(10.0f * dt);
                if (escaped)
                {
                    s.IsAlive = false;
                    Surge = Mathf.Clamp(Surge + 12f, 0f, 100f);
                    Score -= 150;
                }
            }
        }
    }
}
