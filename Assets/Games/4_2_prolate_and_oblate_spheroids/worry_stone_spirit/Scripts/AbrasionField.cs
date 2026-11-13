using System;
using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public class AbrasionField
    {
        public const int Width = 96;
        public const int Height = 48;
        public const float MaxDepthMm = WorryStoneSpiritConfig.MaxAshDepthMm; // 1.20 mm
        public const float LayerThickness = WorryStoneSpiritConfig.LayerThicknessMm; // 0.20 mm

        public float[,] Heights = new float[Width, Height];
        public float[,] Hardness = new float[Width, Height];
        public int[,] Layers = new int[Width, Height];

        public event Action<int, int, int> OnLayerDecreased; // (x, y, newLayer)

        public AbrasionField()
        {
            Reset();
        }

        public void Reset()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Heights[x, y] = MaxDepthMm;
                    Hardness[x, y] = 1.0f;
                    Layers[x, y] = Mathf.FloorToInt(MaxDepthMm / LayerThickness); // Layer 6 (or 5 if 1.20 is capped to 5)
                    if (Layers[x, y] > 5) Layers[x, y] = 5; // Layer indices 0..5
                }
            }
        }

        public float GetMeanHeight(int centerX, int centerY, int halfPatch = 1)
        {
            float sum = 0f;
            int count = 0;
            for (int dx = -halfPatch; dx <= halfPatch; dx++)
            {
                for (int dy = -halfPatch; dy <= halfPatch; dy++)
                {
                    int x = (centerX + dx + Width) % Width;
                    int y = Mathf.Clamp(centerY + dy, 0, Height - 1);
                    sum += Heights[x, y];
                    count++;
                }
            }
            return sum / count;
        }

        public void ApplyDirectRemoval(int x, int y, float amountMm)
        {
            x = (x + Width) % Width;
            y = Mathf.Clamp(y, 0, Height - 1);

            int oldLayer = Layers[x, y];
            Heights[x, y] = Mathf.Clamp(Heights[x, y] - amountMm, 0f, MaxDepthMm);
            int newLayer = Mathf.Clamp(Mathf.FloorToInt((Heights[x, y] - 0.001f) / LayerThickness), 0, 5);

            if (newLayer < oldLayer)
            {
                Layers[x, y] = newLayer;
                OnLayerDecreased?.Invoke(x, y, newLayer);
            }
        }

        public void ApplyAbrasion(int centerX, int centerY, float pN, float vMmS, float sFactor, float dt)
        {
            // Law: dh/dt = -kw * (p/p0) * clamp(v/v0, 0.25, 1.60)^0.7 * S * (1/H)
            float pRatio = pN / WorryStoneSpiritConfig.NominalPressureP0;
            float vRatio = Mathf.Clamp(vMmS / WorryStoneSpiritConfig.NominalSpeedV0, 0.25f, 1.60f);
            float vTerm = Mathf.Pow(vRatio, 0.7f);

            // Update 3x3 footprint
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = (centerX + dx + Width) % Width;
                    int y = Mathf.Clamp(centerY + dy, 0, Height - 1);

                    float hLocal = Hardness[x, y];
                    float dhDt = WorryStoneSpiritConfig.KwRemovalRate * pRatio * vTerm * sFactor * (1.0f / hLocal);
                    float removedMm = dhDt * dt;

                    ApplyDirectRemoval(x, y, removedMm);
                }
            }
        }
    }
}
