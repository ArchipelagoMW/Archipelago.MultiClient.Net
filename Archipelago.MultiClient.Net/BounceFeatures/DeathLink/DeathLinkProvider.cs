using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public static class DeathLinkProvider
    {
        /// <summary>
        ///     Ensures the DeathLink tag is set with the session and then creates and returns a <see cref="DeathLinkService"/> for this <paramref name="session"/>.
        /// </summary>
        public static DeathLinkService CreateDeathLinkServiceAndEnable(this ArchipelagoSession session)
        {
            EnsureDeathLinkTagIsSet(session);

            return new DeathLinkService(session);
        }

        private static void EnsureDeathLinkTagIsSet(ArchipelagoSession session)
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