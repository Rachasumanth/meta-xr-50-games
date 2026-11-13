using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting
{
    public class CradleModel
    {
        public bool LeftPalmContact { get; set; } = true;
        public bool RightPalmContact { get; set; } = true;
        public bool IsGripValid => LeftPalmContact && RightPalmContact;
    }

    public class EffectorContact
    {
        public int Id;
        public Vector3 Position;
        public float SignedDistanceMm;
        public float FilteredPenetrationMm;
        public bool InContact;
        public int HoldTargetVentId = -1; // -1 none, -2 bare crust
    }

    public class ContactSampler
    {
        public EffectorContact[] Effectors { get; } = new EffectorContact[4];

        public ContactSampler()
        {
            for (int i = 0; i < Effectors.Length; i++)
            {
                Effectors[i] = new EffectorContact { Id = i };
            }
        }

        public void Sample(int index, float signedDistMm, float dt)
        {
            var e = Effectors[index];
            e.SignedDistanceMm = signedDistMm;
            e.InContact = e.InContact ? (signedDistMm < 8.0f) : (signedDistMm < 5.0f);
            if (!e.InContact)
            {
                e.FilteredPenetrationMm = 0f;
                e.HoldTargetVentId = -1;
                return;
            }

            float D = Mathf.Clamp(5.0f - signedDistMm, 0f, 12.0f);
            e.FilteredPenetrationMm += (1.0f - Mathf.Exp(-dt / 0.040f)) * (D - e.FilteredPenetrationMm);
        }
    }
}
