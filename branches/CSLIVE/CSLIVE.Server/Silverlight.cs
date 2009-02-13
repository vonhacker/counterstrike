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
using System.Diagnostics;

namespace CSLIVE //this file contains code for silverlight and server
{
    public class Config //"./CSLIVE.Web/ClientBin/Config.xml"
    {
        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Config), new[] { typeof(ServerListRoom), typeof(WormsRoom), typeof(CSRoom) });
        public static string _ConfigPath = "./CSLIVE.Web/ClientBin/Config.xml";
        public List<RoomDb> Rooms = new List<RoomDb>() { new ServerListRoom(), new CSRoom() { MapPath = "estate.zip" } };
        public string _ServerName = "CounterStrikeLive Server Test";
        public string _ServerListIp = "localhost:4530";
        public string _WebAllowedIps = ".*";
        public string _WebRedirect = "http://cslive.mindswitch.ru/cs/CounterStrikeLiveTestPage.html";        
        public int _WebPort = 5300;
        public string _WebRoot = @"./";        
        public string _WebDefaultPage = @"./CSLIVE.Web/CSLIVETestPage.html";
        public int _GamePort = 4530;
        public string _Irc = "85.202.112.192:4534";
        public string _IrcRoom = "#cslive";
    }
    
    #region SharedObject
    public interface ISh<T>
    {
        T _SharedObj { get; set; }
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
    public abstract class SharedObj<T> where T : class, INotifyPropertyChanged, ISh<SharedObj<T>>
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
            if(_Properties.Count == 0) throw new Exception("sharedobject does not have attributes");
            _Object._SharedObj = this;
        }
    }
    public class LocalSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh<SharedObj<T>>
    {
        public MemoryStream _ms = new MemoryStream();
        public LocalSharedObj() :base()
        {
            _Object.PropertyChanged += new PropertyChangedEventHandler(Object_PropertyChanged);
        }

        public void Serialize(MemoryStream ms)
        {
            for(int i = 0; i < _Properties.Count; i++)
                if(_Properties[i].GetCustomAttributes(true).Any(a => a is SharedObjectAttribute))
                    WriteBytes(i, _ms);
        }
        public byte[] GetChanges()
        {
            if(_ms.Length == 0) return null;
            using(MemoryStream ms2 = _ms)
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
            int i = _Properties.IndexOf(_Object.GetType().GetProperty(e.PropertyName));
            if(i == -1) Debugger.Break();
            WriteBytes(i, _ms);
        }
    }
    public class RemoteSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh<SharedObj<T>>
    {
        public RemoteSharedObj() : base() { }
        public void OnBytesToRead(MemoryStream ms)
        {
            while(ms.Position != ms.Length)
            {
                int id = ms.ReadByte();
                PropertyInfo _PropertyInfo = _Properties[id];
                if(_PropertyInfo.GetCustomAttributes(true).FirstOrDefault(a => a is SharedObjectAttribute) == null)
                    throw new Exception("Break");
                Type type = _PropertyInfo.PropertyType;
                if(type.IsAssignableFrom(typeof(int)))
                    _PropertyInfo.SetValue(_Object, ms.ReadInt16(), null);
                else if(type.IsAssignableFrom(typeof(string)))
                    _PropertyInfo.SetValue(_Object, ms.ReadString(), null);
                else if(type.BaseType == typeof(Enum))
                    _PropertyInfo.SetValue(_Object, Enum.Parse(type, ms.ReadString(), false), null);
                else if(type.IsAssignableFrom(typeof(float)))
                    _PropertyInfo.SetValue(_Object, ms.ReadSingle(), null);
                else if(type.IsAssignableFrom(typeof(bool)))
                    _PropertyInfo.SetValue(_Object, ms.ReadBoolean(), null);
                else throw new Exception("Break");
            }
        }

    }
    #endregion
    #region Room
    public abstract class RoomDb //base class for room static info
    {        
#if(!SILVERLIGHT)        
        [XmlIgnore]
        public CSLIVE.Server.Program.GameServer.Client[] _Clients = new CSLIVE.Server.Program.GameServer.Client[255];
#endif
    }    
    public class CSRoom : RoomDb //cslive room
    {

        public string MapPath;        
    }    
    public class ServerListRoom : RoomDb { }
    
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
        /// client->server first message from client - join room, after that server send player id [byte] 
        /// </summary>
        joinroom = 39                
    }
    
    
}
