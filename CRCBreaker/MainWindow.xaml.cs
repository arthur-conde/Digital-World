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
using System.Threading;

namespace CRCBreaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CRC16 c16;
        CRC16CCITT c162;

        Thread thdC1;
        Thread thdC2;

        public MainWindow()
        {
            InitializeComponent();
            c16 = new CRC16();
            c162 = new CRC16CCITT( CRC16CCITT.InitialCrcValue.Zeros);

            tCRC16Poly.Text = "0x" + c16.poly.ToString("X");
            tCRC162Poly.Text = "0x"+c162.poly.ToString("X");
            tCRC162IV.Text = "0x"+c162.initialValue.ToString("X");
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            c16.poly = Parse(tCRC16Poly.Text);
            c162.poly = Parse(tCRC162Poly.Text);
            c162.initialValue = Parse(tCRC162IV.Text);

            tCRC16.Text = Visualize(c16.ComputeChecksumBytes(buffer()));
            tCRC162.Text = Visualize(c162.ComputeChecksumBytes(buffer()));
        }

        private ushort Parse(string s)
        {
            if (s.StartsWith("0x"))
            {
                return ushort.Parse(s.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }
            else
                return ushort.Parse(s);
        }

        private byte[] buffer()
        {
            string[] rawData = tData.Text.Trim().Split(' ');
            byte[] buffer = new byte[rawData.Length];
            for (int i = 0; i < rawData.Length; i++)
            {
                buffer[i] = byte.Parse(rawData[i], System.Globalization.NumberStyles.HexNumber);
            }
            return buffer;
        }

        private string Visualize(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("X2"));
                if (i < buffer.Length - 1)
                    sb.Append(" ");
            }
            return sb.ToString();
        }

        private void btnGuess1_Click(object sender, RoutedEventArgs e)
        {
            tCRC16Poly.Text = ushort.MinValue.ToString("X");

            thdC1 = new Thread(new ParameterizedThreadStart(Guess1));
            thdC1.IsBackground = true;
            thdC1.Start(buffer());
        }

        private void btnGuess2_Click(object sender, RoutedEventArgs e)
        {
            thdC2 = new Thread(new ParameterizedThreadStart(Guess2));
            thdC2.IsBackground = true;
            thdC2.Start(buffer());
        }

        private void Guess1(object o)
        {
            byte[] ok = new byte[] { 0xf7, 0x1a };
            byte[] buffer = (byte[]) o;
            try
            {
                for (ushort crc_poly = ushort.MinValue; crc_poly < ushort.MaxValue; crc_poly++)
                {
                    c16.poly = crc_poly++;
                    c16.GenTable(c16.poly);

                    byte[] hash = c16.ComputeChecksumBytes(buffer);

                    tCRC16Poly.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            tCRC16Poly.Text = "0x" + (crc_poly).ToString("X");
                            tCRC16.Text = Visualize(hash);
                        }));

                    if (hash.Equals(ok)) break;
                    if (hash[0] == ok[0] && hash[1] == ok[1]) break;
                    if (hash[0] == ok[1] && hash[1] == ok[0]) break;

                    Thread.Sleep(5);

                    if (crc_poly == ushort.MaxValue) break;
                }
            }
            catch
            {

            }
        }

        private void Guess2(object o)
        {
            byte[] ok = new byte[] { 0xf7, 0x1a };
            byte[] buffer = (byte[])o;
            try
            {
                for(ushort ccitt_poly = 1; ccitt_poly <= ushort.MaxValue; ccitt_poly++)
                {
                    for (ushort ccitt_iv = ushort.MinValue; ccitt_iv <= ushort.MaxValue; ccitt_iv++)
                    {
                        c162.initialValue = ccitt_iv;
                        c162.poly = ccitt_poly;
                        c162.GenTable(ccitt_iv, ccitt_poly);

                        byte[] hash = c162.ComputeChecksumBytes(buffer);

                        tCRC162Poly.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            tCRC162Poly.Text = "0x" + ccitt_poly.ToString("X");
                            tCRC162IV.Text = "0x" + ccitt_iv.ToString("X");
                            tCRC162.Text = Visualize(hash);
                        }));

                        if (hash.Equals(ok)) break;
                        if (hash[0] == ok[0] && hash[1] == ok[1]) break;
                        if (hash[0] == ok[1] && hash[1] == ok[0]) break;

                        Thread.Sleep(5);

                        if (ccitt_iv == ushort.MaxValue) break;
                    }
                    if (ccitt_poly == ushort.MaxValue) break;
                }
            }
            catch
            {

            }
        }

        private void btnCalc_Click(object sender, RoutedEventArgs e)
        {

            DateTime d = DateTime.Now; ;
            if (tCRC16.Text != "")
            {
                d = new DateTime(1970, 1, 1).AddSeconds(double.Parse(tCRC16.Text));
            }
            else if (tCRC162.Text != "")
            {
                d = new DateTime(1970, 1, 1).AddSeconds(double.Parse(tCRC162.Text));
                Random rr = new Random((int)d.Ticks);
                byte[] zbuffer = new byte[334];
                rr.NextBytes(zbuffer);
                tData.Text = See(zbuffer);
                return;
            }
            else
                d = dpTime.SelectedDate.Value.AddHours(double.Parse(tH.Text)).AddMinutes(double.Parse(tM.Text)).AddSeconds(double.Parse(tS.Text));
            DateTime k = TimeZoneInfo.ConvertTimeFromUtc(d.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"));
            Random r = new Random((int)k.Ticks);
            byte[] buffer = new byte[334];
            r.NextBytes(buffer);
            tData.Text = See(buffer);
        }

        public static string See(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            int rows = (int)Math.Ceiling(buffer.Length / 16.0);
            for (int i = 0; i < rows; i++)
            {
                StringBuilder text = new StringBuilder();
                for (int col = 0; col < 16; col++)
                {
                    int pos = col + (i * 16);
                    if (pos < buffer.Length)
                    {
                        sb.Append(buffer[pos].ToString("X2"));
                        if (buffer[pos] > 32 && buffer[pos] < 126)
                            text.Append((char)buffer[pos]);
                        else
                            text.Append(".");
                    }
                    else
                    {
                        sb.Append("  ");
                        text.Append(" ");
                    }
                    sb.Append(" ");
                }
                sb.Append("    ");
                sb.AppendLine(text.ToString());
            }
            return sb.ToString();
        }
    }
}
