namespace CSLIVE.Server
{
#if(!SILVERLIGHT)
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
                    foreach (byte[] _bytes in _Listener.GetMessages())
                        OnReceive(_bytes);
                    if (!_Listener._Connected) Close();
                }

                private void OnReceive(byte[] _bytess)
                {
                    using (MemoryStream _ms = new MemoryStream(_bytess))
                    {
                        PacketType _pk = (PacketType)_ms.ReadB();
                        if (_id == null)
                            switch (_pk)
                            {
                                case PacketType.roomid:
                                    _room = _ms.ReadB();
                                    if (_room > _Rooms.Count) throw new Exception("unknown room");
                                    _id = _Clients.PutToNextFreePlace(this);
                                    _id.Trace("Sended Player id");
                                    Send(new byte[] { (byte)PacketType.serverid, (byte)PacketType.playerid, (byte)_id });
                                    foreach (Client _Client in _Clients) // send all clients id to joined player                                
                                        if (_Client != null && _Client != this)
                                            Send(new byte[] { (byte)_Client._id, (byte)PacketType.PlayerJoined });
                                    SendToAll(new byte[] { (byte)PacketType.PlayerJoined });
                                    break;
                                case PacketType.getmap:
                                    _Sender.Send(new byte[] { (byte)PacketType.map }.Join(_Rooms[_ms.ReadB()]._data));
                                    break;
                            }
                        if (_id != null)
                            switch ((PacketType)_pk)
                            {
                                case PacketType.sendTo:
                                    int clientid = _ms.ReadB();
                                    if (_Clients[clientid] != null) _Clients[clientid].Send(_ms.Read());
                                    break;
                                case PacketType.ping:
                                    _Sender.Send(new byte[] { (byte)PacketType.serverid, (byte)PacketType.pong });
                                    break;
                                case PacketType.pong:
                                    _PingTime = (int)_PingElapsed;
                                    using (MemoryStream _MemoryStream = new MemoryStream())
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
                    if (_data.Length > byte.MaxValue) throw new Exception("cannot send,packet is too large");
                    _data = _data.Join(new byte[] { (byte)_data.Length });
                    foreach (Client _Client in _Clients)
                        if (_Client != null && (_Client != this || includeself))
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
                while (true)
                {
                    Update();
                }
            }

            private void Update()
            {
                foreach (Socket s in _ClientWait.GetClients())
                {
                    Client _Client = new Client { _Socket = s };
                    _Client.Start();
                }
            }
        }
    }
#endif
    [Serializable]
    public abstract class Room
    {
        public static BinaryFormatter _BinaryFormatter = new BinaryFormatter();
        public virtual byte[] _data
        {
            get
            {
                using (MemoryStream _MemoryStream = new MemoryStream())
                {
                    _BinaryFormatter.Serialize(_MemoryStream, this);
                    return _MemoryStream.ToArray();
                }
            }
        }
#if(!SILVERLIGHT)  
        [NonSerialized]
        [XmlIgnore]
        public Program.GameServer.Client[] _Clients = new Program.GameServer.Client[255];
#endif
    }
    [Serializable]
    public class CSRoom : Room //cslive room
    {
        public string mapname;        
    }
    [Serializable]
    public class ServerListRoom : Room { }
    [Serializable]
    public class WormsRoom : Room { } //future worms game room
    public enum PacketType : byte
    {        
        connect = 188,
        possitions = 56,
        nick = 86,
        /// <summary>
        /// client->clients Chat message [[byte - length][text]]
        /// </summary>
        chat = 222,
        /// <summary>
        /// client->clients To Call method CheckWins() []
        /// </summary>
        checkWins = 154,
        /// <summary>
        /// client->clients SharedObject Property  [[Property id][value]]
        /// </summary>
        sharedObject = 232,
        /// <summary>
        /// client->clients Key Pressed [[key code]]
        /// </summary>
        keyDown = 133,
        /// <summary>
        /// client->clients Key Released [[key code]]
        /// </summary>
        keyUp = 134,
        pong = 98,
        ping = 99,
        /// <summary>
        /// Server->client player Ping [[ping - int16]]
        /// </summary>
        pinginfo = 100,
        /// <summary>
        /// client->client add point to player
        /// </summary>
        addPoint = 66,
        /// <summary>
        /// client->client extra damage shoot (head shoot) [byte - player angle]
        /// </summary>
        firstshoot = 45,
        /// <summary>
        /// client->client shoot [byte - player angle]
        /// </summary>
        shoot = 46,
        /// <summary>
        /// static server id
        /// </summary>
        serverid = 254,
        /// <summary>
        /// server->client sets player id [byte]
        /// </summary>
        playerid = 89,
        /// <summary>
        /// client->client player rotation 
        /// </summary>        
        rotation = 56,
        /// <summary>
        /// server->clients player disconnected
        /// </summary>
        PlayerLeaved = 23,
        /// <summary>
        /// server->clients player connected
        /// </summary>
        PlayerJoined = 24,
        /// <summary>
        /// client->server client asks map file
        /// </summary>
        getmap = 40,
        /// <summary>
        /// server->client map [BinaryFormater Room._data]
        /// </summary>
        map = 21,
        /// <summary>
        /// client->client [byte client id][data]
        /// </summary>
        sendTo = 27,
        /// <summary>
        /// client->server first message from client - join room, after that server send player id [byte] 
        /// </summary>
        roomid = 39
    }
}
