using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets
{
    public class PacketFFEF : Packet, IPacket
    {
        public PacketFFEF(short data)
        {
            int time_t = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            packet.Type(-2);
            packet.WriteShort(data);
            packet.WriteInt(time_t);
        }

        public PacketFFEF(int time_t, short data)
        {
            packet.Type(-2);
            packet.WriteShort(data);
            packet.WriteInt(time_t);
        }
    }
}