using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class FriendList:Packet
    {
        public FriendList()
        {
            packet.Type(2404);
            packet.WriteInt(0);
        }

        public FriendList(KeyValuePair<string, int> FriendList)
        {
            packet.Type(2404);
            packet.WriteInt(0);
        }
    }
}
