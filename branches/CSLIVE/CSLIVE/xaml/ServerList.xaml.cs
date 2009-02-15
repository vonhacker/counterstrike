using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using doru;
using System.Net.Sockets;
using doru.Tcp;
using System.IO;
using System.Collections.ObjectModel;

namespace CSLIVE
{
    public partial class ServerList : UserControl, IUpdate
    {
        public ServerList()
        {
            _Local._Server = false;
            _Local._Name = _LocalDatabase._Nick;
            _Local._IpAddress = "unkown";
            InitializeComponent();
        
        }

        public void Start()
        {
            Helper.Connect(_Config._BossServerIp).Completed +=
                delegate(object o, SocketAsyncEventArgs e2) { Dispatcher.BeginInvoke(new Action<SocketAsyncEventArgs>(Connected), e2); };
        }
        Listener _Listener;
        Sender _Sender;
        public void Connected(SocketAsyncEventArgs e2) // connected to boss server
        {
            Debug.Assert(e2.SocketError == SocketError.Success);
            _Listener = new Listener { _Socket = (Socket)e2.UserToken };
            _Listener.Start();
            _Sender = new Sender { _Socket = (Socket)e2.UserToken };
            _Sender.Send(PacketType.getrooms);
        }
        ObservableCollection<BossClient> _BossClients = new ObservableCollection<BossClient>();
        public void Update()
        {
            if (_Listener != null)
            {
                foreach (byte[] bts in _Listener.GetMessages())
                {
                    MemoryStream _MemoryStream = new MemoryStream(bts);
                    int _playerid = _MemoryStream.ReadB();
                    PacketType pk = (PacketType)_MemoryStream.ReadB();
                    Trace.Assert(pk.IsValid());
                    if (_playerid == Common._ServerId)
                        switch (pk)
                        {
                            case PacketType.rooms:
                                {
                                    List<RoomDb> _RoomDb = (List<RoomDb>)Common._XmlSerializerRoom.Deserialize(_MemoryStream);
                                    BossRoom bs = (BossRoom)_RoomDb.First(room => room is BossRoom);
                                    _Sender.Send(PacketType.joinroom, new byte[] { (byte)_RoomDb.IndexOf(bs) });
                                }
                                break;
                            case PacketType.JoinRoomSuccess:
                                _Status = Status.Connected;
                                _id = _MemoryStream.ReadB();
                                break;
                            case PacketType.PlayerJoined: //adding new client, sending shared object
                                {
                                    BossClient bs = new RemoteSharedObj<BossClient>();
                                    bs._Id = _MemoryStream.ReadB();
                                    _BossClients.Add(bs);
                                    //_Sender.Send(PacketType.sharedObject, _Local._LocalSharedObj.Serialize(), bs._Id);
                                }
                                break;
                            case PacketType.PlayerLeaved: //remocing client
                                _BossClients.Remove(GetClient(_MemoryStream.ReadB()));
                                break;
                            default:
                                Trace.Fail("wrong packet");
                                break;
                        }
                    else
                    {
                        BossClient _Client = GetClient(_id);
                        Trace.Assert(_Client != null);
                        switch (pk)
                        {
                            case PacketType.sharedObject:
                                _Client._RemoteSharedObj.OnBytesToRead(_MemoryStream); //updating client`s sharedobject
                                break;                            
                            default:
                                Trace.Fail("wrong packet");
                                break;
                        }
                    }
                    Trace.Assert(_MemoryStream.Length == _MemoryStream.Position);
                }
                if (_Status == Status.Connected)
                {
                    byte[] bts2 = _Local._LocalSharedObj.GetChanges();
                    if (bts2 != null) _Sender.Send(PacketType.sharedObject, bts2);
                }
            }
        }

        public BossClient GetClient(int id)
        {
            return _BossClients.FirstOrDefault(a => a._Id == _id); 
        }
        public int _id;
        public BossClient _Local = new LocalSharedObj<BossClient>();
        Status _Status = Status.Disconnected;
        public enum Status { Connected, Disconnected }
    }
}
