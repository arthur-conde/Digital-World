using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Digital_World.Network
{
    public class SocketWrapper
    {
        public int ListenPort = 0;

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private Thread tWorker;
        private Socket listener;

        public SocketWrapper()
        {
        }

        public void Listen(int Port)
        {
            tWorker = new Thread(new ParameterizedThreadStart(Start));
            tWorker.IsBackground = true;
            tWorker.Start(Port);
        }

        public void Listen(ServerInfo info)
        {
            if (tWorker != null)
            {
                if (tWorker.ThreadState != ThreadState.Aborted)
                    return;
            }
            tWorker = new Thread(new ParameterizedThreadStart(Start));
            tWorker.IsBackground = true;
            tWorker.Start(info);

        }
    
        public void Stop()
        {
            if (tWorker != null)
            {
                tWorker.Abort();
            }
        }

        public delegate void dlgAccept(Client client);
        public delegate void dlgRead(Client client, byte[] buffer, int length);
        public delegate void dlgSend(IAsyncResult ar);
        public delegate void dlgClose(Client client);

        /// <summary>
        /// Called when a connection is accepted
        /// </summary>
        public event dlgAccept OnAccept;
        /// <summary>
        /// Called when a complete packet is read
        /// </summary>
        public event dlgRead OnRead;
        public event dlgSend OnSend;
        public event dlgClose OnClose;

        private void Start(object state)
        {
            IPAddress ipAddress;
            int Port = 0;
            if (state.GetType() == typeof(ServerInfo))
            {
                Console.WriteLine("ServerInfo found...");
                ServerInfo info = (ServerInfo)state;
                Port = info.Port;
                ipAddress = info.Host;

                 Console.WriteLine("Listening on {0}", info);
            }
            else
            {
                Port = (int)state;

                IPHostEntry hostInfo = Dns.GetHostEntry("localhost");
                ipAddress = hostInfo.AddressList[1];
            }
            
            byte[] bytes = new byte[Client.BUFFER_SIZE];

            IPEndPoint localEP = new IPEndPoint(ipAddress, Port);

            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEP);
                listener.Listen(100);

                Console.WriteLine("Listening...");

                while (true)
                {
                    allDone.Reset();

                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    allDone.WaitOne();
                }
            }
            catch (ThreadAbortException)
            {
                listener.Close();
                Console.WriteLine("Shutting down server...");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();

                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);    //DMO Client

                Console.WriteLine("Accepting a client: {0}", handler.RemoteEndPoint);

                Client state = new Client();
                state.m_socket = handler;

                if (OnAccept != null)
                {
                    OnAccept.BeginInvoke(state, new AsyncCallback(ProcessedAccept), state);
                }
                else
                    handler.BeginReceive(state.buffer, 0, Client.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (ObjectDisposedException)
            {
                
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: AcceptCallback\n{0}", e);
            }
        }

        private void ProcessedAccept(IAsyncResult ar)
        {
            OnAccept.EndInvoke(ar);

            Client state = (Client)ar.AsyncState;
            Socket handler = state.m_socket;

            try
            {
                handler.BeginReceive(state.buffer, 0, Client.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (ObjectDisposedException)
            {
                if (OnClose != null)
                    OnClose.BeginInvoke(state, new AsyncCallback(EndClose), state);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            Client state = (Client)ar.AsyncState;
            Socket handler = state.m_socket;
            try
            {
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    int len = BitConverter.ToInt16(state.buffer, 0);
                    if (bytesRead != len)
                    {
                        //If the packet is incomplete
                        //Check if there is an incomplete packet in memory
                        if (state.oldBuffer != null && state.oldBuffer.Length != 0)
                        {
                            //And concat the two.
                            byte[] buffer = new byte[bytesRead + state.oldBuffer.Length];
                            Array.Copy(state.oldBuffer, buffer, state.oldBuffer.Length);
                            Array.Copy(state.buffer, 0, buffer, state.oldBuffer.Length, bytesRead);
                            state.buffer = buffer;

                            if (OnRead != null)
                            {
                                byte[] buffer2 = new byte[bytesRead];
                                Array.Copy(state.buffer, buffer2, bytesRead);
                                OnRead.BeginInvoke(state, buffer2, bytesRead, new AsyncCallback(ProcessedRead), null);
                            }
                        }
                        else
                        {
                            //Otherwise, store the received data
                            state.oldBuffer = new byte[state.buffer.Length];
                            state.buffer.CopyTo(state.oldBuffer, 0);

                            //And listen for more.
                            handler.BeginReceive(state.buffer, 0, Client.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
                            return;
                        }
                    }
                    else
                    {
                        if (OnRead != null)
                        {
                            byte[] buffer = new byte[bytesRead];
                            Array.Copy(state.buffer, buffer, bytesRead);
                            OnRead.BeginInvoke(state, buffer, bytesRead, new AsyncCallback(ProcessedRead), null);
                        }
                    }
                    handler.BeginReceive(state.buffer, 0, Client.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
                }
            }
            catch (ObjectDisposedException)
            {
                if (OnClose != null)
                    OnClose.BeginInvoke(state, new AsyncCallback(EndClose), state);
            }
            catch (SocketException)
            {
                if (OnClose != null)
                    OnClose.BeginInvoke(state, new AsyncCallback(EndClose), state);
            }
        }

        private void ProcessedRead(IAsyncResult ar)
        {
            try
            {
                OnRead.EndInvoke(ar);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Send(Socket handler, byte[] buffer)
        {
            handler.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
        }

        private void EndClose(IAsyncResult ar)
        {
            try
            {
                OnClose.EndInvoke(ar);
            }
            catch
            {
            }
        }


        public bool Running
        {
            get
            {
                try
                {
                    if (tWorker.ThreadState == ThreadState.Running)
                        return true;
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
