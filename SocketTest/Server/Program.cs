using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        public Program()
        {
            Logging._AllowDuplicates = true;
            Logging.Setup();
            new PolicyServer().StartAsync();
            TcpListener _TcpListener = new TcpListener(IPAddress.Any,4530);
            _TcpListener.Start();
            while (true)
                try
                {
                    Socket _Socket = _TcpListener.AcceptSocket();
                    new Client(_Socket);
                }
                catch (SocketException) { }
        }        
        
        
    }
}
