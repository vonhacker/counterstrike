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
            Socket _Socket = Helper.Connect("localhost", Helper._DefaultSilverlightPort);
            while (true)
            {
                byte[] bts = new byte[]{1,2,3,4,5,6};
                _Socket.Send(bts);
                _btscount += bts.Length;
                _pkcount++;
                Console.Write("\rbtscount" + _btscount + " packet count" + _pkcount);
                Thread.Sleep(2);
            }
        }
        public int _btscount;
        public int _pkcount;
    }
}
