using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Specialized;
using Server.Properties;
using doru;
using GameServer;

namespace Server
{

    internal class Program
    {
        public Settings Settings { get { return Properties.Settings.Default; } }
        private static void Main()
        {
            
            Logging.Setup("../../../");
            //Spammer3.StartRemoteConsoleAsync
            new Program();
        }
        XmlSerializer _XmlSerializer = Helper.CreateSchema("cslive", typeof(GameServer.Database));
        public Program()
        {
            "started".Trace();
            GameServer.Database db = (GameServer.Database)_XmlSerializer.Deserialize(File.Open(Settings._Maps, FileMode.Open));
            List<GameServer.Server> svrs = new List<GameServer.Server>();
            foreach(Task task in db._tasks)
            {
                GameServer.Server _Server = new GameServer.Server();
                _Server._Task = task;
                _Server.StartAsync();
                svrs.Add(_Server);
            }

            PolicyServer ps = new PolicyServer { policyFile = "Server/PolicyFile.xml" };
            ps.StartAsync();            

            WebServer.WebServer _WebServer = new WebServer.WebServer();
            _WebServer.StartAsync();
            Thread.Sleep(-1);
        }
    }

}