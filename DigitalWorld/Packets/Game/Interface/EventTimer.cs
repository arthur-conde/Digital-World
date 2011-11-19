using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    /// <summary>
    /// Packet of unknown function
    /// \nLength: 14
    /// </summary>
    public class RewardCountdown:Packet
    {
        public RewardCountdown(int i1, DateTime timeLeft)
        {
            packet.Type(3106);
            packet.WriteInt(i1);
            packet.WriteInt((int)timeLeft.TimeOfDay.TotalSeconds);
        }
    }
}
