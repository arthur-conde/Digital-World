using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Digital_World.Helpers;
using Digital_World.Packets;
using Digital_World.Entities;
using Digital_World.Systems;
using Digital_World.Database;

namespace Digital_World
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Yggdrasil Yggdrasil;

        public MainWindow()
        {
            InitializeComponent();


            Logger lLog = new Logger(tLog);
            Yggdrasil = new Yggdrasil();
        }

        private void mi_opt_Click(object sender, RoutedEventArgs e)
        {
            Options winOpt = new Options();
            winOpt.ShowDialog();
        }

        private void bStart_Click(object sender, RoutedEventArgs e)
        {
            Yggdrasil.Start();
        }

        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            Yggdrasil.Stop();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string[] cmd = tConsole.Text.Split(' ');
            switch (cmd[0])
            {
                case "test":
                    {
                        TacticsDB.Load("Data\\Tactics.bin");
                        break;
                    }
                case "settings":
                    {
                        Settings s = new Settings();
                        s.Serialize("Settings.xml");
                        break;
                    }
                case "hatch":
                    {
                        Settings s = new Settings();
                        int[][] Rate = new int[5][] { new int[3], new int[3], new int[3], new int[3], new int[3] };
                        Random r = new Random();
                        for (int i = 0; i < 1000; i++ )
                        {
                            int Level = r.Next(0, 5);
                            int res = (int)s.GameServer.HatchRates.Hatch(Level);
                            Rate[Level][res]++;
                        }
                        Console.WriteLine("Level 1: {0} succeeded, {1} failed, {2} broke, {3} total", 
                            Rate[0][0], Rate[0][1], Rate[0][2], 
                            Rate[0][0] + Rate[0][1] + Rate[0][2]);
                        Console.WriteLine("Level 2: {0} succeeded, {1} failed, {2} broke, {3} total",
                            Rate[1][0], Rate[1][1], Rate[1][2],
                            Rate[1][0] + Rate[1][1] + Rate[1][2]);
                        Console.WriteLine("Level 3: {0} succeeded, {1} failed, {2} broke, {3} total",
                            Rate[2][0], Rate[2][1], Rate[2][2],
                            Rate[2][0] + Rate[2][1] + Rate[2][2]);
                        Console.WriteLine("Level 4: {0} succeeded, {1} failed, {2} broke, {3} total",
                            Rate[3][0], Rate[3][1], Rate[3][2],
                            Rate[3][0] + Rate[3][1] + Rate[3][2]);
                        Console.WriteLine("Level 5: {0} succeeded, {1} failed, {2} broke, {3} total",
                            Rate[4][0], Rate[4][1], Rate[4][2],
                            Rate[4][0] + Rate[4][1] + Rate[4][2]);
                        break;
                    }
                case "hatch2":
                    {
                        Settings s = new Settings();
                        int[] total = new int[6];
                        int eggs = 100;
                        if (cmd.Length >= 2)
                            int.TryParse(cmd[1], out eggs);
                        for (int i = 0; i < eggs; i++)
                        {
                            int level = 0;
                            while (level != 5)
                            {
                                int res = (int)s.GameServer.HatchRates.Hatch(level);
                                if (res == 0)
                                    level++;
                                else if (res == -1)
                                    continue;
                                else
                                    break;
                            }
                            total[level]++;
                        
                        }
                        Console.WriteLine("No Inp. - {0} = {1}", total[0], total[0] / (float)eggs);
                        Console.WriteLine("Level 1 - {0} = {1}", total[1], total[1] / (float)eggs);
                        Console.WriteLine("Level 2 - {0} = {1}", total[2], total[2] / (float)eggs);
                        Console.WriteLine("Level 3 - {0} = {1}", total[3], total[3] / (float)eggs);
                        Console.WriteLine("Level 4 - {0} = {1}", total[4], total[4] / (float)eggs);
                        Console.WriteLine("Level 5 - {0} = {1}", total[5], total[5] / (float)eggs);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
