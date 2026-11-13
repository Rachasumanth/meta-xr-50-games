using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class PearlState
    {
        private float _charge;
        private int _integrity = 100;
        private float _shield;
        private int _score;

        public float Charge => _charge;
        public int Integrity => _integrity;
        public float Shield => _shield;
        public int Score => _score;

        public void AddScore(int amount) => _score += amount;
        public void TakeDamage(int amount) => _integrity = Mathf.Clamp(_integrity - amount, 0, 100);
        public void AddCharge(float amount) => _charge = Mathf.Clamp(_charge + amount, 0f, 100f);
        public void SetShield(float amount) => _shield = Mathf.Clamp01(amount);

        public void Reset()
        {
            _charge = 0f;
            _integrity = 100;
            _shield = 0f;
            _score = 0;
        }
    }
}
