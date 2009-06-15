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
using CounterStrikeLive.Server.Properties;
using doru;
using System.Text.RegularExpressions;
using System.Reflection;
using CounterStrikeLive.Server;
using CounterStrikeLive.Service;
using doru.Tcp;


namespace CounterStrikeLive.Server
{
    public static class Debug
    {
        public static void WriteLine(object obj)
        {
            WriteLine(obj.ToString());
        }
        public static void WriteLine(string obj)
        {
            System.Diagnostics.Trace.WriteLine("CounterStrikeLive.CounterStrikeLive.Server:" + obj);
        }
    }
    

    public class GameServer
    {
        public static GameServer _This;
        public GameServer()
        {
            _This = this;
        }

        public List<string> _Console { get { return Logging._console; } }
        Config _Config = Config._This;
        public string _Map;
        public int _WebPort { get { return _Config._WebPort; } }
        public int _Port { get { return _Config._GamePort; } }
        public string _ServerName { get { return Settings.Default._ServerName; } }

        public const byte _Serverid = 254;
        private readonly Client[] _Clients = new Client[20];


        public void StartAsync()
        {
            Thread _Thread = new Thread(Start);
            _Thread.Name = "CounterStrikeLive.CounterStrikeLive.Server";
            _Thread.Start();
        }
        ClientWait _ClientWait = new ClientWait();
        public void Start()
        {
            Console.WriteLine("CounterStrikeLive.CounterStrikeLive.Server Started " + _Port);
            _ClientWait._Port = _Port;
            _ClientWait.StartAsync();                        

            
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
            List<Socket> _TCPClients = _ClientWait.GetClients();
            foreach (Socket _TcpClient in _TCPClients)
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
            UpdateConsole();
            if (_Timer4.TimeElapsed(200)) fps = (int)_Timer4.GetFps();
            _StringBuilder.Append(" " + fps + "fps");
            string s2 = _StringBuilder.ToString();
            //_Title = s2;
            Thread.Sleep(10);
            _Timer4.Update();
        }

        private void UpdateConsole()
        {
            for (int i = 0; i < _Console.Count; i++)
            {
                string s = _Console[i];
                _Console.RemoveAt(0);
                string s1 = Regex.Match(s, @"kick (\d+)").Groups[1].Value;
                if (s1.Length != 0)
                {
                    int a = int.Parse(s1);
                    if (_Clients[a] != null)
                    {
                        _Clients[a].Close();
                    }
                    else
                        Console.WriteLine("Client Not Found");
                }

            }
        }
        TimerA _Timer4 = new TimerA();
        public int clientcount;
        public int id = Helper._Random.Next(99999);
        


        

        private void CreateNewClient(Socket _TcpClient)
        {
            Client _Client = new Client();
            _Client._Clients = _Clients;
            _Client._Server = this;
            _Client.Start(_TcpClient);
        }
        internal class Client
        {
            public bool _isJoined;
            public static Settings Settings { get { return Settings.Default; } }
            public int _PingTime;
            private double? _PingElapsed;
            private Listener _Listener;
            private Sender _Sender;
            public Client[] _Clients = null;
            public IEnumerable<Client> _ClientList { get { return (from a in _Clients where a != null && a._isJoined select a); } }
            public IEnumerable<Client> _ClientListFull { get { return (from a in _Clients where a != null select a); } }
            public int _id = -1;
            Config _Config = Config._This;
            public void Start(Socket _Socket)
            {
                _Sender = new Sender();
                _Sender._Socket = _Socket;
                _Listener = new Listener();
                _Listener._Socket = _Socket;

                _Sender._NetworkStream = _Listener._NetworkStream = _Config.GenerateServerLag ? new LagStream(_Socket) : new NetworkStream(_Socket);
                _Listener.StartAsync("client");

                _id = _Clients.PutToNextFreePlace(this);
                Console.WriteLine("Client Conneted:" + _id);
                Send(new byte[] { (byte)GameServer._Serverid, (byte)PacketType.playerid, (byte)_id });

                if (_ClientList.Count() == 0)
                {
                    Send(PacketType.SelectMap);
                } else
                    Send(PacketType.map, _Server._Map.ToBytes());        
            }
            public GameServer _Server;



            
            public void Send(PacketType _PacketType)
            {
                Send(new byte[] { (byte)PacketType.serverid, (byte)_PacketType });
            }
            public void Send(PacketType _PacketType, byte[] data)
            {
                Send(Helper.JoinBytes((byte)PacketType.serverid, (byte)_PacketType, data));
            }
            

            private void SendJoin()
            {
                Debug.WriteLine("Sended join");
                foreach (Client _Client in _Clients) // send all clients id to joined player
                {
                    if (_Client != null && _Client != this)
                        Send(new byte[] { (byte)_Client._id, (byte)PacketType.PlayerJoined });
                }
                SendToAll(new byte[] { (byte)PacketType.PlayerJoined });
            }

            public void Send(byte[] _buffer)
            {
                if(_Sender._Socket.Connected)
                    _Sender.Send(_buffer);
            }
            public void Close()
            { Close("Client Disconected"); }
            public void Close(string s)
            {
                _Clients[_id] = null;
                this.Trace(s);
                var _Data = new[] { (byte)PacketType.PlayerLeaved };
                SendToAll(_Data);
                _Listener._Socket.Close();
            }
            public int _Received;
            public string _MapVoting;            
            private void onReceive(byte[] _data)
            {
                _Received++;
                using (MemoryStream _MemoryStream = new MemoryStream(_data))                
                switch ((PacketType)_MemoryStream.ReadB())
                {                    
                    case PacketType.sendTo:
                        {
                            byte[] _Data1 = new byte[_data.Length - 1]; //-2 sendto,id +1 sendfrom-id
                            Buffer.BlockCopy(_data, 2, _Data1, 1, _data.Length - 2);
                            _Data1[0] = (byte)_id;
                            if (_Clients[_data[1]] != null) _Clients[_data[1]].Send(_Data1);
                        }
                        break;
                    case PacketType.Join:
                        Debug.WriteLine("Sended Player id:" + _id);
                        _isJoined = true;
                        SendJoin();
                        Ping();
                        break;
                    case PacketType.voteMap:
                        {
                            SendToAll(_data);
                            _MapVoting = _MemoryStream.ReadStringToEnd();
                            if (GetVoteCount() > _ClientList.Count() / 2)
                            {
                                _Server._Map = _MapVoting;
                                SendToAll(PacketType.map, (byte)PacketType.serverid, true, true, _Server._Map.ToBytes());
                            }
                        }
                        break;
                    case PacketType.MapSelected:
                        {
                            if (_ClientList.Count() == 0)                            
                            {
                                string s = _MemoryStream.ReadStringToEnd();
                                Trace.Assert(s != null && s != "");
                                _Server._Map = s;                                
                            }
                            SendToAll(PacketType.map, (byte)PacketType.serverid, true, true, _Server._Map.ToBytes());
                        } break;
                    case PacketType.ping:
                        {
                            Send(new byte[] { (byte)PacketType.serverid, (byte)PacketType.pong });
                        }
                        break;
                    case PacketType.pong:
                        {
                            _PingTime = (int)_PingElapsed;
                            SendToAll(PacketType.pinginfo, _id, true, false, BitConverter.GetBytes((Int16)_PingElapsed));
                            _PingElapsed = null;
                            _Server._Timer4.AddMethod(1000, Ping);
                        }
                        break;
                    default:
                        {
                            SendToAll(_data);
                        }
                        break;
                }
            }

            private int GetVoteCount()
            {
                return (from a in _ClientList where a._MapVoting == _MapVoting select a).Count();
            }


            private void SendToAll(PacketType _PacketType, int id, bool incudeself, bool includeallClients)
            {
                SendToAll(new byte[]{(byte)_PacketType}, incudeself, id, includeallClients);
            }

            private void SendToAll(PacketType _PacketType, int id, bool incudeself, bool includeallClients, params object[] _data)
            {
                SendToAll(Helper.JoinBytes((byte)_PacketType, Helper.JoinBytes(_data)), incudeself, id, includeallClients);
            }

            
            private void SendToAll(PacketType _PacketType, int id, byte[] _data)
            {
                SendToAll(Helper.JoinBytes((byte)_PacketType, _data), false, id,false);
            }
            
            private void SendToAll(byte[] _data) { SendToAll(_data, false, _id,false); }

            private void SendToAll(byte[] _data, bool includeself, int id,bool includeallClients)
            {
                Trace.Assert(id != -1);
                var _Data1 = new byte[_data.Length + 1];
                _Data1[0] = (byte)id;
                Buffer.BlockCopy(_data, 0, _Data1, 1, _data.Length);

                foreach (Client _Client in _Clients)
                    if (_Client != null && _Client != this)
                        if (_Client._isJoined || includeallClients)
                            _Client.Send(_Data1);
                if (includeself) this.Send(_Data1);

            }
            public void Ping()
            {
                _PingElapsed = 0;
                Send(new byte[] { GameServer._Serverid, (byte)PacketType.ping });
            }
            public override string ToString()
            {
                return "Client" + _id;
            }
            public void Update()
            {
                if (_PingElapsed != null)
                {
                    _PingElapsed += _Server._Timer4._TimeElapsed;
                    if (_Config._MaxLatency != null && _PingElapsed > _Config._MaxLatency) Close("Kicked To High Latency");
                }
                List<byte[]> _messages = _Listener.GetMessages();
                foreach (byte[] _data in _messages)
                    onReceive(_data);
                if (_Listener._Connected == false)
                    Close();
            }

        }
    }




}
