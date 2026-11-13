using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class SessionFSM
    {
        public enum State
        {
            Idle,
            Playing,
            Paused,
            Resume,
            GameOver,
            Completed
        }

        private State _currentState = State.Idle;
        public State CurrentState => _currentState;

        public void ChangeState(State newState)
        {
            _currentState = newState;
        }

        public void Reset()
        {
            _currentState = State.Idle;
        }
    }
}
