
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Forms;
using System.IO.Compression;
using System.Windows.Controls;
using System.ComponentModel;
using System.Web;
using ICSharpCode.SharpZipLib.GZip;

namespace doru
{
	public class PolicyServer
	{
		class PolicyConnection
		{
			private Socket m_connection;
			private byte[] m_buffer;
			private int m_received;
			private byte[] m_policy;
			private static string s_policyRequestString = "<policy-file-request/>";
			public PolicyConnection(Socket client, byte[] policy)
			{
				m_connection = client;
				m_policy = policy;
				m_buffer = new byte[s_policyRequestString.Length];
				m_received = 0;
				try
				{
					m_connection.BeginReceive(m_buffer, 0, s_policyRequestString.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
				} catch (SocketException)
				{
					m_connection.Close();
				}
			}
			private void OnReceive(IAsyncResult res)
			{
				try
				{
					m_received += m_connection.EndReceive(res);
					if (m_received < s_policyRequestString.Length)
					{
						m_connection.BeginReceive(m_buffer, m_received, s_policyRequestString.Length - m_received, SocketFlags.None, new AsyncCallback(OnReceive), null);
						return;
					}
					string request = System.Text.Encoding.UTF8.GetString(m_buffer, 0, m_received);
					if (StringComparer.InvariantCultureIgnoreCase.Compare(request, s_policyRequestString) != 0)
					{
						m_connection.Close();
						return;
					}
					m_connection.BeginSend(m_policy, 0, m_policy.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
				} catch (SocketException)
				{
					m_connection.Close();
				}
			}
			public void OnSend(IAsyncResult res)
			{
				try
				{
					m_connection.EndSend(res);
				} finally
				{
					m_connection.Close();
				}
			}
		}
		private Socket m_listener;
		private byte[] m_policy;
		public string policyFile;
        public int _PolicyPort = 943;
		public void StartAsync()
		{
			Console.WriteLine("PolicyServer Started");

			if (policyFile == null)
			{
				m_policy = @"<?xml version=""1.0"" encoding =""utf-8""?>
<access-policy>
  <cross-domain-access>
    <policy>
      <allow-from>
        <domain uri=""*"" />
      </allow-from>
      <grant-to>
        <socket-resource port=""4530"" protocol=""tcp"" />
        <socket-resource port=""4531"" protocol=""tcp"" />
        <socket-resource port=""4532"" protocol=""tcp"" />
        <socket-resource port=""4533"" protocol=""tcp"" />
        <socket-resource port=""4534"" protocol=""tcp"" />                
      </grant-to>
    </policy>
  </cross-domain-access>
</access-policy>".ToBytes();
			} else m_policy = File.ReadAllBytes(policyFile);
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_listener.Bind(new IPEndPoint(IPAddress.Any, _PolicyPort));
			m_listener.Listen(10);
			m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
		}

		public void OnConnection(IAsyncResult res)
		{
			Socket client = null;
			try
			{
				client = m_listener.EndAccept(res);
			} catch (SocketException)
			{
				return;
			}
			Trace.WriteLine("Client Connected " + ((IPEndPoint)client.RemoteEndPoint).Address);
			PolicyConnection pc = new PolicyConnection(client, m_policy);
			m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
		}
		public void Close()
		{
			m_listener.Close();
		}
	}
	    public class TcpRedirect
    {
        public string _LocalhostReplace;
        public delegate Socket GetSocket();
        public event GetSocket _RemoteIpDelegate;
        public int _LocalPort;
        Thread _Thread;
        public void StartAsync()
        {
            _Thread = new Thread(Start);
            _Thread.IsBackground = true;
            _Thread.Start();
        }
        const string hosts = @"C:\WINDOWS\system32\drivers\etc\hosts";
        TcpListener _TcpListener;

        public void Start()
        {
            if (Directory.Exists("logs")) Directory.Delete("logs", true);
            if (_LocalhostReplace != null) File.WriteAllText(hosts, "127.0.0.1 " + _LocalhostReplace);
            Thread.Sleep(500);
            _TcpListener = new TcpListener(IPAddress.Any, _LocalPort);
            _TcpListener.Start();
            int i = 0;
            try
            {
                while (true)
                {
                    i++;
                    Socket _LocalSocket = _TcpListener.AcceptSocket();
                    File.WriteAllText(hosts, "");
                    Socket _RemoteSocket = _RemoteIpDelegate();
                    if (_LocalhostReplace != null) File.WriteAllText(hosts, "127.0.0.1 " + _LocalhostReplace);
                    Trace.Write("<<<<<Client Connected>>>>>>>>" + i + "\n");
                    Client _LocalListen = new Client() { _ListenSocket = _LocalSocket, _SendToSocket = _RemoteSocket, _name = i + "Sended" };
                    _LocalListen.StartAsync();
                    Client _RemoteListen = new Client() { _ListenSocket = _RemoteSocket, _SendToSocket = _LocalSocket, _name = i + "Received" };
                    _RemoteListen.StartAsync();
                    Thread.Sleep(2);
                }
            } catch (Exception e)
            {
                Trace.WriteLine("TpcRedirect: " + e.Message);
            }
        }
        public void Stop()
        {
            _TcpListener.Stop();
            File.WriteAllText(hosts, "");
            Thread.Sleep(500);
        }
        //[DebuggerStepThrough]
        public class Client
        {
            public Socket _ListenSocket;
            public Socket _SendToSocket;
            public string _name;
            public void StartAsync()
            {
                Thread _Thread = new Thread(Start);
                _Thread.IsBackground = true;
                _Thread.Start();
            }
            void Start()
            {
                Directory.CreateDirectory("logs");
                FileStream _FileStream = new FileStream("logs/" + _name + ".txt", FileMode.Create, FileAccess.Write);

                Byte[] data = new byte[99999];
                try
                {
                    while (true)
                    {
                        int count = _ListenSocket.Receive(data);
                        if (count == 0) break;
                        Trace.WriteLine(_name + "<<<<<<<<<<<<<<<<<<<<" + count + " Bytes>>>>>>>>>>>>>>>>>");
                        Trace.WriteLine(Encoding.Default.GetString(data, 0, count));
                        _FileStream.Write(data, 0, count);
                        _SendToSocket.Send(data, count, SocketFlags.None);
                    }
                } catch (SocketException e) { Trace.WriteLine(e.Message); }
                Trace.WriteLine(_name + " <<<<<<<<<<<<<<Disconnected>>>>>>>>>>>>");
                _ListenSocket.Close();
                _SendToSocket.Close();
                _FileStream.Close();
            }
        }
    }

    //[DebuggerStepThrough]
    public static class Proxy
    {
        public static Socket Socks5Connect(string _proxyAddress, int _proxyPort, string _DestAddress, int _DestPort)
        {
            TcpClient _TcpClient = new TcpClient(_proxyAddress, _proxyPort);
            Socket _Socket = _TcpClient.Client;

            _Socket.Send(new byte[] { 5, 1, 0 });
            byte[] _bytes = _Socket.Receive(2);
            Trace.WriteLine("<<<<<<<socks5 received1>>>>>>>>");
            if (_bytes[0] != 5 && _bytes[1] != 1) throw new ExceptionA();
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                _BinaryWriter.Write(new byte[] {
                    5, // version
                    1, // tcp stream
                    0, // reserved
                    3 //type - domainname 
                });
                _BinaryWriter.Write(_DestAddress);
                _BinaryWriter.Write(BitConverter.GetBytes((UInt16)_DestPort).ReverseA());
                _Socket.Send(_MemoryStream.ToArray());
            }
            Trace.WriteLine("<<<<<<<<socks5 received2>>>>>>>>");
            byte[] _response = _Socket.Receive(10);
            if (_response[1] != 0) throw new ExceptionA("socket Error: " + _response[1]);
            return _Socket;
        }
    }
    //[DebuggerStepThrough]
    public static class Http
    {
        public static string Boundary()
        {
            return "--"+H.Randomstr(8);
        }
        public static string Length(string _bytes)
        {
            H.Replace(ref _bytes, "_length_", (_bytes.Length - 4 - _bytes.IndexOf("\r\n\r\n")).ToString(), 1);
            return _bytes;
        }
        public static void Length(ref string _bytes)
        {
            H.Replace(ref _bytes, "_length_", (_bytes.Length - 4 - _bytes.IndexOf("\r\n\r\n")).ToString(), 1);
        }
        public static void Length(ref byte[] _bytes)
        {
            H.Replace(ref _bytes, "_length_".ToBytes(), (_bytes.Length - 4 - _bytes.IndexOf2("\r\n\r\n")).ToString().ToBytes(), 1);
        }
        public static byte[] HttpLength(this byte[] _bytes)
        {
            H.Replace(ref _bytes, "_length_".ToBytes(), (_bytes.Length - 4 - _bytes.IndexOf2("\r\n\r\n")).ToString().ToBytes(), 1);
            return _bytes;
        }

      
        public static byte[] ReadHttp(Socket _Socket)
        {
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            _NetworkStream.ReadTimeout = 10000;
            return ReadHttp(_NetworkStream);
        }

        public static Action<double> Progress;
        
        public static byte[] ReadHttp(Stream _Stream)
        {
            byte[] _headerbytes;
            _headerbytes = _Stream.Cut("\r\n\r\n");
            byte[] _Content = null;
            string _header = Encoding.Default.GetString(_headerbytes);
            Match _Match = Regex.Match(_header, @"Content-Length: (\d+)");
            if (_Match.Success)
            {
                int length = int.Parse(_Match.Groups[1].Value);
                if (length == 0) return _headerbytes;
                _Content = _Stream.Read(length);
            } else if (Regex.IsMatch(_header, "Transfer-Encoding: chunked"))
            {
                _Content = ReadChunk(_Stream);
            } else //if (Regex.IsMatch(_header, @"Proxy-Connection\: close|Connection\: close",RegexOptions.IgnoreCase))
            {
                _Content = DownloadHttp(_Stream);
            }
            //else throw new ExceptionA("Header Error:"+_header);

            if (Regex.IsMatch(_header, "Content-Encoding: gzip"))
            {
                _Content = Unpack(_Content);
            }
            return _headerbytes.Join(_Content);

        }


        public static void RedirectCheck(ref string str,string host)
        {
            using (TcpClient _TcpClient = new TcpClient(host, 80))
            {
                Socket _Socket = _TcpClient.Client;
                NetworkStream _NetworkStream = new NetworkStream(_Socket);


                Match _Match = Regex.Match(str, @"Location: (.*)", RegexOptions.IgnoreCase);
                if (_Match.Success)
                {
                    byte[] _bytes2 = File.ReadAllBytes("1 Sended getlist.html");
                    Helper.Replace(ref _bytes2, "_id_".ToBytes(), ("/" + _Match.Groups[1].Value.Trim()).ToBytes());
                    _Socket.Send(_bytes2);
                    Trace.WriteLine("Redirect");
                    str = Http.ReadHttp(_NetworkStream).Save().ToStr();
                }
            }
        }

        private static byte[] DownloadHttp(Stream _Stream)
        {
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                while (true)
                {
                    int i = _Stream.ReadByte();
                    if (i == -1) return _MemoryStream.ToArray();
                    _MemoryStream.WriteByte((byte)i);
                }
            }
        }
        private static byte[] Unpack(byte[] _bytes)
        {

            GZipInputStream _GZipStream = new GZipInputStream(new MemoryStream(_bytes));
            byte[] _buffer2 = new byte[99999];
            int count = _GZipStream.Read(_buffer2, 0, _buffer2.Length);
            return _buffer2.Substr(count);
        }

        static readonly byte[] _rn = new byte[] { 13, 10 };
        public static byte[] ReadChunk(Stream _Stream)
        {

            MemoryStream _MemoryStream = new MemoryStream();
            while (true)
            {
                byte[] _bytes = _Stream.Cut("\r\n");
                int length = int.Parse(Encoding.Default.GetString(_bytes), System.Globalization.NumberStyles.HexNumber);
                if (length == 0) break;
                _MemoryStream.Write(_Stream.Read(length), 0, length);
                if (!H.Compare(_Stream.Read(2), _rn)) throw new ExceptionA("ReadChunk: cant find Chunk");
            }
            return _MemoryStream.ToArray();
        }


    }
    //[DebuggerStepThrough]
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
