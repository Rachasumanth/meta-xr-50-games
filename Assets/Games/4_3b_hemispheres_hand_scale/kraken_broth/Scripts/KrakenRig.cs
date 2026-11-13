using UnityEngine;

namespace XRGames.Chapter4.KrakenBroth
{
    public class KrakenRig : MonoBehaviour
    {
        [Header("Model References")]
        [SerializeField] private Transform bowlTransform;
        [SerializeField] private Transform tentacleTransform;

        [Header("State")]
        public KrakenSimulation Simulation { get; private set; }
        public float BowlLiftMm = 0f;

        private void Awake()
        {
            Simulation = new KrakenSimulation();
        }

        private void Update()
        {
            if (Simulation == null) return;
            Simulation.Step(Time.deltaTime, BowlLiftMm);

            if (tentacleTransform != null)
            {
                // If any quadrant is armed, raise tentacle
                bool anyArmed = false;
                foreach (var q in Simulation.Quadrants.Values)
                {
                    if (q.Armed) { anyArmed = true; break; }
                }
                tentacleTransform.gameObject.SetActive(anyArmed);
            }
        }

        public bool TriggerDrumStrike(QuadrantId qId, float vn, int finger)
        {
            if (Simulation == null) return false;
            return Simulation.RecordStrike(qId, vn, finger, Time.time, out _);
        }
    }
}
