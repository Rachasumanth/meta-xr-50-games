namespace XRGames.Chapter4.OcularTurret
{
    public class VigilDirector
    {
        public int CurrentVigilLevel { get; private set; } = 1;
        public int RemainingGargoyles { get; private set; } = 12;

        public void StartVigil(int level)
        {
            CurrentVigilLevel = level;
            RemainingGargoyles = level * 4;
        }

        public void DefeatGargoyle()
        {
            if (RemainingGargoyles > 0) RemainingGargoyles--;
        }
    }
}
