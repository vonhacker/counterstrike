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
namespace CSLIVE.Server
{
    public partial class Program
    {
        public class ServerList
        {
            Listener _Listener;
            Sender _Sender;
            
            Client _Local = new LocalSharedObj<Client>();
            public void StartAsync()
            {
                new Thread(Start).StartBackground("ServerListClient");
            }
            private void Start()
            {
                "boss sender started".Trace();
                Client _Client = new LocalSharedObj<Client>();

                _Local._Name = _Config._ServerName;                
                string[] ipport = _Config._ServerListIp.Split(":");
                Socket _Socket = Helper.Connect(ipport[0], int.Parse(ipport[1]));
                _Listener = new Listener { _Socket = _Socket };
                _Listener.StartAsync("ServerListClientListener");
                _Sender = new Sender { _Socket = _Socket };
                _Sender.Send(new[] { (byte)PacketType.getip });
                while(true)
                    Update();
            }                                                
            public void Update()
            {
                foreach (byte[] msg in _Listener.GetMessages())
                    using (MemoryStream ms = new MemoryStream(msg))
                    {                        
                        int _playerid = ms.ReadB();
                        PacketType _pk = (PacketType)ms.ReadB();
                        if (_playerid == (int)PacketType.serverid)
                            switch (_pk)
                            {
                                case PacketType.ip:
                                    _Local._IpAddress = ms.ReadString();
                                    _Sender.Send(new[] { (byte)PacketType.getrooms });
                                    break;                                
                                case PacketType.rooms:
                                    List<RoomDb> _rooms = (List<RoomDb>)_XmlSerializerRoom.Deserialize(ms);
                                    int i = 0;
                                    while(!(_rooms[i++] is ServerListRoom)) ;                                                                        
                                    _Sender.Send(new byte[] { (byte)PacketType.roomid, (byte)i });
                                    break;                                
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
                                            _Sender.Send(ms2.ToArray());
                                        }
                                    }
                                    break;                  
                                default:
                                    _pk.Trace("skipped");                                    
                                    break;
                            }

                    }
                byte[] bts = _Local._LocalSharedObj.GetChanges();
                if (bts != null) _Sender.Send(Helper.JoinBytes((byte)PacketType.sharedObject, bts));
                Thread.Sleep(20);
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
