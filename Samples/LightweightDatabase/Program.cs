using System;
using System.Collections.Generic;
using System.Text;

namespace LightweightDatabase
{
    using RakNetDotNet;

    class Program
    {
        static void Main(string[] args)
        {
#if true
            char ch;
            bool isServer;
            LightweightDatabaseServer databaseServer = new LightweightDatabaseServer();
            LightweightDatabaseClient databaseClient = new LightweightDatabaseClient();
            RakPeerInterface rakPeer = RakNetworkFactory.GetRakPeerInterface();
            string str;
            string columnName;
            string tableName = "", tablePassword = "";
            Console.Write("(S)erver or (C)lient?\n");
            ch = Console.ReadKey(true).KeyChar;
            if (ch == 's')
            {
                isServer = true;
                rakPeer.SetMaximumIncomingConnections(32);
                SocketDescriptor socketDescriptor = new SocketDescriptor(12345, string.Empty);
                rakPeer.Startup(32, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                rakPeer.AttachPlugin(databaseServer);
                Console.Write("Server started\n");
                Console.Write("(C)reate table\n");
                Console.Write("(R)emove table\n");
            }
            else
            {
                isServer = false;
                SocketDescriptor socketDescriptor = new SocketDescriptor();
                rakPeer.Startup(1, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                rakPeer.AttachPlugin(databaseClient);
                Console.Write("Client started\n");

                Console.Write("Enter server IP: ");
                str = Console.ReadLine();
                if (str.Equals(string.Empty))
                    str = "127.0.0.1";
                Console.Write("Connecting to server.\n");
                rakPeer.Connect(str, 12345, string.Empty, 0);
                Console.Write("(Q)uery table\n");
                Console.Write("(U)pdate row\n");
                Console.Write("(R)emove row\n");
            }
            Console.Write("(E)xit\n");

            Packet p;
            while (true)
            {
                p = rakPeer.Receive();
                while (p != null)
                {
                    byte[] data = p.data;
                    if (data[0] == RakNetBindings.ID_DISCONNECTION_NOTIFICATION)
                        Console.Write("ID_DISCONNECTION_NOTIFICATION\n");
                    else if (data[0] == RakNetBindings.ID_CONNECTION_LOST)
                        Console.Write("ID_CONNECTION_LOST\n");
                    else if (data[0] == RakNetBindings.ID_NO_FREE_INCOMING_CONNECTIONS)
                        Console.Write("ID_NO_FREE_INCOMING_CONNECTIONS\n");
                    else if (data[0] == RakNetBindings.ID_NEW_INCOMING_CONNECTION)
                        Console.Write("ID_NEW_INCOMING_CONNECTION\n");
                    else if (data[0] == RakNetBindings.ID_CONNECTION_REQUEST_ACCEPTED)
                        Console.Write("ID_CONNECTION_REQUEST_ACCEPTED\n");
                    else if (data[0] == RakNetBindings.ID_CONNECTION_ATTEMPT_FAILED)
                        Console.Write("ID_CONNECTION_ATTEMPT_FAILED\n");
                    else if (data[0] == RakNetBindings.ID_DATABASE_UNKNOWN_TABLE)
                        Console.Write("ID_DATABASE_UNKNOWN_TABLE\n");
                    else if (data[0] == RakNetBindings.ID_DATABASE_INCORRECT_PASSWORD)
                        Console.Write("ID_DATABASE_INCORRECT_PASSWORD\n");
                    else if (data[0] == RakNetBindings.ID_DATABASE_QUERY_REPLY)
                    {
                        Console.Write("Incoming table:\n");
                        Table table = null;
                        byte[] serializedTable = new byte[data.Length - sizeof(byte)];
                        data.CopyTo(serializedTable, sizeof(byte));  // ugly copy
                        if (TableSerializer.DeserializeTable(serializedTable, (uint)serializedTable.Length, table))
                        {
                            TableRowPage cur = table.GetListHead();
                            int i;

                            Console.Write("Columns:\n");
                            for (i = 0; i < table.GetColumns().Size(); i++)
                            {
                                Console.Write("%i. %s : ", i + 1, table.GetColumns()[i].columnName);
                                if (table.GetColumns()[i].columnType == Table.ColumnType.BINARY)
                                    Console.Write("BINARY");
                                else if (table.GetColumns()[i].columnType == Table.ColumnType.NUMERIC)
                                    Console.Write("NUMERIC");
                                else
                                    Console.Write("STRING");
                                Console.Write("\n");
                            }
                            if (cur != null)
                                Console.Write("Rows:\n");
                            else
                                Console.Write("Table has no rows.\n");
                            while (cur != null)
                            {
                                for (i = 0; i < cur.size; i++)
                                {
                                    StringBuilder sb = new StringBuilder(256);
                                    table.PrintRow(sb, sb.Capacity, ',', true, cur.GetData(i));
                                    Console.Write("RowID %i: %s\n", cur.GetKey(i), sb.ToString());
                                }
                                cur = cur.next;
                            }
                        }
                        else
                            Console.Write("Deserialization of table failed.\n");
                    }

                    rakPeer.DeallocatePacket(p);
                    p = rakPeer.Receive();
                }

                if (_kbhit() != 0)
                {
                    char _ch = Console.ReadKey(true).KeyChar;
                    if (isServer)
                    {
                        if (_ch == 'c')
                        {
                            bool allowRemoteUpdate;
                            bool allowRemoteQuery;
                            bool allowRemoteRemove;
                            string queryPassword = string.Empty;
                            string updatePassword = string.Empty;
                            string removePassword = string.Empty;
                            bool oneRowPerSystemAddress = false;
                            bool onlyUpdateOwnRows = false;
                            bool removeRowOnPingFailure = false;
                            bool removeRowOnDisconnect = false;
                            bool autogenerateRowIDs;

                            Console.Write("Enter name of table to create: ");
                            tableName = Console.ReadLine();
                            if (tableName.Equals(string.Empty))
                                tableName = "Default Table";

                            Console.Write("Allow remote row updates? (y (Default) / n)\n");
                            if (Console.ReadKey(false).KeyChar == 'n')
                            {
                                Console.Write("\n");
                                allowRemoteUpdate = false;
                            }
                            else
                            {
                                Console.Write("\n");
                                allowRemoteUpdate = true;
                                Console.Write("Enter remote update password (Enter for none): ");
                                updatePassword = Console.ReadLine();

                                Console.Write("Only allow one row per uploading IP? (y (Default) / n)\n");
                                oneRowPerSystemAddress = (Console.ReadKey(false).KeyChar == 'n') == false;

                                Console.Write("Only allow updates on rows created by that system? (y (Default) / n)\n");
                                onlyUpdateOwnRows = (Console.ReadKey(false).KeyChar == 'n') == false;

                                Console.Write("Remove row if can't ping system? (y (Default) / n)\n");
                                removeRowOnPingFailure = (Console.ReadKey(false).KeyChar == 'n') == false;

                                Console.Write("Remove row on system disconnect? (y / n (Default))\n");
                                removeRowOnDisconnect = (Console.ReadKey(false).KeyChar == 'y') == true;
                            }

                            Console.Write("Allow remote table queries? (y (Default) / n)\n");
                            if (Console.ReadKey(false).KeyChar == 'n')
                            {
                                Console.Write("\n");
                                allowRemoteQuery = false;
                            }
                            else
                            {
                                Console.Write("\n");
                                allowRemoteQuery = true;
                                Console.Write("Enter remote table query password (Enter for none): ");
                                queryPassword = Console.ReadLine();
                            }

                            Console.Write("Allow remote row removal? (y (Default) / n)\n");
                            if (Console.ReadKey(false).KeyChar == 'n')
                            {
                                Console.Write("\n");
                                allowRemoteRemove = false;
                            }
                            else
                            {
                                Console.Write("\n");
                                allowRemoteRemove = true;
                                Console.Write("Enter remote row removal password (Enter for none): ");
                                removePassword = Console.ReadLine();
                            }

                            Console.Write("Autogenerate row ids? (y (Default) / n)\n");
                            autogenerateRowIDs = (Console.ReadKey(false).KeyChar == 'n') == false;

                            Table table;
                            table = databaseServer.AddTable(tableName, allowRemoteUpdate, allowRemoteQuery, allowRemoteRemove, queryPassword, updatePassword, removePassword, oneRowPerSystemAddress, onlyUpdateOwnRows, removeRowOnPingFailure, removeRowOnDisconnect, autogenerateRowIDs);
                            if (table != null)
                            {
                                Console.Write("Table {0} created.\n", tableName);
                                while (true)
                                {
                                    Console.Write("Enter name of new column\n");
                                    Console.Write("Hit enter when done\n");
                                    columnName = Console.ReadLine();
                                    if (columnName.Equals(string.Empty))
                                        break;
                                    Table.ColumnType columnType;
                                    Console.Write("Enter column type\n1=STRING\n2=NUMERIC\n3=BINARY\n");
                                    str = Console.ReadLine();
                                    if (str[0] == '1')
                                        columnType = Table.ColumnType.STRING;
                                    else if (str[0] == '2')
                                        columnType = Table.ColumnType.NUMERIC;
                                    else if (str[0] == '3')
                                        columnType = Table.ColumnType.BINARY;
                                    else
                                    {
                                        Console.Write("Defaulting to string\n");
                                        columnType = Table.ColumnType.STRING;
                                    }
                                    table.AddColumn(columnName, columnType);
                                    Console.Write("{0} added.\n", columnName);
                                }
                                Console.Write("Done.\n");
                            }
                            else
                                Console.Write("Table {0} creation failed.  Possibly already exists.\n", tableName);

                        }
                        else if (_ch == 'r')
                        {
                            Console.Write("Enter name of table to remove: ");
                            str = Console.ReadLine();
                            if (str.Equals(string.Empty))
                                str = "Default Table";
                            if (databaseServer.RemoveTable(str))
                                Console.Write("Success\n");
                            else
                                Console.Write("Table %s not found\n", str);
                        }
                    }
                    else
                    {
                        if (_ch == 'q' || _ch == 'u' || _ch == 'r')
                        {
                            Console.Write("Enter table name: ");
                            tableName = Console.ReadLine();
                            if (tableName.Equals(string.Empty))
                                tableName = "Default Table";
                            Console.Write("Enter password (if any): ");
                            tablePassword = Console.ReadLine();
                        }

                        if (_ch == 'q')
                        {
                            // TODO - let the user enter filters, columns, and rows to return.
                            // TODO - not yet.
                            //databaseClient.QueryTable(tableName, tablePassword, 0, 0, 0, 0, 0, 0, UNASSIGNED_SYSTEM_ADDRESS, true);
                        }
                        else if (_ch == 'u')
                        {
                            RowUpdateMode updateMode;
                            uint rowId;
                            bool hasRowId;
                            Console.Write("Enter row update mode\n1=update existing\n2=update or add\n3=add new\n");
                            str = Console.ReadLine();
                            if (str[0] == '1')
                            {
                                updateMode = RowUpdateMode.RUM_UPDATE_EXISTING_ROW;
                                Console.Write("Updating existing row (You must enter a row ID).\n");
                            }
                            else if (str[0] == '2')
                            {
                                updateMode = RowUpdateMode.RUM_UPDATE_OR_ADD_ROW;
                                Console.Write("Updating existing row and adding\nnew row if existing row does not exist.\n");
                            }
                            else
                            {
                                updateMode = RowUpdateMode.RUM_ADD_NEW_ROW;
                                Console.Write("Adding new row.\n");
                            }

                            Console.Write("Enter row ID (enter for none): ");
                            str = Console.ReadLine();
                            if (!str.Equals(string.Empty))
                            {
                                rowId = uint.Parse(str);
                                hasRowId = true;
                            }
                            else
                            {
                                hasRowId = false;
                                rowId = 0;
                            }

                            Console.Write("Enter cells\n");
                            byte numCellUpdates;
                            DatabaseCellUpdate[] cellUpdates = new DatabaseCellUpdate[64];
                            for (numCellUpdates = 0; numCellUpdates < 64; numCellUpdates++)
                            {
                                cellUpdates[numCellUpdates] = new DatabaseCellUpdate();
                                Console.Write("Enter column name (Enter when done): ");

                                cellUpdates[numCellUpdates].columnName = Console.ReadLine();
                                if (cellUpdates[numCellUpdates].columnName.Equals(string.Empty))
                                    break;
                                Console.Write("Enter column type\n1=STRING\n2=NUMERIC\n3=BINARY\n");
                                str = Console.ReadLine();
                                if (str[0] == '1' || str.Equals(string.Empty))
                                {
                                    cellUpdates[numCellUpdates].columnType = Table.ColumnType.STRING;
                                    Console.Write("Enter string value: ");
                                    str = Console.ReadLine();
                                    cellUpdates[numCellUpdates].cellValue.Set(str);
                                }
                                else if (str[0] == '2')
                                {
                                    cellUpdates[numCellUpdates].columnType = Table.ColumnType.NUMERIC;
                                    Console.Write("Enter numeric value: ");
                                    str = Console.ReadLine();
                                    cellUpdates[numCellUpdates].cellValue.Set(int.Parse(str));
                                }
                                else
                                {
                                    cellUpdates[numCellUpdates].columnType = Table.ColumnType.BINARY;
                                    // TODO - Pain in the ass to write this demo code
                                    Console.Write("TODO\n");
                                }
                            }

                            // TODO: not yet.
                            //databaseClient.UpdateRow(tableName, tablePassword, updateMode, hasRowId, rowId, cellUpdates, numCellUpdates, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
                        }
                        else if (_ch == 'r')
                        {
                            uint rowId;
                            Console.Write("Enter row ID to remove: ");
                            str = Console.ReadLine();
                            rowId = uint.Parse(str);
                            databaseClient.RemoveRow(tableName, tablePassword, rowId, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
                        }
                    }

                    if (_ch == 'e')
                        break;

                    _ch = '\0';
                }

                RakNetBindings.RakSleep(30);
            }

            rakPeer.Shutdown(100, 0);
            RakNetworkFactory.DestroyRakPeerInterface(rakPeer);
#endif
        }

        [System.Runtime.InteropServices.DllImport("crtdll.dll")]
        public static extern int _kbhit();  // I do not want to use this.
    }
}
