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
        
        public static Config _Config;


        public Program()
        {
            _Config = Config._XmlSerializer.DeserealizeOrCreate(Config._ConfigPath, new Config());
            GameServer _GameServer = new GameServer();
            _GameServer.StartAsync();
            new PolicyServer().StartAsync();

            WebServer _WebServer = new WebServer();
            _WebServer.Start();
            Thread.Sleep(-1);

            ServerList _ServerList = new ServerList();

        }


    }

}
