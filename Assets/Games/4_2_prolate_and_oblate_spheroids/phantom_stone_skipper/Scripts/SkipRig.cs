using UnityEngine;

namespace XRGames.Chapter4.PhantomStoneSkipper
{
    public class SkipRig : MonoBehaviour
    {
        public Transform stoneVisual;
        public Transform phantomVisual;

        public PropStone Stone;
        public SnapDetector Snap;
        public TapJudge Judge;
        public PhantomStone Phantom;

        private void Awake()
        {
            Stone = new PropStone();
            Snap = new SnapDetector();
            Judge = new TapJudge();
            Phantom = new PhantomStone();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Phantom.Step(dt, 0.92f);

            if (stoneVisual != null)
            {
                stoneVisual.localPosition = Stone.Position;
                stoneVisual.localRotation = Stone.Rotation;
            }
        }
    }
}
