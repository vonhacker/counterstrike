using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using doru.Tcp;
#if(!SILVERLIGHT)
using System.Diagnostics;
using System.Collections.ObjectModel;
#else

#endif
namespace CSLIVE //this file contains code for silverlight and server
{
    public class Config //"./CSLIVE.Web/ClientBin/Config.xml"
    {
        public List<RoomDb> Rooms = new List<RoomDb>();
        public string _ServerName = "CounterStrikeLive Server Test";
        public string _BossServerIp = "localhost:4530";
        public string _WebAllowedIps = ".*";
        public string _WebRedirect = "http://cslive.mindswitch.ru/cs/CounterStrikeLiveTestPage.html";        
        public int _WebPort = 5300;
        public string _WebRoot = @"./";        
        public string _WebDefaultPage = @"./CSLIVE.Web/CSLIVETestPage.html";
        public int _GamePort = 4530;
        public string _Irc = "85.202.112.192:4534";
        public string _IrcRoom = "#cs-general";
    }
    public static class Common
    {
        public const byte _ServerId = 254;
        public static XmlSerializer _XmlSerializerConfig = new XmlSerializer(typeof(Config), new[] { typeof(BossRoom), typeof(WormsRoom), typeof(CSRoom) });
        public static string _ConfigPath = "./CSLIVE.Web/ClientBin/Config.xml";
        public static XmlSerializer _XmlSerializerRoom = new XmlSerializer(typeof(List<RoomDb>), new Type[] { typeof(BossRoom), typeof(CSRoom), typeof(WormsRoom) });
    }

    public static class Extensions
    {
        public static void Send(this Sender sender, PacketType pk) { sender.Send(pk, new byte[] { }); }
        public static void Send(this Sender sender, PacketType pk, byte[] data) { sender.Send(pk, data, null); }
        public static void Send(this Sender sender, PacketType pk, byte[] data, int? to)
        {
            Debug.Assert(pk.IsValid());
            if (to != null)
            {
                sender.Send(Helper.JoinBytes(PacketType.sendTo, (byte)to, (byte)pk, data));
            }
            else
                sender.Send(Helper.JoinBytes((byte)pk, data));
        }
    }

    #region SharedObjectProviderClasses
    public interface ISh
    {
        object _SharedObj { get; set; }
    }
    public class SharedObjectAttribute : Attribute
    {
        public int _Priority;
        public SharedObjectAttribute() { }
        public SharedObjectAttribute(int _Priority)
        {
            this._Priority = _Priority;
        }
    }
    public abstract class SharedObj<T> where T : class, INotifyPropertyChanged, ISh
    {
        public T _Object = Activator.CreateInstance<T>();
        protected List<PropertyInfo> _Properties;
        public static implicit operator T(SharedObj<T> s)
        {
            return s._Object;
        }
        public SharedObj()
        {
            _Properties = (from p in _Object.GetType().GetProperties()
                           from a in p.GetCustomAttributes(true)
                           where a is SharedObjectAttribute
                           orderby (a as SharedObjectAttribute)._Priority
                           select p).ToList();
            if (_Properties.Count == 0) throw new Exception("sharedobject does not have attributes");
            _Object._SharedObj = this;
        }
    }
    public class LocalSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh
    {
        public MemoryStream _ms = new MemoryStream();
        public LocalSharedObj()
            : base()
        {
            _Object.PropertyChanged += new PropertyChangedEventHandler(Object_PropertyChanged);
        }
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serialize(ms);
                return ms.ToArray();
            }
        }
        public void Serialize(MemoryStream ms)
        {
            for (int i = 0; i < _Properties.Count; i++)
                if (_Properties[i].GetCustomAttributes(true).Any(a => a is SharedObjectAttribute))
                    WriteBytes(i, ms);
        }
        public byte[] GetChanges()
        {
            if (_ms.Length == 0) return null;
            using (MemoryStream ms2 = _ms)
            {
                _ms = new MemoryStream();
                return ms2.ToArray();
            }
        }
        private void WriteBytes(int i, MemoryStream _ms)
        {
            PropertyInfo _PropertyInfo = _Properties[i];
            object value = _PropertyInfo.GetValue(_Object, null);

            _ms.WriteByte((byte)i);
            if (value is int) _ms.Write((Int16)(int)value);
            else if (value is string) _ms.WriteString((string)value);
            else if (value is Enum) _ms.Write(value.ToString());
            else if (value is float) _ms.Write((float)value);
            else if (value is byte) _ms.Write((byte)value);
            else if (value is bool) _ms.Write((bool)value);
            else throw new Exception("Shared Send Unkown value");

        }

        void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int i = _Properties.IndexOf(_Object.GetType().GetProperty(e.PropertyName));
            Trace.Assert(i != -1);
            WriteBytes(i, _ms);
        }
    }
    public class RemoteSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh
    {
        public bool _Serialized = false;
        public RemoteSharedObj() : base() { }
        public event Action _OnSerialized;
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
                    _PropertyInfo.SetValue(_Object, ms.ReadFloat(), null);
                else if (type.IsAssignableFrom(typeof(bool)))
                    _PropertyInfo.SetValue(_Object, ms.ReadBoolean(), null);
                else throw new Exception("Break");
            }
            if (_Serialized == false)
            {
                if (_OnSerialized != null) _OnSerialized();
                _Serialized = true;
            }
        }

    }
    #endregion
    #region Rooms
    public abstract class RoomDb //base class for room static info
    {
        public string MapName { get; set; }
        public string _Type { get { return this.GetType().ToString(); } }
#if(!CLIENT)        
        public int _PlayerCount { get { return _Clients.Where(a => a != null).Count(); } set { } }
        [XmlIgnore]
        public CSLIVE.Server.Program.GameServer.Client[] _Clients = new CSLIVE.Server.Program.GameServer.Client[255];
#else   
        public int _PlayerCount { get; set; }
#endif
    }    
    public class CSRoom : RoomDb //cslive room
    {        
        public string MapPath { get; set; }        
    }    
    public class BossRoom : RoomDb { }
    
    public class WormsRoom : RoomDb { } //future worms game room
    #endregion        
    
    public enum PacketType : byte
    {
        /// <summary>
        /// client->server
        /// </summary>
        getip = 122,
        /// <summary>
        /// server->client
        /// </summary>
        ip = 123,
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
        getrooms = 40,
        /// <summary>
        /// server->client List<room> serialized data
        /// </summary>
        rooms = 21,
        /// <summary>
        /// client->client [byte client id][data]
        /// </summary>
        sendTo = 27,
        /// <summary>
        /// server->client sets player id [byte]
        /// </summary>
        JoinRoomSuccess = 39,
        /// <summary>
        /// client->server first message from client - join room        
        /// </summary>
        joinroom = 89,

    }

    public class BossClient : INotifyPropertyChanged, ISh
    {        
        private bool server;
        [SharedObject]
        public bool _Server { get { return server; } set { server = value; new PropertyChangedEventArgs("_Server"); } }
        public object _SharedObj { get; set; }
        public LocalSharedObj<BossClient> _LocalSharedObj { get { return (LocalSharedObj<BossClient>)_SharedObj; } }
        public RemoteSharedObj<BossClient> _RemoteSharedObj
        {
            get
            {
                try
                {
                    return (RemoteSharedObj<BossClient>)_SharedObj;
                } catch { System.Diagnostics.Debugger.Break(); return null; }
            }
        }
        private string name;
        [SharedObject]
        public string _Name { get { return name; } set { name = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_Name")); } }
        private string ipAddress;
        [SharedObject]
        public string _IpAddress { get { return ipAddress; } set { ipAddress = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_IpAddress")); } }        
        public event PropertyChangedEventHandler PropertyChanged;
        public int _Id;
        int ping;
        public int _Ping { get { return ping; } set { ping = value; PropertyChanged(this, new PropertyChangedEventArgs("_Ping")); } }
    }
}
