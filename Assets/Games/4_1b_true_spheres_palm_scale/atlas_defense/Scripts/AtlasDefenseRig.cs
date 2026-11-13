using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense
{
    public class AtlasDefenseRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform globeTransform;

        [Header("Core Systems")]
        public readonly PropTracker PropTracker = new PropTracker();
        public readonly ContactBus ContactBus = new ContactBus();
        public readonly RollKinematics RollKinematics = new RollKinematics();
        public readonly GlobeController GlobeController = new GlobeController();
        public readonly MeteorDirector MeteorDirector = new MeteorDirector();
        public readonly ShieldSystem ShieldSystem = new ShieldSystem();
        public readonly CityGrid CityGrid = new CityGrid();
        public readonly ScoreCombo ScoreCombo = new ScoreCombo();
        public readonly WaveStateMachine WaveStateMachine = new WaveStateMachine();
        public readonly FeedbackRouter FeedbackRouter = new FeedbackRouter();
        public readonly Compositor Compositor = new Compositor();

        private void Update()
        {
            float dt = Time.deltaTime;

            if (globeTransform != null)
            {
                PropTracker.SetPose(globeTransform.position, globeTransform.rotation);
                GlobeController.SetRotation(globeTransform.rotation);
            }
        }
    }
}
