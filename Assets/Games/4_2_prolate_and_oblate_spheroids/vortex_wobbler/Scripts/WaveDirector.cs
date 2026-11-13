using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.VortexWobbler
{
    public class WaveDirector
    {
        public List<FieldEntity> Entities { get; } = new List<FieldEntity>();
        public int CurrentWave { get; private set; } = 1;
        public float WaveTimer { get; private set; } = 0f;
        public bool IsWaveClear => Entities.TrueForAll(e => e.IsResolved);

        public void SpawnWave(int waveIndex)
        {
            Entities.Clear();
            CurrentWave = waveIndex;
            WaveTimer = 0f;

            int debrisCount = (waveIndex == 2) ? 8 : 6;
            int podCount = (waveIndex == 2) ? 4 : 3;

            for (int i = 0; i < debrisCount; i++)
            {
                Entities.Add(new FieldEntity(EntityType.Debris, 60.0f + i * 5.0f, i * 45.0f));
            }

            for (int i = 0; i < podCount; i++)
            {
                Entities.Add(new FieldEntity(EntityType.Pod, 50.0f + i * 8.0f, 20.0f + i * 90.0f));
            }
        }

        public void Step(float dt, VortexField field, RockModel rock, AirlockModel airlock)
        {
            WaveTimer += dt;

            if (rock.State == VortexState.OVERDRIVE)
            {
                airlock.StepOverdriveHeat(dt);
            }

            foreach (var e in Entities)
            {
                if (e.IsResolved) continue;

                float vRad = field.ComputeRadialInflowSpeed(e.RadiusMm, rock.AmplitudeDeg, e.Type == EntityType.Debris);
                float vTang = field.ComputeTangentialVelocity(e.RadiusMm, rock.AmplitudeDeg);
                e.StepInflow(vRad, vTang, dt);

                if (e.Type == EntityType.Pod && e.RadiusMm <= VortexWobblerConfig.AirlockRadiusMm)
                {
                    e.IsResolved = true;
                    airlock.HandlePodDock();
                }
                else if (e.Type == EntityType.Debris && e.RadiusMm <= VortexWobblerConfig.BreachRadiusMm)
                {
                    e.IsResolved = true;
                    airlock.HandleDebrisBreach();
                }
            }
        }
    }
}
