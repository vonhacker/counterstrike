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
        private const int ResponseBufferSize = 512;

        private Socket socket;
        private DnsEndPoint endPoint;
        private Dispatcher _owner;        
        private bool Initialized = false;
        private object initData = null;

        private List<string> MessagesToProcess = null;

        private object receiveLock = new object();
        private static object sendLock = new object();
        private static object onSendLock = new object();
        MemoryStream stream = null;
        private static bool ReceiveThreadBusy = false;

        public SocketsProvider(string host, int port)
        {
            
            //_owner = UIElement.Dispatcher;
            endPoint = new DnsEndPoint(host, port, AddressFamily.InterNetwork);
            MessagesToProcess = new List<string>();
            stream = new MemoryStream();
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

            //if (isConnected)
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(LoginRequestInfo));
            //    string Data = Commons.Serialize(xs, _info);

            //    SendMessage(Data);
            //}

            if (isConnected)
            {
                Initialized = true;

                SendMessage(initData);
                //_owner.BeginInvoke(
                //    delegate
                //    {
                //        if (ConnectFailed != null)
                //        {
                //            ConnectFailed(this, new EventArgs());
                //        }
                //    });
            }
        }

        protected override void SendMessage(object data)
        {
            if (data == null) return;

            if (!Initialized)
            {
                initData = data;
                Start();
            }
            else
            {
                if (IsConnected)
                {
                    //lock (sendLock)
                    {
                        //MemoryStream stream = new MemoryStream();
                        //DataContractSerializer dcs = new DataContractSerializer(typeof(object), KnownTypes.Get());
                        //dcs.WriteObject(stream, data);
                        //stream.Position = 0;
                        //StreamReader reader = new StreamReader(stream);
                        //string xmlData = reader.ReadToEnd();

                        //List<byte> bytes = new List<byte>();
                        //bytes.AddRange(BitConverter.GetBytes(xmlData.Length));
                        //bytes.AddRange(Encoding.UTF8.GetBytes(xmlData));

                        //SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                        //args.SetBuffer(bytes.ToArray(), 0, bytes.Count);
                        //args.UserToken = socket;
                        //args.RemoteEndPoint = endPoint;
                        //args.Completed -= new EventHandler<SocketAsyncEventArgs>(OnConnect);
                        //args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

                        //socket.SendAsync(args);
                    }
                }
                else
                {
                    //uiThread.Post(CallConnectionFailedEvent, null);
                }
            }
        }

        #region Events


        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    // Prepare receiving.
                    //lock (onSendLock)
                    {
                        Socket s = e.UserToken as Socket;

                        byte[] response = new byte[ResponseBufferSize];
                        e.SetBuffer(response, 0, response.Length);
                        e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnSend);
                        e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                        s.ReceiveAsync(e);
                    }
                }
            }
            else
            {
                //uiThread.Post(CallConnectionFailedEvent, null);
            }
        }

        //private void ParseStream(Stream stream)
        //{
        //    string frame = string.Empty;
        //    if (stream.Length > 0)
        //    {
        //        int size = TryParseSize(stream);

        //        if (size > 0)
        //        {
        //            frame = TryParseFrame(stream, size);
        //            if (!string.IsNullOrEmpty(frame))
        //            {
        //                MessagesToProcess.Add(frame);
        //            }
        //        }
        //    }

        //    if (stream.Length > 0 && !string.IsNullOrEmpty(frame))
        //    {
        //        stream.Position = 0;
        //        ParseStream(stream);
        //    }
        //}

        //private int TryParseSize(Stream stream)
        //{
        //    int size = -1;

        //    if (stream.Length >= 4)
        //    {
        //        byte[] buffer = new byte[4];
        //        stream.Read(buffer, 0, buffer.Length);
        //        size = BitConverter.ToInt32(buffer, 0);
        //        stream.Position -= buffer.Length;

        //        if (size > 100000)
        //        {
        //            uiThread.Post(delegate(object obj)
        //            {
        //                Application.Log((string)obj);
        //            }, "Message size " + size.ToString() + " is too big." + Environment.NewLine +
        //                                "Fatal communication error!");
        //            //throw new Exception("Message size " + size.ToString() + " is too big." + Environment.NewLine +
        //            //            "Fatal communication error!");
        //        }
        //    }

        //    return size;
        //}

        //private string TryParseFrame(Stream stream, int Size)
        //{
        //    string frame = string.Empty;

        //    Size += 4;

        //    if (stream.Length - stream.Position >= Size)
        //    {
        //        byte[] buffer = new byte[Size - 4];
        //        stream.Position = 4;
        //        stream.Read(buffer, 0, buffer.Length);
        //        frame = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

        //        // Trim stream
        //        byte[] byteBuffer = new byte[stream.Length];
        //        stream.Position = 0;
        //        stream.Read(byteBuffer, 0, byteBuffer.Length);
        //        stream.SetLength(0);
        //        List<byte> bytes = new List<byte>(byteBuffer).GetRange(Size, byteBuffer.Length - Size);
        //        stream.Write(bytes.ToArray(), 0, bytes.Count);
        //    }

        //    return frame;
        //}

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            //uiThread.Post(delegate(object obj)
            //{
            //    Commons.Storage.Log((string)obj);
            //}, "Enter OnReceive before lock");
            lock (stream)
            {
                if (ReceiveThreadBusy)
                {
                    //uiThread.Post(delegate(object obj)
                    //{
                    //    Application.Log((string)obj);
                    //}, "Stream concurrent access error.");
                }
                ReceiveThreadBusy = true;
                //uiThread.Post(delegate(object obj)
                //{
                //    Commons.Storage.Log((string)obj);
                //}, "Enter OnReceive after lock");
//                string str = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
                try
                {
                    //stream.SetLength(stream.Length + e.BytesTransferred);
                    //stream.Position = stream.Length - e.BytesTransferred;
                    //stream.Write(e.Buffer, 0, e.BytesTransferred);
                    //stream.Position = 0;
                    //ParseStream(stream);

                    //while (MessagesToProcess.Count != 0)
                    //{
                    //    string messageToProcess = "<root>" + MessagesToProcess.First() + "</root>";
                    //    MessagesToProcess.Remove(MessagesToProcess.First());

                    //    XDocument xdoc = XDocument.Parse(messageToProcess);

                    //    XElement root = new List<XElement>(xdoc.Descendants(XName.Get("root")))[0];

                    //    foreach (XElement elem in root.Elements())
                    //    {
                    //        string xmlData = elem.ToString();
                    //        MemoryStream xmlStream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlData));
                    //        DataContractSerializer dcs = new DataContractSerializer(typeof(object), KnownTypes.Get());
                    //        object data = dcs.ReadObject(xmlStream);

                    //        ProcessMessage(data);
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    ReceiveThreadBusy = false;
                }

                //uiThread.Post(delegate(object obj)
                //{
                //    Commons.Storage.Log((string)obj);
                //}, "Exit OnReceive in lock");
            }
            //uiThread.Post(delegate(object obj)
            //{
            //    Commons.Storage.Log((string)obj);
            //}, "Exit OnReceive out lock");
            Socket s = e.UserToken as Socket;

            byte[] response = new byte[ResponseBufferSize];
            e.SetBuffer(response, 0, response.Length);
            s.ReceiveAsync(e);

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
            Initialized = false;
            socket.Shutdown(SocketShutdown.Both);
            if (socket.Connected)
                socket.Close();
        }

    }
}
