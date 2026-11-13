using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.TortoiseBastion
{
    public enum ImpactResult
    {
        Ricochet,
        Absorb,
        Penetrate
    }

    public class ScutePlate
    {
        public int Ring;   // 1..6
        public int Sector; // 0..11
        public float BaseThicknessMm;
        public float BracedTimerSec = 0f;
        public float ScorchedTimerSec = 0f;

        public bool IsBraced => BracedTimerSec > 0f;
        public bool IsScorched => ScorchedTimerSec > 0f;

        public float EffectiveThickness => BaseThicknessMm * (IsBraced ? 1.30f : 1.0f);

        public ScutePlate(int ring, int sector)
        {
            Ring = ring;
            Sector = sector;
            BaseThicknessMm = 6.0f - 0.6f * ring;
        }

        public bool TryBrace(bool nearSeam)
        {
            if (nearSeam) return false;
            if (IsScorched) return false;

            BracedTimerSec = TortoiseBastionConfig.BraceDurationSec;
            return true;
        }

        public void Scorch()
        {
            ScorchedTimerSec = TortoiseBastionConfig.ScorchDurationSec;
            BracedTimerSec = 0f;
        }

        public void Update(float dt)
        {
            if (BracedTimerSec > 0f)
                BracedTimerSec = Mathf.Max(0f, BracedTimerSec - dt);
            if (ScorchedTimerSec > 0f)
                ScorchedTimerSec = Mathf.Max(0f, ScorchedTimerSec - dt);
        }
    }

    public class DragonAgent
    {
        public int Id;
        public int Hp = 3;
        public Vector3 Position;

        public DragonAgent(int id, Vector3 pos)
        {
            Id = id;
            Position = pos;
        }
    }

    public class TortoiseSimulation
    {
        public Dictionary<int, ScutePlate> Plates = new Dictionary<int, ScutePlate>();
        public List<DragonAgent> Dragons = new List<DragonAgent>();

        public float DomeYawDeg = 0f;
        public int Integrity = TortoiseBastionConfig.InitialIntegrity;
        public int Score = 0;
        public int Penetrations = 0;
        public int Absorbs = 0;
        public int Ricochets = 0;
        public int DragonsKilled = 0;

        public float CannonChargeSec = 0f;
        public bool CannonFired = false;
        public bool SiegeEnded = false;

        public TortoiseSimulation()
        {
            for (int k = 1; k <= 6; k++)
            {
                for (int s = 0; s < 12; s++)
                {
                    int key = k * 100 + s;
                    Plates[key] = new ScutePlate(k, s);
                }
            }

            Dragons.Add(new DragonAgent(1, new Vector3(0, 250f, 550f)));
            Dragons.Add(new DragonAgent(2, new Vector3(400f, 250f, 400f)));
        }

        public static void MapCoordinatesToPlate(float thetaDeg, float lambdaDeg, out int ring, out int sector)
        {
            ring = Mathf.Clamp(Mathf.FloorToInt(thetaDeg / 10.0f) + 1, 1, 6);
            sector = Mathf.Clamp(Mathf.FloorToInt(lambdaDeg / 30.0f), 0, 11);
        }

        public ScutePlate GetPlate(int ring, int sector)
        {
            int key = ring * 100 + sector;
            return Plates.TryGetValue(key, out var plate) ? plate : null;
        }

        public void SetDomeYaw(float yawDeg)
        {
            DomeYawDeg = yawDeg;
        }

        public ImpactResult ResolveFireball(ScutePlate plate, Vector3 fireballDir, Vector3 surfaceNormal, float fireballPowerMm, bool onSeam)
        {
            float impactAngleDeg = Vector3.Angle(-fireballDir, surfaceNormal);
            if (impactAngleDeg > TortoiseBastionConfig.RicochetAngleDeg)
            {
                Ricochets++;
                Score += 100;
                return ImpactResult.Ricochet;
            }

            float tEff = plate.EffectiveThickness;
            if (onSeam || tEff < fireballPowerMm)
            {
                // Penetration
                plate.Scorch();
                Integrity--;
                Penetrations++;
                Score -= 100;
                if (Integrity <= 0)
                {
                    SiegeEnded = true;
                }
                return ImpactResult.Penetrate;
            }
            else
            {
                // Absorb
                Absorbs++;
                Score += 150;
                return ImpactResult.Absorb;
            }
        }

        public bool ChargeApexCannon(float dt, float pressForceN)
        {
            if (pressForceN >= 0.6f)
            {
                CannonChargeSec += dt;
                if (CannonChargeSec >= TortoiseBastionConfig.CannonChargeTimeSec)
                {
                    return true;
                }
            }
            else
            {
                CannonChargeSec = 0f;
            }
            return false;
        }

        public bool ReleaseApexCannon(Vector3 apexNormal, out DragonAgent targetHit)
        {
            targetHit = null;
            if (CannonChargeSec < TortoiseBastionConfig.CannonChargeTimeSec)
            {
                CannonChargeSec = 0f;
                return false;
            }

            CannonChargeSec = 0f;
            CannonFired = true;

            // Hitscan check against dragons
            foreach (var dragon in Dragons)
            {
                if (dragon.Hp <= 0) continue;
                Vector3 toDragon = dragon.Position;
                if (toDragon.magnitude <= TortoiseBastionConfig.CannonRangeMm)
                {
                    float angle = Vector3.Angle(apexNormal, toDragon);
                    if (angle <= TortoiseBastionConfig.CannonConeDeg)
                    {
                        dragon.Hp--;
                        targetHit = dragon;
                        if (dragon.Hp <= 0)
                        {
                            DragonsKilled++;
                            Score += 400;
                        }
                        return true;
                    }
                }
            }
            return true;
        }

        public void Step(float dt)
        {
            foreach (var p in Plates.Values)
            {
                p.Update(dt);
            }
        }
    }
}
