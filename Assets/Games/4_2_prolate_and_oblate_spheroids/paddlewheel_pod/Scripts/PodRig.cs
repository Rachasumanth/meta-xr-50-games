using UnityEngine;

namespace XRGames.Chapter4.PaddlewheelPod
{
    public class PodRig : MonoBehaviour
    {
        public Transform podVisual;

        public PropEgg Egg;
        public PinchModel Pinch;
        public SpinModel Spin;
        public SteerModel Steer;
        public PodModel Pod;

        private void Awake()
        {
            Egg = new PropEgg();
            Pinch = new PinchModel();
            Spin = new SpinModel();
            Steer = new SteerModel();
            Pod = new PodModel();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Spin.Step(dt);
            float vUp = Spin.ComputeAscentSpeed(Pod.Steam, Pod.IsInKelp);
            float drain = Spin.ComputeSteamDrain(dt);
            Pod.Step(vUp, drain, dt);

            if (podVisual != null)
            {
                podVisual.localPosition = new Vector3(Steer.LateralPosMm * 0.001f, -Pod.DepthMm * 0.001f, 0f);
            }
        }
    }
}
