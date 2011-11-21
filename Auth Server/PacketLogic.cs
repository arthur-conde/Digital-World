using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Packets;

namespace Digital_World
{
    public class PacketLogic
    {
        public static void Process(Client client, byte[] buffer)
        {
            PacketReader packet = null;
            try
            {
                packet = new PacketReader(buffer);
            }
            catch
            {
                return;
            }
            switch (packet.Type)
            {
                case -1:
                    {
                        /*
                        PacketWriter resp = new PacketWriter();
                        resp.Type(-2);
                        resp.WriteBytes(new byte[] { 0xcf, 0xa6, 0x8f, 0xd8, 0xb4, 0x4e });
                         * */
                        Console.WriteLine("Accepted connection: {0}", client.m_socket.RemoteEndPoint);

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
                        SqlDB.CreateUser(user, pass);
                        Console.WriteLine("Creating user {0}...", user);
#else
                        int success = SqlDB.Validate(client, user, pass);
                        switch (success)
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
                                break;
                        }
#endif
                        break;
                    }
                case 1702:
                    {
                        //Requesting IP of Server
                        int serverID = BitConverter.ToInt32(buffer, 4);
                        KeyValuePair<int, string> server = SqlDB.GetServer(serverID);
                        SqlDB.LoadUser(client);
                        client.Send(new Packets.Auth.ServerIP(server.Value, server.Key, client.AccountID, client.UniqueID));
                        break;
                    }
                case 0x6A5:
                    {
                        client.Send(new Packets.Auth.ServerList(SqlDB.GetServers(), client.Username, client.Characters));
                        break;
                    }
                case -3:
                    break;
                default:
                    {
                        Console.WriteLine("Unknown Packet ID: {0}", packet.Type);
                        Console.WriteLine(Packet.Visualize(buffer));
                        break;
                    }
            }
        }   
    }
}
