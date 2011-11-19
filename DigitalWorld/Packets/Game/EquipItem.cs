using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class EquipItem:Packet
    {
        public EquipItem(short itemId, short slot)
        {
            packet.Type(1264);
            packet.WriteShort(itemId);
            packet.WriteShort(slot);
        }
    }
}
