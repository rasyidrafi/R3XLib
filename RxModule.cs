using System.IO;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace R3xLib
{
    public class RxModule
    {
        private WebClient wc = new WebClient();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool WaitNamedPipe(string name, int timeout);

    }
}
