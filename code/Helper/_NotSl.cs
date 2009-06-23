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
using System.Windows.Media;

namespace doru
{
	public static class Extensions3
	{
        public static T Center<T>(this T ui) where T : FrameworkElement
        {

            TransformGroup t = new TransformGroup();
            t.Children.Add(new TranslateTransform(-ui.Width / 2, -ui.Height / 2));
            ui.RenderTransform = t;
            return ui;
        }
        public static void Upload(this Socket _Socket, MemoryStream _MemoryStream, Action<double> Progress)
        {
            int bfzie = 50, i = 0;
            while (true)
            {
                byte[] bb = _MemoryStream.ReadBlock(1024 * bfzie);
                if (bb.Length == 0) break;
                _Socket.Send(bb);                
                i += bfzie;
                if (Progress != null) Progress(i);
            }
            while (_Socket.Poll(1000, SelectMode.SelectWrite) == false) ;
        }
        public static byte[] Receive(this Socket _Socket)
        {

            byte[] _buffer = new byte[99999];
            int count = _Socket.Receive(_buffer);
            if (count == 0) throw new SocketException();
            return _buffer.Substr(count);

        }

        public static byte[] Receive(this Socket _Socket, int length)
        {
            byte[] _buffer = new byte[length];
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                while (true)
                {
                    int count = _Socket.Receive(_buffer, length, 0);
                    if (count == 0) throw new ExceptionA("Read Socket failed");
                    _MemoryStream.Write(_buffer, 0, count);
                    length -= count;
                    if (length == 0) return _MemoryStream.ToArray();
                }
            }
        }
        public static void SetSource(this BitmapImage bm, Stream st)
        {
            bm.BeginInit();
            bm.StreamSource = st;
            bm.EndInit();
        }
        //static Extensions()
        //{
        //    if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
        //}

        public static string Save(this string s, string comment)
        {
            Encoding.Default.GetBytes(s).Save(comment);
            return s;
        }
        public static string Save(this string s)
        {
            Encoding.Default.GetBytes(s).Save();
            return s;
        }
        public static byte[] Save(this byte[] s)
        {
            return Save(s, "");
        }
        public static Random _Random = new Random();
        public static byte[] Save(this byte[] s, string comment)
        {
            string path = "./logs/" + DateTime.Now.ToString().Replace(":", "-") + _Random.RandomString(4) + ".html";
            File.WriteAllBytes(path, s);
            System.Diagnostics.Trace.WriteLine(Path.GetFullPath(path) + ":" + comment);
            return s;
        }
        public static string Send(this Socket _Socket, string s)
        {
            _Socket.Send(Encoding.Default.GetBytes(s));
            return s;
        }
        public static string ReceiveText(this Socket _Socket)
        {
            byte[] _buffer = new byte[99999];
            int count = _Socket.Receive(_buffer);
            if (count == 0) throw new ExceptionA();
            return Encoding.Default.GetString(_buffer.Substr(count));
        }
		public static void StartUpdate(this Dispatcher ds, Action d)
		{
			d();
			Thread.Sleep(10);
			ds.BeginInvoke(new Action<Dispatcher, Action>(StartUpdate), DispatcherPriority.SystemIdle, ds, d);
		}
	}

	//[DebuggerStepThrough]
	public class ClientWait
	{
		public int _Port;
		private List<Socket> _Sockets = new List<Socket>();


		public void StartAsync()
		{
			new Thread(Start).StartBackground("ClientWait");
		}
		private void Start()
		{
			TcpListener _TcpListener = new TcpListener(IPAddress.Any, _Port);
			_TcpListener.Start();
			while (true)
			{
				Socket _Socket = _TcpListener.AcceptSocket();
				lock ("clientwait")
					_Sockets.Add(_Socket);
				Thread.Sleep(10);
			}
		}


		public List<Socket> GetClients()
		{
			lock ("clientwait")
			{
				List<Socket> _Return = _Sockets;
				_Sockets = new List<Socket>();
				return _Return;
			}
		}
	}
	public abstract class Encoding : System.Text.Encoding
	{
		public new static System.Text.Encoding Default = System.Text.Encoding.Default;
		public static System.Text.Encoding Default2 { get { return System.Text.Encoding.Default; } }
	}
    
    [XmlRoot("dictionary")]
    //[DebuggerStepThrough]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {

            return null;

        }



        public void ReadXml(System.Xml.XmlReader reader)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();



            if (wasEmpty)

                return;



            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {

                reader.ReadStartElement("item");



                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);

                reader.ReadEndElement();



                reader.ReadStartElement("value");

                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();



                this.Add(key, value);



                reader.ReadEndElement();

                reader.MoveToContent();

            }

            reader.ReadEndElement();

        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            foreach (TKey key in this.Keys)
            {

                writer.WriteStartElement("item");



                writer.WriteStartElement("key");

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();



                writer.WriteStartElement("value");

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();



                writer.WriteEndElement();

            }

        }

        #endregion
    }
    //[DebuggerStepThrough]
    public class MemoryStreamA : MemoryStream
    {
        public SortedList<int, byte[]> _List = new SortedList<int, byte[]>();
        public int _i;
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                int a = base.Read(buffer, offset, count);
                if (a > 0) return a;
                Thread.Sleep(20);
            }
        }
        public override int ReadByte()
        {
            int b;
            while (true)
            {
                b = base.ReadByte();
                if (b != -1) break;
                Thread.Sleep(20);
            }
            return b;
        }
        public void Write(byte[] buffer, int index)
        {
            _List.Add(index, buffer);
            if (_i == 0) _i = index - 1;
            if (index <= _i) throw new Exception("Cannot Write Index Error");
            while (true)
            {
                if (_List.ContainsKey(_i + 1))
                {
                    _i++;
                    Write(_List[_i], 0, _List[_i].Length);
                } else
                    break;
            }

        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            long oldpos = Position;
            Seek(0, SeekOrigin.End);
            base.Write(buffer, offset, count);
            Seek(oldpos, SeekOrigin.Begin);
        }
    }
    //[DebuggerStepThrough]
    
    //[DebuggerStepThrough]
    public class intA
    {

        public override string ToString()
        {
            return i.ToString();
        }
        string file;
        public intA(string s)
        {
            file = s;
        }
        int? _i;

        public int i
        {
            get
            {
                lock ("intA")
                {
                    if (_i != null) return _i.Value;
                    else
                    {
                        if (File.Exists(file)) _i = int.Parse(File.ReadAllText(file));
                        else _i = 0;
                    }
                    return _i.Value;
                }
            }
            set
            {
                lock ("intA")
                {
                    _i = value; File.WriteAllText(file, _i.ToString());
                }
            }
        }
    }
    //[DebuggerStepThrough]
    public class ListA : List<string>
    {
        string file;
        public ListA(string file)
        {
            this.file = file;
            if (File.Exists(file))
                foreach (string s2 in File.ReadAllLines(file))
                {
                    base.Add(s2);
                }
        }
        public new bool Add(string s)
        {
            lock ("add")
                if (!Contains(s))
                {
                    base.Add(s);
                    return true;
                } else return false;
        }
        public void Flush()
        {
            lock ("flush")
                File.WriteAllLines(file, this.ToArray(), Encoding.Default);
        }
    }

    //[DebuggerStepThrough]
    public class ListB<T> : List<T>
    {
        public ListB()
        {
        }
        string file;
        public ListB(string file)
        {
            this.file = file;
            _XmlSerializer = new XmlSerializer(this.GetType(), new Type[] { typeof(T) });
            if (File.Exists(file))
                using (FileStream _FileStream = File.Open(file, FileMode.Open))
                {
                    List<T> list = (List<T>)_XmlSerializer.Deserialize(_FileStream);
                    foreach (T t in list)
                    {
                        Add(t);
                    }
                }
        }
        XmlSerializer _XmlSerializer;

        public void Flush()
        {
            lock ("flush")
            {
                using (FileStream _FileStream = new FileStream("users.xml", FileMode.Create, FileAccess.Write))
                    _XmlSerializer.Serialize(_FileStream, this);
            }
        }
    }



    //[DebuggerStepThrough]

    //[DebuggerStepThrough]
	[Obsolete]
    public class Spammer3 : Logging { }
    //[DebuggerStepThrough]
    public class Logging
    {
        public static string title;
        public static string _Title
        {
            set { if (title != value) title = Console.Title = value; }
        }
        public static Random _Random = new Random();

        public static bool done;
        public static bool Beep = true;

        public static void Setup() { Setup("../../"); }
        public static bool _supsend;
        public static IEnumerable<Process> FindProcess(string s)
        {
            IEnumerable<Process> ps = (from p in Process.GetProcesses() where p.ProcessName == s select p);
            return ps;
        }

        public static List<string> _console = new List<string>();

        public static void StartRemoteConsoleAsync(int port)
        {
            Thread _Thread = new Thread(StartRemoteConsole);
            _Thread.IsBackground = true;
            _Thread.Start(port);
        }
        private static void StartRemoteConsole(object o)
        {
            int port = (int)o;
            TcpListener _TcpListener = new TcpListener(IPAddress.Any, port);
            _TcpListener.Start();
            while (true)
            {
                Socket _Socket = _TcpListener.AcceptSocket();
                Thread _Thread = new Thread(StartListenClient);
                _Thread.IsBackground = true;
                _Thread.Start(_Socket);
            }
        }

        private static void StartListenClient(object o)
        {
            Socket _Socket = (Socket)o;
            "Console Connected".Trace();
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            TextWriterTraceListener _TextWriterTraceListener = new TextWriterTraceListener(_NetworkStream);
            Trace.Listeners.Add(_TextWriterTraceListener);
            try
            {
                while (true)
                {
                    string s = _NetworkStream.ReadLine();
                    _console.Add(s);
                }
            } catch (IOException) { }
            Trace.Listeners.Remove(_TextWriterTraceListener);
            "Console Disconnected".Trace();
        }
        private static void StartReadConsole()
        {
            while (true)
                _console.Add(Console.ReadLine());
        }
        public static bool _RedirectOutPut = true;
        public static void Setup(string s)
        {

            if (done == true) return;
            done = true;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Process _Process = Process.GetCurrentProcess();
            if (!_AllowDuplicates && FindProcess(_Process.ProcessName).Count() > 1)
            {
                Console.Beep(100, 10);
                System.Windows.MessageBox.Show("process already exists");
                _Process.Kill();
            }
            try
            {
                Directory.SetCurrentDirectory(Assembly.GetEntryAssembly().Location + "../../" + s);
            } catch { Directory.SetCurrentDirectory(@"D:\Documents and Settings\doru\My Documents\code\STDSTUDIO2\"); }
            if (Directory.Exists("./logs/")) Directory.Delete("logs", true);
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            if (Console.LargestWindowHeight != 0)
            {
                //new Thread(StartReadConsole).StartBackground();
                Console.Title = Assembly.GetEntryAssembly().GetName().Name;
                if (_RedirectOutPut)
                    Trace.Listeners.Add(new TextWriterTraceListener(Console.OpenStandardOutput()));
            }
            Trace.AutoFlush = true;
            Trace.WriteLine("Programm Started " + DateTime.Now);
        }
        public static bool _AllowDuplicates;
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(e.ExceptionObject);
            if (Console.LargestWindowHeight != 0 && Beep)
                Console.Beep(100, 10);
        }

    }
}
