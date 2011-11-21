using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Interface
{
    public class DataInputFailure:Packet
    {
        public DataInputFailure(short Handle, bool Broken)
        {
            packet.Type(1040); //1040
            packet.WriteShort(Handle);
            packet.WriteByte((byte)(Broken ? 0 : 1));
        }
    }
}
