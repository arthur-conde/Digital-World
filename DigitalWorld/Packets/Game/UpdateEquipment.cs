using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game
{
    public class UpdateEquipment:Packet
    {
        /// <summary>
        /// Update Model?
        /// </summary>
        /// <param name="Slot"></param>
        public UpdateEquipment(short InvSlot, byte Slot)
        {
            packet.Type(1310);
            packet.WriteShort(InvSlot);
            packet.WriteByte(Slot);
            packet.WriteInt(0);
            packet.WriteUInt(0);
            packet.WriteShort(0);
            packet.WriteShort(0);
        }

        public UpdateEquipment(Item item)
        {
            packet.Type(1310);
            packet.WriteShort(8);
            packet.WriteByte((byte)item.ItemData.uShorts2[6]);
            packet.WriteInt(0);
            packet.WriteUInt(0);
            packet.WriteShort(0);
            packet.WriteShort(0);
        }

        public UpdateEquipment(short InvSlot, byte Slot, Item item)
        {
            packet.Type(1310);
            packet.WriteShort(InvSlot);
            packet.WriteByte(Slot);
            packet.WriteInt(item.ID);
            packet.WriteUInt(item.time_t);
            packet.WriteShort(item.Attributes[0]);
            packet.WriteShort(item.Attributes[1]);
        }
    }
}
