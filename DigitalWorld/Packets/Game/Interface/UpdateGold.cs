using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class UpdateGold : Packet
    {
        public UpdateGold(int Change, int Total)
        {
            packet.Type(3923);
            packet.WriteInt(Change);
            packet.WriteInt(Total);
            packet.WriteInt(0);
        }
    }
}
