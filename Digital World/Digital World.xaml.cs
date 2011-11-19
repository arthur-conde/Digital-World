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
                        ItemDB.Load("Data\\ItemList.bin");
                        break;
                    }
                case "find":
                    {
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
