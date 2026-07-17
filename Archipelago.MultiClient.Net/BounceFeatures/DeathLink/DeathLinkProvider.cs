
namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public static class DeathLinkProvider
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        ///     creates and returns a <see cref="DeathLinkService"/> for this <paramref name="session"/>.
        /// </summary>
        public static DeathLinkService CreateDeathLinkService(this ArchipelagoSession session) => 
	        new DeathLinkService(session.Socket, session.ConnectionInfo);

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        ///     creates and returns a <see cref="DeathLinkService"/> for this <paramref name="session"/>,
        ///     scoped to the DeathLink <paramref name="group"/>. Only players sharing the same group name
        ///     send and receive deaths to one another; a null or empty group joins the default DeathLink pool.
        /// </summary>
        public static DeathLinkService CreateDeathLinkService(this ArchipelagoSession session, string group) =>
	        new DeathLinkService(session.Socket, session.ConnectionInfo, group);
    }
}