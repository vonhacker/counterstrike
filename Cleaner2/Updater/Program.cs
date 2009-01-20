using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using doru;
using System.Threading;
using System.Net;
namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        public Program()
        {
            Updater _Updater =new Updater();
            _Updater.Start();
        }
        public class Updater
        {
            DateTime _OldDateTime = DateTime.MinValue;
            public void Start()
            {
                while (true)
                {
                    TcpClient _TcpClient = new TcpClient("counterstrike.googlecode.com", 80);
                    Socket _Socket = _TcpClient.Client;
                    _Socket.Send(Res.downloadupdate);
                    NetworkStream _NetworkStream = new NetworkStream(_Socket);
                    string s = _NetworkStream.Cut("\r\n\r\n").ToStr();
                    _Socket.Close();
                    Match m = Regex.Match(s, @"Last-Modified\:(.+)");
                    int len = int.Parse(Regex.Match(s, @"Content-Length\: \d+").Groups[1].Value);
                    DateTime _DateTime = DateTime.Parse(m.Groups[1].Value);
                    
                    if (_DateTime != _OldDateTime)
                    {
                        WebClient _WebClient =new WebClient();
                        _WebClient.DownloadFile(
                    }
                    
                    Thread.Sleep(TimeSpan.FromDays(1));
                }
            }
        }
    }
}
