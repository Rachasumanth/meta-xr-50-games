using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense
{
    public class City
    {
        public int Id;
        public Vector3 PositionOnGlobe;
        public int HP = 3;
        public bool IsDestroyed => HP <= 0;
    }

    public class CityGrid
    {
        public List<City> Cities { get; } = new List<City>();

        public CityGrid()
        {
            for (int i = 0; i < AtlasDefenseConfig.TotalCities; i++)
            {
                float angle = i * 72f * Mathf.Deg2Rad;
                Cities.Add(new City
                {
                    Id = i,
                    PositionOnGlobe = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * AtlasDefenseConfig.GlobeRadiusMm,
                    HP = 3
                });
            }
        }

        public City FindNearestCity(Vector3 impactPointMm)
        {
            City nearest = Cities[0];
            float bestDist = float.MaxValue;
            foreach (var c in Cities)
            {
                float d = Vector3.Distance(c.PositionOnGlobe, impactPointMm);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = c;
                }
            }
            return nearest;
        }

        public void DamageCity(City c, int damage = 1)
        {
            if (c != null && !c.IsDestroyed)
            {
                c.HP = Mathf.Max(0, c.HP - damage);
            }
        }
    }
}
