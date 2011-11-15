using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Lobby
{
    public class CharDelete:Packet
    {
        public CharDelete(int result)
        {
            packet.Type(1304);
            packet.WriteInt(result);
        }
    }
}
