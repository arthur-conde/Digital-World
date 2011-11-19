using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class DespawnPlayer : Packet
    {
        public DespawnPlayer(short hTamer, short hDigimon)
        {
            packet.Type(1006);
            packet.WriteShort(514);
            packet.WriteByte(0);
            packet.WriteShort(hTamer);
            packet.WriteShort(hDigimon);
            packet.WriteByte(0);
        }
    }
}
