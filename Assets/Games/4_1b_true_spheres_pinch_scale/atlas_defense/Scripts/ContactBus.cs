namespace XRGames.Chapter4.AtlasDefense
{
    public class ContactBus
    {
        public bool HasContact { get; private set; }

        public void UpdateContact(float distanceMm, float dt)
        {
            if (!HasContact)
            {
                if (distanceMm <= 5.0f) HasContact = true;
            }
            else
            {
                if (distanceMm >= 8.0f) HasContact = false;
            }
        }
    }
}
