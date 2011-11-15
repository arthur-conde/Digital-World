using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Digital_World.Helpers
{
    [Serializable]
    public class ItemList
    {
        private Item[] items;

        public ItemList(int max)
        {
            items = new Item[max];
            for (int i = 0; i < items.Length; i++)
                items[i] = new Item();
        }

        public Item this[int idx]
        {
            get
            {
                return items[idx];
            }
        }

        public bool Add(Item i)
        {
            int slot = GetOpenSlot();
            if (slot == -1)
                return false;
            else
            {
                items[slot] = i;
                return true;
            }
        }

        private int GetOpenSlot()
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i].ItemId == 0)
                    return i;
            return -1;
        }

        public byte[] Serialize()
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter f = new BinaryFormatter();
            f.Serialize(m, this);
            byte[] b =  m.ToArray();
            m.Close();
            return b;
        }

        public static ItemList Deserialize(byte[] buffer)
        {
            BinaryFormatter f = new BinaryFormatter();
            return (ItemList)f.Deserialize(new MemoryStream(buffer));
        }

        public byte[] ToArray()
        {
            MemoryStream m = new MemoryStream();
            byte[] buffer = null;
            for (int i = 0; i < items.Length; i++)
            {
                m.Write(items[i].ToArray(),0, 24);
            }
            buffer = m.ToArray();
            m.Close();
            return buffer;
        }
    }
}
