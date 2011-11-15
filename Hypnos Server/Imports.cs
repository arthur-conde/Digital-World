using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections;

namespace Hypnos_Server
{
    public class Hypnos
    {
        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public const string PIPE_NAME = "\\\\.\\pipe\\guilmon\\";
        public const uint BUFFER_SIZE = 8192;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);

        SafeFileHandle hRecv;
        SafeFileHandle hSend;
        FileStream fRecv;
        FileStream fSend;
        Thread tSend;
        Thread tRecv;
        Thread tMain;
        Thread tMon;
        MainWindow parent;
        Queue<Packet> packets = new Queue<Packet>();

        public bool StopRecord = false;

        public Hypnos(MainWindow parent)
        {
            this.parent = parent;

            tMain = new Thread(new ThreadStart(Work));
            tMain.IsBackground = true;
            tMain.Start();

            tMon = new Thread(new ThreadStart(MonitorQueue));
            tMon.IsBackground = true;
            tMon.Start();
        }

        public void Work()
        {
            hRecv = CreateNamedPipe(PIPE_NAME + "recv", DUPLEX | FILE_FLAG_OVERLAPPED, 0, 255, BUFFER_SIZE, BUFFER_SIZE, 0, IntPtr.Zero);
            hSend = CreateNamedPipe(PIPE_NAME + "send", DUPLEX | FILE_FLAG_OVERLAPPED, 0, 255, BUFFER_SIZE, BUFFER_SIZE, 0, IntPtr.Zero);

            int success = ConnectNamedPipe(hSend, IntPtr.Zero);
            if (success == -1)
            {

            }
            success = ConnectNamedPipe(hRecv, IntPtr.Zero);
            if (success == -1)
            {

            }

            parent.sbInfo.Dispatcher.BeginInvoke(new Action(() => { parent.sbInfo.Content = "Guilmon located. Uplink established..."; }));

            fRecv = new FileStream(hRecv, FileAccess.ReadWrite, (int)BUFFER_SIZE, true);
            tRecv = new Thread(new ThreadStart(ReadRecv));
            tRecv.IsBackground = true;
            tRecv.Start();

            fSend = new FileStream(hSend, FileAccess.ReadWrite, (int)BUFFER_SIZE, true);
            tSend = new Thread(new ThreadStart(ReadSend));
            tSend.IsBackground = true;
            tSend.Start();
        }

        public void MonitorQueue()
        {
            while (true)
            {
                if (packets.Count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }
                Packet p = packets.Dequeue();
                if (p == null) continue;
                if (!StopRecord)
                {
                    parent.Recv(p.ToString());
                }
            }
        }

        public void ReadRecv()
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            while (true)
            {
                int bytesRead = fRecv.Read(buffer, 0, (int)BUFFER_SIZE);
                if (bytesRead > 0)
                {
                    lock (packets)

                    { packets.Enqueue(new Packet(buffer, false, bytesRead)); }

                    fRecv.Flush();
                }
            }
        }

        public void ReadSend()
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            while (true)
            {
                int bytesRead = fSend.Read(buffer, 0, (int)BUFFER_SIZE);
                //if (bytesRead != 0 && bytesRead != BUFFER_SIZE)
                if (bytesRead > 0)
                {
                    lock (packets)

                    { packets.Enqueue(new Packet(buffer, true, bytesRead)); }
                    fSend.Flush();
                }
            }
        }

        public static string Visualize(byte[] buffer, int len)
        {
            StringBuilder sb = new StringBuilder();
            int rows = (int)Math.Ceiling(len / 16.0);
            for (int i = 0; i < rows; i++)
            {
                StringBuilder text = new StringBuilder();
                for (int col = 0; col < 16; col++)
                {
                    int pos = col + (i * 16);
                    if (pos < len && pos < buffer.Length)
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

        public class Packet
        {
            public byte[] buffer;
            public string direction;
            public int length;

            public Packet(byte[] b, bool FromClient, int length)
            {
                this.length = length;
                buffer = b;
                direction = FromClient ? "Client->Server" : "Server->Client";
            }

            public override string ToString()
            {
                ushort len = BitConverter.ToUInt16(buffer, 0);
                if (BitConverter.ToInt16(buffer,2) == 1003)
                {

                }
                return string.Format("{0}\n{1}\n", direction, Visualize(buffer, length));
            }
        }
    }
}
