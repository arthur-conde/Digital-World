using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Packets;
using System.IO;
using Digital_World.Entities;
using Digital_World.Database;
using Digital_World.Helpers;
using Digital_World.Packets.Game;
using Digital_World.Packets.Game.Interface;
using Digital_World.Packets.Game.Chat;

namespace Digital_World.Systems
{
    public partial class Yggdrasil
    {
        /// <summary>
        /// For Handles
        /// </summary>
        static Random Rand = new Random();

        /// <summary>
        /// Process incoming packets
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        public void Process(Client client, PacketReader packet)
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

                        //Event Item
                        client.Send(new Packets.Game.RewardCountdown(8, new DateTime(1970, 1, 1, 0, 0, 10)));

                        client.Send(new Packets.Game.Status(client.Tamer.DigimonHandle, client.Tamer.Partner.Stats));
                        //Console.WriteLine("3FF Sent.", packet.Type);
                        break;
                    }
                case -3:
                    {
                        break;
                    }
                case 1004:
                    {
                        int unknown1 = packet.ReadInt();
                        short handle = packet.ReadShort();
                        int X = packet.ReadInt();
                        int Y = packet.ReadInt();
                        float dir = packet.ReadFloat();

                        if (handle == Tamer.TamerHandle)
                        {
                            Tamer.Location.PosY = Y;
                            Tamer.Location.PosX = X;
                        }
                        else
                        {
                            Tamer.Partner.Location.Map = Tamer.Location.Map;
                            Tamer.Partner.Location.PosX = X;
                            Tamer.Partner.Location.PosY = Y;
                        }

                        Packet p26b2 = new Packets.Game.UpdateMS(
                            client.Tamer.TamerHandle, client.Tamer.DigimonHandle,
                            (short)client.Tamer.MS, client.Tamer.Partner.Stats.MS);
                        client.Send(p26b2);
                        break;
                    }
                case 1008:
                    {
                        //Chat Type Normal
                        string text = packet.ReadString();
                        if (text.StartsWith("!"))
                            Command(client, text.Substring(1).Split(' '));
                        else
                            client.Send(new ChatNormal(Tamer.TamerHandle, text));
                        break;
                    }
                case 1016:
                    {
                        SqlDB.SaveTamer(client);
                        //client.m_socket.Close();
                        break;
                    }
                case 1028:
                    {
                        //Shinka!
                        short handle = packet.ReadShort();
                        int stage = packet.ReadByte();

                        Digimon mon = Tamer.Partner;

                        EvolutionLine evolveLine = EvolutionDB.GetLine(mon.Species, mon.CurrentForm);

                        stage = 1 + stage + evolveLine.iLevel;
                        int id = mon.Species;
                        bool devolve = false;
                        if (evolveLine.Line.ContainsKey(stage))
                            id = evolveLine.Line[stage];
                        else if (evolveLine.Line.ContainsKey(65537))
                        {
                            id = evolveLine.Line[65537];
                            devolve = true;
                        }

                        client.Send(new Evolve(Tamer.DigimonHandle, Tamer.TamerHandle, id, (byte)(devolve ? 5 : 0)));
                        mon.CurrentForm = id;

                        DigimonData dData = DigimonDB.GetDigimon(Tamer.Partner.CurrentForm);
                        if (dData != null)
                        {
                            Tamer.Partner.Stats = dData.Default(Tamer, Tamer.Partner.Stats.Intimacy, Tamer.Partner.Size);
                            client.Send(new UpdateStats(Tamer, Tamer.Partner));
                        }
                        //Send to nearby players
                        break;
                    }
                case 1041:
                    {
                        //Mercenary Switch
                        byte slot = packet.ReadByte();
                        Digimon target = Tamer.DigimonList[slot];
                        Digimon current = Tamer.DigimonList[0];

                        client.Send(new UpdateStats(Tamer, target));
                        client.Send(new DigimonSwitch(Tamer.DigimonHandle, slot, current, target));

                        Tamer.Partner = target;

                        Tamer.DigimonList[slot] = current;
                        Tamer.DigimonList[0] = target;
                        break;
                    }
                case 1325:
                    {
                        //Verify if rideable
                        client.Send(new RidingMode(Tamer.TamerHandle, Tamer.DigimonHandle));
                        client.Send(new UpdateMS(Tamer.TamerHandle, Tamer.DigimonHandle, Tamer.Partner.Stats.MS, 1, 1));
                        break;
                    }
                case 1326:
                    {
                        client.Send(new StopRideMode(Tamer.TamerHandle, Tamer.DigimonHandle));
                        client.Send(new UpdateMS(Tamer.TamerHandle, Tamer.DigimonHandle, (short)Tamer.MS, Tamer.Partner.Stats.MS));
                        break;
                    }
                case 1703:
                    {
                        client.Send(packet);
                        break;
                    }
                case 1709:
                    {
                        //Map Change
                        int portalId = packet.ReadInt();
                        Portal Portal = PortalDB.GetPortal(portalId);
                        MapData Map = MapDB.GetMap(Portal.MapId);
                        Tamer.Location = new Position(Portal);

                        SqlDB.SaveTamer(client);
                        client.Send(new MapChange(Properties.Settings.Default.Host, Properties.Settings.Default.Port, Portal, Map.Name));
                        client.Send(new SendHandle(Tamer.TamerHandle));
                        break;
                    }
                case 1706:
                    {
                        //Verify access code and send char data.
                        uint u = packet.ReadUInt();
                        uint acctId = packet.ReadUInt();
                        int accessCode = packet.ReadInt();

                        SqlDB.LoadUser(client, acctId, accessCode);
                        SqlDB.LoadTamer(client);

                        lock (Clients) { Clients.Add(client); }

                        MakeHandles(client.Tamer, (uint)client.time_t);

                        Packet p = new Packets.Game.CharInfo(client.Tamer);
                        File.WriteAllBytes("W:\\My.packet", p.ToArray());
                        //Console.WriteLine(Packet.Visualize(buffer));

                        //byte[] buffer = File.ReadAllBytes("W:\\CharInfo.packet");
                        client.Send(new Packets.Game.Packet1704());
                        client.Send(p);

                        Maps[client.Tamer.Location.Map].Enter(client); //Enter GameMap
                        break;
                    }
                case 2404:
                    {
                        //FriendList
                        client.Send(new Packets.Game.FriendList());
                        break;
                    }
                case 3204:
                    {
                        Dictionary<int, Digimon> ArchivedDigimon = new Dictionary<int, Digimon>();
                        for (int i = 0; i < Tamer.ArchivedDigimon.Length; i++)
                        {
                            if (Tamer.ArchivedDigimon[i] == 0) continue;
                            Digimon dMon = SqlDB.LoadDigimon(Tamer.ArchivedDigimon[i]);
                            dMon.intHandle = dMon.ProperModel() + Rand.Next(0, 255);
                            ArchivedDigimon.Add(i,dMon);
                        }
                        client.Send(new DigimonArchive(Tamer.ArchiveSize, 40, ArchivedDigimon));
                        break;
                    }
                case 3201:
                    {
                        int Slot = packet.ReadInt();
                        int ArchiveSlot = packet.ReadInt() - 1000;
                        int uInt = packet.ReadInt();

                        if (Tamer.DigimonList[Slot] == null)
                        {
                            //Archive to Digivice
                            Digimon archiveDigimon = SqlDB.LoadDigimon(Tamer.ArchivedDigimon[ArchiveSlot]);
                            archiveDigimon.intHandle = archiveDigimon.ProperModel() + Rand.Next(0, 255);
                            Tamer.ArchivedDigimon[ArchiveSlot] = 0;
                            Tamer.DigimonList[Slot] = archiveDigimon;

                        }
                        else if (Tamer.ArchivedDigimon[ArchiveSlot] == 0)
                        {
                            //Digivice to Archive
                            Digimon Mon1 = Tamer.DigimonList[Slot];
                            SqlDB.SaveDigimon(Mon1);

                            Tamer.ArchivedDigimon[ArchiveSlot] = Mon1.DigiId;
                            Tamer.DigimonList[Slot] = null;
                        }
                        else
                        {
                            //Swapping
                            Digimon Mon1 = Tamer.DigimonList[Slot];
                            SqlDB.SaveDigimon(Mon1);

                            Digimon Mon2 = SqlDB.LoadDigimon(Tamer.ArchivedDigimon[ArchiveSlot]);
                            Tamer.ArchivedDigimon[ArchiveSlot] = Mon1.DigiId;
                            Mon2.intHandle = Mon2.ProperModel() + Rand.Next(0, 255);
                            Tamer.DigimonList[Slot] = Mon2;
                        }
                        SqlDB.SaveTamer(client);
                        client.Send(new StoreDigimon(Slot, ArchiveSlot + 1000, 0));
                        break;
                    }
                case 3904:
                    {
                        //Equip/unequip
                        short Slot1 = packet.ReadShort();
                        short Slot2 = packet.ReadShort();

                        //If s1 is a slot in the inventory
                        if (Slot1 < 63 && Slot2 < 63)
                        {
                            //Both items are inventory slots
                            Item iSource = Tamer.Inventory[Slot1];
                            Item iDest = Tamer.Inventory[Slot2];

                            if (iSource.ID == iDest.ID)
                            {
                                //Combine Stacks
                                iDest.Count += iSource.Count;
                                Tamer.Inventory.Remove(Slot1);
                            }
                            else
                            {
                                //Switch items
                                Item iTemp = iSource;
                                Tamer.Inventory[Slot1] = iDest;
                                Tamer.Inventory[Slot2] = iSource;
                            }
                            client.Send(packet.ToArray());
                        }
                        else if (Slot1 < 63 && Slot2 >= 1000)
                        {
                            //Equipping an item
                            int iSlot = Tamer.Inventory.EquipSlot(Slot2);
                            if (iSlot == -1)
                            {
                                iSlot = 0;
                            }
                            Item e = Tamer.Inventory[Slot1];
                            Item cur_e = Tamer.Equipment[iSlot];
                            if (cur_e.ItemId != 0)
                                Tamer.Inventory.Add(cur_e);
                            Tamer.Inventory.Remove(e);
                            Tamer.Equipment[iSlot] = e;
                            client.Send(packet.ToArray());
                            client.Send(new UpdateStats(Tamer, Tamer.Partner));
                            client.Send(new UpdateEquipment(Tamer.TamerHandle, (byte)iSlot, e));
                        }
                        else if (Slot1 >= 1000 && Slot2 < 63)
                        {
                            //Unequip
                            int eSlot = Tamer.Inventory.EquipSlot(Slot1);
                            Item e = Tamer.Equipment[eSlot];
                            if (Tamer.Inventory.Count >= Tamer.InventorySize) return;
                            Tamer.Inventory.Add(e);
                            Tamer.Equipment[eSlot] = new Item();
                            client.Send(packet.ToArray());
                            client.Send(new UpdateStats(Tamer, Tamer.Partner));
                            client.Send(new UpdateEquipment(Tamer.TamerHandle, (byte)eSlot));
                        }
                        break;
                    }
                case 3907:
                    {
                        //Split stack
                        short sItemToSplit = packet.ReadShort();
                        short sDestination = packet.ReadShort();
                        byte sAmountToSplit = packet.ReadByte();

                        Item iToSplit = Tamer.Inventory[sItemToSplit];
                        if (Tamer.Inventory.Count > Tamer.InventorySize) return; //Don't have the stop this action packet T_T
                        Item iNew = new Item(0);
                        iNew.ID = iToSplit.ID;
                        iNew.time_t = iToSplit.time_t;
                        iNew.Count = sAmountToSplit;
                        iToSplit.Count -= sAmountToSplit;

                        Tamer.Inventory.Add(iNew);

                        client.Send(packet);
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
            Tamer.intHandle = (uint)(Tamer.ProperModel + Rand.Next(1,255));

            for (int i = 0; i < Tamer.DigimonList.Length; i++)
            {
                Digimon mon = Tamer.DigimonList[i];
                mon.intHandle = mon.ProperModel() + Rand.Next(1, 255);
            }
            Tamer.DigimonHandle = Tamer.Partner.MapHandle;
        }

    }
}
