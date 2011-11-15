using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRCBreaker
{
    public class CRC16CCITT
    {

        public enum InitialCrcValue { Zeros, NonZero1 = 0xffff, NonZero2 = 0x1D0F }

        public  ushort poly = 4129;
        ushort[] table = new ushort[256];
        public ushort initialValue = 0;

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = this.initialValue;
            for (int i = 0; i < bytes.Length; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public void GenTable(ushort initialValue, ushort polynomial)
        {
            ushort temp, a;
            for (int i = 0; i < table.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                    {
                        temp = (ushort)((temp << 1) ^ poly);
                    }
                    else
                    {
                        temp <<= 1;
                    }
                    a <<= 1;
                }
                table[i] = temp;
            }
        }

        public CRC16CCITT(InitialCrcValue initialValue)
        {
            this.initialValue = (ushort)initialValue;
            GenTable((ushort)initialValue, (ushort)4219);
        }

        public CRC16CCITT(ushort initialValue, ushort polynomial)
        {
            this.initialValue = (ushort)initialValue;
            this.poly = polynomial;
            GenTable(initialValue, polynomial);
        }
    }
}