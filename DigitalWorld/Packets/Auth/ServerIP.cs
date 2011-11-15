using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Auth
{
    public class ServerIP:Packet,IPacket
    {
        public ServerIP(string IP, int Port, uint AccountID, int UniqueID)
        {
            packet.Type(901);
            packet.WriteUInt(AccountID);
            packet.WriteInt(UniqueID);
            packet.WriteString(IP);
            packet.WriteInt(Port);
        }
    }
}
