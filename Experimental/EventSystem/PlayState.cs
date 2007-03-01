using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;

    class PlayState : AbstractGameState, IDisposable
    {
        #region Ogre-like singleton implementation.
        static PlayState instance;
        public PlayState(ushort _clientPort, string _serverIP)
            : base("Play")
        {
            Debug.Assert(instance == null);
            instance = this;

            clientPort = _clientPort;
            serverIP = _serverIP;
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;
        }
        public static PlayState Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }
        #endregion
        #region IGameState Members
        public override void Enter()
        {
            log("starting...");
            Load("application.xml");

            try
            {
                ConnectToServer();
            }
            catch (NetworkException e)
            {
                Console.WriteLine(e.ToString());
                ChangeState(IntroState.Instance);
                return;
            }
        }
        public override void Exit()
        {
            ServiceConfigurator.Resolve<SampleEventFactory>().Reset();
            rpcCalls.Reset();
            ClientWorld.Instance.Dispose();

            eventCenterClient.Dispose();
        }
        public override void Pause()
        {
        }
        public override void Resume()
        {
        }
        public override bool KeyPressed(char key)
        {
            return true;
        }
        public override bool FrameStarted()
        {
            Update(0.0f);  // hmm

            return true;
        }
        #endregion
        #region Protected Members
        protected void Update(float dt)
        {
            UpdateNetwork();

            EventCenterClient.Instance.Update();
        }
        protected void UpdateNetwork()
        {
            EventCenterClient.Instance.Update();
        }
        protected void Load(string xmlFile)
        {
            // TODO - use xml reader
            networkConnectTimeout = 100;
        }
        protected void ConnectToServer()
        {
            log("starting network...");

            new ClientWorld();

            SampleEventFactory factory = ServiceConfigurator.Resolve<SampleEventFactory>();
            factory.Reset();
            rpcCalls = ServiceConfigurator.Resolve<RpcCalls>();
            rpcCalls.Reset();
            rpcCalls.Handler = factory;

            eventCenterClient = new EventCenterClient("client.xml");
            rpcCalls.ProcessEventOnClientSide += eventCenterClient.ProcessEvent;
            eventCenterClient.OverrideClientPort(clientPort);
            eventCenterClient.OverrideServerPort(serverIP);
            eventCenterClient.ConnectPlayer();

            log("client started...");

            // try a few times
            bool success = false;
            for (int j = 0; j < 5 && !success; ++j)
            {
                ClientWorld.Instance.TestConnectionWithServer();
                bool reply = false;
                long i = 1;
                long milliSecondWait = networkConnectTimeout;
                while (!reply && i <= milliSecondWait)
                {
                    reply = ClientWorld.Instance.GetTestResponseFromServer();
                    System.Threading.Thread.Sleep(1);
                    EventCenterClient.Instance.Update();
                    ++i;

                    if (i % 10 == 0) Console.WriteLine("{0} {1}", i, networkConnectTimeout);
                }

                if (reply) success = true;
            }
        }
        #endregion
        #region Private Members
        void log(string message)
        {
            Console.WriteLine(message);
        }

        long networkConnectTimeout;

        ushort clientPort;
        string serverIP;

        EventCenterClient eventCenterClient;
        RpcCalls rpcCalls;
        #endregion
    }
}
