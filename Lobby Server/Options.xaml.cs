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

namespace Digital_World
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();

            tHost.Text = Properties.Settings.Default.Host;
            tPort.Text = Properties.Settings.Default.Port.ToString();
            tMHost.Text = Properties.Settings.Default.MapServer;
            tMPort.Text = Properties.Settings.Default.MapPort.ToString();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Host = tHost.Text;
            Properties.Settings.Default.MapServer = tMHost.Text;
            try
            {
                Properties.Settings.Default.Port = int.Parse(tPort.Text);
                Properties.Settings.Default.MapPort = int.Parse(tMPort.Text);
            }
            catch (FormatException)
            {
                Properties.Settings.Default.Port = 6999;
                Properties.Settings.Default.MapPort = 7012;
            }
            Properties.Settings.Default.Save();

            this.DialogResult = new bool?(true);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
        }
    }
}
