using System;
using System.Collections.Generic;
using System.Text;

namespace RakNetDotNet.Custom
{
    struct SystemAddressStruct
    {
        public SystemAddressStruct(SystemAddress refObject)
        {
            binaryAddress = refObject.binaryAddress;
            port = refObject.port;
        }

        public SystemAddress ToRefObject()
        {
            SystemAddress refObject = new SystemAddress();
            refObject.binaryAddress = binaryAddress;
            refObject.port = port;
            return refObject;
        }

        public uint binaryAddress;
        public ushort port;
    }
}
