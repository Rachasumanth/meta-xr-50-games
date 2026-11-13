using System;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public class PlanarDrive
    {
        public const float Mass = 0.095f;                 // kg
        public const float SkirtDragCv = 0.55f;            // N*s/m
        public const float FrictionMu = 0.42f;
        public const float Gravity = 9.81f;
        public const float StaticBreakawayF = 0.391f;      // N
        public const float Kp = 6.0f;                      // N*s/m
        public const float FMax = 1.4f;                    // N
        public const float MotionGain = 3.0f;
        public const float VelocityDeadzone = 0.005f;      // 5 mm/s

        // State
        public Vector3 PhysicalPosition;                   // meters
        public Vector3 PhysicalVelocity;                   // m/s
        public Vector3 VirtualPosition;                    // meters
        public Vector3 VirtualVelocity => PhysicalVelocity * MotionGain;
        public Vector3 Heading = Vector3.forward;

        // Clutch state
        public bool IsClutching { get; private set; }
        public float ClutchTimer { get; private set; }
        public Vector3 ClutchOffset;

        // Drift & Heat
        public float Heat { get; private set; }
        public bool IsOverheated { get; private set; }
        public float OverheatTimer { get; private set; }
        public float DriftReducedFrictionTimer { get; private set; }

        // Tracking loss handling
        public float TrackingLossTimer { get; private set; }
        public bool IsPaused { get; private set; }

        public PlanarDrive()
        {
            Reset();
        }

        public void Reset()
        {
            PhysicalPosition = Vector3.zero;
            PhysicalVelocity = Vector3.zero;
            VirtualPosition = Vector3.zero;
            ClutchOffset = Vector3.zero;
            Heading = Vector3.forward;
            IsClutching = false;
            ClutchTimer = 0f;
            Heat = 0f;
            IsOverheated = false;
            OverheatTimer = 0f;
            DriftReducedFrictionTimer = 0f;
            TrackingLossTimer = 0f;
            IsPaused = false;
        }

        public void TriggerDrift()
        {
            if (IsOverheated) return;

            // Lateral impulse: 0.21 m/s lateral
            Vector3 right = Vector3.Cross(Vector3.up, Heading).normalized;
            PhysicalVelocity += right * 0.21f;
            Heat = Mathf.Min(100f, Heat + 26.0f);
            DriftReducedFrictionTimer = 0.45f;

            if (Heat >= 100f)
            {
                IsOverheated = true;
                OverheatTimer = 3.0f;
            }
        }

        public void AddHeat(float amount)
        {
            Heat = Mathf.Min(100f, Heat + amount);
            if (Heat >= 100f)
            {
                IsOverheated = true;
                OverheatTimer = 3.0f;
            }
        }

        public void StartClutch()
        {
            IsClutching = true;
            ClutchTimer = 0.150f; // 150 ms
        }

        public void StepTrackingLoss(bool isTracking, float dt)
        {
            if (!isTracking)
            {
                TrackingLossTimer += dt;
                if (TrackingLossTimer >= 1.5f)
                {
                    IsPaused = true;
                }
            }
            else
            {
                TrackingLossTimer = 0f;
                IsPaused = false;
            }
        }

        public void Step(Vector3 handVelocity, float dt)
        {
            // Heat cooling
            if (Heat > 0f)
            {
                Heat = Mathf.Max(0f, Heat - 8.0f * dt);
            }
            if (IsOverheated)
            {
                OverheatTimer -= dt;
                if (OverheatTimer <= 0f)
                {
                    IsOverheated = false;
                }
            }
            if (DriftReducedFrictionTimer > 0f)
            {
                DriftReducedFrictionTimer -= dt;
            }

            // Clutch handling
            if (IsClutching)
            {
                ClutchTimer -= dt;
                if (ClutchTimer <= 0f)
                {
                    IsClutching = false;
                    ClutchOffset += new Vector3(0.032f, 0f, 0f); // 32mm re-centre
                }
                return; // Zero virtual displacement during freeze
            }

            if (IsPaused) return;

            // Drive force calculation
            Vector3 vRel = handVelocity - PhysicalVelocity;
            if (TrackingLossTimer > 0.200f)
            {
                vRel = Vector3.zero; // Thrust = 0 after 200 ms tracking loss
            }

            float speedRel = vRel.magnitude;
            Vector3 force = Vector3.zero;

            if (speedRel > VelocityDeadzone)
            {
                float fMag = Mathf.Clamp(Kp * speedRel, -FMax, FMax);
                if (IsOverheated)
                {
                    fMag = Mathf.Min(fMag, 0.5f); // Thrust capped at 0.5N during overheat
                }
                force = vRel.normalized * fMag;
            }

            float fMagTotal = force.magnitude;
            float physSpeed = PhysicalVelocity.magnitude;

            // Static breakaway friction
            if (physSpeed < 0.001f)
            {
                if (fMagTotal < StaticBreakawayF)
                {
                    PhysicalVelocity = Vector3.zero;
                    return;
                }
            }

            // Dynamic integration
            float activeMu = FrictionMu;
            float activeCv = SkirtDragCv;
            if (DriftReducedFrictionTimer > 0f)
            {
                activeCv *= 0.55f;
            }

            Vector3 drag = -activeCv * PhysicalVelocity;
            Vector3 friction = -activeMu * Mass * Gravity * (physSpeed > 0.0001f ? PhysicalVelocity.normalized : Vector3.zero);
            Vector3 netForce = force + drag + friction;
            Vector3 accel = netForce / Mass;

            PhysicalVelocity += accel * dt;
            if (PhysicalVelocity.magnitude < 0.0005f && force.magnitude == 0f)
            {
                PhysicalVelocity = Vector3.zero;
            }

            PhysicalPosition += PhysicalVelocity * dt;
            VirtualPosition += PhysicalVelocity * MotionGain * dt;

            if (PhysicalVelocity.sqrMagnitude > 0.0001f)
            {
                Heading = Vector3.Slerp(Heading, PhysicalVelocity.normalized, 10f * dt);
            }

            // Clutch boundary check: 48mm horizontal displacement
            if (Mathf.Abs(PhysicalPosition.x) > 0.048f || Mathf.Abs(PhysicalPosition.z) > 0.048f)
            {
                StartClutch();
            }
        }
    }
}
