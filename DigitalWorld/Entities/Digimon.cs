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
        /// <summary>
        /// The ID of the Digimon in the database
        /// </summary>
        public uint DigiId = 0;
        public uint Model = 0;
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

        public uint ProperModel()
        {
            uint pModel = 0;
            pModel += (uint)((CurrentForm * 128) + 16);
            return (pModel << 8);
        }

        /// <summary>
        /// The handle to the Digimon entity
        /// </summary>
        public short Handle
        {
            get
            {
                byte[] b = new byte[] { (byte)((Model >> 32) & 0xFF), 0x10 };
                return BitConverter.ToInt16(b, 0);
            }
        }

        public byte byteHandle
        {
            get
            {
                return (byte)((Model >> 32) & 0xFF);
            }
        }
    }
}
