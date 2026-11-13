using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public class MysticSeerRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform orbTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        public PropTracker PropTracker { get; private set; }
        public HandShell HandShell { get; private set; }
        public FogField FogField { get; private set; }
        public ThiefDirector ThiefDirector { get; private set; }
        public TapJudge TapJudge { get; private set; }
        public SeanceScoring Scoring { get; private set; }
        public RoundStateMachine FSM { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            PropTracker = new PropTracker();
            HandShell = new HandShell();
            FogField = new FogField();
            ThiefDirector = new ThiefDirector();
            TapJudge = new TapJudge();
            Scoring = new SeanceScoring();
            FSM = new RoundStateMachine();
            Feedback = new FeedbackRouter();

            FSM.StartRound();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            FogField.Tick(dt, HandShell);
            ThiefDirector.Tick(dt, FogField);
            FSM.Tick(dt, ThiefDirector, FogField);

            if (orbTransform != null)
            {
                PropTracker.CheckRestDisc(orbTransform.position);
                PropTracker.Rotation = orbTransform.rotation;
            }
        }
    }
}
