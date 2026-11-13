using UnityEngine;

namespace XRGames.C4_1A.SocketTrackball
{
    public class SocketTrackballRig : MonoBehaviour
    {
        [SerializeField] private Transform marbleTransform;
        [SerializeField] private Transform shipTransform;

        private PropTracker _propTracker;
        private ShipController _shipController;

        private void Awake()
        {
            _propTracker = new PropTracker();
            _shipController = new ShipController();
        }

        private void Update()
        {
            if (marbleTransform != null)
            {
                marbleTransform.position = _propTracker.Center;
                marbleTransform.rotation = _propTracker.Orientation;
            }

            if (shipTransform != null)
            {
                shipTransform.localRotation = Quaternion.Euler(_shipController.Pitch, 0f, _shipController.Bank);
            }
        }
    }
}
