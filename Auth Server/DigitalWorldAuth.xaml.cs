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

namespace Digital_World
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketWrapper server;
        List<Client> clients = new List<Client>();

        public MainWindow()
        {
            InitializeComponent();

            server = new SocketWrapper();
            server.OnAccept += new SocketWrapper.dlgAccept(m_auth_OnAccept);
            server.OnRead += new SocketWrapper.dlgRead(m_auth_OnRead);
            server.OnClose += new SocketWrapper.dlgClose(server_OnClose);

            Logger _writer = new Logger(tLog);
        }

        void m_auth_OnRead(Client client, byte[] buffer, int length)
        {
            //TODO: Packet Response Logic
            int type = BitConverter.ToInt16(buffer, 2);
            switch (type)
            {
                case -1:
                    {
                        /*
                        PacketWriter resp = new PacketWriter();
                        resp.Type(-2);
                        resp.WriteBytes(new byte[] { 0xcf, 0xa6, 0x8f, 0xd8, 0xb4, 0x4e });
                         * */
                        PacketReader packet = new PacketReader(buffer);
                        Console.WriteLine("Accepted connection: {0}", client.m_socket.RemoteEndPoint);
                        Console.WriteLine("Received FFFF: \n{0}", packet.ToString());

                        packet.Skip(8);
                        ushort u1 = (ushort)packet.ReadShort();
                        ushort u2 = (ushort)packet.ReadShort();

                        client.Send(new Packets.PacketFFEF((short)(client.handshake ^ 0x7e41)));
                        break;
                    }
                case 3301:
                    {
                        //Login information
                        string user = Encoding.ASCII.GetString(buffer, 13, buffer[12]);
                        string pass = Encoding.ASCII.GetString(buffer, 15 + buffer[12], buffer[buffer[12] + 14]);

                        Console.WriteLine("Receiving login request: {0}", user);
#if CREATE
                        Database.CreateUser(user, pass);
                        Console.WriteLine("Creating user {0}...", user);
#else
                        int success = SqlDB.Validate(client, user, pass);
                        switch(success)
                        {
                            case -1:
                                //Banned or non-existent
                                Console.WriteLine("Banned or nonexistent login: {0}", user);
                                client.Send(new Packets.Auth.LoginMessage(string.Format("This username has been banned.")));
                                break;
                            case -2:
                                //Wrong Pass;
                                Console.Write("Incorrect password: {0}", user);
                                client.Send(new Packets.Auth.LoginMessage("The password provided does not match."));
                                break;
                            case -3:
                                client.Send(new Packets.Auth.LoginMessage("This username does not exist."));
                                break;
                            default:
                                //Normal Login
                                Console.WriteLine("Successful login: {0}\n Sending Server List", user);
                                client.Send(new Packets.Auth.ServerList(SqlDB.GetServers(), user, client.Characters));
                                //state.Send(new Packets.LoginMessage(string.Format("You are not a valid user.")));
                                break;
                        }
#endif
                        break;
                    }
                case 1702:
                    {
                        Console.WriteLine("Packet: Type 1702 from {0}",client.m_socket.RemoteEndPoint);
                        //Requesting IP of Server
                        int serverID = BitConverter.ToInt32(buffer, 4);
                        KeyValuePair<int, string> server = SqlDB.GetServer(serverID);
                        SqlDB.LoadUser(client);
                        Packets.Auth.ServerIP packet = new Packets.Auth.ServerIP(server.Value, server.Key, client.AccountID, client.UniqueID);
                        client.Send(packet);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown Packet ID: {0}", type);
                        Console.WriteLine(Packet.Visualize(buffer));
                        break;
                    }
            }
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
            winOpt.ShowDialog();
        }
    }
}
