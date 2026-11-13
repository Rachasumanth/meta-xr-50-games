using UnityEngine;

namespace XRGames.Chapter4.CagePinball
{
    public class PinballSystem
    {
        public bool CheckCrystalHit(float angleOffTargetDeg)
        {
            return Mathf.Abs(angleOffTargetDeg) <= CagePinballConfig.PinballAimThresholdDeg;
        }
    }

    public class CrystalRing
    {
        public const int TotalSlots = 8;
        public bool[] Slots { get; } = new bool[TotalSlots];
        public int BrokenCount { get; private set; }

        public CrystalRing()
        {
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < TotalSlots; i++) Slots[i] = true;
            BrokenCount = 0;
        }

        public bool BreakSlot(int index)
        {
            if (index >= 0 && index < TotalSlots && Slots[index])
            {
                Slots[index] = false;
                BrokenCount++;
                return true;
            }
            return false;
        }
    }
}
