using UnityEngine;

namespace XRGames.Chapter4.VortexWobbler
{
    public class VortexRig : MonoBehaviour
    {
        public Transform discVisual;

        public PropPebble Pebble;
        public RockModel Rock;
        public VortexField Field;
        public AirlockModel Airlock;
        public WaveDirector Director;

        private void Awake()
        {
            Pebble = new PropPebble();
            Rock = new RockModel();
            Field = new VortexField();
            Airlock = new AirlockModel();
            Director = new WaveDirector();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Director.Step(dt, Field, Rock, Airlock);

            if (discVisual != null)
            {
                discVisual.localPosition = Pebble.Position;
                discVisual.localRotation = Quaternion.Euler(Rock.AmplitudeDeg, Rock.PsiDeg, 0f);
            }
        }
    }
}
