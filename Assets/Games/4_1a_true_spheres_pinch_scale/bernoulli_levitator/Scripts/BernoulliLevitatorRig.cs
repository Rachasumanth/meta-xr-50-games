using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class BernoulliLevitatorRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform nozzleTransform;
        [SerializeField] private Transform orbTransform;

        private NozzleFrame _nozzleFrame = new NozzleFrame();
        private BallRegistration _ballRegistration = new BallRegistration();
        private PinchTracker _pinchTracker = new PinchTracker();
        private HeightController _heightController = new HeightController();
        private SpinFusion _spinFusion = new SpinFusion();
        private OrbBody _orbBody = new OrbBody();
        private RingField _ringField = new RingField();
        private GustField _gustField = new GustField();
        private PressureState _pressureState = new PressureState();
        private WaveDirector _waveDirector = new WaveDirector();
        private FeedbackDirector _feedbackDirector = new FeedbackDirector();
        private SessionFSM _sessionFSM = new SessionFSM();

        private float _sessionTimer;

        public NozzleFrame NozzleFrame => _nozzleFrame;
        public BallRegistration BallRegistration => _ballRegistration;
        public PinchTracker PinchTracker => _pinchTracker;
        public HeightController HeightController => _heightController;
        public SpinFusion SpinFusion => _spinFusion;
        public OrbBody OrbBody => _orbBody;
        public RingField RingField => _ringField;
        public GustField GustField => _gustField;
        public PressureState PressureState => _pressureState;
        public WaveDirector WaveDirector => _waveDirector;
        public FeedbackDirector FeedbackDirector => _feedbackDirector;
        public SessionFSM SessionFSM => _sessionFSM;

        private void Start()
        {
            _sessionFSM.StartSession();
        }

        private void Update()
        {
            if (_sessionFSM.State != SessionState.Playing) return;

            float dt = Time.deltaTime;
            _sessionTimer += dt;

            if (nozzleTransform != null)
            {
                _nozzleFrame.Update(nozzleTransform.position, nozzleTransform.rotation, dt, false);
            }

            // Sample ball registration if not registered
            if (!_ballRegistration.IsRegistered && nozzleTransform != null)
            {
                _ballRegistration.AddSample(nozzleTransform.position, nozzleTransform.up);
            }

            // Step gust and orb
            Vector3 gustAcc = _gustField.GetLateralAcceleration(_sessionTimer);
            _orbBody.Step(dt, Vector3.zero);

            if (orbTransform != null)
            {
                orbTransform.position = _orbBody.Position;
                orbTransform.rotation = _orbBody.Rotation;
            }
        }
    }
}
