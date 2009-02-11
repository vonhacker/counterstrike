using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace CSLIVE
{
    public class Config 
    {
        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Config), new[] { typeof(ServerListRoom), typeof(WormsRoom), typeof(CSRoom) });
        public static string _ConfigPath = "./CSLIVE.Web/ClientBin/Config.xml";
        public List<Room> Rooms = new List<Room>() { new ServerListRoom() };
        public string _ServerName = "CounterStrikeLive Server Test";
        public string _ServerListIp = "85.157.182.183:4530";
        public string _WebAllowedIps = ".*";
        public string _WebRedirect = "http://cslive.mindswitch.ru/cs/CounterStrikeLiveTestPage.html";        
        public int _WebPort = 5300;
        public string _WebRoot = @"./";        
        public string _WebDefaultPage = @"./CSLIVE.Web/CSLIVETestPage.html";
        public int _GamePort = 4530;
        public string _Irc = "85.202.112.192:4534";
        public string _IrcRoom = "#cslive";        
    }
    
    public abstract class Room
    {        
#if(!SILVERLIGHT)        
        [XmlIgnore]
        public CSLIVE.Server.Program.GameServer.Client[] _Clients = new CSLIVE.Server.Program.GameServer.Client[255];
#endif
    }
    
    public class CSRoom : Room //cslive room
    {
        public string mapname;        
    }
    
    public class ServerListRoom : Room { }
    
    public class WormsRoom : Room { } //future worms game room
    public enum PacketType : byte
    {
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
        getmap = 40,
        /// <summary>
        /// server->client map [BinaryFormater Room._data]
        /// </summary>
        map = 21,
        /// <summary>
        /// client->client [byte client id][data]
        /// </summary>
        sendTo = 27,
        /// <summary>
        /// client->server first message from client - join room, after that server send player id [byte] 
        /// </summary>
        roomid = 39
    }
}
