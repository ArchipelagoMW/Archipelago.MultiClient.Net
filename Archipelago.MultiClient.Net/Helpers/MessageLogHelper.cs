using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Allows clients to easily subscribe to incoming messages and helps formulating those messages correctly
	/// </summary>
    public class MessageLogHelper
    {
		/// <summary>
		/// Method signature for the OnMessageReceived event
		/// </summary>
		/// <param name="message">A message to display to the user</param>
		public delegate void MessageReceivedHandler(LogMessage message);

        /// <summary>
        /// Triggered for each message that should be presented to the player
        /// </summary>
        public event MessageReceivedHandler OnMessageReceived;

        readonly IReceivedItemsHelper items;
        readonly ILocationCheckHelper locations;
        readonly IPlayerHelper players;
        readonly IConnectionInfoProvider connectionInfo;

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

        void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            if (OnMessageReceived == null)
                return;

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

        static PrintJsonPacket ToPrintJson(PrintPacket printPacket) => 
	        new PrintJsonPacket { Data = new [] { new JsonMessagePart { Text = printPacket.Text } } };

        void TriggerOnMessageReceived(PrintJsonPacket printJsonPacket)
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
					case CountdownPrintJsonPacket countdownPrintJson:
						message = new CountdownLogMessage(parts, countdownPrintJson.RemainingSeconds);
						break;
                    default:
                        message = new LogMessage(parts);
                        break;
                }

                OnMessageReceived?.Invoke(message);
            }
        }

        static IEnumerable<PrintJsonPacket> SplitPacketsPerLine(PrintJsonPacket printJsonPacket)
        {
            var packetsPerLine = new List<PrintJsonPacket>();
            var messageParts = new List<JsonMessagePart>();

            foreach (var part in printJsonPacket.Data)
            {
                var lines = part.Text.Split('\n');

                for (var i = 0; i < lines.Length; i++)
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

        static PrintJsonPacket CloneWithoutData(PrintJsonPacket source)
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
                        Item = itemPrintJson.Item
                    };
				case CountdownPrintJsonPacket countdownPrintJson:
					return new CountdownPrintJsonPacket
					{
						RemainingSeconds = countdownPrintJson.RemainingSeconds
					};
				default:
                    return new PrintJsonPacket
                    {
                        MessageType = source.MessageType,
                    };
            }
        }

        internal MessagePart[] GetParsedData(PrintJsonPacket packet) => 
	        packet.Data.Select(GetMessagePart).ToArray();

        MessagePart GetMessagePart(JsonMessagePart part)
        {
            switch (part.Type)
            {
                case JsonMessagePartType.ItemId:
                case JsonMessagePartType.ItemName:
                    return new ItemMessagePart(items, part);
                case JsonMessagePartType.PlayerId:
                case JsonMessagePartType.PlayerName:
                    return new PlayerMessagePart(players, connectionInfo, part);
                case JsonMessagePartType.LocationId:
                case JsonMessagePartType.LocationName:
                    return new LocationMessagePart(locations, part);
                case JsonMessagePartType.EntranceName:
                    return new EntranceMessagePart(part);
                default:
                    return new MessagePart(MessagePartType.Text, part);
            }
        }
    }

	/// <summary>
	/// A message to display to the user, consisting of an array of message parts to form a sentence
	/// </summary>
    public class LogMessage
    {
		/// <summary>
		/// Different part of a message that should be used to build a sentence
		/// The order of the parts is the order the different sections should appear in
		/// </summary>
		public MessagePart[] Parts { get; }

        internal LogMessage(MessagePart[] parts)
        {
            Parts = parts;
        }

		/// <summary>
		/// Uses the the Parts to form a correct sentence
		/// </summary>
		/// <returns>the sentence this LogMessage is representing</returns>
        public override string ToString()
        {
            if (Parts.Length == 1)
                return Parts[0].Text;

            var builder = new StringBuilder(Parts.Length);

            foreach (var part in Parts)
                builder.Append(part.Text);

            return builder.ToString();
        }
    }

	/// <summary>
	/// A item send message to display to the user, consisting of an array of message parts to form a sentence
	/// item send messages contain additional information about the item that was send for more specific processing
	/// </summary>
	public class ItemSendLogMessage : LogMessage
    {
		/// <summary>
		/// The player slot number of the player who received the item
		/// </summary>
        public int ReceivingPlayerSlot { get; }
		/// <summary>
		/// The player slot number of the player who send the item
		/// </summary>
		public int SendingPlayerSlot { get; }
		/// <summary>
		/// The Item that was send
		/// </summary>
		public NetworkItem Item { get; }

        internal ItemSendLogMessage(MessagePart[] parts, int receiver, int sender, NetworkItem item) : base(parts)
        {
            ReceivingPlayerSlot = receiver;
            SendingPlayerSlot = sender;
            Item = item;
        }
    }

	/// <summary>
	/// A item hint message to display to the user, consisting of an array of message parts to form a sentence
	/// item hint messages contain additional information about the item that was send for more specific processing
	/// </summary>
	public class HintItemSendLogMessage : LogMessage
	{
		/// <summary>
		/// The player slot number of the player who received the item
		/// </summary>
		public int ReceivingPlayerSlot { get; }
		/// <summary>
		/// The player slot number of the player who send the item
		/// </summary>
		public int SendingPlayerSlot { get; }
		/// <summary>
		/// The Item that was send
		/// </summary>
		public NetworkItem Item { get; }
		/// <summary>
		/// Indicates if the location of this item was already checked
		/// </summary>
		public bool IsFound { get; }

        internal HintItemSendLogMessage(MessagePart[] parts, int receiver, int sender, NetworkItem item, bool found) : base(parts)
        {
	        ReceivingPlayerSlot = receiver;
	        SendingPlayerSlot = sender;
	        Item = item;
			IsFound = found;
        }
    }

	/// <summary>
	/// A countdown message to display to the user, consisting of an array of message parts to form a sentence
	/// countdown message contain additional information about the item that was send for more specific processing
	/// </summary>
	public class CountdownLogMessage : LogMessage
    {
		/// <summary>
		/// The amount of seconds remaining in the countdown
		/// </summary>
	    public int RemainingSeconds { get; }

	    internal CountdownLogMessage(MessagePart[] parts, int remainingSeconds) : base(parts)
	    {
		    RemainingSeconds = remainingSeconds;
	    }
    }

	/// <summary>
	/// 
	/// </summary>
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

        static Color GetColor(JsonMessagePartColor color)
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

        public override string ToString() => Text;
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
                case JsonMessagePartType.ItemId:
                    ItemId = long.Parse(part.Text);
                    Text = items.GetItemName(ItemId) ?? $"Item: {ItemId}";
                    break; 
                case JsonMessagePartType.ItemName:
                    ItemId = 0; // we are not going to try to reverse lookup this value based on the game of the receiving player
                    Text = part.Text;
                    break;
            }
        }

        static Color GetColor(ItemFlags flags)
        {
            if (HasFlag(flags, ItemFlags.Advancement))
                return Color.Plum;
            if (HasFlag(flags, ItemFlags.NeverExclude))
                return Color.SlateBlue;
            if (HasFlag(flags, ItemFlags.Trap))
                return Color.Salmon;

            return Color.Cyan;
        }

        static bool HasFlag(ItemFlags flags, ItemFlags flag) =>
#if NET35
            (flags & flag) > 0;
#else
            flags.HasFlag(flag);
#endif
    }

    public class PlayerMessagePart : MessagePart
    {
        public bool IsActivePlayer { get; }
        public int SlotId { get; }
        
        internal PlayerMessagePart(IPlayerHelper players, IConnectionInfoProvider connectionInfo, JsonMessagePart part) : base (MessagePartType.Player, part)
        {
            switch (part.Type)
            {
                case JsonMessagePartType.PlayerId:
                    SlotId = int.Parse(part.Text);
                    IsActivePlayer = SlotId == connectionInfo.Slot;
                    Text = players.GetPlayerAlias(SlotId) ?? $"Player {SlotId}";
                    break;
                case JsonMessagePartType.PlayerName:
                    SlotId = 0; // value is not slot resolvable according to docs
                    IsActivePlayer = false;
                    Text = part.Text;
                    break;
            }

            Color = GetColor(IsActivePlayer);
        }

        static Color GetColor(bool isActivePlayer)
        {
            if (isActivePlayer)
                return Color.Magenta;

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
                case JsonMessagePartType.LocationId:
                    LocationId = long.Parse(part.Text);
                    Text = locations.GetLocationNameFromId(LocationId) ?? $"Location: {LocationId}";
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
        internal EntranceMessagePart(JsonMessagePart messagePart) : base(MessagePartType.Entrance, messagePart, Color.Blue)
        {
            Text = messagePart.Text;
        }
    }
}
