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
using Digital_World.Packets.Game.Combat;

namespace Digital_World.Systems
{
    public partial class Yggdrasil
    {
        public void Command(Client client, string[] cmd)
        {
            if (client.AccessLevel <= 0) return;
            if (cmd.Length == 0) return;
            Character Tamer = client.Tamer;
            GameMap ActiveMap = null;
            if (Tamer != null && Tamer.Partner != null)
            {
                ActiveMap = Maps[client.Tamer.Location.Map];
            }
            switch (cmd[0])
            {
                case "inc":
                    {
                        client.Send(new ChatNormal(Tamer.DigimonHandle, string.Format("Incubator: Level {1} {0}", Tamer.Incubator, Tamer.IncubatorLevel)));
                        break;
                    }
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
                        e.Unknown = (short)e.Max;
                        e.Amount = 1;
                        if (cmd.Length == 3)
                        {
                            int count = 1;
                            int.TryParse(cmd[2], out count);
                            count = count > e.Max ? e.Max : count;
                            count = count < 0 ? 1 : count;
                            e.Amount = count;
                        }
                        if (cmd.Length == 9)
                        {
                            short[] shorts = new short[6];
                            for (int i = 3; i < cmd.Length; i++)
                            {
                                shorts[i - 3] = short.Parse(cmd[i]);
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
                            client.Send(new CashWHItem(slot, e, ((e.Modifier ^ 1) / 2), e.Max));
                        }
                        break;
                    }
                case "sk1":
                    {
                        Client Target = ActiveMap.Find(cmd[1]);
                        if (Target == null) return;
                        short skill = 0; short.TryParse(cmd[2], out skill);
                        client.Send(new UseSkill(Tamer.DigimonHandle,Target.Tamer.DigimonHandle, skill, 1, 9999));
                        break;
                    }
                case "hatch":
                        {
                            int fullId = 31001;
                            int.TryParse(cmd[1], out fullId);
                            Send(new BroadcastHatch(Tamer.Name, "I am a banana", fullId, 65000, 5));
                            break;
                        }
                case "sk2":
                    {
                        short skill = 0; short.TryParse(cmd[1], out skill);
                        client.Send(new UseSkill(Tamer.DigimonHandle, Tamer.TamerHandle, skill, 1, 9999));
                        break;
                    }
                case "list":
                    {
                        client.Send(new BaseChat(ChatType.Normal, Tamer.DigimonHandle, "Players on this map:"));
                        foreach (Client other in ActiveMap.Tamers)
                        {
                            client.Send(new BaseChat(ChatType.Normal, Tamer.DigimonHandle, string.Format("{0} - {1}", other.Tamer, other.Tamer.Partner)));
                        }
                        break;
                    }
                case "force":
                    {
                        foreach (Client other in ActiveMap.Tamers)
                        {
                            ActiveMap.Spawn(other);
                        }
                        break;
                    }
                case "spawn":
                    {
                        uint value = 0;
                        uint.TryParse(cmd[1], out value);
                        MDBDigimon Mob = MonsterDB.GetDigimon(value);
                        if (Mob == null)
                        {
                            client.Send(new BaseChat(ChatType.Normal, Tamer.DigimonHandle, string.Format("Mob {0} was not found.", value)));
                        }
                        uint id = GetModel((uint)(64 + (Mob.Models[0] * 128)) << 8);
                        GameMap cMap = Maps[Tamer.Location.Map];
                        cMap.Send(new SpawnObject(id, Tamer.Location.PosX, Tamer.Location.PosY));
                        break;
                    }
                case "rld":
                case "reload":
                    {
                        ActiveMap.Leave(client); 
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
                        Send(new BaseChat().Megaphone(Tamer.Name, string.Join(" ", cmd, 1, cmd.Length - 1), 402417));
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

                            ActiveMap.Leave(client); 
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
                        uint digiId = SqlDB.CreateMercenary(client.Tamer.CharacterId, cmd[1], value, 5, 14000, 100);
                        if (digiId == 0)
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
                                Tamer.Level = value;
                                ActiveMap.Send(new UpdateLevel(Tamer.TamerHandle, (byte)value));
                                break;
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
                            case "size":
                                ActiveMap.Send(new ChangeSize(Tamer.TamerHandle, value, 0));
                                break;
                            case "bits":
                                Tamer.Money = value;
                                //client.Send(new UpdateMoney());
                                break;
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
                                ActiveMap.Send(new UpdateLevel(Tamer.DigimonHandle, (byte)value));
                                break;
                            case "exp":
                                Tamer.Partner.EXP = value;
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
                                ActiveMap.Send(new ChangeSize(Tamer.DigimonHandle, value, 0));
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
