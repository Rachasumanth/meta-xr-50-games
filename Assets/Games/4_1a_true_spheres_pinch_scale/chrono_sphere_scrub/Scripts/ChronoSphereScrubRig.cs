using UnityEngine;

namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class ChronoSphereScrubRig : MonoBehaviour
    {
        [Header("Physical Transforms")]
        [SerializeField] private Transform marbleTransform;

        [Header("Core Systems")]
        public readonly PropMarble PropMarble = new PropMarble();
        public readonly PinchModel PinchModel = new PinchModel();
        public readonly TwistIntegrator TwistIntegrator = new TwistIntegrator();
        public readonly Timeline Timeline = new Timeline();
        public readonly DioramaModel DioramaModel = new DioramaModel();
        public readonly ResonanceJudge ResonanceJudge = new ResonanceJudge();
        public readonly ScenarioDirector ScenarioDirector = new ScenarioDirector();
        public readonly DioramaRig DioramaRig = new DioramaRig();
        public readonly ChronoDial ChronoDial = new ChronoDial();

        private void Update()
        {
            float dt = Time.deltaTime;
            Timeline.Step(dt, PinchModel.IsHoldValid);
            ChronoDial.DialAngleDeg = Timeline.T * 0.36f;
        }
    }
}
