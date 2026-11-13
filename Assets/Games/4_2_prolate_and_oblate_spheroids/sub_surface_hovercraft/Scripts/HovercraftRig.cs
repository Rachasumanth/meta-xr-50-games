using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public class HovercraftRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform craftVisual;
        public Transform patchCenter;

        [Header("State")]
        public PlanarDrive Drive;
        public DepthSonar Sonar;
        public HarpoonSystem Harpoons;
        public HazardDirector Hazards;
        public ChannelStateMachine StateMachine;
        public ScoreCombo Combo;
        public FeedbackRouter Feedback;
        public PropTracker Tracker;

        private void Awake()
        {
            Drive = new PlanarDrive();
            Sonar = new DepthSonar();
            Harpoons = new HarpoonSystem();
            Hazards = new HazardDirector();
            StateMachine = new ChannelStateMachine();
            Combo = new ScoreCombo();
            Feedback = new FeedbackRouter();
            Tracker = new PropTracker();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            StateMachine.Step(dt, Drive.VirtualPosition.x, Hazards.HullIntegrity);
            Drive.Step(Vector3.zero, dt);
            Sonar.Step(dt);
            Harpoons.Step(dt);
            Hazards.Step(dt, Drive.VirtualPosition, Sonar.CurrentBand);
            Combo.Step(dt);

            if (craftVisual != null)
            {
                craftVisual.localPosition = Drive.VirtualPosition;
                craftVisual.localRotation = Quaternion.LookRotation(Drive.Heading, Vector3.up);
            }
        }
    }
}
