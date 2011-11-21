using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Digital_World.Helpers;

namespace Digital_World.Database
{
    public class TacticsDB
    {
        public static Dictionary<int, TDBTactic> Tactics = new Dictionary<int, TDBTactic>();

        public static void Load(string fName)
        {
            using (Stream s = File.OpenRead(fName))
            {
                using (BitReader read = new BitReader(s))
                {
                    int c = read.ReadInt();
                    for (int i = 0; i < c; i++)
                    {
                        TDBTactic t = new TDBTactic();
                        t.ItemId = read.ReadInt();
                        t.Species = read.ReadInt();
                        t.uInt1 = read.ReadInt();
                        t.Data = read.ReadInt();
                        t.uInt2 = read.ReadInt();
                        t.uInt3 = read.ReadInt();
                        t.uInt4 = read.ReadInt();

                        Tactics.Add(t.ItemId, t);
                    }
                    
                    //More data at the end. Digimon names and descriptions.
                }
            }
            Console.WriteLine("[TacticsDB] Loaded {0} entries.", Tactics.Count);
        }

        public static TDBTactic Get(int ItemId)
        {
            if (Tactics.ContainsKey(ItemId))
                return Tactics[ItemId];
            return null;
        }

        public static TDBTactic Get(short ItemId)
        {
            TDBTactic tactic = null;
            foreach (KeyValuePair<int, TDBTactic> DE in Tactics)
            {
                if (((short)DE.Key) == ItemId)
                {
                    tactic = DE.Value;
                    break;
                }
            }
            return tactic;
        }
    }

    public class TDBTactic
    {
        public int ItemId = 0;
        public int Species;
        public int uInt1;
        public int Data = 0;
        public int uInt2, uInt3, uInt4;
    }
}
