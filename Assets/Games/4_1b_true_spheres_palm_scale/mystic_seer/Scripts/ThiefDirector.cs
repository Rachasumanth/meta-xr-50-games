using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public enum ThiefState
    {
        Hidden,
        Revealed,
        Banished,
        Escaped
    }

    public class Thief
    {
        public int Id;
        public Vector3 Direction;
        public ThiefState State = ThiefState.Hidden;
        public float RevealHoldTimer = 0f;
        public float EscapeTimer = 0f;
    }

    public class ThiefDirector
    {
        public List<Thief> Thieves { get; } = new List<Thief>();
        public int RelicsRemaining { get; private set; } = 3;
        public int EscapeCount { get; private set; } = 0;
        public bool IsGameOver { get; private set; } = false;

        public event Action<Thief> OnThiefRevealed;
        public event Action<Thief> OnThiefEscaped;
        public event Action<Thief> OnThiefBanished;
        public event Action OnGameOver;

        public void SpawnThief(int id, Vector3 dir)
        {
            Thieves.Add(new Thief
            {
                Id = id,
                Direction = dir.normalized,
                State = ThiefState.Hidden,
                RevealHoldTimer = 0f,
                EscapeTimer = 0f
            });
        }

        public void Tick(float dt, FogField fog)
        {
            if (IsGameOver) return;

            for (int i = 0; i < Thieves.Count; i++)
            {
                var t = Thieves[i];
                if (t.State == ThiefState.Hidden)
                {
                    float f = fog.Sample(t.Direction);
                    if (f < MysticSeerConfig.RevealThreshold)
                    {
                        t.RevealHoldTimer += dt;
                        if (t.RevealHoldTimer >= MysticSeerConfig.RevealHoldTimeSec)
                        {
                            t.State = ThiefState.Revealed;
                            t.EscapeTimer = 0f;
                            OnThiefRevealed?.Invoke(t);
                        }
                    }
                    else
                    {
                        t.RevealHoldTimer = 0f;
                    }
                }
                else if (t.State == ThiefState.Revealed)
                {
                    t.EscapeTimer += dt;
                    if (t.EscapeTimer >= MysticSeerConfig.EscapeDurationSec)
                    {
                        EscapeThief(t, fog);
                    }
                }
            }
        }

        public void EscapeThief(Thief t, FogField fog = null)
        {
            if (IsGameOver || t.State == ThiefState.Escaped || t.State == ThiefState.Banished) return;

            t.State = ThiefState.Escaped;
            EscapeCount++;
            RelicsRemaining = Mathf.Max(0, RelicsRemaining - 1);
            if (fog != null)
            {
                fog.AddBurst(t.Direction, 0.35f);
            }
            OnThiefEscaped?.Invoke(t);

            if (EscapeCount >= MysticSeerConfig.MaxEscapesAllowed && !IsGameOver)
            {
                IsGameOver = true;
                OnGameOver?.Invoke();
            }
        }

        public void BanishThief(Thief t)
        {
            if (t.State == ThiefState.Revealed)
            {
                t.State = ThiefState.Banished;
                OnThiefBanished?.Invoke(t);
            }
        }
    }
}
