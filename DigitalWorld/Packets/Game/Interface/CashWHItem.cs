using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game.Interface
{
    public class CashWHItem : Packet
    {
        public CashWHItem(int slot, Item item, int amount,  int max)
        {
            packet.Type(3936);
            packet.WriteByte(0);
            packet.WriteByte((byte)slot);
            packet.WriteInt(item.ID);
            packet.WriteByte((byte)amount);
            packet.WriteByte((byte)max);
            packet.WriteUInt(item.time_t);
        }
    }
}
