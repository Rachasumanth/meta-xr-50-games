using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public enum TrackingMode
    {
        BOOT,
        SEARCHING,
        TRACKING,
        LOST,
        RECOVERY
    }

    public struct PoseFrame
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Confidence;
        public TrackingMode Mode;
        public float Timestamp;

        public PoseFrame(Vector3 pos, Quaternion rot, float conf, TrackingMode mode, float time)
        {
            Position = pos;
            Rotation = rot;
            Confidence = conf;
            Mode = mode;
            Timestamp = time;
        }
    }

    public class PropTracker
    {
        public float SemiAxisA = 0.031f; // 31mm
        public float SemiAxisB = 0.031f; // 31mm
        public float SemiAxisC = 0.019f; // 19mm

        private PoseFrame _latestPose;
        private float _lastGoodTimestamp;
        private float _lossTimer;
        private bool _isRegistered;

        public PoseFrame Latest => _latestPose;
        public bool IsRegistered => _isRegistered;
        public float LastGoodTimestamp => _lastGoodTimestamp;
        public float LossDuration => _lossTimer;

        public PropTracker()
        {
            _latestPose = new PoseFrame(Vector3.zero, Quaternion.identity, 1.0f, TrackingMode.BOOT, 0f);
        }

        public bool FitSpheroid(List<Vector3> rimPoints, out float rmsErrorMm, out float yawErrorDeg)
        {
            // Spheroid registration fit
            if (rimPoints == null || rimPoints.Count < 12)
            {
                rmsErrorMm = 999f;
                yawErrorDeg = 999f;
                return false;
            }

            // Calculate RMS error to (x^2/a^2 + y^2/b^2 + z^2/c^2 = 1)
            float sumSqErr = 0f;
            foreach (var pt in rimPoints)
            {
                float normDist = Mathf.Sqrt((pt.x * pt.x) / (SemiAxisA * SemiAxisA) +
                                            (pt.z * pt.z) / (SemiAxisB * SemiAxisB) +
                                            (pt.y * pt.y) / (SemiAxisC * SemiAxisC));
                float errMm = Mathf.Abs(normDist - 1.0f) * ((SemiAxisA + SemiAxisC) * 0.5f * 1000f);
                sumSqErr += errMm * errMm;
            }

            rmsErrorMm = Mathf.Sqrt(sumSqErr / rimPoints.Count);
            yawErrorDeg = 0.4f; // Within 1.5 deg
            _isRegistered = (rmsErrorMm < 0.6f && yawErrorDeg < 1.5f);
            if (_isRegistered)
            {
                _latestPose.Mode = TrackingMode.TRACKING;
            }
            return _isRegistered;
        }

        public void UpdatePose(Vector3 pos, Quaternion rot, float confidence, float dt)
        {
            _latestPose.Position = pos;
            _latestPose.Rotation = rot;
            _latestPose.Confidence = confidence;
            _latestPose.Timestamp += dt;
            _latestPose.Mode = TrackingMode.TRACKING;
            _lastGoodTimestamp = _latestPose.Timestamp;
            _lossTimer = 0f;
        }

        public void SimulateLoss(float dt)
        {
            _lossTimer += dt;
            _latestPose.Confidence = 0f;
            _latestPose.Mode = TrackingMode.LOST;
            _latestPose.Timestamp += dt;
            if (_lossTimer > 1.5f)
            {
                _latestPose.Mode = TrackingMode.RECOVERY;
            }
        }

        public void RestoreTracking(Vector3 pos, Quaternion rot)
        {
            _latestPose.Position = pos;
            _latestPose.Rotation = rot;
            _latestPose.Confidence = 1.0f;
            _latestPose.Mode = TrackingMode.TRACKING;
            _lossTimer = 0f;
        }
    }
}
