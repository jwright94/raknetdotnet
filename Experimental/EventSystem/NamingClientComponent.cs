using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    internal sealed class NamingClientComponent : INamingComponent
    {
        public NamingClientComponent(ILogger logger)
        {
            this.logger = logger;
        }

        #region INamingComponent Members

        public void OnStartup(RakPeerInterface peer)
        {
            peer.AttachPlugin(databaseClient);
        }

        public void OnConnectionRequestAccepted(RakPeerInterface peer, Packet packet)
        {
            byte numCellUpdates = 0;
            DatabaseCellUpdates cellUpdates = new DatabaseCellUpdates(8);
            cellUpdates[numCellUpdates].columnName = "Name";
            cellUpdates[numCellUpdates].columnType = Table.ColumnType.STRING;
            cellUpdates[numCellUpdates].cellValue.Set("Unknown Service");
            numCellUpdates++;

            databaseClient.UpdateRow("Services", string.Empty, RowUpdateMode.RUM_UPDATE_OR_ADD_ROW, false, 0,
                                     cellUpdates, numCellUpdates, packet.systemAddress, false);
        }

        public void OnDatabaseQueryReply(RakPeerInterface peer, Packet packet)
        {
            NamingComponentHelper.PrintIncomingTable(packet);
        }

        #endregion

        private LightweightDatabaseClient databaseClient = new LightweightDatabaseClient();
        private readonly ILogger logger;
    }
}