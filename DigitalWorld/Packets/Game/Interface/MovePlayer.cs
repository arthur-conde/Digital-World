using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game.Interface
{
    public class MovePlayer:Packet
    {
        public MovePlayer(Character Tamer)
        {
            Digimon Partner = Tamer.Partner;
            packet.Type(0x3ee);
            packet.WriteShort(0x205);
            packet.WriteByte(0);

            packet.WriteShort(Tamer.TamerHandle);
            packet.WriteInt(Tamer.Location.PosX);
            packet.WriteInt(Tamer.Location.PosY);
            packet.WriteShort(Tamer.DigimonHandle);
            packet.WriteInt(Partner.Location.PosX);
            packet.WriteInt(Partner.Location.PosY);

            packet.WriteByte(0);
        }
    }
}
