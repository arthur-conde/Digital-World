using System;
using System.Collections.Generic;
using Digital_World.Entities;
using MySql.Data.MySqlClient;
using Digital_World.Helpers;
using Digital_World.Database;

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
                        tamer.Inventory = ItemList.Deserialize((byte[])dr["inventory"]);
                        tamer.Equipment = ItemList.Deserialize((byte[])dr["equipment"]);
                        tamer.Storage = ItemList.Deserialize((byte[])dr["storage"]);
                        tamer.Quests = QuestList.Deserialize((byte[])dr["quests"]);
                        tamer.InventorySize = (int)dr["inventoryLimit"];
                        tamer.StorageSize = (int)dr["storageLimit"];
                        tamer.ArchiveSize = (int)dr["archiveLimit"];
                        tamer.Location = new Helpers.Position((short)(int)dr["map"], (int)dr["x"], (int)dr["y"]);
                        tamer.MaxHP = (int)dr["maxHP"];
                        tamer.MaxDS = (int)dr["maxDS"];
                        tamer.HP = (int)dr["HP"];
                        tamer.DS = (int)dr["DS"];
                        tamer.AT = (int)dr["AT"];
                        tamer.DE = (int)dr["DE"];
                        tamer.EXP = (int)dr["experience"];
                        tamer.MS = (int)dr["MS"];
                        tamer.Fatigue = (int)dr["Fatigue"];

                        Digimon partner = LoadDigimon((uint)(int)dr["partner"]);
                        tamer.DigimonList.Add(partner);
                        tamer.Partner = tamer.DigimonList[0];

                        if (dr["mercenary1"] != DBNull.Value)
                        {
                            int mercId = (int)dr["mercenary1"];
                            Digimon merc = LoadDigimon((uint)mercId);
                            tamer.DigimonList.Add(merc);
                        }
                        if (dr["mercenary2"] != DBNull.Value)
                        {
                            int mercId = (int)dr["mercenary2"];
                            Digimon merc = LoadDigimon((uint)mercId);
                            tamer.DigimonList.Add(merc);
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
            if (m_con == null) m_con = Connect();
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
                        digimon.DigiType = (int)dr["digiType"];
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
    }
}
