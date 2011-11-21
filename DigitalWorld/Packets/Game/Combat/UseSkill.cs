using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Combat
{
    public class UseSkill:Packet
    {
        public UseSkill(short hCaster, short hTarget, short skillSlot, byte RemainingHP, ushort Damage)
        {
            packet.Type(1101); //Uses a skill lol
            packet.WriteShort(hCaster);
            packet.WriteShort(hTarget);
            packet.WriteShort(skillSlot);
            packet.WriteShort(0);
            packet.WriteByte(RemainingHP);
            packet.WriteByte(3);
            packet.WriteUShort(Damage);
            packet.WriteByte(0xff);
        }
    }
}
