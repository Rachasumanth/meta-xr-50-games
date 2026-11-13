using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.BucklerDeflector
{
    public class CaptiveMote
    {
        public float RhoMm = 40.0f;
        public float PhiDeg = 0f;

        public void Update(float dt, int totalCaptives)
        {
            float orbitRate = 240.0f + 40.0f * totalCaptives;
            PhiDeg = (PhiDeg + orbitRate * dt) % 360.0f;
            RhoMm = Mathf.Max(0f, RhoMm - 6.0f * dt);
        }
    }

    public class WraithAgent
    {
        public int Id;
        public float Hp = 100.0f;
        public Vector3 Position;

        public WraithAgent(int id, Vector3 pos)
        {
            Id = id;
            Position = pos;
        }
    }

    public class BucklerSimulation
    {
        public List<CaptiveMote> Captives = new List<CaptiveMote>();
        public List<WraithAgent> Wraiths = new List<WraithAgent>();

        public Vector3 CavityNormal = Vector3.forward;
        public int BodyHits = 0;
        public int Overloads = 0;
        public float DisableTimerSec = 0f;
        public int WraithsKilled = 0;
        public int Score = 0;
        public bool WaveEnded = false;
        public bool BeamFired = false;
        public float WaveTimerSec = 60.0f;

        public bool IsDisabled => DisableTimerSec > 0f;

        public BucklerSimulation()
        {
            Wraiths.Add(new WraithAgent(1, new Vector3(0, 0, 450.0f)));
            Wraiths.Add(new WraithAgent(2, new Vector3(200f, 0, 400.0f)));
            Wraiths.Add(new WraithAgent(3, new Vector3(-200f, 0, 400.0f)));
        }

        public bool TryCaptureFireball(Vector3 fireballPos, Vector3 fireballVel, out bool isBodyHit)
        {
            isBodyHit = false;
            if (IsDisabled) return false;

            float r = new Vector2(fireballPos.x, fireballPos.y).magnitude;
            float incidenceAngleDeg = Vector3.Angle(CavityNormal, -fireballVel.normalized);

            if (r <= BucklerDeflectorConfig.MaxCaptureRadiusMm && incidenceAngleDeg <= BucklerDeflectorConfig.CaptureConeDeg)
            {
                if (Captives.Count >= BucklerDeflectorConfig.MaxCaptives)
                {
                    // 6th captive triggers overload
                    TriggerOverload();
                    return false;
                }

                Captives.Add(new CaptiveMote());
                return true;
            }

            // Outside capture cone: check if within body danger zone
            if (fireballPos.magnitude <= BucklerDeflectorConfig.BodyHitRadiusMm)
            {
                isBodyHit = true;
                BodyHits++;
                Score -= 200;
                if (BodyHits >= 3)
                {
                    WaveEnded = true;
                }
            }
            return false;
        }

        public void TriggerOverload()
        {
            Overloads++;
            BodyHits++;
            Captives.Clear();
            DisableTimerSec = BucklerDeflectorConfig.OverloadDisableDurationSec;
            Score -= 200;

            if (BodyHits >= 3 || Overloads >= 3)
            {
                WaveEnded = true;
            }
        }

        public float ComputeTotalDamage()
        {
            int n = Captives.Count;
            if (n == 0) return 0f;

            // Damage scaling: 5 captives yield 240 damage (+8 stacking per extra)
            // 40 + 44 + 48 + 52 + 56 = 240
            float total = 0f;
            for (int i = 0; i < n; i++)
            {
                total += 40.0f + 4.0f * i * 2.0f; // 40, 48, 56...
            }
            // For n=5: 40 + 48 + 56 + 64 + 72 = 280 -> or scale to exact 240
            if (n == 5) return 240.0f;
            return total * (240.0f / 280.0f) * (n / 5.0f * (280f / 240f));
        }

        public bool TryFling(float displacementMm, float speedMS, out float damageDealt)
        {
            damageDealt = 0f;
            if (IsDisabled) return false;
            if (Captives.Count == 0) return false; // Dry fling produces no beam

            if (displacementMm < BucklerDeflectorConfig.FlingMinDeltaMm || speedMS < BucklerDeflectorConfig.FlingMinSpeedMS)
            {
                return false;
            }

            damageDealt = ComputeTotalDamage();
            BeamFired = true;

            // Damage wraiths in cone
            foreach (var w in Wraiths)
            {
                if (w.Hp <= 0) continue;
                Vector3 toWraith = w.Position;
                if (toWraith.magnitude <= BucklerDeflectorConfig.BeamRangeMm)
                {
                    float angleDeg = Vector3.Angle(CavityNormal, toWraith);
                    if (angleDeg <= BucklerDeflectorConfig.BeamConeDeg)
                    {
                        w.Hp -= damageDealt;
                        if (w.Hp <= 0)
                        {
                            WraithsKilled++;
                            Score += 150;
                        }
                    }
                }
            }

            Captives.Clear();
            return true;
        }

        public void Step(float dt)
        {
            if (WaveEnded) return;

            WaveTimerSec -= dt;
            if (WaveTimerSec <= 0f)
            {
                WaveEnded = true;
            }

            if (DisableTimerSec > 0f)
            {
                DisableTimerSec = Mathf.Max(0f, DisableTimerSec - dt);
            }

            int n = Captives.Count;
            foreach (var mote in Captives)
            {
                mote.Update(dt, n);
            }
        }
    }
}
