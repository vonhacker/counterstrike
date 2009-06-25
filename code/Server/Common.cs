using System.Linq;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using doru.Tcp;
using doru;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Collections;

namespace CounterStrikeLive.Service
{
    /// <summary>
    /// Protocol Headers    
    /// </summary>
    public enum PacketType : byte
    {
        ServerIsFull = 16,
        Reloading = 15,
        voteMap=14,
        Join = 13,
        MapSelected = 12,
        SelectMap = 11,        
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
        /// CounterStrikeLive.CounterStrikeLive.GameServer->client player Ping [[ping - int16]]
        /// </summary>
        pinginfo = 100,
        /// <summary>
        /// client->client add point to player
        /// </summary>
        addPoint = 66,
        /// <summary>
        /// client->client extra damage shoot (head shoot) [byte - player _dir]
        /// </summary>
        firstshoot = 45,
        /// <summary>
        /// client->client shoot [byte - player _dir]
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
        /// server->client map [byte len][string]
        /// </summary>
        map = 21,
        /// <summary>
        /// client->client [byte client id][data]
        /// </summary>
        sendTo = 27,
    }
    public class Config 
    {
        public bool _AutoSelect { get { return this.Get<bool>("AutoSelect", true) && Debugger.IsAttached; } set { this.Set("AutoSelect", value); } }
        public bool GenerateClientLag = false;
        public int _MaxPlayers = 10;
        public bool GenerateServerLag = false;
        public bool GenerateWebServerLag = false;
        public static Config _This;
        public int _WebPort = 5300;        
        public int? _MaxLatency = null;
        public string _ContentFolder = "Content/";
        public int _GamePort = 4530;
        public string _WebAllowedIps = ".*";
        public string _WebRedirect = "http://dorumon.no-ip.org";
        public string _WebRoot = @"./";     
        public string _WebDefaultPage = @"./CounterStrikeLiveWeb/CounterStrikeLiveTestPage.html";
        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Config));
        public Config()
        {
            //if (_This != null) throw new Exception("Config can have only one Copy");
            _This = this;
            Helper._ContentFolder = _ContentFolder;
            
        }
        public MapInfo[] _Maps = new MapInfo[]
            {
                new MapInfo{ MapName = "estate.zip"},
                new MapInfo{ MapName = "italy.zip"},
                new MapInfo{ MapName = "nuke.zip"}
            };
    }
    [XmlRoot]
    public class FolderList 
    {
        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(FolderList));
        public FolderList()
        {
            
        }
        [XmlAttribute]
        public string FileName = "Content";
        [XmlIgnore]
        public string path;
        [XmlIgnore]
        public Stream _Stream;
        [XmlIgnore]
        public BitmapImage _BitmapImage;
        
        public static Dictionary<string, FolderList> keys = new Dictionary<string, FolderList>();
        public List<FolderList> fls= new List<FolderList>();
        public bool _isfile;
        public static FolderList _This;
        public void Load()
        {
            Trace.Assert(!loaded);
            loaded = true;
            _This = this;
            Load("");
        }
        bool loaded;
        private void Load(string root)
        {            
            path = root;
            keys.Add(FileName, this);
            if (_isfile)
            {
                _Stream = Application.GetResourceStream(new Uri(path + FileName, UriKind.Relative)).Stream;
                if (FileName.EndsWith(".png"))
                {
                    _BitmapImage = new BitmapImage();
                    _BitmapImage.SetSource(_Stream);
                }
            }
            foreach (FolderList fl in fls)            
                fl.Load(path + FileName + "/");                            
        }
		public static FolderList Find(string s)
		{
			return _This.find(s);
		}
        public FolderList find(string s)
        {
            Trace.Assert(loaded);
            return keys[s];
        }
        
        //public FolderList GetFolder(string s)
        //{
        //    if (FileName == s) return this;
        //    foreach (FolderList fl in fls)
        //    {                
        //        if (fl.FileName.StartsWith(s))
        //            return fl.GetFolder(s.Substring(fl.FileName.Length) + 1);
        //    }
        //    throw new Exception("folder not exists");

        //}
        public override string ToString()
        {
            return FileName+ " ("+ fls.Count+")";
        }

        


    }
    
    public class MapInfo
    {
        public string MapName{get;set;}
        public string MapDescription{get;set;}
        public string Icon = "default.png";
        [XmlIgnore]
        public ImageSource ImageSource { get { return new BitmapImage(new Uri(Icon,UriKind.Relative)); } }
    }
}
