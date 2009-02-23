using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.Net;

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
            Socket _Socket = _TcpListener.AcceptSocket();
            
            while (true)
            {
                byte[] bytes = _Socket.Receive();
                _totalBytes += bytes.Length;
                _packetscount++;
                Console.Write("\r totalbytes:" + _totalBytes + " packetCount" + _packetscount);
            }
        }
        int _packetscount=0;
        int _totalBytes = 0;
    }
}
