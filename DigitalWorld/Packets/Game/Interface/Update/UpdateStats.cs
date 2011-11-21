using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game
{
    public class UpdateStats:Packet
    {
        public UpdateStats(Character Tamer, Digimon Partner)
        {
            packet.Type(1043);
            packet.WriteShort((short)Tamer.MaxHP);
            packet.WriteShort((short)Tamer.MaxDS);
            packet.WriteShort((short)Tamer.HP);
            packet.WriteShort((short)Tamer.DS);
            packet.WriteShort((short)Tamer.AT);
            packet.WriteShort((short)Tamer.DE);
            packet.WriteShort((short)Tamer.MS);

            packet.WriteShort(Partner.Stats.MaxHP);
            packet.WriteShort(Partner.Stats.MaxDS);
            packet.WriteShort(Partner.Stats.HP);
            packet.WriteShort(Partner.Stats.DS);

            packet.WriteShort(Partner.Stats.Intimacy);

            packet.WriteShort(Partner.Stats.AT);
            packet.WriteShort(Partner.Stats.DE);
            packet.WriteShort(Partner.Stats.CR);
            packet.WriteShort(Partner.Stats.AS);
            packet.WriteShort(Partner.Stats.EV);
            packet.WriteShort(Partner.Stats.HT);
            packet.WriteShort(80);
        }

        public UpdateStats(Character Tamer, Digimon Partner, byte uByte)
        {
            packet.Type(1043);
            packet.WriteShort((short)Tamer.MaxHP);
            packet.WriteShort((short)Tamer.MaxDS);
            packet.WriteShort((short)Tamer.HP);
            packet.WriteShort((short)Tamer.DS);
            packet.WriteShort((short)Tamer.AT);
            packet.WriteShort((short)Tamer.DE);
            packet.WriteByte(uByte);
            packet.WriteShort((short)Tamer.MS);

            packet.WriteShort(Partner.Stats.MaxHP);
            packet.WriteShort(Partner.Stats.MaxDS);
            packet.WriteShort(Partner.Stats.HP);
            packet.WriteShort(Partner.Stats.DS);

            packet.WriteShort(Partner.Stats.Intimacy);

            packet.WriteShort(Partner.Stats.AT);
            packet.WriteShort(Partner.Stats.DE);
            packet.WriteShort(Partner.Stats.CR);
            packet.WriteShort(Partner.Stats.AS);
            packet.WriteShort(Partner.Stats.EV);
            packet.WriteShort(Partner.Stats.HT);
            packet.WriteShort(8000);
        }
    }
}
