using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public class StoneRig : MonoBehaviour
    {
        public Transform stoneVisual;

        public PropTracker Tracker;
        public AbrasionField Field;
        public ThumbKinematics Thumb;
        public FragmentField Fragments;
        public TideController Tide;
        public TideStateMachine StateMachine;
        public ScoreCombo Combo;
        public FeedbackRouter Feedback;

        private void Awake()
        {
            Tracker = new PropTracker();
            Field = new AbrasionField();
            Thumb = new ThumbKinematics();
            Fragments = new FragmentField();
            Tide = new TideController();
            StateMachine = new TideStateMachine();
            Combo = new ScoreCombo();
            Feedback = new FeedbackRouter();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Thumb.Step(dt, 0f);
            Tide.Step(dt, Fragments);
            Fragments.CheckReveals(Field);
            StateMachine.Step(dt, Tide.IsCompleted);
            Combo.Step(dt);

            if (stoneVisual != null)
            {
                stoneVisual.localPosition = Tracker.Position;
                stoneVisual.localRotation = Tracker.Rotation;
            }
        }
    }
}
