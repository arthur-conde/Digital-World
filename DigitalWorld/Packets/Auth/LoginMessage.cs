using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Auth
{
    /// <summary>
    /// The login message shown by Joymax
    /// </summary>
    public class LoginMessage : Packet,IPacket
    {
        public LoginMessage(string message)
        {
            packet.Type(3306);
            packet.WriteInt(0);
            packet.WriteString(message);
            packet.WriteString("NULL");
        }
    }
}
