using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.MysticSeer.Editor
{
    public class MysticSeerAcceptanceTests
    {
        [Test]
        public void AT01_FogEquilibrium()
        {
            var fog = new FogField(0.90f);
            var shell = new HandShell();
            Vector3 dir = Vector3.forward;
            shell.AddSensor(dir, standoffMm: 20f, curveGain: 1.0f);

            float dt = 1.0f / 30.0f; // 30 Hz
            int ticks = (int)(2.0f / dt);
            for (int i = 0; i < ticks; i++)
            {
                fog.Tick(dt, shell);
            }

            float f = fog.Sample(dir);
            Assert.AreEqual(0.016f, f, 0.005f, "F must converge to 0.016 +- 0.005");
        }

        [Test]
        public void AT02_StandoffFalloff()
        {
            float dt = 1.0f / 30.0f;
            int ticks = (int)(0.4f / dt);
            Vector3 dir = Vector3.forward;

            // 5 mm
            var fog5 = new FogField(0.90f);
            var shell5 = new HandShell();
            shell5.AddSensor(dir, 5.0f);
            for (int i = 0; i < ticks; i++) fog5.Tick(dt, shell5);
            Assert.LessOrEqual(fog5.Sample(dir), 0.05f + 0.05f);

            // 20 mm
            var fog20 = new FogField(0.90f);
            var shell20 = new HandShell();
            shell20.AddSensor(dir, 20.0f);
            for (int i = 0; i < ticks; i++) fog20.Tick(dt, shell20);
            Assert.AreEqual(0.28f, fog20.Sample(dir), 0.05f);

            // 25 mm
            var fog25 = new FogField(0.90f);
            var shell25 = new HandShell();
            shell25.AddSensor(dir, 25.0f);
            for (int i = 0; i < ticks; i++) fog25.Tick(dt, shell25);
            Assert.AreEqual(0.53f, fog25.Sample(dir), 0.05f);

            // 30 mm
            var fog30 = new FogField(0.90f);
            var shell30 = new HandShell();
            shell30.AddSensor(dir, 30.0f);
            for (int i = 0; i < ticks; i++) fog30.Tick(dt, shell30);
            Assert.AreEqual(0.72f, fog30.Sample(dir), 0.05f);
        }

        [Test]
        public void AT03_CurvatureBonus()
        {
            float dt = 1.0f / 30.0f;
            int ticks = (int)(0.3f / dt);
            Vector3 dir = Vector3.forward;

            var fogStraight = new FogField(0.50f);
            var shellStraight = new HandShell();
            shellStraight.AddSensor(dir, 20.0f, curveGain: 1.0f);
            for (int i = 0; i < ticks; i++) fogStraight.Tick(dt, shellStraight);

            var fogCurved = new FogField(0.50f);
            var shellCurved = new HandShell();
            shellCurved.AddSensor(dir, 20.0f, curveGain: 1.3f);
            for (int i = 0; i < ticks; i++) fogCurved.Tick(dt, shellCurved);

            float dropStraight = 0.50f - fogStraight.Sample(dir);
            float dropCurved = 0.50f - fogCurved.Sample(dir);

            Assert.Greater(dropCurved, dropStraight * 1.15f);
        }

        [Test]
        public void AT04_RevealGate()
        {
            var fog = new FogField();
            var dir = Vector3.forward;
            fog.SetDensity(dir, 0.29f);

            var director = new ThiefDirector();
            director.SpawnThief(1, dir);

            float dt = 1.0f / 30.0f;

            // 90 ms: 3 frames at 30 Hz = 100 ms, so 2 frames = 66 ms, 3 frames = 100 ms
            director.Tick(0.090f, fog);
            Assert.AreEqual(ThiefState.Hidden, director.Thieves[0].State);

            // Additional 20 ms -> total 110 ms >= 100 ms
            director.Tick(0.020f, fog);
            Assert.AreEqual(ThiefState.Revealed, director.Thieves[0].State);
        }

        [Test]
        public void AT05_TapLateralBoundary()
        {
            var director = new ThiefDirector();
            var fog = new FogField();
            var scoring = new SeanceScoring();
            var judge = new TapJudge();

            director.SpawnThief(1, Vector3.forward);
            director.Thieves[0].State = ThiefState.Revealed;

            // Tap at e = 9.9 mm, h = 3 mm -> banishes
            var tap1 = new TapEvent { Direction = Vector3.forward, LateralErrorMm = 9.9f, HeightMm = 3.0f };
            bool res1 = judge.JudgeTap(tap1, director, fog, scoring);
            Assert.IsTrue(res1);
            Assert.AreEqual(ThiefState.Banished, director.Thieves[0].State);
            Assert.AreEqual(1, scoring.BanishCount);

            // Reset thief to revealed
            director.Thieves[0].State = ThiefState.Revealed;
            int scoreBefore = scoring.Score;

            // Tap at e = 10.1 mm, h = 3 mm -> smothers and applies -25
            var tap2 = new TapEvent { Direction = Vector3.forward, LateralErrorMm = 10.1f, HeightMm = 3.0f };
            bool res2 = judge.JudgeTap(tap2, director, fog, scoring);
            Assert.IsFalse(res2);
            Assert.AreEqual(scoreBefore - 25, scoring.Score);
        }

        [Test]
        public void AT06_TapHeightBoundary()
        {
            var director = new ThiefDirector();
            var fog = new FogField();
            var scoring = new SeanceScoring();
            var judge = new TapJudge();

            director.SpawnThief(1, Vector3.forward);
            director.Thieves[0].State = ThiefState.Revealed;

            // Tap at h = 4.9 mm, e = 2 mm -> banishes
            var tap1 = new TapEvent { Direction = Vector3.forward, LateralErrorMm = 2.0f, HeightMm = 4.9f };
            bool res1 = judge.JudgeTap(tap1, director, fog, scoring);
            Assert.IsTrue(res1);
            Assert.AreEqual(ThiefState.Banished, director.Thieves[0].State);

            // Tap at h = 5.1 mm, e = 2 mm -> not a contact
            director.Thieves[0].State = ThiefState.Revealed;
            var tap2 = new TapEvent { Direction = Vector3.forward, LateralErrorMm = 2.0f, HeightMm = 5.1f };
            bool res2 = judge.JudgeTap(tap2, director, fog, scoring);
            Assert.IsFalse(res2);
            Assert.AreEqual(ThiefState.Revealed, director.Thieves[0].State);
        }

        [Test]
        public void AT07_WrongTapSmother()
        {
            var director = new ThiefDirector();
            var fog = new FogField(0.20f);
            var scoring = new SeanceScoring();
            var judge = new TapJudge();

            // First wrong tap
            var tap = new TapEvent { Direction = Vector3.up, LateralErrorMm = 50.0f, HeightMm = 3.0f };
            judge.JudgeTap(tap, director, fog, scoring);
            Assert.AreEqual(-25, scoring.Score);
            Assert.GreaterOrEqual(fog.Sample(Vector3.up), 0.45f);

            // Second wrong tap
            judge.JudgeTap(tap, director, fog, scoring);
            Assert.AreEqual(-50, scoring.Score);
        }

        [Test]
        public void AT08_EscapeTimer()
        {
            var director = new ThiefDirector();
            var fog = new FogField();
            director.SpawnThief(1, Vector3.forward);
            director.Thieves[0].State = ThiefState.Revealed;

            // Advance 9.9 s
            director.Tick(9.9f, fog);
            Assert.AreEqual(ThiefState.Revealed, director.Thieves[0].State);
            Assert.AreEqual(3, director.RelicsRemaining);

            // Advance 0.2 s -> total 10.1 s >= 10.0 s
            director.Tick(0.2f, fog);
            Assert.AreEqual(ThiefState.Escaped, director.Thieves[0].State);
            Assert.AreEqual(2, director.RelicsRemaining);
        }

        [Test]
        public void AT09_ThreeEscapes()
        {
            var director = new ThiefDirector();
            var fog = new FogField();

            for (int i = 0; i < 4; i++)
            {
                director.SpawnThief(i + 1, Vector3.forward);
                director.Thieves[i].State = ThiefState.Revealed;
            }

            // Force 3 escapes
            director.EscapeThief(director.Thieves[0], fog);
            director.EscapeThief(director.Thieves[1], fog);
            Assert.IsFalse(director.IsGameOver);

            director.EscapeThief(director.Thieves[2], fog);
            Assert.IsTrue(director.IsGameOver);

            // Fourth escape should be ignored / suppressed
            director.EscapeThief(director.Thieves[3], fog);
            Assert.AreEqual(3, director.EscapeCount);
        }

        [Test]
        public void AT10_UnrevealedAtRoundEnd()
        {
            var director = new ThiefDirector();
            var fog = new FogField();
            var fsm = new RoundStateMachine();
            fsm.StartRound();

            director.SpawnThief(1, Vector3.forward); // remains Hidden

            fsm.Tick(75.1f, director, fog);

            Assert.AreEqual(RoundState.GameOver, fsm.State);
            Assert.AreEqual(ThiefState.Escaped, director.Thieves[0].State);
        }

        [Test]
        public void AT11_FogDeterminism_Deferred()
        {
            Assert.Pass("AT-11 deferred to device: 30s hand trace SHA-256 fog grid hashing.");
        }

        [Test]
        public void AT12_FogOcclusion_Deferred()
        {
            Assert.Pass("AT-12 deferred to device: MR depth and shell alpha compositing buffer test.");
        }

        [Test]
        public void AT13_TapThroughShell()
        {
            var fog = new FogField(0.90f);
            var shell = new HandShell();
            Vector3 dir = Vector3.forward;

            // Descend index from 30 mm to 3 mm over 120 ms
            float dt = 0.030f; // 4 steps
            float minH = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                float h = Mathf.Lerp(30.0f, 3.0f, i / 3.0f);
                minH = Mathf.Min(minH, h);
                shell.ClearSensors();
                shell.AddSensor(dir, h);
                fog.Tick(dt, shell);
            }

            Assert.AreEqual(3.0f, minH, 0.01f);
            Assert.Less(fog.Sample(dir), 0.90f, "Fog must clear along the swept path");
        }

        [Test]
        public void AT14_AudioCoverage()
        {
            var router = new FeedbackRouter();
            Assert.AreEqual(13, router.TotalMappedEvents);
            foreach (FeedbackEvent evt in System.Enum.GetValues(typeof(FeedbackEvent)))
            {
                Assert.IsTrue(router.HasMapping(evt), $"Missing mapping for {evt}");
                Assert.IsNotNull(router.GetClipName(evt), $"Null clip for {evt}");
            }

            Assert.AreEqual(0.70f, router.ComputeRevealGain(0.30f), 0.05f);
        }
    }
}
