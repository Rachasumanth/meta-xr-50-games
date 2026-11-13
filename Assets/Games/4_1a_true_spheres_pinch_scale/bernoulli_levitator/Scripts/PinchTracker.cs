using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class PinchTracker
    {
        public PinchState State { get; private set; } = PinchState.Released;
        private float _stableTimer;

        public void Update(float d_ti, float dt)
        {
            bool wasPinched = (State == PinchState.Pinched);

            if (!wasPinched)
            {
                if (d_ti <= BernoulliLevitatorConfig.MinPinchDistanceMm / 1000f + 0.0001f)
                {
                    _stableTimer += dt;
                    if (_stableTimer >= 0.030f) // 30 ms stable
                    {
                        State = PinchState.Pinched;
                        _stableTimer = 0f;
                    }
                }
                else
                {
                    _stableTimer = 0f;
                }
            }
            else
            {
                if (d_ti >= BernoulliLevitatorConfig.MaxPinchDistanceMm / 1000f - 0.0001f)
                {
                    _stableTimer += dt;
                    if (_stableTimer >= 0.080f) // 80 ms stable
                    {
                        State = PinchState.Released;
                        _stableTimer = 0f;
                    }
                }
                else
                {
                    _stableTimer = 0f;
                }
            }
        }
    }
}
