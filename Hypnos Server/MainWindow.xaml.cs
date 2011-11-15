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
using System.IO;

namespace Hypnos_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Hypnos _hypnos;
        public MainWindow()
        {
            InitializeComponent();
        }

        delegate void dlgU(string a);

        public void Recv(string info)
        {
            if (System.Threading.Thread.CurrentThread != this.Dispatcher.Thread)
                this.Dispatcher.BeginInvoke(new dlgU(Recv), info);
            else
            {
                tRecv.AppendText(string.Format("{0}",  info));
            }
        }

        private void mi_save_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter write = new StreamWriter(File.Open(@"W:\packets.log", FileMode.Append));
            write.Write(tRecv.Text);
            tRecv.Text = string.Empty;
            write.Close();
        }

        private void mi_listen_Click(object sender, RoutedEventArgs e)
        {
            _hypnos = new Hypnos(this);
            sbInfo.Content = "Waiting for Guilmon...";
        }

        private void mi_recording_Click(object sender, RoutedEventArgs e)
        {
            mi_recording.IsChecked = !mi_recording.IsChecked;
            _hypnos.StopRecord = !mi_recording.IsChecked;
        }

        private void mi_clear_Click(object sender, RoutedEventArgs e)
        {
            tRecv.Text = string.Empty;
        }
    }
}
