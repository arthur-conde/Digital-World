using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Net;

namespace Digital_World.Helpers
{
    [Serializable()]
    public class Settings
    {
        public class DatabaseSettings
        {
            [XmlAttribute("Host")]
            public string Host = "localhost";
            [XmlAttribute("Username")]
            public string Username = "";
            [XmlAttribute("Password")]
            public string Password = "";
            [XmlAttribute("Schema")]
            public string Schema = "";
        }

        public class ServerSettings
        {
            public string Host = "DOUBLE";
            public int Port = 6999;
            public bool AutoStart = false;

            [XmlIgnore()]
            public IPEndPoint EndPoint
            {
                get
                {
                    IPAddress IP = Dns.GetHostEntry(Host).AddressList[0];
                    IPEndPoint ipep = new IPEndPoint(IP, Port);
                    return ipep;
                }
            }

            [XmlIgnore()]
            public IPAddress IP
            {
                get
                {
                    IPAddress[] List = Dns.GetHostEntry(Host).AddressList;
                    foreach (IPAddress ip in List)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            return ip;
                    }
                    return List[0];
                }
            }
        }

        public class AuthServerSettings:ServerSettings
        {
            public AuthServerSettings()
            {
                this.AutoStart = true;
                this.Port = 7029;
            }
        }

        public class LobbyServerSettings : ServerSettings
        {
            public LobbyServerSettings()
            {
                this.AutoStart = true;
                this.Port = 699;
            }
        }

        public class GameServerSettings : ServerSettings
        {
            public HatchRateSetting HatchRates = new HatchRateSetting();

            public class HatchRateSetting
            {
                [XmlIgnore()]
                private double[][] HatchRate = new double[5][] { new double[3], new double[3], new double[3], new double[3], new double[3] };

                public HatchRateSetting()
                {
                    HatchRate[0] = new double[] { 0.90d, 0.10d, 0.0d };
                    HatchRate[1] = new double[] { 0.80d, 0.15d, 0.05d };
                    HatchRate[2] = new double[] { 0.70d, 0.25d, 0.05d };
                    HatchRate[3] = new double[] { 0.50d, 0.35d, 0.15d };
                    HatchRate[4] = new double[] { 0.30d, 0.50d, 0.20d };
                }

                public class HatchLevelSetting
                {
                    [XmlAttribute("Success")]
                    public double Success = 1.0d;
                    [XmlAttribute("Failure")]
                    public double Failure = 0.0d;
                    [XmlAttribute("Broken")]
                    public double Broken = 0.0d;

                    public HatchLevelSetting(double success, double failure, double broken)
                    {
                        Success = success;
                        Failure = failure;
                        Broken = broken;
                    }

                    public HatchLevelSetting() { }

                    public HatchLevelSetting(double[] p)
                    {
                        this.Success = p[0];
                        this.Failure = p[1];
                        this.Broken = p[2];
                    }

                    public double[] ToArray()
                    {
                        return new double[] { Success, Failure, Broken };
                    }

                    public List<Tuple<int, double>> ToList()
                    {
                        List<Tuple<int, double>> list = new List<Tuple<int, double>>();
                        list.Add(new Tuple<int, double>(0, Success));
                        list.Add(new Tuple<int, double>(1, Failure));
                        list.Add(new Tuple<int, double>(2, Broken));
                        //list.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                        return list;
                    }

                    public double Sum()
                    {
                        return Success + Failure + Broken;
                    }
                }

                public HatchLevelSetting Level1
                {
                    get
                    {
                        return new HatchLevelSetting(HatchRate[0]);
                    }
                    set
                    {
                        HatchRate[0] = value.ToArray();
                    }
                }

                public HatchLevelSetting Level2
                {
                    get
                    {
                        return new HatchLevelSetting(HatchRate[1]);
                    }
                    set
                    {
                        HatchRate[1] = value.ToArray();
                    }
                }

                public HatchLevelSetting Level3
                {
                    get
                    {
                        return new HatchLevelSetting(HatchRate[2]);
                    }
                    set
                    {
                        HatchRate[2] = value.ToArray();
                    }
                }

                public HatchLevelSetting Level4
                {
                    get
                    {
                        return new HatchLevelSetting(HatchRate[3]);
                    }
                    set
                    {
                        HatchRate[3] = value.ToArray();
                    }
                }

                public HatchLevelSetting Level5
                {
                    get
                    {
                        return new HatchLevelSetting(HatchRate[4]);
                    }
                    set
                    {
                        HatchRate[4] = value.ToArray();
                    }
                }

                [XmlIgnore()]
                private Random RNG = new Random();

                public int Hatch(int level)
                {
                    int result = 0;
                    HatchLevelSetting hls = new HatchLevelSetting(HatchRate[level]);
                    List<Tuple<int, double>> list = hls.ToList();
                    double rnd = RNG.NextDouble();

                    foreach(Tuple<int, double> rate in list)
                    {
                        if (rnd < rate.Item2)
                        {
                            result = rate.Item1;
                            break;
                        }
                        rnd -= rate.Item2;
                    }
                    return result;
                }
            }

            public class SizeSetting
            {
                private double[][] SizeRates;
            }

            public GameServerSettings()
            {
                this.Port = 7000;
            }
        }

        public Settings()
        {
            Database = new DatabaseSettings();
            LobbyServer = new LobbyServerSettings();
            AuthServer = new AuthServerSettings();
            GameServer = new GameServerSettings();
        }

        [XmlElement("Database")]
        public DatabaseSettings Database = new DatabaseSettings();
        [XmlElement("LobbyServer")]
        public LobbyServerSettings LobbyServer = new LobbyServerSettings();
        [XmlElement("AuthServer")]
        public AuthServerSettings AuthServer = new AuthServerSettings();
        [XmlElement("GameServer")]
        public GameServerSettings GameServer = new GameServerSettings();

        public void Serialize(string fileName)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Settings));
            using (Stream s = File.Open(fileName, FileMode.Create))
            {
                xml.Serialize(s, this);
            }
        }

        public static Settings Deserialize(string fileName)
        {
            Settings Settings = new Settings();
            if (File.Exists(fileName))
            {
                XmlSerializer xml = new XmlSerializer(typeof(Settings));
                using (Stream s = File.OpenRead(fileName))
                {
                    Settings = (Settings)xml.Deserialize(s);
                }
            }
            else
                Settings.Serialize("Settings.xml");
            return Settings;
        }

        public static Settings Deserialize()
        {
            return Deserialize("Settings.xml");
        }

        public void Serialize()
        {
            Serialize("Settings.xml");
        }
    }
}