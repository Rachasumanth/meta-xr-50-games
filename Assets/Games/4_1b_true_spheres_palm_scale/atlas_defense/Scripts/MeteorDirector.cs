using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense
{
    public class MeteorInstance
    {
        public int Id;
        public Vector3 Direction;
        public float LockTimeSec;
        public bool IsLocked;
        public bool IsResolved;
    }

    public class MeteorDirector
    {
        public List<MeteorInstance> ActiveMeteors { get; } = new List<MeteorInstance>();
        public int SpawnedThisWave { get; private set; }

        public bool TrySpawnMeteor(Vector3 dir)
        {
            if (ActiveMeteors.Count >= AtlasDefenseConfig.MaxConcurrentMeteors) return false;
            if (SpawnedThisWave >= AtlasDefenseConfig.MeteorsPerWave) return false;

            ActiveMeteors.Add(new MeteorInstance
            {
                Id = ++SpawnedThisWave,
                Direction = dir.normalized
            });
            return true;
        }

        public void Retire(MeteorInstance m)
        {
            ActiveMeteors.Remove(m);
        }

        public void ResetWave()
        {
            ActiveMeteors.Clear();
            SpawnedThisWave = 0;
        }
    }
}
