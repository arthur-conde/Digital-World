using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class StoreDigimon:Packet
    {
        public StoreDigimon(int Slot, int ArchiveSlot, int Bits)
        {
            packet.Type(3201);
            packet.WriteInt(Slot);
            packet.WriteInt(ArchiveSlot);
            packet.WriteInt(Bits);
        }
    }
}
