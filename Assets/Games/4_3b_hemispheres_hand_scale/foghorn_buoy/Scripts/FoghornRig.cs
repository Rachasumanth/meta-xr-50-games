using UnityEngine;

namespace XRGames.Chapter4.FoghornBuoy
{
    public class FoghornRig : MonoBehaviour
    {
        [Header("Model References")]
        [SerializeField] private Transform buoyTransform;
        [SerializeField] private Transform beaconLantern;

        [Header("State")]
        public FoghornSimulation Simulation { get; private set; }
        public float HandPressTorqueNm = 0f;
        public bool PalmCoverPause = false;

        private void Awake()
        {
            Simulation = new FoghornSimulation();
        }

        private void Update()
        {
            if (Simulation == null) return;
            Simulation.Step(Time.deltaTime, HandPressTorqueNm, PalmCoverPause);

            if (buoyTransform != null)
            {
                // Visual 3x tilt exaggeration per spec section 7
                float visualTiltDeg = Simulation.Pendulum.ThetaDeg * 3.0f;
                buoyTransform.localRotation = Quaternion.Euler(visualTiltDeg, 0, 0);
            }
        }
    }
}
