using UnityEngine;

namespace XRGames.Chapter4.BucklerDeflector
{
    public class BucklerRig : MonoBehaviour
    {
        [Header("Model References")]
        [SerializeField] private Transform bucklerTransform;
        [SerializeField] private Transform beamTransform;

        [Header("State")]
        public BucklerSimulation Simulation { get; private set; }

        private void Awake()
        {
            Simulation = new BucklerSimulation();
        }

        private void Update()
        {
            if (Simulation == null) return;
            Simulation.Step(Time.deltaTime);

            if (bucklerTransform != null)
            {
                bucklerTransform.forward = Simulation.CavityNormal;
            }

            if (beamTransform != null)
            {
                beamTransform.gameObject.SetActive(Simulation.BeamFired);
                Simulation.BeamFired = false;
            }
        }
    }
}
