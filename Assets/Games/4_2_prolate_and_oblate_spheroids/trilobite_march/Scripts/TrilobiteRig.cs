using UnityEngine;

namespace XRGames.Chapter4.TrilobiteMarch
{
    public class TrilobiteRig : MonoBehaviour
    {
        public Transform trilobiteVisual;

        public PropMouse Mouse;
        public GaitModel Gait;
        public CurlModel Curl;
        public MigrationDirector Director;

        private void Awake()
        {
            Mouse = new PropMouse();
            Gait = new GaitModel();
            Curl = new CurlModel();
            Director = new MigrationDirector();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Gait.Step(dt);
            Director.CheckAdvance(Gait.DistanceMm);

            if (trilobiteVisual != null)
            {
                trilobiteVisual.localPosition = new Vector3(0f, -Gait.SinkLevel * 0.005f, Gait.DistanceMm * 0.001f);
            }
        }
    }
}
