using UnityEngine;
using System.Collections.Generic;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class NozzleFrame
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Vector3 Velocity { get; private set; }

        private Vector3 _lastPos;
        private bool _initialized;

        public void Update(Vector3 targetPos, Quaternion targetRot, float dt, bool tableBumped)
        {
            if (tableBumped)
            {
                // Blend or damp on table bump
                targetPos = Vector3.Lerp(Position, targetPos, 0.5f);
            }

            if (!_initialized)
            {
                Position = targetPos;
                Rotation = targetRot;
                Velocity = Vector3.zero;
                _lastPos = targetPos;
                _initialized = true;
                return;
            }

            if (dt > 0.0001f)
            {
                Velocity = (targetPos - _lastPos) / dt;
            }
            else
            {
                Velocity = Vector3.zero;
            }

            _lastPos = targetPos;
            Position = targetPos;
            Rotation = targetRot;
        }
    }

    public class BallRegistration
    {
        public bool IsRegistered { get; private set; }
        public float MeasuredRadius { get; private set; } = BernoulliLevitatorConfig.BallRadiusMm;
        public float RMS { get; private set; }
        public Vector3 CapturedZUp { get; private set; } = Vector3.up;

        private List<Vector3> _samples = new List<Vector3>();

        public void AddSample(Vector3 samplePos, Vector3 zUp)
        {
            _samples.Add(samplePos);
            CapturedZUp = zUp.normalized;

            if (_samples.Count >= 60)
            {
                FitSphere();
            }
        }

        private void FitSphere()
        {
            if (_samples.Count == 0) return;

            Vector3 sum = Vector3.zero;
            foreach (var s in _samples) sum += s;
            Vector3 center = sum / _samples.Count;

            float totalDist = 0f;
            float sqrErrorSum = 0f;
            foreach (var s in _samples)
            {
                float dist = Vector3.Distance(s, center);
                totalDist += dist;
            }
            float avgRadius = totalDist / _samples.Count;

            foreach (var s in _samples)
            {
                float dist = Vector3.Distance(s, center);
                float diff = dist - avgRadius;
                sqrErrorSum += diff * diff;
            }

            RMS = Mathf.Sqrt(sqrErrorSum / _samples.Count);
            MeasuredRadius = avgRadius * 1000f; // convert to mm assuming meters input, or keep as is if mm. Let's assume input in meters, scale to mm.
            // If samples are in meters (e.g. 0.02m), MeasuredRadius in mm = avgRadius * 1000.
            // Let's ensure standard unit handling: if MeasuredRadius is around 0.02, multiply by 1000.
            if (MeasuredRadius < 5f) MeasuredRadius *= 1000f;

            IsRegistered = (MeasuredRadius >= BernoulliLevitatorConfig.BallMinRadiusMm &&
                            MeasuredRadius <= BernoulliLevitatorConfig.BallMaxRadiusMm &&
                            RMS <= BernoulliLevitatorConfig.MaxRmsMm);
        }

        public void ResetRegistration()
        {
            _samples.Clear();
            IsRegistered = false;
            MeasuredRadius = BernoulliLevitatorConfig.BallRadiusMm;
            RMS = 0f;
            CapturedZUp = Vector3.up;
        }

        public void LoseAcquisition()
        {
            IsRegistered = false;
        }
    }

    public enum PinchState
    {
        Released,
        Pinched
    }

    public class PinchTracker
    {
        public PinchState State { get; private set; } = PinchState.Released;

        private float _stableTimer = 0f;

        public void Update(float d_ti, float dt)
        {
            float distMm = d_ti * 1000f;

            if (State == PinchState.Released)
            {
                if (distMm <= BernoulliLevitatorConfig.PinchEnterDistMm)
                {
                    _stableTimer += dt * 1000f;
                    if (_stableTimer >= BernoulliLevitatorConfig.PinchEnterStableMs)
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
                if (distMm >= BernoulliLevitatorConfig.PinchReleaseDistMm)
                {
                    _stableTimer += dt * 1000f;
                    if (_stableTimer >= BernoulliLevitatorConfig.PinchReleaseStableMs)
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

    public class HeightController
    {
        public float Height { get; private set; }
        public float Velocity { get; private set; }
        public float TargetHeight { get; private set; }

        public void SetTarget(float target)
        {
            TargetHeight = Mathf.Clamp(target, BernoulliLevitatorConfig.MinHeightMm, BernoulliLevitatorConfig.MaxHeightMm);
        }

        public void Step(float dt)
        {
            float currentM = Height / 1000f;
            float targetM = TargetHeight / 1000f;

            float wn = BernoulliLevitatorConfig.ServoWn;
            float zeta = BernoulliLevitatorConfig.ServoZeta;

            float acc = wn * wn * (targetM - currentM) - 2f * zeta * wn * (Velocity / 1000f);
            acc = Mathf.Clamp(acc, -BernoulliLevitatorConfig.MaxAccelerationMps2, BernoulliLevitatorConfig.MaxAccelerationMps2);

            float newVel = Mathf.Clamp((Velocity / 1000f) + acc * dt, -BernoulliLevitatorConfig.MaxVelocityMps, BernoulliLevitatorConfig.MaxVelocityMps);
            float newHeight = Mathf.Clamp(currentM + newVel * dt, BernoulliLevitatorConfig.MinHeightMm / 1000f, BernoulliLevitatorConfig.MaxHeightMm / 1000f);

            Height = newHeight * 1000f;
            Velocity = newVel * 1000f;
        }
    }

    public class SpinFusion
    {
        public Vector3 AngularVelocity { get; private set; }

        public void Step(Vector3 inputOmega, float dt)
        {
            float maxDeg = BernoulliLevitatorConfig.MaxSlipAngularSpeedDeg;
            float speed = inputOmega.magnitude;
            if (speed > maxDeg)
            {
                inputOmega = inputOmega.normalized * maxDeg;
            }

            // Low pass filter / fusion simulation
            AngularVelocity = Vector3.Lerp(AngularVelocity, inputOmega, dt * 10f);
        }
    }

    public class OrbBody
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Vector3 Velocity { get; private set; }
        public bool IsFalling { get; private set; }

        private float _targetHeight = 120f;
        private float _currentHeightM = 0.12f;
        private float _velocityMps = 0f;

        public void SetTargetHeight(float target)
        {
            _targetHeight = target;
        }

        public void SetFalling(bool falling)
        {
            IsFalling = falling;
        }

        public void Step(float dt, Vector3 spinInput)
        {
            if (IsFalling)
            {
                _velocityMps -= 9.81f * dt;
                _currentHeightM += _velocityMps * dt;
                if (_currentHeightM <= 0f)
                {
                    _currentHeightM = 0f;
                    _velocityMps = 0f;
                }
            }
            else
            {
                float targetM = _targetHeight / 1000f;
                float wn = BernoulliLevitatorConfig.ServoWn;
                float zeta = BernoulliLevitatorConfig.ServoZeta;

                float acc = wn * wn * (targetM - _currentHeightM) - 2f * zeta * wn * _velocityMps;
                acc = Mathf.Clamp(acc, -BernoulliLevitatorConfig.MaxAccelerationMps2, BernoulliLevitatorConfig.MaxAccelerationMps2);

                _velocityMps = Mathf.Clamp(_velocityMps + acc * dt, -BernoulliLevitatorConfig.MaxVelocityMps, BernoulliLevitatorConfig.MaxVelocityMps);
                _currentHeightM = Mathf.Clamp(_currentHeightM + _velocityMps * dt, 0f, 0.300f);
            }

            Position = new Vector3(0f, _currentHeightM, 0f);
            Velocity = new Vector3(0f, _velocityMps, 0f);

            if (spinInput.sqrMagnitude > 0.001f)
            {
                float angle = spinInput.magnitude * dt;
                Vector3 axis = spinInput.normalized;
                Rotation = Rotation * Quaternion.AngleAxis(angle, axis);
            }
        }
    }

    public class RingField
    {
        public bool CheckPass(float orbDistanceToCenterMm)
        {
            return orbDistanceToCenterMm <= BernoulliLevitatorConfig.RingPassDistanceMm;
        }

        public bool CheckPerfect(float orbDistanceToCenterMm)
        {
            return orbDistanceToCenterMm <= BernoulliLevitatorConfig.RingPerfectDistanceMm;
        }
    }

    public class GustField
    {
        public Vector3 GetLateralAcceleration(float time)
        {
            float x = Mathf.Sin(time * 2f * Mathf.PI * 0.5f) * 0.5f;
            float z = Mathf.Cos(time * 2f * Mathf.PI * 0.3f) * 0.5f;
            return new Vector3(x, 0f, z);
        }
    }

    public class PressureState
    {
        public int Pressure { get; private set; } = 3;
        public int Score { get; private set; }
        public int Combo { get; private set; }

        public void AddScore(int points)
        {
            Score += points * (Combo > 0 ? Combo : 1);
            Combo++;
        }

        public void Penalty()
        {
            Pressure = Mathf.Max(0, Pressure - 1);
            Combo = 0;
        }

        public void Reset()
        {
            Pressure = 3;
            Score = 0;
            Combo = 0;
        }
    }

    public class WaveDirector
    {
        public int CurrentWave { get; private set; } = 1;

        public void NextWave()
        {
            if (CurrentWave < 6)
            {
                CurrentWave++;
            }
        }
    }

    public class FeedbackDirector
    {
        public float AudioVolume { get; private set; } = BernoulliLevitatorConfig.AudioVolumeDefault;
        public int ParticleBudget { get; private set; } = BernoulliLevitatorConfig.ParticleBudget;

        public void SetAudioVolume(float vol)
        {
            AudioVolume = Mathf.Clamp01(vol);
        }
    }

    public enum SessionState
    {
        Ready,
        Running,
        Ended
    }

    public class SessionFSM
    {
        public SessionState State { get; private set; } = SessionState.Ready;

        public void StartSession()
        {
            State = SessionState.Running;
        }

        public void EndSession()
        {
            State = SessionState.Ended;
        }
    }
}
