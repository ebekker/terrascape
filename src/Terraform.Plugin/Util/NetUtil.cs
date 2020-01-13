using System;

namespace Terraform.Plugin.Util
{
    public static class NetUtil
    {
        public static int FindFreePort(int minPort, int maxPort)
        {
            if (maxPort < minPort)
                return -1;

            for (var curPort = minPort; curPort < maxPort; ++curPort)
            {
                var listener = new System.Net.Sockets.TcpListener(
                    System.Net.IPAddress.Loopback, curPort);
                try
                {
                    listener.Start();
                    listener.Stop();
                    return curPort;
                }
                catch (Exception)
                { }
            }

            return -1;
        }        
    }
}