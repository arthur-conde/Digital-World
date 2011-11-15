using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Lobby
{
    public class ConfirmCreate:Packet
    {
        /// <summary>
        /// Confirms character was created successfully
        /// </summary>
        public ConfirmCreate()
        {
            packet.Type(1306);
            packet.WriteShort(12480);
            packet.WriteInt(2);
            packet.WriteShort(0);
        }
    }
}
