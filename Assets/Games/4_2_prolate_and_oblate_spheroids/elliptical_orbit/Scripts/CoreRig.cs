using UnityEngine;

namespace XRGames.Chapter4.EllipticalOrbit
{
    public class CoreRig : MonoBehaviour
    {
        public Transform eggVisual;
        public Transform coreCenter;

        public PropEgg Egg;
        public RollModel Roll;
        public NudgeClassifier Nudge;
        public DustSystem Dust;
        public TailSystem Tail;
        public OrbitTracker Orbit;

        private void Awake()
        {
            Egg = new PropEgg();
            Roll = new RollModel();
            Nudge = new NudgeClassifier();
            Dust = new DustSystem();
            Tail = new TailSystem();
            Orbit = new OrbitTracker();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Roll.Step(dt);

            if (eggVisual != null)
            {
                eggVisual.localPosition = Egg.Position;
                eggVisual.localRotation = Quaternion.Euler(0f, Roll.AlphaRad * Mathf.Rad2Deg, 0f);
            }
        }
    }
}
