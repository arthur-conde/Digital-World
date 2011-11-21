using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Helpers;
using System.IO;
using Digital_World.Entities;

namespace Digital_World.Database
{
    public class DigimonDB
    {
        public static Dictionary<int, DigimonData> Digimon = new Dictionary<int, DigimonData>();
        public static void Load(string fileName)
        {
            if (Digimon.Count > 0) return;
            using (Stream s = File.OpenRead(fileName))
            {
                using (BitReader read = new BitReader(s))
                {

                    int count = read.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        read.Seek(4 + i * 408);

                        DigimonData digiData = new DigimonData();
                        digiData.Species = read.ReadInt();
                        read.Skip(4);
                        digiData.DisplayName = read.ReadZString(Encoding.Unicode);

                        read.Seek(4 + 136 + i * 408);
                        digiData.Name = read.ReadZString(Encoding.ASCII);

                        read.Seek(4 + 228 + i * 408);
                        digiData.HP = read.ReadShort();
                        digiData.DS = read.ReadShort();

                        digiData.DE = read.ReadShort();
                        digiData.EV = read.ReadShort();
                        digiData.MS = read.ReadShort();
                        digiData.CR = read.ReadShort();
                        digiData.AT = read.ReadShort();
                        digiData.AS = read.ReadShort();
                        digiData.uStat = read.ReadShort();
                        digiData.HT = read.ReadShort();
                        digiData.uShort1 = read.ReadShort();

                        digiData.Skill1 = read.ReadShort();
                        digiData.Skill2 = read.ReadShort();
                        digiData.Skill3 = read.ReadShort();

                        Digimon.Add(digiData.Species, digiData);
                    }
                }
            }
            Console.WriteLine("[DigimonDB] Loaded {0} digimon.", Digimon.Count);
        }

        public static DigimonData GetDigimon(int Species)
        {
            if (Digimon.ContainsKey(Species))
                return Digimon[Species];
            else
                return null;
        }

        public static List<int> GetSpecies(string Name)
        {
            List<int> species = new List<int>();
            foreach (KeyValuePair<int, DigimonData> kvp in Digimon)
            {
                DigimonData dData = kvp.Value;
                if (dData.DisplayName.Contains(Name) || dData.Name.Contains(Name))
                    species.Add(dData.Species);
            }
            return species;
        }
    }

    public class DigimonData
    {
        public int Species;
        public string Name;
        public string DisplayName;
        public short HP, DS, DE, AS, MS, CR, AT, EV, uStat, HT, uShort1;
        public short Skill1, Skill2, Skill3;

        public DigimonData() { }

        public DigimonStats Stats(short Size)
        {
            //TODO: Get Stats
            return null;
        }

        public DigimonStats Default(Character Tamer, int Sync, int Size)
        {
            DigimonStats Stats = new DigimonStats();

            Stats.MaxHP = (short)(Math.Min(Math.Floor((decimal)HP * ((ushort)Size / 10000)) + Math.Floor((decimal)Tamer.HP * (Sync / 100)), short.MaxValue));
            Stats.HP = (short)(Math.Min(Math.Floor((decimal)HP * ((ushort)Size / 10000)) + Math.Floor((decimal)Tamer.HP * (Sync / 100)), short.MaxValue));
            Stats.MaxDS = (short)(Math.Min(Math.Floor((decimal)DS * ((ushort)Size / 10000)) + Math.Floor((decimal)Tamer.DS * (Sync / 100)), short.MaxValue));
            Stats.DS = (short)(Math.Max(Math.Floor((decimal)DS * ((ushort)Size / 10000)) + Math.Floor((decimal)Tamer.DS * (Sync / 100)), short.MaxValue));

            Stats.DE = (short)(Math.Min(Math.Floor((decimal)DE * ((ushort)Size / 10000)) + Math.Floor((decimal)Tamer.DE * (Sync / 100)), short.MaxValue));
            Stats.MS = (short)(Math.Min(Math.Floor((decimal)MS * ((ushort)Size / 10000)) + Math.Floor((decimal)Tamer.MS * (Sync / 100)), short.MaxValue));
            Stats.CR = (short)(Math.Min(Math.Floor((decimal)CR * ((ushort)Size / 10000)), short.MaxValue));
            Stats.AT = (short)(Math.Min(Math.Floor((decimal)AT * ((ushort)Size / 10000)) + Math.Floor((decimal)Tamer.AT * (Sync / 100)), short.MaxValue));
            Stats.EV = EV;
            Stats.uStat = uStat;
            Stats.HT = HT;

            Stats.Intimacy = (short)Sync;
            return Stats;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", DisplayName, Species);
        }
    }
}
