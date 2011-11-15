using System;
using System.IO;

namespace Digital_World.Entities
{
    [Serializable]
    public class Item
    {
        private static Random r = new Random();
        public uint Handle = 0;

        public short ItemId = 0;
        public short Modifier = 0;
        public short Count = 0;
        public short Unknown1 = 0;
        public short[] Attributes = new short[2];
        public short Unknown2 = 0;
        public short Unknown3 = 0;
        public short Unknown4 = 0;
        public short Unknown5 = 0;
        public short Unknown6 = 0;
        public short Unknown7 = 0;

        public Item() { }

        public Item(short itemId)
        {
            byte[] b = new byte[4];
            r.NextBytes(b);
            Handle = BitConverter.ToUInt32(b, 0);
            ItemId = itemId;
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
            MemoryStream buffer = new MemoryStream();

            buffer.Write(BitConverter.GetBytes(ItemId), 0, 2);
            buffer.Write(BitConverter.GetBytes(Modifier), 0, 2);
            buffer.Write(BitConverter.GetBytes(Count), 0, 2);
            buffer.Write(BitConverter.GetBytes(Unknown1), 0, 2);
            buffer.Write(BitConverter.GetBytes(Attributes[0]), 0, 2);
            buffer.Write(BitConverter.GetBytes(Attributes[1]), 0, 2);
            buffer.Write(BitConverter.GetBytes(Unknown2), 0, 2);
            buffer.Write(BitConverter.GetBytes(Unknown3), 0, 2);
            buffer.Write(BitConverter.GetBytes(Unknown4), 0, 2);
            buffer.Write(BitConverter.GetBytes(Unknown5), 0, 2);
            buffer.Write(BitConverter.GetBytes(Unknown6), 0, 2);
            buffer.Write(BitConverter.GetBytes(Unknown7), 0, 2);

            return buffer.ToArray();
        }
    }
}
