using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class Evolve : Packet
    {
        public Evolve(short hDigimon, short hTamer, int Model, byte unknown)
        {
            packet.Type(1028);
            packet.WriteShort(hDigimon);
            packet.WriteShort(hTamer);
            packet.WriteInt(Model);
            packet.WriteByte(unknown);
        }
    }
}
