using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Network;
using System.Threading;
using System.Collections.ObjectModel;

namespace Digital_World
{
    public class Yggdrasil
    {
        private SocketWrapper server = null;
        public string Host { get { return Properties.Settings.Default.Host; } }
        public int Port { get { return Properties.Settings.Default.Port; } }
        private Thread tMain = null;

        public ObservableCollection<Client> Clients = new ObservableCollection<Client>();

        public Yggdrasil()
        {
            server = new SocketWrapper();

            tMain = new Thread(new ParameterizedThreadStart(Observe));
            tMain.IsBackground = true;

            server.OnAccept += new SocketWrapper.dlgAccept(server_OnAccept);
            server.OnClose += new SocketWrapper.dlgClose(server_OnClose);
            server.OnRead += new SocketWrapper.dlgRead(server_OnRead);

            //TODO: Load mob/map/item/etc databases

        }

        public void Start()
        {
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
                    List<Client> toRemove = new List<Client>();
                    foreach (Client client in Clients)
                    {
                        if (!client.m_socket.Connected)
                            toRemove.Add(client);
                        else
                        {
                            //Database.Save(client);
                        }
                    }

                    foreach (Client client in toRemove)
                    {
                        Clients.Remove(client);
                    }

                    Thread.Sleep(1 * 60 * 1000);
                }
            }
            catch (ThreadAbortException)
            {

            }
        }

        void server_OnRead(Client client, byte[] buffer, int length)
        {
            PacketLogic.Process(client, new Packets.PacketReader(buffer));
        }

        void server_OnClose(Client client)
        {
            try
            {
                Clients.Remove(client);
            }
            catch { }
        }

        void server_OnAccept(Client client)
        {
            try
            {
                client.Handshake();
                Clients.Add(client);
            }
            catch { }
        }
    }
}
