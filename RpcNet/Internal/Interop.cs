namespace RpcNet.Internal
{
    using System;
    using System.Runtime.InteropServices;

    class Interop
    {
        [DllImport("ws2_32.dll", EntryPoint = "sendto")]
        public static extern unsafe int SendTo(IntPtr Socket, byte* buffer, int len, int flags, byte* to, int tolen);

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto)]
        public static extern int WSAGetLastError();
    }
}
