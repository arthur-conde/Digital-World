using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Digital_World.Packets;
using Digital_World.Entities;

namespace Digital_World
{
    public class Client
    {
        public Socket m_socket = null;
        public const int BUFFER_SIZE = 4096;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public byte[] oldBuffer;

        public string Username;
        public uint AccountID = 0;
        public int UniqueID = 0;
        public int AccessLevel = 0;
        public short handshake = 0;
        public byte Characters = 0;

        public Character Tamer = null;

        public uint time_t = 0;

        /// <summary>
        /// Send a raw packet to the client
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            BeginSend(buffer);
        }

        /// <summary>
        /// Send a formed packet to the cleint
        /// </summary>
        /// <param name="packet"></param>
        public void Send(IPacket packet)
        {
            try
            {
                BeginSend(packet.ToArray());
            }
            catch { }
        }

        private void BeginSend(byte[] buffer)
        {
            m_socket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(EndSend), this);
        }

        private void EndSend(IAsyncResult ar)
        {
            try
            {
                m_socket.EndSend(ar);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Console.WriteLine("Error: EndSend()\n{0}", e);
            }
        }

        public void Handshake()
        {
            int time_t = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            handshake = (short)(time_t & 0xffff);
            Packet packet = new Packets.PacketFFFF(handshake);
            Send(packet);
        }

        public override string ToString()
        {
            if (Tamer == null)
                return string.Format("{0}", m_socket.RemoteEndPoint);
            else
                return string.Format("{0} - {1}", m_socket.RemoteEndPoint, Tamer);
        }

        /// <summary>
        /// Sends the contents of PacketReader to the client.
        /// </summary>
        /// <param name="packet">PacketReader object</param>
        public void Send(PacketReader packet)
        {
            Send(packet.ToArray());
        }
    }
}
