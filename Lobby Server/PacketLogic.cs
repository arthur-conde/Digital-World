using System;
using System.Collections.Generic;
using Digital_World.Entities;
using Digital_World.Packets;
using Digital_World.Helpers;

namespace Digital_World
{
    public class PacketLogic
    {
        public static void Process(Client client, byte[] buffer)
        {
            PacketReader packet = new PacketReader(buffer);

            switch (packet.Type)
            {
                case -1:
                    {
                        Console.WriteLine("Accepted connection: {0}", client.m_socket.RemoteEndPoint);
                        client.Send(new Packets.PacketFFEF((short)(client.handshake ^ 0x7e41)));
                        break;
                    }
                case -3:
                    { break; }
                case 1706:
                    {
                        //request characters?
                        uint AcctId = BitConverter.ToUInt32(buffer, 8);
                        int UniId = BitConverter.ToInt32(buffer, 12);


                        packet.Skip(4);
                        uint tAcct = packet.ReadUInt();
                        int tUni = packet.ReadInt();

                        SqlDB.LoadUser(client, AcctId, UniId);
                        List<Character> listTamers = SqlDB.GetCharacters(client.AccountID);
                        client.Send(new Packets.Lobby.CharList(listTamers));
                        break;
                    }
                case 1703:
                    {
                        client.Send(packet.ToArray());
                        break;
                    }
                case 1302:
                    {
                        //Name Availability
                        string name = packet.ReadString();
                        if (SqlDB.NameAvail(name))
                            client.Send(new Packets.Lobby.NameCheck(1));
                        else
                            client.Send(new Packets.Lobby.NameCheck(0));
                        break;
                    }
                case 1303:
                    {
                        //Create Character
                        int position = packet.ReadByte();
                        int model = packet.ReadInt();
                        string name = packet.ReadZString();
                        packet.Seek(31);
                        int digiModel = packet.ReadInt();
                        string digiName = packet.ReadZString();

                        Console.WriteLine("CreateChar {0} {1}", (CharacterModel)model, name);

                        int charId = SqlDB.CreateCharacter(client.AccountID, position, model, name, digiModel);
                        int digiId = (int)SqlDB.CreateDigimon((uint)charId, digiName, digiModel);
                        SqlDB.SetPartner(charId, digiId);
                        SqlDB.SetTamer(charId, digiId);

                        client.Send(new Packets.Lobby.ConfirmCreate());
                        break;
                    }
                case 1304:
                    {
                        int slot = packet.ReadInt();
                        string code = packet.ReadString();
                        bool canDelete = SqlDB.VerifyCode(client.AccountID, code);
                        if (canDelete)
                        {
                            if (SqlDB.DeleteTamer(client.AccountID, slot))
                                client.Send(new Packets.Lobby.CharDelete(1));
                            else
                                client.Send(new Packets.Lobby.CharDelete(0));
                        }
                            else
                             client.Send(new Packets.Lobby.CharDelete(2));
                        break;
                    }
                case 1305:
                    {
                        //Request Map Server
                        int slot = packet.ReadInt();
                        Position pLoc = null;
                        try
                        {
                            SqlDB.SetLastChar(client.AccountID, slot);
                            pLoc = SqlDB.GetTamerPosition(client.AccountID, slot);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        client.Send(new Packets.Lobby.ServerIP(Properties.Settings.Default.MapServer, Properties.Settings.Default.MapPort,
                            pLoc.Map, pLoc.MapName));
                        break;
                    }
                default:
                    Console.WriteLine("Unknown Packet Type: {0}", packet.Type);
                    Console.WriteLine(packet.ToString());
                    break;
            }
        }
    }
}
