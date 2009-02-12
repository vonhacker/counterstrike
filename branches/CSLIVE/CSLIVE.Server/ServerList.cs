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
            public void StartAsync()
            {
                new Thread(Start).StartBackground("ServerListClient");
            }
            public class Client : INotifyPropertyChanged
            {
                public LocalSharedObj<Client> _LocalSharedObj;
                private string name;
                [SharedObject(0)]
                public string _Name { get { return name; } set { name = value;  } }
                private string ipAddress;
                [SharedObject(0)]
                public string _IpAddress { get { return ipAddress; } set { ipAddress = value; } }
            }
            public void Start()
            {
                _Local._Name = _Config._ServerName;
                string[] ipport = _Config._ServerListIp.Split(":");
                Socket _Socket = Helper.Connect(ipport[0], int.Parse(ipport[1]));
                _Listener = new Listener { _Socket = _Socket };
                _Listener.StartAsync("ServerListClientListener");
                _Sender = new Sender();
                _Sender.Send(new[] { (byte)PacketType.getip });
                while (true)
                    Update();
            }
            public void OnBytesToSend(byte [] bytes)
            {
                _Sender.Send(Helper.JoinBytes((byte)PacketType.sharedObject, bytes));
            }
            BinaryFormatter _BinaryFormatter = new BinaryFormatter();
            Client _Local = new Client();
            public void Update()
            {
                foreach (byte[] msg in _Listener.GetMessages())
                    using (MemoryStream ms = new MemoryStream(msg))
                    {
                        bool skipped;
                        int _playerid = ms.ReadB();
                        PacketType _pk = (PacketType)ms.ReadB();
                        if (_playerid == (int)PacketType.serverid)
                            switch (_pk)
                            {
                                case PacketType.ip:
                                    _Local._IpAddress = ms.ReadString();
                                    _Sender.Send(new[] { (byte)PacketType.getrooms });
                                    break;
                                case PacketType.room:
                                    
                                    _Local._LocalSharedObj = new LocalSharedObj<Client>
                                    {
                                        _SharedObject = _Local,
                                        _OnBytesToSend = OnBytesToSend
                                    };                                    
                                    break;                                
                                case PacketType.PlayerJoined:
                                    using(MemoryStream ms2 = new MemoryStream())
                                    {
                                        ms2.WriteByte((byte)PacketType.sendTo);
                                        ms2.WriteByte((byte)_playerid);
                                        ms2.WriteByte((byte)PacketType.sharedObject);
                                        _Local._LocalSharedObj.WriteAllBytes(ms2);
                                        _Sender.Send(ms2.ToArray());
                                    }
                                    break;                  
                                default:
                                    _pk.Trace("skipped");
                                    skipped = true;
                                    break;
                            }

                    }
                Thread.Sleep(20);
            }
        }
    }
}
