using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Digital_World.Network;
using Digital_World.Packets;
using Digital_World.Helpers;

namespace Digital_World
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketWrapper server;
        List<Client> clients = new List<Client>();
        Settings sSettings;

        public MainWindow()
        {
            InitializeComponent();

            server = new SocketWrapper();
            server.OnAccept += new SocketWrapper.dlgAccept(m_auth_OnAccept);
            server.OnRead += new SocketWrapper.dlgRead(m_auth_OnRead);
            server.OnClose += new SocketWrapper.dlgClose(server_OnClose);

            Logger _writer = new Logger(tLog);

            sSettings = Settings.Deserialize("Settings.xml");
            if (sSettings.AuthServer.AutoStart)
            {
                ServerInfo info = new ServerInfo(sSettings.AuthServer.Port, sSettings.AuthServer.IP);
                server.Listen(info);
            }
        }

        void m_auth_OnRead(Client client, byte[] buffer, int length)
        {
            //TODO: Packet Response Logic
            PacketLogic.Process(client, buffer);
        }

        void m_auth_OnAccept(Client state)
        {
            state.Handshake();
            clients.Add(state);
        }

        void server_OnClose(Client client)
        {
            try
            {
                clients.Remove(client);
            }
            catch { }
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (server.Running) return;
            ServerInfo info = new ServerInfo(Properties.Settings.Default.Port,
                 System.Net.IPAddress.Parse(Properties.Settings.Default.Host));
            server.Listen(info);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            server.Stop();
            
            foreach(Client client in clients)
            {
                client.Send(new Packets.Auth.LoginMessage("Server is shutting down."));
                client.m_socket.Close();
            }
        }

        private void mi_opt_Click(object sender, RoutedEventArgs e)
        {
            Options winOpt = new Options();
            if (winOpt.ShowDialog().Value)
                sSettings = Settings.Deserialize("Settings.xml");
        }
    }
}
