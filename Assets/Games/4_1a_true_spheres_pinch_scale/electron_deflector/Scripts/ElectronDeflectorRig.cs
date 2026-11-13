using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector
{
    public class ElectronDeflectorRig : MonoBehaviour
    {
        [Header("Physical Transforms")]
        [SerializeField] private Transform ballTransform;

        [Header("Core Systems")]
        public readonly PropBall PropBall = new PropBall();
        public readonly PoleClamp PoleClamp = new PoleClamp();
        public readonly SectorMapper SectorMapper = new SectorMapper();
        public readonly BurstSystem BurstSystem = new BurstSystem();
        public readonly NeutronPool NeutronPool = new NeutronPool();
        public readonly RingSet RingSet = new RingSet();
        public readonly ContainmentDirector ContainmentDirector = new ContainmentDirector();
        public readonly NucleusRig NucleusRig = new NucleusRig();
        public readonly StreamRig StreamRig = new StreamRig();

        private void Update()
        {
            float dt = Time.deltaTime;
            ContainmentDirector.Step(dt);

            if (ballTransform != null)
            {
                PropBall.Register(ballTransform.position, Vector3.up);
            }
        }
    }
}
