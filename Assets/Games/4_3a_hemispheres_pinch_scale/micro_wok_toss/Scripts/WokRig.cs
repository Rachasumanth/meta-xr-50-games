using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.MicroWokToss
{
    public class WokRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform BowlTransform;
        public Transform BurnerTransform;
        public Transform PlateTransform;

        [Header("State")]
        public ThermalModel Thermal = new ThermalModel();
        public List<Ingredient> Ingredients = new List<Ingredient>();

        public int DishesCompleted = 0;
        public int OverflowCount = 0;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            Thermal.StepCooking(dt);
            for (int i = 0; i < Ingredients.Count; i++)
            {
                Ingredients[i].StepThermal(Thermal.TemperatureC, 1.0f, dt);
            }
        }
    }
}
