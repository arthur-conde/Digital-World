using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class Packet3EC:Packet
    {
        public Packet3EC(short hTamer, int X, int Y, float dir)
        {
            packet.Type(1004);
            packet.WriteInt(112297);
            packet.WriteShort(hTamer);
            packet.WriteInt(X);
            packet.WriteInt(Y);
            packet.WriteFloat(dir);
        }
    }
}
