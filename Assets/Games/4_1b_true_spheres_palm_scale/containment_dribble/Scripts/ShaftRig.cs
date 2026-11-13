using UnityEngine;

namespace XRGames.Chapter4.ContainmentDribble
{
    public class ShaftRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform foamBallTransform;
        [SerializeField] private Transform dragonTransform;
        [SerializeField] private Transform leftPalmTransform;
        [SerializeField] private Transform rightHandTransform;

        public PropFoam Prop { get; private set; }
        public TapClassifier TapJudge { get; private set; }
        public DragonBody Dragon { get; private set; }
        public FireflyPool Fireflies { get; private set; }
        public SpikeBank Spikes { get; private set; }
        public RunDirector Director { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            Prop = new PropFoam();
            TapJudge = new TapClassifier();
            Dragon = new DragonBody();
            Fireflies = new FireflyPool();
            Spikes = new SpikeBank();
            Director = new RunDirector();
            Feedback = new FeedbackRouter();

            Fireflies.InitializeFireflies();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Dragon.StepPhysics(dt);
            Director.Tick(dt);

            if (foamBallTransform != null)
            {
                Prop.Position = foamBallTransform.position;
                Prop.Rotation = foamBallTransform.rotation;
            }

            if (dragonTransform != null)
            {
                dragonTransform.localPosition = new Vector3(0f, Dragon.HeightMm * 0.001f, 0f);
            }
        }
    }
}
