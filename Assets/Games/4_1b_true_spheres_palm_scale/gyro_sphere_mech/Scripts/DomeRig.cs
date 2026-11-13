using UnityEngine;

namespace XRGames.Chapter4.GyroSphereMech
{
    public class DomeRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform orangeTransform;
        [SerializeField] private Transform mechVisualTransform;
        [SerializeField] private Transform leftPalmTransform;
        [SerializeField] private Transform rightPalmTransform;

        public PropOrange Prop { get; private set; }
        public GyroController Gyro { get; private set; }
        public MechBody Mech { get; private set; }
        public BugPool Bugs { get; private set; }
        public LaserSystem Laser { get; private set; }
        public WaveDirector Waves { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            Prop = new PropOrange();
            Gyro = new GyroController();
            Mech = new MechBody();
            Bugs = new BugPool();
            Laser = new LaserSystem();
            Waves = new WaveDirector();
            Feedback = new FeedbackRouter();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Laser.StepCooldown(dt);

            if (orangeTransform != null)
            {
                Prop.Position = orangeTransform.position;
                Prop.Rotation = orangeTransform.rotation;
            }
        }
    }
}
