using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class Packet26B2:Packet
    {
        public Packet26B2(short TamerId, short DigiId, short TamerMS, short DigiMS)
        {
            packet.Type(9905);
            packet.WriteShort(TamerId);
            packet.WriteShort(DigiId);
            packet.WriteShort(TamerMS);
            packet.WriteShort(DigiMS);
            packet.WriteShort(0);
            packet.WriteShort(0);
            packet.WriteShort(0);
            packet.WriteShort(0);
        }
    }
}
