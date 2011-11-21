using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Helpers;
using System.IO;

namespace Digital_World.Database
{
    public class PortalDB
    {
        public static List<PortalCluster> PortalList = new List<PortalCluster>();

        public static void Load(string fileName)
        {
            if (PortalList.Count > 0) return;
            using (Stream s = File.OpenRead(fileName))
            {
                using (BitReader read = new BitReader(s))
                {

                    int count = read.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        PortalCluster Cluster = new PortalCluster();
                        Cluster.Count = read.ReadInt();

                        for (int h = 0; h < Cluster.Count; h++)
                        {
                            Portal portal = new Portal();
                            portal.PortalId = read.ReadInt();

                            for (int j = 0; j < portal.uInts1.Length; j++)
                                portal.uInts1[j] = read.ReadInt();

                            portal.MapId = read.ReadInt();

                            for (int j = 0; j < portal.uInts2.Length; j++)
                                portal.uInts2[j] = read.ReadInt();

                            Cluster.Add(portal);
                        }
                        PortalList.Add(Cluster);
                    }
                }
            }
            Console.WriteLine("[PortalDB] Loaded {0} portals", PortalList.Count);
        }

        public static Portal GetPortal(int portalId)
        {
            PortalCluster Cluster =  PortalList.Find(delegate(PortalCluster lCluster)
            {
                if (lCluster.PortalList.ContainsKey(portalId))
                    return true;
                return false;
            });
            return Cluster[portalId];
        }
    }

    public class PortalCluster
    {
        public int Count;
        public Dictionary<int, Portal> PortalList = new Dictionary<int, Portal>();

        public void Add(Portal portal)
        {
            PortalList.Add(portal.PortalId, portal);
        }

        public Portal this[int portalId]
        {
            get
            {
                if (PortalList.ContainsKey(portalId))
                    return PortalList[portalId];
                else
                    return null;
            }
        }
    }

    public class Portal
    {
        public int PortalId;
        public int MapId;
        public int[] uInts1;
        public int[] uInts2;

        public Portal()
        {
            uInts1 = new int[4];
            uInts2 = new int[8];
        }
    }
}
