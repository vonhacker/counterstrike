using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using doru;
using System.IO;
using System.Net.Sockets;
using doru.Tcp;

using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace CSLIVE.Menu
{

    public class ServerListGame : BossClient, IUpdate
    {
        

        public ServerListGame()
            : base()
        {
            
        }
        public Dispatcher Dispatcher;
        public void Load()
        {
            
            _RemoteSharedObj._OnSerialized += new Action(Connect);
        }
        void Connect()
        {
            if (_Server)            
                Helper.Connect(_IpAddress, Dispatcher, Connected);                            
        }        
        
        public new LocalSharedObj<ServerListGame> _LocalSharedObj { get { return (LocalSharedObj<ServerListGame>)_SharedObj; } }
        public new RemoteSharedObj<ServerListGame> _RemoteSharedObj { get { return (RemoteSharedObj<ServerListGame>)_SharedObj; } }
        public void Connected(SocketAsyncEventArgs e)
        {            
            Trace.Assert(e.SocketError == SocketError.Success);
            Socket _Socket = (Socket)e.UserToken;
            NetworkStream nw = new NetworkStream(_Socket);
            _Listener = new Listener { _NetworkStream = nw ,_Socket = _Socket };
            _Listener.StartAsync("bossClientListerer");
            _Sender = new Sender() { _NetworkStream = nw ,_Socket = _Socket};

            new DispatcherTimer().StartRepeatMethod(10, SendGetRooms); SendGetRooms();
            new DispatcherTimer().StartRepeatMethod(1, Ping);            
        }
        Listener _Listener;
        Sender _Sender;
        DateTime _PingDate;

        public void SendGetRooms()
        {
            _Sender.Send(PacketType.getrooms);
        }
        
        private void OnRoomsReceived(MemoryStream _MemoryStream)
        {
            List<RoomInfo> _rooms = (List<RoomInfo>)Common._XmlSerializerRoom.Deserialize(_MemoryStream);
           // Helper.MergeList(_RoomList, _rooms);
            _RoomList.Clear();
            foreach (RoomInfo room in _rooms)
                _RoomList.Add(room);
        }
        public void Pong()
        {
            TimeSpan ts = DateTime.Now - _PingDate;
            _Ping = (int)ts.TotalMilliseconds;
        }
        private ObservableCollection<RoomInfo> roomList = new ObservableCollection<RoomInfo>();
        public ObservableCollection<RoomInfo> _RoomList
        {
            get { return roomList; }
            set
            {
                roomList = value;                
            }
        }
        //public new event PropertyChangedEventHandler PropertyChanged;
        public void Ping()
        {            
            _PingDate = DateTime.Now;
            _Sender.Send(PacketType.ping);
        }

        
        public void Update()
        {
            if (_Listener != null)            
                foreach (byte[] msg in _Listener.GetMessages())
                {
                    MemoryStream _MemoryStream = new MemoryStream(msg);
                    byte idFrom = _MemoryStream.ReadB();
                    PacketType pk = (PacketType)_MemoryStream.ReadB();
                    if (idFrom == Common._ServerId)
                        switch (pk)
                        {
                            case PacketType.pong:
                                Pong();
                                break;
                            case PacketType.rooms:
                                OnRoomsReceived(_MemoryStream);
                                break;
                            default:                            
                                Trace.Fail("wrong packet");
                                break;
                        } else Trace.Fail("wrong packet");
                }
            
        }

        
    }
    
}

