using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebSocketSharp;

namespace Archipelago.MultiClient.Net
{
    public class ArchipelagoSession
    {
        public delegate void PacketReceivedHandler(ArchipelagoPacketBase packet);
        public event PacketReceivedHandler PacketReceived;

        public string Url { get; private set; }
        public bool Connected { get => Socket.IsAlive; }

        private WebSocket Socket;

        public ArchipelagoSession(string urlToHost)
        {
            this.Url = urlToHost;
            this.Socket = new WebSocket(urlToHost);
            this.Socket.OnMessage += OnMessageReceived;
        }

        public void Connect()
        {
            if (!Socket.IsAlive)
            {
                Socket.Connect();
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                Debug.WriteLine("======Incoming Packet below.=======");
                Debug.WriteLine(e.Data);
                Debug.WriteLine("======End packet.=======");
                var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(e.Data, new ArchipelagoPacketConverter());
                foreach (var packet in packets)
                {
                    PacketReceived(packet);
                }
            }
        }

        public void SendPacket(ArchipelagoPacketBase packet)
        {
            SendMultiplePackets(new List<ArchipelagoPacketBase> { packet });
        }

        public void SendMultiplePackets(List<ArchipelagoPacketBase> packets)
        {
            if (Socket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                Socket.Send(packetAsJson);
            }
        }
    }
}
