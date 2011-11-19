using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using Digital_World.Database;
using Digital_World.Helpers;

namespace Digital_World.Packets.Game
{
    public class CharInfo:Packet
    {
        public CharInfo(Character Tamer)
        {
            packet.Type(1003);
            packet.WriteInt(Tamer.Location.PosX); //X
            packet.WriteInt(Tamer.Location.PosY); //Y

            packet.WriteUInt(Tamer.intHandle);
            packet.WriteString(Tamer.Name);

            packet.WriteInt(Tamer.Money);
            packet.WriteInt(0);

            packet.WriteShort((short)Tamer.InventorySize);
            packet.WriteShort((short)Tamer.StorageSize);

            packet.WriteInt(Tamer.EXP);

            packet.WriteShort((short)Tamer.Level);
            packet.WriteShort((short)Tamer.MaxHP);
            packet.WriteShort((short)Tamer.MaxDS);
            packet.WriteShort((short)Tamer.HP);
            packet.WriteShort((short)Tamer.DS);
            packet.WriteShort((short)Tamer.Fatigue);
            packet.WriteShort((short)Tamer.AT);
            packet.WriteShort((short)Tamer.DE);
            packet.WriteShort((short)Tamer.MS);

            packet.WriteBytes(Tamer.Equipment.ToArray());
            packet.WriteBytes(Tamer.Inventory.ToArray());
            packet.WriteBytes(Tamer.Storage.ToArray());

            packet.WriteBytes(new byte[384]); //Unknown.

            packet.WriteBytes(Tamer.Quests.ToArray());
            packet.WriteInt(0);
            packet.WriteInt(0);

            Digimon(Tamer.Partner);

            int Mons = 1;
            for (int i = 1; i < 3; i++)
            {
                if (Tamer.DigimonList[i] != null)
                {
                    Mons++;
                    packet.WriteByte((byte)i);
                    Digimon(Tamer.DigimonList[i]);
                }

            }
            
            packet.WriteByte(99);
            packet.WriteInt(1); //Channel
                packet.WriteShort(128);
                packet.WriteBytes(new byte[194]);
            packet.WriteByte((byte)Mons);
            packet.WriteInt(0);
            packet.WriteInt(0);
            packet.WriteByte(99);
            packet.WriteBytes(new byte[130]);

            packet.WriteByte(0);
            packet.WriteBytes(new byte[20]);

            packet.WriteInt(1);
            packet.WriteInt(0);

            packet.WriteShort(18000);
            packet.WriteShort(0);
            packet.WriteShort(7200);
            packet.WriteShort(0);
            packet.WriteShort(1);
            packet.WriteShort(0);
            packet.WriteShort(0);
            packet.WriteShort(18000);
            packet.WriteShort(0);
        }

        private void Digimon(Digimon Mon)
        {
            packet.WriteInt(Mon.intHandle);

            packet.WriteString(Mon.Name);
            packet.WriteByte((byte)Mon.Scale);
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
        }
    }
}
