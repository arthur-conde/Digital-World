using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using Digital_World.Database;

namespace Digital_World.Systems
{
    /// <summary>
    /// Provides framework for moving around the world and its maps
    /// </summary>
    public partial class Yggdrasil
    {
        public static Dictionary<int, GameMap> Maps = new Dictionary<int, GameMap>();

        /// <summary>
        /// Initialize GameMaps
        /// </summary>
        public void World()
        {
            foreach (KeyValuePair<int, MapData> kvp in MapDB.MapList)
            {
                MapData Map = kvp.Value;
                GameMap gMap = new GameMap(Map.MapID);

                Maps.Add(gMap.MapId, gMap);
            }
        }
    }
}
