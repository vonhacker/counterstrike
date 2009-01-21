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

namespace Updater
{
    class Program
    {
        static Chilkat.Zip _Zip;
        static void Main(string[] args)
        {
            
            Spammer3.Setup("../../csliveServer");            

            _Zip = new Chilkat.Zip();
            if (!_Zip.UnlockComponent("30-day trial")) throw new Exception("chilcat");
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
                        TcpClient _TcpClient = new TcpClient(_Uri.Host, 80);
                        Socket _Socket = _TcpClient.Client;
                        _Socket.Send(Res.downloadupdate);
                        NetworkStream _NetworkStream = new NetworkStream(_Socket);
                        string s = _NetworkStream.Cut("\r\n\r\n").ToStr();
                        _Socket.Close();
                        Match m = Regex.Match(s, @"Last-Modified\:(.+)");
                        int len = int.Parse(Regex.Match(s, @"Content-Length\: (\d+)").Groups[1].Value);
                        if (len == 0) throw new Exception("File Length is zero");
                        DateTime _DateTime = DateTime.Parse(m.Groups[1].Value);
                        if (_DateTime != _OldDateTime)
                        {
                            Trace.WriteLine("downloading "+ len /1000 +" KBytes");
                            if (File.Exists("cslive.zip")) File.Delete("cslive.zip");
                            new WebClient().DownloadFile(_Uri.ToString(), "cslive.zip"); ;
                            _Zip.OpenZip("cslive.zip");
                            Kill();
                            Thread.Sleep(1000);
                            _Zip.ExtractNewer("./");
                            _Process = Helper.StartProcess(Settings._processPath);
                            _OldDateTime = _DateTime;
                            Trace.WriteLine("updated");
                        }

                        Thread.Sleep(TimeSpan.FromHours(5));
                    }
                    catch (IOException e) { e.Trace(); }
                    catch (SocketException e) { e.Trace(); }
                    Thread.Sleep(10000);
                }
                
            }
            Process _Process;
        }        
    }
}
