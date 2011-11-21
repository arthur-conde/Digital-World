using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Helpers;
using System.IO;

namespace Digital_World.Database
{
    /// <summary>
    /// Parses MonsterList.bin
    /// </summary>
    public class MonsterDB
    {
        public static Dictionary<uint, MDBNPC> NPCs = new Dictionary<uint, MDBNPC>();
        public static Dictionary<uint, MDBDigimon> Digimon = new Dictionary<uint, MDBDigimon>();
        public static Dictionary<uint, MDBMap> Maps = new Dictionary<uint, MDBMap>();

        public static void Load(string fileName)
        {
            if (NPCs.Count > 0) return;
            using (Stream s = File.OpenRead(fileName))
            {
                using (BitReader read = new BitReader(s))
                {

                    int count = read.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        MDBNPC entry = new MDBNPC();
                        entry.i0 = read.ReadInt();
                        entry.Id = read.ReadUInt();

                        entry.Name = read.ReadZString(Encoding.Unicode, 128);
                        entry.Desc = read.ReadZString(Encoding.Unicode, 1024);
                        int counter = read.ReadInt();
                        entry.uInts = new int[counter];
                        for (int j = 0; j < counter; j++)
                            entry.uInts[j] = read.ReadInt();
                        NPCs.Add(entry.Id, entry);
                    }

                    count = read.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        MDBDigimon entry = new MDBDigimon();
                        entry.i1 = read.ReadInt();
                        entry.Id = read.ReadUInt();

                        entry.Name = read.ReadZString(Encoding.Unicode, 128);
                        entry.Desc = read.ReadZString(Encoding.Unicode, 1024);
                        int counter = read.ReadInt();
                        entry.Models = new int[counter];
                        for (int j = 0; j < counter; j++)
                            entry.Models[j] = read.ReadInt();
                        Digimon.Add(entry.Id, entry);
                    }

                    count = read.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        MDBMap entry = new MDBMap();
                        entry.i2 = read.ReadInt();
                        entry.Id = read.ReadUInt();

                        entry.Name = read.ReadZString(Encoding.Unicode, 128);
                        entry.Desc = read.ReadZString(Encoding.Unicode, 1024);
                        int counter = read.ReadInt();
                        entry.uInts = new int[counter];
                        for (int j = 0; j < counter; j++)
                            entry.uInts[j] = read.ReadInt();
                        Maps.Add(entry.Id, entry);
                    }
                }
            }
            Console.WriteLine("[MonsterDB] Loaded {0} NPCs, {1} Digimon, {2} Maps.", NPCs.Count, Digimon.Count, Maps.Count);
        }

        public static MDBDigimon GetDigimon(uint Id)
        {
            MDBDigimon mdbDigimon = null;
            if (Digimon.ContainsKey(Id))
                mdbDigimon = Digimon[Id];
            return mdbDigimon;
        }
    }

    /// <summary>
    /// NPC data loaded from MonsterList.bin
    /// </summary>
    public class MDBNPC
    {
        /// <summary>
        /// Possibly the type of the entry
        /// </summary>
        public int i0;
        /// <summary>
        /// Id of the entry
        /// </summary>
        public uint Id;
        /// <summary>
        /// The display name of the NPC. Unicode.
        /// </summary>
        public string Name;
        /// <summary>
        /// A description of the NPC. Unicode.
        /// </summary>
        public string Desc;
        /// <summary>
        /// An array of ints. Purpose unknown.
        /// </summary>
        public int[] uInts;
    }

    /// <summary>
    /// Digimon data loaded from MonsterList.bin
    /// </summary>
    public class MDBDigimon
    {
        /// <summary>
        /// Possibly the type of entry
        /// </summary>
        public int i1;
        /// <summary>
        /// Entry id
        /// </summary>
        public uint Id;
        public string Name;
        public string Desc;
        /// <summary>
        /// An array of models associated with this Digimon
        /// </summary>
        public int[] Models;
    }

    /// <summary>
    /// Map data loaded from MonsterList.bin
    /// </summary>
    public class MDBMap
    {
        public int i2;
        /// <summary>
        /// Entry Id
        /// </summary>
        public uint Id;
        /// <summary>
        /// Entry name
        /// </summary>
        public string Name;
        /// <summary>
        /// Description of the entry
        /// </summary>
        public string Desc;
        /// <summary>
        /// An array of ints. Purpose unknown.
        /// </summary>
        public int[] uInts;
    }
}
