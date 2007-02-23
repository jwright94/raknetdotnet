using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;
    using RakNetDotNet;

    sealed class EventCenterClient : IDisposable
    {
        #region Ogre-like singleton implementation.
        static EventCenterClient instance;
        public EventCenterClient(string xmlFile)
        {
            Debug.Assert(instance == null);
            instance = this;

            Load(xmlFile);
            isConnected = false;

            if (isOnline)
            {
                rakClientInterface = RakNetworkFactory.GetRakPeerInterface();
                rakClientInterface.RegisterAsRemoteProcedureCall("sendeventtoclient", typeof(RpcCalls).GetMethod("SendEventToClient"));
            }
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;

            if (IsOnline)
            {
                log("Closing connection...");
                rakClientInterface.Shutdown(10);
                rakClientInterface.UnregisterAsRemoteProcedureCall("sendeventtoclient");
                RakNetworkFactory.DestroyRakPeerInterface(rakClientInterface);
                log("Closed.");
            }
        }
        public static EventCenterClient Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }
        #endregion
        public void OverrideClientPort(ushort newPort)
        {
            clientPort = newPort;
            log("Overwrote client port = {0}", clientPort);
        }
        public void OverrideServerPort(string serverIP)
        {
            ipString = serverIP;
            log("Overwrote server IP = {0}", ipString);
        }
        public void ReportEvent(IEvent _event)
        {
            if (isOnline)
            {
                PacketPriority priority = PacketPriority.HIGH_PRIORITY;
                PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
                byte orderingChannel = 0;
                uint shiftTimestamp = 0;

                if (false) log("sending event: [{0}] ...", _event.ToString());

                bool success = rakClientInterface.RPC(
                    "sendeventtoserver", _event.Stream,
                    priority, reliability, orderingChannel,
                    RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true,
                    shiftTimestamp, RakNetBindings.UNASSIGNED_NETWORK_ID, null);

                if (!success)
                    log("Unable to send event to server.");
                else
                    log("Event was sent.");
            }
            else
            {
                log("not connected");
            }
        }
        public void ProcessEvent(IEvent _event)
        {
            if (isOnline)
            {
                if (isConnected)
                    _event.Perform();
                else
                {
                    if (_event.PerformBeforeConnectOnClient)
                        _event.Perform();
                }
            }
        }
        public bool ConnectPlayer()
        {
            if (isOnline)
                return ConnectPlayer(ipString, serverPort);
            else
                return false;
        }
        public bool ConnectPlayer(string ip)
        {
            if (isOnline)
                return ConnectPlayer(ip, serverPort);
            else
                return false;
        }
        public bool ConnectPlayer(string ip, ushort serverPort)
        {
            if (isOnline)
            {
                log("connecting to the server...");

                int internalSleep = threadSleepTimierMS;
                SocketDescriptor socketDescriptor = new SocketDescriptor(clientPort, string.Empty);

                log("starting client on port {0}", clientPort);
                log("connecting to server on ip= {0}, port = {1}", ip, serverPort);

                rakClientInterface.Startup(1, internalSleep, new SocketDescriptor[] { socketDescriptor }, 1);
                bool success = rakClientInterface.Connect(ip, serverPort, string.Empty, 0);

                if (success)
                {
                    log("connected!");
                    isConnected = true;
                    return true;
                }
                else
                {
                    isOnline = false;
                    isConnected = false;
                    throw new NetworkException("Couldn't connect to server!");
                }
            }
            else
            {
                return false;
            }
        }
        public void Update()
        {
            if (IsOnline)
            {
                Packet packet = rakClientInterface.Receive();
                while (packet != null)
                {
                    StringBuilder message = new StringBuilder("recieved Message from player ");
                    message.Append(packet.systemAddress.ToString());
                    message.AppendFormat(" from ip {0}", packet.systemAddress.binaryAddress);
                    message.AppendFormat(" from port {0}", packet.systemAddress.port);

                    BitStream stream = new BitStream(packet, false);
                    byte packetIdentifier;
                    stream.Read(out packetIdentifier);
                    message.AppendFormat(", stream = [{0}]", packetIdentifier);

                    if (true) log(message.ToString());

                    rakClientInterface.DeallocatePacket(packet);
                    packet = rakClientInterface.Receive();
                }
            }
        }
        public bool IsOnline
        {
            get { return isOnline; }
        }
        #region Private Members
        void Load(string xmlFile)
        {
            // TODO - Use xml reader.
            ipString = "127.0.0.1";
            isOnline = true;
            serverPort = 6000;
            clientPort = 20000;
            threadSleepTimierMS = 0;
        }
        void log(string message)
        {
            Console.WriteLine(message);
        }
        void log(string format, params object[] args)
        {
            log(string.Format(format, args));
        }
        RakPeerInterface rakClientInterface;
        string ipString;
        ushort serverPort;
        ushort clientPort;
        int threadSleepTimierMS;
        bool isOnline;
        bool isConnected;
        #endregion
    }
}
