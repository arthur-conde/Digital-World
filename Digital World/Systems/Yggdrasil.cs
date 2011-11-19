using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Network;
using System.Threading;
using System.Collections.ObjectModel;
using Digital_World.Database;

namespace Digital_World.Systems
{
    public partial class Yggdrasil
    {
        private SocketWrapper server = null;
        public string Host { get { return Properties.Settings.Default.Host; } }
        public int Port { get { return Properties.Settings.Default.Port; } }
        private Thread tMain = null;

        public ObservableCollection<Client> Clients = new ObservableCollection<Client>();

        public Yggdrasil()
        {
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

            Initialize();
        }

        /// <summary>
        /// Initializes threads
        /// </summary>
        private void Initialize()
        {
            tMain = new Thread(new ParameterizedThreadStart(Observe));
            tMain.IsBackground = true;

            World();
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
                server.Listen(new ServerInfo(Port, System.Net.IPAddress.Parse(Host)));

                //Starts monitoring the client list
                ThreadPool.QueueUserWorkItem(new WaitCallback(Monitor));
                ThreadPool.QueueUserWorkItem(new WaitCallback(LifeSupport));

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
                    lock (Clients)
                    {
                        List<Client> toRemove = new List<Client>();
                        foreach (Client client in Clients)
                        {
                            if (!client.m_socket.Connected)
                                toRemove.Add(client);
                            else
                            {
                                SqlDB.SaveTamer(client);
                            }
                        }

                        foreach (Client client in toRemove)
                        {
                            Clients.Remove(client);
                        }
                    }

                    Thread.Sleep(30 * 1000);
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
            Process(client, new Packets.PacketReader(buffer));
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
