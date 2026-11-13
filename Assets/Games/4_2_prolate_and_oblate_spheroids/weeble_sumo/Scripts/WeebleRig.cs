using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class WeebleRig : MonoBehaviour
    {
        public Transform sumoVisual;
        public Transform ringTransform;

        public PropTracker Tracker;
        public PressModel Press;
        public RockingBody Rocking;
        public LungeSolver Lunge;
        public BellyCollider Belly;
        public ImpDirector Imps;
        public StabilityMeter Stability;
        public BoutStateMachine StateMachine;
        public ScoreCombo Combo;
        public FeedbackRouter Feedback;

        private void Awake()
        {
            Tracker = new PropTracker();
            Press = new PressModel();
            Rocking = new RockingBody();
            Lunge = new LungeSolver();
            Belly = new BellyCollider();
            Imps = new ImpDirector();
            Stability = new StabilityMeter();
            StateMachine = new BoutStateMachine();
            Combo = new ScoreCombo();
            Feedback = new FeedbackRouter();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Rocking.Step(dt);
            Lunge.Step(dt);
            Imps.Step(dt, Lunge.Position, Stability, Rocking);
            Stability.Step(dt, Rocking.ThetaDeg);
            StateMachine.Step(dt, Stability.IsBoutLost, Stability.IsGameOver, Imps.Imps.Count);
            Combo.Step(dt);

            if (sumoVisual != null)
            {
                sumoVisual.localPosition = Lunge.Position;
                sumoVisual.localRotation = Quaternion.Euler(Rocking.ThetaDeg, 0f, 0f);
            }
        }
    }
}
