using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public enum CradleState
    {
        Held,
        Slip,
        Dropped
    }

    public class CradleMonitor
    {
        public CradleState State { get; private set; } = CradleState.Held;
        public int CradleContactsCount { get; set; } = 3;

        public void SetState(CradleState state)
        {
            State = state;
        }

        public void UpdateGrip(float propAngularSpeedDegS)
        {
            if (CradleContactsCount < 2)
            {
                State = CradleState.Dropped;
            }
            else if (propAngularSpeedDegS > 30.0f)
            {
                State = CradleState.Slip;
            }
            else
            {
                State = CradleState.Held;
            }
        }
    }
}
