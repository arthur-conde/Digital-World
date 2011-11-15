using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Digital_World.Network
{
    /// <summary>
    /// Server Information
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// Port to listen on
        /// </summary>
        public int Port = 0;

        /// <summary>
        /// IP to bind to
        /// </summary>
        public IPAddress Host = IPAddress.Loopback;

        public ServerInfo(int Port, IPAddress Host)
        {
            this.Port = Port;
            this.Host = Host;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Host, Port);
        }

        public static List<IPAddress> IPAddresses()
        {
            List<IPAddress> ips = new List<IPAddress>();
            ips.AddRange(Dns.GetHostEntry("localhost").AddressList);
            ips.AddRange(Dns.GetHostEntry(Dns.GetHostName()).AddressList);
            return ips;
        }
    }
}
