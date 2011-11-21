using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Digital_World.Packets;
using Digital_World.Packets.Game;
using Digital_World.Packets.Game.Interface;

namespace Digital_World.Entities
{
    public class GameMap
    {
        public const int CHAT_DISTANCE = 300;

        /// <summary>
        /// Map Id
        /// </summary>
        public int MapId = 0;
        public List<Client> Tamers = new List<Client>();
        private Thread tMonitor = null;

        public GameMap(int MapId)
        {
            //Call Monster AI here.
            this.MapId = MapId;

            tMonitor = new Thread(new ThreadStart(Monitor));
            tMonitor.IsBackground = true;
            tMonitor.Start();
        }

        public void Enter(Client client)
        {
            //Add to Tamers list
            if (!Contains(client))
                Tamers.Add(client);
        }

        public void Spawn(Client client)
        {
            Character Tamer = client.Tamer;

            foreach (Client other in Tamers)
            {
                if (other == client) continue;
                //client.Send(new SpawnPlayer(other.Tamer, other.Tamer.Partner));
                other.Send(new SpawnPlayer(Tamer));
                other.Send(new SpawnPlayer(Tamer.Partner, Tamer.TamerHandle));

                client.Send(new SpawnPlayer(other.Tamer));
                client.Send(new SpawnPlayer(other.Tamer.Partner, other.Tamer.TamerHandle));
                //client.Send(new UpdateMS(other.Tamer.TamerHandle, other.Tamer.DigimonHandle, (short)other.Tamer.MS, other.Tamer.Partner.Stats.MS));
            }
        }

        public void Leave(Client client)
        {
            //Send leave packet to all players
            this.Send(new DespawnPlayer(client.Tamer.TamerHandle, client.Tamer.DigimonHandle), client);
            Tamers.Remove(client);
        }

        private void Monitor()
        {
            while (true)
            {
                lock (Tamers)
                {
                    List<Client> ToRemove = new List<Client>();
                    foreach (Client Client in Tamers)
                    {
                        if (Client.Tamer.Location.Map != MapId || !Client.m_socket.Connected)
                        {
                            ToRemove.Add(Client);
                        }
                        else
                        {
                            Character Tamer = Client.Tamer;
                            Digimon Partner = Tamer.Partner;

                            for (int i = 0; i < Tamer.DigimonList.Length; i++)
                            {
                                //Check if in battle?
                                if (Tamer.DigimonList[i] == null) continue;
                                Digimon digimon = Tamer.DigimonList[i];

                                //Console.WriteLine("Recovering {0}...", digimon.Name);
                                digimon.Stats.Recover();
                            }

                            try
                            {
                                Client.Send(new Packets.Game.Status(Tamer.DigimonHandle, Partner.Stats));
                            }
                            catch
                            {
                                ToRemove.Add(Client);
                            }
                        }
                    }

                    foreach (Client Client in ToRemove)
                    {
                        Tamers.Remove(Client);
                        this.Send(new DespawnPlayer(Client.Tamer.TamerHandle, Client.Tamer.DigimonHandle));
                    }   
                }

                Thread.Sleep(30 * 1000); //Sleep 30s
            }
        }

        /// <summary>
        /// Send a packet to all clients in this map
        /// </summary>
        /// <param name="Packet"></param>
        public void Send(IPacket Packet)
        {
            List<Client> ToRemove = new List<Client>();

            Client[] Clients = Tamers.ToArray();
            for (int i = 0; i < Clients.Length; i++)
            {
                try { Clients[i].Send(Packet); }
                catch { ToRemove.Add(Clients[i]); }
            }

            lock (Tamers)
            {
                foreach (Client Client in ToRemove)
                {
                    Tamers.Remove(Client);
                    this.Send(new DespawnPlayer(Client.Tamer.TamerHandle, Client.Tamer.DigimonHandle));
                }
            }
        }

        /// <summary>
        /// Send a packet to all clients except origin
        /// </summary>
        /// <param name="Packet"></param>
        /// <param name="origin"></param>
        public void Send(IPacket Packet, Client origin)
        {
            List<Client> ToRemove = new List<Client>();

            Client[] Clients = Tamers.ToArray();
            for (int i = 0; i < Clients.Length; i++)
            {
                if (Clients[i] == origin) continue;
                try { Clients[i].Send(Packet); }
                catch { ToRemove.Add(Clients[i]); }
            }

            lock (Tamers)
            {
                foreach (Client Client in ToRemove)
                {
                    Tamers.Remove(Client);
                    this.Send(new DespawnPlayer(Client.Tamer.TamerHandle, Client.Tamer.DigimonHandle));
                }
            }
        }

        /// <summary>
        /// Extending the Contains method
        /// </summary>
        /// <param name="list"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool Contains(Client client)
        {
            bool contains = false;
            foreach (Client c1 in Tamers)
                if (client.Equals(c1))
                {
                    contains = true;
                    break;
                }
            return contains;
        }

        public Client Find(string p)
        {
            Client c = null;
            foreach (Client Tamer in Tamers)
            {
                if (Tamer.Tamer.Name.Contains(p))
                {
                    c = Tamer;
                    break;
                }
            }
            return c;
        }
    }
}
