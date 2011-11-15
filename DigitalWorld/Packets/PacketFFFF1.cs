using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets
{
    public class PacketFFFF:Packet
    {
        public PacketFFFF(short time)
        {
            // 0x8e, 0xd8
            packet.Type(-1);
            packet.WriteShort((short)time);
        }
    }
}
