using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public class EquatorialParticleRingRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform ballTransform;
        [SerializeField] private Transform dominantHandTransform;
        [SerializeField] private Transform nonDominantHandTransform;

        public PropTracker PropTracker { get; private set; }
        public CradleMonitor Cradle { get; private set; }
        public EquatorTracer Tracer { get; private set; }
        public RingCharge Charge { get; private set; }
        public LaunchSolver Launch { get; private set; }
        public TargetDirector Targets { get; private set; }
        public RunScoring Scoring { get; private set; }
        public RunStateMachine FSM { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            PropTracker = new PropTracker();
            Cradle = new CradleMonitor();
            Tracer = new EquatorTracer();
            Charge = new RingCharge();
            Launch = new LaunchSolver();
            Targets = new TargetDirector();
            Scoring = new RunScoring();
            FSM = new RunStateMachine();
            Feedback = new FeedbackRouter();

            Targets.InitializeTargets();
            FSM.StartRun();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Targets.UpdateDrift(dt);
            Charge.Update(dt, Tracer, Cradle.State);
            FSM.Tick(dt, Charge, Scoring);

            if (ballTransform != null)
            {
                PropTracker.Position = ballTransform.position;
                PropTracker.Rotation = ballTransform.rotation;
            }
        }
    }
}
