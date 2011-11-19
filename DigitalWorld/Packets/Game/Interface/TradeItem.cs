using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game
{
    public class TradeItem:Packet
    {
        public TradeItem(short handle, Item item)
        {
            packet.Type(1508);
            packet.WriteShort(handle);
            packet.WriteBytes(item.ToArray());
        }
    }
}
