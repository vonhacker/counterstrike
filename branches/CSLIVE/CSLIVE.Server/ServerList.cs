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
namespace CSLIVE.Server
{
    public partial class Program
    {
        public class ServerList
        {
            Listener _Listener;
            Sender _Sender;
            public void Start()
            {
                string[] ipport = _Database._ServerListIp.Split(":");
                Socket _Socket = Helper.Connect(ipport[0], int.Parse(ipport[1]));
                _Listener = new Listener { _Socket = _Socket };
                _Listener.StartAsync("serverlistlistener");
                _Sender = new Sender();
                _Sender.Send(new byte[] { (byte)PacketType.getmap, 0 });
                while (true)
                    Update();
            }
            BinaryFormatter _BinaryFormatter = new BinaryFormatter();
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
                                case PacketType.map:
                                    ServerListRoom _ServerListRoom = (ServerListRoom)_BinaryFormatter.Deserialize(ms);
                                    _Sender.Send(new byte[] { (byte)PacketType.roomid, 0 });
                                    break;

                            }
                    }
                Thread.Sleep(20);
            }
        }
    }
}
