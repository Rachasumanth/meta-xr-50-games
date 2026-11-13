using System;
using UnityEngine;

namespace XRGames.Chapter4.PhantomShellGame
{
    public class ShellRig : MonoBehaviour
    {
        [Header("Models")]
        public Transform ShellTransform;
        public Transform HologramLeft;
        public Transform HologramRight;
        public Transform FairyTransform;

        [Header("State")]
        public SlideModel Slide = new SlideModel();
        public LiftModel Lift = new LiftModel();
        public HologramModel HoloLeftModel = new HologramModel();
        public HologramModel HoloRightModel = new HologramModel();

        public int Trust = 3;
        public int Score = 0;
        public bool IsPaused = false;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime;
            Vector2 targetLeft = Slide.PositionMm + new Vector2(-55.0f, 0f);
            Vector2 targetRight = Slide.PositionMm + new Vector2(55.0f, 0f);

            HoloLeftModel.Step(targetLeft, dt);
            HoloRightModel.Step(targetRight, dt);

            if (ShellTransform != null)
            {
                ShellTransform.localPosition = new Vector3(Slide.PositionMm.x * 0.001f, Lift.LiftMm * 0.001f, Slide.PositionMm.y * 0.001f);
            }
        }
    }
}
