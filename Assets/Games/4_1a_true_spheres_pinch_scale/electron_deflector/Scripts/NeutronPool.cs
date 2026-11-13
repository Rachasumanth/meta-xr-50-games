using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector
{
    public class NeutronAgent
    {
        public int Id;
        public Vector3 Position;
        public int HP = 1;
        public bool IsHeavy;
        public bool IsAbsorbed;
        public bool IsCollected;
    }

    public class NeutronPool
    {
        public List<NeutronAgent> Neutrons { get; } = new List<NeutronAgent>();

        public NeutronAgent SpawnNeutron(bool isHeavy = false)
        {
            var n = new NeutronAgent
            {
                Id = Neutrons.Count + 1,
                Position = Vector3.forward * 200f,
                IsHeavy = isHeavy,
                HP = isHeavy ? 2 : 1
            };
            Neutrons.Add(n);
            return n;
        }

        public bool ApplyBurstDamage(NeutronAgent n, int damage = 1)
        {
            if (n == null || n.IsAbsorbed || n.IsCollected) return false;
            n.HP -= damage;
            return n.HP <= 0;
        }
    }
}
