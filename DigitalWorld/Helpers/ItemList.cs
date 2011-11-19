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

        public int Count
        {
            get
            {
                int i = 0;
                for (int j = 0; j < items.Length; j++)
                {
                    if (items[j].ItemId != 0)
                        i++;
                }
                return i;
            }
        }

        public int FindSlot(ushort itemId)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i].ItemId == itemId)
                    return i;
            return -1;
        }

        public Item Find(short itemId)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i].ItemId == itemId)
                    return items[i];
            return null;
        }

        public int EquipSlot(short slotId)
        {
            int slot = 0;
            switch (slotId)
            {
                case 5000:
                    {
                        slot= 21;
                        break;
                    }
                case 1000:
                case 1001:
                case 1002:
                case 1003:
                case 1004:
                case 1005:
                case 1006:
                    slot = slotId - 1000;
                    break;
                case 4000:
                    slot = 9;
                    break;
                default:
                    {
                        break;
                    }
            }
            return slot;
        }

        /// <summary>
        /// Gets or sets the item at idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Item this[int idx]
        {
            get
            {
                return items[idx];
            }
            set
            {
                items[idx] = value;
            }
        }

        /// <summary>
        /// Adds item i to an open slot.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int Add(Item i)
        {
            int slot = GetOpenSlot();
            if (slot == -1)
                return -1;
            else
            {
                items[slot] = i;
                return slot;
            }
        }

        public bool Remove(Item i)
        {
            int slot = FindSlot(i.ItemId);
            if (slot != -1)
            {
                items[slot] = new Item();
                return true;
            }
            else
                return false;
        }

        public bool Remove(int Slot)
        {
            if (Slot != -1)
            {
                items[Slot] = new Item();
                return true;
            }
            else
                return false;
        }

        public bool Contains(ushort itemId)
        {
            if (FindSlot(itemId) == -1)
                return false;
            return true;
        }

        public bool Contains(Item i)
        {
            return Contains(i.ItemId);
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
