using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class TideDirector
    {
        private int _currentTide = 1;
        private float _tideTimer;

        public int CurrentTide => _currentTide;

        public void AdvanceTide()
        {
            if (_currentTide < 5) _currentTide++;
        }

        public void UpdateTide(float deltaTime)
        {
            _tideTimer += deltaTime;
        }

        public void Reset()
        {
            _currentTide = 1;
            _tideTimer = 0f;
        }
    }
}
