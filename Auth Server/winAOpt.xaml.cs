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

            mySettings = Settings.Deserialize("Settings.xml");

            tHost.Text = mySettings.AuthServer.Host;
            tPort.Text = mySettings.AuthServer.Port.ToString();
            chkStart.IsChecked = new bool?(mySettings.AuthServer.AutoStart);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            mySettings.AuthServer.Host = tHost.Text;
            mySettings.AuthServer.AutoStart = chkStart.IsChecked.Value;
            try
            {
                mySettings.AuthServer.Port = int.Parse(tPort.Text);
            }
            catch (FormatException)
            {
                mySettings.AuthServer.Port = 6999;
            }
            mySettings.Serialize("Settings.xml");

            this.DialogResult = new bool?(true);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
        }
    }
}
