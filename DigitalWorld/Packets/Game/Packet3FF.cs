using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using Digital_World.Helpers;

namespace Digital_World.Packets.Game
{
    public class Packet3FF:Packet
    {
        public Packet3FF(short hMap, DigimonStats stats)
        {
            packet.Type(1023);
            packet.WriteShort(stats.HP);
            packet.WriteShort(stats.DS);
            packet.WriteInt(0);
        }
    }
}
