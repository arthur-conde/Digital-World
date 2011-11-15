using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class Packet1704:Packet
    {
        public Packet1704()
        {
            packet.Type(1047);
            packet.WriteByte(0);
        }
    }
}
