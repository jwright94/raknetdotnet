using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    internal sealed class NamingServerComponent : INamingComponent
    {
        public NamingServerComponent(ILogger logger)
        {
            this.logger = logger;
        }

        #region INamingComponent Members

        public void OnStartup(RakPeerInterface peer)
        {
            peer.AttachPlugin(databaseServer);

            string tableName = "Services";
            Table table =
                databaseServer.AddTable(tableName, true, true, true, string.Empty, string.Empty, string.Empty, true,
                                        true, true, true, true);
            if (table != null)
            {
                logger.Debug("Table {0} created.\n", tableName);
                table.AddColumn("Name", Table.ColumnType.STRING);
            }
        }

        public void OnConnectionRequestAccepted(RakPeerInterface peer, Packet packet)
        {
        }

        public void OnDatabaseQueryReply(RakPeerInterface peer, Packet packet)
        {
            NamingComponentHelper.PrintIncomingTable(packet);
        }

        #endregion

        private LightweightDatabaseServer databaseServer = new LightweightDatabaseServer();
        private readonly ILogger logger;
    }
}