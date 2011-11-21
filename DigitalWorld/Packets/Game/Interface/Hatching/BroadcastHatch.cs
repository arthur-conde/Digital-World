using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class BroadcastHatch:Packet
    {
        public BroadcastHatch(string tamerName, string digiName, int digiType, int Size, int Scale)
        {
            packet.Type(1048);
            packet.WriteString(tamerName);
            packet.WriteString(digiName);
            packet.WriteInt(digiType);
            packet.WriteInt(Size);
            packet.WriteInt(Scale);
        }
    }
}
