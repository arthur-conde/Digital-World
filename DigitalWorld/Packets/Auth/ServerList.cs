using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Digital_World.Packets.Auth
{
    public class ServerList:Packet, IPacket
    {
        public ServerList(Dictionary<int, string> servers, string user, int characters)
        {
            packet.Type(3302);
            packet.WriteByte((byte)servers.Count);
            foreach(KeyValuePair<int, string> server in servers)
            {
                packet.WriteInt(server.Key);
                packet.WriteString(server.Value);
                packet.WriteByte(0);
                packet.WriteByte(0); //Selected Character?
                packet.WriteByte((byte)characters); //Characters?
            }
            packet.WriteString(user);
        }
    }
}
