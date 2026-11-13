using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class MicroPlanetScannerRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform marbleTransform;
        [SerializeField] private Transform moonHologramTransform;
        [SerializeField] private Transform rightIndexTipTransform;

        [Header("Core Systems")]
        public readonly PropTracker PropTracker = new PropTracker();
        public readonly TripodGate TripodGate = new TripodGate();
        public readonly RollFusion RollFusion = new RollFusion();
        public readonly ScannerProjector ScannerProjector = new ScannerProjector();
        public readonly LockAccumulator LockAccumulator = new LockAccumulator();
        public readonly WormField WormField = new WormField();
        public readonly MoonState MoonState = new MoonState();
        public readonly HologramRenderer HologramRenderer = new HologramRenderer();
        public readonly WaveDirector WaveDirector = new WaveDirector();
        public readonly FeedbackDirector FeedbackDirector = new FeedbackDirector();
        public readonly SessionFSM SessionFSM = new SessionFSM();

        private void Update()
        {
            float dt = Time.deltaTime;
            SessionFSM.Step(dt);

            if (marbleTransform != null)
            {
                PropTracker.SetPose(marbleTransform.position, marbleTransform.rotation);
            }

            if (moonHologramTransform != null && marbleTransform != null)
            {
                moonHologramTransform.position = HologramRenderer.GetCoreVirtualPosition(marbleTransform.position);
                moonHologramTransform.rotation = RollFusion.IntegratedRotation;
                moonHologramTransform.localScale = Vector3.one * HologramRenderer.ScaleFactor;
            }

            if (rightIndexTipTransform != null && marbleTransform != null)
            {
                Vector3 toTip = rightIndexTipTransform.position - marbleTransform.position;
                ScannerProjector.ProjectTip(toTip);
            }
        }
    }
}
