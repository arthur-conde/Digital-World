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
using System.Windows.Shapes;
using Digital_World.Helpers;

namespace Digital_World
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        private Settings mySettings;

        public Options()
        {
            InitializeComponent();

            mySettings = Settings.Deserialize();

            tHost.Text = mySettings.GameServer.Host;
            tPort.Text = mySettings.GameServer.Port.ToString();
            chkStart.IsChecked = new bool?(mySettings.GameServer.AutoStart);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            mySettings.GameServer.Host = tHost.Text;
            mySettings.GameServer.AutoStart = chkStart.IsChecked.Value;
            try
            {
                mySettings.GameServer.Port = int.Parse(tPort.Text);
            }
            catch (FormatException)
            {
                mySettings.GameServer.Port = 7000;
            }
            mySettings.Serialize();

            this.DialogResult = new bool?(true);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
        }
    }
}
