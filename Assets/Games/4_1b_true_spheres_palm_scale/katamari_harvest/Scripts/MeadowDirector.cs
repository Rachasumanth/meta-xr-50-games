using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest
{
    public class MeadowDirector
    {
        public List<HarvestEntity> Entities { get; } = new List<HarvestEntity>();
        public int ActiveSheepCount => Entities.FindAll(e => e.Kind == HarvestKind.Sheep && !e.Captured).Count;

        private float _respawnTimer = 0f;
        private const float RespawnDelay = 1.5f;

        public void Initialize(int sheepCount = 60, int fenceCount = 16, int barnCount = 5, int mineCount = 4)
        {
            Entities.Clear();
            int id = 0;
            for (int i = 0; i < sheepCount; i++)
            {
                Entities.Add(new HarvestEntity { Id = ++id, Kind = HarvestKind.Sheep, RadiusMm = 9.0f, Mass = 10.0f, Position = new Vector2(i * 5f - 50f, 20f) });
            }
            for (int i = 0; i < fenceCount; i++)
            {
                Entities.Add(new HarvestEntity { Id = ++id, Kind = HarvestKind.Fence, RadiusMm = 15.0f, Mass = 25.0f, Position = new Vector2(i * 10f - 80f, 60f) });
            }
            for (int i = 0; i < barnCount; i++)
            {
                Entities.Add(new HarvestEntity { Id = ++id, Kind = HarvestKind.Barn, RadiusMm = 38.0f, Mass = 100.0f, Position = new Vector2(i * 40f - 80f, 100f) });
            }
            for (int i = 0; i < mineCount; i++)
            {
                Entities.Add(new HarvestEntity { Id = ++id, Kind = HarvestKind.Mine, RadiusMm = 12.0f, Mass = 0.0f, Position = new Vector2(i * 30f - 60f, -40f) });
            }
        }

        public void Update(float dt, Vector2 spherePositionMm)
        {
            _respawnTimer += dt;
            if (_respawnTimer >= RespawnDelay)
            {
                _respawnTimer = 0f;
                // Respawn floor: keep active sheep >= 45
                int needed = 45 - ActiveSheepCount;
                if (needed > 0)
                {
                    int idCounter = Entities.Count;
                    for (int i = 0; i < needed; i++)
                    {
                        Vector2 pos;
                        int attempts = 0;
                        do
                        {
                            pos = new Vector2(Random.Range(-120f, 120f), Random.Range(-90f, 90f));
                            attempts++;
                        } while (Vector2.Distance(pos, spherePositionMm) < 60.0f && attempts < 20);

                        if (Vector2.Distance(pos, spherePositionMm) < 60.0f)
                        {
                            pos = spherePositionMm + new Vector2(65.0f, 0f);
                        }

                        Entities.Add(new HarvestEntity
                        {
                            Id = ++idCounter,
                            Kind = HarvestKind.Sheep,
                            RadiusMm = 9.0f,
                            Mass = 10.0f,
                            Position = pos,
                            Captured = false
                        });
                    }
                }
            }
        }
    }
}
