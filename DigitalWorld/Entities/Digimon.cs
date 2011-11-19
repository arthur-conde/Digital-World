using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Helpers;
using System.IO;

namespace Digital_World.Entities
{
    public class Digimon
    {
        public uint DigiId = 0;
        public int intHandle = 0;
        public int CharacterId = 0;
        public string Name = "Genericmon";
        public int Level = 1;
        public int Species = 31001;
        public int CurrentForm = 0;
        public int Scale = 3; //Unknown
        public short Size = 10000;
        public int EXP = 0;


        public Position Location = new Position();
        public DigimonStats Stats = new DigimonStats();
        public EvolvedForms Forms = new EvolvedForms();

        public Digimon() { }

        public Digimon(string digiName, int digiModel)
        {
            Name = digiName;
            Species = digiModel;
        }

        public override string ToString()
        {
            return string.Format("{0}\nLv {1} {2}", Name, Level, Species);
        }

        public int ProperModel()
        {
            int pModel = 0x3C8C90;
            int bId = 31001;

            pModel += ((CurrentForm - bId) * 128);
            return (pModel << 8);
        }

        public short MapHandle
        {
            get
            {
                byte[] b = new byte[] { (byte)((intHandle >> 32) & 0xFF), 0x10 };
                return BitConverter.ToInt16(b, 0);
            }
        }       
    }
}
