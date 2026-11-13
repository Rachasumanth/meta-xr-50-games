using UnityEngine;

namespace XRGames.Chapter4.MedusaJellyfish
{
    public class JellyRig : MonoBehaviour
    {
        [Header("Model References")]
        [SerializeField] private Transform bellTransform;
        [SerializeField] private Transform[] tentacleJoints;

        [Header("State")]
        public JellyfishSimulation Simulation { get; private set; }
        public float LeftPalmForceN = 0f;
        public float RightPalmForceN = 0f;
        public float TiltAngleDeg = 0f;

        private void Awake()
        {
            Vector3 initPos = bellTransform != null ? bellTransform.position : transform.position;
            Simulation = new JellyfishSimulation(initPos);
        }

        private void Update()
        {
            if (Simulation == null) return;

            float dt = Time.deltaTime;
            Simulation.Step(dt, TiltAngleDeg);

            if (bellTransform != null)
            {
                // Reflect depth visually
                Vector3 p = bellTransform.localPosition;
                p.y = -Simulation.Jet.DepthMm * 0.001f;
                bellTransform.localPosition = p;

                // Reflect tilt visually
                bellTransform.localRotation = Quaternion.Euler(0, 0, -TiltAngleDeg);
            }

            // Sync tentacle visual joints if bound
            if (tentacleJoints != null && Simulation.Tentacles != null)
            {
                int count = Mathf.Min(tentacleJoints.Length, Simulation.Tentacles.Segments.Count);
                for (int i = 0; i < count; i++)
                {
                    if (tentacleJoints[i] != null)
                    {
                        tentacleJoints[i].localPosition = Simulation.Tentacles.Segments[i].Position * 0.001f;
                    }
                }
            }
        }

        public bool TriggerContraction()
        {
            if (Simulation == null) return false;
            return Simulation.TryExecuteJet(LeftPalmForceN, RightPalmForceN, out _);
        }

        public void SetPalmPress(float leftN, float rightN)
        {
            LeftPalmForceN = leftN;
            RightPalmForceN = rightN;
        }

        public void SetTilt(float tiltDeg)
        {
            TiltAngleDeg = tiltDeg;
        }
    }
}
