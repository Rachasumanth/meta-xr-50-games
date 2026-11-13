using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting
{
    public class VolcanoRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform orangeTransform;
        [SerializeField] private Transform leftPalmTransform;
        [SerializeField] private Transform rightPalmTransform;

        public PropOrange Prop { get; private set; }
        public CradleModel Cradle { get; private set; }
        public ContactSampler Sampler { get; private set; }
        public VentField Vents { get; private set; }
        public PressArbiter Arbiter { get; private set; }
        public PressureIntegrator Pressure { get; private set; }
        public CrustModel Crust { get; private set; }
        public PhaseDirector Director { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            Prop = new PropOrange();
            Cradle = new CradleModel();
            Sampler = new ContactSampler();
            Vents = new VentField();
            Arbiter = new PressArbiter();
            Pressure = new PressureIntegrator();
            Crust = new CrustModel();
            Director = new PhaseDirector();
            Feedback = new FeedbackRouter();

            Vents.Initialize(8);
            Director.StartPhase(1);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Director.Tick(dt, Vents, Pressure, Crust, Prop.IsCovered);

            if (orangeTransform != null)
            {
                Prop.Position = orangeTransform.position;
                Prop.Rotation = orangeTransform.rotation;
            }
        }
    }
}
