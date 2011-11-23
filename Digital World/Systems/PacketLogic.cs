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
using Digital_World.Packets.Game.Interface.Hatching;

namespace Digital_World.Systems
{
    public partial class Yggdrasil
    {
        /// <summary>
        /// For Handles
        /// </summary>
        static Random Rand = new Random();

        /// <summary>
        /// Server-wide Send to all connected clients.
        /// </summary>
        /// <param name="Packet">Packet to Send</param>
        public void Send(IPacket Packet)
        {
            List<Client> remove = new List<Client>();
            Client[] temp = Clients.ToArray();
            for (int i = 0; i < temp.Length;i++ )
            {
                Client Client = temp[i];
                try
                {
                    Client.Send(Packet);
                }
                catch
                {
                    remove.Add(Client);
                }
            }

            lock (Clients)
            {
                foreach (Client Client in remove)
                {
                    Client.Close();
                    Clients.Remove(Client);
                }
            }
        }

        /// <summary>
        /// Process incoming packets
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        public void Process(Client client, PacketReader packet)
        {
            Character Tamer = client.Tamer;
            Digimon ActiveMon = null;
            GameMap ActiveMap = null;
            if (Tamer != null && Tamer.Partner != null)
            {
                ActiveMon = Tamer.Partner;
                ActiveMap = Maps[client.Tamer.Location.Map];
            }

            //Console.WriteLine("Processing type {0}", packet.Type);
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
                        //Movement speed
                        client.Send(new Packets.Game.UpdateMS(Tamer.TamerHandle, Tamer.DigimonHandle,
                            (short)Tamer.MS, Tamer.Partner.Stats.MS));

                        //Event Item
                        client.Send(new Packets.Game.RewardCountdown(8, new DateTime(1970, 1, 1, 0, 0, 10)));
                        ActiveMap.Spawn(client); 
                        break;
                    }
                case -3:
                    {
                        break;
                    }
                case 1004:
                    {
                        //Move
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

                        Maps[Tamer.Location.Map].Send(new MovePlayer(Tamer));
                        break;
                    }
                case 1008:
                    {
                        //Chat Type Normal
                        string text = packet.ReadString();
                        if (text.StartsWith("!"))
                            Command(client, text.Substring(1).Split(' '));
                        else
                        {
                            if (ActiveMap == null)
                                client.Send(new ChatNormal(Tamer.TamerHandle, text));
                            else
                                ActiveMap.Send(new ChatNormal(Tamer.TamerHandle, text));
                            
                        }
                        break;
                    }
                case 1016:
                    {
                        short h1 = packet.ReadShort();
                        short h2 = packet.ReadShort();
                        if (h1 != Tamer.TamerHandle)
                        {
                            SqlDB.SaveTamer(client);
                            ActiveMap.Send(new DespawnPlayer(Tamer.TamerHandle, Tamer.DigimonHandle));
                        }
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

                        ActiveMap.Send(new Evolve(Tamer.DigimonHandle, Tamer.TamerHandle, id, (byte)(devolve ? 5 : 0)));
                        mon.CurrentForm = id;
                        mon.Model = GetModel(mon.ProperModel(), mon.byteHandle);

                        DigimonData dData = DigimonDB.GetDigimon(Tamer.Partner.CurrentForm);
                        if (dData != null)
                        {
                            Tamer.Partner.Stats = dData.Default(Tamer, Tamer.Partner.Stats.Intimacy, Tamer.Partner.Size);
                            client.Send(new UpdateStats(Tamer, Tamer.Partner));
                        }
                        //Send to nearby players
                        break;
                    }
                case 1036:
                    {
                        int Slot = packet.ReadInt();
                        int NPC= packet.ReadInt();
                        TDBTactic egg = TacticsDB.Get(Tamer.Inventory[Slot].BaseID);
                        if (egg == null)
                        {
                            egg = TacticsDB.Get(Tamer.Inventory[Slot].ItemId);
                        }

                        if (egg != null)
                        {
                            Tamer.Incubator = egg.ItemId;
                            Tamer.IncubatorLevel = 0;
                            Tamer.Inventory.Remove(Slot);
                        }
                        break;
                    }
                case 1037:
                    {
                        //Data Input
                        int slot = packet.ReadInt();
                        TDBTactic egg = TacticsDB.Get(Tamer.Incubator);
                        int res = Opt.GameServer.HatchRates.Hatch(Tamer.IncubatorLevel);
                        if (Tamer.Incubator < 10000)
                            res = 0;
                        if (res == 0)
                        {
                            //Great Success
                            Tamer.IncubatorLevel++;
                            if (Tamer.IncubatorLevel < 3)
                                client.Send(new DataInputSuccess(Tamer.TamerHandle));
                            else
                                client.Send(new DataInputSuccess(Tamer.TamerHandle, (byte)Tamer.IncubatorLevel));
                        }
                        else if (res == 1)
                        {
                            client.Send(new DataInputFailure(Tamer.TamerHandle, false));
                        }
                        else
                        {
                            Tamer.IncubatorLevel = 0;
                            Tamer.Incubator = 0;
                            client.Send(new DataInputFailure(Tamer.TamerHandle, true));
                        }
                        if (egg != null)
                            Tamer.Inventory[slot].Amount -= egg.Data;
                        break;
                    }
                case 1038:
                    {
                        //Hatch
                        packet.ReadByte();
                        string name = packet.ReadZString();
                        int NPC = packet.ReadInt();
                        if (Tamer.IncubatorLevel < 3)
                            client.Close();
                        TDBTactic egg = TacticsDB.Get(Tamer.Incubator);
                        if (egg != null)
                        {
                            uint digiId = SqlDB.CreateMercenary(Tamer.CharacterId, name, egg.Species, Tamer.IncubatorLevel,
                                Opt.GameServer.SizeRanges.Size(Tamer.IncubatorLevel), 0);
                            if (digiId == 0) return;
                            Digimon mon = SqlDB.GetDigimon(digiId);
                            for (int i = 0; i < Tamer.DigimonList.Length; i++)
                            {
                                if (Tamer.DigimonList[i] == null)
                                {
                                    Tamer.DigimonList[i] = mon;
                                    client.Send(new Hatch(mon, i));
                                    break;
                                }
                            }
                            Send(new BroadcastHatch(Tamer.Name, name, egg.Species, mon.Size, Tamer.IncubatorLevel));
                            Tamer.IncubatorLevel = 0;
                            Tamer.Incubator = 0;
                        }
                        break;
                    }
                case 1039:
                    {
                        //Remove Egg.
                        int NPC = packet.ReadInt();
                        if (Tamer.IncubatorLevel == 0 && Tamer.Incubator != 0)
                        {
                            Item e = new Item();
                            e.ID = Tamer.Incubator;
                            e.Amount = 1;
                            Tamer.Inventory.Add(e);
                        }
                        break;
                    }

                case 1041:
                    {
                        //Mercenary Switch
                        byte slot = packet.ReadByte();
                        Digimon target = Tamer.DigimonList[slot];
                        Digimon current = Tamer.DigimonList[0];

                        client.Send(new UpdateStats(Tamer, target));
                        ActiveMap.Send(new DigimonSwitch(Tamer.DigimonHandle, slot, current, target));

                        Tamer.Partner = target;

                        Tamer.DigimonList[slot] = current;
                        Tamer.DigimonList[0] = target;
                        break;
                    }
                case 1325:
                    {
                        //Riding Mode
                        //TODO: Verify if rideable
                        client.Send(new RidingMode(Tamer.TamerHandle, Tamer.DigimonHandle));
                        client.Send(new UpdateMS(Tamer.TamerHandle, Tamer.DigimonHandle, Tamer.Partner.Stats.MS, 1, 1));
                        break;
                    }
                case 1326:
                    {
                        //Riding mode
                        client.Send(new StopRideMode(Tamer.TamerHandle, Tamer.DigimonHandle));
                        client.Send(new UpdateMS(Tamer.TamerHandle, Tamer.DigimonHandle, (short)Tamer.MS, Tamer.Partner.Stats.MS));
                        break;
                    }
                case 1703:
                    {
                        //Appears at the end of map switching
                        client.Send(packet);
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

                        if (client.Tamer == null)
                            client.m_socket.Close();

                        lock (Clients) { Clients.Add(client); }

                        MakeHandles(client.Tamer, (uint)client.time_t);

                        Packet p = new Packets.Game.CharInfo(client.Tamer);
                        //File.WriteAllBytes("W:\\My.packet", p.ToArray());
                        //Console.WriteLine(Packet.Visualize(buffer));

                        //byte[] buffer = File.ReadAllBytes("W:\\CharInfo.packet");
                        client.Send(new Packets.Game.Packet1704());
                        client.Send(p);

                        Maps[client.Tamer.Location.Map].Enter(client); //Enter GameMap
                        break;
                    }
                case 1709:
                    {
                        //Map Change
                        int portalId = packet.ReadInt();
                        Portal Portal = PortalDB.GetPortal(portalId);
                        MapData Map = MapDB.GetMap(Portal.MapId);

                        Maps[client.Tamer.Location.Map].Leave(client);
                        Tamer.Location = new Position(Portal);

                        SqlDB.SaveTamer(client);
                        client.Send(new MapChange(Opt.GameServer.IP.ToString(), Opt.GameServer.Port, Portal, Map.Name));
                        client.Send(new SendHandle(Tamer.TamerHandle));
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
                            dMon.Model = GetModel(dMon.ProperModel());
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
                            archiveDigimon.Model = GetModel(archiveDigimon.ProperModel());
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
                            Mon2.Model = GetModel(Mon2.ProperModel());
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

                            if (iSource.ItemId == iDest.ItemId)
                            {
                                //Combine Stacks
                                iDest.Amount += iSource.Amount;
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
                        //Split 
                        short sItemToSplit = packet.ReadShort();
                        short sDestination = packet.ReadShort();
                        byte sAmountToSplit = packet.ReadByte();

                        Item iToSplit = Tamer.Inventory[sItemToSplit];
                        if (Tamer.Inventory.Count > Tamer.InventorySize) return; //Don't have the stop this action packet T_T
                        Item iNew = new Item(0);
                        iNew.ID = iToSplit.ID;
                        iNew.time_t = iToSplit.time_t;
                        iNew.Amount = sAmountToSplit;
                        iToSplit.Amount -= sAmountToSplit;
                        if (iToSplit.Amount == 0)
                        {
                            Tamer.Inventory.Remove(sItemToSplit);
                        }

                        Tamer.Inventory.Add(iNew);

                        client.Send(packet);
                        break;
                    }
                case 3922:
                    {
                        //Scan DigiEgg
                        int NpcId = packet.ReadInt();
                        byte uByte = packet.ReadByte();
                        int Slot = packet.ReadInt();
                        //Check for distance?
                        break;
                    }
                case 3923:
                    {
                        //Return DigiEgg
                        int NpcId = packet.ReadInt();
                        int Slot = packet.ReadInt();
                        //Check for NPC distance?
                        int amount = Tamer.Inventory[Slot].BaseID;
                        if (90101 <= amount && amount < 90108)
                        {
                            if (amount == 90101)
                            {
                                amount = 25 * Tamer.Inventory[Slot].Amount;
                            }
                            else
                            {
                                amount = 100 + (50 * (amount - 90102));
                            }
                            Tamer.Money += amount * Math.Max(Tamer.Inventory[Slot].Amount, 1);
                            Tamer.Inventory.Remove(Slot);
                            client.Send(new ReturnEggs(amount, Tamer.Money, 0));
                        }
                        else
                        {
                            client.Close();
                        }
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown Packet Type {0}\n{1}", packet.Type, packet);
                        break;
                    }
            }
        }

        public static uint GetModel(uint Model)
        {
            uint hEntity = Model;
            return (uint)(hEntity + Rand.Next(1, 255));
        }

        public static uint GetModel(uint Model, byte Id)
        {
            uint hEntity = Model;
            return (uint)(hEntity + Id);
        }

        public static short GetHandle(uint Model, byte type)
        {
            byte[] b = new byte[] { (byte)((Model >> 32) & 0xFF), type };
            return BitConverter.ToInt16(b, 0);
        }

        public static void MakeHandles(Character Tamer, uint time_t)
        {
            Tamer.intHandle = (uint)(Tamer.ProperModel + Rand.Next(1,255));

            for (int i = 0; i < Tamer.DigimonList.Length; i++)
            {
                if (Tamer.DigimonList[i] == null) continue;
                Digimon mon = Tamer.DigimonList[i];
                mon.Model = GetModel(mon.ProperModel());
            }
            Tamer.DigimonHandle = Tamer.Partner.Handle;
        }

    }
}
