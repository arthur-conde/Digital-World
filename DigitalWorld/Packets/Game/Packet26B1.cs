using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class UpdateMS:Packet
    {
        public UpdateMS(short TamerId, short DigiId, short TamerMS, short DigiMS)
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

        public UpdateMS(short TamerId, short DigiId, short MS, int uInt1, int uInt2)
        {
            packet.Type(9905);
            packet.WriteShort(TamerId);
            packet.WriteShort(DigiId);
            packet.WriteShort(MS);
            packet.WriteShort(MS);
            packet.WriteInt(uInt1);
            packet.WriteInt(uInt2);
        }
    }
}
