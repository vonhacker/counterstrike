namespace CSLIVE.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using doru;
    using System.Xml.Serialization;
    using System.IO;
    using doru.Tcp;
    using System.Net.Sockets;
    using System.Threading;
    using System.Diagnostics;
    using System.Runtime.Serialization.Formatters.Binary;


    public partial class Program
    {

        public class GameServer
        {

            public static TimerA _TimerA = new TimerA();
            public class Client
            {

                public Socket _Socket;
                Listener _Listener;
                Sender _Sender;
                public int? _id;
                public int? _room;
                public Client[] _Clients { get { return _Rooms[_room.Value]._Clients; } }
                public double _PingElapsed;
                public int _PingTime;
                public void Start()
                {
                    _Listener = new Listener() { _Socket = _Socket };
                    _Listener.StartAsync(_id.ToString());
                    _Sender = new Sender() { _Socket = _Socket };
                }

                public void Update()
                {
                    _PingElapsed += _TimerA._TimeElapsed;
                    foreach(byte[] _bytes in _Listener.GetMessages())
                        OnReceive(_bytes);
                    if(!_Listener._Connected) Close();
                }
                XmlSerializer _XmlSerializer = Helper.CreateSchema("room", typeof(Room), typeof(CSRoom), typeof(WormsRoom));
                private void OnReceive(byte[] _bytess)
                {
                    using(MemoryStream _ms = new MemoryStream(_bytess))
                    {
                        PacketType _pk = (PacketType)_ms.ReadB();
                        if(_id == null)
                            switch(_pk)
                            {
                                case PacketType.roomid:
                                    _room = _ms.ReadB();
                                    if(_room > _Rooms.Count) throw new Exception("unknown room");
                                    _id = _Clients.PutToNextFreePlace(this);
                                    _id.Trace("Sended Player id");
                                    Send(new byte[] { (byte)PacketType.serverid, (byte)PacketType.playerid, (byte)_id });
                                    foreach(Client _Client in _Clients) // send all clients id to joined player                                
                                        if(_Client != null && _Client != this)
                                            Send(new byte[] { (byte)_Client._id, (byte)PacketType.PlayerJoined });
                                    SendToAll(new byte[] { (byte)PacketType.PlayerJoined });
                                    break;
                                case PacketType.getmap:
                                    byte[] data =_XmlSerializer.Serialize(_Rooms[_ms.ReadB()]);
                                    _Sender.Send(new byte[] { (byte)PacketType.map }.Join(data));
                                    break;
                            }
                        if(_id != null)
                            switch((PacketType)_pk)
                            {
                                case PacketType.sendTo:
                                    int clientid = _ms.ReadB();
                                    if(_Clients[clientid] != null) _Clients[clientid].Send(_ms.Read());
                                    break;
                                case PacketType.ping:
                                    _Sender.Send(new byte[] { (byte)PacketType.serverid, (byte)PacketType.pong });
                                    break;
                                case PacketType.pong:
                                    _PingTime = (int)_PingElapsed;
                                    using(MemoryStream _MemoryStream = new MemoryStream())
                                    {
                                        BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                                        _BinaryWriter.Write((byte)PacketType.pinginfo);
                                        _BinaryWriter.Write((Int16)_PingElapsed);
                                        SendToAll(_MemoryStream.ToArray(), true, _id.Value);
                                    }
                                    _TimerA.AddMethod(1000, Ping);
                                    break;
                                default:
                                    SendToAll(_bytess);
                                    break;
                            }
                    }
                }
                public void Ping()
                {
                    _PingElapsed = 0;
                    Send(new byte[] { (byte)PacketType.serverid, (byte)PacketType.ping });
                }

                private void Close()
                {
                    _Clients[_id.Value] = null;
                    _id.Trace("Client Disconected");
                    SendToAll(new byte[] { (byte)PacketType.PlayerLeaved });
                }

                private void SendToAll(byte[] _data) { SendToAll(_data, false, _id.Value); }
                private void SendToAll(byte[] _data, bool includeself, int id)
                {
                    if(_data.Length > byte.MaxValue) throw new Exception("cannot send,packet is too large");
                    _data = _data.Join(new byte[] { (byte)_data.Length });
                    foreach(Client _Client in _Clients)
                        if(_Client != null && (_Client != this || includeself))
                            _Client.Send(_data);
                }

                private void Send(byte[] _bytes)
                {
                    _Sender.Send(_bytes);
                }
            }
            public static List<Room> _Rooms { get { return _Database.Rooms; } }
            ClientWait _ClientWait;
            public void StartAsync() { new Thread(Start).StartBackground("GameServer"); }
            public void Start()
            {
                "game server started".Trace();
                _ClientWait = new ClientWait() { _Port = _Database._GamePort };
                _ClientWait.Start();
                while(true)
                {
                    Update();
                }
            }

            private void Update()
            {
                foreach(Socket s in _ClientWait.GetClients())
                {
                    Client _Client = new Client { _Socket = s };
                    _Client.Start();
                }
            }
        }
    }
}
