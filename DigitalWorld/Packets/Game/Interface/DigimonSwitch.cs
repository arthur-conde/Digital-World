using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game
{
    public class DigimonSwitch:Packet
    {
        /// <summary>
        /// Switch Mon1 to Mon2
        /// </summary>
        /// <param name="Mon1"></param>
        /// <param name="Mon2"></param>
        public DigimonSwitch(short DigimonHandle,byte slot, Digimon Mon1, Digimon Mon2)
        {
            packet.Type(1041);
            packet.WriteShort(DigimonHandle);
            packet.WriteInt(Mon1.Species);
            packet.WriteByte(slot);
            packet.WriteInt(Mon2.Species);
            packet.WriteByte((byte)Mon2.Level);
            packet.WriteString(Mon2.Name);
            packet.WriteShort(Mon2.Size);
        }
    }
}
