using System;
using System.IO;
using Digital_World.Database;

namespace Digital_World.Entities
{
    [Serializable]
    public class Item
    {
        private static Random r = new Random();
        public uint Handle = 0;

        public ushort ItemId = 0;
        public ushort Modifier = 0;
        public short Unknown = 0;
        public short Unknown1 = 0;
        public short[] Attributes = new short[2];
        public short Unknown2 = 0;
        public short Unknown3 = 0;
        public short Unknown4 = 0;
        public short Unknown5 = 0;
        public short Unknown6 = 0;
        public uint time_t = 0;

        public Item()
        {
        }

        public Item(short itemId)
        {
            byte[] b = new byte[4];
            r.NextBytes(b);
            Handle = BitConverter.ToUInt32(b, 0);
            ItemId = (ushort)itemId;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Item))
            {
                Item o = (Item)obj;
                return o.Handle == this.Handle;
            }
            else
                return base.Equals(obj);
        }

        public static bool operator ==(Item i1, Item i2)
        {
            return i1.Equals(i2);
        }

        public static bool operator !=(Item i1, Item i2)
        {
            return i1.Equals(i2);
        }

        public byte[] ToArray()
        {
            byte[] buffer = new byte[0];
            using (MemoryStream m = new MemoryStream())
            {
                m.Write(BitConverter.GetBytes(ItemId), 0, 2);
                m.Write(BitConverter.GetBytes(Modifier), 0, 2);
                m.Write(BitConverter.GetBytes(Unknown), 0, 2);
                m.Write(BitConverter.GetBytes(Unknown1), 0, 2);
                m.Write(BitConverter.GetBytes(Attributes[0]), 0, 2);
                m.Write(BitConverter.GetBytes(Attributes[1]), 0, 2);
                m.Write(BitConverter.GetBytes(Unknown2), 0, 2);
                m.Write(BitConverter.GetBytes(Unknown3), 0, 2);
                m.Write(BitConverter.GetBytes(Unknown4), 0, 2);
                m.Write(BitConverter.GetBytes(Unknown5), 0, 2);
                m.Write(BitConverter.GetBytes(time_t), 0, 4);

                buffer = m.ToArray();
            }
            return buffer;
        }

        /// <summary>
        /// Full ID of the item
        /// </summary>
        public int ID
        {
            get
            {
                return (Modifier << 16) + ItemId;
            }
            set
            {
                Modifier = (ushort)(value >> 16);
                ItemId = (ushort)(value & 0xffff);
            }
        }

        public int Amount
        {
            get
            {
                return ((ItemData.Mod ^ Modifier) / 2);
            }
            set
            {
                Modifier = (ushort)((value * 2) ^ ItemData.Mod);
            }
        }

        public int BaseID
        {
            get
            {
                return (1<< 16) + ItemId;
            }
        }

        public int GetID(int mod)
        {
            return (ushort)((mod << 16) + ItemId);
        }

        /// <summary>
        /// Property linked to item database
        /// </summary>
        public ItemData ItemData
        {
            get
            {
                ItemData data = ItemDB.GetItem(this.ID);
                if (data == null)
                {
                    data = ItemDB.GetItem(this.ItemId);
                }
                return data;
            }
        }

        /// <summary>
        /// Maximum amount allowed per inventory slot
        /// </summary>
        public int Max
        {
            get
            {
                return ItemData.Stack;
            }
        }
    }
}
