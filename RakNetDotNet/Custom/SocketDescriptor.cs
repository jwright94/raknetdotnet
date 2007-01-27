namespace RakNetDotNet {

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
public struct SocketDescriptor {
  public SocketDescriptor(ushort _port, string _hostAddress) {
      port = _port;
      hostAddress = _hostAddress;
  }

  public ushort port;
  [MarshalAs(UnmanagedType.LPStr, SizeConst=32)]
  public string hostAddress;
}

}
