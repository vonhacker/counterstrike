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
    public class SharedObjectAttribute : Attribute
    {
        public int _Priority;
        public SharedObjectAttribute() { }
        public SharedObjectAttribute(int _Priority)
        {
            this._Priority = _Priority;
        }
    }
    public interface ISh<T>
    {
        T _SharedObj { get; set; }
    }
    public abstract class SharedObj<T> where T : class, INotifyPropertyChanged , ISh<SharedObj<T>>
    {
        public T _Object = Activator.CreateInstance<T>(); 
        protected List<PropertyInfo> _Properties;
        public static implicit operator T (SharedObj<T> s)
        {            
            return s._Object;
        }
        public SharedObj()
        {            
            _Properties = (from p in _Object.GetType().GetProperties() where (p.GetCustomAttributes(true).FirstOrDefault(a => a is SharedObjectAttribute) != null) select p).ToList();
            if(_Properties.Count == 0) throw new Exception("sharedobject does not have attributes");
        }
    }
    public class LocalSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh<SharedObj<T>>
    {
        public MemoryStream _ms = new MemoryStream();
        public LocalSharedObj()
        {
            _Object.PropertyChanged += new PropertyChangedEventHandler(Object_PropertyChanged);            
        }                        
        //private byte[] Serialize()
        //{
        //    MemoryStream ms = new MemoryStream();            
        //    for(int i = 0; i < _Properties.Count; i++)
        //        WriteBytes(i, ms);
        //    return ms.ToArray();
        //}
        public void Serialize(MemoryStream ms)
        {            
            for (int i = 0; i < _Properties.Count; i++)
                if (_Properties[i].GetCustomAttributes(true).Any(a => a is SharedObjectAttribute))
                    WriteBytes(i, _ms);                
        }
        public byte[] GetChanges()
        {
            if (_ms.Length == 0) return new byte[] { };
            using (_ms)            
                return _ms.ToArray();                        
        }
        private void WriteBytes(int i, MemoryStream _ms)
        {
            PropertyInfo _PropertyInfo = _Properties[i];
            object value = _PropertyInfo.GetValue(_Object, null);

            _ms.WriteByte((byte)i);
            if(value is int) _ms.Write((Int16)(int)value);
            else if(value is string) _ms.Write((string)value);
            else if(value is Enum) _ms.Write(value.ToString());
            else if(value is float) _ms.Write((float)value);
            else if(value is byte) _ms.Write((byte)value);
            else if(value is bool) _ms.Write((bool)value);
            else throw new Exception("Shared Send Unkown value");

        }
                
        void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WriteBytes(_Properties.IndexOf(_Object.GetType().GetProperty(e.PropertyName)),_ms);
        }
    }
    public class RemoteSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh<SharedObj<T>>
    {
        public void OnBytesToRead(MemoryStream ms)
        {
            while (ms.Position != ms.Length)
            {
                int id = ms.ReadByte();
                PropertyInfo _PropertyInfo = _Properties[id];
                if (_PropertyInfo.GetCustomAttributes(true).FirstOrDefault(a => a is SharedObjectAttribute) == null)
                    throw new Exception("Break");
                Type type = _PropertyInfo.PropertyType;
                if (type.IsAssignableFrom(typeof(int)))
                    _PropertyInfo.SetValue(_Object, ms.ReadInt16(), null);
                else if (type.IsAssignableFrom(typeof(string)))
                    _PropertyInfo.SetValue(_Object, ms.ReadString(), null);
                else if (type.BaseType == typeof(Enum))
                    _PropertyInfo.SetValue(_Object, Enum.Parse(type, ms.ReadString(), false), null);
                else if (type.IsAssignableFrom(typeof(float)))
                    _PropertyInfo.SetValue(_Object, ms.ReadSingle(), null);
                else if (type.IsAssignableFrom(typeof(bool)))
                    _PropertyInfo.SetValue(_Object, ms.ReadBoolean(), null);
                else throw new Exception("Break");
            }
        }
        
    }

    public partial class Program
    {
        public static XmlSerializer _XmlSerializerRoom = Helper.CreateSchema("room", typeof(List<RoomDb>), typeof(RoomDb), typeof(CSRoom), typeof(WormsRoom));
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
                
                
                private void OnReceive(byte[] _bytess)
                {
                    using(MemoryStream _ms = new MemoryStream(_bytess))
                    {
                        PacketType _packet = (PacketType)_ms.ReadB();
                        if(_id == null) //client not in room
                            switch(_packet)
                            {
                                case PacketType.roomid:  //player join room 
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
                                case PacketType.getrooms:
                                    byte[] data =_XmlSerializerRoom.Serialize(_Rooms);
                                    _Sender.Send(new byte[] { (byte)PacketType.rooms }.Join(data));
                                    break;
                            }
                        if(_id != null) //client is in room
                            switch((PacketType)_packet)
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
            public static List<RoomDb> _Rooms { get { return _Config.Rooms; } }
            ClientWait _ClientWait;
            public void StartAsync() { new Thread(Start).StartBackground("GameServer"); }
            public void Start()
            {
                "game server started".Trace();
                _ClientWait = new ClientWait() { _Port = _Config._GamePort };
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
