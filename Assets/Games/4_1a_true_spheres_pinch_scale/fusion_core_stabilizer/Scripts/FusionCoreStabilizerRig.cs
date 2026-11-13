using UnityEngine;

namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public class FusionCoreStabilizerRig : MonoBehaviour
    {
        [Header("Hand Transforms")]
        [SerializeField] private Transform leftPalmTransform;
        [SerializeField] private Transform rightPalmTransform;

        [Header("Core Systems")]
        public readonly PalmRig PalmRig = new PalmRig();
        public readonly ContainmentModel ContainmentModel = new ContainmentModel();
        public readonly PulseDirector PulseDirector = new PulseDirector();
        public readonly ReactorModel ReactorModel = new ReactorModel();
        public readonly StarRig StarRig = new StarRig();
        public readonly ArcHUD ArcHUD = new ArcHUD();
        public readonly BurnDirector BurnDirector = new BurnDirector();

        private void Update()
        {
            float dt = Time.deltaTime;

            if (leftPalmTransform != null && rightPalmTransform != null)
            {
                PalmRig.UpdatePalms(leftPalmTransform.position * 1000f, rightPalmTransform.position * 1000f, true);
                ContainmentModel.ComputePressure(PalmRig.SeparationMm, PalmRig.IsTwoHanded);
                ContainmentModel.StepSeparation(PalmRig.SeparationMm, dt);
            }

            ReactorModel.UpdateRamp(ContainmentModel.M, dt);
            StarRig.UpdateFromM(ContainmentModel.M);
        }
    }
}
