using UnityEngine;

namespace XRGames.Chapter4.PulseCoreReactor
{
    public class PulseCoreReactorRig : MonoBehaviour
    {
        [Header("Physical Transforms")]
        [SerializeField] private Transform coreTransform;

        [Header("Core Systems")]
        public readonly PropTracker PropTracker = new PropTracker();
        public readonly GripSolver GripSolver = new GripSolver();
        public readonly DeformationModel DeformationModel = new DeformationModel();
        public readonly ShieldController ShieldController = new ShieldController();
        public readonly HeatSystem HeatSystem = new HeatSystem();
        public readonly RhythmClassifier RhythmClassifier = new RhythmClassifier();
        public readonly ThreatDirector ThreatDirector = new ThreatDirector();
        public readonly ThreatResolver ThreatResolver = new ThreatResolver();
        public readonly ScoreSystem ScoreSystem = new ScoreSystem();
        public readonly CoreStateMachine CoreStateMachine = new CoreStateMachine();
        public readonly FeedbackRouter FeedbackRouter = new FeedbackRouter();

        private void Update()
        {
            float dt = Time.deltaTime;
            HeatSystem.Step(GripSolver.Pressure, dt);
            ShieldController.UpdateShield(GripSolver.Pressure, dt);
        }
    }
}
