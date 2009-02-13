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
    using System.ComponentModel;
    using System.Reflection;
    using System.Net;
    

    public partial class Program
    {
        
        public partial class GameServer
        {                                    
            ClientWait _ClientWait;
            public void StartAsync() { new Thread(Start).StartBackground("GameServer"); }
            private void Start()
            {
                "game server started".Trace();
                
                _ClientWait = new ClientWait() { _Port = _Config._GamePort };
                _ClientWait.StartAsync();
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
                    _Clients.Add(_Client);
                    _Client.Start();
                }

                StringBuilder sb = new StringBuilder();
                foreach(Client _Client in _Clients)
                {
                    _Client.Update();
                    sb.AppendFormat("{0}:{1}:{2} ", _Client._room, _Client._id, _Client._PingTime);
                }
                Logging._Title = sb.ToString();
                Thread.Sleep(20);
            }
            public class Client
            {
                public Socket _Socket;
                Listener _Listener;
                Sender _Sender;
                public int? _id;
                public int? _room;
                public Client[] _ClientsInRoom { get { return _Rooms[_room.Value]._Clients; } }
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
                    foreach (byte[] _bytes in _Listener.GetMessages())
                        OnReceive(_bytes);
                    if (!_Listener._Connected) Close();
                }

                
                private void OnReceive(byte[] _bytess)
                {
                    using (MemoryStream _ms = new MemoryStream(_bytess))
                    {
                        PacketType _packet = (PacketType)_ms.ReadB();
                        Debug.Assert(_packet.IsValid());                        
                        if (_room == null) //client not in room
                            switch (_packet)
                            {
                                case PacketType.joinroom:  //player join room 
                                    _room = _ms.ReadB();
                                    Debug.Assert(_room <= _Rooms.Count);
                                    _id = _ClientsInRoom.PutToNextFreePlace(this);
                                    _id.Trace("Sended Player id");
                                    Send(Config._ServerId, PacketType.playerid, new[] { (byte)_id });
                                    foreach (Client _Client in _ClientsInRoom) // send all clients id to joined player                                
                                        if (_Client != null && _Client != this)
                                            Send(_Client._id.Value, PacketType.PlayerJoined);
                                    SendToAll(Config._ServerId, PacketType.PlayerJoined );
                                    break;
                                case PacketType.getrooms:
                                    byte[] data = _XmlSerializerRoom.Serialize(_Rooms);
                                    Send(Config._ServerId, PacketType.rooms, data);
                                    break;
                                case PacketType.getip:
                                    Send(Config._ServerId,PacketType.ip, ((IPEndPoint)_Socket.RemoteEndPoint).Address.ToString().ToBytes());                                    
                                    break;
                                default:
                                    Debug.Fail("wrong packet");
                                    break;
                            }
                        else //client is in room
                            switch ((PacketType)_packet)
                            {
                                case PacketType.sendTo:
                                    int clientid = _ms.ReadB();
                                    if (_ClientsInRoom[clientid] != null) _ClientsInRoom[clientid].Send(_id.Value,_packet,_ms.Read());
                                    break;
                                case PacketType.ping:
                                    Send(Config._ServerId, PacketType.pong);                                    
                                    break;
                                case PacketType.pong:
                                    _PingTime = (int)_PingElapsed;
                                    SendToAll(_id.Value, PacketType.pinginfo, BitConverter.GetBytes((Int16)_PingElapsed), true);                                    
                                    _TimerA.AddMethod(1000, Ping);
                                    break;
                                default:
                                    SendToAll(_id.Value, _packet,_ms.Read());
                                    break;
                            }
                    }
                }

                public void Ping()
                {
                    _PingElapsed = 0;
                    Send(Config._ServerId, PacketType.ping);
                }

                private void Close()
                {
                    if (_room != null)
                    {
                        _Clients[_id.Value] = null;
                        _id.Trace("Client Disconected from " + _room);
                        SendToAll(_id.Value , PacketType.PlayerLeaved);
                    }
                    _Clients.Remove(this);
                }
                private void SendToAll(int id, PacketType _PacketType) { SendToAll(id, _PacketType, new byte[] { }); }
                private void SendToAll(int id, PacketType _PacketType, byte[] _data) { SendToAll(id, _PacketType, _data, false); }
                private void SendToAll(int id, PacketType _PacketType, byte[] _data, bool includeself)
                {                    
                    _data = _data.Join(new byte[] { (byte)_data.Length });
                    foreach (Client _Client in _ClientsInRoom)
                        if (_Client != null && (_Client != this || includeself))
                            _Client.Send(id,_PacketType,_data);
                }
                private void Send(int id, PacketType _PacketType) { Send(id, _PacketType, new byte[] { }); }
                private void Send(int id, PacketType _PacketType, byte[] _bytes)
                {
                    Debug.Assert(id < 255);
                    Debug.Assert(_PacketType.IsValid());
                    _Sender.Send(Helper.JoinBytes((byte)id, (byte)_PacketType, _bytes));
                }
            }
        }
    }
}
