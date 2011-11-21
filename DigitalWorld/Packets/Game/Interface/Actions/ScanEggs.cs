using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game.Interface
{
    public class ScanEgg:Packet
    {
        /// <summary>
        /// A response to an egg scanning request.
        /// </summary>
        /// <param name="Id">Item Id</param>
        /// <param name="Max">Stack size</param>
        /// <param name="Amount">Amount added</param>
        /// <param name="Unknown">Unknown Function, try 0.</param>
        public ScanEgg(int Id, byte Max, byte Amount, int Unknown)
        {
            packet.Type(3922);
            packet.WriteInt(Id);
            packet.WriteByte(Max);
            packet.WriteByte(Amount);
            packet.WriteInt(Unknown);
        }
    }
}
