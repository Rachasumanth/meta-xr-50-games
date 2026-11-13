using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public class Fragment
    {
        public int Id;
        public float LatitudePhiDeg;
        public float LongitudeLambdaDeg;
        public int GridX;
        public int GridY;
        public bool IsRevealed;
        public bool IsLost;

        public Fragment(int id, float phi, float lambda, int gx, int gy)
        {
            Id = id;
            LatitudePhiDeg = phi;
            LongitudeLambdaDeg = lambda;
            GridX = gx;
            GridY = gy;
            IsRevealed = false;
            IsLost = false;
        }
    }

    public class FragmentField
    {
        public List<Fragment> Fragments { get; } = new List<Fragment>();
        public int RevealedCount { get; private set; } = 0;
        public int LostCount { get; private set; } = 0;

        public event Action<Fragment> OnFragmentRevealed;
        public event Action<Fragment> OnFragmentLost;

        public FragmentField()
        {
            Reset();
        }

        public void Reset()
        {
            Fragments.Clear();
            RevealedCount = 0;
            LostCount = 0;

            // Seed 10 fragments
            for (int i = 0; i < 10; i++)
            {
                float phi = 15.0f + i * 7.0f; // 15 to 78 deg
                float lambda = i * 15.0f;
                int gx = Mathf.FloorToInt((lambda / 360f) * AbrasionField.Width) % AbrasionField.Width;
                int gy = Mathf.Clamp(Mathf.FloorToInt(((phi + 90f) / 180f) * AbrasionField.Height), 0, AbrasionField.Height - 1);
                Fragments.Add(new Fragment(i, phi, lambda, gx, gy));
            }
        }

        public void CheckReveals(AbrasionField field)
        {
            foreach (var f in Fragments)
            {
                if (f.IsRevealed || f.IsLost) continue;

                float meanH = field.GetMeanHeight(f.GridX, f.GridY, 1);
                if (meanH < 0.15f)
                {
                    f.IsRevealed = true;
                    RevealedCount++;
                    OnFragmentRevealed?.Invoke(f);
                }
            }
        }

        public void CheckTideLoss(float tidePhiDeg)
        {
            foreach (var f in Fragments)
            {
                if (f.IsRevealed || f.IsLost) continue;

                if (tidePhiDeg > (f.LatitudePhiDeg - 5.0f))
                {
                    f.IsLost = true;
                    LostCount++;
                    OnFragmentLost?.Invoke(f);
                }
            }
        }

        public int ComputeMaxConnectedComponent()
        {
            // Simple graph search between revealed fragments within 30 deg
            var revealed = Fragments.FindAll(f => f.IsRevealed);
            if (revealed.Count == 0) return 0;

            int n = revealed.Count;
            bool[] visited = new bool[n];
            int maxComp = 0;

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                int currentComp = 0;
                var queue = new Queue<int>();
                queue.Enqueue(i);
                visited[i] = true;

                while (queue.Count > 0)
                {
                    int curr = queue.Dequeue();
                    currentComp++;

                    for (int j = 0; j < n; j++)
                    {
                        if (!visited[j])
                        {
                            float dPhi = revealed[curr].LatitudePhiDeg - revealed[j].LatitudePhiDeg;
                            float dLam = Mathf.DeltaAngle(revealed[curr].LongitudeLambdaDeg, revealed[j].LongitudeLambdaDeg);
                            float angularDist = Mathf.Sqrt(dPhi * dPhi + dLam * dLam);
                            if (angularDist <= 30.0f)
                            {
                                visited[j] = true;
                                queue.Enqueue(j);
                            }
                        }
                    }
                }
                maxComp = Mathf.Max(maxComp, currentComp);
            }
            return maxComp;
        }
    }
}
