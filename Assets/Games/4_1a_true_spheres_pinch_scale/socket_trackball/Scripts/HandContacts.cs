using UnityEngine;

namespace XRGames.C4_1A.SocketTrackball
{
    public struct ContactRecord
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector3 velocity;
        public float age;
        public bool isValid;
    }

    public class HandContacts
    {
        private ContactRecord _thumbTip;
        private ContactRecord _indexTip;
        private ContactRecord _middleTip;

        public ContactRecord ThumbTip => _thumbTip;
        public ContactRecord IndexTip => _indexTip;
        public ContactRecord MiddleTip => _middleTip;

        public void UpdateThumb(Vector3 pos, Vector3 norm, Vector3 vel, float dt)
        {
            _thumbTip.position = pos;
            _thumbTip.normal = norm.normalized;
            _thumbTip.velocity = vel;
            _thumbTip.age += dt;
            _thumbTip.isValid = true;
        }

        public void UpdateIndex(Vector3 pos, Vector3 norm, Vector3 vel, float dt)
        {
            _indexTip.position = pos;
            _indexTip.normal = norm.normalized;
            _indexTip.velocity = vel;
            _indexTip.age += dt;
            _indexTip.isValid = true;
        }

        public void UpdateMiddle(Vector3 pos, Vector3 norm, Vector3 vel, float dt)
        {
            _middleTip.position = pos;
            _middleTip.normal = norm.normalized;
            _middleTip.velocity = vel;
            _middleTip.age += dt;
            _middleTip.isValid = true;
        }

        public void ClearInvalid(float maxAge = 0.1f)
        {
            if (_thumbTip.age > maxAge) _thumbTip.isValid = false;
            if (_indexTip.age > maxAge) _indexTip.isValid = false;
            if (_middleTip.age > maxAge) _middleTip.isValid = false;
        }
    }
}
