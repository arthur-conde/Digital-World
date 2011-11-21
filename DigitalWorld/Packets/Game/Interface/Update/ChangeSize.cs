using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Interface
{
    public class ChangeSize: Packet
    {
        public enum ChangeType
        {
            /// <summary>
            /// A permanent size change.
            /// </summary>
            Permanent = 0,
            /// <summary>
            /// A temporary size change. Lasts 3 min
            /// </summary>
            Temporary= 2
        }

        public ChangeSize(short handle, int Size, ChangeType Type)
        {
            packet.Type(9942);
            packet.WriteShort(handle);
            packet.WriteInt(Size);
            packet.WriteShort((short)Type); //Unknown
        }
    }
}
