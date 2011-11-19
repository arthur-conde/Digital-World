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
            //Send spawn packet all players
            Character Tamer = client.Tamer;

            this.Send(new SpawnPlayer(Tamer, Tamer.Partner));
            this.Send(new UpdateMS(Tamer.TamerHandle, Tamer.DigimonHandle, (short)Tamer.MS, Tamer.Partner.Stats.MS));

            Tamers.Add(client);
        }

        public void Leave(Client client)
        {
            //Send leave packet to all players
            this.Send(new DespawnPlayer(client.Tamer.TamerHandle, client.Tamer.DigimonHandle));
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
                    }

                    foreach (Client Client in ToRemove)
                    {
                        Tamers.Remove(Client);
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
            lock (Tamers)
            {
                foreach (Client Client in Tamers)
                {
                    Client.Send(Packet);
                }
            }
        }

        /// <summary>
        /// Send a packet to all clients in this map
        /// </summary>
        /// <param name="Packet"></param>
        public void Send(PacketReader Packet)
        {
            lock (Tamers)
            {
                foreach (Client Client in Tamers)
                {
                    Client.Send(Packet);
                }
            }
        }
    }
}
