using System;
using System.Collections.Generic;
using System.IO;
using Digital_World.Helpers;
using System.Text;

namespace Digital_World.Database
{
    public class MapDB
    {
        public static Dictionary<int, MapData> MapList = new Dictionary<int, MapData>();

        public static void Load(string fileName)
        {
            if (MapList.Count > 0) return;
            using (Stream s = File.OpenRead(fileName))
            {
                using (BitReader read = new BitReader(s))
                {

                    int count = read.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        read.Seek(4 + i * 672);
                        MapData map = new MapData();
                        map.MapID = read.ReadInt();

                        map.MapNumber = read.ReadInt();
                        read.Skip(4);
                        map.Name = read.ReadZString(Encoding.ASCII);
                        read.Skip(672 - (int)(336 + (read.InnerStream.BaseStream.Position - (672 * i)) - 4));

                        map.DisplayName = read.ReadZString(Encoding.Unicode);

                        MapList.Add(map.MapID, map);
                    }
                }
            }
            Console.WriteLine("[MapDB] Loaded {0} maps.", MapList.Count);
        }

        public static MapData GetMap(int mapId)
        {
            if (MapList.ContainsKey(mapId))
                return MapList[mapId];
            else
                return null;
        }
    }

    public class MapData
    {
        public int MapID;
        public int MapNumber;
        public string Name;
        public string DisplayName;

        public override string ToString()
        {
            return string.Format("{1} {0}",DisplayName, MapID);
        }
    }
}
