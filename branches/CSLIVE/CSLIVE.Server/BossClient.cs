using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using doru.Tcp;
using System.IO;
using System.Threading;
using doru;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Diagnostics;
namespace CSLIVE.Server
{
    public partial class Program
    {
        public class BossClient
        {
            Listener _Listener;
            Sender _Sender;
            
            Client _Local = new LocalSharedObj<Client>();
            public void StartAsync()
            {
                new Thread(Start).StartBackground("BossClient");
            }
            private void Start()
            {
                "Boss Client started".Trace();
                Client _Client = new LocalSharedObj<Client>();

                _Local._Name = _Config._ServerName;                
                string[] ipport = _Config._BossServerIp.Split(":");
                Socket _Socket = Helper.Connect(ipport[0], int.Parse(ipport[1]));
                _Listener = new Listener { _Socket = _Socket };
                _Listener.StartAsync("BossClientListener");
                _Sender = new Sender { _Socket = _Socket };
                Send(PacketType.getip);
                while(true)
                    Update();
            }                       
            public enum Status{Connected,Disconnected,Connecting };
            public Status _Status = Status.Connecting;
            public int _id;
            public void Update()
            {
                foreach (byte[] msg in _Listener.GetMessages())
                    using (MemoryStream ms = new MemoryStream(msg))
                    {                        
                        int _playerid = ms.ReadB();                        
                        PacketType _pk = (PacketType)ms.ReadB();
                        Debug.Assert(_pk.IsValid());
                        if (_playerid == Config._ServerId)
                            switch (_pk)
                            {
                                case PacketType.ip:
                                    _Local._IpAddress = ms.Read().ToStr();                                    
                                    Send(PacketType.getrooms);
                                    break;
                                case PacketType.rooms:
                                    List<RoomDb> _rooms = (List<RoomDb>)_XmlSerializerRoom.Deserialize(ms);
                                    _Status = Status.Connected;
                                    int i = 0;
                                    while (!(_rooms[i++] is BossRoom)) ;
                                    Send(PacketType.joinroom, new byte[] { (byte)i });
                                    break;                         
                                case PacketType.playerid:
                                    _id = ms.ReadB();
                                    break;
                                case PacketType.PlayerJoined:
                                    ms.ReadB();
                                    break;
                                case PacketType.PlayerLeaved:
                                    ms.ReadB();
                                    break;
                                default:
                                    Debug.Fail("wrong packet");
                                    break;
                            }
                        else
                            switch (_pk)
                            {
                                case PacketType.sharedObject:
                                    Client _RemoteClient = new RemoteSharedObj<Client>();
                                    _RemoteClient._RemoteSharedObj.OnBytesToRead(ms);
                                    if (!_RemoteClient._Server)
                                    {
                                        using (MemoryStream ms2 = new MemoryStream())
                                        {
                                            ms2.WriteByte((byte)PacketType.sendTo);
                                            ms2.WriteByte((byte)_playerid);
                                            ms2.WriteByte((byte)PacketType.sharedObject);
                                            _Local._LocalSharedObj.Serialize(ms2);
                                            Send(PacketType.sendTo, ms2.ToArray(), _playerid);
                                        }
                                    }
                                    break;
                                default:
                                    Debug.Fail("wrong packet");
                                    break;
                            }
                        Trace.Assert(ms.Length == ms.Position, "packet too long");
                    }
                if (_Status == Status.Connected)
                {
                    byte[] bts = _Local._LocalSharedObj.GetChanges();
                    if (bts != null) Send(PacketType.sharedObject, bts);
                }
                Thread.Sleep(20);
            }
            public void Send(PacketType pk) { Send(pk, new byte[] { }); }
            public void Send(PacketType pk, byte[] data) { Send(pk, data, null); }
            public void Send(PacketType pk, byte[] data, int? to)
            {
                Debug.Assert(pk.IsValid());                
                if(to!=null)
                {
                    Debug.Assert(to < 255);
                    _Sender.Send(Helper.JoinBytes((byte)to, (byte)pk, data));
                }
                else
                    _Sender.Send(Helper.JoinBytes((byte)pk ,data));
            }
            public class Client : INotifyPropertyChanged, ISh<SharedObj<Client>>
            {

                private bool server;
                [SharedObject]
                public bool _Server { get { return server; } set { server = value; new PropertyChangedEventArgs("_Server"); } }                
                public SharedObj<Client> _SharedObj { get; set; }                
                public LocalSharedObj<Client> _LocalSharedObj { get { return (LocalSharedObj<Client>)_SharedObj; } }                
                public RemoteSharedObj<Client> _RemoteSharedObj { get { return (RemoteSharedObj<Client>)_SharedObj; } }
                private string name;
                [SharedObject]
                public string _Name { get { return name; } set { name = value; PropertyChanged(this, new PropertyChangedEventArgs("_Name")); } }
                private string ipAddress;
                [SharedObject]
                public string _IpAddress { get { return ipAddress; } set { ipAddress = value; PropertyChanged(this, new PropertyChangedEventArgs("_IpAddress")); } }

                public event PropertyChangedEventHandler PropertyChanged;
            }
        }
    }
}
