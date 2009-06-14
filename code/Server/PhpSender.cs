using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CounterStrikeLive.Server.Properties;
using System.Reflection;
using CounterStrikeLive.Service;
using System.Diagnostics;
using doru;
using System.Net.Sockets;
using System.Threading;

namespace CounterStrikeLive.Server
{
    public class PhpSender
    {
        Settings _Settings { get { return Settings.Default; } }
        Server _Server = Server._This;
        Config _Config = Config._This;
        Uri _Uri;
        public void StartAsync()
        {
            _Uri = new Uri(_Settings._ServerList);   
            new Thread(SendHttp).Start();
        }
        void SendHttp(object _object)
        {
            while (true)
            {
                try
                {
                    string post = @"POST {6} HTTP/1.1
Host: {5}
Content-Type: application/x-www-form-urlencoded
Content-Length: _length_

name={0}&map={1}&version={2}&port={3}&webport={7}&players={4}";
                    post = String.Format(post, _Server._ServerName, _Server._Map, Assembly.GetExecutingAssembly().GetName().Version,
                        _Server._Port.ToString(), _Server.clientcount.ToString(), _Uri.Host, _Uri.PathAndQuery, _Config._WebPort);
                    int len = post.IndexOf("\r\n\r\n") + 4;
                    if (len == 0) Debugger.Break();
                    Http.Length(ref post);
                    TcpClient _TcpClient = new TcpClient(_Uri.Host, 80);
                    Socket _Socket = _TcpClient.Client;
                    _TcpClient.Client.Send(post);
                    string s = _Socket.Receive().ToStr(); ;
                    //Http.ReadHttp(_Socket.Client).Save();
                    Thread.Sleep(200);
                    _TcpClient.Close();
                } catch (SocketException) { }
                Thread.Sleep(1000 * 60 * 60);
            }
        }
    }
}
