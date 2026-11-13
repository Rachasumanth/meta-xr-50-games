using System;
using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest
{
    public enum HarvestKind
    {
        Sheep,
        Fence,
        Barn,
        Mine
    }

    public class HarvestEntity
    {
        public int Id;
        public HarvestKind Kind;
        public Vector2 Position;
        public float RadiusMm;
        public float Mass;
        public bool Captured;
    }

    public class HarvestResolver
    {
        public float Score { get; private set; } = 0f;
        public int Chain { get; private set; } = 0;
        public float LastCaptureTime { get; private set; } = -999f;
        public bool IsStunned { get; private set; } = false;
        public float StunTimer { get; private set; } = 0f;

        public float Multiplier => 1.0f + 0.15f * Mathf.Min(Chain, 10);

        public event Action<FeedbackEvent> OnFeedback;

        public bool CheckCapture(StickySphere sphere, Vector2 spherePos, HarvestEntity entity, float now)
        {
            if (entity.Captured || IsStunned) return false;

            float d = Vector2.Distance(spherePos, entity.Position);

            if (entity.Kind == HarvestKind.Mine)
            {
                if (d <= sphere.RadiusMm + 11.0f)
                {
                    TriggerMine(sphere);
                    return false;
                }
                return false;
            }

            // Capture boundary: d <= R + 0.3 * e.r and e.r <= 0.85 * R
            if (d <= (sphere.RadiusMm + 0.3f * entity.RadiusMm + 0.01f) && entity.RadiusMm <= 0.85f * sphere.RadiusMm)
            {
                Capture(sphere, entity, now);
                return true;
            }

            return false;
        }

        public void Capture(StickySphere sphere, HarvestEntity entity, float now)
        {
            if (entity.Captured) return;
            entity.Captured = true;

            if (now - LastCaptureTime <= KatamariHarvestConfig.ChainTimeoutSec && LastCaptureTime >= 0f)
            {
                Chain++;
            }
            else
            {
                Chain = 0;
            }
            LastCaptureTime = now;

            float mult = Multiplier;
            sphere.SetMass(sphere.Mass + entity.Mass);
            Score += Mathf.Round(entity.Mass * mult);
            if (entity.Kind == HarvestKind.Barn)
            {
                Score += 150f;
            }

            switch (entity.Kind)
            {
                case HarvestKind.Sheep: OnFeedback?.Invoke(FeedbackEvent.SheepCapture); break;
                case HarvestKind.Fence: OnFeedback?.Invoke(FeedbackEvent.FenceCapture); break;
                case HarvestKind.Barn: OnFeedback?.Invoke(FeedbackEvent.BarnCapture); break;
            }
            OnFeedback?.Invoke(FeedbackEvent.ChainStep);
        }

        public void TriggerMine(StickySphere sphere)
        {
            sphere.ApplyMineDamage();
            Chain = 0;
            IsStunned = true;
            StunTimer = 0.8f;
            OnFeedback?.Invoke(FeedbackEvent.MineTrigger);
        }

        public void Update(float dt)
        {
            if (IsStunned)
            {
                StunTimer -= dt;
                if (StunTimer <= 0f)
                {
                    IsStunned = false;
                }
            }
        }

        public void ResetChain()
        {
            Chain = 0;
            OnFeedback?.Invoke(FeedbackEvent.ChainBreak);
        }
    }
}
