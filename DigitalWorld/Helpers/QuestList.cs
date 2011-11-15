using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Digital_World.Helpers
{
    [Serializable]
    public class QuestList
    {
        private Quest[] list = new Quest[20];
        private static BinaryFormatter f = new BinaryFormatter();

        public QuestList()
        {
            list = new Quest[20];
            for (int i = 0; i < 20; i++)
                list[i] = new Quest();
        }

        private int FindSlot()
        {
            int slot = -1;
            for (int i = 0; i < 20; i++)
            {
                if (list[i].QuestId >= 0)
                {
                    slot = i;
                    break;
                }
            }
            return slot;
        }

        public bool Add(Quest q)
        {
            int slot = FindSlot();
            if (slot == -1) return false;
            list[slot] = q;
            return true;
        }

        public byte[] Serialize()
        {
            MemoryStream m = new MemoryStream();
            f.Serialize(m, this);
            byte[] buffer = m.ToArray();
            m.Close();
            return buffer;
        }

        public static QuestList Deserialize(byte[] buffer)
        {
            return (QuestList)f.Deserialize(new MemoryStream(buffer));
        }

        public byte[] ToArray()
        {
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < 20; i++)
            {
                m.Write(list[i].ToArray(), 0, 7);
            }

            byte[] buffer = m.ToArray();
            m.Close();

            return buffer;
        }
    }
}
