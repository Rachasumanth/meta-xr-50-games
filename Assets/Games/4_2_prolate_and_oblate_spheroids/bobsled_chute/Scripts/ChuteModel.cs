using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public class ChuteModel
    {
        public const int TableSize = 256;
        public const float TotalLength = BobsledChuteConfig.ChuteLengthM; // 0.900 m
        public float[] CurvatureTable = new float[TableSize];

        public ChuteModel()
        {
            // Sample curvature kappa(s) with apexes at s = 150mm and 450mm reaching kappa = 2.42 m^-1
            for (int i = 0; i < TableSize; i++)
            {
                float s = (i / (float)(TableSize - 1)) * TotalLength;
                // Double hairpin model
                float h1 = Mathf.Exp(-Mathf.Pow((s - 0.150f) / 0.050f, 2f));
                float h2 = Mathf.Exp(-Mathf.Pow((s - 0.450f) / 0.050f, 2f));
                CurvatureTable[i] = Mathf.Max(0.1f, 2.42f * (h1 + h2));
            }
        }

        public float GetCurvature(float sArc)
        {
            float norm = Mathf.Clamp01(sArc / TotalLength);
            int idx = Mathf.Min(TableSize - 1, Mathf.FloorToInt(norm * (TableSize - 1)));
            return CurvatureTable[idx];
        }
    }
}
