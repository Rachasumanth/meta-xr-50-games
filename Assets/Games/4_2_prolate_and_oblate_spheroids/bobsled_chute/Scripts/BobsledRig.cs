using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public class BobsledRig : MonoBehaviour
    {
        public Transform sledVisual;
        public Transform chuteTrack;

        public PropTracker Tracker;
        public SlideControl Slide;
        public BankControl Bank;
        public ChuteModel Chute;
        public SledBody Sled;
        public SplitTimer Timer;
        public DescentStateMachine StateMachine;
        public ScoreCombo Combo;
        public FeedbackRouter Feedback;

        private void Awake()
        {
            Tracker = new PropTracker();
            Slide = new SlideControl();
            Bank = new BankControl();
            Chute = new ChuteModel();
            Sled = new SledBody();
            Timer = new SplitTimer();
            StateMachine = new DescentStateMachine();
            Combo = new ScoreCombo();
            Feedback = new FeedbackRouter();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float aCtrl = Slide.ComputeControlAcceleration(Slide.FilteredSlideMm);
            float kappa = Chute.GetCurvature(Sled.ArcDistanceM);
            Sled.Step(dt, aCtrl, Bank.BankDeg, kappa);
            StateMachine.Step(dt, Sled.ArcDistanceM >= BobsledChuteConfig.ChuteLengthM, Sled.CrashCount);

            if (sledVisual != null)
            {
                sledVisual.localPosition = new Vector3(Sled.LateralPosM, 0.012f, Sled.ArcDistanceM);
                sledVisual.localRotation = Quaternion.Euler(0f, 0f, -Bank.BankDeg);
            }
        }
    }
}
