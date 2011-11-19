using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class RidingMode : Packet
    {
        public RidingMode(short hTamer, short hDigimon)
        {
            packet.Type(1325);
            packet.WriteShort(hTamer);
            packet.WriteShort(hDigimon);
        }
    }
}
