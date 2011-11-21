using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Interface.Hatching
{
    public class DataInputSuccess : Packet
    {
        /// <summary>
        /// Data Input success
        /// </summary>
        /// <param name="Handle"></param>
        public DataInputSuccess(short Handle)
        {
            packet.Type(1037); //1040
            packet.WriteShort(Handle);
            packet.WriteByte(1);
        }

        /// <summary>
        /// Data Input success. Allow the egg to hatch.
        /// </summary>
        /// <param name="Handle">Tamer Handle</param>
        /// <param name="Scale">Digimon Scale</param>
        public DataInputSuccess(short Handle, byte Scale)
        {
            packet.Type(1037); //1040
            packet.WriteShort(Handle);
            packet.WriteByte(Scale);
        }
    }
}
