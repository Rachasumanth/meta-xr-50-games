namespace XRGames.Chapter4.PulseCoreReactor
{
    public class CoreStateMachine
    {
        public int CoreHP { get; private set; } = 100;
        public bool IsGameOver => CoreHP <= 0;

        public void ApplyDamage(int amount)
        {
            CoreHP = System.Math.Max(0, CoreHP - amount);
        }

        public void Reset()
        {
            CoreHP = 100;
        }
    }
}
