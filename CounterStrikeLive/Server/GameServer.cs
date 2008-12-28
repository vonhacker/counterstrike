using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Specialized;

using Server.Properties;
using doru;
using System.Text.RegularExpressions;

namespace GameServer
{
    /// <summary>
    /// Advanced debug
    /// </summary>
    public static class Debug
    {
        public static void WriteLine(object obj)
        {
            WriteLine(obj.ToString());
        }
        public static void WriteLine(string obj)
        {
            System.Diagnostics.Trace.WriteLine("Server:" + obj);
        }
    }    

    public class Server
    {
        public List<string> _Console = new List<string>();
        public string _Map { get { return Settings.Default._Map; } }
        public int _WebPort { get { return Settings.Default._WebPort; } }

        public const byte _Serverid = 254;
        private readonly Client[] _Clients = new Client[20];
        private int _Port { get { return Settings.Default._GameServerPort; } }

        public void StartAsync()
        {
            Thread _Thread = new Thread(Start);
            _Thread.Name = "Server";
            _Thread.Start();
        }
        ClientWait _ClientWait = new ClientWait();
        FileInfo _FileInfo = new FileInfo("SilverlightApplication8.xap");
        public void Start()
        {
            Console.WriteLine("Server Started " + _Port);
            _ClientWait._Port = _Port;
            Thread _Thread = new Thread(_ClientWait.Start);
            _Thread.Name = "ClientWait";
            _Thread.Start();

            new Thread(SendHttp).Start();
            while (true)
            {
                Update();
            }
        }
        string title;
        public string _Title
        {
            set { if (title != value) title = Console.Title = value; }
        }
        double fps;
        private void Update()
        {
            List<TcpClient> _TCPClients = _ClientWait.GetClients();
            foreach (TcpClient _TcpClient in _TCPClients)
            {
                CreateNewClient(_TcpClient);
            }
            StringBuilder _StringBuilder = new StringBuilder("serv ");
            clientcount = 0;
            for (int i = 0; i < _Clients.Length; i++)
            {

                Client _Client = _Clients[i];
                if (_Client != null)
                {
                    clientcount++;
                    _Client.Update();
                    _StringBuilder.AppendFormat("{0}:{1} ", _Client._id, _Client._PingTime);
                }
            }
            for (int i = 0; i < _Console.Count; i++)
            {
                string s = _Console[i];
                _Console.RemoveAt(0);
                string s1 = Regex.Match(s,@"kick (\d+)").Groups[1].Value;
                if(s1.Length != 0)
                {
                    int a= int.Parse(s1);
                    if (_Clients[a] != null)
                    {
                        _Clients[a].Close();
                    }
                    else
                        Console.WriteLine("Client Not Found");
                }

            }
            if (STimer.TimeElapsed(200)) fps = (int)STimer.GetFps();
            _StringBuilder.Append(" " + fps + "fps");
            string s2 = _StringBuilder.ToString();
            _Title = s2;
            Thread.Sleep(10);
            STimer.Update();
        }
        int clientcount;

        void SendHttp(object _object)
        {
            try
            {
                while (true)
                {
                    string post = @"POST /cs/serv.php HTTP/1.1
Host: igorlevochkin.ig.funpic.org
Content-Type: application/x-www-form-urlencoded
Content-Length: _length_

name={0}&map={1}&version={2}&port={3}&players={4}";
                    post = String.Format(post, _ServerName, _Map, _FileInfo.LastWriteTime.ToShortDateString(),
                        _WebPort.ToString(), clientcount.ToString());
                    int len = post.IndexOf("\r\n\r\n") + 4;
                    if (len == 0) Debugger.Break();
                    post = post.Replace("_length_", len.ToString());


                    TcpClient _TcpClient = new TcpClient("igorlevochkin.ig.funpic.org", 80);
                    int count = _TcpClient.Client.Send(ASCIIEncoding.ASCII.GetBytes(post));
                    Thread.Sleep(200);
                    _TcpClient.Close();
                    Thread.Sleep(10000);
                }
            }
            catch (SocketException e) { Trace.WriteLine("phpSender:" + e.Message); }
        }
        public string _ServerName { get { return Settings.Default._ServerName; } }


        private void CreateNewClient(TcpClient _TcpClient)
        {
            Client _Client = new Client();
            _Client._Clients = _Clients;
            _Client._Server = this;
            _Client.Start(_TcpClient);
        }
    }
    internal class Client
    {
        public int _PingTime;
        private double _PingElapsed;
        private Listener _Listener;
        private Sender _Sender;
        public Client[] _Clients;
        public int _id = -1;

        public void Start(TcpClient _TcpClient)
        {            
            _Sender = new Sender();
            _Sender._TcpClient = _TcpClient;
            _Listener = new Listener();
            _Listener._TcpClient = _TcpClient;
            int id = Arrays.putToNextFreePlace(this, _Clients);
            _id = id;
            Console.WriteLine("Client Conneted:" + _id);
            Thread _Thread = new Thread(_Listener.Start);
            SendID();
            SendMapFileName();
            Ping();
            _Thread.Name = "_ClientListener:" + _id;
            _Thread.Start();            
        }
        public Server _Server;

        private void SendMapFileName()
        {
            using (var _MemoryStream = new MemoryStream())
            {
                var _BinaryWriter = new BinaryWriter(_MemoryStream);
                _BinaryWriter.Write((byte)PacketType.serverid);
                _BinaryWriter.Write((byte)PacketType.map);
                _BinaryWriter.Write(_Server._Map);
                Send(_MemoryStream.ToArray());
            }
        }

        private void SendID()
        {
            Debug.WriteLine("Sended Player id:" + _id);
            Send(new byte[] { (byte)Server._Serverid, (byte)PacketType.playerid, (byte)_id });
            foreach (Client _Client in _Clients) // send all clients id to joined player
            {
                if (_Client != null && _Client != this)
                    Send(new byte[] { (byte)_Client._id, (byte)PacketType.PlayerJoined });
            }
            SendToAll(new byte[] { (byte)PacketType.PlayerJoined });
        }

        public void Send(byte[] _buffer)
        {
            _Sender.Send(_buffer);
        }

        public void Close()
        {
            _Clients[_id] = null;
            Console.WriteLine("Client Disconected");
            var _Data = new[] { (byte)PacketType.PlayerLeaved };
            SendToAll(_Data);
            _Listener._TcpClient.Client.Close();
        }
        public int _Received;
        private void onReceive(byte[] _data)
        {
            _Received++;
            switch ((PacketType)_data[0])
            {                                                            
                case PacketType.sendTo:
                    {
                        byte[] _Data1 = new byte[_data.Length - 1]; //-2 sendto,id +1 sendfrom-id
                        Buffer.BlockCopy(_data, 2, _Data1, 1, _data.Length - 2);
                        _Data1[0] = (byte)_id;
                        if(_Clients[_data[1]]!=null) _Clients[_data[1]].Send(_Data1);
                    }
                    break;
                case PacketType.ping:
                    {
                        _Sender.Send(new byte[] { (byte)PacketType.serverid, (byte)PacketType.pong });

                    }
                    break;
                case PacketType.pong:
                    {
                        _PingTime = (int)_PingElapsed;
                        using (MemoryStream _MemoryStream = new MemoryStream())
                        {
                            BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                            _BinaryWriter.Write((byte)PacketType.pinginfo);
                            _BinaryWriter.Write((Int16)_PingElapsed);
                            SendToAll(_MemoryStream.ToArray(), true, _id);
                        }
                        STimer.AddMethod(1000, Ping);
                    }
                    break;
                default:
                    {
                        SendToAll(_data);
                    }
                    break;
            }
        }
        private void SendToAll(byte[] _data)
        {
            SendToAll(_data, false, _id);
        }
        private void SendToAll(byte[] _data, bool includeself, int id)
        {
            var _Data1 = new byte[_data.Length + 1];
            _Data1[0] = (byte)_id;
            Buffer.BlockCopy(_data, 0, _Data1, 1, _data.Length);

            foreach (Client _Client in _Clients)
            {
                if (_Client != null)
                    if (_Client != this || includeself)
                        _Client.Send(_Data1);
            }
        }
        public void Ping()
        {
            _PingElapsed = 0;
            Send(new byte[] { Server._Serverid, (byte)PacketType.ping });
        }
        public void Update()
        {
            _PingElapsed += STimer._TimeElapsed;
            List<byte[]> _messages = _Listener.GetMessages();
            foreach (byte[] _data in _messages)
            {
                onReceive(_data);
            }
            if (_Listener._Connected == false)
                Close();
        }
        
    }
    public enum PacketType : byte
    {
        connect = 188,
        serverid = 254,
        playerid = 89,
        possitions = 56,
        PlayerLeaved = 23,
        PlayerJoined = 24,
        map = 21,
        nick = 86,
        sendTo = 27,
        ping = 99,
        pinginfo = 100,
        pong = 98
    }
    public static class Arrays
    {

        public static int putToNextFreePlace<T>(T item, IList<T> items)
        {
            int id = items.IndexOf(default(T));
            if (id == -1)
            {
                items.Add(item);
                return items.Count - 1;
            }
            else
            {
                items[id] = item;
                return id;
            }
        }
    }

}
