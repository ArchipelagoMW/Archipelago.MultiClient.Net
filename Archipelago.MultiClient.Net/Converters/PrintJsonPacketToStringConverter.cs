using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net.Converters
{
    public static class PrintJsonPacketExtensions
    {
        public static MessagePart[] GetParsedData(this IPrintJsonPacket packet, ArchipelagoSession session)
        {
            return packet.Data.Select(part => GetMessagePart(session, part)).ToArray();
        }

        public static string ToString(this IPrintJsonPacket packet, ArchipelagoSession session)
        {
            var builder = new StringBuilder(packet.Data.Length);

            foreach (var part in packet.Data)
            {
                builder.Append(GetMessagePart(session, part));
            }

            return builder.ToString();
        }

        private static MessagePart GetMessagePart(ArchipelagoSession session, JsonMessagePart part)
        {
            switch (part.Type)
            {
                case JsonMessagePartType.ItemId:
                case JsonMessagePartType.ItemName:
                    return new ItemMessagePart(session, part);
                case JsonMessagePartType.PlayerId:
                case JsonMessagePartType.PlayerName:
                    return new PlayerMessagePart(session, part);
                case JsonMessagePartType.LocationId:
                case JsonMessagePartType.LocationName:
                    return new LocationMessagePart(session, part);
                case JsonMessagePartType.EntranceName:
                    return new EntranceMessagePart(part);
                default:
                    return new MessagePart(MessagePartType.Text, part);
            }
        }
    }

    public enum MessagePartType
    {
        Text,
        Player,
        Item,
        Location,
        Entrance
    }

    public class MessagePart
    {
        public string Text { get; internal set; }
        public MessagePartType Type { get; internal set; }
        public Color Color { get; internal set; }
        public bool IsBackgroundColor { get; internal set; }

        public MessagePart(MessagePartType type, JsonMessagePart messagePart, Color? color = null)
        {
            Type = type;
            Text = messagePart.Text;

            if (color.HasValue)
            {
                Color = color.Value;
            }
            else if (messagePart.Color.HasValue)
            {
                Color = GetColor(messagePart.Color.Value);
                IsBackgroundColor = messagePart.Color.Value >= JsonMessagePartColor.BlackBg;
            }
            else
            {
                Color = Color.White;
            }
        }

        private static Color GetColor(JsonMessagePartColor color)
        {
            switch (color)
            {
                case JsonMessagePartColor.Red:
                    return Color.Red;
                case JsonMessagePartColor.Green:
                    return Color.Green;
                case JsonMessagePartColor.Yellow:
                    return Color.Yellow;
                case JsonMessagePartColor.Blue:
                    return Color.Blue;
                case JsonMessagePartColor.Magenta:
                    return Color.Magenta;
                case JsonMessagePartColor.Cyan:
                    return Color.Cyan;
                case JsonMessagePartColor.Black:
                    return Color.Black;
                case JsonMessagePartColor.White:
                    return Color.White;
                default:
                    return Color.White;
            }
        }

        public override string ToString()
        {
            return Text;
        }
    }
    
    public class ItemMessagePart : MessagePart
    {
        public ItemFlags Flags { get; }
        public long ItemId { get; }

        public ItemMessagePart(ArchipelagoSession session, JsonMessagePart part) : base(MessagePartType.Item, part)
        {
            Flags = part.Flags ?? ItemFlags.None;
            Color = GetColor(Flags);

            switch (part.Type)
            {
                case JsonMessagePartType.ItemId:
                    ItemId = long.Parse(part.Text);
                    Text = session.Items.GetItemName(ItemId);
                    break; 
                case JsonMessagePartType.ItemName:
                    ItemId = 0; // we are not going to try to reverse lookup this value based on the game of the receiving player
                    Text = part.Text;
                    break;
            }
        }

        private static Color GetColor(ItemFlags flags)
        {
            if (HasFlag(flags, ItemFlags.Advancement))
            {
                return Color.Plum;
            }
            if (HasFlag(flags, ItemFlags.NeverExclude))
            {
                return Color.SlateBlue;
            }
            if (HasFlag(flags, ItemFlags.Trap))
            {
                return Color.Salmon;
            }

            return Color.Cyan;
        }

        private static bool HasFlag(ItemFlags flags, ItemFlags flag)
        {
#if NET35
            return (flags & flag) > 0;
#else
            return flags.HasFlag(flag);
#endif
        }
    }

    public class PlayerMessagePart : MessagePart
    {
        public bool IsActivePlayer { get; }
        public int SlotId { get; }
        
        public PlayerMessagePart(ArchipelagoSession session, JsonMessagePart part) : base (MessagePartType.Player, part)
        {
            switch (part.Type)
            {
                case JsonMessagePartType.PlayerId:
                    SlotId = int.Parse(part.Text);
                    IsActivePlayer = SlotId == session.ConnectionInfo.Slot;
                    Text = session.Players.GetPlayerAlias(SlotId);
                    break;
                case JsonMessagePartType.PlayerName:
                    SlotId = 0; // value is not slot resolvable according to docs
                    IsActivePlayer = false;
                    Text = part.Text;
                    break;
            }

            Color = GetColor(IsActivePlayer);
        }

        private static Color GetColor(bool isActivePlayer)
        {
            if (isActivePlayer)
            {
                return Color.Magenta;
            }

            return Color.Yellow;
        }
    }

    public class LocationMessagePart : MessagePart
    {
        public long LocationId { get; }

        public LocationMessagePart(ArchipelagoSession session, JsonMessagePart part) 
            : base(MessagePartType.Location, part, Color.Green)
        {
            switch (part.Type)
            {
                case JsonMessagePartType.LocationId:
                    LocationId = long.Parse(part.Text);
                    Text = session.Locations.GetLocationNameFromId(LocationId);
                    break;
                case JsonMessagePartType.PlayerName:
                    LocationId = 0; // we are not going to try to reverse lookup as we don't know the game this location belongs to
                    Text = part.Text;
                    break;
            }
        }
    }

    public class EntranceMessagePart : MessagePart
    {
        public EntranceMessagePart(JsonMessagePart messagePart) : base(MessagePartType.Entrance, messagePart, Color.Blue)
        {
            Text = messagePart.Text;
        }
    }
}
