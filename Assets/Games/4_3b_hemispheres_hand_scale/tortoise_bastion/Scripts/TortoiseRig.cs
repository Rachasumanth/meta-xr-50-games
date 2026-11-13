using UnityEngine;

namespace XRGames.Chapter4.TortoiseBastion
{
    public class TortoiseRig : MonoBehaviour
    {
        [Header("Model References")]
        [SerializeField] private Transform shellTransform;
        [SerializeField] private Transform cannonMount;

        [Header("State")]
        public TortoiseSimulation Simulation { get; private set; }
        public float HandYawDeg = 0f;

        private void Awake()
        {
            Simulation = new TortoiseSimulation();
        }

        private void Update()
        {
            if (Simulation == null) return;
            Simulation.Step(Time.deltaTime);
            Simulation.SetDomeYaw(HandYawDeg);

            if (shellTransform != null)
            {
                shellTransform.localRotation = Quaternion.Euler(0, HandYawDeg, 0);
            }
        }
    }
}
