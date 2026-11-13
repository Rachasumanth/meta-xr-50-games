using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public enum SessionState
    {
        Ready,
        Running,
        Ended
    }

    public class SessionFSM
    {
        public SessionState State { get; private set; } = SessionState.Ready;

        public void StartSession()
        {
            State = SessionState.Running;
        }

        public void EndSession()
        {
            State = SessionState.Ended;
        }
    }
}
