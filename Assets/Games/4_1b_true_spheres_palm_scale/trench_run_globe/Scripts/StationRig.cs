using UnityEngine;

namespace XRGames.Chapter4.TrenchRunGlobe
{
    public class StationRig : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform ballTransform;
        [SerializeField] private Transform readHeadIndexTransform;
        [SerializeField] private Transform palmTransform;

        public PropTennis Prop { get; private set; }
        public ReadHeadModel ReadHead { get; private set; }
        public TrenchMap Trench { get; private set; }
        public TowerArray Towers { get; private set; }
        public BoltSystem Bolts { get; private set; }
        public PortJudge Port { get; private set; }
        public SectorDirector Director { get; private set; }
        public FeedbackRouter Feedback { get; private set; }

        private void Awake()
        {
            Prop = new PropTennis();
            ReadHead = new ReadHeadModel();
            Trench = new TrenchMap();
            Towers = new TowerArray();
            Bolts = new BoltSystem();
            Port = new PortJudge();
            Director = new SectorDirector();
            Feedback = new FeedbackRouter();

            Towers.InitializeTowers();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            ReadHead.UpdateLift(dt);
            Director.ProcessWallScrape(ReadHead.VCrossTrackMm, dt);

            if (ballTransform != null)
            {
                Prop.Position = ballTransform.position;
                Prop.Rotation = ballTransform.rotation;
            }
        }
    }
}
