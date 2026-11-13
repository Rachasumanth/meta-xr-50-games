using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class BucklerRig : MonoBehaviour
    {
        public Transform bucklerVisual;

        public PropTracker Tracker;
        public HeatCharge Heat;
        public BeamDirector Beams;
        public TargetField Targets;
        public WaveStateMachine StateMachine;
        public ScoreCombo Combo;
        public FeedbackRouter Feedback;

        private void Awake()
        {
            Tracker = new PropTracker();
            Heat = new HeatCharge();
            Beams = new BeamDirector();
            Targets = new TargetField();
            StateMachine = new WaveStateMachine();
            Combo = new ScoreCombo();
            Feedback = new FeedbackRouter();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Heat.Step(dt);
            StateMachine.Step(dt, Beams.PlayerHull, Targets.Targets.Count);
            Combo.Step(dt);

            if (bucklerVisual != null)
            {
                bucklerVisual.localPosition = Tracker.Position;
                bucklerVisual.localRotation = Tracker.Rotation;
            }
        }
    }
}
