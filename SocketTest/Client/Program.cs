using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        public Program()
        {
            
            Socket _Socket = new TcpClient("localhost", Helper._DefaultSilverlightPort).Client;
            new Server.Client(_Socket);
        }

        
    }
}
