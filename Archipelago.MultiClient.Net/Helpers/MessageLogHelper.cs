using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class MessageLogHelper
    {
        public delegate void MessageReceivedHandler(LogMessage message);
        public event MessageReceivedHandler OnMessageReceived;

        private readonly IReceivedItemsHelper items;
        private readonly ILocationCheckHelper locations;
        private readonly IPlayerHelper players;
        private readonly IConnectionInfoProvider connectionInfo;

        internal MessageLogHelper(IArchipelagoSocketHelper socket,
            IReceivedItemsHelper items, ILocationCheckHelper locations,
            IPlayerHelper players, IConnectionInfoProvider connectionInfo)
        {
            this.items = items;
            this.locations = locations;
            this.players = players;
            this.connectionInfo = connectionInfo;

            socket.PacketReceived += Socket_PacketReceived;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            if (OnMessageReceived == null)
            {
                return;
            }

            switch (packet)
            {
                case PrintPacket printPacket:
                    TriggerOnMessageReceived(ToPrintJson(printPacket));
                    break;
                case PrintJsonPacket printJsonPacket:
                    TriggerOnMessageReceived(printJsonPacket);
                    break;
            }
        }

        private static PrintJsonPacket ToPrintJson(PrintPacket printPacket)
        {
            return new PrintJsonPacket { Data = new [] { new JsonMessagePart { Text = printPacket.Text } } };
        }

        private void TriggerOnMessageReceived(PrintJsonPacket printJsonPacket)
        {
            foreach (var linePacket in SplitPacketsPerLine(printJsonPacket))
            {
                LogMessage message;

                var parts = GetParsedData(linePacket);

                switch (linePacket)
                {
                    case HintPrintJsonPacket hintPrintJson:
                        message = new HintItemSendLogMessage(parts,
                            hintPrintJson.ReceivingPlayer, hintPrintJson.Item.Player,
                            hintPrintJson.Item, hintPrintJson.Found.HasValue && hintPrintJson.Found.Value);
                        break;
                    case ItemPrintJsonPacket itemPrintJson:
                        message = new ItemSendLogMessage(parts,
                            itemPrintJson.ReceivingPlayer, itemPrintJson.Item.Player, itemPrintJson.Item);
                        break;
                    default:
                        message = new LogMessage(parts);
                        break;
                }

                if (OnMessageReceived != null)
                {
                    OnMessageReceived(message);
                }
            }
        }

        private IEnumerable<PrintJsonPacket> SplitPacketsPerLine(PrintJsonPacket printJsonPacket)
        {
            List<PrintJsonPacket> packetsPerLine = new List<PrintJsonPacket>();

            List<JsonMessagePart> messageParts = new List<JsonMessagePart>();

            foreach (var part in printJsonPacket.Data)
            {
                var lines = part.Text.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    messageParts.Add(new JsonMessagePart {
                        Text = line,
                        Type = part.Type,
                        Color = part.Color,
                        Flags = part.Flags,
                        Player = part.Player
                    });

                    if (i < (lines.Length -1))
                    {
                        var splittedPrintJsonPacket = CloneWithoutData(printJsonPacket);
                        splittedPrintJsonPacket.Data = messageParts.ToArray();

                        packetsPerLine.Add(splittedPrintJsonPacket);

                        messageParts = new List<JsonMessagePart>();
                    }
                }
            }

            var lastPrintJsonPacket = CloneWithoutData(printJsonPacket);
            lastPrintJsonPacket.Data = messageParts.ToArray();

            packetsPerLine.Add(lastPrintJsonPacket);

            return packetsPerLine;
        }

        private static PrintJsonPacket CloneWithoutData(PrintJsonPacket source)
        {
            switch (source)
            {
                case HintPrintJsonPacket hintPrintJsonPacket:
                    return new HintPrintJsonPacket {
                        MessageType = hintPrintJsonPacket.MessageType,
                        ReceivingPlayer = hintPrintJsonPacket.ReceivingPlayer,
                        Item = hintPrintJsonPacket.Item,
                        Found = hintPrintJsonPacket.Found
                    };
                case ItemPrintJsonPacket itemPrintJson:
                    return new ItemPrintJsonPacket
                    {
                        MessageType = itemPrintJson.MessageType,
                        ReceivingPlayer = itemPrintJson.ReceivingPlayer,
                        Item = itemPrintJson.Item,
                    };
                default:
                    return new PrintJsonPacket
                    {
                        MessageType = source.MessageType,
                    };
            }
        }

        internal MessagePart[] GetParsedData(PrintJsonPacket packet)
        {
            return packet.Data.Select(GetMessagePart).ToArray();
        }

        private MessagePart GetMessagePart(JsonMessagePart part)
        {
            switch (part.Type)
            {
#if USE_OCULUS_NEWTONSOFT
                case "item_id":
                case "item_name":
#else
                case JsonMessagePartType.ItemId:
                case JsonMessagePartType.ItemName:
#endif
                    return new ItemMessagePart(items, part);
#if USE_OCULUS_NEWTONSOFT
                case "player_id":
                case "player_name":
#else
                case JsonMessagePartType.PlayerId:
                case JsonMessagePartType.PlayerName:
#endif
                    return new PlayerMessagePart(players, connectionInfo, part);
#if USE_OCULUS_NEWTONSOFT
                case "location_id":
                case "location_name":
#else
                case JsonMessagePartType.LocationId:
                case JsonMessagePartType.LocationName:
#endif
                    return new LocationMessagePart(locations, part);
#if USE_OCULUS_NEWTONSOFT
                case "entrance_name":
#else
                case JsonMessagePartType.EntranceName:
#endif
                    return new EntranceMessagePart(part);
                default:
                    return new MessagePart(MessagePartType.Text, part);
            }
        }
    }

    public class LogMessage
    {
        public MessagePart[] Parts { get; }

        internal LogMessage(MessagePart[] parts)
        {
            Parts = parts;
        }

        public override string ToString()
        {
            if (Parts.Length == 1)
            {
                return Parts[0].Text;
            }

            var builder = new StringBuilder(Parts.Length);

            foreach (var part in Parts)
            {
                builder.Append(part.Text);
            }

            return builder.ToString();
        }
    }
    
    public class ItemSendLogMessage : LogMessage
    {
        public int ReceivingPlayerSlot { get; }
        public int SendingPlayerSlot { get; }
        public NetworkItem Item { get; }

        internal ItemSendLogMessage(MessagePart[] parts, int receiver, int sender, NetworkItem item) : base(parts)
        {
            ReceivingPlayerSlot = receiver;
            SendingPlayerSlot = sender;
            Item = item;
        }
    }

    public class HintItemSendLogMessage : ItemSendLogMessage
    {
        public bool IsFound { get; }

        internal HintItemSendLogMessage(MessagePart[] parts, int receiver, int sender, NetworkItem item, bool found) 
            : base(parts, receiver, sender, item)
        {
            IsFound = found;
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

        internal MessagePart(MessagePartType type, JsonMessagePart messagePart, Color? color = null)
        {
            Type = type;
            Text = messagePart.Text;

            if (color.HasValue)
            {
                Color = color.Value;
            }
#if USE_OCULUS_NEWTONSOFT
            else if (!string.IsNullOrEmpty(messagePart.Color))
            {
                Color = GetColor(messagePart.Color);
                IsBackgroundColor = messagePart.Color.EndsWith("_bg");
            }
#else
            else if (messagePart.Color.HasValue)
            {
                Color = GetColor(messagePart.Color.Value);
                IsBackgroundColor = messagePart.Color.Value >= JsonMessagePartColor.BlackBg;
            }
#endif
            else
            {
                Color = Color.White;
            }
        }

#if USE_OCULUS_NEWTONSOFT
        private static Color GetColor(string color)
        {
            switch (color)
            {
                case "red":
                case "red_bg":
                    return Color.Red;
                case "green":
                case "green_bg":
                    return Color.Green;
                case "yellow":
                case "yellow_bg":
                    return Color.Yellow;
                case "blue":
                case "blue_bg":
                    return Color.Blue;
                case "magenta":
                case "magenta_bg":
                    return Color.Magenta;
                case "cyan":
                case "cyan_bg":
                    return Color.Cyan;
                case "black":
                case "black_bg":
                    return Color.Black;
                case "white":
                case "white_bg":
                    return Color.White;
                default:
                    return Color.White;
            }
        }
#else
        private static Color GetColor(JsonMessagePartColor color)
        {
            switch (color)
            {
                case JsonMessagePartColor.Red:
                case JsonMessagePartColor.RedBg:
                    return Color.Red;
                case JsonMessagePartColor.Green:
                case JsonMessagePartColor.GreenBg:
                    return Color.Green;
                case JsonMessagePartColor.Yellow:
                case JsonMessagePartColor.YellowBg:
                    return Color.Yellow;
                case JsonMessagePartColor.Blue:
                case JsonMessagePartColor.BlueBg:
                    return Color.Blue;
                case JsonMessagePartColor.Magenta:
                case JsonMessagePartColor.MagentaBg:
                    return Color.Magenta;
                case JsonMessagePartColor.Cyan:
                case JsonMessagePartColor.CyanBg:
                    return Color.Cyan;
                case JsonMessagePartColor.Black:
                case JsonMessagePartColor.BlackBg:
                    return Color.Black;
                case JsonMessagePartColor.White:
                case JsonMessagePartColor.WhiteBg:
                    return Color.White;
                default:
                    return Color.White;
            }
        }
#endif

        public override string ToString()
        {
            return Text;
        }
    }
    
    public class ItemMessagePart : MessagePart
    {
        public ItemFlags Flags { get; }
        public long ItemId { get; }

        internal ItemMessagePart(IReceivedItemsHelper items, JsonMessagePart part) : base(MessagePartType.Item, part)
        {
            Flags = part.Flags ?? ItemFlags.None;
            Color = GetColor(Flags);

            switch (part.Type)
            {
#if USE_OCULUS_NEWTONSOFT
                case "item_id":
#else
                case JsonMessagePartType.ItemId:
#endif
                    ItemId = long.Parse(part.Text);
                    Text = items.GetItemName(ItemId) ?? $"Item: {ItemId}";
                    break;
#if USE_OCULUS_NEWTONSOFT
                case "item_name":
#else
                case JsonMessagePartType.ItemName:
#endif
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
        
        internal PlayerMessagePart(IPlayerHelper players, IConnectionInfoProvider connectionInfo, JsonMessagePart part) : base (MessagePartType.Player, part)
        {
            switch (part.Type)
            {
#if USE_OCULUS_NEWTONSOFT
                case "player_id":
#else
                case JsonMessagePartType.PlayerId:
#endif
                    SlotId = int.Parse(part.Text);
                    IsActivePlayer = SlotId == connectionInfo.Slot;
                    Text = players.GetPlayerAlias(SlotId) ?? $"Player {SlotId}";
                    break;
#if USE_OCULUS_NEWTONSOFT
                case "player_name":
#else
                case JsonMessagePartType.PlayerName:
#endif
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

        internal LocationMessagePart(ILocationCheckHelper locations, JsonMessagePart part) 
            : base(MessagePartType.Location, part, Color.Green)
        {
            switch (part.Type)
            {
#if USE_OCULUS_NEWTONSOFT
                case "location_id":
#else
                case JsonMessagePartType.LocationId:
#endif
                    LocationId = long.Parse(part.Text);
                    Text = locations.GetLocationNameFromId(LocationId) ?? $"Location: {LocationId}";
                    break;
#if USE_OCULUS_NEWTONSOFT
                case "location_name":
#else
                case JsonMessagePartType.LocationName:
#endif
                    LocationId = 0; // we are not going to try to reverse lookup as we don't know the game this location belongs to
                    Text = part.Text;
                    break;
            }
        }
    }

    public class EntranceMessagePart : MessagePart
    {
        internal EntranceMessagePart(JsonMessagePart messagePart) : base(MessagePartType.Entrance, messagePart, Color.Blue)
        {
            Text = messagePart.Text;
        }
    }
}
