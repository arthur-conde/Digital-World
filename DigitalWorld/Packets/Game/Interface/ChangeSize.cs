using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Interface
{
    public class ChangeSize: Packet
    {
        public ChangeSize(short handle, int Size)
        {
            packet.Type(9942);
            packet.WriteShort(handle);
            packet.WriteInt(Size);
            packet.WriteShort(2); //Unknown
        }

        public ChangeSize(short handle, int Size, short u)
        {
            packet.Type(9942);
            packet.WriteShort(handle);
            packet.WriteInt(Size);
            packet.WriteShort(u); //Unknown
        }
    }
}
