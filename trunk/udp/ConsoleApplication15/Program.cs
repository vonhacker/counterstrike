using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace ConsoleApplication15
{
    public class bs
    {
        public static List<Client> clients = new List<Client>();
    }
    public class Programm : bs
    {
        
        private static void Main(string[] arrgs)
        {
            new Programm();
        }
        public Programm()
        {
            var ca = new Client() { listenPort = 5301, sendPort = 5300 }.Init().Start();
            var cl = clients;
            while (true)
            {
                
                Thread.Sleep(200);
            }
            
        }
    }
    public class Client:bs
    {
        public Thread t;
        public UdpClient to;
        public UdpClient from;
        public int listenPort;
        public int sendPort;
        public long total = 0;
        public Client Init()
        {
            from = new UdpClient(listenPort);
            to = new UdpClient();            
            to.Connect(new IPEndPoint(IPAddress.Loopback, sendPort));
            return this;
        }
        public Client Start()
        {
            clients.Add(this);
            t = new Thread(StartThread);
            t.IsBackground = true;
            t.Start();
            return this;
        }

        void StartThread()
        {
            Debug.WriteLine("started");
            IPEndPoint ep=null;
            while (true)
            {
                var bts = from.Receive(ref ep);
                if (listenPort != 0)
                {
                    from.Connect(IPAddress.Loopback, listenPort);
                    new Client { from = to, to = from }.Start();
                }
                total += bts.Length;
                to.Send(bts, bts.Length);
                Thread.Sleep(2);
            }
        }
        public override string ToString()
        {
            return "listen port" + listenPort + " sendport" + sendPort;
        }
    }

}