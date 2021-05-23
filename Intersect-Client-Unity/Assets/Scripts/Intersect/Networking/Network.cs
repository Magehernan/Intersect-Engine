using System;
using Intersect.Client.Framework.Network;
using Intersect.Client.General;
using Intersect.Client.MessageSystem;
using Intersect.Client.UnityGame.Network;
using Intersect.Configuration;
using Intersect.Logging;
using Intersect.Network;
using Intersect.Network.Events;
using Intersect.Plugins.Helpers;

namespace Intersect.Client.Networking
{

    public static class Network
    {

        public static bool Connecting { get; private set; }

        private static bool sConnected;

        private static GameSocket socket;

        private static PacketHandler packetHandler;

        private static NetworkHelper networkHelper;

        private static int sPing;

        public static bool Connected => socket?.IsConnected() ?? sConnected;

        private static byte[] rsaBytes;
        public static int Ping
        {
            get => socket?.Ping ?? sPing;
            set => sPing = value;
        }

        public static void InitNetwork(byte[] bytes)
        {

            Logger logger = Log.Default;
            PacketTypeRegistry packetTypeRegistry = new PacketTypeRegistry(logger);
            if (!packetTypeRegistry.TryRegisterBuiltIn())
            {
                logger.Error("Failed to register built-in packets.");
                return;
            }

            PacketHandlerRegistry packetHandlerRegistry = new PacketHandlerRegistry(packetTypeRegistry, logger);
            networkHelper = new NetworkHelper(packetTypeRegistry, packetHandlerRegistry);
            PackedIntersectPacket.AddKnownTypes(networkHelper.AvailablePacketTypes);
            packetHandler = new PacketHandler(logger, networkHelper.HandlerRegistry);

            rsaBytes = bytes;
            socket = new UnitySocket();
            socket.Connected += MySocket_OnConnected;
            socket.Disconnected += MySocket_OnDisconnected;
            socket.DataReceived += MySocket_OnDataReceived;
            socket.ConnectionFailed += MySocket_OnConnectionFailed;
            TryConnect();
        }

        private static void TryConnect()
        {
            sConnected = false;
            MessageManager.SendMessage(MessageTypes.NetworkStatus, NetworkStatus.Connecting);
            socket?.Connect(ClientConfiguration.Instance.Host, ClientConfiguration.Instance.Port, rsaBytes, networkHelper);
        }

        private static void MySocket_OnConnectionFailed(INetworkLayerInterface sender, ConnectionEventArgs connectionEventArgs, bool denied)
        {
            MessageManager.SendMessage(MessageTypes.NetworkStatus, connectionEventArgs.NetworkStatus);
            sConnected = false;
            if (!denied)
            {
                TryConnect();
            }
        }

        private static void MySocket_OnDataReceived(IPacket packet)
        {
            packetHandler.HandlePacket(packet);
        }

        private static void MySocket_OnDisconnected(INetworkLayerInterface sender, ConnectionEventArgs connectionEventArgs)
        {
            MessageManager.SendMessage(MessageTypes.NetworkStatus, connectionEventArgs.NetworkStatus);
            //Not sure how to handle this yet!
            sConnected = false;
            if (Globals.GameState == GameStates.InGame || Globals.GameState == GameStates.Loading)
            {
                Globals.ConnectionLost = true;
                socket?.Disconnect(string.Empty);
                TryConnect();
            }
            else
            {
                socket?.Disconnect(string.Empty);
                TryConnect();
            }
        }


        private static void MySocket_OnConnected(INetworkLayerInterface sender, ConnectionEventArgs connectionEventArgs)
        {
            MessageManager.SendMessage(MessageTypes.NetworkStatus, connectionEventArgs.NetworkStatus);
            //Not sure how to handle this yet!
            sConnected = true;
        }

        public static void Close(string reason)
        {
            if (socket is null)
            {
                return;
            }

            try
            {
                sConnected = false;
                Connecting = false;
                socket.Disconnect(reason);
                socket.Dispose();
                socket.Connected -= MySocket_OnConnected;
                socket.Disconnected -= MySocket_OnDisconnected;
                socket.DataReceived -= MySocket_OnDataReceived;
                socket.ConnectionFailed -= MySocket_OnConnectionFailed;
                socket = null;
                MessageManager.SendMessage(MessageTypes.NetworkStatus, NetworkStatus.Offline);
            }
            catch (Exception exception)
            {
                Log.Trace(exception);
            }
        }

        public static void SendPacket(IntersectPacket packet)
        {
            socket?.SendPacket(packet);
        }

        public static void Update()
        {
            if (sConnected)
            {
                socket?.Update();
            }
        }
    }
}
