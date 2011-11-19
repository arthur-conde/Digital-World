using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using Digital_World.Packets.Game;
using Digital_World.Helpers;
using Digital_World.Database;
using Digital_World.Packets.Game.Interface;
using Digital_World.Packets.Game.Chat;

namespace Digital_World.Systems
{
    public partial class Yggdrasil
    {
        public static void Command(Client client, string[] cmd)
        {
            if (client.AccessLevel <= 0) return;
            if (cmd.Length == 0) return;
            Character Tamer = client.Tamer;
            switch (cmd[0])
            {
                case "item":
                    {
                        int fullId = int.Parse(cmd[1]);
                        Item e = new Item(0);
                        e.ID = fullId;
                        if (e.ItemData == null)
                        {
                            client.Send(new ChatNormal(Tamer.DigimonHandle, string.Format("An item with the id {0} was not found.", fullId)));
                            return;
                        }
                        e.Count = 1;
                        if (cmd.Length == 3)
                        {
                            int count = 1;
                            int.TryParse(cmd[2], out count);
                            count = count > e.Max ? e.Max : count;
                            count = count < 0 ? 1 : count;
                            e.Count = (short)count;
                        }
                        if (cmd.Length == 9)
                        {
                            short[] shorts = new short[6];
                            for (int i = 4; i < cmd.Length; i++)
                            {
                                shorts[i - 4] = short.Parse(cmd[i]);
                            }
                            e.Unknown1 = shorts[0];
                            e.Unknown2 = shorts[1];
                            e.Unknown3 = shorts[2];
                            e.Unknown4 = shorts[3];
                            e.Unknown5 = shorts[4];
                            e.Unknown6 = shorts[5];

                        }
                        e.time_t = 0xFFFFFFFF;
                        int slot = Tamer.Inventory.Add(e);
                        if (slot != -1)
                        {
                            client.Send(new CashWHItem(slot, e, e.Count, e.Max));
                        }
                        break;
                    }
                case "test":
                    {
                        int value = 0;
                        int.TryParse(cmd[1], out value);
                        uint id = (uint)(0x29A9C000 + Rand.Next(0, 255) + ((value * 0x80) << 8));
                        GameMap cMap = Maps[Tamer.Location.Map];
                        cMap.Send(new SpawnObject(
                            16442, Tamer.Location.PosX, Tamer.Location.PosY,
                            Tamer.Location.PosX, Tamer.Location.PosY, id, Tamer.Location.PosX, Tamer.Location.PosY,
                            Tamer.DigimonHandle, Tamer.Partner.Location.PosX, Tamer.Partner.Location.PosY
                            ));
                        break;
                    }
                case "reload":
                    {
                        client.Send(new MapChange(Properties.Settings.Default.Host, Properties.Settings.Default.Port,
                            Tamer.Location.Map, Tamer.Location.PosX, Tamer.Location.PosY, Tamer.Location.MapName));
                        break;
                    }
                case "save":
                    {
                        client.Send(new BaseChat(ChatType.Shout, "ADMIN", "Saving..."));
                        SqlDB.SaveTamer(client);
                        break;
                    };
                case "ann":
                    {
                        client.Send(new BaseChat((ChatType)262, "ADMIN", string.Join(" ", cmd, 1, cmd.Length - 1)));
                        SqlDB.SaveTamer(client);
                        break;
                    };
                case "map":
                    {
                        int mapId = int.Parse(cmd[1]);
                        int X = int.Parse(cmd[2]);
                        int Y = int.Parse(cmd[3]);
                        MapData Map = MapDB.GetMap(mapId);
                        Tamer.Location = new Position(mapId, X, Y);
                        SqlDB.SaveTamerPosition(client);
                        client.Send(new MapChange(Properties.Settings.Default.Host, Properties.Settings.Default.Port, mapId, X, Y, Map.Name));
                        break;
                    };
                case "tele":
                    {
                        Position p = null;
                        switch (cmd[1].ToLower())
                        {
                            case "dats":
                            default:
                                p = new Position(1, 29877, 22184);
                                break;
                        }
                        if (p != null)
                        {
                            SqlDB.SaveTamerPosition(client);
                            Tamer.Location = p;
                            client.Send(new MapChange(Properties.Settings.Default.Host, Properties.Settings.Default.Port, p.Map, p.PosX, p.PosY, p.MapName));
                        }
                        break;
                    }
                case "where":
                case "pos":
                case "loc":
                    {
                        client.Send(new ChatNormal(Tamer.DigimonHandle, string.Format("You are at {0}", Tamer.Location)));
                        break;
                    }
                case "merc":
                    {
                        int value = 0;
                        if (!int.TryParse(cmd[1], out value)) return;

                        DigimonData dData = DigimonDB.GetDigimon(value);
                        if (dData == null)
                            client.Send(new ChatNormal(Tamer.DigimonHandle, string.Format("Mercenary id {0} was not found.", value)));
                        else
                            client.Send(new ChatNormal(Tamer.DigimonHandle, string.Format("Mercenary {1} Found: {0}", dData.DisplayName, value)));
                        break;
                    }
                case "mk":
                    {
                        if (cmd.Length < 3) return;
                        int value = 0;
                        if (!int.TryParse(cmd[2], out value)) return;

                        DigimonData dData = DigimonDB.GetDigimon(value);
                        if (dData == null)
                            return;
                        int digiId = SqlDB.CreateMercenary((int)client.Tamer.CharacterId, cmd[1], value, 5, 14000);
                        if (digiId == -1)
                        {
                            client.Send(new ChatNormal(Tamer.DigimonHandle, "Failed to create mercenary."));
                            return;
                        }
                        else
                        {
                            for (int i = 0; i < Tamer.ArchivedDigimon.Length; i++)
                            {
                                if (Tamer.ArchivedDigimon[i] != 0) continue;
                                Tamer.ArchivedDigimon[i] = (uint)digiId;
                                break;
                            }
                            client.Send(new ChatNormal(Tamer.DigimonHandle, "Mercenary added to Digimon Archive."));
                        }
                        break;
                    }
                case "self.setav":
                case "tamer.setav":
                    {
                        int value = 0;
                        if (!int.TryParse(cmd[2], out value)) return;
                        switch (cmd[1].ToLower())
                        {
                            case "level":
                            case "lv":
                                Tamer.Level = value; break;
                            case "at":
                                Tamer.AT = value; break;
                            case "de":
                                Tamer.DE = value; break;
                            case "hp":
                                Tamer.MaxHP = value;
                                Tamer.HP = value; break;
                            case "ds":
                                Tamer.MaxDS = value;
                                Tamer.DS = value; break;
                            case "fatigue":
                                Tamer.Fatigue = value; break;
                            case "ms":
                                Tamer.MS = (short)value; break;
                            case "tamer":
                                Tamer.Model = (CharacterModel)value; break;
                            case "archive":
                                Tamer.ArchiveSize = value; break;
                            case "inv":
                                Tamer.InventorySize = value; break;
                            case "storage":
                                Tamer.StorageSize = value; break;
                        }
                        client.Send(new UpdateStats(Tamer, Tamer.Partner));
                        break;
                    }
                case "mon.setav":
                case "digimon.setav":
                    {
                        if (cmd[1].ToLower() == "min")
                        {
                            Tamer.Partner.Stats = new DigimonStats();
                            Tamer.Partner.Level = 1;
                            Tamer.Partner.EXP = 0;
                            client.Send(new UpdateStats(Tamer, Tamer.Partner));
                            return;
                        }
                        else if (cmd[1].ToLower() == "max")
                        {
                            Tamer.Partner.Stats.Max();
                            Tamer.Level = 99;
                            client.Send(new UpdateStats(Tamer, Tamer.Partner));
                            return;
                        }
                        else if (cmd[1].ToLower() == "default")
                        {
                            DigimonData dData = DigimonDB.GetDigimon(Tamer.Partner.CurrentForm);
                            if (dData != null)
                            {
                                Tamer.Partner.Stats = dData.Default(Tamer, Tamer.Partner.Stats.Intimacy, Tamer.Partner.Size);
                                client.Send(new UpdateStats(Tamer, Tamer.Partner));
                            }
                        }
                        int value = 0;
                        if (!int.TryParse(cmd[2], out value)) return;
                        switch (cmd[1].ToLower())
                        {
                            case "lv":
                            case "level":
                                Tamer.Partner.Level = value;
                                break;
                            case "hp":
                                Tamer.Partner.Stats.MaxHP = (short)value;
                                Tamer.Partner.Stats.HP = (short)value;
                                break;
                            case "ds":
                                Tamer.Partner.Stats.MaxDS = (short)value; Tamer.Partner.Stats.DS = (short)value; break;
                            case "at":
                                Tamer.Partner.Stats.AT = (short)value; break;
                            case "de":
                                Tamer.Partner.Stats.DE = (short)value; break;
                            case "ev":
                                Tamer.Partner.Stats.EV = (short)value; break;
                            case "ht":
                                Tamer.Partner.Stats.HT = (short)value; break;
                            case "cr":
                                Tamer.Partner.Stats.CR = (short)value; break;
                            case "as":
                                Tamer.Partner.Stats.AS = (short)value; break;
                            case "ms":
                                Tamer.Partner.Stats.MS = (short)value; break;
                            case "int":
                            case "sync":
                            case "intimacy":
                                Tamer.Partner.Stats.Intimacy = (short)value; break;
                            case "type":
                                Tamer.Partner.Species = value;
                                Tamer.Partner.CurrentForm = value;
                                break;
                            case "name":
                                Tamer.Partner.Name = cmd[2]; break;
                            case "size":
                                Tamer.Partner.Size = (short)value;
                                client.Send(new ChangeSize(Tamer.DigimonHandle, value, 0));
                                break;
                            case "scale":
                                Tamer.Partner.Scale = (byte)value; break;
                        }
                        client.Send(new UpdateStats(Tamer, Tamer.Partner));
                        break;
                    }
            }
        }
    }
}
