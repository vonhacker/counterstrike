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
        public class BossClientApp
        {
            Listener _Listener;
            Sender _Sender;
            
            BossClient _Local = new LocalSharedObj<BossClient>();
            public void StartAsync()
            {
                new Thread(Start).StartBackground("BossClient");
            }
            private void Start()
            {
                "Boss Client started".Trace();
                BossClient _Client = new LocalSharedObj<BossClient>();

                _Local._Name = _Config._ServerName;
                _Local._Server = true;
                string[] ipport = _Config._BossServerIp.Split(":");
                Socket _Socket = Helper.Connect(ipport[0], int.Parse(ipport[1]));
                _Listener = new Listener { _Socket = _Socket };
                _Listener.StartAsync("BossClientListener");
                _Sender = new Sender { _Socket = _Socket };
                _Sender.Send(PacketType.getip);
                while(true)
                    Update();
            }                       
            public enum Status{Connected,Disconnected,Connecting };
            public Status _Status = Status.Connecting;
            public int _id;
            public void Update()
            {
                foreach (byte[] msg in _Listener.GetMessages())
                    using (MemoryStream _MemoryStream = new MemoryStream(msg))
                    {                        
                        int _playerid = _MemoryStream.ReadB();                        
                        PacketType _pk = (PacketType)_MemoryStream.ReadB();
                        Debug.Assert(_pk.IsValid());
                        if (_playerid == Common._ServerId)
                            switch (_pk)
                            {
                                case PacketType.ip:
                                    _Local._IpAddress = _MemoryStream.Read().ToStr();                                    
                                    _Sender.Send(PacketType.getrooms);
                                    break;
                                case PacketType.rooms:
                                    List<RoomDb> _rooms = (List<RoomDb>)Common._XmlSerializerRoom.Deserialize(_MemoryStream);
                                    _Status = Status.Connected;
                                    int i = -1;
                                    while (!(_rooms[(i++) + 1] is BossRoom)) ;
                                    _Sender.Send(PacketType.joinroom, new byte[] { (byte)i });
                                    break;
                                case PacketType.JoinRoomSuccess:
                                    _id = _MemoryStream.ReadB();
                                    break;
                                case PacketType.PlayerJoined: //sending shared object
                                    {
                                        int playerid = _MemoryStream.ReadB();
                                        _Sender.Send(PacketType.sharedObject, _Local._LocalSharedObj.Serialize(), playerid);
                                    }
                                    break;
                                case PacketType.PlayerLeaved:
                                    { int playerid = _MemoryStream.ReadB(); }
                                    break;
                                default:
                                    Debug.Fail("wrong packet");
                                    break;
                            }
                        else
                            switch (_pk)
                            {
                                case PacketType.sharedObject: //doing nothing
                                    BossClient _RemoteClient = new RemoteSharedObj<BossClient>();
                                    _RemoteClient._RemoteSharedObj.OnBytesToRead(_MemoryStream);                                                                                                                                                           
                                    break;                                
                                default:
                                    Debug.Fail("wrong packet");
                                    break;
                            }
                        Trace.Assert(_MemoryStream.Length == _MemoryStream.Position, "packet too long");
                    }
                if (_Status == Status.Connected)
                {
                    byte[] bts = _Local._LocalSharedObj.GetChanges();
                    if (bts != null) _Sender.Send(PacketType.sharedObject, bts);
                }
                Thread.Sleep(20);
            }

            
            
        }
    }
}
