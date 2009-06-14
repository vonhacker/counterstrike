using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using doru.Tcp;
using doru;

namespace CounterStrikeLive.Service
{
    /// <summary>
    /// Protocol Headers    
    /// </summary>
    public enum PacketType : byte
    {        
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
        
        public bool GenerateClientLag = false;        
        public bool GenerateServerLag = false;
        public bool GenerateWebServerLag = false;
        public static Config _This;
        public int _WebPort = 5300;
        public int _MaxLatency = 2000;
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
            
        }
        public List<MapInfo> _Maps = new List<MapInfo>();
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
