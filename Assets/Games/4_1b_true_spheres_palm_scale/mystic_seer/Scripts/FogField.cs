using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public class FogField
    {
        private const int GridCols = 64;
        private const int GridRows = 32;
        private readonly float[] _field = new float[GridCols * GridRows];
        private float _smotherFloor = 0.0f;

        public FogField(float initialFog = 0.90f)
        {
            Reset(initialFog);
        }

        public void Reset(float fog = 0.90f)
        {
            _smotherFloor = 0.0f;
            for (int i = 0; i < _field.Length; i++)
            {
                _field[i] = fog;
            }
        }

        public float Sample(Vector3 dir)
        {
            int idx = DirToIndex(dir);
            return Mathf.Max(_smotherFloor, _field[idx]);
        }

        public void SetDensity(Vector3 dir, float val)
        {
            int idx = DirToIndex(dir);
            _field[idx] = Mathf.Clamp01(val);
        }

        public void ApplySmother(float floor = 0.45f)
        {
            _smotherFloor = Mathf.Max(_smotherFloor, floor);
        }

        public void AddBurst(Vector3 dir, float amount = 0.35f)
        {
            int idx = DirToIndex(dir);
            _field[idx] = Mathf.Clamp01(_field[idx] + amount);
        }

        public void Tick(float dt, HandShell handShell, List<Vector3> thiefAuras = null)
        {
            float r = MysticSeerConfig.OrbRadiusMm;

            for (int i = 0; i < _field.Length; i++)
            {
                Vector3 cellDir = IndexToDir(i);

                float R = MysticSeerConfig.FogEquilibriumRateR;
                if (thiefAuras != null)
                {
                    for (int t = 0; t < thiefAuras.Count; t++)
                    {
                        float angleRad = Vector3.Angle(cellDir, thiefAuras[t]) * Mathf.Deg2Rad;
                        if (angleRad < (12.0f / r))
                        {
                            R += MysticSeerConfig.AuraRegenR;
                        }
                    }
                }

                float S = 0f;
                if (handShell != null)
                {
                    for (int s = 0; s < handShell.Sensors.Count; s++)
                    {
                        var sensor = handShell.Sensors[s];
                        float angleRad = Vector3.Angle(cellDir, sensor.SurfaceDir) * Mathf.Deg2Rad;
                        float d = r * angleRad;
                        float h = sensor.StandoffMm;

                        if (d < 27.0f && h < 36.0f)
                        {
                            float g = Mathf.Exp(-(d * d) / (2.0f * 9.0f * 9.0f)) * Mathf.Exp(-(h * h) / (2.0f * 12.0f * 12.0f));
                            S += 12.0f * g * sensor.CurveGain;
                        }
                    }
                }

                float currentF = _field[i];
                float dF = dt * (R * (1.0f - currentF) - S * currentF);
                _field[i] = Mathf.Clamp01(currentF + dF);
            }
        }

        private int DirToIndex(Vector3 dir)
        {
            Vector3 d = dir.normalized;
            float phi = Mathf.Asin(Mathf.Clamp(d.y, -1f, 1f)); // [-pi/2, pi/2]
            float lambda = Mathf.Atan2(d.z, d.x); // [-pi, pi]
            if (lambda < 0) lambda += 2.0f * Mathf.PI;

            int col = Mathf.Clamp((int)((lambda / (2.0f * Mathf.PI)) * GridCols), 0, GridCols - 1);
            int row = Mathf.Clamp((int)(((phi + Mathf.PI * 0.5f) / Mathf.PI) * GridRows), 0, GridRows - 1);
            return row * GridCols + col;
        }

        private Vector3 IndexToDir(int idx)
        {
            int row = idx / GridCols;
            int col = idx % GridCols;

            float phi = (row + 0.5f) / GridRows * Mathf.PI - Mathf.PI * 0.5f;
            float lambda = (col + 0.5f) / GridCols * 2.0f * Mathf.PI;

            float cosPhi = Mathf.Cos(phi);
            return new Vector3(cosPhi * Mathf.Cos(lambda), Mathf.Sin(phi), cosPhi * Mathf.Sin(lambda));
        }
    }
}
