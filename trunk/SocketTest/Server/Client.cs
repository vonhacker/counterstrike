using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace Server
{
    class Client
    {
        Socket _Socket;
        public Client(Socket _Socket)
        {
            this._Socket = _Socket;
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(5);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
            new Thread(delegate()
                {
                    while (true)
                    {
                        
                        byte[] bytes = _Socket.Receive();
                        _receivedtotalBytes += bytes.Length;
                        _receivedpacketscount++;
                        Console.Write("\r receivedtotalbytes:" + _receivedtotalBytes + " packetCount" + _receivedpacketscount);
                        Thread.Sleep(2);
                    }
                }).Start();
            
            System.Windows.Forms.Application.Run();
        }

        void dt_Tick(object sender, EventArgs e)
        {
            Update();
        }
        
        public void Update()
        {
            byte[] bts = new byte[] { 1 };
            _sendedPacketcount++;
            _sendedtotalbytes += bts.Length;
            _Socket.Send(bts);            
            Logging._Title = ("sendedtotalbytes:" + _sendedtotalbytes + " packetCount" + _sendedPacketcount);
            Thread.Sleep(2);
        }
        int _sendedPacketcount;
        int _sendedtotalbytes;
        int _receivedpacketscount = 0;
        int _receivedtotalBytes = 0;
    }
}
