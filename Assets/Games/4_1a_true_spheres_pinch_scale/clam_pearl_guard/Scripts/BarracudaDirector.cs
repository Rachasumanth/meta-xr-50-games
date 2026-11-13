using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class BarracudaDirector
    {
        public enum StrikeResult
        {
            Wait,      // strike still incoming, cage not needed yet
            Block,     // cage held: no integrity loss (early) or late-block dent
            Bitten,    // cage failed or window passed: full bite damage
            GameOver   // integrity exhausted
        }

        private float _strikeTimer;
        private bool _isTelegraphing;

        public bool IsTelegraphing => _isTelegraphing;

        public void StartStrike(float durationMs)
        {
            _strikeTimer = durationMs;
            _isTelegraphing = true;
        }

        public void UpdateTimer(float deltaTime)
        {
            if (_strikeTimer > 0f)
            {
                _strikeTimer -= deltaTime * 1000f;
                if (_strikeTimer <= 0f)
                {
                    _isTelegraphing = false;
                }
            }
        }

        // Bite timing resolution (spec item 8): strike landing at 610 ms with the
        // cage complete -> full block, damage 0; at 700 ms -> late block, damage 11;
        // at 800 ms -> full bite, damage 22. The late window [620, 740] still blocks
        // with a dent; beyond it the bite lands regardless of cage state.
        // closureMs decides the fast bonus when the early block scores (item 7).
        public StrikeResult ResolveStrike(float strikeTimeMs, bool cageClosed, float closureMs, ref int integrity, ref int score)
        {
            StrikeResult result;

            if (strikeTimeMs <= ClamPearlGuardConfig.StrikeTimeEarlyMs)
            {
                if (cageClosed)
                {
                    result = StrikeResult.Block;
                    score += closureMs <= ClamPearlGuardConfig.CageClosureTimeMsMax
                        ? ClamPearlGuardConfig.ScoreFastBonusPoints
                        : ClamPearlGuardConfig.ScoreNormalBlockPoints;
                }
                else
                {
                    result = StrikeResult.Wait;
                }
            }
            else if (strikeTimeMs <= ClamPearlGuardConfig.StrikeTimeLateMs)
            {
                if (cageClosed)
                {
                    integrity -= ClamPearlGuardConfig.DamageEarlyBlock;
                    score += ClamPearlGuardConfig.ScoreNormalBlockPoints;
                    result = StrikeResult.Block;
                }
                else
                {
                    integrity -= ClamPearlGuardConfig.DamageBitten;
                    result = StrikeResult.Bitten;
                }
            }
            else
            {
                // Beyond the late window the bite lands even with the cage closed.
                integrity -= ClamPearlGuardConfig.DamageBitten;
                result = StrikeResult.Bitten;
            }

            if (integrity <= 0)
            {
                integrity = Mathf.Max(0, integrity);
                return StrikeResult.GameOver;
            }
            return result;
        }
    }
}
