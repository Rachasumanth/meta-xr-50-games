using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    // Thin MonoBehaviour wiring the pure-C# cores to scene transforms. All device
    // inputs are optional: with nothing assigned the rig stays idle and headless
    // EditMode tests cover the cores; with Meta XR sim hand data assigned at
    // runtime the full loop runs (palm frame -> cage -> sun -> barracuda -> tide).
    public class ClamPearlGuardRig : MonoBehaviour
    {
        [SerializeField] private Transform pearlTransform;
        [SerializeField] private Transform wristTransform;
        [SerializeField] private Transform indexMcpTransform;
        [SerializeField] private Transform middleMcpTransform;
        [SerializeField] private Transform pinkyMcpTransform;
        [SerializeField] private Transform[] fingertipTransforms = new Transform[5];
        [SerializeField] private float[] fistCalibration = new float[] { 35f, 35f, 35f, 35f, 35f };
        [SerializeField] private float strikeDurationMs = 800f;

        private readonly PalmFrame _palmFrame = new PalmFrame();
        private readonly PearlRegistration _pearlReg = new PearlRegistration();
        private readonly CurlEstimator _curlEstimator = new CurlEstimator();
        private readonly CageClassifier _cageClassifier = new CageClassifier();
        private readonly SunCharger _sunCharger = new SunCharger();
        private readonly BarracudaDirector _barracudaDirector = new BarracudaDirector();
        private readonly PearlState _pearlState = new PearlState();
        private readonly ClamRenderer _clamRenderer = new ClamRenderer();
        private readonly TideDirector _tideDirector = new TideDirector();
        private readonly FeedbackDirector _feedbackDirector = new FeedbackDirector();
        private readonly SessionFSM _sessionFSM = new SessionFSM();

        private float _strikeElapsedMs;
        private bool _strikeActive;

        public CageClassifier Cage => _cageClassifier;
        public SunCharger Sun => _sunCharger;
        public PearlState State => _pearlState;
        public SessionFSM Fsm => _sessionFSM;
        public ClamRenderer Renderer => _clamRenderer;

        private void Start()
        {
            _pearlState.Reset();
            _sessionFSM.ChangeState(SessionFSM.State.Playing);
            if (pearlTransform != null) _pearlReg.SetInitialPosition(pearlTransform.position);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (_sessionFSM.CurrentState == SessionFSM.State.Paused) return;

            // Palm frame from hand anchors (device input; idle when unassigned).
            if (wristTransform != null && indexMcpTransform != null &&
                middleMcpTransform != null && pinkyMcpTransform != null)
            {
                _palmFrame.UpdatePalm(wristTransform.position, indexMcpTransform.position,
                                      middleMcpTransform.position, pinkyMcpTransform.position,
                                      1f, dt);
            }

            // Cage classification from fingertip anchors (device input).
            Vector3 palmCenter = _palmFrame.PalmCenter;
            if (fingertipTransforms != null && fingertipTransforms.Length == 5 &&
                fingertipTransforms[0] != null)
            {
                Vector3[] tips = new Vector3[5];
                for (int i = 0; i < 5; i++) tips[i] = fingertipTransforms[i].position;
                _cageClassifier.Evaluate(tips, palmCenter, fistCalibration, Time.time, dt);
            }

            // Charge from palm openness + tilt (openness follows curl state; until
            // hand curl data streams in, the cage state stands in for openness).
            float openness = _cageClassifier.IsCageClosed ? 0f : _curlEstimator.Openness;
            float tiltDeg = _palmFrame.PalmRotation.eulerAngles.x > 180f
                ? _palmFrame.PalmRotation.eulerAngles.x - 360f
                : _palmFrame.PalmRotation.eulerAngles.x;
            _sunCharger.UpdateCharge(openness, tiltDeg, dt);

            // Pearl registration shift / drop watch.
            if (pearlTransform != null && !_pearlReg.IsDropped)
            {
                _pearlReg.CheckShiftAndDrop(pearlTransform.position, dt);
                if (_pearlReg.IsDropped)
                {
                    _sessionFSM.ChangeState(SessionFSM.State.Resume);
                    _feedbackDirector.PlayCue("pearl_drop");
                }
            }

            // Barracuda strike cycle: telegraph, then resolve at the landing time.
            if (!_strikeActive && _tideDirector.CurrentTide >= 1)
            {
                _barracudaDirector.StartStrike(strikeDurationMs);
                _strikeActive = true;
                _strikeElapsedMs = 0f;
            }
            if (_strikeActive)
            {
                _strikeElapsedMs += dt * 1000f;
                if (_strikeElapsedMs >= strikeDurationMs)
                {
                    int integrity = _pearlState.Integrity;
                    int score = _pearlState.Score;
                    var result = _barracudaDirector.ResolveStrike(_strikeElapsedMs, _cageClassifier.IsCageClosed,
                                                                  _cageClassifier.ClosureMs, ref integrity, ref score);
                    _pearlState.AddScore(score - _pearlState.Score);
                    if (integrity < _pearlState.Integrity)
                    {
                        _pearlState.TakeDamage(_pearlState.Integrity - integrity);
                        _feedbackDirector.SetDucking(0.4f);
                    }
                    if (result == BarracudaDirector.StrikeResult.GameOver)
                    {
                        _sessionFSM.ChangeState(SessionFSM.State.GameOver);
                    }
                    _strikeActive = false;
                }
            }

            // Visual shell mirrors openness; tide timer ticks.
            _clamRenderer.UpdateShell(openness, dt);
            _clamRenderer.SetOcclusion(_palmFrame.IsDropped);
            _tideDirector.UpdateTide(dt);
        }
    }
}
