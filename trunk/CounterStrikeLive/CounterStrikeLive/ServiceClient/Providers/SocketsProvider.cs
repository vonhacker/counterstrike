using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Runtime.Serialization;

namespace CounterStrikeLive.ServiceClient
{
    public partial class SocketsProvider : ServiceClientProvider
    {
        Socket socket;

        Sender sender = new Sender();
        Listener listener = new Listener();

        EndPoint endPoint;

        public SocketsProvider(string Host, int Port)
        {
            endPoint = new DnsEndPoint(Host, Port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool IsConnected
        {
            get
            {
                if (socket == null) return false;

                return socket.Connected;
            }
        }

        public void Start()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            args.UserToken = socket;
            args.RemoteEndPoint = endPoint;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            socket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);
            socket.ConnectAsync(args);

            if (args.SocketError != SocketError.Success)
                throw new SocketException((int)args.SocketError);
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            bool isConnected = (e.SocketError == SocketError.Success);

            if (isConnected)
            {
                uiThread.Post(CallServerConnectedEvent, null);
                StartReceive();
            }
            else
            {
                uiThread.Post(CallServerFailedEvent, null);
            }
        }

        public override void SendMessage(object data)
        {
            byte[] byteData = data as byte[];
            byte[] buffer = new byte[byteData.Length + 1];
            buffer[0] = (byte)byteData.Length;

            Buffer.BlockCopy(byteData, 0, buffer, 1, byteData.Length);
            if (byteData.Length == 0) throw new Exception("Break");

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, 0, buffer.Length);
            socket.SendAsync(args);
        }

        #region Events


        //private void OnSend(object sender, SocketAsyncEventArgs e)
        //{
        //    if (e.SocketError == SocketError.Success)
        //    {
        //        if (e.LastOperation == SocketAsyncOperation.Send)
        //        {
        //            // Prepare receiving.
        //            //lock (onSendLock)
        //            {
        //                Socket s = e.UserToken as Socket;

        //                byte[] response = new byte[ResponseBufferSize];
        //                e.SetBuffer(response, 0, response.Length);
        //                e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnSend);
        //                e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
        //                s.ReceiveAsync(e);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //uiThread.Post(CallConnectionFailedEvent, null);
        //    }
        //}

        private MemoryStream memoryStream = new MemoryStream();
        private long position;

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            lock ("Receive")
            {
                memoryStream.Write(e.Buffer, 0, e.BytesTransferred);
                memoryStream.Seek(position, SeekOrigin.Begin);
                while (true)
                {
                    int length = memoryStream.ReadByte();
                    if (length == -1 || memoryStream.Length <= position + length) break;
                    Byte[] buffer = new byte[length];

                    memoryStream.Read(buffer, 0, length);
                    position = memoryStream.Position;
                    lock ("Get")
                        ProcessMessage(buffer);
                }
                memoryStream.Seek(0, SeekOrigin.End);
                StartReceive();
            }
        }

        private void StartReceive()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
            //_Socket.ReceiveBufferSize = 100;
            //_Socket.SendBufferSize = 100;
            args.SetBuffer(new byte[100], 0, 100);
            socket.ReceiveAsync(args);
        }


        #endregion

        #region "Error processing"

        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (s.Connected)
                        s.Close();
                }
            }

            //throw new SocketException((int)e.SocketError);
        }

        #endregion

        public override void Disconnect()
        {
            //Initialized = false;
            socket.Shutdown(SocketShutdown.Both);
            if (socket.Connected)
                socket.Close();
        }

    }
}
