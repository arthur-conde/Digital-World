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
            int lastChar = -1, charId = -1;
            try
            {
                using (MySqlConnection con = Connect())
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", con))
                {
                    cmd.Parameters.AddWithValue("@acct", client.AccountID);
                    using (MySqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (dr.HasRows && dr.Read())
                        {
                            lastChar = (int)dr["lastChar"];
                            if (lastChar != -1)
                            {
                                charId = (int)dr[string.Format("char{0}", lastChar + 1)];
                            }
                        }
                    }
                }

                if (lastChar != -1)
                {
                    using (MySqlConnection con = Connect())
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `chars` WHERE `characterId` = @char", Connect()))
                    {
                        cmd.Parameters.AddWithValue("@char", charId);
                        using (MySqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {

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

                                //Incubator
                                tamer.Incubator = (int)dr["incubator"];
                                tamer.IncubatorLevel = (int)dr["incubatorLevel"];
                                if (tamer.Incubator == 0) tamer.IncubatorLevel = 0;

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
                                    using (MemoryStream m = new MemoryStream((byte[])dr["archive"]))
                                        tamer.ArchivedDigimon = (uint[])f.Deserialize(m);
                                }
                                catch { tamer.ArchivedDigimon = new uint[40]; }

                                Digimon partner = LoadDigimon((uint)(int)dr["partner"]);
                                tamer.Partner = partner;
                                tamer.Partner.Location = tamer.Location.Clone(); ;

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
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static Digimon LoadDigimon(uint DigiId)
        {
            Digimon digimon = null;
            try
            {
                using (MySqlConnection con = Connect())
                {
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `digimon` WHERE `digimonId` = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", DigiId);

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
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
                                        using (MemoryStream m = new MemoryStream((byte[])dr["forms"]))
                                            digimon.Forms = (EvolvedForms)bf.Deserialize(m);

                                    }
                                    catch { }
                                    if (digimon.Forms.Count != forms)
                                        digimon.Forms = new EvolvedForms(forms);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: GetDigimon({1})\n{0}", e, DigiId);
            }
            return digimon;
        }

        public static void SaveTamer(Client client)
        {
            int lastChar = -1, charId = -1;

            try
            {
                using (MySqlConnection con = Connect())
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", con))
                {
                    cmd.Parameters.AddWithValue("@acct", client.AccountID);
                    using (MySqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        if (dr.HasRows && dr.Read())
                        {
                            lastChar = (int)dr["lastChar"];
                            if (lastChar != -1)
                            {
                                charId = (int)dr[string.Format("char{0}", lastChar + 1)];
                            }
                        }
                }

                if (lastChar != -1)
                {
                    Character Tamer = client.Tamer;
                    using (MySqlConnection con = Connect())
                    {
                        Query qry = new Query(Query.QueryMode.UPDATE, "chars", new Tuple<string, object>("characterId", Tamer.CharacterId));
                        qry.Add("charModel", (int)Tamer.Model);
                        qry.Add("charName", Tamer.Name);
                        qry.Add("charLv", Tamer.Level);
                        qry.Add("experience", Tamer.EXP);
                        qry.Add("money", Tamer.Money);

                        qry.Add("partner", Tamer.DigimonList[0].DigiId);
                        if (Tamer.DigimonList[1] == null) qry.Add("mercenary1", null);
                        else qry.Add("mercenary1", Tamer.DigimonList[1].DigiId);
                        if (Tamer.DigimonList[2] == null) qry.Add("mercenary2", null);
                        else qry.Add("mercenary2", Tamer.DigimonList[2].DigiId);

                        qry.Add("map", Tamer.Location.Map);
                        qry.Add("x", Tamer.Location.PosX);
                        qry.Add("y", Tamer.Location.PosY);

                        qry.Add("inventoryLimit", Tamer.InventorySize);
                        qry.Add("storageLimit", Tamer.StorageSize);
                        qry.Add("archiveLimit", Tamer.ArchiveSize);

                        qry.Add("maxHP", Tamer.MaxHP);
                        qry.Add("maxDS", Tamer.MaxDS);
                        qry.Add("HP", Tamer.HP);
                        qry.Add("DS", Tamer.DS);
                        qry.Add("AT", Tamer.AT);
                        qry.Add("DE", Tamer.DE);
                        qry.Add("MS", Tamer.MS);
                        qry.Add("Fatigue", Tamer.Fatigue);

                        qry.Add("incubator", Tamer.Incubator);
                        qry.Add("incubatorLevel", Tamer.IncubatorLevel);

                        BinaryFormatter f = new BinaryFormatter();
                        using (MemoryStream m = new MemoryStream())
                        {
                            f.Serialize(m, Tamer.ArchivedDigimon);
                            qry.Add("archive", m.ToArray());
                        }

                        qry.Add("inventory", Tamer.Inventory.Serialize());
                        qry.Add("equipment", Tamer.Equipment.Serialize());
                        qry.Add("storage", Tamer.Storage.Serialize());
                        qry.Add("quests", Tamer.Quests.Serialize());

                        using (MySqlCommand cmd = qry.GetCommand(con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
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
        }

        public static void SaveDigimon(Digimon digimon)
        {
            try
            {
                using (MySqlConnection connection = Connect())
                {
                    Query qry = new Query(Query.QueryMode.UPDATE, "digimon", new Tuple<string, object>("digimonId", digimon.DigiId));
                    qry.Add("digiModel", digimon.CurrentForm);

                    qry.Add("digiName", digimon.Name);
                    qry.Add("digiLv", digimon.Level);
                    qry.Add("exp", digimon.EXP);
                    qry.Add("digiSize", digimon.Size);

                    qry.Add("maxHP", digimon.Stats.MaxHP);
                    qry.Add("maxDS", digimon.Stats.MaxDS);
                    qry.Add("HP", digimon.Stats.HP);
                    qry.Add("DS", digimon.Stats.DS);
                    qry.Add("AT", digimon.Stats.AT);
                    qry.Add("DE", digimon.Stats.DE);
                    qry.Add("sync", digimon.Stats.Intimacy);
                    qry.Add("HT", digimon.Stats.HT);
                    qry.Add("EV", digimon.Stats.EV);
                    qry.Add("CR", digimon.Stats.CR);
                    qry.Add("MS", digimon.Stats.MS);

                    qry.Add("forms", digimon.Forms.Serialize());

                    using (MySqlCommand cmd = qry.GetCommand(connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: GetDigimon({1})\n{0}", e, digimon);
            }
        }

        public static void SaveTamerPosition(Client client)
        {
            int lastChar = -1, charId = -1;
            try
            {
                using (MySqlConnection con = Connect())
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", con))
                {
                    cmd.Parameters.AddWithValue("@acct", client.AccountID);

                    using (MySqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (dr.HasRows && dr.Read())
                        {
                            lastChar = (int)dr["lastChar"];
                            if (lastChar != -1)
                            {
                                charId = (int)dr[string.Format("char{0}", lastChar + 1)];
                            }
                        }
                    }
                }
                if (lastChar != -1)
                {
                    Character Tamer = client.Tamer;
                    using (MySqlConnection con = Connect())
                    {
                        Query qry = new Query(Query.QueryMode.UPDATE, "chars",new Tuple<string,object>("characterId", Tamer.CharacterId));
                        qry.Add("map", Tamer.Location.Map);
                        qry.Add("x", Tamer.Location.PosX);
                        qry.Add("y", Tamer.Location.PosY);
                        using (MySqlCommand cmd = qry.GetCommand(con))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        for (int i = 0; i < Tamer.DigimonList.Length; i++)
                        {
                            if (Tamer.DigimonList[i] != null)
                                SaveDigimon(Tamer.DigimonList[i]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static uint CreateMercenary(uint charId, string digiName, int digiModel, int digiScale, int digiSize, int intimacy)
        {
            uint digiId = 0;
            try
            {
                DigimonData dData = DigimonDB.GetDigimon(digiModel);
                using (MySqlConnection con = Connect())
                {
                    Query qry = new Query(Query.QueryMode.INSERT, "digimon");
                    qry.Add("digiName", digiName);
                    qry.Add("digiModel", digiModel);
                    qry.Add("digiType", digiModel);
                    qry.Add("characterId", charId);

                    qry.Add("digiScale", digiScale);
                    qry.Add("digiSize", digiSize);

                    qry.Add("maxHP", dData.HP);
                    qry.Add("maxDS", dData.DS);
                    qry.Add("HP", dData.HP);
                    qry.Add("DS", dData.DS);

                    qry.Add("DE", dData.DE);
                    qry.Add("AT", dData.AT);
                    qry.Add("sync", intimacy);
                    qry.Add("HT", dData.HT);
                    qry.Add("EV", dData.EV);
                    qry.Add("CR", dData.CR);
                    qry.Add("MS", dData.MS);
                    qry.Add("AS", dData.AS);

                    using (MySqlCommand cmd = qry.GetCommand(con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                using (MySqlCommand
                cmd = new MySqlCommand(
                    "SELECT * FROM `digimon` WHERE `characterId` = @charId AND `digiName` = @charName",
                    Connect()))
                {
                    cmd.Parameters.AddWithValue("@charId", charId);
                    cmd.Parameters.AddWithValue("@charName", digiName);
                    using (MySqlDataReader read = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (read.HasRows && read.Read())
                        {
                            digiId = (uint)(int)read["digimonId"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: CreateDigimon()\n{0}", e);
            }
            return digiId;
        }
    }
}
