using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Helpers;

namespace Digital_World.Entities
{
    public class Digimon
    {
        public uint DigiId = 0;
        public int intHandle = 0;
        public int CharacterId = 0;
        public string Name = "Genericmon";
        public int Level = 1;
        public int DigiType = 31001;
        public int Scale = 3; //Unknown
        public short Size = 10000;
        public int EXP = 0;

        public DigimonStats Stats = new DigimonStats();


        public Digimon() { }

        public Digimon(string digiName, int digiModel)
        {
            Name = digiName;
            DigiType = digiModel;
        }

        public override string ToString()
        {
            return string.Format("{0}\nLv {1} {2}", Name, Level, DigiType);
        }

        public int ProperModel()
        {
            int pModel = 0x3C8C90;
            int bId = 31001;

            if (DigiType > 40000)
            {
                pModel = 0x4C8C90;
                bId += 10000;
            }
            else if (DigiType > 50000)
            {
                pModel = 0x5C8C90;
                bId += 20000;
            }
            else if (DigiType > 60000)
            {
                pModel = 0x6C8C90;
                bId += 30000;
            }
            else if (DigiType > 70000)
            {
                pModel = 0x7C8C90;
                bId += 40000;
            }

            pModel += ((DigiType - bId) * 128);

            /*switch (DigiType)
            {
                case 31001:
                    pModel = -29552;
                    break;
                case 31002:
                    pModel = -29424;
                    break;
                case 31003:
                    pModel = -29296;
                    break;
            }*/
            return (pModel << 8);
        }

       //public short hMap = 0;
        public short MapHandle
        {
            get
            {
                byte[] b = new byte[] { (byte)((intHandle >> 24) & 0xFF), 0x10 };
                return BitConverter.ToInt16(b, 0);
            }
        }
    }
}
