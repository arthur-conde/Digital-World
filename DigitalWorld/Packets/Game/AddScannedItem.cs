using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game
{
    public class AddScannedItem:Packet
    {
        public AddScannedItem(int ItemId,byte stack, int amount)
        {
            packet.Type(3922);
            packet.WriteInt(ItemId);
            packet.WriteByte(stack);
            packet.WriteInt(amount);//Possibly byte Int
            packet.WriteByte(0);
        }
    }
}
