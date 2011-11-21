using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Network;
using System.Threading;
using System.Collections.ObjectModel;
using Digital_World.Database;
using Digital_World.Helpers;

namespace Digital_World.Systems
{
    public partial class Yggdrasil
    {
        private SocketWrapper server = null;
        private Thread tMain = null;
        private Settings Opt = null;


        public ObservableCollection<Client> Clients = new ObservableCollection<Client>();

        public Yggdrasil()
        {
            Opt = Settings.Deserialize();

            server = new SocketWrapper();
            server.OnAccept += new SocketWrapper.dlgAccept(server_OnAccept);
            server.OnClose += new SocketWrapper.dlgClose(server_OnClose);
            server.OnRead += new SocketWrapper.dlgRead(server_OnRead);

            //TODO: Load mob/map/item/etc databases
            EvolutionDB.Load("Data\\DigimonEvolve.bin");
            MapDB.Load("Data\\MapList.bin");
            PortalDB.Load("Data\\MapPortal.bin");
            DigimonDB.Load("Data\\DigimonList.bin");
            ItemDB.Load("Data\\ItemList.bin");
            MonsterDB.Load("Data\\MonsterList.bin");
            TacticsDB.Load("Data\\Tactics.bin");

            World();

            Initialize();
        }

        /// <summary>
        /// Initializes threads
        /// </summary>
        private void Initialize()
        {
            tMain = new Thread(new ParameterizedThreadStart(Observe));
            tMain.IsBackground = true;
        }

        public void Start()
        {
            if (tMain != null ) 
            {
                if (tMain.ThreadState == ThreadState.Aborted)
                    Initialize();
                else if (tMain.ThreadState == ThreadState.Running)
                    return;
            }
            
            tMain.Start(null);
        }

        public void Stop()
        {
            tMain.Abort();
        }

        private void Observe(object o)
        {
            try
            {
                //Starts listening
                server.Listen(new ServerInfo(Opt.GameServer.Port, Opt.GameServer.IP));

                //Starts monitoring the client list
                ThreadPool.QueueUserWorkItem(new WaitCallback(Monitor));

                while (true)
                {
                }
            }
            catch (ThreadAbortException)
            {
                server.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Monitors the list of clients and removes them
        /// </summary>
        private void Monitor(object state)
        {
            try
            {
                while (true)
                {
                    Client[] temp = Clients.ToArray();
                    List<Client> toRemove = new List<Client>();

                    for (int i = 0; i < temp.Length; i++)
                    {
                        Client client = temp[i];
                        try
                        {
                            if (!client.m_socket.Connected &&
                                !client.m_socket.Poll(1000, System.Net.Sockets.SelectMode.SelectRead | System.Net.Sockets.SelectMode.SelectWrite))
                            {
                                toRemove.Add(client);
                            }
                            else
                            {
                                SqlDB.SaveTamer(client);
                            }
                        }
                        catch
                        {
                            client.Close();
                            toRemove.Add(client);
                            continue;
                        }
                    }
                    lock (Clients)
                    {
                        foreach (Client client in toRemove)
                        {
                            Clients.Remove(client);
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException)
            {

            }
        }

        private Client Find(short Handle)
        {
            Client client = null;
            foreach(Client _client in Clients)
                if (_client.Tamer != null && _client.Tamer.TamerHandle == Handle)
                {
                    client = _client;
                    break;
                }
            return client;
        }

        void server_OnRead(Client client, byte[] buffer, int length)
        {
            Packets.PacketReader Packet = null;
            try
            {
                Packet = new Packets.PacketReader(buffer);
            }
            catch
            {
                Console.WriteLine("Packet checksum failed!");
            }
            Process(client, Packet);
        }

        void server_OnClose(Client client)
        {
            try
            {
                lock (Clients)
                {
                    Clients.Remove(client);
                }
            }
            catch { }
        }

        void server_OnAccept(Client client)
        {
            try
            {
                client.Handshake();
                //Clients.Add(client);
            }
            catch { }
        }
    }
}
