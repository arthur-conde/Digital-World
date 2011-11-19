using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Chat
{
    public class ChatNormal :Packet
    {
        public ChatNormal(short hSpeaker, string message)
        {
            packet.Type(1006);
            packet.WriteShort((short)ChatType.Normal);
            packet.WriteShort(hSpeaker);
            packet.WriteString(message);
            packet.WriteByte(0);
        }
    }
}
