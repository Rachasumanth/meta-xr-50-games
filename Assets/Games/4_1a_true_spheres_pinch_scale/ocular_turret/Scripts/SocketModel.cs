namespace XRGames.Chapter4.OcularTurret
{
    public class SocketModel
    {
        public int IntegrityPips { get; private set; } = 3;
        public bool IsSocketClosed { get; private set; } = true;

        public void SetSocketOpen(bool isOpen)
        {
            IsSocketClosed = !isOpen;
        }

        public void DamageSocket()
        {
            if (IntegrityPips > 0) IntegrityPips--;
        }

        public void Reset()
        {
            IntegrityPips = 3;
            IsSocketClosed = true;
        }
    }
}
