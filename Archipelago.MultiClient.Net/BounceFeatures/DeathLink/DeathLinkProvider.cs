using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public static class DeathLinkProvider
    {
        public static DeathLinkService EnableDeathLink(this ArchipelagoSession session)
        {
            EnsureDeathLinkTagIsSet(session);

            return new DeathLinkService(session);
        }

        static void EnsureDeathLinkTagIsSet(ArchipelagoSession session)
        {
            if (!session.Tags.Contains("DeathLink"))
            {
                var newTags = new List<string>(session.Tags.Count + 1);
                newTags.AddRange(session.Tags);
                newTags.Add("DeathLink");


                session.UpdateTags(newTags);
            }
        }
    }
}