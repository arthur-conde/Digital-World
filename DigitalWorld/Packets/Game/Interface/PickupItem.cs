using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class PickupItem : Packet
    {
        public PickupItem(short objId, short itemId, short modifier, short amount)
        {
            packet.Type(3910);
            packet.WriteShort(objId);
            packet.WriteShort(itemId);
            packet.WriteShort(modifier);
            packet.WriteShort(amount);
        }
    }
}
