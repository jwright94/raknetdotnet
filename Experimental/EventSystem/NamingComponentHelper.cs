using System;
using System.Text;
using RakNetDotNet;

namespace EventSystem
{
    internal sealed class NamingComponentHelper
    {
        public static bool PrintIncomingTable(Packet packet)
        {
            byte[] data = packet.data;
            Console.Write("Incoming table:\n");
            Table table = new Table();
            byte[] serializedTable = new byte[data.Length - sizeof (byte)];
            Array.Copy(data, sizeof (byte), serializedTable, 0, data.Length - sizeof (byte)); // ugly copy
            if (TableSerializer.DeserializeTable(serializedTable, (uint) serializedTable.Length, table))
            {
                TableRowPage cur = table.GetListHead();
                int i;

                Console.Write("Columns:\n");
                for (i = 0; i < table.GetColumns().Size(); i++)
                {
                    Console.Write("{0}. {1} : ", i + 1, table.GetColumns()[i].columnName);
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
                        Console.Write("RowID {0}: {1}\n", cur.GetKey(i), sb.ToString());
                    }
                    cur = cur.next;
                }
                return true;
            }
            else
            {
                Console.Write("Deserialization of table failed.\n");
                return false;
            }
        }
    }
}