using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Digital_World.Database
{
    public class DigiCode
    {
        private static Random Rand = new Random();
        public static short MakeHandle(int DigiType)
        {
            //TEMPORARY SOLUTION.
            byte[] bHandle = new byte[2];
            Rand.NextBytes(bHandle);
            switch (DigiType)
            {
                case 31001:
                case 31003:
                    bHandle[1] = 0x90;
                    break;
                case 31002:
                case 31004:
                    bHandle[1] = 0x10;
                    break;
            }
            return BitConverter.ToInt16(bHandle, 0);
        }

        public static short GetModel(int DigiType)
        {
            byte[] bModel = new byte[] { 0, 0x3C };
            switch (DigiType)
            {
                case 31001:
                case 31002:
                    bModel[0] = 0x8C;
                    break;
                case 31003:
                case 31004:
                    bModel[0] = 0x8D;
                    break;
            }
            return BitConverter.ToInt16(bModel, 0);
        }


        public static byte GetB2(int DigiType)
        {
            byte b = 0;
            switch (DigiType)
            {
                case 31001:
                case 31003:
                    b = 0x90;
                    break;
                case 31002:
                case 31004:
                    b = 0x10;
                    break;
            }
            return b;
        }
    }
}
