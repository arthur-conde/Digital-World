using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Combat
{
    public class CombatOn:Packet
    {
        /// <summary>
        /// Toggles combat on
        /// </summary>
        /// <param name="Handle"></param>
        public CombatOn(short Handle)
        {
            packet.Type(1034);
            packet.WriteShort(Handle);
            
            /*packet.Type(2308);
            packet.WriteShort(Handle);
            packet.WriteInt(9999);
            packet.WriteInt(0);*/
            /*
            packet.Type(1101); //Uses a skill lol
            packet.WriteShort(Handle);
            packet.WriteShort(Handle);
            packet.WriteShort(1);
            packet.WriteShort(0);
            packet.WriteByte(200);
            packet.WriteByte(3);
            packet.WriteShort(9999);
            packet.WriteByte(0xff);
             * */
            /*
            packet.Type(1035);
            packet.WriteShort(Handle);
            packet.WriteInt(17985);
            packet.WriteInt(0);
             * */
        }
    }
}
