using UnityEngine;

namespace XRGames.Chapter4.ScarabCanyon
{
    public class ScarabCanyonRig : MonoBehaviour
    {
        [Header("Physical Transforms")]
        [SerializeField] private Transform marbleTransform;
        [SerializeField] private Transform palmTransform;

        [Header("Core Systems")]
        public readonly PalmSurface PalmSurface = new PalmSurface();
        public readonly MarbleTracker MarbleTracker = new MarbleTracker();
        public readonly TiltModel TiltModel = new TiltModel();
        public readonly GroovePhysics GroovePhysics = new GroovePhysics();
        public readonly CanyonDirector CanyonDirector = new CanyonDirector();
        public readonly OrbJudge OrbJudge = new OrbJudge();
        public readonly ScarabRig ScarabRig = new ScarabRig();
        public readonly CanyonRig CanyonRig = new CanyonRig();
        public readonly RunDirector RunDirector = new RunDirector();

        private void Update()
        {
            float dt = Time.deltaTime;

            if (palmTransform != null)
            {
                float palmRoll = palmTransform.eulerAngles.z;
                float filtered = TiltModel.ProcessRoll(palmRoll);
                GroovePhysics.Step(filtered, dt);
                CanyonDirector.CheckPalmHeight(palmTransform.position.y * 1000f, dt);
            }

            CanyonDirector.UpdateGlowSpeed(RunDirector.Glow);
        }
    }
}
