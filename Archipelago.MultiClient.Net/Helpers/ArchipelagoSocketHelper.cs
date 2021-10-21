using Archipelago.MultiClient.Net.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ArchipelagoSocketHelper

    {
        public delegate void PacketReceivedHandler(ArchipelagoPacketBase packet);
        public event PacketReceivedHandler PacketReceived;

        public delegate void ErrorReceivedHandler(Exception e, string message);
        public event ErrorReceivedHandler ErrorReceived;

        public delegate void SocketClosedHandler(CloseEventArgs e);
        public event SocketClosedHandler SocketClosed;

        public string Url { get; private set; }
        public bool Connected { get => Socket.IsAlive; }

        private WebSocket Socket;

        public ArchipelagoSocketHelper(string hostUrl)
        {
            Url = hostUrl;
            Socket = new WebSocket(hostUrl);
            Socket.OnMessage += OnMessageReceived;
            Socket.OnError += OnError;
            Socket.OnClose += OnClose;
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            if (SocketClosed != null)
            {
                SocketClosed(e);
            }
        }

        public void Connect()
        {
            if (!Socket.IsAlive)
            {
                Socket.Connect();
            }
        }

        public void ConnectAsync()
        {
            if (!Socket.IsAlive)
            {
                Socket.ConnectAsync();
            }
        }

        public void Disconnect()
        {
            if (Socket.IsAlive)
            {
                Socket.Close();
            }
        }

        public void DisconnectAsync()
        {
            if (Socket.IsAlive)
            {
                Socket.CloseAsync();
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.IsText && PacketReceived != null)
            {
                var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(e.Data, new ArchipelagoPacketConverter());
                foreach (var packet in packets)
                {
                    PacketReceived(packet);
                }
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            if (ErrorReceived != null)
            {
                ErrorReceived(e.Exception, e.Message);
            }
        }

        public void SendPacket(ArchipelagoPacketBase packet)
        {
            SendMultiplePackets(new List<ArchipelagoPacketBase> { packet });
        }

        public void SendMultiplePackets(List<ArchipelagoPacketBase> packets)
        {
            SendMultiplePackets(packets.ToArray());
        }

        public void SendMultiplePackets(params ArchipelagoPacketBase[] packets)
        {
            if (Socket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                Socket.Send(packetAsJson);
            }
        }

        public void SendPacketAsync(Action<bool> onComplete, ArchipelagoPacketBase packet)
        {
            SendMultiplePacketsAsync(onComplete, new List<ArchipelagoPacketBase> { packet });
        }

        public void SendMultiplePacketsAsync(Action<bool> onComplete, List<ArchipelagoPacketBase> packets)
        {
            SendMultiplePacketsAsync(onComplete, packets.ToArray());
        }

        public void SendMultiplePacketsAsync(Action<bool> onComplete, params ArchipelagoPacketBase[] packets)
        {
            if (Socket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                Socket.SendAsync(packetAsJson, onComplete);
            }
        }
    }
}
