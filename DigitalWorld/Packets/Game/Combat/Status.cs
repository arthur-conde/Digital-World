using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using Digital_World.Helpers;

namespace Digital_World.Packets.Game
{
    public class Status:Packet
    {
        public Status(short hMap, DigimonStats stats)
        {
            packet.Type(1023);
            packet.WriteShort(hMap);
            packet.WriteShort(stats.HP);
            packet.WriteShort(stats.DS);
            packet.WriteInt(0);
        }
    }
}
