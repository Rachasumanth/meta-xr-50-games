using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest
{
    public class KatamariHarvestRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform ballTransform;
        [SerializeField] private Transform trayTransform;
        [SerializeField] private Transform palmTransform;

        public PropTracker PropTracker { get; private set; }
        public PalmContact PalmContact { get; private set; }
        public TwoSurfaceKinematics Kinematics { get; private set; }
        public StrokeAnalyzer StrokeAnalyzer { get; private set; }
        public StickySphere Sphere { get; private set; }
        public MeadowDirector Meadow { get; private set; }
        public HarvestResolver Resolver { get; private set; }
        public SeasonStateMachine FSM { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            PropTracker = new PropTracker();
            PalmContact = new PalmContact();
            Kinematics = new TwoSurfaceKinematics();
            StrokeAnalyzer = new StrokeAnalyzer();
            Sphere = new StickySphere();
            Meadow = new MeadowDirector();
            Resolver = new HarvestResolver();
            FSM = new SeasonStateMachine();
            Feedback = new FeedbackRouter();

            Meadow.Initialize();
            FSM.StartSeason();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Resolver.Update(dt);
            FSM.Tick(dt, Sphere.Mass);

            if (ballTransform != null)
            {
                PropTracker.Position = ballTransform.position;
                PropTracker.Rotation = ballTransform.rotation;
                Vector2 spherePosMm = new Vector2(ballTransform.position.x * 1000f, ballTransform.position.z * 1000f);
                Meadow.Update(dt, spherePosMm);
            }
        }
    }
}
