using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

namespace Digital_World
{
    /// <summary>
    /// MySQL Database Wrapper for the Digital World
    /// </summary>
    public partial class SqlDB
    {
        private static string db_host = "localhost";
        private static string db_user = "dmouser";
        private static string db_pass = "shikifuuin";
        private static string db_schema = "dmo";
        private static MySqlConnection m_con;
        private static MySqlConnection Connection
        {
            get
            {
                if (m_con.State != ConnectionState.Open)
                    m_con = Connect();
                return m_con;
            }
            set { m_con = value; }
        }

        private static Random RNG = new Random();

        static SqlDB()
        {
            Connection = Connect();
        }

        public static MySqlConnection Connect()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(string.Format("server={0};uid={1};pwd={2};database={3};", db_host, db_user, db_pass, db_schema));
                conn.Open();
                return conn;
            }
            catch (MySqlException)
            {
                return null;
            }
        }

        public static int Validate(Client client, string user, string pass)
        {
            if (Connection == null) Connection = Connect();
            MySqlDataReader read = null;
            int level = 0;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `username` = @user", Connection);

                cmd.Parameters.AddWithValue("@user", user);

                read = cmd.ExecuteReader();

                if (read.HasRows)
                {
                    if (read.Read())
                    {
                        if (read["username"].ToString() == user && read["password"].ToString() == SHA2(pass))
                        {
                            level = (int)read["level"];
                            client.AccessLevel = (int)read["level"];
                            client.Username = user;
                            client.AccountID = Convert.ToUInt32((int)read["accountId"]); ;
                        }
                        else
                        {
                            //Wrong Pass
                            level = -2;
                        }
                    }
                    read.Close();

                    cmd = new MySqlCommand("SELECT * FROM `chars` WHERE `accountId` = @id", Connect());
                    cmd.Parameters.AddWithValue("@id", client.AccountID);
                    read = cmd.ExecuteReader();
                    if (read.HasRows)
                    {
                        while (read.Read())
                        {
                            client.Characters++;
                        }
                    }
                    else client.Characters = 0;
                    read.Close();
                }
                else
                {
                    try { read.Close(); }
                    catch { }

                    //Return Username not found
                    level = -3;
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error: Validate\n{0}", e);
                level = -1;
            }
            finally
            {
                try { read.Close(); }
                catch { }
            }
            return level;
        }

        public static bool CreateUser(string user, string pass)
        {
            if (Connection == null) Connection = Connect();
            try
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO `acct` (`username`, `password`)  VALUES (@user, @pass)", Connection);

                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@pass", SHA2(pass));

                int r = cmd.ExecuteNonQuery();

                if (r == 1)
                {
                    return true;
                }
                return false;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error: CreateUser\n{0}", e);
                return false;
            }
        }

        /// <summary>
        /// Loads all user information into the Client class
        /// </summary>
        /// <param name="client">Client</param>
        public static void LoadUser(Client client)
        {
            if (Connection == null) Connection = Connect();
            MySqlDataReader read = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `username` = @user", Connection);
                cmd.Parameters.AddWithValue("@user", client.Username);

                read = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (read.HasRows)
                {
                    if (read.Read())
                    {
                        client.AccessLevel = (int)read["level"];
                        client.AccountID = Convert.ToUInt32((int)read["accountId"]);
                        
                        int uniId =RNG.Next(1, int.MaxValue);
                        MySqlCommand updateUniId = 
                            new MySqlCommand("UPDATE `acct` SET `uniId` = @uniId WHERE `accountId` = @id", Connect());
                        updateUniId.Parameters.AddWithValue("@uniId", uniId);
                        updateUniId.Parameters.AddWithValue("@id", read["accountId"]);
                        updateUniId.ExecuteNonQuery();

                        client.UniqueID = uniId;
                    }
                    read.Close();
                }
                else
                {
                    try { read.Close(); }
                    catch { }
                    //Return Username not found
                }
            }
            catch (MySqlException e)
            {
                try { if (read != null) read.Close(); }
                catch { }
                Console.WriteLine("Error: Validate\n{0}", e);
            }
        }

        /// <summary>
        /// Loads all user data into the Client class. Used by the Lobby Server
        /// </summary>
        /// <param name="client">client</param>
        /// <param name="AccountID">AccountID to find</param>
        /// <param name="UniId">Unique ID</param>
        public static void LoadUser(Client client, uint AccountID, int UniId)
        {
            if (Connection == null) Connection = Connect();
            MySqlDataReader read = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `acct` WHERE `accountId` = @id", Connection);
                cmd.Parameters.AddWithValue("@id", AccountID);

                read = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (read.HasRows)
                {
                    if (read.Read() && (int)read["uniId"] == UniId)
                    {
                        client.AccessLevel = (int)read["level"];
                        client.AccountID = AccountID;

                        /*
                        MySqlCommand updateUniId =
                            new MySqlCommand("UPDATE `acct` SET `uniId` = NULL WHERE `accountId` = @id", Connect());
                        updateUniId.Parameters.AddWithValue("@id", AccountID);
                        updateUniId.ExecuteNonQuery();
                        */

                        client.UniqueID = UniId;
                    }
                    read.Close();
                }
                else
                {
                    try { if (read != null) read.Close(); }
                    catch { }
                }
            }
            catch (MySqlException e)
            {
                try { if (read != null) read.Close(); }
                catch { }
                Console.WriteLine("Error: Validate\n{0}", e);
            }
        }

        public static Dictionary<int, string> GetServers()
        {
            if (Connection == null) Connection = Connect();
            MySqlDataReader data = null;
            Dictionary<int, string> servers = new Dictionary<int, string>();
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT `serverid`, `name` FROM servers", Connection);
                data = cmd.ExecuteReader();
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        servers.Add((int)data["serverid"], data["name"].ToString());
                    }
                    data.Close();
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error: GetServers\n{0}", e);
            }
            finally
            {
                try { data.Close(); }
                catch { }
            }
            return servers;
        }

        public static KeyValuePair<int, string> GetServer(int ID)
        {
            if (Connection == null) Connection = Connect();
            KeyValuePair<int, string> k = new KeyValuePair<int,string>(6999,"127.0.0.1");
            MySqlDataReader data = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT port, INET_NTOA(ip) ip FROM servers WHERE `serverId` = @id", Connection);
                cmd.Parameters.AddWithValue("@id", ID);

                data = cmd.ExecuteReader();

                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        k = new KeyValuePair<int, string>((int)data["port"], data["ip"].ToString());
                    }
                    data.Close();
                }
                return k;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error: CreateUser\n{0}", e);
            }
            finally
            {
                try { data.Close(); }
                catch { }
            }
            return k;
        }

        /// <summary>
        /// Get the number of characters for a specific account id
        /// </summary>
        /// <param name="AcctId">Account Id to match</param>
        /// <returns>The number of characters tied to AcctId</returns>
        public static int GetNumChars(uint AcctId)
        {
            if (Connection == null) Connection = Connect();
            int characters = 0;
            MySqlDataReader read = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM `chars` WHERE `accountId` = @id"
                    , Connection);
                cmd.Parameters.AddWithValue("@id", AcctId);

                read = cmd.ExecuteReader();
                if (read.HasRows)
                {
                    while (read.Read())
                    {
                        characters++;
                    }
                }
                read.Close();
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
            return characters;
        }

        public static string SHA2(string value)
        {
            SHA256 shaM = new SHA256Managed();

            byte[] buffer = Encoding.UTF8.GetBytes(value);
            buffer = shaM.ComputeHash(buffer);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
                sb.Append(buffer[i].ToString("X2"));

            return sb.ToString();
        }
    }
}
