using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using Digital_World.Helpers;

namespace Digital_World.Packets.Game.Interface.Hatching
{
    public class Hatch:Packet
    {
        public Hatch(Digimon Mon, int slot)
        {
            packet.Type(1038);
            packet.WriteInt(slot);

            packet.WriteUInt(Mon.Model);
            packet.WriteString(Mon.Name);
            packet.WriteShort((short)Mon.Size);
            packet.WriteInt(Mon.EXP);

            packet.WriteShort((short)Mon.Level);
            packet.WriteBytes(Mon.Stats.ToArray());
            packet.WriteShort(600); //Unknown
            packet.WriteShort(10); //Unknown
            packet.WriteInt(Mon.Species);
            packet.WriteByte((byte)Mon.Forms.Count);

            for (int i = 0; i < Mon.Forms.Count; i++)
            {
                EvolvedForm form = Mon.Forms[i];
                form.uByte5 = 0x1d;
                form.uByte4 = 0x34;
                form.b128 = 129;
                form.b0 = 0x95;
                form.Skill1 = 8;
                form.Skill2 = 8;

                packet.WriteBytes(form.ToArray());
            }

            packet.WriteBytes(new byte[22]);
        }
    }
}
