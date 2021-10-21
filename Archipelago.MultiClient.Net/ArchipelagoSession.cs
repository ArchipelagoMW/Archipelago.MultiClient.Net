using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.MultiClient.Net
{
    public class ArchipelagoSession
    {
        public ArchipelagoSocketHelper Socket { get; }

        public ReceivedItemsHelper Items { get; }

        internal ArchipelagoSession(ArchipelagoSocketHelper socket, ReceivedItemsHelper items)
        {
            Socket = socket;
            Items = items;
        }
    }
}