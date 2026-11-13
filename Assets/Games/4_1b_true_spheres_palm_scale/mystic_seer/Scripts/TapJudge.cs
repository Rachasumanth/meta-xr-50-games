using System;
using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public struct TapEvent
    {
        public Vector3 Direction;
        public float LateralErrorMm;
        public float HeightMm;
    }

    public class TapJudge
    {
        public bool JudgeTap(TapEvent tap, ThiefDirector thiefDirector, FogField fog, SeanceScoring scoring)
        {
            float r = MysticSeerConfig.OrbRadiusMm;
            Thief bestThief = null;
            float bestDist = float.MaxValue;

            // Height boundary: tap height must be <= 5 mm
            if (tap.HeightMm <= MysticSeerConfig.MaxTapHeightMm)
            {
                for (int i = 0; i < thiefDirector.Thieves.Count; i++)
                {
                    var t = thiefDirector.Thieves[i];
                    if (t.State == ThiefState.Revealed)
                    {
                        float angleRad = Vector3.Angle(tap.Direction, t.Direction) * Mathf.Deg2Rad;
                        float e = r * angleRad;
                        if (tap.LateralErrorMm > 0)
                        {
                            e = tap.LateralErrorMm;
                        }

                        if (e <= MysticSeerConfig.MaxTapDistanceMm && e < bestDist)
                        {
                            bestDist = e;
                            bestThief = t;
                        }
                    }
                }
            }

            if (bestThief != null)
            {
                thiefDirector.BanishThief(bestThief);
                if (scoring != null)
                {
                    scoring.OnBanish(bestThief, fog.Sample(bestThief.Direction));
                }
                return true;
            }
            else
            {
                // Wrong tap or missed contact
                if (fog != null)
                {
                    fog.ApplySmother(0.45f);
                }
                if (scoring != null)
                {
                    scoring.OnWrongTap();
                }
                return false;
            }
        }
    }
}
