using System;
using System.Collections.Generic;
using Digital_World.Entities;
using MySql.Data.MySqlClient;
using Digital_World.Helpers;
using Digital_World.Database;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Digital_World
{
    public partial class SqlDB
    {
        public static byte[][] TamerData = new byte[][] { new byte[] { 10, 2, 90, 80 }, new byte[] { 8, 1, 100, 90 }, new byte[] { 8, 1, 90, 100 } };
        /// <summary>
        /// Checks the database if name is available
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns>True if name is available; otherwise false.</returns>
        public static bool NameAvail(string name)
        {
            if (Connection == null) Connection = Connect();
            bool avail = false;
            try
            {
                using (MySqlConnection con = Connect())
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `chars` WHERE `charName` = @name", con))
                {
                    cmd.Parameters.AddWithValue("@name", name);

                    using (MySqlDataReader read = cmd.ExecuteReader())
                    {
                        if (read.HasRows) avail = false;
                        else avail = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: NameAvail({1})\n{0}", e, name);
            }
            return avail;
        }

        public static List<Character> GetCharacters(uint AcctId)
        {
            List<Character> chars = new List<Character>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `chars` WHERE `accountId` = @id"
                    , Connect()))
                {
                    cmd.Parameters.AddWithValue("@id", AcctId);

                   using(MySqlDataReader dr = cmd.ExecuteReader())
                   {
                       if (dr.HasRows)
                       {
                           while (dr.Read())
                           {
                               Character Tamer = new Character();
                               Tamer.AccountId = AcctId;
                               Tamer.CharacterId = Convert.ToUInt32((int)dr["characterId"]); ;
                               Tamer.Model = (CharacterModel)(int)dr["charModel"];
                               Tamer.Name = (string)dr["charName"];
                               Tamer.Level = (int)dr["charLv"];
                               Tamer.Location = new Helpers.Position((short)(int)dr["map"], (int)dr["x"], (int)dr["y"]);

                               Tamer.Partner = GetDigimon((uint)(int)dr["partner"]);
                               if (dr["mercenary1"] != DBNull.Value)
                               {
                                   int mercId = (int)dr["mercenary1"];
                                   Digimon merc = GetDigimon((uint)mercId);
                                   Tamer.DigimonList[1] = merc;
                               }
                               if (dr["mercenary2"] != DBNull.Value)
                               {
                                   int mercId = (int)dr["mercenary2"];
                                   Digimon merc = GetDigimon((uint)mercId);
                                   Tamer.DigimonList[2] = merc;
                               }

                               try
                               {
                                   BinaryFormatter f = new BinaryFormatter();
                                   using (MemoryStream m = new MemoryStream((byte[])dr["archive"]))
                                   {
                                       Tamer.ArchivedDigimon = (uint[])f.Deserialize(m);
                                   }
                                   for (int i = 0; i < Tamer.ArchivedDigimon.Length; i++)
                                   {
                                       if (Tamer.ArchivedDigimon[i] != 0)
                                       {
                                           Digimon digi = GetDigimon(Tamer.ArchivedDigimon[i]);
                                           ResetModel(digi.DigiId, digi.Species);
                                       }
                                   }

                               }
                               catch { Tamer.ArchivedDigimon = new uint[40]; }

                               try
                               {
                                   Tamer.Equipment = ItemList.Deserialize((byte[])dr["equipment"]);
                               }
                               catch
                               {
                                   Tamer.Equipment = new ItemList(27);
                               }

                               chars.Add(Tamer);
                           }
                       }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: GetCharacters\n{0}", e);
            }

            return chars;
        }

        public static Digimon GetDigimon(uint DigiId)
        {
            Digimon digimon = null;
            try
            {
                using (MySqlConnection mysql = Connect())
                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `digimon` WHERE `digimonId` = @id"
                    , mysql))
                {
                    cmd.Parameters.AddWithValue("@id", DigiId);

                    using (MySqlDataReader read = cmd.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            if (read.Read())
                            {
                                digimon = new Digimon();
                                digimon.DigiId = DigiId;
                                digimon.CharacterId = (int)read["characterId"];
                                digimon.Name = (string)read["digiName"];
                                digimon.Level = (int)read["digiLv"];
                                digimon.Species = (int)read["digiType"];
                                digimon.CurrentForm = digimon.Species;
                                digimon.Size = (short)(int)read["digiSize"];
                                digimon.Scale = (int)read["digiScale"];

                                ResetModel(DigiId, digimon.Species);
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

        public static void ResetModel(uint DigiId, int digiType)
        {
            try
            {
                using (MySqlConnection mysql = Connect())
                using (MySqlCommand cmd = new MySqlCommand("UPDATE `digimon` SET `digiModel` = @type WHERE `digimonId` = @id", mysql))
                {
                    cmd.Parameters.AddWithValue("@id", DigiId);
                    cmd.Parameters.AddWithValue("@type", digiType);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error: ResetModel({1}, {2})\n{0}", e, DigiId, digiType);
            }
        }

        public static Position GetTamerPosition(uint acctId, int slot)
        {
            Position Location = null;
            int charId = -1;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", Connect()))
                {
                    cmd.Parameters.AddWithValue("@acct", acctId);
                    using (MySqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (dr.HasRows && dr.Read())
                        {
                            if (slot != -1)
                            {
                                charId = (int)dr[string.Format("char{0}", slot + 1)];
                            }
                        }
                    }
                }

                if (slot != -1)
                {
                    using (MySqlCommand cmd = new MySqlCommand("SELECT `map`,`x`,`y` FROM `chars` WHERE `characterId` = @char", Connect()))
                    {
                        cmd.Parameters.AddWithValue("@char", charId);
                        using (MySqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {

                            if (dr.HasRows && dr.Read())
                            {
                                Location = new Helpers.Position((int)dr["map"], (int)dr["x"], (int)dr["y"]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Location;
        }

        public static uint CreateDigimon(uint charId, string digiName, int digiModel)
        {
            uint digiId = 0;
            try
            {
                digiId = CreateMercenary(charId, digiName, digiModel, 0, 10000, 5);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: CreateDigimon()\n{0}", e);
            }
            return digiId;
        }

        public static int CreateCharacter(uint AcctId, int pos, int charModel, string charName, int digiModel)
        {
            int charId = -1;
            try
            {
                using (MySqlConnection con = Connect())
                {
                    Query qry = new Query(Query.QueryMode.INSERT,"chars");
                    qry.Add("charName", charName);
                    qry.Add("charModel", charModel);
                    qry.Add("accountId", AcctId);

                    qry.Add("inventory", new ItemList(63).Serialize());
                    qry.Add("storage", new ItemList(70).Serialize());
                    qry.Add("equipment", new ItemList(27).Serialize());
                    qry.Add("quests", new QuestList().Serialize());

                    qry.Add("maxHP", TamerData[charModel - 80001][2]);
                    qry.Add("maxDS", TamerData[charModel - 80001][3]);
                    qry.Add("HP", TamerData[charModel - 80001][2]);
                    qry.Add("DS", TamerData[charModel - 80001][3]);
                    qry.Add("AT", TamerData[charModel - 80001][0]);
                    qry.Add("DE", TamerData[charModel - 80001][1]);

                    using (MySqlCommand cmd = qry.GetCommand(con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                using (MySqlConnection con = Connect())
                {
                    using (MySqlCommand cmd = new MySqlCommand(
                           "SELECT * FROM `chars` WHERE `accountId` = @acctId AND `charName` = @charName",
                           con))
                    {
                        cmd.Parameters.AddWithValue("@acctId", AcctId);
                        cmd.Parameters.AddWithValue("@charName", charName);

                        using (MySqlDataReader read = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {
                            if (read.HasRows)
                            {
                                if (read.Read())
                                {
                                    charId = (int)read["characterId"];
                                }
                            }
                        }
                    }
                }
                using (MySqlConnection con = Connect())
                {
                    using (MySqlCommand
                        cmd = new MySqlCommand(
                            string.Format("UPDATE `acct` SET `char{0}` = @charId WHERE `accountId` = @acct", pos + 1),
                            con))
                    {
                        cmd.Parameters.AddWithValue("@charId", charId);
                        cmd.Parameters.AddWithValue("@acct", AcctId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: CreateCharacter()\n{0}", e);
            }
            return charId;
        }

        public static void SetPartner(int charId, int digiId)
        {
            try
            {
                using (MySqlConnection con = Connect())
                using (MySqlCommand cmd = new MySqlCommand(
                "UPDATE `chars` SET `partner` = @digiId WHERE `characterId` = @charId", con))
                {
                    cmd.Parameters.AddWithValue("@digiId", digiId);
                    cmd.Parameters.AddWithValue("@charId", charId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: SetPartner()\n{0}", e);
            }
        }

        public static void SetTamer(int charId, int digiId)
        {
            try
            {
                using (MySqlConnection con = Connect())
                using (MySqlCommand cmd = new MySqlCommand(
                    "UPDATE `digimon` SET `characterId` = @charId WHERE `digimonId` = @digiId", Connection))
                {
                    cmd.Parameters.AddWithValue("@digiId", digiId);
                    cmd.Parameters.AddWithValue("@charId", charId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: SetTamer()\n{0}", e);
            }
        }

        public static bool VerifyCode(uint acctId, string code)
        {
            bool allow = false;
            try
            {
                using (MySqlConnection con = Connect())
                {
                    using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT `email` from `acct` WHERE `accountId` = @acct",
                    con))
                    {
                        cmd.Parameters.AddWithValue("@acct", acctId);
                        using (MySqlDataReader read = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {
                            if (read.HasRows && read.Read())
                            {
                                if (code.Equals((string)read["email"]))
                                    allow = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: VerifyCode()\n{0}", e);
            }
            return allow;
        }

        /// <summary>
        /// Deletes a Tamer and unbinds the emptied slot from account
        /// </summary>
        /// <param name="acctId">Id of the account to delete from</param>
        /// <param name="slot">The slot to delete</param>
        public static bool DeleteTamer(uint acctId, int slot)
        {
            int charId = -1;
            bool completed = false;
            try
            {
                string sSlot = string.Format("char{0}", slot + 1);
                using (MySqlCommand cmd = new MySqlCommand(
                    string.Format("SELECT `char{0}` FROM `acct` WHERE `accountId` = @acct", slot + 1), Connect()))
                {
                    cmd.Parameters.AddWithValue("@acct", acctId);
                    using (MySqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {

                        if (dr.HasRows && dr.Read())
                        {
                            charId = (int)dr[sSlot];
                        }
                    }
                }
                if (charId != -1)
                {

                    DeleteDigimons(charId);

                    using (MySqlCommand cmd = new MySqlCommand("DELETE FROM `chars` WHERE `characterId` = @char", Connection))
                    {
                        cmd.Parameters.AddWithValue("@char", charId);
                        cmd.ExecuteNonQuery();
                    }
                    using (MySqlCommand cmd = new MySqlCommand(
                        string.Format("UPDATE `acct` SET `char{0}` = NULL WHERE `accountId` = @acct", slot + 1)
                        , Connection))
                    {
                        cmd.Parameters.AddWithValue("@acct", acctId);
                        cmd.ExecuteNonQuery();
                    }
                    completed = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: DeleteTamer()\n{0}", e);
            }
            return completed;
        }

        /// <summary>
        /// Deletes all Digimon belonging to the character with the id charId
        /// </summary>
        /// <param name="charId">Id of the character whose Digimon to delete</param>
        public static void DeleteDigimons(int charId)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(
                    "DELETE FROM `digimon` WHERE `characterId` = @char"
                    , Connection))
                {
                    cmd.Parameters.AddWithValue("@char", charId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: DeleteDigimon()\n{0}", e);
            }
        }

        /// <summary>
        /// Update the last character used
        /// </summary>
        /// <param name="acctId">Id of the account to update</param>
        /// <param name="slot">Slot last used</param>
        public static void SetLastChar(uint acctId, int slot)
        {
            try
            {
                using (MySqlConnection Connection = Connect())
                using (MySqlCommand cmd = new MySqlCommand("UPDATE `acct` SET `lastChar` = @char WHERE `accountId` = @acct",
                    Connection))
                {
                    cmd.Parameters.AddWithValue("@acct", acctId);
                    cmd.Parameters.AddWithValue("@char", slot);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: SetLastChar()\n{0}", e);
            }
        }
    }
}
