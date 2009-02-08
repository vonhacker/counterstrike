using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using System.Collections.Specialized;
using Server;
using Server.Properties;
using doru;

namespace WebServer
{

    class WebServer
    {
        public int _WebPort { get { return Settings.Default._WebPort; } }

        public void StartAsync()
        {
            Thread _Thread = new Thread(Start);
            _Thread.Name = "WebServer";
            _Thread.Start();
        }
        int i = 0;
        WebClient _WebClient = new WebClient();
        TcpListener _TcpListener;
        public void Start()
        {
            Console.WriteLine("Web SErver Started");
            _TcpListener = new TcpListener(IPAddress.Any, _WebPort);
            _TcpListener.Start();

            while (true)
            {
                Update();
            }
        }
        Uri _Uri = new Uri("http://cslive.mindswitch.ru/cs/serv.php");
        private void Update()
        {

            TcpClient _TcpClient = _TcpListener.AcceptTcpClient();
            Trace.WriteLine("web client COnnected");
            Client _Client = new Client { _TcpClient = _TcpClient, _Name = "WebClient" + i };
            _Client.StartAsync();
            Thread.Sleep(40);
            i++;
        }

        class Client
        {
            public string _Name;
            public TcpClient _TcpClient;
            public void StartAsync()
            {
                Thread _Thread = new Thread(Start);
                _Thread.Name = _Name;
                _Thread.Start();
            }
            public void Start()
            {
                using (_TcpClient)
                    try
                    {
                        while (true)
                        {
                            byte[] _buffer = new byte[99999];
                            int count = _TcpClient.Client.Receive(_buffer);
                            if (count == 0) break;
                            string s = ASCIIEncoding.ASCII.GetString(_buffer, 0, count);
                            Match _Match = Regex.Match(s, @"GET /([\w/.%&|-]*)", RegexOptions.IgnoreCase);
                            string path = _Match.Groups[1].Value;
                            path = HttpUtility.UrlDecode(path);

                            if (_Match.Success)
                                try
                                {
                                    SendFile(s, path);
                                }
                                catch (EncoderFallbackException e) 
                                {
                                    Trace.WriteLine("File Not Found:" + path + " error:" + e);
                                    NotFound("not found");
                                }
                            else
                            {
                                Trace.WriteLine("Unknown connection");
                                NotFound("error");
                            }
                        }
                    }
                    catch (ExceptionA) { }
                    catch (ObjectDisposedException) { }
                    catch (SocketException)
                    {
                        //Trace.WriteLine("Socket Erorr " + e); 
                    }
            }
            public static Settings Settings { get { return Server.Properties.Settings.Default; } }
            private void SendFile(string s, string path)
            {

                if (!Regex.IsMatch((_TcpClient.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), Settings._allowedIps, RegexOptions.IgnoreCase))
                {
                    Trace.WriteLine("Redirect:" + path);
                    string s1 = @"HTTP/1.1 302 Found
Connection: close
Location: http://cslive.mindswitch.ru/cs/CounterStrikeLiveTestPage.html

";
                    _TcpClient.Client.Send(s1);
                    _TcpClient.Client.Close();
                    return;
                }
                if (Path.GetFileName(path).Length == 0) path += "CounterStrikeLiveTestPage.html";
                if (!File.Exists(path)) throw new ExceptionA("File Not Exists");
                if (Path.IsPathRooted(path)) throw new ExceptionA("Path Rooted");
                if (!Path.GetFullPath(path).Contains(Environment.CurrentDirectory)) throw new ExceptionA("not allowed path");
                FileInfo _FileInfo = new FileInfo(path);
                string etag = _FileInfo.LastWriteTime.ToString();

                Trace.WriteLine("Sending" + path);
                byte[] _bytes = File.ReadAllBytes(path);
                string header = @"HTTP/1.1 200 OK
Connection: Keep-Alive
Content-Length: " + _bytes.Length + @"
Accept-Ranges: bytes
ETag: """ + etag + @"""
Date:" + new FileInfo(path).LastWriteTimeUtc + @"
Content-Type: " + DMime.GetMime(Path.GetExtension(path)) + @"

";
                using (MemoryStream _MemoryStream = new MemoryStream())
                {
                    BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                    _BinaryWriter.Write(ASCIIEncoding.ASCII.GetBytes(header));
                    _BinaryWriter.Write(_bytes);
                    _TcpClient.Client.Send(_MemoryStream.ToArray());
                }


            }

            private void NotFound(string msg)
            {
                string header = @"HTTP/1.1 404 Not Found
Connection: Keep-Alive
Content-Type: text/plain


" + msg;
                _TcpClient.Client.Send(ASCIIEncoding.ASCII.GetBytes(header));
                //Trace.WriteLine("File not found");
            }
        }


        public static string ReadString(NetworkStream _NetworkStream)
        {

            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                try
                {
                    while (true)
                    {
                        int _byte = _NetworkStream.ReadByte();
                        if (_byte == -1) break;
                        _MemoryStream.WriteByte((byte)_byte);
                        if (_byte == 10)
                        {
                            string s = ASCIIEncoding.ASCII.GetString(_MemoryStream.ToArray());
                            //Trace.WriteLine("<<<<<<<<string received>>>>>>>" + s.Length);
                            return s;
                        }
                        Thread.Sleep(10);
                    }
                }
                catch (IOException)
                {
                    //Trace.WriteLine(e.Message);
                }
            }
            return null;
        }


    }
    
}
