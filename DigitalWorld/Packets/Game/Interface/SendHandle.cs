using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Interface
{
    public class SendHandle:Packet
    {
        public SendHandle(int Handle)
        {
            packet.Type(1016);
            packet.WriteInt(Handle);
        }
    }
}
