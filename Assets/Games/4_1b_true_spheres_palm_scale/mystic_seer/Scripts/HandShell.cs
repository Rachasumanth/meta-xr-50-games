using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public struct SensorPoint
    {
        public Vector3 SurfaceDir;
        public float RadiusMm;
        public float StandoffMm;
        public float CurveGain;
    }

    public class HandShell
    {
        public List<SensorPoint> Sensors { get; } = new List<SensorPoint>();

        public void ClearSensors()
        {
            Sensors.Clear();
        }

        public void AddSensor(Vector3 dir, float standoffMm, float curveGain = 1.0f)
        {
            Sensors.Add(new SensorPoint
            {
                SurfaceDir = dir.normalized,
                RadiusMm = MysticSeerConfig.OrbRadiusMm + standoffMm,
                StandoffMm = standoffMm,
                CurveGain = curveGain
            });
        }
    }
}
