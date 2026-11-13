using UnityEngine;

namespace XRGames.Chapter4.CagePinball
{
    public class CagePinballRig : MonoBehaviour
    {
        [Header("Physical Elements")]
        [SerializeField] private Transform ballTransform;
        [SerializeField] private Transform trayTransform;

        [Header("Core Systems")]
        public readonly PropTracker PropTracker = new PropTracker();
        public readonly TrayFrame TrayFrame = new TrayFrame();
        public readonly CageFitter CageFitter = new CageFitter();
        public readonly FingertipMap FingertipMap = new FingertipMap();
        public readonly BumperSystem BumperSystem = new BumperSystem();
        public readonly PinballSystem PinballSystem = new PinballSystem();
        public readonly CrystalRing CrystalRing = new CrystalRing();
        public readonly BallPhysics BallPhysics = new BallPhysics();
        public readonly ScoreService ScoreService = new ScoreService();
        public readonly FeedbackDirector FeedbackDirector = new FeedbackDirector();
        public readonly SessionFSM SessionFSM = new SessionFSM();

        private void Update()
        {
            float dt = Time.deltaTime;
            TrayFrame.Step(dt);

            if (ballTransform != null)
            {
                PropTracker.SetPose(ballTransform.position, Vector3.zero);
            }
        }
    }
}
