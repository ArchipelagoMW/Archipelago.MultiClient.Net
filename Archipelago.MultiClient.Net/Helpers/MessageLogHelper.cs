using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Allows clients to easily subscribe to incoming messages and helps formulating those messages correctly
	/// </summary>
	public interface IMessageLogHelper
	{
		/// <summary>
		/// Triggered for each message that should be presented to the player
		/// </summary>
		event MessageLogHelper.MessageReceivedHandler OnMessageReceived;
	}

	///<inheritdoc/>
	public class MessageLogHelper : IMessageLogHelper
	{
		/// <summary>
		/// Delegate for the OnMessageReceived event
		/// </summary>
		/// <param name="message">The log message object, this object can be MessageLog.Messages classes, switch this message based on type to get access to finer details</param>
		public delegate void MessageReceivedHandler(LogMessage message);

		/// <inheritdoc />
		public event MessageReceivedHandler OnMessageReceived;

		readonly IItemInfoResolver itemInfoResolver;
        readonly IPlayerHelper players;
        readonly IConnectionInfoProvider connectionInfo;

        internal MessageLogHelper(
	        IArchipelagoSocketHelper socket, IItemInfoResolver itemInfoResolver,
            IPlayerHelper players, IConnectionInfoProvider connectionInfo)
        {
            this.itemInfoResolver = itemInfoResolver;
            this.players = players;
            this.connectionInfo = connectionInfo;

            socket.PacketReceived += Socket_PacketReceived;
        }

        void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            if (OnMessageReceived == null || !(packet is PrintJsonPacket printJsonPacket))
                return;

            TriggerOnMessageReceived(printJsonPacket);
        }

        void TriggerOnMessageReceived(PrintJsonPacket printJsonPacket)
        {
            foreach (var linePacket in SplitPacketsPerLine(printJsonPacket))
            {
                LogMessage message;

                var parts = GetParsedData(linePacket);

                switch (linePacket)
                {
	                case ItemPrintJsonPacket itemPrintJson:
		                message = new ItemSendLogMessage(parts, players,
							itemPrintJson.ReceivingPlayer, itemPrintJson.Item.Player, itemPrintJson.Item, itemInfoResolver);
		                break;
	                case ItemCheatPrintJsonPacket itemCheatPrintJson:
		                message = new ItemCheatLogMessage(parts, players,
			                itemCheatPrintJson.Team, itemCheatPrintJson.ReceivingPlayer, 
			                itemCheatPrintJson.Item, itemInfoResolver);
		                break;
					case HintPrintJsonPacket hintPrintJson:
                        message = new HintItemSendLogMessage(parts, players,
							hintPrintJson.ReceivingPlayer, hintPrintJson.Item.Player,
                            hintPrintJson.Item, hintPrintJson.Found.HasValue && hintPrintJson.Found.Value, itemInfoResolver);
                        break;
	                case JoinPrintJsonPacket joinPrintJson:
		                message = new JoinLogMessage(parts, players,
							joinPrintJson.Team, joinPrintJson.Slot, joinPrintJson.Tags);
		                break;
	                case LeavePrintJsonPacket leavePrintJson:
		                message = new LeaveLogMessage(parts, players,
							leavePrintJson.Team, leavePrintJson.Slot);
		                break;
	                case ChatPrintJsonPacket chatPrintJson:
		                message = new ChatLogMessage(parts, players,
							chatPrintJson.Team, chatPrintJson.Slot, chatPrintJson.Message);
		                break;
	                case ServerChatPrintJsonPacket serverChatPrintJson:
		                message = new ServerChatLogMessage(parts, serverChatPrintJson.Message);
		                break;
	                case TutorialPrintJsonPacket _:
		                message = new TutorialLogMessage(parts);
		                break;
	                case TagsChangedPrintJsonPacket tagsPrintJson:
		                message = new TagsChangedLogMessage(parts, players,
			                tagsPrintJson.Team, tagsPrintJson.Slot, tagsPrintJson.Tags);
						break;
	                case CommandResultPrintJsonPacket _:
		                message = new CommandResultLogMessage(parts);
		                break;
	                case AdminCommandResultPrintJsonPacket _:
		                message = new AdminCommandResultLogMessage(parts);
		                break;
	                case GoalPrintJsonPacket goalPrintJsonPacket:
		                message = new GoalLogMessage(parts, players,
			                goalPrintJsonPacket.Team, goalPrintJsonPacket.Slot);
		                break;
	                case ReleasePrintJsonPacket releasePrintJsonPacket:
		                message = new ReleaseLogMessage(parts, players,
			                releasePrintJsonPacket.Team, releasePrintJsonPacket.Slot);
		                break;
	                case CollectPrintJsonPacket collectPrintJsonPacket:
		                message = new CollectLogMessage(parts, players,
			                collectPrintJsonPacket.Team, collectPrintJsonPacket.Slot);
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
                        Player = part.Player,
						HintStatus = part.HintStatus
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
	            case ItemPrintJsonPacket itemPrintJson:
		            return new ItemPrintJsonPacket
		            {
			            MessageType = itemPrintJson.MessageType,
			            ReceivingPlayer = itemPrintJson.ReceivingPlayer,
			            Item = itemPrintJson.Item
		            };
	            case ItemCheatPrintJsonPacket itemCheatPrintJson:
		            return new ItemCheatPrintJsonPacket
		            {
			            MessageType = itemCheatPrintJson.MessageType,
			            ReceivingPlayer = itemCheatPrintJson.ReceivingPlayer,
			            Item = itemCheatPrintJson.Item,
			            Team = itemCheatPrintJson.Team
		            };
				case HintPrintJsonPacket hintPrintJsonPacket:
                    return new HintPrintJsonPacket {
                        MessageType = hintPrintJsonPacket.MessageType,
                        ReceivingPlayer = hintPrintJsonPacket.ReceivingPlayer,
                        Item = hintPrintJsonPacket.Item,
                        Found = hintPrintJsonPacket.Found
                    };
	            case JoinPrintJsonPacket joinPrintJson:
		            return new JoinPrintJsonPacket
					{
			            MessageType = joinPrintJson.MessageType,
			            Team = joinPrintJson.Team,
			            Slot = joinPrintJson.Slot,
			            Tags = joinPrintJson.Tags
		            };
	            case LeavePrintJsonPacket leavePrintJson:
		            return new LeavePrintJsonPacket
					{
			            MessageType = leavePrintJson.MessageType,
			            Team = leavePrintJson.Team,
			            Slot = leavePrintJson.Slot,
		            };
	            case ChatPrintJsonPacket chatPrintJson:
		            return new ChatPrintJsonPacket
		            {
			            MessageType = chatPrintJson.MessageType,
			            Team = chatPrintJson.Team,
			            Slot = chatPrintJson.Slot,
						Message = chatPrintJson.Message
					};
	            case ServerChatPrintJsonPacket serverChatPrintJson:
		            return new ServerChatPrintJsonPacket
					{
			            MessageType = serverChatPrintJson.MessageType,
			            Message = serverChatPrintJson.Message
		            };
	            case TutorialPrintJsonPacket tutorialPrintJson:
		            return new TutorialPrintJsonPacket
					{
			            MessageType = tutorialPrintJson.MessageType,
		            };
	            case TagsChangedPrintJsonPacket tagsChangedPrintJson:
		            return new TagsChangedPrintJsonPacket
					{
						MessageType = tagsChangedPrintJson.MessageType,
						Team = tagsChangedPrintJson.Team,
						Slot = tagsChangedPrintJson.Slot,
						Tags = tagsChangedPrintJson.Tags
					};
	            case CommandResultPrintJsonPacket commandResultPrintJson:
		            return new CommandResultPrintJsonPacket 
		            {
			            MessageType = commandResultPrintJson.MessageType,
		            };

	            case AdminCommandResultPrintJsonPacket adminCommandResultPrintJson:
		            return new AdminCommandResultPrintJsonPacket
					{
			            MessageType = adminCommandResultPrintJson.MessageType,
		            };
	            case GoalPrintJsonPacket goalPrintJson:
		            return new GoalPrintJsonPacket
					{
			            MessageType = goalPrintJson.MessageType,
			            Team = goalPrintJson.Team,
			            Slot = goalPrintJson.Slot,
		            };
	            case ReleasePrintJsonPacket releasePrintJson:
		            return new ReleasePrintJsonPacket
					{
			            MessageType = releasePrintJson.MessageType,
			            Team = releasePrintJson.Team,
			            Slot = releasePrintJson.Slot,
		            };
	            case CollectPrintJsonPacket collectPrintJson:
		            return new CollectPrintJsonPacket
					{
			            MessageType = collectPrintJson.MessageType,
			            Team = collectPrintJson.Team,
			            Slot = collectPrintJson.Slot,
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
                    return new ItemMessagePart(players, itemInfoResolver, part);
                case JsonMessagePartType.PlayerId:
                case JsonMessagePartType.PlayerName:
                    return new PlayerMessagePart(players, connectionInfo, part);
                case JsonMessagePartType.LocationId:
                case JsonMessagePartType.LocationName:
                    return new LocationMessagePart(players, itemInfoResolver, part);
                case JsonMessagePartType.EntranceName:
                    return new EntranceMessagePart(part);
				case JsonMessagePartType.HintStatus:
					return new HintStatusMessagePart(part);
                default:
                    return new MessagePart(MessagePartType.Text, part);
            }
        }
    }
}
