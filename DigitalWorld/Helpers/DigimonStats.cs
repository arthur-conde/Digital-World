using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Digital_World.Helpers
{
    public class DigimonStats
    {
        public short MaxHP = 0;
        public short MaxDS = 0;
        public short HP = 0;
        public short DS = 0;
        /// <summary>
        /// Attack Stat
        /// </summary>
        public short AT = 0;
        /// <summary>
        /// Defense stat
        /// </summary>
        public short DE = 0;
        /// <summary>
        /// Hit Rate
        /// </summary>
        public short HT = 0;
        /// <summary>
        /// Evade
        /// </summary>
        public short EV = 0;
        /// <summary>
        /// Critical rate
        /// </summary>
        public short CR = 0;
        /// <summary>
        /// Attack Speed
        /// </summary>
        public short AS = 0;
        /// <summary>
        /// Movement Speed?
        /// </summary>
        public short MS = 0;

        public short Intimacy = 0;

        public DigimonStats() { }

        public byte[] ToArray()
        {
            byte[] buffer = null;
            MemoryStream m = new MemoryStream();

            m.Write(BitConverter.GetBytes(MaxHP), 0, 2);
            m.Write(BitConverter.GetBytes(MaxDS), 0, 2);
            m.Write(BitConverter.GetBytes(DE), 0, 2);
            m.Write(BitConverter.GetBytes(AT), 0, 2);
            m.Write(BitConverter.GetBytes(HP), 0, 2);
            m.Write(BitConverter.GetBytes(DS), 0, 2);
            m.Write(BitConverter.GetBytes(Intimacy), 0, 2);
            m.Write(BitConverter.GetBytes(HT), 0, 2);
            m.Write(BitConverter.GetBytes(EV), 0, 2);
            m.Write(BitConverter.GetBytes(CR), 0, 2);
            m.Write(BitConverter.GetBytes(MS), 0, 2);
            m.Write(BitConverter.GetBytes(AS), 0, 2);

            buffer = m.ToArray();
            m.Close();

            return buffer;
        }
    }
}
