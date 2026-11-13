using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.MedusaJellyfish
{
    public enum GameState
    {
        Boot,
        Calibrate,
        Drift,
        Contract,
        Jet,
        Stun,
        Surface,
        DiveClear,
        Victory,
        Score,
        Pause
    }

    public enum OcclusionState
    {
        DirectVisible,
        DimmedVisible,
        Occluded
    }

    public class TentacleSegment
    {
        public Vector3 Position;
        public Vector3 PrevPosition;

        public TentacleSegment(Vector3 pos)
        {
            Position = pos;
            PrevPosition = pos;
        }
    }

    public class SeaMine
    {
        public Vector3 Position;
        public float RadiusMm = 12.0f;
        public bool Detonated = false;

        public SeaMine(Vector3 pos)
        {
            Position = pos;
        }
    }

    public class PalmPressSolver
    {
        public float MaxForceN = 8.0f;
        public float MaxCompressionMm = 6.0f;

        public float ComputeKappa(float leftForceN, float rightForceN)
        {
            float combined = leftForceN + rightForceN;
            return Mathf.Clamp01(combined / MaxForceN);
        }

        public float ComputeCompression(float kappa)
        {
            return kappa * MaxCompressionMm;
        }
    }

    public class JetModel
    {
        public const float VirtualMassKg = 0.120f;
        public const float BaseImpulseNs = 0.055f;
        public const float BaseJetHeightMm = 70.0f; // 40-90 mm range
        public const float DriftSinkSpeedMmS = 8.0f; // 8 mm/s downwards
        public const float SteerMaxSpeedMmS = 120.0f;

        public float Vy { get; set; } = 0f; // m/s (positive up)
        public float DepthMm { get; set; } = 500.0f; // Trench depth (surface = 0)
        public float LateralSpeedMmS { get; private set; } = 0f;
        public float LastJetHeightMm { get; private set; } = 0f;

        public float ApplyJetImpulse(float kappa, bool bloom = false)
        {
            float impulse = BaseImpulseNs * kappa;
            float deltaV = impulse / VirtualMassKg;
            Vy += deltaV;

            float heightMultiplier = bloom ? 1.5f : 1.0f;
            LastJetHeightMm = BaseJetHeightMm * kappa * heightMultiplier;
            DepthMm -= LastJetHeightMm;

            return deltaV;
        }

        public float ComputeSteerSpeed(float tiltDeg)
        {
            float clampedTilt = Mathf.Clamp(tiltDeg, -20.0f, 20.0f);
            LateralSpeedMmS = SteerMaxSpeedMmS * Mathf.Sin(clampedTilt * Mathf.Deg2Rad);
            return LateralSpeedMmS;
        }

        public void StepDrift(float dt)
        {
            // With no active jet upward, bell sinks at 8 mm/s
            DepthMm += DriftSinkSpeedMmS * dt;
        }
    }

    public class OxygenModel
    {
        public float CurrentOxygen { get; set; } = 100.0f;
        public const float MaxOxygen = 100.0f;
        public const float JetCost = 2.0f;
        public const float DriftRatePerSec = 0.60f;
        public const float MineDamage = 18.0f;

        public void ConsumeJet()
        {
            CurrentOxygen = Mathf.Max(0f, CurrentOxygen - JetCost);
        }

        public void ConsumeDrift(float dt)
        {
            CurrentOxygen = Mathf.Max(0f, CurrentOxygen - DriftRatePerSec * dt);
        }

        public void ApplyMineDamage()
        {
            CurrentOxygen = Mathf.Max(0f, CurrentOxygen - MineDamage);
        }
    }

    public class TentacleRope
    {
        public List<TentacleSegment> Segments { get; } = new List<TentacleSegment>();
        public const int SegmentCount = 6;
        public const float SegmentLengthMm = 20.0f;

        public TentacleRope(Vector3 bellPos)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                Vector3 p = bellPos - new Vector3(0, (i + 1) * SegmentLengthMm, 0);
                Segments.Add(new TentacleSegment(p));
            }
        }

        public void Update(Vector3 bellPos, float dt)
        {
            Vector3 prev = bellPos;
            for (int i = 0; i < Segments.Count; i++)
            {
                Vector3 current = Segments[i].Position;
                Vector3 vel = current - Segments[i].PrevPosition;
                Segments[i].PrevPosition = current;
                current += vel * 0.95f;
                current.y -= 0.18f * 1000.0f * dt * dt;

                Vector3 delta = current - prev;
                float dist = delta.magnitude;
                if (dist > 0.001f)
                {
                    current = prev + delta.normalized * SegmentLengthMm;
                }
                Segments[i].Position = current;
                prev = current;
            }
        }
    }

    public class JellyfishSimulation
    {
        public GameState State { get; private set; } = GameState.Drift;
        public PalmPressSolver PressSolver { get; } = new PalmPressSolver();
        public JetModel Jet { get; } = new JetModel();
        public OxygenModel Oxygen { get; } = new OxygenModel();
        public TentacleRope Tentacles { get; }
        public List<SeaMine> Mines { get; } = new List<SeaMine>();

        public int CleanContractionStreak { get; private set; } = 0;
        public bool BloomActive { get; private set; } = false;
        public int MineStrikes { get; private set; } = 0;
        public float StunTimer { get; private set; } = 0f;
        public int DivesCleared { get; private set; } = 0;
        public bool DiveClearProcessed { get; private set; } = false;

        public JellyfishSimulation(Vector3 bellPos)
        {
            Tentacles = new TentacleRope(bellPos);
        }

        public void SetState(GameState s)
        {
            State = s;
        }

        public bool TryExecuteJet(float leftForceN, float rightForceN, out float deltaV)
        {
            deltaV = 0f;
            if (StunTimer > 0f) return false;
            if (Oxygen.CurrentOxygen <= 0f) return false;

            float kappa = PressSolver.ComputeKappa(leftForceN, rightForceN);
            if (kappa < 0.35f) return false;

            deltaV = Jet.ApplyJetImpulse(kappa, BloomActive);
            Oxygen.ConsumeJet();

            if (BloomActive)
            {
                BloomActive = false;
            }

            if (kappa >= 0.7f)
            {
                CleanContractionStreak++;
                if (CleanContractionStreak >= 5)
                {
                    BloomActive = true;
                    CleanContractionStreak = 0;
                }
            }
            else
            {
                CleanContractionStreak = 0;
            }

            State = GameState.Jet;
            return true;
        }

        public bool CheckMineCollision(Vector3 minePos, float detectionDistMm = 6.0f)
        {
            foreach (var seg in Tentacles.Segments)
            {
                float d = Vector3.Distance(seg.Position, minePos);
                if (d <= detectionDistMm)
                {
                    OnMineDetonation();
                    return true;
                }
            }
            return false;
        }

        public void OnMineDetonation()
        {
            Oxygen.ApplyMineDamage();
            MineStrikes++;
            StunTimer = 1.0f;
            State = GameState.Stun;

            if (Oxygen.CurrentOxygen <= 0f || MineStrikes >= 3)
            {
                State = GameState.Score;
            }
        }

        public OcclusionState EvaluateOcclusion(Vector3 minePos, Vector3 bellPos)
        {
            if (minePos.z > bellPos.z && Mathf.Abs(minePos.x - bellPos.x) < MedusaJellyfishConfig.OuterRadiusMm)
            {
                return OcclusionState.DimmedVisible;
            }
            return OcclusionState.DirectVisible;
        }

        public void Step(float dt, float tiltDeg = 0f)
        {
            if (State == GameState.Score || State == GameState.Victory || State == GameState.Pause)
                return;

            if (StunTimer > 0f)
            {
                StunTimer = Mathf.Max(0f, StunTimer - dt);
                if (StunTimer <= 0f && State == GameState.Stun)
                {
                    State = GameState.Drift;
                }
            }

            Jet.ComputeSteerSpeed(tiltDeg);

            bool isDrifting = (State == GameState.Drift || State == GameState.Stun);
            if (isDrifting)
            {
                Oxygen.ConsumeDrift(dt);
                Jet.StepDrift(dt);
            }

            Tentacles.Update(new Vector3(0, -Jet.DepthMm, 0), dt);

            if (Jet.DepthMm <= 0f && !DiveClearProcessed)
            {
                DiveClearProcessed = true;
                DivesCleared++;
                State = GameState.DiveClear;
            }

            if (Oxygen.CurrentOxygen <= 0f || MineStrikes >= 3)
            {
                State = GameState.Score;
            }
        }
    }
}
