using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class UpdateLevel:Packet
    {
        public UpdateLevel(short Handle, byte Level)
        {
            packet.Type(1019);
            packet.WriteShort(Handle);
            packet.WriteByte(Level);
        }
    }
}
