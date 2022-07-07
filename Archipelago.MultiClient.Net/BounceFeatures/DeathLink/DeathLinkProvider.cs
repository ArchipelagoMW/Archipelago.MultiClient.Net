using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public static class DeathLinkProvider
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        ///     Ensures the DeathLink tag is set with the session and then creates and returns a <see cref="DeathLinkService"/> for this <paramref name="session"/>.
        /// </summary>
        public static DeathLinkService CreateDeathLinkServiceAndEnable(this ArchipelagoSession session)
        {
            EnsureDeathLinkTagIsSet(session);

            return new DeathLinkService(session.Socket, session.ConnectionInfo, session.DataStorage);
        }

        private static void EnsureDeathLinkTagIsSet(ArchipelagoSession session)
        {
            if (Array.IndexOf(session.ConnectionInfo.Tags, "DeathLink") == -1)
            {
                var newTags = new List<string>(session.ConnectionInfo.Tags.Length + 1);
                newTags.AddRange(session.ConnectionInfo.Tags);
                newTags.Add("DeathLink");

                session.UpdateConnectionOptions(newTags.ToArray(), session.ConnectionInfo.ItemsHandlingFlags);
            }
        }
    }
}