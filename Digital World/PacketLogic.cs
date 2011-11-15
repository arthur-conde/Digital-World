using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Packets;
using System.IO;
using Digital_World.Entities;
using Digital_World.Database;
using Digital_World.Helpers;

namespace Digital_World
{
    public class PacketLogic
    {
        public static void Process(Client client, PacketReader packet)
        {
            Character Tamer = client.Tamer;
            Digimon ActiveMon = null;
            if (Tamer != null && Tamer.Partner != null)
                ActiveMon = Tamer.Partner;
            switch (packet.Type)
            {
                case -1:
                    {
                        client.time_t = (uint)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        client.Send(new Packets.PacketFFEF((int)client.time_t, (short)(client.handshake ^ 0x7e41)));
                        break;
                    }
                case 1001:
                    {
                        //Movement speed, map handle
                        Packet p26b2 = new Packets.Game.Packet26B2(
                            client.Tamer.MapHandle, client.Tamer.Partner.MapHandle,
                            (short)client.Tamer.MS, client.Tamer.Partner.Stats.MS);
                        client.Send(p26b2);

                        //Event Item
                        client.Send(new Packets.Game.PacketC22(8, new DateTime(1970, 1, 1, 0, 0, 10)));

                        /*client.Send(new Packets.Game.Packet3FF(client.Tamer.Partner.hMap, client.Tamer.Partner.Stats));
                        Console.WriteLine("3FF Sent.", packet.Type);*/
                        break;
                    }
                case 2404:
                    {
                        //FriendList
                        client.Send(new Packets.Game.FriendList());
                        break;
                    }
                case 1706:
                    {
                        uint u = packet.ReadUInt();
                        uint acctId = packet.ReadUInt();
                        int accessCode = packet.ReadInt();

                        SqlDB.LoadUser(client, acctId, accessCode);
                        SqlDB.LoadTamer(client);

                        MakeHandles(client.Tamer, (uint)client.time_t);

                        Packet p = new Packets.Game.CharInfo(client.Tamer);
                        File.WriteAllBytes("W:\\My.packet", p.ToArray());
                        //Console.WriteLine(Packet.Visualize(buffer));

                        //byte[] buffer = File.ReadAllBytes("W:\\CharInfo.packet");
                        client.Send(new Packets.Game.Packet1704());
                        client.Send(p);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown Packet Type {0}\n{1}", packet.Type, packet);
                        break;
                    }
            }
        }
        public static void MakeHandles(Character Tamer, uint time_t)
        {
            Random Rand = new Random();
            //byte[] bRand = Import.GetRandBytes(time_t, 0xcf);
            //Rand.NextBytes(bRand);

            Tamer.intHandle = (uint)(Tamer.ProperModel + Rand.Next(1,30));

            for (int i = 0; i < Tamer.DigimonList.Count; i++)
            {
                Digimon mon = Tamer.DigimonList[i];
                //Rand.NextBytes(bRand);

                mon.intHandle = mon.ProperModel() + Rand.Next(1, 255);
            }
        }

    }
}
