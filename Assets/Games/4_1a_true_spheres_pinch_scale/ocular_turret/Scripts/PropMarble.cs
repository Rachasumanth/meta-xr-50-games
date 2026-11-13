using UnityEngine;

namespace XRGames.Chapter4.OcularTurret
{
    public class PropMarble
    {
        public Vector3 Center { get; private set; } = Vector3.zero;
        public Quaternion PoseP { get; private set; } = Quaternion.identity;
        public bool IsRegistered { get; private set; }

        public bool Register(Vector3[] samples, float expectedRadiusMm = OcularTurretConfig.MarbleRadiusMm)
        {
            if (samples == null || samples.Length == 0)
            {
                Center = Vector3.zero;
                PoseP = Quaternion.identity;
                IsRegistered = true;
                return true;
            }

            Vector3 sum = Vector3.zero;
            foreach (var s in samples) sum += s;
            Center = sum / samples.Length;
            PoseP = Quaternion.identity;
            IsRegistered = true;
            return true;
        }

        public void SetPose(Vector3 pos, Quaternion rot)
        {
            Center = pos;
            PoseP = rot;
            IsRegistered = true;
        }
    }
}
