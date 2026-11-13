using UnityEngine;

namespace XRGames.Chapter4.OcularTurret
{
    public class OcularTurretRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform marbleTransform;
        [SerializeField] private Transform gazeBeamTransform;

        [Header("Core Systems")]
        public readonly PropMarble PropMarble = new PropMarble();
        public readonly RollIntegrator RollIntegrator = new RollIntegrator();
        public readonly GazeModel GazeModel = new GazeModel();
        public readonly SocketModel SocketModel = new SocketModel();
        public readonly GargoylePool GargoylePool = new GargoylePool();
        public readonly BeamJudge BeamJudge = new BeamJudge();
        public readonly VigilDirector VigilDirector = new VigilDirector();
        public readonly OcularFeedback OcularFeedback = new OcularFeedback();

        private void Update()
        {
            float dt = Time.deltaTime;
            GazeModel.Step(dt);
            GargoylePool.StepCooldown(dt);

            if (marbleTransform != null)
            {
                PropMarble.SetPose(marbleTransform.position, RollIntegrator.Orientation);
            }

            if (gazeBeamTransform != null)
            {
                gazeBeamTransform.forward = GazeModel.GazeDirection;
            }
        }
    }
}
