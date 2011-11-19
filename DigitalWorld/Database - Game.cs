using System;
using System.Collections.Generic;
using Digital_World.Entities;
using MySql.Data.MySqlClient;
using Digital_World.Helpers;
using Digital_World.Database;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Digital_World.Packets.Game;

namespace Digital_World
{
    public partial class SqlDB
    {
        private static Random DRand = new Random();

        public static void LoadTamer(Client client)
        {
            MySqlDataReader dr = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", Connect());
                cmd.Parameters.AddWithValue("@acct", client.AccountID);
                dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                int lastChar = -1, charId = -1;
                if (dr.HasRows && dr.Read())
                {
                    lastChar = (int)dr["lastChar"];
                    if (lastChar != -1)
                    {
                        charId = (int)dr[string.Format("char{0}", lastChar + 1)];
                    }
                }
                dr.Close();

                if (lastChar != -1)
                {
                    cmd = new MySqlCommand("SELECT * FROM `chars` WHERE `characterId` = @char", Connect());
                    cmd.Parameters.AddWithValue("@char", charId);
                    dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                    if (dr.HasRows && dr.Read())
                    {
                        Character tamer = new Character();

                        tamer.CharacterId = Convert.ToUInt32((int)dr["characterId"]);
                        tamer.AccountId = Convert.ToUInt32((int)dr["accountId"]);
                        tamer.Model = (CharacterModel)(int)dr["charModel"];
                        tamer.Name = (string)dr["charName"];
                        tamer.Level = (int)dr["charLv"];
                        tamer.InventorySize = (int)dr["inventoryLimit"];
                        tamer.StorageSize = (int)dr["storageLimit"];
                        tamer.ArchiveSize = (int)dr["archiveLimit"];
                        tamer.Location = new Helpers.Position((int)dr["map"], (int)dr["x"], (int)dr["y"]);
                        tamer.MaxHP = (int)dr["maxHP"];
                        tamer.MaxDS = (int)dr["maxDS"];
                        tamer.HP = (int)dr["HP"];
                        tamer.DS = (int)dr["DS"];
                        tamer.AT = (int)dr["AT"];
                        tamer.DE = (int)dr["DE"];
                        tamer.EXP = (int)dr["experience"];
                        tamer.MS = (int)dr["MS"];
                        tamer.Fatigue = (int)dr["Fatigue"];
                        tamer.Starter = (int)dr["starter"];
                        tamer.Money = (int)dr["money"];

                        try { tamer.Inventory = ItemList.Deserialize((byte[])dr["inventory"]); }
                        catch { tamer.Inventory = new ItemList(63); }
                        try
                        {
                            tamer.Equipment = ItemList.Deserialize((byte[])dr["equipment"]);
                        }
                        catch
                        {
                            tamer.Equipment = new ItemList(27);
                        }
                        try { tamer.Storage = ItemList.Deserialize((byte[])dr["storage"]); }
                        catch { tamer.Storage = new ItemList(70); }
                        try { tamer.Quests = QuestList.Deserialize((byte[])dr["quests"]); }
                        catch { tamer.Quests = new QuestList(); };
                        try
                        {
                            BinaryFormatter f = new BinaryFormatter();
                            tamer.ArchivedDigimon = (uint[])f.Deserialize(new MemoryStream((byte[])dr["archive"]));
                        }
                        catch { tamer.ArchivedDigimon = new uint[40]; }

                        Digimon partner = LoadDigimon((uint)(int)dr["partner"]);
                        tamer.Partner = partner;

                        if (dr["mercenary1"] != DBNull.Value)
                        {
                            int mercId = (int)dr["mercenary1"];
                            Digimon merc = LoadDigimon((uint)mercId);
                            tamer.DigimonList[1] = merc;
                        }
                        if (dr["mercenary2"] != DBNull.Value)
                        {
                            int mercId = (int)dr["mercenary2"];
                            Digimon merc = LoadDigimon((uint)mercId);
                            tamer.DigimonList[2] = merc;
                        }

                        client.Tamer = tamer;
                    }
                    dr.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (dr != null && !dr.IsClosed) dr.Close();
            }
        }

        public static Digimon LoadDigimon(uint DigiId)
        {
            if (Connection == null) Connection = Connect();
            Digimon digimon = null;
            MySqlDataReader dr = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `digimon` WHERE `digimonId` = @id"
                    , Connect());
                cmd.Parameters.AddWithValue("@id", DigiId);

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    if (dr.Read())
                    {
                        digimon = new Digimon();

                        digimon.DigiId = DigiId;
                        digimon.CharacterId = (int)dr["characterId"];
                        digimon.Name = (string)dr["digiName"];
                        digimon.Level = (int)dr["digiLv"];

                        digimon.Species = (int)dr["digiType"];
                        digimon.CurrentForm = (int)dr["digiModel"];

                        digimon.EXP = (int)dr["exp"];
                        digimon.Size = (short)(int)dr["digiSize"];
                        digimon.Scale = (int)dr["digiScale"];

                        digimon.Stats = new DigimonStats();
                        digimon.Stats.MaxHP = (short)(int)dr["maxHP"];
                        digimon.Stats.MaxDS = (short)(int)dr["maxDS"];
                        digimon.Stats.HP = (short)(int)dr["HP"];
                        digimon.Stats.DS = (short)(int)dr["DS"];
                        digimon.Stats.DE = (short)(int)dr["DE"];
                        digimon.Stats.AT = (short)(int)dr["AT"];
                        digimon.Stats.Intimacy = (short)(int)dr["sync"];
                        digimon.Stats.HT = (short)(int)dr["HT"];
                        digimon.Stats.EV = (short)(int)dr["EV"];
                        digimon.Stats.CR = (short)(int)dr["CR"];
                        digimon.Stats.MS = (short)(int)dr["MS"];
                        digimon.Stats.AS = (short)(int)dr["AS"];

                        BinaryFormatter bf = new BinaryFormatter();
                        int forms = EvolutionDB.EvolutionList[digimon.Species].Digivolutions;
                        try
                        {
                            MemoryStream m = new MemoryStream((byte[])dr["forms"]);
                            digimon.Forms = (EvolvedForms)bf.Deserialize(m);

                        }
                        catch { }
                        if (digimon.Forms.Count != forms)
                            digimon.Forms = new EvolvedForms(forms);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: GetDigimon({1})\n{0}", e, DigiId);
            }
            finally
            {
                try { dr.Close(); }
                catch { }
            }
            return digimon;
        }

        public static void SaveTamer(Client client)
        {
            MySqlDataReader dr = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", Connect());
                cmd.Parameters.AddWithValue("@acct", client.AccountID);
                dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                int lastChar = -1, charId = -1;
                if (dr.HasRows && dr.Read())
                {
                    lastChar = (int)dr["lastChar"];
                    if (lastChar != -1)
                    {
                        charId = (int)dr[string.Format("char{0}", lastChar + 1)];
                    }
                }
                dr.Close();

                if (lastChar != -1)
                {
                    Character Tamer = client.Tamer;
                    cmd = new MySqlCommand("UPDATE `chars` SET " +
                        "`charModel` = @model,`charName` = @name,  `charLv` = @lv,  `inventoryLimit` = @invlim,  `storageLimit` = @stolim,  `archiveLimit` = @arclim,  `map` = @map,  `x` = @x,  `y` = @y,  `maxHP` = @mhp,  `maxDS` = @mds,  `AT` = @at,  `DE` = @de, " +
                        "`archive` = @archive, `partner` = @partner, `mercenary1` = @merc1, `mercenary2` = @merc2, `HP` = @hp, `DS` = @ds, `experience` = @exp,  `MS` = @ms,  `Fatigue` = @fatigue,  `money` = @money,  `inventory` = @inv,  `equipment` = @eq,  `storage` = @storage,  `quests` = @quests" +
                        " WHERE `characterId` = @char", Connect());
                    cmd.Parameters.AddWithValue("@char", charId);
                    cmd.Parameters.AddWithValue("@model", (int)Tamer.Model);

                    cmd.Parameters.AddWithValue("@name", Tamer.Name);
                    cmd.Parameters.AddWithValue("@lv", Tamer.Level);

                    cmd.Parameters.AddWithValue("@partner", Tamer.DigimonList[0].DigiId);
                    if (Tamer.DigimonList[1] != null)
                        cmd.Parameters.AddWithValue("@merc1", Tamer.DigimonList[1].DigiId);
                    else
                        cmd.Parameters.AddWithValue("@merc1", null);
                    if (Tamer.DigimonList[2] != null)
                        cmd.Parameters.AddWithValue("@merc2", Tamer.DigimonList[2].DigiId);
                    else
                        cmd.Parameters.AddWithValue("@merc2", null);

                    cmd.Parameters.AddWithValue("@invlim", Tamer.InventorySize);
                    cmd.Parameters.AddWithValue("@stolim", Tamer.StorageSize);
                    cmd.Parameters.AddWithValue("@arclim", Tamer.ArchiveSize);

                    cmd.Parameters.AddWithValue("@map", Tamer.Location.Map);
                    cmd.Parameters.AddWithValue("@x", Tamer.Location.PosX);
                    cmd.Parameters.AddWithValue("@y", Tamer.Location.PosY);

                    cmd.Parameters.AddWithValue("@mhp", Tamer.MaxHP);
                    cmd.Parameters.AddWithValue("@mds", Tamer.MaxDS);
                    cmd.Parameters.AddWithValue("@hp", Tamer.HP);
                    cmd.Parameters.AddWithValue("@ds", Tamer.DS);
                    cmd.Parameters.AddWithValue("@at", Tamer.AT);
                    cmd.Parameters.AddWithValue("@de", Tamer.DE);
                    cmd.Parameters.AddWithValue("@ms", Tamer.MS);
                    cmd.Parameters.AddWithValue("@fatigue", Tamer.Fatigue);

                    cmd.Parameters.AddWithValue("@money", Tamer.Money);
                    cmd.Parameters.AddWithValue("@exp", Tamer.EXP);

                    cmd.Parameters.AddWithValue("@inv", Tamer.Inventory.Serialize());
                    cmd.Parameters.AddWithValue("@storage", Tamer.Storage.Serialize());
                    cmd.Parameters.AddWithValue("@eq", Tamer.Equipment.Serialize());
                    cmd.Parameters.AddWithValue("@quests", Tamer.Quests.Serialize());

                    BinaryFormatter f = new BinaryFormatter();
                    MemoryStream m = new MemoryStream();
                    f.Serialize(m, Tamer.ArchivedDigimon);
                    cmd.Parameters.AddWithValue("@archive", m.ToArray());
                    m.Close();
                    
                    cmd.ExecuteNonQuery();

                    for (int i = 0; i < Tamer.DigimonList.Length; i++)
                    {
                        if (Tamer.DigimonList[i] != null)
                            SaveDigimon(Tamer.DigimonList[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (dr != null && !dr.IsClosed) dr.Close();
            }
        }

        public static void SaveDigimon(Digimon digimon)
        {
            if (Connection == null) Connection = Connect();
            MySqlDataReader dr = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE `digimon` SET " +
                    "`digiName` = @name, `digiLv` = @lv, `exp` = @exp, `digiSize` = @size, `digiScale` = @scale, `maxHP` = @mhp, `maxDS` = @mds, `HP` = @hp, `DS` = @ds, `AT` = @at, `DE` = @de, `sync` = @sync, " +
                    "`HT` = @ht, `EV` = @ev, `CR` = @cr, `MS` = @ms, `AS` = @as, `forms` = @forms, `digiModel` = @model" +
                    " WHERE `digimonId` = @id"
                    , Connect());
                cmd.Parameters.AddWithValue("@id", digimon.DigiId);
                cmd.Parameters.AddWithValue("@model", digimon.CurrentForm);

                cmd.Parameters.AddWithValue("@name", digimon.Name);
                cmd.Parameters.AddWithValue("@lv", digimon.Level);
                cmd.Parameters.AddWithValue("@exp", digimon.EXP);
                cmd.Parameters.AddWithValue("@size", digimon.Size);
                cmd.Parameters.AddWithValue("@scale", digimon.Scale);

                cmd.Parameters.AddWithValue("@mhp", digimon.Stats.MaxHP);
                cmd.Parameters.AddWithValue("@mds", digimon.Stats.MaxDS);
                cmd.Parameters.AddWithValue("@hp", digimon.Stats.HP);
                cmd.Parameters.AddWithValue("@ds", digimon.Stats.DS);
                cmd.Parameters.AddWithValue("@at", digimon.Stats.AT);
                cmd.Parameters.AddWithValue("@de", digimon.Stats.DE);
                cmd.Parameters.AddWithValue("@sync", digimon.Stats.Intimacy);
                cmd.Parameters.AddWithValue("@ht", digimon.Stats.HT);
                cmd.Parameters.AddWithValue("@ev", digimon.Stats.EV);
                cmd.Parameters.AddWithValue("@cr", digimon.Stats.CR);
                cmd.Parameters.AddWithValue("@ms", digimon.Stats.MS);
                cmd.Parameters.AddWithValue("@as", digimon.Stats.AS);

                cmd.Parameters.AddWithValue("@forms", digimon.Forms.Serialize());

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: GetDigimon({1})\n{0}", e, digimon);
            }
            finally
            {
                try { dr.Close(); }
                catch { }
            }
        }

        public static void SaveTamerPosition(Client client)
        {
            MySqlDataReader dr = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", Connect());
                cmd.Parameters.AddWithValue("@acct", client.AccountID);
                dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                int lastChar = -1, charId = -1;
                if (dr.HasRows && dr.Read())
                {
                    lastChar = (int)dr["lastChar"];
                    if (lastChar != -1)
                    {
                        charId = (int)dr[string.Format("char{0}", lastChar + 1)];
                    }
                }
                dr.Close();

                if (lastChar != -1)
                {
                    Character Tamer = client.Tamer;
                    cmd = new MySqlCommand("UPDATE `chars` SET " +
                        "`map` = @map,  `x` = @x,  `y` = @y" +
                        " WHERE `characterId` = @char", Connect());
                    cmd.Parameters.AddWithValue("@char", charId);

                    cmd.Parameters.AddWithValue("@map", Tamer.Location.Map);
                    cmd.Parameters.AddWithValue("@x", Tamer.Location.PosX);
                    cmd.Parameters.AddWithValue("@y", Tamer.Location.PosY);

                    cmd.ExecuteNonQuery();

                    for (int i = 0; i < Tamer.DigimonList.Length; i++)
                    {
                        if (Tamer.DigimonList[i] != null)
                            SaveDigimon(Tamer.DigimonList[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (dr != null && !dr.IsClosed) dr.Close();
            }
        }

        public static int CreateMercenary(int charId, string digiName, int digiModel, int digiScale, int digiSize)
        {
            int digiId = -1;
            MySqlDataReader read = null;
            try
            {
                DigimonData dData = DigimonDB.GetDigimon(digiModel);

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO `digimon` (`digiName`,`digiType`, `characterId`, `digiModel`, `maxHP`, `HP`, `maxDS`, `DS`, `DE`, `AT`, `sync`, `HT`, `EV`, `CR`, `MS`, `AS`, `digiScale`, `digiSize`)" +
                    "VALUES (@digiName, @digiModel, @char, @digiModel, @hp, @hp, @ds, @ds, @de, @at, @sync, @ht, @ev, @cr, @ms, @as, @scale,@size)"
                    , Connect());

                cmd.Parameters.AddWithValue("@digiName", digiName);
                cmd.Parameters.AddWithValue("@digiModel", digiModel);
                cmd.Parameters.AddWithValue("@char", charId);

                cmd.Parameters.AddWithValue("@scale", digiScale);
                cmd.Parameters.AddWithValue("@size", digiSize);

                cmd.Parameters.AddWithValue("@hp", dData.HP);
                cmd.Parameters.AddWithValue("@ds", dData.DS);

                cmd.Parameters.AddWithValue("@de", dData.DE);
                cmd.Parameters.AddWithValue("@at", dData.AT);
                cmd.Parameters.AddWithValue("@sync", 0);
                cmd.Parameters.AddWithValue("@ht", dData.HT);
                cmd.Parameters.AddWithValue("@ev", dData.EV);
                cmd.Parameters.AddWithValue("@cr", dData.CR);
                cmd.Parameters.AddWithValue("@ms", dData.MS);
                cmd.Parameters.AddWithValue("@as", dData.AS);

                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(
                    "SELECT * FROM `digimon` WHERE `characterId` = @charId AND `digiName` = @charName",
                    Connect());
                cmd.Parameters.AddWithValue("@charId", charId);
                cmd.Parameters.AddWithValue("@charName", digiName);
                read = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                if (read.HasRows && read.Read())
                {
                    digiId = (int)read["digimonId"];
                }
                read.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: CreateDigimon()\n{0}", e);
            }
            finally
            {
                try { read.Close(); }
                catch { }
            }
            return digiId;
        }
    }
}
