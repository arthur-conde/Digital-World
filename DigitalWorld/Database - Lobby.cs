using System;
using System.Collections.Generic;
using Digital_World.Entities;
using MySql.Data.MySqlClient;
using Digital_World.Helpers;

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
            if (m_con == null) m_con = Connect();
            bool avail = false;
            MySqlDataReader read = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `chars` WHERE `charName` = @name"
                    , m_con);
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
            if (m_con == null) m_con = Connect();
            List<Character> chars = new List<Character>();
            MySqlDataReader read = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `chars` WHERE `accountId` = @id"
                    , m_con);
                cmd.Parameters.AddWithValue("@id", AcctId);

                read = cmd.ExecuteReader();
                if (read.HasRows)
                {
                    while (read.Read())
                    {
                        Character Tamer = new Character();
                        Tamer.AccountId = AcctId;
                        Tamer.CharacterId = Convert.ToUInt32((int)read["characterId"]); ;
                        Tamer.Model = (CharacterModel)(int)read["charModel"];
                        Tamer.Name = (string)read["charName"];
                        Tamer.Level = (int)read["charLv"];
                        Tamer.Location = new Helpers.Position((short)(int)read["map"], (int)read["x"], (int)read["y"]);

                        Tamer.DigimonList.Add(GetDigimon((uint)(int)read["partner"]));
                        Tamer.Partner = Tamer.DigimonList[0];

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
                try { read.Close(); }
                catch { }
            }
            return chars;
        }

        public static Digimon GetDigimon(uint DigiId)
        {
            if (m_con == null) m_con = Connect();
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
                        digimon.DigiType = (int)read["digiType"];
                        digimon.Size = (short)(int)read["digiSize"];
                        digimon.Scale = (int)read["digiScale"];
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

        public static int CreateDigimon(int charId, string digiName, int digiModel)
        {
            int digiId = -1;
            MySqlDataReader read = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO `digimon` (`digiName`,`digiType`, `characterId`)" +
                    "VALUES (@digiName, @digiModel, @char)"
                    , Connect());
                cmd.Parameters.AddWithValue("@digiName", digiName);
                cmd.Parameters.AddWithValue("@digiModel", digiModel);
                cmd.Parameters.AddWithValue("@char", charId);
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

        public static int CreateCharacter(uint AcctId, int pos, int charModel, string charName)
        {
            int charId = -1;
            MySqlDataReader read = null;
            try
            {

                MySqlConnection con = Connect();
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO `chars` (`charName`, `charModel`, `accountId`, `inventory`, `storage`, `equipment`, `quests`, `maxHP`, `maxDS`, `HP`, `DS`,`AT`, `DE`) " +
                    "VALUES (@charName, @charModel, @acctId, @inv, @bank,@equip,@quest, @hp, @mp, @hp, @mp, @at, @de)"
                    , con);
                cmd.Parameters.AddWithValue("@charName", charName);
                cmd.Parameters.AddWithValue("@charModel", charModel);
                cmd.Parameters.AddWithValue("@acctId", AcctId);
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
                    "UPDATE `chars` SET `partner` = @digiId WHERE `characterId` = @charId", m_con);
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
                    "UPDATE `digimon` SET `characterId` = @charId WHERE `digimonId` = @digiId", m_con);
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

                    cmd = new MySqlCommand("DELETE FROM `chars` WHERE `characterId` = @char", m_con);
                    cmd.Parameters.AddWithValue("@char", charId);
                    cmd.ExecuteNonQuery();

                    cmd = new MySqlCommand(
                        string.Format("UPDATE `acct` SET `char{0}` = NULL WHERE `accountId` = @acct", slot + 1)
                        , m_con);
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
                    , m_con);
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
                    m_con);
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
