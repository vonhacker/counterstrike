using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using doru;
using System.Net;
using System.Web;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using CounterStrikeLive.Service;
using doru.Tcp;
namespace CounterStrikeLive.Server
{

    public class WebServer
    {
        TcpListener _TcpListener;
        public void StartAsync()
        {
            new Thread(Start).StartBackground("webserver");
        }
        public static Config _Config = Config._This;
        private void Start()
        {

            Console.WriteLine("Web SErver Started");
            _TcpListener = new TcpListener(IPAddress.Any, _Config._WebPort);
            try
            {
                _TcpListener.Start();
            } catch (SocketException) { _Config._WebPort.Trace("cannot listen port"); return; }
            while (true)
            {
                Socket _Socket = _TcpListener.AcceptSocket();
                Client _Client = new Client { _Socket = _Socket };
                _Client.StartAsync();
                Thread.Sleep(40);
            }
        }
        public class Client
        {
            public Socket _Socket;
            public void StartAsync()
            {
                new Thread(Start).StartBackground("webclient");
            }
            public void Start()
            {

                NetworkStream _NetworkStream = 
                    _Config.GenerateWebServerLag ? new LagStream(_Socket) : new NetworkStream(_Socket);
                string ip = (((IPEndPoint)_Socket.RemoteEndPoint).Address).ToString().Trace("webclient connected");
                try
                {
                    while (true)
                    {

                        string s = _NetworkStream.Cut("\r\n\r\n").ToStr();
                        Match m;
                        if ((m = Regex.Match(s, @"GET /(.*?)(\?.*)? HTTP", RegexOptions.IgnoreCase)).Success)
                        {
                            string s2 = HttpUtility.UrlDecode(m.Groups[1].Value).Trace("get");
                            string date = Regex.Match(s, "If-None-Match: (.+?)\r\n").Groups[1].Value;
                            
                            try
                            {
                                _NetworkStream.Write(OnReceive(s2, m.Groups[2].Value,date));
                            } catch (ExceptionA e)
                            {

                                _NetworkStream.Write(Http.Length(String.Format(Res._notfound, e.Message.Trace(), "404 Not Found")));
                            }
                        } else
                            _NetworkStream.Write(Http.Length(String.Format(Res._notfound, "error", "500 Error".Trace())));

                    }
                } catch (IOException) { }
                ip.Trace("webclient disc");
            }

            private string OnReceive(string path, string query,string _etag)
            {
                if (!Regex.IsMatch((_Socket.RemoteEndPoint as IPEndPoint).Address.ToString(), _Config._WebAllowedIps, RegexOptions.IgnoreCase))
                {
                    _Config._WebRedirect.Trace("redirect");
                    return string.Format(Res._redirect, _Config._WebRedirect);
                } else
                {

                    if (path.Length == 0) return string.Format(Res._redirect, _Config._WebDefaultPage + query);
                    path = Path.Combine(_Config._WebRoot, path);
                    if (!File.Exists(path)) throw new ExceptionA("File Not Exists");
                    if (Path.IsPathRooted(path)) throw new ExceptionA("Path Rooted");
                    if (!Path.GetFullPath(path).Contains(Environment.CurrentDirectory)) throw new ExceptionA("not allowed path");
                    
                    path.Trace("sending");
                    FileInfo _FileInfo = new FileInfo(path);                    
                    string etag = Helper.getMD5HashFromFile(path);
                    if (_etag != "" && etag == _etag)
                    {
                        this.Trace("not modificated");
                        return Res._notmodificied;
                    }
                    string content = File.ReadAllText(path, Encoding.Default);
                    if (Path.GetExtension(path) == ".html") content = content.Replace("_params_", query.TrimStart('?'));
                    string ret = Http.Length(string.Format(Res._reply, etag, _FileInfo.LastWriteTimeUtc+" GTM", DMime.GetMime(Path.GetExtension(path)), content));
                    //if(path.EndsWith("xap")) Debugger.Break();
                    return ret;
                }
            }
        }
    }
}
//public static class Ext
//{
//    public static T Trace<T>(this T t) { return Trace(t, ""); }
//    public static T Trace<T>(this T t, string s)
//    {
//        Console.WriteLine(s + t);
//        return t;
//    }
//}

