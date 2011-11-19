using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Helpers;
using System.IO;

namespace Digital_World.Database
{
    public class ItemDB
    {
        public static Dictionary<int, ItemData> Items = new Dictionary<int, ItemData>();
        public static void Load(string fileName)
        {
            if (Items.Count > 0) return;
            BitReader read = new BitReader(File.OpenRead(fileName));

            int count = read.ReadInt();
            for (int i = 0; i < count; i++)
            {
                read.Seek(4 + i * 940);

                ItemData iData = new ItemData();
                iData.ItemId = read.ReadInt();
                iData.Name = read.ReadZString(Encoding.Unicode);

                read.Seek(4 + 132 + i * 940);
                iData.uInt1 = read.ReadInt();
                iData.Desc = read.ReadZString(Encoding.Unicode);

                read.Seek(4 + 520 + i * 940);
                iData.Icon = read.ReadZString(Encoding.ASCII);

                read.Seek(4 + 584 + i * 940);
                iData.ItemType = read.ReadShort();
                iData.Kind = read.ReadZString(Encoding.Unicode);

                read.Seek(4 + 714 + i * 940);

                for (int j = 0; j < iData.uShorts1.Length; j++)
                    iData.uShorts1[j] = read.ReadShort();
                iData.Stack = read.ReadShort();
                for (int j = 0; j < iData.uShorts2.Length; j++)
                    iData.uShorts2[j] = read.ReadShort();

                iData.Buy = read.ReadInt();
                iData.Sell = read.ReadInt();

                Items.Add(iData.ItemId, iData);
            }
            Console.WriteLine("[ItemDB] Loaded {0} items.", Items.Count);
        }

        public static ItemData GetItem(int fullId)
        {
            ItemData iData = null;
            foreach (KeyValuePair<int, ItemData> kvp in Items)
            {
                if (kvp.Value.ItemId == fullId)
                {
                    iData = kvp.Value;
                    break;
                }
            }
            return iData;
        }
    }

    public class ItemData
    {
        public int ItemId;
        public string Name;
        public int uInt1;
        public string Desc;
        public string Icon;
        public short ItemType;
        public string Kind;
        public short Stack;
        public short[] uShorts1; //8
        public short[] uShorts2; //12
        public int Buy, Sell;

        public ItemData()
        {
            uShorts1 = new short[8];
            uShorts2 = new short[12];
        }
    }
}
