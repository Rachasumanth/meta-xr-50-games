namespace XRGames.Chapter4.ElectronDeflector
{
    public class PoleClamp
    {
        public bool PolesHeld { get; private set; } = true;

        public void SetPolesHeld(bool held)
        {
            PolesHeld = held;
        }
    }
}
