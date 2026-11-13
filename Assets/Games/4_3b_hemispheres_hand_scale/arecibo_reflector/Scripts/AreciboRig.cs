using UnityEngine;

namespace XRGames.Chapter4.AreciboReflector
{
    public class AreciboRig : MonoBehaviour
    {
        [Header("Model References")]
        [SerializeField] private Transform dishTransform;
        [SerializeField] private Transform receiverTransform;
        [SerializeField] private Transform roverATransform;
        [SerializeField] private Transform roverBTransform;
        [SerializeField] private Transform roverCTransform;

        [Header("State")]
        public AreciboSimulation Simulation { get; private set; }

        private void Awake()
        {
            Simulation = new AreciboSimulation();
        }

        private void Update()
        {
            if (Simulation == null) return;
            Simulation.Step(Time.deltaTime);

            if (roverATransform != null)
                roverATransform.localPosition = Simulation.RoverA.GetPosition() * 0.001f;
            if (roverBTransform != null)
                roverBTransform.localPosition = Simulation.RoverB.GetPosition() * 0.001f;
            if (roverCTransform != null)
                roverCTransform.localPosition = Simulation.RoverC.GetPosition() * 0.001f;
        }

        public bool PlaceRover(bool isRoverA, int gridI, int gridJ)
        {
            if (Simulation == null) return false;
            return Simulation.TryPlaceRover(isRoverA, new GridCell(gridI, gridJ));
        }
    }
}
