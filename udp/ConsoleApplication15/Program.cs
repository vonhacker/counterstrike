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
    }
    public class Program : bs
    {
        
        private static void Main(string[] arrgs)
        {
            new Program();
        }
        IPEndPoint epf = null;
        IPEndPoint ept = null;
        UdpClient f ;
        UdpClient t ;
        int w = 30;
        public Program()
        {
            asd();
        }

        private void asd()
        {
            while (true)
                try
                {
                    f = new UdpClient(5301);
                    t = new UdpClient();
                    t.Connect(IPAddress.Loopback, 5300);
                    new Thread(Start).Start();
                    while (true)
                    {
                        var bts = f.Receive(ref ept);
                        t.Send(bts, bts.Length);
                        Thread.Sleep(w);
                    }
                }
                catch (Exception e) { Debug.WriteLine(e.Message); Thread.Sleep(1000); }
        }

        private void Start()
        {
            try
            {
                while (true)
                {

                    var bts = t.Receive(ref epf);
                    if (!f.Client.Connected)
                        f.Connect(ept);
                    f.Send(bts, bts.Length);
                    Thread.Sleep(w);
                }
            }
            catch { Thread.Sleep(1000); }
        }
    }

}