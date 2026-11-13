namespace XRGames.Chapter4.PulseCoreReactor
{
    public class ThreatResolver
    {
        public bool ResolveBeam(float shieldRadiusMm, float beamWidthMm, out int damage)
        {
            if (shieldRadiusMm >= beamWidthMm)
            {
                damage = 0;
                return true; // deflected
            }
            damage = 18;
            return false;
        }

        public bool ResolveBomb(float shieldDensity, out int damage)
        {
            if (shieldDensity >= PulseCoreReactorConfig.BombDensityThreshold)
            {
                damage = 0;
                return true; // crushed
            }
            damage = 22;
            return false;
        }
    }
}
