using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Interface
{
    public class ReturnEggs :Packet
    {
        /// <summary>
        /// Response to returning cracked digitamas
        /// </summary>
        /// <param name="Bits">Bits gained</param>
        /// <param name="Total">Total number of bits</param>
        /// <param name="Unknown">Function unknown</param>
        public ReturnEggs(int Bits, int Total, int Unknown)
        {
            packet.Type(3923);
            packet.WriteInt(Bits);
            packet.WriteInt(Total);
            packet.WriteInt(Unknown);
        }
    }
}
