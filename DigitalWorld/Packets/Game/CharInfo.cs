using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using Digital_World.Database;

namespace Digital_World.Packets.Game
{
    public class CharInfo:Packet
    {
        private short hs = 0;
        public CharInfo(Character Tamer)
        {
            packet.Type(1003);
            packet.WriteInt(Tamer.Location.PosX); //X
            packet.WriteInt(Tamer.Location.PosY); //Y

            packet.WriteUInt(Tamer.intHandle);
            packet.WriteString(Tamer.Name);

            packet.WriteInt(0); //Unknown
            packet.WriteInt(0); //Unknown

            packet.WriteShort((short)Tamer.InventorySize);
            packet.WriteShort((short)Tamer.StorageSize);

            packet.WriteShort(0); //Unknown
            packet.WriteShort(0); //Unknown

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

            int max = Size(Tamer.DigimonList[0].DigiType) - (packet.Length + 42 - Tamer.Partner.Name.Length - Tamer.Name.Length);

            for (int i = 1; i < 3; i++)
            {
                packet.WriteByte((byte)(i < Tamer.DigimonList.Count ? i : 0));
                if (i < Tamer.DigimonList.Count)
                    Digimon(Tamer.DigimonList[i]);
            }

            //Padding?

            short[] s58 = new short[58];
            s58[24] = 128;
            s58[25] = 256;
            s58[26] = 1;

            int fillBytes = 0;
            if (Tamer.DigimonList.Count > 1)
                packet.WriteBytes(new byte[Tamer.DigimonList.Count - 1]);
            switch (Tamer.DigimonList[0].DigiType)
            {
                case 31001:
                    {
                        fillBytes = 230;
                        break;
                    }
                case 31002:
                    {
                        fillBytes = 172;
                        break;
                    }
                case 31003:
                    {
                        fillBytes = 346;
                        break;
                    }
            }
            for (int i = 0; i < fillBytes / 58; i++)
            {
                packet.WriteBytes(Struct58(s58));
            }

            packet.WriteBytes(new byte[48]);
            packet.WriteShort(128);
            packet.WriteShort(256);
            packet.WriteShort(1);
            packet.WriteShort(0);

            packet.WriteByte(99);
            packet.WriteInt(1); //Channel
            packet.WriteShort(128);
            packet.WriteBytes(new byte[194]);
            packet.WriteByte((byte)Tamer.DigimonList.Count);
            packet.WriteInt(0);
            packet.WriteInt(0);
            packet.WriteByte(99);
            packet.WriteBytes(new byte[130]);

            if (Tamer.DigimonList.Count > 1)
                packet.WriteByte(1);
            else
                packet.WriteByte(0);
            packet.WriteBytes(new byte[28]);

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
            packet.WriteShort(0); //Unknown
            packet.WriteShort(0); //Unknown

            packet.WriteShort((short)Mon.Level);
            packet.WriteBytes(Mon.Stats.ToArray());
            packet.WriteShort(600); //Unknown
            packet.WriteShort(10); //Unknown
            packet.WriteInt(Mon.DigiType);
            switch (Mon.DigiType)
            {
                case 31138: packet.WriteByte(5); break;
                case 31066: packet.WriteByte(5); break;
                case 31003: packet.WriteByte(8); break;
                case 31005: packet.WriteByte(8); break;
                case 31001: packet.WriteByte(9); break;
                case 31004: packet.WriteByte(9); break;
                case 31002: packet.WriteByte(11); break;
            }
            //packet.WriteBytes(new byte[290]); //Unknown

            //Skills?
            short[] stuff = new short[29];
                stuff[25] = 128;
                stuff[26] = 256;
                stuff[27] = 1;
            for (int i = 0; i < 5; i++)
            {
                packet.WriteBytes(Struct58(stuff));
            }
        }

        private byte[] Struct58(short[] stuff)
        {
            byte[] buffer = new byte[58];
            for (int i = 0; i < 29; i++)
            {
                byte[] info = BitConverter.GetBytes(stuff[i]);
                buffer[2 * i] = info[0];
                buffer[2 * i + 1] = info[1];
            }
            return buffer;
        }

        private int Size(int digiType)
        {
                int m_size = 0;
                switch (digiType)
                {
                    case 31001: m_size = 5384; break;
                    case 31002: m_size = 5500; break;
                    case 31003: m_size = 5326; break;
                    case 31004: m_size = 5442; break;
                }
                return m_size;
        }
    }
}
