using System;
using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class BellyCollider
    {
        public const float RestitutionE = 0.35f;
        public const float EffMass = 0.01765f; // kg
        public const float ImpMass = 0.020f;   // 20g
        public const float BellyRadius = 0.022f; // 22mm
        public const float ImpRadius = 0.006f;   // 6mm

        public event Action<float> OnImpBumped;

        public bool CheckBump(Vector3 sumoPos, float thetaDeg, float vBelly, ImpEntity imp, out float resultingImpSpeed)
        {
            resultingImpSpeed = 0f;
            if (!imp.IsAlive || imp.IsRingOut) return false;

            float dist = Vector3.Distance(sumoPos, imp.Position);
            if (dist <= (BellyRadius + ImpRadius + 0.005f)) // ~28-33mm
            {
                // J_imp = (1 + e) * m_eff * v_rel
                float jImp = (1.0f + RestitutionE) * EffMass * vBelly;
                resultingImpSpeed = jImp / ImpMass; // outward speed 0.45 - 0.65 m/s
                imp.Velocity = (imp.Position - sumoPos).normalized * resultingImpSpeed;
                imp.WasBumped = true;
                OnImpBumped?.Invoke(thetaDeg);
                return true;
            }
            return false;
        }
    }
}
