using System;
using System.Collections.Generic;
using System.IO;
using Digital_World.Helpers;

namespace Digital_World.Database
{
    /// <summary>
    /// Parse DigimonEvolve.bin
    /// </summary>
    public static class EvolutionDB
    {
        public static Dictionary<int, Evolution> EvolutionList = new Dictionary<int, Evolution>();

        public static void Load(string fileName)
        {
            if (EvolutionList.Count > 0) return;
            using (Stream s = File.OpenRead(fileName))
            {
                using (BitReader read = new BitReader(s))
                {
                    int count = read.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        Evolution evo = new Evolution();
                        evo.digiId = read.ReadInt();
                        evo.Digivolutions = read.ReadInt();

                        for (int j = 0; j < evo.Digivolutions; j++)
                        {
                            EvolutionLine line = new EvolutionLine();
                            line.digiId = read.ReadInt();
                            line.iLevel = read.ReadShort();
                            line.uShort1 = read.ReadShort();

                            line.Line = new Dictionary<int, int>();
                            for (int k = 0; k < 6; k++)
                            {
                                int key = read.ReadInt();
                                int val = read.ReadInt();
                                if (line.Line.ContainsKey(key) && key.Equals(0))
                                    continue;
                                line.Line.Add(key, val);
                            }

                            line.uInts1 = new int[11];
                            for (int k = 0; k < 11; k++)
                            {
                                line.uInts1[k] = read.ReadInt();
                            }

                            line.uStats = new int[8];
                            for (int k = 0; k < 8; k++)
                            {
                                line.uStats[k] = read.ReadInt();
                            }

                            line.uInts2 = new int[5];
                            for (int k = 0; k < 5; k++)
                            {
                                line.uInts2[k] = read.ReadInt();
                            }

                            line.uShorts1 = new short[4];
                            for (int k = 0; k < 4; k++)
                            {
                                line.uShorts1[k] = read.ReadShort();
                            }

                            line.uInts3 = new int[28];
                            for (int k = 0; k < 28; k++)
                            {
                                line.uInts3[k] = read.ReadInt();
                            }

                            evo.Evolutions.Add(line);
                        }
                        EvolutionList.Add(evo.digiId, evo);
                    }
                }
            }
            Console.WriteLine("[EvolutionDB] Loaded {0} Digimon evolutions",EvolutionList.Count);
        }

        public static EvolutionLine GetLine(int digiType, int evolvedType)
        {
            Evolution evo = EvolutionDB.EvolutionList[digiType];
            EvolutionLine line = evo.Evolutions.Find(
                delegate(EvolutionLine evoline)
                {
                    return evoline.digiId == evolvedType;
                });
            return line;
        }
    }

    public class Evolution
    {
        public int digiId = 0;
        public int Digivolutions = 0;
        public List<EvolutionLine> Evolutions = new List<EvolutionLine>();

        public Evolution() { }
    }

    public class EvolutionLine
    {
        public enum EvoLevel
        {
            Child = 1,
            Adult = 2,
            Perfect = 3,
            Ultimate = 4,
            Burst = 5
        };

        public int digiId = 0;
        public int iLevel = 0;
        public EvoLevel Level
        {
            get
            {
                return (EvoLevel)iLevel;
            }
        }
        public short uShort1 = 0;
        public Dictionary<int, int> Line = new Dictionary<int, int>();

        public int[] uInts1 = new int[11];
        public int[] uStats = new int[8];
        public int[] uInts2 = new int[5];
        public short[] uShorts1 = new short[4];
        public int[] uInts3 = new int[28];
    }
}
