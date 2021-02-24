using MultiClient.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;

namespace MultiClient.Net
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
                var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(e.Data);
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
