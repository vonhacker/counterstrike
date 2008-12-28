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
        Uri _Uri = new Uri("http://igorlevochkin.ig.funpic.org/cs/serv.php");
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

            private void SendFile(string s, string path)
            {
                if (!Settings.Default._EnableWebServer)
                {
                    Trace.WriteLine("Redirect:" + path);
                    string s1 = @"HTTP/1.1 302 Found
Connection: close
Location: http://igorlevochkin.ig.funpic.org/game/

";
                    _TcpClient.Client.Send(s1);
                    _TcpClient.Client.Close();
                    return;
                }
                if (Path.GetFileName(path).Length == 0) path+= "default.html";
                if (!File.Exists(path)) throw new ExceptionA("File Not Exists");
                if (Path.IsPathRooted(path)) throw new ExceptionA("Path Rooted");
                if (!Path.GetFullPath(path).Contains(Environment.CurrentDirectory)) throw new ExceptionA("not allowed path");
                FileInfo _FileInfo = new FileInfo(path);
                string etag = _FileInfo.LastWriteTime.ToString();
                
                
                if (Settings.Default._EnableCache && Regex.IsMatch(s, @"If-None-Match: """ + etag + '"', RegexOptions.IgnoreCase)) //&& !Regex.IsMatch(Path.GetExtension(path), @"\.?(html?|xap|xml|txt|js)", RegexOptions.IgnoreCase)
                {
                    Trace.WriteLine("Cache"+path);
                    string header = @"HTTP/1.1 304 Not Modified

ETag: """ + etag + @"""
Content-Length: 0

";
                    _TcpClient.Client.Send(ASCIIEncoding.ASCII.GetBytes(header));

                }
                else
                {
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
    public class DMime
    {
        static Dictionary<String, string> _Mimes = new Dictionary<string, string>();
        static DMime()
        {
            foreach (Match _Match in Regex.Matches(mimetypes, @"([\w.*]+)\s+([\w/\-.]+)"))
            {
                string key = _Match.Groups[1].Value;
                string value = _Match.Groups[2].Value;
                _Mimes[key] = value;
            }
        }
        public static string GetMime(string _type)
        {
            if (_Mimes.ContainsKey(_type)) return _Mimes[_type];
            return _Mimes[".*"];
        }
        public const string mimetypes = @".* 	 application/octet-stream
.323 	 text/h323
.acx 	 application/internet-property-stream
.ai 	 application/postscript
.aif 	 audio/x-aiff
.aifc 	 audio/aiff
.aiff 	 audio/aiff
.application 	 application/x-ms-application
.asf 	 video/x-ms-asf
.asr 	 video/x-ms-asf
.asx 	 video/x-ms-asf
.au 	 audio/basic
.avi 	 video/x-msvideo
.axs 	 application/olescript
.bas 	 text/plain
.bcpio 	 application/x-bcpio
.bin 	 application/octet-stream
.bmp 	 image/bmp
.c 	 text/plain
.cat 	 application/vndms-pkiseccat
.cdf 	 application/x-cdf
.cer 	 application/x-x509-ca-cert
.clp 	 application/x-msclip
.cmx 	 image/x-cmx
.cod 	 image/cis-cod
.cpio 	 application/x-cpio
.crd 	 application/x-mscardfile
.crl 	 application/pkix-crl
.crt 	 application/x-x509-ca-cert
.csh 	 application/x-csh
.css 	 text/css
.dcr 	 application/x-director
.deploy 	 application/octet-stream
.der 	 application/x-x509-ca-cert
.dib 	 image/bmp
.dir 	 application/x-director
.disco 	 text/xml
.dll 	 application/x-msdownload
.doc 	 application/msword
.dot 	 application/msword
.dvi 	 application/x-dvi
.dxr 	 application/x-director
.eml 	 message/rfc822
.eps 	 application/postscript
.etx 	 text/x-setext
.evy 	 application/envoy
.exe 	 application/octet-stream
.fif 	 application/fractals
.flr 	 x-world/x-vrml
.gif 	 image/gif
.gtar 	 application/x-gtar
.gz 	 application/x-gzip
.h 	 text/plain
.hdf 	 application/x-hdf
.hlp 	 application/winhlp
.hqx 	 application/mac-binhex40
.hta 	 application/hta
.htc 	 text/x-component
.htm 	 text/html
.html 	 text/html
.htt 	 text/webviewhtml
.ico 	 image/x-icon
.ief 	 image/ief
.iii 	 application/x-iphone
.ins 	 application/x-internet-signup
.isp 	 application/x-internet-signup
.IVF 	 video/x-ivf
.jfif 	 image/pjpeg
.jpe 	 image/jpeg
.jpeg 	 image/jpeg
.jpg 	 image/jpeg
.js 	 application/x-javascript
.latex 	 application/x-latex
.lsf 	 video/x-la-asf
.lsx 	 video/x-la-asf
.m13 	 application/x-msmediaview
.m14 	 application/x-msmediaview
.m1v 	 video/mpeg
.m3u 	 audio/x-mpegurl
.man 	 application/x-troff-man
.manifest 	 application/x-ms-manifest
.mdb 	 application/x-msaccess
.me 	 application/x-troff-me
.mht 	 message/rfc822
.mhtml 	 message/rfc822
.mid 	 audio/mid
.mmf 	 application/x-smaf
.mny 	 application/x-msmoney
.mov 	 video/quicktime
.movie 	 video/x-sgi-movie
.mp2 	 video/mpeg
.mp3 	 audio/mpeg
.mpa 	 video/mpeg
.mpe 	 video/mpeg
.mpeg 	 video/mpeg
.mpg 	 video/mpeg
.mpp 	 application/vnd.ms-project
.mpv2 	 video/mpeg
.ms 	 application/x-troff-ms
.mvb 	 application/x-msmediaview
.nc 	 application/x-netcdf
.nws 	 message/rfc822
.oda 	 application/oda
.ods 	 application/oleobject
.p10 	 application/pkcs10
.p12 	 application/x-pkcs12
.p7b 	 application/x-pkcs7-certificates
.p7c 	 application/pkcs7-mime
.p7m 	 application/pkcs7-mime
.p7r 	 application/x-pkcs7-certreqresp
.p7s 	 application/pkcs7-signature
.pbm 	 image/x-portable-bitmap
.pdf 	 application/pdf
.pfx 	 application/x-pkcs12
.pgm 	 image/x-portable-graymap
.pko 	 application/vndms-pkipko
.pma 	 application/x-perfmon
.pmc 	 application/x-perfmon
.pml 	 application/x-perfmon
.pmr 	 application/x-perfmon
.pmw 	 application/x-perfmon
.png 	 image/png
.pnm 	 image/x-portable-anymap
.pnz 	 image/png
.pot 	 application/vnd.ms-powerpoint
.ppm 	 image/x-portable-pixmap
.pps 	 application/vnd.ms-powerpoint
.ppt 	 application/vnd.ms-powerpoint
.prf 	 application/pics-rules
.ps 	 application/postscript
.pub 	 application/x-mspublisher
.qt 	 video/quicktime
.ra 	 audio/x-pn-realaudio
.ram 	 audio/x-pn-realaudio
.ras 	 image/x-cmu-raster
.rgb 	 image/x-rgb
.rmi 	 audio/mid
.roff 	 application/x-troff
.rtf 	 application/rtf
.rtx 	 text/richtext
.scd 	 application/x-msschedule
.sct 	 text/scriptlet
.setpay 	 application/set-payment-initiation
.setreg 	 application/set-registration-initiation
.sh 	 application/x-sh
.shar 	 application/x-shar
.sit 	 application/x-stuffit
.smd 	 audio/x-smd
.smx 	 audio/x-smd
.smz 	 audio/x-smd
.snd 	 audio/basic
.spc 	 application/x-pkcs7-certificates
.spl 	 application/futuresplash
.src 	 application/x-wais-source
.sst 	 application/vndms-pkicertstore
.stl 	 application/vndms-pkistl
.stm 	 text/html
.sv4cpio 	 application/x-sv4cpio
.sv4crc 	 application/x-sv4crc
.t 	 application/x-troff
.tar 	 application/x-tar
.tcl 	 application/x-tcl
.tex 	 application/x-tex
.texi 	 application/x-texinfo
.texinfo 	 application/x-texinfo
.tgz 	 application/x-compressed
.tif 	 image/tiff
.tiff 	 image/tiff
.tr 	 application/x-troff
.trm 	 application/x-msterminal
.tsv 	 text/tab-separated-values
.txt 	 text/plain
.uls 	 text/iuls
.ustar 	 application/x-ustar
.vcf 	 text/x-vcard
.wav 	 audio/wav
.wbmp 	 image/vnd.wap.wbmp
.wcm 	 application/vnd.ms-works
.wdb 	 application/vnd.ms-works
.wks 	 application/vnd.ms-works
.wmf 	 application/x-msmetafile
.wps 	 application/vnd.ms-works
.wri 	 application/x-mswrite
.wrl 	 x-world/x-vrml
.wrz 	 x-world/x-vrml
.wsdl 	 text/xml
.xaf 	 x-world/x-vrml
.xbm 	 image/x-xbitmap
.xla 	 application/vnd.ms-excel
.xlc 	 application/vnd.ms-excel
.xlm 	 application/vnd.ms-excel
.xls 	 application/vnd.ms-excel
.xlt 	 application/vnd.ms-excel
.xlw 	 application/vnd.ms-excel
.xml 	 text/xml
.xof 	 x-world/x-vrml
.xpm 	 image/x-xpixmap
.xsd 	 text/xml
.xsl 	 text/xml
.xwd 	 image/x-xwindowdump
.z 	 application/x-compress
.zip 	 application/x-zip-compressed";

    }
}
