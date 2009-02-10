using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Xml.Serialization;
using System.IO;
using doru.Tcp;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace CSLIVE.Server
{
    public partial class Program
    {
        static void Main(string[] args)
        {            
            Logging.Setup("../../../");
            new Program();
        }                
        XmlSerializer _XmlSerializer = Helper.CreateSchema("cslive", typeof(Database),typeof(ServerListRoom),typeof(WormsRoom),typeof(CSRoom));
        public static Database _Database;
        public class Database
        {
            public List<Room> Rooms = new List<Room>() { new ServerListRoom() };            
            public string _ServerName = "CounterStrikeLive Server Test";
            public string _ServerListIp = "85.157.182.183:4530";
            public string _WebAllowedIps = ".*";
            public string _WebRedirect = "http://cslive.mindswitch.ru/cs/CounterStrikeLiveTestPage.html";
            public int _WebPort = 5300;
            public string _WebRoot = @"./";
            public string _WebDefaultPage = @"./CSLIVE.Web/CSLIVETestPage.html";
            public int _GamePort = 4530;
        }
        
        public Program()
        {
            _Database = _XmlSerializer.DeserealizeOrCreate("config.xml", new Database());
            GameServer _GameServer = new GameServer();
            _GameServer.StartAsync();
            new PolicyServer().StartAsync();

            WebServer _WebServer = new WebServer();
            _WebServer.Start();
            Thread.Sleep(-1);
        }
        

    }
    
}
