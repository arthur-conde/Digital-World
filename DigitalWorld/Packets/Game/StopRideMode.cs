using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class StopRideMode : Packet
    {
        public StopRideMode(short hTamer, short hDigimon)
        {
            packet.Type(1326);
            packet.WriteShort(hTamer);
            packet.WriteShort(hDigimon);
        }
    }
}
