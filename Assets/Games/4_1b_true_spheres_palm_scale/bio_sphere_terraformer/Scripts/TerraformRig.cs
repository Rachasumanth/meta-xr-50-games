using UnityEngine;

namespace XRGames.Chapter4.BioSphereTerraformer
{
    public class TerraformRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform appleTransform;
        [SerializeField] private Transform stemTransform;
        [SerializeField] private Transform tapFingerTransform;

        public PropApple Prop { get; private set; }
        public StemTwist Stem { get; private set; }
        public SeedBank Seeds { get; private set; }
        public SeasonDirector Director { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            Prop = new PropApple();
            Stem = new StemTwist();
            Seeds = new SeedBank();
            Director = new SeasonDirector();
            Feedback = new FeedbackRouter();

            Director.InitializeCells();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Director.Tick(dt, Stem.AxisTiltDeg, Stem.SpinOmegaDegS);

            if (appleTransform != null)
            {
                Prop.Position = appleTransform.position;
                Prop.Rotation = appleTransform.rotation;
            }
        }
    }
}
