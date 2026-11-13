using System;
using UnityEngine;

namespace XRGames.C4_1A.SocketTrackball
{
    // NOTE: ContactRecord and HandContacts live in HandContacts.cs (single definition).
    // This file owns the rest of the section-15 architecture.

    public enum FSMState
    {
        Boot,
        Register,
        GripCheck,
        WaveIntro,
        Steer,
        Surge,
        Gate,
        Resolve,
        Recovery,
        Paused,
        GameOver,
        Victory
    }

    // Per spec section 15: visual odometry, radius fit, loss/re-acquire FSM.
    public class PropTracker
    {
        public Vector3 Center { get; private set; } = Vector3.zero;
        public float Radius { get; private set; } = SocketTrackballConfig.MarbleRadiusMm;
        public Quaternion Orientation { get; private set; } = Quaternion.identity;
        public Vector3 Velocity { get; private set; } = Vector3.zero;
        public bool IsTracked { get; private set; } = true;

        // Fit quality bookkeeping per acceptance item 1:
        // CenterRmsErrMm = RMS radial deviation of sampled rim frames from the
        // fitted center radius (synthetic stand-in for multi-frame center RMS),
        // MaxFrameErrMm = worst single-frame radial deviation.
        public float CenterRmsErrMm { get; private set; } = 0f;
        public float MaxFrameErrMm { get; private set; } = 0f;

        private float _lossTimer = 0f;
        public Vector3 LastOmegaRad { get; private set; } = Vector3.zero;

        // Least-squares-ish radius fit: mean-center fit over sampled rim points.
        public void Fit(Vector3[] rimPoints)
        {
            if (rimPoints == null || rimPoints.Length < 12) return;
            Vector3 sum = Vector3.zero;
            foreach (var p in rimPoints) sum += p;
            Center = sum / rimPoints.Length;

            float sumR = 0f, sum2 = 0f, maxDev = 0f;
            foreach (var p in rimPoints)
            {
                float d = Vector3.Distance(p, Center);
                sumR += d;
                float dev = Mathf.Abs(d - SocketTrackballConfig.MarbleRadiusMm);
                sum2 += dev * dev;
                if (dev > maxDev) maxDev = dev;
            }
            Radius = sumR / rimPoints.Length;
            CenterRmsErrMm = Mathf.Sqrt(sum2 / rimPoints.Length);
            MaxFrameErrMm = maxDev;

            IsTracked = true;
            _lossTimer = 0f;
            LastOmegaRad = Vector3.zero;
        }

        public void UpdatePose(Quaternion omegaDelta, float dt)
        {
            if (!IsTracked) return;
            Orientation = (omegaDelta * Orientation).normalized;
        }

        // Angular-velocity integration (rad/s about vector omega) through the
        // alpha slew (driven rolling) then vector rotation.
        public void IntegrateOmega(Vector3 omegaRadPerSec, float dt)
        {
            if (!IsTracked) return;
            Vector3 w = SphereKinematics.AlphaSlew(omegaRadPerSec, LastOmegaRad, dt);
            LastOmegaRad = w;
            Orientation = SphereKinematics.Integrate(Orientation, w, dt);
        }

        public void NotifyTrackingLoss(float dt)
        {
            _lossTimer += dt;
            if (_lossTimer > 1.5f)
            {
                IsTracked = false;
            }
        }

        public void Reacquire(Vector3[] rimPoints)
        {
            Fit(rimPoints);
            _lossTimer = 0f;
            IsTracked = true;
        }
    }

    // Rolling kinematics: omega = (n x v_t) / r, clamps, and vector integration.
    public class SphereKinematics
    {
        // Rolling law (acceptance item 4): omega = (n x vt) / r in rad/s, RAW.
        // Do NOT clamp here: 16.4 demands the exact 10 rad/s law (80/8), while
        // the 540 deg/s slip cap is an explicit downstream stage (16.5).
        public static Vector3 ComputeAngularVelocity(Vector3 normal, Vector3 tangentVelocity, float radiusMm)
        {
            if (radiusMm <= 0f) return Vector3.zero;
            return Vector3.Cross(normal, tangentVelocity) / radiusMm;
        }

        // Slip clamp (acceptance item 5): |omega| <= OmegaMaxDegPerSec AND
        // |alpha| <= AlphaMaxDegPerSecSq versus the previous step's omega.
        public static Vector3 ClampOmega(Vector3 omegaRad, Vector3 prevOmegaRad, float dt)
        {
            float maxOmegaRad = SocketTrackballConfig.OmegaMaxDegPerSec * Mathf.Deg2Rad;
            Vector3 w = omegaRad;
            if (w.magnitude > maxOmegaRad)
            {
                w = w.normalized * maxOmegaRad;
            }
            if (dt > 1e-5f)
            {
                float maxAlphaRad = SocketTrackballConfig.AlphaMaxDegPerSecSq * Mathf.Deg2Rad;
                Vector3 alpha = (w - prevOmegaRad) / dt;
                if (alpha.magnitude > maxAlphaRad)
                {
                    w = prevOmegaRad + alpha.normalized * (maxAlphaRad * dt);
                }
            }
            return w;
        }

        // Quaternion integration: rotates |omega|*dt degrees about the axis of omega.
        public static Quaternion Integrate(Quaternion current, Vector3 omegaRad, float dt)
        {
            float angleDeg = omegaRad.magnitude * Mathf.Rad2Deg * dt;
            if (angleDeg < 1e-6f) return current;
            Vector3 axis = omegaRad / omegaRad.magnitude;
            return (Quaternion.AngleAxis(angleDeg, axis) * current).normalized;
        }

        // Driven-rolling integration slew: limits alpha (rate of change of omega)
        // without capping absolute omega, so the 16.4 law (57.3 deg in 0.1 s from
        // 10 rad/s) stays true while the alpha bound is honored.
        public static Vector3 AlphaSlew(Vector3 demandRad, Vector3 prevRad, float dt)
        {
            Vector3 d = demandRad - prevRad;
            float step = SocketTrackballConfig.AlphaMaxDegPerSecSq * Mathf.Deg2Rad * dt;
            if (d.magnitude <= step) return demandRad;
            return prevRad + d.normalized * step;
        }
    }

    // C-ring grip geometry and damping (spec section 15: GripIntegrity).
    public class GripIntegrity
    {
        public float CurrentG { get; private set; } = 1.0f;

        public float Update(bool okPos, bool okNorm, bool okCont, float dt)
        {
            float target = (okPos && okNorm && okCont) ? 1.0f : SocketTrackballConfig.GripDampFactor;
            // 120 ms slew to the damping floor (acceptance items 6 and 7).
            float rate = 1.0f / 0.12f;
            CurrentG = Mathf.MoveTowards(CurrentG, target, rate * dt);
            return CurrentG;
        }
    }

    // Crown roll to steering: contact gate, dead zone with release hysteresis,
    // saturation curve, and the bank/pitch steering axis mapping (spec section 6).
    public class RollController
    {
        public bool SteeringActive { get; private set; } = false;

        // Contact gate gain (acceptance item 2): 1.0 inside the crown cone,
        // 0.0 outside. Crown cone boundary is CrownGateDeg from the crown axis.
        public float ContactGateGain(Vector3 contactNormal, Vector3 crownAxis)
        {
            float dot = Vector3.Dot(contactNormal.normalized, crownAxis.normalized);
            return dot >= Mathf.Cos(SocketTrackballConfig.CrownGateDeg * Mathf.Deg2Rad) ? 1.0f : 0.0f;
        }

        /// <summary>
        /// Rolling authority for this frame. Returns the velocity saturator u in
        /// [0, 1]; bankRate/pitchRate carry the steering rates for this frame.
        /// Lean: omega about the up axis (lateral crowning) drives bank; omega
        /// about the right axis (forward tumble) drives pitch.
        /// </summary>
        public float ComputeOutput(Vector3 contactNormal, Vector3 thumbVelocity, Vector3 crownAxis, out float bankRate, out float pitchRate)
        {
            bankRate = 0f;
            pitchRate = 0f;

            if (ContactGateGain(contactNormal, crownAxis) <= 0f)
            {
                SteeringActive = false;
                return 0f;
            }

            float vMag = thumbVelocity.magnitude;

            // Dead zone with release hysteresis (acceptance item 3):
            // enter above DeadZoneRoll, stay active down to DeadZoneRelease.
            float dz = SteeringActive
                ? SocketTrackballConfig.DeadZoneReleaseMmPerSec
                : SocketTrackballConfig.DeadZoneRollMmPerSec;
            if (vMag < dz)
            {
                SteeringActive = false;
                return 0f;
            }
            SteeringActive = true;

            float u = Mathf.Clamp((vMag - dz) / (SocketTrackballConfig.VelocitySatMmPerSec - dz), 0f, 1f);
            Vector3 omega = SphereKinematics.ComputeAngularVelocity(contactNormal, thumbVelocity, SocketTrackballConfig.MarbleRadiusMm);

            bankRate = -SocketTrackballConfig.KBankDegPerSec * u * Mathf.Sign(Vector3.Dot(omega, Vector3.up));
            pitchRate = SocketTrackballConfig.KPitchDegPerSec * u * Mathf.Sign(Vector3.Dot(omega, Vector3.right));

            return u;
        }
    }

    // Bank/pitch attitude integration with saturating clamp (spec section 6).
    public class ShipController
    {
        public float Bank { get; private set; } = 0f;
        public float Pitch { get; private set; } = 0f;
        public Vector3 PositionOffset { get; private set; } = Vector3.zero;

        public void UpdateRates(float bankRate, float pitchRate, float gripDamp, float dt)
        {
            Bank = Mathf.Clamp(Bank + bankRate * gripDamp * dt, -SocketTrackballConfig.BankMaxDeg, SocketTrackballConfig.BankMaxDeg);
            Pitch = Mathf.Clamp(Pitch + pitchRate * gripDamp * dt, -SocketTrackballConfig.PitchMaxDeg, SocketTrackballConfig.PitchMaxDeg);
        }
    }

    // Deterministic rails and near-miss bookkeeping (spec section 8).
    public class AsteroidField
    {
        public int Wave { get; set; } = 1;
        public float AsteroidSpeed { get; private set; } = 350f;

        public void InitWave(int wave)
        {
            Wave = wave;
            AsteroidSpeed = 350f + 50f * (wave - 1);
        }

        public bool CheckNearMiss(Vector3 shipPos, Vector3 asteroidPos)
        {
            float dist = Vector3.Distance(shipPos, asteroidPos);
            return dist <= SocketTrackballConfig.NearMissDistanceMm && dist > 12f;
        }
    }

    // Sphere-capsule overlap and gate pass tests (spec section 10).
    public class CollisionWorld
    {
        public bool CheckSphereCapsule(Vector3 sphereCenter, float sphereRadius, Vector3 capStart, Vector3 capEnd, float capRadius)
        {
            Vector3 ab = capEnd - capStart;
            Vector3 ac = sphereCenter - capStart;
            float t = Mathf.Clamp01(Vector3.Dot(ac, ab) / ab.sqrMagnitude);
            Vector3 closest = capStart + t * ab;
            float dist = Vector3.Distance(sphereCenter, closest);
            return dist <= (sphereRadius + capRadius);
        }

        public bool CheckGatePass(Vector3 objectPos, Vector3 gateCenter, float gateRadius)
        {
            float dist = Vector3.Distance(objectPos, gateCenter);
            return dist <= gateRadius;
        }
    }

    // Six-wave table driver with FSM timer bookkeeping (spec section 9).
    public class WaveDirector
    {
        public int CurrentWave { get; private set; } = 1;
        public FSMState CurrentState { get; private set; } = FSMState.Boot;
        public float Timer { get; private set; } = 0f;

        public void AdvanceState(FSMState nextState, float resetTimer = 0f)
        {
            CurrentState = nextState;
            Timer = resetTimer;
        }

        public void Tick(float dt)
        {
            Timer += dt;
        }
    }

    // Multiplier chains and score persistence hooks (spec section 8).
    public class ScoreService
    {
        public int Score { get; private set; } = 0;
        public int ChainMultiplier { get; private set; } = 1;

        public void AddScore(int amount)
        {
            Score += amount * ChainMultiplier;
        }

        public void IncrementChain()
        {
            if (ChainMultiplier < 4) ChainMultiplier++;
        }

        public void ResetChain()
        {
            ChainMultiplier = 1;
        }
    }

    // Cue table and spatialisation bookkeeping (spec section 12).
    public class AudioDirector
    {
        public string LastPlayedCue { get; private set; } = string.Empty;

        public void PlayCue(string cueName)
        {
            LastPlayedCue = cueName;
        }
    }

    // Session state machine shell (spec section 9).
    public class SessionFSM
    {
        public FSMState State { get; private set; } = FSMState.Boot;
        public WaveDirector WaveDir { get; } = new WaveDirector();

        public void Transition(FSMState newState)
        {
            State = newState;
        }
    }
}
