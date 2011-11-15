using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Lobby
{
    public class NameCheck:Packet,IPacket
    {
        public NameCheck(int available)
        {
            packet.Type(1302);
            packet.WriteInt(available);
        }
    }
}
