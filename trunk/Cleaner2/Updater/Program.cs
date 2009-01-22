using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using doru;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Updater
{
    class Program
    {
        static FastZip _Zip;
        static void Main(string[] args)
        {            
            Spammer3.Setup("../../csliveServer");
            _Zip = new FastZip();
            
            new Program();
        }

        public Program()
        {            
            Updater _Updater =new Updater();
            _Updater.Start();
        }
        public static Properties.Settings Settings { get { return Properties.Settings.Default; } }
        private static void Kill()
        {            
            
            Process _Process = Spammer3.FindProcess(Path.GetFileNameWithoutExtension(Settings._processPath)).FirstOrDefault();
            if (_Process != null) _Process.Kill();
        }
        
        public class Updater
        {
            Uri _Uri = new Uri(Settings._webpath);
            DateTime _OldDateTime = DateTime.MinValue;
            public void Start()
            {                
                while (true)
                {
                    try
                    {
                        string s;
                        using (TcpClient _TcpClient = new TcpClient(_Uri.Host, 80))
                        {
                            Socket _Socket = _TcpClient.Client;

                            _Socket.Send(String.Format(Res.downloadupdate, _Uri.AbsolutePath, _Uri.Host));
                            NetworkStream _NetworkStream = new NetworkStream(_Socket);
                            s = _NetworkStream.Cut("\r\n\r\n").ToStr().Trace();
                        }
                        Match m = Regex.Match(s, @"(?:Last-Modified)|(?:Date)\:(.+)").Trace();
                        if (!m.Success) throw new ExceptionC("web file not found");
                        int len = int.Parse(Regex.Match(s, @"Content-Length\: (\d+)").Groups[1].Value);
                        DateTime _DateTime = DateTime.Parse(m.Groups[1].Value);
                        if (_DateTime != _OldDateTime)
                        {
                            Trace.WriteLine("downloading " + len / 1000 + " KBytes");
                            if (File.Exists("cslive.zip")) File.Delete("cslive.zip");
                            new WebClient().DownloadFile(_Uri.ToString(), "cslive.zip"); ;
                            Kill();
                            _Zip.ExtractZip("cslive.zip", "./", "");
                            Thread.Sleep(1000);
                            _Process = Helper.StartProcess(Settings._processPath);
                            _OldDateTime = _DateTime;
                            Trace.WriteLine("updated");
                        }

                        Thread.Sleep(Settings._interval);

                    }
                    catch (IOException e) { e.Trace(); }
                    catch (SocketException e) { e.Trace(); }
                    

                    Debugger.Break();
                    Thread.Sleep(10000);
                }
                
            }
            Process _Process;
        }        
    }
}
