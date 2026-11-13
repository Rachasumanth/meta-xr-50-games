using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public enum PinchState
    {
        Released,
        Pinched
    }

    public class PinchTracker
    {
        private float stateTimer = 0f;
        public PinchState State { get; private set; } = PinchState.Released;

        public void Update(float d_ti, float dt)
        {
            stateTimer += dt * 1000.0f; // in milliseconds

            if (State == PinchState.Released)
            {
                if (d_ti <= BernoulliLevitatorConfig.PinchDistanceEnterMm)
                {
                    if (stateTimer >= BernoulliLevitatorConfig.EnterStabilityMs)
                    {
                        State = PinchState.Pinched;
                        stateTimer = 0f;
                    }
                }
                else
                {
                    stateTimer = 0f;
                }
            }
            else
            {
                if (d_ti >= BernoulliLevitatorConfig.PinchDistanceReleaseMm)
                {
                    if (stateTimer >= BernoulliLevitatorConfig.ReleaseStabilityMs)
                    {
                        State = PinchState.Released;
                        stateTimer = 0f;
                    }
                }
                else
                {
                    stateTimer = 0f;
                }
            }
        }
    }
}
