using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Digital_World.Helpers
{
    [Serializable]
    public class EvolvedForms
    {
        private EvolvedForm[] m_coll;

        public EvolvedForms()
        {
            m_coll = new EvolvedForm[4];
            for (int i = 0; i < m_coll.Length; i++)
                m_coll[i] = new EvolvedForm();
        }

        public EvolvedForms(int count)
        {
            m_coll = new EvolvedForm[count];
            for (int i = 0; i < m_coll.Length; i++)
                m_coll[i] = new EvolvedForm();
        }

        public byte[] Serialize()
        {
            byte[] buffer = new byte[0];
            using (MemoryStream m = new MemoryStream())
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(m, this);

                buffer = m.ToArray();
            }
            return buffer;
        }

        public int Count
        {
            get
            {
                return m_coll.Length;
            }
        }

        public EvolvedForm this[int idx]
        {
            get
            {
                return m_coll[idx];
            }
            set
            {
                m_coll[idx] = value;
            }
        }
    }

    [Serializable]
    public class EvolvedForm
    {
        public short[] uShorts1;
        public byte uByte4, uByte5, b128, b0, uByte3, Skill1, Skill2, Skill3;
        public short uShort1;

        public EvolvedForm()
        {
            uShort1 = 0;
            uShorts1 = new short[24];
            b128 = 128;
            Skill1 = 1;
            Skill2 = 1;
        }

        public EvolvedForm(byte[] Unknowns, byte[] Skills)
        {
            b128 = Unknowns[0];
            b0 = Unknowns[1];
            uByte3 = Unknowns[2];

            Skill1 = Skills[0];
            Skill2 = Skills[1];
            Skill3 = Skills[2];
        }

        public byte[] ToArray()
        {
            byte[] buffer = new byte[0];
            using (MemoryStream m = new MemoryStream())
            {
                for (int i = 0; i < uShorts1.Length; i++)
                {
                    m.Write(BitConverter.GetBytes(uShorts1[i]), 0, 2);
                }
                m.WriteByte(uByte4);
                m.WriteByte(uByte5);
                m.WriteByte(b128);
                m.WriteByte(b0);
                m.WriteByte(uByte3);

                m.WriteByte(Skill1);
                m.WriteByte(Skill2);
                m.WriteByte(Skill3);

                m.Write(BitConverter.GetBytes(uShort1), 0, 2);

                buffer = m.ToArray();
            }
            return buffer;
        }
    }
}
