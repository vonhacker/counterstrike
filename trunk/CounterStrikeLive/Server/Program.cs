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

namespace Server
{
    internal class Program
    {
        private static void Main()
        {
            Spammer3.Setup("../../../");
            new Program();
        }
        public Program()
        {
            PolicyServer.PolicyServer ps = new PolicyServer.PolicyServer { policyFile = "Server/PolicyFile.xml" };

            ps.StartAsync();

            GameServer.Server _Server = new GameServer.Server();
            _Server.StartAsync();

            WebServer.WebServer _WebServer = new WebServer.WebServer();
            _WebServer.StartAsync();

            while (true)
            {
                string s = Console.ReadLine();
                _Server._Console.Add(s);
            }
        }
    }

}