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
            MySqlDataReader read = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `chars` WHERE `charName` = @name"
                    , Connection);
                cmd.Parameters.AddWithValue("@name", name);

                read = cmd.ExecuteReader();
                if (read.HasRows) avail = false;
                else avail = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: NameAvail({1})\n{0}", e, name);
            }
            finally
            {
                try { read.Close(); }
                catch { }
            }
            return avail;
        }

        public static List<Character> GetCharacters(uint AcctId)
        {
            if (Connection == null) Connection = Connect();
            List<Character> chars = new List<Character>();
            MySqlDataReader dr = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `chars` WHERE `accountId` = @id"
                    , Connection);
                cmd.Parameters.AddWithValue("@id", AcctId);

                dr = cmd.ExecuteReader();
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
                            Tamer.ArchivedDigimon = (uint[])f.Deserialize(new MemoryStream((byte[])dr["archive"]));
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
            catch (Exception e)
            {
                Console.WriteLine("Error: GetCharacters\n{0}", e);
            }
            finally
            {
                try { dr.Close(); }
                catch { }
            }
            return chars;
        }

        public static Digimon GetDigimon(uint DigiId)
        {
            if (Connection == null) Connection = Connect();
            Digimon digimon = null;
            MySqlDataReader read = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `digimon` WHERE `digimonId` = @id"
                    , Connect());
                cmd.Parameters.AddWithValue("@id", DigiId);

                read = cmd.ExecuteReader();
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
            catch (Exception e)
            {
                Console.WriteLine("Error: GetDigimon({1})\n{0}", e, DigiId);
            }
            finally
            {
                try { read.Close(); }
                catch { }
            }
            return digimon;
        }

        public static void ResetModel(uint DigiId, int digiType)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand("UPDATE `digimon` SET `digiModel` = @type WHERE `digimonId` = @id", Connect());
                cmd.Parameters.AddWithValue("@id", DigiId);
                cmd.Parameters.AddWithValue("@type", digiType);
                cmd.ExecuteNonQuery();
            }
            catch(MySqlException e)
            {
                Console.WriteLine("Error: ResetModel({1}, {2})\n{0}", e, DigiId, digiType);
            }
        }

        public static Position GetTamerPosition(uint acctId, int slot)
        {
            MySqlDataReader dr = null;
            Position Location = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @acct", Connect());
                cmd.Parameters.AddWithValue("@acct", acctId);
                dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                int charId = -1;
                if (dr.HasRows && dr.Read())
                {
                    if (slot != -1)
                    {
                        charId = (int)dr[string.Format("char{0}", slot + 1)];
                    }
                }
                dr.Close();

                if (slot != -1)
                {
                    cmd = new MySqlCommand("SELECT `map`,`x`,`y` FROM `chars` WHERE `characterId` = @char", Connect());
                    cmd.Parameters.AddWithValue("@char", charId);
                    dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                    if (dr.HasRows && dr.Read())
                    {
                        Location = new Helpers.Position((int)dr["map"], (int)dr["x"], (int)dr["y"]);
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
            return Location;
        }

        public static int CreateDigimon(int charId, string digiName, int digiModel)
        {
            int digiId = -1;
            MySqlDataReader read = null;
            try
            {
                DigimonData dData = DigimonDB.GetDigimon(digiModel);

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO `digimon` (`digiName`,`digiType`, `characterId`, `digiModel`, `maxHP`, `HP`, `maxDS`, `DS`, `DE`, `AT`, `sync`, `HT`, `EV`, `CR`, `MS`, `AS`)" +
                    "VALUES (@digiName, @digiModel, @char, @digiModel, @hp, @hp, @ds, @ds, @de, @at, @sync, @ht, @ev, @cr, @ms, @as)"
                    , Connect());

                cmd.Parameters.AddWithValue("@digiName", digiName);
                cmd.Parameters.AddWithValue("@digiModel", digiModel);
                cmd.Parameters.AddWithValue("@char", charId);

                cmd.Parameters.AddWithValue("@hp", dData.HP);
                cmd.Parameters.AddWithValue("@ds", dData.DS);

                cmd.Parameters.AddWithValue("@de", dData.DE);
                cmd.Parameters.AddWithValue("@at", dData.AT);
                if (31001 <= digiModel && digiModel <= 31004)
                    cmd.Parameters.AddWithValue("@sync", 5);
                else
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

        public static int CreateCharacter(uint AcctId, int pos, int charModel, string charName, int digiModel)
        {
            int charId = -1;
            MySqlDataReader read = null;
            try
            {

                MySqlConnection con = Connect();
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO `chars` "+
                    "(`charName`, `charModel`, `accountId`, `inventory`, `storage`, `equipment`, `quests`, `maxHP`, `maxDS`, `HP`, `DS`,`AT`, `DE`) " +
                    "VALUES "+
                    "(@charName, @charModel, @acctId, @inv, @bank,@equip,@quest, @hp, @mp, @hp, @mp, @at, @de)"
                    , con);
                cmd.Parameters.AddWithValue("@charName", charName);
                cmd.Parameters.AddWithValue("@charModel", charModel);
                cmd.Parameters.AddWithValue("@acctId", AcctId);
                cmd.Parameters.AddWithValue("@digiModel", digiModel);
                cmd.Parameters.AddWithValue("@inv", new ItemList(63).Serialize());
                cmd.Parameters.AddWithValue("@bank", new ItemList(70).Serialize());
                cmd.Parameters.AddWithValue("@equip", new ItemList(27).Serialize());
                cmd.Parameters.AddWithValue("@quest", new QuestList().Serialize());
                switch ((CharacterModel)charModel)
                {
                    case CharacterModel.Masaru:
                        {
                            cmd.Parameters.AddWithValue("@hp", TamerData[0][2]);
                            cmd.Parameters.AddWithValue("@mp", TamerData[0][3]);
                            cmd.Parameters.AddWithValue("@at", TamerData[0][0]);
                            cmd.Parameters.AddWithValue("@de", TamerData[0][1]);
                            break;
                        }
                    case CharacterModel.Tohma:
                        {
                            cmd.Parameters.AddWithValue("@hp", TamerData[1][2]);
                            cmd.Parameters.AddWithValue("@mp", TamerData[1][3]);
                            cmd.Parameters.AddWithValue("@at", TamerData[1][0]);
                            cmd.Parameters.AddWithValue("@de", TamerData[1][1]);
                            break;
                        }
                    case CharacterModel.Yoshino:
                        {
                            cmd.Parameters.AddWithValue("@hp", TamerData[2][2]);
                            cmd.Parameters.AddWithValue("@mp", TamerData[2][3]);
                            cmd.Parameters.AddWithValue("@at", TamerData[2][0]);
                            cmd.Parameters.AddWithValue("@de", TamerData[2][1]);
                            break;
                        }
                }

                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(
                    "SELECT * FROM `chars` WHERE `accountId` = @acctId AND `charName` = @charName",
                    Connect());
                cmd.Parameters.AddWithValue("@acctId", AcctId);
                cmd.Parameters.AddWithValue("@charName", charName);

                read = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                if (read.HasRows)
                {
                    if (read.Read())
                    {
                        charId = (int)read["characterId"];
                    }
                }
                read.Close();

                cmd = new MySqlCommand(
                    string.Format("UPDATE `acct` SET `char{0}` = @charId WHERE `accountId` = @acct", pos + 1),
                    Connect());
                cmd.Parameters.AddWithValue("@charId", charId);
                cmd.Parameters.AddWithValue("@acct", AcctId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: CreateCharacter()\n{0}", e);
            }
            finally
            {
                try { read.Close(); }
                catch { }
            }
            return charId;
        }

        public static void SetPartner(int charId, int digiId)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE `chars` SET `partner` = @digiId WHERE `characterId` = @charId", Connection);
                cmd.Parameters.AddWithValue("@digiId", digiId);
                cmd.Parameters.AddWithValue("@charId", charId);
                cmd.ExecuteNonQuery();
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
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE `digimon` SET `characterId` = @charId WHERE `digimonId` = @digiId", Connection);
                cmd.Parameters.AddWithValue("@digiId", digiId);
                cmd.Parameters.AddWithValue("@charId", charId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: SetTamer()\n{0}", e);
            }
        }

        public static bool VerifyCode(uint acctId, string code)
        {
            bool allow = false;
            MySqlDataReader read = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT `email` from `acct` WHERE `accountId` = @acct",
                    Connect());
                cmd.Parameters.AddWithValue("@acct", acctId);
                read = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                if (read.HasRows && read.Read())
                {
                    if (code.Equals((string)read["email"]))
                        allow = true;
                }
                read.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: VerifyCode()\n{0}", e);
            }
            finally
            {
                try { read.Close(); }
                catch { }
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
            MySqlDataReader dr = null;
            bool completed = false;
            try
            {
                string sSlot = string.Format("char{0}", slot+1);
                MySqlCommand cmd = new MySqlCommand(
                    string.Format("SELECT `char{0}` FROM `acct` WHERE `accountId` = @acct",slot+1), Connect());
                cmd.Parameters.AddWithValue("@acct", acctId);
                dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                int charId = -1;
                if (dr.HasRows && dr.Read())
                {
                    charId = (int)dr[sSlot];
                }
                dr.Close();

                if (charId != -1)
                {

                    DeleteDigimons(charId);

                    cmd = new MySqlCommand("DELETE FROM `chars` WHERE `characterId` = @char", Connection);
                    cmd.Parameters.AddWithValue("@char", charId);
                    cmd.ExecuteNonQuery();

                    cmd = new MySqlCommand(
                        string.Format("UPDATE `acct` SET `char{0}` = NULL WHERE `accountId` = @acct", slot + 1)
                        , Connection);
                    cmd.Parameters.AddWithValue("@acct", acctId);
                    cmd.ExecuteNonQuery();

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
                MySqlCommand cmd = new MySqlCommand(
                    "DELETE FROM `digimon` WHERE `characterId` = @char"
                    , Connection);
                cmd.Parameters.AddWithValue("@char", charId);
                cmd.ExecuteNonQuery();
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
                MySqlCommand cmd = new MySqlCommand("UPDATE `acct` SET `lastChar` = @char WHERE `accountId` = @acct",
                    Connection);
                cmd.Parameters.AddWithValue("@acct", acctId);
                cmd.Parameters.AddWithValue("@char", slot);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: SetLastChar()\n{0}", e);
            }
        }
    }
}
