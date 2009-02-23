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
    class Client
    {
        public Client(Socket _Socket)
        {

            new Thread(delegate()
            {
                while (true)
                {
                    byte[] bts = new byte[] { 1 };
                    _sendedPacketcount++;
                    _sendedtotalbytes += bts.Length;
                    _Socket.Send(bts);
                    Logging._Title = ("sendedtotalbytes:" + _sendedtotalbytes + " packetCount" + _sendedPacketcount);
                    Thread.Sleep(2);
                }

            }).Start();
            while (true)
            {
                byte[] bytes = _Socket.Receive();
                _receivedtotalBytes += bytes.Length;
                _receivedpacketscount++;
                Console.Write("\r receivedtotalbytes:" + _receivedtotalBytes + " packetCount" + _receivedpacketscount);
                Thread.Sleep(2);
            }
        }
        int _sendedPacketcount;
        int _sendedtotalbytes;
        int _receivedpacketscount = 0;
        int _receivedtotalBytes = 0;
    }
}
