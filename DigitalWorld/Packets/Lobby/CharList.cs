using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Lobby
{
    public class CharList : Packet, IPacket
    {
        public CharList(List<Character> listTamers)
        {
            packet.Type(1301);
            byte iChar = 0;
            foreach (Character Tamer in listTamers)
            {
                packet.WriteByte(iChar++);
                packet.WriteShort((short)Tamer.Location.Map);
                packet.WriteInt((int)Tamer.Model);
                packet.WriteByte((byte)Tamer.Level);
                packet.WriteString(Tamer.Name);
                for (int i = 0; i < 9; i++)
                {
                    Item item = Tamer.Equipment[i];
                    packet.WriteBytes(item.ToArray());
                    /*packet.WriteInt(0); //Unknown
                    packet.WriteShort((short)item.Attributes[0]);
                    packet.WriteShort((short)item.Attributes[1]);
                    packet.WriteInt(0); //Unknown
                    packet.WriteInt(0); //Unknown
                    packet.WriteInt(0); //Unknown. 0 or -1;*/
                }
                packet.WriteInt(Tamer.Partner.DigiType);
                packet.WriteByte((byte)Tamer.Partner.Level);
                packet.WriteString(Tamer.Partner.Name);
                packet.WriteShort((short)Tamer.Partner.Size); //Partner Size
            }
            packet.WriteByte(99);
        }
    }
}
